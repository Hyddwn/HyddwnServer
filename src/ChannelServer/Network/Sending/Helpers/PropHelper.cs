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

				// [200200, NA242 (2016-12-15)]
				// The playing effect for instruments was turned into a prop,
				// presumably to have something to reference in the world
				// for jams, and to make it more than a temp effect.
				if (prop is PlayingInstrumentProp)
				{
					var piProp = (prop as PlayingInstrumentProp);

					// This part is basically the old play effect
					packet.PutByte(piProp.HasMML);
					packet.PutString(piProp.CompressedMML);
					packet.PutInt(piProp.ScoreId);
					packet.PutInt(2);
					packet.PutShort(0);
					packet.PutInt(22124);
					packet.PutByte((byte)piProp.Quality); // Originally 0~3, now 0~100
					packet.PutByte((byte)piProp.Instrument);
					packet.PutByte(0);
					packet.PutByte(0);
					packet.PutByte(1); // loops?

					// This part is new in MusicQ
					packet.PutByte(0);
					packet.PutByte(1);
					packet.PutLong(piProp.StartTime);
					packet.PutLong(0); // Jam leader or something like that?
					packet.PutByte(0);
					packet.PutInt(0);
					packet.PutLong(piProp.CreatureEntityId);
					packet.PutInt(0);
					packet.PutByte(0);
					packet.PutInt(0);

				}
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
