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
  public static PrintToken CreatePrintToken(this Application @this, string printer) => new PrintToken(@this, printer);
}

#endif
