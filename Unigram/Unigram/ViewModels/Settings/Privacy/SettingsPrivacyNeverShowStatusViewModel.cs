﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Services;

namespace Unigram.ViewModels.Settings.Privacy
{
    class SettingsPrivacyNeverShowStatusViewModel : SettingsPrivacyNeverViewModelBase
    {
        public SettingsPrivacyNeverShowStatusViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator, new UserPrivacySettingShowStatus())
        {
        }

        public override string Title => Strings.Resources.NeverShareWithTitle;
    }
}
