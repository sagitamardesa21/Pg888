﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unigram.Views;
using Unigram.ViewModels.Settings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Unigram.Common;
using Telegram.Td.Api;

namespace Unigram.Views.Settings
{
    public sealed partial class SettingsPhoneSentCodePage : Page
    {
        public SettingsPhoneSentCodeViewModel ViewModel => DataContext as SettingsPhoneSentCodeViewModel;

        public SettingsPhoneSentCodePage()
        {
            InitializeComponent();
            DataContext = TLContainer.Current.Resolve<SettingsPhoneSentCodeViewModel>();

            ViewModel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SENT_CODE_INVALID":
                    VisualUtilities.ShakeView(PrimaryInput);
                    break;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PrimaryInput.Focus(FocusState.Keyboard);
        }

        #region Binding

        private string ConvertType(AuthenticationCodeInfo codeInfo, string number)
        {
            if (codeInfo == null)
            {
                return null;
            }

            switch (codeInfo.Type)
            {
                case AuthenticationCodeTypeTelegramMessage appType:
                    return Strings.Resources.SentAppCode;
                case AuthenticationCodeTypeSms smsType:
                    return string.Format(Strings.Resources.SentSmsCode, PhoneNumber.Format(number));
            }

            return null;
        }

        #endregion
    }
}
