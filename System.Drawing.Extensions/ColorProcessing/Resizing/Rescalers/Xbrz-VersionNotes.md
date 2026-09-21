# xBRZ version differences — derived specification

Material derived by reading Zenju's xBRZ 1.6–1.9 releases, per rung 2 of the re-use ladder in
`AGENTS.md`: xBRZ is GPL-3.0 and this library ships LGPL-3.0-or-later, so the source may be read to
build a specification but not copied or transliterated. Nothing here is Zenju's code; it is a
description of behaviour, written so an implementation can be produced from it and checked.

Sources: <https://sourceforge.net/projects/xbrz/files/xBRZ/> (release archives and `Changelog.txt`).

## What each release changed

| Release | Changelog | Changes output? |
|---|---|---|
| 1.5 (2017-08-07) | added RGB conversion routines | no — API only |
| 1.6 (2018-02-27) | bilinear scaling, option to skip colour buffer, licence | no — API and performance only |
| 1.7 (2019-07-04) | fixed asymmetric colour distance; new parameter "Center direction bias" | **yes**, see below |
| 1.8 (2019-11-28) | consider ARGB outside area as transparent; fixed ARGB border issue | **yes**, but see below |
| 1.9 (2026-01-24) | reuse input kernels; round fractional colour values; adjusted steep direction threshold | **yes**, see below |

## 1.7 — already matched by this implementation

Two separate things are bundled under 1.7, and neither is a gap here.

**"Center direction bias" is not a behaviour change.** The corner pre-pass weights the distance
across the centre pair four times as heavily as the surrounding pairs. Before 1.7 that factor was a
hard-coded `4`; 1.7 made it a configuration field whose default is also `4`. Output at default
settings is identical. `Xbrz.cs` uses a constant `4` for the same term, so it already agrees with
1.7's default. Making it configurable would be a feature, not a version difference.

**The asymmetric colour distance never applied here.** Before 1.7, colour distance was looked up in
a table indexed by packing per-channel differences into a byte each, via `(diff + 0xFF) / 2`. That
packing is not symmetric about zero — Zenju's own note gives `+46` unpacking as `45` while `-46`
unpacks as `-47` — so `dist(a, b)` could differ from `dist(b, a)`. 1.7 replaced the packing so it
rounds symmetrically. This implementation never used that table: it calls a colour metric directly,
so distance has always been symmetric.

**Conclusion: the existing `xBRZ` entry is 1.7-equivalent.** It does not need a version suffix.

## 1.8 — covered by the out-of-bounds modes

1.8 treats the area outside the image as transparent when scaling ARGB, and fixes the resulting
border handling. This library already exposes that decision as `OutOfBoundsMode`, which the caller
chooses per axis — constant extension, mirroring, wrap-around or a flat colour. Transparent-outside
is the flat-colour mode with a transparent canvas, so 1.8's behaviour is reachable already and is
configurable rather than fixed. A separate 1.8 variant would duplicate an existing knob.

## 1.9 — the genuine difference

Two behavioural changes, both of which this implementation currently lacks.

### Steep direction threshold: 2.2 → 2.4

The blend step classifies a line as shallow or steep by comparing two accumulated distances against
this threshold. `Xbrz.cs` encodes the 2.2 comparison as the integer ratio `x * 22 <= y * 10`, so the
1.9 form is `x * 24 <= y * 10`. It appears twice per scale kernel, for the shallow and the steep
test, across all five scales.

Raising the threshold makes the test harder to satisfy, so fewer near-diagonal runs are treated as
steep or shallow lines. Visible on gentle diagonals and on curved outlines.

### Fractional colour values are rounded rather than truncated

Blending mixes two colours with integer weights `M` and `N`, per channel:

    result = (front * M + back * (N - M)) / N

Up to 1.8 that division truncated. 1.9 rounds to nearest instead. For an eight-bit channel this
shifts individual channel values by at most one, but it applies to every blended pixel, so gradients
and anti-aliased edges come out marginally lighter and less biased downwards.

`ColorLerpInt` currently truncates — `(a.C1 * w1 + b.C1 * w2) / total` — so this library matches
pre-1.9 behaviour and a 1.9 variant needs a rounding blend.

## What was implemented

1. A variant is carried as a struct type parameter `TVariant` on each per-scale kernel, so both
   knobs fold to constants when the kernel is specialised and the pre-1.9 path takes no branch.
   `Xbrz` selects it through an internal constructor; its public constructor is unchanged.
2. The 1.9 variant uses the `24` numerator in both steep/shallow comparisons, at all ten sites.
3. The 1.9 variant rounds both the weighted and the 50/50 blend. See the decision below.
4. `Xbrz19` is registered as `[ScalerInfo("xBRZ 1.9")]`, its own entry alongside `XBR` /
   `XBR NoBlend` / `XBR 3x Original`. The existing `xBRZ` entry keeps its name and its pixels.

## The design decision, and why

`ILerp` is picked by the pipeline, not by the scaler: `Color4BLerpInt<Bgra8888>` is hard-coded at
every shared call site in `BitmapScalerExtensions` and `BitmapFilterExtensions`. Two options were
considered in this document and both were rejected.

A rounding counterpart *type* that the 1.9 path selects cannot work. `Xbrz.InvokeKernel` implements
`IRescaler`, so its constraints are fixed by that interface and it receives whatever `TLerp` the
call site chose; it also cannot substitute one itself, because `TWork` is generic there and it
cannot know whether the 3-component or the 4-component rounding struct is the right one. Rounding
inside the kernels was rejected too: it means not calling `lerp` at all and open-coding the channel
arithmetic, discarding the colour-space abstraction the interface exists to provide.

**What was done instead: `ILerp<T>` grew a rounding counterpart *method*, `LerpRounded`, in both
overloads.** The kernels keep blending through the interface and simply call the member matching
their variant.

The members are declared, not defaulted. Default interface methods would have kept the interface
source-compatible for outside implementors, but they need runtime support this package does not
have: it targets down to net35, and net35 through net48 make the compiler reject them outright
(CS8701). `BatchDistanceDefaults` already exists in this package for the same reason. All eleven
shipped implementations implement the new members explicitly, which also keeps them off the boxing
path — these are hot loops reached through a `struct` generic constraint. Implementations for which
rounding is meaningless forward to the truncating member and say so in a comment: the float-based
lerps, where the blend never truncated to a representable step, and the no-op lerps.

Outside implementors of `ILerp<T>` must add the two members. That is a source break with no
alternative on these target frameworks, and it is why the members are documented with the behaviour
a forwarding implementation reproduces.

## How it is checked

`XbrzVersionTests` pins all three properties exactly, since all three are exact and integer.

- **The existing entry does not move.** Its output at all five scales is pinned by SHA-256 against
  hashes captured from the code *before* 1.9 support existed. Verified stable across net45, net48,
  netcoreapp3.1 and net9.0, and across x86 and x64.
- **The threshold.** A source whose accumulated distance ratios straddle 2.2 and 2.4, compared
  between the 2.2 and 2.4 kernels with blending held at the truncating mode, so a difference can
  only be the threshold's.
- **The rounding.** Blends whose weighted sum leaves a non-zero remainder against the total weight,
  covering both the weighted and the 50/50 path, plus a kernel-level comparison with the threshold
  held at 2.2.

The two single-change variants exist only for that attribution: asserting against the combined 1.9
variant alone would let either change go missing unnoticed, because the other still produces a
difference. Every assertion was confirmed to bite by perturbing what it tests — restoring a `22`,
dropping a rounding term, and pointing the public entry at 1.9 — and checking the matching test
fails.
