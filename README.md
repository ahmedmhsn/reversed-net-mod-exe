# Reversed NetMod EXE

Evidence-based reverse-engineering research and architecture documentation for
**NetMod VPN Client 4.6.0** — a Windows desktop client for multi-protocol proxy
tunnelling.

> **Research status: PARTIALLY reverse-engineered.** The managed code of the main
> executable is ~99.96 % recovered and a validated call graph exists, but the
> protected helper assembly, three method bodies, and every runtime behaviour
> remain unresolved. No dynamic testing was performed. See
> [Limitations](docs/LIMITATIONS.md).

---

## What this is

This repository is **original research documentation**, not a copy of the software
and not a decompilation dump. It answers three questions:

1. **How does this application work?** — recovered architecture, component
   responsibilities, data flows and network behaviour.
2. **What is wrong with it?** — confirmed, inferred and unresolved security
   findings, each tied to specific evidence.
3. **How was the analysis done?** — a reproducible static-analysis methodology,
   including the protection that had to be defeated and the corrections made along
   the way.

There are **no binaries, no decompiled source and no third-party DLLs** here. Only
documentation, diagrams, findings and the analysis tooling needed to reproduce the
method.

---

## Table of contents

| Section | Document |
|---|---|
| Scope and research status | [docs/OVERVIEW.md](docs/OVERVIEW.md) |
| System and component architecture | [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) |
| Component inventory with confidence | [docs/COMPONENT_INVENTORY.md](docs/COMPONENT_INVENTORY.md) |
| Verified technology stack | [docs/TECHNOLOGY_STACK.md](docs/TECHNOLOGY_STACK.md) |
| Assembly references (all 38, validated) | [docs/ASSEMBLY_REFERENCES.md](docs/ASSEMBLY_REFERENCES.md) |
| Data flows | [docs/DATA_FLOWS.md](docs/DATA_FLOWS.md) |
| Network behaviour: listeners, routes, DNS | [docs/NETWORK_BEHAVIOR.md](docs/NETWORK_BEHAVIOR.md) |
| Configuration and persisted state | [docs/CONFIGURATION_AND_STATE.md](docs/CONFIGURATION_AND_STATE.md) |
| Reverse-engineering method | [docs/REVERSE_ENGINEERING_METHOD.md](docs/REVERSE_ENGINEERING_METHOD.md) |
| Findings summary | [findings/README.md](findings/README.md) |
| Full security assessment | [docs/SECURITY_ASSESSMENT.md](docs/SECURITY_ASSESSMENT.md) |
| Limitations and open questions | [docs/LIMITATIONS.md](docs/LIMITATIONS.md) |
| Validation results | [docs/VALIDATION.md](docs/VALIDATION.md) |
| Evidence index and hashes | [evidence/EVIDENCE_INDEX.md](evidence/EVIDENCE_INDEX.md) |
| Reference / related work | [docs/REFERENCES.md](docs/REFERENCES.md) |
| Contributing | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Security policy | [SECURITY.md](SECURITY.md) |
| Changelog | [CHANGELOG.md](CHANGELOG.md) |

**Diagrams** — [architecture/](architecture/):
[system context](architecture/system-context.mmd) ·
[component architecture](architecture/component-architecture.mmd) ·
[connection flow](architecture/connection-flow.mmd) ·
[update flow](architecture/update-flow.mmd) ·
[trust boundaries](architecture/trust-boundaries.mmd)

---

## The analyzed artifact

| | |
|---|---|
| Installer | Inno Setup 6.1.0 bootstrapper, 33,509,274 bytes |
| Installer SHA-256 | `075fcdf4209d8182131e866c551ca28105137605d98bf0a64e650606b76ec44b` |
| Payload container | WiX Burn bundle, ~89.7 MiB decompressed, 84 embedded PE images |
| Main executable | `NetMod.exe`, 625,664 bytes |
| `NetMod.exe` SHA-256 | `60b1be47f1e74b8283fc31ee72ea07a808ea7d941515cf3a3a444bf126f31743` |

Both hashes are recomputed from the research artifacts and recorded in
[evidence/HASH_MANIFEST.md](evidence/HASH_MANIFEST.md).

**The installer and the executable are deliberately not redistributed here.**

---

## Technology stack (verified)

