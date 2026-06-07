# Open Source Contribution Log

## Contribution #1

**Student:** Satyam Pathak
**Member ID:** 104926
**Program:** CodePath AI301 — Summer 2026, Section 1C

**Issue:** [Path mapping: Manually edited path is not used #523](https://github.com/Listenarrs/Listenarr/issues/523)
**Repository:** [Listenarrs/Listenarr](https://github.com/Listenarrs/Listenarr)
**Status:** Phase I In Progress

---

### Why I Chose This Issue

This issue matched my existing skills in C# and TypeScript. Listenarr is a C#/Vue/TypeScript project, and the bug is isolated to the path mapping form — a well-scoped frontend/backend interaction that I can realistically fix. I wanted to contribute to a real project with an active maintainer and no competing PRs, and this fit all those criteria.

### Understanding the Issue

When adding a remote path mapping for a download client, users can either type a local path manually or select one via a file browser modal. However, when the path is typed manually, that value is never sent to the backend — the request payload shows `localPath: ""` regardless of what was typed. Only paths selected through the modal are correctly passed through.

### Reproduction Steps

1. Navigate to Settings → Download Clients
2. Add or edit a download client with remote path mapping
3. Manually type a value into the local path field (do not use the file browser)
4. Save — observe the network payload shows `localPath: ""`

### Solution Approach

Read the manually entered input value and include it in the backend query, rather than only using the value set by the file browser modal.
