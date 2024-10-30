// <auto-generated/>
using System;

namespace Telegram.Api.TL.Messages.Methods
{
	/// <summary>
	/// RCP method messages.getRecentStickers.
	/// Returns <see cref="Telegram.Api.TL.TLMessagesRecentStickersBase"/>
	/// </summary>
	public partial class TLMessagesGetRecentStickers : TLObject
	{
		[Flags]
		public enum Flag : Int32
		{
			Attached = (1 << 0),
		}

		public bool IsAttached { get { return Flags.HasFlag(Flag.Attached); } set { Flags = value ? (Flags | Flag.Attached) : (Flags & ~Flag.Attached); } }

		public Flag Flags { get; set; }
		public Int32 Hash { get; set; }

		public TLMessagesGetRecentStickers() { }
		public TLMessagesGetRecentStickers(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.MessagesGetRecentStickers; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			Hash = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x5EA192C9);
			to.Write((Int32)Flags);
			to.Write(Hash);
		}
	}
}