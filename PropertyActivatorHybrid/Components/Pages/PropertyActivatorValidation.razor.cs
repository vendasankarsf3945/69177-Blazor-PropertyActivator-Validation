using Microsoft.AspNetCore.Components;
using PropertyActivatorHybrid.Components.PropertyActivatorValidation;

namespace PropertyActivatorHybrid.Components.Pages;

public class PropertyActivatorValidationBase : InheritedInjectionBase
{
    [Inject]
    public ActivationProbeService OrdinaryProbe { get; set; } = default!;

    [Inject(Key = ValidationConstants.KeyedServiceKey)]
    public ActivationProbeService KeyedProbe { get; set; } = default!;

    [Inject]
    public ValidationStateStore StateStore { get; set; } = default!;

    [Inject]
    public ActivationLogStore LogStore { get; set; } = default!;

    [Inject]
    private ActivationProbeService NonPublicProbe { get; set; } = default!;

    protected ActivationProbeService NonPublicProbeDisplay => NonPublicProbe;
}
