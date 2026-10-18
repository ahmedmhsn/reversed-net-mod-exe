# Component Inventory

Every component listed here is backed by a specific evidence source. Confidence
labels are used consistently:

| Label | Meaning |
|---|---|
| **CONFIRMED** | Directly supported by recovered bytes or metadata |
| **INFERRED** | Supported by several observations, not fully demonstrated |
| **UNRESOLVED** | Evidence insufficient |

Type names are **as they appear in the binary**. ConfuserEx-style renaming affects
10 type identifiers, which were replaced by zero-width characters and are
**permanently unrecoverable**.

> **Method, field and property counts in this document were read from the binary
> with .NET's own metadata reader** (`System.Reflection.Metadata`), independently
> of the Python analysis tooling. The per-type method counts sum to exactly
> **2,452**, matching the `MethodDef` row count — a consistency check that a
> mis-parsed layout would fail. Reproduce with `tools/SrmProbe`.

---

## 1. Entry point and application lifecycle

| Type | Methods | Fields | Responsibility | Evidence | Confidence |
|---|---:|---:|---|---|---|
| `NetModMaterial.App` | 11 | 3 | `Main`, tray icon, single-instance coordination | MethodDef `0x0600007E` = `Main`; COR20 entry token `0x0600007E`; `0x06000075` = `.ctor`; field `NotifyIcon` | **CONFIRMED** |
| `NetModMaterial.MainWindow` | 19 | 6 | Main window | `TypeDef` | **CONFIRMED** |
| `NetModMaterial.UI.Window.MetroWindowCustom` | 2 | 0 | Window base class; calls `0x060002A8` | `TypeDef` + call edge | **CONFIRMED** |
| *(unrecoverable name)* | — | — | Called by `MetroWindowCustom` | Call edge only; **body has an illegal method header** | **UNRESOLVED** |

## 2. Connection services

| Type | Methods | Fields | Responsibility | Evidence | Confidence |
|---|---:|---:|---|---|---|
| `Service.VpnService` | 11 | 15 | TAP resolution, helper spawn, routes, DNS, teardown | Recovered bodies; WMI literals; argv templates; properties `Alive`, `DnsAddresses` | **CONFIRMED** |
| `Service.ProxyServer` | 11 | 20 | Local SOCKS5/HTTP listener | Property `ListenAddress`, field `_listener`; body at `0x0600067F` binds | **CONFIRMED** |
| `Service.ProxyHandler` | — | — | Request parsing; TLS `CONNECT` | Body `0x06000691` | **CONFIRMED** |
| `Service.V2RayService` | 14 | 9 | Generate V2Ray config; launch the Go binary | Decoded bodies; `-config`/`-c` literals | **CONFIRMED** |
| `Service.SshService` | — | — | SSH transport | Type + 5 settings keys | **CONFIRMED** (surface), **UNRESOLVED** (argv) |
| `Service.ServiceNotifier` | — | — | User-facing status | `TypeDef` | **CONFIRMED** |

### Key connection methods

| Token | Role |
|---|---|
| `0x060006BF` | Connect: adapter → config file → helper spawn (1145 IL bytes) |
| `0x060006C0` | Route installation; default-gateway removal |
| `0x060006C3` | Disconnect: kill helper, remove routes, **restore gateway** |
| `0x060006C5` | Aborted-path cleanup |

## 3. TLS

| Item | Token | Role | Evidence | Confidence |
|---|---|---|---|---|
| `ProxyHandler` | `0x06000691` | HTTP `CONNECT`; terminates upstream TLS | 505-byte body, decodes to offset 371 | **CONFIRMED** up to that point |
| `SslConnectionHandler` | `0x06000456` | Direct outbound TLS client | 562-byte body, fully decoded | **CONFIRMED** |
| *(callback)* | `0x06000457` | Certificate-validation callback | RVA `0x10665`, body `17 2a` = `ldc.i4.1 ; ret` | **CONFIRMED** |
| *(callback)* | `0x06000692` | Same callback, second binding site | Identical RVA and body | **CONFIRMED** |

See [NF-01](../findings/confirmed/NF-01-tls-certificate-validation.md).

## 4. Configuration and persistence

