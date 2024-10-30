﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL;
using Unigram.Common;
using Unigram.Services;
using Unigram.Views.Chats;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using ChatCreateStep2Tuple = Telegram.Api.TL.TLTuple<string, object>;

namespace Unigram.ViewModels.Chats
{
    public class ChatCreateStep1ViewModel : UnigramViewModelBase
    {
        private bool _uploadingPhoto;
        private Action _uploadingCallback;

        public ChatCreateStep1ViewModel(IProtoService protoService, ICacheService cacheService, IEventAggregator aggregator)
            : base(protoService, cacheService, aggregator)
        {
            SendCommand = new RelayCommand(SendExecute, () => !string.IsNullOrWhiteSpace(Title));
            EditPhotoCommand = new RelayCommand<StorageFile>(EditPhotoExecute);
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                Set(ref _title, value);
                SendCommand.RaiseCanExecuteChanged();
            }
        }

        private BitmapImage _preview;
        public BitmapImage Preview
        {
            get
            {
                return _preview;
            }
            set
            {
                Set(ref _preview, value);
            }
        }

        public RelayCommand SendCommand { get; }
        private void SendExecute()
        {
            {
                NavigationService.Navigate(typeof(ChatCreateStep2Page), new ChatCreateStep2Tuple(_title, null));
            }
        }

        public RelayCommand<StorageFile> EditPhotoCommand { get; }
        private async void EditPhotoExecute(StorageFile file)
        {
            _uploadingPhoto = true;
        }

        private void ContinueUploadingPhoto()
        {
            NavigationService.Navigate(typeof(ChatCreateStep2Page), new ChatCreateStep2Tuple(_title, null));
        }
    }
}
