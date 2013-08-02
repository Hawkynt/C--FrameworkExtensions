#region (c)2010-2020 Hawkynt
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
namespace System {
  /// <summary>
  /// This is for using colored output in the console window in a thread-safe way.
  /// </summary>
  internal static partial class ConsoleExtensions {
    /// <summary>
    /// Can be locked to make console color transactions like lines with different colored words.
    /// </summary>
    public static readonly object Lock = new object();
    private static ConsoleColor _enForeground = ConsoleColor.Gray;
    private static ConsoleColor _enBackground = ConsoleColor.Black;

    private static ConsoleColor _enForegroundBackup;
    private static ConsoleColor _enBackgroundBackup;

    /// <summary>
    /// Safes the actual console colors to backup.
    /// </summary>
    private static void _voidSafeColors() {
      _enForegroundBackup = Console.ForegroundColor;
      _enBackgroundBackup = Console.BackgroundColor;
    }
    /// <summary>
    /// Loads the actual console colors from backup.
    /// </summary>
    private static void _voidLoadColors() {
      Console.ForegroundColor = _enForegroundBackup;
      Console.BackgroundColor = _enBackgroundBackup;
    }
    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <param name="strText">The data.</param>
    public static void Write<T>(T varData) {
      Write(varData, _enForeground, _enBackground);
    }
    /// <summary>
    /// Writes the line of data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="varData">The data.</param>
    public static void WriteLine<T>(T varData) {
      WriteLine(varData, _enForeground, _enBackground);
    }
    /// <summary>
    /// Writes a new line.
    /// </summary>
    public static void WriteLine() {
      WriteLine("", _enForeground, _enBackground);
    }
    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="varData">The data.</param>
    /// <param name="enForeground">The foreground color used to write that piece.</param>
    public static void Write<T>(T varData, ConsoleColor enForeground) {
      Write(varData, enForeground, _enBackground);
    }
    /// <summary>
    /// Writes the line of data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="varData">The data.</param>
    /// <param name="enForeground">The foreground color used to write that piece.</param>
    public static void WriteLine<T>(T varData, ConsoleColor enForeground) {
      WriteLine(varData, enForeground, _enBackground);
    }
    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="varData">The data.</param>
    /// <param name="enForeground">The foreground color used to write that piece.</param>
    /// <param name="enBackground">The background color used to write that piece.</param>
    public static void Write<T>(T varData, ConsoleColor enForeground, ConsoleColor enBackground) {
      lock (Lock) {
        _voidSafeColors();
        Console.ForegroundColor = enForeground;
        Console.BackgroundColor = enBackground;
        Console.Write(varData);
        _voidLoadColors();
      }
    }
    /// <summary>
    /// Writes the line of data.
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    /// <param name="varData">The data.</param>
    /// <param name="enForeground">The foreground color used to write that piece.</param>
    /// <param name="enBackground">The background color used to write that piece.</param>
    public static void WriteLine<T>(T varData, ConsoleColor enForeground, ConsoleColor enBackground) {
      lock (Lock) {
        _voidSafeColors();
        Console.ForegroundColor = enForeground;
        Console.BackgroundColor = enBackground;
        Console.WriteLine(varData);
        _voidLoadColors();
      }
    }
    /// <summary>
    /// Escapes the data to be displayed.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="varData">The data.</param>
    /// <returns>An escaped string version of the data.</returns>
    public static string EscapeAdv<T>(T varData) {
      return (varData.ToString().Replace("\\", "\\\\").Replace("{", "\\{"));
    }
    /// <summary>
    /// Advance write text line.
    /// NOTE: Every { and \ in the format string must be escaped with a leading \
    /// NOTE: Every {f} switches the foreground to that color
    /// NOTE: Every {f,b} switches the foreground and the background color
    /// NOTE: Every {,b} switches the background to that color
    /// NOTE: Every {} resets to default
    /// </summary>
    /// <param name="strFormat">The format string.</param>
    public static void WriteLineAdv(string strFormat) {
      lock (Lock) {
        WriteAdv(strFormat);
        WriteLine();
      }
    }
    /// <summary>
    /// Advance write text.
    /// NOTE: Every { and \ in the format string must be escaped with a leading \
    /// NOTE: Every {f} switches the foreground to that color
    /// NOTE: Every {f,b} switches the foreground and the background color
    /// NOTE: Every {,b} switches the background to that color
    /// NOTE: Every {} resets to default
    /// </summary>
    /// <param name="strFormat">The format string.</param>
    public static void WriteAdv(string strFormat) {
      if (strFormat != null) {
        bool boolIsEscaping = false;
        int intLen = strFormat.Length;
        lock (Lock) {
          ConsoleColor enF = _enForeground;
          ConsoleColor enB = _enBackground;
          for (int intI = 0; intI < intLen; intI++) {
            char chrCur = strFormat[intI];
            if (boolIsEscaping) {
              // current char is escpaped
              Console.Write(chrCur);
              boolIsEscaping = false;
            } else {
              if (chrCur == '\\') {
                // found escape char
                boolIsEscaping = true;
              } else if (chrCur == '{') {
                // color indicator found
                string strColor = "";
                bool boolEnd = false;
                while (intI < intLen && !boolEnd) {
                  if (strFormat[++intI] == '}')
                    boolEnd = true;
                  else
                    strColor += strFormat[intI];
                }
                string[] arrCols;
                if (!string.IsNullOrWhiteSpace(strColor))
                  arrCols = strColor.Split(new char[] { ',' }, 2);
                else
                  arrCols = null;
                if (arrCols == null || arrCols.Length == 0) {
                  // reset colors
                  Console.ForegroundColor = enF;
                  Console.BackgroundColor = enB;
                } else if (arrCols.Length == 1) {
                  // set only foreground color
                  if (!string.IsNullOrWhiteSpace(arrCols[0]))
                    Console.ForegroundColor = _enGetColor_ByIDX(byte.Parse(arrCols[0]));
                } else {
                  // set foreground and background color
                  if (!string.IsNullOrWhiteSpace(arrCols[0]))
                    Console.ForegroundColor = _enGetColor_ByIDX(byte.Parse(arrCols[0]));
                  if (!string.IsNullOrWhiteSpace(arrCols[1]))
                    Console.BackgroundColor = _enGetColor_ByIDX(byte.Parse(arrCols[1]));
                }
              } else {
                Console.Write(chrCur);
              }
            }
          } // next
          Console.ForegroundColor = enF;
          Console.BackgroundColor = enB;
        }
      } else {
        // null means null
      }
    }

