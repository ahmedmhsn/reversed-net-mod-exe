# NF-01 — TLS certificate validation is disabled unconditionally

| | |
|---|---|
| **Classification** | `CONFIRMED` |
| **Severity** | **Critical** |
| **Status** | Closed — proven at byte level from two independent sites |
| **Component** | `ProxyHandler` (`0x06000691`), `SslConnectionHandler` (`0x06000456`) |
| **Confidence** | High — the callback body is two bytes and is unambiguous |

---

## Finding

Both certificate-validation callbacks supplied to `SslStream` authentication in
this application reduce to a single instruction sequence that returns `true`:

```
ldc.i4.1
ret
```

The complete IL is the two bytes `17 2a`. This means **every server certificate
is accepted**, regardless of issuer, expiry, host name, or revocation state.

## Evidence

| Property | Value |
|---|---|
| Callback method token (site 1) | `0x06000457` |
| Callback method token (site 2) | `0x06000692` |
| RVA (both) | `0x00010665` |
| File offset | `0x0000EA65` |
| Body bytes | `17 2a` |
| SHA-256 of the body | `3e652f9ec0ddcdd64c52c9cba261a1f70bba3ffa41ff4296d6eba45e6514b378` |

**Both callbacks are the same method.** They are bound with `ldftn` at two
distinct call sites:

| Binding site | Instruction | Enclosing method |
|---|---|---|
| `IL_00A6` | `ldftn` → `0x06000457` | `0x06000456` (`SslConnectionHandler`) |
| `IL_00C2` | `ldftn` → `0x06000692` | `0x06000691` (`ProxyHandler`) |

This is significant: it shows the override is not an oversight in one code path
but a **deliberate, shared decision** — the developer factored the callback into
one method and used it in both places.

### The callbacks are only used for logging

In both enclosing methods, the code that consumes the callback comes *after* the
`AuthenticateAsClient` call:

| Method | `AuthenticateAsClient` | Validation | `get_Subject` / `get_Issuer` |
|---|---|---|---|
| `0x06000456` | `IL_00D3` | — | `IL_01C1`, `IL_01E2` |
| `0x06000691` | `IL_00C7` | — | `IL_01D7`, `IL_01F5` |

240+ bytes of post-handshake code reads `get_Subject` and `get_Issuer` from the
remote certificate and writes them somewhere — almost certainly the log.

**So the application logs the certificate it accepted without ever checking
that it should have.** That is the sharpest way to describe the defect: the
trust decision is made, then discarded.

## Scope — what this finding does and does not cover

**Covered:**
* the two examined certificate-validation callbacks, in full;
* the fact that both are bound in exactly two places;
* the absence of any compensating validation logic after the callback returns.

**Not covered / not claimed:**
* That an attacker performed a man-in-the-middle attack. **No attack was
  performed and none is demonstrated.**
* The behaviour of any TLS client inside the bundled third-party binaries
  (`v2ray.exe`, `ssr-local.exe`, `plink.exe`). Those are separate codebases and
  were not analysed.
* Whether upstream transport security is provided by some other layer. The
  user-facing `v2ray_allow_insecure` setting
  ([CONFIGURATION_AND_STATE.md §2](../../docs/CONFIGURATION_AND_STATE.md)) suggests
  the product *also* permits unauthenticated upstream TLS by configuration —
  which is a separate, independent weakness.

## Why the call path is established

The finding does not rely on the existence of a call graph edge alone:

1. `ldftn` at `IL_00A6`/`IL_00C2` places this exact method in the
   `RemoteCertificateValidationCallback` argument slot of
   `AuthenticateAsClient`. Both call sites were read as instructions, not inferred.
2. A trust-API sweep across **all 2,452 methods** found **zero** calls to
   `X509Chain`, `X509Certificate2.Verify`, `X509Store`, `SslPolicyErrors`, or any
   other validation primitive. There is no alternative check to compensate.
3. The update code path was checked separately: the intersection of the update
   methods and the TLS methods is **empty**, so this is not an update-specific
   behaviour.

## Validation status

| Check | Result |
|---|---|
| Body bytes located in the file | `17 2a` at offset `0x0000EA65` |
| Body decodes as valid CIL | Yes — `ldc.i4.1` (`0x17`), `ret` (`0x2A`) |
| Both tokens point at the same RVA | Yes |
| `ldftn` bindings located | Yes, two sites |
| Compensating validation anywhere in the binary | None found |
| Runtime confirmation | **None — not performed** |

## Recommended defensive action

**For users.** Treat every connection this client makes as being over an
unauthenticated channel. Do not use it for traffic whose confidentiality or
integrity matters until a fixed build is available. If you must, verify the
tunnel endpoint's certificate out of band.

**For the vendor.**

1. Remove the callbacks and let `SslStream` use its default policy, which
   performs full chain, name and revocation validation.
2. If a callback is genuinely needed, reject on `sslPolicyErrors != None` and
   log the reason — never return a constant.
3. Remove the now-unused `get_Subject`/`get_Issuer` logging, or move it behind
   the validation result.

## References

* [ARCHITECTURE.md §3.3](../../docs/ARCHITECTURE.md) — `ProxyServer` / `ProxyHandler`
* [TECHNOLOGY_STACK.md](../../docs/TECHNOLOGY_STACK.md)
* [SECURITY_ASSESSMENT.md](../../docs/SECURITY_ASSESSMENT.md)
* [trust-boundaries.mmd](../../architecture/trust-boundaries.mmd) — TB2