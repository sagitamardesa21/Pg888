using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Services;
using Unigram.ViewModels.Delegates;
using Unigram.Views;
using Unigram.Views.Settings;
using Unigram.Views.Supergroups;
using Unigram.Views.Users;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Supergroups
{
    public class SupergroupEditAdministratorViewModel : TLViewModelBase, IDelegable<IMemberDelegate>
    {
        public IMemberDelegate Delegate { get; set; }

        public SupergroupEditAdministratorViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            ProfileCommand = new RelayCommand(ProfileExecute);
            SendCommand = new RelayCommand(SendExecute);
            TransferCommand = new RelayCommand(TransferExecute);
            DismissCommand = new RelayCommand(DismissExecute);
        }

        private Chat _chat;
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

        private ChatMember _member;
        public ChatMember Member
        {
            get
            {
                return _member;
            }
            set
            {
                Set(ref _member, value);
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var bundle = parameter as ChatMemberNavigation;
            if (bundle == null)
            {
                return;
            }

            Chat = ProtoService.GetChat(bundle.ChatId);

            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var response = await ProtoService.SendAsync(new GetChatMember(chat.Id, bundle.UserId));
            if (response is ChatMember member && chat.Type is ChatTypeSupergroup super)
            {
                var item = ProtoService.GetUser(member.UserId);
                var cache = ProtoService.GetUserFull(member.UserId);

                var group = ProtoService.GetSupergroup(super.SupergroupId);

                Delegate?.UpdateMember(chat, group, item, member);
                Delegate?.UpdateUser(chat, item, false);

                if (cache == null)
                {
                    ProtoService.Send(new GetUserFullInfo(member.UserId));
                }
                else
                {
                    Delegate?.UpdateUserFullInfo(chat, item, cache, false, false);
                }

                Member = member;

                if (member.Status is ChatMemberStatusAdministrator administrator)
                {
                    CanChangeInfo = administrator.CanChangeInfo;
                    CanDeleteMessages = administrator.CanDeleteMessages;
                    CanEditMessages = administrator.CanEditMessages;
                    CanInviteUsers = administrator.CanInviteUsers;
                    CanPinMessages = administrator.CanPinMessages;
                    CanPostMessages = administrator.CanPostMessages;
                    CanPromoteMembers = administrator.CanPromoteMembers;
                    CanRestrictMembers = administrator.CanRestrictMembers;

                    CustomTitle = administrator.CustomTitle;
                }
                else
                {
                    CanChangeInfo = true;
                    CanDeleteMessages = true;
                    CanEditMessages = true;
                    CanInviteUsers = true;
                    CanPinMessages = true;
                    CanPostMessages = true;
                    CanPromoteMembers = member.Status is ChatMemberStatusCreator;
                    CanRestrictMembers = true;

                    if (member.Status is ChatMemberStatusCreator creator)
                    {
                        CustomTitle = creator.CustomTitle;
                    }
                }
            }
        }

        private bool _isAdminAlready = true;
        public bool IsAdminAlready
        {
            get
            {
                return _isAdminAlready;
            }
            set
            {
                Set(ref _isAdminAlready, value);
            }
        }

        public bool CanTransferOwnership
        {
            get
            {
                var chat = _chat;
                if (chat == null)
                {
                    return false;
                }

                var supergroup = CacheService.GetSupergroup(chat);
                if (supergroup == null)
                {
                    return false;
                }

                return _canChangeInfo &&
                    _canDeleteMessages &&
                    _canInviteUsers &&
                    _canPromoteMembers &&
                    (supergroup.IsChannel ? _canEditMessages : true) &&
                    (supergroup.IsChannel ? true : _canPinMessages) &&
                    (supergroup.IsChannel ? _canPostMessages : true) &&
                    (supergroup.IsChannel ? true :_canRestrictMembers);
            }
        }

        #region Flags

        private bool _canChangeInfo;
        public bool CanChangeInfo
        {
            get
            {
                return _canChangeInfo;
            }
            set
            {
                Set(ref _canChangeInfo, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        private bool _canPostMessages;
        public bool CanPostMessages
        {
            get
            {
                return _canPostMessages;
            }
            set
            {
                Set(ref _canPostMessages, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        private bool _canEditMessages;
        public bool CanEditMessages
        {
            get
            {
                return _canEditMessages;
            }
            set
            {
                Set(ref _canEditMessages, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        private bool _canDeleteMessages;
        public bool CanDeleteMessages
        {
            get
            {
                return _canDeleteMessages;
            }
            set
            {
                Set(ref _canDeleteMessages, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        private bool _canRestrictMembers;
        public bool CanRestrictMembers
        {
            get
            {
                return _canRestrictMembers;
            }
            set
            {
                Set(ref _canRestrictMembers, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        private bool _canInviteUsers;
        public bool CanInviteUsers
        {
            get
            {
                return _canInviteUsers;
            }
            set
            {
                Set(ref _canInviteUsers, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        private bool _canPinMessages;
        public bool CanPinMessages
        {
            get
            {
                return _canPinMessages;
            }
            set
            {
                Set(ref _canPinMessages, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        private bool _canPromoteMembers;
        public bool CanPromoteMembers
        {
            get
            {
                return _canPromoteMembers;
            }
            set
            {
                Set(ref _canPromoteMembers, value);
                RaisePropertyChanged(() => CanTransferOwnership);
            }
        }

        #endregion

        private string _customTitle;
        public string CustomTitle
        {
            get => _customTitle;
            set => Set(ref _customTitle, value);
        }

        public RelayCommand ProfileCommand { get; }
        private async void ProfileExecute()
        {
            var member = _member;
            if (member == null)
            {
                return;
            }

            var response = await ProtoService.SendAsync(new CreatePrivateChat(member.UserId, false));
            if (response is Chat chat)
            {
                NavigationService.Navigate(typeof(ProfilePage), chat.Id);
            }
        }

        public RelayCommand SendCommand { get; }
        private async void SendExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var member = _member;
            if (member == null)
            {
                return;
            }

            var supergroup = chat.Type as ChatTypeSupergroup;
            if (supergroup == null)
            {
                return;
            }

            ChatMemberStatus status;
            if (member.Status is ChatMemberStatusCreator creator)
            {
                status = new ChatMemberStatusCreator(_customTitle ?? string.Empty, creator.IsMember);
            }
            else
            {
                status = new ChatMemberStatusAdministrator
                {
                    CanChangeInfo = _canChangeInfo,
                    CanDeleteMessages = _canDeleteMessages,
                    CanEditMessages = supergroup.IsChannel ? _canEditMessages : false,
                    CanInviteUsers = _canInviteUsers,
                    CanPinMessages = supergroup.IsChannel ? false : _canPinMessages,
                    CanPostMessages = supergroup.IsChannel ? _canPostMessages : false,
                    CanPromoteMembers = _canPromoteMembers,
                    CanRestrictMembers = supergroup.IsChannel ? false : _canRestrictMembers,
                    CustomTitle = _customTitle ?? string.Empty
                };
            }

            var response = await ProtoService.SendAsync(new SetChatMemberStatus(chat.Id, member.UserId, status));
            if (response is Ok)
            {
                NavigationService.RemoveLastIf(typeof(SupergroupAddAdministratorPage));
                NavigationService.GoBack();
                NavigationService.Frame.ForwardStack.Clear();
            }
            else
            {
                // TODO: ...
            }
        }

        public RelayCommand TransferCommand { get; }
        private async void TransferExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var supergroup = CacheService.GetSupergroup(chat);
            if (supergroup == null)
            {
                return;
            }

            var member = _member;
            if (member == null)
            {
                return;
            }

            var user = CacheService.GetUser(member.UserId);
            if (user == null)
            {
                return;
            }

            var canTransfer = await ProtoService.SendAsync(new CanTransferOwnership());
            if (canTransfer is CanTransferOwnershipResultPasswordNeeded || canTransfer is CanTransferOwnershipResultPasswordTooFresh || canTransfer is CanTransferOwnershipResultSessionTooFresh)
            {
                var primary = Strings.Resources.OK;

                var builder = new StringBuilder();
                builder.AppendFormat(supergroup.IsChannel ? Strings.Resources.EditChannelAdminTransferAlertText : Strings.Resources.EditAdminTransferAlertText, user.FirstName);
                builder.AppendLine();
                builder.AppendLine($"� {Strings.Resources.EditAdminTransferAlertText1}");
                builder.AppendLine($"� {Strings.Resources.EditAdminTransferAlertText2}");

                if (canTransfer is CanTransferOwnershipResultPasswordNeeded)
                {
                    primary = Strings.Resources.EditAdminTransferSetPassword;
                }
                else
                {
                    builder.AppendLine();
                    builder.AppendLine(Strings.Resources.EditAdminTransferAlertText3);
                }

                var confirm = await TLMessageDialog.ShowAsync(builder.ToString(), Strings.Resources.EditAdminTransferAlertTitle, primary, Strings.Resources.Cancel);
                if (confirm == Windows.UI.Xaml.Controls.ContentDialogResult.Primary && canTransfer is CanTransferOwnershipResultPasswordNeeded)
                {
                    NavigationService.Navigate(typeof(SettingsPasswordPage));
                }
            }
            else if (canTransfer is CanTransferOwnershipResultOk)
            {
                var confirm = await TLMessageDialog.ShowAsync(Strings.Resources.EditAdminTransferReadyAlertText, supergroup.IsChannel ? Strings.Resources.EditAdminChannelTransfer : Strings.Resources.EditAdminGroupTransfer, Strings.Resources.EditAdminTransferChangeOwner, Strings.Resources.Cancel);
                if (confirm != Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    return;
                }

                var input = new InputDialog();
                input.Title = "YOLO";
                input.Header = "Yolo";
                input.PlaceholderText = "Yolo";
                input.PrimaryButtonText = Strings.Resources.OK;
                input.SecondaryButtonText = Strings.Resources.Cancel;

                var result = await input.ShowQueuedAsync();
                if (result != Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    return;
                }

                var response = await ProtoService.SendAsync(new TransferChatOwnership(chat.Id, user.Id, input.Text));
                if (response is Ok)
                {

                }
            }
        }

        public RelayCommand DismissCommand { get; }
        private async void DismissExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var member = _member;
            if (member == null)
            {
                return;
            }

            var response = await ProtoService.SendAsync(new SetChatMemberStatus(chat.Id, member.UserId, new ChatMemberStatusMember()));
            if (response is Ok)
            {
                NavigationService.RemoveLastIf(typeof(SupergroupAddAdministratorPage));
                NavigationService.GoBack();
                NavigationService.Frame.ForwardStack.Clear();
            }
            else
            {
                // TODO: ...
            }
        }
    }
}
