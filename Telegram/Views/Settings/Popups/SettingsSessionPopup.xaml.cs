//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Common;
using Telegram.Controls;
using Telegram.Controls.Cells;
using Telegram.Converters;
using Telegram.Td.Api;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Telegram.Views.Settings.Popups
{
    public sealed partial class SettingsSessionPopup : ContentPopup
    {
        public SettingsSessionPopup(Session session)
        {
            InitializeComponent();

            var icon = SessionCell.IconForSession(session);

            IconBackground.Background = new SolidColorBrush(icon.Backgroud);

            if (icon.Animation != null)
            {
                Icon.ColorReplacements = new Dictionary<int, int> { { 0x000000, icon.Backgroud.ToValue() } };
                Icon.FrameSize = new Size(50, 50);
                Icon.DecodeFrameType = DecodePixelType.Logical;
                Icon.Source = new Uri($"ms-appx:///Assets/Animations/Device{icon.Animation}.json");
            }
            else
            {

            }

            Title.Text = session.DeviceModel;
            Subtitle.Text = Formatter.DateExtended(session.LastActiveDate);

            Application.Badge = string.Format("{0} {1}", session.ApplicationName, session.ApplicationVersion);
            Location.Badge = session.Country;
            Address.Badge = session.Ip;

            AcceptCalls.IsChecked = session.CanAcceptCalls;

            AcceptSecretChats.IsChecked = session.CanAcceptSecretChats;
            AcceptSecretChatsPanel.Visibility = session.ApiId == 2040 || session.ApiId == 2496
                ? Visibility.Collapsed
                : Visibility.Visible;

            PrimaryButtonText = Strings.Terminate;
            SecondaryButtonText = Strings.Done;
        }

        public bool CanAcceptCalls => AcceptCalls.IsChecked == true;

        public bool CanAcceptSecretChats => AcceptSecretChats.IsChecked == true;

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            await Task.Delay(1000);
            Icon.Play();
        }

        private void AcceptCallsPanel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AcceptCalls.IsChecked = AcceptCalls.IsChecked != true;
        }

        private void AcceptSecretChatsPanel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AcceptSecretChats.IsChecked = AcceptSecretChats.IsChecked != true;
        }
    }
}
