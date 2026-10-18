# Findings Index

Every finding in this repository appears here with its classification, the
evidence that supports it, and — importantly — **what is explicitly not claimed**.

## Classification

| Label | Meaning |
|---|---|
| **`CONFIRMED`** | Directly supported by recovered bytes, metadata, or a reproducible validation |
| **`INFERRED`** | Supported by multiple observations but not fully demonstrated |
| **`UNRESOLVED`** | Evidence insufficient, or the behaviour was not recovered |
| **`WITHDRAWN`** | An earlier claim invalidated by later evidence; preserved, not hidden |

## Severity

Severity describes the **demonstrated defect**, not a demonstrated exploit.
Nothing here was executed, no endpoint was contacted, and no proof-of-concept
exists. See [SECURITY.md](../SECURITY.md) for the responsible-use position.

| Severity | Meaning in this repository |
|---|---|
| **Critical** | A security control is provably disabled in shipped code |
| **High** | A required security control is provably absent |
| **Medium** | A design weakness with a plausible but unproven consequence |
| **Low** | Hygiene or robustness issue |
| **—** | Not rated: the impact could not be established |

---

## Summary table

| ID | Summary | Class | Severity | Component | Status |
|---|---|---|---|---|---|
| [NF-01](confirmed/NF-01-tls-certificate-validation.md) | Both TLS certificate-validation callbacks return `true` unconditionally | `CONFIRMED` | **Critical** | `ProxyHandler`, `SslConnectionHandler` | Closed |
| [NF-02](confirmed/NF-02-update-integrity.md) | Update payload is fetched with no integrity verification | `CONFIRMED` (defect) | **High** | `UpdateApi`, `Constants` | Defect closed; launch `UNRESOLVED` |
| [NF-03](confirmed/NF-03-default-gateway-restore.md) | Default-gateway removal is undone only on the orderly path | `CONFIRMED` (design) / `INFERRED` (crash effect) | **Medium** | `VpnService`, `NetworkManagement` | Closed |
| [NF-04](confirmed/NF-04-ipc-broadcast-channel.md) | Unauthenticated session-wide message channel for single-instance coordination | `CONFIRMED` (channel) / `UNRESOLVED` (impact) | **Medium** | `App` | Channel closed; receiver `UNRESOLVED` |
| [NF-05](confirmed/NF-05-embedded-api-key.md) | Third-party geo-IP API key embedded in cleartext | `CONFIRMED` | **Low** | `IpInfoModel` caller | Closed |
| [NF-06](unresolved/NF-06-update-payload-launch.md) | Whether the update payload is ever launched | `UNRESOLVED` | — | `UpdateApi` → `ProcessRunner` | Open |
| [NF-07](unresolved/NF-07-embedded-assembly-gap.md) | A 47,936-byte encrypted assembly is loaded into the process | `UNRESOLVED` | — | `<Module>` → `Assembly.Load` | Open |
| [NF-08](inferred/NF-08-anti-tamper.md) | Runtime IL encryption prevents third-party audit | `CONFIRMED` | Informational | whole binary | Closed |

## What is deliberately absent

No finding in this repository asserts that a vulnerability is **exploitable**.

Two claims that a less careful review would have made are explicitly rejected:

* **"Arbitrary code execution via the update path."** The absence of verification
  is proven; the link from the downloaded payload to a process launch was never
  found. See [NF-06](unresolved/NF-06-update-payload-launch.md).
* **"The broadcast IPC channel allows an attacker to hijack the client."** The
  channel is proven; what the receiving code does with a message is not
  recovered. See [NF-04](confirmed/NF-04-ipc-broadcast-channel.md).

## Withdrawn claims

Preserved rather than deleted, because knowing what *not* to believe is part of
the value:

| Former claim | Rounds | Status |
|---|---|---|
| The obfuscator scrambled the `MemberRef` parent column | 1–4 | `WITHDRAWN` in round 5 — a metadata stride error, not obfuscation |
| The `Valid` bitmask was tampered with | 1–4 | `WITHDRAWN` in round 6 — the field is 8 bytes; the reader truncated it to 4 |
| Reflection edges number 35 | 1–5 | `WITHDRAWN` — only **3** edges actually invoke code |
| The binary contains 3 update URLs | 1–5 | `WITHDRAWN` — there are **16** |
| `apps_vpn_filter` / `exclude_vpn_apps` are CIDR route-bypass settings | 1–5 | `WITHDRAWN` — they are resource/localisation strings |
| Update code execution is CRITICAL | 1–5 | **Downgraded to High** in round 6; launch relationship unresolved |
| `ManifestResource` rows are embedded | 6 | `WITHDRAWN` — both rows resolve to **AssemblyRef #24**, an obfuscated name; the resources are **external** to `NetMod.exe` |

Full detail: [VALIDATION.md](../docs/VALIDATION.md#corrections-preserved).