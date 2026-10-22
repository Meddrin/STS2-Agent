# Add an AST parsing gate for every PowerShell script

## Background

There are **30 `.ps1` files** under `scripts/` (`git ls-files scripts/*.ps1`), and they are the entry points for building, packaging, releasing,
live verification, and CI self-tests. Today **no check parses their syntax**:

- Of the 6 gates, `script-encoding` only checks "whether scripts containing non-ASCII carry a UTF-8 BOM";
- the preflight "Compile Python sources" only `py_compile`s the two `.py` files of `mcp_server`;
- CI runs only a few specific scripts among them.

Consequence: a wrong bracket or a missing quote in some `.ps1` is not found until someone actually runs it,
and that usually happens at the moment that should least go wrong, "before a release" or "during live verification".

This is the same kind of gap as the syntax coverage added last round for the 16 uncompiled C# source files: **missing "the cheapest static backstop"**.

## Goals

Every `.ps1` is parsed at least once; when syntax breaks the gate turns red and names the file and line number.

## Acceptance criteria

- [x] `check_verification_gates.py` gains the gate `ps1-syntax`, entering the `GATES` registry,
      appearing in the `--only` candidates, and participating in the full run by default.
- [x] Covers **all** files under `<repo-root>/scripts/**/*.ps1` (currently 30), reporting the count one by one (the file count is visible in the gate output).
- [x] **Parse only, never execute**: use `[System.Management.Automation.Language.Parser]::ParseFile`,
      never `Invoke-Expression` / `-File` / dot-source any repo script.
- [x] When parsing fails, report `file:line:column + message`, and the gate exit code is 1.
- [x] When no PowerShell interpreter is found, or the directory has no `.ps1`, **skip and explain** (consistent with the skip shape of `docs-tracked` when there is no `.git`),
      neither silently passing nor erroring.
- [x] `scripts/test-verification-gates.ps1` adds self-tests: put a syntactically broken `.ps1` into the fixture's `scripts/` => the gate must reject;
      put a valid `.ps1` => the gate must accept.
- [x] The gate table and count in `.trellis/spec/operations/validation-and-release.md` change from 6 to 7, and the `--only` list is synced.
- [x] Full gates, preflight, and CI are green.

## Out of scope

- Do not parse `.sh` (there is no equivalent static parsing capability, and CI runs only Windows).
- Do not do PSScriptAnalyzer-style **rule** checks (unused variables, aliases, etc.): syntax only.
- Do not change the content of any existing script (this task only adds checking capability).

