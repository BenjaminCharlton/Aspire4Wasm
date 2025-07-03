// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http.Json;
using DummyApp.HttpContracts;

namespace DummyApp.BlazorWasm;

public class WeatherApiClient(HttpClient httpClient) : IWeatherClient
{
    public async Task<WeatherForecastResponse[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecastResponse>? forecasts = null;

        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecastResponse>("/weatherforecast", cancellationToken).ConfigureAwait(false))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? [];
    }
}
