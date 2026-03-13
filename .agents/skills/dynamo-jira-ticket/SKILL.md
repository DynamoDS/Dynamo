---
name: dynamo-jira-ticket
description: Create structured Jira tickets for Dynamo from bug reports, failing tests, or feature requests. Use this skill whenever writing a Jira ticket, triaging a bug, turning a vague issue into an actionable ticket, or writing acceptance criteria. Also use when the user mentions "file a ticket", "write a bug report", or "create a Jira issue" for Dynamo.
---

# Dynamo Jira Ticket Writer

## When to use

- Creating a new Jira ticket from a bug report, failing test, or code investigation.
- Refining a vague issue description into a structured, actionable ticket.
- Writing acceptance criteria from a feature request or design discussion.

## When not to use

- PR descriptions -- use [dynamo-pr-description](../dynamo-pr-description/SKILL.md) instead.
- Architecture questions -- use [dynamo-onboarding](../dynamo-onboarding/SKILL.md) instead.

## Inputs expected

A bug description, error log, failing test output, feature request, or code investigation findings.

## Output format

A complete Jira ticket body ready to paste, following the canonical template in `template.md`.

---

## Workflow

1. **Gather evidence first.** Before writing, check the codebase for supporting details:
   - For node issues: check the node implementation in `src/Libraries/` or `src/DynamoCore/`.
   - For engine issues: check `src/Engine/ProtoCore/` and `src/Engine/ProtoScript/`.
   - For UI issues: check `src/DynamoCoreWpf/` and the relevant view extension.
   - For PublicAPI errors: check `PublicAPI.Unshipped.txt` / `PublicAPI.Shipped.txt` in the affected project.
   - For test failures: read the NUnit test output and the relevant test file.
2. **Write the ticket** using `template.md` in this folder.
3. **Review** -- ensure every section has concrete, verifiable content. No vague language.

## Writing Rules

- **Title**: short, concrete, behavior-focused. Bad: "Dynamo crash". Good: "DynamoSandbox crashes on startup when global.json specifies missing SDK version".
- **Prefer behavior over implementation**: describe what the user sees, not internal class names.
- **Include evidence**: paste error messages, stack traces, screenshots. Trim to relevant lines.
- **Separate facts from assumptions**: if you're guessing at root cause, say so explicitly.
- **Acceptance criteria must be testable**: each criterion should be verifiable by running a test or checking a specific behavior.

## Ticket Template

Use the canonical template at `template.md` in this folder.

Required sections in each generated ticket:
- Title
- Problem
- Expected Behavior
- Repro Steps
- Impact
- Acceptance Criteria
- Investigation Notes

---

**Example: Good ticket**

```markdown
# [DYN-5678] String.FromObject node returns null for custom Python class instances

## Problem

When a Python Script node returns a custom class instance, passing it to `String.FromObject` produces `null` instead of calling `__str__` or `ToString()`. This breaks downstream string operations.

## Expected Behavior

`String.FromObject` should call the object's `ToString()` method (or Python `__str__`) and return the string representation, consistent with how it handles built-in types.

## Repro Steps

1. Open Dynamo Sandbox
2. Create a Python Script node with: `class Foo:\n  def __str__(self): return "hello"\nOUT = Foo()`
3. Connect output to a `String.FromObject` node
4. Run the graph
5. Expected: `String.FromObject` outputs `"hello"`
6. Actual: `String.FromObject` outputs `null`

## Impact

- **Users affected**: Users with Python-heavy workflows using custom classes
- **Frequency**: Always (deterministic)
- **Severity**: Major (breaks string formatting workflows)

## Acceptance Criteria

- [ ] `String.FromObject` returns `__str__` result for Python objects with `__str__` defined
- [ ] `String.FromObject` returns `ToString()` result for .NET objects with custom `ToString()`
- [ ] NUnit test added in `test/Libraries/CoreNodesTests/StringTests.cs`
- [ ] No regression on built-in types (int, float, list, dict)

## Investigation Notes

- **Relevant files**: `src/Libraries/CoreNodes/String.cs`, `src/Engine/ProtoCore/`
- **Root cause hypothesis**: The engine may be wrapping the Python return value in a way that loses the original type's `ToString()` override
```
