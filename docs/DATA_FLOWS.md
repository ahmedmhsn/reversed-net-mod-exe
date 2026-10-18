# Data Flows

> These flows are reconstructed from recovered IL and metadata. **Nothing was
> executed and no data was observed moving.** Each flow states what is `CONFIRMED`
> structurally and what remains `UNRESOLVED`.

---

## 1. Flow map

```
┌──────────────────────────────────────────────────────────────────┐
│  USER INPUT                                                      │
│  settings · profile config · licence file                       │
└───────┬──────────────────────────────────────────────────────────┘
        │
        ▼
┌──────────────────────────────────────────────────────────────────┐
│  CONFIGURATION  (3 layers, see CONFIGURATION_AND_STATE.md)       │
│  DefaultPreferences ──┐                                         │
│  Settings (user) ─────┼──► ConfigPreferences ──► DefaultPreferences
│  ConfigSettings (lic) ┘                              │
└─────────────────────────────────────────────────────┼────────────┘
                                                      │ resolved
                            ┌─────────────────────────┼──────────┐
                            ▼                         ▼          ▼
                    ┌───────────────┐        ┌─────────────┐  ┌────────┐
                    │ tun2socks cfg │        │ v2ray JSON  │  │ plink  │
                    └───────┬───────┘        └──────┬──────┘  └───┬────┘
                            │                       │             │
                            └───────┬───────────────┴─────────────┘
                                    ▼
                        ┌───────────────────────┐
                        │  child process        │  ProcessRunner 0x06000254
                        │  (protocol transport) │  ← ONLY Process.Start site
                        └───────────┬───────────┘
                                    ▼
                        ┌───────────────────────┐
                        │  TAP adapter          │  10.4.2.0/24
                        │  host routes + DNS    │  ← host state mutated
                        └───────────────────────┘
```

---

## 2. DF-1 — User settings to runtime configuration

| | |
|---|---|
| **Source** | `Properties.Settings` (32 properties) |
| **Via** | `Utils.ConfigPreferences` (42 methods, 17 properties) → `Utils.DefaultPreferences` (67 methods, 34 properties) |
| **Sink** | Service constructors and config writers |
| **Format** | .NET `userSettings` XML written by `CustomSettingsProvider` |
| **Status** | Structure `CONFIRMED`; exact precedence rules `INFERRED` |

Three layers are established: compiled defaults, user settings, and
licence-imposed settings. `ConfigPreferences` exposes the licence keys in
PascalCase, which is what identifies it as the merge point.

