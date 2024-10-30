//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Converters;
using Unigram.ViewModels.Folders;

namespace Unigram.Views.Folders
{
    public sealed partial class FoldersPage : HostedPage
    {
        public FoldersViewModel ViewModel => DataContext as FoldersViewModel;

        public FoldersPage()
        {
            InitializeComponent();
            Title = Strings.Resources.Filters;
        }

        private void Items_ElementPrepared(Microsoft.UI.Xaml.Controls.ItemsRepeater sender, Microsoft.UI.Xaml.Controls.ItemsRepeaterElementPreparedEventArgs args)
        {
            var button = args.Element as BadgeButton;
            var filter = button.DataContext as ChatFilterInfo;

            var icon = Icons.ParseFilter(filter.IconName);

            button.Glyph = Icons.FilterToGlyph(icon).Item1;
            button.Content = filter.Title;
            button.Command = ViewModel.EditCommand;
            button.CommandParameter = filter;
            button.ChevronGlyph = args.Index < ViewModel.ClientService.Options.ChatFilterCountMax ? Icons.ChevronRight : Icons.LockClosed;
        }

        private void Recommended_ElementPrepared(Microsoft.UI.Xaml.Controls.ItemsRepeater sender, Microsoft.UI.Xaml.Controls.ItemsRepeaterElementPreparedEventArgs args)
        {
            var content = args.Element as StackPanel;
            var filter = content.DataContext as RecommendedChatFilter;

            var grid = content.Children[0] as Grid;

            var button = grid.Children[0] as BadgeButton;
            var add = grid.Children[1] as Button;

            var separator = content.Children[1];

            var icon = Icons.ParseFilter(filter.Filter);

            button.Glyph = Icons.FilterToGlyph(icon).Item1;
            button.Content = filter.Filter.Title;
            button.Badge = filter.Description;

            add.Command = ViewModel.RecommendCommand;
            add.CommandParameter = filter;

            separator.Visibility = args.Index < sender.ItemsSourceView.Count - 1
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Item_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var flyout = new MenuFlyout();

            var element = sender as FrameworkElement;
            var chat = element.DataContext as ChatFilterInfo;

            flyout.CreateFlyoutItem(ViewModel.DeleteCommand, chat, Strings.Resources.FilterDeleteItem, new FontIcon { Glyph = Icons.Delete });

            args.ShowAt(flyout, element);
        }

        #region Binding

        private Visibility ConvertCreate(int count)
        {
            return count < 10 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

    }
}
