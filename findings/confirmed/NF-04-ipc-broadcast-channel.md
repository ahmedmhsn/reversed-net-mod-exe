# NF-04 — Single-instance coordination uses an unauthenticated session-wide channel

| | |
|---|---|
| **Classification** | `CONFIRMED` (the channel) · `UNRESOLVED` (what the receiver does) |
| **Severity** | **Medium** |
| **Status** | Channel closed. Receiver behaviour is open — see below |
| **Component** | `NetModMaterial.App`, method `0x06000075` (`App..ctor`), `IL_0037`–`IL_0095` |
| **Confidence** | High for the channel; **none** for its effect |

---

## Finding

To enforce single-instance operation, the client uses a **named mutex** plus a
**global atom table** and a **broadcast window message**:

```
GlobalAddAtom(message content)
  -> SendMessage(HWND_BROADCAST = 0xFFFF, messageId, length, atomHandle)
  -> GlobalDeleteAtom(atomHandle)
```

`HWND_BROADCAST` delivers the message to **every top-level window in the same
desktop session**. The message identifier is read from a **static field**
(`ldsfd`), not from `RegisterWindowMessage`, so the identifier is fixed and
predictable rather than negotiated.

## Evidence

| Item | Value |
|---|---|
| Enclosing method | `0x06000075` (`App..ctor`), 157 IL bytes, fully decoded |
| Sequence | `IL_0037` – `IL_0095` |
| Broadcast handle | `0xFFFF` (`HWND_BROADCAST`), a literal in the body |
| Message ids | `ldsfd` loads from static fields — **not** `RegisterWindowMessage` |
| P/Invokes used | `GlobalAddAtom` `0x060002AD`, `GlobalDeleteAtom` `0x060002AE`, `SendMessage` `0x060002B0`, `GlobalGetAtomName` `0x060002AF`, `RegisterWindowMessage` `0x060002AC` |

### Why the channel is unauthenticated

| Property | Consequence |
|---|---|
| Atom table is **session-wide** | Any process running in the same logon session can add and read atoms |
| `HWND_BROADCAST` is **session-wide** | Any process in the session can receive the message |
| Message id is a **fixed static field value** | A third party can read the id rather than guess it |
| The atom carries **user-supplied data** | Command-line arguments are passed this way — see below |

The client's own `App..ctor` is a legitimate user of this mechanism: it reads
the command line, and if the mutex is already held it hands its argument to the
running instance through the atom table and broadcast, then exits.

## What is explicitly NOT claimed

> **That an attacker can hijack or control the client through this channel is
> not proven.**

What was **not** recovered:

* the `WndProc` override that receives the broadcast — likely in `MainWindow` or
  `MetroWindowCustom`, but no override was identified;
* `GlobalGetAtomName` (`0x060002AF`) has **no proven caller edge** in the call
  graph, so even the read side of the exchange is unconfirmed;
* what the receiver does with the atom's contents — parse, ignore, act.

Without the receiver, the impact cannot be characterised. Publishing "local
privilege escalation" or "argument injection" here would be speculation.

## Realistic risk, stated conservatively

The defensible statement is about **robustness and isolation**, not escalation:

* Any process in the user's session can (a) read the atom content the client
  publishes — which may contain command-line arguments, i.e. **server
  addresses, credentials or licence tokens if passed that way**; and
  (b) inject atoms and broadcasts that the client will process.
* The mechanism relies on a fixed, publicly readable identifier rather than a
  negotiated one, so it offers no confidentiality and no authenticity.

Whether injected messages change client state is `UNRESOLVED`.

## Validation status

| Check | Result |
|---|---|
| Sequence decoded | Yes — `IL_0037`–`IL_0095` |
| `HWND_BROADCAST` literal present | Yes — `0xFFFF` |
| Message ids are static fields | Yes — `ldsfd`, not `RegisterWindowMessage` |
| `GlobalGetAtomName` has a caller | **No caller edge found** |
| `WndProc` receiver located | **No** |
| Runtime confirmation | **None — not performed** |

## Recommended defensive action

**For users.** Do not pass secrets on the command line if you use this client;
the first instance publishes its argument through a session-readable atom.

**For the vendor.**

1. Prefer a named pipe or a per-user mutex for single-instance handoff; both give
   a private, authenticated channel scoped to the user.
2. If a broadcast must be used, validate the atom contents strictly and treat
   them as untrusted input regardless of origin.
3. Replace the fixed message id with `RegisterWindowMessage`, which at least
   prevents cross-application message confusion.

## References

* [NF-07 — undecoded embedded assembly](../unresolved/NF-07-embedded-assembly-gap.md)
* [trust-boundaries.mmd](../../architecture/trust-boundaries.mmd) — TB1
* [ARCHITECTURE.md §3.1](../../docs/ARCHITECTURE.md)