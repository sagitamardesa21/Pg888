// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLInputMediaUploadedPhoto : TLInputMediaBase 
	{
		[Flags]
		public enum Flag : Int32
		{
			Stickers = (1 << 0),
			TTLSeconds = (1 << 1),
		}

		public bool HasStickers { get { return Flags.HasFlag(Flag.Stickers); } set { Flags = value ? (Flags | Flag.Stickers) : (Flags & ~Flag.Stickers); } }
		public bool HasTTLSeconds { get { return Flags.HasFlag(Flag.TTLSeconds); } set { Flags = value ? (Flags | Flag.TTLSeconds) : (Flags & ~Flag.TTLSeconds); } }

		public Flag Flags { get; set; }
		public TLInputFileBase File { get; set; }
		public TLVector<TLInputDocumentBase> Stickers { get; set; }
		public Int32? TTLSeconds { get; set; }

		public TLInputMediaUploadedPhoto() { }
		public TLInputMediaUploadedPhoto(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.InputMediaUploadedPhoto; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			File = TLFactory.Read<TLInputFileBase>(from);
			if (HasStickers) Stickers = TLFactory.Read<TLVector<TLInputDocumentBase>>(from);
			if (HasTTLSeconds) TTLSeconds = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			UpdateFlags();

			to.WriteInt32((Int32)Flags);
			to.WriteObject(File);
			if (HasStickers) to.WriteObject(Stickers);
			if (HasTTLSeconds) to.WriteInt32(TTLSeconds.Value);
		}

		private void UpdateFlags()
		{
			HasStickers = Stickers != null;
			HasTTLSeconds = TTLSeconds != null;
		}
	}
}