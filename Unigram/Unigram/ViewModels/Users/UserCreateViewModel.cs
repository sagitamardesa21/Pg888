﻿using System;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Entities;
using Unigram.Navigation.Services;
using Unigram.Services;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Users
{
    public class UserCreateViewModel : TLViewModelBase
    {
        public UserCreateViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            SendCommand = new RelayCommand(SendExecute, () => !string.IsNullOrEmpty(_firstName) && !string.IsNullOrEmpty(_phoneNumber));
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, NavigationState state)
        {
            ProtoService.Send(new GetCountryCode(), result =>
            {
                if (result is Text text)
                {
                    BeginOnUIThread(() => GotUserCountry(text.TextValue));
                }
            });

            IsLoading = false;
            return Task.CompletedTask;
        }

        private void GotUserCountry(string code)
        {
            Country country = null;
            foreach (var local in Country.All)
            {
                if (string.Equals(local.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    country = local;
                    break;
                }
            }

            if (country != null && SelectedCountry == null && string.IsNullOrEmpty(PhoneNumber))
            {
                BeginOnUIThread(() =>
                {
                    SelectedCountry = country;
                });
            }
        }

        private Country _selectedCountry;
        public Country SelectedCountry
        {
            get => _selectedCountry;
            set => Set(ref _selectedCountry, value);
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                Set(ref _firstName, value);
                SendCommand.RaiseCanExecuteChanged();
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => Set(ref _lastName, value);
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                Set(ref _phoneNumber, value);
                SendCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SendCommand { get; }
        private async void SendExecute()
        {
            var phoneNumber = _phoneNumber?.Trim('+').Replace(" ", string.Empty);

            var response = await ProtoService.SendAsync(new ImportContacts(new[] { new Contact(phoneNumber, _firstName, _lastName, string.Empty, 0) }));
            if (response is ImportedContacts imported)
            {
                if (imported.UserIds.Count > 0)
                {
                    var create = await ProtoService.SendAsync(new CreatePrivateChat(imported.UserIds[0], false));
                    if (create is Chat chat)
                    {
                        NavigationService.NavigateToChat(chat);
                    }
                    else
                    {
                        await MessagePopup.ShowAsync(Strings.Resources.ContactNotRegistered, Strings.Resources.AppName, Strings.Resources.Invite, Strings.Resources.Cancel);
                    }
                }
                else
                {
                    await MessagePopup.ShowAsync(Strings.Resources.ContactNotRegistered, Strings.Resources.AppName, Strings.Resources.Invite, Strings.Resources.Cancel);
                }
            }
            else
            {
                await MessagePopup.ShowAsync(Strings.Resources.ContactNotRegistered, Strings.Resources.AppName, Strings.Resources.Invite, Strings.Resources.Cancel);
            }
        }
    }
}
