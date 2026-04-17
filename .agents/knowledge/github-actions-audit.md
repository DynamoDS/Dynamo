# GitHub Actions Audit

Findings and remediation plan from a full review of all 18 workflows under `.github/workflows/`.
Use this document when planning CI/CD work or triaging workflow failures.

For the durable "never do this" rules derived from this audit, see [`rules/ci-rules.md`](../rules/ci-rules.md).

**Audit date:** 2026-04-15  
**Workflows reviewed:** `build_dynamo_all.yml`, `build_dynamo_core.yml`, `codeql_analysis.yml`,
`pr_jira_check.yml`, `pr_description_check.yml`, `check_file_size.yml`, `auto_cherrypick.yml`,
`close_stale_issues.yml`, `extract_release_notes.yml`, `move_issue.yml`, `Issues_workflow.yml`,
`dynamo_bin_diff.yml`, `dynamo_post_bin_diff.yml`, `trigger_l10n_jenkins.yml`,
`auto_update_src_html.yml`, `issue_type_predicter.yml`, `validate_agent_skills.yml`,
`generate_changelog.yml`

---

## Issue Inventory

### Critical Bugs

| ID | Workflow | Location | Description |
|----|----------|----------|-------------|
| BUG-01 | `build_dynamo_core.yml` | Line 93 | Linux binary check **always passes** — `test "./DynamoCLI.exe"` evaluates the non-empty string, not the file. Also checks for `.exe` which doesn't exist on Linux. |
| BUG-02 | `auto_cherrypick.yml` | Line 48 | `if [[ $milestone -eq "" ]]` uses arithmetic `-eq` to compare a string; always evaluates as equal when milestone is non-numeric — null milestone check silently never fires. |
| BUG-03 | `Issues_workflow.yml` | Lines 61, 137, 144, 150 | `curl -u admin:${{ secrets.GITHUB_TOKEN }}` is the wrong format for GitHub API auth. Will return 401 silently because `--fail` is absent. |

**Fixes:**

```bash
# BUG-01 — build_dynamo_core.yml (linux job)
# Before
test "./DynamoCLI.exe" && echo "DynamoCLI exists!"
# After
if test -f "./DynamoCLI"; then
  echo "DynamoCLI exists!"
else
  echo "::error::DynamoCLI was not found!"; exit 1
fi
```

```bash
# BUG-02 — auto_cherrypick.yml
# Before
if [[ $milestone -eq "" ]]; then
# After
if [[ -z "$milestone" ]]; then
```

```bash
# BUG-03 — Issues_workflow.yml
# Before
curl -v -u admin:${{ secrets.GITHUB_TOKEN }} -d '...' ${{ github.event.issue.url }}/labels
# After
curl -v -H "Authorization: token ${{ secrets.GITHUB_TOKEN }}" -d '...' ${{ github.event.issue.url }}/labels
```

---

### Security Issues

