﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using Telegram.Td.Api;
using Unigram.Services;
using Unigram.Views.SignIn;

namespace Unigram.Views
{
    public sealed partial class BlankPage : Page
    {
        public BlankPage()
        {
            InitializeComponent();
            DataContext = new object();

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && Frame.ForwardStack.Any(x => x.SourcePageType == typeof(SignInPage)))
            {
                TLContainer.Current.Resolve<IProtoService>().Send(new Destroy());
            }
        }
    }
}
