using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Core.Common;
using Unigram.Core.Helpers;
using Unigram.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Settings
{
    public class SettingsWallPaperViewModel : TLViewModelBase
    {
        private const string TempWallpaperFileName = "temp_wallpaper.jpg";

        public SettingsWallPaperViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            Items = new MvxObservableCollection<Wallpaper>();

            ProtoService.Send(new GetWallpapers(), result =>
            {
                if (result is Wallpapers wallpapers)
                {
                    var items = wallpapers.WallpapersValue.ToList();

                    var predefined = items.FirstOrDefault(x => x.Id == 1000001);
                    if (predefined != null)
                    {
                        items.Remove(predefined);
                        items.Insert(0, predefined);
                    }

                    BeginOnUIThread(() =>
                    {
                        Items.ReplaceWith(items);
                        UpdateView();
                    });
                }
            });

            LocalCommand = new RelayCommand(LocalExecute);
            DoneCommand = new RelayCommand(DoneExecute);
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            UpdateView();
            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        private async void UpdateView()
        {
            if (Items == null)
            {
                return;
            }

            var selected = Settings.SelectedBackground;
            if (selected == -1)
            {
                IsLocal = true;
                SelectedItem = null;

                var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileUtils.GetFilePath(Constants.WallpaperFileName));
                if (item is StorageFile file)
                {
                    using (var stream = await file.OpenReadAsync())
                    {
                        var bitmap = new BitmapImage();
                        try
                        {
                            await bitmap.SetSourceAsync(stream);
                        }
                        catch { }
                        Local = bitmap;
                    }
                }
            }
            else
            {
                SelectedItem = Items.FirstOrDefault(x => x.Id == selected) ?? Items.FirstOrDefault(x => x.Id == 1000001);
            }
        }

        private bool _isLocal;
        public bool IsLocal
        {
            get
            {
                return _isLocal;
            }
            set
            {
                Set(ref _isLocal, value);
            }
        }

        private BitmapImage _local;
        public BitmapImage Local
        {
            get
            {
                return _local;
            }
            set
            {
                Set(ref _local, value);
            }
        }

        private Wallpaper _selectedItem;
        public Wallpaper SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                Set(ref _selectedItem, value);

                if (value != null)
                {
                    Local = null;
                    IsLocal = false;
                }
            }
        }

        public MvxObservableCollection<Wallpaper> Items { get; private set; }

        public RelayCommand LocalCommand { get; }
        private async void LocalExecute()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.AddRange(Constants.PhotoTypes);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var result = await FileUtils.CreateTempFileAsync(TempWallpaperFileName);
                await file.CopyAndReplaceAsync(result);

                IsLocal = true;
                SelectedItem = null;

                using (var stream = await result.OpenReadAsync())
                {
                    var bitmap = new BitmapImage();
                    try
                    {
                        await bitmap.SetSourceAsync(stream);
                    }
                    catch { }
                    Local = bitmap;
                }
            }
        }

        public RelayCommand DoneCommand { get; }
        private async void DoneExecute()
        {
            var wallpaper = _selectedItem;
            if (wallpaper != null && wallpaper.Sizes != null && wallpaper.Sizes.Count > 0)
            {
                if (wallpaper.Id != 1000001)
                {
                    //var photoSize = wallpaper.Full as TLPhotoSize;
                    //var location = photoSize.Location as TLFileLocation;
                    //var fileName = string.Format("{0}_{1}_{2}.jpg", location.VolumeId, location.LocalId, location.Secret);

                    //var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileUtils.GetTempFilePath(fileName));
                    //if (item is StorageFile file)
                    //{
                    //    var result = await FileUtils.CreateFileAsync(Constants.WallpaperFileName);
                    //    await file.CopyAndReplaceAsync(result);

                    //    var accent = await ImageHelper.GetAccentAsync(result);
                    //    Theme.Current.AddOrUpdateColor("MessageServiceBackgroundBrush", accent[0]);
                    //    Theme.Current.AddOrUpdateColor("MessageServiceBackgroundPressedBrush", accent[1]);
                    //}
                    //else
                    //{
                    //    return;
                    //}
                }
                else
                {
                    Theme.Current.AddOrUpdateColor("MessageServiceBackgroundBrush", Color.FromArgb(0x66, 0x7A, 0x8A, 0x96));
                    Theme.Current.AddOrUpdateColor("MessageServiceBackgroundPressedBrush", Color.FromArgb(0x88, 0x7A, 0x8A, 0x96));
                }

                Settings.SelectedBackground = wallpaper.Id;
                Settings.SelectedColor = 0;
            }
            else if (wallpaper != null)
            {
                Settings.SelectedBackground = wallpaper.Id;
                Settings.SelectedColor = wallpaper.Color;
            }
            else if (_selectedItem == null && _isLocal)
            {
                var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileUtils.GetTempFilePath(TempWallpaperFileName));
                if (item is StorageFile file)
                {
                    var result = await FileUtils.CreateFileAsync(Constants.WallpaperFileName);
                    await file.CopyAndReplaceAsync(result);

                    var accent = await ImageHelper.GetAccentAsync(result);
                    Theme.Current.AddOrUpdateColor("MessageServiceBackgroundBrush", accent[0]);
                    Theme.Current.AddOrUpdateColor("MessageServiceBackgroundPressedBrush", accent[1]);
                }
                else
                {
                    return;
                }

                Settings.SelectedBackground = -1;
                Settings.SelectedColor = 0;
            }

            Aggregator.Publish("Wallpaper");
            NavigationService.GoBack();
        }
    }
}
