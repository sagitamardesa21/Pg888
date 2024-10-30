﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Controls.Cells;
using Unigram.Controls.Items;
using Unigram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls
{
    public class ChatListView : GroupedListView
    {
        public TLViewModelBase ViewModel => DataContext as TLViewModelBase;

        public MasterDetailState _viewState;
        public ChatFilterMode _filterMode;

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
                content.UpdateViewState(args.Item as Chat, _filterMode, args.ItemContainer.IsSelected && SelectionMode == ListViewSelectionMode.Single, _viewState == MasterDetailState.Compact);
                content.UpdateChat(ViewModel.ProtoService, ViewModel.Aggregator, args.Item as Chat);
                args.Handled = true;
            }
        }

        private void OnSelectionModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateVisibleChats();
        }

        public void UpdateViewState(MasterDetailState state)
        {
            _viewState = state;
            UpdateVisibleChats();
        }

        public void UpdateFilterMode(ChatFilterMode filter)
        {
            _filterMode = filter;
            UpdateVisibleChats();
        }

        private void UpdateVisibleChats()
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
                    content.UpdateViewState(ItemFromContainer(container) as Chat, _filterMode, container.IsSelected && SelectionMode == ListViewSelectionMode.Single, _viewState == MasterDetailState.Compact);
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
                content.UpdateViewState(_list.ItemFromContainer(this) as Chat, _list._filterMode, this.IsSelected && _list.SelectionMode == ListViewSelectionMode.Single, _list._viewState == MasterDetailState.Compact);
            }
        }
    }
}
