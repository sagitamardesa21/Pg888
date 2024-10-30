﻿using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.System.UserProfile;

namespace Unigram.Services
{
    public interface IDeviceInfoService
    {
        string DeviceModel { get; }
        string ApplicationVersion { get; }
        string SystemVersion { get; }
        string SystemLanguageCode { get; }
    }

    public class DeviceInfoService : IDeviceInfoService
    {
        public static string DefaultSystemManufacturer = "System manufacturer";
        public static string DefaultSystemProductName = "System Product Name";
        public static string DefaultSystemSku = "SKU";

        public string DeviceModel
        {
            get
            {
                var deviceInfo = new EasClientDeviceInformation();
                var systemSku = string.IsNullOrEmpty(deviceInfo.SystemSku)
                    || DefaultSystemSku == deviceInfo.SystemSku
                    ? null
                    : deviceInfo.SystemSku;
                var systemProductName = string.IsNullOrEmpty(deviceInfo.SystemProductName)
                    || DefaultSystemProductName == deviceInfo.SystemProductName
                    ? null
                    : deviceInfo.SystemProductName;
                var systemManufacturer = string.IsNullOrEmpty(deviceInfo.SystemManufacturer)
                    || DefaultSystemManufacturer == deviceInfo.SystemManufacturer
                    ? null
                    : deviceInfo.SystemManufacturer;
                return systemProductName ?? systemSku ?? deviceInfo.FriendlyName ?? "Desktop";
            }
        }

        public string SystemVersion
        {
            get
            {
                string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                ulong version = ulong.Parse(deviceFamilyVersion);
                ulong major = (version & 0xFFFF000000000000L) >> 48;
                ulong minor = (version & 0x0000FFFF00000000L) >> 32;
                ulong build = (version & 0x00000000FFFF0000L) >> 16;
                ulong revision = version & 0x000000000000FFFFL;

                if (minor > 0)
                {
                    return $"Windows {major}.{minor}";
                }

                return $"Windows {major}";
                return $"Windows {major}.{minor}.{build}.{revision}";
            }
        }

        public string SystemLanguageCode
        {
            get
            {
                return GlobalizationPreferences.Languages.Count > 0 ? GlobalizationPreferences.Languages[0] : "en";
            }
        }

        public string ApplicationVersion
        {
            get
            {
                //return "4.7";

                var v = Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}";
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }
    }
}
