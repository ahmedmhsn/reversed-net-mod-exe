# Limitations

This document exists because the most important thing about this research is
where it stops. Read it before relying on any conclusion here.

---

## 1. Headline

> **The application is partially reverse-engineered.** The managed code of the
> main executable is ~99.96 % recovered and a validated 12,064-edge call graph
> exists. The protected embedded assembly, three method bodies, and **every**
> runtime behaviour remain unresolved. **No dynamic testing was performed.**

| Area | Status |
|---|---|
| Installer container and payload inventory | **CONFIRMED** |
| Managed metadata, type system, native imports | **CONFIRMED** (independent parsers agree) |
| Method bodies | **2,236 of 2,239** decoded; 1 strongly inferred; **3 unresolved** |
| Call graph | 12,064 edges, all validated; **0 inferred** |
| TLS behaviour | **CONFIRMED** at byte level |
| Update verification absent | **CONFIRMED**; launch relationship **UNRESOLVED** |
| Host network mutation and restoration | **CONFIRMED** for the orderly path |
| Embedded protected assembly | **UNRESOLVED** |
| All runtime behaviour | **UNRESOLVED** |

---

## 2. No dynamic analysis was performed

**Nothing was executed.** Not the installer, not the executable, not any
extracted payload, not any bundled third-party binary.

This is the single largest limitation, and it is structural:

* Analysis was conducted on an **everyday host**, where executing a
  network-manipulating client would modify the machine's routes, DNS, firewall
  and adapters. That was judged unacceptable and was not done.
* No **disposable, snapshotted virtual machine** was available in the
  environment. Without snapshot isolation, dynamic analysis of a client that
  installs routes and hijacks DNS is unsafe by definition.
* Network access to any vendor or third-party endpoint was out of scope.

### What that costs

| Cannot be stated | Why |
|---|---|
| What the client actually does on connect | Only the code path was read, never run |
| Whether the update payload is launched | Needs observation or complete async decoding |
| Whether host state survives a crash | [NF-03](../findings/confirmed/NF-03-default-gateway-restore.md) is design-level only |
| Whether the IPC receiver acts on a forged message | [NF-04](../findings/confirmed/NF-04-ipc-broadcast-channel.md) |
| Whether the `Settings` locks are enforced in the UI | Needs BAML/event-handler recovery or observation |
| Actual socket bind addresses | `127.0.0.1` appears as a literal; no socket was observed |
| Whether any bundled binary honours its contract | 84 PE images were catalogued, none executed |

**Every claim in this repository is therefore a claim about code, not about
observed behaviour.** Where the distinction matters — and it usually does — the
finding says so explicitly.

---

## 3. Unresolved technical items

| # | Item | Why it is open | Closing it |
|---|---|---|---|
| 1 | **Embedded FieldRVA assembly** | Stage 3 of the decode pipeline unresolved; LZMA range-coder invariants unsatisfied by stage-2 output | Memory dump after `<Module>.cctor`, or reimplementing ConfuserEx `0x06000017` |
| 2 | **`0x060002A8`** | Illegal method header `0x19`; called by `MetroWindowCustom` at `IL_000D` | Memory dump |
| 3 | **`0x06000691`** | Fails strict validation at IL offset 372; ≥2 dwords damaged. Decodes to offset 371, which covers the TLS `AuthenticateAsClient` call used by NF-01 | Memory dump |
| 4 | **`0x060006B0`** | Fails at IL offset 219; 41 non-unique candidate decodings | Memory dump |
| 5 | **Update launch step** | No edge to `ProcessRunner 0x06000254`; `0x0600028A` partially decoded | Walk the 1,313-byte state machine and its `MoveNext` continuation |
| 6 | **IPC receiver** | `WndProc` override not located; `GlobalGetAtomName` has no caller edge | Decode BAML / locate the handler |
| 7 | **`setup-0` installer payload** | Needs `LZMADecompSmall` reimplemented | Obtain `issrc` or `innoextract` sources |
| 8 | **10 obfuscated type names** | Replaced by zero-width characters; the original names are **not stored anywhere in the binary** | **Permanently unrecoverable** |
| 9 | **BAML / compiled XAML** | Not attempted — and **it is not in `NetMod.exe`**. Both `ManifestResource` rows resolve to AssemblyRef #24, so the UI resource set is almost certainly inside the encrypted payload (NF-07) | Decode the payload first, then its resources |
| 10 | **`.rsrc`, manifest, icon** | Not attempted | Not blocked — simply not done |

