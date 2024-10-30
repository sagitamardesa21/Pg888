//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using Telegram.Common;
using Telegram.Streams;
using Telegram.Td.Api;
using Telegram.ViewModels;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace Telegram.Controls.Messages.Content
{
    public sealed class VideoStickerContent : HyperlinkButton, IContent, IPlayerView
    {
        private MessageViewModel _message;
        public MessageViewModel Message => _message;

        private long _fileToken;

        private CompositionAnimation _thumbnailShimmer;

        public VideoStickerContent(MessageViewModel message)
        {
            _message = message;

            DefaultStyleKey = typeof(VideoStickerContent);
            Click += Button_Click;
        }

        #region InitializeComponent

        private AspectView LayoutRoot;
        private AnimatedImage Player;
        private bool _templateApplied;

        protected override void OnApplyTemplate()
        {
            LayoutRoot = GetTemplateChild(nameof(LayoutRoot)) as AspectView;
            Player = GetTemplateChild(nameof(Player)) as AnimatedImage;

            Player.Ready += Player_Ready;

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

            var sticker = GetContent(message);
            if (sticker == null || !_templateApplied)
            {
                return;
            }

            if (message.Content is MessageAnimatedEmoji animatedEmoji)
            {
                LayoutRoot.MaxWidth = 180 * message.ClientService.Config.GetNamedNumber("emojies_animated_zoom", 0.625f);
                LayoutRoot.MaxHeight = 180 * message.ClientService.Config.GetNamedNumber("emojies_animated_zoom", 0.625f);
            }
            else
            {
                LayoutRoot.MaxWidth = 180;
                LayoutRoot.MaxHeight = 180;
            }

            LayoutRoot.Constraint = message;

            if (!sticker.StickerValue.Local.IsDownloadingCompleted)
            {
                UpdateThumbnail(message, sticker);
            }

            UpdateManager.Subscribe(this, message, sticker.StickerValue, ref _fileToken, UpdateFile, true);
            UpdateFile(message, sticker.StickerValue);
        }

        private void UpdateFile(object target, File file)
        {
            UpdateFile(_message, file);
        }

        private void UpdateFile(MessageViewModel message, File file)
        {
            var sticker = GetContent(message);
            if (sticker == null || !_templateApplied)
            {
                return;
            }

            if (sticker.StickerValue.Id != file.Id)
            {
                return;
            }

            if (file.Local.IsDownloadingCompleted)
            {
                using (Player.BeginBatchUpdate())
                {
                    Player.LoopCount = PowerSavingPolicy.AutoPlayStickersInChats ? 0 : 1;
                    Player.FrameSize = ImageHelper.Scale(sticker.Width, sticker.Height, 180);
                    Player.Source = new LocalFileSource(file);
                }

                message.Delegate.ViewVisibleMessages();
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                Player.Source = null;
                message.ClientService.DownloadFile(file.Id, 1);
            }
        }

        private void UpdateThumbnail(MessageViewModel message, Sticker sticker)
        {
            _thumbnailShimmer = CompositionPathParser.ParseThumbnail(sticker, out ShapeVisual visual);
            ElementCompositionPreview.SetElementChildVisual(Player, visual);
        }

        private void Player_Ready(object sender, EventArgs e)
        {
            _thumbnailShimmer = null;
            ElementCompositionPreview.SetElementChildVisual(Player, null);
        }

        public void Recycle()
        {
            _message = null;

            UpdateManager.Unsubscribe(this, ref _fileToken, true);

            if (_templateApplied)
            {
                Player.Source = null;
            }
        }

        public bool IsValid(MessageContent content, bool primary)
        {
            if (content is MessageSticker sticker)
            {
                return sticker.Sticker.Format is StickerFormatWebm;
            }
            else if (content is MessageText text && text.WebPage != null && !primary)
            {
                return text.WebPage.Sticker != null && text.WebPage.Sticker.Format is StickerFormatWebm;
            }

            return false;
        }

        private Sticker GetContent(MessageViewModel message)
        {
            if (message?.Delegate == null)
            {
                return null;
            }

            var content = message.GeneratedContent ?? message.Content;
            if (content is MessageSticker sticker)
            {
                return sticker.Sticker;
            }
            else if (content is MessageText text && text.WebPage != null)
            {
                return text.WebPage.Sticker;
            }

            return null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sticker = GetContent(_message);
            if (sticker == null)
            {
                return;
            }

            if (PowerSavingPolicy.AutoPlayStickersInChats /*|| Player.IsPlaying*/)
            {
                _message.Delegate.OpenSticker(sticker);
            }
            else
            {
                Player.Play();
            }
        }

        #region IPlaybackView

        public int LoopCount => Player?.LoopCount ?? 1;

        public bool Play()
        {
            // TODO: returned value is not used
            if (PowerSavingPolicy.AutoPlayStickersInChats)
            {
                Player?.Play();
                return true;
            }

            return false;
        }

        public void Pause()
        {
            Player?.Pause();
        }

        public void Unload()
        {
            // TODO: this is not used
        }

        #endregion
    }
}
