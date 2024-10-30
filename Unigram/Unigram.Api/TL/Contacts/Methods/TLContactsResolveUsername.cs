// <auto-generated/>
using System;

namespace Telegram.Api.TL.Contacts.Methods
{
	/// <summary>
	/// RCP method contacts.resolveUsername.
	/// Returns <see cref="Telegram.Api.TL.TLContactsResolvedPeer"/>
	/// </summary>
	public partial class TLContactsResolveUsername : TLObject
	{
		public String Username { get; set; }

		public TLContactsResolveUsername() { }
		public TLContactsResolveUsername(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.ContactsResolveUsername; } }

		public override void Read(TLBinaryReader from)
		{
			Username = from.ReadString();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0xF93CCBA3);
			to.Write(Username);
		}
	}
}