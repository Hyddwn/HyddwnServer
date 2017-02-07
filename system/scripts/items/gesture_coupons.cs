//--- Aura Script -----------------------------------------------------------
// Gesture Coupon Script
//--- Description -----------------------------------------------------------
// Script to give usable gesture coupons
//---------------------------------------------------------------------------

[ItemScript(75497)]
public class ILoveYouGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("i_love_you_gesture"))
		{
			AddGesture(cr, "i_love_you_gesture", "I Love You!");
			cr.Inventory.Remove(75497);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(75498)]
public class CourtshipGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("propose_gesture"))
		{
			AddGesture(cr, "propose_gesture", "Courtship");
			cr.Inventory.Remove(75498);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(76003)]
public class CourtshipGestureTradableItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("propose_gesture"))
		{
			AddGesture(cr, "propose_gesture", "Courtship");
			cr.Inventory.Remove(76003);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85720)]
public class RageGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_1"))
		{
			AddGesture(cr, "rare_gesture_1", "Rage");
			cr.Inventory.Remove(85720);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}
[ItemScript(85721)]
public class SplatGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_2"))
		{
			AddGesture(cr, "rare_gesture_2", "Splat");
			cr.Inventory.Remove(85721);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85722)]
public class ShiverGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_3"))
		{
			AddGesture(cr, "rare_gesture_3", "Shiver");
			cr.Inventory.Remove(85722);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85723)]
public class CrossedArmsGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_4"))
		{
			AddGesture(cr, "rare_gesture_4", "Crossed Arms");
			cr.Inventory.Remove(85723);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85724)]
public class LookAroundGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_5"))
		{
			AddGesture(cr, "rare_gesture_5", "Look Around");
			cr.Inventory.Remove(85724);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85725)]
public class JumpGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_6"))
		{
			AddGesture(cr, "rare_gesture_6", "Jump");
			cr.Inventory.Remove(85725);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85726)]
public class FormalGreeting2GestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_7"))
		{
			AddGesture(cr, "rare_gesture_7", "Formal Greeting 2");
			cr.Inventory.Remove(85726);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85727)]
public class CheerGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_8"))
		{
			AddGesture(cr, "rare_gesture_8", "Cheer");
			cr.Inventory.Remove(85727);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85728)]
public class CollapseGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_9"))
		{
			AddGesture(cr, "rare_gesture_9", "Collapse");
			cr.Inventory.Remove(85728);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85729)]
public class GaspGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_10"))
		{
			AddGesture(cr, "rare_gesture_10", "Gasp");
			cr.Inventory.Remove(85729);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85730)]
public class ClumsyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_11"))
		{
			AddGesture(cr, "rare_gesture_11", "Clumsy");
			cr.Inventory.Remove(85730);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85731)]
public class ToadyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{ 
		if (!cr.HasKeyword("rare_gesture_12"))
		{
			AddGesture(cr, "rare_gesture_12", "Toady");
			cr.Inventory.Remove(85731);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85732)]
public class CutieGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_13"))
		{
			AddGesture(cr, "rare_gesture_13", "Cutie");
			cr.Inventory.Remove(85732);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85733)]
public class WahooGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("rare_gesture_14"))
		{
			AddGesture(cr, "rare_gesture_14", "Wahoo");
			cr.Inventory.Remove(85733);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85755)]
public class RespectfulBowGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("motionNewYearsBow"))
		{
			AddGesture(cr, "motionNewYearsBow", "Respectful Bow");
			cr.Inventory.Remove(85755);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(85863)]
public class AdorableGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("mask_gesture"))
		{
			AddGesture(cr, "mask_gesture", "Adorable");
			cr.Inventory.Remove(85863);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86003)]
public class AgonyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_01"))
		{
			AddGesture(cr, "GesturePack_s1_01", "Agony");
			cr.Inventory.Remove(86003);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86004)]
public class SubservientGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_02"))
		{
			AddGesture(cr, "GesturePack_s1_02", "Subservient");
			cr.Inventory.Remove(86004);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86005)]
public class ProtestGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_03"))
		{
			AddGesture(cr, "GesturePack_s1_03", "Protest");
			cr.Inventory.Remove(86005);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86006)]
public class NavigationGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{	
		if (!cr.HasKeyword("GesturePack_s1_04"))
		{
			AddGesture(cr, "GesturePack_s1_04", "Navigation");
			cr.Inventory.Remove(86006);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86007)]
public class NiftyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_05"))
		{
			AddGesture(cr, "GesturePack_s1_05", "Nifty");
			cr.Inventory.Remove(86007);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86008)]
public class SolicitingGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_06"))
		{
			AddGesture(cr, "GesturePack_s1_06", "Soliciting");
			cr.Inventory.Remove(86008);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86009)]
public class Ahing2GestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_07"))
		{
			AddGesture(cr, "GesturePack_s1_07", "Ahing 2");
			cr.Inventory.Remove(86009);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86010)]
public class EccoGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_08"))
		{
			AddGesture(cr, "GesturePack_s1_08", "Ecco");
			cr.Inventory.Remove(86010);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86011)]
public class AhryeonGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_09"))
		{
			AddGesture(cr, "GesturePack_s1_09", "Ahryeon");
			cr.Inventory.Remove(86011);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86012)]
public class HeadacheGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_10"))
		{
			AddGesture(cr, "GesturePack_s1_10", "Headache");
			cr.Inventory.Remove(86012);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86013)]
public class RetailGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_11"))
		{
			AddGesture(cr, "GesturePack_s1_11", "Retail");
			cr.Inventory.Remove(86013);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86014)]
public class CameraGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_12"))
		{
			AddGesture(cr, "GesturePack_s1_12", "Camera");
			cr.Inventory.Remove(86014);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86015)]
public class ResearchGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("GesturePack_s1_13"))
		{
			AddGesture(cr, "GesturePack_s1_13", "Research");
			cr.Inventory.Remove(86015);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86041)]
public class ApplauseGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("cheerup"))
		{
			AddGesture(cr, "cheerup", "Applause");
			cr.Inventory.Remove(86041);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}

[ItemScript(86071)]
public class InsectCollectorGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.HasKeyword("bughunting_gesture"))
		{
			AddGesture(cr, "bughunting_gesture", "Insect Collector");
			cr.Inventory.Remove(86071);
		}
		else
		{
			Send.Notice(cr, L("You already have this gesture."));
		}
	}
}