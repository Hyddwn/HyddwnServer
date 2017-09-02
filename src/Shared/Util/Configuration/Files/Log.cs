// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Util.Configuration.Files
{
    /// <summary>
    ///     Represents log.conf
    /// </summary>
    public class LogConfFile : ConfFile
    {
        public bool Archive { get; protected set; }
        public LogLevel Hide { get; protected set; }

        public void Load()
        {
            Require("system/conf/log.conf");

            Archive = GetBool("archive", true);
            Hide = (LogLevel) GetInt("cmd_hide", (int) LogLevel.Debug);

            if (Archive)
                Log.Archive = "log/archive/";
            Log.LogFile = string.Format("log/{0}.txt",
                AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "").Replace(".vshost", ""));
            Log.Hide |= Hide;
        }
    }
}