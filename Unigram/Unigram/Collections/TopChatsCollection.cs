//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Td.Api;
using Unigram.Services;
using Windows.Foundation;

namespace Unigram.Collections
{
    public class TopChatsCollection : MvxObservableCollection<Chat>, ISupportIncrementalLoading
    {
        private readonly IClientService _clientService;
        private readonly TopChatCategory _category;
        private readonly int _limit;

        private readonly bool _hasMore = false;

        public TopChatsCollection(IClientService clientService, TopChatCategory category, int limit)
        {
            _clientService = clientService;
            _category = category;
            _limit = limit;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async token =>
            {
                count = 0;

                var response = await _clientService.SendAsync(new GetTopChats(_category, _limit));
                if (response is Chats chats)
                {
                    foreach (var id in chats.ChatIds)
                    {
                        var chat = _clientService.GetChat(id);
                        if (chat != null)
                        {
                            Add(chat);
                            count++;
                        }
                    }
                }

                return new LoadMoreItemsResult { Count = count };
            });
        }

        public bool HasMoreItems => _hasMore;
    }
}
