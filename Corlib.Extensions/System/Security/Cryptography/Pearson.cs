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

namespace System.Security.Cryptography {
  /// <summary>
  /// </summary>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class Pearson : HashAlgorithm, IAdvancedHashAlgorithm {

    private static readonly byte[] _DEFAULT_S_BOX ={
    
      // 256 values 0-255 in any (random) order suffices
       98,  6, 85,150, 36, 23,112,164,135,207,169,  5, 26, 64,165,219, //  1
       61, 20, 68, 89,130, 63, 52,102, 24,229,132,245, 80,216,195,115, //  2
       90,168,156,203,177,120,  2,190,188,  7,100,185,174,243,162, 10, //  3
      237, 18,253,225,  8,208,172,244,255,126,101, 79,145,235,228,121, //  4
      123,251, 67,250,161,  0,107, 97,241,111,181, 82,249, 33, 69, 55, //  5
       59,153, 29,  9,213,167, 84, 93, 30, 46, 94, 75,151,114, 73,222, //  6
      197, 96,210, 45, 16,227,248,202, 51,152,252,125, 81,206,215,186, //  7
       39,158,178,187,131,136,  1, 49, 50, 17,141, 91, 47,129, 60, 99, //  8
      154, 35, 86,171,105, 34, 38,200,147, 58, 77,118,173,246, 76,254, //  9
      133,232,196,144,198,124, 53,  4,108, 74,223,234,134,230,157,139, // 10
      189,205,199,128,176, 19,211,236,127,192,231, 70,233, 88,146, 44, // 11
      183,201, 22, 83, 13,214,116,109,159, 32, 95,226,140,220, 57, 12, // 12
      221, 31,209,182,143, 92,149,184,148, 62,113, 65, 37, 27,106,166, // 13
        3, 14,204, 72, 21, 41, 56, 66, 28,193, 40,217, 25, 54,179,117, // 14
      238, 87,240,155,180,170,242,212,191,163, 78,218,137,194,175,110, // 15
       43,119,224, 71,122,142, 42,160,104, 48,247,103, 15, 11,138,239  // 16
    };

    private byte[] _state;
    private byte[] _sBox;
    private bool _isStarted;

    public Pearson(int numberOfResultBits = 8, byte[] sbox = null) {
      this.OutputBits = numberOfResultBits;
      this.IV = sbox;
      this.Initialize();
    }

    #region Implementation of IAdvancedHashAlgorithm
    public string Name { get { return (string.Format("Pearson({0})", this._state.Length << 3)); } }

    public int OutputBits {
      get { return (this._state.Length << 3); }
      set {
        if (!SupportedOutputBits.Contains(value))
          throw new ArgumentException();

        this._state = new byte[value >> 3];
        this.Initialize();
      }
    }

    public byte[] IV {
      get { return (this._sBox); }
      set {
        if (value != null && !SupportedIVBits.Contains(value.Length << 3))
          throw new ArgumentException();

        this._sBox = value ?? _DEFAULT_S_BOX;
        this.Initialize();
      }
    }
    #endregion

    #region Algorithm basics
    public static readonly int MinOutputBits = 8;
    public static readonly int MaxOutputBits = 256 << 3;
    public static readonly int[] SupportedOutputBits = Enumerable.Range(MinOutputBits >> 3, ((MaxOutputBits - MinOutputBits) >> 3) + 1).Select(i => i << 3).ToArray();

    public static readonly bool SupportsIV = true;
    public static readonly int MinIVBits = 256 << 3;
    public static readonly int MaxIVBits = MinIVBits;
    public static readonly int[] SupportedIVBits = { MinIVBits };
    #endregion

    #region Overrides of HashAlgorithm

    public override sealed void Initialize() {
      this._isStarted = false;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
      var sBox = this._sBox;
      var state = this._state;
      if (!this._isStarted) {
        for (var i = 0; i < state.Length; ++i)
          state[i] = sBox[(array[ibStart] + i) & 0xff];

        this._isStarted = true;
      }

      for (var i = ibStart + 1; i < ibStart + cbSize; ++i)
        for (var j = 0; j < state.Length; ++j)
          state[j] = sBox[state[j] ^ array[i]];
    }

    protected override byte[] HashFinal() {
      return (this._isStarted ? this._state : new byte[this._state.Length]);
    }

    #endregion
  }
}
