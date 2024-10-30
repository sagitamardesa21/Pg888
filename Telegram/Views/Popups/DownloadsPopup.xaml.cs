//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Telegram.Common;
using Telegram.Controls;
using Telegram.Controls.Cells;
using Telegram.Controls.Media;
using Telegram.Navigation.Services;
using Telegram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Telegram.Views.Popups
{
    public sealed partial class DownloadsPopup : ContentPopup
    {
        public DownloadsViewModel ViewModel => DataContext as DownloadsViewModel;

        public DownloadsPopup(int sessionId, INavigationService navigationService)
        {
            InitializeComponent();
            DataContext = TypeResolver.Current.Resolve<DownloadsViewModel>(sessionId);

            PrimaryButtonText = Strings.Close;

            ViewModel.Dispatcher = navigationService.Dispatcher;
            ViewModel.NavigationService = navigationService;
            ViewModel.Hide = Hide;
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            _ = ViewModel.NavigatedToAsync(null, default, null);
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            ViewModel.NavigatedFrom(null, false);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void Menu_ContextRequested(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel == null)
            {
                return;
            }

            var flyout = new MenuFlyout();

            if (viewModel.TotalActiveCount > 0)
            {
                flyout.CreateFlyoutItem(ViewModel.ToggleAllPaused, Strings.PauseAll, Icons.Pause);
            }
            else if (viewModel.TotalPausedCount > 0)
            {
                flyout.CreateFlyoutItem(ViewModel.ToggleAllPaused, Strings.ResumeAll, Icons.Play);
            }

            flyout.CreateFlyoutItem(ViewModel.OpenSettings, Strings.Settings, Icons.Settings);

            if (viewModel.Items.Count > 0)
            {
                // TODO: DeleteAll => RemoveAll?
                flyout.CreateFlyoutItem(ViewModel.RemoveAll, Strings.DeleteAll, Icons.Delete);
            }

            flyout.ShowAt(sender as DependencyObject, FlyoutPlacementMode.BottomEdgeAlignedRight);
        }

        private void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            if (args.ItemContainer == null)
            {
                args.ItemContainer = new ListViewItem();
                args.ItemContainer.Style = sender.ItemContainerStyle;
                args.ItemContainer.ContentTemplate = sender.ItemTemplate;
                args.ItemContainer.ContextRequested += OnContextRequested;
            }

            args.IsContainerPrepared = true;
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            var content = args.ItemContainer.ContentTemplateRoot as FileDownloadCell;
            var file = args.Item as FileDownloadViewModel;

            content.UpdateFileDownload(ViewModel, file);
            args.ItemContainer.Tag = file;
        }

        private void OnContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var element = sender as FrameworkElement;
            var fileDownload = element.Tag as FileDownloadViewModel;

            var flyout = new MenuFlyout();

            if (fileDownload.CompleteDate == 0)
            {
                flyout.CreateFlyoutItem(ViewModel.RemoveFileDownload, fileDownload, Strings.AccActionCancelDownload, Icons.Dismiss);
                flyout.CreateFlyoutItem(ViewModel.ViewFileDownload, fileDownload, Strings.ViewInChat, Icons.ChatEmpty);
            }
            else
            {
                flyout.CreateFlyoutItem(ViewModel.ViewFileDownload, fileDownload, Strings.ViewInChat, Icons.ChatEmpty);
                flyout.CreateFlyoutItem(ViewModel.ShowFileDownload, fileDownload, Strings.ShowInFolder, Icons.FolderOpen);

                flyout.CreateFlyoutItem(ViewModel.RemoveFileDownload, fileDownload, Strings.DeleteFromRecent, Icons.Delete);

                //flyout.CreateFlyoutSeparator();
                //flyout.CreateFlyoutItem(_ => { }, fileDownload, Strings.lng_context_select_msg, Icons.CheckmarkCircle);
            }

            args.ShowAt(flyout, sender as FrameworkElement);
        }
    }
}
