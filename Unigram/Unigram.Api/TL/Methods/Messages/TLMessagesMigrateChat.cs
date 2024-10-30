// <auto-generated/>
using System;

namespace Telegram.Api.TL.Methods.Messages
{
	/// <summary>
	/// RCP method messages.migrateChat.
	/// Returns <see cref="Telegram.Api.TL.TLUpdatesBase"/>
	/// </summary>
	public partial class TLMessagesMigrateChat : TLObject
	{
		public Int32 ChatId { get; set; }

		public TLMessagesMigrateChat() { }
		public TLMessagesMigrateChat(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.MessagesMigrateChat; } }

		public override void Read(TLBinaryReader from)
		{
			ChatId = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x15A3B8E3);
			to.Write(ChatId);
		}
	}
}