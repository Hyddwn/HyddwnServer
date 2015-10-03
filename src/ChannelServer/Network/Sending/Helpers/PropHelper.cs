// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using System;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class PropHelper
	{
		public static Packet AddPropInfo(this Packet packet, Prop prop)
		{
			packet.PutLong(prop.EntityId);
			packet.PutInt(prop.Info.Id);

			// Client side props (A0 range, instead of A1)
			// look a bit different.
			if (prop.ServerSide)
			{
				packet.PutString(prop.Ident);
				packet.PutString(prop.Title);
				packet.PutBin(prop.Info);

				packet.PutString(prop.State);
				packet.PutLong(0);

				packet.PutByte(prop.HasXml);
				if (prop.HasXml)
					packet.PutString(prop.Xml.ToString());

				var extensions = prop.Extensions.GetList();
				packet.PutInt(extensions.Count);
				foreach (var extension in extensions)
				{
					packet.PutInt((int)extension.SignalType);
					packet.PutInt((int)extension.EventType);
					packet.PutString(extension.Name);
					packet.PutByte(extension.Mode);
					packet.PutString(extension.Value.ToString());
				}

				packet.PutShort(0);
			}
			else
			{
				packet.PutString(prop.State);
				packet.PutLong(DateTime.Now);

				packet.PutByte(prop.HasXml);
				if (prop.HasXml)
					packet.PutString(prop.Xml.ToString());

				packet.PutFloat(prop.Info.Direction);

				// Done't add if there aren't any.
				if (prop.Extensions.HasAny)
				{
					var extensions = prop.Extensions.GetList();

					packet.PutInt(extensions.Count);
					foreach (var extension in extensions)
					{
						packet.PutInt((int)extension.SignalType);
						packet.PutInt((int)extension.EventType);
						packet.PutString(extension.Name);
						packet.PutByte(extension.Mode);
						packet.PutString(extension.Value.ToString());
					}
				}
			}

			return packet;
		}

		public static Packet AddPropUpdateInfo(this Packet packet, Prop prop)
		{
			packet.PutString(prop.State);
			packet.PutLong(DateTime.Now);

			packet.PutByte(prop.HasXml);
			if (prop.HasXml)
				packet.PutString(prop.Xml.ToString());

			packet.PutFloat(prop.Info.Direction);
			packet.PutShort(0);

			return packet;
		}
	}
}
