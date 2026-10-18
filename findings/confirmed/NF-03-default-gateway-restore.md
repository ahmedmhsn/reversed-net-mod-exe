# NF-03 — Default-gateway restoration is implemented, but only on the orderly path

| | |
|---|---|
| **Classification** | `CONFIRMED` (design) · `INFERRED` (crash effect) |
| **Severity** | **Medium** |
| **Status** | Closed |
| **Component** | `Service.VpnService` (`0x060006C0`, `0x060006C3`), `Utils.NetworkManagement` |
| **Confidence** | High that restoration exists and is single-sited; the crash behaviour was never observed |

---

## Finding

While a tunnel is active, the client **removes the host's default-gateway route**
(user setting `remove_def_gw`, defaulting on). It correctly saves the removed
entry and restores it on the orderly disconnect path.

What is missing is any mechanism that restores host state when the client exits
**without** reaching that code — a crash, a kill, a power loss, or an early
return or exception inside the connect sequence.

## Evidence

### Restoration is implemented — this is not a careless omission

| Observation | Evidence |
|---|---|
| The removed gateway is saved | `NetworkManagement` holds the saved entry among its 24 fields |
| Routes added by the client are tracked | Route list retained for removal |
| Restoration exists | `0x060006C3` — disconnect: kill helper, remove routes, **restore gateway** |
| An aborted path is handled | `0x060006C5` — 64-byte cleanup method |

The application **knows** it must undo this and **does** so on the paths it
controls.

### The gap is what happens outside those paths

| Gap | Classification |
|---|---|
| No `AppDomain.ProcessExit` / `Console.CancelKeyPress` handler restoring routes | `CONFIRMED` — no such handler exists in 305 types |
| Restoration lives in **one** method, reached from one call site | `CONFIRMED` — `0x060006C3` |
| No on-disk record of the removed gateway | `CONFIRMED` — no such file reference was found |
| Crash leaves the host without a default gateway | `INFERRED` — **not observed** |

The single-siting is the practical risk: one early `return`, or one unhandled
exception anywhere in the connect path, skips restoration entirely — and no other
code will pick it up.

## Consequence

If the client is terminated abnormally while tunnelling, the host may be left
with **no default gateway**. Typical user-visible effects:

* no outbound internet until a default route is restored manually;
* DNS resolution that depends on the upstream resolver failing;
* on a machine with a single uplink, apparent loss of connectivity.

Recovery requires manual intervention, e.g. re-adding the default route in an
elevated console or via the Network Connections UI.

**This consequence is `INFERRED`.** It follows from the design, but no dynamic
test was performed, so the actual post-crash state was never observed.

## Why this is Medium and not High

* The restoration logic **is** present and correct for the normal path.
* The failure requires abnormal termination, not ordinary use.
* The impact is availability and local connectivity, not confidentiality,
  integrity, or remote code execution.
* No remote input triggers it.

## Validation status

| Check | Result |
|---|---|
| `remove_def_gw` setting present | Yes — `Properties.Settings` |
| Gateway removal located | Yes — `0x060006C0` |
| Restoration located | Yes — `0x060006C3` |
| Process-exit handler present | **No** |
| Restoration recorded to disk | **No** |
| Restoration call sites | Exactly one |
| Runtime crash test | **Not performed** |

## Recommended defensive action

**For users.** If the client is killed or crashes while connected, check your
default route before assuming the network is broken.

**For the vendor.**

1. Persist the removed gateway entry, its interface index and its metric to a
   small file *before* removal, and delete it only after successful restoration.
2. On start-up, detect a stale record and offer to restore.
3. Register an `AppDomain.ProcessExit` handler and wrap the connect path in
   `try`/`finally` so the restoration method is always reached.

## References

* [connection-flow.mmd](../../architecture/connection-flow.mmd)
* [trust-boundaries.mmd](../../architecture/trust-boundaries.mmd) — TB5
* [CONFIGURATION_AND_STATE.md §2](../../docs/CONFIGURATION_AND_STATE.md)
* [NETWORK_BEHAVIOR.md](../../docs/NETWORK_BEHAVIOR.md)