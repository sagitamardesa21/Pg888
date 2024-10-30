﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Converters;
using Unigram.Services;
using Unigram.Services.Settings;
using Unigram.ViewModels.Settings;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Unigram.Views.Settings
{
    public sealed partial class SettingsStickersPage : HostedPage, IHandle<UpdateFile>
    {
        public SettingsStickersViewModel ViewModel => DataContext as SettingsStickersViewModel;

        private readonly FileContext<StickerSetInfo> _filesMap = new FileContext<StickerSetInfo>();

        private readonly Dictionary<string, DataTemplate> _typeToTemplateMapping = new Dictionary<string, DataTemplate>();
        private readonly Dictionary<string, HashSet<SelectorItem>> _typeToItemHashSetMapping = new Dictionary<string, HashSet<SelectorItem>>();

        private readonly AnimatedListHandler<StickerSetInfo> _handler;

        public SettingsStickersPage()
        {
            InitializeComponent();
            DataContext = TLContainer.Current.Resolve<SettingsStickersViewModel>();

            _handler = new AnimatedListHandler<StickerSetInfo>(List);

            _typeToItemHashSetMapping.Add("AnimatedItemTemplate", new HashSet<SelectorItem>());
            _typeToItemHashSetMapping.Add("ItemTemplate", new HashSet<SelectorItem>());

            _typeToTemplateMapping.Add("AnimatedItemTemplate", Resources["AnimatedItemTemplate"] as DataTemplate);
            _typeToTemplateMapping.Add("ItemTemplate", Resources["ItemTemplate"] as DataTemplate);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Aggregator.Subscribe(this);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Aggregator.Unsubscribe(this);
        }

        private void FeaturedStickers_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsStickersPage), (int)StickersType.Trending, new DrillInNavigationTransitionInfo());
        }

        private void ArchivedStickers_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsStickersPage), (int)StickersType.Archived, new DrillInNavigationTransitionInfo());
        }

        private void ArchivedMasks_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsStickersPage), (int)StickersType.MasksArchived, new DrillInNavigationTransitionInfo());
        }

        private void Masks_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsStickersPage), (int)StickersType.Masks, new DrillInNavigationTransitionInfo());
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.StickerSetOpenCommand.Execute(e.ClickedItem);
        }

        private void ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (args.DropResult == Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move)
            {
                ViewModel.ReorderCommand.Execute(args.Items.FirstOrDefault());
            }
        }

        #region Recycle

        private void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            var stickerSet = args.Item as StickerSetInfo;
            var cover = stickerSet.GetThumbnail(out _, out bool animated);

            var typeName = animated ? "AnimatedItemTemplate" : "ItemTemplate";
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
                    var item = new ListViewItem();
                    item.ContentTemplate = _typeToTemplateMapping[typeName];
                    item.Style = sender.ItemContainerStyle;
                    item.Tag = typeName;
                    item.ContextRequested += StickerSet_ContextRequested;
                    args.ItemContainer = item;
                }
            }

            // Indicate to XAML that we picked a container for it
            args.IsContainerPrepared = true;
        }

        private async void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            var content = args.ItemContainer.ContentTemplateRoot as Grid;
            var stickerSet = args.Item as StickerSetInfo;

            var title = content.Children[1] as TextBlock;
            title.Text = stickerSet.Title;

            var subtitle = content.Children[2] as TextBlock;
            subtitle.Text = Locale.Declension("Stickers", stickerSet.Size);

            var file = stickerSet.GetThumbnail(out var outline, out _);
            if (file == null)
            {
                return;
            }

            if (file.Local.IsDownloadingCompleted)
            {
                if (content.Children[0] is Image photo)
                {
                    photo.Source = await PlaceholderHelper.GetWebPFrameAsync(file.Local.Path, 48);
                    ElementCompositionPreview.SetElementChildVisual(content.Children[0], null);
                }
                else if (args.Phase == 0 && content.Children[0] is LottieView lottie)
                {
                    lottie.Source = UriEx.ToLocal(file.Local.Path);
                }
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                if (content.Children[0] is Image photo)
                {
                    photo.Source = null;
                }
                else if (args.Phase == 0 && content.Children[0] is LottieView lottie)
                {
                    lottie.Source = null;
                }

                CompositionPathParser.ParseThumbnail(outline, out ShapeVisual visual, false);
                ElementCompositionPreview.SetElementChildVisual(content.Children[0], visual);

                _filesMap[file.Id].Add(stickerSet);
                ViewModel.ProtoService.DownloadFile(file.Id, 1);
            }

            args.Handled = true;
        }

        #endregion

        #region Binding

        private bool IsInstalled(StickersType type)
        {
            return type == StickersType.Installed;
        }

        private bool IsMasks(StickersType type)
        {
            return type == StickersType.Masks;
        }

        private string ConvertSuggest(StickersSuggestionMode mode)
        {
            switch (mode)
            {
                case StickersSuggestionMode.All:
                    return Strings.Resources.SuggestStickersAll;
                case StickersSuggestionMode.Installed:
                    return Strings.Resources.SuggestStickersInstalled;
                case StickersSuggestionMode.None:
                    return Strings.Resources.SuggestStickersNone;
            }

            return null;
        }

        #endregion

        #region Handle

        public void Handle(UpdateFile update)
        {
            if (!update.File.Local.IsDownloadingCompleted)
            {
                return;
            }

            if (_filesMap.TryGetValue(update.File.Id, out List<StickerSetInfo> stickers))
            {
                this.BeginOnUIThread(async () =>
                {
                    foreach (var stickerSet in stickers.ToImmutableHashSet())
                    {
                        stickerSet.UpdateFile(update.File);

                        var container = List.ContainerFromItem(stickerSet) as SelectorItem;
                        if (container == null)
                        {
                            continue;
                        }

                        var content = container.ContentTemplateRoot as Grid;
                        if (content == null)
                        {
                            continue;
                        }

                        if (content.Children[0] is Image photo)
                        {
                            photo.Source = await PlaceholderHelper.GetWebPFrameAsync(update.File.Local.Path, 48);
                            ElementCompositionPreview.SetElementChildVisual(content.Children[0], null);
                        }
                        else if (content.Children[0] is LottieView lottie)
                        {
                            lottie.Source = UriEx.ToLocal(update.File.Local.Path);
                            _handler.ThrottleVisibleItems();
                        }
                    }
                });
            }
        }

        #endregion

        #region Context menu

        private void StickerSet_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            if (ViewModel.Type != StickersType.Installed && ViewModel.Type != StickersType.Masks)
            {
                return;
            }

            var flyout = new MenuFlyout();

            var element = sender as FrameworkElement;
            var stickerSet = List.ItemFromContainer(element) as StickerSetInfo;

            if (stickerSet == null || stickerSet.Id == 0)
            {
                return;
            }

            if (stickerSet.IsOfficial)
            {
                flyout.CreateFlyoutItem(ViewModel.StickerSetHideCommand, stickerSet, Strings.Resources.StickersHide, new FontIcon { Glyph = Icons.Archive });
            }
            else
            {
                flyout.CreateFlyoutItem(ViewModel.StickerSetHideCommand, stickerSet, Strings.Resources.StickersHide, new FontIcon { Glyph = Icons.Archive });
                flyout.CreateFlyoutItem(ViewModel.StickerSetRemoveCommand, stickerSet, Strings.Resources.StickersRemove, new FontIcon { Glyph = Icons.Delete });
                //CreateFlyoutItem(ref flyout, ViewModel.StickerSetShareCommand, stickerSet, Strings.Resources.StickersShare);
                //CreateFlyoutItem(ref flyout, ViewModel.StickerSetCopyCommand, stickerSet, Strings.Resources.StickersCopy);
            }

            args.ShowAt(flyout, element);
        }

        #endregion

    }
}
