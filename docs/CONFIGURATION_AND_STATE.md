# Configuration and Persisted State

> The identifiers below are **read from the binary's metadata**, not quoted from
> prose. Their presence is `CONFIRMED`; what each one *means* is marked
> `INFERRED` unless a decoded body proves otherwise. No configuration file was
> read, because the application was never run.

---

## 1. The three-layer configuration model

| Layer | Type | Properties | Who writes it |
|---|---|---:|---|
| Compiled defaults | `NetModMaterial.Utils.DefaultPreferences` | 34 | The assembly itself |
| User settings | `NetModMaterial.Properties.Settings` | 32 | `CustomSettingsProvider`, on the user's action |
| Licence settings | `NetModMaterial.Properties.ConfigSettings` | 17 | Supplied by the licence document |

`Utils.ConfigPreferences` (42 methods, 17 properties) is the typed view that
merges these; its property names are the `ConfigSettings` keys in PascalCase,
which is strong evidence that it reads the licence-imposed layer.

Storage format: `CustomSettingsProvider` emits the classic .NET
`<configuration><userSettings>…` document — established from provider literals
including `userSettings` and `setting`. **The exact file path was not recovered.**

---

## 2. User settings — `Properties.Settings` (32 properties)

| # | Key | Inferred meaning | Evidence class |
|---:|---|---|---|
| 1 | `Default` | (settings-class accessor) | — |
| 2 | `connection_mode` | Which transport is active | `CONFIRMED` name |
| 3 | `remove_def_gw` | Remove the saved default gateway while tunnelling | **CONFIRMED** — see [NF-03](../findings/confirmed/NF-03-default-gateway-restore.md) |
| 4 | `tap_network_name` | TAP adapter connection name | **CONFIRMED** — matches the WMI `NetConnectionID` predicate |
| 5 | `default_dns` | Resolver used when no profile DNS is set | `INFERRED` |
| 6 | `dns_primary` / 7 `dns_secondary` | Upstream resolvers | `INFERRED` |
| 8 | `proxy_tethering` | Share the connection with other devices | **CONFIRMED** — consumed by `Utils.ProxyTethering` |
| 9 | `http_local_port` | Local HTTP proxy port | **CONFIRMED** — matches the local-proxy bind literal |
| 10 | `proxy_server_local_port` | Local SOCKS port handed to `tun2socks` | **CONFIRMED** — matches `--socks-server-addr 127.0.0.1:<port>` |
| 11 | `proxy_server_tls_version` | TLS version for the local proxy | `INFERRED` |
| 12 | `proxy_so_buff_send` / 13 `proxy_so_buff_rcv` | Socket buffer sizes | `INFERRED` |
| 14 | `ssh_local_port` | Local SSH-forward port | `INFERRED` |
| 15 | `ssh_udpgw_port` / 16 `ssh_udpgw_buff` | UDP gateway port and buffer | `INFERRED` |
| 17 | `ssh_reconnect_max` | Reconnect attempts | `INFERRED` |
| 18 | `ssh_compress` | SSH compression | `INFERRED` |
| 19 | `v2ray_mode` | V2Ray run mode | `INFERRED` |
| 20 | `v2ray_fakedns` | FakeDNS toggle | **CONFIRMED** — key present in the generated config |
| 21 | `v2ray_sniffing` | Traffic sniffing toggle | **CONFIRMED** — same |
| 22 | `v2ray_allow_insecure` / 23 `v2ray_allow_insecure_ciphers` | Insecure-cipher tolerance | **CONFIRMED** — same |
| 24 | `hc_sni_target_host` / 25 `hc_sni_target_port` | Hidden-connection SNI target | `INFERRED` |
| 26 | `hc_sni_host` | Hidden-connection SNI | `INFERRED` |
| 27 | `hc_proxy_host` / 28 `hc_proxy_port` | Hidden-connection proxy endpoint | `INFERRED` |
| 29 | `hc_url` / 30 `hc_method` / 31 `hc_proxy_cb` | Hidden-connection request shape | `INFERRED` |
| 32 | `theme` | UI theme | **CONFIRMED** — consumed by the presentation layer |

**`v2ray_allow_insecure` deserves attention.** It is a user-facing switch that
permits the upstream TLS peer to be unauthenticated. It is separate from the
local proxy's certificate-validation callback ([NF-01](../findings/confirmed/NF-01-tls-certificate-validation.md)),
which is unconditional. Both exist; neither is a substitute for the other, and
neither is documented in the product.

---

## 3. Licence settings — `Properties.ConfigSettings` (17 properties)

These are the settings a licence can impose. The naming pattern is unambiguous.

| # | Key | Inferred meaning |
|---:|---|---|
| 1 | `Default` | accessor |
| 2 | `expire_date` | Licence expiry |
| 3 | `payload` | Licence payload |
| 4 | `config_msg` | Licence status message shown in the UI |
| 5 | `sni` | Licence-pinned SNI |
| 6 | `sni_lock` | **Lock** the SNI setting against user change |
| 7 | `rr_mode` | "rr" mode selector |
| 8 | `rr_manual` | "rr" manual override |
| 9 | `rr_lock` | **Lock** the "rr" settings |
| 10 | `payload_lock` | **Lock** the payload setting |
| 11 | `prevent_wifi` | Disable Wi-Fi-dependent behaviour |
| 12 | `proxy_cb` | Proxy callback setting |
| 13 | `ssh_cb` | SSH callback setting |
| 14 | `ssl_cb` | SSL callback setting |
| 15 | `proxy_index` | Locked proxy-profile index |
| 16 | `ssh_index` | Locked SSH-profile index |
| 17 | `v2ray_index` | Locked V2Ray-profile index |

