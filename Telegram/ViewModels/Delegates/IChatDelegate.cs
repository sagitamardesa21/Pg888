//
// Copyright Fela Ameghino 2015-2024
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Telegram.Td.Api;

namespace Telegram.ViewModels.Delegates
{
    public interface IChatDelegate : IViewModelDelegate
    {
        void UpdateChat(Chat chat);
        void UpdateChatTitle(Chat chat);
        void UpdateChatPhoto(Chat chat);
    }
}
