# Network Behaviour

> Every claim here is derived from recovered code. **No socket was opened, no
> packet was sent, and no endpoint was contacted.** Where the code says "binds to
> `127.0.0.1`", that is a *literal in a decoded body* — not an observed socket.

---

## 1. Listeners

| Component | Bind address | Evidence | Status |
|---|---|---|---|
| `ProxyServer` (`0x0600067F`) | `127.0.0.1` | Literal `127.0.0.1` present in the listener body | `CONFIRMED` (literal) · `INFERRED` (actual bind) |
| `ProxyServer` port | user setting `http_local_port`, `proxy_server_local_port` | `Settings` properties | `CONFIRMED` |
| `ProxyTethering` | — | `_listener` field | `INFERRED` |
| V2Ray SOCKS inbound | loopback | `V2RayConfig` property `SocksIpEndPoint` | `INFERRED` |
| V2Ray HTTP inbound | loopback | `V2RayConfig` property `HttpIpEndPoint` | `INFERRED` |
| `dnstt-client` | loopback | Bundled binary, not analysed | `UNRESOLVED` |

### The loopback claim, precisely

The literal `127.0.0.1` appears in the listener method, and the argument template
for `tun2socks` reads `--socks-server-addr 127.0.0.1:<port>`. Together these make
a **loopback-only** listener strongly indicated.

However:

* no call to `IPAddress.Any` (`0.0.0.0`) was found in the same path — **absence of
  evidence, not proof**;
* `V2RayConfig` exposes `ListenHost` as a *property*, which means the listen
  address is **configurable**. Its default was not recovered.

**Therefore:** the default appears to be loopback, and the host is configurable.
Whether a user or a licence could set it to a routable address is `UNRESOLVED`.
This is the single most consequential open question in this document, because a
non-loopback proxy on a tunnelled host would expose the tunnel to the LAN.

---

## 2. Protocol handling

The client is a **front end**, not a tunnel implementation. It owns
configuration, process lifecycle and host network state; the transport is
delegated.

| Protocol | Implemented in | Launched with | Status |
|---|---|---|---|
| VMess / VLESS / Trojan / Shadowsocks | `v2ray.exe` (Go) | `-config "<file>"` or `-c "<file>" -u…` | `CONFIRMED` |
| ShadowsocksR | `ssr-local.exe` | — | `CONFIRMED` |
| SOCKS5 / HTTP | **in-process** (`ProxyServer`) | — | `CONFIRMED` |
| Userspace TUN | `badvpn-tun2socks.exe` | See §3 | `CONFIRMED` |
| DNS tunnelling | `dnstt-client.exe` | — | `CONFIRMED` (binary present) |
| SSH | `plink.exe` | — | `INFERRED` — settings present, argv unrecovered |

V2Ray transports configured by the generated config: TCP, mKCP, WebSocket,
HTTP/2, QUIC, gRPC — all with TLS — plus FakeDNS and traffic sniffing. All
`CONFIRMED` from the recovered key vocabulary.

> A `Trojan-Go` interface exists but is **configuration-generation only** and is
> marked deprecated in code. **No `trogo.exe` is bundled**, and no Trojan-Go
> runtime was found. `CONFIRMED ABSENT`.

---

## 3. The `tun2socks` argument template

Recovered from `VpnService` — this is the client's only userspace-TUN bridge:

```
--tundev "tap0901:<if>:<local>:<gateway>:<netmask>"
--netif-ipaddr   10.4.2.2
--netif-netmask  255.255.255.0
--socks-server-addr      127.0.0.1:<proxy_server_local_port>
--udpgw-connection-buffer-size  <proxy_so_buff_rcv>
--socks5-udp
--udpgw-remote-server-addr      127.0.0.1:<port>
-c <config file>
```

| Element | Class |
|---|---|
| Device `tap0901`, the standard TAP-Windows adapter | `CONFIRMED` |
| `10.4.2.0/24` on-device addressing | `CONFIRMED` |
| Local SOCKS endpoint on loopback | `CONFIRMED` |
| UDP gateway enabled (`--socks5-udp`) | `CONFIRMED` |

**Traffic path:** application → TAP adapter → `tun2socks` → local SOCKS proxy →
upstream tunnel → remote. The client's own TCP/TLS stack is used between the
local proxy and the upstream endpoint for the SOCKS/HTTP path; for V2Ray the Go
binary owns TLS end to end.

---

## 4. Adapter discovery

WMI, verbatim:

```sql
SELECT * FROM Win32_NetworkAdapter
WHERE Manufacturer = 'TAP-Windows Provider V9'
  AND NetConnectionID = '<tap_network_name>'
```

`tap_network_name` is a user setting. Route changes are watched by
`NetworkStatusReceiver` (23 methods, singleton) with:

```sql
SELECT * FROM Win32_IP4RouteTableEvent
```

**Consequence worth stating:** the client watches the routing table and reacts to
external changes. The handler body was decoded, but the reaction policy was not
fully characterised — `UNRESOLVED`.

---

## 5. Route and DNS mutation

| Operation | Component | Status |
|---|---|---|
| Add/remove routes | `NetworkManagement`, IP Helper P/Invokes | `CONFIRMED` |
| Set interface DNS | `NetworkManagement` | `CONFIRMED` |
| Remove the saved default gateway | `VpnService` `0x060006C0`, setting `remove_def_gw` | `CONFIRMED` |
| Restore the default gateway | `VpnService` `0x060006C3` | `CONFIRMED` — orderly path only |

Native APIs used: `CreateIpForwardEntry`, `DeleteIpForwardEntry`,
`GetIpForwardTable`, `GetBestRoute`, `GetBestInterfaceEx`.

