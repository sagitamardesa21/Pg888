﻿using Telegram.Td.Api;

namespace Unigram.ViewModels.Delegates
{
    public interface IDialogDelegate : IProfileDelegate
    {
        void UpdateChatReplyMarkup(Chat chat, MessageViewModel message);
        void UpdateChatUnreadMentionCount(Chat chat, int unreadMentionCount);
        void UpdateChatDefaultDisableNotification(Chat chat, bool defaultDisableNotification);

        void UpdatePinnedMessage(Chat chat, MessageViewModel message, bool loading);

        void UpdateNotificationSettings(Chat chat);



        void PlayMessage(MessageViewModel message);
    }
}
