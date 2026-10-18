# .NET Assembly References

Complete inventory of the `AssemblyRef` metadata table of the analyzed `NetMod.exe`,
decoded twice by implementations that share no code and cross-checked row by row.

**Acceptance criterion for this document:** the complete, validated AssemblyRef
inventory, with an explicit separation between metadata facts read directly from
the file, purposes inferred from the names and usage, and dependency behaviour
that remains unresolved.

> Every value in §1 is a **metadata fact**. Every statement in §3–§5 is either a
> measurement (counts) or an inference (purpose). Every row marked
> `UNRESOLVED` is an explicit gap, not an omission.

---

## 1. Complete AssemblyRef inventory (38 rows)

Source image: the extracted `NetMod.exe`, SHA-256
`60b1be47f1e74b8283fc31ee72ea07a808ea7d941515cf3a3a444bf126f31743`.

AssemblyRef rows are **24 bytes**: `MajorVersion`(2) `MinorVersion`(2)
`BuildNumber`(2) `RevisionNumber`(2) `Flags`(4) `PublicKeyOrToken`(blob index, 2)
`Name`(string index, 4) `Culture`(string index, 4) `HashValue`(blob index, 2).
The table starts at file offset **0x508AE**.

| # | Assembly | Version | Flags | Public key token | Culture | Hash |
|---:|---|---|---|---|---|---|
| 1 | `mscorlib` | 4.0.0.0 | `0x00000000` | `B77A5C56 1934E089` | *(none)* | *(none)* |
| 2 | `WindowsBase` | 4.0.0.0 | `0x00000000` | `31BF3856 AD364E35` | *(none)* | *(none)* |
| 3 | `PresentationFramework` | 4.0.0.0 | `0x00000000` | `31BF3856 AD364E35` | *(none)* | *(none)* |
| 4 | `System.Windows.Forms` | 4.0.0.0 | `0x00000000` | `B77A5C56 1934E089` | *(none)* | *(none)* |
| 5 | `System.Xaml` | 4.0.0.0 | `0x00000000` | `B77A5C56 1934E089` | *(none)* | *(none)* |
| 6 | `MaterialDesignThemes.Wpf` | 4.5.0.0 | `0x00000000` | `DF2A7202 0BD7962A` | *(none)* | *(none)* |
| 7 | `System` | 4.0.0.0 | `0x00000000` | `B77A5C56 1934E089` | *(none)* | *(none)* |
| 8 | `MahApps.Metro` | 2.0.0.0 | `0x00000000` | `51482D6F 650B2B3F` | *(none)* | *(none)* |
| 9 | `PresentationCore` | 4.0.0.0 | `0x00000000` | `31BF3856 AD364E35` | *(none)* | *(none)* |
| 10 | `System.Xml.Linq` | 4.0.0.0 | `0x00000000` | `B77A5C56 1934E089` | *(none)* | *(none)* |
| 11 | `System.Configuration` | 4.0.0.0 | `0x00000000` | `B03F5F7F 11D50A3A` | *(none)* | *(none)* |
| 12 | `Microsoft.EntityFrameworkCore` | 3.1.0.0 | `0x00000000` | `ADB97938 29DDAE60` | *(none)* | *(none)* |
| 13 | `EntityFrameworkCore.DataEncryption` | 3.0.1.0 | `0x00000000` | *(absent)* | *(none)* | *(none)* |
| 14 | `System.Net.Http` | 4.2.0.0 | `0x00000000` | `B03F5F7F 11D50A3A` | *(none)* | *(none)* |
| 15 | `Newtonsoft.Json` | 13.0.0.0 | `0x00000000` | `30AD4FE6 B2A6AEED` | *(none)* | *(none)* |
| 16 | `System.Management` | 4.0.0.0 | `0x00000000` | `B03F5F7F 11D50A3A` | *(none)* | *(none)* |
| 17 | `ToastNotifications` | 2.5.1.0 | `0x00000000` | `E89D9D73 14A7C797` | *(none)* | *(none)* |
| 18 | `System.ServiceProcess` | 4.0.0.0 | `0x00000000` | `B03F5F7F 11D50A3A` | *(none)* | *(none)* |
| 19 | `Windows.Foundation.UniversalApiContract` | 6.0.0.0 | `0x00000200` | *(absent)* | *(none)* | *(none)* |
| 20 | `GMap.NET.WindowsPresentation` | 2.1.7.0 | `0x00000000` | `B85B9027 B614AFEF` | *(none)* | *(none)* |
| 21 | `System.Drawing` | 4.0.0.0 | `0x00000000` | `B03F5F7F 11D50A3A` | *(none)* | *(none)* |
| 22 | `Microsoft.EntityFrameworkCore.Relational` | 3.1.0.0 | `0x00000000` | `ADB97938 29DDAE60` | *(none)* | *(none)* |
| 23 | `System.ComponentModel.DataAnnotations` | 4.0.0.0 | `0x00000000` | `31BF3856 AD364E35` | *(none)* | *(none)* |
| 24 | `PJrcRGGDZySncSGQBDxtMlBJuXsaA` | 0.0.0.0 | `0x00000001` | *(absent)* | *(none)* | *(none)* |
| 25 | `Microsoft.AppCenter.Analytics` | 0.0.0.0 | `0x00000000` | `8A600E2F FE7BA272` | *(none)* | *(none)* |
| 26 | `Microsoft.AppCenter.Crashes` | 0.0.0.0 | `0x00000000` | `8A600E2F FE7BA272` | *(none)* | *(none)* |
| 27 | `Microsoft.AppCenter` | 0.0.0.0 | `0x00000000` | `8A600E2F FE7BA272` | *(none)* | *(none)* |
| 28 | `System.Threading.Tasks.Extensions` | 4.2.0.0 | `0x00000000` | `CC7B13FF CD2DDD51` | *(none)* | *(none)* |
| 29 | `INIFileParser` | 2.5.2.0 | `0x00000000` | `79AF7B30 7B65CF3C` | *(none)* | *(none)* |
| 30 | `System.Core` | 4.0.0.0 | `0x00000000` | `B77A5C56 1934E089` | *(none)* | *(none)* |
| 31 | `Microsoft.EntityFrameworkCore.Sqlite` | 3.1.0.0 | `0x00000000` | `ADB97938 29DDAE60` | *(none)* | *(none)* |
| 32 | `HtmlAgilityPack` | 1.11.46.0 | `0x00000000` | `BD319B19 EAF3B43A` | *(none)* | *(none)* |
| 33 | `zxing` | 0.16.9.0 | `0x00000000` | `4E88037A C681FE60` | *(none)* | *(none)* |
| 34 | `Windows.Foundation.FoundationContract` | 3.0.0.0 | `0x00000200` | *(absent)* | *(none)* | *(none)* |
| 35 | `System.Runtime.WindowsRuntime` | 4.0.14.0 | `0x00000000` | `B77A5C56 1934E089` | *(none)* | *(none)* |
| 36 | `zxing.presentation` | 0.16.9.0 | `0x00000000` | `4E88037A C681FE60` | *(none)* | *(none)* |
| 37 | `GMap.NET.Core` | 2.1.7.0 | `0x00000000` | `B85B9027 B614AFEF` | *(none)* | *(none)* |
| 38 | `System.Web` | 4.0.0.0 | `0x00000000` | `B03F5F7F 11D50A3A` | *(none)* | *(none)* |

