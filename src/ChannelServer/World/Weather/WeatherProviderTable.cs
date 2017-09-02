// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Data;
using Aura.Data.Database;

namespace Aura.Channel.World.Weather
{
    /// <summary>
    ///     Official random weather pattern, based on data loaded from db.
    /// </summary>
    public class WeatherProviderTable : IWeatherProviderTable
    {
        public WeatherProviderTable(int regionId, string name)
        {
            Name = name;
            RegionId = regionId;
            GroupId = AuraData.RegionInfoDb.GetGroupId(regionId);
        }

        public string Name { get; }
        public int RegionId { get; }
        public int GroupId { get; }

        public WeatherDetails GetWeather(DateTime dt)
        {
            return AuraData.WeatherTableDb.GetWeather(Name, dt);
        }

        public float GetWeatherAsFloat(DateTime dt)
        {
            var details = GetWeather(dt);
            if (details == null)
                return 0.5f;

            if (details.Type == WeatherType.Clear)
                return 0.5f;

            if (details.Type == WeatherType.Clouds)
                return 1.0f;

            return 1.95f + 0.5f / 20 * details.RainStrength;
        }
    }
}