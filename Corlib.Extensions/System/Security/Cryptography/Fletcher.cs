﻿#region (c)2010-2042 Hawkynt
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

using System.Linq;

namespace System.Security.Cryptography;

public sealed class Fletcher : HashAlgorithm, IAdvancedHashAlgorithm {

  public Fletcher() {
    this.OutputBits = SupportedOutputBits.First();
    this.Initialize();
  }

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
      case 16: {
        byte state = 0;
        byte sum = 0;

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;
          
        void Reset() {
          state = 0;
          sum = 0;
        }

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index)
              sum += state += array[index];
        }

        byte[] Final() => new[] { state, sum };

      }
      case 32: {
        ushort state = 0;
        ushort sum = 0;

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        void Reset() {
          state = 0;
          sum = 0;
        }

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index)
            sum += state += array[index];
        }

        byte[] Final() => BitConverter.GetBytes(sum << 16 | state);

      }
      case 64: {
        uint state = 0;
        uint sum = 0;

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        void Reset() {
          state = 0;
          sum = 0;
        }

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index)
            sum += state += array[index];
        }

        byte[] Final() => BitConverter.GetBytes((ulong)sum << 32 | state);

      }
      case 128: {
        ulong state = 0;
        ulong sum = 0;

        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        void Reset() {
          state = 0;
          sum = 0;
        }

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index)
            sum += state += array[index];
        }

        byte[] Final() => BitConverter.GetBytes(state).Concat(BitConverter.GetBytes(sum)).ToArray();

      }
      default: 
        throw new NotSupportedException();
    }
    
    this._reset();
  }

  protected override void HashCore(byte[] array, int ibStart, int cbSize) => this._core(array, ibStart, cbSize);

  protected override byte[] HashFinal() => this._final();

  #endregion

  #region Implementation of IAdvancedHashAlgorithm

  public string Name => $"Fletcher({this.OutputBits})";

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

  public static int MinOutputBits => 16;
  public static int MaxOutputBits => 128;
  public static int[] SupportedOutputBits => new[]{ 16, 32, 64, 128 };

  public static bool SupportsIV => false;
  public static int MinIVBits => 0;
  public static int MaxIVBits => MinIVBits;
  public static int[] SupportedIVBits => Utilities.Array.Empty<int>();

  #endregion
}