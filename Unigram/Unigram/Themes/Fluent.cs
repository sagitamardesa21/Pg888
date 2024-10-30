﻿using System;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace Unigram.Themes
{
    public class Fluent : ResourceDictionary
    {
        public Fluent()
        {
            //if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            //{
            //    this["EllipsisButtonPadding"] = new Thickness(16, 19, 16, 0);
            //    this["GlyphButtonFontSize"] = 16d;
            //    this["ChatPhotoSize"] = 30d;
            //}
            //else
            {
                this["EllipsisButtonPadding"] = new Thickness(12, 19, 12, 0);
                //this["GlyphButtonFontSize"] = 20d;
                this["ChatPhotoSize"] = 36d;

                this["GlyphButtonFontSize"] = 16d;
                this["AppBarThemeCompactHeight"] = 48d;
                this["NavigationViewTopPaneHeight"] = 48d;
            }

            var commonStyles = new ResourceDictionary { Source = new Uri("ms-appx:///Common/CommonStyles.xaml") };
            MergedDictionaries.Add(commonStyles);

            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("ms-appx:///Themes/Fluent.xaml") });
            }
            else
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("ms-appx:///Themes/Plain.xaml") });
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
            }
            else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
            {
                MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
            }
            else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
            }
            else
            {
                // We don't want any kind of fluent effect prior to Fall Creators Update (so fluent will affect PCs only)
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("ms-appx://Microsoft.UI.Xaml.2.4/Microsoft.UI.Xaml/Themes/rs2_themeresources.xaml") });
                //this["NavigationViewTopPaneHeight"] = 48d;
            }
        }
    }
}
