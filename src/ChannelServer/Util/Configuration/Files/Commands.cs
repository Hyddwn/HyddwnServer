// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration.Files
{
    public class CommandsConfFile : ConfFile
    {
        public char Prefix { get; protected set; }
        public string Prefix2 { get; protected set; }

        public Dictionary<string, CommandAuthConf> Auth { get; protected set; }

        public void Load()
        {
            Require("system/conf/commands.conf");

            // Prefix
            Prefix = GetString("prefix", ">")[0];
            Prefix2 = new string(Prefix, 2);

            // Commands
            Auth = new Dictionary<string, CommandAuthConf>();

            foreach (var option in _options)
            {
                if (option.Key == "prefix")
                    continue;

                var sAuth = option.Value.Split(',');
                if (sAuth.Length < 2)
                {
                    Log.Error("Commands need 2 auth settings, '{0}' has less.", option.Key);
                    continue;
                }

                int auth1, auth2;
                if (!int.TryParse(sAuth[0], out auth1) || !int.TryParse(sAuth[1], out auth2))
                {
                    Log.Error("Unable to parse auth for '{0}'.", option.Key);
                    continue;
                }

                Auth[option.Key.Trim()] = new CommandAuthConf(auth1, auth2);
            }
        }

        /// <summary>
        ///     Returns auth for command, or a new auth with the default values
        ///     if the command wasn't found in the config.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="defaultAuth"></param>
        /// <param name="defaultCharAuth"></param>
        /// <returns></returns>
        public CommandAuthConf GetAuth(string command, int defaultAuth, int defaultCharAuth)
        {
            CommandAuthConf result;
            Auth.TryGetValue(command, out result);
            if (result == null)
                result = new CommandAuthConf(defaultAuth, defaultCharAuth);

            return result;
        }
    }

    public class CommandAuthConf
    {
        public int Auth, CharAuth;

        public CommandAuthConf(int auth, int charAuth)
        {
            Auth = auth;
            CharAuth = charAuth;
        }
    }
}