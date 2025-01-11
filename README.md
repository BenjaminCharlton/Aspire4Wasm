# Aspire4Wasm    
An easy way to pass service discovery information from a distributed application in Aspire down to your Blazor WebAssembly (client) applications. You can then add service discovery to the client app just like any other Aspire resource. Don't need the source code? Get the Nuget package: https://www.nuget.org/packages/Aspire4Wasm/

## Problem statement
.NET Aspire doesn't currently (as of early 2025) facilitate a Blazor WebAssembly (client) app discovering Aspire resources, even if the app has been added to the distributed application, because Blazor WebAssembly apps run in the browser and are "standalone". This has been commented on here:

* https://github.com/dotnet/aspire/issues/4785

The expectation is that these apps will need to be aware of the web APIs they're supposed to call without relying on Aspire, and that they will store these in `appsettings.json` or `appsettings.{environmentName}.json`. This works fine, but if the endpoint changes, or if it differs in your development and production environments, you have to remember to manage those changes in your client app as well as your other resources. This is precisely the problem Aspire is intended to solve.

My little library Aspire4Wasm solves the problem by writing the service discovery information to the `appsettings.{environmentName}.json` file of your client app for you.

## Quickstart
Install Aspire4Wasm in your AppHost project via the Nuget package. No need to install it on the client project.
In your AppHost project's Program.cs file:

1. Add the Web Apis you want your client to be able to call.
2. Add your Blazor Server app then chain a call to `AddWebAssemblyClient` to add your client app.
5. Chain a call to `WithReference` to point the client to each web API (you can repeat this for as many Web APIs as you need)

In your client's `Program.cs` file:

1. Call `AddServiceDiscovery`
2. Configure your `HttpClient`s either globally or one at a time. In each client's `BaseAddress` property, use the name you gave to the resource in your AppHost.

See the example below:

## Example Program.cs in AppHost
```
var builder = DistributedApplication.CreateBuilder(args);

var inventoryApi = builder.AddProject<Projects.AspNetCoreWebApi>("inventoryapi");
var billingApi = builder.AddProject<Projects.SomeOtherWebApi>("billingapi");

builder.AddProject<Projects.Blazor>("blazorServer")
    .AddWebAssemblyClient<Projects.Blazor_Client>("blazorWasmClient")
    .WithReference(inventoryApi)
    .WithReference(billingApi);

builder.Build().Run();
```
## Example Program.cs in your Blazor WebAssembly Client
Install (on the WebAssembly client) the `Microsoft.Extensions.ServiceDiscovery` Nuget package to get the official Aspire service discovery functionality that is going to read your resource information from your app settings.
```
builder.Services.AddServiceDiscovery();
builder.Services.ConfigureHttpClientDefaults(static http =>
{
    http.AddServiceDiscovery();
});

builder.Services.AddHttpClient<IInventoryService, InventoryService>(
    client =>
    {
        client.BaseAddress = new Uri("https+http://inventoryapi");
    });

    builder.Services.AddHttpClient<IBillingService, BillingService>(
    client =>
    {
        client.BaseAddress = new Uri("https+http://billingapi");
    });
```
(I recommend extracting the names of the resources (e.g. "billingapi" and "inventoryapi") into a string constant somewhere shared by the AppHost and the client, and your other referenced projects. That way, the name is consistent throughout the whole solution.) You probably do this already though!
## Default behaviour
Using the default behaviour (in the example) your AppHost will write the service discovery information for all the referenced resources into the `appsettings.{environmentName}.json` file of your client app for you.
It uses the following structure. The structure is important because it allows Aspire to "discover" the information on the client.
```
{
  "Services": {
    "inventoryapi": {
      "https": [
        "https://localhost:1234"
      ],
      "http": [
        "http://localhost:4321"
      ]
    },
    "billingapi": {
      "https": [
        "https://localhost:9876"
      ],
      "http": [
        "http://localhost:6789"
      ]
    }
  }
}
```
## Custom behaviours (optional)
If you want to serialize the service discovery information some other way in your WebAssembly application (for example, in a different JSON file, or in an XML file) you can do so in the AppHost `Program.cs` by creating a custom implementation of `IServiceDiscoveryInfoSerializer` and passing it to the call to `AddWebAssemblyClient` via the `WebAssemblyProjectBuilderOptions` class, like this:
```
var builder = DistributedApplication.CreateBuilder(args);

var inventoryApi = builder.AddProject<Projects.AspNetCoreWebApi>("inventoryapi");
var billingApi = builder.AddProject<Projects.SomeOtherWebApi>("billingapi");

builder.AddProject<Projects.Blazor>("blazorServer")
    .AddWebAssemblyClient<Projects.Blazor_Client>("blazorWasmClient" options => {
        options.ServiceDiscoveryInfoSerializer = yourImplementation;
    })
    .WithReference(inventoryApi)
    .WithReference(billingApi);

builder.Build().Run();
```
If you choose to make a custom implementation of `IServiceDiscoveryInfoSerializer`, you only need to override one method:
```
public void SerializeServiceDiscoveryInfo(IResourceWithServiceDiscovery resource) { }
```
Note: If you choose to override the default behaviour with an output format that Aspire can't read from your WebAssembly client app, you'll also need to override the discovery behaviour on the client, which is outside the scope of what I've developed here.
## CORS (optional) in your web API
Now that your Blazor WebAssembly (client) app has a reference to one or more web APIs, you can
## Contributing
I'm a hobbyist. I know there are loads of people out there who be able to improve this in ways I can't, or see opportunities for improvement that I can't even imagine. If you want to contribute, bring it on! Send me a pull request.
