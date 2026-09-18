using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components;
//using PropertyActivatorHostedWasm.Client.PropertyActivatorValidation;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.Services.AddScoped(_ => new ActivationProbeService("di-default", "di:ordinary"));
//builder.Services.AddKeyedScoped<ActivationProbeService>(
//    ValidationConstants.KeyedServiceKey,
//    static (_, key) => new ActivationProbeService("di-default", "di:keyed", key?.ToString()));
//builder.Services.AddScoped<ValidationStateStore>();
//builder.Services.AddScoped<ActivationLogStore>();
//builder.Services.AddScoped<IComponentPropertyActivator, ValidationComponentPropertyActivator>();

await builder.Build().RunAsync();
