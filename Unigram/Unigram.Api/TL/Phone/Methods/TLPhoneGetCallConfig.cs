// <auto-generated/>
using System;

namespace Telegram.Api.TL.Phone.Methods
{
	/// <summary>
	/// RCP method phone.getCallConfig.
	/// Returns <see cref="Telegram.Api.TL.TLDataJSON"/>
	/// </summary>
	public partial class TLPhoneGetCallConfig : TLObject
	{
		public TLPhoneGetCallConfig() { }
		public TLPhoneGetCallConfig(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.PhoneGetCallConfig; } }

		public override void Read(TLBinaryReader from)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x55451FA9);
		}
	}
}