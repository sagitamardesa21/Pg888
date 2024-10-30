﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Native;
using Unigram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Selectors
{
    public class AutocompleteTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MentionTemplate { get; set; }
        public DataTemplate CommandTemplate { get; set; }
        public DataTemplate HashtagTemplate { get; set; }
        public DataTemplate EmojiTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is User)
            {
                return MentionTemplate;
            }
            else if (item is UserCommand)
            {
                return CommandTemplate;
            }
            else if (item is EmojiSuggestion)
            {
                return EmojiTemplate;
            }

            //return ItemTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
