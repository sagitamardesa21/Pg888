// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLUpdateReadChannelOutbox : TLUpdateBase 
	{
		public Int32 ChannelId { get; set; }
		public Int32 MaxId { get; set; }

		public TLUpdateReadChannelOutbox() { }
		public TLUpdateReadChannelOutbox(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.UpdateReadChannelOutbox; } }

		public override void Read(TLBinaryReader from)
		{
			ChannelId = from.ReadInt32();
			MaxId = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteInt32(ChannelId);
			to.WriteInt32(MaxId);
		}
	}
}