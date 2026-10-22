# Execution checklist (four workers, with non-overlapping write scopes)

## W1: docs tracking consistency

Write scope: `.gitignore`, `docs/model-compatibility-matrix.md`, `docs/phase-6-validation-2026-03-30-dynamic-card-values.md`,
`scripts/check_verification_gates.py`, `scripts/test-verification-gates.ps1`.

1. Delete the `docs/` line in `.gitignore` (keeping the other entries of the same section), and confirm that after the deletion `git status` shows only those two extra documents.
2. Check the two documents in with `git add` (`-f` is no longer needed).
3. Add a gate to `check_verification_gates.py` (name up to you, `docs-tracked` suggested):
   - enumerate `docs/**/*.md` and judge one by one with `git ls-files --error-unmatch <path>` (or get a set with one `git ls-files -z -- docs`) whether it is tracked;
   - untracked files must be listed one by one in the error message, with the hint "docs/ is no longer ignored, please git add";
   - when the repo root has no `.git` (for example a source tarball), **skip and print an explanation**, which does not count as a failure;
   - add it to the `GATES` dict and to the gate list in the file header docstring.
4. Add a case to `test-verification-gates.ps1`: put a docs/*.md in the fixture but do **not** track it -> the gate turns red;
   after restoring it turns green. Note that this script contains non-ASCII and **must keep the UTF-8 BOM** (the script-encoding gate checks this),
   and the fixture must be recognizable by `git`: if the fixture is not a git repository the gate takes the "skip" branch,
   so this case must either initialize a temporary git repo for the fixture (`git init` + `git add` of the tracked files),
   or be written in a form that "verifies on the real repo"; go by the simplest reliable approach at implementation time, and explain the choice in the report.

## W2: operations spec and AGENTS refresh

Write scope: `.trellis/spec/operations/validation-and-release.md`, `AGENTS.md` (the latter is a **gitignored local file**,
changes to it stay only in the working tree, **never `git add -f`**).

1. Gate table: complete the description on the `check_verification_gates.py` row to the current five gates
   (lockfile / api-doc / api-facts / doc-marks / script-encoding), and refresh the `--only` candidate string in sync.
   Before implementing, run `python scripts/check_verification_gates.py` and `--help` once to confirm the actual names.
2. "Release metadata" section: three places -> **five files**, and point to the actual validation scope of `scripts/check_release_metadata.py`
   (mod_manifest.json / mod_id.json / Router.cs / pyproject.toml / uv.lock).
3. Add a small section "Script inventory" (after Offline checks):
   - list the **offline-runnable check entry points** (one command each + a one-line purpose);
   - list the **deliberately unwired scripts** with reasons (`scan-assembly-strings.ps1` is documented in `docs/reverse-engineering.md`;
     `generate-sts2-knowledge.ps1` is the regenerator of its product docs/game-knowledge/*.md;
     `sts2-coop-full-run-acceptance.ps1` / `test-coop-play-together.ps1` are manual acceptance orchestration needing a real machine;
     `sts2-validation-secrets.ps1` serves only the former).
     Each needs `file:line` evidence and a note that "these are not dead code, they are manual/real-machine entry points".
4. `AGENTS.md`: write the new offline self-test entry (the one wired in W3) into the scripts description paragraph, keeping the existing layout.

## W3: wire the orphan self-test into preflight and CI

Write scope: `scripts/preflight-release.ps1`, `.github/workflows/validate.yml`,
`scripts/check_release_package.py` (only when its semantics are confirmed suitable).

1. First confirm that `python scripts/sts2-model-budget-proxy-selftest.py` is offline, fast, and exits 0 (measured 4.2s on this machine).
2. `preflight-release.ps1`: add one step in the existing `Invoke-Step -Name "..." -Action { ... }` style
   (suggested name "Check the model budget proxy (no-cost self-test)"), placed after the Python-related steps.
   **Keep the script's UTF-8 BOM**.
3. `.github/workflows/validate.yml`: add one step running the same command (consistent with the shell/working directory style of existing steps).
4. `check_release_package.py`: read the semantics of `SOURCE_FILES` (source contract = must exist in the source tree).
   **Only when confirmed that it does not require the file to enter the release artifact** add the self-test script to it; if the two semantics are coupled, do not change that file,
   and explain the reason in the report. Either way, ensure that "deleting this self-test script turns some gate red".

## W4: version guard before packaging

Write scope: `scripts/package-release.ps1`.

1. Before the script starts building, call `python scripts/check_release_metadata.py` (or an equivalent inline check),
   abort on failure with a readable reason (for example "versions inconsistent, run preflight-release.ps1 first").
2. Do not change other script behavior (artifact structure, zip names, the `check_release_package.py --artifact` call all untouched).
3. Falsifiable demonstration: temporarily change the version in `mcp_server/pyproject.toml` by one digit -> running the packaging script should fail immediately and **produce no zip**;
   run it once more after restoring to confirm recovery (if a full package needs closing the game or takes too long, use `-WhatIf`/an equivalent or run only up to the guard step,
   and explain the demonstration method in the report).

## Common requirements

- Do not `git add/commit/push` (the main agent commits uniformly).
- Run the gates for real and paste the output; assertions must be falsifiable (break -> red -> restore).
- Report: absolute paths of changed files, acceptance evidence, and items that are live-only/unproven.
