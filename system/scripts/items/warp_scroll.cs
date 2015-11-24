//--- Aura Script -----------------------------------------------------------
// Warp Scroll Items
//--- Description -----------------------------------------------------------
// Handles items like wings, that warp you to a pre-defined location.
//---------------------------------------------------------------------------

using Aura.Channel.Skills.Hidden;

[ItemScript("/warp_scroll/")]
public class WarpScrollItemScript : ItemScript
{
	public override void OnUse(Creature creature, Item item)
	{
		// A few items prepare a skill, instead of sending use, to reduce
		// redundancy we'll call into the skill handler.
		HiddenTownBack.Warp(creature, item);
	}
}
