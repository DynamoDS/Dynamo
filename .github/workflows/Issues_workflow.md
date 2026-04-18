---
name: Issue Workflow
description: Agentic issue triage workflow for Dynamo repository issues.
on:
  issues:
    types: [opened, reopened, edited]
  roles: all
permissions:
  contents: read
  issues: read
  pull-requests: read
tools:
  github:
    toolsets: [default]
safe-outputs:
  add-labels:
    max: 4
  remove-labels:
    max: 4
  add-comment:
    max: 1
    hide-older-comments: true
  assign-to-user:
    max: 2
---

# Issue Triage

Triage issue #${{ github.event.issue.number }} in `${{ github.repository }}`.

Goals:
1. Apply one issue **type** label and one **priority** label.
2. Identify likely duplicates.
3. Ask clarifying questions when details are incomplete.
4. Assign to the most appropriate team member.

Triage process:
1. Read the issue title, body, current labels, current assignees, and recent comments.
2. Classify issue type from repository conventions (for example: bug, enhancement/feature, question, documentation, regression, wishlist, host-specific labels like Revit).
3. Determine priority (critical/high/medium/low or p0/p1/p2/p3 style used by this repository).
4. Search for potentially duplicate open/closed issues using strong title keywords, stack trace fragments, host/version details, and key symptoms.
5. Determine whether information is missing (especially repro steps, expected vs actual behavior, environment/version, and supporting artifacts).
6. Assign to the most suitable team member by matching component/host area and ownership patterns from similar recently triaged issues.

Rules:
- Use existing repository labels only; do not invent new labels.
- Keep exactly one type label and one priority label whenever possible:
  - Remove outdated type/priority labels before adding the final selection.
- If a likely duplicate exists:
  - Add an appropriate duplicate-related label if available.
  - Add a concise comment linking candidate duplicate issues and confidence.
- If the description is unclear or incomplete:
  - Add `needs more info` if available.
  - Post a short clarifying comment with specific questions needed to unblock triage.
- If the issue is clear, avoid unnecessary comments.
- Assign to one or two maintainers with relevant ownership; do not assign bots or the issue author unless they are explicitly a maintainer for that area.
- Be conservative: if uncertain between options, explain uncertainty briefly in the comment and choose the best available label/assignee from repository patterns.
