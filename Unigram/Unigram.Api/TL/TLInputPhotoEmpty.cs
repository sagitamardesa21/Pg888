// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLInputPhotoEmpty : TLInputPhotoBase 
	{
		public TLInputPhotoEmpty() { }
		public TLInputPhotoEmpty(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.InputPhotoEmpty; } }

		public override void Read(TLBinaryReader from)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x1CD7BF0D);
		}
	}
}