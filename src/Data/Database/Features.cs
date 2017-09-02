// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
    [Serializable]
    public class FeatureData
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    /// <summary>
    ///     Indexed by feature name.
    /// </summary>
    public class FeaturesDb : DatabaseJsonIndexed<string, FeatureData>
    {
        /// <summary>
        ///     The last time an entry was read.
        /// </summary>
        public DateTime LastEntryRead { get; private set; }

        public bool IsEnabled(string featureName)
        {
            var invert = false;
            if (featureName.StartsWith("!"))
            {
                featureName = featureName.Substring(1);
                invert = true;
            }

            var entry = Entries.GetValueOrDefault(featureName);
            if (entry == null) return false;

            if (!invert)
                return entry.Enabled;
            return !entry.Enabled;
        }

        protected override void ReadEntry(JObject entry)
        {
            ParseObjectRecursive(entry, true);
            LastEntryRead = DateTime.Now;
        }

        private void ParseObjectRecursive(JObject entry, bool parentEnabled)
        {
            entry.AssertNotMissing("name", "enabled");

            var data = new FeatureData();
            data.Name = entry.ReadString("name");
            data.Enabled = entry.ReadBool("enabled") && parentEnabled;

            Entries[data.Name] = data;

            // Stop if there are no children
            if (!entry.ContainsKeys("children"))
                return;

            foreach (JObject child in entry["children"].Where(a => a.Type == JTokenType.Object))
                ParseObjectRecursive(child, data.Enabled);
        }
    }
}