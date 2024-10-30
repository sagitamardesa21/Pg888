//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Xaml;
using System;
using Telegram.Td;
using Telegram.Td.Api;
using Unigram.Converters;
using Unigram.ViewModels;
using Unigram.Views.Popups;
using Windows.Storage;

namespace Unigram.Views
{
    public sealed partial class DiagnosticsPage : HostedPage
    {
        public DiagnosticsViewModel ViewModel => DataContext as DiagnosticsViewModel;

        public DiagnosticsPage()
        {
            InitializeComponent();
            Title = "Diagnostics";
        }

        #region Binding

        private string ConvertVerbosity(VerbosityLevel level)
        {
            return Enum.GetName(typeof(VerbosityLevel), level);
        }

        private string ConvertSize(ulong size)
        {
            return FileSizeConverter.Convert((long)size);
        }

        #endregion

        private async void Calls_Click(object sender, RoutedEventArgs e)
        {
            var log = await ApplicationData.Current.LocalFolder.TryGetItemAsync("tgcalls.txt") as StorageFile;
            if (log != null)
            {
                await SharePopup.Create().ShowAsync(new InputMessageDocument(new InputFileLocal(log.Path), null, true, null));
            }
        }

        private async void GroupCalls_Click(object sender, RoutedEventArgs e)
        {
            var log = await ApplicationData.Current.LocalFolder.TryGetItemAsync("tgcalls_group.txt") as StorageFile;
            if (log != null)
            {
                await SharePopup.Create().ShowAsync(new InputMessageDocument(new InputFileLocal(log.Path), null, true, null));
            }
        }

        private async void Log_Click(object sender, RoutedEventArgs e)
        {
            var log = await ApplicationData.Current.LocalFolder.TryGetItemAsync("tdlib_log.txt") as StorageFile;
            if (log != null)
            {
                await SharePopup.Create().ShowAsync(new InputMessageDocument(new InputFileLocal(log.Path), null, true, null));
            }
        }

        private async void LogOld_Click(object sender, RoutedEventArgs e)
        {
            var log = await ApplicationData.Current.LocalFolder.TryGetItemAsync("tdlib_log.txt.old") as StorageFile;
            if (log != null)
            {
                await SharePopup.Create().ShowAsync(new InputMessageDocument(new InputFileLocal(log.Path), null, true, null));
            }
        }

        private void Crash_Click(object sender, RoutedEventArgs e)
        {
            Client.Execute(new AddLogMessage(0, "Crash_Click"));
        }
    }
}
