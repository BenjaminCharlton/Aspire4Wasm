// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Hosting;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for configuring service discovery between Blazor applications.
/// </summary>
public static class BlazorDistributedApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a standalone Blazor WebAssembly project (client) to the application model. Passes Aspire service discovery information to the WebAssembly client via a section called "Services" in the client's appsettings.{Environment}.json file.
    /// To use another means to pass service discovery information to the client, use the <see cref="AddStandaloneBlazorWebAssemblyProject{TStandAloneBlazorWebAssemblyProject}(IDistributedApplicationBuilder, string){TStandAloneBlazorWebAssemblyProject}(IResourceBuilder{ProjectResource}, string, Action{WebAssemblyProjectBuilderOptions})" /> method
    /// instead, passing the desired behaviour in it the <see cref="WebAssemblyProjectBuilderOptions" /> parameter.
    /// </summary>
    /// <typeparam name="TStandAloneBlazorWebAssemblyProject">A type that represents the project reference. It must be a stand-alone Blazor WebAssembly project.</typeparam>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="webAssemblyProjectName">The name of the Blazor WebAssembly project resource. This name will be used for service discovery when referenced in a dependency.</param>
    /// <returns>A reference to the <see cref="ClientSideBlazorBuilder{TProject}"/>.</returns>
    /// <remarks>
    /// <para>
    /// This overload of the <see cref="AddStandaloneBlazorWebAssemblyProject{TWebAssemblyProject}(IResourceBuilder{ProjectResource}, string)" /> method takes
    /// a <typeparamref name="TStandAloneBlazorWebAssemblyProject"/> type parameter. The <typeparamref name="TStandAloneBlazorWebAssemblyProject"/> type parameter is constrained
    /// to types that implement the <see cref="IProjectMetadata"/> interface and it also needs to be a stand-alone Blazor WebAssembly project.
    /// </para>
    /// <para>
    /// Classes that implement the <see cref="IProjectMetadata"/> interface are generated when a .NET project is added as a reference
    /// to the app host project. The generated class contains a property that returns the path to the referenced project file. Using this path
    /// .NET Aspire parses the <c>launchSettings.json</c> file to determine which launch profile to use when running the project, and
    /// what endpoint configuration to automatically generate.
    /// </para>
    /// <para>
    /// The name of the automatically generated project metadata type is a normalized version of the project name. Periods, dashes, and
    /// spaces in project names are converted to underscores. This normalization may lead to naming conflicts. If a conflict occurs the <c>&lt;ProjectReference /&gt;</c>
    /// that references the project can have a <c>AspireProjectMetadataTypeName="..."</c> attribute added to override the name.
    /// </para>
    /// <para name="kestrel">
    /// Note that endpoints coming from the Kestrel configuration are automatically added to the project. The Kestrel Url and Protocols are used
    /// to build the equivalent <see cref="EndpointAnnotation"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// Example of adding a standalone Blazor WebAssembly project to the application model with a reference to a web API.
    /// <code lang="csharp">
    /// var builder = DistributedApplication.CreateBuilder(args);
    /// 
    /// var api = builder.AddProject&lt;Projects.InventoryWebApi&gt;("inventoryapi");
    /// 
    /// builder.AddStandaloneBlazorWebAssemblyProject&lt;Projects.BlazorInventoryApp&gt;("inventoryapp")
    /// .WithReference(api);
    /// 
    /// builder.Build().Run();
    /// </code>
    /// </example>
    public static IResourceBuilder<ProjectResource> AddStandaloneBlazorWebAssemblyProject<TStandAloneBlazorWebAssemblyProject>(
        this IDistributedApplicationBuilder builder,
        string webAssemblyProjectName)
        where TStandAloneBlazorWebAssemblyProject : IProjectMetadata, new()
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        return builder.AddStandaloneBlazorWebAssemblyProject<TStandAloneBlazorWebAssemblyProject>(webAssemblyProjectName, (options, clientProject, environment) =>
        {
            var appSettingsAccessor = new AppSettingsJsonFileAccessor(clientProject.ProjectPath, environment.EnvironmentName);
            var serviceDiscoverySerializer = new JsonServiceDiscoveryInfoSerializer(appSettingsAccessor);
            options.ServiceDiscoveryInfoSerializer = serviceDiscoverySerializer;
        });
    }

    /// <summary>
    /// Adds a standalone Blazor WebAssembly project (client) to the application model. Passes Aspire service discovery information to the WebAssembly client using a means passed in the <paramref name="configure"/> action.
    /// </summary>
    /// <typeparam name="TStandAloneBlazorWebAssemblyProject">A type that represents the project reference. It must be a stand-alone Blazor WebAssembly project.</typeparam>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="webAssemblyProjectName">The name of the Blazor WebAssembly project resource. This name will be used for service discovery when referenced in a dependency.</param>
    /// <param name="configure"></param>
    /// <returns>A reference to the <see cref="ClientSideBlazorBuilder{TProject}"/>.</returns>
    /// <remarks>
    /// <para>
    /// This overload of the <see cref="AddStandaloneBlazorWebAssemblyProject{TStandAloneBlazorWebAssemblyProject}(IResourceBuilder{ProjectResource}, string, Action{WebAssemblyProjectBuilderOptions})" /> method takes
    /// a <typeparamref name="TStandAloneBlazorWebAssemblyProject"/> type parameter. The <typeparamref name="TStandAloneBlazorWebAssemblyProject"/> type parameter is constrained
    /// to types that implement the <see cref="IProjectMetadata"/> interface and it also needs to be a stand-alone Blazor WebAssembly project.
    /// </para>
    /// <para>
    /// Classes that implement the <see cref="IProjectMetadata"/> interface are generated when a .NET project is added as a reference
    /// to the app host project. The generated class contains a property that returns the path to the referenced project file. Using this path
    /// .NET Aspire parses the <c>launchSettings.json</c> file to determine which launch profile to use when running the project, and
    /// what endpoint configuration to automatically generate.
    /// </para>
    /// <para>
    /// The name of the automatically generated project metadata type is a normalized version of the project name. Periods, dashes, and
    /// spaces in project names are converted to underscores. This normalization may lead to naming conflicts. If a conflict occurs the <c>&lt;ProjectReference /&gt;</c>
    /// that references the project can have a <c>AspireProjectMetadataTypeName="..."</c> attribute added to override the name.
    /// </para>
    /// <para name="kestrel">
    /// Note that endpoints coming from the Kestrel configuration are automatically added to the project. The Kestrel Url and Protocols are used
    /// to build the equivalent <see cref="EndpointAnnotation"/>.
    /// </para>
    /// <para name="configure">
    /// The configure action can be used to customize how the service discovery information is passed from the Blazor Server app to the Blazor WebAssembly (client) app, but you must also ensure that the
    /// Blazor WebAssembly (client) app can access, deserialize and use that information.
    /// </para>
    /// </remarks>
    /// <example>
    /// Example of adding a Blazor Server project to the application model followed by a Blazor WebAssembly client with a reference to a web API.
    /// <code lang="csharp">
    /// var builder = DistributedApplication.CreateBuilder(args);
    /// 
    /// var api = builder.AddProject&lt;Projects.InventoryWebApi&gt;("inventoryapi");
    /// 
    /// builder.AddStandaloneBlazorWebAssemblyProject&lt;Projects.BlazorInventoryApp&gt;("inventorywasmclient", (options, clientProject, environment) =>
    ///   {
    ///        var appSettingsAccessor = new AppSettingsJsonFileAccessor(clientProject.ProjectPath, environment.EnvironmentName);
    ///        var serviceDiscoverySerializer = new JsonServiceDiscoveryInfoSerializer(appSettingsAccessor);
    ///        options.ServiceDiscoveryInfoSerializer = serviceDiscoverySerializer;
    ///   });.WithReference(api);
    /// 
    /// builder.Build().Run();
    /// </code>
    /// </example>
    public static IResourceBuilder<ProjectResource> AddStandaloneBlazorWebAssemblyProject<TStandAloneBlazorWebAssemblyProject>(
            this IDistributedApplicationBuilder builder,
            string webAssemblyProjectName,
            Action<WebAssemblyProjectBuilderOptions, IProjectMetadata, IHostEnvironment> configure)
            where TStandAloneBlazorWebAssemblyProject : IProjectMetadata, new()
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var webAssemblyProjectBuilder = builder.AddProject<TStandAloneBlazorWebAssemblyProject>(webAssemblyProjectName);

        var options = new WebAssemblyProjectBuilderOptions();
        configure(options, webAssemblyProjectBuilder.Resource.GetProjectMetadata(), builder.Environment);

        return new ClientSideBlazorBuilder<TStandAloneBlazorWebAssemblyProject>(webAssemblyProjectBuilder, webAssemblyProjectName, options.ServiceDiscoveryInfoSerializer);
    }
}
