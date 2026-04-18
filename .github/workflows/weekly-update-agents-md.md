---
name: Weekly AGENTS.md Maintenance
description: Reviews merged PRs and source updates since the previous run, updates AGENTS.md, and opens or updates a maintenance PR
on:
  schedule: weekly
  workflow_dispatch:

permissions:
  contents: read
  issues: read
  pull-requests: read
  actions: read

concurrency:
  group: weekly-update-agents-md
  cancel-in-progress: true

engine: copilot
network: defaults
strict: true
timeout-minutes: 30

tools:
  github:
    toolsets: [default, actions]
  edit:
  bash:
    - "git status"
    - "git diff -- AGENTS.md"
    - "cat AGENTS.md"

safe-outputs:
  create-pull-request:
    expires: 7d
    title-prefix: "[agents-maintenance] "
    labels: [automation, documentation]
    draft: false
  push-to-pull-request-branch:
  noop:
  missing-tool:
---

# Weekly AGENTS.md Maintenance

You maintain `/AGENTS.md` so it accurately reflects current repository architecture, workflows, tooling, and constraints.

## Mission

Every run, review changes since the previous successful run and keep `AGENTS.md` current. If updates are needed, commit only `AGENTS.md` and open or update a pull request.

## Process

1. Determine the comparison window:
   - Identify the previous successful run of this workflow using Actions data.
   - If no prior successful run exists, use the last 7 days as the baseline.

2. Collect repository changes since the baseline:
   - Merged pull requests.
   - Source and config file changes that affect developer guidance (for example: `src/**`, `test/**`, `.github/workflows/**`, `.agents/**`, build/test scripts, and top-level engineering docs).

3. Review the current `AGENTS.md` and assess whether any sections are stale, missing, or inaccurate based on the collected changes.

4. Update `AGENTS.md` only when needed:
   - Keep edits factual, concise, and derived from repository evidence.
   - Preserve existing structure and tone.
   - Do not add speculative guidance.

5. Validate the result:
   - Confirm `AGENTS.md` remains internally consistent.
   - Confirm no unrelated files are modified.

6. Open or update PR:
   - If no `AGENTS.md` changes are required, call `noop` with a short explanation.
   - If changes are required:
     - Reuse an existing open maintenance PR if one exists with title prefix `[agents-maintenance]` by pushing to its branch.
     - Otherwise create a new PR.
     - PR title must clearly state this is a weekly AGENTS.md refresh and summarize what changed.
     - PR description must list: baseline time window, merged PRs reviewed, and key AGENTS.md updates.

## Guardrails

- Never modify files other than `AGENTS.md`.
- Do not use direct GitHub write permissions; use safe outputs only.
- If required context cannot be retrieved, report with `noop` and explain what is missing.
