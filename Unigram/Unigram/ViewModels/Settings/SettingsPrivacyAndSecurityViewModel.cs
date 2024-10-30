//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Navigation.Services;
using Unigram.Services;
using Unigram.ViewModels.Settings.Privacy;
using Unigram.Views.Popups;
using Unigram.Views.Settings;
using Unigram.Views.Settings.Password;

namespace Unigram.ViewModels.Settings
{
    public class SettingsPrivacyAndSecurityViewModel : TLMultipleViewModelBase
        , IHandle
        //, IHandle<UpdateOption>
    {
        private readonly IContactsService _contactsService;
        private readonly IPasscodeService _passcodeService;

        private readonly SettingsPrivacyShowForwardedViewModel _showForwardedRules;
        private readonly SettingsPrivacyShowPhoneViewModel _showPhoneRules;
        private readonly SettingsPrivacyShowPhotoViewModel _showPhotoRules;
        private readonly SettingsPrivacyShowStatusViewModel _showStatusRules;
        private readonly SettingsPrivacyAllowCallsViewModel _allowCallsRules;
        private readonly SettingsPrivacyAllowChatInvitesViewModel _allowChatInvitesRules;
        private readonly SettingsPrivacyAllowPrivateVoiceAndVideoNoteMessagesViewModel _allowPrivateVoiceAndVideoNoteMessages;

        public SettingsPrivacyAndSecurityViewModel(IClientService clientService, ISettingsService settingsService, IEventAggregator aggregator, IContactsService contactsService, IPasscodeService passcodeService, SettingsPrivacyShowForwardedViewModel showForwarded, SettingsPrivacyShowPhoneViewModel showPhone, SettingsPrivacyShowPhotoViewModel showPhoto, SettingsPrivacyShowStatusViewModel statusTimestamp, SettingsPrivacyAllowCallsViewModel phoneCall, SettingsPrivacyAllowChatInvitesViewModel chatInvite, SettingsPrivacyAllowPrivateVoiceAndVideoNoteMessagesViewModel privateVoiceAndVideoNoteMessages)
            : base(clientService, settingsService, aggregator)
        {
            _contactsService = contactsService;
            _passcodeService = passcodeService;

            _showForwardedRules = showForwarded;
            _showPhoneRules = showPhone;
            _showPhotoRules = showPhoto;
            _showStatusRules = statusTimestamp;
            _allowCallsRules = phoneCall;
            _allowChatInvitesRules = chatInvite;
            _allowPrivateVoiceAndVideoNoteMessages = privateVoiceAndVideoNoteMessages;

            PasscodeCommand = new RelayCommand(PasscodeExecute);
            PasswordCommand = new RelayCommand(PasswordExecute);
            ChangeEmailCommand = new RelayCommand(ChangeEmailExecute);
            ClearDraftsCommand = new RelayCommand(ClearDraftsExecute);
            ClearContactsCommand = new RelayCommand(ClearContactsExecute);
            ClearPaymentsCommand = new RelayCommand(ClearPaymentsExecute);

            Children.Add(_showForwardedRules);
            Children.Add(_showPhotoRules);
            Children.Add(_showPhoneRules);
            Children.Add(_showStatusRules);
            Children.Add(_allowCallsRules);
            Children.Add(_allowChatInvitesRules);
            Children.Add(_allowPrivateVoiceAndVideoNoteMessages);
        }

        protected override Task OnNavigatedToAsync(object parameter, NavigationMode mode, NavigationState state)
        {
            ClientService.Send(new GetAccountTtl(), result =>
            {
                if (result is AccountTtl ttl)
                {
                    BeginOnUIThread(() =>
                    {
                        if (ttl.Days == 0)
                        {
                            _accountTtl = _accountTtlIndexer[2];
                            RaisePropertyChanged(nameof(AccountTtl));
                            return;
                        }

                        int? period = null;

                        var max = 2147483647;
                        foreach (var days in _accountTtlIndexer)
                        {
                            int abs = Math.Abs(ttl.Days - days);
                            if (abs < max)
                            {
                                max = abs;
                                period = days;
                            }
                        }

                        _accountTtl = period ?? _accountTtlIndexer[2];
                        RaisePropertyChanged(nameof(AccountTtl));
                    });
                }
            });

            ClientService.Send(new GetBlockedMessageSenders(0, 1), result =>
            {
                if (result is MessageSenders senders)
                {
                    BeginOnUIThread(() => BlockedUsers = senders.TotalCount);
                }
            });

            ClientService.Send(new GetPasswordState(), result =>
            {
                if (result is PasswordState passwordState)
                {
                    BeginOnUIThread(() =>
                    {
                        HasEmailAddress = passwordState.LoginEmailAddressPattern.Length > 0;
                        HasPassword = passwordState.HasPassword;
                    });
                }
            });

            ClientService.Send(new GetDefaultMessageAutoDeleteTime(), result =>
            {
                if (result is MessageAutoDeleteTime messageTtl)
                {
                    BeginOnUIThread(() => DefaultTtl = messageTtl.Time);
                }
            });

            if (ApiInfo.IsPackagedRelease && ClientService.Options.CanIgnoreSensitiveContentRestrictions)
            {
                ClientService.Send(new GetOption("ignore_sensitive_content_restrictions"), result =>
                {
                    BeginOnUIThread(() => RaisePropertyChanged(nameof(IgnoreSensitiveContentRestrictions)));
                });
            }

            HasPasscode = _passcodeService.IsEnabled;
            return Task.CompletedTask;
        }

        public override void Subscribe()
        {
            Aggregator.Subscribe<UpdateOption>(this, Handle);
        }

        #region Properties

        public SettingsPrivacyShowForwardedViewModel ShowForwardedRules => _showForwardedRules;
        public SettingsPrivacyShowPhoneViewModel ShowPhoneRules => _showPhoneRules;
        public SettingsPrivacyShowPhotoViewModel ShowPhotoRules => _showPhotoRules;
        public SettingsPrivacyShowStatusViewModel ShowStatusRules => _showStatusRules;
        public SettingsPrivacyAllowCallsViewModel AllowCallsRules => _allowCallsRules;
        public SettingsPrivacyAllowChatInvitesViewModel AllowChatInvitesRules => _allowChatInvitesRules;
        public SettingsPrivacyAllowPrivateVoiceAndVideoNoteMessagesViewModel AllowPrivateVoiceAndVideoNoteMessages => _allowPrivateVoiceAndVideoNoteMessages;

        private int _accountTtl;
        public int AccountTtl
        {
            get => Array.IndexOf(_accountTtlIndexer, _accountTtl);
            set
            {
                if (value >= 0 && value < _accountTtlIndexer.Length && _accountTtl != _accountTtlIndexer[value])
                {
                    ClientService.SendAsync(new SetAccountTtl(new AccountTtl(_accountTtl = _accountTtlIndexer[value])));
                    RaisePropertyChanged();
                }
            }
        }

        private readonly int[] _accountTtlIndexer = new[]
        {
            30,
            90,
            180,
            365
        };

        public List<SettingsOptionItem<int>> AccountTtlOptions { get; } = new()
        {
            new SettingsOptionItem<int>(30, Locale.Declension("Months", 1)),
            new SettingsOptionItem<int>(90, Locale.Declension("Months", 3)),
            new SettingsOptionItem<int>(180, Locale.Declension("Months", 6)),
            new SettingsOptionItem<int>(365, Locale.Declension("Years", 1))
        };

        private int _blockedUsers;
        public int BlockedUsers
        {
            get => _blockedUsers;
            set => Set(ref _blockedUsers, value);
        }

        private bool _hasPassword;
        public bool HasPassword
        {
            get => _hasPassword;
            set => Set(ref _hasPassword, value);
        }

        private bool _hasEmailAddress;
        public bool HasEmailAddress
        {
            get => _hasEmailAddress;
            set => Set(ref _hasEmailAddress, value);
        }

        private bool _hasPasscode;
        public bool HasPasscode
        {
            get => _hasPasscode;
            set => Set(ref _hasPasscode, value);
        }

        private int _defaultTtl;
        public int DefaultTtl
        {
            get => _defaultTtl;
            set => Set(ref _defaultTtl, value);
        }

        public bool IsContactsSyncEnabled
        {
            get => Settings.IsContactsSyncEnabled;
            set
            {
                Settings.IsContactsSyncEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool IsContactsSuggestEnabled
        {
            get => !ClientService.Options.DisableTopChats;
            set => SetSuggestContacts(value);
        }

        public bool IsArchiveAndMuteEnabled
        {
            get => ClientService.Options.ArchiveAndMuteNewChatsFromUnknownUsers;
            set
            {
                ClientService.Options.ArchiveAndMuteNewChatsFromUnknownUsers = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSecretPreviewsEnabled
        {
            get => Settings.IsSecretPreviewsEnabled;
            set
            {
                Settings.IsSecretPreviewsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool IgnoreSensitiveContentRestrictions
        {
            get => ClientService.Options.IgnoreSensitiveContentRestrictions;
            set
            {
                if (ClientService.Options.CanIgnoreSensitiveContentRestrictions)
                {
                    ClientService.Options.IgnoreSensitiveContentRestrictions = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public void Handle(UpdateOption update)
        {
            if (update.Name.Equals("disable_top_chats"))
            {
                BeginOnUIThread(() => RaisePropertyChanged(nameof(IsContactsSuggestEnabled)));
            }
            else if (update.Name.Equals("ignore_sensitive_content_restrictions"))
            {
                BeginOnUIThread(() => RaisePropertyChanged(nameof(IgnoreSensitiveContentRestrictions)));
            }
        }

        private async void SetSuggestContacts(bool value)
        {
            if (!value)
            {
                var confirm = await MessagePopup.ShowAsync(XamlRoot, Strings.Resources.SuggestContactsAlert, Strings.Resources.AppName, Strings.Resources.MuteDisable, Strings.Resources.Cancel);
                if (confirm != ContentDialogResult.Primary)
                {
                    RaisePropertyChanged(nameof(IsContactsSuggestEnabled));
                    return;
                }
            }

            ClientService.Options.DisableTopChats = !value;
        }

        public RelayCommand PasscodeCommand { get; }
        private void PasscodeExecute()
        {
            NavigationService.NavigateToPasscode();
        }

        public RelayCommand PasswordCommand { get; }
        private async void PasswordExecute()
        {
            var response = await ClientService.SendAsync(new GetPasswordState());
            if (response is PasswordState passwordState)
            {
                if (passwordState.HasPassword)
                {
                    NavigationService.Navigate(typeof(SettingsPasswordPage));
                }
                else if (passwordState.RecoveryEmailAddressCodeInfo != null)
                {
                    var state = new NavigationState
                    {
                        { "pattern", passwordState.RecoveryEmailAddressCodeInfo.EmailAddressPattern },
                        { "length", passwordState.RecoveryEmailAddressCodeInfo.Length }
                    };

                    NavigationService.Navigate(typeof(SettingsPasswordConfirmPage), state: state);
                }
                else
                {
                    NavigationService.Navigate(typeof(SettingsPasswordIntroPage));
                }
            }
        }

        public void OpenAutoDelete()
        {
            NavigationService.Navigate(typeof(SettingsAutoDeletePage));
        }

        public RelayCommand ChangeEmailCommand { get; }
        private async void ChangeEmailExecute()
        {
            var response = await ClientService.SendAsync(new GetPasswordState());
            if (response is PasswordState passwordState)
            {
                var confirm = await MessagePopup.ShowAsync(XamlRoot, Strings.Resources.EmailLoginChangeMessage, passwordState.LoginEmailAddressPattern, Strings.Resources.ChangeEmail, Strings.Resources.Cancel);
            }
        }

        public RelayCommand ClearDraftsCommand { get; }
        private async void ClearDraftsExecute()
        {
            var confirm = await MessagePopup.ShowAsync(XamlRoot, Strings.Resources.AreYouSureClearDrafts, Strings.Resources.AppName, Strings.Resources.OK, Strings.Resources.Cancel);
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            var clear = await ClientService.SendAsync(new ClearAllDraftMessages(true));
            if (clear is Error)
            {
                // TODO
            }
        }

        public RelayCommand ClearContactsCommand { get; }
        private async void ClearContactsExecute()
        {
            var confirm = await MessagePopup.ShowAsync(XamlRoot, Strings.Resources.SyncContactsDeleteInfo, Strings.Resources.Contacts, Strings.Resources.OK, Strings.Resources.Cancel);
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            IsContactsSyncEnabled = false;

            var clear = await ClientService.SendAsync(new ClearImportedContacts());
            if (clear is Error)
            {
                // TODO
            }

            var contacts = await ClientService.SendAsync(new GetContacts());
            if (contacts is Telegram.Td.Api.Users users)
            {
                var delete = await ClientService.SendAsync(new RemoveContacts(users.UserIds));
                if (delete is Error)
                {
                    // TODO
                }
            }
        }

        public RelayCommand ClearPaymentsCommand { get; }
        private async void ClearPaymentsExecute()
        {
            var dialog = new ContentPopup();
            var stack = new StackPanel();
            var checkShipping = new CheckBox { Content = Strings.Resources.PrivacyClearShipping, IsChecked = true };
            var checkPayment = new CheckBox { Content = Strings.Resources.PrivacyClearPayment, IsChecked = true };

            var toggle = new RoutedEventHandler((s, args) =>
            {
                dialog.IsPrimaryButtonEnabled = checkShipping.IsChecked == true || checkPayment.IsChecked == true;
            });

            checkShipping.Checked += toggle;
            checkShipping.Unchecked += toggle;
            checkPayment.Checked += toggle;
            checkPayment.Unchecked += toggle;

            stack.Margin = new Thickness(12, 16, 12, 0);
            stack.Children.Add(checkShipping);
            stack.Children.Add(checkPayment);

            dialog.Title = Strings.Resources.PrivacyPayments;
            dialog.Content = stack;
            dialog.PrimaryButtonText = Strings.Resources.ClearButton;
            dialog.SecondaryButtonText = Strings.Resources.Cancel;

            var confirm = await dialog.ShowQueuedAsync(XamlRoot);
            if (confirm == ContentDialogResult.Primary)
            {
                var info = checkShipping.IsChecked == true;
                var credential = checkPayment.IsChecked == true;

                if (info)
                {
                    ClientService.Send(new DeleteSavedOrderInfo());
                }

                if (credential)
                {
                    ClientService.Send(new DeleteSavedCredentials());
                }
            }
        }

        public override async void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            if (propertyName.Equals(nameof(IsContactsSyncEnabled)))
            {
                if (IsContactsSyncEnabled)
                {
                    ClientService.Send(new GetContacts(), async result =>
                    {
                        if (result is Telegram.Td.Api.Users users)
                        {
                            await _contactsService.SyncAsync(users);
                        }
                    });
                }
                else
                {
                    await _contactsService.RemoveAsync();
                }
            }
        }
    }
}
