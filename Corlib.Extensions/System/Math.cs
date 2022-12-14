#region (c)2010-2042 Hawkynt
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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

#if NET5_0_OR_GREATER || NETCOREAPP
#define SUPPORTS_FMADD
#define SUPPORTS_MATHF
#endif

#if NET45_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_INLINING 
#endif

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using word = System.UInt16;
using dword = System.UInt32;
using qword = System.UInt64;

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantCast
// ReSharper disable CompareOfFloatsByEqualityOperator
namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class MathEx {

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
    public static float Floor(this float @this) => MathF.Floor(@this);
#else
    public static float Floor(this float @this) => (float)Math.Floor(@this);
#endif

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
    public static float Ceiling(this float @this) => MathF.Ceiling(@this);
#else
    public static float Ceiling(this float @this) => (float)Math.Ceiling(@this);
#endif

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
    public static float Truncate(this float @this) => MathF.Truncate(@this);
#else
    public static float Truncate(this float @this) => (float)Math.Truncate(@this);
#endif

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
    public static float Round(this float @this) => MathF.Round(@this);
#else
    public static float Round(this float @this) => (float)Math.Round(@this);
#endif

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static float Round(this float @this, int digits) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(digits >= 0 && digits <= 15);
#endif
#if SUPPORTS_MATHF
      return MathF.Round(@this, digits);
#else
      return (float)Math.Round(@this, digits);
#endif
    }

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
    public static float Round(this float @this, MidpointRounding method) => MathF.Round(@this, method);
#else
    public static float Round(this float @this, MidpointRounding method) => (float)Math.Round(@this, method);
#endif

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static float Round(this float @this, int digits, MidpointRounding method) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(digits >= 0 && digits <= 15);
#endif
#if SUPPORTS_MATHF
      return MathF.Round(@this, digits, method);
#else
      return (float)Math.Round(@this, digits, method);
#endif
    }

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Log(this double @this, double @base) => Math.Log(@this, @base);

    /// <summary>
    /// Calculates the cubic root.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Cbrt(this double @this) => Math.Pow(@this, 1d / 3);

    /// <summary>
    /// Calculates the cotangent.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Cot(this double @this) => Math.Cos(@this) / Math.Sin(@this);

    /// <summary>
    /// Calculates the hyperbolic cotangent.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Coth(this double @this) {
      var ex = Math.Exp(@this);
      var em = 1 / ex;
      return (ex + em) / (ex - em);
    }

    /// <summary>
    /// Calculates the cosecant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Csc(this double @this) => 1 / Math.Sin(@this);

    /// <summary>
    /// Calculates the hyperbolic cosecant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Csch(this double @this) {
      var ex = Math.Exp(@this);
      return 2 / (ex - 1 / ex);
    }

    /// <summary>
    /// Calculates the secant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Sec(this double @this) => 1 / Math.Cos(@this);

    /// <summary>
    /// Calculates the hyperbolic secant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Sech(this double @this) {
      var ex = Math.Exp(@this);
      return 2 / (ex + 1 / ex);
    }

    /// <summary>
    /// Calculates the area hyperbolic sine.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Arsinh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this + 1));

    /// <summary>
    /// Calculates the area hyperbolic cosine.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Arcosh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this - 1));

    /// <summary>
    /// Calculates the area hyperbolic tangent.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Artanh(this double @this) => 0.5d * Math.Log((1 + @this) / (1 - @this));

    /// <summary>
    /// Calculates the area hyperbolic cotangent.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Arcoth(this double @this) => 0.5d * Math.Log((@this + 1) / (@this - 1));

    /// <summary>
    /// Calculates the area hyperbolic secant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Arsech(this double @this) => Math.Log((1 + Math.Sqrt(1 - @this * @this)) / @this);

    /// <summary>
    /// Calculates the area hyperbolic cosecant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Arcsch(this double @this) => Math.Log((1 + Math.Sqrt(1 + @this * @this)) / @this);

    /// <summary>
    /// Calculates the arcus sine.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Asin(this double @this) => Math.Asin(@this);

    /// <summary>
    /// Calculates the arcus cosine.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Acos(this double @this) => Math.Acos(@this);

    /// <summary>
    /// Calculates the arcus tangent.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Atan(this double @this) => Math.Atan(@this);

    /// <summary>
    /// Calculates the arcus cotangent.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Acot(this double @this) => Math.Atan(1 / @this);

    /// <summary>
    /// Calculates the arcus secant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Asec(this double @this) => Math.Acos(1 / @this);

    /// <summary>
    /// Calculates the arcus cosecant.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double Acsc(this double @this) => Math.Asin(1 / @this);
  
  }
}
