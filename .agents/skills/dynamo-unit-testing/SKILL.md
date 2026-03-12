---
name: dynamo-unit-testing
description: Write comprehensive NUnit tests for the Dynamo codebase following Dynamo testing patterns, conventions, and architectural constraints. Use this skill when writing test methods, test classes, setting up test infrastructure, mocking dependencies, testing with .dyn files, or reviewing test code in Dynamo.
---

# Dynamo Unit Testing Expert

## When to use
- Writing new test methods or test classes for Dynamo components
- Setting up test infrastructure (base classes, setup/teardown)
- Testing nodes, workspaces, or UI components
- Working with .dyn test files or sample graphs
- Reviewing existing tests for quality and patterns
- Debugging test failures or flaky tests

## When not to use
- General coding questions -- use [dynamo-dotnet-expert](../dynamo-dotnet-expert/SKILL.md) instead
- Architecture questions -- use [dynamo-onboarding](../dynamo-onboarding/SKILL.md) instead
- PR descriptions -- use [dynamo-pr-description](../dynamo-pr-description/SKILL.md) instead

## Inputs expected
Test requirements, failing functionality to test, existing code to cover, or test code to review.

## Output format
Complete test classes with proper setup, test methods using Dynamo patterns, and explanations of testing approach.

---

## Workflow

> 🚀 **Quick Start**: [test-patterns.md](./assets/test-patterns.md) has ready-to-use templates

### 1. Choose Your Base Class
Select the appropriate test base class based on what you're testing:
- **UnitTestBase** - Utilities, algorithms, no Dynamo model needed
- **DynamoModelTestBase** - Nodes, workspaces, commands (most common)
- **DSEvaluationUnitTestBase** - DesignScript expression evaluation

### 2. Apply Testing Standards
Follow NUnit conventions and Dynamo patterns. See [quality-checklist.md](./references/quality-checklist.md) for guidelines and anti-patterns and Dynamo coding standards for general coding practices.
- [Dynamo Coding Standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [Dynamo Naming Standards](https://github.com/DynamoDS/Dynamo/wiki/Naming-Standards)

### 3. Write Focused Tests
One behavior per test, descriptive names, Arrange-Act-Assert structure.

## Testing Principles

- **NUnit only** - Use NUnit framework exclusively (never xUnit/MSTest)
- **One behavior per test** - Each test validates exactly one scenario
- **Pragmatic .dyn testing** - .dyn file tests allowed in unit categories for focused functionality

## Documentation

**Assets & References:**
- **[test-patterns.md](./assets/test-patterns.md)** - Base classes, code templates, common scenarios, complete examples
- **[quality-checklist.md](./references/quality-checklist.md)** - Quality guidelines, anti-patterns, best practices

**Related Skills:**
[dynamo-dotnet-expert](../dynamo-dotnet-expert/SKILL.md) • [dynamo-onboarding](../dynamo-onboarding/SKILL.md)
