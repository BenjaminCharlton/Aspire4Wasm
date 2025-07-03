// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DummyApp.AppHost;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var api = builder.AddProject<Projects.DummyApp_WebApi>("api");

        var blazorServer = builder.AddProject<Projects.DummyApp_BlazorServer>("blazor")
            .WithReference(api)
            .WaitFor(api)
            .AddWebAssemblyClient<Projects.DummyApp_BlazorWasm>("wasm")
            .WithReference(api);

        api.WithReference(blazorServer);

        builder.Build().Run();
    }
}
