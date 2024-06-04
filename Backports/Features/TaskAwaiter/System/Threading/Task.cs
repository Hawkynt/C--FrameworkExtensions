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

#if !SUPPORTS_TASK_AWAITER && SUPPORTS_ASYNC

using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class TaskPolyfills {

  public static TaskAwaiter GetAwaiter(this Task task) => new(task);
  public static TaskAwaiter<TResult> GetAwaiter<TResult>(this Task<TResult> task) => new(task);
  public static ConfiguredTaskAwaitable ConfigureAwait(this Task task, bool continueOnCapturedContext) => new(task, continueOnCapturedContext);
  public static ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task, bool continueOnCapturedContext) => new(task, continueOnCapturedContext);

}

#endif
