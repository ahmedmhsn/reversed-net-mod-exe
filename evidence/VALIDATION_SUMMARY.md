# Validation Summary

One-page summary of what was checked. Full detail:
[../docs/VALIDATION.md](../docs/VALIDATION.md).

---

## Headline

| | |
|---|---|
| **Independent implementations** | 3 (Python model, .NET `SRM`, from-scratch walker) |
| **Cross-check assertions** | **345, 0 failures** |
| **Consistency invariants** | 7, all passing |
| **Findings published** | 8 (5 `CONFIRMED`, 1 `CONFIRMED`/informational, 2 `UNRESOLVED`) |
| **Withdrawn claims** | 7, all preserved rather than deleted |
| **Tooling errors caught** | 6 |
| **Method bodies decoded** | 2,236 of 2,239 (99.96 %) |
| **Dynamic experiments executed** | **0** |

---

## Core counts — two readers agree

| Quantity | Value |
|---|---:|
| `TypeDef` | 305 |
| `MethodDef` | 2,452 |
| `TypeRef` | 626 |
| `MemberRef` | 1,588 |
| `TypeSpec` | 176 |
| `AssemblyRef` | 38 |
| `ImplMap` | 34 |
| `Property` | 765 |
| Per-type method counts, summed | **2,452** ✓ |
| Per-namespace type counts, summed | **305** ✓ |

---

## Invariants

| # | Invariant | Result |
|---|---|---|
| 1 | Per-type method counts sum to the `MethodDef` row count | pass |
| 2 | Per-namespace type counts sum to the `TypeDef` row count | pass |
| 3 | `MethodList` monotonic across `TypeDef` rows | pass (0 violations) |
| 4 | `Valid` mask (8 bytes) consistent with all 30 tables | pass |
| 5 | `#~` walk within stream bounds | pass (2 bytes padding) |
| 6 | FieldRVA stage-2 output hash | pass |
| 7 | TLS callback body hash | pass |

Invariant 1 is the load-bearing one: a mis-parsed metadata layout shifts the
`MethodList`/`FieldList` ranges and the sum stops equalling 2,452 immediately.

---

## Corrections preserved

| Earlier claim | Resolution |
|---|---|
| `MemberRef` parents were scrambled | **`WITHDRAWN`** — a 16-byte `MethodDef` row shifted the offsets |
| The `Valid` mask was tampered with | **`WITHDRAWN`** — the field is 8 bytes; the reader truncated to 4 |
| 35 reflection edges | **`WITHDRAWN`** — only **3** invoke code |
| 3 update URLs | **`WITHDRAWN`** — there are **16** |
| `apps_vpn_filter` is a CIDR bypass setting | **`WITHDRAWN`** — a localisation string |
| Update RCE is CRITICAL | **Downgraded to High** — the launch edge is unproven |
| `ManifestResource` rows are embedded | **`WITHDRAWN`** — `Implementation` index is non-zero |

---

## Tooling errors caught

| # | Error | Wrong conclusion avoided |
|---|---|---|
| T1 | Coded-index tag mapping assumed from spec | Would have reported the entire 38-row dependency table as dead code |
| T2 | `ImplMap.ImportScope` read as a `MemberRef` parent | Wrong module attribution for native imports |
| T3 | `AssemblyRef` blob index width | Out-of-range reads |
| T4 | `AssemblyRef` name/culture offsets | Empty assembly name |
| T5 | PowerShell `-replace` with unescaped `\|` | Destroyed a 646 KB report |
| T6 | Module row size assumed from the wrong `GUIDSZ` | Wrong `TypeRef` base offset |

---

## The one that matters

**T1** is the instructive one. The primary Python walker reported that **zero**
`TypeRef` rows resolved to an `AssemblyRef`. Taken at face value that means all 38
assembly references were dead metadata — a striking, publishable claim that was
simply wrong.

The independent .NET reader reported **621**. The cause was an assumed coded-index
tag mapping that does not match this image. Once the measured mapping was used
(`tag 2 = AssemblyRef`, `tag 3 = TypeRef`), the two implementations agreed
exactly.

It is recorded here because it is the characteristic failure mode of hand-written
metadata readers: **a confidently wrong answer that looks completely plausible.**
Cross-checking against a second implementation is the only thing that caught it.

---

## What was **not** validated

| Not validated | Why |
|---|---|
| That the application behaves as the code says | Nothing was executed |
| That bundled binaries do what their names imply | 84 PE images catalogued, none analysed |
| That any finding is exploitable | No exploit attempted or demonstrated |
| Claims outside the audited set | Only claims with a stated evidence reference were machine-checked |

> Validation establishes that the **published evidence** is accurate. It cannot
> establish that the **application** behaves as documented.
> See [../docs/LIMITATIONS.md](../docs/LIMITATIONS.md).