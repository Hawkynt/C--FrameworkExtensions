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

#if !SUPPORTS_VECTOR_64_FULL && !SUPPORTS_VECTOR_64_TYPE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

/// <summary>
/// Polyfill for Vector64 full API (operators, advanced methods) added in .NET 7.0.
/// </summary>
public static partial class Vector64Polyfills {

  extension(Vector64) {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector64<T> Load<T>(T* source) where T : struct
      => Unsafe.ReadUnaligned<Vector64<T>>(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector64<T> LoadUnsafe<T>(ref T source) where T : struct
      => Unsafe.ReadUnaligned<Vector64<T>>(ref Unsafe.As<T, byte>(ref source));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Store<T>(Vector64<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreUnsafe<T>(Vector64<T> source, ref T destination) where T : struct
      => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAligned<T>(Vector64<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAlignedNonTemporal<T>(Vector64<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Create<T>(T value) where T : struct {
      var count = 8 / Unsafe.SizeOf<T>();
      unsafe {
        var buffer = stackalloc byte[8];
        var ptr = (T*)buffer;
        for (var i = 0; i < count; ++i)
          ptr[i] = value;
        return Unsafe.ReadUnaligned<Vector64<T>>(buffer);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Add<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Add(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Subtract<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Subtract(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Multiply<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Divide<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Divide(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Negate<T>(Vector64<T> vector) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Negate(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Abs<T>(Vector64<T> vector) where T : struct {
       if (Scalar<T>.IsUnsigned)
         return vector;

       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Abs(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Sqrt<T>(Vector64<T> vector) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Sqrt(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(Vector64<T> vector) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Scalar<T>.Zero();
       ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));

       for(int i=0; i<count; i++)
           result = Scalar<T>.Add(result, Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Scalar<T>.Zero();
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));

       for(int i=0; i<count; i++) {
           var product = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           result = Scalar<T>.Add(result, product);
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> BitwiseAnd<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
       var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
       var res = leftBits & rightBits;
       return Unsafe.As<ulong, Vector64<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> BitwiseOr<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
       var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
       var res = leftBits | rightBits;
       return Unsafe.As<ulong, Vector64<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Xor<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
       var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
       var res = leftBits ^ rightBits;
       return Unsafe.As<ulong, Vector64<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> OnesComplement<T>(Vector64<T> vector) where T : struct {
       var bits = Unsafe.As<Vector64<T>, ulong>(ref vector);
       var res = ~bits;
       return Unsafe.As<ulong, Vector64<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> AndNot<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
       var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
       var res = leftBits & ~rightBits;
       return Unsafe.As<ulong, Vector64<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Equals<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var eq = Scalar<T>.ObjectEquals(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = eq ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> GreaterThan<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var gt = Scalar<T>.GreaterThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = gt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> GreaterThanOrEqual<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var ge = Scalar<T>.GreaterThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = ge ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> LessThan<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var lt = Scalar<T>.LessThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = lt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> LessThanOrEqual<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var le = Scalar<T>.LessThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = le ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Min<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Min(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Max<T>(Vector64<T> left, Vector64<T> right) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Max(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Floor<T>(Vector64<T> vector) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Floor(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Ceiling<T>(Vector64<T> vector) where T : struct {
       int count = 8 / Unsafe.SizeOf<T>();
       var result = Vector64<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector64<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Ceiling(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetElement<T>(Vector64<T> vector, int index) where T : struct {
      if ((uint)index >= 8 / Unsafe.SizeOf<T>())
        throw new ArgumentOutOfRangeException(nameof(index));

      ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
      return Unsafe.Add(ref rVector, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> WithElement<T>(Vector64<T> vector, int index, T value) where T : struct {
      if ((uint)index >= 8 / Unsafe.SizeOf<T>())
        throw new ArgumentOutOfRangeException(nameof(index));

      var result = vector;
      ref var rResult = ref Unsafe.As<Vector64<T>, T>(ref result);
      Unsafe.Add(ref rResult, index) = value;
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits<T>(Vector64<T> vector) where T : struct {
      uint result = 0;
      int count = 8 / Unsafe.SizeOf<T>();
      ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));

      for (var i = 0; i < count; ++i) {
        if (Scalar<T>.ExtractMostSignificantBit(Unsafe.Add(ref rVector, i)))
          result |= 1u << i;
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<byte> vector) => ExtractMostSignificantBits<byte>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<sbyte> vector) => ExtractMostSignificantBits<sbyte>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<short> vector) => ExtractMostSignificantBits<short>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<ushort> vector) => ExtractMostSignificantBits<ushort>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<int> vector) => ExtractMostSignificantBits<int>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<uint> vector) => ExtractMostSignificantBits<uint>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<long> vector) => ExtractMostSignificantBits<long>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<ulong> vector) => ExtractMostSignificantBits<ulong>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<float> vector) => ExtractMostSignificantBits<float>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector64<double> vector) => ExtractMostSignificantBits<double>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Shuffle<T>(Vector64<T> vector, Vector64<T> indices) where T : struct {
      return UnsafeShuffle(vector, indices);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<byte> Shuffle(Vector64<byte> vector, Vector64<byte> indices)
      => Shuffle<byte>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> Shuffle(Vector64<sbyte> vector, Vector64<sbyte> indices)
      => Shuffle<sbyte>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<short> Shuffle(Vector64<short> vector, Vector64<short> indices)
      => Shuffle<short>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<ushort> Shuffle(Vector64<ushort> vector, Vector64<ushort> indices)
      => Shuffle<ushort>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> Shuffle(Vector64<int> vector, Vector64<int> indices)
      => Shuffle<int>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<uint> Shuffle(Vector64<uint> vector, Vector64<uint> indices)
      => Shuffle<uint>(vector, indices);

    private static unsafe Vector64<T> UnsafeShuffle<T>(Vector64<T> vector, Vector64<T> indices) where T : struct {
        var size = Unsafe.SizeOf<T>();
        var count = 8 / size;
        var result = Vector64<T>.Zero;

        ref var rVector = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in vector));
        ref var rIndices = ref Unsafe.As<Vector64<T>, T>(ref Unsafe.AsRef(in indices));
        ref var rResult = ref Unsafe.As<Vector64<T>, T>(ref result);

        for (int i = 0; i < count; i++) {
             var idxVal = Unsafe.Add(ref rIndices, i);
             int idx = 0;

             // Use pointers to avoid generic conversion issues and Scalar dependency
             if (typeof(T) == typeof(byte)) idx = *(byte*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(sbyte)) idx = *(sbyte*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(ushort)) idx = *(ushort*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(short)) idx = *(short*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(uint)) idx = (int)*(uint*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(int)) idx = *(int*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(ulong)) idx = (int)*(ulong*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(long)) idx = (int)*(long*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(float)) idx = (int)*(float*)Unsafe.AsPointer(ref idxVal);
             else if (typeof(T) == typeof(double)) idx = (int)*(double*)Unsafe.AsPointer(ref idxVal);

             idx &= (count - 1);
             var val = Unsafe.Add(ref rVector, idx);
             Unsafe.Add(ref rResult, i) = val;
        }
        return result;
    }

    // ===== COMPARISON ALL/ANY METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAll<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.ObjectEquals(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.ObjectEquals(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAll<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThan(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAny<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThan(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAll<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThanOrEqual(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAny<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThanOrEqual(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAll<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThan(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAny<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThan(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAll<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThanOrEqual(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAny<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      for (var i = 0; i < 8 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThanOrEqual(Vector64.GetElement(left, i), Vector64.GetElement(right, i)))
          return true;
      return false;
    }

    // ===== TOSCALAR =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToScalar<T>(Vector64<T> vector) where T : struct
      => Vector64.GetElement(vector, 0);

    // ===== COPYSIGN =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> CopySign(Vector64<float> value, Vector64<float> sign) {
      var count = 8 / sizeof(float);
      var result = Vector64<float>.Zero;
      for (var i = 0; i < count; ++i) {
        var v = Vector64.GetElement(value, i);
        var s = Vector64.GetElement(sign, i);
        var absV = MathF.Abs(v);
        result = Vector64.WithElement(result, i, s < 0 ? -absV : absV);
      }
      return result;
    }

    // ===== CLASSIFICATION METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> IsNaN(Vector64<float> vector) {
      var count = 8 / sizeof(float);
      var result = Vector64<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isNan = float.IsNaN(Vector64.GetElement(vector, i));
        result = Vector64.WithElement(result, i, isNan ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> IsNegative(Vector64<float> vector) {
      var count = 8 / sizeof(float);
      var result = Vector64<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector64.GetElement(vector, i);
        var isNeg = val < 0 || (val == 0 && float.IsNegativeInfinity(1f / val));
        result = Vector64.WithElement(result, i, isNeg ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> IsPositive(Vector64<float> vector) {
      var count = 8 / sizeof(float);
      var result = Vector64<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector64.GetElement(vector, i);
        var isPos = val > 0 || (val == 0 && float.IsPositiveInfinity(1f / val));
        result = Vector64.WithElement(result, i, isPos ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> IsZero(Vector64<float> vector) {
      var count = 8 / sizeof(float);
      var result = Vector64<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isZero = Vector64.GetElement(vector, i) == 0f;
        result = Vector64.WithElement(result, i, isZero ? allBitsSet : 0f);
      }
      return result;
    }

    // ===== NARROW METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<short> Narrow(Vector64<int> lower, Vector64<int> upper) {
      var result = Vector64<short>.Zero;
      result = Vector64.WithElement(result, 0, (short)Vector64.GetElement(lower, 0));
      result = Vector64.WithElement(result, 1, (short)Vector64.GetElement(lower, 1));
      result = Vector64.WithElement(result, 2, (short)Vector64.GetElement(upper, 0));
      result = Vector64.WithElement(result, 3, (short)Vector64.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<ushort> Narrow(Vector64<uint> lower, Vector64<uint> upper) {
      var result = Vector64<ushort>.Zero;
      result = Vector64.WithElement(result, 0, (ushort)Vector64.GetElement(lower, 0));
      result = Vector64.WithElement(result, 1, (ushort)Vector64.GetElement(lower, 1));
      result = Vector64.WithElement(result, 2, (ushort)Vector64.GetElement(upper, 0));
      result = Vector64.WithElement(result, 3, (ushort)Vector64.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<sbyte> Narrow(Vector64<short> lower, Vector64<short> upper) {
      var result = Vector64<sbyte>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector64.WithElement(result, i, (sbyte)Vector64.GetElement(lower, i));
        result = Vector64.WithElement(result, i + 4, (sbyte)Vector64.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<byte> Narrow(Vector64<ushort> lower, Vector64<ushort> upper) {
      var result = Vector64<byte>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector64.WithElement(result, i, (byte)Vector64.GetElement(lower, i));
        result = Vector64.WithElement(result, i + 4, (byte)Vector64.GetElement(upper, i));
      }
      return result;
    }

    // ===== CONVERTTO METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<int> ConvertToInt32(Vector64<float> vector) {
      var result = Vector64<int>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector64.WithElement(result, i, (int)Vector64.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> ConvertToSingle(Vector64<int> vector) {
      var result = Vector64<float>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector64.WithElement(result, i, (float)Vector64.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<float> ConvertToSingle(Vector64<uint> vector) {
      var result = Vector64<float>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector64.WithElement(result, i, (float)Vector64.GetElement(vector, i));
      return result;
    }
  }

  // Extension operators for Vector64<T>
  extension<T>(Vector64<T>) where T : struct {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator +(Vector64<T> left, Vector64<T> right) => Vector64.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator -(Vector64<T> left, Vector64<T> right) => Vector64.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator *(Vector64<T> left, Vector64<T> right) => Vector64.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator /(Vector64<T> left, Vector64<T> right) => Vector64.Divide(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator -(Vector64<T> vector) => Vector64.Negate(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator &(Vector64<T> left, Vector64<T> right) => Vector64.BitwiseAnd(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator |(Vector64<T> left, Vector64<T> right) => Vector64.BitwiseOr(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator ^(Vector64<T> left, Vector64<T> right) => Vector64.Xor(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator ~(Vector64<T> vector) => Vector64.OnesComplement(vector);
  }
}

#endif