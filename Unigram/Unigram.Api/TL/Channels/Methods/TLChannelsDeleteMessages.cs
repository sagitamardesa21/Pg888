// <auto-generated/>
using System;

namespace Telegram.Api.TL.Channels.Methods
{
	/// <summary>
	/// RCP method channels.deleteMessages.
	/// Returns <see cref="Telegram.Api.TL.TLMessagesAffectedMessages"/>
	/// </summary>
	public partial class TLChannelsDeleteMessages : TLObject
	{
		public TLInputChannelBase Channel { get; set; }
		public TLVector<Int32> Id { get; set; }

		public TLChannelsDeleteMessages() { }
		public TLChannelsDeleteMessages(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.ChannelsDeleteMessages; } }

		public override void Read(TLBinaryReader from)
		{
			Channel = TLFactory.Read<TLInputChannelBase>(from);
			Id = TLFactory.Read<TLVector<Int32>>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x84C1FD4E);
			to.WriteObject(Channel);
			to.WriteObject(Id);
		}
	}
}