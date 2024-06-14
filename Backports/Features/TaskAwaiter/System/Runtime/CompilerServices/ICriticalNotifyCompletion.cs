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
namespace System.Runtime.CompilerServices;

public interface ICriticalNotifyCompletion : INotifyCompletion {
  void UnsafeOnCompleted(Action continuation);
}

#endif
