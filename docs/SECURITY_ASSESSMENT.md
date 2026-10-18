# Security Assessment

An assessment of the security properties of **the code that was examined**,
derived from static analysis of one shipped artifact. Nothing was executed, no
endpoint was contacted, and no exploit was demonstrated.

Read this together with [FINDINGS.md](FINDINGS.md) and
[LIMITATIONS.md](LIMITATIONS.md). Where a defect is proven but its consequence is
not, this document says so in the same paragraph.

---

## 1. Rating model

Severity describes the **demonstrated defect**, not a demonstrated exploit.

| Rating | Meaning |
|---|---|
| **Critical** | A security control is provably disabled in shipped code |
| **High** | A required security control is provably absent |
| **Medium** | A design weakness with a plausible but unproven consequence |
| **Low** | Hygiene or robustness issue |
| **Not rated** | The impact could not be established |

### Defect established vs. exploitability established

These are tracked as two independent columns throughout, because conflating them
is the most common error in published vulnerability work.

---

## 2. Assessment summary

| ID | Finding | Defect | Exploitability |
|---|---|---|---|
| NF-01 | TLS certificate validation returns `true` unconditionally | **PROVEN** | Plausible; **not demonstrated** |
| NF-02 | Update payload has no integrity verification | **PROVEN** | **NOT PROVEN** — no launch edge found |
| NF-03 | Gateway restoration is single-sited | **PROVEN** (design) | **NOT DEMONSTRATED** — no crash test |
| NF-04 | Unauthenticated session-wide IPC channel | **PROVEN** (channel) | **NOT PROVEN** — receiver unrecovered |
| NF-05 | Embedded third-party API key | **PROVEN** | n/a — hygiene |
| NF-06 | Is the payload launched? | — | **UNRESOLVED** |
| NF-07 | What does the embedded assembly do? | — | **UNRESOLVED** — cannot be rated |
| NF-08 | Anti-tamper blocks audit | **PROVEN** | n/a — informational |

---

## 3. NF-01 — TLS certificate validation disabled · **Critical**

**Defect: PROVEN.** One two-byte method (`17 2a` = `ldc.i4.1 ; ret`) is bound as
the certificate-validation callback at both TLS call sites in the binary. No
compensating validation API is called anywhere across 2,452 methods.

**Exploitability: plausible, not demonstrated.** Anyone positioned to intercept
the connection could present any certificate. *No interception was performed.*

**Why Critical rather than High:** a security control is not merely weakened, it
is inverted. The code affirmatively asserts that every certificate is valid.

**Compounding factor:** the `v2ray_allow_insecure` user setting independently
permits unauthenticated upstream TLS. Two overlapping controls, neither effective.

**Remediation:** remove the callbacks and use the default `SslStream` policy; if a
callback is required, reject on `sslPolicyErrors != None`. See
[NF-01](../findings/confirmed/NF-01-tls-certificate-validation.md).

---

## 4. NF-02 — Unverified update payload · **High**

**Defect: PROVEN.** Zero signature, hash or authenticode verification edges
exist across the whole binary. The payload is used unauthenticated.

**Exploitability: NOT PROVEN.** No call edge connects any update method to the
binary's only `Process.Start` site. The async state machine carrying the update
logic is only partially decoded, so the absence of an edge is not conclusive.

**Why High rather than Critical:** exploitation additionally requires defeating
the transport to the update host, which may itself be subject to NF-01. The
impact chain has an unproven middle link, and this repository does not paper
over it.

