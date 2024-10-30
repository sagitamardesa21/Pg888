// <auto-generated/>
using System;

namespace Telegram.Api.TL.Help.Methods
{
	/// <summary>
	/// RCP method help.getSupport.
	/// Returns <see cref="Telegram.Api.TL.TLHelpSupport"/>
	/// </summary>
	public partial class TLHelpGetSupport : TLObject
	{
		public TLHelpGetSupport() { }
		public TLHelpGetSupport(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.HelpGetSupport; } }

		public override void Read(TLBinaryReader from)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x9CDF08CD);
		}
	}
}