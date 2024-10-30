﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls.Messages.Content;
using Unigram.Converters;
using Unigram.Selectors;
using Unigram.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Unigram.Controls.Messages
{
    public sealed partial class MessageBubble : StackPanel
    {
        private MessageViewModel _message;

        public MessageBubble()
        {
            InitializeComponent();
        }

        public void UpdateAdaptive(HorizontalAlignment alignment)
        {
            UpdateAttach(_message, alignment == HorizontalAlignment.Left);

            HorizontalAlignment = alignment;
        }

        public void UpdateMessage(MessageViewModel message)
        {
            _message = message;
            Tag = message;

            UpdateAttach(message);
            UpdateMessageHeader(message);
            UpdateMessageReply(message);
            UpdateMessageContent(message);

            Footer.UpdateMessage(message);
            Markup.Update(message, message.ReplyMarkup);

            if (_highlight != null)
            {
                _highlight.StopAnimation("Opacity");
                _highlight.Opacity = 0;
            }
        }

        public string GetAutomationName()
        {
            if (_message == null)
            {
                return null;
            }

            return UpdateAutomation(_message);
        }

        public string UpdateAutomation(MessageViewModel message)
        {
            var chat = message.GetChat();

            var sticker = message.Content is MessageSticker;
            var light = sticker || message.Content is MessageVideoNote;

            var title = string.Empty;

            if (!light && message.IsFirst && !message.IsOutgoing && !message.IsChannelPost && (chat.Type is ChatTypeBasicGroup || chat.Type is ChatTypeSupergroup))
            {
                var sender = message.GetSenderUser();
                title = sender?.GetFullName();
            }
            else if (!light && message.IsChannelPost && chat.Type is ChatTypeSupergroup)
            {
                title = message.ProtoService.GetTitle(chat);
            }
            else if (!light && message.IsFirst && message.IsSaved())
            {
                if (message.ForwardInfo is MessageForwardedFromUser fromUser)
                {
                    title = message.ProtoService.GetUser(fromUser.SenderUserId)?.GetFullName();
                }
                else if (message.ForwardInfo is MessageForwardedPost post)
                {
                    title = message.ProtoService.GetTitle(message.ProtoService.GetChat(post.ChatId));
                }
                else if (message.ForwardInfo is MessageForwardedFromHiddenUser fromHiddenUser)
                {
                    title = fromHiddenUser.SenderName;
                }
            }

            var builder = new StringBuilder();
            if (title?.Length > 0)
            {
                builder.AppendLine($"{title}. ");
            }

            if (message.ReplyToMessage != null)
            {
                var user = message.ProtoService.GetUser(message.ReplyToMessage.SenderUserId);
                if (user != null)
                {
                    builder.AppendLine($"{Strings.Resources.AccDescrReplying} {user.GetFullName()}. ");
                }
            }

            builder.Append(Automation.GetSummary(message.ProtoService, message.Get()));

            var date = string.Format(Strings.Resources.TodayAtFormatted, BindConvert.Current.ShortTime.Format(Utils.UnixTimestampToDateTime(message.Date)));
            if (message.IsOutgoing)
            {
                builder.Append(string.Format(Strings.Resources.AccDescrSentDate, date));
            }
            else
            {
                builder.Append(string.Format(Strings.Resources.AccDescrReceivedDate, date));
            }

            builder.Append(". ");

            return builder.ToString();
        }

        public void UpdateAttach(MessageViewModel message, bool wide = false)
        {
            var topLeft = 15d;
            var topRight = 15d;
            var bottomRight = 15d;
            var bottomLeft = 15d;

            if (message.IsOutgoing && !wide)
            {
                if (message.IsFirst && message.IsLast)
                {
                }
                else if (message.IsFirst)
                {
                    bottomRight = 4;
                }
                else if (message.IsLast)
                {
                    topRight = 4;
                }
                else
                {
                    topRight = 4;
                    bottomRight = 4;
                }
            }
            else
            {
                if (message.IsFirst && message.IsLast)
                {
                }
                else if (message.IsFirst)
                {
                    bottomLeft = 4;
                }
                else if (message.IsLast)
                {
                    topLeft = 4;
                }
                else
                {
                    topLeft = 4;
                    bottomLeft = 4;
                }
            }

            if (message.ReplyMarkup is ReplyMarkupInlineKeyboard)
            {
                if (!(message.Content is MessageSticker || message.Content is MessageVideoNote))
                {
                    ContentPanel.CornerRadius = new CornerRadius(topLeft, topRight, 4, 4);
                }

                Markup.CornerRadius = new CornerRadius(4, 4, bottomRight, bottomLeft);
            }
            else if (message.Content is MessageSticker || message.Content is MessageVideoNote)
            {
                ContentPanel.CornerRadius = new CornerRadius();
            }
            else
            {
                ContentPanel.CornerRadius = new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            }

            Margin = new Thickness(0, message.IsFirst ? 2 : 1, 0, message.IsLast ? 2 : 1);

            //UpdateMessageContent(message, true);
        }

        public void UpdateMessageReply(MessageViewModel message)
        {
            if (Reply == null && message.ReplyToMessageId != 0)
            {
                FindName("Reply");
            }

            if (Reply != null)
            {
                Reply.UpdateMessageReply(message);
            }
        }

        private void MaybeUseInner(ref MessageViewModel message)
        {
            if (message.Content is MessageChatEvent chatEvent)
            {
                if (chatEvent.Event.Action is ChatEventMessageDeleted messageDeleted)
                {
                    message = new MessageViewModel(message.ProtoService, message.PlaybackService, message.Delegate, messageDeleted.Message) { IsFirst = true, IsLast = true, IsOutgoing = false };
                }
                else if (chatEvent.Event.Action is ChatEventMessageEdited messageEdited)
                {
                    message = new MessageViewModel(message.ProtoService, message.PlaybackService, message.Delegate, messageEdited.NewMessage) { IsFirst = true, IsLast = true, IsOutgoing = false };
                }
                else if (chatEvent.Event.Action is ChatEventMessagePinned messagePinned)
                {
                    message = new MessageViewModel(message.ProtoService, message.PlaybackService, message.Delegate, messagePinned.Message) { IsFirst = true, IsLast = true, IsOutgoing = false };
                }
            }
        }

        public void UpdateMessageHeader(MessageViewModel message)
        {
            MaybeUseInner(ref message);

            var paragraph = HeaderLabel;
            var admin = AdminLabel;
            var parent = Header;

            paragraph.Inlines.Clear();

            if (message == null)
            {
                return;
            }

            var chat = message.GetChat();

            var sticker = message.Content is MessageSticker;
            var light = sticker || message.Content is MessageVideoNote;
            var shown = false;

            if (!light && message.IsFirst && !message.IsOutgoing && !message.IsChannelPost && (chat.Type is ChatTypeBasicGroup || chat.Type is ChatTypeSupergroup))
            {
                var sender = message.GetSenderUser();

                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(new Run { Text = sender?.GetFullName() });
                hyperlink.UnderlineStyle = UnderlineStyle.None;
                hyperlink.Foreground = PlaceholderHelper.GetBrush(message.SenderUserId);
                hyperlink.Click += (s, args) => From_Click(message);

                paragraph.Inlines.Add(hyperlink);
                shown = true;
            }
            else if (!light && message.IsChannelPost && chat.Type is ChatTypeSupergroup)
            {
                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(new Run { Text = message.ProtoService.GetTitle(chat) });
                hyperlink.UnderlineStyle = UnderlineStyle.None;
                //hyperlink.Foreground = Convert.Bubble(message.ChatId);
                hyperlink.Click += (s, args) => From_Click(message);

                paragraph.Inlines.Add(hyperlink);
                shown = true;
            }
            else if (!light && message.IsFirst && message.IsSaved())
            {
                var title = string.Empty;
                if (message.ForwardInfo is MessageForwardedFromUser fromUser)
                {
                    title = message.ProtoService.GetUser(fromUser.SenderUserId)?.GetFullName();
                }
                else if (message.ForwardInfo is MessageForwardedPost post)
                {
                    title = message.ProtoService.GetTitle(message.ProtoService.GetChat(post.ChatId));
                }
                else if (message.ForwardInfo is MessageForwardedFromHiddenUser fromHiddenUser)
                {
                    title = fromHiddenUser.SenderName;
                }

                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(new Run { Text = title ?? string.Empty });
                hyperlink.UnderlineStyle = UnderlineStyle.None;
                //hyperlink.Foreground = Convert.Bubble(message.FwdFrom?.FromId ?? message.FwdFrom?.ChannelId ?? 0);
                hyperlink.Click += (s, args) => FwdFrom_Click(message);

                paragraph.Inlines.Add(hyperlink);
                shown = true;
            }

            if (shown)
            {
                if (admin != null && !message.IsOutgoing && message.Delegate != null && message.Delegate.IsAdmin(message.SenderUserId))
                {
                    paragraph.Inlines.Add(new Run { Text = " " + Strings.Resources.ChatAdmin, Foreground = null });
                }
            }

            var forward = false;

            if (message.ForwardInfo != null && !sticker && !message.IsSaved())
            {
                if (paragraph.Inlines.Count > 0)
                    paragraph.Inlines.Add(new LineBreak());

                paragraph.Inlines.Add(new Run { Text = Strings.Resources.ForwardedMessage, FontWeight = FontWeights.Normal });
                paragraph.Inlines.Add(new LineBreak());
                paragraph.Inlines.Add(new Run { Text = Strings.Resources.From + " ", FontWeight = FontWeights.Normal });

                var title = string.Empty;
                if (message.ForwardInfo is MessageForwardedFromUser fromUser)
                {
                    title = message.ProtoService.GetUser(fromUser.SenderUserId)?.GetFullName();
                }
                else if (message.ForwardInfo is MessageForwardedPost post)
                {
                    title = message.ProtoService.GetTitle(message.ProtoService.GetChat(post.ChatId));
                }
                else if (message.ForwardInfo is MessageForwardedFromHiddenUser fromHiddenUser)
                {
                    title = fromHiddenUser.SenderName;
                }

                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(new Run { Text = title });
                hyperlink.UnderlineStyle = UnderlineStyle.None;
                hyperlink.Foreground = light ? new SolidColorBrush(Colors.White) : paragraph.Foreground;
                hyperlink.Click += (s, args) => FwdFrom_Click(message);

                paragraph.Inlines.Add(hyperlink);
                forward = true;
            }

            //if (message.HasViaBotId && message.ViaBot != null && !message.ViaBot.IsDeleted && message.ViaBot.HasUsername)
            var viaBot = message.ProtoService.GetUser(message.ViaBotUserId);
            if (viaBot != null && viaBot.Type is UserTypeBot && !string.IsNullOrEmpty(viaBot.Username))
            {
                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(new Run { Text = (paragraph.Inlines.Count > 0 ? " via @" : "via @"), FontWeight = FontWeights.Normal });
                hyperlink.Inlines.Add(new Run { Text = viaBot.Username });
                hyperlink.UnderlineStyle = UnderlineStyle.None;
                hyperlink.Foreground = light ? new SolidColorBrush(Colors.White) : paragraph.Foreground;
                hyperlink.Click += (s, args) => ViaBot_Click(message);

                if (paragraph.Inlines.Count > 0 && !forward)
                {
                    paragraph.Inlines.Insert(1, hyperlink);
                }
                else
                {
                    paragraph.Inlines.Add(hyperlink);
                }
            }

            if (paragraph.Inlines.Count > 0)
            {
                if (admin != null && shown && !message.IsOutgoing && message.Delegate != null && message.Delegate.IsAdmin(message.SenderUserId))
                {
                    admin.Visibility = Visibility.Visible;
                }
                else if (admin != null)
                {
                    admin.Visibility = Visibility.Collapsed;
                }

                paragraph.Inlines.Add(new Run { Text = " " });
                paragraph.Visibility = Visibility.Visible;
                parent.Visibility = Visibility.Visible;
            }
            else
            {
                if (admin != null)
                {
                    admin.Visibility = Visibility.Collapsed;
                }

                paragraph.Visibility = Visibility.Collapsed;
                parent.Visibility = message.ReplyToMessageId != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ViaBot_Click(MessageViewModel message)
        {
            message.Delegate.OpenViaBot(message.ViaBotUserId);
        }

        private async void FwdFrom_Click(MessageViewModel message)
        {
            if (message.ForwardInfo is MessageForwardedFromUser fromUser)
            {
                message.Delegate.OpenUser(fromUser.SenderUserId);
            }
            else if (message.ForwardInfo is MessageForwardedPost post)
            {
                // TODO: verify if this is sufficient
                message.Delegate.OpenChat(post.ChatId, post.MessageId);
            }
            else if (message.ForwardInfo is MessageForwardedFromHiddenUser fromHiddenUser)
            {
                await TLMessageDialog.ShowAsync(Strings.Resources.HidAccount, Strings.Resources.AppName, Strings.Resources.OK);
            }
        }

        private void From_Click(MessageViewModel message)
        {
            if (message.IsChannelPost)
            {
                message.Delegate.OpenChat(message.ChatId);
            }
            else
            {
                message.Delegate.OpenUser(message.SenderUserId);
            }
        }


        //private Thickness UpdateFirst(bool isFirst)
        //{
        //    OnMessageChanged(HeaderLabel, AdminLabel, Header);
        //    return isFirst ? new Thickness(0, 2, 0, 0) : new Thickness();
        //}

        //private void OnMediaChanged(object sender, EventArgs e)
        //{
        //    OnMediaChanged();
        //}

        public void UpdateMessageState(MessageViewModel message)
        {
            Footer.UpdateMessageState(message);
        }

        public void UpdateMessageEdited(MessageViewModel message)
        {
            Footer.UpdateMessageEdited(message);
            Markup.Update(message, message.ReplyMarkup);
        }

        public void UpdateMessageViews(MessageViewModel message)
        {
            Footer.UpdateMessageViews(message);
        }

        public void UpdateMessageContentOpened(MessageViewModel message)
        {
            if (Media.Child is IContentWithFile content && content.IsValid(message.Content, true))
            {
                content.UpdateMessageContentOpened(message);
            }
        }

        public void UpdateMessageContent(MessageViewModel message, bool padding = false)
        {
            MaybeUseInner(ref message);

            string display = null;

            //if (message == null || message.Media == null || message.Media is TLMessageMediaEmpty || empty)
            if (message.Content is MessageText text && text.WebPage == null)
            {
                display = text.Text.Text;

                Media.Margin = new Thickness(0);
                Placeholder.Visibility = Visibility.Visible;
                FooterToNormal();
                Grid.SetRow(Footer, 2);
                Grid.SetRow(Message, 2);
            }
            else if (IsFullMedia(message.Content))
            {
                var left = -10;
                var top = -4;
                var right = -10;
                var bottom = -6;

                if (!(message.Content is MessageVenue))
                {
                    var chat = message.GetChat();
                    if (message.IsFirst && !message.IsOutgoing && !message.IsChannelPost && (chat.Type is ChatTypeBasicGroup || chat.Type is ChatTypeSupergroup))
                    {
                        top = 4;
                    }
                    if (message.IsFirst && message.IsSaved())
                    {
                        top = 4;
                    }
                    if ((message.ForwardInfo != null && !message.IsSaved()) || message.ViaBotUserId != 0 || message.ReplyToMessageId != 0 || message.IsChannelPost)
                    {
                        top = 4;
                    }
                }

                var caption = message.Content is MessageVenue || message.Content.HasCaption();
                if (caption)
                {
                    FooterToNormal();
                    bottom = 4;
                }
                else if (message.Content is MessageCall || (message.Content is MessageLocation location && location.LivePeriod > 0 && BindConvert.Current.DateTime(message.Date + location.LivePeriod) > DateTime.Now))
                {
                    FooterToHidden();
                }
                else
                {
                    FooterToMedia();
                }

                Media.Margin = new Thickness(left, top, right, bottom);
                Placeholder.Visibility = caption ? Visibility.Visible : Visibility.Collapsed;
                Grid.SetRow(Footer, caption ? 4 : 3);
                Grid.SetRow(Message, caption ? 4 : 2);
            }
            else if (message.Content is MessageSticker || message.Content is MessageVideoNote)
            {
                Media.Margin = new Thickness(-10, -4, -10, -6);
                Placeholder.Visibility = Visibility.Collapsed;
                FooterToLightMedia(message.IsOutgoing && !message.IsChannelPost);
                Grid.SetRow(Footer, 3);
                Grid.SetRow(Message, 2);
            }
            else if ((message.Content is MessageText webPage && webPage.WebPage != null) || message.Content is MessageGame || (message.Content is MessageContact contact && !string.IsNullOrEmpty(contact.Contact.Vcard)))
            {
                Media.Margin = new Thickness(0);
                Placeholder.Visibility = Visibility.Collapsed;
                FooterToNormal();
                Grid.SetRow(Footer, 4);
                Grid.SetRow(Message, 2);
            }
            else if (message.Content is MessagePoll)
            {
                Media.Margin = new Thickness(0);
                Placeholder.Visibility = Visibility.Collapsed;
                FooterToNormal();
                Grid.SetRow(Footer, 3);
                Grid.SetRow(Message, 2);
            }
            else if (message.Content is MessageInvoice invoice)
            {
                var caption = invoice.Photo == null;

                Media.Margin = new Thickness(0);
                Placeholder.Visibility = caption ? Visibility.Visible : Visibility.Collapsed;
                FooterToNormal();
                Grid.SetRow(Footer, caption ? 3 : 4);
                Grid.SetRow(Message, 2);
            }
            else /*if (IsInlineMedia(message.Media))*/
            {
                var caption = message.Content.HasCaption();
                //if (message.Media is ITLMessageMediaCaption captionMedia)
                //{
                //    display = captionMedia.Caption;
                //    caption = !string.IsNullOrWhiteSpace(captionMedia.Caption);
                //}

                if (message.Content is MessageCall)
                {
                    FooterToHidden();
                }
                else
                {
                    FooterToNormal();
                }

                Media.Margin = new Thickness(0, 4, 0, caption ? 8 : 2);
                Placeholder.Visibility = caption ? Visibility.Visible : Visibility.Collapsed;
                Grid.SetRow(Footer, caption ? 4 : 3);
                Grid.SetRow(Message, caption ? 4 : 2);
            }

            //if (display != null)
            //{
            //    var direction = NativeUtils.GetDirectionality(display);
            //    if (direction == 2)
            //    {
            //        Message.FlowDirection = FlowDirection.RightToLeft;
            //        Footer.HorizontalAlignment = HorizontalAlignment.Left;
            //    }
            //    else
            //    {
            //        Message.FlowDirection = FlowDirection.LeftToRight;
            //        Footer.HorizontalAlignment = HorizontalAlignment.Right;
            //    }
            //}

            if (padding)
            {
                return;
            }

            UpdateMessageText(message);

            if (Media.Child is IContent content && content.IsValid(message.Content, true))
            {
                content.UpdateMessage(message);
            }
            else
            {
                if (message.Content is MessageText textMessage && textMessage.WebPage != null)
                {
                    if (textMessage.WebPage.IsSmallPhoto())
                    {
                        Media.Child = new WebPageSmallPhotoContent(message);
                    }
                    else
                    {
                        Media.Child = new WebPageContent(message);
                    }
                }
                else if (message.Content is MessageAlbum)
                {
                    Media.Child = new AlbumContent(message);
                }
                else if (message.Content is MessageAnimation)
                {
                    Media.Child = new AnimationContent(message);
                }
                else if (message.Content is MessageAudio)
                {
                    Media.Child = new AudioContent(message);
                }
                else if (message.Content is MessageCall)
                {
                    Media.Child = new CallContent(message);
                }
                else if (message.Content is MessageContact)
                {
                    Media.Child = new ContactContent(message);
                }
                else if (message.Content is MessageDocument)
                {
                    Media.Child = new DocumentContent(message);
                }
                else if (message.Content is MessageGame)
                {
                    Media.Child = new GameContent(message);
                }
                else if (message.Content is MessageInvoice invoice)
                {
                    if (invoice.Photo == null)
                    {
                        Media.Child = new InvoiceContent(message);
                    }
                    else
                    {
                        Media.Child = new InvoicePhotoContent(message);
                    }
                }
                else if (message.Content is MessageLocation)
                {
                    Media.Child = new LocationContent(message);
                }
                else if (message.Content is MessagePhoto)
                {
                    Media.Child = new PhotoContent(message);
                }
                else if (message.Content is MessagePoll)
                {
                    Media.Child = new PollContent(message);
                }
                else if (message.Content is MessageSticker)
                {
                    Media.Child = new StickerContent(message);
                }
                else if (message.Content is MessageVenue)
                {
                    Media.Child = new VenueContent(message);
                }
                else if (message.Content is MessageVideo)
                {
                    Media.Child = new VideoContent(message);
                }
                else if (message.Content is MessageVideoNote)
                {
                    Media.Child = new VideoNoteContent(message);
                }
                else if (message.Content is MessageVoiceNote)
                {
                    Media.Child = new VoiceNoteContent(message);
                }
                else
                {
                    Media.Child = null;
                }
            }
        }

        public void UpdateFile(MessageViewModel message, File file)
        {
            if (Media.Child is IContentWithFile content)
            {
                content.UpdateFile(message, file);
            }

            if (Reply != null)
            {
                Reply.UpdateFile(message, file);
            }
        }

        private void UpdateMessageText(MessageViewModel message)
        {
            Span.Inlines.Clear();

            var result = false;
            var adjust = false;

            if (message.Content is MessageText text)
            {
                result = ReplaceEntities(message, Span, text.Text, out adjust);
            }
            else if (message.Content is MessageAlbum album)
            {
                result = ReplaceEntities(message, Span, album.Caption, out adjust);
            }
            else if (message.Content is MessageAnimation animation)
            {
                result = ReplaceEntities(message, Span, animation.Caption, out adjust);
            }
            else if (message.Content is MessageAudio audio)
            {
                result = ReplaceEntities(message, Span, audio.Caption, out adjust);
            }
            else if (message.Content is MessageDocument document)
            {
                result = ReplaceEntities(message, Span, document.Caption, out adjust);
            }
            else if (message.Content is MessagePhoto photo)
            {
                result = ReplaceEntities(message, Span, photo.Caption, out adjust);
            }
            else if (message.Content is MessageVideo video)
            {
                result = ReplaceEntities(message, Span, video.Caption, out adjust);
            }
            else if (message.Content is MessageVoiceNote voiceNote)
            {
                result = ReplaceEntities(message, Span, voiceNote.Caption, out adjust);
            }
            else if (message.Content is MessageUnsupported unsupported)
            {
                result = GetEntities(message, Span, Strings.Resources.UnsupportedMedia, out adjust);
            }
            else if (message.Content is MessageVenue venue)
            {
                Span.Inlines.Add(new Run { Text = venue.Venue.Title, FontWeight = FontWeights.SemiBold });
                Span.Inlines.Add(new LineBreak());
                Span.Inlines.Add(new Run { Text = venue.Venue.Address });
                result = true;
            }

            Message.Visibility = result ? Visibility.Visible : Visibility.Collapsed;
            //Footer.HorizontalAlignment = adjust ? HorizontalAlignment.Left : HorizontalAlignment.Right;

            if (adjust)
            {
                Span.Inlines.Add(new LineBreak());
            }
        }

        private bool GetEntities(MessageViewModel message, Span span, string text, out bool adjust)
        {
            if (string.IsNullOrEmpty(text))
            {
                //Message.Visibility = Visibility.Collapsed;
                adjust = false;
                return false;
            }
            else
            {
                //Message.Visibility = Visibility.Visible;

                var response = message.ProtoService.Execute(new GetTextEntities(text));
                if (response is TextEntities entities)
                {
                    return ReplaceEntities(message, span, text, entities.Entities, out adjust);
                }

                Span.Inlines.Add(new Run { Text = text });

                adjust = false;
                return true;
            }
        }

        private bool ReplaceEntities(MessageViewModel message, Span span, FormattedText text, out bool adjust)
        {
            if (text == null)
            {
                adjust = false;
                return false;
            }

            return ReplaceEntities(message, span, text.Text, text.Entities, out adjust);
        }

        private bool ReplaceEntities(MessageViewModel message, Span span, string text, IList<TextEntity> entities, out bool adjust)
        {
            if (string.IsNullOrEmpty(text))
            {
                adjust = false;
                return false;
            }

            var previous = 0;

            foreach (var entity in entities.OrderBy(x => x.Offset))
            {
                if (entity.Offset > previous)
                {
                    //span.Inlines.Add(new Run { Text = text.Substring(previous, entity.Offset - previous) });
                    AddRawText(new RichTextBlock(), span, text.Substring(previous, entity.Offset - previous), FontStyle.Normal, FontWeights.Normal, new FontFamily("Segoe UI"));
                }

                if (entity.Length + entity.Offset > text.Length)
                {
                    previous = entity.Offset + entity.Length;
                    continue;
                }

                if (entity.Type is TextEntityTypeBold)
                {
                    span.Inlines.Add(new Run { Text = text.Substring(entity.Offset, entity.Length), FontWeight = FontWeights.SemiBold });
                }
                else if (entity.Type is TextEntityTypeItalic)
                {
                    span.Inlines.Add(new Run { Text = text.Substring(entity.Offset, entity.Length), FontStyle = FontStyle.Italic });
                }
                else if (entity.Type is TextEntityTypeCode)
                {
                    span.Inlines.Add(new Run { Text = text.Substring(entity.Offset, entity.Length), FontFamily = new FontFamily("Consolas") });
                }
                else if (entity.Type is TextEntityTypePre || entity.Type is TextEntityTypePreCode)
                {
                    // TODO any additional
                    span.Inlines.Add(new Run { Text = text.Substring(entity.Offset, entity.Length), FontFamily = new FontFamily("Consolas") });
                }
                else if (entity.Type is TextEntityTypeUrl || entity.Type is TextEntityTypeEmailAddress || entity.Type is TextEntityTypePhoneNumber || entity.Type is TextEntityTypeMention || entity.Type is TextEntityTypeHashtag || entity.Type is TextEntityTypeCashtag || entity.Type is TextEntityTypeBotCommand)
                {
                    var hyperlink = new Hyperlink();
                    var data = text.Substring(entity.Offset, entity.Length);

                    hyperlink.Click += (s, args) => Entity_Click(message, entity.Type, data);
                    hyperlink.Inlines.Add(new Run { Text = data });
                    hyperlink.Foreground = GetLinksBrush();
                    //hyperlink.Foreground = foreground;
                    span.Inlines.Add(hyperlink);

                    if (entity.Type is TextEntityTypeUrl)
                    {
                        MessageHelper.SetEntity(hyperlink, data);
                    }
                }
                else if (entity.Type is TextEntityTypeTextUrl || entity.Type is TextEntityTypeMentionName)
                {
                    var hyperlink = new Hyperlink();
                    object data;
                    if (entity.Type is TextEntityTypeTextUrl textUrl)
                    {
                        data = textUrl.Url;
                        MessageHelper.SetEntity(hyperlink, textUrl.Url);
                        ToolTipService.SetToolTip(hyperlink, textUrl.Url);
                    }
                    else if (entity.Type is TextEntityTypeMentionName mentionName)
                    {
                        data = mentionName.UserId;
                    }

                    hyperlink.Click += (s, args) => Entity_Click(message, entity.Type, null);
                    hyperlink.Inlines.Add(new Run { Text = text.Substring(entity.Offset, entity.Length) });
                    hyperlink.Foreground = GetLinksBrush();
                    //hyperlink.Foreground = foreground;
                    span.Inlines.Add(hyperlink);
                }

                previous = entity.Offset + entity.Length;
            }

            if (text.Length > previous)
            {
                //span.Inlines.Add(new Run { Text = text.Substring(previous) });
                AddRawText(new RichTextBlock(), span, text.Substring(previous), FontStyle.Normal, FontWeights.Normal, new FontFamily("Segoe UI"));
            }

            if (AdjustEmojis(span, text))
            {
                adjust = true;
            }
            else if (ApiInfo.FlowDirection == FlowDirection.LeftToRight && MessageHelper.IsAnyCharacterRightToLeft(text))
            {
                //Footer.HorizontalAlignment = HorizontalAlignment.Left;
                //span.Inlines.Add(new LineBreak());
                adjust = true;
            }
            else if (ApiInfo.FlowDirection == FlowDirection.RightToLeft && !MessageHelper.IsAnyCharacterRightToLeft(text))
            {
                //Footer.HorizontalAlignment = HorizontalAlignment.Left;
                //span.Inlines.Add(new LineBreak());
                adjust = true;
            }
            else
            {
                //Footer.HorizontalAlignment = HorizontalAlignment.Right;
                adjust = false;
            }

            return true;
        }

        private bool AdjustEmojis(Span span, string text)
        {
            if (Emojis.TryCountEmojis(text, out int count, 3))
            {
                switch (count)
                {
                    case 1:
                        Message.TextAlignment = TextAlignment.Center;
                        span.FontSize = 32;
                        return true;
                    case 2:
                        Message.TextAlignment = TextAlignment.Center;
                        span.FontSize = 28;
                        return true;
                    case 3:
                        Message.TextAlignment = TextAlignment.Center;
                        span.FontSize = 24;
                        return true;
                }
            }

            Message.TextAlignment = TextAlignment.DetectFromContent;
            span.FontSize = (double)App.Current.Resources["MessageFontSize"];
            return false;
        }

        private Brush GetLinksBrush()
        {
            if (Resources.TryGetValue("MessageForegroundLinkBrush", out object value))
            {
                return value as SolidColorBrush;
            }

            return App.Current.Resources["MessageForegroundLinkBrush"] as SolidColorBrush;
        }

        private static void AddRawText(RichTextBlock text_block, Span par, string raw_text, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily)
        {
            var textEnumerator = StringInfo.GetTextElementEnumerator(raw_text);
            bool cont = false;

            var nextElements = new List<Tuple<string, byte[], string>>();

            //string nextText = null;
            //byte[] nextBytes = null;
            //string nextBytesStr = null;

            StringBuilder sb = new StringBuilder();

            // Note: Begins at element -1 (none).
            cont = textEnumerator.MoveNext();
            while (cont)
            {
                string text;
                byte[] bytes;
                string bytesStr;

                if (nextElements.Count > 0)
                {
                    text = nextElements[0].Item1;
                    bytes = nextElements[0].Item2;
                    bytesStr = nextElements[0].Item3;

                    nextElements.RemoveAt(0);
                }
                else
                {
                    text = textEnumerator.GetTextElement();
                    bytes = Encoding.BigEndianUnicode.GetBytes(text);
                    bytesStr = ConvertToHexString(bytes);
                }

                if (Emoji.FlagsPrefixes.ContainsKey(bytesStr) && textEnumerator.MoveNext())
                {
                    var text2 = textEnumerator.GetTextElement();
                    var bytes2 = Encoding.BigEndianUnicode.GetBytes(text2);
                    var bytesStr2 = ConvertToHexString(bytes2);

                    bytesStr += bytesStr2;
                    text += text2;
                }

                string bytesValue2;
                if (string.IsNullOrEmpty(bytesStr)
                    || Emoji.SkinsDict.TryGetValue(bytesStr, out bytesValue2))
                {
                    if (nextElements.Count == 0)
                    {
                        cont = textEnumerator.MoveNext();
                    }
                    continue;   //skip unknown skin emoji
                }

                string bytesValue;
                TreeNode node;
                //System.Diagnostics.Debug.WriteLine(bytesStr);
                // skinned emoji
                if (Emoji.SkinnedDict.TryGetValue(bytesStr, out bytesValue))
                {
                    if (textEnumerator.MoveNext())
                    {
                        var text2 = textEnumerator.GetTextElement();
                        var bytes2 = Encoding.BigEndianUnicode.GetBytes(text2);
                        var bytesStr2 = ConvertToHexString(bytes2);

                        if (string.IsNullOrEmpty(bytesStr2))
                        {
                            if (textEnumerator.MoveNext())
                            {
                                text2 = textEnumerator.GetTextElement();
                                bytes2 = Encoding.BigEndianUnicode.GetBytes(text2);
                                bytesStr2 = ConvertToHexString(bytes2);
                            }
                        }

                        // skins
                        if (Emoji.SkinsDict.TryGetValue(bytesStr2, out bytesValue2) && Emoji.SkinnedDict.TryGetValue(bytesStr + bytesStr2, out bytesValue2))
                        {
                            bytesValue = bytesValue2;
                        }
                        // joined emoji
                        else if (Emoji.JoinedEmojiTree.Values.TryGetValue(bytesStr, out node) && node.Values.TryGetValue(bytesStr2, out node))
                        {
                            var initBytesValue = bytesValue;
                            bytesValue += bytesStr2;

                            var localNextElements = new List<Tuple<string, byte[], string>>();

                            var isJoinedEmoji = true;

                            cont = textEnumerator.MoveNext();
                            while (cont)
                            {
                                text2 = textEnumerator.GetTextElement();
                                bytes2 = Encoding.BigEndianUnicode.GetBytes(text2);
                                bytesStr2 = ConvertToHexString(bytes2);
                                localNextElements.Add(new Tuple<string, byte[], string>(text2, bytes2, bytesStr2));

                                if (string.IsNullOrEmpty(bytesStr2))
                                {
                                    if (textEnumerator.MoveNext())
                                    {
                                        text2 = textEnumerator.GetTextElement();
                                        bytes2 = Encoding.BigEndianUnicode.GetBytes(text2);
                                        bytesStr2 = ConvertToHexString(bytes2);
                                        localNextElements.Add(new Tuple<string, byte[], string>(text2, bytes2, bytesStr2));
                                    }
                                }

                                var previousNode = node;
                                if (node.Values.TryGetValue(bytesStr2, out node))
                                {
                                    bytesValue += bytesStr2;
                                    if (node.Values.Count == 0)
                                    {
                                        localNextElements.Clear();
                                        break;
                                    }
                                }
                                else
                                {
                                    if (previousNode.IsEnded)
                                    {
                                        localNextElements.Clear();
                                        localNextElements.Add(new Tuple<string, byte[], string>(text2, bytes2, bytesStr2));
                                    }

                                    isJoinedEmoji = previousNode.IsEnded;
                                    break;
                                }

                                cont = textEnumerator.MoveNext();
                                if (!cont)
                                {
                                    localNextElements.Clear();
                                }
                            }

                            if (!isJoinedEmoji)
                            {
                                bytesValue = initBytesValue;
                            }

                            for (var i = 0; i < localNextElements.Count; i++)
                            {
                                nextElements.Add(localNextElements[i]);
                            }
                        }
                        else
                        {
                            nextElements.Add(new Tuple<string, byte[], string>(text2, bytes2, bytesStr2));
                        }
                    }

                    AddTextAndEmoji(text_block, par, fontStyle, fontWeight, fontFamily, sb, bytesValue);
                }
                else if (Emoji.Dict.TryGetValue(bytesStr, out bytesValue))
                {
                    AddTextAndEmoji(text_block, par, fontStyle, fontWeight, fontFamily, sb, bytesValue);
                }
                else
                {
                    sb = sb.Append(text);
                }

                if (nextElements.Count == 0)
                {
                    cont = textEnumerator.MoveNext();
                }
            }

            var sbStrLast = sb.ToString();

            if (sbStrLast != string.Empty)
            {
                par.Inlines.Add(GetRunWithStyle(sbStrLast, text_block, fontStyle, fontWeight, fontFamily));
            }
        }

        private static void AddTextAndEmoji(RichTextBlock text_block, Span par, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily, StringBuilder sb, string bytesValue)
        {
            var sbStr = sb.ToString();
            sb.Clear();
            if (sbStr != string.Empty)
            {
                par.Inlines.Add(GetRunWithStyle(sbStr, text_block, fontStyle, fontWeight, fontFamily));
            }
            double imageHeight;
            //System.Diagnostics.Debug.WriteLine(bytesValue);
            par.Inlines.Add(GetImage(bytesValue, out imageHeight));
            text_block.MinHeight = imageHeight; //+ 10;
        }

        public static string ConvertToHexString(byte[] bytes)
        {
            //remove end FE0F/FE0E bytes
            var length = bytes.Length;
            if (bytes.Length >= 2
                && bytes[bytes.Length - 2] == 254
                && (bytes[bytes.Length - 1] == 15 || bytes[bytes.Length - 1] == 14))
            {
                length -= 2;
            }
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                sb = sb.Append(System.Convert.ToString(bytes[i], 16).PadLeft(2, '0'));
            }

            return sb.ToString().ToUpperInvariant();
        }

        // Fetch run with PhoneTextNormalStyle
        public static Run GetRunWithStyle(string text, RichTextBlock richTextBox, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily)
        {
            var run = new Run();

            run.FontFamily = richTextBox.FontFamily;
            //run.FontSize = richTextBox.FontSize * TextScaleFactor;
            //run.Foreground = richTextBox.Foreground;// (Brush)Application.Current.Resources["PhoneForegroundBrush"];
            run.Text = text;
            run.FontStyle = fontStyle;
            run.FontWeight = fontWeight;
            run.FontFamily = fontFamily;

            return run;
        }

        private static InlineUIContainer GetImage(string name, out double height)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Emojis/Separated/{0}.png", name))) { DecodePixelWidth = 18, DecodePixelHeight = 18, DecodePixelType = DecodePixelType.Logical };
            image.Height = 18;
            image.Width = 18;
            image.VerticalAlignment = VerticalAlignment.Center;
            image.Margin = new Thickness(0, 4.0, 0, -4.0);
            var container = new InlineUIContainer { };

            height = image.Height;

            container.Child = image;//new Border {Child = image, Background = new SolidColorBrush(Colors.Red), Opacity = 0.7};

            return container;
        }

        private void Entity_Click(MessageViewModel message, TextEntityType type, string data)
        {
            if (type is TextEntityTypeBotCommand)
            {
                message.Delegate.SendBotCommand(data);
            }
            else if (type is TextEntityTypeEmailAddress)
            {
                message.Delegate.OpenUrl("mailto:" + data, false);
            }
            else if (type is TextEntityTypePhoneNumber)
            {
                message.Delegate.OpenUrl("tel:" + data, false);
            }
            else if (type is TextEntityTypeHashtag || type is TextEntityTypeCashtag)
            {
                message.Delegate.OpenHashtag(data);
            }
            else if (type is TextEntityTypeMention)
            {
                message.Delegate.OpenUsername(data);
            }
            else if (type is TextEntityTypeMentionName mentionName)
            {
                message.Delegate.OpenUser(mentionName.UserId);
            }
            else if (type is TextEntityTypeTextUrl textUrl)
            {
                message.Delegate.OpenUrl(textUrl.Url, true);
            }
            else if (type is TextEntityTypeUrl)
            {
                message.Delegate.OpenUrl(data, false);
            }
        }

        private void FooterToLightMedia(bool isOut)
        {
            VisualStateManager.GoToState(LayoutRoot, "LightState" + (isOut ? "Out" : string.Empty), false);

            if (Reply != null)
            {
                VisualStateManager.GoToState(Reply.Content as UserControl, "LightState", false);
            }
        }

        private void FooterToMedia()
        {
            VisualStateManager.GoToState(LayoutRoot, "MediaState", false);

            if (Reply != null)
            {
                VisualStateManager.GoToState(Reply.Content as UserControl, "Normal", false);
            }
        }

        private void FooterToHidden()
        {
            VisualStateManager.GoToState(LayoutRoot, "HiddenState", false);

            if (Reply != null)
            {
                VisualStateManager.GoToState(Reply.Content as UserControl, "Normal", false);
            }
        }

        private void FooterToNormal()
        {
            VisualStateManager.GoToState(LayoutRoot, "Normal", false);

            if (Reply != null)
            {
                VisualStateManager.GoToState(Reply.Content as UserControl, "Normal", false);
            }
        }

        private void Footer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                Placeholder.Width = e.NewSize.Width;
            }
        }

        private SpriteVisual _highlight;

        public void Highlight()
        {
            var message = _message;
            if (message == null)
            {
                return;
            }

            var overlay = _highlight;
            if (overlay == null)
            {
                _highlight = overlay = ElementCompositionPreview.GetElementVisual(this).Compositor.CreateSpriteVisual();
            }

            FrameworkElement target;
            if (Media.Child is IContentWithMask)
            {
                ElementCompositionPreview.SetElementChildVisual(ContentPanel, null);
                ElementCompositionPreview.SetElementChildVisual(Media, overlay);
                target = Media;
            }
            else
            {
                ElementCompositionPreview.SetElementChildVisual(Media, null);
                ElementCompositionPreview.SetElementChildVisual(ContentPanel, overlay);
                target = ContentPanel;
            }

            //Media.Content is IContentWithMask ? Media : (FrameworkElement)ContentPanel;

            //var overlay = ElementCompositionPreview.GetElementChildVisual(target) as SpriteVisual;
            //if (overlay == null)
            //{
            //    overlay = ElementCompositionPreview.GetElementVisual(this).Compositor.CreateSpriteVisual();
            //    ElementCompositionPreview.SetElementChildVisual(target, overlay);
            //}

            var settings = new UISettings();
            var fill = overlay.Compositor.CreateColorBrush(settings.GetColorValue(UIColorType.Accent));
            var brush = (CompositionBrush)fill;

            if (Media.Child is IContentWithMask withMask)
            {
                var alpha = withMask.GetAlphaMask();
                if (alpha != null)
                {
                    var mask = overlay.Compositor.CreateMaskBrush();
                    mask.Source = brush;
                    mask.Mask = alpha;

                    brush = mask;
                }
            }

            overlay.Size = new System.Numerics.Vector2((float)target.ActualWidth, (float)target.ActualHeight);
            overlay.Opacity = 0f;
            overlay.Brush = brush;

            var animation = overlay.Compositor.CreateScalarKeyFrameAnimation();
            animation.Duration = TimeSpan.FromSeconds(2);
            animation.InsertKeyFrame(300f / 2000f, 0.4f);
            animation.InsertKeyFrame(1700f / 2000f, 0.4f);
            animation.InsertKeyFrame(1, 0);

            overlay.StartAnimation("Opacity", animation);
        }

        #region Actions

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            var message = _message;
            if (message == null)
            {
                return;
            }

            message.Delegate.OpenReply(_message);
        }

        private void ReplyMarkup_ButtonClick(object sender, ReplyMarkupInlineButtonClickEventArgs e)
        {
            var message = _message;
            if (message == null)
            {
                return;
            }

            message.Delegate.OpenInlineButton(message, e.Button);
        }

        #endregion

        public void Mockup(string message, bool outgoing, DateTime date)
        {
            ContentPanel.CornerRadius = new CornerRadius(15);
            Margin = new Thickness(0, 2, 0, 2);

            Header.Visibility = Visibility.Collapsed;

            Footer.Mockup(outgoing, date);

            Media.Margin = new Thickness(0);
            Placeholder.Visibility = Visibility.Visible;
            FooterToNormal();
            Grid.SetRow(Footer, 2);
            Grid.SetRow(Message, 2);

            Span.Inlines.Clear();
            Span.Inlines.Add(new Run { Text = message });

            if (ApiInfo.FlowDirection == FlowDirection.LeftToRight && MessageHelper.IsAnyCharacterRightToLeft(message))
            {
                Span.Inlines.Add(new LineBreak());
            }
            else if (ApiInfo.FlowDirection == FlowDirection.RightToLeft && !MessageHelper.IsAnyCharacterRightToLeft(message))
            {
                Span.Inlines.Add(new LineBreak());
            }

            UpdateMockup();
        }

        public void Mockup(string message, string sender, string reply, bool outgoing, DateTime date)
        {
            ContentPanel.CornerRadius = new CornerRadius(15);
            Margin = new Thickness(0, 2, 0, 2);

            Header.Visibility = Visibility.Visible;
            HeaderLabel.Visibility = Visibility.Collapsed;
            AdminLabel.Visibility = Visibility.Collapsed;

            FindName("Reply");

            Reply.Visibility = Visibility.Visible;
            Reply.Mockup(sender, reply);

            Footer.Mockup(outgoing, date);

            Media.Margin = new Thickness(0);
            Placeholder.Visibility = Visibility.Visible;
            FooterToNormal();
            Grid.SetRow(Footer, 2);
            Grid.SetRow(Message, 2);

            Span.Inlines.Clear();
            Span.Inlines.Add(new Run { Text = message });

            if (ApiInfo.FlowDirection == FlowDirection.LeftToRight && MessageHelper.IsAnyCharacterRightToLeft(message))
            {
                Span.Inlines.Add(new LineBreak());
            }
            else if (ApiInfo.FlowDirection == FlowDirection.RightToLeft && !MessageHelper.IsAnyCharacterRightToLeft(message))
            {
                Span.Inlines.Add(new LineBreak());
            }

            UpdateMockup();
        }

        public void UpdateMockup()
        {
            Span.FontSize = (double)App.Current.Resources["MessageFontSize"];
        }






        protected override Size MeasureOverride(Size availableSize)
        {
            var availableWidth = Math.Min(availableSize.Width, Math.Min(double.IsNaN(Width) ? double.PositiveInfinity : Width, 320));
            var availableHeight = Math.Min(availableSize.Height, Math.Min(double.IsNaN(Height) ? double.PositiveInfinity : Height, 420));

            var ttl = false;
            var width = 0.0;
            var height = 0.0;

            var constraint = Tag;
            if (constraint is MessageViewModel viewModel)
            {
                ttl = viewModel.Ttl > 0;
                constraint = viewModel.Content;
            }
            else if (constraint is Message message)
            {
                ttl = message.Ttl > 0;
                constraint = message.Content;
            }

            if (constraint is MessageAnimation animationMessage)
            {
                constraint = animationMessage.Animation;
            }
            else if (constraint is MessageInvoice invoiceMessage)
            {
                constraint = invoiceMessage.Photo;
            }
            else if (constraint is MessageLocation locationMessage)
            {
                constraint = locationMessage.Location;
            }
            else if (constraint is MessagePhoto photoMessage)
            {
                constraint = photoMessage.Photo;
            }
            else if (constraint is MessageSticker stickerMessage)
            {
                constraint = stickerMessage.Sticker;
            }
            else if (constraint is MessageVenue venueMessage)
            {
                constraint = venueMessage.Venue;
            }
            else if (constraint is MessageVideo videoMessage)
            {
                constraint = videoMessage.Video;
            }
            else if (constraint is MessageVideoNote videoNoteMessage)
            {
                constraint = videoNoteMessage.VideoNote;
            }
            else if (constraint is MessageChatChangePhoto chatChangePhoto)
            {
                constraint = chatChangePhoto.Photo;
            }
            else if (constraint is MessageAlbum album)
            {
                var groupedMessages = album.Layout;
                if (groupedMessages.Messages.Count == 1)
                {
                    if (groupedMessages.Messages[0].Content is MessagePhoto photoContent)
                    {
                        constraint = photoContent.Photo;
                    }
                    else if (groupedMessages.Messages[0].Content is MessageVideo videoContent)
                    {
                        constraint = videoContent.Video;
                    }
                }
                else
                {

                    width = groupedMessages.Width / 800d * Math.Min(availableSize.Width, AlbumContent.MAX_WIDTH - AlbumContent.ITEM_MARGIN);
                    height = width / (AlbumContent.MAX_WIDTH - AlbumContent.ITEM_MARGIN) * (AlbumContent.MAX_HEIGHT - AlbumContent.ITEM_MARGIN);
                    height = groupedMessages.Height * height;

                    goto Calculate;
                }
            }

            if (constraint is Animation animation)
            {
                width = animation.Width;
                height = animation.Height;

                goto Calculate;
            }
            else if (constraint is Location location)
            {
                width = 320;
                height = 200;

                goto Calculate;
            }
            else if (constraint is Photo photo)
            {
                if (ttl)
                {
                    width = 240;
                    height = 240;
                }
                else
                {
                    width = photo.Sizes.OrderByDescending(x => x.Width).FirstOrDefault().Width;
                    height = photo.Sizes.OrderByDescending(x => x.Width).FirstOrDefault().Height;
                }

                goto Calculate;
            }
            else if (constraint is Sticker sticker)
            {
                // We actually don't have to calculate bubble width for stickers,
                // As it might be wider due to reply
                //width = sticker.Width;
                //height = sticker.Height;

                //goto Calculate;
            }
            else if (constraint is Venue venue)
            {
                width = 320;
                height = 200;

                goto Calculate;
            }
            else if (constraint is Video video)
            {
                if (ttl)
                {
                    width = 240;
                    height = 240;
                }
                else
                {
                    width = video.Width;
                    height = video.Height;
                }

                goto Calculate;
            }
            else if (constraint is VideoNote videoNote)
            {
                // We actually don't have to calculate bubble width for video notes,
                // As it might be wider due to reply/forward
                //width = 200;
                //height = 200;

                //goto Calculate;
            }

            //if (constraint is MessageText)
            //{
            //    Message.Measure(new Size(availableSize.Width - 20, availableSize.Height));
            //    return base.MeasureOverride(new Size(Message.DesiredSize.Width + 20, availableSize.Height));
            //}

            return base.MeasureOverride(availableSize);

        Calculate:

            if (Footer.DesiredSize.IsEmpty)
                Footer.Measure(availableSize);

            width = Math.Max(Footer.DesiredSize.Width + /*margin left*/ 8 + /*padding right*/ 6 + /*margin right*/ 6, Math.Max(width, 96));

            if (width > availableWidth || height > availableHeight)
            {
                var ratioX = availableWidth / width;
                var ratioY = availableHeight / height;
                var ratio = Math.Min(ratioX, ratioY);

                return base.MeasureOverride(new Size(Math.Max(96, width * ratio), availableSize.Height));
            }
            else
            {
                return base.MeasureOverride(new Size(Math.Max(96, width), availableSize.Height));
            }
        }

        private void Message_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private static bool IsFullMedia(MessageContent content, bool width = false)
        {
            switch (content)
            {
                case MessageLocation location:
                case MessageVenue venue:
                case MessagePhoto photo:
                case MessageVideo video:
                case MessageAnimation animation:
                case MessageAlbum album:
                    return true;
                case MessageInvoice invoice:
                    return width && invoice.Photo != null;
                default:
                    return false;
            }
        }
    }
}
