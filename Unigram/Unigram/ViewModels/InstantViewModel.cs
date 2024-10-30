﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Unigram.Common;
using Unigram.Controls.Views;
using Unigram.Views;

namespace Unigram.ViewModels
{
    public class InstantViewModel : UnigramViewModelBase
    {
        public InstantViewModel(IMTProtoService protoService, ICacheService cacheService, ITelegramEventAggregator aggregator) 
            : base(protoService, cacheService, aggregator)
        {
            _gallery = new InstantGalleryViewModel();
        }

        public Uri ShareLink { get; set; }
        public string ShareTitle { get; set; }

        private InstantGalleryViewModel _gallery;
        public InstantGalleryViewModel Gallery
        {
            get
            {
                return _gallery;
            }
            set
            {
                Set(ref _gallery, value);
            }
        }

        public RelayCommand<TLChannel> ChannelOpenCommand => new RelayCommand<TLChannel>(ChannelOpenExecute);
        private void ChannelOpenExecute(TLChannel channel)
        {
            if (channel != null)
            {
                NavigationService.NavigateToDialog(channel);
            }
        }

        public RelayCommand<TLChannel> ChannelJoinCommand => new RelayCommand<TLChannel>(ChannelJoinExecute);
        private async void ChannelJoinExecute(TLChannel channel)
        {
            if (channel != null && channel.IsLeft)
            {
                var response = await ProtoService.JoinChannelAsync(channel);
                if (response.IsSucceeded)
                {
                    channel.RaisePropertyChanged(() => channel.IsLeft);
                }
            }
        }

        public RelayCommand ShareCommand => new RelayCommand(ShareExecute);
        private async void ShareExecute()
        {
            if (ShareLink != null)
            {
                await ShareView.Current.ShowAsync(ShareLink, ShareTitle);
            }
        }

        public RelayCommand FeedbackCommand => new RelayCommand(FeedbackExecute);
        private async void FeedbackExecute()
        {
            var user = CacheService.GetUser("previews");
            if (user == null)
            {
                var response = await ProtoService.ResolveUsernameAsync("previews");
                if (response.IsSucceeded)
                {
                    user = response.Result.Users.FirstOrDefault();
                }
            }

            if (user != null)
            {
                NavigationService.NavigateToDialog(user);
            }
        }
    }
}
