using PropertyActivatorHostedWasm.Client.Pages;
using PropertyActivatorHostedWasm.Client.PropertyActivatorValidation;
using PropertyActivatorHostedWasm.Components;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddScoped(_ => new ActivationProbeService("di-default", "di:ordinary"));
builder.Services.AddKeyedScoped<ActivationProbeService>(
    ValidationConstants.KeyedServiceKey,
    static (_, key) => new ActivationProbeService("di-default", "di:keyed", key?.ToString()));
builder.Services.AddScoped<ValidationStateStore>();
builder.Services.AddScoped<ActivationLogStore>();
#if PROPERTY_ACTIVATOR_CUSTOM_ENABLED
builder.Services.AddScoped<IComponentPropertyActivator, ValidationComponentPropertyActivator>();
#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PropertyActivatorHostedWasm.Client._Imports).Assembly);

app.Run();
