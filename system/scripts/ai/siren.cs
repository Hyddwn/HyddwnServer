//--- Aura Script -----------------------------------------------------------
// Siren AI
//--- Description -----------------------------------------------------------
// AI for Siren.
//---------------------------------------------------------------------------

[AiScript("siren")]
public class SirenAi : AiScript
{
	public SirenAi()
	{
		SetVisualField(1600, 120);
		SetAggroRadius(1000);

		Doubts("/pc/", "/pet/");
		HatesBattleStance(8000);

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.CriticalKnockDown, SkillId.Smash, OnCriticalKnockDown);
	}
	
	protected IEnumerable LoseMask()
	{
		Do(RemoveSkills(SkillId.NaturalShieldPassive, SkillId.HeavyStanderPassive, SkillId.ManaDeflectorPassive));
		var mask = this.Creature.Inventory.GetItemAt(Pocket.Head, 0, 0);
		mask.Drop(this.Creature.Region, this.Creature.GetPosition(), 200, this.Creature, true);
		this.Creature.Inventory.Remove(mask);
		Send.EquipmentMoved(this.Creature, Pocket.Head);
				
		yield break;
	}

	protected override IEnumerable Idle()
	{
		if (HasEquipped("/glasses/"))
		{
			Do(CancelSkill());
			SwitchRandom();
			if (Case(15))
			{
				Do(Wander(100, 500));
				Do(Say("Aaaahh..."));
			}
			else if (Case(15))
			{
				Do(Wander(100, 500, false));
				Do(Say(Rnd("Fall in love with me...", "Is there anyone out there? Anyone that'll fall in love with my beauty...", "", "")));
			}
			else if (Case(20))
			{
				Do(Wait(500, 3000));
			}
			else if (Case(50))
			{
				Do(PrepareSkill(SkillId.PlayingInstrument));		
			}
		}
		else
		{
			if (Random() < 30)
			{
				Do(Wander(100, 500, false));
				Do(Say(Rnd("Ahhhh! My mask!", "My mask!", "I hate it! I hate it!","Where's my mask?")));
			}
			else
			{
				Do(Wait(1000, 3000));
			}
		}
	}

	protected override IEnumerable Alert()
	{
		if (HasEquipped("/glasses/"))
		{
			Do(CancelSkill());
			if (Random() < 70)
			{		
				Do(Say(Rnd("Come see me. Haha!","Tell me you love me...", "", "")));
				Do(Wait(1000, 4000));
				Do(Circle(600, 1000, 2000));
			}
			else
			{
				Do(Say(Rnd("Would you like to hear my song?","Listen to my song...","Fall deeply into my singing.")));
				Do(Timeout(10000, PrepareSkill(SkillId.PlayingInstrument)));
			}
			Do(Wait(0, 10000));
		}
		else
		{
			if (Random() < 10)
			{
				Do(Say(Rnd("Don't look at me!", "", "")));
				Do(KeepDistance(1000, true, 2000));
			}
			Do(Wait(1000, 3000));
		}
	}

	protected override IEnumerable Aggro()
	{
		Do(CancelSkill());
		if (HasEquipped("/glasses/"))
		{
			if (Random(100) < 50)
			{
				Do(Circle(300, 1000, 1000, false));
			}
			SwitchRandom();
			if (Case(15))
			{
				Do(Say(Rnd("Hahaha... What do you think?", "", "")));
				Do(Attack(Rnd(1, 2, 2, 3, 3), 3000));
			}
			else if (Case(20))
			{
				Do(Attack(1, 700));
				Do(Wait(1000));
				Do(Say(Rnd("Now, keep falling... Keep falling with me... into the after life...", "", "")));
				Do(Attack(1, 700));
				Do(Wait(1000));
				if (Random(100) < 50)
				{
					Do(PrepareSkill(SkillId.Windmill));
					Do(UseSkill());
					Do(Say(Rnd("What do you think about my legs?", "Yoohoo...", "", "")));
					Do(Wait(1000, 3000));
					Do(CancelSkill());
				}
			}
			else if (Case(5))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Follow(200, true, 5000));
				Do(Say(Rnd("Don't you love it? Don't you love that you're fighting me?","Now, keep falling... keep falling with me... into the world of death...", "", "")));
				Do(Attack(Rnd(1, 2, 3), 3000));
			}
			else if (Case(10))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(200, true, 5000));
				Do(Say(Rnd("Come to me right now.","Come near me...", "", "")));
				Do(CancelSkill());
			}
			else if (Case(10))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(2000, 4000));
				Do(Say(Rnd("What do you say?", "", "")));
				Do(CancelSkill());
			}
			else if (Case(20))
			{
				Do(Say(Rnd("What do you say?","Feel it.","How would you be like?", "", "")));
				Do(StackAttack(SkillId.Thunder));
			}
			else if (Case(10))
			{
				Do(Summon(13001, 3, 100, 1000)); // Sahagin Soldier
				Do(Summon(13003, 2, 100, 1000)); // Sahagin Ranger
			}
			else if (Case(10))
			{
				Do(Summon(13002, 3, 100, 1000)); // Sahagin Fighter
				Do(Summon(13004, 2, 100, 1000)); // Sahagin Warrior
			}
			if (Random() < 50)
			{
				Do(Say(Rnd("Hoho","Keep falling... until you can't fall anymore. Then you'll be ice cold.","Don't you want to touch me?", "", "")));
				Do(Circle(500, 1000, 3000, true));
			}		
		}
		else
		{
			SwitchRandom();
			if (Case(20))
			{
				Do(Say(Rnd("Don't look at me!", "Yoooooopp~", "I hate you! I hate you!", "", "")));
				Do(Attack(Rnd(2, 2, 3), 3000));
			}
			else if (Case(30))
			{
				Do(Attack(1, 700));
				Do(Wait(1000, 1000));
				Do(Say(Rnd("Get off me!!", "", "")));
				Do(Attack(1, 700));
				Do(Wait(1000));
				if (Random() < 50)
				{
					Do(PrepareSkill(SkillId.Windmill));
					Do(UseSkill());
					Do(Say(Rnd("You're horrible!", "", "")));
					Do(Wait(1000, 3000));
					Do(CancelSkill());
				}
			}
			else if (Case(5))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Follow(200, true, 5000));
				Do(Say(Rnd("Hooo!", "", "")));
				Do(Attack(1, 5000));
			}
			else if (Case(10))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(200, true, 5000));
				Do(Say(Rnd("What are you?", "Yaaay!", "", "")));
				Do(CancelSkill());
			}
			else if (Case(10))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(2000, 4000));
				Do(Say(Rnd("I hate it!", "", "")));
				Do(CancelSkill());
			}
		}
	}

	private IEnumerable OnHit()
	{
		if (HasEquipped("/glasses/"))
		{
			Do(Say("Oh no!"));
			if (Random() < 70)
			{
				Do(Attack(Rnd(2, 2, 3), 3000));
				Do(Say(Rnd("What? Do you like me?", "", "")));
				Do(KeepDistance(3000, false, 3000));
			}
			else
			{
				Do(Say(Rnd("Hahaha","What do you think? Are you excited?","I'm not that easy to get.", "", "")));
				Do(PrepareSkill(SkillId.DarkLord));
				Do(Attack(Rnd(1, 2, 2, 2, 3), 3000));
			}
		}
		else
		{
			Do(Say("Kaaack!"));
			SwitchRandom();
			if (Case(40))
			{
				Do(Say(Rnd("Where is my mask?", "", "")));
				Do(KeepDistance(3000, false, 2000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(UseSkill());
				Do(Say(Rnd("You're horrible!", "", "")));
				Do(Wait(1000, 3000));
				Do(CancelSkill());
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(2000, 4000));
				Do(Say(Rnd("Get it off me!", "", "")));
				Do(CancelSkill());
			}
		}
	}

	private IEnumerable OnKnockDown()
	{
		if (HasEquipped("/glasses/"))
		{
			Do(Say(Rnd("Not bad.","Ahhhh","Oh wow!", "", "")));
			Do(Wait(500, 1000));
			Do(Say(Rnd("Hahaha","What do you think? Are you excited?","I'm not that easy to get.", "", "")));
			Do(PrepareSkill(SkillId.DarkLord));
			SwitchRandom();
			if (Case(50))
			{
				Do(Attack(Rnd(1, 2, 3), 3000));
			}
			else if (Case(25))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
			else if (Case(25))
			{
				Do(KeepDistance(1200, false, 5000));
			}
		}
		else
		{
			Do(Say("Kaaack"));
			SwitchRandom();
			if (Case(20))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
			else if (Case(40))
			{
				Do(Attack(Rnd(1, 2, 3), 3000));
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(UseSkill());
				Do(Say(Rnd("Kaakkkk!","You're horrible!", "", "")));
				Do(Wait(1000, 3000));
				Do(CancelSkill());
			}
		}
	}

	private IEnumerable OnCriticalKnockDown()
	{
		if (HasEquipped("/glasses/"))
		{
			Do(Say("Kaaack"));
			Do(LoseMask());
			Do(Say(Rnd("Oh! My mask!","My mask!","My mask fell off!")));
		}
		else
		{
			SwitchRandom();
			if (Case(20))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
			else if (Case(40))
			{
				Do(Attack(Rnd(1, 2, 3), 3000));
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(UseSkill());
				Do(Say(Rnd("Kaakkkk!","You're horrible!", "", "")));
				Do(Wait(1000, 3000));
				Do(CancelSkill());
			}
		}
	}
}
