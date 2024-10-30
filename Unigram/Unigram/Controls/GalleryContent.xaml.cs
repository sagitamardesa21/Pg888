﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TdWindows;
using Unigram.Common;
using Unigram.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Unigram.Controls
{
    public sealed partial class GalleryContent : Grid
    {
        private IGalleryDelegate _delegate;
        private GalleryItem _item;

        public GalleryItem Item => _item;

        public GalleryContent()
        {
            InitializeComponent();
        }

        public void UpdateItem(IGalleryDelegate delegato, GalleryItem item)
        {
            _delegate = delegato;
            _item = item;

            Tag = item;

            Panel.Background = null;
            Texture.Source = null;

            if (item == null)
            {
                return;
            }

            var data = item.GetFile();
            var thumb = item.GetThumbnail();

            Panel.Constraint = item.Constraint;
            Panel.InvalidateMeasure();

            if (thumb != null && (item.IsVideo || (item.IsPhoto && !data.Local.IsDownloadingCompleted)))
            {
                UpdateThumbnail(item, thumb);
            }

            UpdateFile(item, data);
        }

        public void UpdateFile(GalleryItem item, File file)
        {
            var data = item.GetFile();
            var thumb = item.GetThumbnail();

            if (thumb != null && thumb.Id == file.Id)
            {
                UpdateThumbnail(item, file);
                return;
            }
            else if (data.Id != file.Id)
            {
                return;
            }

            var size = Math.Max(file.Size, file.ExpectedSize);
            if (file.Local.IsDownloadingActive)
            {
                Button.Glyph = "\uE10A";
                Button.Progress = (double)file.Local.DownloadedSize / size;
                Button.Opacity = 1;
            }
            else if (file.Remote.IsUploadingActive)
            {
                Button.Glyph = "\uE10A";
                Button.Progress = (double)file.Remote.UploadedSize / size;
                Button.Opacity = 1;
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingCompleted)
            {
                Button.Glyph = "\uE118";
                Button.Progress = 0;
                Button.Opacity = 1;

                if (item.IsPhoto)
                {
                    item.ProtoService.Send(new DownloadFile(file.Id, 1));
                }
            }
            else
            {
                if (item.IsVideo)
                {
                    Button.Glyph = "\uE102";
                    Button.Progress = 1;
                    Button.Opacity = 1;
                }
                else if (item.IsPhoto)
                {
                    Button.Opacity = 0;
                    Texture.Source = new BitmapImage(new Uri("file:///" + file.Local.Path));
                }
            }
        }

        private void UpdateThumbnail(GalleryItem item, File file)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                //Texture.Source = new BitmapImage(new Uri("file:///" + file.Local.Path));
                Panel.Background = new ImageBrush { ImageSource = PlaceholderHelper.GetBlurred(file.Local.Path), Stretch = Stretch.UniformToFill };
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                item.ProtoService.Send(new DownloadFile(file.Id, 1));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_item == null)
            {
                return;
            }

            var file = _item.GetFile();
            if (file.Local.IsDownloadingActive)
            {
                _item.ProtoService.Send(new CancelDownloadFile(file.Id, false));
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive && !file.Local.IsDownloadingCompleted)
            {
                _item.ProtoService.Send(new DownloadFile(file.Id, 1));
            }
            else
            {
                if (_item.IsVideo)
                {
                    _delegate?.OpenFile(_item, file);
                }
            }
        }
    }
}
