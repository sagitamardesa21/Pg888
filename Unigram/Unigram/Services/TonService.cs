﻿#define TEST_TON

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Ton.Tonlib;
using Ton.Tonlib.Api;
using Windows.Storage;

namespace Unigram.Services
{
    public interface ITonService
    {
        IEncryptionService Encryption { get; }

        BaseObject Execute(Function function);

        void Send(Function function);
        void Send(Function function, ClientResultHandler handler);
        void Send(Function function, Action<BaseObject> handler);
        Task<BaseObject> SendAsync(Function function);

        void CleanUp();

        SyncState GetSyncState();

        void SetCreationState(WalletCreationState state);
        bool TryGetCreationState(out WalletCreationState state);
        WalletCreationState CreationState { get; }

        AccountAddress GetAccountAddress(string publicKey, long? walletId = null);

        bool IsCreating { get; }

        long DefaultWalletId { get; }
    }

    public class TonService : ITonService, ClientResultHandler
    {
        private Client _client;

        private readonly int _session;

        private readonly IProtoService _protoService;
        private readonly IEncryptionService _encryptionService;
        private readonly ISettingsService _settingsService;
        private readonly IEventAggregator _aggregator;

        private TaskCompletionSource<bool> _initializeTask;

        private long _defaultWalletId;

        private SyncState _syncState;
        private WalletCreationState _creationState;

        public TonService(int session, IProtoService protoService, IEncryptionService encryptionService, ISettingsService settingsService, IEventAggregator aggregator)
        {
            _session = session;

            _protoService = protoService;
            _encryptionService = encryptionService;
            _settingsService = settingsService;
            _aggregator = aggregator;

            _initializeTask = new TaskCompletionSource<bool>();

            Initialize();
        }

        public long DefaultWalletId => _defaultWalletId;

        private async void Initialize()
        {
            _client = Client.Create(this);

            // TODO: no buono
            var config = await GetConfigAsync();
            if (config == null)
            {
                return;
            }

            var info = Client.Execute(new OptionsValidateConfig(config)) as OptionsConfigInfo;
            if (info == null)
            {
                return;
            }

            _defaultWalletId = info.DefaultWalletId;

            await Task.Run(() =>
            {
                Directory.CreateDirectory(Path.Combine(ApplicationData.Current.LocalFolder.Path, $"{_session}", "ton"));

                _client.Send(new SetLogStream(new LogStreamFile(Path.Combine(ApplicationData.Current.LocalFolder.Path, $"{_session}", "ton", "log.txt"), 10 * 1024 * 1024)));
                _client.Send(new SetLogVerbosityLevel(SettingsService.Current.VerbosityLevel));

                _client.Send(new Init(new Options(config, new KeyStoreTypeDirectory(Path.Combine(ApplicationData.Current.LocalFolder.Path, $"{_session}", "ton")))), result =>
                {
                    _initializeTask.SetResult(true);
                });
                _client.Run();
            });
        }

        private async Task<Config> GetConfigAsync()
        {
            var config = _settingsService.Wallet.Config;
            var name = _settingsService.Wallet.Name;

            var response = await _protoService.SendAsync(new Telegram.Td.Api.GetApplicationConfig());
            if (response is Telegram.Td.Api.JsonValueObject json)
            {
                foreach (var member in json.Members)
                {
                    if (string.Equals(member.Key, "wallet_config", StringComparison.OrdinalIgnoreCase) && member.Value is Telegram.Td.Api.JsonValueString configValue)
                    {
                        _settingsService.Wallet.Config = configValue.Value;
                        config = configValue.Value;
                    }
                    else if (string.Equals(member.Key, "wallet_blockchain_name", StringComparison.OrdinalIgnoreCase) && member.Value is Telegram.Td.Api.JsonValueString nameValue)
                    {
                        _settingsService.Wallet.Name = nameValue.Value;
                        name = nameValue.Value;
                    }
                }
            }

#if TEST_TON
            if (name == null)
            {
                using (var client = new HttpClient())
                {
                    config = await client.GetStringAsync("https://test.ton.org/config.json");
                    name = "testnet";
                }
            }
#endif

            return new Config(config, name, true, false);
        }

        public IEncryptionService Encryption => _encryptionService;



        public BaseObject Execute(Function function)
        {
            return Client.Execute(function);
        }



        public void Send(Function function)
        {
            _client.Send(function);
        }

        public void Send(Function function, ClientResultHandler handler)
        {
            _client.Send(function, handler);
        }

        public void Send(Function function, Action<BaseObject> handler)
        {
            _client.Send(function, handler);
        }

        public async Task<BaseObject> SendAsync(Function function)
        {
            await _initializeTask.Task;
            return await _client.SendAsync(function);
        }



        public void CleanUp()
        {
            _encryptionService.Delete(_protoService.Options.WalletPublicKey);
            _protoService.Options.WalletPublicKey = null;

            _client.Send(new DeleteAllKeys());

            _creationState = null;
        }



        public void OnResult(BaseObject update)
        {
            if (update is UpdateSyncState updateSyncState)
            {
                _syncState = updateSyncState.SyncState;
            }
            else if (update is UpdateSendLiteServerQuery updateSendLiteServerQuery)
            {
                Handle(updateSendLiteServerQuery);
            }

            _aggregator.Publish(update);
        }

        private async void Handle(UpdateSendLiteServerQuery update)
        {
            //var response = await _protoService.SendAsync(new Telegram.Td.Api.SendTonLiteServerRequest(update.Data));
            //if (response is Telegram.Td.Api.TonLiteServerResponse liteServerResponse)
            //{
            //    Send(new OnLiteServerQueryResult(update.Id, liteServerResponse.Response));
            //}
            //else if (response is Telegram.Td.Api.Error error)
            //{
            //    Send(new OnLiteServerQueryError(update.Id, new Error(error.Code, error.Message)));
            //}
        }



        public SyncState GetSyncState()
        {
            return _syncState;
        }

        public void SetCreationState(WalletCreationState state)
        {
            if (_creationState != null && state != null)
            {
                throw new InvalidOperationException("Wallet is being created already");
            }

            _creationState = state;
        }

        public bool TryGetCreationState(out WalletCreationState state)
        {
            state = _creationState;
            return state != null;
        }

        public WalletCreationState CreationState => _creationState;
        public bool IsCreating => _creationState != null;

        

        public AccountAddress GetAccountAddress(string publicKey, long? walletId)
        {
            return Execute(new WalletV3GetAccountAddress(new WalletV3InitialAccountState(publicKey, walletId ?? _defaultWalletId))) as AccountAddress;
        }
    }

    public class WalletCreationState
    {
        public IList<byte> LocalPassword { get; set; }

        public Key Key { get; set; }

        public IList<string> WordList { get; set; }

        public IList<int> Indices { get; set; }
    }
}
