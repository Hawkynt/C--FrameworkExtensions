﻿#region (c)2010-2042 Hawkynt

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

#if DEBUG && !PLATFORM_X86
using System.Runtime.InteropServices;
#endif

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

static partial class ArrayExtensions {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _SequenceUnsafe(byte[] source, int sourceOffset, byte[] target, int targetOffset, int count) {
    fixed (byte* sourceFixedPointer = &source[sourceOffset])
    fixed (byte* targetFixedPointer = &target[targetOffset]) {
      var sourcePointer = sourceFixedPointer;
      var targetPointer = targetFixedPointer;

      const int THRESHOLD = 4;

      {
        // try 2048-Bit
        var localCount = count >> 8;
        if (localCount >= THRESHOLD) {
          var result = _SequenceEqual256Bytewise(ref sourcePointer, ref targetPointer, localCount);
          if (!result)
            return false;

          count &= 0b11111111;
          if (count == 0)
            return true;
        }
      }

#if !PLATFORM_X86
      {
        // try 512-Bit
        var localCount = count >> 6;
        if (localCount >= THRESHOLD) {
          var result = _SequenceEqual64Bytewise(ref sourcePointer, ref targetPointer, localCount);
          if (!result)
            return false;

          count &= 0b111111;
          if (count == 0)
            return true;
        }
      }
#endif

      {
        // try 256-Bit
        var localCount = count >> 5;
        if (localCount >= THRESHOLD) {
          var result = _SequenceEqual32Bytewise(ref sourcePointer, ref targetPointer, localCount);
          if (!result)
            return false;

          count &= 0b11111;
          if (count == 0)
            return true;
        }
      }

#if !PLATFORM_X86
      {
        // try 64-Bit
        var localCount = count >> 3;
        if (localCount >= THRESHOLD) {
          var result = _SequenceEqual8Bytewise(ref sourcePointer, ref targetPointer, localCount);
          if (!result)
            return false;

          count &= 0b111;
          if (count == 0)
            return true;
        }
      }
#endif

      {
        // try 32-Bit
        var localCount = count >> 2;
        if (localCount >= THRESHOLD) {
          var result = _SequenceEqual4Bytewise(ref sourcePointer, ref targetPointer, localCount);
          if (!result)
            return false;

          count &= 0b11;
          if (count == 0)
            return true;
        }
      }

      if (count > 0)
        return _SequenceEqualBytewise(ref sourcePointer, ref targetPointer, count);

      return true;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _SequenceEqualBytewise(ref byte* source, ref byte* target, int count) {
    while (count > 0) {
      if (*source != *target)
        return false;

      ++source;
      ++target;
      --count;
    }

    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _SequenceEqual4Bytewise(ref byte* s, ref byte* t, int count) {
    var source = (uint*)s;
    var target = (uint*)t;
    while (count > 0) {
      if (*source != *target)
        return false;

      ++source;
      ++target;
      --count;
    }

    s = (byte*)source;
    t = (byte*)target;
    return true;
  }

#if !PLATFORM_X86

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _SequenceEqual8Bytewise(ref byte* s, ref byte* t, int count) {
    var source = (ulong*)s;
    var target = (ulong*)t;

    while (count > 0) {
      if (*source != *target)
        return false;

      ++source;
      ++target;
      --count;
    }

    s = (byte*)source;
    t = (byte*)target;
    return true;
  }

#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _SequenceEqual32Bytewise(ref byte* s, ref byte* t, int count) {
    var source = (Block32*)s;
    var target = (Block32*)t;

    while (count > 0) {
      if (
        (*source).a != (*target).a
        || (*source).b != (*target).b
        || (*source).c != (*target).c
        || (*source).d != (*target).d
        || (*source).e != (*target).e
        || (*source).f != (*target).f
        || (*source).g != (*target).g
        || (*source).h != (*target).h
      )
        return false;

      ++source;
      ++target;
      --count;
    }

    s = (byte*)source;
    t = (byte*)target;
    return true;
  }

#if !PLATFORM_X86

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _SequenceEqual64Bytewise(ref byte* s, ref byte* t, int count) {
    var source = (Block64*)s;
    var target = (Block64*)t;

    while (count > 0) {
      if (
        (*source).a != (*target).a
        || (*source).b != (*target).b
        || (*source).c != (*target).c
        || (*source).d != (*target).d
        || (*source).e != (*target).e
        || (*source).f != (*target).f
        || (*source).g != (*target).g
        || (*source).h != (*target).h
      )
        return false;

      ++source;
      ++target;
      --count;
    }

    s = (byte*)source;
    t = (byte*)target;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _SequenceEqual256Bytewise(ref byte* s, ref byte* t, int count) {
    var source = (Block64*)s;
    var target = (Block64*)t;

    while (count > 0) {
      if (
        (*source).a != (*target).a
        || (*source).b != (*target).b
        || (*source).c != (*target).c
        || (*source).d != (*target).d
        || (*source).e != (*target).e
        || (*source).f != (*target).f
        || (*source).g != (*target).g
        || (*source).h != (*target).h
        || source[1].a != target[1].a
        || source[1].b != target[1].b
        || source[1].c != target[1].c
        || source[1].d != target[1].d
        || source[1].e != target[1].e
        || source[1].f != target[1].f
        || source[1].g != target[1].g
        || source[1].h != target[1].h
        || source[2].a != target[2].a
        || source[2].b != target[2].b
        || source[2].c != target[2].c
        || source[2].d != target[2].d
        || source[2].e != target[2].e
        || source[2].f != target[2].f
        || source[2].g != target[2].g
        || source[2].h != target[2].h
        || source[3].a != target[3].a
        || source[3].b != target[3].b
        || source[3].c != target[3].c
        || source[3].d != target[3].d
        || source[3].e != target[3].e
        || source[3].f != target[3].f
        || source[3].g != target[3].g
        || source[3].h != target[3].h
      )
        return false;

      source += 4;
      target += 4;
      --count;
    }

    s = (byte*)source;
    t = (byte*)target;
    return true;
  }

#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool _SequenceEqual256Bytewise(ref byte* s, ref byte* t, int count) {
      var source = (Block32*)s;
      var target = (Block32*)t;

      while (count > 0) {
        if (
          (*source).a != (*target).a
          || (*source).b != (*target).b
          || (*source).c != (*target).c
          || (*source).d != (*target).d
          || (*source).e != (*target).e
          || (*source).f != (*target).f
          || (*source).g != (*target).g
          || (*source).h != (*target).h

          || source[1].a != target[1].a
          || source[1].b != target[1].b
          || source[1].c != target[1].c
          || source[1].d != target[1].d
          || source[1].e != target[1].e
          || source[1].f != target[1].f
          || source[1].g != target[1].g
          || source[1].h != target[1].h

          || source[2].a != target[2].a
          || source[2].b != target[2].b
          || source[2].c != target[2].c
          || source[2].d != target[2].d
          || source[2].e != target[2].e
          || source[2].f != target[2].f
          || source[2].g != target[2].g
          || source[2].h != target[2].h

          || source[3].a != target[3].a
          || source[3].b != target[3].b
          || source[3].c != target[3].c
          || source[3].d != target[3].d
          || source[3].e != target[3].e
          || source[3].f != target[3].f
          || source[3].g != target[3].g
          || source[3].h != target[3].h

          || source[4].a != target[4].a
          || source[4].b != target[4].b
          || source[4].c != target[4].c
          || source[4].d != target[4].d
          || source[4].e != target[4].e
          || source[4].f != target[4].f
          || source[4].g != target[4].g
          || source[4].h != target[4].h

          || source[5].a != target[5].a
          || source[5].b != target[5].b
          || source[5].c != target[5].c
          || source[5].d != target[5].d
          || source[5].e != target[5].e
          || source[5].f != target[5].f
          || source[5].g != target[5].g
          || source[5].h != target[5].h

          || source[6].a != target[6].a
          || source[6].b != target[6].b
          || source[6].c != target[6].c
          || source[6].d != target[6].d
          || source[6].e != target[6].e
          || source[6].f != target[6].f
          || source[6].g != target[6].g
          || source[6].h != target[6].h

          || source[7].a != target[7].a
          || source[7].b != target[7].b
          || source[7].c != target[7].c
          || source[7].d != target[7].d
          || source[7].e != target[7].e
          || source[7].f != target[7].f
          || source[7].g != target[7].g
          || source[7].h != target[7].h
        )
          return false;

        source += 8;
        target += 8;
        --count;
      }

      s = (byte*)source;
      t = (byte*)target;
      return true;
    }

#endif

  private static bool _SequenceEqual(byte[] source, int sourceOffset, byte[] target, int targetOffset, int count)
    => _SequenceUnsafe(source, sourceOffset, target, targetOffset, count);

  public static bool SequenceEqual(this byte[] source, int sourceOffset, byte[] target, int targetOffset, int count) {
    if (ReferenceEquals(source, target) && sourceOffset == targetOffset)
      return true;
    if (source == null || target == null)
      return false;

    var sourceLeft = source.Length - sourceOffset;
    var targetLeft = target.Length - targetOffset;
    if (sourceLeft < count)
      throw new ArgumentOutOfRangeException("Source has too few bytes left");

    if (targetLeft < count)
      throw new ArgumentOutOfRangeException("Target has too few bytes left");

    return _SequenceEqual(source, sourceOffset, target, targetOffset, sourceLeft);
  }

  public static bool SequenceEqual(this byte[] source, int sourceOffset, byte[] target, int targetOffset) {
    if (ReferenceEquals(source, target) && sourceOffset == targetOffset)
      return true;
    if (source == null || target == null)
      return false;

    var sourceLeft = source.Length - sourceOffset;
    var targetLeft = target.Length - targetOffset;
    if (sourceLeft != targetLeft)
      return false;

    return _SequenceEqual(source, sourceOffset, target, targetOffset, sourceLeft);
  }

  public static bool SequenceEqual(this byte[] source, byte[] target) {
    if (ReferenceEquals(source, target))
      return true;
    if (source == null || target == null)
      return false;

    var sourceLeft = source.Length;
    var targetLeft = target.Length;
    if (sourceLeft != targetLeft)
      return false;

    if (sourceLeft == 0)
      return true;

    return _SequenceEqual(source, 0, target, 0, sourceLeft);
  }
}
