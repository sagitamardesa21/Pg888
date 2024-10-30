// <auto-generated/>
using System;

namespace Telegram.Api.TL.Payments
{
	public partial class TLPaymentsSavedInfo : TLObject 
	{
		[Flags]
		public enum Flag : Int32
		{
			HasSavedCredentials = (1 << 1),
			SavedInfo = (1 << 0),
		}

		public bool IsHasSavedCredentials { get { return Flags.HasFlag(Flag.HasSavedCredentials); } set { Flags = value ? (Flags | Flag.HasSavedCredentials) : (Flags & ~Flag.HasSavedCredentials); } }
		public bool HasSavedInfo { get { return Flags.HasFlag(Flag.SavedInfo); } set { Flags = value ? (Flags | Flag.SavedInfo) : (Flags & ~Flag.SavedInfo); } }

		public Flag Flags { get; set; }
		public TLPaymentRequestedInfo SavedInfo { get; set; }

		public TLPaymentsSavedInfo() { }
		public TLPaymentsSavedInfo(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.PaymentsSavedInfo; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			if (HasSavedInfo) SavedInfo = TLFactory.Read<TLPaymentRequestedInfo>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			UpdateFlags();

			to.Write(0xFB8FE43C);
			to.Write((Int32)Flags);
			if (HasSavedInfo) to.WriteObject(SavedInfo);
		}

		private void UpdateFlags()
		{
			HasSavedInfo = SavedInfo != null;
		}
	}
}