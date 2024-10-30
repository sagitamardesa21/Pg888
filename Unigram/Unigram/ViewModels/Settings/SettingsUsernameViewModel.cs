using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TdWindows;
using Telegram.Api;
using Telegram.Api.TL;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Converters;
using Unigram.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Settings
{
    public class SettingsUsernameViewModel : UnigramViewModelBase
    {
        public SettingsUsernameViewModel(IProtoService protoService, ICacheService cacheService, IEventAggregator aggregator) 
            : base(protoService, cacheService, aggregator)
        {
            SendCommand = new RelayCommand(SendExecute);
            CopyCommand = new RelayCommand(CopyExecute);
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            IsValid = false;
            IsLoading = false;
            ErrorMessage = null;

            var response = await ProtoService.SendAsync(new GetMe());
            if (response is User user)
            {
                _username = user.Username;
            }

            RaisePropertyChanged(() => Username);
        }

        private string _username;
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                Set(ref _username, value);
                UpdateIsValid(value);
            }
        }

        private bool _isValid;
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                Set(ref _isValid, value);
            }
        }

        private bool _isAvailable;
        public bool IsAvailable
        {
            get
            {
                return _isAvailable;
            }
            set
            {
                Set(ref _isAvailable, value);
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                Set(ref _errorMessage, value);
            }
        }

        public async void CheckAvailability(string text)
        {
            var myid = ProtoService.GetOption<OptionValueInteger>("my_id");

            var response = await ProtoService.SendAsync(new SearchPublicChat(text));
            if (response is Chat chat)
            {
                if (chat.Type is ChatTypePrivate privata && privata.UserId == myid.Value)
                {
                    IsLoading = false;
                    IsAvailable = true;
                    ErrorMessage = null;
                }
                else
                {
                    IsLoading = false;
                    IsAvailable = false;
                    ErrorMessage = Strings.Android.UsernameInUse;
                }
            }
            else if (response is Error error)
            {
                if (error.TypeEquals(TLErrorType.USERNAME_INVALID))
                {
                    IsLoading = false;
                    IsAvailable = false;
                    ErrorMessage = Strings.Android.UsernameInvalid;
                }
                else if (error.TypeEquals(TLErrorType.USERNAME_OCCUPIED))
                {
                    IsLoading = false;
                    IsAvailable = false;
                    ErrorMessage = Strings.Android.UsernameInUse;
                }
                else if (error.TypeEquals(TLErrorType.USERNAME_NOT_OCCUPIED))
                {
                    IsLoading = false;
                    IsAvailable = true;
                    ErrorMessage = null;
                }
            }
        }

        public bool UpdateIsValid(string username)
        {
            IsValid = IsValidUsername(username);
            IsLoading = false;
            IsAvailable = false;

            if (!IsValid)
            {
                if (string.IsNullOrEmpty(username))
                {
                    ErrorMessage = null;
                }
                else if (_username.Length < 5)
                {
                    ErrorMessage = Strings.Android.UsernameInvalidShort;
                }
                else if (_username.Length > 32)
                {
                    ErrorMessage = Strings.Android.UsernameInvalidLong;
                }
                else
                {
                    ErrorMessage = Strings.Android.UsernameInvalid;
                }
            }
            else
            {
                IsLoading = true;
                ErrorMessage = null;
            }

            return IsValid;
        }

        public bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }

            if (username.Length < 5)
            {
                return false;
            }

            if (username.Length > 32)
            {
                return false;
            }

            for (int i = 0; i < username.Length; i++)
            {
                if (!MessageHelper.IsValidUsernameSymbol(username[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public RelayCommand SendCommand { get; }
        private async void SendExecute()
        {
            var response = await ProtoService.SendAsync(new SetUsername(_username));
            if (response is Ok)
            {
                NavigationService.GoBack();
            }
            else if (response is Error error)
            {
                if (error.CodeEquals(TLErrorCode.FLOOD))
                {
                    //this.HasError = true;
                    //this.Error = Strings.Resources.FloodWaitString;
                    //Telegram.Api.Helpers.Dispatch(delegate
                    //{
                    //    MessageBox.Show(Strings.Resources.FloodWaitString, Strings.Resources.Error, 0);
                    //});
                }
                else if (error.CodeEquals(TLErrorCode.INTERNAL))
                {
                    //StringBuilder messageBuilder = new StringBuilder();
                    //messageBuilder.AppendLine(Strings.Resources.ServerErrorMessage);
                    //messageBuilder.AppendLine();
                    //messageBuilder.AppendLine("Method: account.updateUsername");
                    //messageBuilder.AppendLine("Result: " + error);
                    //this.HasError = true;
                    //this.Error = Strings.Resources.ServerError;
                    //Telegram.Api.Helpers.Dispatch(delegate
                    //{
                    //    MessageBox.Show(messageBuilder.ToString(), Strings.Resources.ServerError, 0);
                    //});
                }
                else if (error.CodeEquals(TLErrorCode.BAD_REQUEST))
                {
                    if (error.TypeEquals(TLErrorType.USERNAME_INVALID))
                    {
                        //this.HasError = true;
                        //this.Error = Strings.Resources.UsernameInvalid;
                        //Telegram.Api.Helpers.Dispatch(delegate
                        //{
                        //    MessageBox.Show(Strings.Resources.UsernameInvalid, Strings.Resources.Error, 0);
                        //});
                    }
                    else if (error.TypeEquals(TLErrorType.USERNAME_OCCUPIED))
                    {
                        //this.HasError = true;
                        //this.Error = Strings.Resources.UsernameOccupied;
                        //Telegram.Api.Helpers.Dispatch(delegate
                        //{
                        //    MessageBox.Show(Strings.Resources.UsernameOccupied, Strings.Resources.Error, 0);
                        //});
                    }
                    else if (error.TypeEquals(TLErrorType.USERNAME_NOT_MODIFIED))
                    {
                        NavigationService.GoBack();
                    }
                    else
                    {
                        //this.HasError = true;
                        //this.Error = error.ToString();
                    }
                }
                else
                {
                    //this.HasError = true;
                    //this.Error = string.Empty;
                    //Telegram.Api.Helpers.Execute.ShowDebugMessage("account.updateUsername error " + error);
                }
            }
        }

        public RelayCommand CopyCommand { get; }
        private async void CopyExecute()
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(MeUrlPrefixConverter.Convert(_username));
            ClipboardEx.TrySetContent(dataPackage);

            await TLMessageDialog.ShowAsync(Strings.Android.LinkCopied, Strings.Android.AppName, Strings.Android.OK);
        }
    }
}
