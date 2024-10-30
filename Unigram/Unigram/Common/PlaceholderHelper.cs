﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Converters;
using Unigram.Native;
using Unigram.Services;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Unigram.Common
{
    public static class PlaceholderHelper
    {
        private static Color[] _colors = new Color[7]
        {
            Color.FromArgb(0xFF, 0xE5, 0x65, 0x55),
            Color.FromArgb(0xFF, 0xF2, 0x8C, 0x48),
            Color.FromArgb(0xFF, 0x8E, 0x85, 0xEE),
            Color.FromArgb(0xFF, 0x76, 0xC8, 0x4D),
            Color.FromArgb(0xFF, 0x5F, 0xBE, 0xD5),
            Color.FromArgb(0xFF, 0x54, 0x9C, 0xDD),
            Color.FromArgb(0xFF, 0xF2, 0x74, 0x9A),
        };

        public static SolidColorBrush GetBrush(int i)
        {
            return new SolidColorBrush(_colors[Math.Abs(i % _colors.Length)]);
        }

        public static ImageSource GetIdenticon(IList<byte> hash, int side)
        {
            var bitmap = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                PlaceholderImageHelper.GetForCurrentView().DrawIdenticon(hash, side, stream);
                try
                {
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetChat(Chat chat, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    if (chat.Type is ChatTypePrivate privata)
                    {
                        PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[Math.Abs(privata.UserId % _colors.Length)], InitialNameStringConverter.Convert(chat), stream);
                    }
                    else if (chat.Type is ChatTypeSecret secret)
                    {
                        PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[Math.Abs(secret.UserId % _colors.Length)], InitialNameStringConverter.Convert(chat), stream);
                    }
                    else if (chat.Type is ChatTypeBasicGroup basic)
                    {
                        PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[Math.Abs(basic.BasicGroupId % _colors.Length)], InitialNameStringConverter.Convert(chat), stream);
                    }
                    else if (chat.Type is ChatTypeSupergroup super)
                    {
                        PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[Math.Abs(super.SupergroupId % _colors.Length)], InitialNameStringConverter.Convert(chat), stream);
                    }

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetChat(ChatInviteLinkInfo chat, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[0], InitialNameStringConverter.Convert(chat), stream);

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetUser(User user, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[Math.Abs(user.Id % _colors.Length)], InitialNameStringConverter.Convert(user), stream);

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetNameForUser(string firstName, string lastName, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[5], InitialNameStringConverter.Convert(firstName, lastName), stream);

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetNameForUser(string name, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[5], InitialNameStringConverter.Convert((object)name), stream);

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetNameForChat(string title, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.GetForCurrentView().DrawProfilePlaceholder(_colors[5], InitialNameStringConverter.Convert(title), stream);

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetSavedMessages(int id, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.GetForCurrentView().DrawSavedMessages(_colors[Math.Abs(id % _colors.Length)], stream);

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetChat(IProtoService protoService, Chat chat, int side)
        {
            if (chat.Type is ChatTypePrivate privata && protoService != null && protoService.IsSavedMessages(chat))
            {
                return GetSavedMessages(privata.UserId, side);
            }

            var file = chat.Photo?.Small;
            if (file != null)
            {
                if (file.Local.IsDownloadingCompleted)
                {
                    return new BitmapImage(new Uri("file:///" + file.Local.Path)) { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
                }
                else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
                {
                    protoService?.DownloadFile(file.Id, 1);
                }
            }
            else
            {
                return GetChat(chat, side);
            }

            return null;
        }

        public static ImageSource GetChat(IProtoService protoService, ChatInviteLinkInfo chat, int side)
        {
            var file = chat.Photo?.Small;
            if (file != null)
            {
                if (file.Local.IsDownloadingCompleted)
                {
                    return new BitmapImage(new Uri("file:///" + file.Local.Path)) { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
                }
                else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
                {
                    protoService?.DownloadFile(file.Id, 1);
                }
            }
            else
            {
                return GetChat(chat, side);
            }

            return null;
        }

        public static ImageSource GetUser(IProtoService protoService, User user, int side)
        {
            var file = user.ProfilePhoto?.Small;
            if (file != null)
            {
                if (file.Local.IsDownloadingCompleted)
                {
                    return new BitmapImage(new Uri("file:///" + file.Local.Path)) { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
                }
                else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
                {
                    protoService?.DownloadFile(file.Id, 1);
                }
            }
            else
            {
                return GetUser(user, side);
            }

            return null;
        }

        public static ImageSource GetBitmap(IProtoService protoService, File file, int width, int height)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                return new BitmapImage(new Uri("file:///" + file.Local.Path)) { DecodePixelWidth = width, DecodePixelHeight = height, DecodePixelType = DecodePixelType.Logical };
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                protoService.DownloadFile(file.Id, 1);
            }

            return null;
        }

        public static ImageSource GetBlurred(string path, float amount = 3)
        {
            var bitmap = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.GetForCurrentView().DrawThumbnailPlaceholder(path, amount, stream);

                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static async Task<ImageSource> GetWebpAsync(string path)
        {
            if (ApiInfo.CanDecodeWebp)
            {
                return new BitmapImage(new Uri("file:///" + path));
            }
            else
            {
                var temp = await StorageFile.GetFileFromPathAsync(path);
                var buffer = await FileIO.ReadBufferAsync(temp);

                return WebPImage.DecodeFromBuffer(buffer);
            }
        }
    }
}
