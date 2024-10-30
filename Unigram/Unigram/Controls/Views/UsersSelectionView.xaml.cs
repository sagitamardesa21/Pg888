﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Api.TL;
using Unigram.ViewModels;
using Unigram.ViewModels.Settings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Unigram.Controls.Views
{
    public sealed partial class UsersSelectionView : Grid
    {
        public UsersSelectionViewModel ViewModel => DataContext as UsersSelectionViewModel;

        public UsersSelectionView()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                DataContext = new SettingsBlockUserViewModel(null, null, null);
            }

            InitializeComponent();

            var observable = Observable.FromEventPattern<TextChangedEventArgs>(SearchField, "TextChanged");
            var throttled = observable.Throttle(TimeSpan.FromMilliseconds(500)).ObserveOnDispatcher().Subscribe(x =>
            {
                if (string.IsNullOrWhiteSpace(SearchField.Text))
                {
                    ViewModel.Search.Clear();
                    return;
                }

                ViewModel.SearchAsync(SearchField.Text);
            });
        }

        public void Attach()
        {
            ViewModel.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel.SelectionMode == ListViewSelectionMode.None)
            {
                return;
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    List.SelectedItems.Add(item);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    List.SelectedItems.Remove(item);
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectionMode == ListViewSelectionMode.None)
            {
                return;
            }

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    ViewModel.SelectedItems.Add(item as TLUser);
                }
            }

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    ViewModel.SelectedItems.Remove(item as TLUser);
                }
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ViewModel.SelectionMode == ListViewSelectionMode.None)
            {
                ViewModel.SelectedItems.Clear();
                ViewModel.SelectedItems.Add(e.ClickedItem as TLUser);
                ViewModel.SendCommand.Execute();
            }
        }

        private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchList.Visibility = string.IsNullOrEmpty(SearchField.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TagsTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollingHost.ChangeView(null, ScrollingHost.ScrollableHeight, null);
        }

        #region Binding

        private Visibility ConvertMaximum(int maximum, bool infinite)
        {
            return (maximum == int.MaxValue && infinite) || maximum == 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        public object Header { get; set; }

        public DataTemplate HeaderTemplate { get; set; }
    }
}
