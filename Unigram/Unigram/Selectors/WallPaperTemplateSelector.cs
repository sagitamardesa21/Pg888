﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.TL;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Selectors
{
    public class WallPaperTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }

        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate SolidTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TLWallPaperSolid)
            {
                return SolidTemplate;
            }
            else if (item is TLWallPaper wallpaper && wallpaper.Id.Equals(1000001))
            {
                return DefaultTemplate;
            }

            return ItemTemplate;
        }
    }
}
