# Curation Rules

## The raw generator is a starting point, not a filter

`scripts/generate-release-notes.ps1` lists every PR merged to `master` within a date
window, tagged `[CP: ]` (cherry-picked into the RC branch), `[NO CP]`, or `[DIRECT RC]`.
None of these tags mean "user-facing" or "belongs in this release":

- Plenty of `[NO CP]` entries are real user-facing work that shipped pre-RC.
- Plenty of `[CP: ]` entries are internal-only (version bumps, test-only fixes).
- The tags say nothing about whether the fix already shipped under an *earlier* release —
  see the duplicate-publication check below.

Treat the raw dump as "everything to triage," never as "everything to include."

## Dedup and drop

- **Collapse revert/reapply churn.** A ticket that appears multiple times because a fix
  was reverted and reapplied (or reworked) within the window collapses to one bullet
  describing the final net behavior change.
- **Drop internal/build/test/version-bump-only entries.** Pure version bumps,
  test-only fixes, CI/pipeline changes, and other non-user-facing churn don't belong in
  user-facing release notes, regardless of CP tag.

## Shipped-baseline bug-inclusion rule

A bug fix belongs in this release's notes only if the underlying bug existed in an
already-*shipped* release (a version that real users have run). Bugs introduced and
fixed entirely within the current development cycle are excluded — **regardless of beta
exposure** (a bug only ever seen in a beta build doesn't count as "shipped" for this
purpose, since betas aren't the general release population).

Rationale: users reading the notes for "what changed since the last release you had"
shouldn't see a bug fix for a bug they never could have hit.

## Security-fix identification

Look for: CVE references, fixes tagged as closing a CA2327/CA2328/CA2329/CA2330 analyzer
violation (see `.claude/rules/dynamo-core-rules.md`), credential/secret-handling fixes,
injection-class fixes, or anything the PR itself explicitly calls a security fix. When in
doubt, ask the requester rather than silently bucketing it as a plain Bug Fix — security
fixes get their own `#### Security Fixes` section.

## Breaking-changes sweep

A minor or patch release should have **zero** breaking changes by definition. Check
`PublicAPI.Unshipped.txt`/`PublicAPI.Shipped.txt` diffs for any removed or renamed public
member within the window. If you find one, don't fold it quietly into a Bug Fixes bullet
— flag it explicitly to the requester as a process question (why did a breaking change
land in a minor/patch cycle?), separate from the normal editorial "needs a call" list.

## External-contributor credit sweep

Identify commit/PR authors outside the normal Autodesk employee set. For each one, do
**not** assume from username or profile alone whether they're:

- A genuine external community contributor (credit them).
- An employee of an agency contracted by Autodesk to work on Dynamo (no separate
  attribution — they're effectively internal for this purpose, even though their commits
  don't come from an autodesk.com-affiliated account).
- A bot (e.g. Copilot) — exclude entirely, never credit.

Ask the requester to confirm anyone not already known to be one of the three. This
distinction has to be re-confirmed per release, since contractor engagements and
community-contributor rosters both change over time.

## Category QA pass

Re-check the raw generator's bucketing — it doesn't reason about category, only about CP
status. Use:

- **Features** — a wholly new capability.
- **Enhancements** — an improvement to existing, already-shipped capability.
- **Bug Fixes** — corrects behavior that was wrong relative to intent.
- **Security Fixes** — closes a security gap (see above).

## Duplicate-publication cross-check (C19)

Before finalizing, diff every candidate PR number against the **entire** existing
`Release-Notes.md` content — every prior version section on the wiki page, not just the
one immediately before this release. The raw generator's date-window logic doesn't
exclude PRs whose fixes already shipped under an intermediate patch release that also
branched from `master` within the same window.

This caught 5 duplicate PRs in the 4.2.0 pass (already published, verbatim, under
`### 4.1.1`) that the generator's own CP tags gave no signal about at all — the PRs
looked like ordinary new `[NO CP]`/`[CP: ]` entries for 4.2.0 with nothing to distinguish
them. Drop any exact match. See DynaNotes `DynamoRelease/improvement-plan.md` item
**C19** for the full incident writeup.

## When something doesn't fit these rules

Add it to the "needs a call" list rather than guessing, and — once resolved — consider
whether the rule set here needs updating for next time (see the skill's Retro step).
