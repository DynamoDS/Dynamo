# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Project

Dynamo is a visual programming tool accessible to non-programmers and programmers alike. Users can visually script behavior, define custom logic, and script using textual programming languages. Built with C# and WPF, targeting .NET 10. The core engine (`DynamoCore.sln`) is cross-platform; the full UI (`Dynamo.All.sln`) is Windows-only.

- **Language**: C# (.NET 10)
- **UI**: WPF (Windows only)
- **Tests**: NUnit
- **Build (Windows full)**: `msbuild src/Dynamo.All.sln /p:Configuration=Release`
- **Build (core only)**: `dotnet build src/DynamoCore.sln -c Release`
- **Test**: `dotnet test` or Visual Studio Test Explorer

## Key Conventions

- Follow [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards) and [Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards).
- XML documentation required on all public methods and properties.
- New public APIs must be added to `PublicAPI.Unshipped.txt` (format: `namespace.ClassName.MemberName -> ReturnType`).
- Security analyzers CA2327/CA2329/CA2330/CA2328 are errors.
- NUnit for all tests. Do not introduce xUnit or MSTest.
- User-facing strings in `.resx` files.

## Detailed Guidance

Read `.agents/` for comprehensive skills, rules, and templates:

- **Skills** (task workflows): `.agents/skills/<skill-name>/SKILL.md`
  - `dynamo-dotnet-expert` -- C#/.NET patterns, testing, PublicAPI
  - `dynamo-onboarding` -- Dynamo architecture, ecosystem, debugging
  - `dynamo-pr-description` -- PR descriptions matching Dynamo template (uses `.github/PULL_REQUEST_TEMPLATE.md`)
  - `dynamo-jira-ticket` -- creating/refining Jira tickets (includes template.md)
- **Rules** (constraints): `.agents/rules/`
  - `dynamo-core-rules.md` -- .NET/Dynamo constraints

## Important Links

- [Dynamo Wiki](https://github.com/DynamoDS/Dynamo/wiki)
- [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes)
- [Developer Resources](https://developer.dynamobim.org/)
