# Changelog

All notable changes to this repository are recorded here.

The format follows [Keep a Changelog](https://keepachangelog.com/), and the
project uses [semantic versioning](https://semver.org/) for the **documentation**,
not for any software product.

---

## [0.1.0] — 2026-10-19

Initial public release.

### Added

**Documentation**
- `README.md` — entry point, five-minute orientation, verified stack, component
  summary, research progression, responsible-use and affiliation statements.
- `docs/OVERVIEW.md` — scope, research status, the four-label classification
  scheme, safety posture.
- `docs/ARCHITECTURE.md` — packaging layers, namespace map, per-component
  responsibility with evidence and confidence.
- `docs/COMPONENT_INVENTORY.md` — every component, with method/field/property
  counts read independently from the binary.
- `docs/TECHNOLOGY_STACK.md` — verified technologies, executable characteristics,
  protection mechanisms, native imports, tooling, unresolved hypotheses.
- `docs/ASSEMBLY_REFERENCES.md` — **complete 38-row `AssemblyRef` inventory**,
  all fields, validated by two independent implementations; plus usage analysis,
  the 34-row `ImplMap` table, and shipped-but-unreferenced dependencies.
- `docs/DATA_FLOWS.md` — nine reconstructed flows with per-step confidence.
- `docs/NETWORK_BEHAVIOR.md` — listeners, protocols, routes, DNS, tethering,
  telemetry, with the loopback claim stated precisely.
- `docs/CONFIGURATION_AND_STATE.md` — the three-layer configuration model, all 32
  `Settings` keys, 17 `ConfigSettings` keys, 34 `DefaultPreferences` properties,
  licence schema, persistence.
- `docs/REVERSE_ENGINEERING_METHOD.md` — protection defeat, validated metadata
  geometry, call-graph construction, independent verification, reproduction steps.
- `docs/FINDINGS.md` — findings narrative and the systemic observation.
- `docs/SECURITY_ASSESSMENT.md` — ratings, threat model, remediation, disclosure.
- `docs/LIMITATIONS.md` — what is not known, deliberate non-goals, and what would
  close each gap.
- `docs/VALIDATION.md` — cross-checks, invariants, preserved corrections, tooling
  errors.
- `docs/REFERENCES.md` — specifications, tools, related work.

**Diagrams** (`architecture/`, Mermaid)
- `system-context.mmd`, `component-architecture.mmd`, `connection-flow.mmd`,
  `update-flow.mmd`, `trust-boundaries.mmd` — solid edges confirmed, dashed edges
  inferred, `UNRESOLVED` labelled explicitly.

**Findings**
- `NF-01` TLS certificate validation returns `true` unconditionally — `CONFIRMED`,
  Critical.
- `NF-02` Update payload has no integrity verification — `CONFIRMED` defect,
  High; launch `UNRESOLVED`.
- `NF-03` Default-gateway restoration is single-sited — `CONFIRMED`, Medium.
- `NF-04` Unauthenticated session-wide IPC channel — `CONFIRMED` channel, Medium;
  receiver `UNRESOLVED`.
- `NF-05` Embedded third-party API key — `CONFIRMED`, Low; value redacted.
- `NF-06` Update payload launch — `UNRESOLVED`, not rated.
- `NF-07` Embedded protected assembly — `UNRESOLVED`, not rated.
- `NF-08` Anti-tamper blocks third-party audit — `CONFIRMED`, Informational.

**Evidence and tooling**
- `evidence/EVIDENCE_INDEX.md` — claim-to-evidence mapping and redaction register.
- `evidence/HASH_MANIFEST.md` — SHA-256 for every published file.
- `evidence/VALIDATION_SUMMARY.md` — one-page validation status.
- `tools/SrmProbe/` — self-contained, read-only .NET metadata reader for
  independently re-checking the published metadata claims.

**Project**
- `SECURITY.md`, `CONTRIBUTING.md`, `.gitignore`.

### Research status at release

| | |
|---|---|
| Method bodies decoded | 2,236 of 2,239 (99.96 %) |
| Call-graph edges | 12,064, 0 inferred, 0 bad tokens |
| Assembly references validated | 38 of 38 |
| Dynamic experiments executed | **0 of 7** |
| Research status | **Partially reverse-engineered** |

### Corrections published in this release

Seven claims from earlier research rounds were withdrawn or downgraded, and are
preserved rather than deleted. The most consequential:

- The obfuscator **never** corrupted the `MemberRef` parent column — a 16-byte
  `MethodDef` row had shifted every offset.
- The `Valid` bitmask was **never** tampered with — the field is 8 bytes; the
  reader truncated it to 4.
- "Arbitrary code execution via updates" was **overstated** — the missing
  verification is proven; the launch edge is not.

### Excluded from this release

| Excluded | Reason |
|---|---|
| The installer and the executable | Redistribution rights are not ours |
| 84 extracted PE images, incl. third-party DLLs | Proprietary or independently licensed |
| The embedded API key value | A live third-party credential |
| Any proof-of-concept or exploit instructions | Deliberate |
| IL decryptor / FieldRVA transform code | Protection-defeating and generalisable |
| Analyst paths, usernames, host names | No evidentiary value |

---

## Future

Planned, in priority order, if the required environment becomes available:

1. Post-`.cctor` memory dump in a disposable analysis VM — would close NF-06,
   NF-07, the three residual method bodies, and the IPC receiver in one step.
2. Full instruction-level walk of the 1,313-byte update state machine.
3. BAML decoding to recover WPF event bindings.
4. AssemblyRef audit extension to the shipped third-party libraries.

See [docs/LIMITATIONS.md](docs/LIMITATIONS.md#7-what-would-close-the-open-questions).

---

[0.1.0]: #010--2026-10-19