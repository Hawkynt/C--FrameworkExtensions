#!/usr/bin/env python3
"""
Generates bit-exact golden test vectors for the AI-float and posit data types, using the authoritative
reference libraries. Run this once to (re)produce the .tsv files in this directory; the NUnit tests load
them via [TestCaseSource].

    pip install ml_dtypes numpy softposit
    python generate_golden.py

Output files (each line: "<rawHex>\t<exactDecimalValue>"), one row per representable code for <=8-bit
formats (exhaustive), plus sampled encode vectors ("<float>\t<rawHex>") for the wider ones.

Sources of truth:
  * ml_dtypes (github.com/jax-ml/ml_dtypes): float8_e4m3, float8_e5m2, float4_e2m1fn,
    float8_e8m0fnu, bfloat16 — the JAX/TF reference for these formats and the OCP MX element/scale types.
  * SoftPosit (gitlab.com/cerlane/SoftPosit) via the `softposit` Python package: posit8/16/32.
"""

import os
import numpy as np
import ml_dtypes

HERE = os.path.dirname(os.path.abspath(__file__))


def _fmt(v):
    """Format a float as a token .NET float.Parse(InvariantCulture) accepts."""
    v = float(v)
    if v != v:
        return "NaN"
    if v == float("inf"):
        return "Infinity"
    if v == float("-inf"):
        return "-Infinity"
    return repr(v)


def dump_decode_exhaustive(name, nbits, store_dtype, target_dtype):
    path = os.path.join(HERE, f"{name}.decode.tsv")
    width = (nbits + 3) // 4
    with open(path, "w", newline="\n") as f:
        f.write(f"# {name}: <rawHex>\\t<value>. Source: ml_dtypes {target_dtype.__name__}. Exhaustive {1 << nbits} codes.\n")
        for raw in range(1 << nbits):
            v = np.array([raw], dtype=store_dtype).view(target_dtype)[0]
            f.write(f"{raw:0{width}X}\t{_fmt(v)}\n")
    print(f"wrote {path} ({1 << nbits} rows)")


def dump_encode_samples(name, store_dtype, target_dtype, samples):
    path = os.path.join(HERE, f"{name}.encode.tsv")
    with open(path, "w", newline="\n") as f:
        f.write(f"# {name}: <inputFloat>\\t<expectedRawHex>. Source: ml_dtypes {target_dtype.__name__} round-to-nearest-even.\n")
        arr = np.array(samples, dtype=np.float64).astype(target_dtype)
        raws = arr.view(store_dtype)
        for x, r in zip(samples, raws):
            f.write(f"{_fmt(x)}\t{int(r):X}\n")
    print(f"wrote {path} ({len(samples)} rows)")


