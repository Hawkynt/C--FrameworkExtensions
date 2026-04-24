#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Drawing;
using System.Drawing.Extensions.ColorProcessing.Resizing;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Optional sibling interface implemented by resamplers whose kernel exposes the
/// <see cref="IResampleKernelWithSafePath{TPixel,TWork,TKey,TDecode,TProject,TEncode}"/>
/// safe-interior fast path. The top-level Bitmap.Resample extension detects this
/// interface at dispatch time and routes the entire resize through the zero-OOB pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Design: this is a parallel interface to <see cref="IResampler"/>, not a subclass, so
/// opting in is fully ABI-additive — existing resamplers and the dozens of kernel
/// implementers (filters, content-aware scalers) are unaffected. The dispatch check is
/// one <c>is</c> pattern-match per resize (boxing the struct once), which is negligible
/// at a per-image granularity and critically has <b>no reflection</b> — safe for AOT,
/// trimming, and Native-AOT.
/// </para>
/// <para>
/// To opt in, a resampler:
/// <list type="number">
/// <item>Declares <c>: IResampler, IResamplerWithSafePath</c>.</item>
/// <item>Implements <see cref="ResampleWithSafePath"/> by instantiating its concrete kernel type
/// and forwarding to <c>BitmapScalerExtensions.InvokeSafePathResampler&lt;TKernel&gt;</c>
/// (or equivalent) — this gives the JIT enough type information to statically dispatch
/// to <c>ScalerPipeline.ExecuteResampleParallelWithSafePath</c>.</item>
/// <item>Ensures its kernel struct declares <c>: IResampleKernelWithSafePath&lt;...&gt;</c>
/// and implements <c>GetSafeDestinationRegion</c> + <c>ResampleUnchecked</c>.</item>
/// </list>
/// </para>
/// </remarks>
public interface IResamplerWithSafePath {

  /// <summary>
  /// Resamples <paramref name="source"/> to the target dimensions with full out-of-bounds
  /// control, canvas colour (for <see cref="OutOfBoundsMode.FlatColor"/>), and grid
  /// centring. Internally routes through the safe-path pipeline: destination split into
  /// 4 edge bands + 1 safe interior, interior sampled without any OOB overhead.
  /// </summary>
  Bitmap ResampleWithSafePath(
    Bitmap source,
    int targetWidth,
    int targetHeight,
    OutOfBoundsMode horizontalMode,
    OutOfBoundsMode verticalMode,
    Color canvasColor,
    bool useCenteredGrid
  );
}
