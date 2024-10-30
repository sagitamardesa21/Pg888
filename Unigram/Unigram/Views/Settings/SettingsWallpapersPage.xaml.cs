﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Services;
using Unigram.ViewModels.Settings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Unigram.Views.Settings
{
    public sealed partial class SettingsWallpapersPage : Page, IHandle<UpdateFile>
    {
        public SettingsWallpapersViewModel ViewModel => DataContext as SettingsWallpapersViewModel;

        public SettingsWallpapersPage()
        {
            InitializeComponent();
            DataContext = TLContainer.Current.Resolve<SettingsWallpapersViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Aggregator.Subscribe(this);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Aggregator.Unsubscribe(this);
        }

        private async void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            var wallpaper = args.Item as Wallpaper;
            var root = args.ItemContainer.ContentTemplateRoot as Grid;

            var check = root.Children[1] as UIElement;
            check.Visibility = wallpaper.Id == ViewModel.SelectedItem?.Id ? Visibility.Visible : Visibility.Collapsed;

            if (wallpaper.Id == 1000001)
            {
                return;
            }
            else if (wallpaper.Id == Constants.WallpaperLocalId)
            {
                //var content = root.Children[0] as Image;
                //content.Source = new BitmapImage(new Uri($"ms-appdata:///local/{ViewModel.SessionId}/{Constants.WallpaperLocalFileName}"));

                var file = await ApplicationData.Current.LocalFolder.GetFileAsync($"{ViewModel.SessionId}\\{Constants.WallpaperLocalFileName}");
                using (var stream = await file.OpenReadAsync())
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);

                    var content = root.Children[0] as Image;
                    content.Source = bitmap;
                }
            }
            else if (wallpaper.Sizes.Count > 0)
            {
                var small = wallpaper.GetSmall();
                if (small == null)
                {
                    return;
                }

                var content = root.Children[0] as Image;
                content.Source = PlaceholderHelper.GetBitmap(ViewModel.ProtoService, small.Photo, 64, 64);                
            }
            else
            {
                var content = root.Children[0] as Rectangle;
                content.Fill = new SolidColorBrush(Color.FromArgb(0xFF, (byte)((wallpaper.Color >> 16) & 0xFF), (byte)((wallpaper.Color >> 8) & 0xFF), (byte)(wallpaper.Color & 0xFF)));
            }
        }

        public void Handle(UpdateFile update)
        {
            this.BeginOnUIThread(() =>
            {
                foreach (var item in ViewModel.Items)
                {
                    if (item.UpdateFile(update.File))
                    {
                        var container = List.ContainerFromItem(item) as SelectorItem;
                        if (container == null)
                        {
                            continue;
                        }

                        var root = container.ContentTemplateRoot as Grid;
                        if (root == null)
                        {
                            continue;
                        }

                        var content = root.Children[0] as Image;
                        if (content == null)
                        {
                            return;
                        }

                        var small = item.GetSmall();
                        if (small == null)
                        {
                            return;
                        }

                        content.Source = PlaceholderHelper.GetBitmap(ViewModel.ProtoService, small.Photo, 64, 64);
                    }
                }
            });
        }

        private void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Wallpaper wallpaper)
            {
                ViewModel.NavigationService.Navigate(typeof(WallpaperPage), wallpaper.Id);
            }
        }
    }
}
