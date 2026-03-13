# Dynamo Agent Guidance

Shared skills, rules, and templates for AI-assisted development in the Dynamo repo. These files are the **canonical source of truth** -- tool-specific mirrors (`.cursor/skills/`, `.github/copilot-instructions.md`, `CLAUDE.md`, `.github/agents/`) point back here.

## Canonical Policy

- Edit behavior and workflow guidance in `.agents/` only.
- Tool-specific files (`.github/agents/`, `.github/copilot-instructions.md`, `CLAUDE.md`, `.cursor/skills/`) are mirrors or wrappers.
- If any mirror/wrapper conflicts with `.agents/`, `.agents/` wins.
- For generated wrappers, do not hand-edit generated sections.

## Wrapper Sync

Use the wrapper sync script to keep `.github/agents/` aligned with canonical skills:

```powershell
# Regenerate wrappers from canonical skills
./tools/agents/sync-agent-wrappers.ps1

# Validate wrappers are in sync (non-zero exit code on drift)
./tools/agents/sync-agent-wrappers.ps1 -Check

# Print a compact diagnostics summary (does not change pass/fail)
./tools/agents/sync-agent-wrappers.ps1 -Check -VerboseReport
```

Check mode validates:
- Missing mapped wrappers
- Content drift in mapped wrappers
- Orphan auto-generated wrappers in `.github/agents/` that are no longer declared in the wrapper map

## Cross-Tool Parity Matrix

This matrix documents how canonical skills in `.agents/skills/` are exposed in each tool surface.

| Canonical skill | Cursor | Copilot (VS Code/GitHub) | Claude Code |
|---|---|---|---|
| `dynamo-dotnet-expert` | Reference `@.agents/skills/dynamo-dotnet-expert/SKILL.md` (or mirror from `.cursor/skills/` when present) | Wrapper: `.github/agents/Dynamo Dotnet Expert.md` | Referenced from `CLAUDE.md` via `.agents/` |
| `dynamo-onboarding` | Reference `@.agents/skills/dynamo-onboarding/SKILL.md` | Wrapper: `.github/agents/Dynamo Onboarding.md` | Referenced from `CLAUDE.md` via `.agents/` |
| `dynamo-pr-description` | Reference `@.agents/skills/dynamo-pr-description/SKILL.md` | Wrapper: `.github/agents/Dynamo PR Description.md` | Referenced from `CLAUDE.md` via `.agents/` |
| `dynamo-jira-ticket` | Reference `@.agents/skills/dynamo-jira-ticket/SKILL.md` | Wrapper: `.github/agents/Dynamo Jira Ticket.md` | Referenced from `CLAUDE.md` via `.agents/` |
| `dynamo-skill-writer` | Reference `@.agents/skills/dynamo-skill-writer/SKILL.md` | Wrapper: `.github/agents/Dynamo Skill Writer.md` | Referenced from `CLAUDE.md` via `.agents/` |

Notes:
- Skill logic lives only in `.agents/skills/`.
- Copilot wrappers are generated/validated by `tools/agents/sync-agent-wrappers.ps1`.
- If mirrors differ from canonical files, canonical files win.

## Quick Reference

### Skills (task workflows that produce outputs)

Each skill lives in its own folder with a `SKILL.md` and optionally a `template.md`.

| Skill | When to use | Repo scope |
|-------|-------------|------------|
| [dynamo-dotnet-expert](skills/dynamo-dotnet-expert/SKILL.md) | Writing or reviewing C#/.NET code. Code design, testing, performance, PublicAPI management. | Repo-specific variant |
| [dynamo-onboarding](skills/dynamo-onboarding/SKILL.md) | Learning the Dynamo codebase, architecture briefings, finding where to start on a Jira ticket. | Repo-specific variant |
| [dynamo-pr-description](skills/dynamo-pr-description/SKILL.md) | Writing PR descriptions matching the Dynamo template. | Repo-specific variant |
| [dynamo-jira-ticket](skills/dynamo-jira-ticket/SKILL.md) | Creating or refining Jira tickets from bugs, test failures, or feature requests. | Repo-specific variant |
| [dynamo-skill-writer](skills/dynamo-skill-writer/SKILL.md) | Authoring/updating skills and enforcing sync across Copilot, Cursor, and Claude surfaces. | Repo-specific variant |
| [dynamo-unit-testing](skills/dynamo-unit-testing/SKILL.md) | Writing NUnit tests following Dynamo patterns. Test classes, setup/teardown, .dyn file testing. | Repo-specific variant |

### Templates (bundled with skills)

Templates are co-located inside the skill folder that uses them:

| Template | Location | Purpose |
|----------|----------|---------|
| PR description | [.github/PULL_REQUEST_TEMPLATE.md](../../.github/PULL_REQUEST_TEMPLATE.md) | GitHub PR template (referenced by PR description skill) |
| Jira triage | [dynamo-jira-ticket/assets/template.md](./skills/dynamo-jira-ticket/assets/template.md) | Triage a Jira ticket into a structured issue |

### Rules (short guardrails -- always applicable)

| Rule | Scope | Repo scope |
|------|-------|------------|
| [dynamo-core-rules](./rules/dynamo-core-rules.md) | C#/.NET coding standards, NUnit, PublicAPI, security, quality checks. | Repo-specific variant |

## Folder Structure

```
.agents/
├── skills/
│   ├── dynamo-dotnet-expert/
│   │   └── SKILL.md
│   ├── dynamo-onboarding/
│   │   └── SKILL.md
│   ├── dynamo-pr-description/
│   │   ├── SKILL.md          ← workflow for writing PR descriptions
│   ├── dynamo-jira-ticket/
│   │   ├── SKILL.md          ← workflow for writing Jira tickets
│   │   └── template.md       ← copy/paste Jira template
│   ├── dynamo-skill-writer/
│   │   └── SKILL.md          ← workflow for authoring and syncing skills
│   └── dynamo-unit-testing/
│       ├── SKILL.md          ← workflow for writing tests
│       ├── test-patterns.md  ← code templates & examples
│       └── quality-checklist.md  ← guidelines & best practices
├── rules/
│   └── dynamo-core-rules.md
└── README.md                  ← you are here
```

## File classification

- **Identical across repos**: Content is the same in both Dynamo and DynamoMCP repos. Changes should be mirrored.
- **Repo-specific variant**: Same skill name across repos, but content is tailored to each repo's architecture, templates, and tools.

## How this relates to other guidance files

```
.agents/             <-- canonical source of truth (you are here)
.cursor/skills/      <-- Cursor skill mirror/pointers
.github/copilot-instructions.md  <-- Copilot guidance + pointers here
.github/agents/      <-- existing Copilot agents (Janitor, UX Designer, ContentDesigner)
../AGENTS.md            <-- Agent guidance overview + pointers here
../CLAUDE.md            <-- Claude guidance + pointers here
```

## For Cursor users

Reference skills with `@.agents/skills/dynamo-dotnet-expert/SKILL.md`. Use `.cursor/skills/` as the Cursor-facing mirror/pointer location.

## For Copilot users

Read `.github/copilot-instructions.md` for project context. Existing agents in `.github/agents/` are also available. Reference skills here for task workflows.

## For Claude users

Read `CLAUDE.md` at the repo root for project context. Reference skills here for detailed task guidance.
