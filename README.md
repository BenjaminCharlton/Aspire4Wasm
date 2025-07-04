# Aspire4Wasm    
An easy way to pass service discovery information from a distributed application in Aspire down to your Blazor WebAssembly (client) applications.
You can add service discovery to the client app just like any other Aspire resource.
It also allows you to configure your WebAssembly application(s) as `AllowedOrigins` in CORS in your ASP .NET Core Web API(s).

## Problem statement
.NET Aspire doesn't currently (as of mid 2025) facilitate a Blazor WebAssembly (client) app discovering Aspire resources, even if the app has been added to the distributed application, because Blazor WebAssembly apps run in the browser and are "standalone".
This has been commented on here:

* https://github.com/dotnet/aspire/issues/4785

Microsoft's expectation is that these apps will need to be aware of the web APIs they're supposed to call without relying on Aspire, and that they will store these in `appsettings.json` or `appsettings.{environmentName}.json`.
This works fine, but if the endpoint changes, or if it differs in your development and production environments, you have to remember to manage those changes in your client app as well as your other resources.
This is precisely the problem Aspire is intended to solve.

My little library Aspire4Wasm solves the problem by:
1. Writing the service discovery information from the AppHost to the `appsettings.{environmentName}.json` file of your client app for you
2. Providing some helper methods to set up service discovery on your WebAssembly clients
3. Providing some helper methods for configuring CORS on your ASP .NET Core Web API projects, so that the WebAssembly clients are allowed to call the API.

## Recent changes
Version 6.x.x separates the solution into three separate Nuget packages.
If you used the original `Aspire4Wasm` package (versions 1.x.x to 5.x.x), you need to change to the new `Aspire4Wasm.AppHost` package.
You can then optionally install the two new packages, detailed below.

## Don't need the source code? Get the Nuget packages:

1. For your AppHost project: https://www.nuget.org/packages/Aspire4Wasm.AppHost/ (essential)
2. For your WebAssembly project: https://www.nuget.org/packages/Aspire4Wasm.WebAssembly/ (helpful, but you can write the helper methods yourself if you prefer)
3. For your WebApi project: https://www.nuget.org/packages/Aspire4Wasm.WebApi/ (optional, if you need to configure CORS)

## Aspire4Wasm sample apps

* Aspire + WebAPI + Hosted Blazor WebAssembly + Bootstrap: https://github.com/BenjaminCharlton/Aspire4Wasm.Samples.Hosted
* Aspire + WebAPI + Standalone Blazor WebAssembly + Bootstrap: https://github.com/BenjaminCharlton/Aspire4Wasm.Samples.Standalone
* Aspire + WebAPI + Hosted Blazor WebAssembly + MudBlazor: https://github.com/BenjaminCharlton/Aspire4Wasm.Samples.Hosted.Mud
* Aspire + WebAPI + Standalone Blazor WebAssembly + MudBlazor: https://github.com/BenjaminCharlton/Aspire4Wasm.Samples.Standalone.Mud

## QuickStart For a web API with a stand-alone Blazor WebAssembly client
### In your AppHost project
1. Install Aspire4Wasm.AppHost via the Nuget package.
2. In `Program.cs`:
 a. Add the Web Api project(s) the usual way, with `builder.AddProject`.
 b. Add the stand-alone Blazor WebAssembly project(s) with `builder.AddStandaloneBlazorWebAssemblyProject`.
 c. Chain calls to `WithReference` in one or both directions.
    That is, the client(s) need(s) a reference to any API(s) and the API(s) need a reference any client(s) that call it.
    If you've configured `AllowAnyOrigin` in CORS (which isn't very isn't very secure) then your API(s) won't need references to clients.
