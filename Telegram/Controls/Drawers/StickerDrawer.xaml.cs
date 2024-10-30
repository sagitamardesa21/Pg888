//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using LinqToVisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Common;
using Telegram.Converters;
using Telegram.Services;
using Telegram.Td.Api;
using Telegram.ViewModels.Drawers;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace Telegram.Controls.Drawers
{
    public sealed partial class StickerDrawer : UserControl, IDrawer
    {
        public StickerDrawerViewModel ViewModel => DataContext as StickerDrawerViewModel;

        public Action<Sticker, bool> ItemClick { get; set; }
        public event TypedEventHandler<UIElement, ItemContextRequestedEventArgs<Sticker>> ItemContextRequested;
        public event EventHandler ChoosingItem;

        private readonly AnimatedListHandler _handler;
        private readonly ZoomableListHandler _zoomer;

        private readonly AnimatedListHandler _toolbarHandler;

        private readonly Dictionary<string, DataTemplate> _typeToTemplateMapping = new Dictionary<string, DataTemplate>();
        private readonly Dictionary<string, HashSet<SelectorItem>> _typeToItemHashSetMapping = new Dictionary<string, HashSet<SelectorItem>>();

        private bool _isActive;

        public StickerDrawer()
        {
            InitializeComponent();

            ElementCompositionPreview.GetElementVisual(this).Clip = Window.Current.Compositor.CreateInsetClip();

            _handler = new AnimatedListHandler(List, AnimatedListType.Stickers);
            _toolbarHandler = new AnimatedListHandler(Toolbar, AnimatedListType.Stickers);

            _zoomer = new ZoomableListHandler(List);
            _zoomer.Opening = _handler.UnloadVisibleItems;
            _zoomer.Closing = _handler.ThrottleVisibleItems;
            _zoomer.DownloadFile = fileId => ViewModel.ClientService.DownloadFile(fileId, 32);
            _zoomer.SessionId = () => ViewModel.ClientService.SessionId;

            _typeToItemHashSetMapping.Add("AnimatedItemTemplate", new HashSet<SelectorItem>());
            _typeToItemHashSetMapping.Add("VideoItemTemplate", new HashSet<SelectorItem>());
            _typeToItemHashSetMapping.Add("ItemTemplate", new HashSet<SelectorItem>());

            _typeToTemplateMapping.Add("AnimatedItemTemplate", Resources["AnimatedItemTemplate"] as DataTemplate);
            _typeToTemplateMapping.Add("VideoItemTemplate", Resources["VideoItemTemplate"] as DataTemplate);
            _typeToTemplateMapping.Add("ItemTemplate", Resources["ItemTemplate"] as DataTemplate);

            //_toolbarHandler = new AnimatedStickerHandler<StickerSetViewModel>(Toolbar);

            var header = DropShadowEx.Attach(Separator);
            header.Clip = header.Compositor.CreateInsetClip(0, 40, 0, -40);

            //var debouncer = new EventDebouncer<TextChangedEventArgs>(Constants.TypingTimeout, handler => FieldStickers.TextChanged += new TextChangedEventHandler(handler));
            //debouncer.Invoked += async (s, args) =>
            //{
            //    var items = ViewModel.SearchStickers;
            //    if (items != null && string.Equals(FieldStickers.Text, items.Query))
            //    {
            //        await items.LoadMoreItemsAsync(1);
            //        await items.LoadMoreItemsAsync(2);
            //    }
            //};
        }

        public Services.Settings.StickersTab Tab => Services.Settings.StickersTab.Stickers;

        public Thickness ScrollingHostPadding
        {
            get => List.Padding;
            set => List.Padding = new Thickness(2, value.Top, 2, value.Bottom);
        }

        public ListViewBase ScrollingHost => List;

        public void Activate(Chat chat, EmojiSearchType type = EmojiSearchType.Default)
        {
            _isActive = true;
            _handler.ThrottleVisibleItems();
            _toolbarHandler.ThrottleVisibleItems();

            //SearchField.SetType(ViewModel.ClientService, type);
            ViewModel.Update(chat);
        }

        public void Deactivate()
        {
            _isActive = false;
            _handler.UnloadItems();
            _toolbarHandler.UnloadItems();

            // This is called only right before XamlMarkupHelper.UnloadObject
            // so we can safely clean up any kind of anything from here.
            _zoomer.Release();
            Bindings.StopTracking();
        }

        public void LoadVisibleItems()
        {
            if (_isActive)
            {
                _handler.LoadVisibleItems(false);
                _toolbarHandler.LoadVisibleItems(false);
            }
        }

        public void UnloadVisibleItems()
        {
            _handler.UnloadVisibleItems();
            _toolbarHandler.UnloadVisibleItems();
        }

        private async void UpdateSticker(object target, File file)
        {
            var content = target as Grid;
            if (content == null)
            {
                return;
            }

            if (content.Children[0] is Border border && border.Child is Image photo)
            {
                photo.Source = await PlaceholderHelper.GetWebPFrameAsync(file.Local.Path, 68);
                ElementCompositionPreview.SetElementChildVisual(content.Children[0], null);
            }
            else if (content.Children[0] is LottieView lottie)
            {
                lottie.Source = UriEx.ToLocal(file.Local.Path);
                _handler.ThrottleVisibleItems();
            }
            else if (content.Children[0] is AnimationView video)
            {
                video.Source = new LocalVideoSource(file);
                _handler.ThrottleVisibleItems();
            }
        }

        private async void UpdateStickerSet(object target, File file)
        {
            var content = target as Grid;
            if (content == null)
            {
                return;
            }

            if (content.Children[0] is Border border && border.Child is Image photo)
            {
                photo.Source = await PlaceholderHelper.GetWebPFrameAsync(file.Local.Path, 68);
                ElementCompositionPreview.SetElementChildVisual(content.Children[0], null);
            }
            else if (content.Children[0] is LottieView lottie)
            {
                lottie.Source = UriEx.ToLocal(file.Local.Path);
                _toolbarHandler.ThrottleVisibleItems();
            }
            else if (content.Children[0] is AnimationView video)
            {
                video.Source = new LocalVideoSource(file);
                _toolbarHandler.ThrottleVisibleItems();
            }
        }

        private void Stickers_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is StickerViewModel sticker && sticker.StickerValue != null)
            {
                var container = List.ContainerFromItem(e.ClickedItem);

                var groupContainer = List.GroupHeaderContainerFromItemContainer(container) as GridViewHeaderItem;
                if (groupContainer == null)
                {
                    ItemClick?.Invoke(sticker, false);
                    return;
                }

                var stickerSet = groupContainer.Content as StickerSetViewModel;
                if (stickerSet != null)
                {
                    ItemClick?.Invoke(sticker, stickerSet.Id != 0);
                }
            }
        }

        private void Stickers_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollingHost = List.Descendants<ScrollViewer>().FirstOrDefault();
            if (scrollingHost != null)
            {
                scrollingHost.VerticalSnapPointsType = SnapPointsType.None;

                // Syncronizes GridView with the toolbar ListView
                scrollingHost.ViewChanged += ScrollingHost_ViewChanged;
                ScrollingHost_ViewChanged(null, null);
            }
        }

        private void Toolbar_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is StickerSetViewModel set && set.Stickers != null)
            {
                List.ScrollIntoView(e.ClickedItem, ScrollIntoViewAlignment.Leading);
            }
        }

        private void ScrollingHost_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollingHost = List.ItemsPanelRoot as ItemsWrapGrid;
            if (scrollingHost != null && _isActive)
            {
                var first = List.ContainerFromIndex(scrollingHost.FirstVisibleIndex);
                if (first != null)
                {
                    var header = List.GroupHeaderContainerFromItemContainer(first) as GridViewHeaderItem;
                    if (header != null && header.Content != Toolbar.SelectedItem)
                    {
                        Toolbar.SelectedItem = header.Content;
                        Toolbar.ScrollIntoView(header.Content);
                    }
                }
            }

            if (sender is ScrollViewer scrollViewer && scrollViewer.VerticalOffset > 0 && e.IsIntermediate)
            {
                ChoosingItem?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.NavigationService.Navigate(typeof(SettingsStickersPage));
        }

        private async void OnChoosingGroupHeaderContainer(ListViewBase sender, ChoosingGroupHeaderContainerEventArgs args)
        {
            if (args.GroupHeaderContainer == null)
            {
                args.GroupHeaderContainer = new GridViewHeaderItem();
                args.GroupHeaderContainer.Style = List.GroupStyle[0].HeaderContainerStyle;
                args.GroupHeaderContainer.ContentTemplate = List.GroupStyle[0].HeaderTemplate;
            }

            if (args.Group is StickerSetViewModel group && !group.IsLoaded)
            {
                group.IsLoaded = true;

                //Debug.WriteLine("Loading sticker set " + group.Id);

                var response = await ViewModel.ClientService.SendAsync(new GetStickerSet(group.Id));
                if (response is StickerSet full)
                {
                    group.Update(full, false);

                    //return;

                    foreach (var sticker in group.Stickers)
                    {
                        var container = List?.ContainerFromItem(sticker) as SelectorItem;
                        if (container == null)
                        {
                            continue;
                        }

                        UpdateContainerContent(sticker, container.ContentTemplateRoot as Grid, UpdateSticker);
                    }
                }
            }
        }

        private void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            var typeName = args.Item is StickerViewModel sticker ? sticker.Format switch
            {
                StickerFormatTgs => "AnimatedItemTemplate",
                StickerFormatWebm => "VideoItemTemplate",
                _ => "ItemTemplate"
            } : "ItemTemplate";
            var relevantHashSet = _typeToItemHashSetMapping[typeName];

            // args.ItemContainer is used to indicate whether the ListView is proposing an
            // ItemContainer (ListViewItem) to use. If args.Itemcontainer != null, then there was a
            // recycled ItemContainer available to be reused.
            if (args.ItemContainer != null)
            {
                if (args.ItemContainer.Tag.Equals(typeName))
                {
                    // Suggestion matches what we want, so remove it from the recycle queue
                    relevantHashSet.Remove(args.ItemContainer);
                }
                else
                {
                    // The ItemContainer's datatemplate does not match the needed
                    // datatemplate.
                    // Don't remove it from the recycle queue, since XAML will resuggest it later
                    args.ItemContainer = null;
                }
            }

            // If there was no suggested container or XAML's suggestion was a miss, pick one up from the recycle queue
            // or create a new one
            if (args.ItemContainer == null)
            {
                // See if we can fetch from the correct list.
                if (relevantHashSet.Count > 0)
                {
                    // Unfortunately have to resort to LINQ here. There's no efficient way of getting an arbitrary
                    // item from a hashset without knowing the item. Queue isn't usable for this scenario
                    // because you can't remove a specific element (which is needed in the block above).
                    args.ItemContainer = relevantHashSet.First();
                    relevantHashSet.Remove(args.ItemContainer);
                }
                else
                {
                    // There aren't any (recycled) ItemContainers available. So a new one
                    // needs to be created.
                    var item = new GridViewItem();
                    item.ContentTemplate = _typeToTemplateMapping[typeName];
                    item.Style = sender.ItemContainerStyle;
                    item.Tag = typeName;
                    item.ContextRequested += OnContextRequested;
                    args.ItemContainer = item;

                    _zoomer.ElementPrepared(args.ItemContainer);
                }
            }

            // Indicate to XAML that we picked a container for it
            args.IsContainerPrepared = true;

        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var content = args.ItemContainer.ContentTemplateRoot as Grid;
            var sticker = args.Item as StickerViewModel;

            if (args.InRecycleQueue || sticker == null)
            {
                if (sticker != null)
                {
                    var tag = args.ItemContainer.Tag as string;
                    var added = _typeToItemHashSetMapping[tag].Add(args.ItemContainer);

                    ClearContainerContent(content);
                }

                return;
            }

            UpdateContainerContent(sticker, content, UpdateSticker, args);
            args.Handled = true;
        }

        private async void UpdateContainerContent(Sticker sticker, Grid content, UpdateHandler<File> handler, ContainerContentChangingEventArgs args = null)
        {
            var file = sticker?.StickerValue;
            if (file == null)
            {
                return;
            }

            if (content.Children.Count > 1 && content.Children[1] is Border panel && panel.Child is TextBlock premium)
            {
                if (sticker.FullType is StickerFullTypeRegular regular && regular.PremiumAnimation != null && ViewModel.ClientService.IsPremiumAvailable)
                {
                    premium.Text = ViewModel.ClientService.IsPremium ? Icons.Premium16 : Icons.LockClosed16;
                    panel.HorizontalAlignment = ViewModel.ClientService.IsPremium ? HorizontalAlignment.Right : HorizontalAlignment.Center;
                    panel.Visibility = Visibility.Visible;
                }
                else
                {
                    panel.Visibility = Visibility.Collapsed;
                }
            }

            if (content.Tag is not null
                || content.Tag is Sticker prev && prev?.StickerValue.Id == file.Id)
            {
                return;
            }

            if ((args == null || args.Phase == 2) && file.Local.IsDownloadingCompleted)
            {
                if (content.Children[0] is Border border && border.Child is Image photo)
                {
                    photo.Source = await PlaceholderHelper.GetWebPFrameAsync(file.Local.Path, 68);
                    ElementCompositionPreview.SetElementChildVisual(content.Children[0], null);
                }
                else if (content.Children[0] is LottieView lottie)
                {
                    lottie.Source = UriEx.ToLocal(file.Local.Path);
                }
                else if (content.Children[0] is AnimationView video)
                {
                    video.Source = new LocalVideoSource(file);
                }

                UpdateManager.Unsubscribe(content);
                content.Tag = sticker;
            }
            else
            {
                ClearContainerContent(content);

                CompositionPathParser.ParseThumbnail(sticker, out ShapeVisual visual, false);
                ElementCompositionPreview.SetElementChildVisual(content.Children[0], visual);

                UpdateManager.Subscribe(content, ViewModel.ClientService, file, handler, true);

                if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive /*&& args.Phase == 0*/)
                {
                    ViewModel.ClientService.DownloadFile(file.Id, 1);
                }
            }

            if (args?.Phase == 0)
            {
                args.RegisterUpdateCallback(2, OnContainerContentChanging);
            }
        }

        private void Toolbar_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            if (args.Item is SupergroupStickerSetViewModel supergroup)
            {
                Automation.SetToolTip(args.ItemContainer, supergroup.Title);

                var chat = ViewModel.ClientService.GetChat(supergroup.ChatId);
                if (chat == null)
                {
                    return;
                }

                var content = args.ItemContainer.ContentTemplateRoot as Border;
                if (content?.Child is not ProfilePicture photo)
                {
                    return;
                }

                photo.SetChat(ViewModel.ClientService, chat, 36);
            }
            else if (args.Item is StickerSetViewModel sticker)
            {
                Automation.SetToolTip(args.ItemContainer, sticker.Title);

                var content = args.ItemContainer.ContentTemplateRoot as Grid;

                if (content == null || sticker == null || (sticker.Thumbnail == null && sticker.Covers == null))
                {
                    return;
                }

                var cover = sticker.GetThumbnail();
                if (cover == null)
                {
                    ClearContainerContent(content);
                    return;
                }

                UpdateContainerContent(cover, content, UpdateStickerSet);
            }
        }

        private void ClearContainerContent(Grid content)
        {
            content.Tag = null;

            if (content.Children[0] is Border border && border.Child is Image photo)
            {
                photo.Source = null;
            }
            else if (content.Children[0] is LottieView lottie)
            {
                lottie.Source = null;
            }
            else if (content.Children[0] is AnimationView video)
            {
                video.Source = null;
            }
        }

        private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Search(SearchField.Text, false);
        }

        private void SearchField_CategorySelected(object sender, EmojiCategorySelectedEventArgs e)
        {
            ViewModel.Search(string.Join(" ", e.Category.Emojis), true);
        }

        private void OnContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var sticker = List.ItemFromContainer(sender) as StickerViewModel;
            if (sticker == null)
            {
                return;
            }

            ItemContextRequested?.Invoke(sender, new ItemContextRequestedEventArgs<Sticker>(sticker, args));
        }
    }
}
