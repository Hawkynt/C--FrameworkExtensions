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

namespace System.Security.Cryptography;

public sealed class JavaHash : HashAlgorithm, IAdvancedHashAlgorithm {

  private const ushort _PRIME = 31;

  public JavaHash():this(MaxOutputBits) { }

  public JavaHash(int outputBits) {
    this.OutputBits = outputBits;
    this.Initialize();
  }

  private Action _reset;
  private Action<byte[], int, int> _core;
  private Func<byte[]> _final;

  #region Overrides of HashAlgorithm

  public override void Initialize() {

    switch (this.OutputBits) {
      case 32: {
        uint state = 0;
        this._reset = Reset;
        this._core = Core;
        this._final = Final;
        break;

        void Reset() => state = 0;

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index)
            state = state * _PRIME + array[index];
        }

        byte[] Final() => BitConverter.GetBytes(state);

      }
      case 64: {
        ulong state = 0;
        this._reset = Reset;
        this._core = Core;
        this._final = Final;

        break;

        void Reset() => state = 0;

        void Core(byte[] array, int index, int count) {
          for (count += index; index < count; ++index)
            state = state * _PRIME + array[index];
        }

        byte[] Final() => BitConverter.GetBytes(state);

      }
      default:
        throw new NotSupportedException();
    }

    this._reset();
  }

  protected override void HashCore(byte[] array, int index, int count) => this._core(array, index, count);

  protected override byte[] HashFinal() => this._final();

  #endregion

  #region Implementation of IAdvancedHashAlgorithm

  public string Name => $"JavaHash{this.OutputBits}";

  private int _outputBits;
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
    set => throw new NotImplementedException();
  }

  public static int MinOutputBits => SupportedOutputBits[0];
  public static int MaxOutputBits => SupportedOutputBits[^1];
  public static int[] SupportedOutputBits => new[] { 32, 64 };
  public static bool SupportsIV => false;
  public static int MinIVBits => 0;
  public static int MaxIVBits => MinIVBits;
  public static int[] SupportedIVBits => Utilities.Array.Empty<int>();

  #endregion
}