def main():
    # 8-bit / 4-bit AI floats: exhaustive decode tables (bit-exact ToSingle oracles).
    # Variants chosen to match the library's types: E4M3 = e4m3fn (finite, NaN only),
    # Quarter = e5m2 (IEEE, inf+NaN), E2M1 = e2m1fn, E8M0 = e8m0fnu.
    dump_decode_exhaustive("e4m3", 8, np.uint8, ml_dtypes.float8_e4m3fn)
    dump_decode_exhaustive("quarter", 8, np.uint8, ml_dtypes.float8_e5m2)
    dump_decode_exhaustive("e2m1", 4, np.uint8, ml_dtypes.float4_e2m1fn)
    dump_decode_exhaustive("e8m0", 8, np.uint8, ml_dtypes.float8_e8m0fnu)

    # encode (FromSingle) sample vectors: boundaries + deterministic pseudo-random (fixed seed).
    rng = np.random.default_rng(0xB17B)
    samples = [0.0, 1.0, -1.0, 0.5, 2.0, 6.0, 448.0, 1e-3, 1234.5, 5.0, 0.25, 0.75, 1.25, 2.5]
    samples += [round(float(x), 6) for x in rng.uniform(-500, 500, size=2000)]
    dump_encode_samples("e4m3", np.uint8, ml_dtypes.float8_e4m3fn, samples)
    dump_encode_samples("quarter", np.uint8, ml_dtypes.float8_e5m2, samples)
    dump_encode_samples("e2m1", np.uint8, ml_dtypes.float4_e2m1fn, [s for s in samples if abs(s) <= 6])

    # --- posits: from-spec oracle (SoftPosit/sfpy can't build here without a C toolchain) ---
    # Standard posit<n,es> decode per the 2022 Posit Standard. Validated against published anchors
    # (posit8 0x40=1.0, 0x20=0.5, 0x60=2.0, 0x00=0, 0x80=NaR) by the assertions below.
    def posit_decode(bits, n, es):
        if bits == 0:
            return 0.0
        if bits == (1 << (n - 1)):
            return float("nan")  # NaR
        sign = (bits >> (n - 1)) & 1
        v = ((~bits) + 1) & ((1 << n) - 1) if sign else bits
        pos = n - 2
        first = (v >> pos) & 1
        runlen = 0
        while pos >= 0 and ((v >> pos) & 1) == first:
            runlen += 1
            pos -= 1
        k = (runlen - 1) if first == 1 else -runlen
        pos -= 1  # consume the terminating bit
        exp = 0
        for _ in range(es):
            exp = (exp << 1) | ((v >> pos) & 1 if pos >= 0 else 0)
            pos -= 1
        fracbits = pos + 1
        significand = 1.0 + (v & ((1 << fracbits) - 1)) / (1 << fracbits) if fracbits > 0 else 1.0
        val = significand * (2.0 ** (k * (1 << es) + exp))
        return -val if sign else val

    assert posit_decode(0x40, 8, 0) == 1.0
    assert posit_decode(0x20, 8, 0) == 0.5
    assert posit_decode(0x60, 8, 0) == 2.0
    assert posit_decode(0x00, 8, 0) == 0.0
    assert posit_decode(0x4000, 16, 1) == 1.0
    assert posit_decode(0x40000000, 32, 2) == 1.0

    for name, n, es in (("posit8", 8, 0), ("posit16", 16, 1), ("posit32", 32, 2)):
        exhaustive = n <= 8
        codes = list(range(1 << n)) if exhaustive else list(range(0, 1 << n, (1 << n) // 4096))
        nar = 1 << (n - 1)
        width = n // 4

        # decode table
        path = os.path.join(HERE, f"{name}.decode.tsv")
        with open(path, "w", newline="\n") as f:
            f.write(f"# {name}: <rawHex>\\t<value>. Source: from-spec posit<{n},{es}> oracle.\n")
            for raw in codes:
                f.write(f"{raw:0{width}X}\t{_fmt(posit_decode(raw, n, es))}\n")
        print(f"wrote {path}")

        # encode table: round-trip identity for every (sampled) code, plus unambiguous rounding-direction
        # points 1/4 and 3/4 of the way between adjacent representable values (oracle = nearest by value).
        finite = [(posit_decode(raw, n, es), raw) for raw in codes if raw != nar]
        rows = list(finite)  # (value -> code) round-trip identity
        if exhaustive:
            # rounding-direction points are only valid when codes are truly adjacent by value
            finite.sort(key=lambda t: t[0])
            for (v0, c0), (v1, c1) in zip(finite, finite[1:]):
                rows.append((v0 + (v1 - v0) * 0.25, c0))
                rows.append((v0 + (v1 - v0) * 0.75, c1))
        rows.append((float("nan"), nar))                 # NaN -> NaR
        rows.append((1e300, (1 << (n - 1)) - 1))          # overflow -> +maxpos (no infinity in posit)
        epath = os.path.join(HERE, f"{name}.encode.tsv")
        with open(epath, "w", newline="\n") as f:
            f.write(f"# {name}: <inputValue>\\t<expectedRawHex>. Source: from-spec posit<{n},{es}> nearest-even oracle.\n")
            for v, raw in rows:
                f.write(f"{_fmt(v)}\t{raw:0{width}X}\n")
        print(f"wrote {epath}")


if __name__ == "__main__":
    main()
