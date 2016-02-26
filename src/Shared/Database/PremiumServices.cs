// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
