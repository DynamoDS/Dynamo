#!/bin/bash
# Validates all skill directories changed in a pull request.
#
# Usage: validate-skills.sh <base-ref> [skills-dir]
#   base-ref    The base branch name to diff against (e.g. "main").
#               When omitted or when the remote is unavailable, all skills are validated.
#   skills-dir  The directory containing skill subdirectories (default: "skills").
#
# Exit codes:
#   0  All validated skills passed.
#   1  One or more skills failed validation.

# -e is intentionally omitted: all error paths are handled explicitly,
# so abort-on-error would conflict with the || FAILED=1 accumulator pattern.
set -uo pipefail

BASE_REF="${1:-}"
SKILLS_DIR="${2:-skills}"

# Strip trailing slash for consistent path handling.
SKILLS_DIR="${SKILLS_DIR%/}"

# Find unique skill directories containing files changed in this PR.
# The three-dot diff requires fetch-depth: 0 and a properly configured remote,
# which is always the case on GitHub Actions but may not be in local act runs.
changed_skills=()
if [ -n "$BASE_REF" ]; then
  mapfile -t changed_skills < <(git diff --name-only "origin/${BASE_REF}...HEAD" -- "$SKILLS_DIR/" \
    2>/dev/null \
    | sed "s|^${SKILLS_DIR}/||" \
    | cut -d'/' -f1 \
    | sort -u \
    | grep -v '^$')
fi

if [ "${#changed_skills[@]}" -eq 0 ]; then
  # Fallback: validate all skill directories (e.g. when git remote is unavailable
  # in local act testing, or when a PR only deletes files with no remaining dirs).
  echo "Could not determine changed skills from git diff; validating all skills."
  mapfile -t changed_skills < <(find "$SKILLS_DIR" -mindepth 1 -maxdepth 1 -type d -exec basename {} \; | sort -u)
fi

if [ "${#changed_skills[@]}" -eq 0 ]; then
  echo "No skill directories found in ${SKILLS_DIR}/, skipping validation."
  exit 0
fi

FAILED=0
for skill in "${changed_skills[@]}"; do
  # Skip skills whose directories were deleted in this PR.
  if [ ! -d "${SKILLS_DIR}/$skill" ]; then
    echo "Skipping deleted skill: $skill"
    continue
  fi

  # Run validation with markdown output so the result is written to the job
  # summary in one pass. --emit-annotations works with any output format, so
  # inline PR annotations are still emitted alongside the markdown report.
  # We use process substitution to:
  # 1. Send all output (including ::error commands) to stdout for GitHub Actions
  # 2. Filter out ::error/::warning/::notice lines before writing to the summary
  skill-validator check --strict --emit-annotations -o markdown "${SKILLS_DIR}/$skill/" \
    | tee >(grep -v '^::' >> "${GITHUB_STEP_SUMMARY:-/dev/null}") || FAILED=1
done

if [ $FAILED -ne 0 ]; then
  echo ""
  echo "Skill validation failed!"
  echo ""
  echo "See the Job Summary for detailed validation results:"
  echo "  https://github.com/$GITHUB_REPOSITORY/actions/runs/$GITHUB_RUN_ID"
  echo ""
fi

exit $FAILED