| Type | Methods | Fields | Props | Responsibility | Evidence | Confidence |
|---|---:|---:|---:|---|---|---|
| `Properties.Settings` | 67 | 1 | **32** | User settings | Property names read from metadata | **CONFIRMED** |
| `Properties.ConfigSettings` | 37 | 1 | **17** | Licence-imposed settings | Property names | **CONFIRMED** |
| `Utils.CustomSettingsProvider` | 17 | 7 | 5 | Writes the `userSettings` XML | Provider literals; properties `PropertiesType`, `UserConfigPath`, `SettingsDictionary` | **CONFIRMED** |
| `Utils.ConfigPreferences` | 42 | 2 | 17 | Typed view over licence settings | Properties `ExpireDate`, `Payload`, `Sni`, `RrMode`, `RrManual`, `*Lock`, `ProxyCb`, `SshCb`, `SslCb`, `*Index` | **CONFIRMED** |
| `Utils.DefaultPreferences` | 67 | 2 | 34 | Resolved service defaults | 34 properties incl. `HWID`, `ConnectionMode`, `RemoveDefGw`, `TapNetworkName`, `Theme` | **CONFIRMED** |
| `Utils.DbManager` | 12 | 5 | 4 | EF Core + SQLite, migrations | Properties `Proxy`, `V2ray`, `Ssh`, `Instance` | **CONFIRMED** |
| `Utils.MainJSON` | 20 | **35** | 0 | Licence document read/write | Field count and literals | **CONFIRMED** |
| `Utils.Encryption` | 5 | 1 | 0 | Mode-parameterised cipher | `'Unknown mode!'` throw | **CONFIRMED** (shape), **UNRESOLVED** (algorithm) |
| `Migrations.initialcreate` | 4 | — | — | Migration `20221005033905_initialcreate` | Type name | **CONFIRMED** |
| `Migrations.AddedPosToProfile` | 4 | — | — | Migration `20230416125047`; added column not determined from IL | Type name | **CONFIRMED** (id) / **UNRESOLVED** (column) |
| `Properties.Resources` | 388 | 2 | **386** | Localisation/accessor class | Property names | **CONFIRMED** |

