# Build Fingerprint Index (Cross-Version)

> Historical snapshot + running index: this page collects the size and hash of every published build. The rows in the tables are historical records and do not represent the current state;
> for the current release status see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md).

## Why This Page Exists

`mod_version` identifies a build only when "one version number is shipped only once". This project does not meet that premise: **v0.12.3 and v0.12.4
were each re-released twice under the same number**, so three builds call themselves `0.12.3` and three more call themselves `0.12.4`, and the version string can never tell them apart.
Each time this happened, the release record could only say "tell them apart by size or hash", and those numbers had been captured by hand after the upload.

So when a player reports "I'm on 0.12.4 and hit X", the first question is not "which version" but
**"which build"**; this page is where that gets answered.

**Rows from v0.12.5 on are copied directly from `build-fingerprint.json` and are no longer collected by hand**; rows for 0.12.4 and earlier were captured after the fact.

Since 2026-09-16, every packaging run writes a `build-fingerprint.json` next to the artifact
(`scripts/lib-build-fingerprint.ps1`, called by `package-release.ps1` and `package-steam-workshop.ps1`),
containing the per-file SHA256, the byte sum Steam uses as `file_size`, and the commit used for the build and whether the working tree was clean.
The row for a new version should be copied straight from that JSON instead of being collected by hand again.

## Steam Workshop (Item 3796486050)

`file_size` is the byte sum of the `content/` directory; after each upload it is re-checked against the Steam Web API and equals the local byte sum.

