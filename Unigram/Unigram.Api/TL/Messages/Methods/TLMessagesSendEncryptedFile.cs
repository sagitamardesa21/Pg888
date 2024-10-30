// <auto-generated/>
using System;

namespace Telegram.Api.TL.Messages.Methods
{
	/// <summary>
	/// RCP method messages.sendEncryptedFile.
	/// Returns <see cref="Telegram.Api.TL.TLMessagesSentEncryptedMessage"/>
	/// </summary>
	public partial class TLMessagesSendEncryptedFile : TLObject
	{
		public TLInputEncryptedChat Peer { get; set; }
		public Int64 RandomId { get; set; }
		public Byte[] Data { get; set; }
		public TLInputEncryptedFileBase File { get; set; }

		public TLMessagesSendEncryptedFile() { }
		public TLMessagesSendEncryptedFile(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.MessagesSendEncryptedFile; } }

		public override void Read(TLBinaryReader from)
		{
			Peer = TLFactory.Read<TLInputEncryptedChat>(from);
			RandomId = from.ReadInt64();
			Data = from.ReadByteArray();
			File = TLFactory.Read<TLInputEncryptedFileBase>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x9A901B66);
			to.WriteObject(Peer);
			to.Write(RandomId);
			to.WriteByteArray(Data);
			to.WriteObject(File);
		}
	}
}