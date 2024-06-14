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

using System.Drawing;

namespace System.Windows.Controls;

public static partial class TextBoxExtensions {
  /// <summary>
  ///   Can be used to temporarely move the caret somewhere and restore the position afterwards.
  /// </summary>
  public interface ICaretPositionToken : IDisposable {
    /// <summary>
    ///   Gets the stored position.
    /// </summary>
    /// <value>
    ///   The position.
    /// </value>
    Point Position { get; }
  }
}
