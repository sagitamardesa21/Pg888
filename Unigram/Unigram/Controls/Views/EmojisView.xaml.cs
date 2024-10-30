﻿using LinqToVisualTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unigram.Common;
using Unigram.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Unigram.Controls.Views
{
    public sealed partial class EmojisView : UserControl
    {
        public event EventHandler Switch;
        public event ItemClickEventHandler ItemClick;

        public EmojisView()
        {
            this.InitializeComponent();

            var shadow = DropShadowEx.Attach(Separator, 20, 0.25f);

            Toolbar.SizeChanged += (s, args) =>
            {
                shadow.Size = args.NewSize.ToVector2();
            };

            EmojisViewSource.Source = Emoji.Items.ToArray();
            Toolbar.ItemsSource = Emoji.Items.ToArray();
            Toolbar.SelectedIndex = 0;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var scrollingHost = List.Descendants<ScrollViewer>().FirstOrDefault() as ScrollViewer;
            if (scrollingHost != null)
            {
                // Syncronizes GridView with the toolbar ListView
                scrollingHost.ViewChanged += ScrollingHost_ViewChanged;
                ScrollingHost_ViewChanged(null, null);
            }
        }

        public void SetView(bool widget)
        {
            VisualStateManager.GoToState(this, widget ? "FilledState" : "NarrowState", false);

            if (Toolbar.ItemsSource is EmojiGroup[] groups)
            {
                var microsoft = string.Equals(SettingsService.Current.Appearance.EmojiSetId, "microsoft");

                if (groups.Length == Emoji.Items.Count && microsoft)
                {
                    EmojisViewSource.Source = Emoji.Items.Take(Emoji.Items.Count - 1).ToArray();
                    Toolbar.ItemsSource = Emoji.Items.Take(Emoji.Items.Count - 1).ToArray();
                }
                else if (groups.Length == Emoji.Items.Count - 1 && !microsoft)
                {
                    EmojisViewSource.Source = Emoji.Items.ToArray();
                    Toolbar.ItemsSource = Emoji.Items.ToArray();
                }
            }
        }

        private void ScrollingHost_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollingHost = List.ItemsPanelRoot as ItemsWrapGrid;
            if (scrollingHost != null)
            {
                var first = List.ContainerFromIndex(scrollingHost.FirstVisibleIndex);
                if (first != null)
                {
                    var header = List.GroupHeaderContainerFromItemContainer(first) as GridViewHeaderItem;
                    if (header != null && header != Toolbar.SelectedItem)
                    {
                        Toolbar.SelectedItem = header.Content;
                        Toolbar.ScrollIntoView(header.Content);
                    }
                }
            }
        }

        private void Toolbar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Pivot.SelectedIndex = Toolbar.SelectedIndex;
        }

        private void Toolbar_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is EmojiGroup group)
            {
                List.ScrollIntoView(e.ClickedItem, ScrollIntoViewAlignment.Leading);
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            Switch?.Invoke(this, EventArgs.Empty);
        }
    }
}
