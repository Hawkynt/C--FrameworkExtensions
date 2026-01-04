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

using MIO = System.Runtime.CompilerServices.MethodImplOptions;

namespace Utilities;

internal static class MethodImplOptions {
  public const MIO Unmanaged = MIO.Unmanaged;
  public const MIO ForwardRef = MIO.ForwardRef;
  public const MIO PreserveSig = MIO.PreserveSig;
  public const MIO InternalCall = MIO.InternalCall;
  public const MIO Synchronized = MIO.Synchronized;
  public const MIO NoInlining = MIO.NoInlining;
  public const MIO NoOptimization = MIO.NoOptimization;

#if SUPPORTS_INLINING
  public const MIO AggressiveInlining = MIO.AggressiveInlining;
#else
  public const MIO AggressiveInlining = (MIO)256;
#endif

#if SUPPORTS_OPTIMIZATION
  public const MIO AggressiveOptimization = MIO.AggressiveOptimization;
#else
  public const MIO AggressiveOptimization = (MIO)512;
#endif

}
