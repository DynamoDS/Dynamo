# Dynamo PR Description Template

Use this template to generate PR descriptions for the Dynamo repository.

## Instructions

1. Fill in the template below from the diff and context.
2. PR title format: `DYN-1234: concise change summary` (include Jira key when known).
3. Keep facts verifiable from the diff. Do not invent Jira keys, reviewers, or test results.
4. Call out breaking changes or migration steps explicitly.
5. If unsure about a field, keep the `(FILL ME IN)` placeholder.

---

## Template

```markdown
### Purpose

(Why this PR exists. Include Jira key if known: DYN-1234)

Key changes:
- (bullet per significant change)

### Declarations

Check these if you believe they are true

- [ ] Is documented according to the [standards](https://github.com/DynamoDS/Dynamo/wiki/Coding-Standards)
- [ ] The level of testing this PR includes is appropriate
- [ ] Changes to the API follow [Semantic Versioning](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Versions) and are documented in the [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes) document.

### Release Notes

(One concise sentence based on the diff, or `N/A` when not user-facing.)

### Reviewers

(FILL ME IN) Reviewer 1 (If possible, assign the Reviewer for the PR)

(FILL ME IN, optional) Any additional notes to reviewers or testers.

### FYIs

(FILL ME IN, Optional) Names of anyone else you wish to be notified of
```

---

## Tips

- **Purpose**: Explain the *why*, not just the *what*. Link to the Jira ticket.
- **Declarations**: Only check boxes you've actually verified. The API versioning checkbox is important for any public API changes.
- **Release Notes**: Write from the user's perspective. `N/A` for internal refactoring.
- **Reviewers**: Suggest someone familiar with the changed area.
