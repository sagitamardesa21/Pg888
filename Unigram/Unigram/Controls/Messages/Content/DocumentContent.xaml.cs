﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Td.Api;
using Unigram.Converters;
using Unigram.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Unigram.Controls.Messages.Content
{
    public enum MessageContentState
    {
        None,
        Download,
        Downloading,
        Uploading,
        Open,
        Ttl,
        Play,
        Pause,
        Theme,
    }

    public sealed partial class DocumentContent : Grid, IContentWithFile
    {
        private MessageContentState _oldState;

        private MessageViewModel _message;
        public MessageViewModel Message => _message;

        public DocumentContent(MessageViewModel message)
        {
            InitializeComponent();
            UpdateMessage(message);
        }

        public void UpdateMessage(MessageViewModel message)
        {
            _oldState = message.Id != _message?.Id ? MessageContentState.None : _oldState;
            _message = message;

            var document = GetContent(message.Content);
            if (document == null)
            {
                return;
            }

            Title.Text = document.FileName;

            if (document.Thumbnail != null)
            {
                UpdateThumbnail(message, document.Thumbnail.Photo);
            }
            else
            {
                Texture.Background = null;
                Button.Style = App.Current.Resources["InlineFileButtonStyle"] as Style;
            }

            UpdateFile(message, document.DocumentValue);
        }

        public void UpdateMessageContentOpened(MessageViewModel message)
        {
            if (message.Ttl > 0)
            {
                //Timer.Maximum = message.Ttl;
                //Timer.Value = DateTime.Now.AddSeconds(message.TtlExpiresIn);
            }
        }

        public void UpdateFile(MessageViewModel message, File file)
        {
            var document = GetContent(message.Content);
            if (document == null)
            {
                return;
            }

            if (document.Thumbnail != null && document.Thumbnail.Photo.Id == file.Id)
            {
                UpdateThumbnail(message, file);
                return;
            }
            else if (document.DocumentValue.Id != file.Id)
            {
                return;
            }

            var size = Math.Max(file.Size, file.ExpectedSize);
            if (file.Local.IsDownloadingActive)
            {
                //Button.Glyph = Icons.Cancel;
                Button.SetGlyph(Icons.Cancel, _oldState != MessageContentState.None && _oldState != MessageContentState.Downloading);
                Button.Progress = (double)file.Local.DownloadedSize / size;

                Subtitle.Text = string.Format("{0} / {1}", FileSizeConverter.Convert(file.Local.DownloadedSize, size), FileSizeConverter.Convert(size));

                _oldState = MessageContentState.Downloading;
            }
            else if (file.Remote.IsUploadingActive || message.SendingState is MessageSendingStateFailed)
            {
                //Button.Glyph = Icons.Cancel;
                Button.SetGlyph(Icons.Cancel, _oldState != MessageContentState.None && _oldState != MessageContentState.Uploading);
                Button.Progress = (double)file.Remote.UploadedSize / size;

                Subtitle.Text = string.Format("{0} / {1}", FileSizeConverter.Convert(file.Remote.UploadedSize, size), FileSizeConverter.Convert(size));

                _oldState = MessageContentState.Uploading;
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingCompleted)
            {
                //Button.Glyph = Icons.Download;
                Button.SetGlyph(Icons.Download, _oldState != MessageContentState.None && _oldState != MessageContentState.Download);
                Button.Progress = 0;

                Subtitle.Text = FileSizeConverter.Convert(size);

                if (message.Delegate.CanBeDownloaded(message))
                {
                    _message.ProtoService.DownloadFile(file.Id, 32);
                }

                _oldState = MessageContentState.Download;
            }
            else
            {
                var theme = document.FileName.EndsWith(".unigram-theme");

                //Button.Glyph = Icons.Document;
                Button.SetGlyph(theme ? Icons.Theme : Icons.Document, _oldState != MessageContentState.None && _oldState != (theme ? MessageContentState.Theme : MessageContentState.Open));
                Button.Progress = 1;

                Subtitle.Text = FileSizeConverter.Convert(size);

                _oldState = theme ? MessageContentState.Theme : MessageContentState.Open;
            }
        }

        private void UpdateThumbnail(MessageViewModel message, File file)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                Texture.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri("file:///" + file.Local.Path)) { DecodePixelWidth = 48, DecodePixelHeight = 48 } };
                Button.Style = App.Current.Resources["ImmersiveFileButtonStyle"] as Style;
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                message.ProtoService.DownloadFile(file.Id, 1);

                Texture.Background = null;
                Button.Style = App.Current.Resources["InlineFileButtonStyle"] as Style;
            }
        }

        public bool IsValid(MessageContent content, bool primary)
        {
            if (content is MessageDocument)
            {
                return true;
            }
            else if (content is MessageText text && text.WebPage != null && !primary)
            {
                return text.WebPage.Document != null;
            }

            return false;
        }

        private Document GetContent(MessageContent content)
        {
            if (content is MessageDocument document)
            {
                return document.Document;
            }
            else if (content is MessageText text && text.WebPage != null)
            {
                return text.WebPage.Document;
            }

            return null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var document = GetContent(_message.Content);
            if (document == null)
            {
                return;
            }

            var file = document.DocumentValue;
            if (file.Local.IsDownloadingActive)
            {
                _message.ProtoService.Send(new CancelDownloadFile(file.Id, false));
            }
            else if (file.Remote.IsUploadingActive || _message.SendingState is MessageSendingStateFailed)
            {
                _message.ProtoService.Send(new DeleteMessages(_message.ChatId, new[] { _message.Id }, true));
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive && !file.Local.IsDownloadingCompleted)
            {
                _message.ProtoService.DownloadFile(file.Id, 32);
            }
            else
            {
                _message.Delegate.OpenFile(file);
            }
        }

        private async void Button_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var document = GetContent(_message.Content);
            if (document == null)
            {
                return;
            }

            var file = document.DocumentValue;
            if (file.Local.IsDownloadingCompleted)
            {
                var item = await StorageFile.GetFileFromPathAsync(file.Local.Path);

                args.AllowedOperations = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                args.Data.SetStorageItems(new[] { item });
                args.DragUI.SetContentFromDataPackage();
            }
        }
    }
}
