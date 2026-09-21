# Blazor Property Activator Validation

This repository validates custom `IComponentPropertyActivator` behavior across several Blazor hosting models:

- Static SSR
- Interactive Server
- Standalone WebAssembly
- Hosted Interactive WebAssembly
- .NET MAUI Windows

## Repository layout

- [PropertyActivatorServer](./PropertyActivatorServer)
- [PropertyActivatorSSR](./PropertyActivatorSSR)
- [PropertyActivator-StandaloneWasm](./PropertyActivator-StandaloneWasm)
- [PropertyActivatorHostedWasm](./PropertyActivatorHostedWasm)
- [PropertyActivatorHybrid](./PropertyActivatorHybrid)
- [Evidence](./Evidence)

## Hosted WebAssembly variant selection

The hosted WebAssembly sample uses the MSBuild property `PropertyActivatorVariant` to control which side registers the custom activator.
The default configuration is set in [PropertyActivatorHostedWasm/Directory.Build.props](./PropertyActivatorHostedWasm/Directory.Build.props) and currently defaults to `ClientOnly`.

The relevant startup files are:

- [PropertyActivatorHostedWasm/PropertyActivatorHostedWasm/Program.cs](./PropertyActivatorHostedWasm/PropertyActivatorHostedWasm/Program.cs)
- [PropertyActivatorHostedWasm/PropertyActivatorHostedWasm.Client/Program.cs](./PropertyActivatorHostedWasm/PropertyActivatorHostedWasm.Client/Program.cs)

### Variant mapping

| Variant | `PropertyActivatorVariant` | Server custom activator | Client custom activator |
|---|---|---|---|
| Full | `Both` | Yes | Yes |
| Server-only | `ServerOnly` | Yes | No |
| Client-only | `ClientOnly` | No | Yes |
| None | `None` | No | No |

The hosted project uses conditional MSBuild logic to interpret `PropertyActivatorVariant` and expose the corresponding server/client behavior. In practical terms:

- `Both` enables custom activator behavior on both sides
- `ServerOnly` keeps the server custom activator on and the client off
- `ClientOnly` keeps the client custom activator on and the server off
- `None` disables the custom activator on both sides

### Config file

`PropertyActivatorHostedWasm/Directory.Build.props`

```xml
<PropertyGroup>
  <PropertyActivatorVariant Condition="'$(PropertyActivatorVariant)' == ''">ClientOnly</PropertyActivatorVariant>
</PropertyGroup>
```

You can override this default per command using `-p:PropertyActivatorVariant=<Both|ServerOnly|ClientOnly|None>`.

## Running the hosted WebAssembly variants

Run these commands from the repository root.

If you want client-only behavior, either use the explicit `ClientOnly` command below or run without `-p:PropertyActivatorVariant` (because the default is `ClientOnly`).

### Full

```powershell
dotnet run --project .\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm.csproj -p:PropertyActivatorVariant=Both
```

### Server-only

```powershell
dotnet run --project .\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm.csproj -p:PropertyActivatorVariant=ServerOnly
```

### Client-only

```powershell
dotnet run --project .\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm.csproj -p:PropertyActivatorVariant=ClientOnly
```

### None

```powershell
dotnet run --project .\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm\PropertyActivatorHostedWasm.csproj -p:PropertyActivatorVariant=None
```

## Standalone WebAssembly

The standalone WebAssembly app registers the custom activator directly in:

- [PropertyActivator-StandaloneWasm/Program.cs](./PropertyActivator-StandaloneWasm/Program.cs)

Run it with:

```powershell
dotnet run --project .\PropertyActivator-StandaloneWasm\PropertyActivator-StandaloneWasm.csproj
```

## Evidence

Published and screenshot evidence is stored under [Evidence](./Evidence).