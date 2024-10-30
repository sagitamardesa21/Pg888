//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Controls.Gallery;
using Unigram.ViewModels;
using Unigram.ViewModels.Chats;

namespace Unigram.Views.Chats
{
    public sealed partial class ChatSharedMediaPage : ChatSharedMediaPageBase
    {
        public ChatSharedMediaPage()
        {
            InitializeComponent();
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            args.ItemContainer.Tag = args.Item;

            var message = args.Item as MessageWithOwner;
            if (message == null)
            {
                return;
            }

            AutomationProperties.SetName(args.ItemContainer,
                Automation.GetSummary(message, true));

            if (args.ItemContainer.ContentTemplateRoot is Grid content)
            {
                var photo = content.Children[0] as ImageView;
                photo.Tag = message;
                content.Tag = message;

                if (message.Content is MessagePhoto photoMessage)
                {
                    var small = photoMessage.Photo.GetSmall();
                    photo.SetSource(ViewModel.ClientService, small.Photo);
                }
                else if (message.Content is MessageVideo videoMessage && videoMessage.Video.Thumbnail != null)
                {
                    photo.SetSource(ViewModel.ClientService, videoMessage.Video.Thumbnail.File);

                    var panel = content.Children[1] as Grid;
                    var duration = panel.Children[1] as TextBlock;
                    duration.Text = videoMessage.Video.GetDuration();
                }
            }
        }

        protected override float TopPadding => 0;

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Photo_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var message = element.Tag as MessageWithOwner;

            var viewModel = new ChatGalleryViewModel(ViewModel.ClientService, ViewModel.StorageService, ViewModel.Aggregator, message.ChatId, 0, message.Get(), true);
            await GalleryView.ShowAsync(viewModel, () => element);
        }
    }
}
