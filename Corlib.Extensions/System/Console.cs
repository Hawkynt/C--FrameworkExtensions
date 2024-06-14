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

namespace System;

/// <summary>
///   This is for using colored output in the console window in a thread-safe way.
/// </summary>
public static partial class ConsoleExtensions {
  /// <summary>
  ///   Can be locked to make console color transactions like lines with different colored words.
  /// </summary>
  public static readonly object Lock = new();

  private static ConsoleColor _foreground = ConsoleColor.Gray;
  private static ConsoleColor _background = ConsoleColor.Black;

  private static ConsoleColor _foregroundBackup;
  private static ConsoleColor _backgroundBackup;

  /// <summary>
  ///   Safes the actual console colors to backup.
  /// </summary>
  private static void _SafeColors() {
    _foregroundBackup = Console.ForegroundColor;
    _backgroundBackup = Console.BackgroundColor;
  }

  /// <summary>
  ///   Loads the actual console colors from backup.
  /// </summary>
  private static void _LoadColors() {
    Console.ForegroundColor = _foregroundBackup;
    Console.BackgroundColor = _backgroundBackup;
  }

  /// <summary>
  ///   Writes the data.
  /// </summary>
  /// <param name="data">The data.</param>
  public static void Write<T>(T data) => Write(data, _foreground, _background);

  /// <summary>
  ///   Writes the line of data.
  /// </summary>
  /// <typeparam name="T">The type of data.</typeparam>
  /// <param name="data">The data.</param>
  public static void WriteLine<T>(T data) => WriteLine(data, _foreground, _background);

  /// <summary>
  ///   Writes a new line.
  /// </summary>
  public static void WriteLine() => WriteLine(string.Empty, _foreground, _background);

  /// <summary>
  ///   Writes the data.
  /// </summary>
  /// <typeparam name="T">The type of data.</typeparam>
  /// <param name="data">The data.</param>
  /// <param name="foreground">The foreground color used to write that piece.</param>
  public static void Write<T>(T data, ConsoleColor foreground) => Write(data, foreground, _background);

  /// <summary>
  ///   Writes the line of data.
  /// </summary>
  /// <typeparam name="T">The type of data.</typeparam>
  /// <param name="data">The data.</param>
  /// <param name="foreground">The foreground color used to write that piece.</param>
  public static void WriteLine<T>(T data, ConsoleColor foreground) => WriteLine(data, foreground, _background);

  /// <summary>
  ///   Writes the data.
  /// </summary>
  /// <typeparam name="T">The type of data.</typeparam>
  /// <param name="data">The data.</param>
  /// <param name="foreground">The foreground color used to write that piece.</param>
  /// <param name="background">The background color used to write that piece.</param>
  public static void Write<T>(T data, ConsoleColor foreground, ConsoleColor background) {
    lock (Lock) {
      _SafeColors();
      Console.ForegroundColor = foreground;
      Console.BackgroundColor = background;
      Console.Write(data);
      _LoadColors();
    }
  }

  /// <summary>
  ///   Writes the line of data.
  /// </summary>
  /// <typeparam name="T">The type of data</typeparam>
  /// <param name="data">The data.</param>
  /// <param name="foreground">The foreground color used to write that piece.</param>
  /// <param name="background">The background color used to write that piece.</param>
  public static void WriteLine<T>(T data, ConsoleColor foreground, ConsoleColor background) {
    lock (Lock) {
      _SafeColors();
      Console.ForegroundColor = foreground;
      Console.BackgroundColor = background;
      Console.WriteLine(data);
      _LoadColors();
    }
  }

  /// <summary>
  ///   Escapes the data to be displayed.
  /// </summary>
  /// <typeparam name="T">The type of data.</typeparam>
  /// <param name="data">The data.</param>
  /// <returns>An escaped string version of the data.</returns>
  public static string EscapeAdv<T>(T data) => data.ToString().Replace("\\", @"\\").Replace("{", "\\{");

  /// <summary>
  ///   Advance write text line.
  ///   NOTE: Every { and \ in the format string must be escaped with a leading \
  ///   NOTE: Every {f} switches the foreground to that color
  ///   NOTE: Every {f,b} switches the foreground and the background color
  ///   NOTE: Every {,b} switches the background to that color
  ///   NOTE: Every {} resets to default
  /// </summary>
  /// <param name="format">The format string.</param>
  public static void WriteLineAdv(string format) {
    lock (Lock) {
      WriteAdv(format);
      Console.WriteLine();
    }
  }

  /// <summary>
  ///   Advance write text line.
  ///   NOTE: Every char between 0x00 and 0x0f changes foreground color
  ///   NOTE: Every char between 0x10 and 0x1f changes background color
  /// </summary>
  /// <param name="format">The format string.</param>
  public static void WriteLineNoSpecials(string format) {
    lock (Lock) {
      WriteNoSpecials(format);
      Console.WriteLine();
    }
  }

