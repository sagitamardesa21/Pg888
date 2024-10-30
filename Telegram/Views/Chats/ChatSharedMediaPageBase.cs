//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using Telegram.Common;
using Telegram.Controls;
using Telegram.Controls.Chats;
using Telegram.Controls.Messages;
using Telegram.Converters;
using Telegram.ViewModels;
using Telegram.ViewModels.Chats;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Telegram.Views.Chats
{
    public class ChatSharedMediaPageBase : Page
    {
        public ProfileViewModel ViewModel => DataContext as ProfileViewModel;

        public ProfileHeader Header => ProfileHeader;

        private CompositionPropertySet _properties;

        private readonly DispatcherTimer _dateHeaderTimer;
        private Visual _dateHeaderPanel;
        private bool _dateHeaderCollapsed = true;

        public ChatSharedMediaPageBase()
        {
            _dateHeaderTimer = new DispatcherTimer();
            _dateHeaderTimer.Interval = TimeSpan.FromMilliseconds(2000);
            _dateHeaderTimer.Tick += (s, args) =>
            {
                _dateHeaderTimer.Stop();
                ShowHideDateHeader(false, true);
            };
        }

        #region Date visibility

        private void ShowHideDateHeader(bool show, bool animate)
        {
            if (_dateHeaderCollapsed != show)
            {
                return;
            }

            _dateHeaderCollapsed = !show;
            DateHeader.Visibility = show || animate ? Visibility.Visible : Visibility.Collapsed;

            _dateHeaderPanel ??= ElementCompositionPreview.GetElementVisual(DateHeader);

            if (!animate)
            {
                _dateHeaderPanel.Opacity = show ? 1 : 0;
                return;
            }

            var batch = _dateHeaderPanel.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, args) =>
            {
                if (show)
                {
                    _dateHeaderCollapsed = false;
                }
                else
                {
                    DateHeader.Visibility = Visibility.Collapsed;
                }
            };

            var opacity = _dateHeaderPanel.Compositor.CreateScalarKeyFrameAnimation();
            opacity.InsertKeyFrame(0, show ? 0 : 1);
            opacity.InsertKeyFrame(1, show ? 1 : 0);

            _dateHeaderPanel.StartAnimation("Opacity", opacity);

            batch.End();
        }

        #endregion

        #region Context menu

        private void Message_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var flyout = new MenuFlyout();

            var element = sender as FrameworkElement;
            var message = element.Tag as MessageWithOwner;

            var selected = ViewModel.SelectedItems;
            if (selected.Count > 0)
            {
                if (selected.Contains(message))
                {
                    flyout.CreateFlyoutItem(ViewModel.ForwardSelectedMessages, Strings.ForwardSelected, new FontIcon { Glyph = Icons.Share });

                    //if (chat.CanBeReported)
                    //{
                    //    flyout.CreateFlyoutItem(ViewModel.MessagesReportCommand, "Report Selected", new FontIcon { Glyph = Icons.ShieldError });
                    //}

                    flyout.CreateFlyoutItem(ViewModel.DeleteSelectedMessages, Strings.DeleteSelected, new FontIcon { Glyph = Icons.Delete });
                    flyout.CreateFlyoutItem(ViewModel.UnselectMessages, Strings.ClearSelection);
                    //flyout.CreateFlyoutSeparator();
                    //flyout.CreateFlyoutItem(ViewModel.MessagesCopyCommand, "Copy Selected as Text", new FontIcon { Glyph = Icons.DocumentCopy });
                }
                else
                {
                    flyout.CreateFlyoutItem(MessageSelect_Loaded, ViewModel.SelectMessage, message, Strings.Select, new FontIcon { Glyph = Icons.CheckmarkCircle });
                }
            }
            else
            {

                flyout.CreateFlyoutItem(MessageView_Loaded, ViewModel.ViewMessage, message, Strings.ShowInChat, new FontIcon { Glyph = Icons.Comment });
                flyout.CreateFlyoutItem(MessageDelete_Loaded, ViewModel.DeleteMessage, message, Strings.Delete, new FontIcon { Glyph = Icons.Delete });
                flyout.CreateFlyoutItem(MessageForward_Loaded, ViewModel.ForwardMessage, message, Strings.Forward, new FontIcon { Glyph = Icons.Share });
                flyout.CreateFlyoutItem(MessageSelect_Loaded, ViewModel.SelectMessage, message, Strings.Select, new FontIcon { Glyph = Icons.CheckmarkCircle });
                flyout.CreateFlyoutItem(MessageSave_Loaded, ViewModel.SaveMessage, message, Strings.SaveAs, new FontIcon { Glyph = Icons.SaveAs });
            }

            args.ShowAt(flyout, element);
        }

        private bool MessageView_Loaded(MessageWithOwner message)
        {
            return true;
        }

        private bool MessageSave_Loaded(MessageWithOwner message)
        {
            return true;
        }

        private bool MessageDelete_Loaded(MessageWithOwner message)
        {
            return message.CanBeDeletedOnlyForSelf || message.CanBeDeletedForAllUsers;
        }

        private bool MessageForward_Loaded(MessageWithOwner message)
        {
            return message.CanBeForwarded;
        }

        private bool MessageSelect_Loaded(MessageWithOwner message)
        {
            return true;
        }

        #endregion

        protected virtual void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            if (args.ItemContainer == null)
            {
                if (sender is ListView)
                {
                    args.ItemContainer = new TableAccessibleChatListViewItem(ViewModel.ClientService);
                }
                else
                {
                    args.ItemContainer = new ChatGridViewItem(ViewModel.ClientService);
                }

                args.ItemContainer.Style = sender.ItemContainerStyle;
                args.ItemContainer.ContentTemplate = sender.ItemTemplate;
                args.ItemContainer.ContextRequested += Message_ContextRequested;
            }

            if (sender.ItemTemplateSelector != null)
            {
                args.ItemContainer.ContentTemplate = sender.ItemTemplateSelector.SelectTemplate(args.Item, args.ItemContainer);
            }

            args.IsContainerPrepared = true;
        }



        private ListViewBase _scrollingHost;
        public ListViewBase ScrollingHost => _scrollingHost ??= FindName(nameof(ScrollingHost)) as ListViewBase;

        private ProfileHeader _profileHeader;
        public ProfileHeader ProfileHeader => _profileHeader ??= FindName(nameof(ProfileHeader)) as ProfileHeader;

        private Grid _headerPanel;
        public Grid HeaderPanel => _headerPanel ??= FindName(nameof(HeaderPanel)) as Grid;

        private MessageService _dateHeader;
        public MessageService DateHeader => _dateHeader ??= FindName(nameof(DateHeader)) as MessageService;

        private TextBlock _dateHeaderLabel;
        public TextBlock DateHeaderLabel => _dateHeaderLabel ??= FindName(nameof(DateHeaderLabel)) as TextBlock;

        private Border _cardBackground;
        public Border CardBackground => _cardBackground ??= FindName(nameof(CardBackground)) as Border;

        private Border _clipperBackground;
        public Border ClipperBackground => _clipperBackground ??= FindName(nameof(ClipperBackground)) as Border;

        protected virtual float TopPadding => 48;

        protected void List_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollingHost = ScrollingHost.GetScrollViewer();
            if (scrollingHost != null)
            {
                Canvas.SetZIndex(ScrollingHost.ItemsPanelRoot, -1);

                var properties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollingHost);
                var visual = ElementCompositionPreview.GetElementVisual(HeaderPanel);
                var panel = ElementCompositionPreview.GetElementVisual(ScrollingHost.ItemsPanelRoot);
                var border = ElementCompositionPreview.GetElementVisual(CardBackground);
                var clipper = ElementCompositionPreview.GetElementVisual(ClipperBackground);

                ElementCompositionPreview.SetIsTranslationEnabled(HeaderPanel, true);

                _properties = visual.Compositor.CreatePropertySet();
                _properties.InsertScalar("ActualHeight", ProfileHeader.ActualSize.Y + 16);
                _properties.InsertScalar("TopPadding", TopPadding);

                var translation = visual.Compositor.CreateExpressionAnimation(
                    "properties.ActualHeight > 16 ? scrollViewer.Translation.Y > -properties.ActualHeight ? 0 : -scrollViewer.Translation.Y - properties.ActualHeight : -scrollViewer.Translation.Y");
                translation.SetReferenceParameter("scrollViewer", properties);
                translation.SetReferenceParameter("properties", _properties);

                var fadeOut = visual.Compositor.CreateExpressionAnimation(
                    "properties.ActualHeight > 16 ? scrollViewer.Translation.Y > -(properties.ActualHeight - 16) ? 1 : 1 - ((-scrollViewer.Translation.Y - (properties.ActualHeight - 16)) / 16) : 0");
                fadeOut.SetReferenceParameter("scrollViewer", properties);
                fadeOut.SetReferenceParameter("properties", _properties);

                var fadeIn = visual.Compositor.CreateExpressionAnimation(
                    "properties.ActualHeight > 16 ? scrollViewer.Translation.Y > -(properties.ActualHeight - 16) ? 0 : ((-scrollViewer.Translation.Y - (properties.ActualHeight - 16)) / 16) : 1");
                fadeIn.SetReferenceParameter("scrollViewer", properties);
                fadeIn.SetReferenceParameter("properties", _properties);

                visual.StartAnimation("Translation.Y", translation);

                border.StartAnimation("Opacity", fadeOut);
                clipper.StartAnimation("Opacity", fadeIn);

                //void handler(object _, object args)
                //{
                //    scrollingHost.LayoutUpdated -= handler;
                //    scrollingHost.ChangeView(null, ViewModel.VerticalOffset, null, true);
                //}

                //scrollingHost.InvalidateScrollInfo();
                //scrollingHost.ChangeView(null, ViewModel.VerticalOffset, null, true);

                //scrollingHost.LayoutUpdated += handler;
                scrollingHost.ViewChanged += OnViewChanged;
            }
        }

        private void ItemsPanelRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Telegram.App.Track();

            ScrollingHost.ItemsPanelRoot.SizeChanged -= ItemsPanelRoot_SizeChanged;

            var scrollingHost = ScrollingHost.GetScrollViewer();
            scrollingHost?.ChangeView(null, ViewModel.VerticalOffset, null, true);
        }

        private void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollingHost)
            {
                ViewModel.VerticalOffset = scrollingHost.VerticalOffset;

                if (DateHeaderLabel == null)
                {
                    return;
                }

                var index = ScrollingHost.ItemsPanelRoot switch
                {
                    ItemsStackPanel stackPanel => stackPanel.FirstVisibleIndex,
                    ItemsWrapGrid wrapGrid => wrapGrid.FirstVisibleIndex,
                    _ => -1
                };

                var container = ScrollingHost.ContainerFromIndex(index) as SelectorItem;
                if (container == null || container.Tag is not MessageWithOwner message)
                {
                    return;
                }

                DateHeaderLabel.Text = Formatter.MonthGrouping(Formatter.ToLocalTime(message.Date));

                _dateHeaderTimer.Stop();
                _dateHeaderTimer.Start();
                ShowHideDateHeader(scrollingHost.VerticalOffset > ProfileHeader.ActualHeight, true);
            }
        }

        protected void ProfileHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Telegram.App.Track();

            _properties?.InsertScalar("ActualHeight", (float)e.NewSize.Height + 16);

            //var panel = ScrollingHost.ItemsPanelRoot;
            //if (panel != null)
            //{
            //    panel.MinHeight = ActualHeight + e.NewSize.Height;
            //}
        }

        protected void Header_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ProfileItem page && page.Type != GetType())
            {
                Frame.Navigate(page.Type, null, new SuppressNavigationTransitionInfo());
            }
        }
    }
}