3. For example:
```
var builder = DistributedApplication.CreateBuilder(args);

var inventoryApi = builder.AddProject<Projects.AspNetCoreWebApi>("inventoryapi");
var billingApi = builder.AddProject<Projects.SomeOtherWebApi>("billingapi");

var blazorApp = builder.AddStandAloneBlazorWebAssemblyProject<Projects.Blazor>("blazor")
    .WithReference(inventoryApi)
    .WithReference(billingApi);

inventoryApi.WithReference(blazorApp);
// Could repeat the line above for billingApi but have not because its CORS settings say AllowAnyOrigin.

builder.Build().Run();
```
### In your stand-alone Blazor WebAssembly project
1. Install Aspire4Wasm.WebAssembly via the Nuget package.
2. In `Program.cs`:
 a. Call `builder.AddServiceDefaults()`. This adds Aspire service discovery to your Blazor WebAssembly app and also configures every `HttpClient` to use it by default.
 b. Configure your services that call APIs like this: `builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new Uri("https+http://api"));`
    where (in this case) "api" is the arbitrary resource name you gave to the web API in the AppHost's `Program.cs`.
3. Example:
```
builder.AddServiceDefaults();

builder.Services.AddHttpClient<InventoryApiService>(client => client.BaseAddress = new Uri("https+http://inventoryapi"));
builder.Services.AddHttpClient<BillingApiService>(client => client.BaseAddress = new Uri("https+http://billingapi"));
```
4. Change `Properties\launchSettings.json` so that `launchBrowser` is `false`. You want your Aspire dashboard to launch when you run your app, not your Blazor app.
    If your Blazor app launches directly it will be at a randomly assigned port in Kestrel and nothing will work.
### In your ASP .NET Core Web API project
1. Install Aspire4Wasm.WebApi via the Nuget package.
2. In `Program.cs`
 a. Call `builder.AddServiceDefaults();` This adds Aspire service discovery to your ASP .NET Core Web API so it can find the references you passed to your API clients. 
 b. Configure CORS using one of the helper methods on `builder.Configuration`. They are `GetServiceEndpoint(string, string)`, `GetServiceEndpoints(string)` and `GetServiceEndpoints(params string)`.
 Assuming your app has one client, the simplest overload will work fine. The example below assumes you named the client resource "blazor" in your Aspire AppHost.
 ```
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var clients = builder.Configuration.GetServiceEndpoints("blazor"); // Get the http and https endpoints for the client known by resource name as "blazor" in the AppHost.
            // var clients = builder.Configuration.GetServiceEndpoints("blazor1", "blazor2"); // This overload does the same thing for multiple clients.
            // var clients = builder.Configuration.GetServiceEndpoint("blazor", "http"); // This overload gets a single named endpoint for a single resource. In this case, the "http" endpoint for the "blazor" resource.

            policy.WithOrigins(clients); // Add the clients as allowed origins for cross origin resource sharing.
            policy.AllowAnyMethod();
            policy.WithHeaders("X-Requested-With");
        });
    });
```
## QuickStart For a web API with a hosted Blazor WebAssembly client (or a Blazor United app with default rendermode of `InteractiveAuto`)

Note that, even with the new "Blazor United" template, as soon as you select `InteractiveAuto` as the default rendermode for your Blazor Web App project, it ceases to be "united". It will be divided into two projects, just like a hosted WebAssembly project. I'm not saying that's wrong. It just does. Aspire4Wasm works the same way for both.
### In your AppHost project
1. Install Aspire4Wasm.AppHost via the Nuget package.
2. In `Program.cs`:
 a. Add the Web Api project(s) the usual way, with `builder.AddProject`.
 b. Add your Blazor Server project with `builder.AddProject` as usual, then chain a call to `AddWebAssemblyClient` to add your client app.
 c. Chain calls to `WithReference` in one or both directions.
 That is, the client(s) need(s) a reference to any API(s) and the API(s) need a reference any client(s) that call it.
 If you've configured `AllowAnyOrigin` in CORS (which isn't very isn't very secure) then your API(s) won't need references to clients.
