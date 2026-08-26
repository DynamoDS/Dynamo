# Publishing to the Dynamo Wiki on Windows

## Why this exists

The `DynamoDS/Dynamo.wiki` repo contains a page named
`Content-Pattern:-Node-Descriptions.md` — the colon is invalid in Windows filenames. This
breaks **any** git operation that tries to validate or materialize the full index or
working tree: a normal `git clone`, `git checkout`, `git add -A`, `git reset`, all fail
with `error: invalid path 'Content-Pattern:-Node-Descriptions.md'`.

The only safe way to edit a single file in this repo on Windows is git plumbing that
operates purely on object SHAs and never touches the working tree or a full index.

## The recipe

1. **Clone without checkout** — fetches all objects without materializing the working
   tree, so the invalid filename never has to be written to disk:
   ```
   git clone --no-checkout https://github.com/DynamoDS/Dynamo.wiki.git
   ```
2. **Extract just the target file** from the current tip, without touching anything else:
   ```
   git show HEAD:Release-Notes.md > Release-Notes.md
   ```
3. **Edit the extracted file** with your normal editor/tooling, then create a new blob
   for it:
   ```
   git hash-object -w Release-Notes.md
   ```
4. **Build a new tree** with only that one entry changed — list the current tree, swap in
   the new blob SHA for the target path, and feed the result to `mktree`:
   ```
   git ls-tree HEAD > tree.txt
   # edit tree.txt: replace the old blob SHA for Release-Notes.md with the new one
   git mktree < tree.txt
   ```
5. **Verify the tree diff is minimal** before committing anything — this is the step that
   catches problems before they become commits:
   ```
   git diff-tree -r HEAD^{tree} <new-tree-sha>
   ```
   This must show exactly one changed entry (the target file). If it shows anything else
   — especially a large number of entries reported as deleted — stop. Do not proceed to
   `commit-tree`.
6. **Create the commit** directly from the tree, bypassing the index entirely:
   ```
   git commit-tree <new-tree-sha> -p HEAD -m "Update Release-Notes.md"
   ```
7. **Push the raw commit SHA** — never checkout the branch first:
   ```
   git push origin <new-commit-sha>:refs/heads/master
   ```
8. **Verify live.** Fetch the raw page after the push and diff it against the pre-push
   content to confirm only the intended lines changed:
   ```
   curl -s https://raw.githubusercontent.com/wiki/DynamoDS/Dynamo/Release-Notes.md
   ```

## The `--no-checkout` index gotcha — read this before improvising

Cloning with `--no-checkout` leaves the git **index empty** — it is not populated from
HEAD. If you create a file directly in the working tree after this clone and then run
`git add`/`git commit` normally (instead of the plumbing above), the resulting commit is
built from the near-empty index, not from HEAD — meaning it would show **every other
tracked file in the repo as deleted**.

This is not a hypothetical: it happened mid-session while publishing the 4.2.0 notes,
and was only caught because `git status` was run (per standing habit) before staging.
`git status` showed the entire wiki — ~100+ files — staged as deleted, with only the
newly created `Release-Notes.md` untracked. The obvious fix, `git reset` to repopulate
the index from HEAD, **also fails** with the same invalid-path error — confirming no
whole-index operation works on this repo on Windows at all. The plumbing recipe above is
the only path that avoids this class of failure entirely, because it never populates or
relies on the index.

**Always run `git status` immediately before the `commit-tree` step**, even when
following this recipe exactly. If it shows anything other than a clean, empty status (no
staged/unstaged changes reported at all — the plumbing recipe never touches the index or
working tree state git status reports on), stop and re-derive the tree diff before
proceeding.

## Confirm before pushing

A GitHub wiki repo has no PR or review step. `git push` to its `master` branch is
immediately live and public — unlike the normal PR-based workflow for the product repo
itself. Always get explicit confirmation from the requester before the push in step 7,
and say plainly that this action is immediate and public when asking.
