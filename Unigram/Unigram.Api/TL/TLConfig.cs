// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLConfig : TLObject 
	{
		[Flags]
		public enum Flag : Int32
		{
			PhoneCallsEnabled = (1 << 1),
			TmpSessions = (1 << 0),
		}

		public bool IsPhoneCallsEnabled { get { return Flags.HasFlag(Flag.PhoneCallsEnabled); } set { Flags = value ? (Flags | Flag.PhoneCallsEnabled) : (Flags & ~Flag.PhoneCallsEnabled); } }
		public bool HasTmpSessions { get { return Flags.HasFlag(Flag.TmpSessions); } set { Flags = value ? (Flags | Flag.TmpSessions) : (Flags & ~Flag.TmpSessions); } }

		public Flag Flags { get; set; }
		public Int32 Date { get; set; }
		public Int32 Expires { get; set; }
		public Boolean TestMode { get; set; }
		public Int32 ThisDC { get; set; }
		public TLVector<TLDCOption> DCOptions { get; set; }
		public Int32 ChatSizeMax { get; set; }
		public Int32 MegaGroupSizeMax { get; set; }
		public Int32 ForwardedCountMax { get; set; }
		public Int32 OnlineUpdatePeriodMs { get; set; }
		public Int32 OfflineBlurTimeoutMs { get; set; }
		public Int32 OfflineIdleTimeoutMs { get; set; }
		public Int32 OnlineCloudTimeoutMs { get; set; }
		public Int32 NotifyCloudDelayMs { get; set; }
		public Int32 NotifyDefaultDelayMs { get; set; }
		public Int32 ChatBigSize { get; set; }
		public Int32 PushChatPeriodMs { get; set; }
		public Int32 PushChatLimit { get; set; }
		public Int32 SavedGifsLimit { get; set; }
		public Int32 EditTimeLimit { get; set; }
		public Int32 RatingEDecay { get; set; }
		public Int32 StickersRecentLimit { get; set; }
		public Int32? TmpSessions { get; set; }
		public Int32 PinnedDialogsCountMax { get; set; }
		public Int32 CallReceiveTimeoutMs { get; set; }
		public Int32 CallRingTimeoutMs { get; set; }
		public Int32 CallConnectTimeoutMs { get; set; }
		public Int32 CallPacketTimeoutMs { get; set; }
		public String MeUrlPrefix { get; set; }
		public TLVector<TLDisabledFeature> DisabledFeatures { get; set; }

		public TLConfig() { }
		public TLConfig(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.Config; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = (Flag)from.ReadInt32();
			Date = from.ReadInt32();
			Expires = from.ReadInt32();
			TestMode = from.ReadBoolean();
			ThisDC = from.ReadInt32();
			DCOptions = TLFactory.Read<TLVector<TLDCOption>>(from);
			ChatSizeMax = from.ReadInt32();
			MegaGroupSizeMax = from.ReadInt32();
			ForwardedCountMax = from.ReadInt32();
			OnlineUpdatePeriodMs = from.ReadInt32();
			OfflineBlurTimeoutMs = from.ReadInt32();
			OfflineIdleTimeoutMs = from.ReadInt32();
			OnlineCloudTimeoutMs = from.ReadInt32();
			NotifyCloudDelayMs = from.ReadInt32();
			NotifyDefaultDelayMs = from.ReadInt32();
			ChatBigSize = from.ReadInt32();
			PushChatPeriodMs = from.ReadInt32();
			PushChatLimit = from.ReadInt32();
			SavedGifsLimit = from.ReadInt32();
			EditTimeLimit = from.ReadInt32();
			RatingEDecay = from.ReadInt32();
			StickersRecentLimit = from.ReadInt32();
			if (HasTmpSessions) TmpSessions = from.ReadInt32();
			PinnedDialogsCountMax = from.ReadInt32();
			CallReceiveTimeoutMs = from.ReadInt32();
			CallRingTimeoutMs = from.ReadInt32();
			CallConnectTimeoutMs = from.ReadInt32();
			CallPacketTimeoutMs = from.ReadInt32();
			MeUrlPrefix = from.ReadString();
			DisabledFeatures = TLFactory.Read<TLVector<TLDisabledFeature>>(from);
		}

		public override void Write(TLBinaryWriter to)
		{
			UpdateFlags();

			to.Write(0xCB601684);
			to.Write((Int32)Flags);
			to.Write(Date);
			to.Write(Expires);
			to.Write(TestMode);
			to.Write(ThisDC);
			to.WriteObject(DCOptions);
			to.Write(ChatSizeMax);
			to.Write(MegaGroupSizeMax);
			to.Write(ForwardedCountMax);
			to.Write(OnlineUpdatePeriodMs);
			to.Write(OfflineBlurTimeoutMs);
			to.Write(OfflineIdleTimeoutMs);
			to.Write(OnlineCloudTimeoutMs);
			to.Write(NotifyCloudDelayMs);
			to.Write(NotifyDefaultDelayMs);
			to.Write(ChatBigSize);
			to.Write(PushChatPeriodMs);
			to.Write(PushChatLimit);
			to.Write(SavedGifsLimit);
			to.Write(EditTimeLimit);
			to.Write(RatingEDecay);
			to.Write(StickersRecentLimit);
			if (HasTmpSessions) to.Write(TmpSessions.Value);
			to.Write(PinnedDialogsCountMax);
			to.Write(CallReceiveTimeoutMs);
			to.Write(CallRingTimeoutMs);
			to.Write(CallConnectTimeoutMs);
			to.Write(CallPacketTimeoutMs);
			to.Write(MeUrlPrefix);
			to.WriteObject(DisabledFeatures);
		}

		private void UpdateFlags()
		{
			HasTmpSessions = TmpSessions != null;
		}
	}
}