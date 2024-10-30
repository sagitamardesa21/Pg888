﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unigram.Common;
using Unigram.Core.Common;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.UserDataAccounts;
using Windows.Foundation.Metadata;
using Windows.Storage;

namespace Unigram.Services
{
    public interface IContactsService
    {
        Task SyncAsync(Telegram.Td.Api.Users result);

        Task<Telegram.Td.Api.BaseObject> ImportAsync();
        Task ExportAsync(Telegram.Td.Api.Users result);

        Task RemoveAsync();
    }

    public class ContactsService : IContactsService
    {
        private readonly IProtoService _protoService;
        private readonly ICacheService _cacheService;
        private readonly IEventAggregator _aggregator;

        private readonly DisposableMutex _syncLock;
        private readonly object _importedPhonesRoot;

        private CancellationTokenSource _syncToken;

        public ContactsService(IProtoService protoService, ICacheService cacheService, IEventAggregator aggregator)
        {
            _protoService = protoService;
            _cacheService = cacheService;
            _aggregator = aggregator;

            _syncLock = new DisposableMutex();
            _importedPhonesRoot = new object();
        }

        public async Task SyncAsync(Telegram.Td.Api.Users result)
        {
            try
            {
                if (_syncToken == null)
                {
                    _syncToken = new CancellationTokenSource();
                }

                using (await _syncLock.WaitAsync(_syncToken.Token))
                {
                    await ExportAsyncInternal(result);
                    await ImportAsyncInternal();
                }
            }
            catch
            {
                Logs.Log.Write("Sync contacts canceled");
                Debug.WriteLine("» Sync contacts canceled");
            }
        }

        #region Import

        public async Task<Telegram.Td.Api.BaseObject> ImportAsync()
        {
            try
            {
                if (_syncToken == null)
                {
                    _syncToken = new CancellationTokenSource();
                }

                using (await _syncLock.WaitAsync(_syncToken.Token))
                {
                    return await ImportAsyncInternal();
                }
            }
            catch
            {
                Logs.Log.Write("Sync contacts canceled");
                Debug.WriteLine("» Sync contacts canceled");
            }

            return null;
        }

        private async Task<Telegram.Td.Api.BaseObject> ImportAsyncInternal()
        {
            Telegram.Td.Api.BaseObject result = null;

            Logs.Log.Write("Importing contacts");
            Debug.WriteLine("» Importing contacts");

            var store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);
            if (store != null)
            {
                result = await ImportAsync(store);
            }

            Logs.Log.Write("Importing contacts completed");
            Debug.WriteLine("» Importing contacts completed");

            return result;
        }

        private async Task<Telegram.Td.Api.BaseObject> ImportAsync(ContactStore store)
        {
            var contacts = await store.FindContactsAsync();
            var importedPhones = new Dictionary<string, Contact>();

            foreach (var contact in contacts)
            {
                foreach (var phone in contact.Phones)
                {
                    importedPhones[phone.Number] = contact;
                }
            }

            var importingContacts = new List<Telegram.Td.Api.Contact>();

            foreach (var phone in importedPhones.Keys.Take(1300).ToList())
            {
                var contact = importedPhones[phone];
                var firstName = contact.FirstName ?? string.Empty;
                var lastName = contact.LastName ?? string.Empty;

                if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                {
                    if (string.IsNullOrEmpty(contact.DisplayName))
                    {
                        continue;
                    }

                    firstName = contact.DisplayName;
                }

                if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                {
                    var item = new Telegram.Td.Api.Contact
                    {
                        PhoneNumber = phone,
                        FirstName = firstName,
                        LastName = lastName
                    };

                    importingContacts.Add(item);
                }
            }

            return await _protoService.SendAsync(new Telegram.Td.Api.ChangeImportedContacts(importingContacts));
        }

        #endregion

        #region Export

        public async Task ExportAsync(Telegram.Td.Api.Users result)
        {
            try
            {
                if (_syncToken == null)
                {
                    _syncToken = new CancellationTokenSource();
                }

                using (await _syncLock.WaitAsync(_syncToken.Token))
                {
                    await ExportAsyncInternal(result);
                }
            }
            catch
            {
                Logs.Log.Write("Sync contacts canceled");
                Debug.WriteLine("» Sync contacts canceled");
            }
        }

        private async Task ExportAsyncInternal(Telegram.Td.Api.Users result)
        {
            Logs.Log.Write("Exporting contacts");
            Debug.WriteLine("» Exporting contacts");

            var store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
            if (store == null)
            {
                return;
            }

            var userDataAccount = await GetUserDataAccountAsync();

            var contactList = await GetContactListAsync(userDataAccount, store);
            var annotationList = await GetAnnotationListAsync(userDataAccount);

            if (contactList != null && annotationList != null)
            {
                await ExportAsync(contactList, annotationList, result);
            }

            Logs.Log.Write("Exporting contacts completed");
            Debug.WriteLine("» Exporting contacts completed");
        }

