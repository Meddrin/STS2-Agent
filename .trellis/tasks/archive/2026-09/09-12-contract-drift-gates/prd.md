# Add gates to contracts that "drift silently"

## Background

This round of investigation confirmed several contracts where **breaking them is caught by no gate**, three of which have really drifted:

1. **`mod_version` at `docs/api.md:124` is a bare fact**: the doc itself says it is "consistent with `ModVersion` in `Router.cs`"
   (`:150`), but the api-doc gate only compares the set of action names
   (`scripts/check_verification_gates.py:342-355`), and `check_release_metadata.py` does not read docs at all.
   The next version bump will leave the doc silently stuck at the old value.
2. **The screen list has no gate**: on the code side `ScreenResolutionContractTests` pins the 26 items of the switch expression,
   but **does not include** the early-return branches in `GameStateService.ResolveNonModalScreen` such as `NUnlockScreen` / `CARD_SELECTION` /
   `CARDS_VIEW` / `MULTIPLAYER_LOBBY` / `COMBAT`; the screen enum at `docs/api.md:70-95`
   and the Screen Routing in `skills/sts2-mcp-player/SKILL.md` are aligned entirely by hand.
3. **The tool list of `mcp_server/README.md` has drifted**: `_LEGACY_ACTION_TOOLS` has 54 items in total,
   and the "current tools" section of the README is measured to be missing 14 (`switch_profile`, `continue_game_over`,
   `dismiss_game_over_wait`, `confirm_unlock`, `close_cards_view`, `unready`,
   `increase_ascension`, `decrease_ascension`, `host_multiplayer_lobby`,
   `join_multiplayer_lobby`, `ready_multiplayer_lobby`, `disconnect_multiplayer_lobby`,
   `invite_ai_teammate`, `confirm_selection`), and no test reads the README.

One more process gap: `scripts/check_release_package.py --source-root .` is a **purely static** check
(it reads only source files and needs no game), yet it is hooked only into the local preflight and CI does not run it
(`.github/workflows/validate.yml` has only 8 steps, none containing it).

## Goals

1. `check_verification_gates.py` adds an `api-facts` gate (plain text, zero dependencies), covering at least:
   - `"mod_version"` in `docs/api.md` == `version` in `STS2AIAgent/mod_manifest.json`;
   - every screen name in the screen enum of `docs/api.md` appears in the code, and the set of screen names produced by `ResolveNonModalScreen` in the code
     **does not exceed** the doc set (a new screen must sync the doc);
   - the default port in `docs/api.md` matches the default value of `HttpServer`.
2. Fix the tool list drift in `mcp_server/README.md`, and add a test that binds the README list to
   `_LEGACY_ACTION_TOOLS` (changing code without changing the doc goes red).
3. Add `check_release_package.py --source-root .` to CI (`.github/workflows/validate.yml`).
4. `scripts/test-verification-gates.ps1` gains self-test cases for the new gate (the whole point of that script is that "the gates themselves must be tested too").

## Acceptance criteria

- [x] `python scripts/check_verification_gates.py` output gains `[gate] api-facts: ok` and its detail lines.
- [x] Each of the three assertions is falsifiable: temporarily break one place (version number / screen name / port) -> that gate turns red with a readable reason -> restore.
- [x] `scripts/test-verification-gates.ps1` has positive and negative cases for the new gate, and still passes overall.
- [x] The tool list in `mcp_server/README.md` matches `_LEGACY_ACTION_TOOLS` (the test asserts two-way set equality).
- [x] `.github/workflows/validate.yml` gains a `check_release_package.py --source-root .` step
      (reproducing the command locally must exit 0).
- [x] All existing gates stay green (`api-doc` still reports 55 actions match).

## Out of scope

- Do not add gates to all the field tables of `docs/api.md` (20+ tables, the benefit/cost is not worth it).
- Do not change the outdated game version numbers in `docs/setup.md` and `docs/reverse-engineering.md`
  (they need real-environment facts, are a manual maintenance item, and are recorded separately).
- Do not add CI compilation for the main mod project (CI has no `sts2.dll`/`GodotSharp.dll`; the local preflight already covers that step).
