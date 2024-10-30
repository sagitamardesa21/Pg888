// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLServerDHInnerData : TLObject 
	{
		public TLInt128 Nonce { get; set; }
		public TLInt128 ServerNonce { get; set; }
		public Int32 G { get; set; }
		public Byte[] DHPrime { get; set; }
		public Byte[] GA { get; set; }
		public Int32 ServerTime { get; set; }

		public TLServerDHInnerData() { }
		public TLServerDHInnerData(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.ServerDHInnerData; } }

		public override void Read(TLBinaryReader from)
		{
			Nonce = new TLInt128(from);
			ServerNonce = new TLInt128(from);
			G = from.ReadInt32();
			DHPrime = from.ReadByteArray();
			GA = from.ReadByteArray();
			ServerTime = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0xB5890DBA);
			to.WriteObject(Nonce);
			to.WriteObject(ServerNonce);
			to.Write(G);
			to.WriteByteArray(DHPrime);
			to.WriteByteArray(GA);
			to.Write(ServerTime);
		}
	}
}