# Technology Stack

Everything below was **verified** against the artifact. Nothing is listed because
it would be appropriate for the task — each entry has a stated source. Where a
version could not be read from metadata it is marked as such.

Cross-reference: the managed dependency inventory with all 38 rows is in
[ASSEMBLY_REFERENCES.md](ASSEMBLY_REFERENCES.md).

---

## 1. Confirmed application technologies

| Technology | Version read | Evidence |
|---|---|---|
| .NET Framework | **4.7.2** | `TargetFrameworkAttribute` string `.NETFramework,Version=v4.7.2` in the metadata; `FrameworkDisplayName = ".NET Framework 4.7.2"` |
| WPF | — | `AssemblyRef`: `PresentationFramework`, `PresentationCore`, `WindowsBase`, `System.Xaml` |
| Windows Forms (tray) | — | `AssemblyRef`: `System.Windows.Forms`; `NotifyIcon` usage |
| Entity Framework Core | **3.1.0.0** | `AssemblyRef` version |
| Entity Framework Core Sqlite | **3.1.0.0** | `AssemblyRef` version |
| Entity Framework Core Relational | **3.1.0.0** | `AssemblyRef` version |
| Entity Framework Core DataEncryption | **3.0.1.0** | `AssemblyRef` version; no public key token |
| MahApps.Metro | **2.0.0.0** | `AssemblyRef` version |
| MaterialDesignThemes.Wpf | **4.5.0.0** | `AssemblyRef` version |
| GMap.NET (WindowsPresentation / Core) | **2.1.7.0** | `AssemblyRef` version |
| ToastNotifications | **2.5.1.0** | `AssemblyRef` version |
| ZXing (+ presentation) | **0.16.9.0** | `AssemblyRef` version |
| Newtonsoft.Json | **13.0.0.0** | `AssemblyRef` version |
| HtmlAgilityPack | **1.11.46.0** | `AssemblyRef` version |
| INIFileParser | **2.5.2.0** | `AssemblyRef` version |
| Microsoft AppCenter (+ Analytics, + Crashes) | 0.0.0.0 | `AssemblyRef` name; version is the assembly's own build stamp, not a package version |
| MVVM | — | `Model` namespace; `ViewModelBase`; `TabItemModel`; observable-collection backing fields |
| SecureCRT session files | — | Literal `\sshprofiles.s2h` |

> `MahApps.Metro` + `MaterialDesignThemes.Wpf` together, plus a `MetroWindow`
> subclass, identify the UI toolkit unambiguously.

---

## 2. Confirmed executable / runtime characteristics

Read directly from the COR20 header and the `Assembly` table:

| Property | Value | Source |
|---|---|---|
| Machine | **I386 (x86)** | COR20 / `CoffHeader.Machine = 0x014C` |
| CLI flags | **0x0003** = `ILONLY \| 32BITREQUIRED` | COR20 `Flags` |
| Runtime version | **2.5** | COR20 `MajorRuntimeVersion / MinorRuntimeVersion` |
| Entry point token | **0x0600007E** | COR20 `EntryPointTokenOrRva` (a token, because `ILONLY`) |
| Assembly name | **NetMod** | `Assembly` row, `#Strings` index `0x4F11` |
| Assembly version | **4.6.0.415** | `Assembly` row |
| Hash algorithm | **SHA1** (`0x8004`) | `Assembly.HashAlgId` |
| Assembly flags | **0x00000000** | `Assembly.Flags` |
| Strong name | **absent** | `PublicKeyOrToken` blob index = 0; `StrongNameSignature` RVA/size = 0/0 |
| Managed resources directory | **absent** | COR20 `Resources` RVA/size = 0/0 |
| Metadata version string | `v4.0.30319` | Metadata root |
| Streams | `#~` 119,624 · `#Strings` 79,576 · `#US` 42,824 · `#GUID` 16 · `#Blob` 27,320 | Stream headers |

### Consequences of `32BITREQUIRED`

The client must run as a 32-bit process. On a 64-bit host this means WOW64, which
in turn means the helper processes are also 32-bit — consistent with the
`lib/x86/…` payload layout.

### Non-standard metadata

