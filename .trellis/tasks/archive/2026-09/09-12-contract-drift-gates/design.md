# Design: the api-facts gate and README binding

## 1. `scripts/check_verification_gates.py`

Add `check_api_facts(repo_root)` and register it in the `GATES` dict (named `api-facts`), following the existing
`read_text` / `GateError` style and the output convention of "print details at each step".

```python
VERSION_PATTERN = re.compile(r'"mod_version"\s*:\s*"([^"]+)"')
SCREEN_ROW = re.compile(r"^\|\s*\`([A-Z][A-Z0-9_]*)\`", re.MULTILINE)   # go by the real table shape in the doc
SCREEN_LITERAL = re.compile(r'=>\s*"([A-Z][A-Z0-9_]*)"')
SCREEN_EARLY_RETURN = re.compile(r'return\s+"([A-Z][A-Z0-9_]*)"')
PORT_PATTERN = re.compile(r'"port"\s*:\s*(\d+)')
```

Checks:

1. `mod_version` in `api.md` == `version` in `STS2AIAgent/mod_manifest.json` (a mismatch is a GateError,
   and the error message gives both values).
2. From the body of the `ResolveNonModalScreen` method in `STS2AIAgent/Game/GameStateService.cs`, extract
   the two kinds of screen names `=> "NAME"` and `return "NAME"` (the latter covers early-return branches), requiring
   `code set - doc set == empty`; and give the reverse difference as a hint (extra items listed in the doc do not count as failure, unless it is confirmed at implementation time that this should be strict).
   **Note**: the extraction must be confined to the body of `ResolveNonModalScreen` (use a method body slice); do not collect
   all the strings in the whole file.
3. Port: take the default value of `HttpServer` as the source of truth (confirm the real constant location at implementation time), and compare it with the one place in `docs/api.md`
   that explicitly declares the "default port"; if the doc has no parseable default port, fall back to "the set of ports appearing in the doc must contain
   the code default".

Each one must be verifiable by "break it and it goes red", and written into `scripts/test-verification-gates.ps1`.

## 2. The `mcp_server/README.md` tool list

- Fix the content first: add the 14 missing items to the "current tools" section (keeping the existing layout and description style, with descriptions taken from
  the `ActionToolSpec` text of `server.py` or the action descriptions of `docs/api.md`).
- Then add the binding: append a test to `mcp_server/tests/test_legacy_action_coverage.py` that
  parses the tool names in the README and asserts a **two-way set** equality with `_LEGACY_ACTION_TOOLS` of `server.py`
  (red if one is missing, red if one is extra).

## 3. CI and gate self-tests

- `.github/workflows/validate.yml`: add a step after the verification gates step:
  `python scripts/check_release_package.py --source-root .` (working directory the same as the existing steps).
- `scripts/test-verification-gates.ps1`: following the existing fixture pattern (temporary copy + break + assert exit != 0),
  add cases for the new gate; **when this script is a .ps1 containing non-ASCII it must carry a UTF-8 BOM** (the script-encoding gate checks this).

## Risks

- Screen name extraction is text-level, so the method body slice must be accurate; at implementation time first print the extraction result to confirm the count is reasonable, then assert the set relations.
- If the shape of the screen enum table in `docs/api.md` does not match the regex above, go by the real shape read at implementation time (do not force the example).
