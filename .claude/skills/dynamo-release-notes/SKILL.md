---
name: dynamo-release-notes
description: Curate, sweep, draft, cross-check, and publish the "### {version}" section of the Dynamo GitHub wiki Release-Notes page, starting from the raw ReleaseNotes_{X.Y.Z}.md generator dump. Use when compiling release notes for a Dynamo release, deciding which PRs belong in a release's notes, or inserting/updating content on the Release-Notes wiki page (https://github.com/DynamoDS/Dynamo/wiki/Release-Notes).
---

# Dynamo Release Notes

## When to use

- Turning a raw `ReleaseNotes_{X.Y.Z}.md` generator dump (or an equivalent PR list) into
  a publishable wiki section for a Dynamo release — major, minor, or patch.
- Deciding whether a specific PR belongs in a release's notes, or which section
  (Features / Enhancements / Bug Fixes / Security Fixes) it belongs in.
- Inserting or correcting content on the live `DynamoDS/Dynamo` wiki's Release-Notes page.

## When not to use

- Writing or polishing release-notes *prose in isolation*, with the PR list already
  decided and no publishing step involved — see the [style guide](./assets/style-guide.md)
  directly.
- Any other content type (node descriptions, UI strings, blog posts, tutorials) — those
  stay with `dynamo-content-designer`.

## Inputs expected

- The target version number (e.g. `4.2.0`).
- The raw source: `ReleaseNotes_{X.Y.Z}.md` (produced by `PushRelease`'s
  `GENERATE_RELEASE_NOTES=on` stage running `scripts/generate-release-notes.ps1`), or an
  equivalent raw PR list for the version if that artifact wasn't generated.
- Whether this is a major/minor or patch release (changes which prior-release baseline
  applies to the bug-inclusion rule below).
- Access to the public `DynamoDS/Dynamo` wiki repo, and — for internal ticket/PR
  cross-refs (e.g. DYN tickets whose PRs live on `git.autodesk.com`) — access to that GHE
  instance.

## Output format

The finalized `### {version}` wiki section, live on
`https://github.com/DynamoDS/Dynamo/wiki/Release-Notes`, verified post-push against a
fresh fetch of the page.

---

## Workflow

1. **Orient before starting.** This skill may be run by someone other than its author —
   don't assume the caller already knows the process. Summarize the steps below, then
   ask two things: (a) proceed with the process as written, or adjust it first; (b)
   confirm the version number, the raw source file, and the release type. Treat this as
   a standing checkpoint on every run, not a one-time question.
2. **Curate the raw dump.** Apply the rules in
   [curation-rules.md](./assets/curation-rules.md): collapse revert/reapply churn on the
   same ticket to its net behavior change, drop internal/build/test/version-bump-only
   entries, and apply the shipped-baseline bug rule (a bug fix belongs in the notes only
   if the underlying bug existed in an already-*shipped* release — bugs introduced and
   fixed entirely within the current dev cycle are excluded regardless of beta exposure).
3. **Run the sweeps** — security-fix identification, breaking-changes, external-contributor
   credit, and category QA. These are independent of each other and of step 2's
   line-by-line curation, so fan them out in parallel (subagents) rather than running
   serially. Details and the contractor-vs-community-contributor distinction are in
   [curation-rules.md](./assets/curation-rules.md).
4. **Flag, don't guess.** Any inclusion, exclusion, or categorization call that isn't
   clear-cut from the raw PR text goes into an explicit "needs a call" list for the
   requester to decide. Never silently resolve ambiguity — this mirrors
   `dynamo-content-designer`'s "never hallucinate, always ask" rule, applied to editorial
   judgment calls instead of prose.
5. **Draft the prose** per `dynamo-content-designer`'s writing principles and the
   [style guide](./assets/style-guide.md) — past-tense verb, neutral tone, no internal
   jargon, one sentence per bullet where possible. Format: `### {version}` heading, then
   `#### Features` / `#### Enhancements` / `#### Bug Fixes` / `#### Security Fixes`
   (only sections with content), each item as `> * <sentence>. [PR](url)`.
6. **Cross-check against everything already published** — not just the immediately
   preceding version. Diff every candidate PR number against the *entire* existing
   `Release-Notes.md` content, all prior version sections. The raw generator only knows
   whether a PR was cherry-picked into this release's RC branch; it has no concept of "this
   PR's fix already shipped under an earlier patch release that also branched from
   `master` in the same window." Drop exact matches. This step exists because it caught 5
   duplicate PRs in the 4.2.0 pass that the generator's own tags completely missed — see
   `curation-rules.md` and DynaNotes `DynamoRelease/improvement-plan.md` item **C19**.
7. **Validate links and content.** For every cited PR: confirm the link resolves, and
   confirm the bullet text matches the PR's own `### Release Notes` body field — not just
   its title. Titles are sometimes misleadingly narrow (e.g. a title mentioning one
   template when the PR's actual described scope covers four).
8. **Insert into the live wiki.** Follow
   [wiki-publish-plumbing.md](./assets/wiki-publish-plumbing.md) exactly. The wiki repo
   contains a Windows-invalid filename that breaks any git operation touching the full
   index or working tree — normal clone/checkout/add/reset will fail or, worse, silently
   stage the entire wiki as deleted. The plumbing recipe never touches the index or
   working tree, so the real safety check is the `diff-tree` comparison in step 5 above —
   confirm it reports exactly one changed entry before proceeding to `commit-tree`. **Ask
   the requester to explicitly confirm before pushing** — a wiki push has no PR or review
   gate and is immediately live and public.
9. **Verify post-push.** Re-fetch the raw page and diff it against the pre-push version;
   confirm only the intended lines changed anywhere on the page.
10. **Retro.** Ask the requester whether anything about this run should change the skill
    itself — a new gotcha, a rule that needs adjusting, a step that was unclear or
    missing. If yes, update this skill and its assets now, while the detail is fresh,
    rather than deferring it to "someday."

## Boundaries

- ✅ **Always**: run the full curate → sweep → flag → cross-check → validate pipeline
  before drafting is considered final; confirm the `diff-tree` check shows exactly one
  changed entry before any commit-equivalent step in a wiki clone; re-verify the page
  after every push.
- ⚠️ **Ask first**: any PR whose inclusion/exclusion/category isn't clear-cut (flag,
  don't guess); crediting or omitting an external contributor when authorship is
  ambiguous; pushing the finalized section to the live wiki.
- 🚫 **Never**: run whole-index git operations (`git add -A`, `git reset`, `git checkout .`,
  a normal `git clone`) against a local clone of the `Dynamo.wiki` repo — see
  `wiki-publish-plumbing.md` for why and what to use instead. Never skip the step 6
  duplicate-publication cross-check. Never skip the pre-push confirmation.

## Assets & References

- **[curation-rules.md](./assets/curation-rules.md)** — dedup/inclusion rules, the four
  sweeps, and the C19 duplicate-publication cross-check in detail.
- **[style-guide.md](./assets/style-guide.md)** — sentence-level style rules and good
  examples for release-note bullets (moved here from `dynamo-content-designer`, which now
  redirects release-notes requests to this skill).
- **[wiki-publish-plumbing.md](./assets/wiki-publish-plumbing.md)** — the exact git
  plumbing recipe for safely editing the `Dynamo.wiki` repo on Windows.
