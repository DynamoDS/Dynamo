# Dynamo Claude Configuration

Skills, rules, and subagents for AI-assisted development in the Dynamo repo.
Canonical source of truth for Claude Code, claude.com, and Copilot (via generated wrappers).

## Directory structure

```
.claude/
├── skills/          # Canonical agent skills (agentskills.io format)
│   └── <name>/
│       └── SKILL.md
├── rules/           # Always-on coding rules
│   └── dynamo-core-rules.md
├── agents/          # Subagents (specialized, with own tools/context)
│   └── security-analyzer.md
└── README.md        # This file
```

## Skills

Skills are reusable prompts following the [agentskills.io](https://agentskills.io/specification) format.
In Claude Code they are invoked with `/skill-name` or auto-triggered by description.
To upload to claude.com, ZIP the skill directory and upload via Settings > Capabilities.

| Skill | When to use |
|-------|-------------|
| [dynamo-codebase-patterns](skills/dynamo-codebase-patterns/SKILL.md) | Discovering and enforcing non-obvious Dynamo structural patterns |
| [dynamo-content-designer](skills/dynamo-content-designer/SKILL.md) | Writing docs, tutorials, release notes, and user-facing technical content |
| [dynamo-dotnet-expert](skills/dynamo-dotnet-expert/SKILL.md) | Writing or reviewing C#/.NET code, testing, PublicAPI management |
| [dynamo-dotnet-janitor](skills/dynamo-dotnet-janitor/SKILL.md) | Janitorial C#/.NET cleanup, modernization, and tech debt remediation |
| [dynamo-ecosystem-reviewer](skills/dynamo-ecosystem-reviewer/SKILL.md) | Reviewing changes for ecosystem compatibility and platform constraints |
| [dynamo-onboarding](skills/dynamo-onboarding/SKILL.md) | Learning Dynamo codebase architecture, finding where to start on tickets |
| [dynamo-pr-description](skills/dynamo-pr-description/SKILL.md) | Writing PR descriptions matching the Dynamo template |
| [dynamo-jira-ticket](skills/dynamo-jira-ticket/SKILL.md) | Creating or refining Jira tickets from bugs, test failures, or feature requests |
| [dynamo-skill-writer](skills/dynamo-skill-writer/SKILL.md) | Authoring/updating skills and keeping Copilot wrappers in sync |
| [dynamo-unit-testing](skills/dynamo-unit-testing/SKILL.md) | Writing NUnit tests following Dynamo patterns |
| [dynamo-ux-designer](skills/dynamo-ux-designer/SKILL.md) | Planning UX flows and producing Weave-aligned interface designs |
| [dynamo-webview-component-scaffold](skills/dynamo-webview-component-scaffold/SKILL.md) | Scaffolding new Dynamo WebView2 view-extension package repositories |

## Rules

Rules in `rules/` are always-on guardrails applied across all sessions.

| Rule | Scope |
|------|-------|
| [dynamo-core-rules](rules/dynamo-core-rules.md) | C#/.NET coding standards, NUnit, PublicAPI, security, quality checks |

## Subagents

Subagents in `agents/` are specialized assistants with their own tools and context.

| Subagent | Purpose |
|---------|---------|
| [security-analyzer](agents/security-analyzer.md) | Deep security analysis for C#/.NET and Dynamo-specific vulnerability patterns |

## Cross-tool parity

Skills are the canonical source. The table below shows how each tool loads them.

| Tool | How skills load |
|------|----------------|
| **Claude Code** | Natively from `.claude/skills/` |
| **claude.com** | Upload ZIP of skill directory via Settings > Capabilities |
| **Copilot (VS Code)** | Natively from `.claude/agents/` (Claude format) **and** `.github/agents/` (generated wrappers) |
| **Copilot (GitHub.com / CLI)** | Generated wrappers in `.github/agents/` only |
| **Cursor** | Reference skill files directly with `@.claude/skills/<name>/SKILL.md` |

## Wrapper sync

Keep Copilot wrappers in `.github/agents/` in sync with skills:

```powershell
# Regenerate wrappers
./.github/scripts/sync_agent_wrappers.ps1

# Validate (non-zero exit on drift — run in CI)
./.github/scripts/sync_agent_wrappers.ps1 -Check -Report
```