**Observation.** Four of these keys are `*_lock` names paired with a value key
(`sni`/`sni_lock`, `rr_mode`/`rr_lock`, `payload`/`payload_lock`). The pattern
indicates the licence can **pin configuration and prevent the user from
changing it**. `CONFIRMED` that the keys exist and follow this pattern;
`INFERRED` that they gate user editability. Whether the lock is enforced in the
UI, in the configuration layer, or both was **not determined** — it would require
recovering the WPF event handlers.

This is a licensing control, not a security control, and is not reported as a
finding. It is documented because a reader assessing the product should know that
a licence document can constrain configuration.

---

## 4. Default preferences — `Utils.DefaultPreferences` (34 properties)

The PascalCase projection of `Settings`, with resolved defaults for services.

`Instance`, `SshLocalPort`, `HttpLocalPort`, `SshUdpGwBuff`, `SshUdpGwPort`,
`ProxyTethering`, `DefaultDns`, `DnsPrimary`, `DnsSecondary`, `V2RayFakeDns`,
`V2RaySniffing`, `V2RayAllowInsecure`, `V2RayAllowInsecureCiphers`,
`SshReconnectMax`, `SshCompress`, `ProxyServerLocalPort`, `ProxyServerTlsVer`,
`ProxyBuffSend`, `ProxyBuffRcv`, `TapNetworkName`, `V2RayMode`,
`HcSniTargetHost`, `HcSniTargetPort`, `HcProxyHost`, `HcProxyPort`, `HcUrl`,
`HcMethod`, `HcSniHost`, `HcProxyCb`, `ConnectionMode`, `RemoveDefGw`,
`PropertyValueCollection`

Plus two properties with no user-facing counterpart:

| Property | Note |
|---|---|
| **`HWID`** | The machine identifier. Its presence is `CONFIRMED`; **its derivation was deliberately not reversed** — see [LIMITATIONS.md](LIMITATIONS.md#deliberate-non-goals). |
| **`Theme`** | UI theme |

---

## 5. Licence document

| Item | Evidence |
|---|---|
| Type | `Utils.MainJSON`, 20 methods, **35 fields** |
| Two filenames | `.nm` and `_unlocked.nm` in literals — one is the licence, one an unlocked variant |
| Cipher | `Utils.Encryption`, mode-parameterised; throws `'Unknown mode!'` for an unknown mode |
| Machine binding | `DefaultPreferences.HWID` |
| Expiry | `ConfigSettings.expire_date`, surfaced through `ConfigPreferences.ExpireDate` |
| UI | `NetModMaterial.UI.Dialog.DialogHwid` |

**What is not claimed:** the licence format, the cipher, and the HWID derivation
are all undocumented here by choice. Reproducing them would serve only to bypass
licensing, which is outside the scope of this research.

The presence of `_unlocked.nm` is recorded because it is a file the application
looks for; **it is not an instruction to create or use one**, and no such file
was produced or examined.

---

## 6. Profile database

| Item | Evidence |
|---|---|
| Mechanism | EF Core 3.1 + SQLite |
| Owner | `Utils.DbManager` (12 methods, 4 properties: `Proxy`, `V2ray`, `Ssh`, `Instance`) |
| Migrations | `20221005033905_initialcreate`, `20230416125047_AddedPosToProfile` |
| Migration history | `__EFMigrationsHistory` insert recovered |
| Column encryption | `EntityFrameworkCore.DataEncryption` — `AesProvider`, `[Encrypted]` attribute |
| `AddedPosToProfile` | The **column it adds was not determined from IL** |

The second migration's name says a position was added to the profile table.
Which column, and why, is `UNRESOLVED`.

---

## 7. Resource / localisation keys

`Properties.Resources` declares **386 properties**. The names are English UI
strings and setting labels, e.g. `add`, `cancel`, `apply_settings`,
`connected_server`, `fail_vpn`, `config_locked`, `allow_insecure_summary`.

Two of them are directly relevant to routing behaviour:

| Key | Reading |
|---|---|
| `apps_vpn_filter` | a UI string about filtering VPN traffic **per application** |
| `exclude_vpn_apps` | a UI string about **excluding** applications from the VPN |

Both are **resource/localisation strings**, not settings keys — they are
properties of `Resources`, not of `Settings`. Their presence establishes that
the product presents per-application split tunnelling in its UI. Whether the
enumeration of applications is persisted, and in what form, was **not
determined**. An earlier internal reading treated these identifiers as CIDR
route-bypass settings; that reading is **withdrawn** — see
[VALIDATION.md](VALIDATION.md#corrections-preserved).

---

## 8. State that is **not** persisted by the client

| State | Where it lives | Consequence |
|---|---|---|
| Saved default-gateway entry | In memory during a session | Lost on crash — [NF-03](../findings/confirmed/NF-03-default-gateway-restore.md) |
| Installed routes | In memory during a session | Same |
| Child-process job object | OS handle, process lifetime | Orphaned helpers possible after abnormal exit |

No registry key, service name or scheduled task belonging to the client was
recovered. The WiX Burn bundle's install-time registry layout was **not decoded**.

---

## 9. What is unknown here

| Question | Status |
|---|---|
| Exact paths and file names of the config, licence and database files | **UNRESOLVED** |
| Where the update payload is written | **UNRESOLVED** |
| Whether `*_lock` keys are enforced in the UI or only in the config layer | **UNRESOLVED** |
| What `AddedPosToProfile` adds | **UNRESOLVED** |
| HWID derivation | **DELIBERATELY NOT ATTEMPTED** |
| The cipher used by `Encryption` | **UNRESOLVED** (schema recovered, algorithm not) |
| Whether persisted settings are encrypted at rest | **UNRESOLVED** — no evidence of encryption was found for `Settings`; only the EF column encryption is proven |