> `Properties.Resources` declaring **386 properties** is consistent with a
> generated accessor class — but the backing store is **not** in this file. The
> COR20 managed resource directory is `0/0`, and both `ManifestResource` rows
> resolve to **AssemblyRef #24**, an external obfuscated name. The accessors are
> resolved at runtime by an `AppDomain.AssemblyResolve` handler backed by the
> FieldRVA payload. See
> [NF-07](../findings/unresolved/NF-07-embedded-assembly-gap.md) and
> [ASSEMBLY_REFERENCES.md §5.1](ASSEMBLY_REFERENCES.md#51-assemblyref-24-is-the-declared-home-of-the-wpf-resources).

## 5. Network and host state

| Type | Methods | Fields | Responsibility | Evidence | Confidence |
|---|---:|---:|---|---|---|
| `Utils.NetworkManagement` | 24 | 24 | Adapters, routes, DNS | WMI + netsh literals; properties `OnRouteTableChanged`, `IsNetworkValid` | **CONFIRMED** |
| `Utils.NetworkStatusReceiver` | 23 | 7 | WMI route-change watcher (singleton) | `SELECT * FROM Win32_IP4RouteTableEvent`; property `Instance` | **CONFIRMED** |
| `Utils.ProcessRunner` | 9 | 5 | **The only** `Process.Start` site in the binary | `0x06000254` sets `FileName`/`Arguments`/`UseShellExecute`/`CreateNoWindow` then `Start` | **CONFIRMED** |
| `Utils.ProcessWatcher` | — | — | Child liveness polling | `TypeDef` | **CONFIRMED** |
| `Utils.ChildProcessTracker` | — | — | Job-object containment | `CreateJobObject`/`SetInformationJobObject`/`AssignProcessToJobObject`; static `s_jobHandle` | **CONFIRMED** |
| `Utils.NtpUdpClient` | — | — | Reachability probe | `<Timeout>` property; call sites in `VpnService` | **CONFIRMED** |
| `Utils.Pinger` | — | — | Liveness | `TypeDef` | **CONFIRMED** |

## 6. Tethering

| Type | Methods | Fields | Responsibility | Evidence | Confidence |
|---|---:|---:|---|---|---|
| `Utils.Wifi.TetheringManager` | 12 | 4 | WinRT `NetworkOperatorTetheringManager` + ICS | Properties `Ssid`, `Password`, `NetworkStatusChanged`, `State`; failure strings | **CONFIRMED** (API), **INFERRED** (trigger) |
| `Utils.Wifi.WlanHostedNetworkManager` | 23 | — | Wi-Fi Direct hosted network | 14 `wlanapi` imports | **CONFIRMED** |
| `Utils.ProxyTethering` | — | — | SOCKS side of sharing | `_listener` field | **CONFIRMED** |

## 7. Update

| Type | Methods | Fields | Responsibility | Evidence | Confidence |
|---|---:|---:|---|---|---|
| `Utils.UpdateApi` | 6 | 5 | HTTP client; singleton | Property `Instance`; `HttpClient.SendAsync` in the call graph | **CONFIRMED** |
| `<CheckForUpdate>d__11` | — | — | Async state machine | `0x0600028A`, 1313 IL bytes | **CONFIRMED** (partially decoded) |
| `<GetUpdate>d__8` | — | — | Async state machine | Compiler-generated type name | **CONFIRMED** |
| `Utils.Constants` | 26 | — | Payload URL constant | `0x060000FA` | **CONFIRMED** |

## 8. V2Ray configuration generation

| Type | Methods | Fields | Props | Responsibility | Evidence |
|---|---:|---:|---:|---|---|
| `Utils.V2RayConfig` | 46 | 21 | 14 | Builds the entire JSON config | Complete key vocabulary in literals; properties `Address`, `Port`, `ListenPort`, `Protocol`, `TrojanCfg`, `SsrConfig`, `DnsAddress`, … |
| `Utils.V2RayAddressConverter` | 3 | 0 | — | Address normalisation | `TypeDef` |
| `Utils.V2RayProtoConverter` | 3 | 0 | — | Protocol normalisation | `TypeDef` |

## 9. Presentation

| Group | Types | Notable |
|---|---:|---|
| `UI.Window` | 14 | `MetroWindowCustom` (2 methods); window classes |
| `UI.Tab` | 10 | `V2RayTab` (27 methods) |
| `UI.Adapter` | 14 | `AdapterProxy`, `AdapterSsh`, `AdapterV2Ray` — **40 methods each** |
| `Model` | 10 | `V2RayModel` (49), `ProxyModel` (26), `SshModel` (24), `MainWindowModel` |
| `Widget` | 5 | `CustomTextBox` |
| `Properties.Resources` | — | 388 methods, 386 properties — the largest type in the binary |

## 10. Largest types in the binary

| Methods | Type |
|---:|---|
| 388 | `NetModMaterial.Properties.Resources` |
| 67 | `NetModMaterial.Utils.DefaultPreferences` |
| 67 | `NetModMaterial.Properties.Settings` |
| 49 | `NetModMaterial.Model.V2RayModel` |
| 46 | `NetModMaterial.Utils.V2RayConfig` |
| 42 | `NetModMaterial.Utils.ConfigPreferences` |
| 40 ×3 | `UI.Adapter.AdapterProxy` / `.AdapterSsh` / `.AdapterV2Ray` |
| 37 | `NetModMaterial.Properties.ConfigSettings` |
| 30 | `NetModMaterial.Utils.Utility` |
| 27 | `IpInfoModel` |
| 27 | `NetModMaterial.UI.Tab.V2RayTab` |
| 26 | `NetModMaterial.Utils.Constants` |
| 26 | `NetModMaterial.Model.ProxyModel` |
| 24 | `NetModMaterial.Utils.NetworkManagement` |
| 24 | `NetModMaterial.Model.SshModel` |

## 11. Native interop — 34 declarations

| Module | Count | Functions |
|---|---:|---|
| `kernel32.dll` | 8 | `VirtualProtect` ×2, Job Object trio, `GlobalAddAtom`, `GlobalDeleteAtom`, `GlobalGetAtomName` |
| `wlanapi.dll` | 14 | Hosted-network and notification APIs |
| `user32.dll` / `user32` | 5 | `GetClassName`, `SendMessage`, `RegisterWindowMessage`, `RegisterDeviceNotification`, `UnregisterDeviceNotification` |
| `iphlpapi` / `Iphlpapi.dll` | 5 | `GetBestRoute`, `GetIpForwardTable`, `CreateIpForwardEntry`, `DeleteIpForwardEntry`, `GetBestInterfaceEx` |
| `dwmapi.dll` | 1 | `DwmSetWindowAttribute` |
| *(anomalous)* | 1 | Row 22 / `0x0600034B`: empty import name, out-of-range module index |

Full table with tokens: [ASSEMBLY_REFERENCES.md §7](ASSEMBLY_REFERENCES.md#7-native-interop-implmap-34-rows).

## 12. Compiler-generated and unrecoverable

| Item | Note |
|---|---|
| Async state machines | `<…>d__N` types; each is one method |
| Closures / display classes | `<>c__DisplayClassN_M`, `<>f__AnonymousTypeN` |
| Compiler caches | `<>9__N_0` |
| **Types renamed to zero-width characters** | **10** — ConfuserEx name protection; **original identifiers are not stored in the binary — unrecoverable** |
| EF migration snapshot | `DbManagerModelSnapshot` |

## 13. Components that are NOT part of the managed client

Present in the installer, not analysed:

| Component | Why excluded |
|---|---|
| `v2ray.exe`, `ssr-local.exe`, `badvpn-tun2socks.exe`, `dnstt-client.exe`, `pdnsd.exe`, `plink.exe` | Native/Go binaries; separate codebases |
| `tapinstall.exe`, `tap0901.sys` | Driver tooling |
| 59 bundled managed libraries | Third-party, independently licensed; only their `AssemblyRef` entries are in scope — see [ASSEMBLY_REFERENCES.md §6](ASSEMBLY_REFERENCES.md#6-present-in-the-installer-but-not-referenced) |

Their presence and identity are **CONFIRMED**; their internals are **not claimed**.