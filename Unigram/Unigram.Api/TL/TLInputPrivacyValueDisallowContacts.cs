// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLInputPrivacyValueDisallowContacts : TLInputPrivacyRuleBase 
	{
		public TLInputPrivacyValueDisallowContacts() { }
		public TLInputPrivacyValueDisallowContacts(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.InputPrivacyValueDisallowContacts; } }

		public override void Read(TLBinaryReader from)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0xBA52007);
		}
	}
}