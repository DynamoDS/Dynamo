---
name: dynamo-skill-writer
description: Author and maintain Dynamo agent skills while enforcing cross-tool sync between canonical .agents skills and generated wrappers for GitHub Copilot, Cursor, and Claude Code.
---

# Dynamo Skill Writer

## When to use

- Creating a new skill in `.agents/skills/`.
- Updating an existing skill and ensuring tool mirrors remain aligned.
- Adding or updating Copilot agent wrappers that should be generated from canonical skills.
- Fixing drift between canonical skill content and generated wrappers.

## When not to use

- Writing product code, tests, or architecture docs unrelated to agent skills.
- One-off prompt output where no canonical skill should be created.

## Inputs expected

A request to add/update a skill, or a request to sync skill surfaces across Copilot/Cursor/Claude.

## Output format

A concrete set of file edits and validation steps that:
- updates canonical skill content in `.agents/skills/`
- updates sync metadata in `.github/scripts/sync_agent_wrappers.ps1` when Copilot exposure is needed
- regenerates and validates wrappers in `.github/agents/`
- updates index/docs references in `.agents/README.md`, `.github/copilot-instructions.md`, and `CLAUDE.md` when applicable

---

## Workflow

1. Determine whether this is a **new skill** or an **update to an existing skill**.
2. Author or update canonical content in `.agents/skills/<skill-name>/SKILL.md`.
3. If the skill should be available as a Copilot agent, add an entry to the `canonicalSkills` array in `.github/scripts/sync_agent_wrappers.ps1`.
4. Regenerate wrappers:
   - `./.github/scripts/sync_agent_wrappers.ps1`
5. Validate sync:
   - `./.github/scripts/sync_agent_wrappers.ps1 -Check -VerboseReport`
6. Update discovery docs when skill inventory changed:
   - `.agents/README.md`
   - `.github/copilot-instructions.md`
   - `CLAUDE.md`
7. Ensure no contradictions between canonical and mirrored surfaces.

## Rules

- `.agents/` is the canonical source of truth for skill logic.
- Generated wrappers in `.github/agents/` must not be hand-edited.
- Skill metadata (name/description) should be concise and stable.
- If mirrors conflict with canonical files, canonical files win.
- Keep changes deterministic so `-Check` mode stays reliable in CI.

## New Skill Checklist

- [ ] Created `.agents/skills/<skill-name>/SKILL.md` with frontmatter (`name`, `description`)
- [ ] Added skill to `.agents/README.md` Quick Reference table
- [ ] Added skill to `.agents/README.md` parity matrix (if cross-tool surfaced)
- [ ] Added skill to `.github/scripts/sync_agent_wrappers.ps1` wrapper map (if Copilot surfaced)
- [ ] Regenerated `.github/agents/*` wrappers
- [ ] Ran `./.github/scripts/sync_agent_wrappers.ps1 -Check -VerboseReport`
- [ ] Updated `.github/copilot-instructions.md` skill list
- [ ] Updated `CLAUDE.md` skill list

## Naming guidance

- Canonical skill id: kebab-case (example: `dynamo-skill-writer`)
- Copilot wrapper agent name: title case (example: `Dynamo Skill Writer`)

## Example intent

"Create a skill that enforces consistent skill-sync behavior across Copilot, Cursor, and Claude."

Expected result:
- New canonical skill in `.agents/skills/`
- New generated wrapper in `.github/agents/`
- Sync script updated and passing in check mode
- README/Copilot/Claude skill indexes updated
