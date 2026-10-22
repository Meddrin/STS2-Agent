# Evidence

## 1. docs tracking drift (confirmed)

- `.gitignore:15` = `docs/`; `.gitignore:17` = `AGENTS.md` (the latter is indeed a local file).
- `git status --porcelain --ignored docs`:
  ```
  !! docs/model-compatibility-matrix.md
  !! docs/phase-6-validation-2026-03-30-dynamic-card-values.md
  ```
- `docs/**/*.md` on disk = 33; `git ls-files docs` = 44 (including non-md).
- The local preflight's doc-marks output really contains `docs/phase-6-validation-2026-03-30-dynamic-card-values.md marked as a snapshot`,
  and that file is not checked in => CI's fresh checkout will not check it.
- `rg --no-ignore -F 'model-compatibility-matrix'` has no hits: that document is currently discovered only by directory enumeration.

## 2. Outdated spec (confirmed)

- The description at `.trellis/spec/operations/validation-and-release.md:12` has only four gates (and lacks api-facts),
  and the `--only lockfile|api-doc|doc-marks` candidate string is outdated (actually five choices).
- The "Release metadata" section of the same file says "three files", while `AGENTS.md:120` and `check_release_metadata.py` are already at five.

## 3. Orphan self-test (confirmed)

- `scripts/sts2-model-budget-proxy-selftest.py`: the only strictly zero-reference script in the repo (multiple `rg` searches found no referrer).
- Measured on this machine: `exit=0 elapsed=4.19s`, with output containing check items such as `{"name":"real_ledger_untouched","ok":true}` and
  `{"name":"http_limit","ok":true}` => offline, zero cost, can enter CI.
- Its host `sts2-model-budget-proxy.py` is referenced only by `sts2-coop-full-run-acceptance.ps1:34` (manual real-machine orchestration).

## 4. Packaging lacks a version guard (confirmed)

- `rg -n 'preflight|check_release_metadata|check_verification_gates' scripts/package-release.ps1` has no hits;
  `scripts/preflight-release.ps1` and `package-release.ps1` do not reference each other.
- `check_release_metadata.py` validates that five files are consistent (mod_manifest / mod_id / Router / pyproject / uv.lock),
  but runs only in CI (`validate.yml:30-32`) and local preflight (`:96`).

## Deliberately not done

- Do not delete `mcp_server/data/eng/` or scripts: a product decision.
- Do not add mod main project compilation to CI: CI has no `sts2.dll` / `GodotSharp.dll`.
