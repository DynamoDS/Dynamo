# CI/CD Rules

Durable constraints for all GitHub Actions work in the Dynamo repository. These rules apply
when writing, reviewing, or modifying any file under `.github/workflows/`.

For the full issue inventory, fix snippets, and phased remediation plan, see
[`knowledge/github-actions-audit.md`](../knowledge/github-actions-audit.md).

## Shell Correctness

- **Never use `test "<string>"` to check file existence** — always use `test -f "<path>"` or
  `[ -f "<path>" ]`. A bare `test "<string>"` evaluates whether the string is non-empty, which
  is always true for a literal path string.
- **Never use `-eq` to compare shell strings** — use `==` or `-z`/`-n` inside `[[ ]]`. The `-eq`
  operator is arithmetic and silently misbehaves on non-numeric values.

## GitHub API Authentication

- **Never authenticate GitHub API `curl` calls with `-u admin:TOKEN`** — this format is rejected
  by GitHub. Use the header form instead:
  ```bash
  curl -H "Authorization: token ${{ secrets.GITHUB_TOKEN }}" ...
  ```
- **Always add `--fail` (or `-f`) to `curl` calls** that must succeed — without it, HTTP 4xx/5xx
  responses are silently treated as success.

## Third-Party Actions

- **Never add a third-party action without pinning it to a full commit SHA.** Floating tags
  (`@v2`, `@latest`) can be silently updated with malicious content. Use:
  ```yaml
  uses: owner/action@<full-sha>  # vX.Y.Z
  ```
- **`tj-actions/changed-files` has a documented supply-chain compromise history** — treat it
  with extra scrutiny and keep the SHA pinned and reviewed on every update.

## Permissions

- **Always declare a `permissions` block on every job.** Omitting it inherits the repo default,
  which is often broader than needed. Start from nothing and add only what the job requires:
  ```yaml
  permissions:
    contents: read
    pull-requests: write
  ```

## Unnecessary Work

- **Never check out the repository in a step that only reads `github.event` context.** Checkout
  costs ~15 seconds and downloads the entire repo; if the step only uses event payload data
  (e.g., `github.event.pull_request.body`), skip it entirely.

## Auto-Generated Content

- **Never hardcode Jira ticket numbers in auto-generated output** (PR titles, commit messages,
  bot comments). Ticket references baked into generated content become stale immediately and
  mislead future readers. Use generic descriptive text instead.
