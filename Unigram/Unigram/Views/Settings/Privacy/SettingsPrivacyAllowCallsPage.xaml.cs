//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unigram.ViewModels.Settings;
using Unigram.ViewModels.Settings.Privacy;

namespace Unigram.Views.Settings.Privacy
{
    public sealed partial class SettingsPrivacyAllowCallsPage : HostedPage
    {
        public SettingsPrivacyAllowCallsViewModel ViewModel => DataContext as SettingsPrivacyAllowCallsViewModel;

        public SettingsPrivacyAllowCallsPage()
        {
            InitializeComponent();
            Title = Strings.Resources.Calls;
        }

        #region Binding

        private Visibility ConvertNever(PrivacyValue value)
        {
            return value is PrivacyValue.AllowAll or PrivacyValue.AllowContacts ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility ConvertAlways(PrivacyValue value)
        {
            return value is PrivacyValue.AllowContacts or PrivacyValue.DisallowAll ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        private void P2PCall_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPrivacyAllowP2PCallsPage));
        }
    }
}
