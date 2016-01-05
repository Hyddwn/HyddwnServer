//--- Aura Script -----------------------------------------------------------
// Gargoyle AI
//--- Description -----------------------------------------------------------
// AI for gargoyles.
//---------------------------------------------------------------------------

[AiScript("gargoyle")]
public class GargoyleAi : AiScript
{
	public GargoyleAi()
	{
		SetVisualField(1200, 120);
		SetAggroRadius(800);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.MagicHit, OnMagicHit);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(10))
			Do(Wander(250, 500, true));
		else if (Case(30))
			Do(Wander(250, 500, false));
		else if (Case(20))
			Do(Wait(4000, 6000));
		else if (Case(10))
			Do(PrepareSkill(SkillId.Lightningbolt));

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(Say("When will my time come...", "Hahaha...", "Growl.", "I'll be watching you.", "Growling", "", "", ""));

		SwitchRandom();
		if (Case(20))
		{
			Do(Wait(1000, 2000));
		}
		else if (Case(30))
		{
			Do(Wait(1000, 4000));

			if (Random() < 90)
			{
				Do(Circle(600, 2000, 2000));
			}
			else
			{
				Do(Attack(Rnd(1, 2, 3), 4000));
			}
		}

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(35))
		{
			if (Random() < 50)
				Do(Wander(100, 200, false));

			Do(Attack(3, 4000));
			Do(Say("I'll make you remember.", "Take this!", "Are you glaring at me?", "", "", "", ""));

			if (Random() < 80)
			{
				Do(Follow(50, false, 3000));
				Do(PrepareSkill(SkillId.Stomp));
				Do(Wait(2000));
				Do(UseSkill());
				Do(Wait(2000));
				Do(Say("..."));
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, false, 5000));
			}
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
			Do(Say("Deceivable human tricks.", "", "", "", ""));
			Do(Attack(1, 4000));
			Do(Attack(2, 4000));
			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			Do(Say("How dare you disregard us.", "I'm coming in!", "", "", "", ""));

			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(CancelSkill());
				Do(Attack(3, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 7000));
			}

			Do(Wait(4000, 2000));
		}
		else if (Case(10))
		{
			Do(Say("Thoughtless humans...", "", "", "", ""));
			if (Random() < 60)
				Do(Circle(400, 2000, 2000, true));
			else
				Do(Follow(400, true, 5000));
		}
		else if (Case(10))
		{
			SwitchRandom();
			if (Case(40))
				Do(Circle(400, 2000, 2000, false));
			else if (Case(30))
				Do(Follow(400, false, 5000));
			else if (Case(30))
				Do(KeepDistance(1000, false, 5000));
		}
		else if (Case(10))
		{
			Do(Say("Don't polute our land with your filth.", "You shall fall!", "I see your sweat running...", "", "", "", ""));
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(1000, 10000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say("Hak!", "Gah!", "Darn!", "", ""));

		SwitchRandom();
		if (Case(20))
			Do(KeepDistance(1000, false, 2000));
		else if (Case(50))
			Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say("Ugh!", "Argh!", "Thump!", "", ""));

		SwitchRandom();
		if (Case(20))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 3000));
		}
		else if (Case(45))
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 60)
				Do(Circle(400, 2000, 2000));
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(25))
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say("Have you read my thoughts?", "That will do!", "Just as I expected.", "You're trapped now.", "You'll only feel terror from now on.", "", ""));
		Do(Attack(3, 4000));

		if (Random() < 40)
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
			Do(Say("..."));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnMagicHit()
	{
		Do(Say("I'll remember this magic.", "And return it back!", "Where do you think you're going?", "Magic?", "Mere human magic!"));
	}
}
