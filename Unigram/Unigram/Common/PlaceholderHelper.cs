//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using RLottie;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Td.Api;
using Unigram.Converters;
using Unigram.Native;
using Unigram.Services;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace Unigram.Common
{
    public static class PlaceholderHelper
    {
        private static readonly Color[] _colors = new Color[7]
        {
            Color.FromArgb(0xFF, 0xE5, 0x65, 0x55),
            Color.FromArgb(0xFF, 0xF2, 0x8C, 0x48),
            Color.FromArgb(0xFF, 0x8E, 0x85, 0xEE),
            Color.FromArgb(0xFF, 0x76, 0xC8, 0x4D),
            Color.FromArgb(0xFF, 0x5F, 0xBE, 0xD5),
            Color.FromArgb(0xFF, 0x54, 0x9C, 0xDD),
            Color.FromArgb(0xFF, 0xF2, 0x74, 0x9A),
        };

        private static readonly Color[] _colorsTop = new Color[7];
        private static readonly Color[] _colorsBottom = new Color[7];

        static PlaceholderHelper()
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                var hsv = _colors[i].ToHSL();
                var luminance = hsv.L;
                hsv.L = luminance * 0.9;
                _colorsTop[i] = hsv.ToRGB();
                hsv.L = luminance * 1.1;
                _colorsBottom[i] = hsv.ToRGB();
            }
        }

        public static SolidColorBrush GetBrush(long i)
        {
            return new SolidColorBrush(_colors[Math.Abs(i % _colors.Length)]);
        }

        public static CompositionBrush GetBrush(Compositor compositor, long i)
        {
            return compositor.CreateColorBrush(_colors[Math.Abs(i % _colors.Length)]);
        }

        public static ImageSource GetIdenticon(IList<byte> hash, int side)
        {
            var bitmap = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawIdenticon(hash, side, stream);
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
                        PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(chat), _colorsTop[Math.Abs(privata.UserId % _colors.Length)], _colorsBottom[Math.Abs(privata.UserId % _colors.Length)], stream);
                    }
                    else if (chat.Type is ChatTypeSecret secret)
                    {
                        PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(chat), _colorsTop[Math.Abs(secret.UserId % _colors.Length)], _colorsBottom[Math.Abs(secret.UserId % _colors.Length)], stream);
                    }
                    else if (chat.Type is ChatTypeBasicGroup basic)
                    {
                        PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(chat), _colorsTop[Math.Abs(basic.BasicGroupId % _colors.Length)], _colorsBottom[Math.Abs(basic.BasicGroupId % _colors.Length)], stream);
                    }
                    else if (chat.Type is ChatTypeSupergroup super)
                    {
                        PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(chat), _colorsTop[Math.Abs(super.SupergroupId % _colors.Length)], _colorsBottom[Math.Abs(super.SupergroupId % _colors.Length)], stream);
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
                    PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(chat), _colorsTop[5], _colorsBottom[5], stream);
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
                    PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(user), _colorsTop[Math.Abs(user.Id % _colors.Length)], _colorsBottom[Math.Abs(user.Id % _colors.Length)], stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetNameForUser(string firstName, string lastName, int side, int color = 5)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(firstName, lastName), _colorsTop[Math.Abs(color % _colors.Length)], _colorsBottom[Math.Abs(color % _colors.Length)], stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetNameForUser(string name, int side, int color = 5)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert((object)name), _colorsTop[Math.Abs(color % _colors.Length)], _colorsBottom[Math.Abs(color % _colors.Length)], stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetNameForChat(string title, int side, int color = 5)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawProfilePlaceholder(InitialNameStringConverter.Convert(title), _colorsTop[Math.Abs(color % _colors.Length)], _colorsBottom[Math.Abs(color % _colors.Length)], stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetSavedMessages(long id, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawSavedMessages(_colorsTop[5], _colorsBottom[5], stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetDeletedUser(User user, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawDeletedUser(_colorsTop[Math.Abs(user.Id % _colors.Length)], _colorsBottom[Math.Abs(user.Id % _colors.Length)], stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetGlyph(string glyph, int id, int side)
        {
            var bitmap = new BitmapImage { DecodePixelWidth = side, DecodePixelHeight = side, DecodePixelType = DecodePixelType.Logical };
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawGlyph(glyph, _colorsTop[Math.Abs(id % _colors.Length)], _colorsBottom[Math.Abs(id % _colors.Length)], stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }


        public static ImageSource GetBitmap(IClientService clientService, PhotoSize photoSize)
        {
            return GetBitmap(clientService, photoSize.Photo, photoSize.Width, photoSize.Height);
        }

        public static ImageSource GetBitmap(IClientService clientService, File file, int width, int height)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                return UriEx.ToBitmap(file.Local.Path, width, height);
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive && clientService != null)
            {
                clientService.DownloadFile(file.Id, 1);
            }

            return null;
        }

        private static readonly DisposableMutex _patternSurfaceLock = new DisposableMutex();

        public static async Task<CanvasBitmap> GetPatternBitmapAsync(ICanvasResourceCreator resourceCreator, File file)
        {
            using var locked = await _patternSurfaceLock.WaitAsync();

            var bitmap = default(CanvasBitmap);

            var cache = $"{file.Remote.UniqueId}.cache.png";
            var relative = System.IO.Path.Combine("wallpapers", cache);

            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(relative) as StorageFile;
            if (item == null)
            {
                item = await ApplicationData.Current.LocalFolder.CreateFileAsync(relative, CreationCollisionOption.ReplaceExisting);

                using (var stream = await item.OpenAsync(FileAccessMode.ReadWrite))
                {
                    try
                    {
                        var text = await GetSvgXml(file.Local.Path);
                        await PlaceholderImageHelper.Current.DrawSvgAsync(text, Colors.White, stream);
                    }
                    catch { }
                }
            }

            if (item != null)
            {
                using (var stream = await item.OpenReadAsync())
                {
                    try
                    {
                        bitmap = await CanvasBitmap.LoadAsync(resourceCreator, stream);
                    }
                    catch { }
                }
            }

            return bitmap;
        }

        private static async Task<string> GetSvgXml(string filePath)
        {
            var styles = new Dictionary<string, string>();
            var text = string.Empty;

            using (var source = System.IO.File.OpenRead(filePath))
            using (var decompress = new GZipStream(source, CompressionMode.Decompress))
            using (var reader = new System.IO.StreamReader(decompress))
            {
                text = await reader.ReadToEndAsync();
            }

            var document = XDocument.Parse(text);
            var svg = XNamespace.Get("http://www.w3.org/2000/svg");

            foreach (var styleNode in document.Root.Descendants(svg + "style"))
            {
                var currentStyleString = styleNode.Value;
                int currentClassNameStartIndex = -1;
                int currentClassContentsStartIndex = -1;

                string currentClassName = null;

                for (int i = 0; i < currentStyleString.Length; i++)
                {
                    var c = currentStyleString[i];
                    if (currentClassNameStartIndex != -1)
                    {
                        if (!char.IsLetterOrDigit(c))
                        {
                            currentClassName = currentStyleString.Substring(currentClassNameStartIndex, i - currentClassNameStartIndex);
                            currentClassNameStartIndex = -1;
                        }
                    }
                    else if (currentClassContentsStartIndex != -1)
                    {
                        if (c == '}')
                        {
                            var classContents = currentStyleString.Substring(currentClassContentsStartIndex, i - currentClassContentsStartIndex);
                            if (currentClassName != null && classContents != null)
                            {
                                styles[currentClassName] = classContents;
                                currentClassName = null;
                            }
                            currentClassContentsStartIndex = -1;
                        }
                    }

                    if (currentClassNameStartIndex == -1 && currentClassContentsStartIndex == -1)
                    {
                        if (c == '.')
                        {
                            currentClassNameStartIndex = i + 1;
                        }
                        else if (c == '{')
                        {
                            currentClassContentsStartIndex = i + 1;
                        }
                    }
                }

            }

            foreach (var styleName in styles)
            {
                text = text.Replace($"class=\"{styleName.Key}\"", $"style=\"{styleName.Value}\"");
            }

            return text;
        }

        public static ImageSource GetBlurred(string path, float amount = 3)
        {
            var bitmap = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawThumbnailPlaceholder(path, amount, stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static async Task<ImageSource> GetBlurredAsync(string path, float amount = 3)
        {
            var bitmap = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    await Task.Run(() => PlaceholderImageHelper.Current.DrawThumbnailPlaceholder(path, amount, stream));
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetBlurred(IList<byte> bytes, float amount = 3)
        {
            var bitmap = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    PlaceholderImageHelper.Current.DrawThumbnailPlaceholder(bytes, amount, stream);
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static async Task<ImageSource> GetBlurredAsync(IList<byte> bytes, float amount = 3)
        {
            var bitmap = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                try
                {
                    await Task.Run(() => PlaceholderImageHelper.Current.DrawThumbnailPlaceholder(bytes, amount, stream));
                    bitmap.SetSource(stream);
                }
                catch { }
            }

            return bitmap;
        }

        public static ImageSource GetWebPFrame(string path, double maxWidth = 512)
        {
            try
            {
                Size size;
                var buffer = PlaceholderImageHelper.Current.DrawWebP(path, (int)maxWidth, out size);

                if (size.Width > 0 && size.Height > 0)
                {
                    var bitmap = new WriteableBitmap((int)size.Width, (int)size.Height);
                    buffer.CopyTo(bitmap.PixelBuffer);

                    return bitmap;
                }
                else
                {
                    return new BitmapImage(UriEx.ToLocal(path));
                }
            }
            catch { }

            return null;
        }

        public static async Task<ImageSource> GetWebPFrameAsync(string path, double maxWidth = 512)
        {
            try
            {
                maxWidth = maxWidth < 512 ? maxWidth * TLWindowContext.Current.RasterizationScale : maxWidth;
                maxWidth = Math.Min(maxWidth, 512);

                Size size = default;
                var buffer = await Task.Run(() => PlaceholderImageHelper.Current.DrawWebP(path, (int)maxWidth, out size));

                if (size.Width > 0 && size.Height > 0)
                {
                    var bitmap = new WriteableBitmap((int)size.Width, (int)size.Height);
                    buffer.CopyTo(bitmap.PixelBuffer);

                    return bitmap;
                }
                else
                {
                    return new BitmapImage(UriEx.ToLocal(path));
                }
            }
            catch { }

            return null;
        }

        public static ImageSource GetLottieFrame(string path, int frame, int width, int height, bool webp = true)
        {
            // Frame size affects disk cache, so we always use 256.
            var frameSize = new Windows.Graphics.SizeInt32 { Width = 256, Height = 256 };

            var animation = LottieAnimation.LoadFromFile(path, frameSize, false, null);
            if (animation == null)
            {
                if (webp)
                {
                    return GetWebPFrame(path, width);
                }

                return null;
            }

            var bitmap = new WriteableBitmap(width, height);
            animation.RenderSync(bitmap, frame);
            animation.Dispose();

            return bitmap;
        }
    }
}
