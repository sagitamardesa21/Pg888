﻿using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls.Drawers;
using Unigram.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Views
{
    public partial class ChatView
    {
        private void Sticker_ContextRequested(UIElement sender, ItemContextRequestedEventArgs<Sticker> args)
        {
            var element = sender as FrameworkElement;
            var sticker = args.Item;

            if (sticker == null)
            {
                return;
            }

            var flyout = new MenuFlyout();
            flyout.CreateFlyoutItem(ViewModel.StickerViewCommand, sticker, Strings.Resources.ViewPackPreview, new FontIcon { Glyph = Icons.Sticker });

            if (ViewModel.ProtoService.IsStickerFavorite(sticker.StickerValue.Id))
            {
                flyout.CreateFlyoutItem(ViewModel.StickerUnfaveCommand, sticker, Strings.Resources.DeleteFromFavorites, new FontIcon { Glyph = Icons.StarOff });
            }
            else
            {
                flyout.CreateFlyoutItem(ViewModel.StickerFaveCommand, sticker, Strings.Resources.AddToFavorites, new FontIcon { Glyph = Icons.Star });
            }

            if (ViewModel.Type == ViewModels.DialogType.History)
            {
                var chat = ViewModel.Chat;
                if (chat == null)
                {
                    return;
                }

                var self = ViewModel.CacheService.IsSavedMessages(chat);

                flyout.CreateFlyoutSeparator();
                flyout.CreateFlyoutItem(new RelayCommand<Sticker>(anim => ViewModel.StickerSendExecute(anim, null, true)), sticker, Strings.Resources.SendWithoutSound, new FontIcon { Glyph = Icons.AlertOff });
                flyout.CreateFlyoutItem(new RelayCommand<Sticker>(anim => ViewModel.StickerSendExecute(anim, true, null)), sticker, self ? Strings.Resources.SetReminder : Strings.Resources.ScheduleMessage, new FontIcon { Glyph = Icons.CalendarClock });
            }

            args.ShowAt(flyout, element);
        }

        private void Animation_ContextRequested(UIElement sender, ItemContextRequestedEventArgs<Animation> args)
        {
            var element = sender as FrameworkElement;
            var animation = args.Item;

            if (animation == null)
            {
                return;
            }

            var flyout = new MenuFlyout();

            if (ViewModel.ProtoService.IsAnimationSaved(animation.AnimationValue.Id))
            {
                flyout.CreateFlyoutItem(ViewModel.AnimationDeleteCommand, animation, Strings.Resources.Delete, new FontIcon { Glyph = Icons.Delete });
            }
            else
            {
                flyout.CreateFlyoutItem(ViewModel.AnimationSaveCommand, animation, Strings.Resources.SaveToGIFs, new FontIcon { Glyph = Icons.Gif });
            }

            if (ViewModel.Type == ViewModels.DialogType.History)
            {
                var chat = ViewModel.Chat;
                if (chat == null)
                {
                    return;
                }

                var self = ViewModel.CacheService.IsSavedMessages(chat);

                flyout.CreateFlyoutSeparator();
                flyout.CreateFlyoutItem(new RelayCommand<Animation>(anim => ViewModel.AnimationSendExecute(anim, null, true)), animation, Strings.Resources.SendWithoutSound, new FontIcon { Glyph = Icons.AlertOff });
                flyout.CreateFlyoutItem(new RelayCommand<Animation>(anim => ViewModel.AnimationSendExecute(anim, true, null)), animation, self ? Strings.Resources.SetReminder : Strings.Resources.ScheduleMessage, new FontIcon { Glyph = Icons.CalendarClock });
            }

            args.ShowAt(flyout, element);
        }
    }
}
