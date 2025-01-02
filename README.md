# Aspire4Wasm
An easy way to pass service discovery information from a distributed application in Aspire down to your Blazor WebAssembly (client) applications. You can then add service discovery to the client app just like any other Aspire resource, and it just works!

## Problem statement
.NET Aspire doesn't currently (as of early 2025) facilitate a Blazor WebAssembly (client) app discovering Aspire resources, even if the app has been added to the distributed application because Blazor WebAssembly apps run in the browser and are "standalone". This has been commented on here:

https://github.com/dotnet/aspire/issues/4785
https://stackoverflow.com/questions/78607828/how-to-add-aspire-to-blazor-webassembly-standalone-app-in-net-8

The expectation is that these apps will still be aware of the web APIs they're supposed to call and can store these in appsettings.json or appsettings.{environmentName}.json. This works fine, but if the endpoint changes, or if it differs in your development and production environments, you have to remember to manage those changes in your client app as well as your other resources. This library solves that problem by writing the service discovery information to the appsettings.{environmentName}.json file of your client app for you.

## Quickstart
Install Aspire4Wasm in your AppHost project via the Nuget package. No need to install it on the client project.
In your AppHost project's Program.cs file:

1. Add the Web Apis you want your client to be able to call.
2. Add your Blazor Server app then chain a call to AddWebAssemblyClient to add your client app.
5. Chain a call to WithReference to point the client to each web API (you can repeat this for as many Web APIs as you need)

In your client's Program.cs file:

1. Call AddServiceDiscovery
2. Configure your HttpClients either globally or one at a time. In each client's BaseAddress property, use the name you gave to the resource in your AppHost.

See the example below:

## Example Program.cs in AppHost
var builder = DistributedApplication.CreateBuilder(args);

var inventoryApi = builder.AddProject<Projects.AspNetCoreWebApi>("inventoryapi");
var billingApi = builder.AddProject<Projects.SomeOtherWebApi>("billingapi");

builder.AddProject<Projects.Blazor>("blazorServer")
    .AddWebAssemblyClient<Projects.Blazor_Client>("blazorWasmClient")
    .WithReference(inventoryApi)
    .WithReference(billingApi);

builder.Build().Run();

## Example Program.cs in your Blazor WebAssembly Client
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

## Default behaviour
Using the default behaviour (in the example) your AppHost will write the service discovery information for all the referenced resources into the appsettings.{environmentName}.json file of your client app for you.
It uses the following structure, recommended by the Aspire team:
{
  "Services": {
    "resourceName": {
      "https": [
        "https://localhost:7222"
      ],
      "http": [
        "http://localhost:5050"
      ]
    }
  }
}

## Custom behaviours
If you want to serialize the service discovery information some other way in your WebAssembly application (for example, in a different JSON file, or in an XML file) you can do so in the AppHost Program.cs by creating a custom implementation of IServiceDiscoveryInfoSerializer and passing it to the call to AddWebAssemblyClient via the WebAssemblyProjectBuilderOptions class, like this:

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

## Custom implementations of IServiceDiscoveryInfoSerializer
If you choose to make a custom implementation, you only need to override on method, ensuring that however you choose to serialize the information, Aspire will be able to read it in your client app:

public void SerializeServiceDiscoveryInfo(IResourceWithServiceDiscovery resource) { }
