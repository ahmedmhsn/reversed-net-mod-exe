# Reverse-Engineering Method

How the analysis was performed, what made it hard, and what was found along the
way — including the errors, because those are the parts a reader replicating the
work needs most.

> **Safety posture for the whole investigation:** read-only access to the
> artifact; nothing was executed; no endpoint was contacted; no host route, DNS,
> registry, service, driver or firewall setting was altered; all output was
> written to a separate workspace, leaving the evidence byte-identical.

---

## 1. The problem

The file under analysis looked ordinary. It was not.

A strict CIL validator, applied to the method bodies as stored, found only **14**
genuine bodies out of **2,452**. The rest were ciphertext occupying valid RVAs.
Decompilers produced garbage, and the .NET runtime itself would throw
`BadImageFormatException` on 47 corrupted method RVAs.

The cause: a module initialiser runs **before any application code**, makes the
code section writable with `VirtualProtect`, and rewrites each method body in
place so the CLR can JIT it.

Everything else in this repository depends on defeating that first.

---

## 2. Investigation progression

| Round | Contribution | Outcome |
|---|---|---|
| 1–2 | Installer decoding; payload inventory; managed metadata characterisation | Protection identified; only 14/2,452 bodies readable |
| 3 | First IL-recovery attempt | **Failed** — established that the transform itself was the blocker |
| 4 | **Recovered the anti-tamper transform and key schedule** | IL decryption became possible |
| 5 | **Corrected a metadata-layout error** | Unlocked the whole type system; enabled the call graph |
| 6 | Independent audit | 1 refutation, 3 corrections, 2 findings tightened |
| 7 | Publication + AssemblyRef audit | 38 references validated by two implementations; several component counts corrected |

---

## 3. Stage 1 — beating the IL encryption

### 3.1 What was found

| Component | Evidence |
|---|---|
| Decryptor | `<Module>.cctor` (`0x06000001`) calls `0x0600000A` — 745 bytes, 462 instructions |
| Memory permission | Two `VirtualProtect` declarations (`0x06000004`, `0x06000009`) |
| RWX section | First section `Wc9A_'CW`, characteristics `0xE0000040` |
| Anti-disassembly | `SuppressIldasmAttribute`, first `MemberRef`, token `0x0A000001` |
| Metadata poisoning | Impossible call targets, e.g. `newobj` → `0x16064457` |
| Renaming | 10 type names replaced by zero-width characters |

### 3.2 Recovering the transform

The decryptor's IL was decoded by hand and its structure identified: a per-method
pass over the body table applying a position-dependent transform under a key
derived from a fixed schedule. The recovered transform was matched against
open-source **ConfuserEx anti-tamper, Normal mode**.

> **Attribution is `INFERRED`.** No version signature was found, so the tool
> identity is a best match, not a proof.

Once the transform and key schedule were implemented, bodies decrypted
reproducibly. That single step moved recovery from 14/2,452 to ~99.8 %.

### 3.3 Recovered method-body counts

| Metric | Value |
|---|---|
| `MethodDef` rows | 2,452 |
| Bodies present | 2,239 |
| **Fully decoded** | **2,236** (99.96 %) |
| Strongly inferred, not verified | 1 (`0x060003D1`) |
| Unresolved | 3 |

The 2,238 encrypted bodies from the pre-decryption count include bodies that later
proved to be compiler-generated artefacts.

### 3.4 The three residual bodies

| Token | Problem | Status |
|---|---|---|
| `0x060002A8` | Illegal method header `0x19` | **UNRESOLVED** |
| `0x06000691` | Fails strict validation at IL offset 372 (≥2 dwords damaged) | **UNRESOLVED** beyond 371 |
| `0x060006B0` | Fails at IL offset 219; 41 non-unique candidates | **UNRESOLVED** |

Importantly, `0x06000691` decodes correctly **through offset 371**, which covers
the `AuthenticateAsClient` call that [NF-01](../findings/confirmed/NF-01-tls-certificate-validation.md)
depends on. The finding is therefore sound despite the body being partially
damaged.

#### A refuted hypothesis, preserved

It was hypothesised that these bodies might be *valid* bodies whose **declared**
IL length is too short. This was tested and **refuted**: of 506 candidate end
lengths for `0x06000691`, **zero** produced a valid body. They are genuinely
damaged, not mis-described.

---

## 4. Stage 2 — getting the metadata right

### 4.1 The error that cost three rounds

For rounds 1–4, two parsers disagreed about the `MemberRef` parent column. The
conclusion drawn was that **the obfuscator had scrambled it**.

That was wrong. The table had been read at the wrong offset because the
**`MethodDef` row size is 16 bytes, not the 8 specified by ECMA-335**. Every
downstream table position was consequently off by a cumulative amount.

Once the row size was corrected, the entire type system resolved cleanly.
**The obfuscator had not corrupted anything.**

### 4.2 Validated metadata geometry

