//--- Aura Script -----------------------------------------------------------
// Sheep AI
//--- Description -----------------------------------------------------------
// AI for sheeps.
//---------------------------------------------------------------------------

[AiScript("sheep")]
public class SheepAi : AiScript
{
	public SheepAi()
	{
		SetVisualField(400, 90);
		SetAggroRadius(300);

		Doubts("/wolf/");
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(10000, 14000));
	}
	
	protected override IEnumerable Alert()
	{
		if (Random() < 25)
			Do(PrepareSkill(SkillId.Defense));			

		Do(Wait(2000, 4000));
		Do(CancelSkill());
		Do(KeepDistance(800, true, 2000));		
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Attack(3));
		Do(Wait(3000, 8000));
	}
}
