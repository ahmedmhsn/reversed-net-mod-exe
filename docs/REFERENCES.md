# References

Specifications, tools and related work consulted during this research.

---

## 1. Specifications

| Reference | Used for |
|---|---|
| **ECMA-335** — *Common Language Infrastructure* | Metadata tables, row layouts, coded indices, `COR20` header |
| ECMA-335 II.22.38 `TypeRef` | `ResolutionScope` layout — see the tag-mapping caveat below |
| ECMA-335 II.22.39 `ImplMap` | `ImportScope` is a `ModuleRef` index |
| ECMA-335 II.23.1.2 `AssemblyRef.Flags` | Flag bit assignments |
| ECMA-335 II.24.2.1 Metadata root | Stream header layout, `MVID` placement |
| ECMA-335 II.24.2.6 `Valid` | **States 4 bytes; the reference implementation uses 8** — see [VALIDATION.md](VALIDATION.md#24-valid-field-tests) |
| ECMA-335 II.25.3.3 `COR20` header | Runtime flags, entry point, resource and strong-name RVAs |
| **PE/COFF** — Microsoft PE Format | Section table, data directories, RVA→file offset |
| **IETF RFC 1951** (DEFLATE), **RFC 1950** (zlib) | Container probing |
| **LZMA specification** | Range-coder framing invariants used in the stage-2/3 analysis |
| **WiX Toolset — Burn** | Bundle layout, attached containers, MSCF cabinets |
| **Inno Setup** | Installer container structure, `setup-0` layout |

> **A caution worth repeating:** two ECMA-335 statements did not match the
> artifact — the `Valid` field width and the coded-index tag order. The spec was
> treated as a starting point, not an authority. See
> [REVERSE_ENGINEERING_METHOD.md §6.1](REVERSE_ENGINEERING_METHOD.md).

---

## 2. Tools used

Versions are those recorded during the research.

| Tool | Version | Role |
|---|---|---|
| Python | 3.12 (msys2 ucrt64) | All analysis scripts |
| .NET SDK | **9.0.319** | `System.Reflection.Emit.OpCodes` (authoritative opcode table); `System.Reflection.Metadata` |
| .NET runtime | 9.0.21 | Running the metadata probe |
| REA MCP — Ghidra provider | **12.1.4** | Disassembly |
| REA MCP — `rea-dotnet-static` | **v1** | Early .NET structure analysis |
| 7-Zip | 26.02 x64 | Container probing |
| msys2 `strings`, `objdump` | — | Native identification |

The .NET metadata probe is published in [`tools/SrmProbe`](../tools/SrmProbe) so
the metadata claims can be re-checked independently.

---

## 3. Protection technology

| Reference | Relevance |
|---|---|
| **ConfuserEx** (open source) | The recovered IL-decryption transform matches its anti-tamper, Normal mode. Attribution is `INFERRED` — no version signature was found. |
| `SuppressIldasmAttribute` | Applied by the protection; blocks the standard disassembler |

ConfuserEx is referenced for **identification only**. No code from it was copied
into this repository, and the protection-defeating implementation used during the
research is not published.

---

## 4. Runtime and platform components

| Reference | Relevance |
|---|---|
| .NET Framework 4.7.2 | Target runtime (`TargetFrameworkAttribute`) |
| WPF — `PresentationFramework`, `PresentationCore`, `WindowsBase` | UI stack |
| MahApps.Metro 2.0.0.0 | Window chrome, `MetroWindow` base class |
| MaterialDesignThemes.Wpf 4.5.0.0 | `DialogHost`, `PackIcon`, `Card` |
| Entity Framework Core 3.1 | Persistence |
| EntityFrameworkCore.DataEncryption 3.0.1.0 | Column encryption |
| Windows `NETCONLib` | ICS COM interop (9 wrapper types) |
| Windows WinRT networking contracts | `NetworkOperatorTetheringManager`, `ConnectionProfile` |
| `iphlpapi` | Route table manipulation (5 imports) |
| `wlanapi` | Hosted network (14 imports) |

Third-party components remain the property of their respective authors under
their own licences. **None is redistributed here.**

---

## 5. Protocol and tooling references

| Reference | Relevance |
|---|---|
| Shadowsocks / ShadowsocksR | Protocol supported via `ssr-local.exe` |
| VMess / VLESS | V2Ray protocols configured by the client |
| Trojan, incl. XTLS | V2Ray protocol configured by the client |
| V2Ray transports: TCP, mKCP, WebSocket, HTTP/2, QUIC, gRPC | Configuration vocabulary recovered from IL |
| badvpn `tun2socks` | Userspace TUN↔SOCKS bridge; argv template recovered |
| dnstt | DNS tunnelling client |
| PuTTY `plink` | SSH transport |
| ZXing.Net | QR generation/decoding |
| GMap.NET | Map display |
| Microsoft AppCenter | Telemetry / crash reporting |

---

## 6. Repository map

| Document | Contents |
|---|---|
| [README.md](../README.md) | Entry point |
| [OVERVIEW.md](OVERVIEW.md) | Scope, status, classification scheme |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Layers, components, packaging |
| [COMPONENT_INVENTORY.md](COMPONENT_INVENTORY.md) | Every component with evidence and confidence |
| [TECHNOLOGY_STACK.md](TECHNOLOGY_STACK.md) | Verified stack and versions |
| [ASSEMBLY_REFERENCES.md](ASSEMBLY_REFERENCES.md) | All 38 references, validated |
| [DATA_FLOWS.md](DATA_FLOWS.md) | Nine reconstructed flows |
| [NETWORK_BEHAVIOR.md](NETWORK_BEHAVIOR.md) | Listeners, routes, DNS, tethering |
| [CONFIGURATION_AND_STATE.md](CONFIGURATION_AND_STATE.md) | Settings, licence, persistence |
| [REVERSE_ENGINEERING_METHOD.md](REVERSE_ENGINEERING_METHOD.md) | How it was done |
| [FINDINGS.md](FINDINGS.md) | Findings narrative |
| [SECURITY_ASSESSMENT.md](SECURITY_ASSESSMENT.md) | Severity and threat model |
| [LIMITATIONS.md](LIMITATIONS.md) | What is not known |
| [VALIDATION.md](VALIDATION.md) | Verification, corrections |
| [findings/](../findings/README.md) | Structured findings index |
| [evidence/](../evidence/EVIDENCE_INDEX.md) | Hashes, evidence index |
| [architecture/](../architecture/) | Mermaid diagrams |
| [tools/](../tools/README.md) | The published metadata probe |

---

## 7. Reproducing this work

The published probe is self-contained:

```
cd tools/SrmProbe
dotnet run -c Release -- <path-to-NetMod.exe> output.txt
```

It reads a file; it never loads or executes it. Expected output is documented in
[REVERSE_ENGINEERING_METHOD.md §8.3](REVERSE_ENGINEERING_METHOD.md).

---

## 8. Note on sources

This repository contains **no third-party code**. The single published tool was
written for this research. Where a specification, an open-source project or a
published vulnerability class informed an analysis step, it is cited above rather
than reproduced.