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
			cr.Inventory.Remove(75497);
			if (!cr.HasKeyword("i_love_you_gesture"))
			{
				AddGesture(cr, "i_love_you_gesture", "I Love You!");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(75497);
			}
		}
}

[ItemScript(75498)]
public class CourtshipGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(75498);
			if (!cr.HasKeyword("propose_gesture"))
			{
				AddGesture(cr, "propose_gesture", "Courtship");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(75498);
			}
		}
}

[ItemScript(76003)]
public class CourtshipGestureTradableItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(76003);
			if (!cr.HasKeyword("propose_gesture"))
			{
				AddGesture(cr, "propose_gesture", "Courtship");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(76003);
			}
		}
}

[ItemScript(85720)]
public class RageGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85720);
			if (!cr.HasKeyword("rare_gesture_1"))
			{
				AddGesture(cr, "rare_gesture_1", "Rage");	
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85720);
			}
		}
}
[ItemScript(85721)]
public class SplatGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85721);
			if (!cr.HasKeyword("rare_gesture_2"))
			{
				AddGesture(cr, "rare_gesture_2", "Splat");	
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85721);
			}
		}
}

[ItemScript(85722)]
public class ShiverGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85722);
			if (!cr.HasKeyword("rare_gesture_3"))
			{
				AddGesture(cr, "rare_gesture_3", "Shiver");	
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85722);
			}
		}
}

[ItemScript(85723)]
public class CrossedArmsGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85723);
			if (!cr.HasKeyword("rare_gesture_4"))
			{
				AddGesture(cr, "rare_gesture_4", "Crossed Arms");	
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85723);
			}
		}
}

[ItemScript(85724)]
public class LookAroundGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85724);
			if (!cr.HasKeyword("rare_gesture_5"))
			{
				AddGesture(cr, "rare_gesture_5", "Look Around");		
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85724);
			}
		}
}

[ItemScript(85725)]
public class JumpGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85725);
			if (!cr.HasKeyword("rare_gesture_6"))
			{
				AddGesture(cr, "rare_gesture_6", "Jump");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85725);
			}
		}
}

[ItemScript(85726)]
public class FormalGreeting2GestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85726);
			if (!cr.HasKeyword("rare_gesture_7"))
			{
				AddGesture(cr, "rare_gesture_7", "Formal Greeting 2");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85726);
			}
		}
}

[ItemScript(85727)]
public class CheerGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85727);
			if (!cr.HasKeyword("rare_gesture_8"))
			{
				AddGesture(cr, "rare_gesture_8", "Cheer");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85727);
			}
		}
}

[ItemScript(85728)]
public class CollapseGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85728);
			if (!cr.HasKeyword("rare_gesture_9"))
			{
				AddGesture(cr, "rare_gesture_9", "Collapse");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85728);
			}
		}
}

[ItemScript(85729)]
public class GaspGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85729);
			if (!cr.HasKeyword("rare_gesture_10"))
			{
				AddGesture(cr, "rare_gesture_10", "Gasp");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85729);
			}
		}
}

[ItemScript(85730)]
public class ClumsyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85730);
			if (!cr.HasKeyword("rare_gesture_11"))
			{
				AddGesture(cr, "rare_gesture_11", "Clumsy");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85730);
			}
		}
}

[ItemScript(85731)]
public class ToadyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{ 
			cr.Inventory.Remove(85731);
			if (!cr.HasKeyword("rare_gesture_12"))
			{
				AddGesture(cr, "rare_gesture_12", "Toady");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85731);
			}
		}
}

[ItemScript(85732)]
public class CutieGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85732);
			if (!cr.HasKeyword("rare_gesture_13"))
			{
				AddGesture(cr, "rare_gesture_13", "Cutie");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85732);
			}
		}
}

[ItemScript(85733)]
public class WahooGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85733);
			if (!cr.HasKeyword("rare_gesture_14"))
			{
				AddGesture(cr, "rare_gesture_14", "Wahoo");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85733);
			}
		}
}