3. Example:
```
var builder = DistributedApplication.CreateBuilder(args);

var inventoryApi = builder.AddProject<Projects.AspNetCoreWebApi>("inventoryapi");
var billingApi = builder.AddProject<Projects.SomeOtherWebApi>("billingapi");

var blazorApp = builder.AddProject<Projects.HostedBlazor>("hostedblazor")
    .AddWebAssemblyClient("wasmclient")
    .WithReference(inventoryApi)
    .WithReference(billingApi);

inventoryApi.WithReference(blazorApp);
// Could repeat the line above for billingApi but have not because its CORS settings say AllowAnyOrigin.

builder.Build().Run();
```
### In your Blazor Server project
No Aspire4Wasm Nuget package necessary in this project.
### In your Blazor WebAssembly project
1. Install Aspire4Wasm.WebAssembly via the Nuget package.
2. In `Program.cs`:
 a. Call `builder.AddServiceDefaults()`. This adds Aspire service discovery to your Blazor WebAssembly app and also configures every `HttpClient` to use it by default.
 b. Configure your services that call APIs like this: `builder.Services.AddHttpClient<IWeatherClient, WeatherApiClient>(client => client.BaseAddress = new Uri("https+http://api"));`
    where (in this case) "api" is the arbitrary resource name you gave to the web API in the AppHost's `Program.cs`.
    The above example assumes that you're going to use one implementation of `IWeatherClient` when the page is rendered (or pre-rendered) on the server, and a different implementation when
    it is rendered on the client. This pattern will be familiar for people accustomed to `InteractiveAuto` render mode.
3. Example:
```
builder.AddServiceDefaults();

builder.Services.AddHttpClient<IInventoryService, InventoryApiService>(client => client.BaseAddress = new Uri("https+http://inventoryapi"));
builder.Services.AddHttpClient<IBillingService, BillingApiService>(client => client.BaseAddress = new Uri("https+http://billingapi"));
```
4. Change `Properties\launchSettings.json` so that `launchBrowser` is `false`. You want your Aspire dashboard to launch when you run your app, not your Blazor app.
    If your Blazor app launches directly it will be at a randomly assigned port in Kestrel and nothing will work.
### In your ASP .NET Core Web API project
1. Install Aspire4Wasm.WebApi via the Nuget package.
2. In `Program.cs`
 a. Call `builder.AddServiceDefaults();` This adds Aspire service discovery to your ASP .NET Core Web API so it can find the references you passed to your API clients. 
 b. Configure CORS using one of the helper methods on `builder.Configuration`. They are `GetServiceEndpoint(string, string)`, `GetServiceEndpoints(string)` and `GetServiceEndpoints(params string)`.
 Assuming your app has one client, the simplest overload will work fine. The example below assumes you named your Blazor host project "hostedblazor" in your Aspire AppHost.
 That's right, you're using the name you gave to your Blazor Server host project here, but it will also grant access to your Blazor WebAssembly client project.
 ```
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var clients = builder.Configuration.GetServiceEndpoints("hostedblazor"); // Get the http and https endpoints for the client known by resource name as "blazor" in the AppHost.
            // var clients = builder.Configuration.GetServiceEndpoints("blazor1", "blazor2"); // This overload does the same thing for multiple clients.
            // var clients = builder.Configuration.GetServiceEndpoint("blazor", "http"); // This overload gets a single named endpoint for a single resource. In this case, the "http" endpoint for the "blazor" resource.

            policy.WithOrigins(clients); // Add the clients as allowed origins for cross origin resource sharing.
            policy.AllowAnyMethod();
            policy.WithHeaders("X-Requested-With");
        });
    });
```
## What it's doing under the hood
All the examples use Aspire4Wasm's default behaviour.
That is, AppHost will write the service discovery information for all the referenced resources into the `appsettings.{environmentName}.json` file of your client apps for you.
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
If you want to serialize the service discovery information some other way in your WebAssembly application (for example, in a different JSON file, or in an XML file) you can.
1. First create a custom implementation of `IServiceDiscoveryInfoSerializer`
2. Then in the `Program.cs` of your `AppHost`, pass your custom implementation to the call to `AddWebAssemblyClient` via the `WebAssemblyProjectBuilderOptions` class, like this:
```
var builder = DistributedApplication.CreateBuilder(args);

var inventoryApi = builder.AddProject<Projects.AspNetCoreWebApi>("inventoryapi");

builder.AddProject<Projects.Blazor>("hostedblazor")
    .AddWebAssemblyClient<Projects.Blazor_Client>("wasmClient" options => {
        options.ServiceDiscoveryInfoSerializer = yourImplementation;
    })
    .WithReference(inventoryApi)

builder.Build().Run();
```
If you choose to make a custom implementation of `IServiceDiscoveryInfoSerializer`, you only need to override one method:
```
public void SerializeServiceDiscoveryInfo(IResourceWithServiceDiscovery resource) { }
```
Note: If you choose to override the default behaviour with an output format that Aspire can't read from your WebAssembly client app, you'll also need to override the discovery behaviour on the client, which is outside the scope of what I've developed here.

