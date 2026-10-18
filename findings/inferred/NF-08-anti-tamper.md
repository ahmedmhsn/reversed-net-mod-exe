# NF-08 — Runtime IL encryption prevents third-party audit

| | |
|---|---|
| **Classification** | `CONFIRMED` |
| **Severity** | **Informational** |
| **Status** | Closed |
| **Component** | whole binary (`<Module>.cctor` → decryptor `0x0600000A`) |
| **Confidence** | High |

---

## Finding

2,238 of 2,452 method bodies (91.3 %) are stored encrypted and decrypted in
place at start-up, so no static tool can read the application without first
defeating the protection. The protection was defeated for this research, but it
is effective against users, administrators and independent auditors.

## Evidence

| Mechanism | Status | Detail |
|---|---|---|
| Runtime IL decryption | `CONFIRMED` | `<Module>.cctor` (`0x06000001`) calls the decryptor before any application code |
| Decryptor size | `CONFIRMED` | `0x0600000A`, 745 bytes / 462 instructions |
| Memory protection change | `CONFIRMED` | Two `VirtualProtect` P/Invoke declarations, `0x06000004` and `0x06000009` |
| `SuppressIldasmAttribute` | `CONFIRMED` | First `MemberRef` row, token `0x0A000001` |
| RWX section | `CONFIRMED` | First section named `Wc9A_'CW`, characteristics `0xE0000040` |
| Metadata poisoning | `CONFIRMED` | Call edges with impossible targets, e.g. `newobj` → `0x16064457` |
| Identifier renaming | `CONFIRMED` | 10 type names replaced by zero-width characters |
| Tool identity | `INFERRED` | Transform matches open-source ConfuserEx anti-tamper, Normal mode; no version signature found |

### Effect on tooling

Before the transform was recovered, a strict CIL validator found only **14**
genuine method bodies out of 2,452. Decompilers showed garbage, and the .NET
runtime itself would throw `BadImageFormatException` on 47 corrupted method RVAs.

## Why this matters beyond inconvenience

| Effect | Detail |
|---|---|
| **Users cannot audit** | A user cannot verify what the client does with their traffic |
| **Administrators cannot inspect** | Endpoint tooling cannot see the code |
| **Defects stay hidden** | The TLS defect [NF-01](../confirmed/NF-01-tls-certificate-validation.md) was not discoverable from the shipped binary without this work |
| **Evidence for this research exists only because the protection was reversed** | The findings in this repository would not otherwise be reproducible |

## This is not evidence of malicious intent

Commercial software is obfuscated routinely, and anti-tamper is a normal
commercial control. The finding is about **auditability**, and it is reported at
Informational severity for that reason.

It does, however, raise the cost of every other finding: an auditor must defeat
the protection before they can check anything at all, which is why this
repository documents the method in detail.

## Recommended defensive action

**For users and administrators.** Treat the application as an opaque component.
If its behaviour matters to your threat model, prefer a product whose code you
can inspect, or use network-level controls that do not depend on the client
being honest.

**For the vendor.** Protect intellectual property with mechanisms that do not
destroy auditability — signed binaries with transparent reproducible builds,
centralised telemetry, and a published SBOM go a long way toward achieving the
commercial goal without blinding users.

## References

* [REVERSE_ENGINEERING_METHOD.md](../../docs/REVERSE_ENGINEERING_METHOD.md)
* [VALIDATION.md](../../docs/VALIDATION.md)
* [TECHNOLOGY_STACK.md §3](../../docs/TECHNOLOGY_STACK.md)