# Validation

How the published numbers were checked, what was cross-checked, and what was
found to be wrong.

---

## 1. Approach

Three principles governed validation throughout:

1. **Independent implementations.** No number is reported on the authority of a
   single parser. Where two readers could disagree, both were written.
2. **Consistency invariants.** A parse that is wrong usually breaks a total
   somewhere. Several such invariants are used as checks.
3. **Record disagreements.** When two readers disagreed, the disagreement was
   investigated and recorded — never silently resolved in favour of one.

### Implementations

| # | Implementation | Shares code with others? | Role |
|---|---|---|---|
| 1 | Python metadata + IL model | — | Primary analysis |
| 2 | .NET `System.Reflection.Metadata` probe | **No** | Independent oracle |
| 3 | From-scratch Python layout walker | **No** | Independent geometry check |

---

## 2. Validation results

### 2.1 Core counts — two independent readers

| Quantity | Python | .NET `SRM` | Agrees |
|---|---:|---:|:--:|
| `TypeDef` rows | 305 | 305 | yes |
| `MethodDef` rows | 2,452 | 2,452 | yes |
| `TypeRef` rows | 626 | 626 | yes |
| `MemberRef` rows | 1,588 | 1,588 | yes |
| `TypeSpec` rows | 176 | 176 | yes |
| `AssemblyRef` rows | 38 | 38 | yes |
| `ImplMap` rows | 34 | 34 | yes |
| `Property` rows | 765 | 765 | yes |
| Per-type method counts, summed | 2,452 | 2,452 | yes |

### 2.2 AssemblyRef cross-check

| Comparison | Assertions | Failures |
|---|---:|---:|
| Name, version, flags, PKT length **and value**, culture, hash length — all 38 rows | 228 | 0 |
| `TypeRef` resolution-scope histogram | 2 | 0 |
| Per-assembly `TypeRef` usage counts | 115 | 0 |
| **Total** | **345** | **0** |

### 2.3 Consistency invariants used

| Invariant | Expected | Observed |
|---|---|---|
| `Valid` mask selects a table set consistent with all row counts | consistent | consistent, 30 tables |
| Sum of per-type method counts = `MethodDef` row count | 2,452 | **2,452** |
| Sum of per-namespace type counts = `TypeDef` row count | 305 | **305** |
| `#~` walk end vs stream end | ≤ stream size | ends 2 bytes short of 0x511E0 (padding) |
| `MethodList` values monotonic across `TypeDef` rows | monotonic | **monotonic**, 0 violations |
| Stage-2 FieldRVA output hash | `d22cb79d…25e9e` | **exact match** |
| TLS callback body SHA-256 | `3e652f9e…1b378` | **exact match** |

The method-count invariant is the most useful: a mis-parsed metadata layout shifts
`MethodList`/`FieldList` ranges and the sum stops equalling 2,452 immediately. It
is what caught the 16-byte `MethodDef` row error.

### 2.4 `Valid` field tests

Three tests, all passing:

| # | Test | Result |
|---|---|---|
| 1 | Read 8 bytes at `#~+8` = `0x00000F093FB6FF57`; popcount gives 30 | pass |
| 2 | All 30 selected tables have a plausible row count; all 34 unselected tables are absent | pass |
| 3 | Reading only the low 4 bytes yields a *different*, inconsistent table set — reproducing the round 1–4 false positive | pass |

Test 3 is deliberately a **negative** control: it demonstrates the original error
rather than merely asserting its absence.

---

<a id="corrections-preserved"></a>
## 3. Corrections preserved
Earlier conclusions invalidated by later evidence are kept, not deleted. Knowing
what *not* to believe is part of the result.

