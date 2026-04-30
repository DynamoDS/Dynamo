---
name: dynamo-dotnet-expert
description: Write and review C#/.NET code in Dynamo following Dynamo coding standards, modern C# patterns, and repo conventions. Use this skill whenever writing C# code, reviewing a PR diff, designing types, managing PublicAPI surface files, choosing patterns, making performance decisions, or refactoring in the Dynamo codebase. Also use when asking about NUnit testing, async patterns, error handling, immutability, or security in Dynamo.
---

# Dynamo dotNet Expert

## When to use

- Writing or reviewing C#/.NET code in Dynamo.
- Designing public APIs, choosing types, or making performance decisions.
- Managing PublicAPI surface files.
- Refactoring for modern C# patterns.
- WPF/UI work in `DynamoCoreWpf`.

## When not to use

- Writing NUnit tests -- use [dynamo-unit-testing](../dynamo-unit-testing/SKILL.md) instead.
- Architecture / onboarding questions -- use [dynamo-onboarding](../dynamo-onboarding/SKILL.md) instead.
- PR descriptions -- use [dynamo-pr-description](../dynamo-pr-description/SKILL.md) instead.

## Inputs expected

A C# file or diff to review, a feature requirement, or a question about .NET patterns in the Dynamo codebase.

## Output format

Code suggestions with explanations, or a review with specific file/line references.

---

## Workflow

1. **Read before suggesting** — read the relevant file(s) before proposing changes.
2. **Repo conventions win** — when repo standards conflict with generic .NET advice, follow the repo.
3. **Check public API impact** — any new public member needs a `PublicAPI.Unshipped.txt` entry.
4. **Run quality checks** — before finishing, verify against the [quality checklist](./references/quality-checklist.md).

---

## Quick Decision Guide

| Situation | Action |
|---|---|
| Adding a public member | Update `PublicAPI.Unshipped.txt` — see [publicapi-guide.md](./assets/publicapi-guide.md) |
| DTO / immutable data | Use `record` |
| Small value type (< 16 bytes) | Use `readonly record struct` |
| Class not designed for inheritance | Mark `sealed` |
| Async in library code | Add `ConfigureAwait(false)` |
| User-facing string | Put it in a `.resx` file |
| New node | Add `.dyn`, `.md`, `.jpg` to `doc/distrib/NodeHelpFiles/` |
| Breaking public API change | File a GitHub issue first |

---

## Project Conventions

- [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [Dynamo Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards)
- `.editorconfig` in the repo root defines indent and encoding — check it before editing.
- Security analyzers CA2327/CA2329/CA2330/CA2328 are treated as **errors**.

### Naming

- PascalCase for public members, classes, methods.
- camelCase for private fields and local variables.
- Prefix private fields with underscore only if the file already does so consistently.
- XML documentation required on all public methods and properties.

### Comments

Comments explain *why*, not what.

---

## Tech Stack

| | Dynamo |
|---|--------|
| TFM | net10.0 (full UI: win-x64 only) |
| Test framework | NUnit |
| Build (full) | `msbuild src/Dynamo.All.sln` |
| Build (core) | `dotnet build src/DynamoCore.sln` |
| IDE | Visual Studio 2022 |

---

## Assets & References

- **[csharp-patterns.md](./assets/csharp-patterns.md)** — Modern C#, type design, async, immutability, performance, WPF/UI, examples
- **[publicapi-guide.md](./assets/publicapi-guide.md)** — PublicAPI.Unshipped.txt workflow, breaking changes, extend-only design
- **[quality-checklist.md](./references/quality-checklist.md)** — Anti-patterns, security rules, file constraints, PR checklist

---

**Related Skills:**
[dynamo-unit-testing](../dynamo-unit-testing/SKILL.md) • [dynamo-onboarding](../dynamo-onboarding/SKILL.md) • [dynamo-pr-description](../dynamo-pr-description/SKILL.md)
