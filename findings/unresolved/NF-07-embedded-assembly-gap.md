# NF-07 — A 47,936-byte encrypted payload is loaded into the process: UNRESOLVED

| | |
|---|---|
| **Classification** | `UNRESOLVED` |
| **Severity** | — (**not rated**) |
| **Status** | **Open** |
| **Component** | `<Module>.cctor` (`0x06000001`) → `FieldRVA` → xorshift32 → LZMA → `Assembly.Load` |
| **Confidence** | High that the load occurs; **zero** knowledge of what the assembly does |

---

## Finding, stated as far as the evidence goes

At start-up, the module initialiser decodes an embedded **47,936-byte** blob
through a three-stage pipeline and loads the result into the running process with
`Assembly.Load`, registering an `AppDomain.AssemblyResolve` handler to serve it.

**What that assembly contains and does is unknown.** Its final decoding stage was
not recovered.

## Evidence — the pipeline

| Stage | Method | Role | Status |
|---|---|---|---|
| 1 | `0x06000007` | xorshift32 + a rolling 16-dword delta over the `FieldRVA` data | **`CONFIRMED`, reproduced byte-identically** |
| 2 | `0x06000006` | LZMA framing + decode | **`CONFIRMED` for framing**; framing determined from `0x06000006` |
| 3 | *(unknown)* | The final transform | **`UNRESOLVED`** |
| load | — | `Assembly.Load` + `AppDomain.AssemblyResolve` | **`CONFIRMED`** as callsite |

Stage-2 output hash, reproducible from the input:
`d22cb79d015107a1d6d7dea553be76767df3cac4edab691a7baf55182ed25e9e`.

### Why stage 3 resisted

LZMA range-coder framing requires specific bytes at the start of the stream. The
stage-2 output **has no `0x00` byte at offsets 0, 5 or 9**, which an LZMA stream
requires. 8,100 plausible decoder configurations were tried; none succeeded.

Two readings remain open:

* the stage-2 output is not yet the LZMA input — i.e. stage 3 is a further
  transform; or
* the LZMA decoder is reimplemented from ConfuserEx `0x06000017` in a way that
  differs from the standard.

Reimplementing the ConfuserEx decoder is **necessary but was not sufficient**.

## Why it matters more than "an unknown helper"

`NetMod.exe` declares **no embedded managed resources** of its own, and the
metadata says exactly where they are instead.

Both `ManifestResource` rows carry `Implementation = 0x0061`. Per ECMA-335
II.22.24 that decodes as **tag 1 = AssemblyRef, row 24** — the same obfuscated
name `PJrcRGGDZySncSGQBDxtMlBJuXsaA` — and the `File` table is absent, so tag 0
is not selectable.

| Row | Resource | Where it is declared to live |
|---:|---|---|
| 1 | `NetMod.g.resources` | AssemblyRef #24 (external) |
| 2 | `NetModMaterial.Properties.Resources.resources` | AssemblyRef #24 (external) |

`NetMod.g.resources` is the conventional container for compiled WPF BAML. Combined
with the `AppDomain.AssemblyResolve` handler that serves the FieldRVA payload,
this makes it very likely that the encrypted blob carries **the application's
entire compiled UI resource set** — not an incidental helper.

*(`CONFIRMED`* for the metadata link; *`INFERRED`* that #24 **is** the payload,
which would require decoding stage 3 and reading the loaded assembly's name.)*

This also resolves an earlier puzzle: several rounds could not find BAML to
decode. **It was never in `NetMod.exe`.**

Full evidence: [evidence/RESOURCE_ORIGIN_VERIFICATION.txt](../../evidence/RESOURCE_ORIGIN_VERIFICATION.txt).

## What would close it

| Approach | Feasibility |
|---|---|
| **Dump process memory after `<Module>.cctor` returns** | Decisive — needs a disposable analysis VM |
| Reimplement ConfuserEx `LzmaDecoder` (`0x06000017`) to C# and retry | Feasible; expected to be necessary but may not be sufficient |
| Recover the stage-3 parameters from the constant pool | Feasible; not yet attempted |

The memory dump alone closes this, [NF-06](../unresolved/NF-06-update-payload-launch.md),
and all three residual method bodies at once. It is the highest-value experiment
that was not performed.

## Why this is not rated

A dynamically-loaded assembly could be benign (a licence check), a deliberate
hardening measure, or a genuine backdoor. **Nothing in the recovered evidence
distinguishes these**, and a severity rating would be invented rather than
derived.

The correct description is: *an unaudited component executes inside the client's
process, in the same security context, and cannot be inspected from the shipped
binary.* That is a real and reportable weakness — an **audit gap**, not a proven
compromise.

## Risk framing

| Question | Answer |
|---|---|
| Does code from this blob run in the client process? | `CONFIRMED` |
| With what privileges? | The client's own |
| What does it do? | **Unknown** |
| Could it install a process-wide `HttpClientHandler` policy, changing behaviour for every outbound request? | **Possible — not demonstrated** |
| Was it analysed? | **No** |

## References

* [ARCHITECTURE.md §3.12](../../docs/ARCHITECTURE.md)
* [component-architecture.mmd](../../architecture/component-architecture.mmd)
* [LIMITATIONS.md](../../docs/LIMITATIONS.md)