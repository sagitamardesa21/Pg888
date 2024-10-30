// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLPhotosPhotos : TLPhotosPhotosBase 
	{
		public TLPhotosPhotos() { }
		public TLPhotosPhotos(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.PhotosPhotos; } }

		public override void Read(TLBinaryReader from)
		{
			Photos = TLFactory.Read<TLVector<TLPhotoBase>>(from);
			Users = TLFactory.Read<TLVector<TLUserBase>>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x8DCA6AA5);
			to.WriteObject(Photos);
			to.WriteObject(Users);
		}
	}
}