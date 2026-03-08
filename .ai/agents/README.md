# Dynamo Agent Guidance

Shared skills, rules, and templates for AI-assisted development in the Dynamo repo. These files are the **canonical source of truth** -- tool-specific mirrors (`.cursor/rules/`, `.github/copilot-instructions.md`, `CLAUDE.md`) point back here.

## Quick Reference

### Skills (task workflows that produce outputs)

Each skill lives in its own folder with a `SKILL.md` and optionally a `template.md`.

| Skill | When to use | Repo scope |
|-------|-------------|------------|
| [dynamo-dotnet-expert](skills/dynamo-dotnet-expert/SKILL.md) | Writing or reviewing C#/.NET code. Code design, testing, performance, PublicAPI management. | Repo-specific variant |
| [dynamo-onboarding](skills/dynamo-onboarding/SKILL.md) | Learning the Dynamo codebase, architecture briefings, finding where to start on a Jira ticket. | Repo-specific variant |
| [dynamo-pr-description](skills/dynamo-pr-description/SKILL.md) | Writing PR descriptions matching the Dynamo template. | Repo-specific variant |
| [dynamo-jira-ticket](skills/dynamo-jira-ticket/SKILL.md) | Creating or refining Jira tickets from bugs, test failures, or feature requests. | Repo-specific variant |

### Templates (bundled with skills)

Templates are co-located inside the skill folder that uses them:

| Template | Location | Purpose |
|----------|----------|---------|
| PR description | [dynamo-pr-description/template.md](skills/dynamo-pr-description/template.md) | Dynamo PR template for quick copy/paste |
| Jira triage | [dynamo-jira-ticket/template.md](skills/dynamo-jira-ticket/template.md) | Triage a Jira ticket into a structured issue |

### Rules (short guardrails -- always applicable)

| Rule | Scope | Repo scope |
|------|-------|------------|
| [dynamo-core-rules](rules/dynamo-core-rules.md) | C#/.NET coding standards, NUnit, PublicAPI, security, quality checks. | Repo-specific variant |

## Folder Structure

```
.ai/agents/
├── skills/
│   ├── dynamo-dotnet-expert/
│   │   └── SKILL.md
│   ├── dynamo-onboarding/
│   │   └── SKILL.md
│   ├── dynamo-pr-description/
│   │   ├── SKILL.md          ← workflow for writing PR descriptions
│   │   └── template.md       ← copy/paste PR template
│   └── dynamo-jira-ticket/
│       ├── SKILL.md          ← workflow for writing Jira tickets
│       └── template.md       ← copy/paste Jira template
├── rules/
│   └── dynamo-core-rules.md
└── README.md                  ← you are here
```

## File classification

- **Identical across repos**: Content is the same in both Dynamo and DynamoMCP repos. Changes should be mirrored.
- **Repo-specific variant**: Same skill name across repos, but content is tailored to each repo's architecture, templates, and tools.

## How this relates to other guidance files

```
.ai/agents/          <-- canonical source of truth (you are here)
.cursor/rules/*.mdc  <-- auto-applied Cursor constraints (auto-apply by glob)
.github/copilot-instructions.md  <-- Copilot guidance + pointers here
.github/agents/      <-- existing Copilot agents (Janitor, UX Designer, ContentDesigner)
CLAUDE.md            <-- Claude guidance + pointers here
```

## For Cursor users

Reference skills with `@.ai/agents/skills/dynamo-dotnet-expert/SKILL.md`. `.cursor/rules/` files auto-apply based on file globs.

## For Copilot users

Read `.github/copilot-instructions.md` for project context. Existing agents in `.github/agents/` are also available. Reference skills here for task workflows.

## For Claude users

Read `CLAUDE.md` at the repo root for project context. Reference skills here for detailed task guidance.
