# Dynamo Core Rules

Shared constraints for all C#/.NET work in the Dynamo repository.

## Coding Standards

- Follow [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards) and [Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards).
- Respect `.editorconfig`: 4-space indentation, LF line endings, UTF-8, trim trailing whitespace.
- XML documentation required on all public methods and properties.

## Testing

- **NUnit** is the test framework for C# tests. Do not introduce xUnit or MSTest.
- NUnit packages: `NUnit`, `NUnit3TestAdapter`, `NUnit.Analyzers`.
- Test naming: `WhenConditionThenExpectedBehavior`.
- One behavior per test. Arrange-Act-Assert pattern.

## Public API Surface

- New public types, methods, or properties must be added to `PublicAPI.Unshipped.txt`.
- Format: `namespace.ClassName.MemberName -> ReturnType`
- Do not remove or rename existing public API members without filing an issue and following [Semantic Versioning](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Versions).
- Document API changes in the [API Changes wiki](https://github.com/DynamoDS/Dynamo/wiki/API-Changes).

## Security

- Security analyzers CA2327, CA2328, CA2329, CA2330 are treated as **errors**.
- Never commit secrets, API keys, or credentials to source code.
- No new network connections without documentation and no-network mode testing.
- No data collection without user consent checks.
- User-facing strings must go in `.resx` files for localization.

## Commits and PRs

- Commit message: short summary (50 chars max), blank line, detailed body (72 char wrap). Optionally reference Jira: `DYN-1234`.
- PR title must include Jira ticket: `DYN-1234: concise summary`.
- Fill all sections in the PR template. Use `N/A` for Release Notes when not user-facing.
- No files larger than 50 MB.

## Quality (Slopwatch checks)

- No disabled tests (`[Ignore]`, `[Explicit]`) without a comment explaining why.
- No empty catch blocks (`catch { }`) -- log and rethrow or handle explicitly.
- No `#pragma warning disable` without a justification comment.
- No weakened assertions (e.g., replacing `Assert.AreEqual` with `Assert.IsNotNull`).
