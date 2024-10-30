﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Td.Api;
using Unigram.Common;
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
    public sealed partial class InvoicePhotoContent : StackPanel, IContentWithFile
    {
        private MessageViewModel _message;
        public MessageViewModel Message => _message;

        public InvoicePhotoContent(MessageViewModel message)
        {
            InitializeComponent();
            UpdateMessage(message);
        }

        public void UpdateMessage(MessageViewModel message)
        {
            _message = message;

            var invoice = message.Content as MessageInvoice;
            if (invoice == null)
            {
                return;
            }

            Title.Text = invoice.Title;
            Description.Text = invoice.Description;

            Photo.Constraint = invoice.Photo;
            Texture.Source = null;

            Footer.UpdateMessage(message);

            var small = invoice.Photo.GetSmall();
            if (small != null)
            {
                UpdateThumbnail(message, small.Photo);
            }
        }

        public void UpdateMessageContentOpened(MessageViewModel message) { }

        public void UpdateFile(MessageViewModel message, File file)
        {
            var invoice = message.Content as MessageInvoice;
            if (invoice == null)
            {
                return;
            }

            var small = invoice.Photo.GetSmall();
            if (small != null && small.Photo.Id == file.Id)
            {
                UpdateThumbnail(message, file);
            }
        }

        private void UpdateThumbnail(MessageViewModel message, File file)
        {
            if (file.Local.IsDownloadingCompleted)
            {
                Texture.Source = new BitmapImage(new Uri("file:///" + file.Local.Path));
            }
            else if (file.Local.CanBeDownloaded && !file.Local.IsDownloadingActive)
            {
                message.ProtoService.DownloadFile(file.Id, 1);
            }
        }

        public bool IsValid(MessageContent content, bool primary)
        {
            return content is MessageInvoice invoice && invoice.Photo != null;
        }
    }
}