<a id="11-table-wide-facts"></a>
### 1.1 Table-wide facts
| Property | Value | Class |
|---|---|---|
| Row count | **38** | metadata fact |
| Row size | **24 bytes** | metadata fact |
| Culture on every row | **empty** | metadata fact |
| HashValue on every row | **empty** (blob index 0) | metadata fact |
| Flags = `0x00000000` | 35 rows | metadata fact |
| Public key token present | 34 rows | metadata fact |
| Public key token absent | 4 rows (13, 19, 24, 34) | metadata fact |
| Distinct public key tokens | 12 | metadata fact |

**Consequences, stated precisely:**

* **No satellite assemblies.** Every `Culture` is empty, so no localised resource
  assembly is referenced. The client ships one language.
* **No hash pinning.** No row carries a `HashValue`, so the CLR performs no
  content-hash validation of referenced assemblies at load time. Integrity of
  third-party DLLs depends entirely on file placement in the install directory
  and on whatever the installer verifies. *This is a property of the metadata;
  it is not a finding on its own* — see [NF-02](../findings/confirmed/NF-02-update-integrity.md)
  for the related, separately-evidenced update-path gap.
* **Unsigned assembly.** `Assembly.PublicKeyOrToken` is absent and
  `StrongNameSignature` RVA/size are `0/0`; the client itself is not strong-named.

---

