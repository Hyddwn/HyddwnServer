// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Hidden
{
	[Skill(SkillId.NameColorChange)]
	public class NameColorChange : IPreparable, IUseable, ICompletable, ICancelable
	{
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var dict = packet.GetString();

			// Get item entity id
			creature.Temp.NameColorItemEntityId = MabiDictionary.Fetch<long>("ITEMID", dict);
			if (creature.Temp.NameColorItemEntityId == 0)
			{
				Log.Warning("NameColorChange: Invalid item id '{0}' from creature '{1:X16}'.", creature.Temp.NameColorItemEntityId, creature.EntityId);
				return false;
			}

			// Go into ready mode
			Send.SkillReady(creature, skill.Info.Id, dict);
			skill.State = SkillState.Ready;

			return true;
		}

		public void Use(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillUse(creature, skill.Info.Id);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
