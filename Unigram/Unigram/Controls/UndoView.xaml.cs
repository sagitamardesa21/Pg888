﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Td.Api;
using Unigram.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Unigram.Controls
{
    public sealed partial class UndoView : UserControl
    {
        private readonly DispatcherTimer _timeout;
        private readonly Queue<UndoOp> _queue;

        private int _remaining = 5;

        private Storyboard _storyboard;

        public UndoView()
        {
            InitializeComponent();

            _timeout = new DispatcherTimer();
            _timeout.Interval = TimeSpan.FromSeconds(1);
            _timeout.Tick += Timeout_Tick;

            _queue = new Queue<UndoOp>();
        }

        private void Timeout_Tick(object sender, object e)
        {
            _remaining--;

            Remaining.Content = _remaining;
            //Slice.StartAngle = 360 - ((360d / 5d) * _remaining);

            if (_remaining == 0)
            {
                Reset(false);
            }
        }

        public void Show(IList<Chat> chats, bool clear, Action<IList<Chat>> action, Action<IList<Chat>> undo)
        {
            _timeout.Stop();
            _storyboard?.Stop();

            _remaining = 5;
            _queue.Enqueue(new UndoOp(chats, action, undo));

            _timeout.Start();

            if (clear)
            {
                Text.Text = Strings.Resources.HistoryClearedUndo;
            }
            else if (chats.Count == 0 && chats[0] is Chat chat)
            {
                if (chat.Type is ChatTypeSupergroup super)
                {
                    Text.Text = super.IsChannel ? Strings.Resources.ChannelDeletedUndo : Strings.Resources.GroupDeletedUndo;
                }
                else
                {
                    Text.Text = Strings.Resources.ChatDeletedUndo;
                }
            }
            else
            {
                Text.Text = Strings.Resources.ChatDeletedUndo;
            }

            IsEnabled = true;

            Slice.StartAngle = 0;
            Remaining.Content = 5;

            StartAnimation();
            Grid.SetRow(LayoutRoot, 0);
        }

        private void StartAnimation()
        {
            var anim = new DoubleAnimation();
            anim.Duration = TimeSpan.FromSeconds(5);
            anim.From = 0;
            anim.To = 360;
            anim.EnableDependentAnimation = true;

            Storyboard.SetTarget(anim, Slice);
            Storyboard.SetTargetProperty(anim, "StartAngle");

            var board = new Storyboard();
            board.Children.Add(anim);
            board.Begin();

            _storyboard = board;
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            Reset(true);
        }

        private void Reset(bool undo)
        {
            while (_queue.Count > 0)
            {
                var current = _queue.Dequeue();
                if (undo)
                {
                    current.Undo.Invoke(current.Chats);
                }
                else
                {
                    current.Delete.Invoke(current.Chats);
                }
            }

            _remaining = 5;

            _timeout.Stop();
            _storyboard?.Stop();
            _storyboard = null;

            IsEnabled = false;

            Grid.SetRow(LayoutRoot, 1);
        }

        private class UndoOp
        {
            public IList<Chat> Chats { get; private set; }
            public Action<IList<Chat>> Delete { get; private set; }
            public Action<IList<Chat>> Undo { get; private set; }

            public UndoOp(IList<Chat> chats, Action<IList<Chat>> delete, Action<IList<Chat>> undo)
            {
                Chats = chats;
                Delete = delete;
                Undo = undo;
            }
        }
    }
}
