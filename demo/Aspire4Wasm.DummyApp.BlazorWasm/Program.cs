// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Aspire4Wasm.DummyApp.BlazorWasm;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(static http =>
        {
            http.AddServiceDiscovery();
            http.ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://api");
            });
        });

        await builder.Build().RunAsync().ConfigureAwait(false);
    }
}

