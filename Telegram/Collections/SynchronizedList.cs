﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Telegram.Collections
{
    public interface ISynchronizedList
    {
        void Disconnect();
    }

    public class SynchronizedList<T> : MvxObservableCollection<T>, ISynchronizedList
    {
        private ObservableCollection<T> _source;

        public void UpdateSource(ObservableCollection<T> source)
        {
            if (_source != null)
            {
                _source.CollectionChanged -= OnCollectionChanged;
            }

            _source = source;

            if (_source != null)
            {
                _source.CollectionChanged += OnCollectionChanged;
                ReplaceWith(_source);
            }
            else
            {
                Clear();
            }
        }

        // TODO: this is needed because DialogViewModel may keep loading messages
        // after the view is already unloaded, causing CollectionChanged handling to fail.
        public void Disconnect()
        {
            if (_source != null)
            {
                _source.CollectionChanged -= OnCollectionChanged;
            }

            _source = null;
            Clear();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InsertRange(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ReplaceWith(_source);
                    break;
            }
        }
    }
}
