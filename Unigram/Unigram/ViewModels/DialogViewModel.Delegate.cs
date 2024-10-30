﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TdWindows;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Controls.Views;
using Unigram.ViewModels.Dialogs;
using Unigram.Views;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Unigram.Services;

namespace Unigram.ViewModels
{
    public partial class DialogViewModel : IMessageDelegate
    {
        private FileContext<MessageViewModel> _filesMap = new FileContext<MessageViewModel>();
        private FileContext<MessageViewModel> _photosMap = new FileContext<MessageViewModel>();

        public bool CanBeDownloaded(MessageViewModel message)
        {
            var content = message.Content as object;
            if (content is MessageAnimation animationMessage)
            {
                content = animationMessage.Animation;
            }
            else if (content is MessageAudio audioMessage)
            {
                content = audioMessage.Audio;
            }
            else if (content is MessageDocument documentMessage)
            {
                content = documentMessage.Document;
            }
            else if (content is MessageGame gameMessage)
            {
                if (gameMessage.Game.Animation != null)
                {
                    content = gameMessage.Game.Animation;
                }
                else if (gameMessage.Game.Photo != null)
                {
                    content = gameMessage.Game.Photo;
                }
            }
            else if (content is MessageInvoice invoiceMessage)
            {
                content = invoiceMessage.Photo;
            }
            else if (content is MessageLocation locationMessage)
            {
                content = locationMessage.Location;
            }
            else if (content is MessagePhoto photoMessage)
            {
                content = photoMessage.Photo;
            }
            else if (content is MessageSticker stickerMessage)
            {
                content = stickerMessage.Sticker;
            }
            else if (content is MessageText textMessage)
            {
                if (textMessage?.WebPage?.Animation != null)
                {
                    content = textMessage?.WebPage?.Animation;
                }
                else if (textMessage?.WebPage?.Document != null)
                {
                    content = textMessage?.WebPage?.Document;
                }
                else if (textMessage?.WebPage?.Sticker != null)
                {
                    content = textMessage?.WebPage?.Sticker;
                }
                else if (textMessage?.WebPage?.Video != null)
                {
                    content = textMessage?.WebPage?.Video;
                }
                else if (textMessage?.WebPage?.VideoNote != null)
                {
                    content = textMessage?.WebPage?.VideoNote;
                }
                // PHOTO SHOULD ALWAYS BE AT THE END!
                else if (textMessage?.WebPage?.Photo != null)
                {
                    content = textMessage?.WebPage?.Photo;
                }
            }
            else if (content is MessageVideo videoMessage)
            {
                content = videoMessage.Video;
            }
            else if (content is MessageVideoNote videoNoteMessage)
            {
                content = videoNoteMessage.VideoNote;
            }
            else if (content is MessageVoiceNote voiceNoteMessage)
            {
                content = voiceNoteMessage.VoiceNote;
            }

            var chat = _chat;
            if (chat == null)
            {
                return false;
            }

            if (content is Animation animation)
            {
                return ProtoService.Preferences.ShouldDownloadAnimation(GetChatType(chat), new NetworkTypeWiFi());
            }
            else if (content is Audio audio)
            {

            }
            else if (content is Document document)
            {
                return ProtoService.Preferences.ShouldDownloadDocument(GetChatType(chat), new NetworkTypeWiFi(), document.DocumentData.Size);
            }
            else if (content is Photo photo)
            {
                return ProtoService.Preferences.ShouldDownloadPhoto(GetChatType(chat), new NetworkTypeWiFi());
            }
            else if (content is Sticker sticker)
            {

            }
            else if (content is Video video)
            {
                return ProtoService.Preferences.ShouldDownloadVideo(GetChatType(chat), new NetworkTypeWiFi(), video.VideoData.Size);
            }
            else if (content is VideoNote videoNote)
            {
                return ProtoService.Preferences.ShouldDownloadVideoNote(GetChatType(chat), new NetworkTypeWiFi());
            }
            else if (content is VoiceNote voiceNote)
            {
                return ProtoService.Preferences.ShouldDownloadVoiceNote(GetChatType(chat), new NetworkTypeWiFi());
            }

            return false;
        }

