# PublicAPI Management Guide

Dynamo tracks its public API surface using Roslyn analyzers (RS0016, RS0017).

## Adding a new public member

1. Add the member to the source file.
2. Add an entry to `PublicAPI.Unshipped.txt` in the relevant project.
   - Format: `namespace.ClassName.MemberName -> ReturnType`
   - Example: `Dynamo.Core.DynamoModel.GetWorkspaceName(string) -> string?`
3. On release, entries move from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`.

## Which project's file to update

| Project | PublicAPI file location |
|---|---|
| DynamoCore | `src/DynamoCore/` |
| DynamoUtilities | `src/DynamoUtilities/` |
| DynamoCoreWpf | `src/DynamoCoreWpf/` |
| NodeServices | `src/NodeServices/` |

## Visibility defaults

Default to minimal exposure: `private` > `internal` > `protected` > `public`.

Adding a public member means committing to backward compatibility. When in doubt, start `internal` and promote later.

## Breaking changes

Removed/renamed public members, or changed signatures, require:
1. File a GitHub issue first.
2. Update the [API Changes wiki](https://github.com/DynamoDS/Dynamo/wiki/API-Changes).
3. Follow [Semantic Versioning](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Versions).

## Extend-only design

Once a public API is shipped, treat it as immutable:
- Add new overloads, new types, or opt-in features.
- Do not modify or remove existing signatures.
- Removal is only allowed after a deprecation period.
