# Tools

## What is published here

One self-contained tool, because it is the piece that lets a third party
independently verify the largest set of published claims.

```
tools/SrmProbe/
├── Program.cs        # the probe
└── SrmProbe.csproj   # project file
```

### What it does

Reads a .NET assembly **as a file** and reports, using .NET's own ECMA-335
reader:

* assembly identity, version, flags, hash algorithm;
* metadata row counts for the tables this research depends on;
* the **complete `AssemblyRef` inventory** — name, version, flags, public key
  token, culture, hash length;
* `TypeRef` resolution-scope histogram and per-assembly usage;
* per-type method/field/property counts for the types this research discusses;
* the largest types by method count;
* a **sum invariant**: per-type method counts must equal the `MethodDef` row count.

### What it does **not** do

| | |
|---|---|
| Load or execute the input | It is opened with `File.OpenRead` and `PEReader`. The assembly is **never loaded into the runtime**. |
| Modify the input | Read-only throughout |
| Contact a network endpoint | No networking code exists |
| Modify host state | No routes, DNS, registry, services, drivers or firewall changes |

---

## Running it

```
cd tools/SrmProbe
dotnet run -c Release -- <path-to-NetMod.exe> output.txt
```

Requires the **.NET SDK 9.0.319** (or a compatible 9.x SDK); built and verified
against runtime 9.0.21.

Expected output for the analyzed artifact:

```
TypeDef=305  MethodDef=2452  AssemblyRef=38
per-type method sum = 2452
```

Anything else means the input is not the artifact this research describes —
compare against the hash in [../evidence/HASH_MANIFEST.md](../evidence/HASH_MANIFEST.md).

---

## Why a .NET tool rather than a Python one

The most consequential finding of the publication stage came from using two
independent implementations.

The primary Python walker reported that **zero** `TypeRef` rows resolved to an
`AssemblyRef` — which would have meant the entire 38-row dependency table was dead
code. That result was wrong, and wrong in a way that looked entirely plausible.

The .NET reader reported **621**. The discrepancy traced to the coded-index tag
mapping for `TypeRefOrDefResolutionScope`: the Python walker assumed the mapping
from the specification text, and for this image the mapping is
`tag 2 = AssemblyRef`, `tag 3 = TypeRef`.

Publishing this tool lets a reader run the other half of that comparison against
any assembly.

---

## Tools **not** published, and why

| Tool | Why not |
|---|---|
| The IL decryptor | Protection-defeating code. Publishing it would hand over a ready-made tool for the same protection in general |
| The xorshift32 / FieldRVA stage-2 implementation | Same reason |
| The IL disassembler and call-graph builder | Analysis-only, but it embeds the same layout knowledge; the method is documented instead |
| The payload decoders (`LZMADecompSmall` reimplementation) | Could not be completed; nothing to publish |
| Round-by-round audit scripts | Superseded by this probe; kept out to avoid presenting several partially-overlapping readers as equally authoritative |

The methodology needed to re-derive the metadata and call-graph results **is**
documented in [../docs/REVERSE_ENGINEERING_METHOD.md](../docs/REVERSE_ENGINEERING_METHOD.md),
including the exact geometry, the row sizes, and the invariants that catch a
mis-parse.

---

## Analysis toolchain used during the research

| Tool | Version | Role |
|---|---|---|
| Python | 3.12 (msys2 ucrt64) | Primary analysis scripts |
| .NET SDK | 9.0.319 | Opcode table; metadata oracle |
| REA MCP — Ghidra | 12.1.4 | Disassembly |
| REA MCP — `rea-dotnet-static` | v1 | Early structure analysis |
| 7-Zip | 26.02 x64 | Container probing |
| msys2 `strings`, `objdump` | — | Native identification |

None of these are required to re-check the published metadata claims.

---

## A note on defensive use

This repository documents how one protected .NET assembly was analysed, because
the findings are only useful if they can be checked. The published tool is
strictly a **reader**: it opens a file and prints metadata.

It cannot decrypt, execute, patch, or instrument anything. If you need to assess a
.NET binary, this tool will tell you what it references — which is often enough
to identify the libraries involved and whether an obvious dependency risk exists.