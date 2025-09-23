# CI/CD Monitoring and Alerting Documentation

## Overview
This document describes how CI/CD pipeline monitoring, alerting, and visualization are set up for the lector-library project.

---

## 1. Pipeline Monitoring
- All pipeline stages for both backend and frontend are tracked using GitHub Actions.
- Each workflow run is logged and visible in the GitHub Actions tab.

## 2. Alerting via Jira
- On any pipeline failure, a Jira issue is automatically created in the configured project.
- The issue contains details about the failure, including a link to the failed workflow run.
- To configure Jira integration, set the following repository secrets:
  - `JIRA_BASE_URL`
  - `JIRA_USER_EMAIL`
  - `JIRA_API_TOKEN`

## 3. Logging
- All pipeline executions and their statuses (success/failure) are logged in GitHub Actions.
- You can view logs for each run in the Actions tab.

## 4. Dashboard
- The real-time status of the pipeline is visible in the GitHub Actions tab.
- You can embed status badges and workflow links in Confluence for team visibility.

## 5. Automated Retries
- Transient failures in test steps are automatically retried up to 3 times.
- This is handled by the `nick-invision/retry` GitHub Action.

## 6. How to Access and Interpret
- **Dashboard:** Go to the GitHub Actions tab for this repository.
- **Logs:** Click any workflow run to see detailed logs.
- **Alerts:** Check Jira for automatically created issues on failures.
- **Manual Retry:** You can re-run failed jobs from the Actions tab.

---

## 7. Example Workflow Snippet (Backend)
```yaml
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - name: Restore
        run: dotnet restore backend/Csp.Api
      - name: Build
        run: dotnet build --configuration Release --no-restore backend/Csp.Api
      - name: Test with Retry
        uses: nick-invision/retry@v2
        with:
          timeout_minutes: 10
          max_attempts: 3
          command: dotnet test --no-build --verbosity normal
      - name: Create Jira Issue on Failure
        if: failure()
        uses: atlassian/gajira-create@v3
        with:
          project: YOURPROJECTKEY
          issuetype: Bug
          summary: "CI/CD pipeline failed for ${{ github.repository }} on ${{ github.ref }}"
          description: |
            The pipeline failed at ${{ github.workflow }}.
            See details: ${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}
        env:
          JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
          JIRA_USER_EMAIL: ${{ secrets.JIRA_USER_EMAIL }}
          JIRA_API_TOKEN: ${{ secrets.JIRA_API_TOKEN }}
```

---

## 8. References
- [GitHub Actions](https://docs.github.com/en/actions)
- [Jira GitHub Action](https://github.com/marketplace/actions/create-jira-issue)
- [Retry Action](https://github.com/marketplace/actions/retry-step)

---

For questions or help, contact the DevOps team.