    private static ConsoleColor _enGetColor_ByIDX(byte byteColor) {
      ConsoleColor enRet;
      switch (byteColor) {
        case 0: {
          enRet = ConsoleColor.Black;
          break;
        }
        case 1: {
          enRet = ConsoleColor.DarkBlue;
          break;
        }
        case 2: {
          enRet = ConsoleColor.DarkGreen;
          break;
        }
        case 3: {
          enRet = ConsoleColor.DarkCyan;
          break;
        }
        case 4: {
          enRet = ConsoleColor.DarkRed;
          break;
        }
        case 5: {
          enRet = ConsoleColor.DarkMagenta;
          break;
        }
        case 6: {
          enRet = ConsoleColor.DarkYellow;
          break;
        }
        case 7: {
          enRet = ConsoleColor.Gray;
          break;
        }
        case 8: {
          enRet = ConsoleColor.DarkGray;
          break;
        }
        case 9: {
          enRet = ConsoleColor.Blue;
          break;
        }
        case 10: {
          enRet = ConsoleColor.Green;
          break;
        }
        case 11: {
          enRet = ConsoleColor.Cyan;
          break;
        }
        case 12: {
          enRet = ConsoleColor.Red;
          break;
        }
        case 13: {
          enRet = ConsoleColor.Magenta;
          break;
        }
        case 14: {
          enRet = ConsoleColor.Yellow;
          break;
        }
        case 15: {
          enRet = ConsoleColor.White;
          break;
        }
        default: {
          throw new ArgumentOutOfRangeException("Color index must be between 0 and 15 inclusive.");
        }
      }
      return (enRet);
    }
    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    /// <value>The color to use.</value>
    public static ConsoleColor Foreground {
      get {
        return (_enForeground);
      }
      set {
        lock (Lock)
          _enForeground = value;
      }
    }
    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    /// <value>The color to use.</value>
    public static ConsoleColor Background {
      get {
        return (_enBackground);
      }
      set {
        lock (Lock)
          _enBackground = value;
      }
    }
  }
}
