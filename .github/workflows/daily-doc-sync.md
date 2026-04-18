---
on:
  workflow_dispatch:
  schedule: daily on weekdays

permissions:
  contents: read
  issues: read
  pull-requests: read

engine: copilot

tools:
  github:
    toolsets: [default]

network: defaults

safe-outputs:
  create-pull-request:
    max: 1
    allowed-files:
      - doc/**
      - README.md
      - CONTRIBUTING.md

---

# daily-doc-sync

Keep repository documentation synchronized with recent source changes.

## Instructions

1. Identify code changes made in the default branch since the last successful run (or the last 24 hours if previous-run metadata is unavailable).
2. Determine which documentation files are likely impacted by those changes. Prioritize:
   - `/doc/**`
   - `/README.md`
   - `/CONTRIBUTING.md`
   - Other user-facing markdown docs in the repository root, excluding protected agent-instruction files.
3. For each candidate doc file, verify if its current content is out of date relative to the code changes.
4. Update only the documentation files that are actually out of sync. Keep edits minimal, accurate, and consistent with existing style.
5. If no documentation changes are needed, finish without creating a pull request.
6. If changes are needed, create exactly one pull request using safe outputs with:
   - Title:
     - If recent related commits/PRs reference a `DYN-####` ticket, use that ticket in the PR title.
     - Otherwise use `DYN-9484 - Update documentation from recent code changes`.
   - A concise summary of what changed and why.
   - A list of updated doc files and the related code areas.
7. Do not modify non-documentation files.

## Notes

- Prefer high-confidence updates over speculative rewrites.
- Avoid duplicating information already documented elsewhere.
