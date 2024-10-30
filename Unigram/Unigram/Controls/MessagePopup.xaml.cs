﻿using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls
{
    public sealed partial class MessagePopup : ContentPopup
    {
        public MessagePopup()
        {
            InitializeComponent();
        }

        public MessagePopup(string message)
            : this(message, null)
        {

        }

        public MessagePopup(string message, string title)
        {
            InitializeComponent();

            Message = message;
            Title = title;
            PrimaryButtonText = "OK";
        }

        public string Message
        {
            get
            {
                return TextBlockHelper.GetMarkdown(MessageLabel);
            }
            set
            {
                TextBlockHelper.SetMarkdown(MessageLabel, value);
            }
        }

        public FormattedText FormattedMessage
        {
            get
            {
                return TextBlockHelper.GetFormattedText(MessageLabel);
            }
            set
            {
                TextBlockHelper.SetFormattedText(MessageLabel, value);
            }
        }

        public string CheckBoxLabel
        {
            get
            {
                return CheckBox.Content.ToString();
            }
            set
            {
                CheckBox.Content = value;
                CheckBox.Visibility = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool? IsChecked
        {
            get
            {
                return CheckBox.IsChecked;
            }
            set
            {
                CheckBox.IsChecked = value;
            }
        }

        public static Task<ContentDialogResult> ShowAsync(string message, string title = null, string primary = null, string secondary = null)
        {
            var dialog = new MessagePopup();
            dialog.Title = title;
            dialog.Message = message;
            dialog.PrimaryButtonText = primary ?? string.Empty;
            dialog.SecondaryButtonText = secondary ?? string.Empty;

            return dialog.ShowQueuedAsync();
        }

        public static Task<ContentDialogResult> ShowAsync(FormattedText message, string title = null, string primary = null, string secondary = null)
        {
            var dialog = new MessagePopup();
            dialog.Title = title;
            dialog.FormattedMessage = message;
            dialog.PrimaryButtonText = primary ?? string.Empty;
            dialog.SecondaryButtonText = secondary ?? string.Empty;

            return dialog.ShowQueuedAsync();
        }
    }
}
