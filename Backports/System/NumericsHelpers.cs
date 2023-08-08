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
namespace System.Numerics;

#if SUPPORTS_INLINING
using Runtime.CompilerServices;
#endif

internal static class NumericsHelpers {

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static uint CombineHash(uint u1, uint u2) => (uint)HashCode.Combine(u1, u2);
#else
  public static uint CombineHash(uint u1, uint u2) => ((u1 << 7) | (u1 >> 25)) ^ (u2 * 257);
#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static int CombineHash(int n1, int n2) => HashCode.Combine(n1, n2);
#else
  public static int CombineHash(int n1, int n2) => (int)CombineHash((uint)n1, (uint)n2);
#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static int CombineHash(int n1, int n2, int n3) => HashCode.Combine(n1, n2, n3);
#else
  public static int CombineHash(int n1, int n2, int n3) => CombineHash(CombineHash(n1, n2), n3);
#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static int CombineHash(int n1, int n2, int n3, int n4) => HashCode.Combine(n1, n2, n3, n4);
#else
  public static int CombineHash(int n1, int n2, int n3, int n4) => CombineHash(CombineHash(n1, n2), CombineHash(n3, n4));
#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5) => HashCode.Combine(n1, n2, n3, n4, n5);
#else
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5) => CombineHash(CombineHash(n1, n2), CombineHash(n3, n4), n5);
#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6) => HashCode.Combine(n1, n2, n3, n4, n5, n6);
#else
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6) => CombineHash(CombineHash(n1, n2, n3), CombineHash(n4, n5, n6));
#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6, int n7) => HashCode.Combine(n1, n2, n3, n4, n5, n6, n7);
#else
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6, int n7) => CombineHash(CombineHash(n1, n2, n3), CombineHash(n4, n5, n6), n7);
#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_SYSTEM_HASHCODE
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6, int n7, int n8) => HashCode.Combine(n1, n2, n3, n4, n5, n6, n7, n8);
#else
  public static int CombineHash(int n1, int n2, int n3, int n4, int n5, int n6, int n7, int n8) => CombineHash(CombineHash(n1, n2, n3, n4), CombineHash(n4, n5, n6, n7, n8));
#endif  

}
