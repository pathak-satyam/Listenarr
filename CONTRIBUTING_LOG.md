# Open Source Contribution Log

## Contribution #1

**Student:** Satyam Pathak
**Member ID:** 104926
**Program:** CodePath AI301 — Summer 2026, Section 1C

**Issue:** [Path mapping: Manually edited path is not used #523](https://github.com/Listenarrs/Listenarr/issues/523)
**Repository:** [Listenarrs/Listenarr](https://github.com/Listenarrs/Listenarr)
**Pull Request:** [#687](https://github.com/Listenarrs/Listenarr/pull/687)
**Status:** Phase III Complete — PR Open, Awaiting Review

---

### Why I Chose This Issue
This issue matched my existing skills in C# and TypeScript. Listenarr is a C#/Vue/TypeScript project, and the bug is isolated to the path mapping form — a well-scoped frontend fix. The issue had no competing PRs and an active maintainer.

### Reproduction Process

#### Environment Setup
Cloned the fork locally and used Claude Code to navigate the codebase. No Docker setup required to trace the bug — identified the root cause through static analysis.

**Branch:** https://github.com/pathak-satyam/Listenarr/tree/fix/523-inline-path-mapping

#### Steps to Reproduce
1. Navigate to Settings → Download Clients
2. Add or edit a download client with remote path mapping
3. Manually type a value into the local path field (do not use the file browser modal)
4. Save — open DevTools → Network → Payload
5. **Expected:** `localPath` contains the typed value
6. **Actual:** `localPath: ""` — empty string sent to backend

### Solution Approach

**Understand:** The local path field in `RemotePathMappingModal.vue` uses `FolderBrowser.vue` with `v-model`. When a user types manually, `FolderBrowser.vue` only emits `path-draft` — never `update:modelValue`. Since `v-model` listens for `update:modelValue`, the typed value never reaches the form.

**Match:** The modal-browse flow already works correctly — `FolderBrowserModal.vue` handles `@path-draft`. The inline input just needed the same commitment to `v-model`.

**Plan:**
1. Add `@input="onInlinePathInput"` to the inline text input in `FolderBrowser.vue`
2. Add `onInlinePathInput()` handler that emits `update:modelValue`, gated on `props.inline && props.autoSelect`
3. Add echo guard on `modelValue` watcher to prevent feedback loop
4. Add unit tests for both cases

**Files changed:**
- `fe/src/components/ui/FolderBrowser.vue`
- `fe/src/__tests__/FolderBrowser.inlineInput.spec.ts` (new)

**Evaluate:** Two unit tests added and passing. Lint and type-check clean.
