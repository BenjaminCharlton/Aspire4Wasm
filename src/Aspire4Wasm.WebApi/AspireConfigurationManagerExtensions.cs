// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Extensions.Configuration;

public static class AspireConfigurationManagerExtensions
{

    private static readonly string[] _protocols = ["http", "https"];

    /// <summary>
    /// Gets the URL of an Aspire resource endpoint matching the resourceName and endpointName.
    /// </summary>
    /// <param name="config">An instance of <see cref="ConfigurationManager"/> storing the configuration of an ASP .NET Core Web API.</param>
    /// <param name="resourceName">The name of a resource specified in an Aspire AppHost. E.g. "MyBlazorWebAssemblyApp".</param>
    /// <param name="endpointName">The name of an endpoint of a resource. Very common names are "http" and "https" but you are allowed to choose your own arbitrary endpoint names.</param>
    /// <returns>The URL of the endpoint as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no matching endpoint is found.</exception>
    public static string GetServiceEndpoint(this ConfigurationManager config, string resourceName, string endpointName)
    {
        return config.GetSection($"services:{resourceName}:{endpointName}")
            .GetChildren()
            .Select(child => child.Get<string>())
            .Single() ?? throw new InvalidOperationException($"No Aspire service for resource '{resourceName}' has an endpoint named '{endpointName}'.");
    }

    /// <summary>
    /// Gets the endpoint URLs of Aspire resources matching any of the resourceNames.
    /// </summary>
    /// <param name="config">An instance of <see cref="ConfigurationManager"/> storing the configuration of an ASP .NET Core Web API.</param>
    /// <param name="resourceNames">The names of resources specified in an Aspire AppHost. E.g. "MyStandaloneBlazorWebAssemblyApp, MyHostedBlazorWebAssemblyApp, MyBlazorWebApp, SomeOtherClientApp".</param>
    /// <returns>The URLs of all matching endpoints as a string array, or an empty array if no matches are found.</returns>
    public static string[] GetServiceEndpoints(this ConfigurationManager config, params string[] resourceNames)
    {
        return [.. resourceNames
            .SelectMany(config.GetServiceEndpoints)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    /// Gets the endpoint URLs of Aspire resources matching the resourceName.
    /// </summary>
    /// <param name="config">An instance of <see cref="ConfigurationManager"/> storing the configuration of an ASP .NET Core Web API.</param>
    /// <param name="resourceName">The names of resource specified in an Aspire AppHost. E.g. "MyBlazorClient".</param>
    /// <returns>The URLs of all matching endpoints as a string array, or an empty array if no matches are found. Typically this would be two results. One for "http" the other for "https".</returns>
    public static string[] GetServiceEndpoints(this ConfigurationManager config, string resourceName)
    {
        return [.. _protocols.SelectMany(protocol =>
                config.GetSection($"services:{resourceName}:{protocol}")
                      .GetChildren()
                      .Select(child => child.Get<string>())
            )
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)];
    }
}
