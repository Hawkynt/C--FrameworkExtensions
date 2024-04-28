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

#if !NETSTANDARD

using System;

namespace Microsoft.Office.Interop.Word {
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
}

#endif