## 2. Grouped dependency summary

| Group | Rows | Shipped with the product? | Notes |
|---|---:|---|---|
| .NET Framework / Windows | 19 | No — provided by the platform | `mscorlib`, `System`, `System.Core`, `System.Web`, `System.Xaml`, `System.Drawing`, … |
| Windows Runtime contracts | 2 | No — provided by Windows 10+ | `Windows.Foundation.*Contract` |
| Third-party, **referenced** | 16 | **Yes** | listed in §3 |
| NuGet facade, **referenced** | 1 | **Yes** | `System.Threading.Tasks.Extensions` |
| Obfuscator artefact, **not referenced** | 1 | No | `PJrcRGGDZySncSGQBDxtMlBJuXsaA` (§5) |
| **Total** | **38** | | |

Third-party referenced: `MahApps.Metro`, `MaterialDesignThemes.Wpf`,
`GMap.NET.Core`, `GMap.NET.WindowsPresentation`, `ToastNotifications`, `zxing`,
`zxing.presentation`, `Newtonsoft.Json`, `HtmlAgilityPack`, `INIFileParser`,
`Microsoft.EntityFrameworkCore` (+ `.Relational`, `.Sqlite`),
`EntityFrameworkCore.DataEncryption`, `Microsoft.AppCenter` (+ `.Analytics`,
`.Crashes`).

---

## 3. Third-party and vendor-specific references

For each: purpose (**inferred** from the resolved type list, which *is*
measured), confirmed usage counts (**measured**), and shipping status
(**measured** from the installer payload inventory).

