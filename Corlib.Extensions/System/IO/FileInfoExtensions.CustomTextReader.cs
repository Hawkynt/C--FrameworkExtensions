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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class FileInfoExtensions {
  private class CustomTextReader {
    private readonly bool _detectEncodingFromByteOrderMark;
    private readonly Encoding _encoding;
    private readonly StringExtensions.LineBreakMode _lineBreakMode;

    private CustomTextReader(bool detectEncodingFromByteOrderMark, Encoding encoding, StringExtensions.LineBreakMode lineBreakMode) {
      this._detectEncodingFromByteOrderMark = detectEncodingFromByteOrderMark;
      this._encoding = encoding;
      this._lineBreakMode = lineBreakMode;
    }

    public sealed class Initialized : CustomTextReader {
      private sealed class LineEndingNode {
        private Dictionary<char, LineEndingNode> _children;
        private LineEndingNode(int pathLength) => this.PathLength = pathLength;

        public bool IsTerminal { get; private set; }
        public int PathLength { get; }

        public int Depth {
          get {
            if (this._children == null)
              return 0;

            return this._children.Values.Select(n => n.Depth).Max() + 1;
          }
        }

        public LineEndingNode GetNodeOrNull(char current) => this._children?.TryGetValue(current, out var result) ?? false ? result : null;

        private static void _UpdateTree(LineEndingNode root, string lineEnding) {
          var current = root;
          foreach (var character in lineEnding) {
            current._children ??= new();
            var parentNode = current;
            current = current._children.GetOrAdd(character, () => new(parentNode.PathLength + 1));
          }

          current.IsTerminal = true;
        }

        public static LineEndingNode BuildTree(string lineEnding) {
          LineEndingNode root = new(0);
          _UpdateTree(root, lineEnding);
          return root;
        }

        public static LineEndingNode BuildTree(params string[] lineEndings) {
          LineEndingNode root = new(0);
          foreach (var lineEnding in lineEndings)
            _UpdateTree(root, lineEnding);

          return root;
        }
      }

      private readonly Func<int> _singleCharacterReader;
      private readonly Func<string> _fullLineReader;

      private Initialized(Stream stream, bool detectEncodingFromByteOrderMark, Encoding encoding, StringExtensions.LineBreakMode lineBreakMode) : base(detectEncodingFromByteOrderMark, encoding, lineBreakMode) {
        long startPosition;

        SaveStartPosition();

        var usedEncoding = this._encoding;
        if (this._detectEncodingFromByteOrderMark) {
          usedEncoding = DetectFromByteOrderMark(stream);
          Reset();
        }

        usedEncoding ??= Encoding.GetEncoding("ISO-8859-1");
        var decoder = usedEncoding.GetDecoder();
        var largestBufferForOneCharacter = new byte[usedEncoding.GetMaxByteCount(1)];
        var oneCharacterOnly = new char[1];
        this._singleCharacterReader = ReadOneChar;

        if (!HasValidPreamble(stream, usedEncoding))
          Reset();

        SaveStartPosition();
        this.PreambleSize = startPosition;

        var possibleLineEndings = BuildLineEndingStateMachine(this, this._lineBreakMode);
        if (this._lineBreakMode == StringExtensions.LineBreakMode.AutoDetect)
          Reset();

        this._fullLineReader = possibleLineEndings == null
            ? ReadToEnd
            : possibleLineEndings.Depth <= 1
              ? ReadOneLineForOneCharacterTerminals
              : ReadOneLine
          ;

        return;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SaveStartPosition() => startPosition = stream.Position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Reset() => stream.Position = startPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool HasValidPreamble(Stream stream, Encoding usedEncoding) {
          var preamble = usedEncoding.GetPreamble();
          var checkPreamble = preamble.Length > 0;
          if (!checkPreamble)
            return true;

          var buffer = new byte[preamble.Length];
          var read = stream.Read(buffer, 0, buffer.Length);
          return read == preamble.Length && buffer.SequenceEqual(preamble);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static LineEndingNode BuildLineEndingStateMachine(Initialized reader, StringExtensions.LineBreakMode lineBreakMode) {
          for (;;)
            switch (lineBreakMode) {
              case StringExtensions.LineBreakMode.None: return null;
              case StringExtensions.LineBreakMode.AutoDetect:
                lineBreakMode = _DetectLineBreakMode(reader);
                break;
              case StringExtensions.LineBreakMode.All:
                return LineEndingNode.BuildTree(
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.CarriageReturn),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.CrLf),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.FormFeed),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.LineFeed),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.LfCr),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.LineSeparator),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.ParagraphSeparator),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.NextLine),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.NegativeAcknowledge),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.EndOfLine),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.Zx),
                  StringExtensions.GetLineJoiner(StringExtensions.LineJoinMode.Null)
                );
              default:
                if (lineBreakMode >= 0)
                  return LineEndingNode.BuildTree(StringExtensions.GetLineJoiner((StringExtensions.LineJoinMode)lineBreakMode));

                throw new NotImplementedException($"Unknown LineBreakMode: {lineBreakMode}");
            }
        }

        int ReadOneChar() {
          // Attempt to read enough bytes for at least one character
          var bytesRead = stream.Read(largestBufferForOneCharacter, 0, largestBufferForOneCharacter.Length);

          // End of stream
          if (bytesRead <= 0)
            return -1;

          // Use the decoder to decode the bytes into the char buffer
          decoder.Convert(
            largestBufferForOneCharacter,
            0,
            bytesRead,
            oneCharacterOnly,
            0,
            1,
            true,
            out var bytesUsed,
            out var charsUsed,
            out _
          );

          // should never happen, because decoding did fail at this point, assume we just reached EOF mid character
          if (charsUsed <= 0)
            return -1;

          // Successfully decoded at least one character
          var bytesReadPastCharacter = bytesRead - bytesUsed;
          if (bytesReadPastCharacter > 0)
            stream.Position -= bytesReadPastCharacter;

          return oneCharacterOnly[0];
        }

        string ReadToEnd() {
          StringBuilder result = new(1024);
          for (;;) {
            var read = ReadOneChar();
            if (read < 0)
              return result.Length > 0 ? result.ToString() : null;

            result.Append((char)read);
          }
        }

        string ReadOneLineForOneCharacterTerminals() {
          StringBuilder result = new(1024);
          var hadContent = false;
          for (;;) {
            var read = ReadOneChar();

            // End of stream handling
            if (read < 0)
              return hadContent ? result.ToString() : null;

            hadContent = true;
            var currentChar = (char)read;

            // Check if the current character matches any line ending
            var currentNode = possibleLineEndings.GetNodeOrNull(currentChar);
            if (currentNode != null)
              return result.ToString();

            // If it's not a line ending character, append it to the result and continue reading
            result.Append(currentChar);
          }
        }

        string ReadOneLine() {
          StringBuilder result = new(1024);
          var hadContent = false;
          long lastTerminalPosition = -1; // Position of the last terminal node encountered
          LineEndingNode lastTerminalNode = null; // Last terminal node encountered
          var currentNode = possibleLineEndings;

          for (;;) {
            var read = ReadOneChar();

            // End of stream handling
            if (read < 0) {
              // If there was a pending terminal node, rollback to its position
              if (lastTerminalNode != null) {
                stream.Position = lastTerminalPosition;

                // we need to strip parts of the result to not include the current line ending
                result.Remove(result.Length - lastTerminalNode.PathLength, lastTerminalNode.PathLength);
                return result.ToString();
              }

              // Return whatever is in the buffer if it's not empty, or null to indicate the end of the stream
              return hadContent ? result.ToString() : null;
            }

            hadContent = true;
            var currentPosition = stream.Position;
            var currentChar = (char)read;
            result.Append(currentChar);

            currentNode = currentNode.GetNodeOrNull(currentChar);
            if (currentNode == null) {
              // If a terminal node was previously encountered but the current character does not continue a valid line ending
              if (lastTerminalNode != null) {
                stream.Position = lastTerminalPosition;

                // we need to strip parts of the result to not include the current line ending
                result.Remove(result.Length - lastTerminalNode.PathLength - 1 /* because the last added char is not part of the tree */, lastTerminalNode.PathLength);
                return result.ToString();
              }

              currentNode = possibleLineEndings;
              continue;
            }

            if (!currentNode.IsTerminal)
              continue;

            lastTerminalNode = currentNode;
            lastTerminalPosition = currentPosition;
          }
        }
      }

      public Initialized(Stream stream, bool detectEncodingFromByteOrderMark, StringExtensions.LineBreakMode lineBreakMode) : this(stream, detectEncodingFromByteOrderMark, null, lineBreakMode) { }

      public Initialized(Stream stream, Encoding encoding, StringExtensions.LineBreakMode lineBreakMode = StringExtensions.LineBreakMode.AutoDetect)
        : this(stream, false, encoding, lineBreakMode)
        => Against.ArgumentIsNull(encoding);

      public long PreambleSize { get; }

      public int Read() => this._singleCharacterReader();

      public string ReadLine() => this._fullLineReader();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Encoding DetectFromByteOrderMark(Stream stream) {
      var buffer = new byte[4];
      return stream.Read(buffer, 0, buffer.Length) switch {
        // UTF-32, big-endian
        >= 4 when buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE && buffer[3] == 0xFF => new UTF32Encoding(bigEndian: true, byteOrderMark: true),
        // UTF-32, little-endian
        >= 4 when buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00 => new UTF32Encoding(bigEndian: false, byteOrderMark: true),
        // UTF-8
        >= 3 when buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF => Encoding.UTF8,
        // UTF-7
#pragma warning disable SYSLIB0001
        >= 3 when buffer[0] == 0x2B && buffer[1] == 0x2F && buffer[2] == 0x76 => Encoding.UTF7,
#pragma warning restore SYSLIB0001
        // UTF-16, big-endian
        >= 2 when buffer[0] == 0xFE && buffer[1] == 0xFF => new UnicodeEncoding(bigEndian: true, byteOrderMark: true),
        // UTF-16, little-endian
        >= 2 when buffer[0] == 0xFF && buffer[1] == 0xFE => new UnicodeEncoding(bigEndian: false, byteOrderMark: true),
        _ => null
      };
    }

  }
}
