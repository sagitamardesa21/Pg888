﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TdWindows;
using Windows.UI.Xaml.Data;

namespace Unigram.Converters
{
    public class FileTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case FileTypeAnimation animation:
                    return Strings.Android.LocalGifCache;
                case FileTypeAudio audio:
                    return Strings.Android.LocalMusicCache;
                case FileTypeDocument document:
                    return Strings.Android.FilesDataUsage;
                case FileTypeNone none:
                    return Strings.Android.TotalDataUsage;
                case FileTypePhoto photo:
                    return Strings.Android.LocalPhotoCache;
                case FileTypeVideo video:
                    return Strings.Android.LocalVideoCache;
                case FileTypeVideoNote videoNote:
                    return Strings.Android.VideoMessagesAutodownload;
                case FileTypeVoiceNote voiceNote:
                    return Strings.Android.AudioAutodownload;
                case FileTypeProfilePhoto profilePhoto:
                    return "Profile photos";
                case FileTypeSticker sticker:
                    return "Stickers";
                case FileTypeThumbnail thumbnail:
                    return "Thumbnails";
                case FileTypeSecret secret:
                case FileTypeSecretThumbnail secretThumbnail:
                case FileTypeUnknown unknown:
                case FileTypeWallpaper wallpaper:
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
