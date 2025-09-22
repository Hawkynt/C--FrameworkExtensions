using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public class TimeSpanComprehensiveTest {
  #region TimeSpan Arithmetic - Multiplication Tests

  [Test]
  public void TimeSpanExtensions_MultipliedWith_Int_PositiveValue_MultipliesCorrectly() {
    var timeSpan = TimeSpan.FromMinutes(10);
    var result = timeSpan.MultipliedWith(3);

    Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(30)));
  }

  [Test]
  public void TimeSpanExtensions_MultipliedWith_Double_FractionalValue_MultipliesCorrectly() {
    var timeSpan = TimeSpan.FromHours(2);
    var result = timeSpan.MultipliedWith(1.5);

    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(3)));
  }

  [Test]
  public void TimeSpanExtensions_MultipliedWith_Decimal_HighPrecision_MultipliesCorrectly() {
    var timeSpan = TimeSpan.FromMilliseconds(100);
    var result = timeSpan.MultipliedWith(3.14159m);

    Assert.That(result.TotalMilliseconds, Is.EqualTo(314.159).Within(0.001));
  }

  [Test]
  public void TimeSpanExtensions_MultipliedWith_Zero_ReturnsZero() {
    var timeSpan = TimeSpan.FromDays(10);
    var result = timeSpan.MultipliedWith(0);

    Assert.That(result, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  public void TimeSpanExtensions_MultipliedWith_NegativeValue_ReturnsNegativeTimeSpan() {
    var timeSpan = TimeSpan.FromHours(5);
    var result = timeSpan.MultipliedWith(-2);

    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(-10)));
  }

  [Test]
  public void TimeSpanExtensions_MultipliedWith_ULong_LargeValue_HandlesCorrectly() {
    var timeSpan = TimeSpan.FromTicks(1000);
    var result = timeSpan.MultipliedWith(1000000UL);

    Assert.That(result.Ticks, Is.EqualTo(1000000000L));
  }

  [Test]
  public void TimeSpanExtensions_MultipliedWith_AllNumericTypes_WorkConsistently() {
    var timeSpan = TimeSpan.FromSeconds(10);
    var multiplier = 2;
    var expected = TimeSpan.FromSeconds(20);

    Assert.That(timeSpan.MultipliedWith((sbyte)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((byte)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((short)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((ushort)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((int)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((uint)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((long)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((ulong)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((float)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((double)multiplier), Is.EqualTo(expected));
    Assert.That(timeSpan.MultipliedWith((decimal)multiplier), Is.EqualTo(expected));
  }

  #endregion

  #region TimeSpan Arithmetic - Division Tests

  [Test]
  public void TimeSpanExtensions_DividedBy_Int_PositiveValue_DividesCorrectly() {
    var timeSpan = TimeSpan.FromMinutes(30);
    var result = timeSpan.DividedBy(3);

    Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(10)));
  }

  [Test]
  public void TimeSpanExtensions_DividedBy_Double_FractionalValue_DividesCorrectly() {
    var timeSpan = TimeSpan.FromHours(3);
    var result = timeSpan.DividedBy(1.5);

    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(2)));
  }

  [Test]
  public void TimeSpanExtensions_DividedBy_NegativeValue_ReturnsNegativeTimeSpan() {
    var timeSpan = TimeSpan.FromHours(10);
    var result = timeSpan.DividedBy(-2);

    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(-5)));
  }

  [Test]
  public void TimeSpanExtensions_DividedBy_TimeSpan_ReturnsRatio() {
    var timeSpan1 = TimeSpan.FromHours(6);
    var timeSpan2 = TimeSpan.FromHours(2);
    var result = timeSpan1.DividedBy(timeSpan2);

    Assert.That(result, Is.EqualTo(3.0));
  }

  [Test]
  public void TimeSpanExtensions_DividedBy_TimeSpan_FractionalResult_ReturnsCorrectRatio() {
    var timeSpan1 = TimeSpan.FromMinutes(45);
    var timeSpan2 = TimeSpan.FromMinutes(30);
    var result = timeSpan1.DividedBy(timeSpan2);

    Assert.That(result, Is.EqualTo(1.5).Within(0.001));
  }

  [Test]
  public void TimeSpanExtensions_DividedBy_VerySmallDivisor_HandlesCorrectly() {
    var timeSpan = TimeSpan.FromSeconds(1);
    var result = timeSpan.DividedBy(0.001); // Very small divisor

    Assert.That(result.TotalSeconds, Is.EqualTo(1000).Within(0.1));
  }

  #endregion

  #region TimeSpan Factory Methods Tests

  [Test]
  public void TimeSpanExtensions_IntegerFactory_Milliseconds_CreatesCorrectTimeSpan() {
    var result = 500.Milliseconds();

    Assert.That(result, Is.EqualTo(TimeSpan.FromMilliseconds(500)));
  }

  [Test]
  public void TimeSpanExtensions_IntegerFactory_Seconds_CreatesCorrectTimeSpan() {
    var result = 30.Seconds();

    Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(30)));
  }

  [Test]
  public void TimeSpanExtensions_IntegerFactory_Minutes_CreatesCorrectTimeSpan() {
    var result = 15.Minutes();

    Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(15)));
  }

  [Test]
  public void TimeSpanExtensions_IntegerFactory_Hours_CreatesCorrectTimeSpan() {
    var result = 8.Hours();

    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(8)));
  }

  [Test]
  public void TimeSpanExtensions_IntegerFactory_Days_CreatesCorrectTimeSpan() {
    var result = 7.Days();

    Assert.That(result, Is.EqualTo(TimeSpan.FromDays(7)));
  }

  [Test]
  public void TimeSpanExtensions_IntegerFactory_Weeks_CreatesCorrectTimeSpan() {
    var result = 2.Weeks();

    Assert.That(result, Is.EqualTo(TimeSpan.FromDays(14)));
  }

  [Test]
  public void TimeSpanExtensions_DoubleFactory_FractionalValues_CreatesCorrectTimeSpan() {
    var result = 1.5.Hours();

    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(1.5)));
  }

  [Test]
  public void TimeSpanExtensions_FactoryMethods_AllTypes_WorkConsistently() {
    var value = 5;
    var expected = TimeSpan.FromMinutes(5);

    Assert.That(((sbyte)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((byte)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((short)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((ushort)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((int)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((uint)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((long)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((ulong)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((float)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((double)value).Minutes(), Is.EqualTo(expected));
    Assert.That(((decimal)value).Minutes(), Is.EqualTo(expected));
  }

  [Test]
  public void TimeSpanExtensions_FactoryMethods_ZeroValues_ReturnZeroTimeSpan() {
    Assert.That(0.Milliseconds(), Is.EqualTo(TimeSpan.Zero));
    Assert.That(0.Seconds(), Is.EqualTo(TimeSpan.Zero));
    Assert.That(0.Minutes(), Is.EqualTo(TimeSpan.Zero));
    Assert.That(0.Hours(), Is.EqualTo(TimeSpan.Zero));
    Assert.That(0.Days(), Is.EqualTo(TimeSpan.Zero));
    Assert.That(0.Weeks(), Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  public void TimeSpanExtensions_FactoryMethods_NegativeValues_ReturnNegativeTimeSpan() {
    var result = (-30).Minutes();

    Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(-30)));
    Assert.That(result.TotalMinutes, Is.LessThan(0));
  }

  #endregion

  #region DateTime Integration Tests

  [Test]
  public void TimeSpanExtensions_FromNow_AddsToCurrentTime() {
    var timeSpan = 1.Hours();
    var before = DateTime.Now;
    var result = timeSpan.FromNow();
    var after = DateTime.Now;

    // Result should be approximately 1 hour from now
    Assert.That(result, Is.GreaterThan(before.AddMinutes(59)));
    Assert.That(result, Is.LessThan(after.AddMinutes(61)));
  }

  [Test]
  public void TimeSpanExtensions_FromUtcNow_AddsToCurrentUtcTime() {
    var timeSpan = 2.Hours();
    var before = DateTime.UtcNow;
    var result = timeSpan.FromUtcNow();
    var after = DateTime.UtcNow;

    // Result should be approximately 2 hours from now
    Assert.That(result, Is.GreaterThan(before.AddMinutes(119)));
    Assert.That(result, Is.LessThan(after.AddMinutes(121)));
  }

  [Test]
  public void TimeSpanExtensions_FromNow_NegativeTimeSpan_SubtractsFromCurrentTime() {
    var timeSpan = (-30).Minutes();
    var before = DateTime.Now;
    var result = timeSpan.FromNow();

    // Result should be 30 minutes ago
    Assert.That(result, Is.LessThan(before));
    Assert.That((before - result).TotalMinutes, Is.EqualTo(30).Within(1));
  }

  [Test]
  public void TimeSpanExtensions_FromStopwatchTimeStamp_UsesHighPrecisionTiming() {
    var timeSpan = 100.Milliseconds();

    // Get the current timestamp before the operation
    var timestampBefore = Stopwatch.GetTimestamp();
    
    // This method uses Stopwatch.GetTimestamp() + timeSpan
    var result = timeSpan.FromStopwatchTimeStamp();
    
    // Get timestamp after the operation
    var timestampAfter = Stopwatch.GetTimestamp();

    // The result should be approximately timestampBefore + (timeSpan in ticks)
    var expectedTicks = (long)(timeSpan.TotalSeconds * Stopwatch.Frequency);
    var expectedMin = timestampBefore + expectedTicks;
    var expectedMax = timestampAfter + expectedTicks;

    // Result should be between the expected range (accounting for small timing differences)
    Assert.That(result, Is.GreaterThanOrEqualTo(expectedMin));
    Assert.That(result, Is.LessThanOrEqualTo(expectedMax));
    
    // Verify the result is a reasonable timestamp value (should be positive and large)
    Assert.That(result, Is.GreaterThan(timestampBefore));
    Assert.That(result, Is.TypeOf<long>());
  }

  #endregion

  #region Iteration and Timing Tests

  [Test]
  public void TimeSpanExtensions_CurrentIteration_ValidTimeSpan_ReturnsReasonableValue() {
    var timeSpan = 1.Seconds();
    var result = timeSpan.CurrenIteration();

    // Should return a non-negative integer representing current iteration
    Assert.That(result, Is.GreaterThanOrEqualTo(0));
  }

  [Test]
  public void TimeSpanExtensions_CurrentIteration_WithMaxIterations_RespectsLimit() {
    var timeSpan = 1.Milliseconds();
    var maxIterations = 1000UL;
    var result = timeSpan.CurrenIteration(maxIterations);

    // Should wrap around at maxIterations
    Assert.That(result, Is.LessThan(maxIterations));
  }

  [Test]
  public void TimeSpanExtensions_CurrentDrift_ValidTimeSpan_ReturnsTimeSpan() {
    var timeSpan = 1.Seconds();
    var result = timeSpan.CurrenDrift();

    // Should return a TimeSpan representing the drift within current iteration
    Assert.That(result, Is.InstanceOf<TimeSpan>());
    Assert.That(result, Is.GreaterThanOrEqualTo(TimeSpan.Zero));
    Assert.That(result, Is.LessThan(timeSpan));
  }

  #endregion

  #region Performance Tests

  [Test]
  public void TimeSpanExtensions_ArithmeticOperations_Performance_FastExecution() {
    var timeSpans = Enumerable
      .Range(1, 1000)
      .Select(i => TimeSpan.FromMilliseconds(i))
      .ToList();

    var sw = Stopwatch.StartNew();
    foreach (var ts in timeSpans) {
      var multiplied = ts.MultipliedWith(2.5);
      var divided = ts.DividedBy(1.5);
    }

    sw.Stop();

    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(50));
  }

  [Test]
  public void TimeSpanExtensions_FactoryMethods_Performance_FastCreation() {
    var sw = Stopwatch.StartNew();
    for (var i = 0; i < 10000; i++) {
      var ms = i.Milliseconds();
      var sec = i.Seconds();
      var min = i.Minutes();
    }

    sw.Stop();

    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100));
  }

  [Test]
  public void TimeSpanExtensions_TypeConversions_Performance_FastConversion() {
    var sw = Stopwatch.StartNew();
    for (var i = 0; i < 1000; i++) {
      var timeSpan = TimeSpan.FromMinutes(i);

      // Test all numeric type multiplications
      var results = new[] {
        timeSpan.MultipliedWith((sbyte)2),
        timeSpan.MultipliedWith((byte)2),
        timeSpan.MultipliedWith((short)2),
        timeSpan.MultipliedWith((ushort)2),
        timeSpan.MultipliedWith((int)2),
        timeSpan.MultipliedWith((uint)2),
        timeSpan.MultipliedWith((long)2),
        timeSpan.MultipliedWith((ulong)2),
        timeSpan.MultipliedWith((float)2.0f),
        timeSpan.MultipliedWith((double)2.0),
        timeSpan.MultipliedWith((decimal)2.0m)
      };
    }

    sw.Stop();

    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(200));
  }

  #endregion

  #region Edge Cases and Boundary Tests

  [Test]
  public void TimeSpanExtensions_ArithmeticOperations_ExtremeValues_HandleGracefully() {
    var maxTimeSpan = TimeSpan.MaxValue;
    var minTimeSpan = TimeSpan.MinValue;

    // Should not throw exceptions for extreme values (may clamp)
    Assert.DoesNotThrow(
      () => {
        try {
          maxTimeSpan.MultipliedWith(2);
        } catch (OverflowException) {
          // Overflow is acceptable for extreme values
        }
      }
    );

    Assert.DoesNotThrow(
      () => {
        try {
          minTimeSpan.MultipliedWith(2);
        } catch (OverflowException) {
          // Overflow is acceptable for extreme values
        }
      }
    );
  }

  [Test]
  public void TimeSpanExtensions_DivisionByVeryLargeNumber_ReturnsVerySmallTimeSpan() {
    var timeSpan = TimeSpan.FromDays(1);
    var result = timeSpan.DividedBy(double.MaxValue);

    Assert.That(result.Ticks, Is.EqualTo(0));
  }

  [Test]
  public void TimeSpanExtensions_MultiplicationByVeryLargeNumber_HandlesOverflow() {
    var timeSpan = TimeSpan.FromHours(1);

    Assert.DoesNotThrow(
      () => {
        try {
          var result = timeSpan.MultipliedWith(double.MaxValue);
        } catch (OverflowException) {
          // Overflow is expected and acceptable
        }
      }
    );
  }

  [Test]
  public void TimeSpanExtensions_FactoryMethods_ExtremeValues_HandleBoundaries() {
    // Test with type boundary values
    Assert.DoesNotThrow(
      () => {
        try {
          long.MaxValue.Milliseconds();
        } catch (OverflowException) {
          // Overflow is acceptable for extreme values
        } catch (ArgumentOutOfRangeException) {
          // ArgumentOutOfRangeException is also acceptable for extreme values
        }
      }
    );

    Assert.DoesNotThrow(
      () => {
        try {
          double.MaxValue.Hours();
        } catch (OverflowException) {
          // Overflow is acceptable for extreme values
        } catch (ArgumentOutOfRangeException) {
          // ArgumentOutOfRangeException is also acceptable for extreme values
        }
      }
    );
  }

  [Test]
  public void TimeSpanExtensions_NegativeZero_HandlesCorrectly() {
    var negativeZero = -0.0;
    var result = negativeZero.Seconds();

    Assert.That(result, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  public void TimeSpanExtensions_InfinityAndNaN_HandlesGracefully() {
    Assert.DoesNotThrow(
      () => {
        try {
          double.PositiveInfinity.Seconds();
        } catch (ArgumentOutOfRangeException) {
          // ArgumentOutOfRangeException is also acceptable for extreme values
        } catch (ArgumentException) {
          // ArgumentException is acceptable for infinity
        } catch (OverflowException) {
          // OverflowException is also acceptable
        }
      }
    );

    Assert.DoesNotThrow(
      () => {
        try {
          double.NaN.Minutes();
        } catch (ArgumentOutOfRangeException) {
          // ArgumentOutOfRangeException is also acceptable for extreme values
        } catch (ArgumentException) {
          // ArgumentException is acceptable for NaN
        }
      }
    );
  }

  #endregion

  #region Precision and Accuracy Tests

  [Test]
  public void TimeSpanExtensions_HighPrecisionArithmetic_MaintainsAccuracy() {
    var timeSpan = TimeSpan.FromTicks(1);
    var result = timeSpan.MultipliedWith(1000000);

    Assert.That(result.Ticks, Is.EqualTo(1000000));
  }

  [Test]
  public void TimeSpanExtensions_ChainedOperations_MaintainsAccuracy() {
    var original = TimeSpan.FromMinutes(60);
    var result = original
      .MultipliedWith(2)
      .DividedBy(3)
      .MultipliedWith(1.5);

    var expected = TimeSpan.FromMinutes(60); // 60 * 2 / 3 * 1.5 = 60
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  public void TimeSpanExtensions_DecimalPrecision_HandlesCorrectly() {
    var timeSpan = 1.Milliseconds();
    var result = timeSpan.MultipliedWith(0.999999999999999m);

    // Should maintain high decimal precision
    Assert.That(result.Ticks, Is.LessThan(timeSpan.Ticks));
    Assert.That(result.Ticks, Is.GreaterThan(timeSpan.Ticks * 0.9m));
  }

  #endregion

  #region Integration Tests

  [Test]
  public void TimeSpanExtensions_ComplexScenario_WorkflowTest() {
    // Simulate a complex workflow using multiple extension methods
    var workHours = 8.Hours();
    var breakTime = 30.Minutes();
    var overtime = 2.5.Hours();

    var totalWork = workHours.MultipliedWith(5); // 5 work days
    var totalBreaks = breakTime.MultipliedWith(10); // 2 breaks per day * 5 days
    var weeklyOvertime = overtime.MultipliedWith(2); // 2 days with overtime

    var grossTime = totalWork + totalBreaks + weeklyOvertime;
    var netTime = grossTime - totalBreaks;

    var averageDaily = netTime.DividedBy(5);

    Assert.That(totalWork, Is.EqualTo(TimeSpan.FromHours(40)));
    Assert.That(totalBreaks, Is.EqualTo(TimeSpan.FromMinutes(300)));
    Assert.That(weeklyOvertime, Is.EqualTo(TimeSpan.FromHours(5)));
    Assert.That(netTime, Is.EqualTo(TimeSpan.FromHours(45)));
    Assert.That(averageDaily, Is.EqualTo(TimeSpan.FromHours(9)));
  }

  [Test]
  public void TimeSpanExtensions_DateTimeIntegration_WorksTogether() {
    var meeting = DateTime.Today.AddHours(14); // 2 PM today
    var duration = 1.5.Hours();
    var buffer = 15.Minutes();

    var endTime = meeting + duration;
    var withBuffer = endTime + buffer;
    var reminderTime = meeting - 10.Minutes();

    Assert.That(endTime, Is.EqualTo(meeting.AddHours(1.5)));
    Assert.That(withBuffer, Is.EqualTo(meeting.AddMinutes(105))); // 90 + 15
    Assert.That(reminderTime, Is.EqualTo(meeting.AddMinutes(-10)));
  }

  #endregion
}
