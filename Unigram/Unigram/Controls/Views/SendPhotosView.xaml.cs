﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unigram.Common;
using Unigram.Core.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
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
    public sealed partial class SendPhotosView : ContentDialogBase, INotifyPropertyChanged
    {
        public ObservableCollection<StorageMedia> Items { get; set; }

        private StorageMedia _selectedItem;
        public StorageMedia SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedItem"));
                }
            }
        }

        private bool _isTtlEnabled;
        public bool IsTTLEnabled
        {
            get
            {
                return _isTtlEnabled;
            }
            set
            {
                if (_isTtlEnabled != value)
                {
                    _isTtlEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTTLEnabled"));
                }
            }
        }

        public SendPhotosView()
        {
            InitializeComponent();
            DataContext = this;

            //var seconds = new int[29];
            //for (int i = 0; i < seconds.Length; i++)
            //{
            //    seconds[i] = i;
            //}

            //TTLSeconds.ItemsSource = seconds;

            TTLSeconds.RegisterPropertyChangedCallback(GlyphButton.GlyphProperty, OnSecondsChanged);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing += InputPane_Showing;
            InputPane.GetForCurrentView().Hiding += InputPane_Hiding;

            if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse)
            {
                CaptionInput.Focus(FocusState.Keyboard);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing -= InputPane_Showing;
            InputPane.GetForCurrentView().Hiding -= InputPane_Hiding;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            KeyboardPlaceholder.Height = new GridLength(args.OccludedRect.Height);
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            KeyboardPlaceholder.Height = new GridLength(1, GridUnitType.Auto);
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Hide(ContentDialogBaseResult.OK);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Hide(ContentDialogBaseResult.Cancel);
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse)
                {
                    Accept_Click(null, null);
                }

                Flip.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private async void More_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.AddRange(Constants.MediaTypes);

            var files = await picker.PickMultipleFilesAsync();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (Path.GetExtension(file.Name).Equals(".mp4"))
                    {
                        //Items.Add(new StorageVideo(file));
                    }
                    else
                    {
                        Items.Add(new StoragePhoto(file));
                    }
                }
            }

            Remove.IsEnabled = Items.Count > 1;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null && Items.Count > 1)
            {
                var index = Items.IndexOf(SelectedItem);
                var next = index > 0 ? Items[index - 1] : null;
                var previous = index < Items.Count - 1 ? Items[index + 1] : null;

                var item = Items[index];

                if (next != null)
                {
                    SelectedItem = next;
                }
                else
                {
                    SelectedItem = previous;
                }

                Items.Remove(item);
            }

            Remove.IsEnabled = Items.Count > 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TTLSeconds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //VisualStateManager.GoToState(TTLSeconds, TTLSeconds.SelectedIndex == 0 ? "Unselected" : "Selected", false);
        }

        private void OnSecondsChanged(DependencyObject sender, DependencyProperty dp)
        {
            VisualStateManager.GoToState(TTLSeconds, SelectedItem.TTLSeconds == null ? "Unselected" : "Selected", false);
            //VisualStateManager.GoToState(this, SelectedItem.TTLSeconds == null ? "Unselected" : "Selected", false);

            // TODO: WRONG!!!
            if (SelectedItem.TTLSeconds == null)
            {
                TTLSeconds.ClearValue(Button.ForegroundProperty);
            }
            else
            {
                TTLSeconds.Foreground = LayoutRoot.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush;
            }
        }

        private async void TTLSeconds_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SelectTTLSecondsView();
            dialog.TTLSeconds = SelectedItem.TTLSeconds;

            var confirm = await dialog.ShowQueuedAsync();
            if (confirm == ContentDialogResult.Primary)
            {
                SelectedItem.TTLSeconds = dialog.TTLSeconds;
            }
        }
    }
}
