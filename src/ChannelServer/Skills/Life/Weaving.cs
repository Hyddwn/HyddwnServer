// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Weaving production skill.
	/// </summary>
	/// <remarks>
	/// Starting production calls Prepare, once the creation process is done,
	/// Complete is called. There is no way to cancel the skill once Prepare
	/// was called.
	/// 
	/// While the client tells us how many items are gonna be produced,
	/// it Prepares the skill again and again, so we must only create
	/// one product at a time.
	/// 
	/// Weaving handles usage of looms and spinning wheels.
	/// 
	/// Var20: Success Rate?
	/// </remarks>
	[Skill(SkillId.Weaving)]
	public class Weaving : ProductionSkill
	{
		protected override bool RequiresProp { get { return true; } }

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success)
		{

		}
	}
}