Two deviations from ECMA-335 were confirmed and are documented in
[REVERSE_ENGINEERING_METHOD.md](REVERSE_ENGINEERING_METHOD.md#metadata-anomalies):

* `MethodDef` rows are **16 bytes**, not the standard 8.
* the `Valid` field is **8 bytes**, not the standard 4.

Both are properties of the protection tool, not of the original source.

---

## 3. Protection and obfuscation mechanisms

| Mechanism | Status | Evidence |
|---|---|---|
| Runtime IL decryption | **CONFIRMED** | `<Module>.cctor` → decryptor rewrites method bodies in place before JIT |
| Implementation | **CONFIRMED** | Recovered transform matched against open-source ConfuserEx anti-tamper, Normal mode |
| Memory protection change | **CONFIRMED** | Two `VirtualProtect` P/Invoke declarations (`0x06000004`, `0x06000009`) |
| `SuppressIldasmAttribute` | **CONFIRMED** | First `MemberRef` row, token `0x0A000001` |
| RWX section | **CONFIRMED** | First section named `Wc9A_'CW` with characteristics `0xE0000040` |
| Metadata cross-reference poisoning | **CONFIRMED** | Call edges observed with impossible targets (`newobj → 0x16064457`) |
| Identifier renaming | **CONFIRMED** | 10 type identifiers replaced by zero-width characters |
| Additional data protection | **CONFIRMED** | A separate xorshift32 + LZMA pipeline protects an embedded assembly |
| Keying of the protection | **PARTIALLY** | The main transform's key schedule was recovered; the embedded payload's final stage is **UNRESOLVED** |

### The two protection layers are distinct

1. **Method-body encryption** — recovers to 2,236 valid bodies. Fully understood.
2. **An embedded 47,936-byte payload** — protected by anti-tamper → xorshift32 →
   LZMA. Stages 1–2 recovered and executed; stage 3 **not decoded**.

These are not the same mechanism and are tracked separately.

---

## 4. Observed native imports

Full table with tokens and module references: [ASSEMBLY_REFERENCES.md §4](ASSEMBLY_REFERENCES.md#4-native-interop-implmap-34-rows).

| Module | Count | Notable |
|---|---:|---|
| `kernel32.dll` | 8 | `VirtualProtect` ×2, Job Object trio, `GlobalAddAtom`/`GlobalDeleteAtom`/`GlobalGetAtomName` |
| `wlanapi.dll` | 14 | Hosted-network creation and notification |
| `user32.dll`, `user32` | 5 | `SendMessage`, `RegisterWindowMessage`, device notification |
| `iphlpapi`, `Iphlpapi.dll` | 5 | Route table manipulation |
| `dwmapi.dll` | 1 | `DwmSetWindowAttribute` |

**34 total.** All are declarations; whether each is *invoked* by reachable code was
not established for every row (see
[findings/unresolved/](../findings/unresolved/)).

---

## 5. Protocols implemented by bundled binaries

Not implemented in the managed client; the client configures and launches them.

| Protocol | Implementation | Confidence |
|---|---|---|
| VMess, VLESS, Trojan (+XTLS), Shadowsocks, ShadowsocksR | `v2ray.exe` (Go), `ssr-local.exe` | **CONFIRMED** from the generated configuration vocabulary |
| SOCKS5 / HTTP proxy | in-process | **CONFIRMED** |
| SSH | `plink.exe` (PuTTY) | **CONFIRMED** (binary present, settings present); **UNRESOLVED** (argv) |
| Userspace TUN | `badvpn-tun2socks.exe` | **CONFIRMED** (argument templates recovered) |
| DNS tunnelling | `dnstt-client.exe` | **CONFIRMED** (binary present) |
| ICS / Wi-Fi Direct sharing | Windows APIs via COM + `wlanapi` | **CONFIRMED** |

Transports configured for V2Ray: TCP, mKCP, WebSocket, HTTP/2, QUIC, gRPC — all
with TLS. **CONFIRMED** from the `streamSettings` key vocabulary.

---

## 6. Installer technology

| Component | Version | Evidence |
|---|---|---|
| Inno Setup | **6.1.0** | Bootstrapper identification; LZMA1 overlay; setup-0 header |
| WiX Burn | — | Bundle structure; two MSCF cabinets; attached-container layout |

> `14_REPRODUCIBILITY.md` in the source research records a decoder pitfall worth
> preserving: capping the LZMA decode at 64 MiB silently truncates the stream and
> corrupts the embedded cabinets.

---

## 7. Reverse-engineering and validation tooling

Tools **actually used**, as recorded in the research artifacts:

| Tool | Version | Used for |
|---|---|---|
| Python | 3.12 (msys2 ucrt64) | All analysis scripts |
| .NET SDK | **9.0.319** | `System.Reflection.Emit.OpCodes` (authoritative opcode table); `System.Reflection.Metadata` as an independent reader |
| .NET runtime | 9.0.21 | Running the metadata probe |
| REA MCP — Ghidra provider | **12.1.4** | Disassembly |
| REA MCP — `rea-dotnet-static` | **v1** | Early .NET structure analysis (see corrections) |
| 7-Zip | **26.02 x64** | Container probing |
| msys2 `strings`, `objdump` | — | Native identification |

A purpose-written strict CIL validator was used throughout; its calibration
result is recorded in [VALIDATION.md](VALIDATION.md).

---

## 8. Research-side scripts

The analysis scripts were written for this investigation and are **not published
as a package**. [tools/README.md](../tools/README.md) documents the one
self-contained reader that is included, and describes the others.

No script executes the target. No script contacts a network endpoint.

---

## 9. Unverified and unresolved technology hypotheses

Recorded so they are not mistaken for findings:

| Hypothesis | Status |
|---|---|
| The obfuscator is a specific ConfuserEx release | **INFERRED** — the recovered transform matches open-source ConfuserEx anti-tamper Normal mode; no version signature was found |
| The bundled V2Ray core supports XTLS/REALITY | **UNRESOLVED** — configuration keys exist; the core was not analysed |
| The embedded 47,936-byte payload is a .NET assembly | **INFERRED** — `Assembly.Load` and `AppDomain.AssemblyResolve` are proven; contents unknown |
| The Burn bundle writes specific registry keys | **UNRESOLVED** — the bundle manifest was not fully decoded |
| `PJrcRGGDZySncSGQBDxtMlBJuXsaA` is a real dependency | **NOT** — it is an obfuscated leaked interop name, not a loadable assembly |
| Trojan-Go support exists in shipped form | **CONFIRMED ABSENT** — the UI is configuration-generation only and is marked deprecated; no `trogo.exe` is bundled |