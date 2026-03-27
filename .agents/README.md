# Dynamo Agent Guidance

Shared skills, rules, and templates for AI-assisted development in the Dynamo repo. These files are the **canonical source of truth** -- tool-specific mirrors (`.github/copilot-instructions.md`, `CLAUDE.md`, `.github/agents/`) point back here.

## Canonical Policy

- Edit behavior and workflow guidance in `.agents/` only.
- Tool-specific files (`.github/agents/`, `.github/copilot-instructions.md`, `CLAUDE.md`) are mirrors or wrappers.
- If any mirror/wrapper conflicts with `.agents/`, `.agents/` wins.
- For generated wrappers, do not hand-edit generated sections.

## Wrapper Sync

Use the wrapper sync script to keep `.github/agents/` aligned with canonical skills:

```powershell
# Regenerate wrappers from canonical skills
./.github/scripts/sync_agent_wrappers.ps1

# Validate wrappers are in sync (non-zero exit code on drift)
./.github/scripts/sync_agent_wrappers.ps1 -Check

# Print a compact diagnostics summary (does not change pass/fail)
./.github/scripts/sync_agent_wrappers.ps1 -Check -VerboseReport
```

Check mode validates:
- Missing mapped wrappers
- Content drift in mapped wrappers
- Orphan auto-generated wrappers in `.github/agents/` that are no longer declared in the wrapper map

## Cross-Tool Parity Matrix

This matrix documents how canonical skills in `.agents/skills/` are exposed in each tool surface.

| Canonical skill | Cursor | Copilot (VS Code/GitHub) | Claude Code |
|---|---|---|---|
| `dynamo-codebase-patterns` | Loaded directly from `.agents/skills/dynamo-codebase-patterns/SKILL.md` | Wrapper: `.github/agents/dynamo-codebase-patterns.agent.md` | Loaded directly from `.agents/skills/dynamo-codebase-patterns/SKILL.md` |
| `dynamo-content-designer` | Loaded directly from `.agents/skills/dynamo-content-designer/SKILL.md` | Wrapper: `.github/agents/dynamo-content-designer.agent.md` | Loaded directly from `.agents/skills/dynamo-content-designer/SKILL.md` |
| `dynamo-dotnet-expert` | Loaded directly from `.agents/skills/dynamo-dotnet-expert/SKILL.md` | Wrapper: `.github/agents/dynamo-dotnet-expert.agent.md` | Loaded directly from `.agents/skills/dynamo-dotnet-expert/SKILL.md` |
| `dynamo-dotnet-janitor` | Loaded directly from `.agents/skills/dynamo-dotnet-janitor/SKILL.md` | Wrapper: `.github/agents/dynamo-dotnet-janitor.agent.md` | Loaded directly from `.agents/skills/dynamo-dotnet-janitor/SKILL.md` |
| `dynamo-ecosystem-reviewer` | Loaded directly from `.agents/skills/dynamo-ecosystem-reviewer/SKILL.md` | Wrapper: `.github/agents/dynamo-ecosystem-reviewer.agent.md` | Loaded directly from `.agents/skills/dynamo-ecosystem-reviewer/SKILL.md` |
| `dynamo-onboarding` | Loaded directly from `.agents/skills/dynamo-onboarding/SKILL.md` | Wrapper: `.github/agents/dynamo-onboarding.agent.md` | Loaded directly from `.agents/skills/dynamo-onboarding/SKILL.md` |
| `dynamo-pr-description` | Loaded directly from `.agents/skills/dynamo-pr-description/SKILL.md` | Wrapper: `.github/agents/dynamo-pr-description.agent.md` | Loaded directly from `.agents/skills/dynamo-pr-description/SKILL.md` |
| `dynamo-jira-ticket` | Loaded directly from `.agents/skills/dynamo-jira-ticket/SKILL.md` | Wrapper: `.github/agents/dynamo-jira-ticket.agent.md` | Loaded directly from `.agents/skills/dynamo-jira-ticket/SKILL.md` |
| `dynamo-skill-writer` | Loaded directly from `.agents/skills/dynamo-skill-writer/SKILL.md` | Wrapper: `.github/agents/dynamo-skill-writer.agent.md` | Loaded directly from `.agents/skills/dynamo-skill-writer/SKILL.md` |
| `dynamo-unit-testing` | Loaded directly from `.agents/skills/dynamo-unit-testing/SKILL.md` | Wrapper: `.github/agents/dynamo-unit-testing.agent.md` | Loaded directly from `.agents/skills/dynamo-unit-testing/SKILL.md` |
| `dynamo-ux-designer` | Loaded directly from `.agents/skills/dynamo-ux-designer/SKILL.md` | Wrapper: `.github/agents/dynamo-ux-designer.agent.md` | Loaded directly from `.agents/skills/dynamo-ux-designer/SKILL.md` |
| `dynamo-webview-component-scaffold` | Loaded directly from `.agents/skills/dynamo-webview-component-scaffold/SKILL.md` | Wrapper: `.github/agents/dynamo-webview-component-scaffold.agent.md` | Loaded directly from `.agents/skills/dynamo-webview-component-scaffold/SKILL.md` |

Notes:
- Skill logic lives only in `.agents/skills/`.
- Copilot (VS Code) wrappers are generated/validated by `.github/scripts/sync_agent_wrappers.ps1`. Cursor and Claude Code load directly from `.agents/`.
- If mirrors differ from canonical files, canonical files win.

## Quick Reference

### Skills (task workflows that produce outputs)

Each skill lives in its own folder with a `SKILL.md` and optionally a `template.md`.

