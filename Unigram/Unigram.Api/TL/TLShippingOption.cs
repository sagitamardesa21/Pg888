// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLShippingOption : TLObject 
	{
		public String Id { get; set; }
		public String Title { get; set; }
		public TLVector<TLLabeledPrice> Prices { get; set; }

		public TLShippingOption() { }
		public TLShippingOption(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.ShippingOption; } }

		public override void Read(TLBinaryReader from)
		{
			Id = from.ReadString();
			Title = from.ReadString();
			Prices = TLFactory.Read<TLVector<TLLabeledPrice>>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteString(Id ?? string.Empty);
			to.WriteString(Title ?? string.Empty);
			to.WriteObject(Prices);
		}
	}
}