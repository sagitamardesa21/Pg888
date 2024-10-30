//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using Telegram.Common;
using Telegram.Controls.Media;
using Telegram.Converters;
using Telegram.Streams;
using Telegram.Td.Api;
using Telegram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Telegram.Controls.Messages.Content
{
    public sealed class AnimationContent : Control, IContent, IPlayerView
    {
        private MessageViewModel _message;
        public MessageViewModel Message => _message;

        private long _fileToken;
        private long _thumbnailToken;

        public AnimationContent(MessageViewModel message)
        {
            _message = message;

            DefaultStyleKey = typeof(AnimationContent);
        }

        #region InitializeComponent

        private AspectView LayoutRoot;
        private Image Texture;
        private FileButton Button;
        private AnimatedImage Player;
        private Border Overlay;
        private TextBlock Subtitle;
        private bool _templateApplied;

        protected override void OnApplyTemplate()
        {
            LayoutRoot = GetTemplateChild(nameof(LayoutRoot)) as AspectView;
            Texture = GetTemplateChild(nameof(Texture)) as Image;
            Button = GetTemplateChild(nameof(Button)) as FileButton;
            Player = GetTemplateChild(nameof(Player)) as AnimatedImage;
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

            var animation = GetContent(message, out bool isSecret);
            if (animation == null || !_templateApplied)
            {
                return;
            }

            LayoutRoot.Constraint = message;
            Texture.Source = null;

            UpdateThumbnail(message, animation, animation.Thumbnail?.File, true, isSecret);

            UpdateManager.Subscribe(this, message, animation.AnimationValue, ref _fileToken, UpdateFile);
            UpdateFile(message, animation.AnimationValue);
        }

        private void UpdateFile(object target, File file)
        {
            UpdateFile(_message, file);
        }

        private void UpdateFile(MessageViewModel message, File file)
        {
            var animation = GetContent(message, out bool isSecret);
            if (animation == null || !_templateApplied)
            {
                return;
            }

            if (animation.AnimationValue.Id != file.Id)
            {
                return;
            }

            var size = Math.Max(file.Size, file.ExpectedSize);
            if (file.Local.IsDownloadingActive)
            {
                Button.SetGlyph(file.Id, MessageContentState.Downloading);
                Button.Progress = (double)file.Local.DownloadedSize / size;

                Subtitle.Text = string.Format("{0} / {1}", FileSizeConverter.Convert(file.Local.DownloadedSize, size), FileSizeConverter.Convert(size));
                Overlay.Opacity = 1;

                Player.Source = null;
            }
            else if (file.Remote.IsUploadingActive || message.SendingState is MessageSendingStateFailed)
            {
                Button.SetGlyph(file.Id, MessageContentState.Uploading);
                Button.Progress = (double)file.Remote.UploadedSize / size;

                Subtitle.Text = string.Format("{0} / {1}", FileSizeConverter.Convert(file.Remote.UploadedSize, size), FileSizeConverter.Convert(size));
                Overlay.Opacity = 1;

                Player.Source = null;
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingCompleted)
            {
                Button.SetGlyph(file.Id, MessageContentState.Download);
                Button.Progress = 0;

                Subtitle.Text = Strings.AttachGif + ", " + FileSizeConverter.Convert(size);
                Overlay.Opacity = 1;

                Player.Source = null;

                if (message.Delegate.CanBeDownloaded(animation, file))
                {
                    _message.ClientService.DownloadFile(file.Id, 32);
                }
            }
            else
            {
                if (isSecret)
                {
                    Button.SetGlyph(file.Id, MessageContentState.Ttl);
                    Button.Progress = 1;

                    if (message.SelfDestructType is MessageSelfDestructTypeTimer timer)
                    {
                        Subtitle.Text = Icons.PlayFilled12 + "\u2004\u200A" + Locale.FormatTtl(Math.Max(timer.SelfDestructTime, animation.Duration), true);
                    }
                    else
                    {
                        Subtitle.Text = Icons.ArrowClockwiseFilled12 + "\u2004\u200A1";
                    }

                    Overlay.Opacity = 1;

                    Player.Source = null;
                }
                else
                {
                    Button.SetGlyph(file.Id, MessageContentState.Animation);
                    Button.Progress = 1;

                    Subtitle.Text = Strings.AttachGif;
                    Overlay.Opacity = 1;

                    Player.Source = new LocalFileSource(file);
                    message.Delegate.ViewVisibleMessages();
                }
            }
        }

        private void UpdateThumbnail(object target, File file)
        {
            var animation = GetContent(_message, out bool isSecret);
            if (animation == null || !_templateApplied)
            {
                return;
            }

            UpdateThumbnail(_message, animation, animation.Thumbnail?.File, false, isSecret);
        }

        private async void UpdateThumbnail(MessageViewModel message, Animation animation, File file, bool download, bool isSecret)
        {
            ImageSource source = null;
            Image brush = Texture;

            if (animation.Thumbnail != null && animation.Thumbnail.Format is ThumbnailFormatJpeg)
            {
                if (file.Local.IsDownloadingCompleted)
                {
                    source = await PlaceholderHelper.GetBlurredAsync(file.Local.Path, isSecret ? 15 : 3);
                }
                else if (download)
                {
                    if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
                    {
                        if (animation.Minithumbnail != null)
                        {
                            source = await PlaceholderHelper.GetBlurredAsync(animation.Minithumbnail.Data, isSecret ? 15 : 3);
                        }

                        message.ClientService.DownloadFile(file.Id, 1);
                    }

                    UpdateManager.Subscribe(this, message, file, ref _thumbnailToken, UpdateThumbnail, true);
                }
            }
            else if (animation.Minithumbnail != null)
            {
                source = await PlaceholderHelper.GetBlurredAsync(animation.Minithumbnail.Data, isSecret ? 15 : 3);
            }

            brush.Source = source;
        }

        public void Recycle()
        {
            _message = null;

            UpdateManager.Unsubscribe(this, ref _fileToken);
            UpdateManager.Unsubscribe(this, ref _thumbnailToken, true);

            if (_templateApplied)
            {
                Player.Source = null;
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

        private Animation GetContent(MessageViewModel message, out bool isSecret)
        {
            if (message?.Delegate == null)
            {
                isSecret = false;
                return null;
            }

            var content = message.Content;
            if (content is MessageAnimation animation)
            {
                isSecret = animation.IsSecret;
                return animation.Animation;
            }
            else if (content is MessageGame game)
            {
                isSecret = false;
                return game.Game.Animation;
            }
            else if (content is MessageText text && text.WebPage != null)
            {
                isSecret = false;
                return text.WebPage.Animation;
            }

            isSecret = false;
            return null;
        }

        public IPlayerView GetPlaybackElement()
        {
            return Player;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var animation = GetContent(_message, out _);
            if (animation == null)
            {
                return;
            }

            var file = animation.AnimationValue;
            if (file.Local.IsDownloadingActive)
            {
                _message.ClientService.CancelDownloadFile(file);
            }
            else if (file.Remote.IsUploadingActive || _message.SendingState is MessageSendingStateFailed)
            {
                if (_message.SendingState is MessageSendingStateFailed or MessageSendingStatePending)
                {
                    _message.ClientService.Send(new DeleteMessages(_message.ChatId, new[] { _message.Id }, true));
                }
                else
                {
                    _message.ClientService.Send(new CancelPreliminaryUploadFile(file.Id));
                }
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive && !file.Local.IsDownloadingCompleted)
            {
                _message.ClientService.DownloadFile(file.Id, 30);
            }
            else
            {
                _message.Delegate.OpenMedia(_message, this);
            }
        }

        #region IPlaybackView

        public int LoopCount => Player?.LoopCount ?? 1;

        public bool Play()
        {
            // TODO: return value is not used
            Player?.Play();
            return true;
        }

        public void Pause()
        {
            Player?.Pause();
        }

        public void Unload()
        {
            // TODO: this is not used
            if (Player != null)
            {
                Player.Source = null;
            }
        }

        #endregion
    }
}
