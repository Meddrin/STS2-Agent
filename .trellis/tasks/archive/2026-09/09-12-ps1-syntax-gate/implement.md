# Execution checklist

1. Read:
   - `scripts/check_verification_gates.py`: the file header comment (explaining the criterion of each gate),
     the `GATES` dict and `main()` (about 630-670), `check_docs_tracked` (580-615, an example of the skip shape),
     `list_tracked_docs` (560-577, the existing usage of `subprocess.run`);
   - `scripts/test-verification-gates.ps1`: `Invoke-Gate` / `Assert-Case` / fixture construction (lines 1-100) and the case list at the end;
   - `.trellis/spec/operations/validation-and-release.md:10-40` (the gate table and count).
2. Implement `check_ps1_syntax` and register it in `GATES`.
3. Add two self-test cases.
4. Update the spec gate table/count/--only.
5. Verify (raw output is required for all):
   ```powershell
   python scripts/check_verification_gates.py                     # all 7 green
   python scripts/check_verification_gates.py --only ps1-syntax    # run alone
   powershell -ExecutionPolicy Bypass -File scripts/test-verification-gates.ps1
   powershell -ExecutionPolicy Bypass -File scripts/preflight-release.ps1
   ```
   Manual confirmation: copy `scripts/build-mod.ps1` and change it into bad syntax and put it in `scripts/` (**temporarily**) => the full gate turns red and reports that file;
   after deleting it, green is restored. (Or rely directly on the self-test cases, but at least do one real "bad script inside the repo" verification by hand.)
6. Do not commit or push; report the changed files, the red/green evidence, and any unexpected findings.

Note: scripts under `scripts/*.ps1` that contain non-ASCII must keep the UTF-8 BOM (the `script-encoding` gate):
you **only add self-test cases and gate code**, and if `test-verification-gates.ps1` already has a BOM, keep it.

