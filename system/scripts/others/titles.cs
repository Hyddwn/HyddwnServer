//--- Aura Script -----------------------------------------------------------
// Titles
//--- Description -----------------------------------------------------------
// Rewards titles to creatures when certain things happen.
//---------------------------------------------------------------------------

public class TitleRewardingScript : GeneralScript
{
	public override void Load()
	{
		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_simon", "before_keywords", SimonBeforeKeywords);
		AddHook("_glenis", "before_keywords", GlenisBeforeKeywords);
	}

	[On("CreatureFished")]
	public void OnCreatureFished(Creature creature, Item item)
	{
		if (item == null)
			return;

		// the Fishing Legend
		// ------------------------------------------------------------------
		var cm = (item.MetaData1.GetFloat("SCALE") * item.Data.BaseSize);
		if (cm > 95)
			creature.Titles.Show(19);

		if (item.Info.Id == 50216) // Carnivorous Fish
			creature.Titles.Enable(19);
	}

	[On("CreatureFinishedProductionOrCollection")]
	public void OnCreatureFinishedProductionOrCollection(Creature creature, bool success)
	{
		// the Butterfingers
		// Show if failed collecting or production something 5 times in a
		// rown, give it at 10 fails in a row.
		// ------------------------------------------------------------------
		if (creature.Titles.IsUsable(20))
			return;

		if (success)
		{
			creature.Vars.Temp["ButterfingerFailCounter"] = 0;
			return;
		}

		if (creature.Vars.Temp["ButterfingerFailCounter"] == null)
			creature.Vars.Temp["ButterfingerFailCounter"] = 0;

		var count = (int)creature.Vars.Temp["ButterfingerFailCounter"];
		count++;

		if (count >= 10)
		{
			creature.Titles.Enable(20);
			count = 0;
		}
		else if (count >= 5)
		{
			creature.Titles.Show(20);
		}

		creature.Vars.Temp["ButterfingerFailCounter"] = count;
	}

	[On("CreatureGotLuckyFinish")]
	public void CreatureGotLuckyFinish(Creature creature, LuckyFinish finish, int amount)
	{
		// the Lucky
		// Enable on lucky finish over 2k gold.
		// ------------------------------------------------------------------
		if (creature.Titles.IsUsable(23) || finish == LuckyFinish.None)
			return;

		if (amount >= 2000)
			creature.Titles.Enable(23);
	}

	[On("SkillRankChanged")]
	public void OnSkillRankChanged(Creature creature, Skill skill)
	{
		// the Elemental Apprentice
		// Enable if creature has all basic bolts.
		// ------------------------------------------------------------------
		if (creature.Titles.IsUsable(28) || !skill.Is(SkillId.Icebolt, SkillId.Firebolt, SkillId.Lightningbolt))
			return;

		if (creature.Skills.Has(SkillId.Icebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Firebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Lightningbolt, SkillRank.RF))
			creature.Titles.Enable(28);
	}

	[On("PlayerLoggedIn")]
	public void OnPlayerLoggedIn(Creature creature)
	{
		// the Elemental Apprentice
		// Enable if creature has all basic bolts. Fallback for players
		// who already have all bolts. TODO: Remove.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(28) && creature.Skills.Has(SkillId.Icebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Firebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Lightningbolt, SkillRank.RF))
			creature.Titles.Enable(28);

		// the Reborn 
		// Enable if creature rebirthed.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(39) && creature.Has(CreatureStates.JustRebirthed))
			creature.Titles.Enable(39);
	}

	[On("CreatureAged")]
	public void OnCreatureAged(Creature creature, int prevAge)
	{
		// the Adult
		// Enable if creature reaches age 18.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(44) && creature.Age >= 18)
			creature.Titles.Enable(44);

		// the Reborn
		// Show if creature reaches age 25.
		// ------------------------------------------------------------------
		if (!creature.Titles.Knows(39) && creature.Age >= 25)
			creature.Titles.Show(39);

		// the All-Knowing
		// Show if creature reaches age 25.
		// ------------------------------------------------------------------
		if (!creature.Titles.Knows(45) && creature.Age >= 25)
			creature.Titles.Show(45);

		// the All-Knowing
		// Enable if creature reaches age 30.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(45) && creature.Age >= 30)
			creature.Titles.Enable(45);

		// the Old
		// Show if creature reaches age 35.
		// ------------------------------------------------------------------
		if (!creature.Titles.Knows(46) && creature.Age >= 35)
			creature.Titles.Show(46);

		// the Old
		// Enable if creature reaches age 40.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(46) && creature.Age >= 40)
			creature.Titles.Enable(46);
	}

