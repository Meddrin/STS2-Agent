# Evidence

## Deliverables

| File | Change |
|---|---|
| `scripts/check_verification_gates.py` | Added the `api-facts` gate (mod_version / screen list / default port), registered in `GATES` |
| `scripts/test-verification-gates.ps1` | The fixture gains `GameStateService.cs`/`HttpServer.cs`, plus 3 new self-test cases (BOM kept) |
| `.github/workflows/validate.yml` | Added a static `check_release_package.py --source-root .` step |
| `mcp_server/README.md` | Filled in the 14 drifted tools (54 in total), and delimited the contract area with `BEGIN/END LEGACY ACTION TOOLS` markers |
| `mcp_server/tests/test_legacy_action_coverage.py` | Appended a two-way set test for README <-> `_LEGACY_ACTION_TOOLS` |

## Acceptance

| Criterion | Evidence |
|---|---|
| New gate output | `[gate] api-facts: ok` + 4 detail lines (mod_version 0.11.0 consistent; all 24 screens in the doc enum; `MODAL` is produced by another producer so only a hint; port 8080 consistent) |
| Falsifiable (three) | Change mod_version -> `states mod_version 0.0.1 but … declares 0.11.0`; delete `CARDS_VIEW` from the doc -> `can emit screens missing from …: CARDS_VIEW`; change `DefaultPort = 9999` -> port mismatch error. All restored |
| Gate self-test | All 15 cases of `test-verification-gates.ps1` PASS (including the 3 new ones and restored) |
| README binding | Delete the `confirm_selection` entry -> `does not list these legacy tools: confirm_selection` red; restore green |
| CI step | Re-ran `python scripts/check_release_package.py --source-root .` locally verbatim -> exit 0 |

## Gates

All 5 gates of `check_verification_gates.py` passed (`api-doc` still 55 actions match); MCP 167 OK;
preflight (including the self-test step of the new gate) `Static preflight complete.`

## Not proven

GitHub Actions was not triggered (no push this round), so the new step has not run on a real runner; the command and working directory were checked verbatim.
The screen assertion is one-way (code ⊆ doc); extra items listed in the doc do not turn it red (`MODAL` is of this kind).
