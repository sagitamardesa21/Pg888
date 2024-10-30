using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Template10.Common;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Converters;
using Unigram.Entities;
using Unigram.Services;
using Unigram.ViewModels.Delegates;
using Unigram.Views.Channels;
using Unigram.Views.Supergroups;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Supergroups
{
    public class SupergroupEditViewModel : TLViewModelBase,
        IDelegable<ISupergroupDelegate>,
        IHandle<UpdateSupergroup>,
        IHandle<UpdateSupergroupFullInfo>
    {
        public ISupergroupDelegate Delegate { get; set; }

        public SupergroupEditViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            EditTypeCommand = new RelayCommand(EditTypeExecute);
            EditDemocracyCommand = new RelayCommand(EditDemocracyExecute);
            EditHistoryCommand = new RelayCommand(EditHistoryExecute);
            EditStickerSetCommand = new RelayCommand(EditStickerSetExecute);
            EditPhotoCommand = new RelayCommand<StorageFile>(EditPhotoExecute);
            DeletePhotoCommand = new RelayCommand(DeletePhotoExecute);

            RevokeCommand = new RelayCommand(RevokeExecute);
            DeleteCommand = new RelayCommand(DeleteExecute);

            SendCommand = new RelayCommand(SendExecute);

            MembersCommand = new RelayCommand(MembersExecute);
            AdminsCommand = new RelayCommand(AdminsExecute);
            BannedCommand = new RelayCommand(BannedExecute);
            KickedCommand = new RelayCommand(KickedExecute);
        }

        protected Chat _chat;
        public Chat Chat
        {
            get
            {
                return _chat;
            }
            set
            {
                Set(ref _chat, value);
            }
        }

        private StorageFile _photo;
        private bool _deletePhoto;

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
            }
        }

        private string _about;
        public string About
        {
            get
            {
                return _about;
            }
            set
            {
                Set(ref _about, value);
            }
        }

        private bool _isSignatures;
        public bool IsSignatures
        {
            get
            {
                return _isSignatures;
            }
            set
            {
                Set(ref _isSignatures, value);
            }
        }

        #region Initialize

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var chatId = (long)parameter;

            Chat = ProtoService.GetChat(chatId);

            var chat = _chat;
            if (chat == null)
            {
                return Task.CompletedTask;
            }

            Aggregator.Subscribe(this);
            Delegate?.UpdateChat(chat);

            if (chat.Type is ChatTypeSupergroup super)
            {
                var item = ProtoService.GetSupergroup(super.SupergroupId);
                var cache = ProtoService.GetSupergroupFull(super.SupergroupId);

                Delegate?.UpdateSupergroup(chat, item);

                if (cache == null)
                {
                    ProtoService.Send(new GetSupergroupFullInfo(super.SupergroupId));
                }
                else
                {
                    Delegate?.UpdateSupergroupFullInfo(chat, item, cache);
                }
            }

            return Task.CompletedTask;
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            Aggregator.Unsubscribe(this);
            return Task.CompletedTask;
        }

        public void Handle(UpdateSupergroup update)
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            if (chat.Type is ChatTypeSupergroup super && super.SupergroupId == update.Supergroup.Id)
            {
                BeginOnUIThread(() => Delegate?.UpdateSupergroup(chat, update.Supergroup));
            }
        }

        public void Handle(UpdateSupergroupFullInfo update)
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            if (chat.Type is ChatTypeSupergroup super && super.SupergroupId == update.SupergroupId)
            {
                BeginOnUIThread(() => Delegate?.UpdateSupergroupFullInfo(chat, ProtoService.GetSupergroup(update.SupergroupId), update.SupergroupFullInfo));
            }
        }

        #endregion

        public RelayCommand SendCommand { get; }
        private async void SendExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            if (chat.Type is ChatTypeSupergroup supergroup)
            {
                var item = ProtoService.GetSupergroup(supergroup.SupergroupId);
                var cache = ProtoService.GetSupergroupFull(supergroup.SupergroupId);

                if (item == null || cache == null)
                {
                    return;
                }

                var about = _about.Format();
                var title = _title.Trim();

                if (!string.Equals(title, chat.Title))
                {
                    var response = await ProtoService.SendAsync(new SetChatTitle(chat.Id, title));
                    if (response is Error)
                    {
                        // TODO:
                    }
                }

                if (!string.Equals(about, cache.Description))
                {
                    var response = await ProtoService.SendAsync(new SetSupergroupDescription(item.Id, about));
                    if (response is Error)
                    {
                        // TODO:
                    }
                }

                if (_isSignatures != item.SignMessages)
                {
                    var response = await ProtoService.SendAsync(new ToggleSupergroupSignMessages(item.Id, _isSignatures));
                    if (response is Error)
                    {
                        // TODO:
                    }
                }

                if (_photo != null)
                {
                    var response = await ProtoService.SendAsync(new SetChatPhoto(chat.Id, await _photo.ToGeneratedAsync()));
                    if (response is Error)
                    {
                        // TODO:
                    }
                }
                else if (_deletePhoto)
                {
                    var response = await ProtoService.SendAsync(new SetChatPhoto(chat.Id, new InputFileId(0)));
                    if (response is Error)
                    {
                        // TODO:
                    }
                }

                NavigationService.GoBack();
            }
        }

        public RelayCommand<StorageFile> EditPhotoCommand { get; }
        private async void EditPhotoExecute(StorageFile file)
        {
            _photo = file;
            _deletePhoto = false;
        }

        public RelayCommand DeletePhotoCommand { get; }
        private void DeletePhotoExecute()
        {
            _photo = null;
            _deletePhoto = true;
        }

        public RelayCommand EditTypeCommand { get; }
        private void EditTypeExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupEditTypePage), chat.Id);
        }

        public RelayCommand EditDemocracyCommand { get; }
        private async void EditDemocracyExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var group = CacheService.GetSupergroup(chat);
            if (group == null)
            {
                return;
            }

            var dialog = new ContentDialog { Style = BootStrapper.Current.Resources["ModernContentDialogStyle"] as Style };
            var stack = new StackPanel();
            stack.Margin = new Thickness(12, 16, 12, 0);
            stack.Children.Add(new RadioButton { Tag = true,  Content = Strings.Resources.WhoCanAddMembersAllMembers, IsChecked = group.AnyoneCanInvite });
            stack.Children.Add(new RadioButton { Tag = false, Content = Strings.Resources.WhoCanAddMembersAdmins, IsChecked = !group.AnyoneCanInvite });

            dialog.Title = Strings.Resources.WhoCanAddMembers;
            dialog.Content = stack;
            dialog.PrimaryButtonText = Strings.Resources.OK;
            dialog.SecondaryButtonText = Strings.Resources.Cancel;

            var confirm = await dialog.ShowQueuedAsync();
            if (confirm == ContentDialogResult.Primary)
            {
                var anyoneCanInvite = true;
                foreach (RadioButton current in stack.Children)
                {
                    if (current.IsChecked == true)
                    {
                        anyoneCanInvite = (bool)current.Tag;
                        break;
                    }
                }

                if (anyoneCanInvite != group.AnyoneCanInvite)
                {
                    ProtoService.Send(new ToggleSupergroupInvites(group.Id, anyoneCanInvite));
                }
            }
        }

        public RelayCommand EditHistoryCommand { get; }
        private async void EditHistoryExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var group = CacheService.GetSupergroup(chat);
            if (group == null)
            {
                return;
            }

            var full = CacheService.GetSupergroupFull(chat);
            if (full == null)
            {
                return;
            }

            var dialog = new ContentDialog { Style = BootStrapper.Current.Resources["ModernContentDialogStyle"] as Style };
            var stack = new StackPanel();
            stack.Margin = new Thickness(12, 16, 12, 0);
            stack.Children.Add(new RadioButton { Tag = true, Content = Strings.Resources.ChatHistoryVisible, IsChecked = full.IsAllHistoryAvailable });
            stack.Children.Add(new TextBlock { Text = Strings.Resources.ChatHistoryVisibleInfo, Margin = new Thickness(28, -6, 0, 8), Style = BootStrapper.Current.Resources["InfoCaptionTextBlockStyle"] as Style });
            stack.Children.Add(new RadioButton { Tag = false, Content = Strings.Resources.ChatHistoryHidden, IsChecked = !full.IsAllHistoryAvailable });
            stack.Children.Add(new TextBlock { Text = Strings.Resources.ChatHistoryHiddenInfo, Margin = new Thickness(28, -6, 0, 8), Style = BootStrapper.Current.Resources["InfoCaptionTextBlockStyle"] as Style });

            dialog.Title = Strings.Resources.ChatHistory;
            dialog.Content = stack;
            dialog.PrimaryButtonText = Strings.Resources.OK;
            dialog.SecondaryButtonText = Strings.Resources.Cancel;

            var confirm = await dialog.ShowQueuedAsync();
            if (confirm == ContentDialogResult.Primary)
            {
                var isAllHistoryAvailable = true;
                foreach (RadioButton current in stack.Children.OfType<RadioButton>())
                {
                    if (current.IsChecked == true)
                    {
                        isAllHistoryAvailable = (bool)current.Tag;
                        break;
                    }
                }

                if (isAllHistoryAvailable != full.IsAllHistoryAvailable)
                {
                    ProtoService.Send(new ToggleSupergroupIsAllHistoryAvailable(group.Id, isAllHistoryAvailable));
                }
            }
        }

        public RelayCommand EditStickerSetCommand { get; }
        private void EditStickerSetExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupEditStickerSetPage), chat.Id);
        }

        public RelayCommand RevokeCommand { get; }
        private async void RevokeExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var confirm = await TLMessageDialog.ShowAsync(Strings.Resources.RevokeAlert, Strings.Resources.RevokeLink, Strings.Resources.RevokeButton, Strings.Resources.Cancel);
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            ProtoService.Send(new GenerateChatInviteLink(chat.Id));
        }

        public RelayCommand DeleteCommand { get; }
        private async void DeleteExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            if (chat.Type is ChatTypeSupergroup super)
            {
                var message = super.IsChannel ? Strings.Resources.ChannelDeleteAlert : Strings.Resources.MegaDeleteAlert;
                var confirm = await TLMessageDialog.ShowAsync(message, Strings.Resources.AppName, Strings.Resources.OK, Strings.Resources.Cancel);
                if (confirm == ContentDialogResult.Primary)
                {
                    var response = await ProtoService.SendAsync(new DeleteSupergroup(super.SupergroupId));
                    if (response is Ok)
                    {
                        NavigationService.RemovePeerFromStack(chat.Id);
                    }
                    else if (response is Error error)
                    {
                        // TODO: ...
                    }
                }
            }
        }

        #region Navigation

        public RelayCommand AdminsCommand { get; }
        private void AdminsExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupAdministratorsPage), chat.Id);
        }

        public RelayCommand BannedCommand { get; }
        private void BannedExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupBannedPage), chat.Id);
        }

        public RelayCommand KickedCommand { get; }
        private void KickedExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupRestrictedPage), chat.Id);
        }

        public RelayCommand MembersCommand { get; }
        private void MembersExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupMembersPage), chat.Id);
        }

        #endregion
    }
}
