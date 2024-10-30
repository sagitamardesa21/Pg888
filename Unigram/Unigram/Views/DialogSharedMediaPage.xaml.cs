﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unigram.Views;
using Unigram.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LinqToVisualTree;
using System.Threading.Tasks;
using Telegram.Api.TL;
using Unigram.Controls;
using Template10.Common;
using System.ComponentModel;
using Unigram.Common;
using Windows.UI.Core;
using Windows.System;

namespace Unigram.Views
{
    public sealed partial class DialogSharedMediaPage : Page, IMasterDetailPage
    {
        public DialogSharedMediaViewModel ViewModel => DataContext as DialogSharedMediaViewModel;

        public DialogSharedMediaPage()
        {
            InitializeComponent();
            DataContext = UnigramContainer.Current.ResolveType<DialogSharedMediaViewModel>();

            ViewModel.PropertyChanged += OnPropertyChanged;

            //ScrollingMedia.RegisterPropertyChangedCallback(ListViewBase.SelectionModeProperty, List_SelectionModeChanged);
            //ScrollingFiles.RegisterPropertyChangedCallback(ListViewBase.SelectionModeProperty, List_SelectionModeChanged);
            //ScrollingLinks.RegisterPropertyChangedCallback(ListViewBase.SelectionModeProperty, List_SelectionModeChanged);
            //ScrollingMusic.RegisterPropertyChangedCallback(ListViewBase.SelectionModeProperty, List_SelectionModeChanged);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            App.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            App.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Escape && !args.KeyStatus.IsKeyReleased && ViewModel.SelectionMode != ListViewSelectionMode.None)
            {
                ViewModel.SelectionMode = ListViewSelectionMode.None;
                args.Handled = true;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedItems"))
            {
                switch (ScrollingHost.SelectedIndex)
                {
                    case 0:
                        ScrollingMedia.SelectedItems.AddRange(ViewModel.SelectedMessages);
                        break;
                    case 1:
                        ScrollingFiles.SelectedItems.AddRange(ViewModel.SelectedMessages);
                        break;
                    case 2:
                        ScrollingLinks.SelectedItems.AddRange(ViewModel.SelectedMessages);
                        break;
                    case 3:
                        ScrollingMusic.SelectedItems.AddRange(ViewModel.SelectedMessages);
                        break;
                }
            }
        }

        public void OnBackRequested(HandledEventArgs args)
        {
            if (ViewModel.SelectionMode != ListViewSelectionMode.None)
            {
                ViewModel.SelectionMode = ListViewSelectionMode.None;
                args.Handled = true;
            }
        }

        private void Photo_Click(object sender, RoutedEventArgs e)
        {
            Themes.Media.Photo_Click(sender);
        }

        private void List_SelectionModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            //ScrollingMedia.IsItemClickEnabled = ViewModel.SelectionMode == ListViewSelectionMode.None;
            //ScrollingFiles.IsItemClickEnabled = ViewModel.SelectionMode == ListViewSelectionMode.None;
            //ScrollingLinks.IsItemClickEnabled = ViewModel.SelectionMode == ListViewSelectionMode.None;
            //ScrollingMusic.IsItemClickEnabled = ViewModel.SelectionMode == ListViewSelectionMode.None;

            if (ViewModel.SelectionMode == ListViewSelectionMode.None)
            {
                ManagePanel.Visibility = Visibility.Collapsed;
                //InfoPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ManagePanel.Visibility = Visibility.Visible;
                //InfoPanel.Visibility = Visibility.Collapsed;
            }

            ViewModel.MessagesForwardCommand.RaiseCanExecuteChanged();
            ViewModel.MessagesDeleteCommand.RaiseCanExecuteChanged();
        }

        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectionMode == ListViewSelectionMode.None)
            {
                ViewModel.SelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                ViewModel.SelectionMode = ListViewSelectionMode.None;
            }
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedMessages = new List<TLMessageCommonBase>(((ListViewBase)sender).SelectedItems.Cast<TLMessageCommonBase>());
        }

        private bool ConvertSelectionMode(ListViewSelectionMode mode)
        {
            List_SelectionModeChanged(null, null);
            return mode == ListViewSelectionMode.None ? false : true;
        }

        #region Context menu

        private void MenuFlyout_Opening(object sender, object e)
        {
            var flyout = sender as MenuFlyout;

            foreach (var item in flyout.Items)
            {
                item.Visibility = Visibility.Visible;
            }
        }

        private void MessageGoto_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as MenuFlyoutItem;
            if (element != null)
            {
                var messageCommon = element.DataContext as TLMessageCommonBase;
                if (messageCommon != null)
                {

                }

                element.Visibility = Visibility.Visible;
            }
        }

        private void MessageDelete_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as MenuFlyoutItem;
            if (element != null)
            {
                element.Visibility = Visibility.Visible;

                var messageCommon = element.DataContext as TLMessageCommonBase;
                if (messageCommon != null)
                {
                    var channel = messageCommon.Parent as TLChannel;
                    if (channel != null)
                    {
                        if (messageCommon.Id == 1 && messageCommon.ToId is TLPeerChannel)
                        {
                            element.Visibility = Visibility.Collapsed;
                        }

                        if (!messageCommon.IsOut && !channel.IsCreator && !channel.HasAdminRights || (channel.AdminRights != null && !channel.AdminRights.IsDeleteMessages))
                        {
                            element.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void MessageForward_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as MenuFlyoutItem;
            if (element != null)
            {
                var messageCommon = element.DataContext as TLMessageCommonBase;
                if (messageCommon != null)
                {

                }

                element.Visibility = Visibility.Visible;
            }
        }

        private void MessageSelect_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as MenuFlyoutItem;
            if (element != null)
            {
                element.Visibility = ViewModel.SelectionMode == ListViewSelectionMode.None ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion
    }
}
