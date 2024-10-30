﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
    public class TLActionInfo : TLObject
    {
        public Int32 SendBefore { get; set; }
        public TLObject Action { get; set; }

        public TLActionInfo() { }
        public TLActionInfo(TLBinaryReader from)
        {
            Read(from);
        }

        public override string ToString()
        {
            return string.Format("send_before={0} action={1}", SendBefore, Action);
        }

        public override void Read(TLBinaryReader from)
        {
            SendBefore = from.ReadInt32();
            Action = TLFactory.Read<TLObject>(from);
        }

        public override void Write(TLBinaryWriter to)
        {
            to.WriteUInt32(0xFFFFFF0D);
            to.WriteInt32(SendBefore);
            Action.Write(to);
        }
    }
}