        private AutoDownloadChat GetChatType(Chat chat)
        {
            if (chat.Type is ChatTypeSupergroup supergroup && !supergroup.IsChannel || chat.Type is ChatTypePrivate)
            {
                return AutoDownloadChat.Group;
            }
            else if (chat.Type is ChatTypePrivate || chat.Type is ChatTypeSecret)
            {
                var user = ProtoService.GetUser(chat);
                if (user == null)
                {
                    return AutoDownloadChat.OtherPrivateChat;
                }
                else if (user.OutgoingLink is LinkStateIsContact)
                {
                    return AutoDownloadChat.Contact;
                }
            }

            return AutoDownloadChat.Channel;
        }

        public void DownloadFile(MessageViewModel message, File file)
        {
            ProtoService.Send(new DownloadFile(file.Id, 1));
        }

        public bool TryGetMessagesForFileId(int fileId, out IList<MessageViewModel> items)
        {
            if (_filesMap.TryGetValue(fileId, out List<MessageViewModel> messages))
            {
                items = messages;
                return true;
            }

            items = null;
            return false;
        }

        public bool TryGetMessagesForPhotoId(int fileId, out IList<MessageViewModel> items)
        {
            if (_photosMap.TryGetValue(fileId, out List<MessageViewModel> messages))
            {
                items = messages;
                return true;
            }

            items = null;
            return false;
        }



        public async void OpenReply(MessageViewModel message)
        {
            await LoadMessageSliceAsync(message.Id, message.ReplyToMessageId);
        }



