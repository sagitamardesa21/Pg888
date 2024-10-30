//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Unigram.ViewModels.Settings;

namespace Unigram.Views.Settings
{
    public sealed partial class SettingsNotificationsPage : HostedPage
    {
        public SettingsNotificationsViewModel ViewModel => DataContext as SettingsNotificationsViewModel;

        public SettingsNotificationsPage()
        {
            InitializeComponent();
            Title = Strings.Resources.NotificationsAndSounds;
        }

        #region Binding

        private string ConvertCountInfo(bool count)
        {
            return count ? "Switch off to show the number of unread chats instead of messages" : "Switch on to show the number of unread messages instead of chats";
        }

        #endregion

    }
}
