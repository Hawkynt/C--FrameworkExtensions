#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_TASK_AWAITER && SUPPORTS_ASYNC
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

public static partial class TaskPolyfills {

  public static TaskAwaiter GetAwaiter(this Task task) => new(task);
  public static TaskAwaiter<TResult> GetAwaiter<TResult>(this Task<TResult> task) => new(task);
  public static ConfiguredTaskAwaitable ConfigureAwait(this Task task, bool continueOnCapturedContext) => new(task, continueOnCapturedContext);
  public static ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task, bool continueOnCapturedContext) => new(task, continueOnCapturedContext);

}

#endif
