﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Unigram.Controls
{
    public sealed partial class ZoomableGridViewPopup : Grid
    {
        private ApplicationView _applicationView;

        public ZoomableGridViewPopup()
        {
            InitializeComponent();

            _applicationView = ApplicationView.GetForCurrentView();
            _applicationView.VisibleBoundsChanged += OnVisibleBoundsChanged;
            OnVisibleBoundsChanged(_applicationView, null);
        }

        private void OnVisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (sender == null)
            {
                return;
            }

            if (/*BackgroundElement != null &&*/ Window.Current?.Bounds is Rect bounds && sender.VisibleBounds != bounds)
            {
                Margin = new Thickness(sender.VisibleBounds.X - bounds.Left, sender.VisibleBounds.Y - bounds.Top, bounds.Width - (sender.VisibleBounds.Right - bounds.Left), bounds.Height - (sender.VisibleBounds.Bottom - bounds.Top));
            }
            else
            {
                Margin = new Thickness();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Padding = new Thickness(0, 0, 0, InputPane.GetForCurrentView().OccludedRect.Height);

            InputPane.GetForCurrentView().Showing += InputPane_Showing;
            InputPane.GetForCurrentView().Hiding += InputPane_Hiding;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing -= InputPane_Showing;
            InputPane.GetForCurrentView().Hiding -= InputPane_Hiding;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            Padding = new Thickness(0, 0, 0, args.OccludedRect.Height);
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            Padding = new Thickness();
        }


        public async void SetSticker(IProtoService protoService, IEventAggregator aggregator, Sticker sticker)
        {
            Title.Text = string.Empty;
            Texture.Constraint = sticker;

            if (sticker.Thumbnail != null)
            {
                UpdateThumbnail(protoService, sticker.Thumbnail.Photo);
            }

            UpdateFile(protoService, sticker.StickerValue);

            var response = await protoService.SendAsync(new GetStickerEmojis(new InputFileId(sticker.StickerValue.Id)));
            if (response is Emojis emojis)
            {
                Title.Text = string.Join(" ", emojis.EmojisValue);
            }
        }

        public void UpdateFile(IProtoService protoService, File file)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                Texture.Source = PlaceholderHelper.GetWebPFrame(file.Local.Path);
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                Texture.Source = null;
                protoService.DownloadFile(file.Id, 1);
            }
        }

        private void UpdateThumbnail(IProtoService protoService, File file)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                Container.Background = new ImageBrush { ImageSource = PlaceholderHelper.GetWebPFrame(file.Local.Path) };
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                Container.Background = null;
                protoService.DownloadFile(file.Id, 1);
            }
        }
    }
}
