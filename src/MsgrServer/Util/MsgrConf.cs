// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Util.Configuration;

namespace Aura.Msgr.Util
{
    public class MsgrConf : BaseConf
    {
        public MsgrConf()
        {
            Msgr = new MsgrConfFile();
        }

        /// <summary>
        ///     msgr.conf
        /// </summary>
        public MsgrConfFile Msgr { get; protected set; }

        public override void Load()
        {
            LoadDefault();
            Msgr.Load();
        }
    }

    /// <summary>
    ///     Represents msgr.conf
    /// </summary>
    public class MsgrConfFile : ConfFile
    {
        public int Port { get; protected set; }
        public int MaxFriends { get; protected set; }

        public void Load()
        {
            Require("system/conf/msgr.conf");

            Port = GetInt("port", 8002);
            MaxFriends = Math.Max(0, GetInt("max_friends", 0));
        }
    }
}