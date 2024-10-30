﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls
{
    public class GlyphRadioButton : RadioButton
    {
        public GlyphRadioButton()
        {
            DefaultStyleKey = typeof(GlyphRadioButton);
        }

        #region Glyph

        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register("Glyph", typeof(string), typeof(GlyphRadioButton), new PropertyMetadata(null));

        #endregion

        #region CheckedGlyph

        public string CheckedGlyph
        {
            get => (string)GetValue(CheckedGlyphProperty);
            set => SetValue(CheckedGlyphProperty, value);
        }

        public static readonly DependencyProperty CheckedGlyphProperty =
            DependencyProperty.Register("CheckedGlyph", typeof(string), typeof(GlyphRadioButton), new PropertyMetadata(null));

        #endregion

    }
}
