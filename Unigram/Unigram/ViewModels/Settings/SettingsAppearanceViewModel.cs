﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unigram.Common;
using Windows.UI.Xaml;
using Unigram.Services;

namespace Unigram.ViewModels.Settings
{
    public class SettingsAppearanceViewModel : TLViewModelBase
    {
        private readonly Dictionary<int, int> _indexToSize = new Dictionary<int, int> { { 0, 12 }, { 1, 13 }, { 2, 14 }, { 3, 15 }, { 4, 16 }, { 5, 17 }, { 6, 18 } };
        private readonly Dictionary<int, int> _sizeToIndex = new Dictionary<int, int> { { 12, 0 }, { 13, 1 }, { 14, 2 }, { 15, 3 }, { 16, 4 }, { 17, 5 }, { 18, 6 } };

        public SettingsAppearanceViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
        }

        public double FontSize
        {
            get
            {
                var size = (int)Theme.Current.GetValueOrDefault("MessageFontSize", 15d);
                if (_sizeToIndex.TryGetValue(size, out int index))
                {
                    return (double)index;
                }

                return 2d;
            }
            set
            {
                var index = (int)Math.Round(value);
                if (_indexToSize.TryGetValue(index, out int size))
                {
                    Theme.Current.AddOrUpdateValue("MessageFontSize", (double)size);
                }

                RaisePropertyChanged();
            }
        }

        public TelegramTheme GetRawTheme()
        {
            var theme = Settings.Appearance.RequestedTheme;
            if (theme.HasFlag(TelegramTheme.Brand))
            {
                return theme & ~TelegramTheme.Brand;
            }

            return theme;
        }

        public ElementTheme GetElementTheme()
        {
            var theme = Settings.Appearance.RequestedTheme;
            return theme.HasFlag(TelegramTheme.Default)
                ? ElementTheme.Default
                : theme.HasFlag(TelegramTheme.Dark)
                ? ElementTheme.Dark
                : ElementTheme.Light;
        }

        public int RequestedTheme
        {
            get
            {
                return (int)GetRawTheme();
            }
            set
            {
                Settings.Appearance.RequestedTheme = IsSystemTheme ? (TelegramTheme)value : ((TelegramTheme)value | TelegramTheme.Brand);
                RaisePropertyChanged();
                RaisePropertyChanged(() => IsSystemTheme);
                RaisePropertyChanged(() => IsThemeChanged);
            }
        }

        public bool IsSystemTheme
        {
            get
            {
                return !Settings.Appearance.RequestedTheme.HasFlag(TelegramTheme.Brand);
            }
            set
            {
                Settings.Appearance.RequestedTheme = value ? GetRawTheme() : (GetRawTheme() | TelegramTheme.Brand);
                RaisePropertyChanged();
                RaisePropertyChanged(() => RequestedTheme);
                RaisePropertyChanged(() => IsThemeChanged);
            }
        }

        public bool IsThemeChanged
        {
            get
            {
                return Settings.Appearance.CurrentTheme != Settings.Appearance.RequestedTheme;
            }
        }
    }
}
