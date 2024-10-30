// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLChannelAdminLogEventActionDeleteMessage : TLChannelAdminLogEventActionBase 
	{
		public TLMessageBase Message { get; set; }

		public TLChannelAdminLogEventActionDeleteMessage() { }
		public TLChannelAdminLogEventActionDeleteMessage(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.ChannelAdminLogEventActionDeleteMessage; } }

		public override void Read(TLBinaryReader from)
		{
			Message = TLFactory.Read<TLMessageBase>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteObject(Message);
		}
	}
}