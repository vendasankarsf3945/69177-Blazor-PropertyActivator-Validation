using Microsoft.AspNetCore.Components;
using PropertyActivatorHostedWasm.Client.PropertyActivatorValidation;

namespace PropertyActivatorHostedWasm.Client.Pages;

public class PropertyActivatorValidationBase : InheritedInjectionBase, IDisposable
{
    private const string PrerenderSnapshotStateKey = "PropertyActivatorValidation.PrerenderSnapshot";
    private PersistingComponentStateSubscription? _persistingSubscription;

    [Inject]
    public ActivationProbeService OrdinaryProbe { get; set; } = default!;

    [Inject(Key = ValidationConstants.KeyedServiceKey)]
    public ActivationProbeService KeyedProbe { get; set; } = default!;

    [Inject]
    public ValidationStateStore StateStore { get; set; } = default!;

    [Inject]
    public ActivationLogStore LogStore { get; set; } = default!;

    [Inject]
    public PersistentComponentState PersistentState { get; set; } = default!;

    [Inject]
    private ActivationProbeService NonPublicProbe { get; set; } = default!;

    protected ActivationProbeService NonPublicProbeDisplay => NonPublicProbe;

    protected ActivationSnapshot? PrerenderSnapshot { get; private set; }

    protected ActivationSnapshot? CurrentSnapshot { get; private set; }

    protected bool IsClientInteractive => OperatingSystem.IsBrowser();

    protected override void OnInitialized()
    {
        if (PersistentState.TryTakeFromJson<ActivationSnapshot>(PrerenderSnapshotStateKey, out var persistedSnapshot))
        {
            PrerenderSnapshot = persistedSnapshot;
        }

        CurrentSnapshot = BuildSnapshot(IsClientInteractive ? "client-interactive" : "server-prerender");
        Console.WriteLine($"[PropertyActivatorValidation] {CurrentSnapshot.ToLogLine()}");

        if (!IsClientInteractive)
        {
            PrerenderSnapshot = CurrentSnapshot;
            _persistingSubscription = PersistentState.RegisterOnPersisting(PersistPrerenderSnapshot);
        }
    }

    private Task PersistPrerenderSnapshot()
    {
        if (PrerenderSnapshot is not null)
        {
            PersistentState.PersistAsJson(PrerenderSnapshotStateKey, PrerenderSnapshot);
        }

        return Task.CompletedTask;
    }

    private ActivationSnapshot BuildSnapshot(string phase)
    {
        return new ActivationSnapshot(
            Phase: phase,
            ActivatorMode: DetectActivatorMode(OrdinaryProbe.Value),
            OrdinaryValue: OrdinaryProbe.Value,
            KeyedValue: KeyedProbe.Value,
            InheritedValue: InheritedProbe.Value,
            NonPublicValue: NonPublicProbeDisplay.Value);
    }

    private static string DetectActivatorMode(string value)
        => value.StartsWith("custom:", StringComparison.Ordinal) ? "custom" : "built-in";

    public void Dispose()
    {
        _persistingSubscription?.Dispose();
    }
}
