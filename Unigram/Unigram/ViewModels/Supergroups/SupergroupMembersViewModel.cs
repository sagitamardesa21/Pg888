//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml.Controls;
using System;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Navigation.Services;
using Unigram.Services;
using Unigram.ViewModels.Delegates;
using Unigram.Views.Popups;
using Unigram.Views.Supergroups;

namespace Unigram.ViewModels.Supergroups
{
    public class SupergroupMembersViewModel : SupergroupMembersViewModelBase, IDelegable<ISupergroupDelegate>
    {
        public SupergroupMembersViewModel(IClientService clientService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(clientService, settingsService, aggregator, new SupergroupMembersFilterRecent(), query => new SupergroupMembersFilterSearch(query))
        {
            AddCommand = new RelayCommand(AddExecute);

            MemberPromoteCommand = new RelayCommand<ChatMember>(MemberPromoteExecute);
            MemberRestrictCommand = new RelayCommand<ChatMember>(MemberRestrictExecute);
            MemberRemoveCommand = new RelayCommand<ChatMember>(MemberRemoveExecute);
        }

        public bool IsEmbedded { get; set; }

        private bool _hasHiddenMembers;
        public bool HasHiddenMembers
        {
            get => _hasHiddenMembers;
            set => SetHiddenMembers(value);
        }

        public void UpdateHiddenMembers(bool value)
        {
            Set(ref _hasHiddenMembers, value, nameof(HasHiddenMembers));
        }

        private void SetHiddenMembers(bool value)
        {
            if (Chat.Type is ChatTypeSupergroup supergroupType && ClientService.TryGetSupergroupFull(Chat, out SupergroupFullInfo supergroup))
            {
                if (supergroup.CanHideMembers)
                {
                    Set(ref _hasHiddenMembers, value, nameof(HasHiddenMembers));
                    ClientService.Send(new ToggleSupergroupHasHiddenMembers(supergroupType.SupergroupId, value));
                }
                else
                {
                    Set(ref _hasHiddenMembers, false, nameof(HasHiddenMembers));
                }
            }
        }

        public RelayCommand AddCommand { get; }
        private async void AddExecute()
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var selected = await SharePopup.PickChatAsync(Strings.Resources.SelectContact);
            var user = ClientService.GetUser(selected);

            if (user == null)
            {
                return;
            }

            var confirm = await MessagePopup.ShowAsync(XamlRoot, string.Format(Strings.Resources.AddToTheGroup, user.FullName()), Strings.Resources.AppName, Strings.Resources.OK, Strings.Resources.Cancel);
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            var response = await ClientService.SendAsync(new AddChatMember(chat.Id, user.Id, (int)ClientService.Options.ForwardedMessageCountMax));
            if (response is Error error)
            {

            }
        }

        #region Context menu

        public RelayCommand<ChatMember> MemberPromoteCommand { get; }
        private void MemberPromoteExecute(ChatMember member)
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupEditAdministratorPage), state: NavigationState.GetChatMember(chat.Id, member.MemberId));
        }

        public RelayCommand<ChatMember> MemberRestrictCommand { get; }
        private void MemberRestrictExecute(ChatMember member)
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            NavigationService.Navigate(typeof(SupergroupEditRestrictedPage), state: NavigationState.GetChatMember(chat.Id, member.MemberId));
        }

        public RelayCommand<ChatMember> MemberRemoveCommand { get; }
        private async void MemberRemoveExecute(ChatMember member)
        {
            var chat = _chat;
            if (chat == null)
            {
                return;
            }

            var index = Members.IndexOf(member);

            Members.Remove(member);

            var response = await ClientService.SendAsync(new SetChatMemberStatus(chat.Id, member.MemberId, new ChatMemberStatusBanned()));
            if (response is Error)
            {
                Members.Insert(index, member);
            }
        }

        #endregion
    }
}
