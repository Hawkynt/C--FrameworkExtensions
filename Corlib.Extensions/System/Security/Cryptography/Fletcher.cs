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

public sealed class Fletcher : HashAlgorithm, IAdvancedHashAlgorithm {

  public Fletcher():this(MaxOutputBits) { }

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
      case 4: {
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
          for (count += index; index < count; ++index) {
            state = (byte)((state + array[index]) % 3);
            sum = (byte)((sum + state) % 3);
          }
        }

        byte[] Final() => new[] { (byte)(sum << 2 | state) };

      }
      case 8: {
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
          for (count += index; index < count; ++index) {
            state = (byte)((state + array[index]) % 15);
            sum = (byte)((sum + state) % 15);
          }
        }

        byte[] Final() => new[] { (byte)(sum << 4 | state) };

      }
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
          for (count += index; index < count; ++index) {
            state = (byte)((state + array[index]) % byte.MaxValue);
            sum = (byte)((sum + state) % byte.MaxValue);
          }
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
          for (count += index; index < count; ++index) {
            state = (ushort)((state + array[index]) % ushort.MaxValue);
            sum = (ushort)((sum + state) % ushort.MaxValue);
          }
        }

        byte[] Final() => BitConverter.GetBytes((uint)sum << 16 | state);

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
          for (count += index; index < count; ++index) {
            state = (state + array[index]) % uint.MaxValue;
            sum = (sum + state) % uint.MaxValue;
          }
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
          for (count += index; index < count; ++index) {
            state = (state + array[index]) % ulong.MaxValue;
            sum = (sum + state) % ulong.MaxValue;
          }
        }

        byte[] Final() {
          var result = new byte[16];
          Array.Copy(BitConverter.GetBytes(state),0,result,0,8);
          Array.Copy(BitConverter.GetBytes(sum), 0, result, 8, 8);
          return result;
        }
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

  public static int MinOutputBits => 4;
  public static int MaxOutputBits => 128;
  public static int[] SupportedOutputBits => new[]{ 4, 8, 16, 32, 64, 128 };

  public static bool SupportsIV => false;
  public static int MinIVBits => 0;
  public static int MaxIVBits => MinIVBits;
  public static int[] SupportedIVBits => Utilities.Array.Empty<int>();

  #endregion
}