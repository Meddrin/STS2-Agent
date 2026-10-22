# Release acceptance entry

The 2026-04-30 v0.7.0 checklist has been archived to the [original record](../history/release-readiness_v0.7.0_2026-04-30.md). Old version checklists are no longer the current release bar.

Before a release, check against these entries:

1. [Current status and acceptance boundary](../PRODUCT_PLAN_CURRENT.md): confirm the candidate version, the existing evidence, and the remaining gaps; historical test results must not be treated as passing for a new candidate.
2. [Release and validation spec](../.trellis/spec/operations/validation-and-release.md): check the version source, commands, and side effects.
3. [Release preflight](../scripts/preflight-release.ps1): run the static gates; passing preflight does not mean live loading or play has passed.
4. [Release packaging](../scripts/package-release.ps1): build and check the release artifacts.
5. [Workshop notes](../steam-workshop/README.md): check the item, visibility, subscribed version, and actual loading for the target channel; a successful upload cannot replace subscription loading acceptance.

Live acceptance should distinguish at least mod loading, the target single-player or multiplayer flow, native results and saves, and install channel verification; record the candidate version, evidence, and failed items one by one, with the exact scope defined by the current status page.

## Historical live evidence

To judge "how something was verified before", look at these pinned records first. Each belongs to the build of its time and does not describe the current state:

- [Native GAME_OVER results and unlock screen classification](evidence/native-game-over-progression/README.md): the 2026-08-31 real-machine regression (PR #53), with the save hash and the screen classification conclusions of that time.
- [PR #56 native profile switch live test](pr56-live-test/README.md): the 2026-09-05 profile switch acceptance.
