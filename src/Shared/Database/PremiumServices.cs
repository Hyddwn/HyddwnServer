// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration.Files;
using System;

namespace Aura.Shared.Database
{
	public class PremiumServices
	{
		public DateTime InventoryPlusExpiration { get; set; }
		public DateTime PremiumExpiration { get; set; }
		public DateTime VipExpiration { get; set; }

		public bool HasInventoryPlusService { get { return this.InventoryPlusExpiration > DateTime.Now; } }
		public bool HasPremiumService { get { return this.PremiumExpiration > DateTime.Now; } }
		public bool HasVipService { get { return this.VipExpiration > DateTime.Now; } }

		public void EvaluateFreeServices(PremiumConfFile premiumConfFile)
		{
			var freeExpiration = DateTime.Parse("2999-12-31 23:59:59");

			if (premiumConfFile.FreeInventoryPlus)
				this.InventoryPlusExpiration = freeExpiration;

			if (premiumConfFile.FreePremium)
				this.PremiumExpiration = freeExpiration;

			if (premiumConfFile.FreeVip)
				this.VipExpiration = freeExpiration;
		}
	}
}
