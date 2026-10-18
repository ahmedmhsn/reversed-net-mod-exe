# Evidence Index

What each published claim rests on, and how to check it.

---

## 1. Artifact provenance

| Artifact | SHA-256 | Status |
|---|---|---|
| Installer | `075fcdf4209d8182131e866c551ca28105137605d98bf0a64e650606b76ec44b` | **Verified at publication time**; matches the value recorded during the research |
| `NetMod.exe` | `60b1be47f1e74b8283fc31ee72ea07a808ea7d941515cf3a3a444bf126f31743` | **Verified at publication time**; matches the value recorded during the research |
| Installer size | 33,509,274 bytes | Verified |
| `NetMod.exe` size | 625,664 bytes | Verified |

> **Neither artifact is redistributed by this repository.** See
> [../SECURITY.md](../SECURITY.md).

---

## 2. Evidence by claim

Each row names the strongest evidence for a published claim and how a reader can
re-check it.

| Claim | Evidence | How to verify |
|---|---|---|
| .NET Framework 4.7.2, x86, WPF | COR20 header + `Assembly` row + `TargetFrameworkAttribute` | `tools/SrmProbe` |
| Assembly version 4.6.0.415, not strong-named | `Assembly` row: `PublicKey` absent, strong-name RVA 0/0 | `tools/SrmProbe` |
| 305 types / 2,452 methods / 626 `TypeRef` / 1,588 `MemberRef` | Metadata tables | `tools/SrmProbe` |
| 38 assembly references, all fields | Full `AssemblyRef` table at file offset `0x508AE` | `tools/SrmProbe`, or a manual walk |
| 34 native imports | Full `ImplMap` table at file offset `0x5073A` | Manual walk; per-row bytes published in [../docs/ASSEMBLY_REFERENCES.md](../docs/ASSEMBLY_REFERENCES.md) |
| No satellite assemblies; no `HashValue` | `Culture` and `HashValue` empty on all 38 rows | `tools/SrmProbe` |
| TLS callback is `ldc.i4.1 ; ret` | 2 bytes `17 2a` at file offset `0x0000EA65`, RVA `0x00010665`, both tokens `0x06000457` / `0x06000692` | Read 2 bytes at that offset; hash `3e652f9e…1b378` |
| Both TLS bindings located | `ldftn` at `IL_00A6` and `IL_00C2` | IL walk of the two enclosing methods |
| No verification API anywhere | Sweep of **all 2,452** methods | Method-by-method enumeration |
| Update URLs: 16 literals | Literal sweep of the `#US` heap | Enumerate user strings |
| IPC uses `HWND_BROADCAST` | Literal `0xFFFF` at `IL_0037`–`IL_0095` in `0x06000075` | IL walk |
| Message ids are static fields | `ldsfd` operands, not `RegisterWindowMessage` | IL walk |
| Only `Process.Start` site is `0x06000254` | Sweep of all 2,452 methods | Enumerate `call`/`callvirt` targets |
| No edge from update methods to `0x06000254` | Call-graph search | Full call graph |
| 12,064 call-graph edges, 0 bad tokens | Graph construction + validation | Graph export |
| 2,236 of 2,239 bodies decoded | Strict CIL validator, calibrated | Re-run validator |
| Three residual bodies | `0x060002A8`, `0x06000691`, `0x060006B0` | Re-run validator |
| FieldRVA stage 2 reproduced | SHA-256 `d22cb79d…25e9e` | Re-run stage 2 |
| Stage 3 unresolved | LZMA range-coder invariant violated at offsets 0/5/9 | 8,100 configurations tried, 0 successes |
| 60 managed images / 84 PE images in payload | Artifact inventory, `is_managed` flags | Installer payload scan |
| 32 `Settings` keys / 17 `ConfigSettings` keys | Property table via `.NET SRM` | `tools/SrmProbe` |
| 10 zero-width type names | `TypeDef` names outside printable ASCII | `.NET SRM` with escaping |
| WPF resources are **not** in `NetMod.exe`; both `ManifestResource` rows resolve to AssemblyRef #24 | `Implementation = 0x0061` → tag 1 = AssemblyRef, row 24; the `File` table is absent so tag 0 is not selectable | [RESOURCE_ORIGIN_VERIFICATION.txt](RESOURCE_ORIGIN_VERIFICATION.txt) |
| Embedded API key | String literal at `0x060001B1` — **value not published** | Presence only |

