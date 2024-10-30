//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using Telegram.Common;
using Telegram.ViewModels.Settings.Password;
using Windows.UI.Xaml;

namespace Telegram.Views.Settings.Password
{
    public sealed partial class SettingsPasswordEmailPage : HostedPage
    {
        public SettingsPasswordEmailViewModel ViewModel => DataContext as SettingsPasswordEmailViewModel;

        public SettingsPasswordEmailPage()
        {
            InitializeComponent();
        }

        private void Walkthrough_Loaded(object sender, RoutedEventArgs e)
        {
            Walkthrough.Header.IndexChanged += Lottie_IndexChanged;
        }

        private int _stop;

        private void Field_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var index = Field1.SelectionStart + Field1.SelectionLength - 1;
            if (index >= 0)
            {
                var rect = Field1.GetRectFromCharacterIndex(Field1.SelectionStart + Field1.SelectionLength - 1, false);
                var position = rect.X / Field1.ActualWidth;

                _stop = (int)(20 + (Math.Min(1, Math.Max(0, position)) * 140));
                Walkthrough.Header.Play(Walkthrough.Header.Index > _stop);
            }
            else
            {
                _stop = 0;
                Walkthrough.Header.Play(true);
            }
        }

        private void Lottie_IndexChanged(object sender, int e)
        {
            this.BeginOnUIThread(() =>
            {
                if (e == _stop)
                {
                    Walkthrough.Header.Pause();
                }
            });
        }
    }
}