```
CLI header  → COR20 header (file offset 0x33A00, 72 bytes)
           → Metadata root (RVA 0x3642C)
           → stream headers: #~ #Strings #US #GUID #Blob

#~ header (file offset 0x33E98, 24 bytes)
  +0   Reserved       (4)
  +4   MajorVersion   (1)   = 2
  +5   MinorVersion   (1)   = 0
  +6   HeapSizes      (1)   = 0x01  → STRSZ=4, GUIDSZ=2, BLOBSZ=2
  +7   Reserved       (1)
  +8   Valid          (8)   = 0x00000F093FB6FF57   ← 8 BYTES, see §4.3
  +16  Sorted         (8)
  +24  RowCounts      (4 × number of present tables = 30 → 120 bytes)
  +144 table data
```

| Table | File offset | Row size | Rows |
|---|---|---:|---:|
| `Module` (`0x00`) | 0x33F28 | 18 | 1 |
| `TypeRef` (`0x01`) | 0x33F34 | 10 | 626 |
| `TypeDef` (`0x02`) | 0x33F3A | 18 | 305 |
| `MethodDef` (`0x06`) | — | **16** | 2,452 |
| `MemberRef` (`0x0A`) | — | 8 | 1,588 |
| `ImplMap` (`0x1C`) | 0x5073A | 10 | 34 |
| `AssemblyRef` (`0x23`) | 0x508AE | 24 | 38 |

The walk ends at `0x511DE`; the `#~` stream ends at `0x511E0` — 2 bytes of slack,
consistent with row padding.

### 4.3 The `Valid` field is not tampered

Rounds 1–4 reported that the `Valid` bitmask had been modified by the
obfuscator. **It had not.**

ECMA-335 II.24.2.6 specifies a 4-byte `Valid`. The reference implementation does
**not** truncate: the field is **8 bytes** here, and its value
`0x00000F093FB6FF57` correctly selects the 30 present tables. Reading 4 bytes and
comparing against an expected value produced a spurious "tampering" finding.

Three tests now cover this; see [VALIDATION.md](VALIDATION.md).

<a id="metadata-anomalies"></a>
### 4.4 Non-standard geometry, summarised
| Deviation | Standard | Actual | Consequence |
|---|---|---|---|
| `MethodDef` row size | 8 bytes | **16 bytes** | All downstream table offsets shift |
| `Valid` field | 4 bytes | **8 bytes** | Truncation produces a false "tampered" reading |

Both are properties of the protection tool, not of the original source.

---

## 5. Stage 3 — the call graph

| Property | Value |
|---|---|
| Edges | **12,064** |
| Nodes | 2,452 |
| Bad tokens | 0 |
| **Inferred edges** | **0** |
| Proven control flow (`call`/`newobj`/`ldftn`) | 4,717 (39.1 %) |
| Runtime-resolved (`callvirt`) | 4,071 (33.7 %) |
| Field edges | 3,252 |

No edge is guessed. `callvirt` edges record the *declared* target; whether that
target executes depends on the runtime type, which is why the two categories are
reported separately and why 33.7 % of edges are labelled runtime-resolved rather
than proven.

A useful invariant: the per-type method counts obtained from the type ownership
ranges sum to **exactly 2,452**, matching the `MethodDef` row count. A mis-parsed
layout fails this check immediately.

---

## 6. Independent verification

Three implementations were used, sharing no code:

| Implementation | Role |
|---|---|
| Python metadata + IL model (round 5) | Primary analysis |
| .NET `System.Reflection.Metadata` probe | **Independent oracle** |
| From-scratch Python layout walker | **Independent geometry check** |

The oracle deliberately **reports** failures rather than suppressing them — an
`SRM` exception is itself treated as a finding.

### 6.1 A disagreement the oracle caught

During the AssemblyRef audit the Python walker reported that **zero** `TypeRef`
rows resolved to an `AssemblyRef` — implying the entire reference table was dead
code. The .NET reader reported **621**.

The Python walker was wrong. `TypeRefOrDefResolutionScope` packs `(row << 2) | tag`,
and the mapping assumed from the specification text (`tag 1 = AssemblyRef`) does
not match this image, where the observed mapping is **`tag 2 = AssemblyRef`,
`tag 3 = TypeRef`**. With the measured mapping, the walker reproduced the
reference reader's histogram exactly.

A second error in the same session — reading `ImplMap.ImportScope` as a
`MemberRef` parent rather than a `ModuleRef` index — produced a plausible but
wrong module attribution and was caught the same way.

> **Both errors would have produced wrong conclusions** had they not been
> cross-checked. They are recorded here because they are the characteristic traps
> of hand-written metadata readers.

### 6.2 Cross-check result

| Comparison | Assertions | Failures |
|---|---:|---:|
| AssemblyRef inventory: name, version, flags, public-key token length **and value**, culture, hash length, all 38 rows | 228 | 0 |
| `TypeRef` resolution-scope histogram | 2 | 0 |
| Per-assembly `TypeRef` usage | 115 | 0 |
| **Total** | **345** | **0** |

