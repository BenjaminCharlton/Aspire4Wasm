// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting;

/// <summary>
/// Extension methods on the <see cref="WebAssemblyHostBuilder"/> to make Aspire work with a WebAssembly app.
/// </summary>
public static class AspireWebAssemblyHostBuilderExtensions
{
    /// <summary>
    /// Adds the core service discovery services and configures defaults for a WebAssembly project, and adds a delegate that will be used to configure
    /// all <see cref="HttpClient"/> instances to use Aspire service discovery.
    /// </summary>
    /// <param name="builder">The host builder for the WebAssembly application.</param>
    /// <returns>The builder.</returns>
    public static WebAssemblyHostBuilder AddServiceDefaults(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddServiceDiscovery();
        });

        return builder;
    }
}
