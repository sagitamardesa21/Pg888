//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using Telegram.Common;
using Telegram.Services.Settings;
using Telegram.Td.Api;
using Telegram.ViewModels.Drawers;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Point = Windows.Foundation.Point;

namespace Telegram.Controls.Drawers
{
    public class ItemContextRequestedEventArgs<T> : EventArgs
    {
        private readonly ContextRequestedEventArgs _args;

        public ItemContextRequestedEventArgs(T item, ContextRequestedEventArgs args)
        {
            _args = args;
            Item = item;
        }

        public bool TryGetPosition(UIElement relativeTo, out Point point)
        {
            return _args.TryGetPosition(relativeTo, out point);
        }

        public T Item { get; }

        public bool Handled
        {
            get => _args.Handled;
            set => _args.Handled = value;
        }
    }

    public sealed partial class AnimationDrawer : UserControl, IDrawer
    {
        public AnimationDrawerViewModel ViewModel => DataContext as AnimationDrawerViewModel;

        public Action<Animation> ItemClick { get; set; }
        public event TypedEventHandler<UIElement, ItemContextRequestedEventArgs<Animation>> ItemContextRequested;

        private readonly AnimatedListHandler _handler;
        private readonly ZoomableListHandler _zoomer;

        private bool _isActive;

        public AnimationDrawer()
        {
            InitializeComponent();

            _handler = new AnimatedListHandler(List, AnimatedListType.Animations);

            _zoomer = new ZoomableListHandler(List);
            _zoomer.Opening = _handler.UnloadVisibleItems;
            _zoomer.Closing = _handler.ThrottleVisibleItems;
            _zoomer.DownloadFile = fileId => ViewModel.ClientService.DownloadFile(fileId, 32);
            _zoomer.SessionId = () => ViewModel.ClientService.SessionId;

            ElementCompositionPreview.GetElementVisual(this).Clip = Window.Current.Compositor.CreateInsetClip();

            var header = DropShadowEx.Attach(Separator);
            header.Clip = header.Compositor.CreateInsetClip(0, 40, 0, -40);

            var debouncer = new EventDebouncer<TextChangedEventArgs>(Constants.TypingTimeout, handler => SearchField.TextChanged += new TextChangedEventHandler(handler));
            debouncer.Invoked += (s, args) =>
            {
                ViewModel.Search(SearchField.Text);
            };
        }

        public StickersTab Tab => StickersTab.Animations;

        public Thickness ScrollingHostPadding
        {
            get => List.Padding;
            set => List.Padding = new Thickness(2, value.Top, 0, value.Bottom);
        }

        public ListViewBase ScrollingHost => List;

        public void Activate(Chat chat, EmojiSearchType type = EmojiSearchType.Default)
        {
            _isActive = true;
            _handler.ThrottleVisibleItems();

            SearchField.SetType(ViewModel.ClientService, type);

            if (chat != null)
            {
                ViewModel.Update();
            }
        }

        public void Deactivate()
        {
            _isActive = false;
            _handler.UnloadItems();

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
            }
        }

        public void UnloadVisibleItems()
        {
            _handler.UnloadVisibleItems();
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Animation animation)
            {
                ItemClick?.Invoke(animation);
            }
        }

        private void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            if (args.ItemContainer == null)
            {
                args.ItemContainer = new GridViewItem();
                args.ItemContainer.Style = sender.ItemContainerStyle;
                args.ItemContainer.ContentTemplate = sender.ItemTemplate;
                args.ItemContainer.ContextRequested += OnContextRequested;

                _zoomer.ElementPrepared(args.ItemContainer);
            }

            args.IsContainerPrepared = true;
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var content = args.ItemContainer.ContentTemplateRoot as Border;
            var animation = args.Item as Animation;

            if (args.InRecycleQueue)
            {
                if (content.Child is AnimationView recycle)
                {
                    recycle.Source = null;
                }

                return;
            }

            var view = content.Child as AnimationView;

            var file = animation.AnimationValue;
            if (file == null)
            {
                return;
            }

            if (args.Phase == 2 && file.Local.IsDownloadingCompleted)
            {
                view.Source = new LocalVideoSource(file);
                view.Thumbnail = null;
            }
            else if (args.Phase == 0)
            {
                view.Source = null;

                UpdateManager.Subscribe(view, ViewModel.ClientService, file, UpdateFile, true);

                if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
                {
                    ViewModel.ClientService.DownloadFile(file.Id, 1);
                }

                var thumbnail = animation.Thumbnail?.File;
                if (thumbnail != null)
                {
                    if (thumbnail.Local.IsDownloadingCompleted)
                    {
                        view.Thumbnail = new BitmapImage(UriEx.ToLocal(thumbnail.Local.Path));
                    }
                    else
                    {
                        view.Thumbnail = null;

                        UpdateManager.Subscribe(content, ViewModel.ClientService, thumbnail, UpdateThumbnail, true);

                        if (thumbnail.Local.CanBeDownloaded && !thumbnail.Local.IsDownloadingActive)
                        {
                            ViewModel.ClientService.DownloadFile(thumbnail.Id, 1);
                        }
                    }
                }
            }

            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(2, OnContainerContentChanging);
            }
        }

        private void OnContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var animation = List.ItemFromContainer(sender) as Animation;
            if (animation == null)
            {
                return;
            }

            ItemContextRequested?.Invoke(sender, new ItemContextRequestedEventArgs<Animation>(animation, args));
        }

        private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            //ViewModel.Search(SearchField.Text, false);
        }

        private void SearchField_CategorySelected(object sender, EmojiCategorySelectedEventArgs e)
        {
            ViewModel.Search(string.Join(" ", e.Category.Emojis));
        }

        private object ConvertItems(object items)
        {
            _handler.ThrottleVisibleItems();
            return items;
        }

        private void UpdateFile(object target, File file)
        {
            if (target is AnimationView view)
            {
                view.Source = new LocalVideoSource(file);
                _handler.ThrottleVisibleItems();
            }
        }

        private void UpdateThumbnail(object target, File file)
        {
            if (target is Border content && content.Child is AnimationView view)
            {
                view.Thumbnail = new BitmapImage(UriEx.ToLocal(file.Local.Path));
            }
        }
    }
}
