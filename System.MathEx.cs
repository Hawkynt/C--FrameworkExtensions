#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Diagnostics.Contracts;
using System.Runtime;
using word = System.UInt16;
using dword = System.UInt32;
using qword = System.UInt64;

namespace System {
  internal static class MathEx {
    [Pure]
    public static bool IsOdd(this byte This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this byte This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static bool IsOdd(this sbyte This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this sbyte This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static bool IsOdd(this word This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this word This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static bool IsOdd(this short This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this short This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static bool IsOdd(this dword This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this dword This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static bool IsOdd(this int This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this int This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static bool IsOdd(this qword This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this qword This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static bool IsOdd(this long This) {
      return ((This & 1) != 0);
    }

    [Pure]
    public static bool IsEven(this long This) {
      return ((This & 1) == 0);
    }

    [Pure]
    public static double Round(this double This) {
      return (Math.Round(This));
    }

    [Pure]
    public static double Round(this double This, int digits) {
      Contract.Requires(digits >= 0 && digits <= 15);
      return (Math.Round(This, digits));
    }

    [Pure]
    public static double Floor(this double This) {
      return (Math.Floor(This));
    }

    [Pure]
    public static double Ceiling(this double This) {
      return (Math.Ceiling(This));
    }

    [Pure]
    public static float Round(this float This) {
      return ((float)Math.Round(This));
    }

    [Pure]
    public static float Round(this float This, int digits) {
      Contract.Requires(digits >= 0 && digits <= 15);
      return ((float)Math.Round(This, digits));
    }

    [Pure]
    public static float Floor(this float This) {
      return ((float)Math.Floor(This));
    }

    [Pure]
    public static float Ceiling(this float This) {
      return ((float)Math.Ceiling(This));
    }

    [Pure]
    public static decimal Round(this decimal This) {
      return (Math.Round(This));
    }

    [Pure]
    public static decimal Round(this decimal This, int digits) {
      Contract.Requires(digits >= 0 && digits <= 28);
      return (Math.Round(This, digits));
    }

    [Pure]
    public static decimal Floor(this decimal This) {
      return (Math.Floor(This));
    }

    [Pure]
    public static decimal Ceiling(this decimal This) {
      return (Math.Ceiling(This));
    }

    [Pure]
    public static byte Abs(this sbyte This) {
      return (This == sbyte.MinValue ? (byte)-sbyte.MinValue : (byte)Math.Abs(This));
    }

    [Pure]
    public static ushort Abs(this short This) {
      return (This == short.MinValue ? (ushort)-short.MinValue : (ushort)Math.Abs(This));
    }

    [TargetedPatchingOptOut("")]
    [Pure]
    public static uint Abs(this int This) {
#if DEBUG
      return (uint)(This < 0 ? -This : This);
#else
      var mask = This >> 31;
      var r = (uint)((This ^ mask) - mask);
      return (r);
#endif
    }

    [TargetedPatchingOptOut("")]
    [Pure]
    public static ulong Abs(this long This) {
#if DEBUG
      return (uint)(This < 0 ? -This : This);
#else
      var mask = This >> 63;
      var r = (uint)((This ^ mask) - mask);
      return (r);
#endif
    }

    [Pure]
    public static float Abs(this float This) {
      return (Math.Abs(This));
    }

    [Pure]
    public static double Abs(this double This) {
      return (Math.Abs(This));
    }

    [Pure]
    public static decimal Abs(this decimal This) {
      return (Math.Abs(This));
    }

    [Pure]
    public static double Sin(this double This) {
      return (Math.Sin(This));
    }

    [Pure]
    public static double Sinh(this double This) {
      return (Math.Sinh(This));
    }

    [Pure]
    public static double Cos(this double This) {
      return (Math.Cos(This));
    }

    [Pure]
    public static double Cosh(this double This) {
      return (Math.Cosh(This));
    }

    [Pure]
    public static double Tan(this double This) {
      return (Math.Tan(This));
    }

    [Pure]
    public static double Tanh(this double This) {
      return (Math.Tanh(This));
    }

    [Pure]
    public static double Sqrt(this double This) {
      return (Math.Sqrt(This));
    }

    [Pure]
    public static double Sqr(this double This) {
      return (Math.Pow(This, 2));
    }

    [Pure]
    public static double Truncate(this double This) {
      return (Math.Truncate(This));
    }

    [Pure]
    public static decimal Truncate(this decimal This) {
      return (Math.Truncate(This));
    }

    [Pure]
    public static int Sign(this sbyte This) {
      return (Math.Sign(This));
    }

    [Pure]
    public static int Sign(this short This) {
      return (Math.Sign(This));
    }

    [Pure]
    public static int Sign(this int This) {
      return (Math.Sign(This));
    }

    [Pure]
    public static int Sign(this long This) {
      return (Math.Sign(This));
    }

    [Pure]
    public static int Sign(this float This) {
      Contract.Requires(!float.IsNaN(This));
      return (Math.Sign(This));
    }

    [Pure]
    public static int Sign(this double This) {
      Contract.Requires(!double.IsNaN(This));
      return (Math.Sign(This));
    }

    [Pure]
    public static int Sign(this decimal This) {
      return (Math.Sign(This));
    }

    [Pure]
    public static double Pow(this double This, double exponent) {
      return (Math.Pow(This, exponent));
    }

    [Pure]
    public static double Log10(this double This) {
      return (Math.Log10(This));
    }

    [Pure]
    public static double Log(this double This) {
      return (Math.Log(This));
    }

    [Pure]
    public static double Log(this double This, double @base) {
      return (Math.Log(This, @base));
    }

    [Pure]
    public static bool IsNaN(this float value) {
      return (float.IsNaN(value));
    }

    [Pure]
    public static bool IsNaN(this double value) {
      return (double.IsNaN(value));
    }

    [Pure]
    public static bool IsInfinity(this float value) {
      return (float.IsInfinity(value));
    }

    [Pure]
    public static bool IsInfinity(this double value) {
      return (double.IsInfinity(value));
    }

    [Pure]
    public static bool IsPositiveInfinity(this float value) {
      return (float.IsPositiveInfinity(value));
    }

    [Pure]
    public static bool IsPositiveInfinity(this double value) {
      return (double.IsPositiveInfinity(value));
    }
    [Pure]
    public static bool IsNegativeInfinity(this float value) {
      return (float.IsNegativeInfinity(value));
    }

    [Pure]
    public static bool IsNegativeInfinity(this double value) {
      return (double.IsNegativeInfinity(value));
    }

    #region fast min/max
    #region int
    /// <summary>
    /// Gets the minimum value from the given ones.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The min value.</returns>
    [Pure]
    public static int Min(params int[] values) {
      var length = values.Length;
      if (length == 0)
        return (default(int));

      var result = values[0];
      for (var i = length; i > 1; ) {
        --i;
        if (values[i] < result)
          result = values[i];
      }

      return (result);
    }

    /// <summary>
    /// Gets the maximum value from the given ones.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The max value.</returns>
    [Pure]
    public static int Max(params int[] values) {
      var length = values.Length;
      if (length == 0)
        return (default(int));

      var result = values[0];
      for (var i = length; i > 1; ) {
        --i;
        if (values[i] > result)
          result = values[i];
      }

      return (result);
    }
    #endregion
    #region double
    /// <summary>
    /// Gets the minimum value from the given ones.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The min value.</returns>
    [Pure]
    public static double Min(params double[] values) {
      var length = values.Length;
      if (length == 0)
        return (default(double));

      var result = values[0];
      for (var i = length; i > 1; ) {
        --i;
        if (values[i] < result)
          result = values[i];
      }

      return (result);
    }

    /// <summary>
    /// Gets the maximum value from the given ones.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The max value.</returns>
    [Pure]
    public static double Max(params double[] values) {
      var length = values.Length;
      if (length == 0)
        return (default(double));

      var result = values[0];
      for (var i = length; i > 1; ) {
        --i;
        if (values[i] > result)
          result = values[i];
      }

      return (result);
    }
    #endregion
    #region decimal
    /// <summary>
    /// Gets the minimum value from the given ones.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The min value.</returns>
    [Pure]
    public static decimal Min(params decimal[] values) {
      var length = values.Length;
      if (length == 0)
        return (default(decimal));

      var result = values[0];
      for (var i = length; i > 1; ) {
        --i;
        if (values[i] < result)
          result = values[i];
      }

      return (result);
    }

    /// <summary>
    /// Gets the maximum value from the given ones.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The max value.</returns>
    [Pure]
    public static decimal Max(params decimal[] values) {
      var length = values.Length;
      if (length == 0)
        return (default(decimal));

      var result = values[0];
      for (var i = length; i > 1; ) {
        --i;
        if (values[i] > result)
          result = values[i];
      }

      return (result);
    }
    #endregion

    #endregion

  }
}