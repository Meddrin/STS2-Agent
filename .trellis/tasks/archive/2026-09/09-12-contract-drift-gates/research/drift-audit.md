# Evidence: contracts that drift silently

## Gate capability matrix (verified)

| Script | CI | Local preflight | Notes |
|---|---|---|---|
| `check_verification_gates.py` | ✓ `validate.yml:33` | ✓ `preflight-release.ps1:132` | Four gates: lockfile / api-doc / doc-marks / script-encoding |
| `check_release_metadata.py` | ✓ `:30-32` | ✓ `:96` | Version consistency across five files |
| `check_release_package.py` | ✗ | ✓ `:128` (`--source-root`) | Purely static, could have been in CI |
| `test-verification-gates.ps1` | ✓ `:35-36` | ✓ `:136` | Gate self-test |

## The three that drifted/were unprotected

1. `docs/api.md:124` `"mod_version": "0.11.0"`, and the doc itself at `:150` says it must match `Router.cs`;
   no gate reads it. `rg 'mod_version' docs` hits only those two places.
2. Screen list: `ScreenResolutionContractTests` covers only the 26 items in `return currentScreen switch`,
   and the early-return branches of `GameStateService.ResolveNonModalScreen` (`UNLOCK` / `CARD_SELECTION` /
   `CARDS_VIEW` / `MULTIPLAYER_LOBBY` / `COMBAT`) are not within it; on the doc side (`docs/api.md:70-95`, 25 lines,
   the Screen Routing in SKILL.md) no script aligns them.
3. The "current tools" section of `mcp_server/README.md` lacks 14 items (against the 54 of `_LEGACY_ACTION_TOOLS`),
   and no test reads the README.

## Already protected (for comparison)

- Action contract: `GameActionService` switch <-> the `docs/api.md` contract block (api-doc gate, 55 actions).
- Five-place version consistency: `check_release_metadata.py` (CI + preflight).
- Dependency lower bounds consistent with the lock: the lockfile gate of `check_verification_gates.py`.
- SKILL.md shared contract <-> `PlayPrompt`: `McpPlayerSkillTests` (inside the test project).
