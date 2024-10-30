// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLUpdateChannelWebPage : TLUpdateBase 
	{
		public Int32 ChannelId { get; set; }
		public TLWebPageBase WebPage { get; set; }
		public Int32 Pts { get; set; }
		public Int32 PtsCount { get; set; }

		public TLUpdateChannelWebPage() { }
		public TLUpdateChannelWebPage(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.UpdateChannelWebPage; } }

		public override void Read(TLBinaryReader from)
		{
			ChannelId = from.ReadInt32();
			WebPage = TLFactory.Read<TLWebPageBase>(from);
			Pts = from.ReadInt32();
			PtsCount = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x40771900);
			to.Write(ChannelId);
			to.WriteObject(WebPage);
			to.Write(Pts);
			to.Write(PtsCount);
		}
	}
}