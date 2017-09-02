// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Util.Configuration.Files
{
	/// <summary>
	/// Represents premium.conf
	/// </summary>
	public class PremiumConfFile : ConfFile
	{
		public bool FreeInventoryPlus { get; protected set; }
		public bool FreePremium { get; protected set; }
		public bool FreeVip { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/premium.conf");

			this.FreeInventoryPlus = this.GetBool("free_inventory_plus", true);
			this.FreePremium = this.GetBool("free_premium", false);
			this.FreeVip = this.GetBool("free_vip", false);
		}
	}
}
