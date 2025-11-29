using BlazorApp;
using BlazorApp.Interfaces;
using BlazorApp.Providers;
using BlazorApp.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection; // <-- Add this using directive
using Microsoft.Extensions.Http; // <-- Optional, but sometimes needed for HttpClient extensions

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// --- BEST PRACTICE: Load appsettings.json into configuration ---
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// *** BEST PRACTICE: Register the ApiClient as a "Typed Client" ***
// The DI container will manage both the ApiClient and the HttpClient it uses.
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    // Read the BaseAddress from configuration and set it here.
    var baseAddress = builder.Configuration["BaseAddress"];
    if (!string.IsNullOrEmpty(baseAddress))
    {
        client.BaseAddress = new Uri(baseAddress);
    }
    else
    {
        // Fallback for safety
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    }
});


//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


builder.Services.AddScoped<ITokenProvider, LocalStorageTokenProvider>();
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddBlazoredLocalStorage();


await builder.Build().RunAsync();
