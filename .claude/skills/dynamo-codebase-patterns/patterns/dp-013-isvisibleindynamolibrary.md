---
id: "dp-013"
name: "[IsVisibleInDynamoLibrary(false)] to hide public members from the library browser"
status: "confirmed"
domain: "Libraries"
canonical_file: "src/Libraries/CoreNodes/List.cs"
added: "2026-03-03"
last_verified: "2026-03-03"
sightings: 3
seen_in: ["scan:Libraries"]
---

## Intent
Use `[IsVisibleInDynamoLibrary(false)]` to exclude public C# methods or classes from appearing in the Dynamo library browser, without making them internal to the assembly.

## Why non-obvious
Standard .NET visibility (`public` / `internal`) is the only access control a developer expects. In Dynamo, the library browser surfaces all `public` members of loaded assemblies as user-facing nodes — there is no middle ground in C# visibility between "accessible to C# code" and "visible to users." `[IsVisibleInDynamoLibrary(false)]` provides that middle ground. Without it, internal helper methods, backward-compatibility shims, and overloads not intended for end users all appear as nodes in the library, polluting the user-facing API surface.

## Correct form
```csharp
[IsVisibleInDynamoLibrary(false)] // hidden from library browser, still callable from C#
public static bool Contains(IList list, [ArbitraryDimensionArrayImport] object item)
{ ... }

// Can also be applied at class level to hide the entire class
[IsVisibleInDynamoLibrary(false)]
public static class InternalHelpers { ... }
```

## Anti-pattern
```csharp
// Wrong: public method appears as a node in the library browser
public static bool Contains(IList list, object item) // visible to all users
{ ... }
```

## When it applies
Any `public` method in a zero-touch library assembly that should be accessible from C# (e.g. for use by other nodes or core code) but is not intended as a user-facing node. Common cases: backward-compatibility overloads, helper methods called by other public methods, methods with parameter types that don't translate cleanly to the node UI.

## Related patterns
- dp-010
- dp-011
