// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Aspire4Wasm_DummyApp_ApiService>("apiservice");

builder.AddProject<Projects.Aspire4Wasm_DummyApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
