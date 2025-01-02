// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting;

/// <summary>
/// A builder for referencing Blazor WebAssembly (client) applications from Blazor Server applications in distributed (Aspire) applications.
/// </summary>
/// <typeparam name="TProject">A type that represents the project reference. It should be a Blazor WebAssembly (i.e. client) project.</typeparam>
/// <remarks>
/// Creates an instance of <see cref="ClientSideBlazorBuilder{TProject}"/> that can be used to pass service discovery information to a Blazor WebAssembly (client) app from an Aspire distributed application.
/// </remarks>
/// <param name="projectBuilder">The instance implementing <see cref="IResourceBuilder{ProjectResource}" /> that was used to add the Blazor WebAssembly project to the application model.</param>
/// <param name="serializer">An implementation of <see cref="IServiceDiscoveryInfoSerializer" /> that can be used to pass service discovery information to the Blazor WebAssembly (client) application. It could, for example,
/// serialize the data and save it in a JSON or XML file to be read by the client app.</param>
/// <exception cref="ArgumentNullException">Will be thrown if the <see cref="ClientSideBlazorBuilder{TProject}" /> or the <see cref="IServiceDiscoveryInfoSerializer" /> is null.</exception>
internal sealed class ClientSideBlazorBuilder<TProject>(IResourceBuilder<ProjectResource> projectBuilder, IServiceDiscoveryInfoSerializer serializer)
    : IClientSideBlazorBuilder<TProject>
    where TProject : IProjectMetadata, new()
{
    private readonly IResourceBuilder<ProjectResource> _projectBuilder = projectBuilder ?? throw new ArgumentNullException(nameof(projectBuilder));
    private readonly IServiceDiscoveryInfoSerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

    public IProjectMetadata GetProjectMetadata()
    {
        return _projectBuilder.Resource.GetProjectMetadata();
    }

    /// <summary>
    /// Injects service discovery information into the Blazor WebAssembly application from the project resource into the destination resource, using the source resource's name as the service name, after the endpoints have been allocated.
    /// </summary>
    /// <param name="source">The resource from which to extract service discovery information.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{ProjectResource}"/>.</returns>
    public IResourceBuilder<ProjectResource> WithReference(IResourceBuilder<IResourceWithServiceDiscovery> source)
    {
        _projectBuilder.ApplicationBuilder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((@event, cancellationToken) =>
        {
            _serializer.SerializeServiceDiscoveryInfo(source.Resource);
            return Task.CompletedTask;
        });

        return _projectBuilder;
    }
}