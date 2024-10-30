//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using LinqToVisualTree;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Controls.Messages;
using Telegram.Entities;
using Telegram.Native;
using Telegram.Services;
using Telegram.Td;
using Telegram.Td.Api;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static Telegram.Services.GenerationService;
using Point = Windows.Foundation.Point;

namespace Telegram.Common
{
    public static class Extensions
    {
        public static void ForEach<T>(this ListViewBase listView, Action<SelectorItem, T> handler) where T : class
        {
            int lastCacheIndex;
            int firstCacheIndex;

            if (listView.ItemsPanelRoot is ItemsStackPanel stack)
            {
                lastCacheIndex = stack.LastCacheIndex;
                firstCacheIndex = stack.FirstCacheIndex;
            }
            else if (listView.ItemsPanelRoot is ItemsWrapGrid wrap)
            {
                lastCacheIndex = wrap.LastCacheIndex;
                firstCacheIndex = wrap.FirstCacheIndex;
            }
            else
            {
                return;
            }

            for (int i = firstCacheIndex; i <= lastCacheIndex; i++)
            {
                var container = listView.ContainerFromIndex(i) as SelectorItem;
                if (container == null)
                {
                    continue;
                }

                handler(container, listView.ItemFromContainer(container) as T);
            }
        }

        public static async Task<StorageMedia> PickSingleMediaAsync(this FileOpenPicker picker)
        {
            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return null;
            }

            return await StorageMedia.CreateAsync(file);
        }

        public static Version ToVersion(this PackageVersion version)
        {
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }

        public static void TryNotifyMutedChanged(this VoipCallCoordinator coordinator, bool muted)
        {
            try
            {
                if (muted)
                {
                    coordinator?.NotifyMuted();
                }
                else
                {
                    coordinator?.NotifyUnmuted();
                }
            }
            catch
            {

            }
        }

        public static void TryNotifyCallActive(this VoipPhoneCall call)
        {
            try
            {
                call.NotifyCallActive();
            }
            catch
            {

            }
        }

        public static void TryNotifyCallEnded(this VoipPhoneCall call)
        {
            try
            {
                call.NotifyCallEnded();
            }
            catch
            {

            }
        }

        public static string ToQuery(this Dictionary<string, string> dictionary)
        {
            var result = string.Empty;

            foreach (var item in dictionary)
            {
                result += $"{item.Key}={item.Value}&";
            }

            return result.TrimEnd('&');
        }

        public static void ShowTeachingTip(this Window app, FrameworkElement target, string text, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight)
        {
            ShowTeachingTip(app, target, text, null, placement);
        }

        public static void ShowTeachingTip(this Window app, FrameworkElement target, string text, IAnimatedVisualSource2 icon, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight)
        {
            ShowTeachingTip(app, target, new FormattedText(text, Array.Empty<TextEntity>()), icon, placement);
        }

        public static void ShowTeachingTip(this Window app, FrameworkElement target, FormattedText text, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight)
        {
            ShowTeachingTip(app, target, text, null, placement);
        }

        public static void ShowTeachingTip(this Window app, FrameworkElement target, FormattedText text, IAnimatedVisualSource2 icon, TeachingTipPlacementMode placement = TeachingTipPlacementMode.TopRight)
        {
            var label = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };

            TextBlockHelper.SetFormattedText(label, text);
            Grid.SetColumn(label, 1);

            var content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            content.ColumnDefinitions.Add(new ColumnDefinition());
            content.Children.Add(label);

            AnimatedIcon animated = null;
            if (icon != null)
            {
                animated = new AnimatedIcon
                {
                    Source = icon,
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(-4, -12, 8, -12)
                };

                AnimatedIcon.SetState(animated, "Normal");
                content.Children.Add(animated);
            }

            var tip = new TeachingTip
            {
                Target = target,
                PreferredPlacement = placement,
                IsLightDismissEnabled = true,
                Content = content,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                MinWidth = 0,
            };


            if (app.Content is FrameworkElement element)
            {
                element.Resources["TeachingTip"] = tip;
            }

            if (animated != null)
            {
                void handler(object sender, RoutedEventArgs e)
                {
                    tip.Loaded -= handler;
                    AnimatedIcon.SetState(animated, "Checked");
                }

                tip.Loaded += handler;
            }