---

## 3. Evidence classes

| Class | Meaning |
|---|---|
| **Raw bytes** | A file offset and a hash. Re-checkable with any hex editor |
| **Metadata table** | A decoded table with a stated geometry. Re-checkable with a reader |
| **Recovered IL** | A decoded method body. Re-checkable with a disassembler, subject to the geometry |
| **Call-graph edge** | Derived from an instruction operand. **No edge in this repository is inferred** |
| **Inference** | A reading of evidence, labelled `INFERRED` at the point of use |
| **Not evidence** | Runtime behaviour. Nothing here is supported by observation |

The last row matters: **no claim in this repository is supported by running the
software.**

---

## 4. What is deliberately absent

| Item | Reason |
|---|---|
| The installer and the executable | Redistribution rights are not ours; value to a reader is near zero given the hash is published |
| The 84 extracted PE images | Include proprietary vendor binaries and third-party redistributables |
| Any third-party DLL | Independently licensed; not ours to redistribute |
| The embedded API key value | A live third-party credential. **Never reproduced in any artefact** |
| Any decompilation output | Not published by default; the architecture documentation is the deliverable |
| Any proof-of-concept | Deliberate. See [../SECURITY.md](../SECURITY.md) |
| The IL decryptor and FieldRVA stage-2 code | Protection-defeating; generalisable to other targets |
| Personal paths and machine names from build artifacts | Analyst environment, not evidence |
| Local usernames and host names | Analyst environment, not evidence |

---

## 5. Cross-check record

| Comparison | Assertions | Failures |
|---|---:|---:|
| `AssemblyRef` inventory — Python vs .NET `SRM` | 228 | 0 |
| `TypeRef` resolution-scope histogram | 2 | 0 |
| Per-assembly `TypeRef` usage | 115 | 0 |
| **Total** | **345** | **0** |

| Invariant | Result |
|---|---|
| Per-type method counts sum to `MethodDef` row count (2,452) | pass |
| Per-namespace type counts sum to `TypeDef` row count (305) | pass |
| `MethodList` monotonic across `TypeDef` rows | pass, 0 violations |
| `Valid` mask consistent with all 30 tables | pass |
| Stage-2 FieldRVA output hash | pass |
| TLS callback body hash | pass |

Detail: [../docs/VALIDATION.md](../docs/VALIDATION.md).

---

## 6. Redaction register

One redaction was applied.

| Item | Type | Location | Risk if published | Action |
|---|---|---|---|---|
| Third-party geo-IP API key | **Live third-party credential** | String literal referenced from `0x060001B1` | Enables unauthorized use of a third party's paid service; reputational and financial exposure for that vendor | Value recorded by **type and location only**. Verified absent from every published file by automated scan |

No other secret, token, password, cookie, connection string or private key was
found in the published content. Personal names, machine names, local usernames and
analyst build paths were removed where they carried no evidentiary value.

> Note: several Windows `PDB` paths from the vendor's own build machines appear in
> the *research* artefacts (for example paths under a developer's home directory).
> Those are evidence of which third-party libraries were compiled and shipped, so
> their **meaning** is used, but the paths themselves are **not** reproduced.

---

## 7. Integrity of the research record

| Property | Result |
|---|---|
| Original artifacts | Never modified, moved, renamed, repackaged or executed |
| Research tree | Preserved byte-identical across all rounds; verified by checksum |
| Corrections | Recorded in **new** documents; no earlier report was edited |
| Publication | A separate workspace; the research tree was not used as the publish root |

---

## 8. File manifest

Every published file and its SHA-256: [HASH_MANIFEST.md](HASH_MANIFEST.md).
Verification summary: [VALIDATION_SUMMARY.md](VALIDATION_SUMMARY.md).