// <auto-generated/>
using System;

namespace Telegram.Api.TL.Contacts.Methods
{
	/// <summary>
	/// RCP method contacts.deleteContact.
	/// Returns <see cref="Telegram.Api.TL.TLContactsLink"/>
	/// </summary>
	public partial class TLContactsDeleteContact : TLObject
	{
		public TLInputUserBase Id { get; set; }

		public TLContactsDeleteContact() { }
		public TLContactsDeleteContact(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.ContactsDeleteContact; } }

		public override void Read(TLBinaryReader from)
		{
			Id = TLFactory.Read<TLInputUserBase>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x8E953744);
			to.WriteObject(Id);
		}
	}
}