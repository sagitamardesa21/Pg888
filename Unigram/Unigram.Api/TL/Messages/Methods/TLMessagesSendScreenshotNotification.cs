// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL.Messages.Methods
{
	/// <summary>
	/// RCP method messages.sendScreenshotNotification.
	/// Returns <see cref="Telegram.Api.TL.TLUpdatesBase"/>
	/// </summary>
	public partial class TLMessagesSendScreenshotNotification : TLObject
	{
		public TLInputPeerBase Peer { get; set; }
		public Int32 ReplyToMsgId { get; set; }
		public Int64 RandomId { get; set; }

		public TLMessagesSendScreenshotNotification() { }
		public TLMessagesSendScreenshotNotification(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.MessagesSendScreenshotNotification; } }

		public override void Read(TLBinaryReader from)
		{
			Peer = TLFactory.Read<TLInputPeerBase>(from);
			ReplyToMsgId = from.ReadInt32();
			RandomId = from.ReadInt64();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteObject(Peer);
			to.WriteInt32(ReplyToMsgId);
			to.WriteInt64(RandomId);
		}
	}
}