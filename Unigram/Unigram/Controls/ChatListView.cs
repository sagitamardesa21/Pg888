﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unigram.Controls.Cells;
using Unigram.Controls.Items;
using Unigram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls
{
    public class ChatListView : GroupedListView
    {
        public UnigramViewModelBase ViewModel => DataContext as UnigramViewModelBase;

        public ChatListView()
        {
            ContainerContentChanging += OnContainerContentChanging;
            RegisterPropertyChangedCallback(SelectionModeProperty, OnSelectionModeChanged);
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            var content = args.ItemContainer.ContentTemplateRoot as ChatCell;
            if (content != null)
            {
                content.UpdateVisualState(args.Item as TdWindows.Chat, args.ItemContainer.IsSelected && SelectionMode == ListViewSelectionMode.Single);
                content.UpdateChat(ViewModel.ProtoService, args.Item as TdWindows.Chat);
                args.Handled = true;
            }
        }

        private void OnSelectionModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            var panel = ItemsPanelRoot as ItemsStackPanel;
            if (panel == null)
            {
                return;
            }

            for (int i = panel.FirstCacheIndex; i <= panel.LastCacheIndex; i++)
            {
                var container = ContainerFromIndex(i) as ListViewItem;
                if (container == null)
                {
                    continue;
                }

                var content = container.ContentTemplateRoot as ChatCell;
                if (content != null)
                {
                    content.UpdateVisualState(ItemFromContainer(container) as TdWindows.Chat, container.IsSelected && SelectionMode == ListViewSelectionMode.Single);
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ChatListViewItem(this);
        }
    }

    public class ChatListViewItem : ListViewItem
    {
        private ChatListView _list;

        public ChatListViewItem()
        {

        }

        public ChatListViewItem(ChatListView list)
        {
            _list = list;
            RegisterPropertyChangedCallback(IsSelectedProperty, OnSelectedChanged);
        }

        private void OnSelectedChanged(DependencyObject sender, DependencyProperty dp)
        {
            var content = ContentTemplateRoot as ChatCell;
            if (content != null)
            {
                content.UpdateVisualState(_list.ItemFromContainer(this) as TdWindows.Chat, this.IsSelected && _list.SelectionMode == ListViewSelectionMode.Single);
            }
        }
    }
}
