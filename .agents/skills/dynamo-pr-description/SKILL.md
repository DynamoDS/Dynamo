---
name: dynamo-pr-description
description: Generate PR descriptions for Dynamo that align with the team template section names and order. Use this skill whenever writing a pull request description, cleaning up a PR body, or generating a review-ready summary from a diff in the Dynamo repo. Also use when the user says "write a PR", "PR description", or "prep this for review."
---

# Dynamo PR Description

## When to use

- Writing a PR description from a diff in the Dynamo repo.
- Cleaning up or reformatting an existing PR body to match the team template.
- Producing review-ready summaries quickly.

## When not to use

- PRs targeting the DynamoMCP repo -- that repo has its own PR description skill.
- Jira ticket triage -- use [template](../dynamo-jira-ticket/template.md) instead.

## Inputs expected

A git diff, commit log, or description of the changes. Optionally a Jira key.

## Output format

A complete PR body matching the Dynamo template in `.github/PULL_REQUEST_TEMPLATE.md`, ready to paste.

---

## Workflow

1. Read the diff (staged changes, commit history, or user-provided summary).
2. Identify the *why* -- what problem does this solve?
3. Fill `.github/PULL_REQUEST_TEMPLATE.md` from the diff and context.
4. For each declaration checkbox, only check it if you've verified it's true.
5. Write the release note from the user's perspective (one sentence, or `N/A`).
6. Leave `(FILL ME IN)` placeholders for anything you can't determine from the diff.

## Rules

- Mirror section names and heading order from the template exactly.
- Keep facts verifiable from the diff. Do not invent Jira keys, reviewers, or test results.
- Call out breaking changes or migration steps explicitly in Purpose.
- If the PR changes public API, mention the affected types and whether `PublicAPI.Unshipped.txt` was updated.
- If the PR adds new nodes, mention the node names and whether help files were included.
- The API versioning declaration is important -- check it only if changes follow [Semantic Versioning](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Versions).
- Release Notes is a **mandatory** section -- always include it, even if just `N/A`.
- If the user provides explicit checklist bullets or section content, treat those as source of truth and override the defaults below.

## PR Title

Format: `DYN-1234: concise change summary` (include Jira key when known).

## Template

Read `.github/PULL_REQUEST_TEMPLATE.md` for the exact template structure. Follow its sections in order.

Content guidance within those sections:
- In Purpose, include a concise **"Key changes:"** bullet list when it helps readability.
- For Release Notes, write one concise sentence from the user's perspective, or `N/A` when not user-facing.

---

**Example: PR adding a new node**

```markdown
### Purpose

DYN-5678: Add `String.Interpolate` node for string formatting with placeholders.

Key changes:
- New `StringInterpolate` method in `src/Libraries/CoreNodes/String.cs`
- Added to `PublicAPI.Unshipped.txt`
- NUnit tests in `test/Libraries/CoreNodesTests/StringTests.cs`
- Help files: `.dyn`, `.md`, `.jpg` in `doc/distrib/NodeHelpFiles/`

### Declarations

- [x] Is documented according to the standards
- [x] The level of testing this PR includes is appropriate
- [x] Changes to the API follow Semantic Versioning and are documented in the API Changes document.

### Release Notes

Added String.Interpolate node for formatting strings with named placeholders.

### Reviewers

(FILL ME IN) Reviewer 1

### FYIs

(FILL ME IN, Optional)
```
