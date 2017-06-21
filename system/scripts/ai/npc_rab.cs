//--- Aura Script -----------------------------------------------------------
// Fleta NPC AI
//--- Description -----------------------------------------------------------
// AI for the Fleta's dog, Rab NPC. Wanders around and rests.
// Will also circle around player characters if it notices them.
//---------------------------------------------------------------------------

[AiScript("npc_rab")]
public class RabNpcAi : AiScript
{
	public RabNpcAi()
	{
		SetVisualField(600, 400);
		SetHeartbeat(500);
		SetMaxDistanceFromSpawn(1000);

		Loves("/pc/", "/pet/");
	}

	protected override IEnumerable Idle()
	{
		if (!HasSkill(SkillId.Rest))
			Do(AddSkill(SkillId.Rest, SkillRank.RF));
		if (Random() < 40)
			Do(SayRandomPhrase());
		Do(Wander(100, 400));
		Do(Wait(8000, 10000));
		Do(StartSkill(SkillId.Rest));
		Do(Wait(2000, 40000));
		Do(StopSkill(SkillId.Rest));
	}

	protected override IEnumerable Love()
	{
		if (!HasSkill(SkillId.Rest))
			Do(AddSkill(SkillId.Rest, SkillRank.RF));
		SwitchRandom();
		if (Case(20))
			Do(Wait(1000, 10000));
		else if (Case(15))
			Do(Circle(250, 1000, 2000));
		else if (Case(15))
			Do(Circle(250, 1000, 2000, false));
		else if (Case(15))
		{
			Do(Circle(500, 1000, 3000));
			Do(Wait(1000, 10000));
		}
		else if (Case(15))
		{
			Do(Circle(500, 1000, 3000, false));
			Do(Wait(1000, 10000));
		}
		else if (Case(20))
		{
			Do(SayRandomPhrase());
			Do(StartSkill(SkillId.Rest));
			Do(Wait(10000, 17000));
			Do(StopSkill(SkillId.Rest));
		}
	}
}