Items 1–4 share one fix. A single post-`.cctor` process memory dump closes all
four at once, which is why it is the highest-value experiment outstanding.

### A refuted hypothesis, preserved

It was hypothesised that the three residual bodies might be *valid* bodies whose
**declared** IL length is shorter than their actual length. This was tested and
**refuted**: of 506 candidate end lengths for `0x06000691`, **zero** produced a
body that validated. The bodies are genuinely damaged, not mis-described.

---

<a id="deliberate-non-goals"></a>
## 4. Deliberate non-goals
Some things were **not attempted on purpose**, and their absence is a scope
decision, not a difficulty:

| Not attempted | Reason |
|---|---|
| **HWID derivation** | Reproducing it would serve only to bypass licensing. The schema and the presence of the field are documented; the algorithm is not. |
| **Licence forgery** | Out of scope by instruction and out of scope ethically. |
| **Payload execution / exploitation** | No proof-of-concept exists in this repository, by design. |
| **Bypassing or patching the anti-tamper** | The protection was **analysed**, not defeated in a shipped sense. Nothing here modifies a product. |
| **Analysis of the bundled third-party binaries' internals** | Separate codebases with separate licences. Their identity and role are catalogued; their code is not published or claimed. |

---

## 5. Scope boundaries

### In scope, and covered

The installer container; the payload inventory; the `NetMod.exe` managed
assembly (metadata, IL, call graph, configuration schema, native imports); the
protection mechanism and how it was analysed.

### Out of scope, and not claimed

* Behaviour of `v2ray.exe`, `ssr-local.exe`, `plink.exe`, `pdnsd.exe`,
  `dnstt-client.exe`, `badvpn-tun2socks.exe` — **identity only**.
* The TAP driver and its installer — **identity only**.
* The 59 bundled managed libraries — **`AssemblyRef` entries only**.
* The WiX Burn bundle's install-time registry and service layout — **not decoded**.
* The server side of any protocol — not present in the artifact.

### Licensing and redistribution

The original installer, the original executable, the extracted third-party
binaries and any decompilation are **not redistributed** here. What is published
is original documentation. See [SECURITY.md](../SECURITY.md).

---

## 6. Reliability of the published numbers

| Number | Confidence |
|---|---|
| 305 type definitions | High — three parsers agree |
| 2,452 method definitions | High — `.NET` reader and Python walker agree |
| 38 assembly references | High — 345 cross-check assertions, 0 failures |
| 34 native imports | High — full table decoded |
| 2,236 / 2,239 method bodies decoded | High — reproduced, with a validator calibrated against known-good bodies |
| 12,064 call-graph edges | High — 0 bad tokens, no inferred edges |
| Per-type method/field/property counts | High — per-type method counts sum to exactly 2,452 |

Two caveats on reading the numbers:

* **Census definitions differ between documents.** There are 305 `TypeDef` rows
  in total, of which **243 own at least one method**. Reports listing ~132
  "global" types are counting only method-owning types. Both are correct for
  their definition; see [VALIDATION.md](VALIDATION.md#census-reconciliation).
* **Names, not functionality.** Where a component's responsibility is stated,
  that is an inference from its members and call edges. A type named `VpnService`
  that manipulates routing tables is consistent with its name, but the name
  itself proves nothing.

---

<a id="what-would-close-the-open-questions"></a>
## 7. What would close the open questions
| Priority | Action | Closes |
|---|---|---|
| **1** | Post-`.cctor` process memory dump in a disposable VM | Items 1–4 above, [NF-06](../findings/unresolved/NF-06-update-payload-launch.md), [NF-07](../findings/unresolved/NF-07-embedded-assembly-gap.md), the IPC receiver, and several architecture gaps |
| **2** | Walk `0x0600028A` (1,313 bytes) instruction by instruction | Update launch step |
| **3** | Decode the FieldRVA payload, then **its** `NetMod.g.resources` → BAML | WPF event bindings; the `*_lock` enforcement question; missing call-graph edges |
| **4** | Reimplement ConfuserEx `LzmaDecoder` (`0x06000017`) in C# | Necessary for item 1; may be sufficient |
| **5** | Decode BAML to locate the `WndProc` override | IPC receiver |
| 6 | Reimplement `LZMADecompSmall` | `setup-0` payload |

Items 2–5 are feasible statically today. Item 1 requires the analysis VM that
was not available.

---

## 8. Honest statement

This research produced a reliable picture of **what the code says**. It did not
produce a picture of **what the program does**, because nothing was run.

That boundary is not a hedge. It is the actual state of the work, and the
distinction between the two is applied consistently in every document here.