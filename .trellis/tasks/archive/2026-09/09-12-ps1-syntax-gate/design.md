# Design

## Gate implementation (`scripts/check_verification_gates.py`)

Add the function `check_ps1_syntax(repo_root: Path) -> list[str]`, with the same signature as the other gates (returns notes, raises `GateError` on failure).

```python
PS1_GLOB = "*.ps1"

def check_ps1_syntax(repo_root: Path) -> list[str]:
    scripts_dir = repo_root / "scripts"
    paths = sorted(p for p in scripts_dir.rglob(PS1_GLOB) if p.is_file())
    if not paths:
        return [f"no PowerShell scripts under {scripts_dir.name}/ to parse"]
    interpreter = shutil.which("pwsh") or shutil.which("powershell")
    if interpreter is None:
        return ["no PowerShell interpreter on PATH; skipping the .ps1 syntax check"]
    ...  # parse all files in one subprocess, collect the JSON result, report errors one by one
```

Key points:

- **Interpreter choice**: prefer `pwsh` (available on CI's `windows-latest` and locally), falling back to `powershell` (Windows PowerShell 5.1).
  Both ship the `Parser` type of `Microsoft.PowerShell.Utility`; either parses the same syntax.
- **Parse all files in one call** (do not start one process per file): pass the file list via stdin or arguments to a
  `-Command` script, which runs `ParseFile` on each file and prints `{file, errors:[{line, column, message}]}` as JSON to stdout.
  At implementation time use `-NoProfile -NonInteractive` to avoid pollution and interactive waits.
- **Do not execute**: use only `[System.Management.Automation.Language.Parser]::ParseFile($path, [ref]$null, [ref]$errors)`.
  `ParseFile` is pure parsing: a file with a syntax error is not executed either.
- **Handling subprocess failure**: if the interpreter fails to start / exits non-zero and no JSON is obtained => raise `GateError` (the gate itself is broken,
  which must not be taken as "the scripts are fine").
- **Output shape**: consistent with the other gates, `notes.append(...)` one by one; on success give a one-line summary (for example
  `30 PowerShell scripts parse cleanly`). Register the gate name in the `GATES` dict, and `--only` picks it up automatically
  (`choices=sorted(GATES)`).

## Self-test (`scripts/test-verification-gates.ps1`)

The existing `Assert-Case` does: build a fixture -> mutate -> run with `--only <gate>` -> assert a non-zero exit. Add two cases:

1. Bad syntax: write unclosed text such as `if (` into `$fixture/scripts/broken-probe.ps1` => expect the gate to reject
   (`Assert-Case -Name "..." -Only "ps1-syntax" -Mutate { ... }`).
2. Good syntax: write a valid `.ps1` => use an "expect to pass" assertion shape (use something like `Assert-Accepts` if the existing script has one,
   otherwise add a small helper in the same pattern, or write the case directly as "the gate exit code is 0").
   Note that the fixture's `scripts/` already contains `check_verification_gates.py` (the only file),
   so the "no `.ps1` => skip" path can be pinned along the way.

For the fixture structure see `scripts/test-verification-gates.ps1:85-101`. **Do not** change the semantics of existing cases.

## Docs

`.trellis/spec/operations/validation-and-release.md`:
- "Six offline gates: ..." -> seven, and add `ps1-syntax` to the parenthetical;
- add a row to the "The six gates are:" table;
- sync the `--only` candidate string (currently written as `--only api-doc|api-facts|doc-marks|docs-tracked|lockfile|script-encoding`).
If `.trellis/spec/operations/index.md` also mentions the gate count, sync it as well.

## Pitfalls

- `check_verification_gates.py` must stay **standard-library-only** and offline; calling `pwsh` is the same kind of practice as its existing
  `subprocess.run(["git", ...])` (`docs-tracked`) and does not violate the constraint.
- The gate must run on the **fixture** (self-tests use `--repo-root <fixture>`): the fixture's `scripts/` has only that `.py`,
  so the "no `.ps1` => skip" branch must really exist, otherwise other cases would falsely go red.
- Derive paths from `repo_root`, not `Path.cwd()` (self-tests are invoked from elsewhere).
- The gate output must not print the full script contents.
- The `.ps1` count will grow: do not hardcode 30, enumerate dynamically.

