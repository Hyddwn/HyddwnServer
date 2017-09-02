// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aura.Shared.Util.Configuration;

namespace Aura.Login.Util
{
    public class LoginConf : BaseConf
    {
        public LoginConf()
        {
            Login = new LoginConfFile();
        }

        /// <summary>
        ///     login.conf
        /// </summary>
        public LoginConfFile Login { get; protected set; }

        public override void Load()
        {
            LoadDefault();
            Login.Load();
        }
    }

    /// <summary>
    ///     Represents login.conf
    /// </summary>
    public class LoginConfFile : ConfFile
    {
        public int Port { get; protected set; }

        public bool NewAccounts { get; protected set; }
        public int NewAccountPoints { get; protected set; }
        public bool EnableSecondaryPassword { get; protected set; }

        public bool ConsumeCharacterCards { get; protected set; }
        public bool ConsumePetCards { get; protected set; }
        public bool ConsumePartnerCards { get; protected set; }

        public int DeletionWait { get; protected set; }

        public int WebPort { get; protected set; }
        public HashSet<string> TrustedSources { get; protected set; }

        public Regex IdentAllow { get; protected set; }

        public void Load()
        {
            Require("system/conf/login.conf");

            Port = GetInt("port", 11000);
            NewAccounts = GetBool("new_accounts", true);
            NewAccountPoints = GetInt("new_account_points", 0);
            EnableSecondaryPassword = GetBool("enable_secondary", false);

            ConsumeCharacterCards = GetBool("consume_character_cards", true);
            ConsumePetCards = GetBool("consume_pet_cards", true);
            ConsumePartnerCards = GetBool("consume_partner_cards", true);

            DeletionWait = GetInt("deletion_wait", 107);

            WebPort = GetInt("web_port", 10999);

            TrustedSources = new HashSet<string>();
            var trusted = GetString("trusted_sources", "127.0.0.1").Split(',');
            foreach (var source in trusted)
                TrustedSources.Add(source.Trim());

            IdentAllow = new Regex(GetString("ident_allow", ""), RegexOptions.Compiled);
        }

        /// <summary>
        ///     Returns true if source is listed in trusted sources.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool IsTrustedSource(string source)
        {
            return TrustedSources.Contains(source);
        }
    }
}