//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using System.Collections.Generic;
using Telegram.Controls.Drawers;
using Telegram.Navigation;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Telegram.Common
{
    public static class MenuFlyoutHelper
    {
        public static void ShowAt(this ContextRequestedEventArgs args, MenuFlyout flyout, FrameworkElement element)
        {
            if (flyout.Items.Count > 0 && args.TryGetPosition(element, out Point point))
            {
                if (point.X < 0 || point.Y < 0)
                {
                    point = new Point(Math.Max(point.X, 0), Math.Max(point.Y, 0));
                }

                try
                {
                    flyout.ShowAt(element, point);
                }
                catch { }
            }
            else if (flyout.Items.Count > 0)
            {
                try
                {
                    flyout.ShowAt(element);
                }
                catch { }
            }

            args.Handled = true;
        }

        public static void ShowAt<T>(this ItemContextRequestedEventArgs<T> args, MenuFlyout flyout, FrameworkElement element)
        {
            if (flyout.Items.Count > 0 && args.TryGetPosition(element, out Point point))
            {
                if (point.X < 0 || point.Y < 0)
                {
                    point = new Point(Math.Max(point.X, 0), Math.Max(point.Y, 0));
                }

                try
                {
                    flyout.ShowAt(element, point);
                }
                catch { }
            }
            else if (flyout.Items.Count > 0)
            {
                try
                {
                    flyout.ShowAt(element);
                }
                catch { }
            }

            args.Handled = true;
        }

        public static MenuFlyoutSeparator CreateFlyoutSeparator(this MenuFlyout flyout)
        {
            if (flyout.Items.Count > 0 && (flyout.Items[flyout.Items.Count - 1] is MenuFlyoutItem or MenuFlyoutSubItem))
            {
                var separator = new MenuFlyoutSeparator();
                flyout.Items.Add(separator);
                return separator;
            }

            return null;
        }

        public static MenuFlyoutSeparator CreateFlyoutSeparator(this MenuFlyoutSubItem flyout)
        {
            if (flyout.Items.Count > 0 && (flyout.Items[flyout.Items.Count - 1] is MenuFlyoutItem or MenuFlyoutSubItem))
            {
                var separator = new MenuFlyoutSeparator();
                flyout.Items.Add(separator);
                return separator;
            }

            return null;
        }

        public static void CreateFlyoutItem<T>(this MenuFlyout flyout, Func<T, bool> visibility, Action<T> command, T parameter, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control) where T : class
        {
            flyout.Items.CreateFlyoutItem(visibility, command, parameter, text, icon, key, modifiers);
        }

        public static void CreateFlyoutItem<T>(this MenuFlyoutSubItem flyout, Func<T, bool> visibility, Action<T> command, T parameter, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control) where T : class
        {
            flyout.Items.CreateFlyoutItem(visibility, command, parameter, text, icon, key, modifiers);
        }

        public static void CreateFlyoutItem<T>(this IList<MenuFlyoutItemBase> items, Func<T, bool> visibility, Action<T> command, T parameter, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control) where T : class
        {
            var value = visibility(parameter);
            if (value)
            {
                var flyoutItem = new MenuFlyoutItem();
                flyoutItem.CommandParameter = parameter;
                flyoutItem.Command = new RelayCommand<T>(command);
                flyoutItem.Text = text;

                if (icon != null)
                {
                    if (icon is FontIcon fontIcon)
                    {
                        //fontIcon.Margin = new Thickness(-2);
                        fontIcon.FontSize = 20;
                        fontIcon.FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily;
                    }

                    flyoutItem.Icon = icon;
                }

                if (key.HasValue)
                {
                    flyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = modifiers, Key = key.Value, IsEnabled = false });
                }

                items.Add(flyoutItem);
            }
        }

        public static void CreateFlyoutItem(this MenuFlyout flyout, Func<bool> visibility, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            flyout.Items.CreateFlyoutItem(visibility, command, text, icon, key, modifiers);
        }

        public static void CreateFlyoutItem(this MenuFlyoutSubItem flyout, Func<bool> visibility, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            flyout.Items.CreateFlyoutItem(visibility, command, text, icon, key, modifiers);
        }

        public static void CreateFlyoutItem(this IList<MenuFlyoutItemBase> items, Func<bool> visibility, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            var value = visibility();
            if (value)
            {
                var flyoutItem = new MenuFlyoutItem();
                flyoutItem.Command = new RelayCommand(command);
                flyoutItem.Text = text;

                if (icon != null)
                {
                    if (icon is FontIcon fontIcon)
                    {
                        //fontIcon.Margin = new Thickness(-2);
                        fontIcon.FontSize = 20;
                        fontIcon.FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily;
                    }

                    flyoutItem.Icon = icon;
                }

                if (key.HasValue)
                {
                    flyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = modifiers, Key = key.Value, IsEnabled = false });
                }

                items.Add(flyoutItem);
            }
        }

        // Probably used only for members context menu
        public static void CreateFlyoutItem<T1, T2, T3>(this MenuFlyout flyout, Func<T1, T2, T3, bool> visibility, Action<T3> command, T1 chatType, T2 status, T3 parameter, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control) where T3 : class
        {
            var value = visibility(chatType, status, parameter);
            if (value)
            {
                var flyoutItem = new MenuFlyoutItem();
                flyoutItem.CommandParameter = parameter;
                flyoutItem.Command = new RelayCommand<T3>(command);
                flyoutItem.Text = text;

                if (icon != null)
                {
                    if (icon is FontIcon fontIcon)
                    {
                        //fontIcon.Margin = new Thickness(-2);
                        fontIcon.FontSize = 20;
                        fontIcon.FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily;
                    }

                    flyoutItem.Icon = icon;
                }

                if (key.HasValue)
                {
                    flyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = modifiers, Key = key.Value, IsEnabled = false });
                }

                flyout.Items.Add(flyoutItem);
            }
        }

        public static MenuFlyoutItem CreateFlyoutItem(this MenuFlyout flyout, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            return CreateFlyoutItem(flyout.Items, command, text, icon, key, modifiers);
        }

        public static MenuFlyoutItem CreateFlyoutItem(this MenuFlyoutSubItem flyout, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            return CreateFlyoutItem(flyout.Items, command, text, icon, key, modifiers);
        }

        public static MenuFlyoutItem CreateFlyoutItem(this IList<MenuFlyoutItemBase> items, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            var flyoutItem = new MenuFlyoutItem();
            flyoutItem.IsEnabled = command != null;
            flyoutItem.Command = new RelayCommand(command);
            flyoutItem.Text = text;

            if (icon != null)
            {
                if (icon is FontIcon fontIcon)
                {
                    //fontIcon.Margin = new Thickness(-2);
                    fontIcon.FontSize = 20;
                    fontIcon.FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily;
                }

                flyoutItem.Icon = icon;
            }

            if (key.HasValue)
            {
                flyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = modifiers, Key = key.Value, IsEnabled = false });
            }

            items.Add(flyoutItem);
            return flyoutItem;
        }

        public static MenuFlyoutItem CreateFlyoutItem(this MenuFlyout flyout, bool enabled, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            return flyout.Items.CreateFlyoutItem(enabled, command, text, icon, key, modifiers);
        }

        public static MenuFlyoutItem CreateFlyoutItem(this MenuFlyoutSubItem flyout, bool enabled, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            return flyout.Items.CreateFlyoutItem(enabled, command, text, icon, key, modifiers);
        }

        public static MenuFlyoutItem CreateFlyoutItem(this IList<MenuFlyoutItemBase> items, bool enabled, Action command, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            var flyoutItem = new MenuFlyoutItem();
            flyoutItem.IsEnabled = enabled;
            flyoutItem.Command = new RelayCommand(command);
            flyoutItem.Text = text;

            if (icon != null)
            {
                if (icon is FontIcon fontIcon)
                {
                    //fontIcon.Margin = new Thickness(-2);
                    fontIcon.FontSize = 20;
                    fontIcon.FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily;
                }

                flyoutItem.Icon = icon;
            }

            if (key.HasValue)
            {
                flyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = modifiers, Key = key.Value, IsEnabled = false });
            }

            items.Add(flyoutItem);
            return flyoutItem;
        }

        public static MenuFlyoutItem CreateFlyoutItem<T>(this MenuFlyout flyout, Action<T> command, T parameter, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            return flyout.Items.CreateFlyoutItem(true, command, parameter, text, icon, key, modifiers);
        }

        public static MenuFlyoutItem CreateFlyoutItem<T>(this MenuFlyoutSubItem flyout, Action<T> command, T parameter, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            return flyout.Items.CreateFlyoutItem(true, command, parameter, text, icon, key, modifiers);
        }

        public static MenuFlyoutItem CreateFlyoutItem<T>(this IList<MenuFlyoutItemBase> items, bool enabled, Action<T> command, object parameter, string text, IconElement icon = null, VirtualKey? key = null, VirtualKeyModifiers modifiers = VirtualKeyModifiers.Control)
        {
            var flyoutItem = new MenuFlyoutItem();
            flyoutItem.IsEnabled = enabled;
            flyoutItem.CommandParameter = parameter;
            flyoutItem.Command = new RelayCommand<T>(command);
            flyoutItem.Text = text;

            if (icon != null)
            {
                if (icon is FontIcon fontIcon)
                {
                    //fontIcon.Margin = new Thickness(-2);
                    fontIcon.FontSize = 20;
                    fontIcon.FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily;
                }

                flyoutItem.Icon = icon;
            }

            if (key.HasValue)
            {
                flyoutItem.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = modifiers, Key = key.Value, IsEnabled = false });
            }

            items.Add(flyoutItem);
            return flyoutItem;
        }

        public static FontIcon Icon(string glyph)
        {
            return new FontIcon
            {
                Glyph = glyph,
                FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily,
                Margin = new Thickness(-2)
            };
        }
    }
}
