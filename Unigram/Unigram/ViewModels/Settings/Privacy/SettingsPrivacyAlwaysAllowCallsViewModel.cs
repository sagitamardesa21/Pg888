﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TdWindows;
using Unigram.Services;

namespace Unigram.ViewModels.Settings.Privacy
{
    public class SettingsPrivacyAlwaysAllowCallsViewModel : SettingsPrivacyAlwaysViewModelBase
    {
        public SettingsPrivacyAlwaysAllowCallsViewModel(IProtoService protoService, ICacheService cacheService, IEventAggregator aggregator)
            : base(protoService, cacheService, aggregator, new UserPrivacySettingAllowCalls())
        {
        }

        public override string Title => Strings.Android.AlwaysAllow;
    }
}
