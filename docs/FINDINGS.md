# Findings

This document is the narrative companion to the structured index in
[`findings/README.md`](../findings/README.md). It explains how findings were
reached, how they were tested, and — importantly — what the evidence does **not**
support.

---

## 1. The standard applied

A finding is published only if it survives this test:

> Can a competent reader re-derive the conclusion from the bytes, using a
> procedure documented in this repository?

That excludes inference from type names, from UI strings, from the product's
marketing, and from intuition about what a VPN client "should" do.

Three further rules applied throughout:

1. **A defect is not an exploit.** Where the code shows a missing control but no
   reachable path to exploitation, both facts are stated and the finding is
   rated on the defect.
2. **Absence of evidence is recorded as such.** "Not found" is reported as
   "not found", never upgraded to "does not exist", except where a complete
   enumeration makes the absence conclusive (as in the trust-API sweep).
3. **Tooling errors are disclosed.** Several conclusions during this research
   were wrong because of mistakes in the analysis code, not in the binary. Those
   are documented rather than quietly fixed, because a reader replicating the
   work needs to know where the traps are.

---

## 2. What was found

### 2.1 NF-01 — TLS certificate validation disabled (`CONFIRMED`, Critical)

Both certificate-validation callbacks in the binary are the same two-byte method,
`17 2a` — `ldc.i4.1 ; ret`. They are bound with `ldftn` at two call sites and
used for both direct outbound TLS and the local proxy's `CONNECT` handling.

The most telling detail: after `AuthenticateAsClient` returns, the code reads
`get_Subject` and `get_Issuer` from the certificate and writes them to a log.
**The trust decision is made, then discarded.**

A sweep of all 2,452 methods found no compensating validation API anywhere.

→ [NF-01](../findings/confirmed/NF-01-tls-certificate-validation.md)

### 2.2 NF-02 — Unverified update payload (`CONFIRMED` defect, High)

The update client fetches a payload and uses it without signature, hash, or
authenticode verification. Zero verification edges exist across 2,452 methods.

The consequence was **deliberately not escalated**: no edge connects any update
method to the binary's only `Process.Start` site.

→ [NF-02](../findings/confirmed/NF-02-update-integrity.md) ·
[NF-06](../findings/unresolved/NF-06-update-payload-launch.md)

### 2.3 NF-03 — Gateway restoration is single-sited (`CONFIRMED`, Medium)

Removing the default gateway is a deliberate, user-visible option, and the
client saves and restores the entry correctly on the orderly path. There is no
process-exit handler and no on-disk record, so an abnormal termination leaves the
host without a default route.

The post-crash state is `INFERRED`, not observed.

→ [NF-03](../findings/confirmed/NF-03-default-gateway-restore.md)

### 2.4 NF-04 — Unauthenticated IPC channel (`CONFIRMED` channel, Medium)

Single-instance coordination writes to the session-wide atom table and broadcasts
to `HWND_BROADCAST` using a fixed, statically-stored message id. Any process in
the user's session can read or inject on this channel.

What the receiver does is `UNRESOLVED` — no `WndProc` was identified, and
`GlobalGetAtomName` has no proven caller.

→ [NF-04](../findings/confirmed/NF-04-ipc-broadcast-channel.md)

### 2.5 NF-05 — Embedded third-party API key (`CONFIRMED`, Low)

A complete geo-IP API key is embedded as cleartext at `0x060001B1`. The value
appears in no published artefact. The key belongs to a third party, which is why
the remedy is rotation rather than removal.

→ [NF-05](../findings/confirmed/NF-05-embedded-api-key.md)

### 2.6 NF-08 — Anti-tamper blocks audit (`CONFIRMED`, Informational)

91.3 % of method bodies are encrypted at rest and decrypted by a module
initialiser. This is a normal commercial control and is not treated as evidence
of intent — but it means users and administrators cannot verify what the client
does with their traffic.

→ [NF-08](../findings/inferred/NF-08-anti-tamper.md)

---

## 3. What was not found

Recorded so that absence is not mistaken for an oversight in the research.

| Area | Result |
|---|---|
| Privilege escalation | **No** evidence. No `SeDebugPrivilege`, no token manipulation, no service creation |
| Credential harvesting | **No** evidence. No browser-credential or form-grab paths |
| Data exfiltration | **No** evidence beyond the documented geo-IP lookup and AppCenter telemetry |
| Dynamic code loading beyond the known pipeline | One instance only — [NF-07](../findings/unresolved/NF-07-embedded-assembly-gap.md) |
| Obfuscated dropper or second-stage loader | **None found** |
| Persistence beyond the installed service state | **None found** |
| Anti-analysis beyond the IL encryption | `SuppressIldasmAttribute`, RWX section, identifier renaming — all documented, none deceptive about intent |

---

## 4. Open questions

| ID | Question | Why it is still open |
|---|---|---|
| [NF-06](../findings/unresolved/NF-06-update-payload-launch.md) | Is the update payload executed? | No call edge to `ProcessRunner 0x06000254`; `0x0600028A` only partially decoded |
| [NF-07](../findings/unresolved/NF-07-embedded-assembly-gap.md) | What does the embedded assembly do? | Final decoding stage not recovered |
| — | What does the IPC receiver do with a broadcast? | `WndProc` override not identified |
| — | What is the HWID derivation? | **Deliberately not attempted** — out of scope |
| — | What does `AddedPosToProfile` add? | Column not determined from IL |
| — | Are `*_lock` licence keys enforced in the UI? | Requires the WPF event handlers |

All are answerable statically except the last two categories, which need either
BAML decoding or a runtime dump. See
[LIMITATIONS.md](LIMITATIONS.md#what-would-close-the-open-questions).

---

## 5. Cross-cutting observation

The most useful single insight from this research is not any one finding, but a
pattern:

> **The application's own defensive choices are inconsistent with each other.**

* TLS trust is disabled *unconditionally* [NF-01], while a user-facing setting
  separately permits insecure upstream TLS — two overlapping controls, neither
  effective.
* Host network state is restored carefully [NF-03] on paths the application
  controls, and not at all outside them.
* The update channel is unauthenticated [NF-02] and the IPC channel is
  unauthenticated [NF-04] — both are places where the product assumed it was the
  only participant.
* An audit-blocking protection [NF-08] sits on top of all of it.

None of this suggests intent to harm users. It reads much more like a codebase
assembled from components without a consistent security model, then hardened
commercially at the end. That is the honest characterisation, and it is more
useful to a maintainer than a claim of malice would be.

---

## 6. Full assessment

Severity assignments, the separation between defect and exploitability, and the
per-finding remediation plan: [SECURITY_ASSESSMENT.md](SECURITY_ASSESSMENT.md).

Structured index: [findings/README.md](../findings/README.md).