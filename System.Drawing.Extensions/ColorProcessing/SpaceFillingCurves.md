# Space-filling traversals

`Hawkynt.ColorProcessing.SpaceFillingCurves` exposes the pixel traversal primitives used by
`RiemersmaDitherer`. They can also be used directly by filters, samplers, visualizers, cache-local
algorithms, or experiments that need a deterministic mapping from a raster to a one-dimensional path.

## Available traversals

| Traversal | Native domain | Recursive subdivision | Consecutive-step locality | `order` |
| --- | --- | --- | --- | --- |
| Hilbert | square | 2×2 | 4-connected on a complete square; clipping can introduce jumps | optional, explicit 1–7 |
| Moore | square | 2×2 | 4-connected and closed on a complete square; clipping can introduce jumps | optional, explicit 1–7 |
| Gilbert | arbitrary rectangle | adaptive | 4-connected for most rectangles; some parity combinations require one diagonal transition | ignored |
| Peano | square | 3×3 | 4-connected on a complete square | optional, 1–5 |
| Coil | square | 3×3 | 4-connected on a complete square | optional, 1–5 |
| Half-coil | square | 3×3 | 4-connected on a complete square | optional, 1–5 |
| Meurthe | square | 3×3 | 4-connected on a complete square | optional, 1–5 |
| Morton / Z-order | square | 2×2 | hierarchical locality, but discontinuous jumps are expected | optional, explicit 1–7 |
| Spiral | arbitrary rectangle | none | 4-connected | ignored |
| Diagonal serpentine | arbitrary rectangle | none | diagonal steps inside each `x + y` diagonal | ignored |
| Linear serpentine | arbitrary rectangle | none | 4-connected | ignored |

For square recursive curves, omitting `order` selects a covering order automatically where supported.
When an explicitly requested square is larger than the requested raster, coordinates outside the raster
are clipped. Clipping preserves exact pixel coverage, but it cannot preserve the continuity guarantee of
the original complete square.

## Riemersma examples

```csharp
// Default: classical Hilbert traversal with 16 error-history entries.
var hilbert = RiemersmaDitherer.Default;

// Closed Hilbert-relative traversal.
var moore = RiemersmaDitherer.Moore;

// Direct arbitrary-rectangle traversal; no padded power-of-two square is generated.
var rectangular = RiemersmaDitherer.Gilbert;

// Continuous 3×3 alternatives with different recursive orientations.
var peano = RiemersmaDitherer.Peano;
var coil = RiemersmaDitherer.Coil;
var halfCoil = RiemersmaDitherer.HalfCoil;
var meurthe = RiemersmaDitherer.Meurthe;

// Useful baselines for studying how traversal continuity affects Riemersma error history.
var morton = RiemersmaDitherer.Morton;
var spiral = RiemersmaDitherer.SpiralScan;
var diagonal = RiemersmaDitherer.DiagonalScan;
```

Custom configuration uses the same enum:

```csharp
var ditherer = new RiemersmaDitherer(
  historySize: 16,
  curveType: SpaceFillingCurve.Meurthe,
  curveOrder: 4
);
```

`curveOrder` is intentionally ignored by `Gilbert`, `Spiral`, `DiagonalSerpentine`, and `Linear` because
those traversals operate directly on the requested rectangle instead of an enclosing recursive square.

## Direct traversal use

```csharp
var path = SpaceFillingCurves.Gilbert(width: 800, height: 600);
foreach (var (x, y) in path) {
  // Process pixels in generalized-Hilbert order.
}
```

All generators return each covered raster coordinate exactly once. The returned list makes the API easy
to consume and matches the existing dithering pipeline; callers that need streaming traversal should not
assume the list is allocation-free.

## The 3×3 engine

Peano, Coil, Half-coil, and Meurthe share one implementation. They use the same ternary reflected-Gray-code
subcell order and reflection state; only the recursive axis permutation differs. This is the two-dimensional
specialization of Haverkort's common framework for 3-regular mono-Wunderlich curves.

That distinction matters: these are not four unrelated hand-coded recursions. The shared engine keeps the
continuity constraints in one place and makes the different recursive orientations directly comparable for
dithering experiments.

## Why some famous curves are not enum aliases

Not every space-filling curve is a sensible square-pixel traversal.

- **Gosper / flowsnake** is naturally defined on a hexagonal lattice. Mapping it to square pixels would
  require choosing a projection with different adjacency semantics, so calling that result "Gosper" would
  be misleading.
- **Sierpiński** variants are naturally triangular and have the same raster-mapping problem.
- **Wunderlich 1/2/3** need exact per-subcell rotation/reflection definitions. The 3×3 engine above is the
  right architectural home for them, but named variants should only be added with verified transform tables.
- **Randomized Hilbert** is only useful if random recursive choices preserve compatible entrance/exit corners.
  Arbitrary rotations or reflections break Hilbert continuity, so a seeded version needs a topology-safe
  transform set rather than random orientation changes.

## References

- Thiadmer Riemersma, *A Balanced Dithering Technique* (1998): https://www.compuphase.com/riemer.htm
- Jakub Červený, generalized Hilbert / Gilbert traversal: https://github.com/jakubcerveny/gilbert
- Herman Haverkort, *Sixteen space-filling curves and traversals for d-dimensional cubes and simplices*:
  https://arxiv.org/abs/1711.04473
- Robert Dickau, Wunderlich curve overview: https://www.robertdickau.com/wunderlich.html
