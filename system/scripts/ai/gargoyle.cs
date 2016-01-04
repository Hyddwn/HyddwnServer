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
		//On(AiState.Aggro, AiEvent.MagicHit, OnMagicHit);
	}

	protected override IEnumerable Idle()
	{
		var num = Random();
		if (num < 10) // 10%
			Do(Wander(250, 500, true));
		else if (num < 40) // 30%
			Do(Wander(250, 500, false));
		else if (num < 60) // 20%
			Do(Wait(4000, 6000));
		else if (num < 70) // 10%
			Do(PrepareSkill(SkillId.Lightningbolt));

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		var num = Random();
		if (num < 35) // 35%
		{
			if (Random() < 50)
				Do(Wander(100, 200, false));

			Do(Attack(3, 4000));
			Do(Say("..."));

			if (Random() < 80)
			{
				Do(Follow(50, false, 3000));
				Do(PrepareSkill(SkillId.Stomp));
				Do(Wait(2000));
				Do(UseSkill());
				Do(Wait(2000));
				Do(Say("!!"));
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, false, 5000));
			}
		}
		else if (num < 50) // 15%
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
			Do(Say("..."));
			Do(Attack(1, 4000));
			Do(Attack(2, 4000));
			Do(Wait(500, 2000));
		}
		else if (num < 70) // 20%
		{
			Do(Say("..."));

			num = Random();
			if (num < 40) // 40%
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if (num < 70) // 30%
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(CancelSkill());
				Do(Attack(3, 4000));
			}
			else // 30%
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 7000));
			}

			Do(Wait(4000, 2000));
		}
		else if (num < 80) // 10%
		{
			Do(Say("..."));
			if (Random() < 60)
				Do(Circle(400, 2000, 2000, true));
			else
				Do(Follow(400, true, 5000));
		}
		else if (num < 90) // 10%
		{
			num = Random();
			if (num < 60) // 40%
				Do(Circle(400, 2000, 2000, false));
			else if (num < 70) // 30%
				Do(Follow(400, false, 5000));
			else // 30%
				Do(KeepDistance(1000, false, 5000));
		}
		else // 10%
		{
			Do(Say("..."));
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(1000, 10000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say("..."));

		var num = Random();
		if (num < 20) // 20%
			Do(KeepDistance(1000, false, 2000));
		else if (num < 70) // 50%
			Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say("Barr!", "Barr!", "", ""));

		var num = Random();
		if (num < 20) // 20%
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 3000));
		}
		else if (num < 65) // 45%
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 60)
				Do(Circle(400, 2000, 2000));
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
		else if (num < 90) // 25%
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say("!"));
		Do(Attack(3, 4000));

		if (Random() < 40)
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
			Do(Say("!!!!!"));
			Do(Wait(1000, 2000));
		}
	}
}
