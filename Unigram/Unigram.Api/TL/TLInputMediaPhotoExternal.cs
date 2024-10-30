// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLInputMediaPhotoExternal : TLInputMediaBase, ITLMessageMediaCaption 
	{
		[Flags]
		public enum Flag : Int32
		{
			TTLSeconds = (1 << 0),
		}

		public bool HasTTLSeconds { get { return Flags.HasFlag(Flag.TTLSeconds); } set { Flags = value ? (Flags | Flag.TTLSeconds) : (Flags & ~Flag.TTLSeconds); } }

		public Flag Flags { get; set; }
		public String Url { get; set; }
		public String Caption { get; set; }
		public Int32? TTLSeconds { get; set; }

		public TLInputMediaPhotoExternal() { }
		public TLInputMediaPhotoExternal(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.InputMediaPhotoExternal; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			Url = from.ReadString();
			Caption = from.ReadString();
			if (HasTTLSeconds) TTLSeconds = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			UpdateFlags();

			to.Write(0x922AEC1);
			to.Write((Int32)Flags);
			to.Write(Url);
			to.Write(Caption);
			if (HasTTLSeconds) to.Write(TTLSeconds.Value);
		}

		private void UpdateFlags()
		{
			HasTTLSeconds = TTLSeconds != null;
		}
	}
}