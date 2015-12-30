//--- Aura Script -----------------------------------------------------------
// Ciar Beginner Golem AI
//--- Description -----------------------------------------------------------
// AI for Ciar Beginner Golem (ID 130014)
//---------------------------------------------------------------------------

[AiScript("ciarbeginnergolem")]
public class CiarBeginnerGolemAi : AiScript
{
	public CiarBeginnerGolemAi()
	{
		SetAggroRadius(1500); // angle 90 audio 1200
		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(StartSkill(SkillId.Rest));
		Do(Wait(1000000000));
	}

	protected override IEnumerable Aggro()
	{
		var rndAggro = Random();
		if (rndAggro < 50)
		{
			if (Random() < 20)
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else
			{
				Do(Attack(3, 4000));
			}
		}
		else if (rndAggro < 70)
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true, 5000));
			Do(CancelSkill());
		}
		else if (rndAggro < 90)
		{
			Do(PrepareSkill(SkillId.Stomp));
			Do(Wait(500, 2000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		Do(Wait(2000, 3000));
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Wait(500));
		if (Creature.Life < Creature.LifeMax * 0.50f)
		{
			Do(Say("Tachy granide inchatora mana prow!"));
			Do(SetHeight(1.2));
			Do(SetStat(Stat.Str, 100));
			Do(PlaySound("data/sound/Glasgavelen_MagicCasting.wav"));
		}
		if (Creature.Life < Creature.LifeMax * 0.30f)
		{
			var rndOKD = Random();
			if (rndOKD < 20)
			{
				Do(PrepareSkill(SkillId.Stomp));
				Do(UseSkill());
				Do(Wait(1500));
			}
			else if (rndOKD < 25)
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if (rndOKD < 35)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
		}
		else
		{
			var rndOKD2 = Random();
			if (rndOKD2 < 20)
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(Wait(4000));
				Do(UseSkill());
			}
			else if (rndOKD2 < 25)
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if (rndOKD2 < 35)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
		}
	}
}