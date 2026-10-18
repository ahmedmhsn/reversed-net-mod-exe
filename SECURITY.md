# Security Policy

## About this repository

This is an independent, evidence-based reverse-engineering study of **NetMod VPN
Client 4.6.0**. It is **not** affiliated with, endorsed by, or sponsored by the
application's authors, distributors or vendors.

It publishes **documentation and findings** derived entirely from static analysis
of a shipped artifact. **No vulnerability was exploited, no proof-of-concept is
included, and no software was executed during the research.**

---

## Reporting a security issue in *this repository*

If you find something wrong — an inaccurate claim, a broken link, a leaked
credential, or content that overstates the evidence — please report it privately
first so it can be fixed before it is widely copied.

**What to include:** the file and line, what the text claims, and why you believe
it is wrong. Evidence is much more useful than assertion.

**What to expect:** a correction and a new evidence entry. Corrections to this
repository are recorded the same way the research corrections were — as new
documents, never by silently editing an existing claim.

---

## Responsible use

This repository documents defects in a third-party product so that users can
protect themselves and maintainers can fix them.

### Please do

* report a defect to the **vendor** before publishing additional technical detail;
* prefer remediation over exploitation — the [recommended actions](docs/SECURITY_ASSESSMENT.md)
  in each finding are the useful part;
* respect the vendor's disclosure timeline once you have contacted them.

### Please do not

* use this material to build or deploy an exploit against the product;
* republish the findings in a way that presents a **defect** as a demonstrated
  **exploit** — the distinction is load-bearing throughout, and two of the eight
  findings would be misread if it were lost;
* attempt licence bypass, HWID spoofing, or any circumvention of the product's
  licensing. The HWID derivation was **deliberately not reversed**, and that
  omission should be respected;
* redistribute the original installer, the executable, the bundled third-party
  binaries, or any extracted proprietary artifact. **They are not in this
  repository, and they should not be added to a fork.**

---

## Confidentiality and disclosure position

The research was conducted **without contacting the vendor**. Findings were
derived solely from static analysis of a distributed artifact, so no confidential
information was obtained.

| Finding | Suggested owner | Recommended action |
|---|---|---|
| NF-01 — TLS validation disabled | Application vendor | High priority; affects every user |
| NF-02 — Unverified update payload | Application vendor | High priority |
| NF-03 — Gateway restoration | Application vendor | Medium |
| NF-04 — Unauthenticated IPC | Application vendor | Medium |
| NF-05 — Embedded API key | **Third-party API vendor** (rotate), and the application vendor | Immediate key rotation |
| NF-06, NF-07 — Open questions | — | Not findings; do not report as vulnerabilities |

**Two observations about disclosure in general**, offered as context rather than
advice:

1. This is a commercial product distributed to end users. Vulnerabilities that
   affect all users are the vendor's to fix, and the fixes here are small — most
   are a few lines.
2. NF-07 describes code that **cannot be inspected** because its final
   obfuscation stage was not recovered. That is an *audit gap*, not a proven
   compromise. Reporting it as "a hidden backdoor" would be inaccurate.

---

## What is deliberately not published

| Withheld | Reason |
|---|---|
| The embedded third-party API key | It is a **live credential belonging to someone else**. Recorded by type and location only; verified absent from every published file |
| Any proof-of-concept or exploit steps | Deliberate. Findings are documented to the level needed to understand and fix the defect |
| The IL decryptor and FieldRVA transform code | Protection-defeating, and generalisable to other targets |
| The installer, executable and bundled binaries | Not ours to redistribute |
| Analyst environment details | Personal paths, usernames and host names serve no evidentiary purpose |

---

## Security-relevant research posture

Recorded so the boundaries of the study are unambiguous:

| | |
|---|---|
| Target executed | **Never** |
| Network endpoint contacted | **Never** |
| Host routes / DNS / firewall / registry / services / drivers modified | **Never** |
| Credential used | **None** |
| Licence or HWID bypass attempted | **No — deliberately out of scope** |
| Proof-of-concept produced | **None** |
| Dynamic experiments executed | **0 of 7 planned** (no disposable analysis VM available) |

The single highest-value experiment — a post-`.cctor` process memory dump — was
**not performed**, and the reason is recorded in
[docs/LIMITATIONS.md](docs/LIMITATIONS.md).

---

## Security properties of the studied software

Summarised here so a reader does not have to assemble it from eight files:

| Risk | Status |
|---|---|
| Man-in-the-middle against upstream TLS | **Plausible** — certificate validation returns `true` unconditionally (NF-01) |
| Update payload substitution | **Not detected** by the client; execution unproven (NF-02, NF-06) |
| Same-session process reading published arguments | **Possible** — session-wide atom table (NF-04) |
| Bundled DLL substitution | No `AssemblyRef` hash pinning exists; **no load-time hash check is configured** |
| Telemetry content | **Unknown** — not determined |
| Privilege escalation | **No evidence found** |
| Credential harvesting | **No evidence found** |

Full assessment: [docs/SECURITY_ASSESSMENT.md](docs/SECURITY_ASSESSMENT.md).

---

## Trademarks and licensing

This repository contains **only original documentation**. It contains no vendor
source code, no vendor binaries, and no third-party libraries.

"NetMod" and the names of all products, protocols and libraries referenced here
are trademarks or registered marks of their respective owners, used solely for
identification. All referenced third-party components remain the property of
their respective authors under their own licences.

**No vendor copyright notice is claimed or implied, and nothing here grants any
right in the original implementation.**

Architecture and behaviour descriptions are **research observations about a
third-party artifact**, not a specification and not a licence grant.

---

## Contact

Via the repository's issue tracker, or a private security advisory if the
repository owner has enabled one. See [CONTRIBUTING.md](CONTRIBUTING.md).