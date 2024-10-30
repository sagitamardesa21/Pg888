﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Template10.Utils;
using Unigram.Collections;
using Unigram.Common;
using Unigram.Converters;
using Unigram.Views;
using Unigram.Views.Channels;
using Unigram.Views.Chats;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Channels
{
    public class ChannelDetailsViewModel : ChannelParticipantsViewModelBase, IHandle<TLUpdateChannel>, IHandle<TLUpdateNotifySettings>
    {
        private readonly IUploadFileManager _uploadFileManager;

        public ChannelDetailsViewModel(IMTProtoService protoService, ICacheService cacheService, ITelegramEventAggregator aggregator, IUploadFileManager uploadFileManager)
            : base(protoService, cacheService, aggregator, null)
        {
            _uploadFileManager = uploadFileManager;
        }

        protected TLChannelFull _full;
        public TLChannelFull Full
        {
            get
            {
                return _full;
            }
            set
            {
                Set(ref _full, value);
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            // SHOULD NOT CALL BASE!

            Item = null;
            Full = null;

            var channel = parameter as TLChannel;
            var peer = parameter as TLPeerChannel;
            if (peer != null)
            {
                channel = CacheService.GetChat(peer.ChannelId) as TLChannel;
            }

            if (channel != null)
            {
                Item = channel;

                var full = CacheService.GetFullChat(channel.Id) as TLChannelFull;
                if (full == null)
                {
                    var response = await ProtoService.GetFullChannelAsync(channel.ToInputChannel());
                    if (response.IsSucceeded)
                    {
                        full = response.Result.FullChat as TLChannelFull;
                    }
                }

                if (full != null)
                {
                    Full = full;

                    if (_item.IsMegaGroup)
                    {
                        Participants = new ItemsCollection(ProtoService, channel.ToInputChannel(), null);
                    }

                    RaisePropertyChanged(() => AreNotificationsEnabled);
                    RaisePropertyChanged(() => Participants);

                    Aggregator.Subscribe(this);
                }
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            Aggregator.Unsubscribe(this);
            return Task.CompletedTask;
        }

        public bool IsEditEnabled
        {
            get
            {
                return _item != null && (_item.IsCreator || (_item.HasAdminRights && _item.AdminRights.IsChangeInfo));
            }
        }

        public bool IsAdminLog
        {
            get
            {
                return _item != null && (_item.IsCreator || _item.HasAdminRights);
            }
        }

        public bool IsInviteUsers
        {
            get
            {
                return _item != null && (_item.IsCreator || (_item.HasAdminRights && _item.AdminRights.IsInviteUsers));
            }
        }

        public bool AreNotificationsEnabled
        {
            get
            {
                var settings = _full?.NotifySettings as TLPeerNotifySettings;
                if (settings != null)
                {
                    return settings.MuteUntil == 0;
                }

                return false;
            }
        }

        public RelayCommand<StorageFile> EditPhotoCommand => new RelayCommand<StorageFile>(EditPhotoExecute);
        private async void EditPhotoExecute(StorageFile file)
        {
            var fileLocation = new TLFileLocation
            {
                VolumeId = TLLong.Random(),
                LocalId = TLInt.Random(),
                Secret = TLLong.Random(),
                DCId = 0
            };

            var fileName = string.Format("{0}_{1}_{2}.jpg", fileLocation.VolumeId, fileLocation.LocalId, fileLocation.Secret);
            var fileCache = await FileUtils.CreateTempFileAsync(fileName);

            await file.CopyAndReplaceAsync(fileCache);
            var fileScale = fileCache;

            var basicProps = await fileScale.GetBasicPropertiesAsync();
            var imageProps = await fileScale.Properties.GetImagePropertiesAsync();

            var fileId = TLLong.Random();
            var upload = await _uploadFileManager.UploadFileAsync(fileId, fileCache.Name, false);
            if (upload != null)
            {
                var response = await ProtoService.EditPhotoAsync(_item, new TLInputChatUploadedPhoto { File = upload.ToInputFile() });
                if (response.IsSucceeded)
                {

                }
            }
        }

        public void Handle(TLUpdateChannel update)
        {
            if (_item == null)
            {
                return;
            }

            if (_item.Id == update.ChannelId)
            {
                RaisePropertyChanged(() => Item);
                RaisePropertyChanged(() => Full);
                RaisePropertyChanged(() => AreNotificationsEnabled);

                RaisePropertyChanged(() => IsInviteUsers);
                RaisePropertyChanged(() => IsEditEnabled);
                RaisePropertyChanged(() => IsAdminLog);
            }
        }

        public void Handle(TLUpdateNotifySettings update)
        {
            var notifyPeer = update.Peer as TLNotifyPeer;
            if (notifyPeer != null)
            {
                var peer = notifyPeer.Peer;
                if (peer is TLPeerChannel && peer.Id == Item.Id)
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        Full.NotifySettings = update.NotifySettings;
                        Full.RaisePropertyChanged(() => Full.NotifySettings);
                        RaisePropertyChanged(() => AreNotificationsEnabled);

                        //var notifySettings = updateNotifySettings.NotifySettings as TLPeerNotifySettings;
                        //if (notifySettings != null)
                        //{
                        //    _suppressUpdating = true;
                        //    MuteUntil = notifySettings.MuteUntil.Value;
                        //    _suppressUpdating = false;
                        //}
                    });
                }
            }
        }

        public RelayCommand EditCommand => new RelayCommand(EditExecute);
        private void EditExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelEditPage), _item.ToPeer());
            //NavigationService.Navigate(typeof(ChannelManagePage), _item.ToPeer());
        }

        public RelayCommand InviteCommand => new RelayCommand(InviteExecute);
        private void InviteExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChatInvitePage), _item.ToPeer());
        }

        public RelayCommand MediaCommand => new RelayCommand(MediaExecute);
        private void MediaExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(DialogSharedMediaPage), _item.ToInputPeer());
        }

        public RelayCommand AdminsCommand => new RelayCommand(AdminsExecute);
        private void AdminsExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelAdminsPage), _item.ToPeer());
        }

        public RelayCommand BannedCommand => new RelayCommand(BannedExecute);
        private void BannedExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelBannedPage), _item.ToPeer());
        }

        public RelayCommand KickedCommand => new RelayCommand(KickedExecute);
        private void KickedExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelKickedPage), _item.ToPeer());
        }

        public RelayCommand ParticipantsCommand => new RelayCommand(ParticipantsExecute);
        private void ParticipantsExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelParticipantsPage), _item.ToPeer());
        }

        public RelayCommand AdminLogCommand => new RelayCommand(AdminLogExecute);
        private void AdminLogExecute()
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelAdminLogPage), _item.ToPeer());
        }

        public RelayCommand ToggleMuteCommand => new RelayCommand(ToggleMuteExecute);
        private async void ToggleMuteExecute()
        {
            if (_item == null || _full == null)
            {
                return;
            }

            var notifySettings = _full.NotifySettings as TLPeerNotifySettings;
            if (notifySettings != null)
            {
                var muteUntil = notifySettings.MuteUntil == int.MaxValue ? 0 : int.MaxValue;
                var settings = new TLInputPeerNotifySettings
                {
                    MuteUntil = muteUntil,
                    IsShowPreviews = notifySettings.IsShowPreviews,
                    IsSilent = notifySettings.IsSilent,
                    Sound = notifySettings.Sound
                };

                var response = await ProtoService.UpdateNotifySettingsAsync(new TLInputNotifyPeer { Peer = _item.ToInputPeer() }, settings);
                if (response.IsSucceeded)
                {
                    notifySettings.MuteUntil = muteUntil;
                    RaisePropertyChanged(() => AreNotificationsEnabled);
                    Full.RaisePropertyChanged(() => Full.NotifySettings);

                    var dialog = CacheService.GetDialog(_item.ToPeer());
                    if (dialog != null)
                    {
                        dialog.NotifySettings = _full.NotifySettings;
                        dialog.RaisePropertyChanged(() => dialog.NotifySettings);
                        dialog.RaisePropertyChanged(() => dialog.IsMuted);
                        dialog.RaisePropertyChanged(() => dialog.Self);
                    }

                    CacheService.Commit();
                }
            }
        }

        #region Context menu

        public RelayCommand<TLChannelParticipantBase> ParticipantEditCommand => new RelayCommand<TLChannelParticipantBase>(ParticipantEditExecute);
        private void ParticipantEditExecute(TLChannelParticipantBase participant)
        {
            if (_item == null)
            {
                return;
            }

            if (participant is TLChannelParticipantAdmin)
            {
                NavigationService.Navigate(typeof(ChannelAdminRightsPage), TLTuple.Create(_item.ToPeer(), participant));
            }
            else if (participant is TLChannelParticipantBanned)
            {
                NavigationService.Navigate(typeof(ChannelBannedRightsPage), TLTuple.Create(_item.ToPeer(), participant));
            }
        }

        public RelayCommand<TLChannelParticipantBase> ParticipantPromoteCommand => new RelayCommand<TLChannelParticipantBase>(ParticipantPromoteExecute);
        private void ParticipantPromoteExecute(TLChannelParticipantBase participant)
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelAdminRightsPage), TLTuple.Create(_item.ToPeer(), participant));
        }

        public RelayCommand<TLChannelParticipantBase> ParticipantRestrictCommand => new RelayCommand<TLChannelParticipantBase>(ParticipantRestrictExecute);
        private void ParticipantRestrictExecute(TLChannelParticipantBase participant)
        {
            if (_item == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(ChannelBannedRightsPage), TLTuple.Create(_item.ToPeer(), participant));
        }

        #endregion
    }

    public class TLChannelParticipantBaseComparer : IComparer<TLChannelParticipantBase>
    {
        private bool _epoch;

        public TLChannelParticipantBaseComparer(bool epoch)
        {
            _epoch = epoch;
        }

        public int Compare(TLChannelParticipantBase x, TLChannelParticipantBase y)
        {

            var xUser = x.User;
            var yUser = y.User;
            if (xUser == null || yUser == null)
            {
                return -1;
            }

            if (_epoch)
            {
                var epoch = LastSeenConverter.GetIndex(yUser).CompareTo(LastSeenConverter.GetIndex(xUser));
                if (epoch == 0)
                {
                    var fullName = xUser.FullName.CompareTo(yUser.FullName);
                    if (fullName == 0)
                    {
                        return yUser.Id.CompareTo(xUser.Id);
                    }

                    return fullName;
                }

                return epoch;
            }
            else
            {
                var fullName = xUser.FullName.CompareTo(yUser.FullName);
                if (fullName == 0)
                {
                    return yUser.Id.CompareTo(xUser.Id);
                }

                return fullName;
            }
        }
    }
}
