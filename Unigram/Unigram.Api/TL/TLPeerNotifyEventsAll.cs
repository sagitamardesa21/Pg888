// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLPeerNotifyEventsAll : TLPeerNotifyEventsBase 
	{
		public TLPeerNotifyEventsAll() { }
		public TLPeerNotifyEventsAll(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.PeerNotifyEventsAll; } }

		public override void Read(TLBinaryReader from)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x6D1DED88);
		}
	}
}