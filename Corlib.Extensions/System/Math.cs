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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Collections.Generic;
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantCast
// ReSharper disable CompareOfFloatsByEqualityOperator
namespace System;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class MathEx {

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
  public static float Floor(this float @this) => MathF.Floor(@this);
#else
  public static float Floor(this float @this) => (float)Math.Floor(@this);
#endif

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
  public static float Ceiling(this float @this) => MathF.Ceiling(@this);
#else
  public static float Ceiling(this float @this) => (float)Math.Ceiling(@this);
#endif

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
  public static float Truncate(this float @this) => MathF.Truncate(@this);
#else
  public static float Truncate(this float @this) => (float)Math.Truncate(@this);
#endif

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
  public static float Round(this float @this) => MathF.Round(@this);
#else
  public static float Round(this float @this) => (float)Math.Round(@this);
#endif

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static float Round(this float @this, int digits) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(digits >= 0 && digits <= 15);
#endif
#if SUPPORTS_MATHF
    return MathF.Round(@this, digits);
#else
    return (float)Math.Round(@this, digits);
#endif
  }

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_MATHF
  public static float Round(this float @this, MidpointRounding method) => MathF.Round(@this, method);
#else
  public static float Round(this float @this, MidpointRounding method) => (float)Math.Round(@this, method);
#endif

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static float Round(this float @this, int digits, MidpointRounding method) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(digits >= 0 && digits <= 15);
#endif
#if SUPPORTS_MATHF
    return MathF.Round(@this, digits, method);
#else
    return (float)Math.Round(@this, digits, method);
#endif
  }

