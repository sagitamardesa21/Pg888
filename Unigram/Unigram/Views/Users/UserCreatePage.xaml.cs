﻿using System.Text;
using Unigram.Common;
using Unigram.Entities;
using Unigram.ViewModels.Users;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Unigram.Views.Users
{
    public sealed partial class UserCreatePage : HostedPage
    {
        public UserCreateViewModel ViewModel => DataContext as UserCreateViewModel;

        public UserCreatePage()
        {
            InitializeComponent();
            DataContext = TLContainer.Current.Resolve<UserCreateViewModel>();
        }

        private void FirstName_Loaded(object sender, RoutedEventArgs e)
        {
            FirstName.Focus(FocusState.Keyboard);
        }

        private void PrimaryInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Back && string.IsNullOrEmpty(PrimaryInput.Text))
            {
                PhoneCode.Focus(FocusState.Keyboard);
                PhoneCode.SelectionStart = PhoneCode.Text.Length;
                e.Handled = true;
            }
        }

        #region Binding

        private ImageSource ConvertPhoto(string firstName, string lastName)
        {
            return PlaceholderHelper.GetNameForUser(firstName, lastName, 64);
        }

        private string ConvertFormat(Country country)
        {
            if (country == null)
            {
                return null;
            }

            var groups = PhoneNumber.Parse(country.PhoneCode);
            var builder = new StringBuilder();

            for (int i = 1; i < groups.Length; i++)
            {
                for (int j = 0; j < groups[i]; j++)
                {
                    builder.Append('-');
                }

                if (i + 1 < groups.Length)
                {
                    builder.Append(' ');
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
