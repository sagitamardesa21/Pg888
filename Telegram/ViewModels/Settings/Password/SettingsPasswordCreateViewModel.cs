//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Telegram.Navigation;
using Telegram.Navigation.Services;
using Telegram.Services;
using Telegram.Views.Settings.Password;

namespace Telegram.ViewModels.Settings.Password
{
    public class SettingsPasswordCreateViewModel : ViewModelBase
    {
        public SettingsPasswordCreateViewModel(IClientService clientService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(clientService, settingsService, aggregator)
        {
        }

        private string _field1;
        public string Field1
        {
            get => _field1;
            set => Set(ref _field1, value);
        }

        private string _field2;
        public string Field2
        {
            get => _field2;
            set => Set(ref _field2, value);
        }

        public async void Continue()
        {
            var field1 = _field1;
            var field2 = _field2;

            if (string.IsNullOrWhiteSpace(field1))
            {
                // Error
                return;
            }

            if (!string.Equals(field1, field2))
            {
                // Error
                await ShowPopupAsync(Strings.PasswordDoNotMatch, Strings.AppName, Strings.OK);
                return;
            }

            var state = new NavigationState
            {
                { "password", field1 }
            };

            NavigationService.Navigate(typeof(SettingsPasswordHintPage), state: state);
        }
    }
}
