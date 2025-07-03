// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DummyApp.BlazorWasm;
using DummyApp.HttpContracts;

namespace DummyApp.BlazorServer;

public class NoOpWeatherClient : IWeatherClient
{
    public async Task<WeatherForecastResponse[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult<WeatherForecastResponse[]>([new WeatherForecastResponse(DateOnly.MinValue, 20, "Server")]).ConfigureAwait(false);
    }
}
