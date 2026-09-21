using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace PropertyActivatorHostedWasm.Client.PropertyActivatorValidation;

public static class ValidationConstants
{
    public const string KeyedServiceKey = "validation-key";
}

public sealed record ActivationProbeService(string Source, string Value, string? Key = null)
{
    public Guid InstanceId { get; } = Guid.NewGuid();
}

public sealed class ValidationStateStore
{
    public int Count { get; private set; }

    public void Increment() => Count++;
}

public sealed record ActivationLogEntry(
    string Activator,
    string Component,
    string Property,
    string? Key,
    string Value);

public sealed record ActivationSnapshot(
    string Phase,
    string ActivatorMode,
    string OrdinaryValue,
    string KeyedValue,
    string InheritedValue,
    string NonPublicValue)
{
    public string ToLogLine()
    {
        return $"phase={Phase};activator={ActivatorMode};ordinary={OrdinaryValue};keyed={KeyedValue};inherited={InheritedValue};nonPublic={NonPublicValue}";
    }
}

public sealed class ActivationLogStore
{
    private readonly List<ActivationLogEntry> _entries = [];

    public IReadOnlyList<ActivationLogEntry> Entries
    {
        get
        {
            lock (_entries)
            {
                return _entries.ToArray();
            }
        }
    }

    public void Add(ActivationLogEntry entry)
    {
        lock (_entries)
        {
            _entries.Add(entry);
        }
    }
}

public abstract class InheritedInjectionBase : ComponentBase
{
    [Inject]
    protected ActivationProbeService InheritedProbe { get; set; } = default!;
}

public sealed class ValidationComponentPropertyActivator(ActivationLogStore logStore) : IComponentPropertyActivator
{
    private readonly ConcurrentDictionary<Type, Action<IServiceProvider, IComponent>> _cache = new();

    public Action<IServiceProvider, IComponent> GetActivator(Type componentType)
        => _cache.GetOrAdd(componentType, BuildActivator);

    private Action<IServiceProvider, IComponent> BuildActivator(Type componentType)
    {
        var injectableProperties = GetInjectableProperties(componentType);

        return (serviceProvider, component) =>
        {
            if (component.GetType() == typeof(Pages.ActivationFailureProbe))
            {
                throw new InvalidOperationException("Deliberate activation failure for validation.");
            }

            foreach (var (property, injectAttribute) in injectableProperties)
            {
                var key = injectAttribute.Key;
                object? value = key is null
                    ? serviceProvider.GetService(property.PropertyType)
                    : serviceProvider.GetKeyedService(property.PropertyType, key);

                if (value is null)
                {
                    throw new InvalidOperationException(
                        $"Unable to resolve service for property '{property.Name}' on component '{componentType.Name}'.");
                }

                if (value is ActivationProbeService probe)
                {
                    value = new ActivationProbeService(
                        Source: "custom-activator",
                        Value: $"custom:{componentType.Name}:{property.Name}",
                        Key: key?.ToString() ?? probe.Key);
                }

                property.SetValue(component, value);

                logStore.Add(new ActivationLogEntry(
                    Activator: "custom",
                    Component: componentType.Name,
                    Property: property.Name,
                    Key: key?.ToString(),
                    Value: value is ActivationProbeService injectedProbe ? injectedProbe.Value : value.GetType().Name));
            }

            if (injectableProperties.Length == 0)
            {
                logStore.Add(new ActivationLogEntry(
                    Activator: "custom",
                    Component: componentType.Name,
                    Property: "(none)",
                    Key: null,
                    Value: "no [Inject] properties"));
            }
        };
    }

    private static (PropertyInfo Property, InjectAttribute InjectAttribute)[] GetInjectableProperties(Type componentType)
    {
        var properties = new List<(PropertyInfo, InjectAttribute)>();
        var current = componentType;

        while (current is not null && current != typeof(object))
        {
            var currentProperties = current.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var property in currentProperties)
            {
                if (property.GetIndexParameters().Length > 0 || property.SetMethod is null)
                {
                    continue;
                }

                var injectAttribute = property.GetCustomAttribute<InjectAttribute>(inherit: false);
                if (injectAttribute is not null)
                {
                    properties.Add((property, injectAttribute));
                }
            }

            current = current.BaseType;
        }

        return [.. properties];
    }
}
