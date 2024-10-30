//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Converters;
using Unigram.ViewModels.Delegates;
using Unigram.ViewModels.Supergroups;

namespace Unigram.Views.Supergroups
{
    public sealed partial class SupergroupEditAdministratorPage : HostedPage, IMemberDelegate
    {
        public SupergroupEditAdministratorViewModel ViewModel => DataContext as SupergroupEditAdministratorViewModel;

        public SupergroupEditAdministratorPage()
        {
            InitializeComponent();
            Title = Strings.Resources.EditAdmin;
        }

        public void UpdateChat(Chat chat)
        {
        }

        public void UpdateChatTitle(Chat chat)
        {
        }

        public void UpdateChatPhoto(Chat chat)
        {
        }

        public void UpdateUser(Chat chat, User user, bool secret)
        {
            Cell.UpdateUser(ViewModel.ClientService, user, 64);
        }

        public void UpdateUserFullInfo(Chat chat, User user, UserFullInfo fullInfo, bool secret, bool accessToken)
        {
        }

        public void UpdateUserStatus(Chat chat, User user)
        {
            Cell.Subtitle = LastSeenConverter.GetLabel(user, true);
        }

        public void UpdateMember(Chat chat, User user, ChatMember member)
        {
            if (member.Status is ChatMemberStatusCreator or ChatMemberStatusAdministrator)
            {
                var canBeEdited = (member.Status is ChatMemberStatusCreator && member.MemberId.IsUser(ViewModel.ClientService.Options.MyId)) || (member.Status is ChatMemberStatusAdministrator administrator && administrator.CanBeEdited);

                Done.Visibility = canBeEdited ? Visibility.Visible : Visibility.Collapsed;
                Dismiss.Visibility = member.Status is ChatMemberStatusAdministrator && canBeEdited ? Visibility.Visible : Visibility.Collapsed;
                PermissionsRoot.Footer = canBeEdited ? null : Strings.Resources.EditAdminCantEdit;
                EditRankField.PlaceholderText = member.Status is ChatMemberStatusCreator ? Strings.Resources.ChannelCreator : Strings.Resources.ChannelAdmin;
                EditRankPanel.Footer = string.Format(Strings.Resources.EditAdminRankInfo, member.Status is ChatMemberStatusCreator ? Strings.Resources.ChannelCreator : Strings.Resources.ChannelAdmin);

                ChangeInfo.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                PostMessages.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                EditMessages.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                DeleteMessages.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                BanUsers.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                AddUsers.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                PinMessages.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                ManageVideoChats.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                AddAdmins.IsEnabled = member.Status is ChatMemberStatusAdministrator && canBeEdited;
                IsAnonymous.IsEnabled = canBeEdited;
            }
            else
            {
                Done.Visibility = Visibility.Visible;
                Dismiss.Visibility = Visibility.Collapsed;
                PermissionsRoot.Footer = null;
                EditRankField.PlaceholderText = Strings.Resources.ChannelAdmin;
                EditRankPanel.Footer = string.Format(Strings.Resources.EditAdminRankInfo, Strings.Resources.ChannelAdmin);
            }

            if (chat.Type is ChatTypeSupergroup group)
            {
                PermissionsRoot.Visibility = Visibility.Visible;

                ChangeInfo.Content = group.IsChannel ? Strings.Resources.EditAdminChangeChannelInfo : Strings.Resources.EditAdminChangeGroupInfo;
                PostMessages.Visibility = group.IsChannel ? Visibility.Visible : Visibility.Collapsed;
                EditMessages.Visibility = group.IsChannel ? Visibility.Visible : Visibility.Collapsed;
                DeleteMessages.Content = group.IsChannel ? Strings.Resources.EditAdminDeleteMessages : Strings.Resources.EditAdminGroupDeleteMessages;
                BanUsers.Visibility = group.IsChannel ? Visibility.Collapsed : Visibility.Visible;
                PinMessages.Visibility = group.IsChannel ? Visibility.Collapsed : Visibility.Visible;
                ManageVideoChats.Visibility = group.IsChannel ? Visibility.Collapsed : Visibility.Visible;
                IsAnonymous.Visibility = group.IsChannel ? Visibility.Collapsed : Visibility.Visible;
                AddUsers.Content = chat.Permissions.CanInviteUsers ? Strings.Resources.EditAdminAddUsersViaLink : Strings.Resources.EditAdminAddUsers;
            }
            else
            {
                PermissionsRoot.Visibility = Visibility.Collapsed;
            }

            //TransferOwnership.Content = group.IsChannel ? Strings.Resources.EditAdminChannelTransfer : Strings.Resources.EditAdminGroupTransfer;
        }

        #region Binding

        private Visibility ConvertActionVisibility(Visibility ownership, Visibility dismiss)
        {
            if (ownership == Visibility.Visible)
            {
                return Visibility.Visible;
            }

            return dismiss;
        }

        #endregion

    }
}
