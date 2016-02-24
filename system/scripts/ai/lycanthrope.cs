//--- Aura Script -----------------------------------------------------------
// Lycanthrope AIs
//--- Description -----------------------------------------------------------
// AIs for Lycanthropes.
//---------------------------------------------------------------------------

[AiScript("lycanthrope")]
public class LycanthropeAi : AiScript
{
	public LycanthropeAi()
	{
		SetVisualField(1200, 120);
		SetAggroRadius(900);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Say("Who's there!", "I can smell you...", "Maybe it was from around here?", "Do not hide.", "Can you feel it too?", "I heard something.", "Let's start from there.", "Where are you!", "I can sense danger.", "", "", "", "", "", "", "", "", "", "", "", ""));
		Do(Wait(1000, 12000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(25))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(400, 5000, 10000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(Attack(4, 5000));
		}
		else if (Case(20))
		{
			Do(Say("I admire your eyes.", "", "", ""));

			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 6000));
			Do(CancelSkill());
		}
		else if (Case(15))
		{
			Do(Circle(400, 2000, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 3000));
		}
		else if (Case(10))
		{
			Do(Say("I've learned from you.", "You're quite strong.", "", "", ""));

			Do(PrepareSkill(SkillId.Smash));

			SwitchRandom();
			if (Case(40))
			{
				Do(Circle(400, 2000, 2000));
			}
			else if (Case(30))
			{
				Do(Wait(1000, 2000));
			}

			Do(Attack(1, 3000));
		}
	}

	private IEnumerable OnHit()
	{
		Creature.GiveExp(2000);
		Do(Say("Ha ha", "Ha", "My, you have gained EXP...", "Is this all you've got?", "Is this it?", "", "", ""));

		if (Random() < 80)
			Do(Attack(3, 4000));
		else
			Do(KeepDistance(10000, false, 2000));
	}

	private IEnumerable OnKnockDown()
	{
		Creature.GiveExp(2000);
		Do(Say("You're a great opponent...", "Show me what you've got.", "Thank you.", "", "", ""));

		if (Creature.Life < Creature.LifeMax * 0.20f)
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 8000));
				Do(CancelSkill());
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
				Do(CancelSkill());
			}
		}
		else
		{
			SwitchRandom();
			if (Case(30))
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(Wait(4000));
				Do(UseSkill());
			}
			else if (Case(20))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 8000));
				Do(CancelSkill());
			}
			else if (Case(20))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
				Do(CancelSkill());
			}
			else if (Case(20))
			{
				Do(Attack(3, 14000));
			}
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
