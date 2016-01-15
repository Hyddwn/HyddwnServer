// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Herbalism training.
	/// </summary>
	[Skill(SkillId.Herbalism)]
	public class Herbalism : IInitiableSkillHandler, ISkillHandler
	{
		/// <summary>
		/// Bonus for success chance at a certain Herbalism rank.
		/// </summary>
		private const float HerbalismPickBonus = 25;

		/// <summary>
		/// Bonus for success chance at a certain Herbalism rank.
		/// </summary>
		private const float HerbalismIdentifyBonus = 50;

		/// <summary>
		/// Subscribes to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureGathered += this.OnCreatureGathered;
		}

		/// <summary>
		/// Calculates bonus chance for Herbalism, based on rank.
		/// </summary>
		/// <remarks>
		/// Chances based on devCAT title debug output.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="collectData"></param>
		/// <returns></returns>
		public static float GetChance(Creature creature, CollectingData collectData)
		{
			var successChance = 0f;

			var herbalism = creature.Skills.Get(SkillId.Herbalism);
			if (herbalism == null)
				return successChance;

			if (collectData.Target.Contains("/baseherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.Novice: successChance = 15; break;
					case SkillRank.RF: successChance = 50; break;
					case SkillRank.RE: successChance = 55; break;
					case SkillRank.RD: successChance = 60; break;
					case SkillRank.RC: successChance = 65; break;
					case SkillRank.RB: successChance = 70; break;
					case SkillRank.RA: successChance = 75; break;
					case SkillRank.R9: successChance = 80; break;
					case SkillRank.R8: successChance = 85; break;
					case SkillRank.R7: successChance = 90; break;
					case SkillRank.R6: successChance = 95; break;
					case SkillRank.R5: successChance = 97; break;
					case SkillRank.R4: successChance = 98; break;
					case SkillRank.R3: successChance = 99; break;
					case SkillRank.R2: successChance = 99; break;
					case SkillRank.R1: successChance = 99; break;
				}
			}
			else if (collectData.Target.Contains("/bloodyherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.Novice: successChance = 8; break;
					case SkillRank.RF: successChance = 20; break;
					case SkillRank.RE: successChance = 25; break;
					case SkillRank.RD: successChance = 30; break;
					case SkillRank.RC: successChance = 50; break;
					case SkillRank.RB: successChance = 60; break;
					case SkillRank.RA: successChance = 65; break;
					case SkillRank.R9: successChance = 70; break;
					case SkillRank.R8: successChance = 75; break;
					case SkillRank.R7: successChance = 80; break;
					case SkillRank.R6: successChance = 85; break;
					case SkillRank.R5: successChance = 90; break;
					case SkillRank.R4: successChance = 93; break;
					case SkillRank.R3: successChance = 96; break;
					case SkillRank.R2: successChance = 99; break;
					case SkillRank.R1: successChance = 99; break;
				}
			}
			else if (collectData.Target.Contains("/sunlightherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.Novice: successChance = 5; break;
					case SkillRank.RF: successChance = 15; break;
					case SkillRank.RE: successChance = 20; break;
					case SkillRank.RD: successChance = 30; break;
					case SkillRank.RC: successChance = 35; break;
					case SkillRank.RB: successChance = 50; break;
					case SkillRank.RA: successChance = 55; break;
					case SkillRank.R9: successChance = 57; break;
					case SkillRank.R8: successChance = 60; break;
					case SkillRank.R7: successChance = 62; break;
					case SkillRank.R6: successChance = 64; break;
					case SkillRank.R5: successChance = 66; break;
					case SkillRank.R4: successChance = 68; break;
					case SkillRank.R3: successChance = 75; break;
					case SkillRank.R2: successChance = 80; break;
					case SkillRank.R1: successChance = 83; break;
				}
			}
			else if (collectData.Target.Contains("/manaherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.Novice: successChance = 3; break;
					case SkillRank.RF: successChance = 10; break;
					case SkillRank.RE: successChance = 15; break;
					case SkillRank.RD: successChance = 20; break;
					case SkillRank.RC: successChance = 25; break;
					case SkillRank.RB: successChance = 30; break;
					case SkillRank.RA: successChance = 35; break;
					case SkillRank.R9: successChance = 50; break;
					case SkillRank.R8: successChance = 53; break;
					case SkillRank.R7: successChance = 56; break;
					case SkillRank.R6: successChance = 60; break;
					case SkillRank.R5: successChance = 70; break;
					case SkillRank.R4: successChance = 72; break;
					case SkillRank.R3: successChance = 74; break;
					case SkillRank.R2: successChance = 76; break;
					case SkillRank.R1: successChance = 80; break;
				}
			}
			else if (collectData.Target.Contains("/whiteherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.RE: successChance = 10; break;
					case SkillRank.RD: successChance = 15; break;
					case SkillRank.RC: successChance = 20; break;
					case SkillRank.RB: successChance = 25; break;
					case SkillRank.RA: successChance = 30; break;
					case SkillRank.R9: successChance = 35; break;
					case SkillRank.R8: successChance = 40; break;
					case SkillRank.R7: successChance = 45; break;
					case SkillRank.R6: successChance = 50; break;
					case SkillRank.R5: successChance = 55; break;
					case SkillRank.R4: successChance = 60; break;
					case SkillRank.R3: successChance = 65; break;
					case SkillRank.R2: successChance = 70; break;
					case SkillRank.R1: successChance = 75; break;
				}
			}
			else if (collectData.Target.Contains("/goldherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.RD: successChance = 7; break;
					case SkillRank.RC: successChance = 9; break;
					case SkillRank.RB: successChance = 13; break;
					case SkillRank.RA: successChance = 15; break;
					case SkillRank.R9: successChance = 17; break;
					case SkillRank.R8: successChance = 19; break;
					case SkillRank.R7: successChance = 21; break;
					case SkillRank.R6: successChance = 23; break;
					case SkillRank.R5: successChance = 25; break;
					case SkillRank.R4: successChance = 27; break;
					case SkillRank.R3: successChance = 50; break;
					case SkillRank.R2: successChance = 50; break;
					case SkillRank.R1: successChance = 50; break;
				}
			}
			else if (collectData.Target.Contains("/ivoryherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.R9: successChance = 25; break;
					case SkillRank.R8: successChance = 30; break;
					case SkillRank.R7: successChance = 35; break;
					case SkillRank.R6: successChance = 40; break;
					case SkillRank.R5: successChance = 50; break;
					case SkillRank.R4: successChance = 55; break;
					case SkillRank.R3: successChance = 60; break;
					case SkillRank.R2: successChance = 65; break;
					case SkillRank.R1: successChance = 70; break;
				}
			}
			else if (collectData.Target.Contains("/purpleherb"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.R9: successChance = 10; break;
					case SkillRank.R8: successChance = 14; break;
					case SkillRank.R7: successChance = 17; break;
					case SkillRank.R6: successChance = 20; break;
					case SkillRank.R5: successChance = 30; break;
					case SkillRank.R4: successChance = 39; break;
					case SkillRank.R3: successChance = 50; break;
					case SkillRank.R2: successChance = 55; break;
					case SkillRank.R1: successChance = 60; break;
				}
			}
			else if (collectData.Target.Contains("/orangeherb/"))
			{
				switch (herbalism.Info.Rank)
				{
					case SkillRank.RA: successChance = 10; break;
					case SkillRank.R9: successChance = 12; break;
					case SkillRank.R8: successChance = 16; break;
					case SkillRank.R7: successChance = 18; break;
					case SkillRank.R6: successChance = 23; break;
					case SkillRank.R5: successChance = 25; break;
					case SkillRank.R4: successChance = 27; break;
					case SkillRank.R3: successChance = 32; break;
					case SkillRank.R2: successChance = 45; break;
					case SkillRank.R1: successChance = 50; break;
				}
			}

			return successChance;
		}

		/// <summary>
		/// Raised when creature collects something, handles gathering conditions.
		/// </summary>
		/// <param name="args"></param>
		private void OnCreatureGathered(CollectEventArgs args)
		{
			var skill = args.Creature.Skills.Get(SkillId.Herbalism);
			if (skill == null) return;

			if (skill.Info.Rank == SkillRank.Novice)
			{
				if (args.CollectData.Target.Contains("/baseherb"))
					skill.Train(1); // Try to pick a Base Herb.

				if (args.ItemId == 51104)
				{
					if (args.Success)
						skill.Train(2); // Succeed at picking a Base Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RF)
			{
				if (args.CollectData.Target.Contains("/baseherb"))
					skill.Train(1); // Try to pick a Base Herb.
				else if (args.CollectData.Target.Contains("/bloodyherb"))
					skill.Train(3); // Try to pick a Bloody Herb.
				else if (args.CollectData.Target.Contains("/sunlightherb"))
					skill.Train(5); // Try to pick a Sunlight Herb.

				if (args.Success)
				{
					if (args.ItemId == 51104)
						skill.Train(2); // Succeed at picking a Base Herb.
					else if (args.ItemId == 51101)
						skill.Train(4); // Succeed at picking a Bloody Herb.
					else if (args.ItemId == 51103)
						skill.Train(6); // Succeed at picking a Sunlight Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RE || skill.Info.Rank == SkillRank.RD)
			{
				if (args.CollectData.Target.Contains("/bloodyherb"))
					skill.Train(2); // Try to pick a Bloody Herb.
				else if (args.CollectData.Target.Contains("/sunlightherb"))
					skill.Train(4); // Try to pick a Sunlight Herb.
				else if (args.CollectData.Target.Contains("/manaherb"))
					skill.Train(6); // Try to pick a Mana Herb.
				else if (args.CollectData.Target.Contains("/goldherb") && skill.Info.Rank == SkillRank.RD)
					skill.Train(8); // Try to pick a Golden Herb.

				if (args.Success)
				{
					if (args.ItemId == 51104)
						skill.Train(1); // Succeed at picking a Base Herb.
					else if (args.ItemId == 51101)
						skill.Train(3); // Succeed at picking a Bloody Herb.
					else if (args.ItemId == 51103)
						skill.Train(5); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(7); // Succeed at picking a Mana Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RC)
			{
				if (args.Success)
				{
					if (args.ItemId == 51104)
						skill.Train(1); // Succeed at picking a Base Herb.
					else if (args.ItemId == 51101)
						skill.Train(2); // Succeed at picking a Bloody Herb.
					else if (args.ItemId == 51103)
						skill.Train(3); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(4); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(5); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(6); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RB)
			{
				if (args.CollectData.Target.Contains("/sunlightherb"))
					skill.Train(1); // Try to pick a Sunlight Herb.
				else if (args.CollectData.Target.Contains("/manaherb"))
					skill.Train(3); // Try to pick a Mana Herb.
				else if (args.CollectData.Target.Contains("/goldherb"))
					skill.Train(5); // Try to pick a Golden Herb.

				if (args.Success)
				{
					if (args.ItemId == 51103)
						skill.Train(2); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(4); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(6); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(7); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RA)
			{
				if (args.CollectData.Target.Contains("/manaherb"))
					skill.Train(2); // Try to pick a Mana Herb.
				else if (args.CollectData.Target.Contains("/goldherb"))
					skill.Train(4); // Try to pick a Golden Herb.

				if (args.Success)
				{
					if (args.ItemId == 51103)
						skill.Train(1); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(3); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(5); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(6); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank >= SkillRank.R9 && skill.Info.Rank <= SkillRank.R6)
			{
				if (args.CollectData.Target.Contains("/orangeherb"))
					skill.Train(5); // Try to pick a Mandrake.

				if (args.Success)
				{
					if (args.ItemId == 51103)
						skill.Train(1); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(2); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(3); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(4); // Succeed at picking a White Herb.
					else if (args.ItemId == 51110)
						skill.Train(6); // Succeed at picking a Mandrake.
				}
			}
			else if (skill.Info.Rank == SkillRank.R5 || skill.Info.Rank == SkillRank.R4)
			{
				if (args.CollectData.Target.Contains("/ivoryherb"))
					skill.Train(4); // Try to pick an Antidote Herb.

				if (args.Success)
				{
					if (args.ItemId == 51102)
						skill.Train(1); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(2); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51110)
						skill.Train(3); // Succeed at picking a Mandrake.
					else if (args.ItemId == 51108)
						skill.Train(5); // Succeed at picking an Antidote Herb.
				}
			}
			else if (skill.Info.Rank >= SkillRank.R3 && skill.Info.Rank <= SkillRank.R1)
			{
				if (args.CollectData.Target.Contains("/purpleherb"))
					skill.Train(3); // Try to pick a Poison Herb.

				if (args.Success)
				{
					if (args.ItemId == 51105)
						skill.Train(1); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51110)
						skill.Train(2); // Succeed at picking a Mandrake.
					else if (args.ItemId == 51109)
						skill.Train(4); // Succeed at picking a Poison Herb.
				}
			}
		}
	}
}