        public async void OpenFile(File file)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                try
                {
                    var temp = await StorageFile.GetFileFromPathAsync(file.Local.Path);
                    var result = await Windows.System.Launcher.LaunchFileAsync(temp);
                    //var folder = await temp.GetParentAsync();
                    //var options = new Windows.System.FolderLauncherOptions();
                    //options.ItemsToSelect.Add(temp);

                    //var result = await Windows.System.Launcher.LaunchFolderAsync(folder, options);
                }
                catch { }
            }
        }

        public void OpenWebPage(WebPage webPage)
        {
            if (webPage.HasInstantView)
            {
                NavigationService.Navigate(typeof(InstantPage), webPage.Url);
            }
            else if (string.Equals(webPage.Type, "telegram_megagroup", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(webPage.Type, "telegram_channel", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(webPage.Type, "telegram_message", StringComparison.OrdinalIgnoreCase))
            {
                MessageHelper.OpenTelegramUrl(ProtoService, NavigationService, webPage.Url);
            }
        }

        public async void OpenSticker(Sticker sticker)
        {
            if (sticker.SetId != 0)
            {
                await StickerSetView.GetForCurrentView().ShowAsync(sticker.SetId, Sticker_Click);
            }
        }

        public async void OpenLocation(Location location, string title)
        {
            if (title != null)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format(CultureInfo.InvariantCulture, "bingmaps:?collection=point.{0}_{1}_{2}", location.Latitude, location.Longitude, WebUtility.UrlEncode(title))));
            }
            else
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format(CultureInfo.InvariantCulture, "bingmaps:?collection=point.{0}_{1}", location.Latitude, location.Longitude)));
            }
        }

        public void OpenInlineButton(MessageViewModel message, InlineKeyboardButton button)
        {
            KeyboardButtonExecute(message, button);
        }



        public async void OpenUsername(string username)
        {
            var response = await ProtoService.SendAsync(new SearchPublicChat(username));
            if (response is Chat chat)
            {
                if (chat.Type is ChatTypePrivate privata)
                {
                    var user = ProtoService.GetUser(privata.UserId);
                    if (user?.Type is UserTypeBot)
                    {
                        NavigationService.NavigateToChat(chat);
                    }
                    else
                    {
                        NavigationService.Navigate(typeof(ProfilePage), chat.Id);
                    }
                }
                else
                {
                    NavigationService.NavigateToChat(chat);
                }
            }
        }

        public async void OpenUser(int userId)
        {
            var response = await ProtoService.SendAsync(new CreatePrivateChat(userId, false));
            if (response is Chat chat)
            {
                var user = ProtoService.GetUser(userId);
                if (user?.Type is UserTypeBot)
                {
                    NavigationService.NavigateToChat(chat);
                }
                else
                {
                    NavigationService.Navigate(typeof(ProfilePage), chat.Id);
                }
            }
        }

        public void OpenViaBot(int viaBotUserId)
        {
            var chat = Chat;
            if (chat != null && chat.Type is ChatTypeSupergroup super && super.IsChannel)
            {
                var supergroup = ProtoService.GetSupergroup(super.SupergroupId);
                if (supergroup != null && !supergroup.CanPostMessages())
                {
                    return;
                }
            }

            var user = ProtoService.GetUser(viaBotUserId);
            if (user != null)
            {
                SetText($"@{user.Username} ");
                ResolveInlineBot(user.Username);
            }
        }

        public void OpenChat(long chatId)
        {
            var chat = ProtoService.GetChat(chatId);
            if (chat == null)
            {
                return;
            }

            NavigationService.NavigateToChat(chat);
        }

        public void OpenChat(long chatId, long messageId)
        {
            var chat = ProtoService.GetChat(chatId);
            if (chat == null)
            {
                return;
            }

            NavigationService.NavigateToChat(chat, message: messageId);
        }

        public async void OpenUrl(string url, bool untrust)
        {
            var navigation = url;
            if (navigation.StartsWith("http") == false)
            {
                navigation = "http://" + url;
            }

            if (Uri.TryCreate(navigation, UriKind.Absolute, out Uri uri))
            {
                if (MessageHelper.IsTelegramUrl(uri))
                {
                    //HandleTelegramUrl(message, navigation);
                    MessageHelper.OpenTelegramUrl(ProtoService, NavigationService, url);
                }
                else
                {
                    //if (message?.Media is TLMessageMediaWebPage webpageMedia)
                    //{
                    //    if (webpageMedia.WebPage is TLWebPage webpage && webpage.HasCachedPage && webpage.Url.Equals(navigation))
                    //    {
                    //        var service = WindowWrapper.Current().NavigationServices.GetByFrameId("Main");
                    //        if (service != null)
                    //        {
                    //            service.Navigate(typeof(InstantPage), webpageMedia);
                    //            return;
                    //        }
                    //    }
                    //}

                    if (untrust)
                    {
                        var confirm = await TLMessageDialog.ShowAsync(string.Format(Strings.Android.OpenUrlAlert, url), Strings.Android.AppName, Strings.Android.OK, Strings.Android.Cancel);
                        if (confirm != ContentDialogResult.Primary)
                        {
                            return;
                        }
                    }

                    try
                    {
                        await Windows.System.Launcher.LaunchUriAsync(uri);
                    }
                    catch { }
                }
            }
        }

        public async void OpenMedia(MessageViewModel message, FrameworkElement target)
        {
            if (message.Content is MessageAnimation || message.Content is MessageVideoNote || message.Content is MessageText text && text.WebPage != null && text.WebPage.Animation != null)
            {
                Delegate?.PlayMessage(message);
            }
            else
            {
                GalleryViewModelBase viewModel;
                if ((message.Content is MessagePhoto || message.Content is MessageVideo) && !message.IsBlurred())
                {
                    viewModel = new DialogGalleryViewModel(ProtoService, Aggregator, message.ChatId, message.Get());
                }
                else
                {
                    viewModel = new SingleGalleryViewModel(ProtoService, Aggregator, new GalleryMessageItem(ProtoService, message.Get()));
                }

                await GalleryView.GetForCurrentView().ShowAsync(viewModel, () => target);
            }
        }

        public void PlayMessage(MessageViewModel message)
        {
            if (message.Content is MessageAnimation || message.Content is MessageVideoNote || message.Content is MessageText text && text.WebPage != null && text.WebPage.Animation != null)
            {
                Delegate?.PlayMessage(message);
            }
            else
            {
                if ((message.Content is MessageVideoNote videoNote && !videoNote.IsViewed && !message.IsOutgoing) || (message.Content is MessageVoiceNote voiceNote && !voiceNote.IsListened && !message.IsOutgoing))
                {
                    ProtoService.Send(new OpenMessageContent(message.ChatId, message.Id));
                }

                _playbackService.Enqueue(message.Get());
            }
        }



        public async void SendBotCommand(string command)
        {
            await SendMessageAsync(command);
        }



        public bool IsAdmin(int userId)
        {
            return false;
        }
    }
}
