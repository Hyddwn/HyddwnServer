// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration.Files;

namespace Aura.Shared.Util.Configuration
{
    public abstract class BaseConf : ConfFile
    {
        protected BaseConf()
        {
            Log = new LogConfFile();
            Database = new DatabaseConfFile();
            Localization = new LocalizationConfFile();
            Internal = new InterConfFile();
            Premium = new PremiumConfFile();
        }

        /// <summary>
        ///     log.conf
        /// </summary>
        public LogConfFile Log { get; protected set; }

        /// <summary>
        ///     database.conf
        /// </summary>
        public DatabaseConfFile Database { get; }

        /// <summary>
        ///     localization.conf
        /// </summary>
        public LocalizationConfFile Localization { get; }

        /// <summary>
        ///     internal.conf
        /// </summary>
        public InterConfFile Internal { get; }

        /// <summary>
        ///     premium.conf
        /// </summary>
        public PremiumConfFile Premium { get; }

        /// <summary>
        ///     Loads several conf files that are generally required,
        ///     like log, database, etc.
        /// </summary>
        protected void LoadDefault()
        {
            Log.Load();
            Database.Load();
            Localization.Load();
            Internal.Load();
            Premium.Load();

            if (Internal.Password == "change_me")
                Util.Log.Warning("Using the default inter server password is risky, please change it in 'inter.conf'.");
        }

        public abstract void Load();
    }
}