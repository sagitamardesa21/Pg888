using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TdWindows;
using Telegram.Api;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Services;
using Unigram.Views;
using Unigram.Views.SignIn;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.SignIn
{
    public class SignInPasswordViewModel : UnigramViewModelBase
    {
        private SignInPasswordPage.NavigationParameters _parameters;

        public SignInPasswordViewModel(IProtoService protoService, ICacheService cacheService, IEventAggregator aggregator) 
            : base(protoService, cacheService, aggregator)
        {
            SendCommand = new RelayCommand(SendExecute, () => !IsLoading);
            ForgotCommand = new RelayCommand(ForgotExecute);
            ResetCommand = new RelayCommand(ResetExecute);
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var authState = ProtoService.GetAuthorizationState();
            if (authState is AuthorizationStateWaitPassword waitPassword)
            {
                PasswordHint = waitPassword.PasswordHint;
            }

            return Task.CompletedTask;
        }

        private string _passwordHint;
        public string PasswordHint
        {
            get
            {
                return _passwordHint;
            }
            set
            {
                Set(ref _passwordHint, value);
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                Set(ref _password, value);
            }
        }

        private bool _isResettable;
        public bool IsResettable
        {
            get
            {
                return _isResettable;
            }
            set
            {
                Set(ref _isResettable, value);
            }
        }

        public RelayCommand SendCommand { get; }
        private async void SendExecute()
        {
            if (string.IsNullOrEmpty(_password))
            {
                RaisePropertyChanged("PASSWORD_INVALID");
                return;
            }

            var response = await ProtoService.SendAsync(new CheckAuthenticationPassword(_password));
            if (response is Error error)
            {
                if (error.TypeEquals(TLErrorType.PASSWORD_HASH_INVALID))
                {
                    //await new MessageDialog(Resources.PasswordInvalidString, Resources.Error).ShowAsync();
                }
                else if (error.CodeEquals(TLErrorCode.FLOOD))
                {
                    //await new MessageDialog($"{Resources.FloodWaitString}\r\n\r\n({result.Error.Message})", Resources.Error).ShowAsync();
                }

                Execute.ShowDebugMessage("account.checkPassword error " + error);
            }
        }

        public RelayCommand ForgotCommand { get; }
        private async void ForgotExecute()
        {
            if (_parameters == null)
            {
                // TODO: ...
                return;
            }

            if (_parameters.Result.HasRecoveryEmailAddress)
            {
                IsLoading = true;

                var response = await LegacyService.RequestPasswordRecoveryAsync();
                if (response.IsSucceeded)
                {
                    await TLMessageDialog.ShowAsync(string.Format(Strings.Android.RestoreEmailSent, response.Result.EmailPattern), Strings.Android.AppName, Strings.Android.OK);

                    // TODO: show recovery page
                }
                else if (response.Error != null)
                {
                    IsLoading = false;
                    await TLMessageDialog.ShowAsync(response.Error.ErrorMessage, Strings.Android.AppName, Strings.Android.OK);
                }
            }
            else
            {
                await TLMessageDialog.ShowAsync(Strings.Android.RestorePasswordNoEmailText, Strings.Android.RestorePasswordNoEmailTitle, Strings.Android.OK);
                IsResettable = true;
            }
        }

        public RelayCommand ResetCommand { get; }
        private async void ResetExecute()
        {
            var confirm = await TLMessageDialog.ShowAsync(Strings.Android.ResetMyAccountWarningText, Strings.Android.ResetMyAccountWarning, Strings.Android.ResetMyAccountWarningReset, Strings.Android.Cancel);
            if (confirm == ContentDialogResult.Primary)
            {
                IsLoading = true;

                var response = await LegacyService.DeleteAccountAsync("Forgot password");
                if (response.IsSucceeded)
                {
                    //var logout = await LegacyService.LogOutAsync();

                    var state = new SignUpPage.NavigationParameters
                    {
                        PhoneNumber = _parameters.PhoneNumber,
                        PhoneCode = _parameters.PhoneCode,
                        Result = _parameters.Result,
                    };

                    NavigationService.Navigate(typeof(SignUpPage), state);
                }
                else if (response.Error != null)
                {
                    IsLoading = false;

                    if (response.Error.ErrorMessage.Contains("2FA_RECENT_CONFIRM"))
                    {
                        await TLMessageDialog.ShowAsync(Strings.Android.ResetAccountCancelledAlert, Strings.Android.AppName, Strings.Android.OK);
                    }
                    else if (response.Error.ErrorMessage.StartsWith("2FA_CONFIRM_WAIT_"))
                    {
                        // TODO: show info
                    }
                    else
                    {
                        await TLMessageDialog.ShowAsync(response.Error.ErrorMessage, Strings.Android.AppName, Strings.Android.OK);
                    }
                }
            }
        }
    }
}