| Layer | Confirmed technology | How verified |
|---|---|---|
| Runtime | .NET Framework **4.7.2**, x86, IL-only, 32-bit-required | COR20 header + `Assembly` row |
| UI | WPF (PresentationFramework/PresentationCore/WindowsBase) | AssemblyRef + TypeRef |
| Client | `NetMod` assembly version **4.6.0.415**, not strong-named | `Assembly` row, `PublicKeyOrToken` absent |
| UI toolkit | MahApps.Metro 2.0.0.0, MaterialDesignThemes.Wpf 4.5.0.0 | AssemblyRef |
| Data | EF Core 3.1 + Sqlite + DataEncryption 3.0.1.0 | AssemblyRef, migration ids in IL |
| Protection | ConfuserEx-style anti-tamper, runtime IL decryption | Recovered decryptor matched to open-source source |
| Installer | Inno Setup 6.1.0 → WiX Burn | Container structure decoded byte-level |

Full details, including 34 native imports: [docs/TECHNOLOGY_STACK.md](docs/TECHNOLOGY_STACK.md).

---

## Architecture at a glance

```
                      ┌────────────────────────────┐
                      │   NetMod.exe  (.NET/WPF)   │
                      │  UI ─ MVVM ─ Services ─ Core│
                      └───┬──────────────┬─────────┘
           spawns / uses  │              │  opens
        ┌─────────────────┘              └───────────────┐
        ▼                                                ▼
┌───────────────────┐                        ┌──────────────────────┐
│ tun2socks (badvpn)│                        │ v2ray / ssr-local /  │
│  TAP ⇄ SOCKS5      │                        │ plink / dnstt-client│
└─────────┬─────────┘                        └──────────────────────┘
          ▼
  TAP-Windows adapter (tap0901)  →  host routing table + DNS
```

The client is a **front end**, not a tunnel implementation: it owns the UI, the
configuration, the local proxy, and the host network state, while the actual
protocol work is delegated to bundled third-party binaries. Full description:
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

---

## Component inventory (abridged)

| Component | Namespace / type | Confidence |
|---|---|---|
| Entry point | `NetModMaterial.App.Main` (`0x0600007E`) | **CONFIRMED** |
| Single-instance coordination | `App..ctor` + `GlobalAddAtom`/`SendMessage` | **CONFIRMED** |
| Connection services | `Service.VpnService`, `ProxyServer`, `V2RayService`, `SshService` | **CONFIRMED** |
| Local proxy + TLS | `ProxyServer`, `ProxyHandler`, `SslConnectionHandler` | **CONFIRMED** |
| Configuration | `Properties.Settings`, `CustomSettingsProvider`, `ConfigPreferences` | **CONFIRMED** |
| Persistence | `Utils.DbManager`, `Migrations.*` (EF Core / SQLite) | **CONFIRMED** |
| Tethering | `Utils.Wifi.TetheringManager`, `WlanHostedNetworkManager` | **CONFIRMED** |
| Update client | `Utils.UpdateApi`, `<CheckForUpdate>d__11` | **CONFIRMED** (paths), **UNRESOLVED** (launch) |
| Licence / HWID | `Utils.MainJSON`, `Utils.Encryption` | **CONFIRMED** (schema), **UNRESOLVED** (derivation) |
| Protected embedded assembly | `FieldRVA` payload → `Assembly.Load` | **UNRESOLVED** |

Full table with evidence and confidence per component:
[docs/COMPONENT_INVENTORY.md](docs/COMPONENT_INVENTORY.md).

---

## Security observations (summary)

Each finding below is scoped to the code that was actually examined. Severity
reflects the demonstrated defect, not a demonstrated exploit.

