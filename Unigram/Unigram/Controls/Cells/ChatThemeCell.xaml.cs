//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Services;
using Unigram.Services.Settings;

namespace Unigram.Controls.Cells
{
    public sealed partial class ChatThemeCell : UserControl
    {
        private IClientService _clientService;
        private ChatTheme _theme;

        public ChatThemeCell()
        {
            InitializeComponent();
        }

        public void Update(IClientService clientService, ChatTheme theme)
        {
            _clientService = clientService;
            _theme = theme;

            Name.Text = theme?.Name ?? string.Empty;

            var settings = ActualTheme == ElementTheme.Light ? theme.LightSettings : theme.DarkSettings;
            if (settings == null)
            {
                NoTheme.Visibility = Visibility.Visible;

                Preview.Unload();
                Outgoing.Fill = null;
                Incoming.Fill = null;
                return;
            }

            NoTheme.Visibility = Visibility.Collapsed;

            Preview.UpdateSource(clientService, settings.Background, true);
            Outgoing.Fill = settings.OutgoingMessageFill;
            Incoming.Fill = new SolidColorBrush(ThemeAccentInfo.Colorize(ActualTheme == ElementTheme.Light ? TelegramThemeType.Day : TelegramThemeType.Tinted, settings.AccentColor.ToColor(), "MessageBackgroundBrush"));
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            if (_clientService != null && _theme != null)
            {
                Update(_clientService, _theme);
            }
        }
    }
}
