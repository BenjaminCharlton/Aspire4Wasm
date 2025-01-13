// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire4Wasm.DummyApp.AppHost;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var apiService = builder.AddProject<Projects.Aspire4Wasm_DummyApp_WebApi>("apiservice");

        var blazorServer = builder.AddProject<Projects.Aspire4Wasm_DummyApp_BlazorServer>("webfrontend")
            .WithExternalHttpEndpoints()
            .WithReference(apiService)
            .WaitFor(apiService);

        var blazorWasm = blazorServer.AddWebAssemblyClient<Projects.Aspire4Wasm_DummyApp_BlazorWasm>("webclientapp")
            .WithReference(apiService);

        apiService.WithReference(blazorWasm);

        builder.Build().Run();
    }
}
