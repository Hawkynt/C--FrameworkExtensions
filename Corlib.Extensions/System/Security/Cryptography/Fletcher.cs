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

using System.Linq;

namespace System.Security.Cryptography {
  /// <summary>
  /// </summary>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class Fletcher : HashAlgorithm, IAdvancedHashAlgorithm {

    public Fletcher() {
      this.OutputBits = SupportedOutputBits.First();
      this.Initialize();
    }
    public Fletcher(int outputBits) {
      this.OutputBits = outputBits;
      this.Initialize();
    }

    private int _outputBits;
    private Action<byte[], int, int> _core;
    private Action _reset;
    private Func<byte[]> _final;

    #region Overrides of HashAlgorithm

    public override sealed void Initialize() {
      var action = this._reset;
      if (action != null)
        action();
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
      var action = this._core;
      if (action != null)
        action(array, ibStart, cbSize);
    }

    protected override byte[] HashFinal() {
      var action = this._final;
      return (action != null ? action() : null);
    }

    #endregion

    #region Implementation of IAdvancedHashAlgorithm

    public string Name { get { return (string.Format("Fletcher({0})", this.OutputBits)); } }

    public int OutputBits {
      get { return this._outputBits; }
      set {
        if (!SupportedOutputBits.Contains(value))
          throw new ArgumentException();

        this._outputBits = value;

        switch (value) {
          case 16: {
            byte state = 0;
            byte sum = 0;
            this._reset = () => {
              state = 0;
              sum = 0;
            };
            this._final = () => new[] { state, sum };
            this._core = (a, s, l) => {
              for (var i = s; i < s + l; ++i)
                sum += (state += a[i]);
            };
            break;
          }
          case 32: {
            ushort state = 0;
            ushort sum = 0;
            this._reset = () => {
              state = 0;
              sum = 0;
            };
            this._final = () => BitConverter.GetBytes(sum << 16 | state);
            this._core = (a, s, l) => {
              for (var i = s; i < s + l; ++i)
                sum += (state += a[i]);
            };
            break;
          }
          case 64: {
            uint state = 0;
            uint sum = 0;
            this._reset = () => {
              state = 0;
              sum = 0;
            };
            this._final = () => BitConverter.GetBytes((ulong)sum << 32 | state);
            this._core = (a, s, l) => {
              for (var i = s; i < s + l; ++i)
                sum += (state += a[i]);
            };
            break;
          }
          case 128: {
            ulong state = 0;
            ulong sum = 0;
            this._reset = () => {
              state = 0;
              sum = 0;
            };
            this._final = () => BitConverter.GetBytes(state).Concat(BitConverter.GetBytes(sum)).ToArray();
            this._core = (a, s, l) => {
              for (var i = s; i < s + l; ++i)
                sum += (state += a[i]);
            };
            break;
          }
          default: {
            throw new NotSupportedException();
          }
        }

      }
    }

    public byte[] IV {
      get { return (null); }
      set { throw new NotSupportedException(); }
    }

    #endregion


    #region Algorithm basics
    public static readonly int MinOutputBits = 16;
    public static readonly int MaxOutputBits = 128;
    public static readonly int[] SupportedOutputBits = { 16, 32, 64, 128 };

    public static readonly bool SupportsIV = false;
    public static readonly int MinIVBits = 0;
    public static readonly int MaxIVBits = MinIVBits;
    public static readonly int[] SupportedIVBits = { };
    #endregion
  }
}