	[On("CreatureStartedPtj")]
	public void OnCreatureStartedPtj(Creature creature, PtjType type)
	{
		// the Diligent
		// Show if creature starts any PTJ.
		// ------------------------------------------------------------------
		if (!creature.Titles.Knows(33))
			creature.Titles.Show(33);
	}

	[On("CreatureCompletedPtj")]
	public void OnCreatureCompletedPtj(Creature creature, PtjType type)
	{
		// the Diligent
		// Enable if creature completed a PTJ 100 times.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(33))
		{
			var trackRecord = creature.Quests.GetPtjTrackRecord(type);
			if (trackRecord.Done >= 100)
			{
				creature.Titles.Enable(33);
				creature.Titles.Show(35);
			}
		}

		// the Super Diligent 
		// Enable if creature completed a PTJ 10,000 times.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(35))
		{
			var trackRecord = creature.Quests.GetPtjTrackRecord(type);
			if (trackRecord.Done >= 10000)
				creature.Titles.Enable(35);
		}

		// the Busy 
		// Enable if creature completed 3 Part-time Jobs in one Erinn day.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(10154))
		{
			var now = ErinnTime.Now;
			var count = 0;

			var trackRecords = creature.Quests.GetPtjTrackRecords();
			foreach (var record in trackRecords)
			{
				var change = new ErinnTime(record.LastChange);
				if (now.Day == change.Day && now.Month == change.Month && now.Year == change.Year)
					count++;
			}

			if (count >= 3)
				creature.Titles.Enable(10154);
		}
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		// the Lazy 
		// Enable if talking to Duncan about rumors while having the title
		// "the Neighboring Part-timer".
		// ------------------------------------------------------------------
		if (!npc.Player.Titles.IsUsable(34))
		{
			var keyword = args[0] as string;
			if (keyword == "rumor" && npc.Player.Titles.SelectedTitle == 20000) // the Neighboring Part-timer
			{
				npc.Msg(L("I thought you were doing a part-time job.<br/>What brings you here?<br/>If you slack on your work, people won't approve...<br/>So be responsible."));
				npc.Player.Titles.Enable(34);
			}
		}

		return HookResult.Continue;
	}

	[On("CreatureAttack")]
	public void OnCreatureAttacked(TargetAction tAction)
	{
		// who experienced Death
		// Show when knocked out by a 500+ dmg attack, enable when
		// surviving a 500+ dmg attack.
		// ------------------------------------------------------------------
		if (tAction.Damage < 500)
			return;

		if (!tAction.Creature.Titles.IsUsable(37))
		{
			if (tAction.Creature.IsDead)
			{
				if (!tAction.Creature.Titles.Knows(37))
					tAction.Creature.Titles.Show(37);
			}
			else
				tAction.Creature.Titles.Enable(37);
		}

		// who transcended Death
		// Show when knocked out by a 1000+ dmg attack, enable when
		// surviving a 1000+ dmg attack.
		// ------------------------------------------------------------------
		if (tAction.Damage < 1000)
			return;

		if (!tAction.Creature.Titles.IsUsable(38))
		{
			if (tAction.Creature.IsDead)
			{
				if (!tAction.Creature.Titles.Knows(38))
					tAction.Creature.Titles.Show(38);
			}
			else
				tAction.Creature.Titles.Enable(38);
		}
	}

	[On("CreatureLevelUp")]
	public void OnCreatureLevelUp(Creature creature)
	{
		// the Wise
		// Shown on level up with more than 80 Int, enabled with more than
		// 200 Int.
		// ------------------------------------------------------------------
		CheckStatTitle(creature, creature.IntBaseTotal, 80, 200, 52);

		// the Strong
		// Shown on level up with more than 80 Str, enabled with more than
		// 200 Str.
		// ------------------------------------------------------------------
		CheckStatTitle(creature, creature.StrBaseTotal, 80, 200, 53);

		// the Skillful
		// Shown on level up with more than 80 Dex, enabled with more than
		// 200 Dex.
		// ------------------------------------------------------------------
		CheckStatTitle(creature, creature.DexBaseTotal, 80, 200, 54);

		// who reached Lv 50 at Age 10
		// Show on level 45 at age 10, enable on level 50.
		// ------------------------------------------------------------------
		CheckStatTitle(creature, creature.Level, 45, 50, 76);
	}

