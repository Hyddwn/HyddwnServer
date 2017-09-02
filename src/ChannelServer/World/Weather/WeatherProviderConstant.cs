// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Data.Database;

namespace Aura.Channel.World.Weather
{
    /// <summary>
    ///     Official random weather pattern, based on data loaded from db.
    /// </summary>
    public class WeatherProviderConstant : IWeatherProviderConstant
    {
        public WeatherProviderConstant(int regionId, float weather)
        {
            RegionId = regionId;
            Weather = weather;
        }

        public int RegionId { get; }
        public float Weather { get; }

        public WeatherDetails GetWeather(DateTime dt)
        {
            var result = new WeatherDetails();
            var val = GetWeatherAsFloat(dt);

            if (val < 1.0f)
            {
                result.Type = WeatherType.Clear;
            }
            else if (val < 1.95f)
            {
                result.Type = WeatherType.Clouds;
            }
            else
            {
                result.Type = WeatherType.Rain;
                result.RainStrength = (int) ((val - 1.95f) * 40);
            }

            return result;
        }

        public float GetWeatherAsFloat(DateTime dt)
        {
            return Weather;
        }
    }
}