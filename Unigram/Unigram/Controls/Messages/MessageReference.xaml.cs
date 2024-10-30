﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unigram.Common;
using Unigram.Converters;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Telegram.Td.Api;
using Unigram.ViewModels;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Documents;

namespace Unigram.Controls.Messages
{
    public sealed partial class MessageReference : HyperlinkButton
    {
        public MessageReference()
        {
            InitializeComponent();
        }

        public long MessageId { get; private set; }

        #region Message

        public object Message
        {
            get { return (object)GetValue(MessageProperty); }
            //set { SetValue(MessageProperty, value); }
            set
            {
                // TODO: shitty hack!!!
                var oldValue = (object)GetValue(MessageProperty);
                SetValue(MessageProperty, value);

                if (oldValue == value)
                {
                    //SetTemplateCore(value);
                }
            }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(object), typeof(MessageReference), new PropertyMetadata(null, OnMessageChanged));

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MessageReference)d).UpdateEmbedData(e.NewValue as MessageEmbedData);
        }

        private void UpdateEmbedData(MessageEmbedData embedded)
        {
            if (embedded == null)
            {
                return;
            }

            if (embedded.WebPagePreview != null)
            {
                MessageId = 0;
                Visibility = Visibility.Visible;

                TitleLabel.Text = embedded.WebPagePreview.SiteName;

                if (!string.IsNullOrEmpty(embedded.WebPagePreview.Title))
                {
                    MessageLabel.Text = embedded.WebPagePreview.Title;
                }
                else if (!string.IsNullOrEmpty(embedded.WebPagePreview.Author))
                {
                    MessageLabel.Text = embedded.WebPagePreview.Author;
                }
                else
                {
                    MessageLabel.Text = embedded.WebPagePreview.Url;
                }
            }
            else if (embedded.EditingMessage != null)
            {
                MessageId = embedded.EditingMessage.Id;
                GetMessageTemplate(embedded.EditingMessage, Strings.Resources.Edit);
            }
            else if (embedded.ReplyToMessage != null)
            {
                MessageId = embedded.ReplyToMessage.Id;
                GetMessageTemplate(embedded.ReplyToMessage, null);
            }
        }

        #endregion

        public void Mockup(string sender, string message)
        {
            TitleLabel.Text = sender;
            ServiceLabel.Text = string.Empty;
            MessageLabel.Text = message;
        }

        //private bool SetTemplateCore(object item)
        //{
        //    if (item == null)
        //    {
        //        return SetUnsupportedTemplate(null, null);
        //    }

        //    var replyInfo = item as ReplyInfo;
        //    if (replyInfo == null)
        //    {
        //        if (item is TLMessageBase)
        //        {
        //            return GetMessageTemplate(item as TLObject);
        //        }

        //        return SetLoadingTemplate(null, null);

        //        return SetUnsupportedTemplate(null, null);
        //    }
        //    else
        //    {
        //        if (replyInfo.Reply == null)
        //        {
        //            //return ReplyLoadingTemplate;
        //            return SetLoadingTemplate(null, null);
        //        }

        //        var contain = replyInfo.Reply as TLMessagesContainter;
        //        if (contain != null)
        //        {
        //            return GetMessagesContainerTemplate(contain);
        //        }

        //        if (replyInfo.ReplyToMsgId == null || replyInfo.ReplyToMsgId.Value == 0)
        //        {
        //            return SetUnsupportedTemplate(null, null);
        //        }

        //        return GetMessageTemplate(replyInfo.Reply);
        //    }
        //}

        #region Container

        //private bool GetMessagesContainerTemplate(TLMessagesContainter container)
        //{
        //    //if (container.WebPageMedia != null)
        //    //{
        //    //    var webpageMedia = container.WebPageMedia as TLMessageMediaWebPage;
        //    //    if (webpageMedia != null)
        //    //    {
        //    //        var pendingWebpage = webpageMedia.Webpage as TLWebPagePending;
        //    //        if (pendingWebpage != null)
        //    //        {
        //    //            return WebPagePendingTemplate;
        //    //        }

        //    //        var webpage = webpageMedia.Webpage as TLWebPage;
        //    //        if (webpage != null)
        //    //        {
        //    //            return WebPageTemplate;
        //    //        }

        //    //        var emptyWebpage = webpageMedia.Webpage as TLWebPageEmpty;
        //    //        if (emptyWebpage != null)
        //    //        {
        //    //            return WebPageEmptyTemplate;
        //    //        }
        //    //    }
        //    //}

        //    if (container.FwdMessages != null)
        //    {
        //        if (container.FwdMessages.Count == 1)
        //        {
        //            var forwardMessage = container.FwdMessages[0];
        //            if (forwardMessage != null)
        //            {
        //                if (!string.IsNullOrEmpty(forwardMessage.Message) && (forwardMessage.Media == null || forwardMessage.Media is TLMessageMediaEmpty || forwardMessage.Media is TLMessageMediaWebPage))
        //                {
        //                    return SetTextTemplate(forwardMessage, "forward");
        //                }

        //                var media = container.FwdMessages[0].Media;
        //                if (media != null)
        //                {
        //                    switch (media.TypeId)
        //                    {
        //                        case TLType.MessageMediaPhoto:
        //                            return SetPhotoTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaGeo:
        //                            return SetGeoTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaGeoLive:
        //                            return SetGeoLiveTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaVenue:
        //                            return SetVenueTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaContact:
        //                            return SetContactTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaGame:
        //                            return SetGameTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaEmpty:
        //                            return SetUnsupportedTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaDocument:
        //                            if (forwardMessage.IsSticker())
        //                            {
        //                                return SetStickerTemplate(forwardMessage, "forward");
        //                            }
        //                            else if (forwardMessage.IsGif())
        //                            {
        //                                return SetGifTemplate(forwardMessage, "forward");
        //                            }
        //                            else if (forwardMessage.IsVoice())
        //                            {
        //                                return SetVoiceMessageTemplate(forwardMessage, "forward");
        //                            }
        //                            else if (forwardMessage.IsVideo())
        //                            {
        //                                return SetVideoTemplate(forwardMessage, "forward");
        //                            }
        //                            else if (forwardMessage.IsRoundVideo())
        //                            {
        //                                return SetRoundVideoTemplate(forwardMessage, "forward");
        //                            }
        //                            else if (forwardMessage.IsAudio())
        //                            {
        //                                return SetAudioTemplate(forwardMessage, "forward");
        //                            }

        //                            return SetDocumentTemplate(forwardMessage, "forward");
        //                        case TLType.MessageMediaUnsupported:
        //                            return SetUnsupportedMediaTemplate(forwardMessage, "forward");
        //                    }
        //                }
        //            }
        //        }

        //        return SetForwardedMessagesTemplate(container.FwdMessages);
        //    }

        //    if (container.EditMessage != null)
        //    {
        //        var editMessage = container.EditMessage;
        //        if (editMessage != null)
        //        {
        //            if (!string.IsNullOrEmpty(editMessage.Message) && (editMessage.Media == null || editMessage.Media is TLMessageMediaEmpty || editMessage.Media is TLMessageMediaWebPage))
        //            {
        //                return SetTextTemplate(editMessage, Strings.Resources.Edit);
        //            }

        //            var media = editMessage.Media;
        //            if (media != null)
        //            {
        //                switch (media.TypeId)
        //                {
        //                    case TLType.MessageMediaPhoto:
        //                        return SetPhotoTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaGeo:
        //                        return SetGeoTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaGeoLive:
        //                        return SetGeoLiveTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaVenue:
        //                        return SetVenueTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaContact:
        //                        return SetContactTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaGame:
        //                        return SetGameTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaEmpty:
        //                        return SetUnsupportedTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaDocument:
        //                        if (editMessage.IsSticker())
        //                        {
        //                            return SetStickerTemplate(editMessage, Strings.Resources.Edit);
        //                        }
        //                        else if (editMessage.IsGif())
        //                        {
        //                            return SetGifTemplate(editMessage, Strings.Resources.Edit);
        //                        }
        //                        else if (editMessage.IsVoice())
        //                        {
        //                            return SetVoiceMessageTemplate(editMessage, Strings.Resources.Edit);
        //                        }
        //                        else if (editMessage.IsVideo())
        //                        {
        //                            return SetVideoTemplate(editMessage, Strings.Resources.Edit);
        //                        }
        //                        else if (editMessage.IsRoundVideo())
        //                        {
        //                            return SetRoundVideoTemplate(editMessage, Strings.Resources.Edit);
        //                        }
        //                        else if (editMessage.IsAudio())
        //                        {
        //                            return SetAudioTemplate(editMessage, Strings.Resources.Edit);
        //                        }

        //                        return SetDocumentTemplate(editMessage, Strings.Resources.Edit);
        //                    case TLType.MessageMediaUnsupported:
        //                        return SetUnsupportedMediaTemplate(editMessage, Strings.Resources.Edit);
        //                }
        //            }
        //        }

        //        return SetUnsupportedTemplate(editMessage, Strings.Resources.Edit);
        //    }

        //    return SetUnsupportedTemplate(null, Strings.Resources.Edit);
        //}

        #endregion

        public void UpdateMessageReply(MessageViewModel message)
        {
            if (message.ReplyToMessageId == 0)
            {
                Visibility = Visibility.Collapsed;
            }
            else if (message.ReplyToMessage != null)
            {
                GetMessageTemplate(message.ReplyToMessage, null);
            }
            else if (message.ReplyToMessageState == ReplyToMessageState.Loading)
            {
                SetLoadingTemplate(null, null);
            }
            else if (message.ReplyToMessageState == ReplyToMessageState.Deleted)
            {
                SetEmptyTemplate(null, null);
            }
        }

        public void UpdateMessage(MessageViewModel message, bool loading, string title)
        {
            if (loading)
            {
                SetLoadingTemplate(null, title);
            }
            else
            {
                MessageId = message.Id;
                GetMessageTemplate(message, title);
            }
        }

        public void UpdateFile(MessageViewModel message, File file)
        {
            // TODO: maybe something better...
            UpdateMessageReply(message);
        }

        private void UpdateThumbnail(MessageViewModel message, PhotoSize photoSize)
        {
            if (photoSize.Photo.Local.IsDownloadingCompleted)
            {
                double ratioX = (double)36 / photoSize.Width;
                double ratioY = (double)36 / photoSize.Height;
                double ratio = Math.Max(ratioX, ratioY);

                var width = (int)(photoSize.Width * ratio);
                var height = (int)(photoSize.Height * ratio);

                ThumbImage.ImageSource = new BitmapImage(new Uri("file:///" + photoSize.Photo.Local.Path)) { DecodePixelWidth = width, DecodePixelHeight = height, DecodePixelType = DecodePixelType.Logical };
            }
            else if (photoSize.Photo.Local.CanBeDownloaded && !photoSize.Photo.Local.IsDownloadingActive)
            {
                message.ProtoService.Send(new DownloadFile(photoSize.Photo.Id, 1));
            }
        }

        #region Reply

        private bool GetMessageTemplate(MessageViewModel message, string title)
        {
            switch (message.Content)
            {
                case MessageText text:
                    return SetTextTemplate(message, text, title);
                case MessageAnimation animation:
                    return SetAnimationTemplate(message, animation, title);
                case MessageAudio audio:
                    return SetAudioTemplate(message, audio, title);
                case MessageCall call:
                    return SetCallTemplate(message, call, title);
                case MessageContact contact:
                    return SetContactTemplate(message, contact, title);
                case MessageDocument document:
                    return SetDocumentTemplate(message, document, title);
                case MessageGame game:
                    return SetGameTemplate(message, game, title);
                case MessageInvoice invoice:
                    return SetInvoiceTemplate(message, invoice, title);
                case MessageLocation location:
                    return SetLocationTemplate(message, location, title);
                case MessagePhoto photo:
                    return SetPhotoTemplate(message, photo, title);
                case MessageSticker sticker:
                    return SetStickerTemplate(message, sticker, title);
                case MessageUnsupported unsupported:
                    return SetUnsupportedMediaTemplate(message, title);
                case MessageVenue venue:
                    return SetVenueTemplate(message, venue, title);
                case MessageVideo video:
                    return SetVideoTemplate(message, video, title);
                case MessageVideoNote videoNote:
                    return SetVideoNoteTemplate(message, videoNote, title);
                case MessageVoiceNote voiceNote:
                    return SetVoiceNoteTemplate(message, voiceNote, title);

                case MessageBasicGroupChatCreate basicGroupChatCreate:
                case MessageChatAddMembers chatAddMembers:
                case MessageChatChangePhoto chatChangePhoto:
                case MessageChatChangeTitle chatChangeTitle:
                case MessageChatDeleteMember chatDeleteMember:
                case MessageChatDeletePhoto chatDeletePhoto:
                case MessageChatJoinByLink chatJoinByLink:
                case MessageChatSetTtl chatSetTtl:
                case MessageChatUpgradeFrom chatUpgradeFrom:
                case MessageChatUpgradeTo chatUpgradeTo:
                case MessageContactRegistered contactRegistered:
                case MessageCustomServiceAction customServiceAction:
                case MessageGameScore gameScore:
                case MessagePaymentSuccessful paymentSuccessful:
                case MessagePinMessage pinMessage:
                case MessageScreenshotTaken screenshotTaken:
                case MessageSupergroupChatCreate supergroupChatCreate:
                    return SetServiceTextTemplate(message, title);
                case MessageExpiredPhoto expiredPhoto:
                case MessageExpiredVideo expiredVideo:
                    return SetServiceTextTemplate(message, title);
            }

            Visibility = Visibility.Collapsed;
            return false;

            //var message = obj as TLMessage;
            //if (message != null)
            //{
            //    if (!string.IsNullOrEmpty(message.Message) && (message.Media == null || message.Media is TLMessageMediaEmpty))
            //    {
            //        return SetTextTemplate(message, Title);
            //    }

            //    var media = message.Media;
            //    if (media != null)
            //    {
            //        switch (media.TypeId)
            //        {
            //            case TLType.MessageMediaPhoto:
            //                return SetPhotoTemplate(message, Title);
            //            case TLType.MessageMediaGeo:
            //                return SetGeoTemplate(message, Title);
            //            case TLType.MessageMediaGeoLive:
            //                return SetGeoLiveTemplate(message, Title);
            //            case TLType.MessageMediaVenue:
            //                return SetVenueTemplate(message, Title);
            //            case TLType.MessageMediaContact:
            //                return SetContactTemplate(message, Title);
            //            case TLType.MessageMediaGame:
            //                return SetGameTemplate(message, Title);
            //            case TLType.MessageMediaEmpty:
            //                return SetUnsupportedTemplate(message, Title);
            //            case TLType.MessageMediaWebPage:
            //                return SetWebPageTemplate(message, Title);
            //            case TLType.MessageMediaDocument:
            //                if (message.IsSticker())
            //                {
            //                    return SetStickerTemplate(message, Title);
            //                }
            //                else if (message.IsGif())
            //                {
            //                    return SetGifTemplate(message, Title);
            //                }
            //                else if (message.IsVoice())
            //                {
            //                    return SetVoiceMessageTemplate(message, Title);
            //                }
            //                else if (message.IsVideo())
            //                {
            //                    return SetVideoTemplate(message, Title);
            //                }
            //                else if (message.IsRoundVideo())
            //                {
            //                    return SetRoundVideoTemplate(message, Title);
            //                }
            //                else if (message.IsAudio())
            //                {
            //                    return SetAudioTemplate(message, Title);
            //                }

            //                return SetDocumentTemplate(message, Title);
            //            case TLType.MessageMediaUnsupported:
            //                return SetUnsupportedMediaTemplate(message, Title);
            //        }
            //    }
            //}

            //var serviceMessage = obj as TLMessageService;
            //if (serviceMessage != null)
            //{
            //    var action = serviceMessage.Action;
            //    if (action is TLMessageActionChatEditPhoto)
            //    {
            //        return SetServicePhotoTemplate(serviceMessage, Title);
            //    }

            //    return SetServiceTextTemplate(serviceMessage, Title);
            //}
            //else
            //{
            //    var emptyMessage = obj as TLMessageEmpty;
            //    if (emptyMessage != null)
            //    {
            //        return SetEmptyTemplate(emptyMessage, Title);
            //    }

            //    return SetUnsupportedTemplate(message, Title);
            //}
        }

        private bool SetTextTemplate(MessageViewModel message, MessageText text, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = string.Empty;
            MessageLabel.Text = text.Text.Text.Replace("\r\n", "\n").Replace('\n', ' ');

            return true;
        }

        private bool SetPhotoTemplate(MessageViewModel message, MessagePhoto photo, string title)
        {
            Visibility = Visibility.Visible;

            // 🖼

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachPhoto;
            MessageLabel.Text = string.Empty;

            if (message.Ttl > 0)
            {
                if (ThumbRoot != null)
                    ThumbRoot.Visibility = Visibility.Collapsed;
            }
            else
            {
                FindName(nameof(ThumbRoot));
                if (ThumbRoot != null)
                    ThumbRoot.Visibility = Visibility.Visible;

                ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = default(CornerRadius);
                //ThumbImage.ImageSource = (ImageSource)DefaultPhotoConverter.Convert(photoMedia, true);

                var small = photo.Photo.GetSmall();
                if (small != null)
                {
                    UpdateThumbnail(message, small);
                }
            }

            if (photo.Caption != null && !string.IsNullOrWhiteSpace(photo.Caption.Text))
            {
                ServiceLabel.Text += ", ";
                MessageLabel.Text += photo.Caption.Text.Replace("\r\n", "\n").Replace('\n', ' ');
            }

            return true;
        }

        private bool SetInvoiceTemplate(MessageViewModel message, MessageInvoice invoice, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = invoice.Title;
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetLocationTemplate(MessageViewModel message, MessageLocation location, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = location.LivePeriod > 0 ? Strings.Resources.AttachLiveLocation : Strings.Resources.AttachLocation;
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetVenueTemplate(MessageViewModel message, MessageVenue venue, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachLocation + ", " + venue.Venue.Title.Replace("\r\n", "\n").Replace('\n', ' ');
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetCallTemplate(MessageViewModel message, MessageCall call, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            var outgoing = message.IsOutgoing;
            var missed = call.DiscardReason is CallDiscardReasonMissed || call.DiscardReason is CallDiscardReasonDeclined;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = missed ? (outgoing ? Strings.Resources.CallMessageOutgoingMissed : Strings.Resources.CallMessageIncomingMissed) : (outgoing ? Strings.Resources.CallMessageOutgoing : Strings.Resources.CallMessageIncoming);
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetGameTemplate(MessageViewModel message, MessageGame game, string title)
        {
            Visibility = Visibility.Visible;

            FindName(nameof(ThumbRoot));
            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Visible;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = $"🎮 {game.Game.Title}";
            MessageLabel.Text = string.Empty;

            ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = default(CornerRadius);
            //ThumbImage.ImageSource = (ImageSource)DefaultPhotoConverter.Convert(gameMedia.Game.Photo, true);

            return true;
        }

        private bool SetContactTemplate(MessageViewModel message, MessageContact contact, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachContact;
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetAudioTemplate(MessageViewModel message, MessageAudio audio, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachMusic;
            MessageLabel.Text = string.Empty;

            //var document = documentMedia.Document as TLDocument;
            //if (document != null)
            //{
            //    ServiceLabel.Text = document.Title;
            //}

            if (audio.Caption != null && !string.IsNullOrWhiteSpace(audio.Caption.Text))
            {
                ServiceLabel.Text += ", ";
                MessageLabel.Text += audio.Caption.Text.Replace('\n', ' ');
            }

            return true;
        }

        private bool SetVoiceNoteTemplate(MessageViewModel message, MessageVoiceNote voiceNote, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachAudio;
            MessageLabel.Text = string.Empty;

            if (voiceNote.Caption != null && !string.IsNullOrWhiteSpace(voiceNote.Caption.Text))
            {
                ServiceLabel.Text += ", ";
                MessageLabel.Text += voiceNote.Caption.Text.Replace("\r\n", "\n").Replace('\n', ' ');
            }

            return true;
        }

        private bool SetWebPageTemplate(MessageViewModel message, MessageText text, string title)
        {
            //var webPageMedia = message.Media as TLMessageMediaWebPage;
            //if (webPageMedia != null)
            //{
            //    var webPage = webPageMedia.WebPage as TLWebPage;
            //    if (webPage != null && webPage.Photo != null && webPage.Type != null)
            //    {
            //        Visibility = Visibility.Visible;

            //        FindName(nameof(ThumbRoot));
            //        if (ThumbRoot != null)
            //            ThumbRoot.Visibility = Visibility.Visible;

            //        TitleLabel.Text = GetFromLabel(message, title);
            //        ServiceLabel.Text = string.Empty;
            //        MessageLabel.Text = message.Message.Replace("\r\n", "\n").Replace('\n', ' ');

            //        ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = default(CornerRadius);
            //        ThumbImage.ImageSource = (ImageSource)DefaultPhotoConverter.Convert(webPage.Photo, true);
            //    }
            //    else
            //    {
            //        return SetTextTemplate(message, title);
            //    }
            //}

            return true;
        }

        private bool SetVideoTemplate(MessageViewModel message, MessageVideo video, string title)
        {
            Visibility = Visibility.Visible;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachVideo;
            MessageLabel.Text = string.Empty;

            if (message.Ttl > 0)
            {
                if (ThumbRoot != null)
                    ThumbRoot.Visibility = Visibility.Collapsed;
            }
            else
            {
                FindName(nameof(ThumbRoot));
                if (ThumbRoot != null)
                    ThumbRoot.Visibility = Visibility.Visible;

                ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = default(CornerRadius);

                if (video.Video.Thumbnail != null)
                {
                    UpdateThumbnail(message, video.Video.Thumbnail);
                }
            }

            if (video.Caption != null && !string.IsNullOrWhiteSpace(video.Caption.Text))
            {
                ServiceLabel.Text += ", ";
                MessageLabel.Text += video.Caption.Text.Replace("\r\n", "\n").Replace('\n', ' ');
            }

            return true;
        }

        private bool SetVideoNoteTemplate(MessageViewModel message, MessageVideoNote videoNote, string title)
        {
            Visibility = Visibility.Visible;

            FindName(nameof(ThumbRoot));
            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Visible;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachRound;
            MessageLabel.Text = string.Empty;

            ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = new CornerRadius(18);

            if (videoNote.VideoNote.Thumbnail != null)
            {
                UpdateThumbnail(message, videoNote.VideoNote.Thumbnail);
            }

            return true;
        }

        private bool SetAnimationTemplate(MessageViewModel message, MessageAnimation animation, string title)
        {
            Visibility = Visibility.Visible;

            FindName(nameof(ThumbRoot));
            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Visible;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.AttachGif;
            MessageLabel.Text = string.Empty;

            if (animation.Caption != null && !string.IsNullOrWhiteSpace(animation.Caption.Text))
            {
                ServiceLabel.Text += ", ";
                MessageLabel.Text += animation.Caption.Text.Replace("\r\n", "\n").Replace('\n', ' ');
            }

            ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = default(CornerRadius);

            if (animation.Animation.Thumbnail != null)
            {
                UpdateThumbnail(message, animation.Animation.Thumbnail);
            }

            return true;
        }

        private bool SetStickerTemplate(MessageViewModel message, MessageSticker sticker, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = string.IsNullOrEmpty(sticker.Sticker.Emoji) ? Strings.Resources.AttachSticker : $"{sticker.Sticker.Emoji} {Strings.Resources.AttachSticker}";
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetDocumentTemplate(MessageViewModel message, MessageDocument document, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = document.Document.FileName;
            MessageLabel.Text = string.Empty;

            if (document.Caption != null && !string.IsNullOrWhiteSpace(document.Caption.Text))
            {
                ServiceLabel.Text += ", ";
                MessageLabel.Text += document.Caption.Text.Replace("\r\n", "\n").Replace('\n', ' ');
            }

            return true;

            //var documentMedia = message.Media as TLMessageMediaDocument;
            //if (documentMedia != null)
            //{
            //    var document = documentMedia.Document as TLDocument;
            //    if (document != null)
            //    {
            //        var photoSize = document.Thumb as TLPhotoSize;
            //        var photoCachedSize = document.Thumb as TLPhotoCachedSize;
            //        if (photoCachedSize != null || photoSize != null)
            //        {
            //            Visibility = Visibility.Visible;

            //            FindName(nameof(ThumbRoot));
            //            if (ThumbRoot != null)
            //                ThumbRoot.Visibility = Visibility.Visible;

            //            ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = default(CornerRadius);
            //            ThumbImage.ImageSource = (ImageSource)DefaultPhotoConverter.Convert(documentMedia.Document, true);
            //        }
            //        else
            //        {
            //            Visibility = Visibility.Visible;

            //            if (ThumbRoot != null)
            //                ThumbRoot.Visibility = Visibility.Collapsed;
            //        }

            //        TitleLabel.Text = GetFromLabel(message, title);
            //        ServiceLabel.Text = document.FileName;
            //        MessageLabel.Text = string.Empty;

            //        if (!string.IsNullOrWhiteSpace(documentMedia.Caption))
            //        {
            //            ServiceLabel.Text += ", ";
            //            MessageLabel.Text += documentMedia.Caption.Replace("\r\n", "\n").Replace('\n', ' ');
            //        }
            //    }
            //}
            return true;
        }

        private bool SetServiceTextTemplate(MessageViewModel message, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = MessageService.GetText(message);
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetServicePhotoTemplate(MessageViewModel message, string title)
        {
            //Visibility = Visibility.Visible;

            //FindName(nameof(ThumbRoot));
            //if (ThumbRoot != null)
            //    ThumbRoot.Visibility = Visibility.Visible;

            //TitleLabel.Text = GetFromLabel(message, title);
            //ServiceLabel.Text = string.Empty;
            //MessageLabel.Text = LegacyServiceHelper.Convert(message);

            //var action = message.Action as TLMessageActionChatEditPhoto;
            //if (action != null)
            //{
            //    ThumbRoot.CornerRadius = ThumbEllipse.CornerRadius = default(CornerRadius);
            //    ThumbImage.ImageSource = (ImageSource)DefaultPhotoConverter.Convert(action.Photo, true);
            //}

            return true;
        }

        private bool SetLoadingTemplate(MessageViewModel message, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = string.Empty;
            ServiceLabel.Text = Strings.Resources.Loading;
            MessageLabel.Text = string.Empty;
            return true;
        }

        private bool SetEmptyTemplate(Message message, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = string.Empty;
            ServiceLabel.Text = message == null ? Strings.Additional.DeletedMessage : string.Empty;
            MessageLabel.Text = string.Empty;
            return true;
        }

        private bool SetUnsupportedMediaTemplate(MessageViewModel message, string title)
        {
            Visibility = Visibility.Visible;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = GetFromLabel(message, title);
            ServiceLabel.Text = Strings.Resources.UnsupportedAttachment;
            MessageLabel.Text = string.Empty;

            return true;
        }

        private bool SetUnsupportedTemplate(MessageViewModel message, string title)
        {
            Visibility = Visibility.Collapsed;

            if (ThumbRoot != null)
                ThumbRoot.Visibility = Visibility.Collapsed;

            TitleLabel.Text = string.Empty;
            ServiceLabel.Text = string.Empty;
            MessageLabel.Text = string.Empty;
            return false;
        }

        #endregion

        //private string GetFromLabel(TLMessage message, string title)
        //{
        //    return GetFromLabelInternal(message, title) + Environment.NewLine;
        //}

        private string GetFromLabel(MessageViewModel message, string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                return title;
            }

            if (message.IsChannelPost)
            {
                var chat = message.GetChat();
                if (chat != null)
                {
                    return message.ProtoService.GetTitle(chat);
                }
            }
            else if (message.IsSaved())
            {
                if (message.ForwardInfo is MessageForwardedFromUser fromUser)
                {
                    return message.ProtoService.GetUser(fromUser.SenderUserId)?.GetFullName();
                }
                else if (message.ForwardInfo is MessageForwardedPost post)
                {
                    return message.ProtoService.GetTitle(message.ProtoService.GetChat(post.ChatId));
                }
            }

            var user = message.GetSenderUser();
            if (user != null)
            {
                return user.GetFullName();
            }

            //if (message.IsPost && (message.ToId is TLPeerChat || message.ToId is TLPeerChannel))
            //{
            //    return message.Parent?.DisplayName ?? string.Empty;
            //}
            //else if (message.IsSaved() && message.FwdFromUser is TLUser user)
            //{
            //    return user.FullName;
            //}

            //var from = message.From?.FullName ?? string.Empty;
            //if (message.ViaBot != null && message.FwdFrom == null)
            //{
            //    from += $" via @{message.ViaBot.Username}";
            //}

            //return from;
            return title ?? string.Empty;
        }

        //private string GetFromLabel(TLMessageService message, string title)
        //{
        //    return GetFromLabelInternal(message, title) + Environment.NewLine;
        //}
    }
}
