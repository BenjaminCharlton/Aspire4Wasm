// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire4Wasm.DummyApp.AppHost;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var api = builder.AddProject<Projects.Aspire4Wasm_DummyApp_WebApi>("apiservice");

        var blazorServer = builder.AddProject<Projects.Aspire4Wasm_DummyApp_BlazorServer>("webfrontend")
            .WithReference(api)
            .WaitFor(api)
            .AddWebAssemblyClient<Projects.Aspire4Wasm_DummyApp_BlazorWasm>("webclientapp")
            .WithReference(api);

        api.WithReference(blazorServer);

        builder.Build().Run();
    }
}
