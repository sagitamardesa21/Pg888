﻿using System;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Converters;
using Unigram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls.Messages.Content
{
    public sealed class AnimationContent : Control, IContentWithFile, IContentWithPlayback
    {
        private MessageViewModel _message;
        public MessageViewModel Message => _message;

        public AnimationContent(MessageViewModel message)
        {
            _message = message;

            DefaultStyleKey = typeof(AnimationContent);
        }

        #region InitializeComponent

        private AspectView LayoutRoot;
        private Image Texture;
        private FileButton Button;
        private AnimationView Player;
        private Border Overlay;
        private TextBlock Subtitle;
        private bool _templateApplied;

        protected override void OnApplyTemplate()
        {
            LayoutRoot = GetTemplateChild(nameof(LayoutRoot)) as AspectView;
            Texture = GetTemplateChild(nameof(Texture)) as Image;
            Button = GetTemplateChild(nameof(Button)) as FileButton;
            Player = GetTemplateChild(nameof(Player)) as AnimationView;
            Overlay = GetTemplateChild(nameof(Overlay)) as Border;
            Subtitle = GetTemplateChild(nameof(Subtitle)) as TextBlock;

            Button.Click += Button_Click;

            _templateApplied = true;

            if (_message != null)
            {
                UpdateMessage(_message);
            }
        }

        #endregion

        public void UpdateMessage(MessageViewModel message)
        {
            _message = message;

            var animation = GetContent(message.Content);
            if (animation == null || !_templateApplied)
            {
                return;
            }

            LayoutRoot.Constraint = message;
            Texture.Source = null;

            UpdateThumbnail(message, animation.Thumbnail, animation.Minithumbnail);
            UpdateFile(message, animation.AnimationValue);
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
            var animation = GetContent(message.Content);
            if (animation == null || !_templateApplied)
            {
                return;
            }

            if (animation.Thumbnail != null && animation.Thumbnail.File.Id == file.Id)
            {
                UpdateThumbnail(message, animation.Thumbnail, null);
                return;
            }
            else if (animation.AnimationValue.Id != file.Id)
            {
                return;
            }

            var size = Math.Max(file.Size, file.ExpectedSize);
            if (file.Local.IsDownloadingActive)
            {
                //Button.Glyph = Icons.Cancel;
                Button.SetGlyph(file.Id, MessageContentState.Downloading);
                Button.Progress = (double)file.Local.DownloadedSize / size;

                Subtitle.Text = string.Format("{0} / {1}", FileSizeConverter.Convert(file.Local.DownloadedSize, size), FileSizeConverter.Convert(size));
                Overlay.Opacity = 1;

                Player.Source = null;
            }
            else if (file.Remote.IsUploadingActive || message.SendingState is MessageSendingStateFailed)
            {
                //Button.Glyph = Icons.Cancel;
                Button.SetGlyph(file.Id, MessageContentState.Uploading);
                Button.Progress = (double)file.Remote.UploadedSize / size;

                Subtitle.Text = string.Format("{0} / {1}", FileSizeConverter.Convert(file.Remote.UploadedSize, size), FileSizeConverter.Convert(size));
                Overlay.Opacity = 1;

                Player.Source = null;
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingCompleted)
            {
                //Button.Glyph = Icons.Download;
                Button.SetGlyph(file.Id, MessageContentState.Download);
                Button.Progress = 0;

                Subtitle.Text = Strings.Resources.AttachGif + ", " + FileSizeConverter.Convert(size);
                Overlay.Opacity = 1;

                Player.Source = null;

                if (message.Delegate.CanBeDownloaded(message))
                {
                    _message.ProtoService.DownloadFile(file.Id, 32);
                }
            }
            else
            {
                if (message.IsSecret())
                {
                    //Button.Glyph = Icons.Ttl;
                    Button.SetGlyph(file.Id, MessageContentState.Ttl);
                    Button.Progress = 1;

                    Subtitle.Text = Locale.FormatTtl(Math.Max(message.Ttl, animation.Duration), true);
                    Overlay.Opacity = 1;

                    Player.Source = null;
                }
                else
                {
                    //Button.Glyph = Icons.Animation;
                    Button.SetGlyph(file.Id, MessageContentState.Animation);
                    Button.Progress = 1;

                    Subtitle.Text = Strings.Resources.AttachGif;
                    Overlay.Opacity = 1;

                    Player.Source = new LocalVideoSource(file);
                }
            }
        }

        private void UpdateThumbnail(MessageViewModel message, Thumbnail thumbnail, Minithumbnail minithumbnail)
        {
            if (thumbnail != null)
            {
                var file = thumbnail.File;
                if (file.Local.IsDownloadingCompleted && thumbnail.Format is ThumbnailFormatJpeg)
                {
                    Texture.Source = PlaceholderHelper.GetBlurred(file.Local.Path);
                }
                else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
                {
                    if (minithumbnail != null)
                    {
                        Texture.Source = PlaceholderHelper.GetBlurred(minithumbnail.Data);
                    }

                    message.ProtoService.DownloadFile(file.Id, 1);
                }
            }
            else if (minithumbnail != null)
            {
                Texture.Source = PlaceholderHelper.GetBlurred(minithumbnail.Data);
            }
        }

        public bool IsValid(MessageContent content, bool primary)
        {
            if (content is MessageAnimation)
            {
                return true;
            }
            else if (content is MessageGame game && !primary)
            {
                return game.Game.Animation != null;
            }
            else if (content is MessageText text && text.WebPage != null && !primary)
            {
                return text.WebPage.Animation != null;
            }

            return false;
        }

        private Animation GetContent(MessageContent content)
        {
            if (content is MessageAnimation animation)
            {
                return animation.Animation;
            }
            else if (content is MessageGame game)
            {
                return game.Game.Animation;
            }
            else if (content is MessageText text && text.WebPage != null)
            {
                return text.WebPage.Animation;
            }

            return null;
        }

        public IPlayerView GetPlaybackElement()
        {
            return Player;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var animation = GetContent(_message.Content);
            if (animation == null)
            {
                return;
            }

            var file = animation.AnimationValue;
            if (file.Local.IsDownloadingActive)
            {
                _message.ProtoService.CancelDownloadFile(file.Id);
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
                _message.Delegate.OpenMedia(_message, this);
            }
        }
    }
}
