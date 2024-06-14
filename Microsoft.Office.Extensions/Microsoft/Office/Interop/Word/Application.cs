#region (c)2010-2042 Hawkynt

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

#if !NETSTANDARD

using System;

namespace Microsoft.Office.Interop.Word;

public class PrintToken : IDisposable {
  private readonly string _currentPrinter;
  private readonly _Application _app;

  public PrintToken(_Application app, string printer) {
    this._currentPrinter = app.ActivePrinter;
    app.ActivePrinter = printer;
    this._app = app;
  }

  public void Dispose() => this._app.ActivePrinter = this._currentPrinter;
}

public static partial class ApplicationExtensions {
  public static PrintToken CreatePrintToken(this Application @this, string printer) => new(@this, printer);
}

#endif
