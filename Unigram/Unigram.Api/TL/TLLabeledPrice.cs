// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLLabeledPrice : TLObject 
	{
		public String Label { get; set; }
		public Int64 Amount { get; set; }

		public TLLabeledPrice() { }
		public TLLabeledPrice(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.LabeledPrice; } }

		public override void Read(TLBinaryReader from)
		{
			Label = from.ReadString();
			Amount = from.ReadInt64();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteString(Label ?? string.Empty);
			to.WriteInt64(Amount);
		}
	}
}