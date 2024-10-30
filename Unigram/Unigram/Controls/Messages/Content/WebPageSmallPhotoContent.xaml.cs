﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Selectors;
using Unigram.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Unigram.Controls.Messages.Content
{
    public sealed partial class WebPageSmallPhotoContent : WebPageContentBase, IContentWithFile
    {
        private MessageViewModel _message;

        public WebPageSmallPhotoContent(MessageViewModel message)
        {
            InitializeComponent();
            UpdateMessage(message);
        }

        public void UpdateMessage(MessageViewModel message)
        {
            _message = message;

            var text = message.Content as MessageText;
            if (text == null)
            {
                return;
            }

            var webPage = text.WebPage;
            if (webPage == null)
            {
                return;
            }

            Texture.Source = null;

            var small = webPage.Photo?.GetSmall();
            if (small != null)
            {
                UpdateFile(message, small.Photo);
            }

            UpdateWebPage(webPage, Label, TitleLabel, SubtitleLabel, ContentLabel);
            UpdateInstantView(webPage, Button, Run1, Run2, Run3);
        }

        public void UpdateMessageContentOpened(MessageViewModel message) { }

        public void UpdateFile(MessageViewModel message, File file)
        {
            var text = message.Content as MessageText;
            if (text == null)
            {
                return;
            }

            var webPage = text.WebPage;
            if (webPage == null)
            {
                return;
            }

            var small = webPage.Photo?.GetSmall();
            if (small == null)
            {
                return;
            }

            if (small.Photo.Id != file.Id)
            {
                return;
            }

            if (file.Local.IsDownloadingCompleted)
            {
                Texture.Source = new BitmapImage(new Uri("file:///" + file.Local.Path));
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                message.ProtoService.Send(new DownloadFile(file.Id, 1));
            }
        }

        public bool IsValid(MessageContent content, bool primary)
        {
            return content is MessageText text && text.WebPage != null && text.WebPage.IsSmallPhoto();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var text = _message.Content as MessageText;
            if (text == null)
            {
                return;
            }

            var webPage = text.WebPage;
            if (webPage == null)
            {
                return;
            }

            _message?.Delegate?.OpenWebPage(webPage);
        }
    }
}
