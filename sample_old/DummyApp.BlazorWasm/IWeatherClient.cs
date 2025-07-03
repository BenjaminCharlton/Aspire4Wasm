// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DummyApp.HttpContracts;

namespace DummyApp.BlazorWasm;

public interface IWeatherClient
{
    Task<WeatherForecastResponse[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default);
}
