---
name: dynamo-skill-writer
description: Author and maintain Dynamo agent skills. Use this skill whenever creating a new skill in .agents/skills/, updating an existing skill, fixing drift between canonical skills and Copilot/Cursor/Claude wrappers, or asking how to write effective skill instructions for the Dynamo repo.
---

# Dynamo Skill Writer

## When to use

- Creating a new skill in `.agents/skills/`.
- Updating an existing skill and ensuring tool mirrors remain aligned.
- Adding or updating Copilot agent wrappers generated from canonical skills.
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
- updates index/docs references in `.agents/README.md`, `.github/copilot-instructions.md`, and `AGENTS.md` when applicable

---

## Workflow

1. Determine whether this is a **new skill** or an **update to an existing skill**.
2. Author or update canonical content in `.agents/skills/<skill-name>/SKILL.md`.
3. **Ask the user** whether the skill should be surfaced as a Copilot agent before touching any wrapper files. The sync steps below are optional — skip them unless the user confirms.
4. *(If confirmed)* Add an entry to the `canonicalSkills` array in `.github/scripts/sync_agent_wrappers.ps1`.
5. *(If confirmed)* Regenerate wrappers: `./.github/scripts/sync_agent_wrappers.ps1`
6. *(If confirmed)* Validate sync: `./.github/scripts/sync_agent_wrappers.ps1 -Check -VerboseReport`
7. Update discovery docs when skill inventory changed:
   - `.agents/README.md`
   - `.github/copilot-instructions.md`
   - `AGENTS.md`
8. Ensure no contradictions between canonical and mirrored surfaces.

## Boundaries

- ✅ **Always**: edit `.agents/skills/<skill>/SKILL.md` and its `assets/`/`references/` files; update `.agents/README.md` and `AGENTS.md`
- ⚠️ **Ask first**: touch `.github/scripts/sync_agent_wrappers.ps1` or trigger wrapper regeneration — confirm Copilot surfacing intent before running the sync script
- 🚫 **Never**: hand-edit generated wrappers in `.github/agents/` — they are always overwritten by the sync script, so manual edits are lost and create drift

## Rules

- `.agents/` is the canonical source of truth for skill logic. When mirrors conflict with canonical files, canonical wins.
- Generated wrappers in `.github/agents/` must never be hand-edited — they exist only as outputs of the sync script.
- Skill metadata (`name`/`description`) should be concise and stable; churn in these fields breaks triggering accuracy.
- Keep changes deterministic so `-Check` mode stays reliable in CI.

## New Skill Checklist

- [ ] Created `.agents/skills/<skill-name>/SKILL.md` with frontmatter (`name`, `description`)
- [ ] Added skill to `.agents/README.md` Quick Reference table
- [ ] Added skill to `.agents/README.md` parity matrix (if cross-tool surfaced)
- [ ] *(If Copilot-surfaced)* Added skill to `.github/scripts/sync_agent_wrappers.ps1` wrapper map
- [ ] *(If Copilot-surfaced)* Regenerated `.github/agents/*` wrappers
- [ ] *(If Copilot-surfaced)* Ran `./.github/scripts/sync_agent_wrappers.ps1 -Check -VerboseReport`
- [ ] Updated `.github/copilot-instructions.md` skill list
- [ ] Updated `AGENTS.md` skill list

## Naming guidance

- Canonical skill id: kebab-case (example: `dynamo-skill-writer`)
- Copilot wrapper agent name: title case (example: `Dynamo Skill Writer`)

## Example intent

> "Create a skill that enforces consistent skill-sync behavior across Copilot, Cursor, and Claude."

Expected result:
- New canonical skill in `.agents/skills/`
- User asked whether Copilot wrapper is needed before any sync steps run
- *(If yes)* New generated wrapper in `.github/agents/`, sync script updated and passing in check mode
- `AGENTS.md`, `.agents/README.md`, and `.github/copilot-instructions.md` skill indexes updated

---

## Writing a High-Quality Skill

### Start minimal, then iterate

