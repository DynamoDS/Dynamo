# Dynamo Claude Configuration

Skills and rules for AI-assisted development in the Dynamo repo.
Canonical source of truth for Claude Code, claude.com, and Copilot (via generated wrappers).

## Directory structure

```
.claude/
├── skills/          # Canonical agent skills (agentskills.io format)
│   └── <name>/
│       └── SKILL.md
├── rules/           # Always-on coding rules
│   └── <rule-name>.md
└── README.md        # This file
```

## Skills

Reusable prompts following the [agentskills.io](https://agentskills.io/specification) format. Each skill lives in `skills/<name>/SKILL.md`. In Claude Code they are auto-triggered by description or invoked explicitly. To upload to claude.com, ZIP the skill directory and upload via Settings > Capabilities.

## Rules

Always-on guardrails in `rules/` applied across all sessions.

## Cross-tool parity

Skills are the canonical source. The table below shows how each tool loads them.

| Tool | How skills load |
|------|----------------|
| **Claude Code** | Natively from `.claude/skills/` |
| **Copilot (VS Code)** | Generated wrappers in `.github/agents/` |
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