        private async Task ExportAsync(ContactList contactList, ContactAnnotationList annotationList, Telegram.Td.Api.Users result)
        {
            if (result == null)
            {
                return;
            }

            foreach (var item in result.UserIds)
            {
                var user = _protoService.GetUser(item);

                var contact = await contactList.GetContactFromRemoteIdAsync("u" + user.Id);
                if (contact == null)
                {
                    contact = new Contact();
                }

                if (user.ProfilePhoto != null && user.ProfilePhoto.Small.Local.IsDownloadingCompleted)
                {
                    contact.SourceDisplayPicture = await StorageFile.GetFileFromPathAsync(user.ProfilePhoto.Small.Local.Path);
                }

                contact.FirstName = user.FirstName ?? string.Empty;
                contact.LastName = user.LastName ?? string.Empty;
                //contact.Nickname = item.Username ?? string.Empty;
                contact.RemoteId = "u" + user.Id;
                //contact.Id = item.Id.ToString();

                var phone = contact.Phones.FirstOrDefault();
                if (phone == null)
                {
                    phone = new ContactPhone();
                    phone.Kind = ContactPhoneKind.Mobile;
                    phone.Number = string.Format("+{0}", user.PhoneNumber);
                    contact.Phones.Add(phone);
                }
                else
                {
                    phone.Kind = ContactPhoneKind.Mobile;
                    phone.Number = string.Format("+{0}", user.PhoneNumber);
                }

                await contactList.SaveContactAsync(contact);

                ContactAnnotation annotation;
                var annotations = await annotationList.FindAnnotationsByRemoteIdAsync(user.Id.ToString());
                if (annotations.Count == 0)
                {
                    annotation = new ContactAnnotation();
                }
                else
                {
                    annotation = annotations[0];
                }

                annotation.ContactId = contact.Id;
                annotation.RemoteId = contact.RemoteId;
                annotation.SupportedOperations = ContactAnnotationOperations.ContactProfile | ContactAnnotationOperations.Message | ContactAnnotationOperations.AudioCall;

                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
                {
                    annotation.SupportedOperations |= ContactAnnotationOperations.Share;
                }

                if (annotation.ProviderProperties.Count == 0)
                {
                    annotation.ProviderProperties.Add("ContactPanelAppID", Package.Current.Id.FamilyName + "!App");
                    annotation.ProviderProperties.Add("ContactShareAppID", Package.Current.Id.FamilyName + "!App");
                }

                var added = await annotationList.TrySaveAnnotationAsync(annotation);
            }
        }

        private async Task<UserDataAccount> GetUserDataAccountAsync()
        {
            var store = await UserDataAccountManager.RequestStoreAsync(UserDataAccountStoreAccessType.AppAccountsReadWrite);

            UserDataAccount userDataAccount = null;
            var id = _cacheService.GetOption<Telegram.Td.Api.OptionValueString>("x_user_data_account");
            if (id != null)
            {
                userDataAccount = await store.GetAccountAsync(id.Value);
            }
            
            if (userDataAccount == null)
            {
                userDataAccount = await store.CreateAccountAsync($"{_cacheService.GetMyId()}");
                await _protoService.SendAsync(new Telegram.Td.Api.SetOption("x_user_data_account", new Telegram.Td.Api.OptionValueString(userDataAccount.Id)));
            }

            return userDataAccount;
        }

        private async Task<ContactList> GetContactListAsync(UserDataAccount userDataAccount, ContactStore store)
        {
            var user = _cacheService.GetUser(_cacheService.GetMyId());
            var displayName = user?.GetFullName() ?? "Unigram";

            ContactList contactList = null;
            var id = _cacheService.GetOption<Telegram.Td.Api.OptionValueString>("x_contact_list");
            if (id != null)
            {
                contactList = await store.GetContactListAsync(id.Value);
            }
            
            if (contactList == null)
            {
                contactList = await store.CreateContactListAsync(displayName, userDataAccount.Id);
                await _protoService.SendAsync(new Telegram.Td.Api.SetOption("x_contact_list", new Telegram.Td.Api.OptionValueString(contactList.Id)));
            }

            contactList.DisplayName = displayName;
            contactList.OtherAppWriteAccess = ContactListOtherAppWriteAccess.None;
            await contactList.SaveAsync();

            return contactList;
        }

        private async Task<ContactAnnotationList> GetAnnotationListAsync(UserDataAccount userDataAccount)
        {
            var store = await ContactManager.RequestAnnotationStoreAsync(ContactAnnotationStoreAccessType.AppAnnotationsReadWrite);
            if (store == null)
            {
                return null;
            }

            ContactAnnotationList contactList = null;
            var id = _cacheService.GetOption<Telegram.Td.Api.OptionValueString>("x_annotation_list");
            if (id != null)
            {
                contactList = await store.GetAnnotationListAsync(id.Value);
            }
            
            if (contactList == null)
            {
                contactList = await store.CreateAnnotationListAsync(userDataAccount.Id);
                await _protoService.SendAsync(new Telegram.Td.Api.SetOption("x_annotation_list", new Telegram.Td.Api.OptionValueString(contactList.Id)));
            }

            return contactList;
        }

        #endregion

        public async Task RemoveAsync()
        {
            if (_syncToken != null)
            {
                _syncToken.Cancel();
                _syncToken = null;
            }

            using (await _syncLock.WaitAsync())
            {
                Debug.WriteLine("UNSYNCING CONTACTS");

                var userDataAccount = await GetUserDataAccountAsync();
                await userDataAccount.DeleteAsync();

                await _protoService.SendAsync(new Telegram.Td.Api.SetOption("x_user_data_account", new Telegram.Td.Api.OptionValueEmpty()));
                await _protoService.SendAsync(new Telegram.Td.Api.SetOption("x_contact_list", new Telegram.Td.Api.OptionValueEmpty()));
                await _protoService.SendAsync(new Telegram.Td.Api.SetOption("x_annotation_list", new Telegram.Td.Api.OptionValueEmpty()));

                Debug.WriteLine("UNSYNCED CONTACTS");
            }
        }
    }
}
