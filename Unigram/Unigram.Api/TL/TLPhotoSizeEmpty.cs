// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLPhotoSizeEmpty : TLPhotoSizeBase 
	{
		public TLPhotoSizeEmpty() { }
		public TLPhotoSizeEmpty(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.PhotoSizeEmpty; } }

		public override void Read(TLBinaryReader from)
		{
			Type = from.ReadString();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0xE17E23C);
			to.Write(Type);
		}
	}
}