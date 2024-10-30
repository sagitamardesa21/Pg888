//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//

using Telegram.Common;

namespace Telegram.Services.Settings
{
    public class DiagnosticsSettings : SettingsServiceBase
    {
        public DiagnosticsSettings()
            : base("Diagnostics")
        {
        }

        private bool? _chatPreviewToolTip;
        public bool ChatPreviewToolTip
        {
            get => _chatPreviewToolTip ??= GetValueOrDefault("ChatPreviewToolTip", ApiInfo.IsPackagedRelease);
            set => AddOrUpdateValue(ref _chatPreviewToolTip, "ChatPreviewToolTip", value);
        }

        private bool? _disableDatabase;
        public bool DisableDatabase
        {
            get => _disableDatabase ??= GetValueOrDefault("DisableDatabase", false);
            set => AddOrUpdateValue(ref _disableDatabase, "DisableDatabase", value);
        }

        private bool? _copyFormattedCode;
        public bool CopyFormattedCode
        {
            get => _copyFormattedCode ??= GetValueOrDefault("CopyFormattedCode", true);
            set => AddOrUpdateValue(ref _copyFormattedCode, "CopyFormattedCode", value);
        }

        private bool? _allowRightToLeft;
        public bool AllowRightToLeft
        {
            get => _allowRightToLeft ??= GetValueOrDefault("AllowRightToLeft", false);
            set => AddOrUpdateValue(ref _allowRightToLeft, "AllowRightToLeft", value);
        }

        private bool? _stickyPhotos;
        public bool StickyPhotos
        {
            get => _stickyPhotos ??= GetValueOrDefault("StickyPhotos", false);
            set => AddOrUpdateValue(ref _stickyPhotos, "StickyPhotos", value);
        }

        private string? _deviceName;
        public string DeviceName
        {
            get => _deviceName ??= GetValueOrDefault("DeviceName", string.Empty);
            set => AddOrUpdateValue(ref _deviceName, "DeviceName", value);
        }

        private string _lastNavigatedPageType;
        public string LastNavigatedPageType
        {
            get => _lastNavigatedPageType ??= GetValueOrDefault("LastNavigatedPageType", string.Empty);
            set => AddOrUpdateValue(ref _lastNavigatedPageType, "LastNavigatedPageType", value);
        }

        private string _lastErrorMessage;
        public string LastErrorMessage
        {
            get => _lastErrorMessage ??= GetValueOrDefault("LastErrorMessage", string.Empty);
            set => AddOrUpdateValue(ref _lastErrorMessage, "LastErrorMessage", value);
        }

        private string _lastErrorProperties;
        public string LastErrorProperties
        {
            get => _lastErrorProperties ??= GetValueOrDefault("LastErrorProperties", string.Empty);
            set => AddOrUpdateValue(ref _lastErrorProperties, "LastErrorProperties", value);
        }

        private int? _lastErrorVersion;
        public int LastErrorVersion
        {
            get => _lastErrorVersion ??= GetValueOrDefault("LastErrorVersion", 0);
            set => AddOrUpdateValue(ref _lastErrorVersion, "LastErrorVersion", value);
        }

        private int? _updateCount;
        public int UpdateCount
        {
            get => _updateCount ??= GetValueOrDefault("UpdateCount", 0);
            set => AddOrUpdateValue(ref _updateCount, "UpdateCount", value);
        }

        private int? _lastUpdateVersion;
        public int LastUpdateVersion
        {
            get => _lastUpdateVersion ??= GetValueOrDefault("LastUpdateVersion", 0);
            set => AddOrUpdateValue(ref _lastUpdateVersion, "LastUpdateVersion", value);
        }

        private int? _lastUpdateTime;
        public int LastUpdateTime
        {
            get => _lastUpdateTime ??= GetValueOrDefault("LastUpdateTime", 0);
            set => AddOrUpdateValue(ref _lastUpdateTime, "LastUpdateTime", value);
        }

        private long? _storageMaxTimeFromLastAccess;
        public long StorageMaxTimeFromLastAccess
        {
            get => _storageMaxTimeFromLastAccess ??= GetValueOrDefault("StorageMaxTimeFromLastAccess", 0L);
            set => AddOrUpdateValue(ref _storageMaxTimeFromLastAccess, "StorageMaxTimeFromLastAccess", value);
        }

        private bool? _useStorageOptimizer;
        public bool UseStorageOptimizer
        {
            get => _useStorageOptimizer ??= GetValueOrDefault("UseStorageOptimizer", false);
            set => AddOrUpdateValue(ref _useStorageOptimizer, "UseStorageOptimizer", value);
        }

        public bool IsLastErrorDiskFull { get; set; }
    }
}