#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Log(this double @this, double @base) => Math.Log(@this, @base);

  /// <summary>
  /// Calculates the cubic root.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Cbrt(this double @this) => Math.Pow(@this, 1d / 3);

  /// <summary>
  /// Calculates the cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Cot(this double @this) => Math.Cos(@this) / Math.Sin(@this);

  /// <summary>
  /// Calculates the hyperbolic cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Coth(this double @this) {
    var ex = Math.Exp(@this);
    var em = 1 / ex;
    return (ex + em) / (ex - em);
  }

  /// <summary>
  /// Calculates the cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Csc(this double @this) => 1 / Math.Sin(@this);

  /// <summary>
  /// Calculates the hyperbolic cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Csch(this double @this) {
    var ex = Math.Exp(@this);
    return 2 / (ex - 1 / ex);
  }

  /// <summary>
  /// Calculates the secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Sec(this double @this) => 1 / Math.Cos(@this);

  /// <summary>
  /// Calculates the hyperbolic secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Sech(this double @this) {
    var ex = Math.Exp(@this);
    return 2 / (ex + 1 / ex);
  }

  /// <summary>
  /// Calculates the area hyperbolic sine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Arsinh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this + 1));

  /// <summary>
  /// Calculates the area hyperbolic cosine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Arcosh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this - 1));

  /// <summary>
  /// Calculates the area hyperbolic tangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Artanh(this double @this) => 0.5d * Math.Log((1 + @this) / (1 - @this));

  /// <summary>
  /// Calculates the area hyperbolic cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Arcoth(this double @this) => 0.5d * Math.Log((@this + 1) / (@this - 1));

  /// <summary>
  /// Calculates the area hyperbolic secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Arsech(this double @this) => Math.Log((1 + Math.Sqrt(1 - @this * @this)) / @this);

  /// <summary>
  /// Calculates the area hyperbolic cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Arcsch(this double @this) => Math.Log((1 + Math.Sqrt(1 + @this * @this)) / @this);

  /// <summary>
  /// Calculates the arcus sine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Asin(this double @this) => Math.Asin(@this);

  /// <summary>
  /// Calculates the arcus cosine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Acos(this double @this) => Math.Acos(@this);

  /// <summary>
  /// Calculates the arcus tangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Atan(this double @this) => Math.Atan(@this);

  /// <summary>
  /// Calculates the arcus cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Acot(this double @this) => Math.Atan(1 / @this);

  /// <summary>
  /// Calculates the arcus secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Asec(this double @this) => Math.Acos(1 / @this);

  /// <summary>
  /// Calculates the arcus cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double Acsc(this double @this) => Math.Asin(1 / @this);

  /// <summary>
  /// Enumerates all primes in the ulong value space.
  /// </summary>
  public static IEnumerable<ulong> EnumeratePrimes => _EnumeratePrimes();

  private readonly struct PrimeSieve {
    private readonly ulong[] _values;
    public PrimeSieve(ulong[] values) => this._values = values;

    public IEnumerable<ulong> Enumerate() {
      ulong prime = 3;
      var values = this._values;
      for (var i = 0; i < values.Length; ++i, prime += 2) {
        if (values[i] != 0)
          continue;

#if SUPPORTS_ASYNC
        var task = this._FillSieveAsync(prime);
        yield return prime;
        task.Wait();
#else
        yield return prime;
        this._FillSieveAction(prime);
#endif
      }
    }

#if SUPPORTS_ASYNC
    private Task _FillSieveAsync(ulong prime) => Task.Factory.StartNew(this._FillSieveAction, prime);
    private void _FillSieveAction(object state) => this._FillSieveAction((ulong)state);
#endif

    private void _FillSieveAction(ulong prime) {
      var doublePrime = prime << 1;
      var values = this._values;
      var maxNumberInSieve = ((ulong)values.Length << 1) + 3;
      for (var j = prime * prime; j < maxNumberInSieve; j += doublePrime)
        values[(int)((j - 3) >> 1)] = j;
    }

  }

  private struct KnownPrimesStorage {
    private readonly ulong[] _primes;
    private int _index;

    // no checks because only used internally and guaranteed to have a primes!=null && primes.Length > 0 
    public KnownPrimesStorage(ulong[] primes) => this._primes = primes;

    private bool _IsSpaceInBufferLeft() => this._index < this._primes.Length;

    // no checks because we guarantee, that all calls occur while there is still space in the array
    public void Add(ulong prime) => this._primes[this._index++] = prime;

    public IEnumerable<ulong> Enumerate() {
      foreach (var prime in this._GenerateAndFillBuffer())
        yield return prime;

#if COLOR_PRIME_GENERATION
      Console.ForegroundColor = ConsoleColor.Yellow;
#endif

      foreach (var prime in this._EnumerateWithFullBuffer())
        yield return prime;
    }

    private IEnumerable<ulong> _GenerateAndFillBuffer() {

      // array always valid
      var primes = this._primes;

      // array always contains at least one prime from the sieve
      var lastKnownPrime = primes[this._index - 1];

#if SUPPORTS_ASYNC
      var task = Task.Factory.StartNew(this._FindNextPrimeWithPartiallyFilledBuffer, lastKnownPrime);
      for(;;) {
        task.Wait();
        lastKnownPrime = task.Result;
        this.Add(lastKnownPrime);
        if (this._IsSpaceInBufferLeft()) {
          task = Task.Factory.StartNew(this._FindNextPrimeWithPartiallyFilledBuffer, lastKnownPrime);
          yield return lastKnownPrime;
        } else {
          yield return lastKnownPrime;
          yield break;
        }
      }
#else
      while (this._IsSpaceInBufferLeft()) {
        lastKnownPrime = this._FindNextPrimeWithPartiallyFilledBuffer(lastKnownPrime);
        this.Add(lastKnownPrime);
        yield return lastKnownPrime;
      }
#endif
    }

#if SUPPORTS_ASYNC
    private ulong _FindNextPrimeWithPartiallyFilledBuffer(object state) => this._FindNextPrimeWithPartiallyFilledBuffer((ulong)state);
#endif

    private ulong _FindNextPrimeWithPartiallyFilledBuffer(ulong lastKnownPrime) {
      var candidate = lastKnownPrime;
      do {
        candidate += 2;
      } while (!this._IsPrimeWithPartiallyFilledBuffer(candidate));

      return candidate;
    }

    private bool _IsPrimeWithPartiallyFilledBuffer(ulong candidate) {
      for (var i = 0; i < this._index; ++i)
        if (candidate % this._primes[i] == 0)
          return false;

      return true;
    }

    private IEnumerable<ulong> _EnumerateWithFullBuffer() {
      var lastKnownPrime = this._primes[^1];
      var upperPrimeSquare = lastKnownPrime * lastKnownPrime;

#if SUPPORTS_ASYNC
      var task = Task.Factory.StartNew(this._FindNextPrimeWithFullBuffer, lastKnownPrime);
      for (var candidate = lastKnownPrime + 2; candidate <= upperPrimeSquare; candidate = task.Result) {
        task.Wait();
        lastKnownPrime = task.Result;

        if (lastKnownPrime <= upperPrimeSquare) {
          task = Task.Factory.StartNew(this._FindNextPrimeWithFullBuffer, lastKnownPrime);
          yield return lastKnownPrime;
        } else {
          yield return lastKnownPrime;
          yield break;
        }
      }
#else
      for (var candidate = lastKnownPrime + 2; candidate <= upperPrimeSquare; candidate += 2) {
        if (this._IsPrimeWithFullBuffer(candidate))
          yield return candidate;
      }
#endif
    }

#if SUPPORTS_ASYNC
    private ulong _FindNextPrimeWithFullBuffer(object state) => this._FindNextPrimeWithFullBuffer((ulong)state);
#endif

    private ulong _FindNextPrimeWithFullBuffer(ulong lastKnownPrime) {
      var candidate = lastKnownPrime;
      do {
        candidate += 2;
      } while (!this._IsPrimeWithFullBuffer(candidate));

      return candidate;
    }

    private bool _IsPrimeWithFullBuffer(ulong candidate) {
      foreach (var prime in this._primes)
        if (candidate % prime == 0)
          return false;

      return true;
    }

  }

  private static IEnumerable<ulong> _EnumeratePrimes() {
#if COLOR_PRIME_GENERATION
    Console.ForegroundColor= ConsoleColor.White;
#endif
    yield return 2;

    var buffer = new ulong[128];
    PrimeSieve sieve = new(buffer);
    KnownPrimesStorage knownPrimes = new(buffer);

#if COLOR_PRIME_GENERATION
    Console.ForegroundColor = ConsoleColor.Cyan;
#endif
    foreach (var prime in sieve.Enumerate()) {
      yield return prime;
      knownPrimes.Add(prime);
    }

#if COLOR_PRIME_GENERATION
    Console.ForegroundColor = ConsoleColor.Green;
#endif
    foreach (var prime in knownPrimes.Enumerate())
      yield return prime;

#if COLOR_PRIME_GENERATION
    Console.ForegroundColor = ConsoleColor.Red;
#endif
    foreach (var prime in EnumerateSlowPrimesWithKnowns())
      yield return prime;

    IEnumerable<ulong> EnumerateSlowPrimesWithKnowns() {
      var largestKnownPrime = buffer[^1];

      // Start from the square of the last known prime plus 2 (to ensure it's odd)
      var candidate = largestKnownPrime * largestKnownPrime + 2;

#if SUPPORTS_ASYNC
      var task = Task.Factory.StartNew(IsPrimeWithBufferAndBeyondT,candidate);
      for (;;) {
        task.Wait();
        var isPrime = task.Result;

        if (isPrime)
          yield return candidate;

        candidate += 2; // Ensure we only check odd numbers
        task = Task.Factory.StartNew(IsPrimeWithBufferAndBeyondT,candidate);
      }
#else
      for (; ; ) {
        var isPrime = IsPrimeWithBufferAndBeyond(candidate);

        if (isPrime)
          yield return candidate;

        candidate += 2; // Ensure we only check odd numbers
      }
#endif
    }

#if SUPPORTS_ASYNC
    bool IsPrimeWithBufferAndBeyondT(object state) => IsPrimeWithBufferAndBeyond((ulong)state);
#endif

    bool IsPrimeWithBufferAndBeyond(ulong candidate) {
      // 1. Check divisibility with all primes in the buffer
      foreach (var prime in buffer)
        if (candidate % prime == 0)
          return false;

      // 2. If none of the primes in the buffer divide the candidate, 
      //    check divisibility with numbers (only odd ones) up to the square root of the candidate
      var sqrtCandidate = (ulong)Math.Sqrt(candidate);
      for (var i = buffer[^1] + 2; i <= sqrtCandidate; i += 2)
        if (candidate % i == 0)
          return false;

      return true;
    }


  }

}