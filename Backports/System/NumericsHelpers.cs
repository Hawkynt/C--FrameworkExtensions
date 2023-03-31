#if !SUPPORTS_VALUE_TUPLE
namespace System.Numerics;

#if SUPPORTS_INLINING
using Runtime.CompilerServices;
#endif

internal static class NumericsHelpers {

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static uint CombineHash(uint u1, uint u2) => ((u1 << 7) | (u1 >> 25)) ^ u2;
  
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int CombineHash(int n1, int n2) => (int)CombineHash((uint)n1, (uint)n2);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int CombineHash(int n1, int n2, int n3) => CombineHash(CombineHash(n1, n2), n3);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int CombineHash(int n1, int n2, int n3,int n4) => CombineHash(CombineHash(n1, n2), CombineHash(n3, n4));

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5) => CombineHash(CombineHash(n1, n2), CombineHash(n3, n4), n5);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6) => CombineHash(CombineHash(n1, n2, n3), CombineHash(n4, n5, n6));

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6, int n7) => CombineHash(CombineHash(n1, n2, n3), CombineHash(n4, n5, n6), n7);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6, int n7, int n8) => CombineHash(CombineHash(n1, n2, n3, n4), CombineHash(n4, n5, n6, n7, n8));

  // Do an in-place two's complement. "Dangerous" because it causes
  // a mutation and needs to be used with care for immutable types.
  public static void DangerousMakeTwosComplement(uint[] d) {
    if (!(d?.Length > 0)) {
      return;
    }

    d[0] = ~d[0] + 1;

    var i = 1;
    // first do complement and +1 as long as carry is needed
    for (; d[i - 1] == 0 && i < d.Length; i++) {
      ref var current = ref d[i];
      current = ~current + 1;
    }

    // now ones complement is sufficient
    for (; i < d.Length; i++) {
      ref var current = ref d[i];
      current = ~current;
    }
  }
}
#endif