| Version | Build | `time_updated` | `file_size` | Content id (`hcontent_file`) | DLL SHA256 | Source commit |
| --- | --- | --- | --- | --- | --- | --- |
| 0.16.1 | Only one (**current**) | 2026-09-24 11:54:48 | 1646567 | `1146374597091505946` | `CD9707AA463005977D885D1D5D6E206BA28B1CF82A69F67132F34019472E1553` | `a64e052` |
| 0.16.0 | Only one | 2026-09-24 02:32:06 | 1645031 | `1458911249290867048` | `7935096C0E95D8B0195EA45BB3720FFE867C4AAB0CDB47B72AF3CFC8EF1F70FB` | `4beaabe` (PR #199) |
| 0.15.0 | Only one | — | 1422823 | — | `20298101EAFCDCBA3B41191F21E272B78DCEF430A8E9E1706B4CD35C398B4129` | `7338b5f` (PR #179) |
| 0.14.6 | Only one | — | 1386471 | — | `2B7C2DD547E8797470DCBE41CCC5806C2F1DE81490A4E45121D2C0919D94012A` | `c024142` (the fingerprint records a package built from a **dirty working tree**) |
| 0.14.0 | Only one | 2026-09-21 10:44:20 | 1360359 | `5672199774606704499` | `F64F8C2AEBFFAC319FF3ADF2976C7C2323D6896E0B73EEB8980654DCECFFFDA3` | `5f9ea72` (PR #174) |
| 0.13.0 | Only one | 2026-09-19 11:32:25 | 1252357 | `3840768014407368680` | `6B9D90C8C13998B2A5EEAA22D7B9D7543A466C5C6688214F73393A489A7FBE42` | `78b085f` (PR #161) |
| 0.12.5 | Only one | 2026-09-17 09:36:37 | 1238533 | `2962740913650521121` | `1624BBF5CBAB50D415FAFCC59C354A6101616899F2FC00E9C8A76CAFF84CD4A7` | `f361bdb` (PR #145) |
| 0.12.4 | Third | 2026-09-16 00:42:43 | 1236997 | `6841790951225102097` | `0A8FBA678A363E23BA20225670C92E98296D741171FDEA037D6A5F9A6F49611C` | `d0c6fbd` (PR #136) |
| 0.12.4 | Second (lived about 1 hour, abandoned) | 2026-09-15 23:34:57 | 1236997 | — | — | — |
| 0.12.4 | First release | 2026-09-15 19:38:00 | 1233413 | — | `0134C8BC10A76F15C71CAFFEAC7D818D71744111A4045DDF52AA80A5AA51A5BF` | `4b8e7c5` (PR #133) |
| 0.12.3 | Third | 2026-09-14 02:17:15 | 1232901 | `6689158800196895712` | — | `c2630a8` (PR #123) |
| 0.12.3 | Second | 2026-09-14 00:09:26 | 1229829 | `6028841468497339213` | — | `0f60ec4` (PR #113) |
| 0.12.3 | First release | 2026-09-13 22:31:22 | 1227269 | `5284257893537057643` | — | `b0217b0` (PR #108) |
| 0.12.2 | Only one | 2026-09-13 16:51:04 | 1225733 | `8439947284938535648` | — | `72b2a81` (merge commit of PR #104) |
| 0.12.1 | Only one | 2026-09-13 12:41:42 | 1213444 | — | — | `640c343` (merge commit of PR #95) |
| 0.12.0 | Only one | 2026-09-13 02:25:02 | 1202181 | manifest `3382317012139913714` | — | `69887a3` (merge commit of PR #86) |
| 0.11.0 | Only one | 2026-09-12 02:33:39 | 1135844 | — | — | `84631b9` |

The 0.15.0 and 0.14.6 rows were copied on 2026-09-24 from the `build-fingerprint.json` that was still in `build/steam-workshop/` at the time:
the post-upload `time_updated` and content id were not recorded then (the Web API only returns an item's current values), so they are left as `—`.

**The second and third builds of 0.12.4 have the same byte count (both 1236997) and can only be told apart by DLL hash or content id.**
No hash was kept for the second one: it was replaced within an hour and there was no fingerprint file yet; this is exactly why
`build-fingerprint.json` was added. A blank `—` means "not recorded at the time", not "has no value".

## GitHub Release

| Version | Tag points at | Asset bytes | Asset SHA256 |
| --- | --- | --- | --- |
| v0.16.1 | `a64e052` (Release commit on dev) | 779153 | `835E22E871DD4A217766F5E02043312D2B144E9D81CEB00B75B32B49D37C7551` |
| v0.16.0 | `4beaabe` (dev->main merge of PR #199) | 777852 | `43A026B56DD77D9B51969E8720D96CEEC0374CADFE5C25744B70E8EAD172E75F` |
| v0.13.0 | `78b085f` (dev->main merge of PR #161) | 576033 | `8CFC0F451EE04D30C4F1708A5BF8553B5860FD1B61F99F249FEAAF87BAB18733` |
| v0.12.5 | `f361bdb` (dev->main merge of PR #145) | 562212 | `0E4A15184A679D95C5064247B2DB0FEFA25E3B68D743D9110A4F59DE0318CFED` |
| v0.12.4 | `3a4ec95` (dev->main merge of PR #139) | 557906 | `AD970DB1204A0DC37602A2751935E87E5B87FC502B21B00E47B2699C4A98EC57` |
| v0.12.3 (third re-cut) | `c2630a8` | 555061 | `E882B653B48B15EF278CFD9E20CC443FDCA3E5D69A51E362A40B934E29E167DF` |
| v0.12.3 (second) | `0f60ec4` | 552014 | `B7684C9F…` |
| v0.12.3 (first release) | `b0217b0` | 549933 | `B9DC1A07…` |
| v0.12.2 | `72b2a81` | 549033 | `35C1F0FC0ED3C664C0F74A73B5759486E4CA2BE92295CC47062FCC65552B5D9D` |
| v0.12.0 | `69887a3` | 533493 | `C2B1F3229D6CF8E7D757D571AAF717004DDBCAFD4F8AAE1276A86A54A3AFA685` |
| v0.11.0 | `84631b9` | 677015 | — |
| v0.10.5 | `04d2466` | — | `27da01401714347143244badfb9674e0e31932d03f1e5bebe1e4c67705fb12b8` |

v0.10.7 (`f9330ba`) only updated the Workshop, with no tag and no Release; that commit is already included in `v0.11.0`.

## How to Check One Report

1. Have the reporter read `mod_version` from the in-game F8 window or `GET /health`.
2. If that version has **only one row** in the table above, you are done.
3. If there are several rows, take `STS2AIAgent.dll` from the subscription directory and compute its SHA256
   (`Get-FileHash -Algorithm SHA256`), then compare it against the DLL hash column.
4. If the hash is not in the table, the file is a local build or a manually installed old file, not any copy the Workshop shipped.

## Full Per-Version Records

The table above keeps only the fields used to identify a build. The full process of each upload, the re-checked fields, and the live / offline evidence of the time are in the respective release records:
[v0.16.1](release-v0.16.1_2026-09-24.md), [v0.16.0](release-v0.16.0_2026-09-24.md), [v0.14.0](release-v0.14.0_2026-09-21.md), [v0.13.0](release-v0.13.0_2026-09-19.md), [v0.12.5](release-v0.12.5_2026-09-17.md), [v0.12.4](workshop-upload-v0.12.4_2026-09-15.md), [v0.12.3](release-v0.12.3_2026-09-13.md),
[v0.12.2](release-v0.12.2_2026-09-13.md), [v0.12.1](release-v0.12.1_2026-09-13.md),
[v0.12.0](release-v0.12.0_2026-09-13.md), [v0.11.0](release-v0.11.0_2026-09-12.md),
[v0.10.7](workshop-upload-v0.10.7_2026-09-12.md).