**Downgraded from Critical** in the later audit round precisely because the
launch relationship was not found. The reasoning is recorded in
[VALIDATION.md](VALIDATION.md#corrections-preserved).

**Remediation:** sign the payload and verify the signature before use; pin the
update host; fail closed.

---

## 5. NF-03 — Default-gateway restoration is single-sited · **Medium**

**Defect: PROVEN.** Gateway removal is a supported option; the entry is saved and
restored on the orderly path. There is no process-exit handler and no persisted
record, so abnormal termination skips restoration.

**Exploitability: NOT APPLICABLE.** No remote input triggers this; it requires
the user to crash or kill the client.

**Why Medium:** the restoration logic is present and correct where the
application controls the flow; the failure requires abnormal termination; the
impact is local connectivity loss, not confidentiality or code execution.

**Remediation:** persist the removed gateway before removing it; detect and offer
to restore on next start-up; wrap the connect path in `try`/`finally`.

---

## 6. NF-04 — Unauthenticated IPC channel · **Medium**

**Defect: PROVEN.** Single-instance coordination writes to the session-wide atom
table and broadcasts to `HWND_BROADCAST` using a fixed, statically-stored
message id. Any process in the same session can read the published atom or inject
one.

**Exploitability: NOT PROVEN.** No `WndProc` receiver was located, and
`GlobalGetAtomName` has no proven caller edge. What the receiver does with a
message is unknown.

**Why Medium rather than High or Low:** the confidentiality concern is concrete —
the client publishes its command-line argument, which may contain connection
details — but no impact on client state has been established.

**Remediation:** use a named pipe or per-user mutex; validate atom contents as
untrusted input; use `RegisterWindowMessage` rather than a fixed id.

---

## 7. NF-05 — Embedded third-party API key · **Low**

**Defect: PROVEN.** A complete third-party geo-IP API key is embedded in
cleartext and readable in the shipped binary.

**Not rated as a user-facing vulnerability:** the key belongs to a third party.
The exposure affects that vendor's quota and reputation, not an end user's
security. It is reported because it is unambiguous and has a clear remedy.

**Remediation:** rotate the key; proxy lookups through the application's own
backend; add a pre-release secret scan.

---

## 8. NF-06 — Update payload launch · **NOT RATED**

Open question, not a finding. See
[NF-06](../findings/unresolved/NF-06-update-payload-launch.md) for why no rating
is defensible and what would settle it.

---

## 9. NF-07 — Embedded protected assembly · **NOT RATED**

A 47,936-byte encrypted blob is decoded through a three-stage pipeline and loaded
with `Assembly.Load` into the client's process. Stages 1–2 were reproduced; stage
3 was not recovered.

**No severity can be assigned**, because the assembly could be a licence check, a
hardening measure, or something else entirely, and nothing in the recovered
evidence distinguishes them.

The defensible characterisation is an **audit gap**: unaudited code executes in
the client's security context and cannot be inspected from the shipped binary.
See [NF-07](../findings/unresolved/NF-07-embedded-assembly-gap.md).

---

## 10. NF-08 — Anti-tamper blocks audit · **Informational**

91.3 % of method bodies are encrypted at rest. This is a normal commercial
control and **no malicious intent is implied**. It is reported because it is the
reason users and administrators cannot verify the product's behaviour — including
the defects above.

---

## 11. Threat model

| Actor | Capability | Outcome |
|---|---|---|
| **Network attacker** | Intercepts or redirects traffic | **NF-01** applies if TLS interception is possible. Requires an active position. |
| **Malicious update server or MITM of it** | Serves a substituted payload | **NF-02** applies: substitution is not detected. Execution is **unproven** ([NF-06](../findings/unresolved/NF-06-update-payload-launch.md)). |
| **Same-session local process** | Reads/injects global atoms | **NF-04**: can read published arguments; effect on client state **unproven**. |
| **Local attacker with install-directory write** | Replaces a bundled DLL | No `AssemblyRef` `HashValue` pinning exists (all 38 rows carry an empty hash). **INFERRED** — no load-time hash check is configured. |
| **Local attacker with admin rights** | Arbitrary | Out of scope; the process offers no additional elevation. |
| **Third party with the API key** | Uses the leaked key | **NF-05**: quota and billing exposure for that vendor. |
| **User, running the client** | Triggers all of the above | — |

### Dependency integrity note

All 38 `AssemblyRef` rows carry an **empty** `HashValue`, so the CLR performs no
content-hash validation of referenced assemblies. This is a metadata fact, not a
finding — but it means the integrity of the 59 bundled managed libraries rests
entirely on install-directory integrity. Documented in
[ASSEMBLY_REFERENCES.md §1.1](ASSEMBLY_REFERENCES.md#11-table-wide-facts).

---

## 12. What was explicitly not found

Recorded so that silence is not mistaken for a clean bill of health.

| Category | Result |
|---|---|
| Privilege escalation | **No evidence** — no `SeDebugPrivilege`, token manipulation, or service creation |
| Credential harvesting | **No evidence** |
| Covert data exfiltration | **No evidence** beyond the documented geo-IP lookup and AppCenter telemetry |
| Second-stage loader / dropper | **None found** |
| Hidden persistence | **None found** |
| Deliberate deception about identity or purpose | **None found** |

---

## 13. Systemic observation

The findings are not independent. They share one root cause:

> **The application treats its own environment as trusted.** It accepts any TLS
> certificate, accepts any update payload, accepts any same-session process on
> its IPC channel, and trusts that only it will be terminated.

That pattern — trust assumptions made per-component and never revisited
globally — explains all four substantive findings at once, and is more useful to
a maintainer than any individual item.

---

## 14. Disclosure position

This repository publishes **defects and their remedies**, not exploits.

It contains no proof-of-concept, no payload, no step-by-step exploitation
instructions, and no reproduction of the leaked credential. Precise technical
detail is provided only to the extent needed to understand and fix the defect.

Because the findings were derived **solely from static analysis of a shipped
artifact**, and no vendor was contacted during the research, no disclosure
timeline applies to this publication.

**Recommendation to any third party considering coordinated disclosure:** report
NF-01 and NF-02 to the vendor before publishing further technical detail. Both
are straightforwardly fixable and both affect users directly.

See [SECURITY.md](../SECURITY.md) for the full responsible-use statement.