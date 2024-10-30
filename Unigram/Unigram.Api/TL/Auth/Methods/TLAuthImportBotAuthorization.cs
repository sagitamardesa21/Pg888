// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL.Auth.Methods
{
	/// <summary>
	/// RCP method auth.importBotAuthorization.
	/// Returns <see cref="Telegram.Api.TL.TLAuthAuthorization"/>
	/// </summary>
	public partial class TLAuthImportBotAuthorization : TLObject
	{
		public Int32 Flags { get; set; }
		public Int32 ApiId { get; set; }
		public String ApiHash { get; set; }
		public String BotAuthToken { get; set; }

		public TLAuthImportBotAuthorization() { }
		public TLAuthImportBotAuthorization(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.AuthImportBotAuthorization; } }

		public override void Read(TLBinaryReader from)
		{
			Flags = from.ReadInt32();
			ApiId = from.ReadInt32();
			ApiHash = from.ReadString();
			BotAuthToken = from.ReadString();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteInt32(Flags);
			to.WriteInt32(ApiId);
			to.WriteString(ApiHash ?? string.Empty);
			to.WriteString(BotAuthToken ?? string.Empty);
		}
	}
}