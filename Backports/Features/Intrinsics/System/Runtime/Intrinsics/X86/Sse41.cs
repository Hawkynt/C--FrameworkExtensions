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
//

#if !SUPPORTS_INTRINSICS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of SSE4.1 intrinsics.
/// </summary>
public static class Sse41 {
  public static bool IsSupported => false;

  #region Blend

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Blend(Vector128<float> left, Vector128<float> right, byte control) {
    var result = new float[4];
    for (var i = 0; i < 4; ++i)
      result[i] = ((control >> i) & 1) != 0
        ? Vector128.GetElement(right, i)
        : Vector128.GetElement(left, i);
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Blend(Vector128<double> left, Vector128<double> right, byte control) {
    var result = new double[2];
    for (var i = 0; i < 2; ++i)
      result[i] = ((control >> i) & 1) != 0
        ? Vector128.GetElement(right, i)
        : Vector128.GetElement(left, i);
    return Vector128.Create(result[0], result[1]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> BlendVariable(Vector128<float> left, Vector128<float> right, Vector128<float> mask) {
    var result = new float[4];
    var maskInt = Vector128.As<float, int>(mask);
    for (var i = 0; i < 4; ++i)
      result[i] = Vector128.GetElement(maskInt, i) < 0
        ? Vector128.GetElement(right, i)
        : Vector128.GetElement(left, i);
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  #endregion

  #region Rounding

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Ceiling(Vector128<float> value) {
    var result = new float[4];
    for (var i = 0; i < 4; ++i)
      result[i] = MathF.Ceiling(Vector128.GetElement(value, i));
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Ceiling(Vector128<double> value) {
    var result = new double[2];
    for (var i = 0; i < 2; ++i)
      result[i] = Math.Ceiling(Vector128.GetElement(value, i));
    return Vector128.Create(result[0], result[1]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Floor(Vector128<float> value) {
    var result = new float[4];
    for (var i = 0; i < 4; ++i)
      result[i] = MathF.Floor(Vector128.GetElement(value, i));
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Floor(Vector128<double> value) {
    var result = new double[2];
    for (var i = 0; i < 2; ++i)
      result[i] = Math.Floor(Vector128.GetElement(value, i));
    return Vector128.Create(result[0], result[1]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToZero(Vector128<float> value) {
    var result = new float[4];
    for (var i = 0; i < 4; ++i)
      result[i] = MathF.Truncate(Vector128.GetElement(value, i));
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToZero(Vector128<double> value) {
    var result = new double[2];
    for (var i = 0; i < 2; ++i)
      result[i] = Math.Truncate(Vector128.GetElement(value, i));
    return Vector128.Create(result[0], result[1]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToNearestInteger(Vector128<float> value) {
    var result = new float[4];
    for (var i = 0; i < 4; ++i)
      result[i] = MathF.Round(Vector128.GetElement(value, i), MidpointRounding.ToEven);
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToNearestInteger(Vector128<double> value) {
    var result = new double[2];
    for (var i = 0; i < 2; ++i)
      result[i] = Math.Round(Vector128.GetElement(value, i), MidpointRounding.ToEven);
    return Vector128.Create(result[0], result[1]);
  }

  #endregion

  #region Conversions

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ConvertToVector128Int16(Vector128<sbyte> value) {
    var result = new short[8];
    for (var i = 0; i < 8; ++i)
      result[i] = Vector128.GetElement(value, i);
    return Vector128.Create(result[0], result[1], result[2], result[3], result[4], result[5], result[6], result[7]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector128<short> value) {
    var result = new int[4];
    for (var i = 0; i < 4; ++i)
      result[i] = Vector128.GetElement(value, i);
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ConvertToVector128Int64(Vector128<int> value) {
    var result = new long[2];
    for (var i = 0; i < 2; ++i)
      result[i] = Vector128.GetElement(value, i);
    return Vector128.Create(result[0], result[1]);
  }

  #endregion

  #region Dot Product

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> DotProduct(Vector128<float> left, Vector128<float> right, byte control) {
    var sum = 0.0f;
    for (var i = 0; i < 4; ++i)
      if (((control >> (i + 4)) & 1) != 0)
        sum += Vector128.GetElement(left, i) * Vector128.GetElement(right, i);

    var result = new float[4];
    for (var i = 0; i < 4; ++i)
      result[i] = ((control >> i) & 1) != 0 ? sum : 0;

    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> DotProduct(Vector128<double> left, Vector128<double> right, byte control) {
    var sum = 0.0;
    for (var i = 0; i < 2; ++i)
      if (((control >> (i + 4)) & 1) != 0)
        sum += Vector128.GetElement(left, i) * Vector128.GetElement(right, i);

    var result = new double[2];
    for (var i = 0; i < 2; ++i)
      result[i] = ((control >> i) & 1) != 0 ? sum : 0;

    return Vector128.Create(result[0], result[1]);
  }

  #endregion

  #region Extract/Insert

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Extract(Vector128<int> value, byte index) => Vector128.GetElement(value, index & 3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Extract(Vector128<byte> value, byte index) => Vector128.GetElement(value, index & 15);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Insert(Vector128<float> value, Vector128<float> data, byte control) {
    // BCL signature: Insert(Vector128<float>, Vector128<float>, byte)
    // Intel INSERTPS control byte: bits 7:6 = source index from data, bits 5:4 = destination index, bits 3:0 = zero mask
    var srcIndex = (control >> 6) & 0x3;
    var destIndex = (control >> 4) & 0x3;
    var zeroMask = control & 0xF;

    var result = new float[4];
    for (var i = 0; i < 4; ++i)
      result[i] = ((zeroMask >> i) & 1) != 0 ? 0f : Vector128.GetElement(value, i);
    result[destIndex] = Vector128.GetElement(data, srcIndex);
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  #endregion

  #region Min/Max

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Max(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = new sbyte[16];
    for (var i = 0; i < 16; ++i)
      result[i] = Math.Max(Vector128.GetElement(left, i), Vector128.GetElement(right, i));
    return Vector128.Create(result[0], result[1], result[2], result[3], result[4], result[5], result[6], result[7],
      result[8], result[9], result[10], result[11], result[12], result[13], result[14], result[15]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Min(Vector128<uint> left, Vector128<uint> right) {
    var result = new uint[4];
    for (var i = 0; i < 4; ++i)
      result[i] = Math.Min(Vector128.GetElement(left, i), Vector128.GetElement(right, i));
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> MinHorizontal(Vector128<ushort> value) {
    var minValue = Vector128.GetElement(value, 0);
    var minIndex = 0;
    for (var i = 1; i < 8; ++i) {
      var current = Vector128.GetElement(value, i);
      if (current < minValue) {
        minValue = current;
        minIndex = i;
      }
    }
    return Vector128.Create(minValue, (ushort)minIndex, (ushort)0, (ushort)0, (ushort)0, (ushort)0, (ushort)0, (ushort)0);
  }

  #endregion

  #region Multiply

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> MultiplyLow(Vector128<int> left, Vector128<int> right) {
    var result = new int[4];
    for (var i = 0; i < 4; ++i)
      result[i] = Vector128.GetElement(left, i) * Vector128.GetElement(right, i);
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> MultiplyLow(Vector128<uint> left, Vector128<uint> right) {
    var result = new uint[4];
    for (var i = 0; i < 4; ++i)
      result[i] = Vector128.GetElement(left, i) * Vector128.GetElement(right, i);
    return Vector128.Create(result[0], result[1], result[2], result[3]);
  }

  #endregion

  #region Pack

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> PackUnsignedSaturate(Vector128<int> left, Vector128<int> right) {
    var result = new ushort[8];
    for (var i = 0; i < 4; ++i) {
      var leftVal = Vector128.GetElement(left, i);
      result[i] = leftVal < 0 ? (ushort)0 : leftVal > ushort.MaxValue ? ushort.MaxValue : (ushort)leftVal;

      var rightVal = Vector128.GetElement(right, i);
      result[i + 4] = rightVal < 0 ? (ushort)0 : rightVal > ushort.MaxValue ? ushort.MaxValue : (ushort)rightVal;
    }
    return Vector128.Create(result[0], result[1], result[2], result[3], result[4], result[5], result[6], result[7]);
  }

  #endregion

  #region Test

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<byte> left, Vector128<byte> right) {
    for (var i = 0; i < 16; ++i)
      if ((Vector128.GetElement(left, i) & Vector128.GetElement(right, i)) != 0)
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<int> left, Vector128<int> right) {
    for (var i = 0; i < 4; ++i)
      if ((Vector128.GetElement(left, i) & Vector128.GetElement(right, i)) != 0)
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<byte> left, Vector128<byte> right) {
    for (var i = 0; i < 16; ++i)
      if ((~Vector128.GetElement(left, i) & Vector128.GetElement(right, i)) != 0)
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<int> left, Vector128<int> right) {
    for (var i = 0; i < 4; ++i)
      if ((~Vector128.GetElement(left, i) & Vector128.GetElement(right, i)) != 0)
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<byte> left, Vector128<byte> right) {
    var zFlag = true;
    var cFlag = true;
    for (var i = 0; i < 16; ++i) {
      var l = Vector128.GetElement(left, i);
      var r = Vector128.GetElement(right, i);
      if ((l & r) != 0)
        zFlag = false;
      if ((~l & r) != 0)
        cFlag = false;
    }
    return !zFlag && !cFlag;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<short> left, Vector128<short> right) {
    var zFlag = true;
    var cFlag = true;
    for (var i = 0; i < 8; ++i) {
      var l = Vector128.GetElement(left, i);
      var r = Vector128.GetElement(right, i);
      if ((l & r) != 0)
        zFlag = false;
      if ((~l & r) != 0)
        cFlag = false;
    }
    return !zFlag && !cFlag;
  }

  #endregion
}

#endif