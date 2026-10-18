# Architecture

> Component names are reported **as they appear in the binary**. Where a
> responsibility is assigned below a type name, that assignment is an inference
> from the type's methods, fields and call edges — not something the type states
> about itself. Confidence is given per component.

Diagrams: [system-context.mmd](../architecture/system-context.mmd) ·
[component-architecture.mmd](../architecture/component-architecture.mmd)

---

## 1. Packaging layers

The installer is two nested packaging systems. Both were decoded byte-level.

| Layer | Technology | Evidence |
|---|---|---|
| Outer | **Inno Setup 6.1.0** bootstrapper, PE32/x86 (Delphi), 33,509,274 bytes | Stub identification; `zlb` overlay magic at the payload boundary; LZMA1 overlay |
| Inner | **WiX Burn** bundle, ~89.7 MiB decompressed | Burn structure; two MSCF cabinets; **84 embedded PE images** |
| Subject | `NetMod.exe`, 625,664 bytes, at decompressed offset `0xE7FBEB` | Extracted PE; hash recorded in [HASH_MANIFEST](../evidence/HASH_MANIFEST.md) |

Payload inventory (identity established from exports, PDB paths and strings):

| Component | Role |
|---|---|
| `NetMod.exe` | The managed client — the subject of this research |
| `v2ray.exe` (~21 MB, Go) | VMess / VLESS / Trojan / Shadowsocks transport |
| `ssr-local.exe` | ShadowsocksR |
| `badvpn-tun2socks.exe` | Userspace TUN↔SOCKS bridge |
| `dnstt-client.exe` | DNS tunnelling |
| `pdnsd.exe` | DNS forwarder |
| `plink.exe` | PuTTY SSH client |
| `tapinstall.exe` (x86/x64) + `tap0901.sys` / `.cat` / `.inf` | TAP-Windows driver installation |
| `geoip.dat`, `geosite.dat` | Geo databases |
| Cygwin/MSVCR/OpenSSL runtime DLLs | Native runtime support |
| 59 further managed assemblies | EF Core 3.1 + providers, MahApps/MaterialDesign/GMap, ZXing, AppCenter, SQLitePCLRaw, and an unexplained EF6 + `System.Data.SQLite` legacy stack — see [ASSEMBLY_REFERENCES.md §6](ASSEMBLY_REFERENCES.md#6-present-in-the-installer-but-not-referenced) |

> The payload paths shown in the binary are relative, e.g.
> `lib/x86/badvpn-tun2socks.exe`. That is where the client looks for its helpers.

---

## 2. The managed client — layer map

`NetMod.exe` contains 305 type definitions across 20 namespaces. The namespaces
map cleanly onto layers.

| Layer | Namespaces | Types | Responsibility |
|---|---|---:|---|
| Presentation | `NetModMaterial.UI.Window` | 14 | Windows: main, Metro base, Socks, V2Ray, About, Log |
| | `NetModMaterial.UI.Tab` | 10 | Tabs: Settings, Proxy, SSH, V2Ray, Pay |
| | `NetModMaterial.UI.Dialog` | 9 | Modal dialogs, including the HWID dialog |
| | `NetModMaterial.UI.Adapter` | 14 | List adapters |
| | `NetModMaterial.UI.Menu` | 3 | Context menus |
| | `NetModMaterial.Widget` | 5 | Custom controls |
| View models | `NetModMaterial.Model` | 10 | `MainWindowModel`, `ProxyModel`, `SshModel`, `V2RayModel`, `TabItemModel`, `CreditModel`, `PayGenModel` |
| Connection services | `NetModMaterial.Service` | 5 | `VpnService`, `ProxyServer`, `V2RayService`, `SshService`, `ServiceNotifier` |
| Core | `NetModMaterial.Utils` | 37 | Config, encryption, network management, update, V2Ray config, process control, logging |
| | `NetModMaterial.Utils.Wifi` | 8 | WLAN / tethering |
| | `NetModMaterial.Utils.Notification` | 3 | Toast / tray notifications |
| Persistence | `NetModMaterial.Properties` | 3 | `Settings`, `ConfigSettings`, `Resources` |
| | `NetModMaterial.Migrations` | 3 | EF Core migrations |
| Interop | `NETCONLib` | 9 | Windows Connection Manager (ICS) COM wrappers |
| Root | `NetModMaterial` | 2 | `App`, `MainWindow` |
| Compiler | `Microsoft.CodeAnalysis`, `XamlGeneratedNamespace`, `System.Runtime.CompilerServices` | 4 | Build artefacts |
| Leftover | `NetModMaterial.Test` | 1 | A test `UserControl1` shipped in release |
| Global | *(unnamed)* | 165 | Compiler-generated closures/async state machines, COM and P/Invoke structs, obfuscated types |

> **Note on the type census.** Two different counts appear in the underlying
> research: 305 `TypeDef` rows in total, of which **243 own at least one method**.
> Reports that list ~132 "global" types are counting only method-owning types.
> Both numbers are correct for their definition; see
> [VALIDATION.md](VALIDATION.md#census-reconciliation).

---

## 3. Component responsibilities, evidence and confidence

### 3.1 `App` — entry point and single-instance coordinator

| | |
|---|---|
| **Responsibility** | Create the WPF application, enforce single-instance operation, host the tray icon |
| **Evidence** | MethodDef `0x0600007E` = `App.Main`; `0x06000075` = `App..ctor`; entry token `0x0600007E` in COR20 |
| **Inputs** | Command line (`Environment.GetCommandLineArgs`) |
| **Outputs** | `Application.Run`; `SendMessage`; tray `NotifyIcon` |
| **Dependencies** | `System.Threading.Mutex`, `NativeMethods` (`GlobalAddAtom`, `SendMessage`) |
| **Confidence** | **CONFIRMED** |

`Main` is 18 bytes and has exactly one path. The constructor creates a tray
icon, reads the command line, and creates a **named mutex** using the entry
assembly's simple name. If the mutex is already held, the second instance passes
its argument to the first through the atom table and a broadcast message, then
shuts down. See [NF-04](../findings/confirmed/NF-04-ipc-broadcast-channel.md).

### 3.2 `VpnService` — tunnel establishment

| | |
|---|---|
| **Responsibility** | Resolve the TAP adapter, configure it, spawn `tun2socks`, install routes and DNS, tear the whole thing down |
| **Evidence** | `0x060006BF` (1145 bytes), `0x060006C0`, `0x060006C3`, `0x060006C5`; WMI query literals; recovered argument templates |
| **Inputs** | Configuration from `DefaultPreferences`; `NetworkManagement` adapter inventory |
| **Outputs** | Child process; route table; interface DNS; `10.4.2.1/24` on the TAP device |
| **Dependencies** | `NetworkManagement`, `ProcessRunner`, `NtpUdpClient`, `Constants` |
| **Confidence** | **CONFIRMED** for the operations; **INFERRED** for their ordering |

Recovered argument template for the helper process — these are `tun2socks` flags:

```
--tundev "tap0901:<if>…:10.4.2.1:10.4.2.0:255.255.255.0"
--netif-ipaddr 10.4.2.2
--netif-netmask 255.255.255.0
--socks-server-addr 127.0.0.1:<local proxy port>
--udpgw-connection-buffer-size <n>
--socks5-udp
--udpgw-remote-server-addr 127.0.0.1:<port>
-c <config file>
```

Adapter discovery uses WMI:
`SELECT * FROM Win32_NetworkAdapter WHERE Manufacturer = 'TAP-Windows Provider V9' AND NetConnectionID = '<tap_network_name>'`.

### 3.3 `ProxyServer` / `ProxyHandler` — the local proxy

| | |
|---|---|
| **Responsibility** | Accept local SOCKS5/HTTP proxy traffic; for `CONNECT`, terminate TLS upstream |
| **Evidence** | `0x0600067F` (bind), `0x06000682` (request parsing, literal `http`), `0x06000691` (CONNECT handler) |
| **Inputs** | Local TCP connections |
| **Outputs** | Upstream TCP/TLS |
| **Dependencies** | `SslStream`, `TcpClient` |
| **Confidence** | **CONFIRMED** |

The bind address literal present in the listener body is `127.0.0.1`. This is
consistent with a loopback-only listener, but was **not confirmed by observing a
socket** — see [NETWORK_BEHAVIOR.md](NETWORK_BEHAVIOR.md).

### 3.4 `V2RayService` / `V2RayConfig` — upstream transport

| | |
|---|---|
| **Responsibility** | Generate a V2Ray JSON configuration and launch the bundled `v2ray` binary |
| **Evidence** | 46-method `V2RayConfig` with the full JSON key vocabulary in literals; `0x060006AE`; launch at `0x060006B3` / `0x060006B4` |
| **Inputs** | Server profiles, transport settings, SNI, path |
| **Outputs** | `profile.json`; child process with `-config "<file>"` or `-c "<file>" -u…` |
| **Dependencies** | `Constants` for paths; `ProcessRunner` |
| **Confidence** | **CONFIRMED** |

Confirmed protocol and transport vocabulary includes VMess, VLESS, Trojan (with
XTLS), Shadowsocks/ShadowsocksR; transports TCP, mKCP, WebSocket, HTTP/2, QUIC and
gRPC, all with TLS; plus FakeDNS and a SOCKS inbound.

### 3.5 `SshService` — SSH transport

| | |
|---|---|
| **Responsibility** | SSH-based tunnelling (PuTTY `plink` is bundled) |
| **Evidence** | Type exists; settings `ssh_local_port`, `ssh_udpgw_port`, `ssh_udpgw_buff`, `ssh_compress`, `ssh_reconnect_max` are read |
| **Confidence** | **CONFIRMED** (type and settings) / **UNRESOLVED** (exact argv and auth method) |

### 3.6 `UpdateApi` — self-update

| | |
|---|---|
| **Responsibility** | Query for a newer version, obtain the payload, and (possibly) run it |
| **Evidence** | `UpdateApi` (6 methods); `<CheckForUpdate>d__11` (`0x0600028A`, 1313 bytes); `Constants 0x060000FA` |
| **Inputs** | Two URLs on one host |
| **Outputs** | Version string; payload |
| **Confidence** | **CONFIRMED** for acquisition; **UNRESOLVED** for launch — see [NF-06](../findings/unresolved/NF-06-update-payload-launch.md) |

### 3.7 `NetworkManagement` — host network state

| | |
|---|---|
| **Responsibility** | Adapter discovery and configuration, route table, DNS, interface inventory |
| **Evidence** | WMI queries, `netsh` argument templates, IP Helper P/Invokes (`0x0600020C`–`0x0600020F`, `0x06000357`) |
| **Confidence** | **CONFIRMED** |

This is the component that mutates the host. It holds the saved default-gateway
entry and the list of routes it added, so that it can put both back.

### 3.8 `ConfigPreferences` / `Settings` / `CustomSettingsProvider`

| | |
|---|---|
| **Responsibility** | Layered configuration: compiled defaults → user settings → licence-imposed settings |
| **Evidence** | Provider emits the classic `configuration/userSettings/setting` XML; 32 `Settings` keys and 15 `ConfigSettings` keys recovered |
| **Confidence** | **CONFIRMED** |

### 3.9 `DbManager` / `Migrations` — persistence

| | |
|---|---|
| **Responsibility** | EF Core + SQLite profile store |
| **Evidence** | Migration ids `20221005033905_initialcreate` and `20230416125047_AddedPosToProfile`; `__EFMigrationsHistory` insert |
| **Confidence** | **CONFIRMED** (mechanism, migration ids) / **UNRESOLVED** (column added by the second migration) |

### 3.10 `TetheringManager` — downstream sharing

| | |
|---|---|
| **Responsibility** | Share the host's (tunnelled) upstream with other devices via ICS / Wi-Fi Direct |
| **Evidence** | `NETCONLib` COM wrappers; 14 `wlanapi` imports; failure strings |
| **Confidence** | **CONFIRMED** (API surface) / **INFERRED** (that it is started inside the connect path) |

### 3.11 `MainJSON` / `Encryption` — licensing

| | |
|---|---|
| **Responsibility** | Read/write a licence document; machine binding; expiry |
| **Evidence** | Full field vocabulary in literals; two filenames `.nm` and `_unlocked.nm`; `Encryption` throws `'Unknown mode!'` for an unknown mode |
| **Confidence** | **CONFIRMED** (schema) / **UNRESOLVED** (HWID derivation — deliberately not reversed) |

### 3.12 `<Module>` — anti-tamper and the protected payload

| | |
|---|---|
| **Responsibility** | Decrypt method bodies at start-up; decode and load an embedded assembly |
| **Evidence** | `<Module>.cctor` (`0x06000001`) calls `0x0600000A` (decryptor) plus helpers; `0x06000007` (xorshift32 + FieldRVA); `0x06000006` (LZMA); then `Assembly.Load` |
| **Confidence** | **CONFIRMED** for stages 1–2 and the load; **UNRESOLVED** for stage 3 — see [NF-07](../findings/unresolved/NF-07-embedded-assembly-gap.md) |

---

## 4. Cross-cutting design observations

**Layering.** The namespaces form a conventional MVVM presentation layer over
service objects over a utility core. **INFERRED FROM CONTROL FLOW** — the
direction of dependency is visible in the call graph, but no interface boundary is
enforced.

**Front-end, not engine.** The client owns *orchestration* and *host state*, not
protocol cryptography. Consequence: the security properties of the tunnel depend
on four separate third-party binaries that are not covered by this analysis.

**Stateful host mutation.** `NetworkManagement` and `VpnService` keep the
information needed to undo what they did. That is good design and it is
implemented — on the orderly path. Nothing was found that guarantees it after a
crash.

**Single point of responsibility for restoration.** Restoration lives in one
method. That is auditable; it also means a single early return or exception skips
it. See [NF-03](../findings/confirmed/NF-03-default-gateway-restore.md).

---

## 5. Explicit non-architecture

The following are **not** part of the managed client and were not analysed:

* the native protocol cores (Go binaries, `plink`, `pdnsd`);
* the TAP driver and its installer;
* the third-party .NET libraries (EF Core, MahApps, GMap, ZXing, AppCenter …);
* the WiX Burn bundle's install-time behaviour and registry layout.

Their presence and identity are established; their internals are not claimed.