            tip.IsOpen = true;
        }

        public static bool IsKeyDown(this CoreWindow window, Windows.System.VirtualKey key)
        {
            return window.GetKeyState(key).HasFlag(CoreVirtualKeyStates.Down);
        }

        public static bool IsKeyDownAsync(this CoreWindow window, Windows.System.VirtualKey key)
        {
            return window.GetAsyncKeyState(key).HasFlag(CoreVirtualKeyStates.Down);
        }

        public static Color ToColor(this int color, bool alpha = false)
        {
            if (alpha)
            {
                byte a = (byte)((color & 0xff000000) >> 24);
                byte r = (byte)((color & 0x00ff0000) >> 16);
                byte g = (byte)((color & 0x0000ff00) >> 8);
                byte b = (byte)(color & 0x000000ff);

                return Color.FromArgb(a, r, g, b);
            }

            return Color.FromArgb(0xFF, (byte)((color >> 16) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)(color & 0xFF));
        }

        public static int ToValue(this Color color, bool alpha = false)
        {
            if (alpha)
            {
                return (color.A << 24) + (color.R << 16) + (color.G << 8) + color.B;
            }

            return (color.R << 16) + (color.G << 8) + color.B;
        }

        public static Brush WithOpacity(this Brush brush, double opacity)
        {
            if (brush is SolidColorBrush solid)
            {
                return new SolidColorBrush(solid.Color) { Opacity = opacity };
            }

            return brush;
        }

        /// <summary>
        /// Test for almost equality to 0.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="epsilon"></param>
        public static bool AlmostEqualsToZero(this double number, double epsilon = 1e-5)
        {
            return number > -epsilon && number < epsilon;
        }

        /// <summary>
        /// Test for almost equality.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        public static bool AlmostEquals(this double number, double other, double epsilon = 1e-5)
        {
            return (number - other).AlmostEqualsToZero(epsilon);
        }

        /// <summary>
        /// Test for almost equality to 0.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="epsilon"></param>
        public static bool AlmostEqualsToZero(this float number, float epsilon = 1e-5f)
        {
            return number > -epsilon && number < epsilon;
        }

        /// <summary>
        /// Test for almost equality.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        public static bool AlmostEquals(this float number, float other, float epsilon = 1e-5f)
        {
            return (number - other).AlmostEqualsToZero(epsilon);
        }

        public static int ToTimestamp(this DateTime dateTime)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime.SpecifyKind(dtDateTime, DateTimeKind.Utc);

            return (int)(dateTime.ToUniversalTime() - dtDateTime).TotalSeconds;
        }

        public static long ToTimestampMilliseconds(this DateTime dateTime)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime.SpecifyKind(dtDateTime, DateTimeKind.Utc);

            return (long)(dateTime.ToUniversalTime() - dtDateTime).TotalMilliseconds;
        }

        public static bool TryGet<T>(this IDictionary<object, object> dict, object key, out T value)
        {
            bool success;
            if (success = dict.TryGetValue(key, out object tryGetValue))
            {
                value = (T)tryGetValue;
            }
            else
            {
                value = default;
            }
            return success;
        }

        public static bool TryGet<T>(this IDictionary<string, object> dict, string key, out T value)
        {
            bool success;
            if (success = dict.TryGetValue(key, out object tryGetValue))
            {
                value = (T)tryGetValue;
            }
            else
            {
                value = default;
            }
            return success;
        }

        public static void Put<T>(this IList<T> source, bool begin, T item)
        {
            if (begin)
            {
                source.Insert(0, item);
            }
            else
            {
                source.Add(item);
            }
        }

        public static string Enqueue(this StorageItemAccessList list, IStorageItem item)
        {
            try
            {
                if (list.Entries.Count >= list.MaximumItemsAllowed - 10)
                {
                    var first = list.Entries.LastOrDefault();
                    if (first.Token != null)
                    {
                        list.Remove(first.Token);
                    }
                }
            }
            catch { }

            try
            {
                return list.Add(item);
            }
            catch
            {
                return null;
            }
        }

        public static string RegexReplace(this string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }

        public static uint GetHeight(this ImageProperties props)
        {
            return props.Height;
            return props.Orientation == PhotoOrientation.Rotate180 ? props.Height : props.Width;
        }

        public static uint GetWidth(this ImageProperties props)
        {
            return props.Width;
            return props.Orientation == PhotoOrientation.Rotate180 ? props.Width : props.Height;
        }



        public static uint GetHeight(this VideoProperties props)
        {
            return props.Orientation is VideoOrientation.Rotate180 or VideoOrientation.Normal ? props.Height : props.Width;
        }

        public static uint GetWidth(this VideoProperties props)
        {
            return props.Orientation is VideoOrientation.Rotate180 or VideoOrientation.Normal ? props.Width : props.Height;
        }

        public static void Shiftino<T>(this T[] array, int offset)
        {
            if (offset < 0)
            {
                while (offset < 0)
                {
                    var element = array[array.Length - 1];
                    Array.Copy(array, 0, array, 1, array.Length - 1);
                    array[0] = element;
                    offset += 1;
                }
            }
            else if (offset > 0)
            {
                while (offset > 0)
                {
                    var element = array[0];
                    Array.Copy(array, 1, array, 0, array.Length - 1);
                    array[array.Length - 1] = element;
                    offset -= 1;
                }
            }
        }


        public static T[] Shift<T>(this T[] array, int offset)
        {
            var output = new T[array.Length];

            if (offset < 0)
            {
                while (offset < 0)
                {
                    var element = array[output.Length - 1];
                    Array.Copy(array, 0, output, 1, array.Length - 1);
                    output[0] = element;
                    offset += 1;

                    array = output;
                }
            }
            else if (offset > 0)
            {
                while (offset > 0)
                {
                    var element = array[0];
                    Array.Copy(array, 1, output, 0, array.Length - 1);
                    output[output.Length - 1] = element;
                    offset -= 1;

                    array = output;
                }
            }
            else
            {
                Array.Copy(array, 0, output, 0, array.Length);
            }

            return output;
        }

        /// <summary>
        /// Applies the action to each element in the list.
        /// </summary>
        /// <typeparam name="T">The enumerable item's type.</typeparam>
        /// <param name="enumerable">The elements to enumerate.</param>
        /// <param name="action">The action to apply to each item in the list.</param>
        public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static string Substr(this string source, int startIndex, int endIndex)
        {
            return source.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static async Task<InputFile> ToGeneratedAsync(this StorageFile file, ConversionType conversion = ConversionType.Copy, string arguments = null, bool forceCopy = false)
        {
            var token = StorageApplicationPermissions.FutureAccessList.Enqueue(file);
            var path = file.Path;

            if (conversion == ConversionType.Copy && arguments == null && NativeUtils.IsFileReadable(file.Path) && !forceCopy)
            {
                return new InputFileLocal(path);
            }

            if (conversion == ConversionType.Compress)
            {
                path = Path.GetFileNameWithoutExtension(path) + ".jpg";
            }

            var props = await file.GetBasicPropertiesAsync();
            return new InputFileGenerated(path, token + "#" + conversion + (arguments != null ? "#" + arguments : string.Empty) + "#" + props.DateModified.ToString("s"), (int)props.Size);
        }

        public static async Task<InputThumbnail> ToThumbnailAsync(this StorageFile file, VideoConversion video = null, ConversionType conversion = ConversionType.Copy, string arguments = null)
        {
            var props = await file.Properties.GetVideoPropertiesAsync();

            double originalWidth = props.GetWidth();
            double originalHeight = props.GetHeight();

            if (!video.CropRectangle.IsEmpty)
            {
                originalWidth = video.CropRectangle.Width;
                originalHeight = video.CropRectangle.Height;
            }

            double ratioX = 90 / originalWidth;
            double ratioY = 90 / originalHeight;
            double ratio = Math.Min(ratioX, ratioY);

            int width = (int)(originalWidth * ratio);
            int height = (int)(originalHeight * ratio);

            return new InputThumbnail(await file.ToGeneratedAsync(conversion, arguments), width, height);
        }

        public static T RemoveLast<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                var last = list[list.Count - 1];
                list.Remove(last);

                return last;
            }

            return default;
        }

        public static bool IsEmpty<T>(this ICollection<T> items)
        {
            return items.Count == 0;
        }

        public static void PutRange<TKey, TItem>(this IDictionary<TKey, TItem> list, IDictionary<TKey, TItem> source)
        {
            foreach (var item in source)
            {
                list[item.Key] = item.Value;
            }
        }


        public static bool Equals(this string input, params string[] check)
        {
            foreach (var str in check)
            {
                if (input.Equals(str))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEmpty(this Rect rect)
        {
            return rect == default || (rect.Width == 0 && rect.Height == 0);
        }

        public static bool GetBoolean(this ApplicationDataContainer container, string key, bool defaultValue)
        {
            if (container.Values.TryGetValue(key, out object value) && value is bool result)
            {
                return result;
            }

            return defaultValue;
        }

        public static int GetInt32(this ApplicationDataContainer container, string key, int defaultValue)
        {
            if (container.Values.TryGetValue(key, out object value) && value is int result)
            {
                return result;
            }

            return defaultValue;
        }

        public static long GetInt64(this ApplicationDataContainer container, string key, long defaultValue)
        {
            if (container.Values.TryGetValue(key, out object value))
            {
                if (value is long result64)
                {
                    return result64;
                }
                else if (value is int result32)
                {
                    return result32;
                }
            }

            return defaultValue;
        }

        public static bool GetBoolean(this ApplicationDataCompositeValue container, string key, bool defaultValue)
        {
            if (container.TryGetValue(key, out object value) && value is bool result)
            {
                return result;
            }

            return defaultValue;
        }

        public static int GetInt32(this ApplicationDataCompositeValue container, string key, int defaultValue)
        {
            if (container.TryGetValue(key, out object value) && value is int result)
            {
                return result;
            }

            return defaultValue;
        }

        public static void BeginOnUIThread(this DependencyObject element, Action action)
        {
            try
            {
                if (element.Dispatcher.HasThreadAccess)
                {
                    action();
                }
                else
                {
                    _ = element.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
                }
            }
            catch
            {
                // Most likey Excep_InvalidComObject_NoRCW_Wrapper, so we can just ignore it
            }
        }

        public static bool TypeEquals(this object o1, object o2)
        {
            if (o1 == null || o2 == null)
            {
                return false;
            }

            return Equals(o1.GetType(), o2.GetType());
        }

        public static Regex _pattern = new Regex("[\\-0-9]+", RegexOptions.Compiled);
        public static int ToInt32(this string value)
        {
            if (value == null)
            {
                return 0;
            }

            var val = 0;
            try
            {
                var matcher = _pattern.Match(value);
                if (matcher.Success)
                {
                    var num = matcher.Groups[0].Value;
                    val = int.Parse(num);
                }
            }
            catch (Exception)
            {
                //FileLog.e(e);
            }

            return val;
        }

        public static int TryParseOrDefault(string value, int defaultValue)
        {
            int.TryParse(value, out defaultValue);
            return defaultValue;
        }

        public static Dictionary<string, string> ParseQueryString(this string query, char separator = '&')
        {
            var first = query.Split('?');
            if (first.Length > 1)
            {
                query = first.Last();
            }

            var queryDict = new Dictionary<string, string>();
            foreach (var token in query.TrimStart(new char[] { '?' }).Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    queryDict[parts[0].Trim()] = WebUtility.UrlDecode(parts[1]).Trim();
                }
                else
                {
                    queryDict[parts[0].Trim()] = "";
                }
            }
            return queryDict;
        }

        public static string GetParameter(this Dictionary<string, string> query, string key)
        {
            query.TryGetValue(key, out string value);
            return value;
        }

        public static bool IsBetween(this TimeSpan value, TimeSpan minimum, TimeSpan maximum)
        {
            // see if start comes before end
            if (minimum < maximum)
            {
                return minimum <= value && value <= maximum;
            }

            // start is after end, so do the inverse comparison
            return !(maximum < value && value < minimum);
        }

        public static bool IsValidUrl(this string text)
        {
            return IsValidEntity<TextEntityTypeUrl>(text);
        }

        public static bool IsValidEmailAddress(this string text)
        {
            return IsValidEntity<TextEntityTypeEmailAddress>(text);
        }

        public static bool IsValidEntity<T>(this string text)
        {
            var response = Client.Execute(new GetTextEntities(text));
            if (response is TextEntities entities)
            {
                return entities.Entities.Count == 1 && entities.Entities[0].Offset == 0 && entities.Entities[0].Length == text.Length && entities.Entities[0].Type is T;
            }

            return false;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool StartsWith(this string source, string[] toCheck, StringComparison comp)
        {
            foreach (var item in toCheck)
            {
                if (source.StartsWith(item, comp))
                {
                    return true;
                }
            }

            return false;
        }

        public static string Format(this string input)
        {
            if (input != null)
            {
                return input.Trim().Replace("\r\n", "\n").Replace('\v', '\n').Replace('\r', '\n');
            }

            return string.Empty;
        }

        public static string TrimStart(this string target, string trimString)
        {
            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string TrimEnd(this string target, string trimString)
        {
            string result = target;
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

        //public static string TrimEnd(this string input, string suffixToRemove)
        //{
        //    if (input != null && suffixToRemove != null && input.EndsWith(suffixToRemove))
        //    {
        //        return input.Substring(0, input.Length - suffixToRemove.Length);
        //    }
        //    else return input;
        //}

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                list.Add(item);
            }
        }

        public static void AddRange<T>(this IList<T> list, params T[] source)
        {
            foreach (var item in source)
            {
                list.Add(item);
            }
        }

        public static List<T> Buffered<T>(int count)
        {
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(default);
            }

            return result;
        }

        public static Hyperlink GetHyperlinkFromPoint(this RichTextBlock text, Point point)
        {
            var position = text.GetPositionFromPoint(point);
            var hyperlink = GetHyperlink(position.Parent as TextElement);

            return hyperlink;
        }

        private static Hyperlink GetHyperlink(TextElement parent)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent is Hyperlink)
            {
                return parent as Hyperlink;
            }

            return GetHyperlink(parent.ElementStart.Parent as TextElement);
        }

        public static bool IsEmpty<T>(this IList<T> list)
        {
            return list.Count == 0;
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action?.Invoke(item);
            }
        }

        public static List<Control> AllChildren(this DependencyObject parent)
        {
            var list = new List<Control>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Control)
                {
                    list.Add(child as Control);
                }
                list.AddRange(AllChildren(child));
            }

            return list;
        }

        public static IEnumerable<T> AllChildren<T>(this DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is T)
                {
                    yield return (T)(object)_Child;
                }
            }
        }

        public static T GetChild<T>(this DependencyObject parentContainer, string controlName) where T : FrameworkElement
        {
            return parentContainer.Descendants<T>().FirstOrDefault(x => x.Name.Equals(controlName));
        }

        public static async Task UpdateLayoutAsync(this FrameworkElement element, bool update = false)
        {
            var tcs = new TaskCompletionSource<bool>();
            void layoutUpdated(object s1, object e1)
            {
                tcs.TrySetResult(true);
            }

            try
            {
                element.LayoutUpdated += layoutUpdated;

                if (update)
                {
                    element.UpdateLayout();
                }

                await tcs.Task;
            }
            finally
            {
                element.LayoutUpdated -= layoutUpdated;
            }
        }
    }

    public static class ClipboardEx
    {
        public static void TrySetContent(DataPackage content)
        {
            try
            {
                Clipboard.SetContent(content);
                Clipboard.Flush();
            }
            catch { }
        }
    }

    // Modified from: https://stackoverflow.com/a/32559623/1680863
    public static class ListViewExtensions
    {
        public static async Task ScrollToItem2(this ListViewBase listViewBase, object item, VerticalAlignment alignment, bool highlight, double? pixel = null)
        {
            var scrollViewer = listViewBase.GetScrollViewer();
            if (scrollViewer == null)
            {
                return;
            }

            //listViewBase.SelectionMode = ListViewSelectionMode.Single;
            //listViewBase.SelectedItem = item;

            var selectorItem = listViewBase.ContainerFromItem(item) as SelectorItem;
            if (selectorItem == null)
            {
                // call task-based ScrollIntoViewAsync to realize the item
                await listViewBase.ScrollIntoViewAsync(item, ScrollIntoViewAlignment.Leading, true);

                // this time the item shouldn't be null again
                selectorItem = (SelectorItem)listViewBase.ContainerFromItem(item);
            }

            if (selectorItem == null)
            {
                return;
            }

            // calculate the position object in order to know how much to scroll to
            var transform = selectorItem.TransformToVisual((UIElement)scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            if (alignment == VerticalAlignment.Top)
            {
                if (pixel is double adjust)
                {
                    position.Y -= adjust;
                }
            }
            else if (alignment == VerticalAlignment.Center)
            {
                position.Y -= (listViewBase.ActualHeight - selectorItem.ActualHeight) / 2d;
            }
            else if (alignment == VerticalAlignment.Bottom)
            {
                position.Y -= listViewBase.ActualHeight - selectorItem.ActualHeight;

                if (pixel is double adjust)
                {
                    position.Y += adjust;
                }
            }

            // scroll to desired position with animation!
            scrollViewer.ChangeView(position.X, position.Y, null, alignment != VerticalAlignment.Center);

            if (highlight)
            {
                var bubble = selectorItem.Descendants<MessageBubble>().FirstOrDefault();
                if (bubble == null)
                {
                    return;
                }

                bubble.Highlight();
            }
        }

        public static async Task ScrollIntoViewAsync(this ListViewBase listViewBase, object item, ScrollIntoViewAlignment alignment, bool updateLayout)
        {
            var tcs = new TaskCompletionSource<bool>();
            var scrollViewer = listViewBase.GetScrollViewer();

            void layoutUpdated(object s1, object e1)
            {
                tcs.TrySetResult(true);
            }

            void viewChanged(object s, ScrollViewerViewChangedEventArgs e)
            {
                scrollViewer.LayoutUpdated += layoutUpdated;

                if (updateLayout)
                {
                    scrollViewer.UpdateLayout();
                }
            }
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                listViewBase.ScrollIntoView(item, alignment);
                await tcs.Task;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
                scrollViewer.LayoutUpdated -= layoutUpdated;
            }
        }

        public static async Task ChangeViewAsync(this ScrollViewer scrollViewer, double? horizontalOffset, double? verticalOffset, bool disableAnimation, bool updateLayout)
        {
            var tcs = new TaskCompletionSource<bool>();

            void layoutUpdated(object s1, object e1)
            {
                tcs.TrySetResult(true);
            }

            void viewChanged(object s, ScrollViewerViewChangedEventArgs e)
            {
                if (e.IsIntermediate)
                {
                    return;
                }

                scrollViewer.LayoutUpdated += layoutUpdated;

                if (updateLayout)
                {
                    scrollViewer.UpdateLayout();
                }
            }
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                if (scrollViewer.ChangeView(horizontalOffset, verticalOffset, null, disableAnimation))
                {
                    await tcs.Task;
                }
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
                scrollViewer.LayoutUpdated -= layoutUpdated;
            }
        }

        public static ScrollViewer GetScrollViewer(this ListViewBase listViewBase)
        {
            //if (listViewBase is ChatsListView bubble)
            //{
            //    return bubble.ScrollingHost;
            //}

            return listViewBase.Descendants<ScrollViewer>().FirstOrDefault();
        }

        public static ScrollViewer GetScrollViewer(this Pivot listViewBase)
        {
            return listViewBase.Descendants<ScrollViewer>().FirstOrDefault();
        }

        public static async Task ConsolidateAsync(this ApplicationView view)
        {
            if (await view.TryConsolidateAsync())
            {
                return;
            }

            Window.Current.Close();
        }
    }

    public static class UriEx
    {
        public static BitmapImage ToBitmap(string path, int width, int height)
        {
            return new BitmapImage(ToLocal(path)) { DecodePixelWidth = width, DecodePixelHeight = height, DecodePixelType = DecodePixelType.Logical };
        }

        public static Uri ToLocal(string path)
        {
            //return new Uri("file:///" + Uri.EscapeUriString(path.Replace('\\', '/')));

            var directory = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);

            return new Uri("file:///" + directory + "\\" + Uri.EscapeUriString(file));
        }
    }
}