[ItemScript(85755)]
public class RespectfulBowGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85755);
			if (!cr.HasKeyword("motionNewYearsBow"))
			{
				AddGesture(cr, "motionNewYearsBow", "Respectful Bow");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85755);
			}
		}
}

[ItemScript(85863)]
public class AdorableGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(85863);
			if (!cr.HasKeyword("mask_gesture"))
			{
				AddGesture(cr, "mask_gesture", "Adorable");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(85863);
			}
		}
}

[ItemScript(86003)]
public class AgonyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86003);
			if (!cr.HasKeyword("GesturePack_s1_01"))
			{
				AddGesture(cr, "GesturePack_s1_01", "Agony");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86003);
			}
		}
}

[ItemScript(86004)]
public class SubservientGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86004);
			if (!cr.HasKeyword("GesturePack_s1_02"))
			{
				AddGesture(cr, "GesturePack_s1_02", "Subservient");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86004);
			}
		}
}

[ItemScript(86005)]
public class ProtestGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86005);
			if (!cr.HasKeyword("GesturePack_s1_03"))
			{
				AddGesture(cr, "GesturePack_s1_03", "Protest");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86005);
			}
		}
}

[ItemScript(86006)]
public class NavigationGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86006);
			if (!cr.HasKeyword("GesturePack_s1_04"))
			{
				AddGesture(cr, "GesturePack_s1_04", "Navigation");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86006);
			}
		}
}

[ItemScript(86007)]
public class NiftyGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86007);
			if (!cr.HasKeyword("GesturePack_s1_05"))
			{
				AddGesture(cr, "GesturePack_s1_05", "Nifty");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86007);
			}
		}
}

[ItemScript(86008)]
public class SolicitingGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86008);
			if (!cr.HasKeyword("GesturePack_s1_06"))
			{
				AddGesture(cr, "GesturePack_s1_06", "Soliciting");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86008);
			}
		}
}

[ItemScript(86009)]
public class Ahing2GestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86009);
			if (!cr.HasKeyword("GesturePack_s1_07"))
			{
				AddGesture(cr, "GesturePack_s1_07", "Ahing 2");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86009);
			}
		}
}

[ItemScript(86010)]
public class EccoGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86010);
			if (!cr.HasKeyword("GesturePack_s1_08"))
			{
				AddGesture(cr, "GesturePack_s1_08", "Ecco");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86010);
			}
		}
}

[ItemScript(86011)]
public class AhryeonGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86011);
			if (!cr.HasKeyword("GesturePack_s1_09"))
			{
				AddGesture(cr, "GesturePack_s1_09", "Ahryeon");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86011);
			}
		}
}

[ItemScript(86012)]
public class HeadacheGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86012);
			if (!cr.HasKeyword("GesturePack_s1_10"))
			{
				AddGesture(cr, "GesturePack_s1_10", "Headache");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86012);
			}
		}
}

[ItemScript(86013)]
public class RetailGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86013);
			if (!cr.HasKeyword("GesturePack_s1_11"))
			{
				AddGesture(cr, "GesturePack_s1_11", "Retail");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86013);
			}
		}
}

[ItemScript(86014)]
public class CameraGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86014);
			if (!cr.HasKeyword("GesturePack_s1_12"))
			{
				AddGesture(cr, "GesturePack_s1_12", "Camera");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86014);
			}
		}
}

[ItemScript(86015)]
public class ResearchGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86015);
			if (!cr.HasKeyword("GesturePack_s1_13"))
			{
				AddGesture(cr, "GesturePack_s1_13", "Research");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86015);
			}
		}
}

[ItemScript(86041)]
public class ApplauseGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86041);
			if (!cr.HasKeyword("cheerup"))
			{
				AddGesture(cr, "cheerup", "Applause");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86041);
			}
		}
}

[ItemScript(86071)]
public class InsectCollectorGestureItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
		{
			cr.Inventory.Remove(86071);
			if (!cr.HasKeyword("bughunting_gesture"))
			{
				AddGesture(cr, "bughunting_gesture", "Insect Collector");
			}
			else
			{
				Send.Notice(cr, L("You already have this gesture."));
				cr.GiveItem(86071);
			}
		}
}