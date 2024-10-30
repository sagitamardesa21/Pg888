// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL.Account.Methods
{
	/// <summary>
	/// RCP method account.unregisterDevice.
	/// Returns <see cref="Telegram.Api.TL.TLBool"/>
	/// </summary>
	public partial class TLAccountUnregisterDevice : TLObject
	{
		public Int32 TokenType { get; set; }
		public String Token { get; set; }
		public TLVector<Int32> OtherUids { get; set; }

		public TLAccountUnregisterDevice() { }
		public TLAccountUnregisterDevice(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.AccountUnregisterDevice; } }

		public override void Read(TLBinaryReader from)
		{
			TokenType = from.ReadInt32();
			Token = from.ReadString();
			OtherUids = TLFactory.Read<TLVector<Int32>>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteInt32(TokenType);
			to.WriteString(Token ?? string.Empty);
			to.WriteObject(OtherUids);
		}
	}
}