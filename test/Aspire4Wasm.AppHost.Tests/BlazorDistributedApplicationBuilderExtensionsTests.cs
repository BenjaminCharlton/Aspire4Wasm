// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting.Tests;

public class BlazorDistributedApplicationBuilderExtensionsTests
{

    [Fact]
    public async Task AddStandAloneWebAssemblyProject_WithoutConfigure_InvokesDefaultConfiguration()
    {
        // Arrange
        var distributedApplicationBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Aspire4Wasm_DummyApp_AppHost>();
        var webApiBuilder = distributedApplicationBuilder.CreateResourceBuilder(new ProjectResource("webapi"));

        // Act
        var clientBuilder = distributedApplicationBuilder.AddStandaloneBlazorWebAssemblyProject<Projects.Aspire4Wasm_DummyApp_Web>("clientapp").WithReference(webApiBuilder);

        // Assert
        Assert.NotNull(clientBuilder);
        Assert.IsType<IResourceBuilder<ProjectResource>>(clientBuilder, false);
    }

    [Fact]
    public async Task AddStandAloneWebAssemblyProject_WithCustomConfiguration_InvokesConfigurationCallback()
    {
        // Arrange
        var distributedApplicationBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Aspire4Wasm_DummyApp_AppHost>();

        var optionsCaptured = false;

        // Act
        var clientBuilder = distributedApplicationBuilder.AddStandaloneBlazorWebAssemblyProject<Projects.Aspire4Wasm_DummyApp_Web>("clientapp",
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
    public void AddStandAloneWebAssemblyProject_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IDistributedApplicationBuilder? nullApplicationBuilder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            nullApplicationBuilder!.AddStandaloneBlazorWebAssemblyProject<Projects.Aspire4Wasm_DummyApp_Web>("TestWebAssemblyProject"));
    }

    [Fact]
    public async Task AddStandAloneWebAssemblyProject_EmptyProjectName_ThrowsArgumentException()
    {
        // Arrange
        var distributedApplicationBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Aspire4Wasm_DummyApp_AppHost>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            distributedApplicationBuilder.AddStandaloneBlazorWebAssemblyProject<Projects.Aspire4Wasm_DummyApp_Web>(string.Empty));
    }
}
