// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
    [Serializable]
    public class RegionData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Indoor { get; set; }
    }

    /// <summary>
    ///     Indexed by region id.
    /// </summary>
    public class RegionDb : DatabaseJsonIndexed<int, RegionData>
    {
        public bool TryGetRegionName(int regionId, out string name)
        {
            name = null;

            if (!Entries.ContainsKey(regionId))
                return false;

            name = Entries[regionId].Name;

            return true;
        }

        protected override void ReadEntry(JObject entry)
        {
            entry.AssertNotMissing("id", "name", "indoor");

            var info = new RegionData();
            info.Id = entry.ReadInt("id");
            info.Name = entry.ReadString("name");
            info.Indoor = entry.ReadBool("indoor");

            Entries[info.Id] = info;
        }
    }
}