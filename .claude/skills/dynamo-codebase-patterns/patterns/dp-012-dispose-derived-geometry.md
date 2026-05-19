---
id: "dp-012"
name: "Dispose derived geometry objects immediately after use"
status: "confirmed"
domain: "Engine/Geometry"
canonical_file: "src/Libraries/GeometryColor/GeometryColor.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:Engine"]
---

## Intent
Explicitly dispose derived geometry objects (`PerimeterCurves()`, `Edges`, `CurveGeometry`, etc.) immediately after use rather than relying on LibG's deferred cleanup mechanism.

## Why non-obvious
LibG suppresses the SWIG finalizers on geometry objects and replaces them with a deferred disposal system: when a geometry object is finalized by the CLR, it is added to a per-thread pending list rather than immediately releasing its native resource. Native resources are then released either when the next ProtoGeometry object is created on that thread (triggering a flush of the pending list) or at post-evaluation via a `postEvaluation` handler on `HostFactory`.

This means skipping `Dispose()` does not cause an immediate crash or permanent leak — LibG will eventually clean up. However, the timing is non-deterministic: within a single graph evaluation, transient geometry objects accumulate in the pending list and hold native ASM resources until the next creation flush or end-of-evaluation cleanup. In tight tessellation loops over complex geometry this accumulation can be significant. The `postEvaluation` cleanup also has known uncertainty in sandbox mode (the thread on which it runs may differ from the object-creating thread, leaving some objects uncleared).

Explicit `Dispose()` releases the native resource immediately and deterministically, avoiding all of the above.

## Correct form
```csharp
foreach (var curve in surf.PerimeterCurves())
{
    curve.Tessellate(package, parameters);
    curve.Dispose(); // release immediately, don't wait for deferred cleanup
}

foreach (var geom in solid.Edges.Select(edge => edge.CurveGeometry))
{
    geom.Tessellate(package, parameters);
    geom.Dispose(); // release immediately
}
```

## Anti-pattern
```csharp
// Not a crash, but native resources accumulate in LibG's pending list
// until the next object creation or end-of-evaluation flush
foreach (var curve in surf.PerimeterCurves())
{
    curve.Tessellate(package, parameters);
    // no Dispose() — resource held until deferred cleanup
}
```

## When it applies
Any code that calls APIs returning transient derived geometry: `PerimeterCurves()`, `Edges`, `CurveGeometry`, `Faces`, `Vertices`, and similar enumeration methods on `Surface`, `Solid`, and `Curve` types. Does not apply to geometry objects returned to the graph as node outputs — those are managed by the DesignScript GC and LibG's deferred system is the intended mechanism for them.

## Related patterns
- dp-010
