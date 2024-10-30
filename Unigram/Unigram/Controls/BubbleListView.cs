﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using Unigram.Converters;
using Unigram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Unigram.Controls
{
   public class BubbleListView : ListView
    {
        public DialogViewModel ViewModel => DataContext as DialogViewModel;

        public ScrollViewer ScrollingHost { get; private set; }
        public ItemsStackPanel ItemsStack { get; private set; }

        public BubbleListView()
        {
            DefaultStyleKey = typeof(ListView);

            Loaded += OnLoaded;
        }

        public void ScrollToBottom()
        {
            if (ScrollingHost != null)
            {
                ScrollingHost.ChangeView(null, ScrollingHost.ScrollableHeight, null);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var panel = ItemsPanelRoot as ItemsStackPanel;
            if (panel != null)
            {
                ItemsStack = panel;
                ItemsStack.ItemsUpdatingScrollMode = UpdatingScrollMode == UpdatingScrollMode.KeepItemsInView || UpdatingScrollMode == UpdatingScrollMode.ForceKeepItemsInView
                    ? ItemsUpdatingScrollMode.KeepItemsInView
                    : ItemsUpdatingScrollMode.KeepLastItemInView;
                ItemsStack.SizeChanged += Panel_SizeChanged;
            }
        }

        protected override void OnApplyTemplate()
        {
            ScrollingHost = (ScrollViewer)GetTemplateChild("ScrollViewer");
            ScrollingHost.ViewChanged += ScrollingHost_ViewChanged;

            base.OnApplyTemplate();
        }

        private async void Panel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ScrollingHost.ScrollableHeight < 120)
            {
                if (!ViewModel.IsFirstSliceLoaded)
                {
                    await ViewModel.LoadPreviousSliceAsync();
                }

                await ViewModel.LoadNextSliceAsync();
            }
        }

        private async void ScrollingHost_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (ScrollingHost.VerticalOffset < 120 && !e.IsIntermediate)
            {
                await ViewModel.LoadNextSliceAsync(true);
            }
            else if (ScrollingHost.ScrollableHeight - ScrollingHost.VerticalOffset < 120 && !e.IsIntermediate)
            {
                if (ViewModel.IsFirstSliceLoaded == false)
                {
                    await ViewModel.LoadPreviousSliceAsync(true);
                }
            }
        }

        #region UpdatingScrollMode

        public UpdatingScrollMode UpdatingScrollMode
        {
            get { return (UpdatingScrollMode)GetValue(UpdatingScrollModeProperty); }
            set { SetValue(UpdatingScrollModeProperty, value); }
        }

        public static readonly DependencyProperty UpdatingScrollModeProperty =
            DependencyProperty.Register("UpdatingScrollMode", typeof(UpdatingScrollMode), typeof(BubbleListView), new PropertyMetadata(UpdatingScrollMode.KeepItemsInView, OnUpdatingScrollModeChanged));

        private static void OnUpdatingScrollModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as BubbleListView;
            if (sender.ItemsStack != null)
            {
                var mode = (UpdatingScrollMode)e.NewValue;
                if (mode == UpdatingScrollMode.ForceKeepItemsInView)
                {
                    sender.ItemsStack.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;
                }
                else if (mode == UpdatingScrollMode.ForceKeepLastItemInView)
                {
                    sender.ItemsStack.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
                }
                else if (mode == UpdatingScrollMode.KeepItemsInView && sender.ScrollingHost.VerticalOffset < 120)
                {
                    sender.ItemsStack.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;
                }
                else if (mode == UpdatingScrollMode.KeepLastItemInView && sender.ScrollingHost.ScrollableHeight - sender.ScrollingHost.VerticalOffset < 120)
                {
                    sender.ItemsStack.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
                }
            }
        }

        #endregion

        private int count;
        protected override DependencyObject GetContainerForItemOverride()
        {
            //Debug.WriteLine($"New listview item: {++count}");
            return new BubbleListViewItem(this);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var bubble = element as BubbleListViewItem;
            var messageCommon = item as TLMessageCommonBase;

            if (bubble != null && messageCommon != null)
            {
                if (messageCommon.IsService())
                {
                    bubble.Padding = new Thickness(12, 0, 12, 0);
                }
                else
                {
                    var message = item as TLMessage;
                    if (message != null && message.ToId is TLPeerChat || message.ToId is TLPeerChannel && !message.IsPost)
                    {
                        if (message.IsOut)
                        {
                            if (message.IsSticker())
                            {
                                bubble.Padding = new Thickness(12, 0, 12, 0);
                            }
                            else
                            {
                                bubble.Padding = new Thickness(52, 0, 12, 0);
                            }
                        }
                        else
                        {
                            if (message.IsSticker())
                            {
                                bubble.Padding = new Thickness(52, 0, 12, 0);
                            }
                            else
                            {
                                bubble.Padding = new Thickness(52, 0, MessageToShareConverter.Convert(message) ? 12 : 52, 0);
                            }
                        }
                    }
                    else
                    {
                        if (message.IsSticker())
                        {
                            bubble.Padding = new Thickness(12, 0, 12, 0);
                        }
                        else
                        {
                            if (message.IsOut && !message.IsPost)
                            {
                                bubble.Padding = new Thickness(52, 0, 12, 0);
                            }
                            else
                            {
                                bubble.Padding = new Thickness(12, 0, MessageToShareConverter.Convert(message) ? 12 : 52, 0);
                            }
                        }
                    }
                }
            }

            base.PrepareContainerForItemOverride(element, item);
        }
    }

    public enum UpdatingScrollMode
    {
        KeepItemsInView,
        ForceKeepItemsInView,
        KeepLastItemInView,
        ForceKeepLastItemInView
    }
}
