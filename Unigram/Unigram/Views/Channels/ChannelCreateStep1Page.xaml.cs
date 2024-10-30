//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Unigram.Common;
using Unigram.Controls;
using Unigram.ViewModels.Channels;
using Unigram.Views.Popups;
using Windows.Storage.Pickers;

namespace Unigram.Views.Channels
{
    public sealed partial class ChannelCreateStep1Page : HostedPage
    {
        public ChannelCreateStep1ViewModel ViewModel => DataContext as ChannelCreateStep1ViewModel;

        public ChannelCreateStep1Page()
        {
            InitializeComponent();
            Title = Strings.Resources.NewChannel;
        }

        private void Title_Loaded(object sender, RoutedEventArgs e)
        {
            TitleLabel.Focus(FocusState.Keyboard);
        }

        private async void EditPhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.AddRange(Constants.PhotoTypes);

                var media = await picker.PickSingleMediaAsync();
                if (media != null)
                {
                    var dialog = new EditMediaPopup(media, ImageCropperMask.Ellipse);

                    var confirm = await dialog.ShowAsync();
                    if (confirm == ContentDialogResult.Primary)
                    {
                        ViewModel.EditPhotoCommand.Execute(media);
                    }
                }
            }
            catch { }
        }

        #region Binding

        private ImageSource ConvertPhoto(string title, BitmapImage preview)
        {
            if (preview != null)
            {
                return preview;
            }

            return PlaceholderHelper.GetNameForChat(title, 64);
        }

        private Visibility ConvertPhotoVisibility(string title, BitmapImage preview)
        {
            return !string.IsNullOrWhiteSpace(title) || preview != null ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion
    }
}
