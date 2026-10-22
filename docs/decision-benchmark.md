# Offline decision quality benchmark

`decision-benchmark.json` is the v1 **set of reproducible cases**: it scores whether a candidate action record follows the current
state, the available actions, and the high-priority constraints the playbook already states. It is not a combat simulator, and it does not claim that an offline score
proves a model will clear a run, reach a given win rate, or produce the same next state in the real game.

This boundary is deliberate: every scorable point must be backed by the same state snapshot and the existing strategy text; card strength, long-term deck building, and combat outcomes
without enough facts are never given an invented score. This fixes the benchmark format at zero cost now, and later, when real-game states are captured from the debug
console, or when a real model is called with the user's permission, the same input and report formats can be reused.

## How to run (no game, no network, no model)

```powershell
# Only validate the structure, evidence paths, and version of the benchmark fixture
python scripts/decision_benchmark.py

# Score one set of candidate actions; the bundled example is a deliberate perfect-score baseline, not a model result
python scripts/decision_benchmark.py `
  --answers docs/decision-benchmark.answers.example.json

# Write the deterministic JSON report to the gitignored build/ for local comparison
python scripts/decision_benchmark.py `
  --answers path/to/candidate-answers.json `
  --output build/decision-benchmark/candidate-report.json

# Offline module contract tests
python scripts/test-decision-benchmark.py
```

If the game was started by the operator, the **explicitly read-only** command below can capture one raw `/state` as candidate material for a manually added case;
it sends no action or model request, writes its output to the gitignored `build/`, and does not
automatically count the state as passed:

```powershell
python scripts/decision_benchmark.py `
  --capture-base-url http://127.0.0.1:8080 `
  --output build/decision-benchmark/manual-state-capture.json
```

These four commands only read JSON under version control (the `--output` of the last one is the exception and writes a local report), and they do not start the game,
access MCP, call HTTP, or use any model/API credentials. `scripts/preflight-release.ps1` runs the structure validation and
the module contract tests, so it ensures the fixture is not broken before a release; it does not treat the example's 100% as evidence of model quality.

## Input contract

### Suite: `decision-benchmark.json`

The top level is fixed as:

```json
{
  "schema_version": 1,
  "title": "...",
  "cases": []
}
```

Each case has these fields:

| Field | Constraint | Meaning |
| --- | --- | --- |
| `id` | globally unique string | stable comparison key; when extending the benchmark, never rewrite the id of an already published case |
| `kind` | `combat` / `event` / `map` / `rest` / `shop` / `reward` | screen type of the decision |
| `title` | non-empty string | readable title, not used for scoring |
| `state` | `/state`-style object | must contain at least `screen` and `available_actions`; real state fields may be added so the format does not become fragile when the mod adds new fields |
| `expectations` | non-empty array | each is an independently scored constraint and must be verifiable by the fixture itself |
| `evidence` | non-empty array of strings | readable sources pointing to strategy/playbook/state facts; not a substitute for the model's reasons |

`screen` must equal the uppercase form of `kind`. Raw state **allows** unknown fields, because clients must be able to read fields added by newer mod versions;
the suite's own top level, cases, and expectation structure are strict: a misspelled field fails instead of silently
changing the score.

Each expectation has a unique `id`, a positive `points`, and one of the following `kind` values:

| expectation kind | Required fields | Scoring semantics |
| --- | --- | --- |
| `action` | `action` | the candidate action name must be the same |
| `option_action` | `action`, `option_index` | both the action and the index must be the same |
| `target_action` | `action`, `target_index` | the action and the enemy/player target index must both be the same |
| `forbid_action` | `action` | the candidate action must not equal this action |
| `state_path` | `path`, `equals` | **fixture self-check**: the dotted path in that state must equal the value; it scores when the case is answered, so that a case with "evidence" is not easier to max out than a plain case; an unanswered case still scores 0 |

`state_path` does not read model output; at suite load time it checks that "this case really has the facts it claims".
In addition, the loader verifies that each recommended action is actually in `available_actions`, and maps every supported `option_action`
back to its snapshot array (cards, map, events, rest, shop, rewards); `target_action` is mapped back to the target card's public
`valid_target_indices`. `option_index` is the benchmark's neutral position of the candidate choice (for `play_card`, the runner
should convert it into the real request's `card_index`); the answer also keeps the real request's `target_index`, so target selection is not
treated as an ordinary option. To evaluate reason quality or multi-step outcomes in the future, bump the schema version and use a new expectation
kind; do not quietly reinterpret the v1 fields.

### Answers: candidate action records

```json
{
  "answers": [
    {
      "case_id": "map-low-health-avoid-elite",
      "action": "choose_map_node",
      "option_index": 1,
      "target_index": 0,
      "reason": "optional human/model rationale; v1 does not score it"
    }
  ]
}
```

`case_id` may appear only once and must reference a case already in the suite; `action` is required, and `option_index` and
`target_index` are optional and must be non-negative integers. Omitting a case does not remove it from the denominator: that case scores 0 and is marked `answered: false` in the report.
This rule makes the coverage of different models/runners explicit, so nobody can get an inflated percentage by submitting only the questions they can answer.

## Scoring and report

The score is `earned_points / available_points` over all expectations. The report contains: the suite name, the number of answered cases,
the total score, the percentage, and the `passed` flag and details of every constraint of every case. The JSON uses sorted keys, two-space indentation, and a trailing newline,
so the same input always produces byte-identical output.

- The reference answer file is a **hand-built perfect-score baseline**; it only proves that the loader/scorer/fixtures are self-consistent. It must not be written into the model compatibility matrix,
  and must not be used to say a provider has passed.
- There is no game state transition, damage simulation, or reward value function: the report states `scoring_scope` explicitly, to keep downstream users from misreading 100% as
  "will win".
- Cases are scored only on exposed data. For example, a shop case can test `enough_gold` / `on_sale`, but it will not claim that a card is "stronger" without card
  metadata and deck-building context.

## Follow-up interfaces for capture and live testing

1. **Live capture (still needs a manually started game)**: with debug actions enabled, use the existing `run_debug_command()` of
   `run_sts2_validation.py` to reach a fixed room/fight, then read one stable `/state`.
   `decision_benchmark.py --capture-base-url ... --output build/...` can be used for one explicit read-only capture; it does not
   navigate, execute actions, or call a model automatically. After a human reviews the state and removes identifiers that should not be committed, archive that state together with reviewable
   `evidence` as a new case; do not commit saves or tokens/secrets to the repository.
2. **Candidate runner (not implemented in this commit)**: for each case, hand the state to a model/agent and record what it actually tried,
   `{case_id, action, option_index, reason}`. Use the runner's own local request budget and provider consent flow first; real calls
   consume quota and need the user's authorization first.
3. **Live outcome (separate, stronger evidence)**: to claim "this model succeeded in situation X", record the initial state, request,
   action response, fresh state, and the observable before/after values, and validate under the project's existing live-validation rules. Do not use the v1
   offline score as a substitute for that evidence.

The current v1 has 11 cases across combat/event/map/rest/shop/reward as a minimal structured set; the planned 20-30
**real debug-console snapshots** are still a later capture task and are not faked by this offline scaffold.
