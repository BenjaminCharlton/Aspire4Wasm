// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DummyApp.HttpContracts;

namespace DummyApp.WebApi;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var clients = builder.Configuration.GetServiceEndpoints("blazor"); // Get the clients from Aspire using Service Discovery.

                policy.WithOrigins(clients); // Add the clients as allowed origins for cross origin resource sharing.
                policy.AllowAnyMethod();
                policy.WithHeaders("X-Requested-With");
                policy.AllowCredentials();
            });
        });

        // Add services to the container.
        builder.Services.AddProblemDetails();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseExceptionHandler();
        app.UseCors();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecastResponse
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast");

        app.MapDefaultEndpoints();

        app.Run();
    }

}
