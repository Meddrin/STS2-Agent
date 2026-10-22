# Evidence

## Deliverables

| File | Change |
|---|---|
| `scripts/check_verification_gates.py` | Added the gate `ps1-syntax`: the `PS1_GLOB`/`PS1_PARSE_PROBE` constants, `check_ps1_syntax()`, registration in `GATES`, the file header description, `import shutil` (standard library only, offline) |
| `scripts/test-verification-gates.ps1` | Added 3 self-test cases (skip when no `.ps1` / accept valid / reject bad syntax); the UTF-8 BOM is unchanged |
| `.trellis/spec/operations/validation-and-release.md` | "Six -> Seven", the `--only` candidate string adds `ps1-syntax`, the gate table gains a row |
| `scripts/serve-sts2-network-mcp.ps1` | **Fixed the real syntax error the new gate caught** (see below) |

`.trellis/spec/operations/index.md` was not changed: grep confirms it does not mention the gate count.

## The new gate immediately caught a real committed bug

`scripts/serve-sts2-network-mcp.ps1:58`:

    Write-Host "Starting STS2 network MCP server on http://$BindHost:$Port$Path"

PowerShell parses `$BindHost:` as a scope/drive-style variable reference => **parsing fails outright**,
that is, this script has never been able to run since it was committed. This is not a false positive: the sibling `start-mcp-network.ps1:39` writes, at the corresponding place,
the escaped colon form (``$BindHost`:$Port``), showing the author knew about this pitfall and merely missed one spot.
And `mcp_server/REMOTE.md:16` documents this script as an executable command: the command "fails on startup".

The fix (aligned with the sibling script, one line): escape the colon after `$BindHost`.
After the fix `Parser::ParseFile` reports 0 errors, and the file is still pure ASCII (no BOM needed, `script-encoding` is green).

## Acceptance

| Criterion | Evidence |
|---|---|
| The gate is in `GATES`, appears in `--only`, and participates in the full run by default | The `--only` in `--help` contains `ps1-syntax`; the full run prints `verification gates passed: api-doc, api-facts, doc-marks, docs-tracked, lockfile, ps1-syntax, script-encoding` |
| Dynamically covers all `.ps1`, reporting the count | A temporary tree with 29 valid scripts -> `29 PowerShell scripts parse cleanly` (count not hardcoded) |
| Parse only, never execute | The probe calls only `[System.Management.Automation.Language.Parser]::ParseFile`, with no `Invoke-Expression`/`-File`/dot-source |
| Failure reports file:line:column + message, exit code 1 | `scripts/serve-sts2-network-mcp.ps1:58:56: Variable reference is not valid...` (before the fix) |
| Skip with an explanation when there is no `.ps1` / no interpreter | Self-test `PASS ps1-syntax gate skips a scripts/ that holds no .ps1`; interpreter branch: forcing `PATH` to leave only PowerShell 5.1 likewise gives `29 ... parse cleanly` |
| Self-test: bad syntax rejected / valid accepted | `PASS ... rejects a script with a syntax error` / `PASS ... accepts a well-formed script` (all 21 self-test cases pass) |
| spec gate table and count 6->7 | See the deliverables table |
| Full gates | All 7 gates green; preflight **372 PASS / 0 FAIL**; C# 351 PASS / 0 FAIL; MCP 165 OK |

**Manual real-repo verification**: copy `build-mod.ps1` into a bad-syntax version and put it in `scripts/` (temporarily) ->
the gate reports both that temporary file and that existing real error -> after deleting the temporary file only the latter remains (that is, it was still red at the time, until the main agent fixed that line).

## CI/environment-only verification

- The "no PowerShell interpreter => skip" branch: this machine has both `pwsh` and `powershell`, so it cannot be locally manufactured that neither exists,
  and can only be confirmed by code review (when `shutil.which("pwsh") or shutil.which("powershell")` is `None` it returns the skip explanation).
- Which interpreter is actually hit on CI `windows-latest` is decided by the runner PATH; compatibility of both paths was verified locally.

## Value note

The design purpose of this gate is "find syntax errors at commit time". On its **first run** it caught a real, committed and documented faulty script,
which is itself evidence that it deserves to exist.
