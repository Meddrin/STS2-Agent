# Evidence

## Deliverables

| File | Change |
|---|---|
| `.gitignore` | Deleted the `docs/` line (the other entries of the same section kept): docs is from now on a version-controlled directory |
| `docs/model-compatibility-matrix.md`, `docs/phase-6-validation-2026-03-30-dynamic-card-values.md` | Checked in (previously ignored and floating outside version control) |
| `scripts/check_verification_gates.py` | Added the 6th gate `docs-tracked` (a set from `git ls-files -z -- docs`; skipped with an explanation when there is no `.git`) |
| `scripts/test-verification-gates.ps1` | Added 3 self-tests (rejects untracked / accepts fully tracked / skips when there is no .git) + the `Invoke-Git` and `Remove-Fixture` helpers |
| `.trellis/spec/operations/validation-and-release.md` | The gate table filled out to 6 gates, the `--only` list refreshed, version sync changed to five files, added `## Script inventory` (including deliberately unwired scripts and the reasons), and fixed 5 drifted line anchors |
| `.trellis/spec/operations/index.md` | "three version sources" -> five, and points to `check_release_metadata.py` and the packaging guard |
| `AGENTS.md` (gitignored local file) | The scripts section and the pre-release check section gain the new offline self-test entry |
| `scripts/preflight-release.ps1` | Added the step "Check the model budget proxy (no-cost self-test)" |
| `.github/workflows/validate.yml` | Added a step with the same command (steps 12 -> a new entry among the 12 steps) |
| `scripts/check_release_package.py` | `SOURCE_FILES` includes this self-test script (`SOURCE_FILES` semantics = must exist in the source tree, need not ship; noted in the comment) |
| `scripts/package-release.ps1` | Added an `Assert-ReleaseMetadataConsistent` guard before the build (+19 lines, purely additive) |
| `scripts/sts2-model-budget-proxy-selftest.py` | **Fixed the harness's intermittent failure**: `MockUpstream.protocol_version = "HTTP/1.1"` + `_drain_body()` |

## Acceptance

| Criterion | Evidence |
|---|---|
| docs no longer ignored, two docs checked in | `git status --ignored docs` has no `!!`; the two files are `A`; the gate reports `all 33 Markdown pages under docs/ are tracked by git` |
| The new gate is falsifiable + skips when there is no `.git` | All three self-tests PASS: `rejects a docs file git does not track` (real-repo red output below), `accepts a fully tracked docs tree`, `skips a tree without .git`; the `--only` list contains `docs-tracked` |
| The self-test script as a whole all PASS | `verification gate self-test passed` (18 cases) |
| spec matches the implementation | Programmatic comparison: the six names in the gate table and the `--only` candidate string == argparse choices; the five version files match the actual validation scope of `check_release_metadata.py`; all `../` links and `#L` anchors inside the spec resolve one by one |
| preflight runs the orphan self-test | New step `[preflight] OK - Check the model budget proxy (no-cost self-test)`; overall `PASS=353 FAIL=0`, OK steps 12 -> **13** |
| CI runs it too | `validate.yml` contains that step; the YAML was validated as legal with `uv run --no-project --with pyyaml` |
| The packaging version guard is falsifiable | Temporarily changing `pyproject.toml` to 0.11.1 -> the packaging script exits 1 **before** `Building release mod artifacts`, zip count 24->24, no `*0.11.1*` artifact; after restoring, the checker is consistent again |
| Full gates | C# **335 PASS / 0 FAIL**; MCP **167 OK**; all 6 gates green; `check_release_package.py --source-root .` passed; preflight 353/0 |

## Extra fix: the orphan self-test itself was flaky (it had to be fixed, otherwise it would add intermittent red to CI)

When wiring it in under W3 it was found that this self-test failed intermittently at about 5-8% (`http_allowlist_and_count` / `sse_usage` reported
`chat status 502`, with proxy-side stderr `upstream failure type=ConnectionAbortedError`).

Root cause (located with a controlled experiment, not a guess):

```
baseline (unchanged)                 : posts=40  failures=2   [(20,502), (34,502)]
keep-alive only (body not read)      : posts=150 failures=2   [(44,502), (93,502)]
keep-alive + drain the request body  : posts=150 failures=0
keep-alive + drain the request body  : posts=400 failures=0
full self-test x15                   : 15/15 exit 0
```

`MockUpstream` uses the default HTTP/1.0 of `BaseHTTPRequestHandler`: it closes the connection right after responding,
while the request body is never read; Windows loopback sends an RST on such "close with unread data",
the proxy's `urllib` reads `ConnectionAbortedError`, and it falls into the 502 fallback branch.
After changing to HTTP/1.1 + `_drain_body()` every request stays framed and the race disappears.

The forwarding semantics of the production proxy were **not** changed (in particular no automatic retry was added to paid POSTs): the fix is only inside the test harness.

## Live-only / unproven

- Actual runs on the CI side (no push, GitHub Actions not triggered): the commands are verbatim identical and local equivalent verification passed, but the runner did not run.
- The "deliberately unwired" scripts in the Script inventory (`scan-assembly-strings.ps1`, `generate-sts2-knowledge.ps1`,
  the two two-instance acceptance scripts, `sts2-validation-secrets.ps1`) need `sts2.dll` / an online game / a DPAPI key, and were not run on this machine.

## Out of scope (not done, a user decision)

Do not delete `mcp_server/data/eng/`, do not delete any script, and do not add mod main project compilation to CI.
