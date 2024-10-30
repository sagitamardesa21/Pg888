﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unigram.Core.Unidecode;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Unigram.Common
{
    public static class Extensions
    {
        public static Dictionary<string, string> ParseQueryString(this string query)
        {
            var first = query.Split('?');
            if (first.Length > 1)
            {
                query = first.Last();
            }

            var queryDict = new Dictionary<string, string>();
            foreach (var token in query.TrimStart(new char[] { '?' }).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    queryDict[parts[0].Trim()] = WebUtility.UrlDecode(parts[1]).Trim();
                else
                    queryDict[parts[0].Trim()] = "";
            }
            return queryDict;
        }

        public static string GetParameter(this Dictionary<string, string> query, string key)
        {
            query.TryGetValue(key, out string value);
            return value;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool IsLike(this string source, string query, StringComparison comp)
        {
            var result = query.Split(' ').All(x =>
            {
                var index = source.IndexOf(x, comp);
                if (index > -1)
                {
                    return index == 0 || char.IsSeparator(source[index - 1]) || !char.IsLetterOrDigit(source[index - 1]);
                }

                return false;
            });

            source = Unidecoder.Unidecode(source);
            return result || query.Split(' ').All(x =>
            {
                var index = source.IndexOf(x, comp);
                if (index > -1)
                {
                    return index == 0 || char.IsSeparator(source[index - 1]) || !char.IsLetterOrDigit(source[index - 1]);
                }

                return false;
            });
        }

        public static string TrimEnd(this string input, string suffixToRemove)
        {
            if (input != null && suffixToRemove != null && input.EndsWith(suffixToRemove))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }
            else return input;
        }

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
                result.Add(default(T));
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
    }
}
