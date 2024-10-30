﻿//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;

namespace Telegram.Td.Api
{
    public class MessageHeaderUnread : MessageContent
    {
        public NativeObject ToUnmanaged()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return nameof(MessageHeaderUnread);
        }
    }
}
