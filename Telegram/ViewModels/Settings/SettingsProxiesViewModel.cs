//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Collections;
using Telegram.Native;
using Telegram.Navigation;
using Telegram.Navigation.Services;
using Telegram.Services;
using Telegram.Td.Api;
using Telegram.Views.Popups;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Telegram.ViewModels.Settings
{
    public class SettingsProxiesViewModel : TLViewModelBase
        , IHandle
    //, IHandle<UpdateConnectionState>
    //, IHandle<UpdateOption>
    {
        private readonly INetworkService _networkService;

        public SettingsProxiesViewModel(IClientService clientService, ISettingsService settingsService, IEventAggregator aggregator, INetworkService networkService)
            : base(clientService, settingsService, aggregator)
        {
            _networkService = networkService;

            Items = new MvxObservableCollection<ConnectionViewModel>();
        }

        protected override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, NavigationState state)
        {
            if (App.Connection != null)
            {
                await App.Connection.SendMessageAsync(new ValueSet { { "LoopbackExempt", true } });
            }

            var systemId = await _networkService.GetSystemProxyId();

            var response = await ClientService.SendAsync(new GetProxies());
            if (response is Proxies proxies)
            {
                var items = proxies.ProxiesValue.Select(x => x.Id == systemId ? new SystemProxyViewModel(x) : new ProxyViewModel(x)).ToList<ConnectionViewModel>();
                var system = items.FirstOrDefault(x => x.Id == systemId);
                if (system != null)
                {
                    system.IsEnabled = _networkService.UseSystemProxy;
                    items.Remove(system);
                }

                items.Insert(0, new ConnectionViewModel());

                if (system != null)
                {
                    items.Insert(1, system);
                }

                Items.ReplaceWith(items);
                SelectedItem = _networkService.UseSystemProxy && system != null ? system : items.OfType<ProxyViewModel>().FirstOrDefault(x => x.IsEnabled) ?? items.FirstOrDefault();

                if (_selectedItem != null)
                {
                    _selectedItem.IsEnabled = true;
                }

                Parallel.ForEach(items, async (item) =>
                {
                    await UpdateAsync(item);
                });

                _networkService.ProxyChanged += OnSystemProxyChanged;
            }
        }

        protected override Task OnNavigatedFromAsync(NavigationState pageState, bool suspending)
        {
            _networkService.ProxyChanged -= OnSystemProxyChanged;
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        public override void Subscribe()
        {
            Aggregator.Subscribe<UpdateConnectionState>(this, Handle)
                .Subscribe<UpdateOption>(Handle);
        }

        private async Task UpdateAsync(ConnectionViewModel proxy)
        {
            var proxyId = proxy.Id;
            if (proxy is SystemProxyViewModel && !HttpProxyWatcher.Current.IsEnabled)
            {
                proxyId = 0;
            }

            var status = await ClientService.SendAsync(new PingProxy(proxyId));
            BeginOnUIThread(() =>
            {
                if (status is Seconds seconds)
                {
                    proxy.Seconds = seconds.SecondsValue;
                    proxy.Error = null;
                    proxy.Status = new ConnectionStatusReady(proxy.IsEnabled, seconds.SecondsValue);
                }
                else if (status is Error error)
                {
                    proxy.Seconds = 0;
                    proxy.Error = error;
                    proxy.Status = new ConnectionStatusError(error);
                }
            });
        }

        private void OnSystemProxyChanged(object sender, Proxy proxy)
        {
            var connection = Items.FirstOrDefault(x => x is SystemProxyViewModel);
            if (connection == null || proxy == null)
            {
                return;
            }

            BeginOnUIThread(async () =>
            {
                var selected = SelectedItem == connection;
                var index = Items.IndexOf(connection);
                if (index != 1)
                {
                    return;
                }

                var edited = new SystemProxyViewModel(proxy);
                Items.Remove(connection);
                Items.Insert(index, edited);

                if (selected)
                {
                    SelectedItem = edited;
                }

                await UpdateAsync(edited);
            });
        }

        public void Handle(UpdateConnectionState update)
        {
            BeginOnUIThread(() => Handle(update.State, ClientService.Options.EnabledProxyId));
        }

        public void Handle(UpdateOption update)
        {
            if (string.Equals(update.Name, "enabled_proxy_id", StringComparison.OrdinalIgnoreCase))
            {
                BeginOnUIThread(() => Handle(ClientService.GetConnectionState(), ClientService.Options.EnabledProxyId));
            }
        }

        private void Handle(ConnectionState state, long enabledProxyId)
        {
            foreach (var item in Items)
            {
                if (_networkService.UseSystemProxy)
                {
                    item.IsEnabled = item is SystemProxyViewModel;
                }
                else
                {
                    item.IsEnabled = item.Id == enabledProxyId;
                }

                if (!item.IsEnabled)
                {
                    if (item.Error != null)
                    {
                        item.Status = new ConnectionStatusError(item.Error);
                    }
                    else
                    {
                        item.Status = new ConnectionStatusReady(false, item.Seconds);
                    }

                    continue;
                }

                switch (state)
                {
                    case ConnectionStateWaitingForNetwork:
                        //ShowStatus(Strings.WaitingForNetwork);
                        break;
                    case ConnectionStateConnecting:
                    case ConnectionStateConnectingToProxy:
                        item.Status = new ConnectionStatusConnecting();
                        break;
                    case ConnectionStateUpdating:
                    case ConnectionStateReady:
                        item.Status = new ConnectionStatusReady(true, item.Seconds);
                        break;
                }
            }
        }

        public MvxObservableCollection<ConnectionViewModel> Items { get; private set; }

        private ConnectionViewModel _selectedItem;
        public ConnectionViewModel SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        public async void Add()
        {
            var dialog = new ProxyPopup();
            var confirm = await ShowPopupAsync(dialog);
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            var response = await ClientService.SendAsync(new AddProxy(dialog.Server, dialog.Port, false, dialog.Type));
            if (response is Proxy proxy)
            {
                var connection = new ProxyViewModel(proxy);
                Items.Add(connection);
                await UpdateAsync(connection);
            }
        }

        public async void Enable(ConnectionViewModel connection)
        {
            MarkAsEnabled(connection);

            if (connection is SystemProxyViewModel)
            {
                _networkService.UseSystemProxy = true;
                await _networkService.UpdateSystemProxy();
            }
            else if (connection is ProxyViewModel proxy)
            {
                _networkService.UseSystemProxy = false;
                await ClientService.SendAsync(new EnableProxy(proxy.Id));
            }
            else
            {
                _networkService.UseSystemProxy = false;
                await ClientService.SendAsync(new DisableProxy());
            }

            Handle(ClientService.GetConnectionState(), ClientService.Options.EnabledProxyId);
        }

        public async void Edit(ConnectionViewModel connection)
        {
            var dialog = new ProxyPopup(connection as ProxyViewModel);
            var confirm = await ShowPopupAsync(dialog);
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            var response = await ClientService.SendAsync(new EditProxy(connection.Id, dialog.Server, dialog.Port, false, dialog.Type));
            if (response is Proxy proxy)
            {
                var index = Items.IndexOf(connection);
                Items.Remove(connection);

                var edited = new ProxyViewModel(proxy);
                Items.Insert(index, edited);
                await UpdateAsync(edited);
            }

            Handle(ClientService.GetConnectionState(), ClientService.Options.EnabledProxyId);
        }

        public async void Remove(ProxyViewModel proxy)
        {
            var confirm = await ShowPopupAsync(Strings.DeleteProxy, Strings.AppName, Strings.OK, Strings.Cancel);
            if (confirm != ContentDialogResult.Primary)
            {
                return;
            }

            var response = await ClientService.SendAsync(new RemoveProxy(proxy.Id));
            if (response is Ok)
            {
                Items.Remove(proxy);
            }

            Handle(ClientService.GetConnectionState(), ClientService.Options.EnabledProxyId);
        }

        public async void Share(ProxyViewModel proxy)
        {
            var response = await ClientService.SendAsync(new GetProxyLink(proxy.Id));
            if (response is HttpUrl httpUrl && Uri.TryCreate(httpUrl.Url, UriKind.Absolute, out Uri uri))
            {
                await SharePopup.GetForCurrentView().ShowAsync(uri, Strings.Proxy);
            }
        }

        private void MarkAsEnabled(ConnectionViewModel connection)
        {
            foreach (var item in Items)
            {
                item.IsEnabled = item.Id == connection.Id;
            }
        }
    }

    public class ConnectionViewModel : BindableBase
    {
        private ConnectionStatus _status = new ConnectionStatusChecking();
        public ConnectionStatus Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public double Seconds { get; set; }
        public Error Error { get; set; }

        public virtual string DisplayName { get; } = "Without Proxy";

        public virtual bool IsEnabled { get; set; }
        public virtual int Id { get; private set; }
    }

    public class ProxyViewModel : ConnectionViewModel
    {
        private readonly Proxy _proxy;

        public ProxyViewModel(Proxy proxy)
        {
            _proxy = proxy;
        }

        public ProxyType Type => _proxy.Type;
        public override bool IsEnabled { get => _proxy.IsEnabled; set => _proxy.IsEnabled = value; }
        public int LastUsedDate => _proxy.LastUsedDate;
        public int Port => _proxy.Port;
        public string Server => _proxy.Server;
        public override int Id => _proxy.Id;

        public override string DisplayName => $"{Server}:{Port}";
    }

    public class SystemProxyViewModel : ProxyViewModel
    {
        public SystemProxyViewModel(Proxy proxy)
            : base(proxy)
        {
        }

        public override string DisplayName => "Use System Proxy Settings";
    }

    public interface ConnectionStatus
    {
    }

    public class ConnectionStatusChecking : ConnectionStatus
    {
    }

    public class ConnectionStatusConnecting : ConnectionStatus
    {
    }

    public class ConnectionStatusReady : ConnectionStatus
    {
        public ConnectionStatusReady(bool connected, double seconds)
        {
            IsConnected = connected;
            Seconds = seconds;
        }

        public bool IsConnected { get; private set; }
        public double Seconds { get; private set; }
    }

    public class ConnectionStatusError : ConnectionStatus
    {
        public ConnectionStatusError(Error error)
        {
            Error = error;
        }

        public Error Error { get; private set; }
    }
}
