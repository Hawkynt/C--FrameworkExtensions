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

#if !SUPPORTS_VECTOR_128_FULL && !SUPPORTS_VECTOR_128_TYPE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

#if !SUPPORTS_VECTOR_128_BASE
public static partial class Vector128 {
#else
public static partial class Vector128Polyfills {
  extension(Vector128) {
    private static void SkipInit<T>(out T result) => result = default;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<T> Load<T>(T* source) where T : struct
      => Unsafe.ReadUnaligned<Vector128<T>>(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<T> LoadUnsafe<T>(ref T source) where T : struct
      => Unsafe.ReadUnaligned<Vector128<T>>(ref Unsafe.As<T, byte>(ref source));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Store<T>(Vector128<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreUnsafe<T>(Vector128<T> source, ref T destination) where T : struct
      => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAligned<T>(Vector128<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAlignedNonTemporal<T>(Vector128<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7, byte e8, byte e9, byte e10, byte e11, byte e12, byte e13, byte e14, byte e15) {
      SkipInit(out Vector128<byte> result);
      // Assuming WithElement exists (it does in our Base, and in BCL)
      // But in BCL 3.1, WithElement might not exist?
      // If not, we need Unsafe fallback like Vector64 above.
      // But let's assume our Base defines it, and BCL usually has it or we assume 7.0+ compat or this polyfill is for 3.1 which HAS Vector128 but maybe lacks these?
      // Actually, Vector128.WithElement was added in .NET Core 3.0? No, .NET 7.
      // So for #else (BCL case), we CANNOT use WithElement.
      // We MUST use Unsafe.
      
      // For simplicity, I'll use Unsafe for everything in the polyfill path if possible, 
      // or specific ifdefs.
      // Since I can't easily ifdef inside method, I'll use Unsafe everywhere here.
      
      unsafe {
          byte* ptr = stackalloc byte[16];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          ptr[8]=e8; ptr[9]=e9; ptr[10]=e10; ptr[11]=e11; ptr[12]=e12; ptr[13]=e13; ptr[14]=e14; ptr[15]=e15;
          return Unsafe.ReadUnaligned<Vector128<byte>>(ptr);
      }
    }

    // ... Other Create overloads ...
    // I will implement them using stackalloc/Unsafe to be safe across both Base and BCL scenarios.
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Create(float e0, float e1, float e2, float e3) {
      unsafe {
          float* ptr = stackalloc float[4];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3;
          return Unsafe.ReadUnaligned<Vector128<float>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Create(double e0, double e1) {
      unsafe {
          double* ptr = stackalloc double[2];
          ptr[0]=e0; ptr[1]=e1;
          return Unsafe.ReadUnaligned<Vector128<double>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> Create(long e0, long e1) {
      unsafe {
          long* ptr = stackalloc long[2];
          ptr[0]=e0; ptr[1]=e1;
          return Unsafe.ReadUnaligned<Vector128<long>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> Create(ulong e0, ulong e1) {
      unsafe {
          ulong* ptr = stackalloc ulong[2];
          ptr[0]=e0; ptr[1]=e1;
          return Unsafe.ReadUnaligned<Vector128<ulong>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> Create(int e0, int e1, int e2, int e3) {
      unsafe {
          int* ptr = stackalloc int[4];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3;
          return Unsafe.ReadUnaligned<Vector128<int>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> Create(uint e0, uint e1, uint e2, uint e3) {
      unsafe {
          uint* ptr = stackalloc uint[4];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3;
          return Unsafe.ReadUnaligned<Vector128<uint>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> Create(short e0, short e1, short e2, short e3, short e4, short e5, short e6, short e7) {
      unsafe {
          short* ptr = stackalloc short[8];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          return Unsafe.ReadUnaligned<Vector128<short>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> Create(ushort e0, ushort e1, ushort e2, ushort e3, ushort e4, ushort e5, ushort e6, ushort e7) {
      unsafe {
          ushort* ptr = stackalloc ushort[8];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          return Unsafe.ReadUnaligned<Vector128<ushort>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7, sbyte e8, sbyte e9, sbyte e10, sbyte e11, sbyte e12, sbyte e13, sbyte e14, sbyte e15) {
      unsafe {
          sbyte* ptr = stackalloc sbyte[16];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          ptr[8]=e8; ptr[9]=e9; ptr[10]=e10; ptr[11]=e11; ptr[12]=e12; ptr[13]=e13; ptr[14]=e14; ptr[15]=e15;
          return Unsafe.ReadUnaligned<Vector128<sbyte>>(ptr);
      }
    }

#if !SUPPORTS_VECTOR_128_BASE
    // Only define generic CreateScalar if Base is NOT present. 
    // Wait, Base has CreateScalar<T>.
    // If we are here, Base is present (partially defined) OR we are in Polyfill mode (BCL).
    // BCL 3.1 does NOT have CreateScalar.
    // So if SUPPORTS_VECTOR_128_BASE is TRUE (BCL used), we NEED CreateScalar.
    // If SUPPORTS_VECTOR_128_BASE is FALSE (Base used), Base HAS CreateScalar.
    // So we only need it in #else branch.
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> CreateScalar<T>(T value) where T : struct {
      // Use generic Create if possible, or Unsafe.
      // Vector128.Create<T>(T) exists in 3.1.
      // But we want scalar (rest 0).
      // Vector128.Create(T) creates all elements = value.
      // So we need to create 0 and set element 0?
      // Or use Unsafe.
      var result = Vector128<T>.Zero;
      unsafe {
          Unsafe.Write(Unsafe.AsPointer(ref Unsafe.AsRef(in result)), value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> CreateScalarUnsafe<T>(T value) where T : struct {
       // Same as CreateScalar for fallback
       return CreateScalar(value);
    }
#endif

    // Only add these methods when BCL has Vector128 but lacks these operations
    #if SUPPORTS_VECTOR_128_BASE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Create<T>(T value) where T : struct {
      var count = 16 / Unsafe.SizeOf<T>();
      unsafe {
        var buffer = stackalloc byte[16];
        var ptr = (T*)buffer;
        for (var i = 0; i < count; ++i)
          ptr[i] = value;
        return Unsafe.ReadUnaligned<Vector128<T>>(buffer);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Add<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Add(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Subtract<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Subtract(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Multiply<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Divide<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Divide(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Negate<T>(Vector128<T> vector) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Negate(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Abs<T>(Vector128<T> vector) where T : struct {
       if (Scalar<T>.IsUnsigned)
         return vector;

       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Abs(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Sqrt<T>(Vector128<T> vector) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Sqrt(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(Vector128<T> vector) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Scalar<T>.Zero();
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));

       for(int i=0; i<count; i++)
           result = Scalar<T>.Add(result, Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Scalar<T>.Zero();
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));

       for(int i=0; i<count; i++) {
           var product = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           result = Scalar<T>.Add(result, product);
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> BitwiseAnd<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
       var res = (leftBits.Item1 & rightBits.Item1, leftBits.Item2 & rightBits.Item2);
       return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> BitwiseOr<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
       var res = (leftBits.Item1 | rightBits.Item1, leftBits.Item2 | rightBits.Item2);
       return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Xor<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
       var res = (leftBits.Item1 ^ rightBits.Item1, leftBits.Item2 ^ rightBits.Item2);
       return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> OnesComplement<T>(Vector128<T> vector) where T : struct {
       var bits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref vector);
       var res = (~bits.Item1, ~bits.Item2);
       return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> AndNot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
       var res = (leftBits.Item1 & ~rightBits.Item1, leftBits.Item2 & ~rightBits.Item2);
       return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Equals<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var eq = Scalar<T>.ObjectEquals(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = eq ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GreaterThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var gt = Scalar<T>.GreaterThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = gt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GreaterThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var ge = Scalar<T>.GreaterThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = ge ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> LessThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var lt = Scalar<T>.LessThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = lt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> LessThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var l = Unsafe.Add(ref rLeft, i);
           var r = Unsafe.Add(ref rRight, i);
           var le = Scalar<T>.LessThanOrEqual(l, r);
           Unsafe.Add(ref rRes, i) = le ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Min<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Min(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Max<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Max(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Floor<T>(Vector128<T> vector) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Floor(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Ceiling<T>(Vector128<T> vector) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Ceiling(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetElement<T>(Vector128<T> vector, int index) where T : struct {
      if ((uint)index >= 16 / Unsafe.SizeOf<T>())
        throw new ArgumentOutOfRangeException(nameof(index));

      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      return Unsafe.Add(ref rVector, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> WithElement<T>(Vector128<T> vector, int index, T value) where T : struct {
      if ((uint)index >= 16 / Unsafe.SizeOf<T>())
        throw new ArgumentOutOfRangeException(nameof(index));

      var result = vector;
      ref var rResult = ref Unsafe.As<Vector128<T>, T>(ref result);
      Unsafe.Add(ref rResult, index) = value;
      return result;
    }

    #endif

    // GreaterThanOrEqual and LessThanOrEqual: only when we define Vector128 ourselves
    #if !SUPPORTS_VECTOR_128_BASE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GreaterThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var ge = Scalar<T>.GreaterThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = ge ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> LessThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var le = Scalar<T>.LessThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = le ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    #endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Shuffle<T>(Vector128<T> vector, Vector128<T> indices) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rIndices = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in indices));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);
       
       for(int i=0; i<count; i++) {
           var idxVal = Unsafe.Add(ref rIndices, i);
           int idx = Scalar<T>.ToInt32(idxVal) & (count - 1);
           var val = Unsafe.Add(ref rVector, idx);
           Unsafe.Add(ref rRes, i) = val;
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits<T>(Vector128<T> vector) where T : struct {
      uint result = 0;
      int count = 16 / Unsafe.SizeOf<T>();
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      
      for (var i = 0; i < count; ++i) {
        if (Scalar<T>.ExtractMostSignificantBit(Unsafe.Add(ref rVector, i)))
          result |= 1u << i;
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ConditionalSelect<T>(Vector128<T> condition, Vector128<T> left, Vector128<T> right) where T : struct {
      // Fallback to Bitwise
      // If Bitwise ops are available on the type (Base has them, BCL has them)
      // But if BCL 3.1 lacks generic BitwiseOr, we might have trouble?
      // Vector128.BitwiseOr<T> exists in 3.1? 
      // Docs say: BitwiseOr<T> added in .NET 7.
      // So we need to cast to byte/ulong and back or use Unsafe.
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var condBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref condition);
      
      var res = (
         (condBits.Item1 & leftBits.Item1) | (~condBits.Item1 & rightBits.Item1),
         (condBits.Item2 & leftBits.Item2) | (~condBits.Item2 & rightBits.Item2)
      );
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftLeft<T>(Vector128<T> vector, int shiftCount) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);
       
       for(int i=0; i<count; i++) {
           Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftLeft(Unsafe.Add(ref rVector, i), shiftCount);
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftRightArithmetic<T>(Vector128<T> vector, int shiftCount) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);
       
       for(int i=0; i<count; i++) {
           Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftRightArithmetic(Unsafe.Add(ref rVector, i), shiftCount);
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftRightLogical<T>(Vector128<T> vector, int shiftCount) where T : struct {
       int count = 16 / Unsafe.SizeOf<T>();
       var result = Vector128<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);
       
       for(int i=0; i<count; i++) {
           Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftRightLogical(Unsafe.Add(ref rVector, i), shiftCount);
       }
       return result;
    }

    // ===== COMPARISON ALL/ANY METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.ObjectEquals(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    // ===== TOSCALAR (for BCL case) =====

#if SUPPORTS_VECTOR_128_BASE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToScalar<T>(Vector128<T> vector) where T : struct
      => Vector128.GetElement(vector, 0);
#endif

    // ===== COPYSIGN =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> CopySign(Vector128<float> value, Vector128<float> sign) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      for (var i = 0; i < count; ++i) {
        var v = Vector128.GetElement(value, i);
        var s = Vector128.GetElement(sign, i);
        var absV = MathF.Abs(v);
        result = Vector128.WithElement(result, i, s < 0 ? -absV : absV);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> CopySign(Vector128<double> value, Vector128<double> sign) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      for (var i = 0; i < count; ++i) {
        var v = Vector128.GetElement(value, i);
        var s = Vector128.GetElement(sign, i);
        var absV = Math.Abs(v);
        result = Vector128.WithElement(result, i, s < 0 ? -absV : absV);
      }
      return result;
    }

    // ===== CLASSIFICATION METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsNaN(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isNan = float.IsNaN(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, isNan ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsNaN(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var isNan = double.IsNaN(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, isNan ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsNegative(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isNeg = val < 0 || (val == 0 && float.IsNegativeInfinity(1f / val));
        result = Vector128.WithElement(result, i, isNeg ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsNegative(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isNeg = val < 0 || (val == 0 && double.IsNegativeInfinity(1d / val));
        result = Vector128.WithElement(result, i, isNeg ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsPositive(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isPos = val > 0 || (val == 0 && float.IsPositiveInfinity(1f / val));
        result = Vector128.WithElement(result, i, isPos ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsPositive(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isPos = val > 0 || (val == 0 && double.IsPositiveInfinity(1d / val));
        result = Vector128.WithElement(result, i, isPos ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsZero(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isZero = Vector128.GetElement(vector, i) == 0f;
        result = Vector128.WithElement(result, i, isZero ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsZero(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var isZero = Vector128.GetElement(vector, i) == 0d;
        result = Vector128.WithElement(result, i, isZero ? allBitsSet : 0d);
      }
      return result;
    }

    // ===== NARROW METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Narrow(Vector128<double> lower, Vector128<double> upper) {
      var result = Vector128<float>.Zero;
      result = Vector128.WithElement(result, 0, (float)Vector128.GetElement(lower, 0));
      result = Vector128.WithElement(result, 1, (float)Vector128.GetElement(lower, 1));
      result = Vector128.WithElement(result, 2, (float)Vector128.GetElement(upper, 0));
      result = Vector128.WithElement(result, 3, (float)Vector128.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> Narrow(Vector128<long> lower, Vector128<long> upper) {
      var result = Vector128<int>.Zero;
      result = Vector128.WithElement(result, 0, (int)Vector128.GetElement(lower, 0));
      result = Vector128.WithElement(result, 1, (int)Vector128.GetElement(lower, 1));
      result = Vector128.WithElement(result, 2, (int)Vector128.GetElement(upper, 0));
      result = Vector128.WithElement(result, 3, (int)Vector128.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> Narrow(Vector128<ulong> lower, Vector128<ulong> upper) {
      var result = Vector128<uint>.Zero;
      result = Vector128.WithElement(result, 0, (uint)Vector128.GetElement(lower, 0));
      result = Vector128.WithElement(result, 1, (uint)Vector128.GetElement(lower, 1));
      result = Vector128.WithElement(result, 2, (uint)Vector128.GetElement(upper, 0));
      result = Vector128.WithElement(result, 3, (uint)Vector128.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> Narrow(Vector128<int> lower, Vector128<int> upper) {
      var result = Vector128<short>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector128.WithElement(result, i, (short)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 4, (short)Vector128.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> Narrow(Vector128<uint> lower, Vector128<uint> upper) {
      var result = Vector128<ushort>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector128.WithElement(result, i, (ushort)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 4, (ushort)Vector128.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> Narrow(Vector128<short> lower, Vector128<short> upper) {
      var result = Vector128<sbyte>.Zero;
      for (var i = 0; i < 8; ++i) {
        result = Vector128.WithElement(result, i, (sbyte)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 8, (sbyte)Vector128.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Narrow(Vector128<ushort> lower, Vector128<ushort> upper) {
      var result = Vector128<byte>.Zero;
      for (var i = 0; i < 8; ++i) {
        result = Vector128.WithElement(result, i, (byte)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 8, (byte)Vector128.GetElement(upper, i));
      }
      return result;
    }

    // ===== WIDEN METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<double> Lower, Vector128<double> Upper) Widen(Vector128<float> vector) {
      var lower = Vector128<double>.Zero;
      var upper = Vector128<double>.Zero;
      lower = Vector128.WithElement(lower, 0, (double)Vector128.GetElement(vector, 0));
      lower = Vector128.WithElement(lower, 1, (double)Vector128.GetElement(vector, 1));
      upper = Vector128.WithElement(upper, 0, (double)Vector128.GetElement(vector, 2));
      upper = Vector128.WithElement(upper, 1, (double)Vector128.GetElement(vector, 3));
      return (lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<long> Lower, Vector128<long> Upper) Widen(Vector128<int> vector) {
      var lower = Vector128<long>.Zero;
      var upper = Vector128<long>.Zero;
      lower = Vector128.WithElement(lower, 0, (long)Vector128.GetElement(vector, 0));
      lower = Vector128.WithElement(lower, 1, (long)Vector128.GetElement(vector, 1));
      upper = Vector128.WithElement(upper, 0, (long)Vector128.GetElement(vector, 2));
      upper = Vector128.WithElement(upper, 1, (long)Vector128.GetElement(vector, 3));
      return (lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<ulong> Lower, Vector128<ulong> Upper) Widen(Vector128<uint> vector) {
      var lower = Vector128<ulong>.Zero;
      var upper = Vector128<ulong>.Zero;
      lower = Vector128.WithElement(lower, 0, (ulong)Vector128.GetElement(vector, 0));
      lower = Vector128.WithElement(lower, 1, (ulong)Vector128.GetElement(vector, 1));
      upper = Vector128.WithElement(upper, 0, (ulong)Vector128.GetElement(vector, 2));
      upper = Vector128.WithElement(upper, 1, (ulong)Vector128.GetElement(vector, 3));
      return (lower, upper);
    }

    // ===== CONVERTTO METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> ConvertToInt32(Vector128<float> vector) {
      var result = Vector128<int>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (int)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> ConvertToSingle(Vector128<int> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (float)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> ConvertToSingle(Vector128<uint> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (float)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> ConvertToInt64(Vector128<double> vector) {
      var result = Vector128<long>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (long)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> ConvertToDouble(Vector128<long> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (double)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> ConvertToDouble(Vector128<ulong> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (double)Vector128.GetElement(vector, i));
      return result;
    }

#if SUPPORTS_VECTOR_128_BASE
  }

  // Extension operators for Vector128<T>
  extension<T>(Vector128<T>) where T : struct {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator +(Vector128<T> left, Vector128<T> right) => Vector128.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator -(Vector128<T> left, Vector128<T> right) => Vector128.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator *(Vector128<T> left, Vector128<T> right) => Vector128.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator /(Vector128<T> left, Vector128<T> right) => Vector128.Divide(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator -(Vector128<T> vector) => Vector128.Negate(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator &(Vector128<T> left, Vector128<T> right) => Vector128.BitwiseAnd(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator |(Vector128<T> left, Vector128<T> right) => Vector128.BitwiseOr(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator ^(Vector128<T> left, Vector128<T> right) => Vector128.Xor(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator ~(Vector128<T> vector) => Vector128.OnesComplement(vector);
  }
}
#else
}
#endif

#endif