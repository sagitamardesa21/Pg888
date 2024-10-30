//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Telegram.Common;
using Telegram.Controls;
using Telegram.Services;
using Telegram.Td.Api;

namespace Telegram.Views.Popups
{
    public sealed partial class IdenticonPopup : ContentPopup
    {
        public IdenticonPopup(int sessionId, Chat chat)
        {
            InitializeComponent();
            Title = Strings.EncryptionKey;

            PrimaryButtonText = Strings.Close;

            if (chat.Type is ChatTypeSecret secret)
            {
                var service = TLContainer.Current.Resolve<IClientService>(sessionId);

                var secretChat = service.GetSecretChat(secret.SecretChatId);
                if (secretChat == null)
                {
                    return;
                }

                var user = service.GetUser(secret.UserId);
                if (user == null)
                {
                    return;
                }

                var builder = new StringBuilder();

                var hash = secretChat.KeyHash;
                if (hash.Count > 16)
                {
                    var hex = BitConverter.ToString(hash.ToArray()).Replace("-", string.Empty);
                    for (int a = 0; a < 32; a++)
                    {
                        if (a != 0)
                        {
                            if (a % 8 == 0)
                            {
                                builder.Append('\n');
                            }
                            else if (a % 4 == 0)
                            {
                                builder.Append(' ');
                            }
                        }

                        builder.Append(hex.Substring(a * 2, 2));
                        builder.Append(' ');
                    }

                    builder.Append("\n");
                }

                Texture.Source = PlaceholderHelper.GetIdenticon(hash, 192);
                Hash.Text = builder.ToString();

                TextBlockHelper.SetMarkdown(Subtitle, string.Format(Strings.EncryptionKeyDescription, user.FirstName, user.FirstName));
            }
        }
    }
}