| ID | Observation | Class | Severity |
|---|---|---|---|
| [NF-01](findings/confirmed/NF-01-tls-certificate-validation.md) | Both TLS certificate-validation callbacks reduce to `ldc.i4.1 ; ret` | **CONFIRMED** | Critical |
| [NF-02](findings/confirmed/NF-02-update-integrity.md) | Update payload is fetched with no signature or hash verification | **CONFIRMED** (defect) | High |
| [NF-03](findings/confirmed/NF-03-default-gateway-restore.md) | Default gateway removal with restoration proven only on the orderly path | **CONFIRMED** (design) / **INFERRED** (crash effect) | Medium |
| [NF-04](findings/confirmed/NF-04-ipc-broadcast-channel.md) | Session-wide unauthenticated message channel for single-instance coordination | **CONFIRMED** (channel) / **UNRESOLVED** (impact) | Medium |
| [NF-05](findings/confirmed/NF-05-embedded-api-key.md) | Third-party geo-IP API key embedded in cleartext | **CONFIRMED** | Low (vendor hygiene) |
| [NF-06](findings/unresolved/NF-06-update-payload-launch.md) | Update payload reaching a process launch | **UNRESOLVED** | — |
| [NF-07](findings/unresolved/NF-07-embedded-assembly-gap.md) | Encrypted assembly loaded into the process at startup | **UNRESOLVED** | Unknown |
| [NF-08](findings/inferred/NF-08-anti-tamper.md) | Anti-tamper protection blocks third-party audit | **CONFIRMED** | Informational |

Read the full assessment, including what is explicitly **not** claimed:
[docs/SECURITY_ASSESSMENT.md](docs/SECURITY_ASSESSMENT.md).

> **Note on disclosure.** This repository deliberately omits any working
> proof-of-concept, exploit step-by-step, or payload. Findings are documented at
> the level needed to understand the defect and to fix it.

---

## Methodology in brief

The hard part was not reading the metadata — it was that **2,238 of 2,452 method
bodies were encrypted in place** and only decrypted at runtime by a module
initialiser. The investigation took six rounds:

| Round | Contribution |
|---|---|
| 1–2 | Installer decoding; 84 embedded PEs identified; managed metadata characterised; protection identified |
| 3 | First IL recovery attempt; failed — established the real blocker |
| 4 | **Recovered the anti-tamper transform and key schedule**; IL decryption became possible |
| 5 | **Corrected a metadata-layout error**, unlocking the type system; built the call graph |
| 6 | Independent audit; refuted one earlier claim, corrected three, tightened two findings |

Three corrections are preserved rather than hidden, because they are the most
useful part of the record:

1. **MemberRef parents were never scrambled.** Earlier rounds concluded the
   obfuscator had corrupted that table. It had not — a metadata stride error put
   the reader at the wrong offset. Corrected in round 5.
2. **The `Valid` bitmask was never tampered.** It is an 8-byte field; earlier
   rounds read 4 bytes and blamed the obfuscator for the difference. Refuted in
   round 6.
3. **"Arbitrary code execution via updates" was overstated.** The absence of
   integrity verification is proven; the link from payload to process launch was
   never found. Narrowed in round 6.

Details: [docs/REVERSE_ENGINEERING_METHOD.md](docs/REVERSE_ENGINEERING_METHOD.md).

---

## Reproducibility

Analysis was **offline and read-only**. Nothing was executed, no network endpoint
was contacted, and no host configuration was modified.

The tooling used is documented in [tools/README.md](tools/README.md). A small,
self-contained metadata reader used for the assembly-reference audit is included
there and can be run against a file you supply. Minimum reproduction steps and
the exact validation procedure: [docs/REVERSE_ENGINEERING_METHOD.md](docs/REVERSE_ENGINEERING_METHOD.md#reproduction).

Legal note: analysing a binary you are authorised to examine is lawful in most
jurisdictions. **Redistribution of the installer, the executable, or any bundled
third-party binary is not granted by this repository.**

---

## Status of the code

There is no source code in this repository, and none is implied. The architecture
and behaviour descriptions are **research observations about a third-party
application**, not a specification, and not a grant of any rights in the original
implementation. See [SECURITY.md](SECURITY.md) for licensing and attribution.

---

## Affiliation and trademarks

This research is **not affiliated with, endorsed by, or sponsored by** the
authors, distributors or vendors of NetMod or of any referenced open-source
project. It is an independent third-party analysis.

"NetMod", and the names of all products, protocols and libraries referenced here,
are trademarks or registered marks of their respective owners and are used solely
for identification. All referenced third-party components remain the property of
their respective authors under their own licences. No vendor copyright notice is
claimed or implied.

---

## Contributing

Corrections, better evidence and additional validated findings are welcome — see
[CONTRIBUTING.md](CONTRIBUTING.md). Claims must be evidence-backed; please read the
evidence policy there first.

## Security policy

For disclosure handling and the responsible-use statement: [SECURITY.md](SECURITY.md).