  /// <summary>
  ///   Advance write text.
  ///   NOTE: Every { and \ in the format string must be escaped with a leading \
  ///   NOTE: Every {f} switches the foreground to that color
  ///   NOTE: Every {f,b} switches the foreground and the background color
  ///   NOTE: Every {,b} switches the background to that color
  ///   NOTE: Every {} resets to default
  /// </summary>
  /// <param name="format">The format string.</param>
  public static void WriteAdv(string format) {
    if (format == null)
      // null means null
      return;

    var isEscaping = false;
    var length = format.Length;
    lock (Lock) {
      var oldForeground = _foreground;
      var oldBackground = _background;
      for (var intI = 0; intI < length; intI++) {
        var current = format[intI];
        if (isEscaping) {
          // current char is escpaped
          Console.Write(current);
          isEscaping = false;
        } else {
          if (current == '\\') {
            // found escape char
            isEscaping = true;
          } else if (current == '{') {
            // color indicator found
            var colorDefinition = string.Empty;
            var isEnded = false;
            while (intI < length && !isEnded)
              if (format[++intI] == '}')
                isEnded = true;
              else
                colorDefinition += format[intI];
            var colors = colorDefinition.IsNullOrWhiteSpace() ? null : colorDefinition.Split([','], 2);
            if (colors == null || colors.Length == 0) {
              // reset colors
              Console.ForegroundColor = oldForeground;
              Console.BackgroundColor = oldBackground;
            } else if (colors.Length == 1) {
              // set only foreground color
              if (colors[0].IsNotNullOrWhiteSpace())
                Console.ForegroundColor = _GetColorByIndex(byte.Parse(colors[0]));
            } else {
              // set foreground and background color
              if (colors[0].IsNotNullOrWhiteSpace())
                Console.ForegroundColor = _GetColorByIndex(byte.Parse(colors[0]));
              if (colors[1].IsNotNullOrWhiteSpace())
                Console.BackgroundColor = _GetColorByIndex(byte.Parse(colors[1]));
            }
          } else {
            Console.Write(current);
          }
        }
      } // next

      Console.ForegroundColor = oldForeground;
      Console.BackgroundColor = oldBackground;
    }
  }

  /// <summary>
  ///   Advance write text.
  ///   NOTE: Every char between 0x00 and 0x0f changes foreground color
  ///   NOTE: Every char between 0x10 and 0x1f changes background color
  /// </summary>
  /// <param name="format">The format string.</param>
  public static void WriteNoSpecials(string format) {
    if (format == null)
      // null means null
      return;

    var length = format.Length;
    lock (Lock) {
      var oldForeground = _foreground;
      var oldBackground = _background;
      for (var intI = 0; intI < length; intI++) {
        var current = format[intI];
        if (current <= '\x0f')
          Console.ForegroundColor = _GetColorByIndex((byte)current);
        else if (current <= '\x1f')
          Console.BackgroundColor = _GetColorByIndex((byte)(current - 0x10));
        else
          Console.Write(current);
      }

      Console.ForegroundColor = oldForeground;
      Console.BackgroundColor = oldBackground;
    }
  }

  private static readonly ConsoleColor[] _COLORS = [
    ConsoleColor.Black,
    ConsoleColor.DarkBlue,
    ConsoleColor.DarkGreen,
    ConsoleColor.DarkCyan,
    ConsoleColor.DarkRed,
    ConsoleColor.DarkMagenta,
    ConsoleColor.DarkYellow,
    ConsoleColor.Gray,
    ConsoleColor.DarkGray,
    ConsoleColor.Blue,
    ConsoleColor.Green,
    ConsoleColor.Cyan,
    ConsoleColor.Red,
    ConsoleColor.Magenta,
    ConsoleColor.Yellow,
    ConsoleColor.White
  ];

  private static ConsoleColor _GetColorByIndex(byte color) {
    if (color >= _COLORS.Length)
      throw new ArgumentOutOfRangeException("Color index must be between 0 and 15 inclusive.");

    return _COLORS[color];
  }

  /// <summary>
  ///   Gets or sets the foreground color.
  /// </summary>
  /// <value>The color to use.</value>
  public static ConsoleColor Foreground {
    get => _foreground;
    set {
      lock (Lock)
        _foreground = value;
    }
  }

  /// <summary>
  ///   Gets or sets the background color.
  /// </summary>
  /// <value>The color to use.</value>
  public static ConsoleColor Background {
    get => _background;
    set {
      lock (Lock)
        _background = value;
    }
  }
}
