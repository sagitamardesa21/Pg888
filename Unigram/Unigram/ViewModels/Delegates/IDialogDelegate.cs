﻿using Microsoft.UI.Xaml;
using System.Collections.Generic;
using Telegram.Td.Api;
using Unigram.Controls.Chats;
using Unigram.ViewModels.Chats;

namespace Unigram.ViewModels.Delegates
{
    public interface IDialogDelegate : IProfileDelegate
    {
        void UpdateChatActions(Chat chat, IDictionary<long, ChatAction> actions);

        void UpdateChatTheme(Chat chat);
        void UpdateChatPermissions(Chat chat);
        void UpdateChatActionBar(Chat chat);
        void UpdateChatHasScheduledMessages(Chat chat);
        void UpdateChatReplyMarkup(Chat chat, MessageViewModel message);
        void UpdateChatUnreadMentionCount(Chat chat, int unreadMentionCount);
        void UpdateChatDefaultDisableNotification(Chat chat, bool defaultDisableNotification);

        void UpdatePinnedMessage();
        void UpdatePinnedMessage(Chat chat, bool known);
        void UpdateCallbackQueryAnswer(Chat chat, MessageViewModel answer);

        void UpdateComposerHeader(Chat chat, MessageComposerHeader header);
        void UpdateSearchMask(Chat chat, ChatSearchViewModel search);

        void UpdateAutocomplete(Chat chat, IAutocompleteCollection collection);

        void UpdateGroupCall(Chat chat, GroupCall groupCall);



        void PlayMessage(MessageViewModel message, FrameworkElement target);

        void ViewVisibleMessages(bool intermediate);


        void HideStickers();
    }
}