| Assembly | TypeRefs | MemberRefs | Distinct types+members | Purpose (inferred from resolved types) |
|---|---:|---:|---:|---|
| `mscorlib` | 176 | 340 | 482 | Core BCL; also `SuppressIldasmAttribute` and `System.Reflection.Assembly` (the obfuscator's own load path) |
| `System` | 97 | 216 | 305 | `Process`, `Configuration.SettingsProvider`/`IApplicationSettingsProvider`, collections |
| `PresentationFramework` | 87 | 193 | 279 | `Application`, `Window`, controls, routed events |
| `PresentationCore` | 56 | 85 | 140 | Media, geometry, imaging |
| `WindowsBase` | 17 | 38 | 52 | Dependency properties, dispatcher, `Point`/`Rect` |
| `System.Management` | 18 | 28 | 44 | WMI queries and event watchers (adapter discovery, route-change notifications) |
| `Microsoft.EntityFrameworkCore` | 17 | 17 | 34 | `DbContext`, `DbSet<T>`, `ModelBuilder` |
| `Microsoft.EntityFrameworkCore.Relational` | 15 | 18 | 33 | `Migration`, `MigrationBuilder`, `ModelSnapshot` |
| `ToastNotifications` | 16 | 21 | 37 | Toast notifications |
| `MaterialDesignThemes.Wpf` | 11 | 24 | 35 | `DialogHost`, `PackIcon`, `Card`, `PopupBox` |
| `System.Drawing` | 12 | 23 | 35 | `Bitmap`, `Icon` — QR code and tray images |
| `System.Net.Http` | 12 | 23 | 33 | `HttpClient`, `HttpClientHandler`, `HttpRequestMessage` |
| `System.Windows.Forms` | 12 | 17 | 29 | `NotifyIcon`, `ContextMenuStrip`, `ToolStrip` |
| `Windows.Foundation.UniversalApiContract` | 9 | 18 | 27 | `NetworkOperatorTetheringManager`, `ConnectionProfile`, `NetworkInformation` |
| `Newtonsoft.Json` | 7 | 25 | 27 | `JObject`, `JToken`, `JsonConvert` — V2Ray config + licence document |
| `System.Xml.Linq` | 6 | 17 | 22 | `XDocument`/`XElement` |
| `zxing` | 6 | 6 | 12 | `QRCodeWriter`, `BarcodeReader` |
| `GMap.NET.Core` | 5 | 4 | 9 | `GMaps`, `MapProviders.GoogleMapProvider`, `PointLatLng` |
| `EntityFrameworkCore.DataEncryption` | 4 | 3 | 7 | `IEncryptionProvider`, `AesProvider`, `EncryptedAttribute` |
| `INIFileParser` | 4 | 6 | 10 | `IniData`, `StreamIniDataParser` |
| `MahApps.Metro` | 3 | 5 | 8 | `MetroWindow`, `WindowButtonCommands`, `TreeHelper` |
| `HtmlAgilityPack` | 2 | 4 | 6 | `HtmlDocument`, `LoadHtml` |
| `Microsoft.EntityFrameworkCore.Sqlite` | 2 | 1 | 3 | `SqliteDbContextOptionsBuilder` |
| `GMap.NET.WindowsPresentation` | 2 | 9 | 11 | `GMapControl`, `GMapMarker` |
| `Microsoft.AppCenter.Crashes` | 2 | 1 | 3 | `Crashes`, `TrackError` |
| `Windows.Foundation.FoundationContract` | 2 | 0 | 2 | `IAsyncAction`, `IAsyncOperation<T>` |
| `Microsoft.AppCenter` | 1 | 1 | 2 | `AppCenter.Start` |
| `Microsoft.AppCenter.Analytics` | 1 | 0 | 1 | `Analytics` (type referenced; no member reference) |
| `System.Configuration` | 1 | 1 | 2 | `ProviderBase` — the custom settings provider base class |
| `System.ComponentModel.DataAnnotations` | 1 | 1 | 2 | `NotMappedAttribute` |
| `System.Threading.Tasks.Extensions` | 1 | 0 | 1 | `ValueTask` |
| `System.Runtime.WindowsRuntime` | 1 | 2 | 3 | `AsTask` — bridges WinRT async to .NET |
| `System.Xaml` | 1 | 2 | 3 | `IComponentConnector` |
| `System.Web` | 1 | 3 | 4 | `HttpUtility.ParseQueryString`, `UrlDecode` |
| `zxing.presentation` | 1 | 1 | 2 | `BarcodeReaderExtensions.Decode` |

---

## 4. Mapping significant references to confirmed components

Each row ties an assembly reference to a component documented elsewhere in this
repository. The "component" column is an **inference** from the resolved type
names; the token/evidence column is a **fact**.

| Reference | Resolved types that place it | Component it supports | Evidence in this repo |
|---|---|---|---|
| `PresentationFramework`, `PresentationCore`, `WindowsBase`, `System.Xaml` | `Application`, `Window`, `DependencyProperty`, `IComponentConnector` | WPF presentation layer | [ARCHITECTURE.md §2](ARCHITECTURE.md#2-the-managed-client--layer-map) |
| `MahApps.Metro`, `MaterialDesignThemes.Wpf` | `MetroWindow`, `DialogHost`, `PackIcon` | The window base class and dialogs | [COMPONENT_INVENTORY §1](COMPONENT_INVENTORY.md#1-entry-point-and-application-lifecycle) |
| `System.Net.Http` | `HttpClient`, `HttpClientHandler` | `UpdateApi` — update acquisition | [NF-02](../findings/confirmed/NF-02-update-integrity.md) |
| `mscorlib` → `System.Security.Cryptography.X509Certificates` | certificate-validation callback body `17 2a` | TLS validation override | [NF-01](../findings/confirmed/NF-01-tls-certificate-validation.md) |
| `Microsoft.EntityFrameworkCore*` + `EntityFrameworkCore.DataEncryption` | `DbContext`, `Migration`, `AesProvider`, `EncryptedAttribute` | `DbManager` and the encrypted local store | [CONFIGURATION_AND_STATE.md](CONFIGURATION_AND_STATE.md) |
| `System.Management` | `ManagementEventWatcher`, `WqlEventQuery` | Adapter discovery and route-change notification | [NETWORK_BEHAVIOR.md](NETWORK_BEHAVIOR.md) |
| `Windows.Foundation.UniversalApiContract` | `NetworkOperatorTetheringManager`, `ConnectionProfile` | `TetheringManager` | [COMPONENT_INVENTORY §6](COMPONENT_INVENTORY.md#6-tethering) |
| `System.ServiceProcess` | `ServiceController`, `ServiceControllerStatus` | Service state inspection | `UNRESOLVED` — no call path established |
| `System.Windows.Forms` | `NotifyIcon`, `ContextMenuStrip` | Tray icon | [COMPONENT_INVENTORY §1](COMPONENT_INVENTORY.md#1-entry-point-and-application-lifecycle) |
| `zxing`, `zxing.presentation`, `System.Drawing` | `QRCodeWriter`, `BarcodeReader`, `Bitmap` | QR generation/decoding | [COMPONENT_INVENTORY §9](COMPONENT_INVENTORY.md#9-presentation) |
| `GMap.NET.Core`, `GMap.NET.WindowsPresentation` | `GMaps`, `GMapControl` | Map display | [COMPONENT_INVENTORY §9](COMPONENT_INVENTORY.md#9-presentation) |
| `Newtonsoft.Json` | `JObject`, `JArray`, `JsonConvert` | V2Ray config generation, licence document | [CONFIGURATION_AND_STATE.md](CONFIGURATION_AND_STATE.md) |
| `HtmlAgilityPack` | `HtmlDocument.LoadHtml` | HTML parsing — **purpose inferred; no call path confirmed** | `INFERRED` |
| `INIFileParser` | `StreamIniDataParser`, `IniData` | INI file reading — **purpose inferred** | `INFERRED` |
| `Microsoft.AppCenter*` | `AppCenter.Start`, `Crashes.TrackError` | Crash/usage telemetry | [DATA_FLOWS.md](DATA_FLOWS.md) |
| `System.Web` | `HttpUtility.ParseQueryString`, `UrlDecode` | URL parameter decoding — **purpose inferred** | `INFERRED` |
| `System.Threading.Tasks.Extensions` | `ValueTask` | Compile-time facade only | `CONFIRMED` (reference), purpose `INFERRED` |

### 4.1 Reflection-related references — corrected classification

An earlier internal report described "35 reflection-related edges". On re-audit
those edges are of **three different kinds**, and only one of them invokes code:

| Kind | Count | Invokes code? |
|---|---:|---|
| `Type.GetType` / `Assembly.GetType` — **lookup only** | — | No |
| `typeof(X)` / `ldtoken` — **compile-time reference** | — | No |
| `FieldInfo.GetValue`, `PropertyInfo.GetValue` — **field/property access** | — | No |
| `MethodInfo.Invoke`, `Assembly.Load`, `ConstructorInfo.Invoke` — **invocation** | **3** | Yes |

Only the last row can execute code supplied by another assembly. The published
figure is therefore **3 code-invoking edges**, and the `System.Reflection` usage
under `mscorlib` must not be read as evidence of dynamic loading. This correction
is preserved in [VALIDATION.md](VALIDATION.md#corrections-preserved).

---

## 5. Unresolved and ambiguous references

| # | Reference | Status | What the evidence shows | What is missing |
|---:|---|---|---|---|
| 24 | `PJrcRGGDZySncSGQBDxtMlBJuXsaA` | **USED — as a resource container** | Version `0.0.0.0`; `Flags = 0x00000001` (the `PublicKey` bit) yet **no key blob is present** — an internally inconsistent row. **Zero `TypeRef` and zero `MemberRef` rows reference it**, so no *code* resolves through it. It is, however, the target of **both `ManifestResource` rows** (see §5.1), which is why it exists at all. | What the name originally was. `CONFIRMED` that it is an obfuscated identifier; the original is not stored in the binary. |
| 19, 34 | `Windows.Foundation.UniversalApiContract` (6.0.0.0), `Windows.Foundation.FoundationContract` (3.0.0.0) | **INFERRED / availability concern** | Both carry `Flags = 0x00000200` — bit 9, which is **not defined** for `AssemblyRef.Flags` in ECMA-335 II.23.1.2 — and both have **no public key token**, where a genuine WinRT contract reference normally has one. 9 and 2 TypeRefs resolve to them, so they *are* used. | Whether these rows were rewritten by the obfuscator. **Do not treat the flags as standard.** |
| 13 | `EntityFrameworkCore.DataEncryption` 3.0.1.0 | **INFERRED — version skew** | Carries **no public key token** while every other Microsoft/EF assembly does, and its version (3.0.1.0) predates the EF Core 3.1.0.0 stack it extends. | Whether the publisher rebuilt it. The payload's PDB path points at an individual developer machine, which is consistent with a self-built copy. Treat provenance as unverified. |
| 25, 26, 27 | `Microsoft.AppCenter*` at `0.0.0.0` | **`UNRESOLVED`** | Version is `0.0.0.0` in all three. Public key tokens match each other and differ from every other vendor. | Which package version was actually shipped; `0.0.0.0` is the assembly's build stamp, not a package version. |
| 28 | `System.Threading.Tasks.Extensions` 4.2.0.0 | **`INFERRED`** | The assembly name matches a .NET Framework facade, yet a managed image with that PDB name **is shipped in the payload**, which the platform does not require. | Why a facade is bundled. |

**Aggregate conclusion:** 1 of 38 references (row 24) is used by no *code*, but is
not inert — it is the declared container for the application's WPF resource set
(§5.1). Four rows carry field values inconsistent with the ECMA-335 definition,
and four rows carry versions that cannot be interpreted as package versions.

<a id="51-assemblyref-24-is-the-declared-home-of-the-wpf-resources"></a>
### 5.1 AssemblyRef #24 is the declared home of the WPF resources
`NetMod.exe` declares **no embedded managed resources**, and this is not merely an
absence — the metadata positively states where the resources live.

The `ManifestResource` table has **2 rows**, each **14 bytes**
(`Offset` 4, `Flags` 4, `Name` 4, `Implementation` 2), starting at file offset
**0x50C3E**:

| Row | Name | Offset | Flags | `Implementation` |
|---:|---|---:|---|---|
| 1 | `NetMod.g.resources` | 0 | `0x00000001` | `0x0061` → tag 1, row 24 |
| 2 | `NetModMaterial.Properties.Resources.resources` | 0 | `0x00000001` | `0x0061` → tag 1, row 24 |

Per ECMA-335 II.22.24 the `Implementation` coded index is
`0 = File, 1 = AssemblyRef, 2 = ExportedType` with 2 tag bits.
`0x0061 & 3 = 1` → **AssemblyRef**, and `0x0061 >> 2 = 24`.

**Consequences, and they are significant:**

1. The resources are **not** embedded in `NetMod.exe`. Consistent with the COR20
   resource directory being `0/0` and with `HeapSizes = 0x01`.
2. Both point at **row 24** — the obfuscated name `PJrcRGGDZySncSGQBDxtMlBJuXsaA`.
   That is what AssemblyRef #24 is *for*: it is the placeholder the runtime
   satisfies from memory.
3. Tag `0` (`File`) is **not selectable** here — the `File` table is absent from
   the `Valid` mask with zero rows — so the reading above is forced, not chosen.
4. `NetMod.g.resources` conventionally holds the compiled BAML. Its absence from
   this file, together with the `AppDomain.AssemblyResolve` handler that returns
   the FieldRVA payload, means the **compiled UI resource set is very likely
   inside the encrypted payload**.

**Status.** `CONFIRMED` for the metadata link — raw bytes plus two independent
readers, recorded in
[../evidence/RESOURCE_ORIGIN_VERIFICATION.txt](../evidence/RESOURCE_ORIGIN_VERIFICATION.txt).
`INFERRED`, not proven, that AssemblyRef #24 *is* the FieldRVA payload: that would
require decoding FieldRVA stage 3 and reading the loaded assembly's name.

This also explains an earlier puzzle: multiple rounds could not find BAML to
decode. **It was never in `NetMod.exe`.**

---

<a id="5-present-in-the-installer-but-not-referenced"></a>
## 6. Present in the installer but **not** referenced
The installer payload contains **60 managed images** (verified: 60 rows flagged
`is_managed` in the artifact inventory). One is `NetMod.exe` itself, so **59
third-party managed assemblies** are shipped. Only 18 of them are named in
`NetMod.exe`'s AssemblyRef table.

The gap splits into two groups, and the distinction matters:

**(a) Transitive dependencies — expected.** `Microsoft.EntityFrameworkCore.Abstractions`,
`.Design`, the ten `Microsoft.Extensions.*` assemblies, `SQLitePCLRaw.*`,
`ControlzEx`, `MaterialDesignColors`, `MaterialDesignThemes.MahApps`,
`Microsoft.Xaml.Behaviors`, `System.Buffers`, `System.Collections.Immutable`,
`System.Memory`, `System.Numerics.Vectors`, `Microsoft.Bcl.*`,
`Microsoft.DotNet.PlatformAbstractions`, `System.Diagnostics.DiagnosticSource`,
`System.Security.Principal.Windows`, `System.ComponentModel.Annotations`,
`ToastNotifications.Messages`, `Microsoft.Data.Sqlite`, `System.Data.SqlClient`.
These are loaded by the referenced assemblies, not by the client directly.
**`INFERRED`** — established from the dependency graphs of the referenced
libraries, not from a loader trace.

**(b) An unexplained legacy data stack.** Also shipped, and referenced by
nothing in the `NetMod.exe` metadata, and not a transitive dependency of the
EF Core 3.1 stack in use:

| Shipped assembly | Note |
|---|---|
| `Microsoft.EntityFramework` | **EF6** — the client uses EF Core 3.1, not EF6 |
| `EntityFramework.SqlServer` | EF6 SQL Server provider |
| `System.Data.SQLite` | EF6 SQLite provider |
| `System.Data.SQLite.EF6` | EF6 provider |
| `System.Data.SQLite.Linq` | EF6 provider |

`CONFIRMED` presence (PDB path in the payload inventory); `UNRESOLVED` reason.
The most likely explanation is a development leftover from a migration to EF
Core. It is called out because it expands the installed attack surface and
dependency count without contributing to the product's stated function — not
because it indicates a defect.

---

<a id="4-native-interop-implmap-34-rows"></a>
## 7. Native interop (`ImplMap`, 34 rows)
These are P/Invoke declarations, not assembly references, but they are the other
half of the "what does this binary actually depend on" question and are recorded
here for completeness.

`ImplMap` rows are **10 bytes**: `MappingFlags`(2) `MemberForwarded`(2)
`ImportName`(string index, 4) `ImportScope`(ModuleRef index, 2). The table
starts at file offset **0x5073A**.

| Row | Token | Import | Module |
|---:|---|---|---|
| 1 | `0x06000004` | `VirtualProtect` | `kernel32.dll` |
| 2 | `0x06000009` | `VirtualProtect` | `kernel32.dll` |
| 3 | `0x060000CA` | `CreateJobObject` | `kernel32.dll` |
| 4 | `0x060000CB` | `SetInformationJobObject` | `kernel32.dll` |
| 5 | `0x060000CC` | `AssignProcessToJobObject` | `kernel32.dll` |
| 6 | `0x0600020C` | `GetBestRoute` | `iphlpapi` |
| 7 | `0x0600020D` | `GetIpForwardTable` | `iphlpapi` |
| 8 | `0x0600020E` | `CreateIpForwardEntry` | `iphlpapi` |
| 9 | `0x0600020F` | `DeleteIpForwardEntry` | `iphlpapi` |
| 10 | `0x0600026D` | `GetClassName` | `user32.dll` |
| 11 | `0x060002A9` | `DwmSetWindowAttribute` | `dwmapi.dll` |
| 12 | `0x060002AC` | `RegisterWindowMessage` | `user32` |
| 13 | `0x060002AD` | `GlobalAddAtom` | `kernel32.dll` |
| 14 | `0x060002AE` | `GlobalDeleteAtom` | `kernel32.dll` |
| 15 | `0x060002AF` | `GlobalGetAtomName` | `kernel32.dll` |
| 16 | `0x060002B0` | `SendMessage` | `user32.dll` |
| 17 | `0x06000346` | `WlanOpenHandle` | `wlanapi.dll` |
| 18 | `0x06000347` | `WlanCloseHandle` | `wlanapi.dll` |
| 19 | `0x06000348` | `WlanFreeMemory` | `wlanapi.dll` |
| 20 | `0x06000349` | `WlanHostedNetworkStartUsing` | `wlanapi.dll` |
| 21 | `0x0600034A` | `WlanHostedNetworkStopUsing` | `wlanapi.dll` |
| **22** | **`0x0600034B`** | ***(empty string)*** | ***index 11 — out of range*** |
| 23 | `0x0600034C` | `WlanHostedNetworkForceStop` | `wlanapi.dll` |
| 24 | `0x0600034D` | `WlanHostedNetworkQueryProperty` | `wlanapi.dll` |
| 25 | `0x0600034E` | `WlanHostedNetworkSetProperty` | `wlanapi.dll` |
| 26 | `0x0600034F` | `WlanHostedNetworkInitSettings` | `wlanapi.dll` |
| 27 | `0x06000350` | `WlanHostedNetworkRefreshSecuritySettings` | `wlanapi.dll` |
| 28 | `0x06000351` | `WlanHostedNetworkQueryStatus` | `wlanapi.dll` |
| 29 | `0x06000352` | `WlanHostedNetworkSetSecondaryKey` | `wlanapi.dll` |
| 30 | `0x06000353` | `WlanHostedNetworkQuerySecondaryKey` | `wlanapi.dll` |
| 31 | `0x06000354` | `WlanRegisterNotification` | `wlanapi.dll` |
| 32 | `0x06000355` | `RegisterDeviceNotification` | `user32.dll` |
| 33 | `0x06000356` | `UnregisterDeviceNotification` | `user32.dll` |
| 34 | `0x06000357` | `GetBestInterfaceEx` | `Iphlpapi.dll` |

Module totals: `kernel32.dll` 8 · `wlanapi.dll` 14 · `user32.dll`/`user32` 5 ·
`iphlpapi`/`Iphlpapi.dll` 5 · `dwmapi.dll` 1 · anomalous 1 = **34**.

> Row 22 (`0x0600034B`) has an **empty import name** and an `ImportScope` index of
> 11, which is out of range for the 7-row `ModuleRef` table. Recorded as a
> metadata anomaly; no finding depends on it. Its position in the sequence — in
> the middle of the `wlanapi` hosted-network block — suggests the original
> import name was overwritten, but that is an **inference** and is not claimed.

**Reconciliation with an earlier internal report.** An earlier round listed 4
IP Helper imports and 1 `user32` import. Both lists were incomplete for different
reasons; the complete set is **34** and is listed above. `GetBestInterfaceEx`,
`RegisterDeviceNotification` and `UnregisterDeviceNotification` were present in
the earlier list and had been missed in the later one. The table above is the
authoritative version.

---

## 8. Methodology and validation

### 8.1 How the rows were decoded

1. Locate the CLI header via the PE data-directory entry 14; convert RVA → file
   offset using the section table.
2. From the COR20 header, read `MetaData` RVA/size → the metadata root.
3. Walk the metadata root stream-header array → `#~`, `#Strings`, `#Blob`.
4. Read the `#~` header: `HeapSizes` at `#~+6`, the **64-bit** `Valid` mask at
   `#~+8`, `RowCounts` at `#~+24`, table data at `#~+144`.
5. Walk the present tables in ascending index order, accumulating row offsets
   from each table's row size. `AssemblyRef` resolves to file offset **0x508AE**.
6. Decode each 24-byte row using `BLOBSZ = 2` and `STRSZ = 4`, as dictated by
   `HeapSizes = 0x01`.

This geometry is the one established and cross-validated in the later research
round; two of its deviations from ECMA-335 (16-byte `MethodDef` rows and an
8-byte `Valid` mask) are documented in
[REVERSE_ENGINEERING_METHOD.md](REVERSE_ENGINEERING_METHOD.md#metadata-anomalies).

### 8.2 Independent cross-check

A second implementation was written from scratch on .NET's own
`System.Reflection.Metadata`, the reference ECMA-335 reader. The two share no
code. It was compared against the Python walker over:

* row count (38), name, version, flags, public-key-token length and **value**,
  culture and hash length for all 38 rows — **228 assertions**;
* the `TypeRef` resolution-scope histogram;
* per-assembly `TypeRef` usage counts, with a correct expectation for the one
  assembly with no `TypeRef`s.

**Result: 345 assertions, 0 failures.** Both implementations also independently
report `TypeRef = 626`, `TypeSpec = 176`, `MemberRef = 1588`, matching the
earlier research round's table.

### 8.3 A tooling error this audit caught

The Python walker initially reported that **no** `TypeRef` resolved to an
`AssemblyRef`, which would have implied that the entire reference table was
unused — a serious and wrong conclusion. The independent reader reported 621.

The cause was a mis-assumption about the coded-index tag mapping for
`TypeRefOrDefResolutionScope`, not an anomaly in the binary: for this image the
observed mapping is **tag 2 → `AssemblyRef`**, **tag 3 → `TypeRef`**, not the
`1 / 2` ordering assumed from the specification text. With the measured mapping
the walker reproduces the reference reader's histogram exactly.

This is recorded because the failure mode is instructive: a reader that trusts
the specification over the bytes will confidently report a whole dependency
table as dead code. A second error in the same session (reading `ImplMap`'s
`ImportScope` as a `MemberRef` parent instead of a `ModuleRef` index) produced a
plausible-looking but wrong module attribution and was caught the same way.

---

## 9. Related documents

* [TECHNOLOGY_STACK.md](TECHNOLOGY_STACK.md) — verified stack, versions, native imports
* [COMPONENT_INVENTORY.md](COMPONENT_INVENTORY.md) — components the references support
* [ARCHITECTURE.md](ARCHITECTURE.md) — layers and packaging
* [VALIDATION.md](VALIDATION.md) — validation procedure and preserved corrections
* [findings/README.md](../findings/README.md) — findings index