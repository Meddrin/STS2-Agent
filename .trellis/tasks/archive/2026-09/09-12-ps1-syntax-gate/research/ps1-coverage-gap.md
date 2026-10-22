# Evidence: nobody checks PowerShell syntax today

## Current state check

| Check | Coverage |
|---|---|
| gate `script-encoding` | Only "whether a `.ps1` containing non-ASCII carries a UTF-8 BOM", **does not parse syntax** |
| preflight "Compile Python sources" | `python -m py_compile <client.py> <server.py>`: two `.py` files, no `.ps1` |
| CI `validate.yml` | Calls only a few specific scripts (profile / gate self-test / exit code propagation), does not parse the rest |
| C# tests | `CompanionStartupTests` and others only read `.ps1` as text (`AgentSourceFixture.Read`), without parsing |

`git ls-files scripts/*.ps1` = **30** (including `build-mod.ps1`, `package-release.ps1`,
`preflight-release.ps1`, two gate self-tests, a dozen-plus live verification scripts, platform mirrors, etc.).
There is no `.ps1` in the repo outside `scripts/`.

## Why this is not a theoretical risk

These scripts are part of the release chain: `package-release.ps1` produces the player zip,
`preflight-release.ps1` is the "pre-release overall check", and `sts2-coop-full-run-acceptance.ps1` is the two-instance co-op acceptance.
Most of them are run **only on a real machine/at release**, exactly the moment that can least go wrong. A syntax error shows up on a real machine as
"the script fails on startup", and no gate would warn about it at commit time.

## Consistency with existing work

- Last round added a Roslyn parsing backstop for the 16 C# source files CI cannot compile (`SourceCoverageTests`);
- this task adds the same kind of "cheapest static backstop" to `.ps1`.
- Both times follow the same principle: **parse only, never execute**.

## CI/environment-related confirmation only

- `pwsh` (PowerShell 7) and `powershell` (5.1) are consistent in parsing the syntax shapes used in my repo;
  both this machine and CI have `pwsh` installed (CI logs show the runner using `C:\Program Files\PowerShell\7\pwsh.exe`).
- The skip path for "neither installed" cannot be verified on this machine (it has both) and can only be confirmed by code review.