**The asymmetry that produces [NF-03](../findings/confirmed/NF-03-default-gateway-restore.md):**
there is exactly **one** restoration call site, reached only from the orderly
disconnect path. There is no `ProcessExit` handler and no persisted record of what
was removed.

---

## 6. DNS

| Aspect | Evidence | Status |
|---|---|---|
| Resolvers configured | `dns_primary`, `dns_secondary`, `default_dns` | `CONFIRMED` |
| Set on the tunnel interface | `VpnService.DnsAddresses` property | `CONFIRMED` |
| FakeDNS (V2Ray) | `v2ray_fakedns`; `V2RayConfig.DnsAddress` | `CONFIRMED` |
| DNS tunnelling binary | `dnstt-client.exe` present | `CONFIRMED` |
| Local forwarder | `pdnsd.exe` present | `CONFIRMED` |
| Sniffing | `v2ray_sniffing` | `CONFIRMED` |

**Note.** `v2ray_sniffing` enables V2Ray's traffic sniffing, which inspects the
first packets of flows to infer protocol. It is a user setting and defaults to
whatever the compiled default is — the default value was **not** recovered.
Sniffing has privacy implications; readers should be aware it exists and is
configurable.

---

## 7. Geo-IP lookup

| Aspect | Evidence |
|---|---|
| Component | `IpInfoModel` (27 methods) caller `0x060001B1` |
| Purpose | Geo-IP lookup of the client's public address |
| Endpoint | Third-party geo-IP service |
| Credential | **API key embedded in cleartext** — [NF-05](../findings/confirmed/NF-05-embedded-api-key.md) |

Used for country/region display in the UI. The value of the key is **not**
published here.

---

## 8. Telemetry

| Aspect | Evidence | Status |
|---|---|---|
| SDK | `Microsoft.AppCenter`, `.Analytics`, `.Crashes` (all `0.0.0.0`) | `CONFIRMED` reference |
| Entry point | `AppCenter.Start` | `CONFIRMED` call site |
| Crash reporting | `Crashes.TrackError`, `ErrorAttachmentLog` | `CONFIRMED` reference |
| What is transmitted | — | **`UNRESOLVED`** — the SDK's own configuration was not recovered |

The SDK version is `0.0.0.0`, which is the assembly build stamp rather than a
package version; **the actual AppCenter version shipped is unknown**
([ASSEMBLY_REFERENCES.md §5](ASSEMBLY_REFERENCES.md#5-present-in-the-installer-but-not-referenced)).

> Because telemetry is transmitted to a third party, users should be able to
> determine what is sent. That was **not** determined here, and it is a genuine
> gap rather than a reassurance.

---

## 9. Tethering and downstream sharing

| Layer | Evidence |
|---|---|
| WinRT | `Windows.Foundation.UniversalApiContract` → `NetworkOperatorTetheringManager`, `ConnectionProfile`, `NetworkInformation` — **9 TypeRefs** |
| Wlan | 14 `wlanapi` imports, `WlanHostedNetworkManager` (23 methods) |
| ICS | `NETCONLib` — 9 COM wrapper types |
| Manager | `Utils.Wifi.TetheringManager`, 12 methods, properties `Ssid`, `Password`, `NetworkStatusChanged`, `State` |

**Whether tethering is started automatically as part of connecting is
`INFERRED`**, not confirmed: the call edge from `VpnService` was not established.
The capability is `CONFIRMED`; the trigger is not.

---

## 10. Connection lifecycle

The confirmed sequence, with the unknowns marked, is drawn in
[connection-flow.mmd](../architecture/connection-flow.mmd).

```
connect() → enumerate TAP adapter (WMI)
          → resolve config (DefaultPreferences / ConfigPreferences)
          → write helper config file
          → spawn helper (ProcessRunner)
          → add routes, set interface DNS
          → remove saved default gateway
disconnect() → kill helper → remove routes → restore default gateway
```

**What is inferred rather than proven:** the *relative ordering* of the last four
connect steps. Each operation is individually located in code, but their sequence
comes from control-flow reading, not observation.

---

## 11. Summary of what a network observer could infer

Offered as an honest capability statement, not a threat claim:

| Observable | Mechanism |
|---|---|
| That the client is running | TCP/UDP to the configured endpoint; DNS to the configured resolver |
| Which endpoint | Encrypted for V2Ray paths; cleartext for the SOCKS/HTTP upstream path **insofar as NF-01 removes TLS protection** |
| Traffic metadata | Destinations and sizes visible to the local network; contents depend on NF-01 |
| DNS queries | To `dns_primary`/`dns_secondary` unless FakeDNS is active |
| AppCenter telemetry | To Microsoft endpoints |

The dominating variable is [NF-01](../findings/confirmed/NF-01-tls-certificate-validation.md):
with certificate validation disabled, the confidentiality of the upstream path
rests on the tunnel protocol's own cryptography alone.

---

## 12. Open questions in this document

| Question | Status |
|---|---|
| Can `V2RayConfig.ListenHost` be set to a routable address? | **`UNRESOLVED` — highest priority** |
| Actual default value of `v2ray_sniffing` | `UNRESOLVED` |
| Whether tethering starts automatically on connect | `INFERRED` |
| What AppCenter transmits | `UNRESOLVED` |
| The reaction policy of the route-change watcher | `UNRESOLVED` |
| Exact `plink` invocation | `UNRESOLVED` |

None of these can be closed without the runtime analysis described in
[LIMITATIONS.md](LIMITATIONS.md).