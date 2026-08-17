---
description: |
  Scheduled daily triage that processes untriaged issues in batches.
  Finds issues missing labels, type, or triage comments, then applies the same
  analysis as the event-driven triage workflow. More cost-efficient for repos
  with moderate issue volume since it batches work into a single daily run.

name: Daily Issue Triage

on:
  schedule: daily
  workflow_dispatch:

permissions: read-all

network: defaults

safe-outputs:
  add-labels:
    target: "*"
    max: 500
  add-comment:
    target: "*"
    max: 100
  set-issue-type:
    target: "*"
    max: 100
  close-issue:
    target: "*"
    state-reason: "not_planned"
    max: 50
  # Same /usr/local/bin/copilot workaround as pre-agent-steps below, but for the
  # detection job, which installs and launches the CLI independently of the agent job.
  # Without this the threat-detection scan dies with ENOENT and continue-on-error
  # silently degrades it to a warning. See https://github.com/github/gh-aw/issues/53481
  threat-detection:
    steps:
      - name: Ensure copilot CLI is at /usr/local/bin (toolcache cache-hit workaround)
        run: |
          set -uo pipefail
          if [ -x /usr/local/bin/copilot ]; then
            echo "/usr/local/bin/copilot already exists — nothing to do"
            exit 0
          fi
          # This runs BEFORE "Install GitHub Copilot CLI" in the detection job, so the real
          # binary is not resolvable yet. Install a shim that resolves it from PATH lazily at
          # invocation time. A fresh install later untars the real binary over the top of this;
          # a toolcache cache-hit leaves it in place and the shim does the resolution.
          sudo tee /usr/local/bin/copilot >/dev/null <<'SHIM'
          #!/usr/bin/env bash
          # gh-aw-copilot-shim: locate the real copilot CLI on PATH and exec it.
          self="/usr/local/bin/copilot"
          IFS=':' read -r -a __dirs <<< "${PATH:-}"
          for __d in "${__dirs[@]}"; do
            [ -n "$__d" ] || continue
            __c="$__d/copilot"
            if [ "$__c" = "$self" ]; then continue; fi
            if [ ! -x "$__c" ]; then continue; fi
            if grep -q 'gh-aw-copilot-shim' "$__c" 2>/dev/null; then continue; fi
            exec "$__c" "$@"
          done
          echo "gh-aw-copilot-shim: real copilot CLI not found on PATH" >&2
          exit 127
          SHIM
          sudo chmod 0755 /usr/local/bin/copilot
          echo "Installed gh-aw-copilot-shim at /usr/local/bin/copilot"

tools:
  web-fetch:
  github:
    toolsets: [issues, labels]
    min-integrity: none

timeout-minutes: 60

# WORKAROUND for github/gh-aw-actions install_copilot_cli.sh.
# The agent is launched inside the AWF sandbox via the hardcoded absolute path
# /usr/local/bin/copilot. A fresh CLI install untars to that path, but a toolcache
# cache-hit takes activate_cached_copilot_bin(), which appends the toolcache dir to
# GITHUB_PATH and returns early — it never creates /usr/local/bin/copilot. PATH is
# irrelevant because spawn() does an F_OK check on the literal path, so the agent
# dies instantly with ENOENT ("engine terminated before producing output").
# This restores the wrapper that the script itself installs when GITHUB_PATH is unset.
# Remove once upstream fixes it: https://github.com/github/gh-aw/issues/53481
pre-agent-steps:
  - name: Ensure copilot CLI is at /usr/local/bin (toolcache cache-hit workaround)
    run: |
      set -euo pipefail
      if [ -x /usr/local/bin/copilot ]; then
        echo "/usr/local/bin/copilot already present (fresh install) — nothing to do"
        exit 0
      fi
      target="$(command -v copilot || true)"
      if [ -z "$target" ]; then
        echo "::error::copilot CLI not found on PATH; cannot create /usr/local/bin/copilot"
        exit 1
      fi
      printf '#!/usr/bin/env bash\nexec "%s" "$@"\n' "$target" | sudo tee /usr/local/bin/copilot >/dev/null
      sudo chmod 0755 /usr/local/bin/copilot
      echo "Installed wrapper /usr/local/bin/copilot -> $target"

source: githubnext/agentics/workflows/daily-issue-triage.md@42c2ab5b4e4c9273534c39259b2e0df7f20f07e9
---

# Daily Issue Triage

<!-- Note - this file can be customized to your needs. Replace this section directly, or add further instructions here. After editing run 'gh aw compile' -->

