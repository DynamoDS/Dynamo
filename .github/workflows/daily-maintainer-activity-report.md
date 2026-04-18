---
description: Daily weekday maintainer email with repository activity summary and blockers
on:
  schedule: daily on weekdays
permissions:
  contents: read
  issues: read
  pull-requests: read
tools:
  github:
    toolsets: [default]
safe-outputs:
  noop:
  jobs:
    send_daily_maintainer_report:
      description: Send the daily activity report email to repository maintainers
      runs-on: ubuntu-latest
      output: Daily maintainer activity report email sent.
      inputs:
        subject:
          description: Email subject line
          required: true
          type: string
        body:
          description: Markdown email body
          required: true
          type: string
      permissions:
        contents: read
      env:
        SMTP_SERVER: ${{ secrets.DAILY_REPORT_SMTP_SERVER }}
        SMTP_PORT: ${{ secrets.DAILY_REPORT_SMTP_PORT }}
        SMTP_USERNAME: ${{ secrets.DAILY_REPORT_SMTP_USERNAME }}
        SMTP_PASSWORD: ${{ secrets.DAILY_REPORT_SMTP_PASSWORD }}
        REPORT_FROM_EMAIL: ${{ secrets.DAILY_REPORT_FROM_EMAIL }}
        MAINTAINERS_EMAILS: ${{ secrets.DAILY_REPORT_MAINTAINERS_EMAILS }}
      steps:
        - name: Extract report payload
          id: payload
          run: |
            item=$(jq -c '.items[] | select(.type == "send_daily_maintainer_report")' "$GH_AW_AGENT_OUTPUT" | head -n 1)
            if [ -z "$item" ]; then
              echo "No send_daily_maintainer_report output was provided."
              exit 1
            fi

            subject=$(echo "$item" | jq -r '.subject')
            body=$(echo "$item" | jq -r '.body')

            if [ -z "$subject" ] || [ "$subject" = "null" ]; then
              echo "Missing subject in send_daily_maintainer_report output."
              exit 1
            fi

            if [ -z "$body" ] || [ "$body" = "null" ]; then
              echo "Missing body in send_daily_maintainer_report output."
              exit 1
            fi

            {
              echo "subject<<__SUBJECT__"
              echo "$subject"
              echo "__SUBJECT__"
              echo "body<<__BODY__"
              echo "$body"
              echo "__BODY__"
            } >> "$GITHUB_OUTPUT"
        - name: Send report email
          env:
            REPORT_SUBJECT: ${{ steps.payload.outputs.subject }}
            REPORT_BODY: ${{ steps.payload.outputs.body }}
          run: |
            python - <<'PY'
            import os
            import ssl
            import smtplib
            from email.message import EmailMessage

            message = EmailMessage()
            message["Subject"] = os.environ["REPORT_SUBJECT"]
            message["From"] = os.environ["REPORT_FROM_EMAIL"]
            message["To"] = os.environ["MAINTAINERS_EMAILS"]
            message.set_content(os.environ["REPORT_BODY"])

            try:
                server = smtplib.SMTP(
                    os.environ["SMTP_SERVER"],
                    int(os.environ["SMTP_PORT"]),
                    timeout=30,
                )
                server.starttls(context=ssl.create_default_context())
                server.login(os.environ["SMTP_USERNAME"], os.environ["SMTP_PASSWORD"])
                server.send_message(message)
                server.quit()
            except Exception as error:
                raise RuntimeError(
                    f"Failed to send daily maintainer report email ({type(error).__name__})."
                ) from error
            PY
---

# Daily Maintainer Activity Report

You produce a concise daily status report for maintainers covering repository activity from the previous 24 hours.

## Your task

1. Gather all issues created in the last 24 hours.
2. Gather all pull requests merged in the last 24 hours.
3. Identify current open blockers:
   - Open issues with labels like `blocker`, `blocked`, `critical`, or `priority:high`
   - Open pull requests with labels like `blocked` or `do-not-merge`
   - Treat label matching as case-insensitive exact label-name matching after normalization (trim whitespace and lowercase only)
4. Attribute bot activity to the humans behind it whenever possible:
   - If a PR was authored by a bot account, credit humans in this order: merger, reviewer, assignee, then triggering actor.
   - Present automation as team leverage, not independent actors.

## Output requirements

- If there is meaningful activity, call `send_daily_maintainer_report` exactly once with:
  - `subject`: A clear subject like `Daily Repository Report - 2026-04-18` using UTC ISO date format
  - `body`: GitHub-flavored markdown containing:
    - `### New Issues`
    - `### Pull Requests Merged`
    - `### Open Blockers`
    - `### Notes`
- Keep the report concise and actionable.
- Include links to issues and pull requests.
- If no new issues, no merged pull requests, and no blockers are found, call `noop` with a message saying there is no reportable activity.

## Quality guidelines

- Focus on signal over noise.
- Avoid speculation.
- Prefer direct facts from repository metadata.
- If data is incomplete, mention what could not be verified.
