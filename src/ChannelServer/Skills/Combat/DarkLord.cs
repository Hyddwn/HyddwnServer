// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Used by Dark Lord, teleports user behind the target.
	/// </summary>
	/// <remarks>
	/// This implementation is guessed. It can be used by players and
	/// monsters, but it's unlikely that it's official.
	/// </remarks>
	[Skill(SkillId.DarkLord)]
	public class DarkLordSkill : IPreparable, ICompletable, ICancelable
	{
		private const int DistanceToTarget = 100;

		/// <summary>
		/// Prepares skill, goes straight to use to skip readying and using it.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillUse(creature, skill.Info.Id, 0);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Cancels skill (do nothing).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}

		/// <summary>
		/// Completes skill, teleporting behind target.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var target = creature.Target;
			if (target != null)
			{
				var pos = creature.GetPosition();
				var targetPos = target.GetPosition();
				var telePos = pos.GetRelative(targetPos, DistanceToTarget);

				Send.Effect(creature, Effect.SilentMoveTeleport, (byte)2, telePos.X, telePos.Y);
				creature.Warp(creature.RegionId, telePos);
			}

			Send.SkillComplete(creature, skill.Info.Id);
		}
	}
}
