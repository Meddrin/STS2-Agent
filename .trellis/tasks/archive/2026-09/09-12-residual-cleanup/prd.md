# Close out the last few leftovers

## Background

After the previous round's wrap-up, four **confirmed small issues that need no product decision** remain:

1. **Tracking drift of `docs/`**: `.gitignore:15` ignores `docs/`, while 31 docs/*.md files were force-added into the repo.
   The working tree has 33 .md files, **two not checked in** (`git status --ignored docs` shows `!!`):
   `docs/model-compatibility-matrix.md` (the current note of 2026-09-08) and
   `docs/phase-6-validation-2026-03-30-dynamic-card-values.md` (a validation record carrying a historical snapshot marker).
   Consequence: `git add .` will not pick up new docs; the local preflight's doc-marks checks
   `phase-6-...-dynamic-card-values.md` (it appears in the measured output) while CI's fresh checkout cannot see that file:
   **the same gate checks a different set locally and on CI**.
2. **The spec gate list is outdated**: `.trellis/spec/operations/validation-and-release.md:12` lists only
   lockfile / api-doc / doc-marks, missing the `api-facts` added last round and the existing `script-encoding`;
   the `--only` candidate string is likewise outdated. The "Release metadata" section of the same file still says "three files", while it is now five.
3. **The orphan self-test has never run**: `scripts/sts2-model-budget-proxy-selftest.py` is the only strictly zero-reference script in the repo,
   but it is exactly the **zero-cost offline self-test** of `sts2-model-budget-proxy.py` (the cost-gating component of the full two-instance acceptance):
   measured offline, 4.2 seconds, exit 0, with checks including `real_ledger_untouched`. It has never been called by preflight or CI.
4. **Packaging lacks a version guard**: `scripts/package-release.ps1` does not call `check_release_metadata.py`, nor
   `preflight-release.ps1`. That is, as long as preflight has not been run by hand first, a release package may be built with five inconsistent version numbers.

## Goals

1. Make `docs/` a controlled directory: remove the ignore, check in the two untracked documents, and add a gate
   "every docs/**/*.md on disk must be tracked by git" (locally it finds "a new document quietly not checked in", and on CI it automatically degrades to always-true).
2. Update the operations spec: add `api-facts` / `script-encoding` to the gate table, refresh the `--only` list,
   change version sync to "five files", and add a script inventory section (offline check entry points + deliberately unwired scripts and reasons).
3. Wire `sts2-model-budget-proxy-selftest.py` into preflight and CI so it goes from an orphan to a covered asset;
   if it is deleted, a gate must turn red (it must not vanish silently).
4. `package-release.ps1` does a version consistency guard before the build starts, aborting on failure with a readable reason.

## Acceptance criteria

- [x] `git status --ignored docs` no longer shows ignored docs files; both documents are checked in.
- [x] The new gate turns red for "docs/*.md that exist on disk but are not tracked by git" and green for the normal state; `--only` can select it alone;
      in a non-git environment (no `.git`) it skips instead of crashing (with an explanation line).
- [x] `scripts/test-verification-gates.ps1` adds one self-test case for the new gate, and still passes entirely.
- [x] The spec's gate table, `--only` list, and version file count match the current implementation; the script inventory is accurate (each with file:line).
- [x] The preflight output has a step running the orphan self-test and it PASSes; `.github/workflows/validate.yml` runs it too.
- [x] `package-release.ps1` **fails first** when versions are inconsistent (demonstrable offline: temporarily change one version number -> the packaging script errors out immediately and produces no zip; restored afterwards).
- [x] Full gates pass: C# unit tests, MCP unit tests, `check_verification_gates.py`, `check_release_package.py`, preflight 350+ PASS / 0 FAIL.

## Out of scope

- **Do not delete** `mcp_server/data/eng/`, **do not delete** any script (deletion is a product decision, not yet decided by the user).
- Do not change the action contract block of `docs/api.md` or other contracts already protected by gates.
- Do not add mod main project compilation to CI (CI has no `sts2.dll` / `GodotSharp.dll`; the local preflight already covers that step).
