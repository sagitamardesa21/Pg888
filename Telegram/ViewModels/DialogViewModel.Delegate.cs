//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Common;
using Telegram.Controls.Gallery;
using Telegram.Controls.Messages;
using Telegram.Services.Updates;
using Telegram.Td.Api;
using Telegram.ViewModels.Chats;
using Telegram.ViewModels.Gallery;
using Telegram.Views;
using Telegram.Views.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Telegram.ViewModels
{
    public partial class DialogViewModel
    {
        public void ViewVisibleMessages()
        {
            Delegate?.ViewVisibleMessages();
        }

        public void DoubleTapped(MessageViewModel message)
        {
            if (Settings.Appearance.IsQuickReplySelected)
            {
                ReplyToMessage(message);
            }
            else if (message.InteractionInfo != null && message.InteractionInfo.Reactions.IsChosen(ClientService.DefaultReaction))
            {
                ClientService.SendAsync(new RemoveMessageReaction(message.ChatId, message.Id, ClientService.DefaultReaction));
            }
            else
            {
                ClientService.SendAsync(new AddMessageReaction(message.ChatId, message.Id, ClientService.DefaultReaction, false, false));
            }
        }

        public async void OpenReply(MessageViewModel message)
        {
            if (message.ReplyToMessageState == ReplyToMessageState.None)
            {
                if (message.ReplyInChatId == message.ChatId || message.ReplyInChatId == 0)
                {
                    await LoadMessageSliceAsync(message.Id, message.ReplyToMessageId);
                }
                else
                {
                    NavigationService.NavigateToChat(message.ReplyInChatId, message.ReplyToMessageId);
                }
            }
        }

        public async void OpenThread(MessageViewModel message)
        {
            long chatId = message.ChatId;
            long threadId = message.Id;

            long? messageId = null;

            if (message.ChatId == ClientService.Options.RepliesBotChatId)
            {
                if (message.ForwardInfo?.Origin is MessageForwardOriginUser or MessageForwardOriginChat)
                {
                    chatId = message.ForwardInfo.FromChatId;
                    threadId = message.ForwardInfo.FromMessageId;

                    messageId = threadId;
                }
                else if (message.ForwardInfo?.Origin is MessageForwardOriginChannel fromChannel)
                {
                    chatId = fromChannel.ChatId;
                    threadId = fromChannel.MessageId;

                    messageId = threadId;
                }

                var original = await ClientService.SendAsync(new GetMessage(chatId, threadId)) as Message;
                if (original == null || !original.CanGetMessageThread)
                {
                    NavigationService.NavigateToChat(chatId, threadId);
                    return;
                }
            }

            var response = await ClientService.SendAsync(new GetMessageThread(chatId, threadId));
            if (response is MessageThreadInfo)
            {
                NavigationService.NavigateToThread(chatId, threadId, messageId);
            }
        }



        public void OpenWebPage(WebPage webPage)
        {
            if (webPage.InstantViewVersion != 0)
            {
                //if (NavigationService is UnigramNavigationService asdas)
                //{
                //    asdas.NavigateToInstant(webPage.Url);
                //    return;
                //}

                NavigationService.NavigateToInstant(webPage.Url);
            }
            else if (MessageHelper.TryCreateUri(webPage.Url, out Uri uri))
            {
                MessageHelper.OpenTelegramUrl(ClientService, NavigationService, uri);
            }
        }

        public async void OpenSticker(Sticker sticker)
        {
            if (sticker.SetId != 0)
            {
                await StickersPopup.ShowAsync(sticker.SetId, Sticker_Click);
            }
        }

        public void Call(MessageViewModel message, bool video)
        {
            Call(video);
        }

        public async void VotePoll(MessageViewModel message, IList<PollOption> options)
        {
            var poll = message.Content as MessagePoll;
            if (poll == null)
            {
                return;
            }

            var ids = options.Select(x => poll.Poll.Options.IndexOf(x)).ToArray();
            if (ids.IsEmpty())
            {
                return;
            }

            await ClientService.SendAsync(new SetPollAnswer(message.ChatId, message.Id, ids));

            var updated = message.Content as MessagePoll;
            if (updated.Poll.Type is PollTypeQuiz quiz)
            {
                if (quiz.CorrectOptionId == ids[0])
                {
                    Aggregator.Publish(new UpdateConfetti());
                }
                else
                {
                    var container = ListField?.ContainerFromItem(message) as SelectorItem;
                    var root = container?.ContentTemplateRoot as MessageSelector;

                    var bubble = root?.Content as MessageBubble;
                    if (bubble == null)
                    {
                        return;
                    }

                    VisualUtilities.ShakeView(bubble);
                }
            }
        }



        public void OpenUser(long userId)
        {
            _messageDelegate.OpenUser(userId);
        }

        public void OpenViaBot(long viaBotUserId)
        {
            var chat = Chat;
            if (chat != null && chat.Type is ChatTypeSupergroup super && super.IsChannel)
            {
                var supergroup = ClientService.GetSupergroup(super.SupergroupId);
                if (supergroup != null && !supergroup.CanPostMessages())
                {
                    return;
                }
            }

            var user = ClientService.GetUser(viaBotUserId);
            if (user != null && user.HasActiveUsername(out string username))
            {
                SetText($"@{username} ");
                ResolveInlineBot(username);
            }
        }

        public void OpenChat(long chatId, bool profile = false)
        {
            var chat = ClientService.GetChat(chatId);
            if (chat == null)
            {
                return;
            }

            if (profile)
            {
                NavigationService.Navigate(typeof(ProfilePage), chat.Id);
            }
            else
            {
                NavigationService.NavigateToChat(chat);
            }
        }

        public void OpenChat(long chatId, long messageId)
        {
            var chat = ClientService.GetChat(chatId);
            if (chat == null)
            {
                return;
            }

            NavigationService.NavigateToChat(chat, message: messageId);
        }

        public void OpenHashtag(string hashtag)
        {
            Search = new ChatSearchViewModel(ClientService, Settings, Aggregator, this, hashtag);
        }

        public void OpenUrl(string url, bool untrust)
        {
            _messageDelegate.OpenUrl(url, untrust);
        }

        public async void OpenMedia(MessageViewModel message, FrameworkElement target, int timestamp = 0)
        {
            if (message.Content is MessageAudio or MessageVoiceNote)
            {
                _playbackService.Play(message, _threadId);

                if (timestamp > 0)
                {
                    _playbackService.Seek(TimeSpan.FromSeconds(timestamp));
                }
            }
            else if (message.Content is MessagePoll poll)
            {
                await ShowPopupAsync(new PollResultsPopup(ClientService, Settings, Aggregator, _messageDelegate, message.ChatId, message.Id, poll.Poll));
            }
            else if (message.Content is MessageGame game && message.ReplyMarkup is ReplyMarkupInlineKeyboard inline)
            {
                foreach (var row in inline.Rows)
                {
                    foreach (var button in row)
                    {
                        if (button.Type is InlineKeyboardButtonTypeCallbackGame)
                        {
                            OpenInlineButton(message, button);
                        }
                    }
                }
            }
            else
            {
                GalleryViewModelBase viewModel = null;

                var webPage = message.Content is MessageText text ? text.WebPage : null;
                if (webPage != null && webPage.IsInstantGallery())
                {
                    viewModel = await InstantGalleryViewModel.CreateAsync(ClientService, StorageService, Aggregator, message, webPage);
                }

                if (viewModel == null && (message.Content is MessageAnimation || webPage?.Animation != null))
                {
                    Delegate?.PlayMessage(message, target);
                }
                else
                {
                    if (viewModel == null)
                    {
                        if (message.Content is MessageVideoNote or MessagePhoto or MessageVideo && !message.IsSecret())
                        {
                            viewModel = new ChatGalleryViewModel(ClientService, _storageService, Aggregator, message.ChatId, _threadId, message.Get());
                        }
                        else
                        {
                            viewModel = new SingleGalleryViewModel(ClientService, _storageService, Aggregator, new GalleryMessage(ClientService, message.Get()));
                        }
                    }

                    await GalleryView.ShowAsync(viewModel, target != null ? () => target : null, timestamp);
                }

                TextField?.Focus(FocusState.Programmatic);
            }
        }

        public void PlayMessage(MessageViewModel message)
        {
            _playbackService.Play(message, _threadId);
        }



        public async void SendBotCommand(string command)
        {
            await SendMessageAsync(command);
        }



        public void Select(MessageViewModel message)
        {
            if (message.IsService())
            {
                return;
            }

            if (message.MediaAlbumId != 0)
            {
                if (message.Content is MessageAlbum album)
                {
                    foreach (var child in album.Messages)
                    {
                        _selectedItems[child.Id] = child;
                        child.SelectionChanged();
                    }

                    message.SelectionChanged();
                }
                else if (_groupedMessages.TryGetValue(message.MediaAlbumId, out MessageViewModel group))
                {
                    _selectedItems[message.Id] = message;
                    message.SelectionChanged();
                    group.SelectionChanged();
                }
            }
            else
            {
                _selectedItems[message.Id] = message;
                message.SelectionChanged();
            }

            RaisePropertyChanged(nameof(CanForwardSelectedMessages));
            RaisePropertyChanged(nameof(CanDeleteSelectedMessages));
            RaisePropertyChanged(nameof(CanCopySelectedMessage));
            RaisePropertyChanged(nameof(CanReportSelectedMessages));

            RaisePropertyChanged(nameof(SelectedCount));
        }

        public void Unselect(MessageViewModel message)
        {
            if (message.MediaAlbumId != 0)
            {
                if (message.Content is MessageAlbum album)
                {
                    foreach (var child in album.Messages)
                    {
                        _selectedItems.TryRemove(child.Id, out _);
                        child.SelectionChanged();
                    }

                    message.SelectionChanged();
                }
                else if (_groupedMessages.TryGetValue(message.MediaAlbumId, out MessageViewModel group))
                {
                    _selectedItems.TryRemove(message.Id, out _);
                    message.SelectionChanged();
                    group.SelectionChanged();
                }
            }
            else
            {
                _selectedItems.TryRemove(message.Id, out _);
                message.SelectionChanged();
            }

            RaisePropertyChanged(nameof(CanForwardSelectedMessages));
            RaisePropertyChanged(nameof(CanDeleteSelectedMessages));
            RaisePropertyChanged(nameof(CanCopySelectedMessage));
            RaisePropertyChanged(nameof(CanReportSelectedMessages));

            RaisePropertyChanged(nameof(SelectedCount));
        }
    }
}
