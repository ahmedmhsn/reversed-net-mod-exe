# NF-05 — A third-party API key is embedded in cleartext

| | |
|---|---|
| **Classification** | `CONFIRMED` |
| **Severity** | **Low** (vendor hygiene / third-party vendor risk) |
| **Status** | Closed |
| **Component** | `IpInfoModel` caller (`0x060001B1`) |
| **Confidence** | High |

---

## Finding

A complete API key for a **third-party** geo-IP service is embedded as a
plaintext string literal in `NetMod.exe`. Any user who inspects the binary —
which is trivial, since the application is not obfuscated at that point — obtains
the key.

## Evidence

| Property | Value |
|---|---|
| Location | String literal referenced from method `0x060001B1` |
| Type of secret | third-party geo-IP service API key |
| Storage | cleartext `#US` (user string) heap entry |
| Owner of the key | **the third-party service vendor**, not the application author |

> **The value is deliberately not reproduced anywhere in this repository.** It is
> recorded by type and location only. Every published artefact was scanned to
> confirm the value appears nowhere in this repository.

## Why this is a real finding

The key is **not the application's own credential**. It belongs to a third party,
and the application is acting as a client of that service on the vendor's behalf.
That means:

* the key is exposed to every user of the product, and to anyone who obtains the
  binary;
* the third party's quota, billing and abuse monitoring are exposed to cost and
  to reputational damage;
* the exposure is **permanent** — a key that has shipped inside a distributed
  binary cannot be rotated retroactively for existing copies.

## Why the severity is Low

* No user data is at risk.
* No privilege boundary is crossed by the disclosure itself.
* The impact falls on a third party's quota rather than on an end user's
  security.

It is included because it is unambiguous, trivially verifiable, and has a clear
remedy — and because silently omitting it would misrepresent the research.

## Validation status

| Check | Result |
|---|---|
| Key literal located | Yes — `0x060001B1` |
| Value redacted from all publication artefacts | Yes — verified by automated scan |
| Obfuscation covering this literal | **None** — it is readable in the binary |

## Recommended defensive action

**For the third-party vendor.** Rotate the key and monitor the service for
abnormal usage attributable to this client.

**For the application vendor.**

1. Proxy geo-IP lookups through your own backend so the upstream key is never
   distributed.
2. If a client-side key is unavoidable, scope it minimally and restrict it by
   origin/referrer where the provider supports it.
3. Add a pre-release secret scan; this class of leak is caught by tooling.

## References

* [ARCHITECTURE.md](../../docs/ARCHITECTURE.md)
* [evidence/EVIDENCE_INDEX.md](../../evidence/EVIDENCE_INDEX.md) — redaction policy
* [SECURITY.md](../../SECURITY.md)