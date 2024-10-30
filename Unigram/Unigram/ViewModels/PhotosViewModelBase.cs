﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;

namespace Unigram.ViewModels
{
    public abstract class PhotosViewModelBase : UnigramViewModelBase
    {
        public PhotosViewModelBase(IMTProtoService protoService, ICacheService cacheService, ITelegramEventAggregator aggregator)
            : base(protoService, cacheService, aggregator)
        {
        }

        public int SelectedIndex
        {
            get
            {
                if (Items == null || SelectedItem == null)
                {
                    return 0;
                }

                var index = Items.IndexOf(SelectedItem);
                if (index == Items.Count - 1)
                {
                    LoadNext();
                }
                if (index == 0)
                {
                    LoadPrevious();
                }

                return index + 1;
            }
        }

        private int _totalItems;
        public int TotalItems
        {
            get
            {
                return _totalItems;
            }
            set
            {
                Set(ref _totalItems, value);
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                Set(ref _selectedItem, value);
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        private object _poster;
        public object Poster
        {
            get
            {
                return _poster;
            }
            set
            {
                Set(ref _poster, value);
            }
        }

        public ObservableCollection<object> Items { get; protected set; }

        protected virtual void LoadPrevious() { }

        protected virtual void LoadNext() { }
    }
}
