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

namespace System.Security.Cryptography;

public sealed class Fletcher : HashAlgorithm, IAdvancedHashAlgorithm {
  public Fletcher() : this(MaxOutputBits) { }

  public Fletcher(int outputBits) {
    this.OutputBits = outputBits;
    this.Initialize();
  }

  private int _outputBits;

  private Action _reset;
  private Action<byte[], int, int> _core;
  private Func<byte[]> _final;

  #region Overrides of HashAlgorithm

  public override void Initialize() {
    switch (this.OutputBits) {
      case 8: {
        byte state = default, sum = default;

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        void Reset() {
          state = 0;
          sum = 0;
        }

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index) {
            state = (byte)((state + array[index]) % 15);
            sum = (byte)((sum + state) % 15);
          }
        }

        byte[] Final() => [(byte)((sum << 4) | state)];
      }
      case 16: {
        byte state = default, sum = default;

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        void Reset() {
          state = 0;
          sum = 0;
        }

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index) {
            state = (byte)((state + array[index]) % byte.MaxValue);
            sum = (byte)((sum + state) % byte.MaxValue);
          }
        }

        byte[] Final() => [sum, state];
      }
      case 32: {
        ushort state = default, sum = default;
        List<byte> carry = new(2);

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        ushort ConvertFrom(byte b0, byte b1) => (ushort)(b0 | (b1 << 8));

        void Round(ushort value) {
          state = (ushort)(((uint)state + value) % ushort.MaxValue);
          sum = (ushort)(((uint)sum + state) % ushort.MaxValue);
        }

        void Reset() {
          state = 0;
          sum = 0;
          carry.Clear();
        }

        void Core(byte[] array, int index, int count) {
          var end = index + count;

          while (carry.Count > 0) {
            if (index >= end)
              return;

            carry.Add(array[index++]);
            if (carry.Count != 2)
              continue;

            Round(ConvertFrom(carry[0], carry[1]));
            carry.Clear();
            break;
          }

          while (index + 1 < end)
            Round(ConvertFrom(array[index++], array[index++]));

          while (index < end)
            carry.Add(array[index++]);
        }

        byte[] Final() {
          if (carry.Count == 1)
            Round(ConvertFrom(carry[0], 0));

          return [(byte)(sum >> 8), (byte)sum, (byte)(state >> 8), (byte)state];
        }
      }
      case 64: {
        uint state = default, sum = default;
        List<byte> carry = new(4);

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        uint ConvertFrom(byte b0, byte b1, byte b2, byte b3) => (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));

        void Round(uint value) {
          state = (uint)(((ulong)state + value) % uint.MaxValue);
          sum = (uint)(((ulong)sum + state) % uint.MaxValue);
        }

        void Reset() {
          state = 0;
          sum = 0;
          carry.Clear();
        }

        void Core(byte[] array, int index, int count) {
          var end = index + count;

          while (carry.Count > 0) {
            if (index >= end)
              return;

            carry.Add(array[index++]);
            if (carry.Count != 4)
              continue;

            Round(ConvertFrom(carry[0], carry[1], carry[2], carry[3]));
            carry.Clear();
            break;
          }

          while (index + 3 < end)
            Round(ConvertFrom(array[index++], array[index++], array[index++], array[index++]));

          while (index < end)
            carry.Add(array[index++]);
        }

        byte[] Final() {
          switch (carry.Count) {
            case 1:
              Round(ConvertFrom(carry[0], 0, 0, 0));
              break;
            case 2:
              Round(ConvertFrom(carry[0], carry[1], 0, 0));
              break;
            case 3:
              Round(ConvertFrom(carry[0], carry[1], carry[2], 0));
              break;
          }

          return [(byte)(sum >> 24), (byte)(sum >> 16), (byte)(sum >> 8), (byte)sum, (byte)(state >> 24), (byte)(state >> 16), (byte)(state >> 8), (byte)state];
        }
      }
      default: throw new NotSupportedException();
    }

    this._reset();
  }

  protected override void HashCore(byte[] array, int ibStart, int cbSize) => this._core(array, ibStart, cbSize);

  protected override byte[] HashFinal() => this._final();

  #endregion

  #region Implementation of IAdvancedHashAlgorithm

  public string Name => $"Fletcher{this.OutputBits}";

  public int OutputBits {
    get => this._outputBits;
    set {
      if (!SupportedOutputBits.Contains(value))
        throw new ArgumentException();

      this._outputBits = value;
    }
  }

  public byte[] IV {
    get => null;
    set => throw new NotSupportedException();
  }

  public static int MinOutputBits => SupportedOutputBits[0];
  public static int MaxOutputBits => SupportedOutputBits[^1];
  public static int[] SupportedOutputBits => [8, 16, 32, 64];

  public static bool SupportsIV => false;
  public static int MinIVBits => 0;
  public static int MaxIVBits => MinIVBits;
  public static int[] SupportedIVBits => Array.Empty<int>();

  #endregion
}
