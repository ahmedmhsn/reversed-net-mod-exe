# NF-06 — Whether the update payload is launched: UNRESOLVED

| | |
|---|---|
| **Classification** | `UNRESOLVED` |
| **Severity** | — (not rated; impact could not be established) |
| **Status** | **Open** |
| **Component** | `<CheckForUpdate>d__11` (`0x0600028A`) → `ProcessRunner` (`0x06000254`) |
| **Confidence** | None either way |

---

## The question

`ProcessRunner 0x06000254` is the **only** site in the binary that calls
`Process.Start`. If the downloaded update payload is executed, it must go
through that method.

**No call-graph edge connects any update method to it.** That is the entire
finding.

## What was established

| Check | Result |
|---|---|
| `0x06000254` is the sole `Process.Start` site | `CONFIRMED` |
| It sets `FileName`, `Arguments`, `UseShellExecute`, `CreateNoWindow` | `CONFIRMED` |
| 2,452 methods searched for edges into `0x06000254` | No edge from any update method |
| Known legitimate callers | `VpnService` (tun2socks/v2ray), `V2RayService` (`-config`), proxy start |
| `0x0600028A` size | 1,313 IL bytes |
| `0x0600028A` fully decoded | **No** |

## Why this is genuinely open

The absence of an edge is **weak evidence** here, for a specific reason: the
update logic is an `async` method compiled into a compiler-generated state
machine. Calls inside such a method are `callvirt`/`call` instructions on
`<>4__this` or on locals, and the enclosing method's own body is only a
dispatcher.

Two independent gaps keep this open:

1. `0x0600028A` is only **partially decoded** — a portion of the 1,313 bytes is
   not yet reliable, and an edge could live there.
2. The receiver of any such call — typically `MoveNext` or an `await` continuation
   — has **not been walked instruction by instruction**.

A third possibility is that the payload is launched by the **installer**, not by
the client: the update flow may hand a file to the WiX Burn bundle, in which case
no in-process launch edge would exist at all, and the answer would be "no
vulnerability".

## What would settle it

| Approach | Feasibility |
|---|---|
| Walk `0x0600028A` instruction by instruction (1,313 bytes) | **Feasible now** — static |
| Walk the `MoveNext` continuation methods | **Feasible now** — static |
| Search the `#US` heap for a path written next to a downloaded file | **Feasible now** |
| Observe the update flow at runtime | Requires a disposable analysis VM — **not available** |

The static routes are the practical path. See
[LIMITATIONS.md](../../docs/LIMITATIONS.md#what-would-close-the-open-questions).

## Why nothing stronger is published

The tempting chain is:

> update is unverified → therefore an attacker can push code → therefore RCE.

Each arrow is unsupported:

* "unverified" is proven ([NF-02](../confirmed/NF-02-update-integrity.md));
* "an attacker can substitute the payload" additionally requires defeating TLS,
  which may be governed by NF-01 but is not established for the update host;
* "the payload is executed" is unproven;
* "the executed payload runs attacker code" is unproven.

Publishing the conclusion without the middle two steps would be the single most
misleading thing this repository could do.

## Related

* [NF-02 — update integrity](../confirmed/NF-02-update-integrity.md) — the part that
  *is* proven
* [update-flow.mmd](../../architecture/update-flow.mmd) — the gap is drawn explicitly
* [SECURITY_ASSESSMENT.md](../../docs/SECURITY_ASSESSMENT.md)