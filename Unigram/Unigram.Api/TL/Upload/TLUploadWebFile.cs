// <auto-generated/>
using System;
using Telegram.Api.TL.Storage;

namespace Telegram.Api.TL.Upload
{
	public partial class TLUploadWebFile : TLObject 
	{
		public Int32 Size { get; set; }
		public String MimeType { get; set; }
		public TLStorageFileTypeBase FileType { get; set; }
		public Int32 Mtime { get; set; }
		public Byte[] Bytes { get; set; }

		public TLUploadWebFile() { }
		public TLUploadWebFile(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.UploadWebFile; } }

		public override void Read(TLBinaryReader from)
		{
			Size = from.ReadInt32();
			MimeType = from.ReadString();
			FileType = TLFactory.Read<TLStorageFileTypeBase>(from);
			Mtime = from.ReadInt32();
			Bytes = from.ReadByteArray();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x21E753BC);
			to.Write(Size);
			to.Write(MimeType);
			to.WriteObject(FileType);
			to.Write(Mtime);
			to.WriteByteArray(Bytes);
		}
	}
}