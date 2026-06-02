#!/usr/bin/env python3
"""
Generates MXFP4 golden vectors from Microsoft's microxcaling reference (the OCP Microscaling reference
implementation), to cross-check System.MXFP4 bit-for-bit on dequantized output.

Requires torch and a checkout of https://github.com/microsoft/microxcaling (not on PyPI):

    pip install torch numpy
    git clone https://github.com/microsoft/microxcaling
    MX_PATH=./microxcaling python generate_mx_golden.py

Each output line is "<input floats>|<reference dequantized floats>" (comma-separated). microxcaling does
not implement NVFP4 (that is NVIDIA's two-level recipe), so only MXFP4 is cross-checked here; NVFP4 is
verified against its documented recipe in MxFp4SpecTests.
"""

import os
import sys

import numpy as np
import torch

MX_PATH = os.environ.get("MX_PATH", os.path.join(os.path.dirname(__file__), "microxcaling"))
sys.path.insert(0, MX_PATH)
from mx.mx_ops import quantize_mx_op           # noqa: E402
from mx.specs import finalize_mx_specs         # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))

_specs = finalize_mx_specs({
    "scale_bits": 8, "w_elem_format": "fp4_e2m1", "a_elem_format": "fp4_e2m1",
    "block_size": 32, "bfloat": 0, "custom_cuda": False,
}, early_exit=False)


def quantize(arr):
    t = torch.tensor(arr, dtype=torch.float32)
    return quantize_mx_op(t, _specs, elem_format="fp4_e2m1", block_size=32, axes=[-1]).tolist()


def main():
    rng = np.random.default_rng(0x3141)
    arrays = [
        [g * 4 for g in (0, 0.5, 1, 1.5, 2, 3, 4, 6)] + [0.0] * 24,   # on-grid, scale 2^2
        [11.0, -3.0] + [0.0] * 30,                                    # amax 11 -> scale 2
        [0.1, 0.9, 1.3, 2.7, 3.9, 5.5, -0.4, -2.2] + [0.0] * 24,      # off-grid
        list(rng.uniform(-10, 10, size=32)),
        list(rng.uniform(-0.01, 0.01, size=32)),                      # small magnitudes
        list(rng.uniform(-1000, 1000, size=64)),                      # two blocks
        list(rng.uniform(-5, 5, size=70)),                            # ragged last block
    ]

    path = os.path.join(HERE, "mxfp4_ref.tsv")
    with open(path, "w", newline="\n") as f:
        f.write("# MXFP4 reference: <inputFloats>|<dequantizedFloats>. Source: microsoft/microxcaling fp4_e2m1, block 32.\n")
        for arr in arrays:
            out = quantize(arr)
            f.write(",".join(repr(float(x)) for x in arr))
            f.write("|")
            f.write(",".join(repr(float(x)) for x in out))
            f.write("\n")
    print(f"wrote {path} ({len(arrays)} arrays)")


if __name__ == "__main__":
    main()
