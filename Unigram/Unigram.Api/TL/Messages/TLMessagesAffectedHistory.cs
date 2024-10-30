// <auto-generated/>
using System;

namespace Telegram.Api.TL.Messages
{
	public partial class TLMessagesAffectedHistory : TLObject, ITLMultiPts 
	{
		public Int32 Pts { get; set; }
		public Int32 PtsCount { get; set; }
		public Int32 Offset { get; set; }

		public TLMessagesAffectedHistory() { }
		public TLMessagesAffectedHistory(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.MessagesAffectedHistory; } }

		public override void Read(TLBinaryReader from)
		{
			Pts = from.ReadInt32();
			PtsCount = from.ReadInt32();
			Offset = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0xB45C69D1);
			to.Write(Pts);
			to.Write(PtsCount);
			to.Write(Offset);
		}
	}
}