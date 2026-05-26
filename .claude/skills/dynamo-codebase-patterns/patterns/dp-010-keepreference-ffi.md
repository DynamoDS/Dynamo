---
id: "dp-010"
name: "[KeepReference] on FFI parameters whose lifetime must outlive the call"
status: "confirmed"
domain: "Engine/FFI"
canonical_file: "src/Libraries/GeometryColor/GeometryColor.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:Engine"]
---

## Intent
Apply `[KeepReference]` to any FFI method parameter whose value is stored as a field on the returned object, so the DesignScript GC does not dispose the parameter while the return object still holds a reference to it.

## Why non-obvious
In standard .NET, returning an object that holds a field reference to a parameter keeps the parameter alive automatically through the object graph. At the DesignScript FFI boundary this is not true: DesignScript tracks `StackValue` handles independently. The GC can mark a parameter's `StackValue` for disposal even if a C# field still references the underlying object, because the GC only sees the DesignScript heap. Without `[KeepReference]`, the returned object holds a dangling C# reference to a disposed geometry wrapper — calls on that geometry produce undefined behavior or crashes, often non-deterministically.

## Correct form
```csharp
public static GeometryColor ByGeometryColor(
    [KeepReference] Geometry geometry, // geometry is stored as a field — must keep reference
    Color color)
{
    if (geometry == null) throw new ArgumentNullException("geometry");
    return new GeometryColor(geometry, color);
}
```

The marshaler detects `[KeepReference]`, adds the parameter's `StackValue` to `referencedParameters`, then expands the return object's heap allocation to embed the reference as a hidden slot — ensuring the GC sees the dependency.

## Anti-pattern
```csharp
// Wrong: geometry will be independently disposed by DesignScript GC
public static GeometryColor ByGeometryColor(Geometry geometry, Color color)
{
    return new GeometryColor(geometry, color); // dangling reference after GC
}
```

## When it applies
Any zero-touch FFI method that stores a parameter in a field of the returned object. Also combine with `[KeepReference]` on `IList<object>` parameters when the list values are stored (see dp-011).

## Related patterns
- dp-011