**Licence precedence** — the `*_lock` keys (`sni_lock`, `rr_lock`, `payload_lock`)
indicate a licence can pin a value and prevent user change. That the keys exist is
`CONFIRMED`; that the lock is enforced is `INFERRED` and
**`UNRESOLVED`** ([LIMITATIONS.md](LIMITATIONS.md#3-unresolved-technical-items)).

**Not recovered:** the exact file paths, and whether the merge happens at
construction or per read.

---

## 3. DF-2 — Configuration to helper process

| | |
|---|---|
| **Source** | Resolved preferences |
| **Sink** | A configuration file on disk, then a child process |
| **Launcher** | `ProcessRunner 0x06000254` — the only `Process.Start` site in the binary |
| **Status** | `CONFIRMED` for `tun2socks` and `v2ray`; `INFERRED` for `plink` |

Two concrete outputs were recovered:

| Helper | Output | Evidence |
|---|---|---|
| `tun2socks` | argv template + config file | Argument literals in `VpnService` |
| `v2ray` | JSON config, `-config "<file>"` | 46-method `V2RayConfig`; complete key vocabulary |

`V2RayConfig` (46 methods, 21 fields, 14 properties: `Address`, `Port`,
`ListenPort`, `Protocol`, `TrojanCfg`, `SsrConfig`, `DnsAddress`,
`SocksIpEndPoint`, `HttpIpEndPoint`, …) builds the whole document. That the JSON
*values* are correct for a given V2Ray version was **not** verified — the core was
not analysed.

**`UNRESOLVED`:** the temp/config file path, and whether the file is deleted after
the helper starts.

---

## 4. DF-3 — Host network state

| | |
|---|---|
| **Source** | `VpnService`, `NetworkManagement` |
| **Sink** | Windows routing table, interface DNS |
| **Status** | `CONFIRMED` for the operations; `INFERRED` for their ordering |

```
        ┌──────────────┐
        │ discover TAP │  WMI query, Manufacturer + NetConnectionID
        └──────┬───────┘
               ▼
        ┌──────────────┐
        │ save default │  ← must precede removal
        │   gateway    │
        └──────┬───────┘
               ▼
        ┌──────────────┐
        │ add routes   │  CreateIpForwardEntry
        │ set if-DNS   │
        └──────┬───────┘
               ▼
        ┌──────────────┐
        │ remove saved│  ← NF-03: no persisted record
        │  def gateway │
        └──────────────┘

        disconnect() ──► reverse all of the above (0x060006C3)
```

**The asymmetry is the finding.** Removal is performed on the connect path;
restoration on exactly one path out of `disconnect`. There is no
`AppDomain.ProcessExit` handler and no state file, so an abnormal exit leaves the
host with no default route.

See [NF-03](../findings/confirmed/NF-03-default-gateway-restore.md).

---

## 5. DF-4 — Persisted profile data

| | |
|---|---|
| **Source** | User-supplied server profiles |
| **Sink** | SQLite via EF Core 3.1 |
| **Owner** | `Utils.DbManager` (12 methods; properties `Proxy`, `V2ray`, `Ssh`, `Instance`) |
| **Status** | `CONFIRMED` for the mechanism |

| Element | Evidence |
|---|---|
| Migrations | `20221005033905_initialcreate`, `20230416125047_AddedPosToProfile` |
| History table | `__EFMigrationsHistory` insert recovered |
| Column encryption | `EntityFrameworkCore.DataEncryption` — `AesProvider`, `[Encrypted]` |
| Encryption key source | **`UNRESOLVED`** — a key must exist somewhere; its location was not recovered |

That credentials or connection strings are encrypted at rest is `INFERRED` from
the presence of `[Encrypted]` support — the specific columns annotated were not
enumerated. **The key's storage is the open question**, and it determines whether
this protection is meaningful.

---

## 6. DF-5 — Licence document

| | |
|---|---|
| **Source** | A `.nm` file |
| **Via** | `Utils.MainJSON` (20 methods, **35 fields**) |
| **Sink** | `Properties.ConfigSettings` → `ConfigPreferences` |
| **Status** | Schema `CONFIRMED`; contents deliberately not reproduced |

```
 .nm file ──► MainJSON ──► Encryption (mode-parameterised)
                │
                ├──► expire_date ──► UI status / ConfigPreferences.ExpireDate
                ├──► sni, sni_lock ──► pinned SNI
                ├──► payload, payload_lock
                ├──► rr_mode, rr_manual, rr_lock
                └──► *_index (proxy / ssh / v2ray) ──► locked profile selection
```

A second filename, `_unlocked.nm`, is referenced. It is recorded as a file the
application looks for; **no such file was created, sought or examined**.

**HWID derivation is deliberately not covered** — see
[LIMITATIONS.md](LIMITATIONS.md#deliberate-non-goals).

---

## 7. DF-6 — Update

| | |
|---|---|
| **Source** | Update host (16 URL literals) |
| **Via** | `UpdateApi` → `HttpClient.SendAsync` |
| **Sink** | **Unknown** — see [NF-06](../findings/unresolved/NF-06-update-payload-launch.md) |
| **Status** | Acquisition `CONFIRMED`; integrity `CONFIRMED ABSENT`; execution `UNRESOLVED` |

```
 update host ──HTTPS──► HttpClient ──► payload in memory / temp file
                                              │
                                       NO SIGNATURE CHECK   ← NF-02
                                       NO HASH CHECK
                                              │
                                   ???  ──────────────  ← NF-06 UNRESOLVED
```

The link from payload to `ProcessRunner 0x06000254` was not found. Drawn
explicitly in [update-flow.mmd](../architecture/update-flow.mmd).

---

## 8. DF-7 — Single-instance handoff

| | |
|---|---|
| **Source** | Second process's command line |
| **Via** | Global atom table + `HWND_BROADCAST` |
| **Sink** | The first instance — **`UNRESOLVED`** |
| **Status** | Channel `CONFIRMED`; receiver `UNRESOLVED` |

```
 instance #2 ──GetCommandLineArgs──► GlobalAddAtom(content)
                                        │
                                        ▼
                          SendMessage(HWND_BROADCAST, id, len, atom)
                                        │
                                        ▼
                          instance #1  ── ??? no WndProc identified ── UNRESOLVED
                                        │
                                        └──► GlobalDeleteAtom
```

Any process in the session can read the atom — so the command line, which may
contain connection details, is exposed. See
[NF-04](../findings/confirmed/NF-04-ipc-broadcast-channel.md).

---

## 9. DF-8 — Telemetry

| | |
|---|---|
| **Source** | The client process |
| **Via** | `Microsoft.AppCenter` (`AppCenter.Start`) |
| **Sink** | Microsoft AppCenter endpoints |
| **Status** | SDK reference `CONFIRMED`; payload `UNRESOLVED` |

`Crashes.TrackError` and `ErrorAttachmentLog` are referenced, so exception data
is eligible for transmission. **What is actually sent was not determined**, and
the shipped AppCenter version is unknown (`0.0.0.0`).

---

## 10. DF-9 — Startup code loading

| | |
|---|---|
| **Source** | Embedded 47,936-byte blob |
| **Via** | FieldRVA → xorshift32 → LZMA → (stage 3 `UNRESOLVED`) |
| **Sink** | `Assembly.Load` + `AppDomain.AssemblyResolve` |
| **Status** | Stages 1–2 `CONFIRMED`; stage 3 `UNRESOLVED` |

```
 <Module>.cctor (0x06000001)
      │
      ├──► IL decryptor 0x0600000A ──► 2,238 bodies restored in place
      │
      └──► 0x06000007  xorshift32 + rolling 16-dword delta   [CONFIRMED]
                │
                ▼
          0x06000006  LZMA framing + decode                 [CONFIRMED]
                │
                ▼
          stage 3 ────────────────────────────  [UNRESOLVED]
                │
                ▼
          Assembly.Load  → executes in-process, same privileges
```

See [NF-07](../findings/unresolved/NF-07-embedded-assembly-gap.md).

---

## 11. Flow-level open questions

| Flow | Open question | Closable statically? |
|---|---|---|
| DF-1 | Are `*_lock` keys enforced in the UI? | Yes — needs BAML/event handlers |
| DF-2 | Config file path; is it deleted? | Yes |
| DF-3 | Exact ordering of the four connect steps | Partly — needs a full walk |
| DF-4 | Where is the column-encryption key stored? | Yes |
| DF-5 | *(deliberately not pursued)* | n/a |
| DF-6 | Is the payload launched? | Yes — walk `0x0600028A` |
| DF-7 | What does the receiver do? | Yes — locate the `WndProc` |
| DF-8 | What is transmitted? | Partly — SDK config |
| DF-9 | What is the embedded assembly? | Needs a memory dump |

The high-value action remains the memory dump: it closes DF-6 and DF-9 together.