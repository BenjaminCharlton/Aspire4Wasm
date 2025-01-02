using Aspire.Hosting.ApplicationModel;
using Moq;

namespace Aspire.Hosting.Tests;

public class ClientSideBlazorBuilderTests
{
    [Fact]
    public void Constructor_NullProjectBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var mockSerializer = new Mock<IServiceDiscoveryInfoSerializer>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ClientSideBlazorBuilder<BlazorFluentUI_Client>(null!, mockSerializer.Object));
    }

    [Fact]
    public void Constructor_NullSerializer_ThrowsArgumentNullException()
    {
        // Arrange
        var mockProjectBuilder = new Mock<IResourceBuilder<ProjectResource>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ClientSideBlazorBuilder<BlazorFluentUI_Client>(mockProjectBuilder.Object, null!));
    }
}

public class BlazorFluentUI_Client : IProjectMetadata
{
    public string ProjectPath => "client";
}

public class BlazorFluentUI_AppHost : IProjectMetadata
{
    public string ProjectPath => "host";
}