## Tips and Troubleshooting
I'll document here whenever I encounter a problem and (hopefully) how to overcome it.
1. I recommend extracting the names of your Aspire resources (e.g. "billingapi" and "inventoryapi") into a string constant in a project shared by the AppHost and all the other projects.
   That way, you can be sure the name is consistent throughout the whole solution.
2. Change `Properties\launchSettings.json` so that `launchBrowser` is `false` in all projects except your `AppHost`. You want your Aspire dashboard to launch when you run your app, not your Blazor apps.
   If your Blazor apps do launch directly, they'll be at a randomly assigned port in Kestrel and nothing will work.

<<<<<<< HEAD
=======
var webApi = builder.AddProject<Projects.InMyCountry_WebApi>("inventoryApi")
 .WithReference(blazorServer); // This will pass the endpoint URL of the Blazor app to the web API so that it can be added as a trusted origin in CORS.

blazorServer.AddWebAssemblyClient<Projects.InMyCountry_UI_Client>("blazorWasmClient") // Now we can add the Blazor WebAssembly (client) app in the Aspire4Wasm package.
    .WithReference(webApi); // And pass the Blazor client a reference to the web API

builder.Build().Run();
```
### The example above will add environment variables to the web API project, for example:
```
services__webclientapp__http__0 = http://localhost:56481
services__webclientapp__https__0 = http://localhost:56480

```
It should add as many clients as you configured in the AppHost.
### Example continued in Program.cs in the web API project
Now that the web API has a reference to the Blazor app in appsettings, we can configure CORS like this:
```
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var clients = GetAllowedOrigins(builder.Configuration, "blazorWasmClient"); // Get the clients from the environment variables. The second argument needs to be the resource name you passed when calling AddWebAssemblyClient in Program.cs of the AppHost project.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(clients); // Add the clients as allowed origins for cross origin resource sharing.
        policy.AllowAnyMethod();
        policy.WithHeaders("X-Requested-With");
        policy.AllowCredentials();
    });
});

private static string[] GetAllowedOrigins(ConfigurationManager config, string resourceName)
{
    var configSection = config.GetSection($"services:{resourceName}");
    var clients = new List<string>();
    foreach (var protocol in new[] { "http", "https" })
    {
        var subSection = configSection.GetSection(protocol);
        foreach (var child in subSection.GetChildren())
        {
            var value = child.Get<string>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                clients.Add(value);
            }
        }
    }

     return [.. clients];
}

// Etc.
```
## Troubleshooting
These are just a few things that I noticed helped me and I hope they help you too.
* You don't need a `launchsettings.json` in your webassembly client project. The one in your Blazor server project will do.
* In the `launchsettings.json` of your blazor server project, I recommend that you set `launchBrowser` to `false` for all profiles. This means that when the Aspire dashboard opens up, you'll need to click the link to open up your Blazor client. This is good! If you don't do this, your Blazor client is going to launch on a random port chosen by Aspire. When launched on a random port, your web API might reject the requests of your Blazor client because it doesn't have the expected origin to comply with the API's CORS policy. I tried to stop this happening but couldn't, so this is my workaround.
>>>>>>> 8198f83c41632ff38794d9644d019f3612f5a5b7
## Contributing
I'm a hobbyist. I know there are loads of people out there who be able to improve this in ways I can't, or see opportunities for improvement that I can't even imagine. If you want to contribute, bring it on! Send me a pull request.
