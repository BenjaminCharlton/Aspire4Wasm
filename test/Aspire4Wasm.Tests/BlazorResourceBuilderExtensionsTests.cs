// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting.Tests;

public class BlazorResourceBuilderExtensionsTests
{

    [Fact]
    public async Task AddWebAssemblyClient_WithoutConfigure_InvokesDefaultConfiguration()
    {
        // Arrange
        var distributedApplicationBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Aspire4Wasm.DummyApp.AppHost.Program>();
        var webApiBuilder = distributedApplicationBuilder.CreateResourceBuilder(new ProjectResource("webapi"));
        var serverBuilder = distributedApplicationBuilder.CreateResourceBuilder(new ProjectResource("serverapp"));

        // Act
        var clientBuilder = serverBuilder.AddWebAssemblyClient<Projects.Aspire4Wasm_DummyApp_BlazorWasm>("clientapp").WithReference(webApiBuilder);

        // Assert
        Assert.NotNull(clientBuilder);
        Assert.IsType<IResourceBuilder<ProjectResource>>(clientBuilder, false);
    }

    [Fact]
    public async Task AddWebAssemblyClient_WithCustomConfiguration_InvokesConfigurationCallback()
    {
        // Arrange
        var distributedApplicationBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Aspire4Wasm.DummyApp.AppHost.Program>();
        var serverBuilder = distributedApplicationBuilder.CreateResourceBuilder(new ProjectResource("serverapp"));

        var optionsCaptured = false;

        // Act
        var clientBuilder = serverBuilder.AddWebAssemblyClient<Projects.Aspire4Wasm_DummyApp_BlazorWasm>("clientapp",
            (options, projectMetadata, environment) =>
            {
                optionsCaptured = true;
                Assert.Equal(distributedApplicationBuilder.Environment, environment);
            });

        // Assert
        Assert.NotNull(clientBuilder);
        Assert.True(optionsCaptured);
    }

    [Fact]
    public void AddWebAssemblyClient_HandlesNullResourceBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IResourceBuilder<ProjectResource>? nullResourceBuilder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            nullResourceBuilder!.AddWebAssemblyClient<Projects.Aspire4Wasm_DummyApp_BlazorWasm>("TestWebAssemblyProject"));
    }

    [Fact]
    public async Task AddWebAssemblyClient_EmptyProjectName_ThrowsArgumentException()
    {
        // Arrange
        var distributedApplicationBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Aspire4Wasm.DummyApp.AppHost.Program>();
        var serverBuilder = distributedApplicationBuilder.CreateResourceBuilder(new ProjectResource("serverapp"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            serverBuilder.AddWebAssemblyClient<Projects.Aspire4Wasm_DummyApp_BlazorWasm>(string.Empty));
    }
}
