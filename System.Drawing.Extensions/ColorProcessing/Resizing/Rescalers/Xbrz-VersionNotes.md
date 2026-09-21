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

## What an implementation has to do

1. Add a variant selector to `Xbrz` and thread it into the per-scale kernels.
2. For the 1.9 variant use the `24` numerator in both steep/shallow comparisons.
3. For the 1.9 variant round the weighted blend to nearest instead of truncating. `ILerp` is chosen
   by the pipeline rather than by the scaler, so this needs a decision: either a rounding
   counterpart to `Color3BLerpInt`/`Color4BLerpInt` that the 1.9 path selects, or a rounding
   operation added to the kernels themselves.
4. Register the variant as its own `[ScalerInfo]` entry, alongside `XBR` / `XBR NoBlend` /
   `XBR 3x Original`, which already establish that graphically distinct variants get their own
   names. Leave the existing `xBRZ` entry's name unchanged so existing scripts and settings keep
   working.

## Checking it

Neither change can be verified by eye alone. Both are exact and integer, so they can be pinned
directly: the threshold change by inputs whose two accumulated distances straddle the 2.2 and 2.4
ratios, and the rounding change by a blend whose weighted sum has a non-zero remainder against `N`.
The existing variant must stay bit-identical, which a comparison against its current output pins.