| Skill | When to use | Repo scope |
|-------|-------------|------------|
| [dynamo-codebase-patterns](skills/dynamo-codebase-patterns/SKILL.md) | Discovering and enforcing non-obvious Dynamo structural patterns in scans and reviews. | Repo-specific variant |
| [dynamo-content-designer](skills/dynamo-content-designer/SKILL.md) | Writing documentation, tutorials, release notes, and user-facing technical content. | Repo-specific variant |
| [dynamo-dotnet-expert](skills/dynamo-dotnet-expert/SKILL.md) | Writing or reviewing C#/.NET code. Code design, testing, performance, PublicAPI management. | Repo-specific variant |
| [dynamo-dotnet-janitor](skills/dynamo-dotnet-janitor/SKILL.md) | Janitorial C#/.NET cleanup, modernization, and technical debt remediation. | Repo-specific variant |
| [dynamo-ecosystem-reviewer](skills/dynamo-ecosystem-reviewer/SKILL.md) | Reviewing changes for ecosystem compatibility and cross-repo/platform constraints. | Repo-specific variant |
| [dynamo-onboarding](skills/dynamo-onboarding/SKILL.md) | Learning the Dynamo codebase, architecture briefings, finding where to start on a Jira ticket. | Repo-specific variant |
| [dynamo-pr-description](skills/dynamo-pr-description/SKILL.md) | Writing PR descriptions matching the Dynamo template. | Repo-specific variant |
| [dynamo-jira-ticket](skills/dynamo-jira-ticket/SKILL.md) | Creating or refining Jira tickets from bugs, test failures, or feature requests. | Repo-specific variant |
| [dynamo-skill-writer](skills/dynamo-skill-writer/SKILL.md) | Authoring/updating skills and enforcing sync across Copilot, Cursor, and Claude surfaces. | Repo-specific variant |
| [dynamo-unit-testing](skills/dynamo-unit-testing/SKILL.md) | Writing NUnit tests following Dynamo patterns. Test classes, setup/teardown, .dyn file testing. | Repo-specific variant |
| [dynamo-ux-designer](skills/dynamo-ux-designer/SKILL.md) | Planning UX flows and producing Weave-aligned interface designs and mockups. | Repo-specific variant |
| [dynamo-webview-component-scaffold](skills/dynamo-webview-component-scaffold/SKILL.md) | Scaffolding new Dynamo WebView2 view-extension package repositories. | Repo-specific variant |

### Templates (bundled with skills)

Templates are co-located inside the skill folder that uses them:

| Template | Location | Purpose |
|----------|----------|---------|
| PR description | [.github/PULL_REQUEST_TEMPLATE.md](../../.github/PULL_REQUEST_TEMPLATE.md) | GitHub PR template (referenced by PR description skill) |
| Jira triage | [dynamo-jira-ticket/template.md](./skills/dynamo-jira-ticket/template.md) | Triage a Jira ticket into a structured issue |

### Rules (short guardrails -- always applicable)

| Rule | Scope | Repo scope |
|------|-------|------------|
| [dynamo-core-rules](./rules/dynamo-core-rules.md) | C#/.NET coding standards, NUnit, PublicAPI, security, quality checks. | Repo-specific variant |

## Folder Structure

```
.agents/
├── skills/
│   ├── dynamo-codebase-patterns/
│   │   ├── SKILL.md
│   │   └── patterns/
│   ├── dynamo-content-designer/
│   │   ├── SKILL.md
│   │   └── assets/
│   ├── dynamo-dotnet-expert/
│   │   └── SKILL.md
│   ├── dynamo-dotnet-janitor/
│   │   └── SKILL.md
│   ├── dynamo-ecosystem-reviewer/
│   │   └── SKILL.md
│   ├── dynamo-onboarding/
│   │   └── SKILL.md
│   ├── dynamo-pr-description/
│   │   └── SKILL.md
│   ├── dynamo-jira-ticket/
│   │   ├── SKILL.md
│   │   └── assets/
│   │       └── template.md       ← copy/paste Jira template
│   ├── dynamo-skill-writer/
│   │   └── SKILL.md
│   ├── dynamo-unit-testing/
│   │   ├── SKILL.md
│   │   ├── assets/
│   │   │   └── test-patterns.md     ← code templates & examples
│   │   └── references/
│   │       └── quality-checklist.md ← guidelines & best practices
│   ├── dynamo-ux-designer/
│   │   └── SKILL.md
│   └── dynamo-webview-component-scaffold/
│       └── SKILL.md
├── rules/
│   └── dynamo-core-rules.md
└── README.md                  ← you are here
```

## File classification

- **Identical across repos**: Content is the same in both Dynamo and DynamoMCP repos. Changes should be mirrored.
- **Repo-specific variant**: Same skill name across repos, but content is tailored to each repo's architecture, templates, and tools.

## How this relates to other guidance files

```
.agents/             <-- canonical source of truth (you are here); loaded directly by Cursor and Claude Code
.github/copilot-instructions.md  <-- Copilot (VS Code) guidance + pointers here
.github/agents/      <-- generated Copilot (VS Code) wrappers that mirror canonical skills
../AGENTS.md            <-- AI Agents guidance overview + pointers here
../CLAUDE.md            <-- Claude guidance + pointers here
```

## For Cursor users

Cursor loads agent skills directly from `.agents/`. No additional configuration needed — agents are available by name in the Cursor chat.

## For Copilot users

Read `.github/copilot-instructions.md` for project context. Existing agents in `.github/agents/` are also available. Reference skills here for task workflows.

## For Claude users

Read `CLAUDE.md` at the repo root for project context. Reference skills here for detailed task guidance.
