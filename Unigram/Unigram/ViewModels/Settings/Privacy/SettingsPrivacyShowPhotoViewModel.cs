//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Navigation.Services;
using Unigram.Services;
using Unigram.ViewModels.Delegates;

namespace Unigram.ViewModels.Settings.Privacy
{
    public class SettingsPrivacyShowPhotoViewModel : SettingsPrivacyViewModelBase, IDelegable<IUserDelegate>, IHandle<UpdateUserFullInfo>
    {
        public IUserDelegate Delegate { get; set; }

        private readonly IProfilePhotoService _profilePhotoService;

        public SettingsPrivacyShowPhotoViewModel(IClientService clientService, ISettingsService settingsService, IEventAggregator aggregator, IProfilePhotoService profilePhotoService)
            : base(clientService, settingsService, aggregator, new UserPrivacySettingShowProfilePhoto())
        {
            _profilePhotoService = profilePhotoService;
        }

        protected override Task OnNavigatedToAsync(object parameter, NavigationMode mode, NavigationState state)
        {
            if (ClientService.TryGetUserFull(ClientService.Options.MyId, out UserFullInfo userFull))
            {
                Delegate?.UpdateUserFullInfo(null, null, userFull, false, false);
            }
            else
            {
                ClientService.Send(new GetUserFullInfo(ClientService.Options.MyId));
            }

            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override void Subscribe()
        {
            Aggregator.Subscribe<UpdateUserFullInfo>(this, Handle);
        }

        public void Handle(UpdateUserFullInfo update)
        {
            if (update.UserId == ClientService.Options.MyId)
            {
                BeginOnUIThread(() => Delegate?.UpdateUserFullInfo(null, null, update.UserFullInfo, false, false));
            }
        }

        public async void SetPhoto()
        {
            await _profilePhotoService.SetPhotoAsync(NavigationService, null, true);
        }

        public async void CreatePhoto()
        {
            await _profilePhotoService.CreatePhotoAsync(NavigationService, null, true);
        }

        public async void RemovePhoto()
        {
            var popup = new MessagePopup();
            popup.Title = Strings.Resources.RemovePublicPhoto;
            popup.Message = Strings.Resources.RemovePhotoForRestDescription;
            popup.PrimaryButtonText = Strings.Resources.Remove;
            popup.SecondaryButtonText = Strings.Resources.Cancel;
            popup.PrimaryButtonStyle = App.Current.Resources["DangerButtonStyle"] as Style;
            popup.DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.None;

            var confirm = await popup.ShowQueuedAsync(XamlRoot);
            if (confirm == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                if (ClientService.TryGetUserFull(ClientService.Options.MyId, out UserFullInfo userFull))
                {
                    if (userFull.PublicPhoto == null)
                    {
                        return;
                    }

                    ClientService.Send(new DeleteProfilePhoto(userFull.PublicPhoto.Id));
                }
            }
        }
    }
}
