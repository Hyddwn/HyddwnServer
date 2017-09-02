// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Util.Configuration.Files
{
    /// <summary>
    ///     Represents premium.conf
    /// </summary>
    public class PremiumConfFile : ConfFile
    {
        public bool FreeInventoryPlus { get; protected set; }
        public bool FreePremium { get; protected set; }
        public bool FreeVip { get; protected set; }

        public void Load()
        {
            Require("system/conf/premium.conf");

            FreeInventoryPlus = GetBool("free_inventory_plus", true);
            FreePremium = GetBool("free_premium", false);
            FreeVip = GetBool("free_vip", false);
        }
    }
}