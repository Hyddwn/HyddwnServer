// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration.Files;
using System;

namespace Aura.Shared.Database
{
	/// <summary>
	/// Manages an account's premium services.
	/// </summary>
	public class PremiumServices
	{
		/// <summary>
		/// Expiration for Inventory Plus.
		/// </summary>
		/// <remarks>
		/// Based on NA.
		/// Enables:
		/// - Bags
		/// - Access to all bank tabs
		/// </remarks>
		public DateTime InventoryPlusExpiration { get; set; }

		/// <summary>
		/// Expiration of Premium Service.
		/// </summary>
		/// <remarks>
		/// Based on NA.
		/// Enables:
		/// - Bags
		/// - Access to all bank tabs
		/// - Premium Gestures
		/// - Guild creation
		/// - Renting houses
		/// - Nao birthday gift
		/// - Daily Advanced Play items
		/// - Taillteann farms
		/// - Combat Exp bonus
		/// - No camping penalty
		/// - No exp loss from reviving in town
		/// - Proficiency bonus
		/// </remarks>
		public DateTime PremiumExpiration { get; set; }

		/// <summary>
		/// Expiration of VIP Service.
		/// </summary>
		/// <remarks>
		/// Based on NA.
		/// Enables same things Premium enables, plus:
		/// - VIP inventory
		/// - Style tab
		/// - Unlimited continent warp
		/// - Daily Shadow Mission Bonus
		/// - Increased item durability
		/// </remarks>
		public DateTime VipExpiration { get; set; }

		/// <summary>
		/// Returns true if Inventory Plus is currently active.
		/// </summary>
		public bool HasInventoryPlusService { get { return this.InventoryPlusExpiration > DateTime.Now; } }

		/// <summary>
		/// Returns true if Premium Service is currently active.
		/// </summary>
		public bool HasPremiumService { get { return this.PremiumExpiration > DateTime.Now; } }

		/// <summary>
		/// Returns true if VIP Service is currently active.
		/// </summary>
		public bool HasVipService { get { return this.VipExpiration > DateTime.Now; } }

		/// <summary>
		/// Returns true if account is allowed to use bags.
		/// </summary>
		public bool CanUseBags { get { return (this.HasInventoryPlusService || this.HasPremiumService || this.HasVipService); } }

		/// <summary>
		/// Returns true if account is allowed to use all bank tabs,
		/// not only its own.
		/// </summary>
		public bool CanUseAllBankTabs { get { return this.HasInventoryPlusService || this.HasPremiumService || this.HasVipService; } }

		/// <summary>
		/// Enables services for free, based on the conf's settings.
		/// </summary>
		/// <param name="premiumConfFile"></param>
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
