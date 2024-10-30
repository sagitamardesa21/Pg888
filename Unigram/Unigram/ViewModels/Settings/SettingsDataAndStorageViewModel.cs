using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Controls.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Unigram.Services;
using Unigram.Views.Settings;
using Telegram.Td.Api;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;

namespace Unigram.ViewModels.Settings
{
    public class SettingsDataAndStorageViewModel : TLViewModelBase
    {
        public SettingsDataAndStorageViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            AutoDownloadCommand = new RelayCommand<AutoDownloadType>(AutoDownloadExecute);
            ResetAutoDownloadCommand = new RelayCommand(ResetAutoDownloadExecute);
            DownloadLocationCommand = new RelayCommand(DownloadLocationExecute);
            UseLessDataCommand = new RelayCommand(UseLessDataExecute);
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            //foreach (var item in AutoDownloads)
            //{
            //    item.Refresh();
            //}

            return Task.CompletedTask;
        }

        public libtgvoip.DataSavingMode UseLessData
        {
            get
            {
                return Settings.UseLessData;
            }
            set
            {
                Settings.UseLessData = value;
                RaisePropertyChanged();
            }
        }

        public bool AutoDownloadEnabled
        {
            get
            {
                return !ProtoService.Preferences.Disabled;
            }
            set
            {
                ProtoService.SetPreferences(ProtoService.Preferences.UpdateDisabled(!value));
                RaisePropertyChanged();
            }
        }

        public string FilesDirectory
        {
            get
            {
                return Settings.FilesDirectory;
            }
            set
            {
                Settings.FilesDirectory = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand UseLessDataCommand { get; }
        private async void UseLessDataExecute()
        {
            var dialog = new ContentDialog { Style = BootStrapper.Current.Resources["ModernContentDialogStyle"] as Style };
            var stack = new StackPanel();
            stack.Margin = new Thickness(12, 16, 12, 0);
            stack.Children.Add(new RadioButton { Tag = 0, Content = Strings.Resources.UseLessDataNever, IsChecked = UseLessData == libtgvoip.DataSavingMode.Never });
            stack.Children.Add(new RadioButton { Tag = 1, Content = Strings.Resources.UseLessDataOnMobile, IsChecked = UseLessData == libtgvoip.DataSavingMode.MobileOnly });
            stack.Children.Add(new RadioButton { Tag = 2, Content = Strings.Resources.UseLessDataAlways, IsChecked = UseLessData == libtgvoip.DataSavingMode.Always });

            dialog.Title = Strings.Resources.VoipUseLessData;
            dialog.Content = stack;
            dialog.PrimaryButtonText = Strings.Resources.OK;
            dialog.SecondaryButtonText = Strings.Resources.Cancel;

            var confirm = await dialog.ShowQueuedAsync();
            if (confirm == ContentDialogResult.Primary)
            {
                var mode = 1;
                foreach (RadioButton current in stack.Children)
                {
                    if (current.IsChecked == true)
                    {
                        mode = (int)current.Tag;
                        break;
                    }
                }

                UseLessData = (libtgvoip.DataSavingMode)mode;
            }
        }

        public RelayCommand DownloadLocationCommand { get; }
        private async void DownloadLocationExecute()
        {
            var dialog = new ContentDialog { Style = BootStrapper.Current.Resources["ModernContentDialogStyle"] as Style };
            var stack = new StackPanel();
            stack.Margin = new Thickness(12, 16, 12, 0);
            stack.Children.Add(new RadioButton { Tag = 1, Content = "Temp folder, cleared on logout or uninstall", IsChecked = FilesDirectory == null });
            stack.Children.Add(new RadioButton { Tag = 2, Content = "Custom folder, cleared only manually", IsChecked = FilesDirectory != null });

            dialog.Title = "Choose download location";
            dialog.Content = stack;
            dialog.PrimaryButtonText = Strings.Resources.OK;
            dialog.SecondaryButtonText = Strings.Resources.Cancel;

            var confirm = await dialog.ShowQueuedAsync();
            if (confirm == ContentDialogResult.Primary)
            {
                var mode = 1;
                var path = FilesDirectory + string.Empty;
                foreach (RadioButton current in stack.Children)
                {
                    if (current.IsChecked == true)
                    {
                        mode = (int)current.Tag;
                        break;
                    }
                }

                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        FilesDirectory = null;
                        break;
                    case 2:
                        var picker = new FolderPicker();
                        picker.SuggestedStartLocation = PickerLocationId.Downloads;
                        picker.FileTypeFilter.Add("*");

                        var folder = await picker.PickSingleFolderAsync();
                        if (folder != null)
                        {
                            StorageApplicationPermissions.FutureAccessList.AddOrReplace("FilesDirectory", folder);
                            FilesDirectory = folder.Path;
                        }

                        break;
                }

                if (string.Equals(path, FilesDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                ProtoService.Send(new Close());
            }
        }

        public RelayCommand<AutoDownloadType> AutoDownloadCommand { get; }
        public void AutoDownloadExecute(AutoDownloadType type)
        {
            NavigationService.Navigate(typeof(SettingsDataAutoPage), type);
        }

        public RelayCommand ResetAutoDownloadCommand { get; }
        private async void ResetAutoDownloadExecute()
        {
            var confirm = await TLMessageDialog.ShowAsync(Strings.Resources.ResetAutomaticMediaDownloadAlert, Strings.Resources.AppName, Strings.Resources.OK, Strings.Resources.Cancel);
            if (confirm == ContentDialogResult.Primary)
            {
                ProtoService.SetPreferences(AutoDownloadPreferences.Default);
                RaisePropertyChanged(() => AutoDownloadEnabled);
            }
        }
    }
}