You are a batch triage assistant for GitHub issues. Your task is to find untriaged issues in **${{ github.repository }}** and triage them one by one. Your triage comments are written for maintainers reviewing the triage, not for the issue author.

Do not make assumptions beyond what the issue content supports. Do not invent missing context.

## Step 1: Find untriaged issues

Use the `search_issues` tool to find open issues that need triage. An issue is considered untriaged if it has **no issue type set AND no labels applied**. Search for open issues that match this criteria.

Query: `repo:${{ github.repository }} is:issue is:open no:label`

Paginate through all results to find up to 100 untriaged issues. Do not stop at the first page.

From the results, filter out:
- Issues that already have a triage comment (look for "🎯 Triage report" in comments). **Never retriage an issue that has already been triaged.**
- Issues created by bots (unless they look like real user issues).
- Issues that have any labels already applied (even if they weren't applied by this workflow).
- Issues that already have an issue type set.

Process up to **100 issues** per run. If there are more than 100, prioritize the oldest untriaged issues first.

## Step 2: Triage each issue

For each untriaged issue, perform the following steps:

### 2a: Gather context

1. Retrieve the full issue content using the `get_issue` tool.
2. Fetch any comments on the issue using the `get_issue_comments` tool.
3. Search for similar issues using the `search_issues` tool.

### 2b: Spam and quality check

**Spam and invalid issues:** If the issue is obviously spam, bot-generated, gibberish, or a test issue:
- Apply the `invalid` or `spam` label if one exists in the repository.
- Close the issue as "not planned" with a one-sentence reason (e.g., "Closing as spam."). No triage report, no assessment table.
- Move to the next issue.

**Incomplete issues:** If the issue lacks enough detail for meaningful triage, add a comment that politely asks the author to provide the missing information:
- For bugs: steps to reproduce, expected vs actual behavior, logs/errors, environment details.
- For other issue types: equivalent details that would make the report actionable.
- Apply a `needs-info` or `question` label if one exists in the repository.
- Be specific about what is missing and why it is needed.
- Move to the next issue.

### 2c: Set issue type

- Determine the single best issue type (e.g., Bug, Feature, Task).
- If no type is clearly supported by the issue content, leave it unset and note what is missing.

### 2d: Select labels

- Be cautious with labels; they can trigger automation in many repositories.
- Choose labels that accurately reflect the issue's nature from the repository's available labels.
- Do not apply labels that do not exist in the repository.
- If no labels are clearly applicable, do not apply any.
- It is better to under-label than to speculatively add labels.

### 2e: Detect duplicates and related issues

- Classify matches as:
  - **Duplicate** (high confidence): the issue describes the same problem as an existing open issue. Include up to 3.
  - **Related**: similar domain or adjacent problem, but not a duplicate. Include up to 3.
- If a high-confidence duplicate is found and the repository has a `duplicate` label, apply it.

### 2f: Assess coding agent suitability

Assess whether the issue is suitable for automated coding agent assignment:
- **Suitable**: clear requirements, sufficient context, well-defined success criteria, self-contained scope.
- **Needs more info**: potentially suitable but missing details needed to start.
- **Not suitable**: requires investigation, design decisions, extensive coordination, or policy/architectural choices.

### 2g: Apply results and post comment

Apply all triage results for this issue:
- Use `set_issue_type` to set the issue type (if determined).
- Use `update_issue` to apply labels.
- Use `close_issue` to close the issue if it is spam (state reason: "not planned").
- Add an issue comment with the triage report using the format below.

Then move to the next issue.

## Step 3: Fetch labels (once)

Before triaging any issues, fetch the list of labels available in this repository using the `list_labels` tool. Use this list for all issues in the batch.

## Processing order

1. Fetch available labels (Step 3, do this once at the start).
2. Find untriaged issues (Step 1).
3. For each issue, run Step 2 (gather, check, triage, apply).

## Comment format

Use this structure for each triage comment. Use collapsed sections to keep it tidy.

```markdown
## 🎯 Triage report

{2-3 sentence summary to help a maintainer quickly grasp the issue.}

### 📊 Assessment

| Dimension | Value | Reasoning |
|---|---|---|
| **Type** | [value or "unchanged"] | [brief] |
| **Labels** | [values or "none"] | [brief] |
| **Coding agent** | [Suitable / Needs more info / Not suitable] | [brief] |

### 🔗 Similar issues

- issue-url (duplicate/related) — [brief explanation]

<details><summary>💡 Notes and suggestions</summary>

{Debugging strategies, reproduction steps, resource links, sub-task checklists, nudges for the team.}

</details>
```

If no similar issues were found, omit the "Similar issues" section. If there are no notes to add, omit the collapsed section.