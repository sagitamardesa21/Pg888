﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
    public partial class TLMessageActionDate : TLMessageActionBase
    {
        public Int32 Date { get; set; }

        // FFFFFF10

        public TLMessageActionDate() { }
        public TLMessageActionDate(TLBinaryReader from)
        {
            Read(from);
        }

        public override TLType TypeId { get { return (TLType)0xFFFFFF10; } }

        public override void Read(TLBinaryReader from)
        {
            Date = from.ReadInt32();
        }

        public override void Write(TLBinaryWriter to)
        {
            to.WriteUInt32(0xFFFFFF10);
            to.WriteInt32(Date);
        }
    }
}
