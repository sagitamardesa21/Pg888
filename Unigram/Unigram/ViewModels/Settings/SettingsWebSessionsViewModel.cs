using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TdWindows;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL;
using Unigram.Collections;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Services;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.Settings
{
    public class SettingsWebSessionsViewModel : UnigramViewModelBase
    {
        public SettingsWebSessionsViewModel(IProtoService protoService, ICacheService cacheService, IEventAggregator aggregator) 
            : base(protoService, cacheService, aggregator)
        {
            Items = new SortedObservableCollection<ConnectedWebsite>(new TLAuthorizationComparer());

            TerminateCommand = new RelayCommand<ConnectedWebsite>(TerminateExecute);
            TerminateOthersCommand = new RelayCommand(TerminateOtherExecute);
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await UpdateSessionsAsync();
        }

        private async Task UpdateSessionsAsync()
        {
            var response = await ProtoService.SendAsync(new GetConnectedWebsites());
            if (response is ConnectedWebsites websites)
            {
                Items.Clear();

                foreach (var item in websites.Websites)
                {
                    Items.Add(item);

                    //if (_cachedItems.ContainsKey(item.Hash))
                    //{
                    //    if (item.IsCurrent)
                    //    {
                    //        var cached = _cachedItems[item.Hash];
                    //        cached.Update(item);
                    //        cached.RaisePropertyChanged(() => cached.AppName);
                    //        cached.RaisePropertyChanged(() => cached.AppVersion);
                    //        cached.RaisePropertyChanged(() => cached.DeviceModel);
                    //        cached.RaisePropertyChanged(() => cached.Platform);
                    //        cached.RaisePropertyChanged(() => cached.SystemVersion);
                    //        cached.RaisePropertyChanged(() => cached.Ip);
                    //        cached.RaisePropertyChanged(() => cached.Country);
                    //        cached.RaisePropertyChanged(() => cached.DateActive);
                    //    }
                    //    else
                    //    {
                    //        Items.Remove(_cachedItems[item.Hash]);
                    //        Items.Add(item);

                    //        _cachedItems[item.Hash] = item;
                    //    }
                    //}
                    //else
                    //{
                    //    _cachedItems[item.Hash] = item;
                    //    if (item.IsCurrent)
                    //    {
                    //        Current = item;
                    //    }
                    //    else
                    //    {
                    //        Items.Add(item);
                    //    }
                    //}
                }
            }
        }

        public ObservableCollection<ConnectedWebsite> Items { get; private set; }

        public RelayCommand<ConnectedWebsite> TerminateCommand { get; }
        private async void TerminateExecute(ConnectedWebsite session)
        {
            var bot = ProtoService.GetUser(session.BotUserId);
            if (bot == null)
            {
                return;
            }

            var dialog = new TLMessageDialog();
            dialog.Title = Strings.Android.AppName;
            dialog.Message = string.Format(Strings.Android.TerminateWebSessionQuestion, session.DomainName);
            dialog.PrimaryButtonText = Strings.Android.OK;
            dialog.SecondaryButtonText = Strings.Android.Cancel;
            dialog.CheckBoxLabel = string.Format(Strings.Android.TerminateWebSessionStop, bot.FirstName);

            var terminate = await dialog.ShowQueuedAsync();
            if (terminate == ContentDialogResult.Primary)
            {
                var response = await ProtoService.SendAsync(new DisconnectWebsite(session.Id));
                if (response is Ok)
                {
                    Items.Remove(session);
                }
                else if (response is Error error)
                {
                    Execute.ShowDebugMessage("auth.resetWebAuthotization error " + error);
                }

                ProtoService.Send(new BlockUser(bot.Id));
            }
        }

        public RelayCommand TerminateOthersCommand { get; }
        private async void TerminateOtherExecute()
        {
            var terminate = await TLMessageDialog.ShowAsync(Strings.Android.AreYouSureWebSessions, Strings.Android.AppName, Strings.Android.OK, Strings.Android.Cancel);
            if (terminate == ContentDialogResult.Primary)
            {
                var response = await ProtoService.SendAsync(new DisconnectAllWebsites());
                if (response is Ok)
                {
                    Items.Clear();
                }
                else if (response is Error error)
                {
                    Execute.ShowDebugMessage("auth.resetWebAuthotizations error " + error);
                }
            }
        }

        public class TLAuthorizationComparer : IComparer<ConnectedWebsite>
        {
            public int Compare(ConnectedWebsite x, ConnectedWebsite y)
            {
                var epoch = y.LastActiveDate.CompareTo(x.LastActiveDate);
                if (epoch == 0)
                {
                    var appName = x.DomainName.CompareTo(y.DomainName);
                    if (appName == 0)
                    {
                        return x.Id.CompareTo(y.Id);
                    }

                    return appName;
                }

                return epoch;
            }
        }
    }
}