	private void CheckStatTitle(Creature creature, float statVal, float knowVal, float enableVal, ushort titleId)
	{
		if (statVal >= knowVal && !creature.Titles.IsUsable(titleId))
		{
			if (statVal >= enableVal)
				creature.Titles.Enable(titleId);
			else if (!creature.Titles.Knows(titleId))
				creature.Titles.Show(titleId);
		}
	}

	[On("PlayerClearedDungeon")]
	public void OnPlayerClearedDungeon(Creature creature, Dungeon dungeon)
	{
		// the Experienced
		// Enable if Rabbie Basic is cleared while having unbroken eggs in
		// inventory.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(55) && dungeon.Name == "dunbarton_rabbie_low_dungeon")
		{
			var eggs = creature.Inventory.GetItems(a => a.HasTag("/usable/food/cooking/solid/*egg/") && a.Info.Pocket != Pocket.VIPInventory);
			if (eggs.Count != 0)
				creature.Titles.Enable(55);
		}
	}

	[On("CreatureKilled")]
	public void OnCreatureKilled(Creature deadCreature, Creature killer)
	{
		// the Tank
		// Enable if a creature dies for a party member.
		// Exact functionality unknown, we're gonna assume if a party member
		// hit the monster first, and you then die by that monster's paw,
		// you died for your teammate.
		// ------------------------------------------------------------------
		if (deadCreature.IsPlayer && !deadCreature.Titles.IsUsable(57) && deadCreature.Party.MemberCount > 1)
		{
			// Get tracker of dead creature to compare the id to the member's.
			var deadTracker = killer.GetHitTracker(deadCreature.EntityId);
			if (deadTracker != null)
			{
				// Go through living party members
				var members = deadCreature.Party.GetMembers();
				foreach (var member in members.Where(a => !a.IsDead))
				{
					// If the member's tracker has a lower id than that of the
					// dead creature, the member attacked first.
					var memberTracker = killer.GetHitTracker(member.EntityId);
					if (memberTracker != null && memberTracker.Id < deadTracker.Id)
						deadCreature.Titles.Enable(57);
				}
			}
		}

		// who was Defeated by a Fox at Age 17
		// Enable when killed by a fox at Age 17+.
		// ------------------------------------------------------------------
		if (!deadCreature.Titles.IsUsable(77) && deadCreature.Age >= 17)
		{
			if (killer.HasTag("/fox/"))
				deadCreature.Titles.Enable(77);
		}

		if (deadCreature.HasTag("/bear/"))
		{
			// who Almost Slew a Bear at 10
			// Show when killing a bear at age 12+, enable on age 11.
			// ------------------------------------------------------------------
			if (killer.Age >= 11 && !killer.Titles.IsUsable(78))
			{
				if (killer.Age >= 12)
					killer.Titles.Show(78);
				else
					killer.Titles.Enable(78);
			}

			// the Bear Slayer with Bare Hands
			// Enable when killing a bear without weapons.
			//
			// the Bear Slayer with a Single Blow
			// Show when killing a bear without weapons.
			// ------------------------------------------------------------------
			if (!killer.Titles.IsUsable(79) && killer.RightHand == null)
			{
				killer.Titles.Enable(79);

				if (!killer.Titles.Knows(81))
					killer.Titles.Show(81);
			}

			// who slew a bear at age 10
			// Enable when killing a bear at age 10 mostly alone.
			// ------------------------------------------------------------------
			if (!killer.Titles.IsUsable(80) && killer.Age == 10 && deadCreature.GetTopDamageDealer().Attacker == killer)
				killer.Titles.Enable(80);

			// the Bear Slayer with a Single Blow
			// Enable when killing... it's right in the title...!
			// ------------------------------------------------------------------
			if (!killer.Titles.IsUsable(81) && deadCreature.GetTotalHits() == 1)
				killer.Titles.Enable(81);
		}
		else if (deadCreature.HasTag("/golem/"))
		{
			// the Golem Slayer with a Single Blow
			// Enable when killing a golem with one hit.
			// ------------------------------------------------------------------
			if (!killer.Titles.IsUsable(82))
			{
				if (deadCreature.GetTotalHits() == 1)
					killer.Titles.Enable(82);
				else if (!killer.Titles.Knows(82))
					killer.Titles.Show(82);
			}

			// the Golem Slayer
			// Enable when killing a golem.
			// ------------------------------------------------------------------
			if (!killer.Titles.IsUsable(83))
				killer.Titles.Enable(83);
		}
		else if (deadCreature.HasTag("/succubus/"))
		{
			// the Succubus Slayer
			// Enable when killing a succubus.
			// ------------------------------------------------------------------
			if (!killer.Titles.IsUsable(84))
				killer.Titles.Enable(84);
		}
		else if (deadCreature.HasTag("/ogre/"))
		{
			// the Ogre Slayer
			// Enable when killing an ogre.
			// ------------------------------------------------------------------
			if (!killer.Titles.IsUsable(85))
				killer.Titles.Enable(85);
		}

		// the Fire Arrow
		// Enabled when killing an enemy with a fire arrow.
		// ------------------------------------------------------------------
		if (!killer.Titles.IsUsable(88))
		{
			// Maybe the temp variable FireArrow could be used for this,
			// but this is safer, no risk of the variable being reset,
			// or still being set when it shouldn't be.
			if (killer.RightHand != null && killer.RightHand.HasTag("/bow/|/bow01/") && Campfire.GetNearbyCampfire(killer, 500) != null)
				killer.Titles.Enable(88);
		}
	}

