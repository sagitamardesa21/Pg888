﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unigram.ViewModels;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Unigram.Services
{
    public interface IShortcutsService
    {
        IList<ShortcutCommand> Process(AcceleratorKeyEventArgs args);
    }

    public class ShortcutsService : TLViewModelBase, IShortcutsService
    {
        #region Const

        private readonly ShortcutCommand[] _autoRepeatCommands = new[]
        {
            //ShortcutCommand.MediaPrevious,
            //ShortcutCommand.MediaNext,
            ShortcutCommand.ChatPrevious,
            ShortcutCommand.ChatNext,
            ShortcutCommand.ChatFirst,
            ShortcutCommand.ChatLast,
        };

        //private readonly ShortcutCommand[] _mediaCommands = new[]
        //{
        //    ShortcutCommand.MediaPlay,
        //    ShortcutCommand.MediaPause,
        //    ShortcutCommand.MediaPlayPause,
        //    ShortcutCommand.MediaStop,
        //    ShortcutCommand.MediaPrevious,
        //    ShortcutCommand.MediaNext,
        //};

        //private readonly ShortcutCommand[] _supportCommands = new[]
        //{
        //    ShortcutCommand.SupportReloadTemplates,
        //    ShortcutCommand.SupportToggleMuted,
        //    ShortcutCommand.SupportScrollToCurrent,
        //    ShortcutCommand.SupportHistoryBack,
        //    ShortcutCommand.SupportHistoryForward,
        //};

        private readonly ShortcutCommand[] _foldersCommands = new[]
        {
            ShortcutCommand.ShowAllChats,
            ShortcutCommand.ShowFolder1,
            ShortcutCommand.ShowFolder2,
            ShortcutCommand.ShowFolder3,
            ShortcutCommand.ShowFolder4,
            ShortcutCommand.ShowFolder5,
            ShortcutCommand.ShowFolder6,
            ShortcutCommand.ShowFolderLast,
        };

        private readonly Dictionary<string, ShortcutCommand> _commandByName = new Dictionary<string, ShortcutCommand>
        {
            { "close_telegram"    , ShortcutCommand.Close },
            { "lock_telegram"     , ShortcutCommand.Lock },
            { "minimize_telegram" , ShortcutCommand.Minimize },
            { "quit_telegram"     , ShortcutCommand.Quit },

            //{ "media_play"        , ShortcutCommand.MediaPlay },
            //{ "media_pause"       , ShortcutCommand.MediaPause },
            //{ "media_playpause"   , ShortcutCommand.MediaPlayPause },
            //{ "media_stop"        , ShortcutCommand.MediaStop },
            //{ "media_previous"    , ShortcutCommand.MediaPrevious },
            //{ "media_next"        , ShortcutCommand.MediaNext },

            { "search"            , ShortcutCommand.Search },

            { "previous_chat"     , ShortcutCommand.ChatPrevious },
            { "next_chat"         , ShortcutCommand.ChatNext },
            { "first_chat"        , ShortcutCommand.ChatFirst },
            { "last_chat"         , ShortcutCommand.ChatLast },
            { "self_chat"         , ShortcutCommand.ChatSelf },

            { "previous_folder"   , ShortcutCommand.FolderPrevious },
            { "next_folder"       , ShortcutCommand.FolderNext },
            { "all_chats"         , ShortcutCommand.ShowAllChats },

            { "folder1"           , ShortcutCommand.ShowFolder1 },
            { "folder2"           , ShortcutCommand.ShowFolder2 },
            { "folder3"           , ShortcutCommand.ShowFolder3 },
            { "folder4"           , ShortcutCommand.ShowFolder4 },
            { "folder5"           , ShortcutCommand.ShowFolder5 },
            { "folder6"           , ShortcutCommand.ShowFolder6 },
            { "last_folder"       , ShortcutCommand.ShowFolderLast },

            { "show_archive"      , ShortcutCommand.ShowArchive },

			// Shortcuts that have no default values.
			{ "message"           , ShortcutCommand.JustSendMessage },
            { "message_silently"  , ShortcutCommand.SendSilentMessage },
            { "message_scheduled" , ShortcutCommand.ScheduleMessage },
			//
		};

        private readonly Dictionary<ShortcutCommand, string> _commandNames = new Dictionary<ShortcutCommand, string>
        {
            { ShortcutCommand.Close          , "close_telegram" },
            { ShortcutCommand.Lock           , "lock_telegram" },
            { ShortcutCommand.Minimize       , "minimize_telegram" },
            { ShortcutCommand.Quit           , "quit_telegram" },

            //{ ShortcutCommand.MediaPlay      , "media_play" },
            //{ ShortcutCommand.MediaPause     , "media_pause" },
            //{ ShortcutCommand.MediaPlayPause , "media_playpause" },
            //{ ShortcutCommand.MediaStop      , "media_stop" },
            //{ ShortcutCommand.MediaPrevious  , "media_previous" },
            //{ ShortcutCommand.MediaNext      , "media_next" },

            { ShortcutCommand.Search         , "search" },

            { ShortcutCommand.ChatPrevious   , "previous_chat" },
            { ShortcutCommand.ChatNext       , "next_chat" },
            { ShortcutCommand.ChatFirst      , "first_chat" },
            { ShortcutCommand.ChatLast       , "last_chat" },
            { ShortcutCommand.ChatSelf       , "self_chat" },

            { ShortcutCommand.FolderPrevious , "previous_folder" },
            { ShortcutCommand.FolderNext     , "next_folder" },
            { ShortcutCommand.ShowAllChats   , "all_chats" },

            { ShortcutCommand.ShowFolder1    , "folder1" },
            { ShortcutCommand.ShowFolder2    , "folder2" },
            { ShortcutCommand.ShowFolder3    , "folder3" },
            { ShortcutCommand.ShowFolder4    , "folder4" },
            { ShortcutCommand.ShowFolder5    , "folder5" },
            { ShortcutCommand.ShowFolder6    , "folder6" },
            { ShortcutCommand.ShowFolderLast , "last_folder" },

            { ShortcutCommand.ShowArchive    , "show_archive" },
        };

        #endregion

        private readonly Dictionary<Shortcut, List<ShortcutCommand>> _commands = new Dictionary<Shortcut, List<ShortcutCommand>>();

        public ShortcutsService(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            InitializeDefault();
        }

        public IList<ShortcutCommand> Process(AcceleratorKeyEventArgs args)
        {
            if (args.EventType != CoreAcceleratorKeyEventType.KeyDown && args.EventType != CoreAcceleratorKeyEventType.SystemKeyDown)
            {
                return new ShortcutCommand[0];
            }

            var alt = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down);
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            var shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);

            var modifiers = VirtualKeyModifiers.None;
            if (alt)
            {
                modifiers |= VirtualKeyModifiers.Menu;
            }
            if (ctrl)
            {
                modifiers |= VirtualKeyModifiers.Control;
            }
            if (shift)
            {
                modifiers |= VirtualKeyModifiers.Shift;
            }

            var shortcut = new Shortcut(modifiers, args.VirtualKey);
            if (_commands.TryGetValue(shortcut, out var value))
            {
                return value;
            }

            return new ShortcutCommand[0];
        }

        private void InitializeDefault()
        {
            set("ctrl+w", ShortcutCommand.Close);
            set("ctrl+f4", ShortcutCommand.Close);
            set("ctrl+l", ShortcutCommand.Lock);
            set("ctrl+m", ShortcutCommand.Minimize);
            set("ctrl+q", ShortcutCommand.Quit);

            //set("media play", ShortcutCommand.MediaPlay);
            //set("media pause", ShortcutCommand.MediaPause);
            //set("toggle media play/pause", ShortcutCommand.MediaPlayPause);
            //set("media stop", ShortcutCommand.MediaStop);
            //set("media previous", ShortcutCommand.MediaPrevious);
            //set("media next", ShortcutCommand.MediaNext);

            set("ctrl+f", ShortcutCommand.Search);
            set("search", ShortcutCommand.Search);
            //set("find", ShortcutCommand.Search);

            set("ctrl+pgdown", ShortcutCommand.ChatNext);
            set("alt+down", ShortcutCommand.ChatNext);
            set("ctrl+pgup", ShortcutCommand.ChatPrevious);
            set("alt+up", ShortcutCommand.ChatPrevious);

            set("ctrl+tab", ShortcutCommand.ChatNext);
            set("ctrl+shift+tab", ShortcutCommand.ChatPrevious);
            //set("ctrl+backtab", ShortcutCommand.ChatPrevious);

            set("ctrl+alt+home", ShortcutCommand.ChatFirst);
            set("ctrl+alt+end", ShortcutCommand.ChatLast);

            //set("f5", ShortcutCommand.SupportReloadTemplates);
            //set("ctrl+delete", ShortcutCommand.SupportToggleMuted);
            //set("ctrl+insert", ShortcutCommand.SupportScrollToCurrent);
            //set("ctrl+shift+x", ShortcutCommand.SupportHistoryBack);
            //set("ctrl+shift+c", ShortcutCommand.SupportHistoryForward);

            set("ctrl+1", ShortcutCommand.ChatPinned1);
            set("ctrl+2", ShortcutCommand.ChatPinned2);
            set("ctrl+3", ShortcutCommand.ChatPinned3);
            set("ctrl+4", ShortcutCommand.ChatPinned4);
            set("ctrl+5", ShortcutCommand.ChatPinned5);

            for (int i = 0; i < _foldersCommands.Length; i++)
            {
                set($"ctrl+{i + 1}", _foldersCommands[i]);
            }

            set("ctrl+shift+down", ShortcutCommand.FolderNext);
            set("ctrl+shift+up", ShortcutCommand.FolderPrevious);

            set("ctrl+0", ShortcutCommand.ChatSelf);

            set("ctrl+9", ShortcutCommand.ShowArchive);
        }

        private void set(string keys, ShortcutCommand command)
        {
            var shortcut = ParseKeys(keys);
            if (shortcut == null)
            {
                return;
            }

            List<ShortcutCommand> commands;
            if (_commands.ContainsKey(shortcut))
            {
                commands = _commands[shortcut];
            }
            else
            {
                commands = _commands[shortcut] = new List<ShortcutCommand>();
            }

            commands.Add(command);
        }

        private Shortcut ParseKeys(string keys)
        {
            var split = keys.Split('+');

            if (int.TryParse(split[split.Length - 1], out int number))
            {
                split[split.Length - 1] = $"number{number}";
            }
            else if (string.Equals(split[split.Length - 1], "pgdown", StringComparison.OrdinalIgnoreCase))
            {
                split[split.Length - 1] = "pagedown";
            }
            else if (string.Equals(split[split.Length - 1], "pgup", StringComparison.OrdinalIgnoreCase))
            {
                split[split.Length - 1] = "pageup";
            }

            if (Enum.TryParse(split[split.Length - 1], true, out VirtualKey result))
            {
                var modifiers = VirtualKeyModifiers.None;
                var key = result;

                for (int i = 0; i < split.Length - 1; i++)
                {
                    if (string.Equals(split[i], "ctrl", StringComparison.OrdinalIgnoreCase))
                    {
                        split[i] = "control";
                    }
                    else if (string.Equals(split[i], "alt", StringComparison.OrdinalIgnoreCase))
                    {
                        split[i] = "menu";
                    }

                    if (Enum.TryParse(split[i], true, out VirtualKeyModifiers modifier))
                    {
                        modifiers |= modifier;
                    }
                }

                //if (modifiers == VirtualKeyModifiers.None)
                //{
                //    return null;
                //}

                return new Shortcut(modifiers, key);
            }
            else
            {
                return null;
            }
        }

        private Shortcut ParseMediaKeys(string keys)
        {
            switch (keys.ToLower())
            {
                case "media play":
                case "media pause":
                case "toggle media play/pause":
                case "media stop":
                case "media previous":
                case "media next":
                    break;
            }

            return null;
        }
    }

    public class Shortcut
    {
        public VirtualKeyModifiers Modifiers { get; private set; }
        public VirtualKey Key { get; private set; }

        public Shortcut(VirtualKeyModifiers modifiers, VirtualKey key)
        {
            Modifiers = modifiers;
            Key = key;
        }

        public override bool Equals(object obj)
        {
            if (obj is Shortcut shortcut)
            {
                return shortcut.Modifiers == Modifiers
                    && shortcut.Key == Key;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Modifiers.GetHashCode()
                ^ Key.GetHashCode();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var modifiers = Enum.GetValues(typeof(VirtualKeyModifiers))
                .Cast<VirtualKeyModifiers>()
                .Where(v => v != VirtualKeyModifiers.None && Modifiers.HasFlag(v));

            foreach (var key in modifiers)
            {
                builder.Append($"{key}+");
            }

            builder.Append(Key);

            return builder.ToString();
        }
    }

    public enum ShortcutCommand
    {
        Close,
        Lock,
        Minimize,
        Quit,

        //MediaPlay,
        //MediaPause,
        //MediaPlayPause,
        //MediaStop,
        //MediaPrevious,
        //MediaNext,

        Search,

        ChatPrevious,
        ChatNext,
        ChatFirst,
        ChatLast,
        ChatSelf,
        ChatPinned1,
        ChatPinned2,
        ChatPinned3,
        ChatPinned4,
        ChatPinned5,

        ShowAllChats,
        ShowFolder1,
        ShowFolder2,
        ShowFolder3,
        ShowFolder4,
        ShowFolder5,
        ShowFolder6,
        ShowFolderLast,

        FolderNext,
        FolderPrevious,

        ShowArchive,

        JustSendMessage,
        SendSilentMessage,
        ScheduleMessage,

        //SupportReloadTemplates,
        //SupportToggleMuted,
        //SupportScrollToCurrent,
        //SupportHistoryBack,
        //SupportHistoryForward,
    }
}
