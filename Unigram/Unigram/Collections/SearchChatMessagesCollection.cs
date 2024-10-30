﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Services;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Unigram.Collections
{
    public class SearchChatMessagesCollection : MvxObservableCollection<Message>, ISupportIncrementalLoading
    {
        private readonly IProtoService _protoService;

        private readonly long _chatId;
        private readonly string _query;
        private readonly int _senderUserId;
        private readonly long _fromMessageId;

        private readonly SearchMessagesFilter _filter;

        public SearchChatMessagesCollection(IProtoService protoService, long chatId, string query, int senderUserId, long fromMessageId, SearchMessagesFilter filter)
        {
            _protoService = protoService;

            _chatId = chatId;
            _query = query;
            _senderUserId = senderUserId;
            _fromMessageId = fromMessageId;
            _filter = filter;
        }



        public int TotalCount { get; private set; }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async token =>
            {
                var fromMessageId = _fromMessageId;
                var offset = -49;

                var last = this.LastOrDefault();
                if (last != null)
                {
                    fromMessageId = last.Id;
                    offset = 0; 
                }

                var response = await _protoService.SendAsync(new SearchChatMessages(_chatId, _query, _senderUserId, fromMessageId, offset, (int)count, _filter));
                if (response is Messages messages)
                {
                    TotalCount = messages.TotalCount;
                    AddRange(messages.MessagesValue);

                    return new LoadMoreItemsResult { Count = (uint)messages.MessagesValue.Count };
                }

                return new LoadMoreItemsResult { Count = 0 };
            });
        }

        public bool HasMoreItems => throw new NotImplementedException();
    }
}