Reproduce with `tools/SrmProbe` (see [tools/README.md](../tools/README.md)).

---

## 7. The second protection: an embedded assembly

A distinct mechanism, tracked separately from IL encryption:

| Stage | Method | Role | Status |
|---|---|---|---|
| 1 | `0x06000007` | xorshift32 + rolling 16-dword delta | **`CONFIRMED`, byte-identical reproduction** |
| 2 | `0x06000006` | LZMA framing + decode | **`CONFIRMED`** for framing |
| 3 | — | Final transform | **`UNRESOLVED`** |
| load | — | `Assembly.Load` + `AppDomain.AssemblyResolve` | **`CONFIRMED`** |

Stage-2 output, reproducible from the input, hashes to
`d22cb79d015107a1d6d7dea553be76767df3cac4edab691a7baf55182ed25e9e`.

### Why stage 3 resisted

LZMA range-coder framing requires specific bytes at the start of the stream. The
stage-2 output **has no `0x00` byte at offsets 0, 5 or 9**, which an LZMA stream
requires. **8,100 decoder configurations were tried; 0 succeeded.**

Two readings remain: either the stage-2 output is not yet the LZMA input (a
further transform exists), or the LZMA decoder differs from the standard.
Reimplementing ConfuserEx `0x06000017` is **necessary but was not sufficient**.

Framing was determined from `0x06000006`: `[0..4]` properties, `[5..8]` dictionary
size, `[9..]` compressed stream.

---

<a id="reproduction"></a>
## 8. Reproduction
### 8.1 Prerequisites

| Tool | Version used |
|---|---|
| Python | 3.12 (msys2 ucrt64) |
| .NET SDK | **9.0.319** (runtime 9.0.21) |
| 7-Zip | 26.02 x64 |
| REA MCP — Ghidra | 12.1.4 |
| REA MCP — `rea-dotnet-static` | v1 |

### 8.2 Minimal reproduction — the metadata geometry

Nothing below requires the original installer, and nothing executes the target.

```python
# Read-only: locate the #~ header and print the table geometry.
import struct
raw = open("NetMod.exe", "rb").read()          # never executed

e = struct.unpack_from("<I", raw, 0x3C)[0]
# ... map RVA -> file offset via the section table ...
# COR20 at file offset 0x33A00; MetaData RVA at COR20+8, size at COR20+12
# #~ stream header: 0x33E98
tbl = 0x33E98
heapsizes = raw[tbl + 6]                       # 0x01 here
valid = struct.unpack_from("<Q", raw, tbl + 8)[0]   # 8 bytes, not 4
tables = [i for i in range(64) if (valid >> i) & 1]
print(len(tables), "tables present")
```

Expected: `HeapSizes = 0x01`, `valid = 0x00000F093FB6FF57`, **30 tables present**.

### 8.3 Full cross-check

```
cd tools/SrmProbe
dotnet run -c Release -- <path-to-NetMod.exe> output.txt
```

Expected: `TypeDef=305`, `MethodDef=2452`, `AssemblyRef=38`, and
`per-type method sum = 2452`.

### 8.4 Stage-1 reproduction

The IL decryptor and the xorshift32/FieldRVA stage-2 implementation are **not
published**. They are protection-defeating code specific to this artifact;
publishing them would provide a ready-made tool for the same protection in
general. The methodology above is sufficient to reproduce the *metadata* and
*call-graph* results, which is where most published claims come from.

### 8.5 Known tool limitations encountered

| Limitation | Effect | Handling |
|---|---|---|
| Hand-written readers diverge from the reference reader on coded-index tag mappings | Would have reported the dependency table as dead | Cross-check against `.NET SRM` |
| `ImplMap.ImportScope` is a `ModuleRef` index, not a `MemberRef` parent | Wrong module attribution | Fixed; full table re-decoded |
| `AssemblyRef` blob/string index widths differ from the version/flags widths | Off-by-2 field reads | `Blob = 2`, `String = 4` from `HeapSizes` |
| PowerShell `-replace` treats unescaped `|` as regex alternation | Destroyed a 646 KB report | Rewritten; **logged as tooling error T5** |
| Console encoding is cp1256 | Non-ASCII output corrupted | Non-ASCII written to files, never printed |

---

## 9. Evidence integrity

| Property | Result |
|---|---|
| Installer SHA-256 | Recomputed, matches the historical value |
| `NetMod.exe` SHA-256 | Recomputed, matches the historical value |
| Original artifact | **Never modified, moved, renamed or executed** |
| Research tree | Preserved byte-identical; verified by checksum |
| All output | Written to a separate workspace |

Full manifest: [evidence/HASH_MANIFEST.md](../evidence/HASH_MANIFEST.md).

---

## 10. What this method does not establish

Method recovery gives you the **code**, not the **behaviour**. Everything in this
repository is a claim about what the code says. Whether it does what it says
requires execution, which was deliberately not performed. That boundary is
applied consistently in every document, and
[LIMITATIONS.md](LIMITATIONS.md) states exactly which questions depend on it.