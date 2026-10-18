# Contributing

Thank you for considering a contribution. This project is evidence-based, and
the bar for a change is correspondingly specific.

---

## The one rule

> **Every claim must be traceable to evidence, and its classification must match
> the strength of that evidence.**

If you cannot point to bytes, metadata, or a reproducible validation, the claim
does not belong here — no matter how plausible it sounds.

---

## What a good contribution looks like

| Valuable | Not valuable |
|---|---|
| A correction backed by a file offset and a hash | A claim that "the code probably does X" |
| A **refutation** of an existing published claim | Restating a finding with stronger language |
| A newly closed `UNRESOLVED` item | Removing an `UNRESOLVED` label without evidence |
| A tool that reproduces a published number | A tool with no stated prerequisite |
| A better explanation of a preserved tooling error | Quietly fixing an error without recording it |

Corrections are **especially welcome**. Seven claims from earlier rounds were
withdrawn after publication-in-progress; finding the next one is a real
contribution.

---

## Classification

Use these labels consistently, and keep using them exactly:

| Label | Use when |
|---|---|
| `CONFIRMED` | Directly supported by bytes, metadata, or a reproducible validation |
| `INFERRED` | Supported by several observations, not fully demonstrated |
| `UNRESOLVED` | Evidence insufficient, or behaviour not recovered |
| `WITHDRAWN` | A previous claim invalidated by later evidence |

**The most common mistake** in a project like this is upgrading `INFERRED` to
`CONFIRMED` for rhetorical effect. Two findings here are specifically documented
so they are not misread:

* The absence of update-payload verification is `CONFIRMED`; whether the payload
  is executed is `UNRESOLVED`. They must not be merged.
* The IPC channel is `CONFIRMED`; what the receiver does is `UNRESOLVED`.

---

## Style

| Guideline | |
|---|---|
| Professional English | This is a reference document, not a blog post |
| Lead with what is established | Then what is not |
| Use file offsets and tokens | `0x06000254`, not "the process runner method" |
| Tables over prose | For anything enumerable |
| Keep `CONFIRMED`/`INFERRED` in the text | Not only in a table |
| Cite a document, not a feeling | Link to the document that supports the claim |

### Diagrams

Mermaid only. Conventions used in [architecture/](architecture/):

| Convention | Meaning |
|---|---|
| Solid edge | Relationship established from recovered bytes or metadata |
| Dashed edge | Inferred, not demonstrated |
| `UNRESOLVED` label | Explicitly unknown |
| Red node | A proven gap |

Please follow them. A diagram that draws an inferred flow as solid is worse than
no diagram.

---

## Redaction requirements

**Mandatory before any pull request.**

| Never publish |
|---|
| API keys, tokens, credentials, cookies, private keys, connection strings |
| Personal information, private logs, local usernames, host names |
| The original installer, the executable, or any extracted proprietary binary |
| Third-party DLLs |
| The embedded third-party API key value |
| Proof-of-concept code or exploitation instructions |
| The IL decryptor or FieldRVA transform implementations |

If you discover a credential, **remove it, report it privately, and recommend
rotation** — do not post it in an issue "so it can be tracked".

A note on build artifacts: some vendor PDB paths in the research artefacts contain
a developer's home directory. Their *meaning* (which library was compiled) is
evidence; the path itself is not. Use the conclusion, not the string.

---

## Adding a finding

1. Create a file under `findings/confirmed/`, `findings/inferred/` or
   `findings/unresolved/`.
2. Use the `NF-NN` identifier; continue the existing sequence.
3. Include: classification, severity, affected component, **evidence with offsets
   or tokens**, validation status, confidence, **limitations**, and a recommended
   defensive action.
4. Add a row to [findings/README.md](findings/README.md).
5. Mention it in [docs/FINDINGS.md](docs/FINDINGS.md) and
   [docs/SECURITY_ASSESSMENT.md](docs/SECURITY_ASSESSMENT.md).

A finding without a **limitations** section will be asked for one. Stating what a
finding does *not* establish is what makes it trustworthy.

---

## Correcting an existing claim

Do **not** edit the original text in place. Instead:

1. Add the correction to
   [docs/VALIDATION.md](docs/VALIDATION.md#corrections-preserved), with the old
   claim, the new evidence, and how it was found.
2. Update the affected documents to state the corrected position.
3. If the original claim was harmful — i.e. it could cause someone to act on a
   wrong conclusion — mark the old text `WITHDRAWN` inline rather than deleting
   it.

This is why seven withdrawn claims are still visible. They are the most useful
part of the record.

---

## Verifying the published numbers

The published metadata claims can be re-checked:

```
cd tools/SrmProbe
dotnet run -c Release -- <path-to-NetMod.exe> output.txt
```

Expected: `TypeDef=305`, `MethodDef=2452`, `AssemblyRef=38`,
`per-type method sum = 2452`.

**You must supply the assembly yourself.** Do not add the artifact to this
repository.

---

## Code of conduct

Assume good faith. Disagreement about evidence is technical and welcome;
disagreement about people is not. Criticise claims, not authors.

Corrections that make the project more accurate are always welcome, including
corrections of the maintainer's work.

---

## Licence

This repository contains original documentation only. See
[SECURITY.md](SECURITY.md#trademarks-and-licensing) for the licensing and
trademark position, which deliberately does **not** apply an open-source licence
over third-party material.