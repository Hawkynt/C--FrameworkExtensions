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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics;

internal static class NoIntrinsicsSupport {
  
  [DoesNotReturn]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DebuggerHidden]
  public static void Throw()=> throw new NotSupportedException("Use dotnet.core for a jitter supporting this command");

  [DoesNotReturn]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DebuggerHidden]
  public static TResult Throw<TResult>() => throw new NotSupportedException("Use dotnet.core for a jitter supporting this command");

}
