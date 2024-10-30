//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls.Messages;
using Unigram.ViewModels.Drawers;
using Unigram.ViewModels.Settings;

namespace Unigram.Views.Settings
{
    public sealed partial class SettingsQuickReactionPage : HostedPage
    {
        public SettingsQuickReactionViewModel ViewModel => DataContext as SettingsQuickReactionViewModel;

        public SettingsQuickReactionPage()
        {
            InitializeComponent();
            Title = Strings.Resources.DoubleTapSetting;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Handle();
            ViewModel.Aggregator.Subscribe<UpdateDefaultReactionType>(this, Handle);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Aggregator.Unsubscribe(this);
        }

        private void Handle(UpdateDefaultReactionType update)
        {
            this.BeginOnUIThread(Handle);
        }

        private async void Handle()
        {
            var reaction = ViewModel.ClientService.DefaultReaction;
            if (reaction is ReactionTypeEmoji emoji)
            {
                var response = await ViewModel.ClientService.SendAsync(new GetEmojiReaction(emoji.Emoji));
                if (response is EmojiReaction emojiReaction)
                {
                    Icon.SetReaction(ViewModel.ClientService, emojiReaction);
                }
            }
            else if (reaction is ReactionTypeCustomEmoji customEmoji)
            {
                Icon.SetCustomEmoji(ViewModel.ClientService, customEmoji.CustomEmojiId);
            }
        }

        private void Reaction_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutReactions.ShowAt(ViewModel.ClientService, EmojiDrawerMode.Reactions, IconPanel, HorizontalAlignment.Right);
        }
    }
}
