﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unigram.Common;
using Unigram.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Selectors
{
    public class MessageStyleSelector : StyleSelector
    {
        public Style MessageStyle { get; set; }
        public Style ServiceStyle { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item is MessageViewModel message && message.IsService())
            {
                return ServiceStyle;
            }

            return MessageStyle;
        }
    }
}