Write a skill to handle one focused task well. Anticipating all requirements upfront produces bloated prompts that are harder to maintain and easier to overfit. Expand based on observed gaps instead.

### The description is the trigger — make it specific and slightly pushy

The description field is what the agent reads when deciding whether to consult this skill. Vague descriptions cause undertriggering; overly broad ones cause false matches. Include the concrete contexts that should activate the skill, not just a general summary.

❌ Vague:
```
description: Helps with code tasks in Dynamo.
```

✅ Specific and triggering:
```
description: Write and review NUnit tests for DynamoCore following Dynamo testing
patterns, base class selection, and AAA structure. Use when writing test methods,
setting up test infrastructure, or reviewing tests for quality.
```

Include the framework and key constraints when they matter (`.NET 10`, `NUnit`, `WPF Windows-only`). Per skill-creator guidance: all "when to use" information belongs in the description, not just the body.

### Commands belong early and must be executable

List commands in an early section with exact flags — not just tool names. An agent that doesn't know the flags will guess, and guesses are wrong.

❌ Missing flags:
```
Run dotnet test to execute tests.
```

✅ Executable:
```bash
dotnet test src/DynamoCoreTests/DynamoCoreTests.csproj --filter "Name~MyTest"
```

### Code examples over prose

One real snippet showing the expected pattern beats three paragraphs describing it. Explain the *why* behind rules rather than issuing mandates — an agent that understands reasoning generalizes better than one following a checklist.

Show both ✅ correct and ❌ anti-pattern forms for non-obvious rules:

❌ Prose only:
```
Override BuildOutputAst and return a list of AssociativeNodes.
```

✅ Code example:
```csharp
public override IEnumerable<AssociativeNode> BuildOutputAst(
    List<AssociativeNode> inputAstNodes)
{
    return new[] { AstFactory.BuildAssignment(
        GetAstIdentifierForOutputIndex(0),
        AstFactory.BuildFunctionCall("MyLib.MyFunc", inputAstNodes)) };
}
```

### Define explicit boundaries

Use a three-tier system so the agent knows what requires human approval before acting:

- ✅ **Always do** — safe, reversible, local actions (read files, run tests, edit `.cs` files)
- ⚠️ **Ask first** — affects shared state or is hard to reverse (add entries to `PublicAPI.Unshipped.txt`, edit `.resx` files, rename public types)
- 🚫 **Never do** — destructive or policy-violating (hand-edit generated wrappers in `.github/agents/`, remove entries from `PublicAPI.Shipped.txt`, suppress CA2327/CA2328/CA2329/CA2330)

### Map file-level responsibilities

When a skill touches multiple directories, state which it reads vs. writes. This prevents unintended side effects and makes it obvious what "done" looks like:

| Path | Access |
|---|---|
| `.agents/skills/<skill>/SKILL.md` | Read + Write (canonical) |
| `.github/agents/*.md` | Read only (generated — never hand-edit) |
| `.github/scripts/sync_agent_wrappers.ps1` | Read + Write (sync map) |
| `.agents/README.md` | Read + Write (index) |

### Reference assets, don't inline everything

Put detailed reference material in `assets/` or `references/` sub-files and link from the skill. This keeps `SKILL.md` under ~500 lines and scannable.

```
## Assets & References
- **[quality-checklist.md](./references/quality-checklist.md)** — anti-patterns and PR checklist
- **[template.md](./assets/template.md)** — copy-paste skill template
```

---

## Skill Template

```markdown
---
name: dynamo-<skill-name>
description: One sentence covering what this skill does AND the specific contexts
that should trigger it. Be concrete. Be slightly pushy.
---

# Dynamo <Skill Name>

## When to use
- Bullet list of triggering scenarios.

## When not to use
- Redirect to the correct skill where applicable.

## Inputs expected
What the caller must provide.

## Output format
What the skill produces.

---

## Workflow
Numbered steps. Reference asset files for detail rather than inlining.

## Boundaries
- ✅ Always: ...
- ⚠️ Ask first: ...
- 🚫 Never: ...

## Assets & References
- **[asset.md](./assets/asset.md)** — description
```
