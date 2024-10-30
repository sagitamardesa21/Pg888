﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TdWindows;
using Telegram.Api.TL;
using Unigram.Converters;
using Unigram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls
{
    public class PaddedListView : SelectListView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var container = element as ListViewItem;
            var message = item as MessageViewModel;

            if (container != null && message != null)
            {
                var chat = message.GetChat();
                if (message.IsService())
                {
                    container.Padding = new Thickness(12, 0, 12, 0);

                    container.HorizontalAlignment = HorizontalAlignment.Stretch;
                    container.Width = double.NaN;
                    container.Height = double.NaN;
                    container.Margin = new Thickness();
                }
                else if (message.IsSaved() || (chat.Type is ChatTypeBasicGroup || chat.Type is ChatTypeSupergroup) && !message.IsChannelPost)
                {
                    if (message.IsOutgoing && !message.IsSaved())
                    {
                        if (message.Content is MessageSticker || message.Content is MessageVideoNote)
                        {
                            container.Padding = new Thickness(12, 0, 12, 0);
                        }
                        else
                        {
                            container.Padding = new Thickness(52, 0, 12, 0);
                        }
                    }
                    else
                    {
                        if (message.Content is MessageSticker || message.Content is MessageVideoNote)
                        {
                            container.Padding = new Thickness(52, 0, 12, 0);
                        }
                        else
                        {
                            container.Padding = new Thickness(52, 0, false ? 12 : 52, 0);
                        }
                    }
                }
                else
                {
                    if (message.Content is MessageSticker || message.Content is MessageVideoNote)
                    {
                        container.Padding = new Thickness(12, 0, 12, 0);
                    }
                    else
                    {
                        if (message.IsOutgoing && !message.IsChannelPost)
                        {
                            container.Padding = new Thickness(52, 0, 12, 0);
                        }
                        else
                        {
                            container.Padding = new Thickness(12, 0, false ? 12 : 52, 0);
                        }
                    }
                }
            }

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
