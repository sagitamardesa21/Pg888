﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Template10.Common;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Controls.Messages;
using Unigram.Converters;
using Unigram.Native.Tasks;
using Unigram.Views;
using Windows.Data.Json;
using Windows.Networking.PushNotifications;
using Windows.System.Threading;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

namespace Unigram.Services
{
    public interface INotificationsService
    {
        Task RegisterAsync();
        Task UnregisterAsync();
        Task CloseAsync();

        Task ProcessAsync(Dictionary<string, string> data);
    }

    public class NotificationsService : INotificationsService,
        IHandle<UpdateUnreadMessageCount>,
        IHandle<UpdateNewMessage>,
        IHandle<UpdateChatReadInbox>,
        IHandle<UpdateServiceNotification>,
        IHandle<UpdateTermsOfService>,
        IHandle<UpdateAuthorizationState>
    {
        private readonly IProtoService _protoService;
        private readonly ICacheService _cacheService;
        private readonly ISessionService _sessionService;
        private readonly ISettingsService _settings;
        private readonly IEventAggregator _aggregator;

        private readonly DisposableMutex _registrationLock;
        private bool _alreadyRegistered;

        public NotificationsService(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, ISessionService sessionService, IEventAggregator aggregator)
        {
            _protoService = protoService;
            _cacheService = cacheService;
            _settings = settingsService;
            _sessionService = sessionService;
            _aggregator = aggregator;

            _registrationLock = new DisposableMutex();

            _aggregator.Subscribe(this);

            Handle(new UpdateUnreadMessageCount(protoService.UnreadCount, protoService.UnreadUnmutedCount));
        }

        public async void Handle(UpdateTermsOfService update)
        {
            var terms = update.TermsOfService;
            if (terms == null)
            {
                return;
            }

            async void DeleteAccount()
            {
                var decline = await TLMessageDialog.ShowAsync(Strings.Resources.TosUpdateDecline, Strings.Resources.TermsOfService, Strings.Resources.DeclineDeactivate, Strings.Resources.Back);
                if (decline != ContentDialogResult.Primary)
                {
                    Handle(update);
                    return;
                }

                var delete = await TLMessageDialog.ShowAsync(Strings.Resources.TosDeclineDeleteAccount, Strings.Resources.AppName, Strings.Resources.Deactivate, Strings.Resources.Cancel);
                if (delete != ContentDialogResult.Primary)
                {
                    Handle(update);
                    return;
                }

                _protoService.Send(new DeleteAccount("Decline ToS update"));
            }

            if (terms.ShowPopup)
            {
                await Task.Delay(2000);
                await WindowContext.Default().Dispatcher.Dispatch(async () =>
                {
                    var confirm = await TLMessageDialog.ShowAsync(terms.Text, Strings.Resources.PrivacyPolicyAndTerms, Strings.Resources.Agree, Strings.Resources.Cancel);
                    if (confirm != ContentDialogResult.Primary)
                    {
                        DeleteAccount();
                        return;
                    }

                    if (terms.MinUserAge > 0)
                    {
                        var age = await TLMessageDialog.ShowAsync(string.Format(Strings.Resources.TosAgeText, terms.MinUserAge), Strings.Resources.TosAgeTitle, Strings.Resources.Agree, Strings.Resources.Cancel);
                        if (age != ContentDialogResult.Primary)
                        {
                            DeleteAccount();
                            return;
                        }
                    }

                    _protoService.Send(new AcceptTermsOfService(update.TermsOfServiceId));
                });
            }
        }

        public void Handle(UpdateServiceNotification update)
        {
            var caption = update.Content.GetCaption();
            if (caption == null)
            {
                return;
            }

            var text = caption.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            WindowContext.Default().Dispatcher.Dispatch(async () =>
            {
                if (update.Type.StartsWith("AUTH_KEY_DROP_"))
                {
                    var confirm = await TLMessageDialog.ShowAsync(text, Strings.Resources.AppName, Strings.Resources.LogOut, Strings.Resources.Cancel);
                    if (confirm == ContentDialogResult.Primary)
                    {
                        _protoService.Send(new Destroy());
                    }
                }
                else
                {
                    await TLMessageDialog.ShowAsync(text, Strings.Resources.AppName, Strings.Resources.OK);
                }
            });
        }

        public void Handle(UpdateChatReadInbox update)
        {
            if (update.UnreadCount == 0)
            {
                var chat = _cacheService.GetChat(update.ChatId);
                if (chat == null)
                {
                    return;
                }

                ToastNotificationManager.History.RemoveGroup(GetGroup(chat), "App");
            }
        }

        public void Handle(UpdateUnreadMessageCount update)
        {
            if (_settings.Notifications.IncludeMutedChats)
            {
                NotificationTask.UpdatePrimaryBadge(update.UnreadCount);
            }
            else
            {
                NotificationTask.UpdatePrimaryBadge(update.UnreadUnmutedCount);
            }
        }

        public void Handle(UpdateNewMessage update)
        {
            if (update.DisableNotification || !_settings.Notifications.InAppPreview)
            {
                return;
            }

            var difference = DateTime.Now.ToTimestamp() - update.Message.Date;
            if (difference > 180)
            {
                return;
            }

            // Adding some delay to be 110% the message hasn't been read already
            ThreadPoolTimer.CreateTimer(timer =>
            {
                var chat = _protoService.GetChat(update.Message.ChatId);
                if (chat == null || chat.LastReadInboxMessageId >= update.Message.Id)
                {
                    return;
                }

                var caption = GetCaption(chat);
                var content = GetContent(chat, update.Message);
                var sound = "";
                var launch = GetLaunch(chat);
                var tag = GetTag(update.Message);
                var group = GetGroup(chat);
                var picture = GetPhoto(chat);
                var date = BindConvert.Current.DateTime(update.Message.Date).ToString("o");
                var loc_key = chat.Type is ChatTypeSupergroup super && super.IsChannel ? "CHANNEL" : string.Empty;

                var user = _protoService.GetUser(_protoService.GetMyId());

                Update(chat, () =>
                {
                    NotificationTask.UpdateToast(caption, content, user?.GetFullName() ?? string.Empty, user?.Id.ToString() ?? string.Empty, sound, launch, tag, group, picture, date, loc_key);
                    NotificationTask.UpdatePrimaryTile($"{_protoService.SessionId}", caption, content, picture);
                });
            }, TimeSpan.FromSeconds(3));
        }

        private void Update(Chat chat, Action action)
        {
            Execute.BeginOnUIThread(() =>
            {
                if (_sessionService.IsActive)
                {
                    var service = WindowContext.GetForCurrentView().NavigationServices.GetByFrameId("Main" + _protoService.SessionId);
                    if (service == null)
                    {
                        return;
                    }

                    if (TLWindowContext.GetForCurrentView().ActivationState != Windows.UI.Core.CoreWindowActivationState.Deactivated && service.CurrentPageType == typeof(ChatPage) && (long)service.CurrentPageParam == chat.Id)
                    {
                        return;
                    }
                }

                action();
            });
        }

        private string GetTag(Message message)
        {
            return $"{message.Id >> 20}";
        }

        private string GetGroup(Chat chat)
        {
            var group = string.Empty;
            if (chat.Type is ChatTypePrivate privata)
            {
                group = "u" + privata.UserId;
            }
            else if (chat.Type is ChatTypeSecret secret)
            {
                group = "s" + secret.SecretChatId;
            }
            else if (chat.Type is ChatTypeSupergroup supergroup)
            {
                group = "c" + supergroup.SupergroupId;
            }
            else if (chat.Type is ChatTypeBasicGroup basicGroup)
            {
                group = "c" + basicGroup.BasicGroupId;
            }

            return group;
        }

        public string GetLaunch(Chat chat)
        {
            var launch = string.Empty;
            if (chat.Type is ChatTypePrivate privata)
            {
                launch += string.Format(CultureInfo.InvariantCulture, "from_id={0}", privata.UserId);
            }
            else if (chat.Type is ChatTypeSecret secret)
            {
                launch += string.Format(CultureInfo.InvariantCulture, "secret_id={0}", secret.SecretChatId);
            }
            else if (chat.Type is ChatTypeSupergroup supergroup)
            {
                launch += string.Format(CultureInfo.InvariantCulture, "channel_id={0}", supergroup.SupergroupId);
            }
            else if (chat.Type is ChatTypeBasicGroup basicGroup)
            {
                launch += string.Format(CultureInfo.InvariantCulture, "chat_id={0}", basicGroup.BasicGroupId);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}&amp;session={1}", launch, _protoService.SessionId);
        }

        public async Task RegisterAsync()
        {
            using (await _registrationLock.WaitAsync())
            {
                var userId = _protoService.GetMyId();
                if (userId == 0)
                {
                    return;
                }

                if (_alreadyRegistered) return;
                _alreadyRegistered = true;

                try
                {
                    var oldUri = _settings.NotificationsToken;
                    var ids = _settings.NotificationsIds.ToList();
                    var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                    if (channel.Uri != oldUri || !ids.Contains(userId))
                    {
                        ids.Remove(userId);

                        var result = await _protoService.SendAsync(new RegisterDevice(new DeviceTokenWindowsPush(channel.Uri), ids));
                        if (result is Ok)
                        {
                            ids.Add(userId);
                            _settings.NotificationsIds = ids.ToArray();
                            _settings.NotificationsToken = channel.Uri;
                        }
                        else
                        {
                            _settings.NotificationsToken = null;
                        }
                    }

                    channel.PushNotificationReceived += OnPushNotificationReceived;
                }
                catch (Exception ex)
                {
                    _alreadyRegistered = false;
                    _settings.NotificationsToken = null;

                    Debugger.Break();
                }
            }
        }

        private void OnPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            if (args.NotificationType == PushNotificationType.Raw)
            {
                args.Cancel = true;
                return;

                if (JsonValue.TryParse(args.RawNotification.Content, out JsonValue node))
                {
                    var notification = node.GetObject();
                    var data = notification.GetNamedObject("data");

                    if (data.ContainsKey("loc_key"))
                    {
                        var muted = data.GetNamedString("mute", "0") == "1";
                        if (muted)
                        {
                            return;
                        }

                        var custom = data.GetNamedObject("custom", null);
                        if (custom == null)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public async Task UnregisterAsync()
        {
            var channel = _settings.NotificationsToken;
            //var response = await _protoService.UnregisterDeviceAsync(8, channel);
            //if (response.IsSucceeded)
            //{
            //}

            _settings.NotificationsToken = null;
        }

        public async Task CloseAsync()
        {
            try
            {
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                channel.Close();
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private TaskCompletionSource<AuthorizationState> _authorizationStateTask = new TaskCompletionSource<AuthorizationState>();

        public void Handle(UpdateAuthorizationState update)
        {
            switch (update.AuthorizationState)
            {
                case AuthorizationStateWaitTdlibParameters waitTdlibParameters:
                case AuthorizationStateWaitEncryptionKey waitEncryptionKey:
                    break;
                default:
                    _authorizationStateTask.TrySetResult(update.AuthorizationState);
                    break;
            }
        }

        public async Task ProcessAsync(Dictionary<string, string> data)
        {
            var state = _protoService.GetAuthorizationState();
            if (!(state is AuthorizationStateReady))
            {
                state = await _authorizationStateTask.Task;
            }

            if (!(state is AuthorizationStateReady))
            {
                return;
            }

            if (data.TryGetValue("QuickMessage", out string text))
            {
                var messageText = text.Replace("\r\n", "\n").Replace('\v', '\n').Replace('\r', '\n');
                var entities = Markdown.Parse(_protoService, ref messageText);

                var replyToMsgId = data.ContainsKey("msg_id") ? int.Parse(data["msg_id"]) >> 0 : 0;
                var chat = default(Chat);

                if (data.TryGetValue("from_id", out string from_id) && int.TryParse(from_id, out int fromId))
                {
                    chat = await _protoService.SendAsync(new CreatePrivateChat(fromId, false)) as Chat;
                }
                else if (data.TryGetValue("channel_id", out string channel_id) && int.TryParse(channel_id, out int channelId))
                {
                    chat = await _protoService.SendAsync(new CreateSupergroupChat(channelId, false)) as Chat;
                }
                else if (data.TryGetValue("chat_id", out string chat_id) && int.TryParse(chat_id, out int chatId))
                {
                    chat = await _protoService.SendAsync(new CreateBasicGroupChat(chatId, false)) as Chat;
                }

                if (chat == null)
                {
                    return;
                }

                var response = await _protoService.SendAsync(new SendMessage(chat.Id, replyToMsgId, false, true, null, new InputMessageText(new FormattedText(messageText, entities), false, false)));
            }
        }

















        private string GetCaption(Chat chat)
        {
            if (chat.Type is ChatTypeSecret)
            {
                return Strings.Resources.AppName;
            }

            return _protoService.GetTitle(chat);
        }

        private string GetContent(Chat chat, Message message)
        {
            if (chat.Type is ChatTypeSecret)
            {
                return Strings.Resources.YouHaveNewMessage;
            }

            return UpdateFromLabel(chat, message) + GetBriefLabel(chat, message);
        }

        private string GetPhoto(Chat chat)
        {
            if (chat.Photo != null && chat.Photo.Small.Local.IsDownloadingCompleted)
            {
                return "ms-appdata:///local/0/profile_photos/" + Path.GetFileName(chat.Photo.Small.Local.Path);
            }

            return string.Empty;
        }



        private string GetBriefLabel(Chat chat, Message value)
        {
            switch (value.Content)
            {
                case MessageAnimation animation:
                    return animation.Caption.Text;
                case MessageAudio audio:
                    return audio.Caption.Text;
                case MessageDocument document:
                    return document.Caption.Text;
                case MessagePhoto photo:
                    return photo.Caption.Text;
                case MessageVideo video:
                    return video.Caption.Text;
                case MessageVoiceNote voiceNote:
                    return voiceNote.Caption.Text;

                case MessageText text:
                    return text.Text.Text;
            }

            return string.Empty;
        }

        private string UpdateFromLabel(Chat chat, Message message)
        {
            if (message.IsService())
            {
                return MessageService.GetText(new ViewModels.MessageViewModel(_protoService, _aggregator, null, message));
            }

            var result = string.Empty;

            if (ShowFrom(chat, message))
            {
                var from = _protoService.GetUser(message.SenderUserId);
                if (from != null)
                {
                    if (!string.IsNullOrEmpty(from.FirstName))
                    {
                        result = $"{from.FirstName.Trim()}: ";
                    }
                    else if (!string.IsNullOrEmpty(from.LastName))
                    {
                        result = $"{from.LastName.Trim()}: ";
                    }
                    else if (!string.IsNullOrEmpty(from.Username))
                    {
                        result = $"{from.Username.Trim()}: ";
                    }
                    else if (from.Type is UserTypeDeleted)
                    {
                        result = $"{Strings.Resources.HiddenName}: ";
                    }
                    else
                    {
                        result = $"{from.Id}: ";
                    }
                }
            }

            if (message.Content is MessageGame gameMedia)
            {
                return result + "\uD83C\uDFAE " + gameMedia.Game.Title;
            }
            if (message.Content is MessageExpiredVideo)
            {
                return result + Strings.Resources.AttachVideoExpired;
            }
            else if (message.Content is MessageExpiredPhoto)
            {
                return result + Strings.Resources.AttachPhotoExpired;
            }
            else if (message.Content is MessageVideoNote)
            {
                return result + Strings.Resources.AttachRound;
            }
            else if (message.Content is MessageSticker sticker)
            {
                if (string.IsNullOrEmpty(sticker.Sticker.Emoji))
                {
                    return result + Strings.Resources.AttachSticker;
                }

                return result + $"{sticker.Sticker.Emoji} {Strings.Resources.AttachSticker}";
            }

            string GetCaption(string caption)
            {
                return string.IsNullOrEmpty(caption) ? string.Empty : ", ";
            }

            if (message.Content is MessageVoiceNote voiceNote)
            {
                return result + Strings.Resources.AttachAudio + GetCaption(voiceNote.Caption.Text);
            }
            else if (message.Content is MessageVideo video)
            {
                return result + Strings.Resources.AttachVideo + GetCaption(video.Caption.Text);
            }
            else if (message.Content is MessageAnimation animation)
            {
                return result + Strings.Resources.AttachGif + GetCaption(animation.Caption.Text);
            }
            else if (message.Content is MessageAudio audio)
            {
                var performer = string.IsNullOrEmpty(audio.Audio.Performer) ? null : audio.Audio.Performer;
                var title = string.IsNullOrEmpty(audio.Audio.Title) ? null : audio.Audio.Title;

                if (performer == null && title == null)
                {
                    return result + Strings.Resources.AttachMusic + GetCaption(audio.Caption.Text);
                }
                else
                {
                    return $"{result}{performer ?? Strings.Resources.AudioUnknownArtist} - {title ?? Strings.Resources.AudioUnknownTitle}" + GetCaption(audio.Caption.Text);
                }
            }
            else if (message.Content is MessageDocument document)
            {
                if (string.IsNullOrEmpty(document.Document.FileName))
                {
                    return result + Strings.Resources.AttachDocument + GetCaption(document.Caption.Text);
                }

                return result + document.Document.FileName + GetCaption(document.Caption.Text);
            }
            else if (message.Content is MessageInvoice invoice)
            {
                return result + invoice.Title;
            }
            else if (message.Content is MessageContact)
            {
                return result + Strings.Resources.AttachContact;
            }
            else if (message.Content is MessageLocation location)
            {
                return result + (location.LivePeriod > 0 ? Strings.Resources.AttachLiveLocation : Strings.Resources.AttachLocation);
            }
            else if (message.Content is MessageVenue vanue)
            {
                return result + Strings.Resources.AttachLocation;
            }
            else if (message.Content is MessagePhoto photo)
            {
                if (string.IsNullOrEmpty(photo.Caption.Text))
                {
                    return result + Strings.Resources.AttachPhoto;
                }

                return result + $"{Strings.Resources.AttachPhoto}, ";
            }
            else if (message.Content is MessageCall call)
            {
                var outgoing = message.IsOutgoing;
                var missed = call.DiscardReason is CallDiscardReasonMissed || call.DiscardReason is CallDiscardReasonDeclined;

                return result + (missed ? (outgoing ? Strings.Resources.CallMessageOutgoingMissed : Strings.Resources.CallMessageIncomingMissed) : (outgoing ? Strings.Resources.CallMessageOutgoing : Strings.Resources.CallMessageIncoming));
            }
            else if (message.Content is MessageUnsupported)
            {
                return result + Strings.Resources.UnsupportedAttachment;
            }

            return result;
        }

        private bool ShowFrom(Chat chat, Message message)
        {
            if (message.IsService())
            {
                return false;
            }

            if (message.IsOutgoing)
            {
                return true;
            }

            if (chat.Type is ChatTypeBasicGroup)
            {
                return true;
            }

            if (chat.Type is ChatTypeSupergroup supergroup)
            {
                return !supergroup.IsChannel;
            }

            return false;
        }
    }
}
