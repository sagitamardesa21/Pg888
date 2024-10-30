//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Services;

namespace Unigram.Views.Popups
{
    public sealed partial class LoginUrlInfoPopup : ContentPopup
    {
        public LoginUrlInfoPopup(IClientService clientService, LoginUrlInfoRequestConfirmation requestConfirmation)
        {
            InitializeComponent();

            Title = Strings.Resources.OpenUrlTitle;
            Message = string.Format(Strings.Resources.OpenUrlAlert2, requestConfirmation.Url);
            PrimaryButtonText = Strings.Resources.Open;
            SecondaryButtonText = Strings.Resources.Cancel;

            var self = clientService.GetUser(clientService.Options.MyId);
            if (self == null)
            {
                // ??
            }

            TextBlockHelper.SetMarkdown(CheckLabel1, string.Format(Strings.Resources.OpenUrlOption1, requestConfirmation.Domain, self.FullName()));

            if (requestConfirmation.RequestWriteAccess)
            {
                var bot = clientService.GetUser(requestConfirmation.BotUserId);
                if (bot == null)
                {
                    // ??
                }

                CheckBox2.Visibility = Visibility.Visible;
                TextBlockHelper.SetMarkdown(CheckLabel2, string.Format(Strings.Resources.OpenUrlOption2, bot.FullName()));
            }
            else
            {
                CheckBox2.Visibility = Visibility.Collapsed;
            }
        }

        public string Message
        {
            get => TextBlockHelper.GetMarkdown(MessageLabel);
            set => TextBlockHelper.SetMarkdown(MessageLabel, value);
        }

        public FormattedText FormattedMessage
        {
            get => TextBlockHelper.GetFormattedText(MessageLabel);
            set => TextBlockHelper.SetFormattedText(MessageLabel, value);
        }

        public bool HasAccepted
        {
            get
            {
                return CheckBox1.IsChecked == true;
            }
        }

        public bool HasWriteAccess
        {
            get
            {
                return CheckBox2.IsChecked == true;
            }
        }
    }
}
