using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PropertyActivator_StandaloneWasm.PropertyActivatorValidation;
using PropertyActivator_StandaloneWasm;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(_ => new ActivationProbeService("di-default", "di:ordinary"));
builder.Services.AddKeyedScoped<ActivationProbeService>(
	ValidationConstants.KeyedServiceKey,
	static (_, key) => new ActivationProbeService("di-default", "di:keyed", key?.ToString()));
builder.Services.AddScoped<ValidationStateStore>();
builder.Services.AddScoped<ActivationLogStore>();
#if PROPERTY_ACTIVATOR_CUSTOM_ENABLED
builder.Services.AddScoped<IComponentPropertyActivator, ValidationComponentPropertyActivator>();
#endif

await builder.Build().RunAsync();