	public async Task<HookResult> SimonBeforeKeywords(NpcScript npc, params object[] args)
	{
		// the Luxurious 
		// Enable if talking to Simon while wearing clothes worth over
		// 500,000 gold.
		// 
		// the Hungry
		// Enable if talking to Simon while wearing only broken beginner
		// clothes with 50% hunger.
		// ------------------------------------------------------------------
		var usable58 = npc.Player.Titles.IsUsable(58);
		var usable59 = npc.Player.Titles.IsUsable(59);

		if ((!usable58 || !usable59) && (args[0] as string) == "personal_info")
		{
			if (!usable58)
			{
				var equip = npc.Player.Inventory.GetEquipment(a => a.Info.Pocket < Pocket.RightHand1);
				var total = equip.Sum(a => a.OptionInfo.Price);
				if (total >= 500000)
				{
					npc.Msg("(Missing dialog: Simon talking about expensive clothes.)");
					npc.Player.Titles.Enable(58);
				}
			}

			if (!usable59)
			{
				var equip = npc.Player.Inventory.GetEquipment();
				if (equip.Length == 1)
				{
					var id = equip[0].Info.Id;
					if (id == 15001 || id == 15002 || id == 15169 || id == 15168 || id == 15228 || id == 15208)
					{
						if (equip[0].Durability == 0 && npc.Player.Hunger >= npc.Player.StaminaMax / 2)
						{
							npc.Msg("(Missing dialog: Simon talking about cheap clothes?)");
							npc.Player.Titles.Enable(59);

							equip[0].Durability = equip[0].OptionInfo.DurabilityMax;
							Send.ItemDurabilityUpdate(npc.Player, equip[0]);
						}
					}
				}
			}
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> GlenisBeforeKeywords(NpcScript npc, params object[] args)
	{
		// the Hungry
		// Show if talking to Glenis while wearing only broken beginner
		// clothes with 50% hunger.
		// ------------------------------------------------------------------
		if (!npc.Player.Titles.Knows(59) && (args[0] as string) == "shop_cloth")
		{
			var equip = npc.Player.Inventory.GetEquipment();
			if (equip.Length == 1)
			{
				var id = equip[0].Info.Id;
				if (id == 15001 || id == 15002 || id == 15169 || id == 15168 || id == 15228 || id == 15208)
				{
					if (equip[0].Durability == 0 && npc.Player.Hunger >= npc.Player.StaminaMax / 2)
					{
						npc.Msg("(Missing dialog: Glenis advising you to talk to Simon?)");
						npc.Player.Titles.Show(59);
					}
				}
			}
		}

		return HookResult.Continue;
	}
}
