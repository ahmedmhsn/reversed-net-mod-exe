# Overview

## 1. Subject

**NetMod VPN Client 4.6.0** is a Windows desktop application that presents a
graphical front end for multi-protocol proxy tunnelling. Despite the name it is
**not a conventional VPN client**: it contains no tunnel implementation of its
own. It generates configuration, manages host network state, and launches bundled
third-party binaries that perform the actual protocol work.

## 2. Scope of this research

| In scope | Out of scope |
|---|---|
| The Inno Setup installer container and its payload | The **runtime behaviour** of the application (nothing was executed) |
| The `NetMod.exe` managed assembly: metadata, IL, call graph | The 84 embedded native/managed payloads, except where their presence is established |
| The anti-tamper protection and how it was defeated | The behaviour of third-party libraries |
| Configuration, persistence and licensing **schema** | The HWID derivation algorithm (deliberately not reversed) |
| Network and update behaviour visible in IL | Any exploitation, proof-of-concept, or live testing |

## 3. Research status — read this first

> **PARTIALLY reverse-engineered.**

| Area | Status |
|---|---|
| Installer container structure and payload inventory | **CONFIRMED** (byte-level) |
| Managed metadata, type system, native imports | **CONFIRMED** (three independent parsers agree) |
| Method-body recovery | **2236 of 2239** bodies decoded; 1 strongly inferred; 3 unresolved |
| Call graph | **12,064 edges**, all validated; 39 % proven control flow |
| TLS behaviour | **CONFIRMED** at byte level |
| Update verification | **CONFIRMED absent**; launch relationship **UNRESOLVED** |
| Host network mutation and restoration | **CONFIRMED** for the orderly path; crash path **UNRESOLVED** |
| Protected embedded assembly | **UNRESOLVED** — stage 3 of its pipeline is not decoded |
| All runtime behaviour | **UNRESOLVED** — no dynamic testing was performed |

The distinction between "the code says X" and "the program does X" is maintained
throughout this repository. Every claim carries one of four labels:

| Label | Meaning |
|---|---|
| `CONFIRMED` | Directly supported by recovered bytes, metadata, or a reproducible validation |
| `INFERRED` | Supported by multiple observations but not fully demonstrated |
| `UNRESOLVED` | Evidence insufficient, or behaviour not recovered |
| `WITHDRAWN` | An earlier claim invalidated by later evidence (preserved for the record) |

## 4. Why the analysis was difficult

The application is protected by a **runtime IL-decrypting obfuscator**. At rest,
2,238 of 2,452 method bodies are ciphertext occupying valid RVAs. A module
initialiser runs before any application code, makes the section writable with
`VirtualProtect`, and rewrites each body in place so the CLR can JIT it.

Consequences for an analyst:

* Decompilers see garbage. A strict CIL validator found only **14 genuine bodies**
  before the transform was recovered.
* Metadata cross-references are partly poisoned, producing plausible-looking but
  false call edges.
* `SuppressIldasmAttribute` is applied.
* The protection also hides the behaviour that matters most for security review —
  licence enforcement, update verification and credential handling.

This is a **protection mechanism, not evidence of malicious intent.** Legitimate
commercial software is obfuscated routinely. Its relevance here is narrower and
still significant: the codebase **cannot be independently audited** by a third
party, including its users.

## 5. What was actually achieved

Six rounds of static analysis, all offline:

* the **anti-tamper transform and key schedule were recovered**, enabling IL
  decryption of 99.8 % of method bodies;
* a **corrected metadata model** was built, unlocking the entire type system;
* a **12,064-edge call graph** was constructed and independently validated;
* the **TLS defect was proven at byte level**, at two independent call sites;
* three earlier incorrect conclusions were found, corrected and preserved.

Details: [REVERSE_ENGINEERING_METHOD.md](REVERSE_ENGINEERING_METHOD.md).

## 6. Safety posture

Throughout the investigation:

* the installer and executable were **never executed**;
* no host route, DNS, registry, service, driver or firewall setting was altered;
* no remote endpoint was contacted;
* no credential was used;
* no HWID was derived, no licence was forged, no check was bypassed;
* all output was written to a separate workspace, leaving the evidence unmodified;
* one third-party API key was found in the binary, redacted from all output, and
  reported to the vendor-side recommendation in [NF-05](../findings/confirmed/NF-05-embedded-api-key.md).

## 7. How to read this repository

* New to the project? Start with the [README](../README.md) and
  [ARCHITECTURE.md](ARCHITECTURE.md).
* Looking for the defects? Go to [findings/](../findings/README.md), then
  [SECURITY_ASSESSMENT.md](SECURITY_ASSESSMENT.md).
* Want to reproduce or critique the method?
  [REVERSE_ENGINEERING_METHOD.md](REVERSE_ENGINEERING_METHOD.md) and
  [VALIDATION.md](VALIDATION.md).
* Want to know what is *not* known? [LIMITATIONS.md](LIMITATIONS.md).

## 8. Terminology

| Term | Meaning here |
|---|---|
| "Confirmed" | Reproduced from bytes by a documented procedure |
| "Inferred" | Strongly suggested by control flow; not directly observed |
| "Proven control-flow edge" | A `call`/`newobj`/`ldftn` operand naming a specific target |
| "Runtime-resolved edge" | A `callvirt`; the declared target is known, the executed one is not |
| "Body" | The IL of one `MethodDef` row |
| "Restored"/"installed" | Applied to host state — routing, DNS, adapters |
| **Note:** "VPN" appears in the product name and in some identifiers; the tunnel itself is provided by other binaries. |