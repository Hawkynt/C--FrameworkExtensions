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

using TCO = System.Threading.Tasks.TaskCreationOptions;

namespace Utilities;

internal static class TaskCreationOptions {
  public const TCO None = TCO.None;
  public const TCO PreferFairness = TCO.PreferFairness;
  public const TCO LongRunning = TCO.LongRunning;
  public const TCO AttachedToParent = TCO.AttachedToParent;

#if SUPPORTS_TASKCREATIONOPTIONS_DENYCHILDATTACH
  public const TCO DenyChildAttach = TCO.DenyChildAttach;
#else
  public const TCO DenyChildAttach = (TCO)8;
#endif

#if SUPPORTS_TASKCREATIONOPTIONS_HIDESCHEDULER
  public const TCO HideScheduler = TCO.HideScheduler;
#else
  public const TCO HideScheduler = (TCO)16;
#endif

#if SUPPORTS_TASKCREATIONOPTIONS_RUNCONTINUATIONSASYNCHRONOUSLY
  public const TCO RunContinuationsAsynchronously = TCO.RunContinuationsAsynchronously;
#else
  public const TCO RunContinuationsAsynchronously = (TCO)64;
#endif

}
