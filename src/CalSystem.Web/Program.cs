using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CalSystem.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CalSystem.Web.App>("#app");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("http://localhost:5112") });

builder.Services.AddScoped<OrderApiService>();

await builder.Build().RunAsync();
