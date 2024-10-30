// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL.Updates
{
	public partial class TLUpdatesChannelDifference : TLUpdatesChannelDifferenceBase 
	{
		[Flags]
		public enum Flag : Int32
		{
			Final = (1 << 0),
			Timeout = (1 << 1),
		}

		public override bool IsFinal { get { return Flags.HasFlag(Flag.Final); } set { Flags = value ? (Flags | Flag.Final) : (Flags & ~Flag.Final); } }
		public bool HasTimeout { get { return Flags.HasFlag(Flag.Timeout); } set { Flags = value ? (Flags | Flag.Timeout) : (Flags & ~Flag.Timeout); } }

		public Flag Flags { get; set; }
		public TLVector<TLMessageBase> NewMessages { get; set; }
		public TLVector<TLUpdateBase> OtherUpdates { get; set; }
		public TLVector<TLChatBase> Chats { get; set; }
		public TLVector<TLUserBase> Users { get; set; }

		public TLUpdatesChannelDifference() { }
		public TLUpdatesChannelDifference(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.UpdatesChannelDifference; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			Pts = from.ReadInt32();
			if (HasTimeout) Timeout = from.ReadInt32();
			NewMessages = TLFactory.Read<TLVector<TLMessageBase>>(from);
			OtherUpdates = TLFactory.Read<TLVector<TLUpdateBase>>(from);
			Chats = TLFactory.Read<TLVector<TLChatBase>>(from);
			Users = TLFactory.Read<TLVector<TLUserBase>>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			UpdateFlags();

			to.WriteInt32((Int32)Flags);
			to.WriteInt32(Pts);
			if (HasTimeout) to.WriteInt32(Timeout.Value);
			to.WriteObject(NewMessages);
			to.WriteObject(OtherUpdates);
			to.WriteObject(Chats);
			to.WriteObject(Users);
		}

		private void UpdateFlags()
		{
			HasTimeout = Timeout != null;
		}
	}
}