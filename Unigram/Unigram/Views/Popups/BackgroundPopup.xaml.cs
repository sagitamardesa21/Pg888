﻿using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Controls.Brushes;
using Unigram.Controls.Chats;
using Unigram.Converters;
using Unigram.Services;
using Unigram.ViewModels;
using Unigram.ViewModels.Delegates;
using Windows.Storage.AccessCache;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Unigram.Views
{
    public sealed partial class BackgroundPopup : ContentPopup, IHandle<UpdateFile>, IBackgroundDelegate
    {
        public BackgroundViewModel ViewModel => DataContext as BackgroundViewModel;

        private readonly FlatFileContext<Background> _backgrounds = new();

        private readonly ChatBackgroundFreeform _freeform = new(false);

        private SpriteVisual _blurVisual;
        private CompositionEffectBrush _blurBrush;
        private Compositor _compositor;

        public BackgroundPopup(Background background)
            : this()
        {
            _ = ViewModel.OnNavigatedToAsync(background, NavigationMode.New, null);
        }

        public BackgroundPopup(string slug)
            : this()
        {
            _ = ViewModel.OnNavigatedToAsync(slug, NavigationMode.New, null);
        }

        private BackgroundPopup()
        {
            InitializeComponent();
            DataContext = TLContainer.Current.Resolve<BackgroundViewModel, IBackgroundDelegate>(this);

            Title = Strings.Resources.BackgroundPreview;
            PrimaryButtonText = Strings.Resources.Set;
            SecondaryButtonText = Strings.Resources.Cancel;

            Message1.Mockup(Strings.Resources.BackgroundPreviewLine1, false, DateTime.Now.AddSeconds(-25));
            Message2.Mockup(Strings.Resources.BackgroundPreviewLine2, true, DateTime.Now);

            //Presenter.Update(ViewModel.SessionId, ViewModel.Settings, ViewModel.Aggregator);

            ElementCompositionPreview.GetElementVisual(ContentPanel).Clip = Window.Current.Compositor.CreateInsetClip();

            InitializeBlur();
        }

        private void InitializeBlur()
        {
            _compositor = Window.Current.Compositor;

            ElementCompositionPreview.GetElementVisual(this).Clip = _compositor.CreateInsetClip();

            var graphicsEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = 0,
                BorderMode = EffectBorderMode.Hard,
                Source = new CompositionEffectSourceParameter("backdrop")
            };

            var effectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "Blur.BlurAmount" });
            var effectBrush = effectFactory.CreateBrush();
            var backdrop = _compositor.CreateBackdropBrush();
            effectBrush.SetSourceParameter("backdrop", backdrop);

            _blurBrush = effectBrush;
            _blurVisual = _compositor.CreateSpriteVisual();
            _blurVisual.Brush = _blurBrush;

            // Why does this crashes due to an access violation exception on certain devices?
            ElementCompositionPreview.SetElementChildVisual(BlurPanel, _blurVisual);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Aggregator.Subscribe(this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Aggregator.Unsubscribe(this);
        }

        private void BlurPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_fill is BackgroundFillFreeformGradient freeform)
            {
                _freeform.UpdateLayout(Background as ImageBrush, e.NewSize.Width, e.NewSize.Height, freeform);
            }

            _blurVisual.Size = e.NewSize.ToVector2();
        }

        private BackgroundFill _fill;
        public BackgroundFill Fill
        {
            get => _fill;
            set
            {
                _fill = value;

                if (value is BackgroundFillFreeformGradient freeform)
                {
                    Background = new ImageBrush { AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Stretch = Stretch.UniformToFill };
                    _freeform.UpdateLayout(Background as ImageBrush, ActualWidth, ActualHeight, freeform);
                }
                else
                {
                    Background = value?.ToBrush();
                }
            }
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.Duration = TimeSpan.FromMilliseconds(300);

            if (sender is CheckBox check && check.IsChecked == true)
            {
                animation.InsertKeyFrame(1, 12);
            }
            else
            {
                animation.InsertKeyFrame(1, 0);
            }

            _blurBrush.Properties.StartAnimation("Blur.BlurAmount", animation);
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            Grid.SetRow(ColorPanel, ColorRadio.IsChecked == true ? 2 : 4);

            if (ColorRadio.IsChecked == true)
            {
                PatternRadio.IsChecked = false;
            }

            RadioColor_Toggled(null, null);
        }

        private void Pattern_Click(object sender, RoutedEventArgs e)
        {
            Grid.SetRow(PatternPanel, PatternRadio.IsChecked == true ? 2 : 4);

            if (PatternRadio.IsChecked == true)
            {
                ColorRadio.IsChecked = false;
            }
        }

        private async void UpdatePresenter(Background wallpaper)
        {
            if (wallpaper.Id == Constants.WallpaperLocalId && StorageApplicationPermissions.FutureAccessList.ContainsItem(wallpaper.Name))
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(wallpaper.Name);
                using (var stream = await file.OpenReadAsync())
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);

                    Presenter.Fill = new ImageBrush { ImageSource = bitmap, AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Stretch = Stretch.UniformToFill };
                }
            }
            else
            {
                var big = wallpaper.Document;
                if (big == null)
                {
                    return;
                }

                if (wallpaper.Type is BackgroundTypeWallpaper)
                {
                    Presenter.Fill = new ImageBrush { ImageSource = PlaceholderHelper.GetBitmap(ViewModel.ProtoService, big.DocumentValue, 0, 0), AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Stretch = Stretch.UniformToFill };
                }
                else if (wallpaper.Type is BackgroundTypePattern)
                {
                    if (string.Equals(wallpaper.Document.MimeType, "application/x-tgwallpattern", StringComparison.OrdinalIgnoreCase))
                    {
                        Presenter.Fill = new TiledBrush { SvgSource = PlaceholderHelper.GetVectorSurface(ViewModel.ProtoService, big.DocumentValue, ViewModel.GetPatternForeground()) };
                    }
                    else
                    {
                        Presenter.Fill = new ImageBrush { ImageSource = PlaceholderHelper.GetBitmap(ViewModel.ProtoService, big.DocumentValue, 0, 0), AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Stretch = Stretch.UniformToFill };
                    }
                }
            }
        }

        #region Delegates

        public void UpdateBackground(Background wallpaper)
        {
            if (wallpaper == null)
            {
                return;
            }

            //Header.CommandVisibility = wallpaper.Id != Constants.WallpaperLocalId ? Visibility.Visible : Visibility.Collapsed;

            if (wallpaper.Type is BackgroundTypeWallpaper)
            {
                Blur.Visibility = Visibility.Visible;

                Message1.Mockup(Strings.Resources.BackgroundPreviewLine1, false, DateTime.Now.AddSeconds(-25));
                Message2.Mockup(Strings.Resources.BackgroundPreviewLine2, true, DateTime.Now);

                UpdatePresenter(wallpaper);
            }
            else
            {
                Blur.Visibility = Visibility.Collapsed;

                if (wallpaper.Type is BackgroundTypeFill or BackgroundTypePattern)
                {
                    UpdatePresenter(wallpaper);

                    Pattern.Visibility = Visibility.Visible;
                    Color.Visibility = Visibility.Visible;
                }

                Message1.Mockup(Strings.Resources.BackgroundColorSinglePreviewLine1, false, DateTime.Now.AddSeconds(-25));
                Message2.Mockup(Strings.Resources.BackgroundColorSinglePreviewLine2, true, DateTime.Now);
            }
        }

        #endregion

        #region Binding

        private BackgroundFill ConvertBackground(BackgroundColor color1, BackgroundColor color2, BackgroundColor color3, BackgroundColor color4, int rotation)
        {
            Fill = ViewModel.GetFill();

            var panel = PatternList.ItemsPanelRoot as ItemsStackPanel;
            if (panel != null)
            {
                for (int i = panel.FirstCacheIndex; i <= panel.LastCacheIndex; i++)
                {
                    var container = PatternList.ContainerFromIndex(i) as SelectorItem;
                    if (container == null)
                    {
                        continue;
                    }

                    var wallpaper = ViewModel.Patterns[i];
                    var root = container.ContentTemplateRoot as Grid;

                    var check = root.Children[1];
                    check.Visibility = wallpaper.Id == ViewModel.SelectedPattern?.Id ? Visibility.Visible : Visibility.Collapsed;

                    var content = root.Children[0] as Image;
                    if (wallpaper.Document != null)
                    {
                        var small = wallpaper.Document.Thumbnail;
                        if (small == null)
                        {
                            continue;
                        }

                        content.Source = PlaceholderHelper.GetBitmap(ViewModel.ProtoService, small.File, wallpaper.Document.Thumbnail.Width, wallpaper.Document.Thumbnail.Height);
                    }
                    else
                    {
                        content.Source = null;
                    }

                    content.Opacity = ViewModel.Intensity / 100d;
                    root.Background = Background;
                }
            }

            return null;
        }

        private string ConvertColor2Glyph(BackgroundColor color)
        {
            return color.IsEmpty ? Icons.Add : Icons.Dismiss;
        }

        private Visibility ConvertColor2Visibility(BackgroundColor color)
        {
            return color.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        private Visibility ConvertColor2Visibility(BackgroundColor color2, BackgroundColor color3)
        {
            return !color2.IsEmpty && color3.IsEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility ConvertColor2Visibility(BackgroundColor color2, BackgroundColor color3, BackgroundColor color4)
        {
            if (!color2.IsEmpty && color3.IsEmpty)
            {
                return Visibility.Visible;
            }

            return !color3.IsEmpty && color4.IsEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        private double ConvertIntensity(int intensity)
        {
            return intensity / 100d;
        }

        #endregion

        public void Handle(UpdateFile update)
        {
            this.BeginOnUIThread(() =>
            {
                if (ViewModel.Item is Background wallpaper && wallpaper.UpdateFile(update.File))
                {
                    var big = wallpaper.Document;
                    if (big == null)
                    {
                        return;
                    }

                    if (wallpaper.Type is BackgroundTypeWallpaper)
                    {
                        Presenter.Fill = new ImageBrush { ImageSource = PlaceholderHelper.GetBitmap(null, big.DocumentValue, 0, 0), AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Stretch = Stretch.UniformToFill };
                    }
                    else if (wallpaper.Type is BackgroundTypePattern pattern)
                    {
                        //content.Background = pattern.Fill.ToBrush();
                        //rectangle.Opacity = pattern.Intensity / 100d;
                        if (string.Equals(wallpaper.Document.MimeType, "application/x-tgwallpattern", StringComparison.OrdinalIgnoreCase))
                        {
                            Presenter.Fill = new TiledBrush { SvgSource = PlaceholderHelper.GetVectorSurface(null, big.DocumentValue, ViewModel.GetPatternForeground()) };
                        }
                        else
                        {
                            Presenter.Fill = new ImageBrush { ImageSource = new BitmapImage(UriEx.ToLocal(big.DocumentValue.Local.Path)), AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Stretch = Stretch.UniformToFill };
                        }
                    }
                }

                if (_backgrounds.TryGetValue(update.File.Id, out Background background))
                {
                    background.UpdateFile(update.File);

                    var small = background.Document.Thumbnail;
                    if (small == null)
                    {
                        return;
                    }

                    var container = PatternList.ContainerFromItem(background) as SelectorItem;
                    if (container == null)
                    {
                        return;
                    }

                    var content = container.ContentTemplateRoot as Grid;
                    var photo = content?.Children[0] as Image;

                    if (photo != null)
                    {
                        photo.Source = PlaceholderHelper.GetBitmap(null, small.File, background.Document.Thumbnail.Width, background.Document.Thumbnail.Height);
                    }
                }
            });
        }

        private void RadioColor_Toggled(object sender, RoutedEventArgs e)
        {
            var row = Grid.GetRow(ColorPanel);
            if (row != 2)
            {
                return;
            }

            if (RadioColor1.IsChecked == true)
            {
                PickerColor.Color = ViewModel.Color1;
            }
            else if (RadioColor2.IsChecked == true)
            {
                PickerColor.Color = ViewModel.Color2;
            }
            else if (RadioColor3.IsChecked == true)
            {
                PickerColor.Color = ViewModel.Color3;
            }
            else if (RadioColor4.IsChecked == true)
            {
                PickerColor.Color = ViewModel.Color4;
            }

            TextColor1.SelectAll();
        }

        private void PickerColor_ColorChanged(Controls.ColorPicker sender, Controls.ColorChangedEventArgs args)
        {
            var row = Grid.GetRow(ColorPanel);
            if (row != 2)
            {
                return;
            }

            TextColor1.Color = args.NewColor;

            if (RadioColor1.IsChecked == true)
            {
                ViewModel.Color1 = args.NewColor;
            }
            else if (RadioColor2.IsChecked == true)
            {
                ViewModel.Color2 = args.NewColor;
            }
            else if (RadioColor3.IsChecked == true)
            {
                ViewModel.Color3 = args.NewColor;
            }
            else if (RadioColor4.IsChecked == true)
            {
                ViewModel.Color4 = args.NewColor;
            }
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            var wallpaper = args.Item as Background;
            var root = args.ItemContainer.ContentTemplateRoot as Grid;

            var check = root.Children[1];
            check.Visibility = wallpaper.Id == ViewModel.SelectedPattern?.Id ? Visibility.Visible : Visibility.Collapsed;

            if (wallpaper.Document != null)
            {
                var small = wallpaper.Document.Thumbnail;
                if (small == null)
                {
                    return;
                }

                var content = root.Children[0] as Image;
                var file = small.File;
                if (file.Local.IsDownloadingCompleted)
                {
                    content.Source = PlaceholderHelper.GetBitmap(null, small.File, wallpaper.Document.Thumbnail.Width, wallpaper.Document.Thumbnail.Height);
                }
                else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
                {
                    _backgrounds[file.Id] = wallpaper;
                    ViewModel.ProtoService.DownloadFile(file.Id, 1);
                }

                content.Opacity = ViewModel.Intensity / 100d;
                root.Background = Background;
            }
            else
            {
                var content = root.Children[0] as Image;
                content.Source = null;

                content.Opacity = 1;
                root.Background = Background;
            }
        }

        private void TextColor_ColorChanged(ColorTextBox sender, Controls.ColorChangedEventArgs args)
        {
            if (sender.FocusState == FocusState.Unfocused)
            {
                return;
            }

            PickerColor.Color = args.NewColor;
        }

        private void RemoveColor_Click(object sender, RoutedEventArgs e)
        {
            if (RadioColor1.IsChecked == true)
            {
                ViewModel.RemoveColor(0);
            }
            else if (RadioColor2.IsChecked == true)
            {
                ViewModel.RemoveColor(1);
            }
            else if (RadioColor3.IsChecked == true)
            {
                ViewModel.RemoveColor(2);
            }
            else if (RadioColor4.IsChecked == true)
            {
                ViewModel.RemoveColor(3);
            }
        }

        private void AddRemoveColor_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Color2.IsEmpty || ViewModel.Color3.IsEmpty || ViewModel.Color4.IsEmpty)
            {
                ViewModel.AddColor();
            }
            else if (RadioColor1.IsChecked == true)
            {
                ViewModel.RemoveColor(0);
            }
            else if (RadioColor2.IsChecked == true)
            {
                ViewModel.RemoveColor(1);
            }
            else if (RadioColor3.IsChecked == true)
            {
                ViewModel.RemoveColor(2);
            }
            else if (RadioColor4.IsChecked == true)
            {
                ViewModel.RemoveColor(3);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_fill is BackgroundFillFreeformGradient freeform)
            {
                _freeform.UpdateLayout(Background as ImageBrush, ActualWidth, ActualHeight, freeform, true);
            }
        }
    }
}
