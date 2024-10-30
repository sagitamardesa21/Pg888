﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls
{
    public class SelectGridView : GridView
    {
        public SelectGridView()
        {
            ContainerContentChanging += OnContainerContentChanging;
            RegisterPropertyChangedCallback(SelectionModeProperty, OnSelectionModeChanged);
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var content = args.ItemContainer.ContentTemplateRoot;
            if (content != null)
            {
                content.IsHitTestVisible = SelectionMode == ListViewSelectionMode.None;
            }
        }

        private void OnSelectionModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            var panel = ItemsPanelRoot as ItemsWrapGrid;
            if (panel == null)
            {
                return;
            }

            for (int i = panel.FirstCacheIndex; i <= panel.LastCacheIndex; i++)
            {
                var container = ContainerFromIndex(i) as GridViewItem;
                if (container == null)
                {
                    continue;
                }

                var content = container.ContentTemplateRoot;
                if (content != null)
                {
                    content.IsHitTestVisible = SelectionMode == ListViewSelectionMode.None;
                }
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as GridViewItem;
            if (container == null)
            {
                return;
            }

            var content = container.ContentTemplateRoot;
            if (content != null)
            {
                content.IsHitTestVisible = SelectionMode == ListViewSelectionMode.None;
            }
        }
    }
}
