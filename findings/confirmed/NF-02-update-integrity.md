# NF-02 — The update payload is fetched with no integrity verification

| | |
|---|---|
| **Classification** | `CONFIRMED` (the defect) · `UNRESOLVED` (consequence) |
| **Severity** | **High** |
| **Status** | Defect closed. Whether the payload is executed is `UNRESOLVED` — see [NF-06](../unresolved/NF-06-update-payload-launch.md) |
| **Component** | `Utils.UpdateApi`, `<CheckForUpdate>d__11`, `Utils.Constants` |
| **Confidence** | High for the absence of verification; **none** for exploitation |

---

## Finding

The application downloads an updated payload over HTTPS and uses it **without
ever verifying that it is the payload the vendor published**. There is no
signature check, no hash comparison, no certificate pinning, and no
authenticode verification — anywhere in the binary.

## Evidence

### The acquisition path is real

| Item | Evidence |
|---|---|
| `UpdateApi` | 6 methods, 5 fields, singleton `Instance` property |
| Async state machine | `0x0600028A`, 1,313 IL bytes |
| HTTP client | `System.Net.Http` `HttpClient`, `HttpClientHandler`, `SendAsync` (confirmed via `AssemblyRef` + call graph) |
| Payload URL constant | `Utils.Constants` method `0x060000FA` |

### The verification path does not exist

A sweep of **all 2,452 methods** was performed for every signature, hash and
certificate API reachable from managed code:

| API family | Calls found |
|---|---|
| `RSA`, `ECDsa`, `RSACryptoServiceProvider` | 0 |
| `X509Certificate2`, `X509Chain`, `SignedCms` | 0 (outside the NF-01 callbacks) |
| `SHA256`/`SHA1`/`MD5` over the payload | 0 |
| `FileSignature`, `WinVerifyTrust` | 0 |
| Any comparison of a received digest against an expected one | 0 |

**Zero verification edges across 2,452 methods.** This is a whole-binary result,
not a result for one function.

### URL inventory

An earlier internal report counted **3** update URLs. The corrected count is
**16** URL literals in the binary. The higher count does not change the finding —
it widens the surface that carries no integrity guarantee.

## What is explicitly NOT claimed

> **Arbitrary code execution through the update path is not proven.**

This finding was downgraded from **Critical** to **High** in the later audit
round, and the downgrade is deliberate:

* The sole process-launch site in the binary is `ProcessRunner 0x06000254`.
* A complete walk of the call graph found **no edge from any update method to
  that site**.
* The 1,313-byte async state machine `0x0600028A` was only partially decoded,
  so the absence of an edge is **not proof of absence** of the behaviour.

The correct statement is therefore two separate claims:

1. **`CONFIRMED`** — the payload is not authenticated before use.
2. **`UNRESOLVED`** — whether it is then executed.

Publishing (1) as (2) would overstate the evidence.

## Impact analysis

| Question | Answer |
|---|---|
| Does HTTPS protect the download in transit? | Yes, at the transport layer — *provided* NF-01 does not apply to that connection |
| Does the application verify what it received? | **No** |
| Is the TLS connection to the update host validated? | **Unknown** — if it goes through `SslConnectionHandler`, NF-01 applies |
| Can a third party substitute the payload? | **Only** if they can act as the update host, intercept the connection, or compromise the install directory |

The realistic threat model is therefore *weaker* than "anyone on the network can
push code": it requires defeating TLS as well. That is why the severity is High
and not Critical.

## Validation status

| Check | Result |
|---|---|
| Update methods identified | Yes |
| Trust-API sweep over all 2,452 methods | 0 verification calls |
| Edge from update methods to `ProcessRunner 0x06000254` | **None found** |
| `0x0600028A` fully decoded | **No** — 1,313 bytes, partial |
| Runtime confirmation | **None — not performed** |

## Recommended defensive action

**For users.** Do not rely on the built-in updater as a trust control. Update by
verifying the downloaded installer against a hash published through a separate
channel.

**For the vendor.**

1. Sign the payload (Authenticode or a detached signature) and verify the
   signature before use, not merely a hash served from the same host.
2. Pin the update host's certificate or use signed update manifests.
3. Fail closed: if verification is unavailable, refuse to install rather than
   proceeding.

## References

* [update-flow.mmd](../../architecture/update-flow.mmd)
* [trust-boundaries.mmd](../../architecture/trust-boundaries.mmd) — TB3
* [NF-06 — update payload launch](../unresolved/NF-06-update-payload-launch.md)
* [ARCHITECTURE.md §3.6](../../docs/ARCHITECTURE.md)