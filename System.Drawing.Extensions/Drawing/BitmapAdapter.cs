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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Pipeline;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.Drawing.Lockers;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for adapting System.Drawing.Bitmap to the color processing pipeline.
/// </summary>
public static class BitmapAdapter {
  
  /// <summary>
  /// Creates a new bitmap from a working space frame.
  /// </summary>
  /// <typeparam name="TWork">Working color type.</typeparam>
  /// <typeparam name="TEncode">Encoder strategy.</typeparam>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bitmap ToBitmap<TWork, TEncode>(
    in WorkFrame<TWork> workFrame,
    TEncode encoder = default
  )
    where TWork : unmanaged, IColorSpace
    where TEncode : struct, IEncode<TWork, Bgra8888> {
    var bitmap = new Bitmap(workFrame.Width, workFrame.Height, PixelFormat.Format32bppArgb);
    bitmap.FromWorkFrame(workFrame, encoder);
    return bitmap;
  }

  /// <param name="this">Source bitmap.</param>
  extension(Bitmap @this)
  {

    /// <summary>
    /// Decodes a 32-bit ARGB bitmap to a working space frame.
    /// </summary>
    /// <typeparam name="TWork">Working color type.</typeparam>
    /// <typeparam name="TDecode">Decoder strategy.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WorkFrame<TWork> ToWorkFrame<TWork, TDecode>(
      TDecode decoder = default
    )
      where TWork : unmanaged, IColorSpace
      where TDecode : struct, IDecode<Bgra8888, TWork> {
      using var locker = new Argb8888BitmapLocker(@this, ImageLockMode.ReadOnly);
      var frame = locker.AsFrame();
      return WorkPipeline.Decode<Bgra8888, TWork, TDecode>(frame, decoder);
    }

    /// <summary>
    /// Encodes a working space frame back to a 32-bit ARGB bitmap.
    /// </summary>
    /// <typeparam name="TWork">Working color type.</typeparam>
    /// <typeparam name="TEncode">Encoder strategy.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromWorkFrame<TWork, TEncode>(
      in WorkFrame<TWork> workFrame,
      TEncode encoder = default
    )
      where TWork : unmanaged, IColorSpace
      where TEncode : struct, IEncode<TWork, Bgra8888> {
      using var locker = new Argb8888BitmapLocker(@this, ImageLockMode.WriteOnly);
      var frame = locker.AsFrame();
      WorkPipeline.Encode(workFrame, frame, encoder);
    }
    
    /// <summary>
    /// Locks a bitmap for span-based 32-bit ARGB access.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IBitmapLocker LockRgba32(ImageLockMode lockMode = ImageLockMode.ReadWrite)
      => new Argb8888BitmapLocker(@this, lockMode);

    /// <summary>
    /// Locks a bitmap for span-based 24-bit RGB access.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IBitmapLocker LockRgb24(ImageLockMode lockMode = ImageLockMode.ReadWrite)
      => new Rgb888BitmapLocker(@this, lockMode);

    /// <summary>
    /// Processes a bitmap through the color processing pipeline.
    /// </summary>
    /// <typeparam name="TWork">Working color type.</typeparam>
    /// <typeparam name="TDecode">Decoder strategy.</typeparam>
    /// <typeparam name="TEncode">Encoder strategy.</typeparam>
    /// <param name="process">Processing function applied to the work frame.</param>
    /// <param name="decoder">Decoder instance.</param>
    /// <param name="encoder">Encoder instance.</param>
    public void Process<TWork, TDecode, TEncode>(
      Action<WorkFrame<TWork>> process,
      TDecode decoder = default,
      TEncode encoder = default
    )
      where TWork : unmanaged, IColorSpace
      where TDecode : struct, IDecode<Bgra8888, TWork>
      where TEncode : struct, IEncode<TWork, Bgra8888> {
      using var workFrame = @this.ToWorkFrame<TWork, TDecode>(decoder);
      process(workFrame);
      @this.FromWorkFrame(workFrame, encoder);
    }
  }
}
