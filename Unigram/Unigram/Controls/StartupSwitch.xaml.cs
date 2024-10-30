﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unigram.Services;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Unigram.Controls
{
    public sealed partial class StartupSwitch : UserControl
    {
        public StartupSwitch()
        {
            InitializeComponent();

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.StartupTaskContract", 2))
            {
#if DESKTOP_BRIDGE
                FindName(nameof(ToggleMinimized));
#endif

                OnLoaded();
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }
        }

        private async void OnLoaded()
        {
            Toggle.Toggled -= OnToggled;

            if (ToggleMinimized != null)
            {
                ToggleMinimized.Toggled -= Minimized_Toggled;
            }

            var task = await StartupTask.GetAsync("Telegram");
            if (task.State == StartupTaskState.Enabled)
            {
                Toggle.IsOn = true;
                Toggle.IsEnabled = true;

                if (ToggleMinimized != null)
                {
                    ToggleMinimized.IsOn = SettingsService.Current.IsLaunchMinimized;
                    ToggleMinimized.Visibility = Visibility.Visible;
                }

                Headered.Footer = string.Empty;

                Visibility = Visibility.Visible;
            }
            else if (task.State == StartupTaskState.Disabled)
            {
                Toggle.IsOn = false;
                Toggle.IsEnabled = true;

                if (ToggleMinimized != null)
                {
                    ToggleMinimized.IsOn = false;
                    ToggleMinimized.Visibility = Visibility.Collapsed;
                }

                Headered.Footer = string.Empty;

                Visibility = Visibility.Visible;
            }
            else if (task.State == StartupTaskState.DisabledByUser)
            {
                Toggle.IsOn = false;
                Toggle.IsEnabled = false;

                if (ToggleMinimized != null)
                {
                    ToggleMinimized.IsOn = false;
                    ToggleMinimized.Visibility = Visibility.Collapsed;
                }

                Headered.Footer = "You can enable this in the Startup tab in Task Manager.";

                Visibility = Visibility.Visible;
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }

            Toggle.Toggled += OnToggled;

            if (ToggleMinimized != null)
            {
                ToggleMinimized.Toggled += Minimized_Toggled;
            }
        }

        private async void OnToggled(object sender, RoutedEventArgs e)
        {
            var task = await StartupTask.GetAsync("Telegram");
            if (Toggle.IsOn)
            {
                await task.RequestEnableAsync();
            }
            else
            {
                task.Disable();
            }

            OnLoaded();
        }

        private void Minimized_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsService.Current.IsLaunchMinimized = ToggleMinimized.IsOn;
        }
    }
}
