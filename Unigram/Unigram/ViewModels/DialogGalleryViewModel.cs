﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.FileManager.EventArgs;
using Telegram.Api.TL;
using Telegram.Api.TL.Messages;
using Template10.Common;
using Unigram.Common;
using Unigram.Controls.Views;
using Unigram.Core.Common;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels
{
    public class DialogGalleryViewModel : GalleryViewModelBase
    {
        private readonly DisposableMutex _loadMoreLock = new DisposableMutex();
        private readonly TLInputPeerBase _peer;

        private int _lastMaxId;

        public DialogGalleryViewModel(TLInputPeerBase peer, TLMessage selected, IMTProtoService protoService)
            : base(protoService, null, null)
        {
            if (selected.Media is TLMessageMediaPhoto photoMedia || selected.IsVideo() || selected.IsRoundVideo() || selected.IsGif())
            {
                Items = new MvxObservableCollection<GalleryItem> { new GalleryMessageItem(selected) };
                SelectedItem = Items[0];
                FirstItem = Items[0];
            }
            else
            {
                Items = new MvxObservableCollection<GalleryItem>();
            }

            _peer = peer;
            _lastMaxId = selected.Id;

            Initialize();
        }

        private async void Initialize()
        {
            using (await _loadMoreLock.WaitAsync())
            {
                var result = await ProtoService.SearchAsync(_peer, string.Empty, new TLInputMessagesFilterPhotoVideo(), 0, 0, 0, _lastMaxId, 15);
                if (result.IsSucceeded)
                {
                    if (result.Result is TLMessagesMessagesSlice)
                    {
                        var slice = result.Result as TLMessagesMessagesSlice;
                        TotalItems = slice.Count;
                    }
                    else
                    {
                        TotalItems = result.Result.Messages.Count + Items.Count;
                    }

                    //Items.Clear();

                    foreach (var photo in result.Result.Messages)
                    {
                        if (photo is TLMessage message && (message.Media is TLMessageMediaPhoto media || message.IsVideo() || message.IsRoundVideo() || message.IsGif()))
                        {
                            Items.Insert(0, new GalleryMessageItem(message));
                            _lastMaxId = message.Id;
                        }
                        else
                        {
                            TotalItems--;
                        }
                    }

                    //Items.ReplaceWith(items);
                    SelectedItem = Items.LastOrDefault();
                }
            }
        }

        //protected override async void LoadNext()
        //{
        //    if (User != null)
        //    {
        //        using (await _loadMoreLock.WaitAsync())
        //        {
        //            var result = await ProtoService.GetUserPhotosAsync(User.ToInputUser(), Items.Count, 0, 0);
        //            if (result.IsSucceeded)
        //            {
        //                foreach (var photo in result.Value.Photos)
        //                {
        //                    Items.Add(photo);
        //                }
        //            }
        //        }
        //    }
        //}

        public override bool CanGoto => true;
    }

    public class GalleryMessageItem : GalleryItem
    {
        protected readonly TLMessage _message;

        public GalleryMessageItem(TLMessage message)
        {
            _message = message;
        }

        public TLMessage Message => _message;

        public override ITLTransferable Source
        {
            get
            {
                if (_message.Media is TLMessageMediaPhoto photoMedia && photoMedia.Photo is TLPhoto photo)
                {
                    return photo;
                }
                else if (_message.Media is TLMessageMediaDocument documentMedia && documentMedia.Document is TLDocument document)
                {
                    return document;
                }

                return null;
            }
        }

        public override string Caption
        {
            get
            {
                if (_message.Media is ITLMessageMediaCaption captionMedia)
                {
                    return captionMedia.Caption;
                }

                return null;
            }
        }

        //public override ITLDialogWith From => _message.IsPost ? _message.Parent : _message.From;

        public override ITLDialogWith From
        {
            get
            {
                if (_message.HasFwdFrom)
                {
                    return (ITLDialogWith)_message.FwdFrom.Channel ?? _message.FwdFrom.User;
                }

                return _message.IsPost ? _message.Parent : _message.From;
            }
        }

        public override int Date => _message.Date;

        public override bool IsVideo => _message.IsVideo() || /*_message.IsGif() ||*/ _message.IsRoundVideo();

        public override bool IsLoop => /*_message.IsGif() ||*/ _message.IsRoundVideo();

        public override bool IsShareEnabled => _message.Parent != null;

        public override bool HasStickers
        {
            get
            {
                if (_message.Media is TLMessageMediaPhoto photoMedia && photoMedia.Photo is TLPhoto photo)
                {
                    return photo.IsHasStickers;
                }
                else if (_message.Media is TLMessageMediaDocument documentMedia && documentMedia.Document is TLDocument document)
                {
                    return document.Attributes.Any(x => x is TLDocumentAttributeHasStickers);
                }

                return false;
            }
        }

        public override TLInputStickeredMediaBase ToInputStickeredMedia()
        {
            if (_message.Media is TLMessageMediaPhoto photoMedia && photoMedia.Photo is TLPhoto photo)
            {
                return new TLInputStickeredMediaPhoto { Id = photo.ToInputPhoto() };
            }
            else if (_message.Media is TLMessageMediaDocument documentMedia && documentMedia.Document is TLDocument document)
            {
                return new TLInputStickeredMediaDocument { Id = document.ToInputDocument() };
            }

            return null;
        }

        public override Uri GetVideoSource()
        {
            if (_message.Media is TLMessageMediaDocument documentMedia && documentMedia.Document is TLDocument document)
            {
                return FileUtils.GetTempFileUri(document.GetFileName());
            }

            return null;
        }

        public override async void Share()
        {
            if (ShouldShare(_message, true))
            {
                await ShareView.Current.ShowAsync(_message);
            }
            else
            {
                await ShareView.Current.ShowAsync(_message.Media.ToInputMedia());
            }
        }

        private bool ShouldShare(TLMessage message, bool allowOut)
        {
            if (message.IsSticker())
            {
                return false;
            }
            else if (message.HasFwdFrom && message.FwdFrom.HasChannelId && (!message.IsOut || allowOut))
            {
                return true;
            }
            else if (message.HasFromId && !message.IsPost)
            {
                if (message.Media is TLMessageMediaEmpty || message.Media == null || message.Media is TLMessageMediaWebPage webpageMedia && !(webpageMedia.WebPage is TLWebPage))
                {
                    return false;
                }

                var user = message.From;
                if (user != null && user.IsBot)
                {
                    return true;
                }

                if (!message.IsOut || allowOut)
                {
                    if (message.Media is TLMessageMediaGame || message.Media is TLMessageMediaInvoice)
                    {
                        return true;
                    }

                    var parent = message.Parent as TLChannel;
                    if (parent != null && parent.IsMegaGroup)
                    {
                        //TLRPC.Chat chat = MessagesController.getInstance().getChat(messageObject.messageOwner.to_id.channel_id);
                        //return chat != null && chat.username != null && chat.username.length() > 0 && !(messageObject.messageOwner.media instanceof TLRPC.TL_messageMediaContact) && !(messageObject.messageOwner.media instanceof TLRPC.TL_messageMediaGeo);

                        return parent.HasUsername && !(message.Media is TLMessageMediaContact) && !(message.Media is TLMessageMediaGeo);
                    }
                }
            }
            //else if (messageObject.messageOwner.from_id < 0 || messageObject.messageOwner.post)
            else if (message.IsPost)
            {
                //if (messageObject.messageOwner.to_id.channel_id != 0 && (messageObject.messageOwner.via_bot_id == 0 && messageObject.messageOwner.reply_to_msg_id == 0 || messageObject.type != 13))
                //{
                //    return Visibility.Visible;
                //}

                if (message.ToId is TLPeerChannel && (!message.HasViaBotId && !message.HasReplyToMsgId))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class GalleryMessageServiceItem : GalleryItem
    {
        private readonly TLMessageService _message;

        public GalleryMessageServiceItem(TLMessageService message)
        {
            _message = message;
        }

        public TLMessageService Message => _message;

        public override ITLTransferable Source
        {
            get
            {
                if (_message.Action is TLMessageActionChatEditPhoto chatEditPhotoAction && chatEditPhotoAction.Photo is TLPhoto photo)
                {
                    return photo;
                }

                return null;
            }
        }

        //public override ITLDialogWith From => _message.IsPost ? _message.Parent : _message.From;

        public override ITLDialogWith From
        {
            get
            {
                return _message.IsPost ? _message.Parent : _message.From;
            }
        }

        public override int Date => _message.Date;

        public override bool HasStickers
        {
            get
            {
                if (_message.Action is TLMessageActionChatEditPhoto chatEditPhotoAction && chatEditPhotoAction.Photo is TLPhoto photo)
                {
                    return photo.IsHasStickers;
                }

                return false;
            }
        }

        public override TLInputStickeredMediaBase ToInputStickeredMedia()
        {
            if (_message.Action is TLMessageActionChatEditPhoto chatEditPhotoAction && chatEditPhotoAction.Photo is TLPhoto photo)
            {
                return new TLInputStickeredMediaPhoto { Id = photo.ToInputPhoto() };
            }

            return null;
        }
    }

    public class GalleryPhotoItem : GalleryItem
    {
        private readonly TLPhoto _photo;
        private readonly ITLDialogWith _from;
        private readonly string _caption;

        public GalleryPhotoItem(TLPhoto photo, ITLDialogWith from)
        {
            _photo = photo;
            _from = from;
        }

        public GalleryPhotoItem(TLPhoto photo, string caption)
        {
            _photo = photo;
            _caption = caption;
        }

        public TLPhoto Photo => _photo;

        public override ITLTransferable Source => _photo;

        public override string Caption => _caption;

        public override ITLDialogWith From => _from;

        public override int Date => _photo.Date;

        public override bool HasStickers => _photo.IsHasStickers;

        public override TLInputStickeredMediaBase ToInputStickeredMedia()
        {
            return new TLInputStickeredMediaPhoto { Id = _photo.ToInputPhoto() };
        }
    }

    public class GalleryDocumentItem : GalleryItem
    {
        private readonly TLDocument _document;
        private readonly ITLDialogWith _from;
        private readonly string _caption;

        public GalleryDocumentItem(TLDocument document, ITLDialogWith from)
        {
            _document = document;
            _from = from;
        }

        public GalleryDocumentItem(TLDocument document, string caption)
        {
            _document = document;
            _caption = caption;
        }

        public TLDocument Document => _document;

        public override ITLTransferable Source => _document;

        public override string Caption => _caption;

        public override ITLDialogWith From => _from;

        public override int Date => _document.Date;

        public override bool IsVideo => TLMessage.IsVideo(_document) || /*TLMessage.IsGif(_document) ||*/ TLMessage.IsRoundVideo(_document);

        public override bool IsLoop => /*TLMessage.IsGif(_document) ||*/ TLMessage.IsRoundVideo(_document);

        public override bool HasStickers => _document.Attributes.Any(x => x is TLDocumentAttributeHasStickers);

        public override TLInputStickeredMediaBase ToInputStickeredMedia()
        {
            return new TLInputStickeredMediaDocument { Id = _document.ToInputDocument() };
        }

        public override Uri GetVideoSource()
        {
            return FileUtils.GetTempFileUri(_document.GetFileName());
        }
    }
}
