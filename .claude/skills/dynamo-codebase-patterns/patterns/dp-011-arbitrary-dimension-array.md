---
id: "dp-011"
name: "[ArbitraryDimensionArrayImport] to preserve nested array structure across FFI"
status: "confirmed"
domain: "Engine/FFI"
canonical_file: "src/Libraries/DesignScriptBuiltin/Dictionary.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:Engine"]
---

## Intent
Apply `[ArbitraryDimensionArrayImport]` to FFI parameters or return values that should be typed as `var[]..[]` in the DS VM, preventing the VM from attempting automatic replication (lacing) over them.

## Why non-obvious
Without this attribute, a collection parameter (`IList<object>`, `IEnumerable<object>`) is imported into the DS VM as a fixed-rank array. The VM will then try to auto-replicate over its elements when the function is called — iterating over the list and invoking the function once per element rather than passing the whole list as a single argument. This is the correct behaviour for most node inputs, but wrong for parameters that are meant to receive the entire collection as one value (e.g. a dictionary's values list, or a list being passed to a function that operates on the whole list at once). Adding the attribute tells the VM to type the parameter as `var[]..[]` (arbitrary rank), so it is always passed wholesale without replication.

## Correct form
```csharp
// Parameter: whole list passed to C# as-is, VM does not replicate over it
public static Dictionary ByKeysValues(
    IList<string> keys,
    [KeepReference][ArbitraryDimensionArrayImport] IList<object> values)
{ ... }

// Return: VM treats return value as var[]..[] regardless of actual structure
public IEnumerable<object> Values
{
    [return: ArbitraryDimensionArrayImport]
    get => D.Values;
}
```

## Anti-pattern
```csharp
// Wrong: VM will replicate over 'values', calling ByKeysValues once per element
public static Dictionary ByKeysValues(IList<string> keys, IList<object> values) { ... }
```

## When it applies
FFI parameters or return values where you need the DS VM to treat the collection as a single `var[]..[]` value rather than replicating over it. Typical cases: parameters that receive an entire list as one argument (dictionary values, sort comparators), and return values that themselves contain lists. When also storing the parameter as a field on the return object, combine with `[KeepReference]` (see dp-010).

## Related patterns
- dp-010
