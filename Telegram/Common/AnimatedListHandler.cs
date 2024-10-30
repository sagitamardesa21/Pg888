//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Controls;
using Telegram.Td.Api;
using Telegram.ViewModels.Drawers;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Telegram.Common
{
    public enum AnimatedListType
    {
        Stickers,
        Animations,
        Emoji,
        Other // Inline bots, chat list,
    }

    public class AnimatedListHandler
    {
        private readonly ListViewBase _listView;
        private readonly DispatcherTimer _debouncer;

        private readonly AnimatedListType _type;

        private readonly Dictionary<long, IPlayerView> _prev = new();

        private bool _unloaded;

        public AnimatedListHandler(ListViewBase listView, AnimatedListType type)
        {
            _listView = listView;
            _listView.SizeChanged += OnSizeChanged;
            _listView.Unloaded += OnUnloaded;

            _debouncer = new DispatcherTimer();
            _debouncer.Interval = TimeSpan.FromMilliseconds(Constants.AnimatedThrottle);
            _debouncer.Tick += (s, args) =>
            {
                _debouncer.Stop();
                LoadVisibleItems(false);
            };

            _type = type;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ListViewBase)
            {
                _listView.SizeChanged -= OnSizeChanged;
                _listView.Items.VectorChanged += OnVectorChanged;

                var scrollViewer = _listView.GetScrollViewer();
                if (scrollViewer != null)
                {
                    scrollViewer.ViewChanged += OnViewChanged;
                }

                var panel = _listView.ItemsPanelRoot;
                if (panel != null)
                {
                    panel.SizeChanged += OnSizeChanged;
                }
            }
            else if (e.PreviousSize.Width < _listView.ActualWidth || e.PreviousSize.Height < _listView.ActualHeight)
            {
                _debouncer.Stop();
                _debouncer.Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnloadItems();
        }

        private void OnVectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs e)
        {
            if (_unloaded)
            {
                return;
            }

            _debouncer.Stop();
            _debouncer.Start();
        }

        private void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            LoadVisibleItems(true);

            _debouncer.Stop();
            _debouncer.Start();
            return;

            if (e.IsIntermediate)
            {
                _debouncer.Start();
            }
            else
            {
                LoadVisibleItems(false);
            }

            //LoadVisibleItems(/*e.IsIntermediate*/ false);
        }

        public void ThrottleVisibleItems()
        {
            _debouncer.Stop();
            _debouncer.Start();
        }

        public bool IsDisabledByPolicy
        {
            get => _type switch
            {
                AnimatedListType.Stickers => !PowerSavingPolicy.AutoPlayStickers,
                AnimatedListType.Animations => !PowerSavingPolicy.AutoPlayAnimations,
                AnimatedListType.Emoji => !PowerSavingPolicy.AutoPlayEmoji,
                _ => false
            };
        }

        public void LoadVisibleItems(bool intermediate)
        {
            int lastVisibleIndex;
            int firstVisibleIndex;

            if (_listView.ItemsPanelRoot is ItemsStackPanel stack)
            {
                lastVisibleIndex = stack.LastVisibleIndex;
                firstVisibleIndex = stack.FirstVisibleIndex;
            }
            else if (_listView.ItemsPanelRoot is ItemsWrapGrid wrap)
            {
                lastVisibleIndex = wrap.LastVisibleIndex;
                firstVisibleIndex = wrap.FirstVisibleIndex;
            }
            else
            {
                return;
            }

            if (lastVisibleIndex < firstVisibleIndex || firstVisibleIndex < 0)
            {
                UnloadVisibleItems();
                return;
            }

            Dictionary<long, IPlayerView> next = null;

            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                var container = _listView.ContainerFromIndex(i) as SelectorItem;
                if (container == null)
                {
                    continue;
                }

                File file = null;

                var item = _listView.ItemFromContainer(container);
                if (item is StickerViewModel viewModel && viewModel.Format is StickerFormatTgs or StickerFormatWebm)
                {
                    file = viewModel.StickerValue;
                }
                else if (item is StickerSetViewModel setViewModel && setViewModel.StickerFormat is StickerFormatTgs or StickerFormatWebm)
                {
                    var cover = setViewModel.GetThumbnail();
                    if (cover != null)
                    {
                        file = cover.StickerValue;
                    }
                }
                else if (item is Sticker sticker && sticker.Format is StickerFormatTgs or StickerFormatWebm)
                {
                    file = sticker.StickerValue;
                }
                else if (item is StickerSetInfo set && set.StickerFormat is StickerFormatTgs or StickerFormatWebm)
                {
                    var cover = set.GetThumbnail();
                    if (cover != null)
                    {
                        file = cover.StickerValue;
                    }
                }
                else if (item is Animation animation)
                {
                    file = animation.AnimationValue;
                }
                else if (item is InlineQueryResultAnimation inlineQueryResultAnimation)
                {
                    file = inlineQueryResultAnimation.Animation.AnimationValue;
                }
                else if (item is InlineQueryResultSticker inlineQueryResultSticker && inlineQueryResultSticker.Sticker.Format is StickerFormatTgs or StickerFormatWebm)
                {
                    file = inlineQueryResultSticker.Sticker.StickerValue;
                }

                if (item is not Chat && (file == null || !file.Local.IsDownloadingCompleted))
                {
                    continue;
                }

                var panel = container.ContentTemplateRoot;
                if (panel is FrameworkElement final)
                {
                    var lottie = final as IPlayerView ?? final.FindName("Player") as IPlayerView;
                    if (lottie != null)
                    {
                        next ??= new();
                        next[item.GetHashCode()] = lottie;
                    }
                }
            }

            if (next != null)
            {
                foreach (var item in _prev.Keys.Except(next.Keys).ToList())
                {
                    _prev[item]?.Pause();
                    _prev.Remove(item);
                }

                foreach (var item in next)
                {
                    if (IsDisabledByPolicy)
                    {
                        // Nothing
                    }
                    else
                    {
                        item.Value?.Play();
                    }

                    _prev[item.Key] = item.Value;
                }
            }
            else
            {
                foreach (var item in _prev)
                {
                    item.Value?.Pause();
                }

                _prev.Clear();
            }

            _unloaded = false;
        }

        public void UnloadVisibleItems()
        {
            foreach (var item in _prev.Values)
            {
                item.Pause();
            }

            _prev.Clear();
            _unloaded = true;
        }

        public void UnloadItems()
        {
            var panel = _listView.ItemsPanelRoot;
            if (panel == null)
            {
                return;
            }

            foreach (var item in panel.Children)
            {
                if (item is SelectorItem container && container.ContentTemplateRoot is FrameworkElement final)
                {
                    var lottie = final.FindName("Player") as IPlayerView;
                    lottie?.Unload();
                }
            }

            _prev.Clear();
            _unloaded = true;
        }
    }
}