| # | Earlier claim | Rounds | Resolution | Evidence |
|---|---|---|---|---|
| **C1** | The obfuscator scrambled the `MemberRef` parent column | 1–4 | **`WITHDRAWN`** — a 16-byte `MethodDef` row shifted every downstream offset | Round 5; both readers now agree on 1,588 rows |
| **C2** | The `Valid` bitmask was tampered with | 1–4 | **`WITHDRAWN`** — the field is 8 bytes; the reader truncated to 4 | §2.4 test 3 |
| **C3** | 35 reflection-related edges | 1–5 | **`WITHDRAWN`** — only **3** edges invoke code; the rest are lookups, `typeof`, or field/property access | Round 6 classification |
| **C4** | 3 update URLs | 1–5 | **`WITHDRAWN`** — there are **16** URL literals | Round 6 literal sweep |
| **C5** | `apps_vpn_filter` / `exclude_vpn_apps` are CIDR route-bypass settings | 1–5 | **`WITHDRAWN`** — both are properties of `Properties.Resources`, i.e. localisation strings | `.NET SRM` property list |
| **C6** | Update code execution is **CRITICAL** | 1–5 | **Downgraded to High** — the defect is proven, the launch edge is not | Round 6 call-graph sweep |
| **C7** | `ManifestResource` rows are embedded | 6 | **`WITHDRAWN`** — both rows carry `Implementation = 0x0061`, which decodes as **tag 1 = AssemblyRef, row 24**. The resources are declared to live in an *external* assembly under an obfuscated name, and the `File` table is absent so tag 0 is not selectable. See [ASSEMBLY_REFERENCES.md §5.1](ASSEMBLY_REFERENCES.md#51-assemblyref-24-is-the-declared-home-of-the-wpf-resources) | Raw bytes + two independent readers; [evidence/RESOURCE_ORIGIN_VERIFICATION.txt](../evidence/RESOURCE_ORIGIN_VERIFICATION.txt) |
| **C8** | `NETCONLib` has 9 types, `Utils` 37, `Utils.Wifi` 8 | 1–2 | **Confirmed, with a definition fix** — 305 `TypeDef` rows total; ~132 "global" counts only method-owning types | §4 |
| **C9** | `Resources` has 388 accessor methods | 1–2 | **Confirmed** — 388 methods, 386 properties | `.NET SRM` |
| **C10** | `MemberRef` count is 1,557 | 1–2 | **Corrected to 1,588** | Two readers agree |

### 3.1 Tooling errors caught in this work

These are errors in the analysis code, not findings about the binary. Each would
have produced a wrong conclusion.

| ID | Error | Wrong conclusion it would have produced | How it was caught |
|---|---|---|---|
| **T1** | `TypeRefOrDefResolutionScope` tag mapping assumed `1 = AssemblyRef` | "Zero `TypeRef` rows resolve to an `AssemblyRef`" → the entire 38-row dependency table reported as dead code | `.NET SRM` reported 621 |
| **T2** | `ImplMap.ImportScope` read as a `MemberRef` parent | Implausible module attribution for the native imports | Same |
| **T3** | `AssemblyRef` blob indices read as 4 bytes instead of 2 | Out-of-range blob reads; crash | Field-layout check against `HeapSizes` |
| **T4** | `AssemblyRef` name/culture offsets shifted | Empty assembly name | Raw byte dump |
| **T5** | PowerShell `-replace` with unescaped `\|` treated as regex alternation | Destroyed a 646 KB report | File size check |
| **T6** | Module row size assumed 18 bytes (`GUIDSZ=4`) instead of 12 | Wrong `TypeRef` base offset | `HeapSizes = 0x01` → `GUIDSZ = 2` |
| **T7** | `ManifestResource.Implementation` read as 4 bytes instead of 2 | Row 2 decoded as `0x20061` (row 32792) instead of `0x0061` (row 24) — pointing at a non-existent assembly rather than the real one | Cross-check against `RESOURCE_ORIGIN_VERIFICATION.txt` |

---

<a id="census-reconciliation"></a>
## 4. Census reconciliation
Two different type counts appear across the research. **Both are correct for
their definition.**

| Definition | Count | Note |
|---|---:|---|
| All `TypeDef` rows | **305** | The authoritative number |
| Types that own ≥ 1 method | **243** | |

Namespace census, counting **all 305 `TypeDef` rows** — sums to 305:

| Namespace | Types |
|---|---:|
| *(unnamed)* | 165 |
| `NetModMaterial.Utils` | 37 |
| `UI.Window` | 14 |
| `UI.Adapter` | 14 |
| `UI.Tab` | 10 |
| `Model` | 10 |
| `NETCONLib` | 9 |
| `UI.Dialog` | 9 |
| `Utils.Wifi` | 8 |
| `Service` | 5 |
| `Widget` | 5 |
| `Utils.Notification` | 3 |
| `UI.Menu` | 3 |
| `Properties` | 3 |
| `Migrations` | 3 |
| `NetModMaterial` | 2 |
| `System.Runtime.CompilerServices` | 2 |
| `Microsoft.CodeAnalysis` | 1 |
| `XamlGeneratedNamespace` | 1 |
| `Test` | 1 |
| **Total** | **305** |

Documents reporting ~132 unnamed types are counting only method-owning types.
This is a **definition** difference, not a contradiction — but it must be stated,
because publishing both without this note invites the reader to conclude one is
wrong.

---

## 5. Validator design and calibration

The IL validator used to decide whether a decoded body is real CIL.

### Design

| Property | Approach |
|---|---|
| Opcode table | `System.Reflection.Emit.OpCodes` from the .NET SDK — the authoritative table, not a hand-typed one |
| Operand widths | Taken from the same table |
| Branch targets | Validated against the body length |
| Stack discipline | Abstract interpretation with a merged stack at branch targets |
| `switch` count | Bounded; an absurd count is rejected rather than trusted |
| Verdict | Strict: any inconsistency marks the body invalid |

### Calibration — the step that makes it meaningful

A validator that rejects everything would produce the same result as a correct
one. So it was calibrated against **known-good** bodies before being trusted:

| Calibration set | Expected | Result |
|---|---|---|
| Bodies that the reference reader accepts | all valid | all valid |
| Bodies that are genuinely encrypted | all invalid | all invalid |
| Random byte strings (fuzz) | all invalid | all invalid |
| Truncated valid bodies | invalid | invalid |

**Pre-recovery it accepted 14 of 2,452 bodies** — consistent with the independent
manual review of the same round. That agreement is the calibration.

### Post-recovery

| Metric | Value |
|---|---|
| Bodies present | 2,239 |
| Fully decoded | 2,236 |
| Strongly inferred (not verified) | 1 |
| Unresolved | 3 |

---

## 6. Independent rechecking of raw bytes

Every significant claim ultimately rests on bytes that were re-read rather than
trusted from a cached intermediate.

| Claim | Re-read how |
|---|---|
| TLS callback is `ldc.i4.1 ; ret` | Body bytes read from file offset `0x0000EA65`; hashed independently |
| Both TLS bindings | `ldftn` operands read from `IL_00A6` and `IL_00C2` |
| No verification API anywhere | Sweep of **all 2,452** methods, not a sample |
| `AssemblyRef` table | Full 38-row dump, both readers, then a 345-assertion diff |
| `ImplMap` table | Full 34-row dump with raw bytes per row |
| Stage-2 output | Reproduced from the input; SHA-256 compared |
| Installer / executable identity | Hashes recomputed at publication time |

---

## 7. Evidence manifest generation

| Property | Approach |
|---|---|
| Order | Hashes computed **after** all documents were final, so the manifest describes the published state |
| Coverage | Every published file, plus the two source artifacts (recorded, **not** published) |
| Format | `SHA-256  path`, sorted, `sha256sum -c` compatible |
| Purpose | Allows a reader to detect later modification of this repository's claims |

See [evidence/HASH_MANIFEST.md](../evidence/HASH_MANIFEST.md).

---

## 8. Evidence preservation

| Property | Result |
|---|---|
| Installer SHA-256 | `075fcdf4…6ec44b` — matches the historical value |
| `NetMod.exe` SHA-256 | `60b1be47…f31743` — matches the historical value |
| Original artifacts | **Never modified, moved, renamed, repackaged or executed** |
| Research tree | Preserved byte-identical; verified by checksum after every round |
| Round 5 artefacts | Verified byte-identical after later rounds |

Corrections were recorded in **new** documents. No earlier report was edited, so
the record of what was believed when remains intact.

---

## 9. What validation does **not** cover

| Not validated | Why |
|---|---|
| That the application behaves as the code says | Nothing was executed |
| That the payloads and native helpers do what their names imply | 84 PE images catalogued, none analysed |
| That the findings are exploitable | No exploit was attempted or demonstrated |
| That the published docs match the binary for claims outside the audited set | Only the claims with a stated evidence reference were machine-checked |
| The round 1–4 reports' individual claims | Some were withdrawn rather than re-verified; see §3 |

> Validation establishes that the *published evidence* is accurate. It cannot
> establish that the *application* behaves as documented — see
> [LIMITATIONS.md](LIMITATIONS.md).