| ID | Workflow | Description | Remediation |
|----|----------|-------------|-------------|
| SEC-01 | `check_file_size.yml` | `tj-actions/changed-files@v46` not pinned to a commit SHA. This action has a [documented supply-chain compromise history](https://github.com/tj-actions/changed-files/issues). | Pin to a specific SHA: `tj-actions/changed-files@<sha>` and document the version in a comment. |
| SEC-02 | `pr_jira_check.yml` | `neofinancial/ticket-check-action@v2` not pinned to SHA. | Pin to SHA. |
| SEC-03 | `move_issue.yml` | `leonsteinhaeuser/project-beta-automations@v2.2.1` not pinned to SHA. | Pin to SHA. |
| SEC-04 | `Issues_workflow.yml`, `issue_type_predicter.yml` | `frabert/replace-string-action@v2.5` not pinned to SHA. | Pin to SHA. |
| SEC-05 | `dynamo_post_bin_diff.yml` | `dawidd6/action-download-artifact@v20` not pinned to SHA. | Pin to SHA. |
| SEC-06 | Multiple (6 workflows) | Missing `permissions` blocks — workflows inherit repo default (read/write everything). See details below. | Add least-privilege `permissions` to each job. |
| SEC-07 | `auto_cherrypick.yml` | Unauthenticated `curl` call to GitHub API (line 38) — will hit rate limits on busy repos. | Add `-H "Authorization: token ${{ secrets.GITHUB_TOKEN }}"`. |

**Missing `permissions` — per-workflow remediation:**

```yaml
# pr_description_check.yml
permissions:
  contents: read
  pull-requests: read

# check_file_size.yml
permissions:
  contents: read

# dynamo_bin_diff.yml
permissions:
  contents: read
  actions: write         # for cache save/restore

# dynamo_post_bin_diff.yml
permissions:
  pull-requests: write   # to create/update comments
  actions: write         # to delete caches

# close_stale_issues.yml
permissions:
  issues: write

# extract_release_notes.yml  (also uses DYNAMO_ISSUES_TOKEN PAT — this is for the workflow runner itself)
permissions:
  contents: read
```

---

### Consistency Issues

| ID | Workflow | Description | Remediation |
|----|----------|-------------|-------------|
| CON-01 | `issue_type_predicter.yml` | Uses `actions/setup-dotnet@v4`; all other workflows use `@v5`. | Upgrade to `@v5`. |
| CON-02 | `build_dynamo_all.yml`, `build_dynamo_core.yml` (Windows jobs), `dynamo_bin_diff.yml` | Hardcodes `repository: DynamoDS/Dynamo` in checkout step — forces checkout of the main repo even when triggered from a fork PR. The Linux job in `build_dynamo_core.yml` correctly omits this. | Remove the `repository:` override from all PR-triggered jobs. Keep it only where cross-repo checkout is intentional. |
| CON-03 | `build_dynamo_all.yml`, `dynamo_bin_diff.yml` | `save_pr_data` job runs on both `push` and `pull_request` triggers. On `push`, `github.event.number` is empty — saves a blank file. | Add `if: github.event_name == 'pull_request'` to the job condition. |
| CON-04 | `build_dynamo_core.yml` | Step name says `Look for DynamoCLI.exe` in the Linux job (line 89) but the binary is `DynamoCLI`. | Fix step name and checked filename (see BUG-01). |

---

### Performance Issues

| ID | Workflow | Description | Remediation |
|----|----------|-------------|-------------|
| PERF-01 | `validate_agent_skills.yml` | `cache: false` on `actions/setup-go@v6` — re-downloads all Go modules on every run. | Remove `cache: false` or set `cache: true`. |
| PERF-02 | `pr_description_check.yml` | Full `actions/checkout@v6` performed but only `github.event.pull_request.body` is used — no repo files are ever read. | Remove the checkout step entirely. |
| PERF-03 | `auto_update_src_html.yml` | `fetch-depth: 0` (full history) used when only two commit SHAs from the PR event are needed. | Use `fetch-depth: 2` or a targeted `git fetch`. |

---

### Maintainability Issues

| ID | Workflow | Description | Remediation |
|----|----------|-------------|-------------|
| MAINT-01 | `auto_update_src_html.yml` | Hardcoded Jira ticket `DYN-9484` in the auto-generated PR title (line 165). | Use a generic title: `"chore: Sync HTML documentation from doc/distrib"`. |
| MAINT-02 | `generate_changelog.yml` | Both `head-ref` and `base-ref` default to `v1.0.0` — silent empty changelog if forgotten. | Remove defaults and set `required: true`. |
| MAINT-03 | `issue_type_predicter.yml` | Workflow/job name uses "Predicter" (incorrect spelling); should be "Predictor". | Rename in workflow name and job name. |
| MAINT-04 | `build_dynamo_core.yml` | Smoke tests are permanently commented out (lines 97–98) with a TODO but no tracking issue. | Link a Jira ticket in the comment or remove the dead block. |
| MAINT-05 | `codeql_analysis.yml` | C# analyzed with `build-mode: none` (autobuild). GitHub recommends `manual` build mode for compiled languages. | Switch to `build-mode: manual` with explicit restore + msbuild steps. |

---

## Phased Remediation Plan

Work is divided into three phases by urgency. Each phase can be a standalone PR.

### Phase 1 — Critical Bugs & Security (Do first; blocks correct CI behavior)

**Goal:** Fix the three silent failures and the highest-risk supply-chain exposure.

| Task | Workflow(s) | Effort |
|------|-------------|--------|
| Fix Linux binary check (`test -f`, remove `.exe`) | `build_dynamo_core.yml` | Trivial |
| Fix milestone null check (`-eq ""` → `-z`) | `auto_cherrypick.yml` | Trivial |
| Fix GitHub API auth in `curl` calls | `Issues_workflow.yml` | Small |
| Pin `tj-actions/changed-files` to a verified SHA | `check_file_size.yml` | Small |
| Add auth header to unauthenticated `curl` | `auto_cherrypick.yml` | Trivial |

Suggested PR title: `DYN-XXXX: Fix critical CI bugs in Linux build check, cherry-pick, and Issues workflow`

---

### Phase 2 — Security Hardening (Do next; reduces attack surface)

**Goal:** Lock down third-party actions and enforce least-privilege permissions.

| Task | Workflow(s) | Effort |
|------|-------------|--------|
| Pin all remaining third-party actions to SHA | `pr_jira_check.yml`, `move_issue.yml`, `Issues_workflow.yml`, `issue_type_predicter.yml`, `dynamo_post_bin_diff.yml` | Small |
| Add `permissions` blocks to all 6 workflows missing them | `pr_description_check.yml`, `check_file_size.yml`, `dynamo_bin_diff.yml`, `dynamo_post_bin_diff.yml`, `close_stale_issues.yml`, `extract_release_notes.yml` | Small |

Suggested PR title: `DYN-XXXX: Harden GitHub Actions — pin third-party actions and add permissions blocks`

---

### Phase 3 — Quality & Consistency (Ongoing; improves reliability and maintainability)

**Goal:** Eliminate inconsistencies, speed up runs, and prevent silent failures going forward.

| Task | Workflow(s) | Effort |
|------|-------------|--------|
| Upgrade `setup-dotnet@v4` → `@v5` | `issue_type_predicter.yml` | Trivial |
| Remove hardcoded `repository: DynamoDS/Dynamo` from PR-triggered checkout steps | `build_dynamo_all.yml`, `build_dynamo_core.yml`, `dynamo_bin_diff.yml` | Small |
| Guard `save_pr_data` jobs with `if: github.event_name == 'pull_request'` | `build_dynamo_all.yml`, `dynamo_bin_diff.yml` | Trivial |
| Remove unnecessary checkout in `pr_description_check.yml` | `pr_description_check.yml` | Trivial |
| Re-enable Go module cache | `validate_agent_skills.yml` | Trivial |
| Reduce `fetch-depth` from `0` to `2` | `auto_update_src_html.yml` | Trivial |
| Remove hardcoded Jira ticket from auto-generated PR title | `auto_update_src_html.yml` | Trivial |
| Fix `generate_changelog.yml` input defaults to prevent empty output | `generate_changelog.yml` | Small |
| Fix "Predicter" → "Predictor" typo in workflow metadata | `issue_type_predicter.yml` | Trivial |
| Link a Jira ticket to commented-out smoke test TODO | `build_dynamo_core.yml` | Trivial |
| Switch CodeQL C# analysis to `build-mode: manual` | `codeql_analysis.yml` | Medium |

Suggested PR title: `DYN-XXXX: GitHub Actions consistency and quality improvements`
