// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Data;

namespace Aura.Channel.World
{
    public class NormalRegion : Region
    {
        public NormalRegion(int regionId)
            : base(regionId)
        {
            Data = AuraData.RegionInfoDb.Find(Id);
            if (Data == null)
                throw new Exception("Region.CreateNormal: No region info data found for '" + Id + "'.");

            var regionData = AuraData.RegionDb.Find(Id);
            if (regionData == null)
                throw new Exception("DynamicRegion: No region data found for '" + Id + "'.");

            Name = Data.Name;
            IsIndoor = regionData.Indoor;

            InitializeFromData();
        }
    }
}