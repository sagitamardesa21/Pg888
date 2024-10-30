﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unigram.Common;
using Unigram.ViewModels.Delegates;

namespace Unigram.Views
{
    public class UnigramContainer
    {
        private static UnigramContainer _instance = new UnigramContainer();

        //private Dictionary<int, IContainer> _containers = new Dictionary<int, IContainer>();
        private IContainer[] _containers = new IContainer[3];

        private UnigramContainer() { }

        public static UnigramContainer Current
        {
            get
            {
                return _instance;
            }
        }

        public void Build(int id, Func<ContainerBuilder, int, IContainer> factory)
        {
            //for (int i = 0; i < Telegram.Api.Constants.AccountsMaxCount; i++)
            //{
            //    //if (_containers.ContainsKey(i))
            //    if (_containers[i] != null)
            //    {
            //        continue;
            //    }

            //}

            _containers[id] = factory(new ContainerBuilder(), id);
        }

        public TService Resolve<TService>(int account = int.MaxValue)
        {
            if (account == int.MaxValue)
            {
                account = ApplicationSettings.Current.SelectedAccount;
            }

            var result = default(TService);
            //if (_containers.TryGetValue(account, out IContainer container))
            var container = _containers[account];
            {
                result = container.Resolve<TService>();
            }

            return result;
        }

        public TService Resolve<TService, TDelegate>(TDelegate delegato, int account = int.MaxValue)
            where TService : IDelegable<TDelegate>
            where TDelegate : IViewModelDelegate
        {
            if (account == int.MaxValue)
            {
                account = ApplicationSettings.Current.SelectedAccount;
            }

            var result = default(TService);
            //if (_containers.TryGetValue(account, out IContainer container))
            var container = _containers[account];
            {
                result = container.Resolve<TService>();
            }

            if (result != null)
            {
                result.Delegate = delegato;
            }

            return result;
        }

        public object ResolveType(Type type, int account = int.MaxValue)
        {
            if (account == int.MaxValue)
            {
                account = ApplicationSettings.Current.SelectedAccount;
            }

            //if (_containers.TryGetValue(account, out IContainer container))
            var container = _containers[account];
            {
                return container.Resolve(type);
            }

            return null;
        }
    }
}
