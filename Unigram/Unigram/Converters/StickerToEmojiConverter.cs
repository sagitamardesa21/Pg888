﻿using System;
using Windows.UI.Xaml.Data;

namespace Unigram.Converters
{
    public class StickerToEmojiConverter : IValueConverter
    {
        //private readonly IStickersService _stickersService = UnigramContainer.Current.ResolveType<IStickersService>();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //if (value is TLDocument document)
            //{
            //    return string.Join(" ", _stickersService.GetEmojiForSticker(document.Id));
            //}

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
