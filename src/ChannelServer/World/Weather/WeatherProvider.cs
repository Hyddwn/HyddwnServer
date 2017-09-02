// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Data.Database;

namespace Aura.Channel.World.Weather
{
    public interface IWeatherProvider
    {
        int RegionId { get; }
        float GetWeatherAsFloat(DateTime dt);
        WeatherDetails GetWeather(DateTime dt);
    }

    public interface IWeatherProviderTable : IWeatherProvider
    {
        string Name { get; }
        int GroupId { get; }
    }

    public interface IWeatherProviderConstant : IWeatherProvider
    {
        float Weather { get; }
    }

    public interface IWeatherProviderConstantSmooth : IWeatherProvider
    {
        float Weather { get; }
        float WeatherBefore { get; }
        int TransitionTime { get; }
    }
}