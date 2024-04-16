using System;
using NUnit.Framework;

namespace Corlib.Tests.System;

using global::System.Diagnostics;

[TestFixture]
internal class RandomTests {

  private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

  [Test]
  public void GenerateRandomBool() {
    var wasFalse = false;
    var wasTrue = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<bool>();
      switch (value) {
        case true:
          wasTrue = true;
          break;
        default:
          wasFalse = true;
          break;
      }

      if (!(wasTrue && wasFalse))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered false:{wasFalse}, true:{wasTrue}");
  }

  [Test]
  public void GenerateRandomNullableBool() {
    var wasFalse = false;
    var wasTrue = false;
    var wasNull = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<bool?>();
      switch (value) {
        case null:
          wasNull = true;
          break;
        case true:
          wasTrue = true;
          break;
        default:
          wasFalse = true;
          break;
      }

      if (!(wasTrue && wasFalse && wasNull))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered false:{wasFalse}, true:{wasTrue}, null:{wasNull}");
  }

  private static void _GenerateRandomSignedInt<T>(T zero, T minValue, T maxValue) where T : struct, IComparable<T>, IEquatable<T> {
    var wasPositive = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<T>();
      if (value.Equals(zero))
        wasZero = true;
      else if (value.Equals(minValue))
        wasNegativeMax = true;
      else if (value.Equals(maxValue))
        wasPositiveMax = true;
      else if (value.CompareTo(zero) < 0)
        wasNegative = true;
      else if (value.CompareTo(zero) > 0)
        wasPositive = true;

      if (!(wasNegative && wasPositive && wasZero && wasNegativeMax && wasPositiveMax))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  private static void _GenerateRandomUnsignedInt<T>(T zero, T maxValue) where T : struct, IComparable<T>, IEquatable<T> {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<T>();
      if (value.Equals(zero))
        wasZero = true;
      else if (value.Equals(maxValue))
        wasPositiveMax = true;
      else if (value.CompareTo(zero) > 0)
        wasPositive = true;

      if (!(wasPositive && wasZero && wasPositiveMax))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  [Test]
  public void GenerateRandomChar() {
    var wasControl = false;
    var wasNumber = false;
    var wasWhiteSpace = false;
    var wasLetter = false;
    var wasSurrogate = false;
    var wasSingleByte = false;
    var wasMultiByte = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<char>();
      if (char.IsControl(value))
        wasControl = true;
      else if (char.IsDigit(value))
        wasNumber = true;
      else if (char.IsWhiteSpace(value))
        wasWhiteSpace = true;
      else if (char.IsLetter(value))
        wasLetter = true;
      else if (char.IsSurrogate(value))
        wasSurrogate = true;
      else if (value < 0x100)
        wasSingleByte = true;
      else if (value > 0x100)
        wasMultiByte = true;

      if (!(wasControl && wasNumber && wasWhiteSpace && wasLetter && wasSurrogate && wasSingleByte && wasMultiByte))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Failed to generate all types of characters within the allotted time. Results: " +
                $"Control: {wasControl}, " +
                $"Number: {wasNumber}, " +
                $"WhiteSpace: {wasWhiteSpace}, " +
                $"Letter: {wasLetter}, " +
                $"Surrogate: {wasSurrogate}, " +
                $"SingleByte: {wasSingleByte}, " +
                $"MultiByte: {wasMultiByte}.");
  }

  [Test]
  public void GenerateRandomFloat() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasNaN = false;
    var wasNegInf = false;
    var wasPosInf = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<float>();
      switch (value) {
        case 0:
          wasZero = true;
          break;
        case float.MaxValue:
          wasPositiveMax = true;
          break;
        case float.MinValue:
          wasNegativeMax = true;
          break;
        default: {
          if (value.IsNegativeInfinity())
            wasNegInf = true;
          else if (value.IsPositiveInfinity())
            wasPosInf = true;
          else if (value > 0)
            wasPositive = true;
          else if (value < 0)
            wasNegative = true;
          else if (value.IsNaN())
            wasNaN = true;
          break;
        }
      }

      if (!(wasPositive && wasZero && wasPositiveMax && wasNegative && wasNegativeMax && wasNaN && wasNegInf && wasPosInf))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered neginf:{wasNegInf}, negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}, posinf:{wasPosInf}, NaN:{wasNaN}");
  }

  [Test]
  public void GenerateRandomDouble() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasNaN = false;
    var wasNegInf = false;
    var wasPosInf = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<double>();
      switch (value) {
        case 0:
          wasZero = true;
          break;
        case double.MaxValue:
          wasPositiveMax = true;
          break;
        case double.MinValue:
          wasNegativeMax = true;
          break;
        default: {
          if (value.IsNegativeInfinity())
            wasNegInf = true;
          else if (value.IsPositiveInfinity())
            wasPosInf = true;
          else if (value > 0)
            wasPositive = true;
          else if (value < 0)
            wasNegative = true;
          else if (value.IsNaN())
            wasNaN = true;
          break;
        }
      }

      if (!(wasPositive && wasZero && wasPositiveMax && wasNegative && wasNegativeMax && wasNaN && wasNegInf && wasPosInf))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered neginf:{wasNegInf}, negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}, posinf:{wasPosInf}, NaN:{wasNaN}");
  }

  [Test]
  public void GenerateRandomDecimal() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasMinusOne = false;
    var wasOne = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<decimal>();
      switch (value) {
        case 0:
          wasZero = true;
          break;
        case decimal.MinusOne:
          wasMinusOne = true;
          break;
        case decimal.One:
          wasOne = true;
          break;
        case decimal.MinValue:
          wasNegativeMax = true;
          break;
        case decimal.MaxValue:
          wasPositiveMax = true;
          break;
        case > 0:
          wasPositive = true;
          break;
        case < 0:
          wasNegative = true;
          break;
      }

      if (!(wasPositive && wasZero && wasPositiveMax && wasNegative && wasNegativeMax && wasMinusOne && wasOne))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered -1:{wasMinusOne}, negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}, +1:{wasOne}");
  }

  [Test]
  public void GenerateRandomInt8() => _GenerateRandomSignedInt<sbyte>(0, sbyte.MinValue, sbyte.MaxValue);

  [Test]
  public void GenerateRandomInt16() => _GenerateRandomSignedInt<short>(0, short.MinValue, short.MaxValue);

  [Test]
  public void GenerateRandomInt32() => _GenerateRandomSignedInt(0, int.MinValue, int.MaxValue);

  [Test]
  public void GenerateRandomInt64() => _GenerateRandomSignedInt(0, long.MinValue, long.MaxValue);

  [Test]
  public void GenerateRandomUInt8() => _GenerateRandomUnsignedInt((byte)0, byte.MaxValue);

  [Test]
  public void GenerateRandomUInt16() => _GenerateRandomUnsignedInt((ushort)0, ushort.MaxValue);

  [Test]
  public void GenerateRandomUInt32() => _GenerateRandomUnsignedInt(0U, uint.MaxValue);

  [Test]
  public void GenerateRandomUInt64() => _GenerateRandomUnsignedInt(0UL, ulong.MaxValue);

  [Test]
  public void GenerateRandomString() {
    var wasNull = false;
    var wasEmpty = false;
    var wasSomeLength = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<string>();
      switch (value) {
        case null:
          wasNull = true;
          break;
        case { Length: <= 0 }:
          wasEmpty = true;
          break;
        case { Length: >= 0 }:
          wasSomeLength = true;
          break;
      }

      if (!(wasNull && wasEmpty && wasSomeLength))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered null:{wasNull}, Empty:{wasEmpty}, someLength:{wasSomeLength}");
  }

  [Test]
  public void GenerateRandomObject() {
    var wasNull = false;
    var wasSomething = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<string>();
      if (value == null)
        wasNull = true;
      else
        wasSomething = true;

      if (!(wasNull && wasSomething))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered null:{wasNull}, something:{wasSomething}");
  }

  private struct DemoStruct {
#pragma warning disable CS0649
    public int x;
    public int y;
#pragma warning restore CS0649
  }

  [Test]
  public void GenerateRandomStruct() {
    var wasNotDefault = false;
    var wasEmpty = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<DemoStruct>();
      if (value is { x: 0, y: 0 })
        wasEmpty = true;
      else
        wasNotDefault = true;

      if (!(wasEmpty && wasNotDefault))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered Empty:{wasEmpty}, someData:{wasNotDefault}");
  }

  // ReSharper disable once ClassNeverInstantiated.Local
  private class DemoClass {
    public readonly int x;
    public readonly int y;

    // ReSharper disable once UnusedMember.Local
    public DemoClass() => this.y = 1;

    // ReSharper disable once UnusedMember.Local
    public DemoClass(int x) => this.x = x;
  }

  [Test]
  public void GenerateRandomClass() {
    var wasNull = false;
    var wasDefaultCtor = false;
    var was2ndCtor = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<DemoClass>();
      switch (value) {
        case null:
          wasNull = true;
          break;
        case { x: 0, y: 1 }:
          wasDefaultCtor = true;
          break;
        default: {
          if (value.x != 0)
            was2ndCtor = true;
          break;
        }
      }

      if (!(wasNull && wasDefaultCtor && was2ndCtor))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered null:{wasNull}, default ctor:{wasDefaultCtor}, other ctor:{was2ndCtor}");
  }

  private enum DemoEnum {
    Apple = -1,
    Unknown = 0,
    Pie = 1,
  }

  [Test]
  public void GenerateRandomEnum() {
    var wasPositive = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<DemoEnum>();
      switch (value) {
        case DemoEnum.Unknown:
          wasZero = true;
          break;
        case DemoEnum.Apple:
          wasNegativeMax = true;
          break;
        case DemoEnum.Pie:
          wasPositiveMax = true;
          break;
        default: {
          switch ((int)value) {
            case < 0:
              wasNegative = true;
              break;
            case > 0:
              wasPositive = true;
              break;
          }

          break;
        }
      }

      if (!(wasNegative && wasPositive && wasZero && wasNegativeMax && wasPositiveMax))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  private enum SmallDemoEnum : byte {
    Unknown = 0,
    Apple = 1,
  }

  [Test]
  public void GenerateRandomSmallEnum() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<SmallDemoEnum>();
      switch (value) {
        case SmallDemoEnum.Unknown:
          wasZero = true;
          break;
        case SmallDemoEnum.Apple:
          wasPositiveMax = true;
          break;
        default:
          wasPositive = true;
          break;
      }

      if (!(wasPositive && wasZero && wasPositiveMax))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  [Flags]
  private enum FlagDemoEnum : byte {
    Apple = 1,
    Pie = 2,
  }

  [Test]
  public void GenerateRandomFlagEnum() {
    var wasPositive = false;
    var wasCombined = false;
    var wasSingle = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<FlagDemoEnum>();
      if (value == FlagDemoEnum.Apple)
        wasSingle = true;
      else if (value == (FlagDemoEnum.Apple | FlagDemoEnum.Pie))
        wasCombined = true;
      else if ((byte)value > 0)
        wasPositive = true;

      if (!(wasPositive && wasSingle && wasCombined))
        continue;

      Assert.Pass();
      return;

    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered single:{wasSingle}, positive:{wasPositive}, combined:{wasCombined}");
  }

}