using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

using System;
using Guard;

public static class Program {
  //public static void Main() => BenchmarkRunner.Run<String_ExchangeAt>();
  public static void Main() => BenchmarkRunner.Run<String_SubString>();
}

internal class BenchmarkConfiguration : ManualConfig {
  public BenchmarkConfiguration() {
    /*
    AddJob(new Job { Environment = { Runtime = ClrRuntime.Net462, Platform = Platform.X64 } });
    AddJob(new Job { Environment = { Runtime = ClrRuntime.Net462, Platform = Platform.X86 } });
    AddJob(new Job { Environment = { Runtime = ClrRuntime.Net48, Platform = Platform.X64 } });
    AddJob(new Job { Environment = { Runtime = ClrRuntime.Net48, Platform = Platform.X86 } });
    AddJob(new Job { Environment = { Runtime = CoreRuntime.Core20, Platform = Platform.X64 } });
    AddJob(new Job { Environment = { Runtime = CoreRuntime.Core20, Platform = Platform.X86 } });
    AddJob(new Job { Environment = { Runtime = CoreRuntime.Core31, Platform = Platform.X64 } });
    AddJob(new Job { Environment = { Runtime = CoreRuntime.Core31, Platform = Platform.X86 } });
    */
    this.AddJob(new Job { Environment = { Runtime = CoreRuntime.Core60, Platform = Platform.X64 } });
    this.AddJob(new Job { Environment = { Runtime = CoreRuntime.Core60, Platform = Platform.X86 } });
  }
}

[Config(typeof(BenchmarkConfiguration))]
[MemoryDiagnoser]
public class String_SubString {

  [Params(null,"","a","abc")]
  public string text;

  [Params(0,1,-1,-100)]
  public int start;

  [Params(0,1,2,-1,-99)]
  public int end;

  [Benchmark(Baseline = true)] public void BaseLine() => this.text.SubString(this.start,this.end);
  [Benchmark] public void Branchless() => SubString(this.text, this.start, this.end);
  [Benchmark] public void WithBranches() => SubString2(this.text, this.start, this.end);

  public static string SubString(string @this, int start, int end = 0) {
    Against.ThisIsNull(@this);

    var length = @this.Length;
    if (length <= 0)
      return string.Empty;

    // if (start < 0) start += length;
    start += length & (start >> 31);

    // if (start < 0) start = 0;
    start &= ~start >> 31;

    // if (end <= 0) end += length;
    end += length & ((end - 1) >> 31);

    if (start == end)
      return @this[start].ToString();

    var len = end - start;
    // if (len > length) len = length - start;
    len -= (len - (length - start)) & ((length - len) >> 31);

    // when reading too less chars -> returns empty string
    if (len <= 0)
      return string.Empty;

    return @this.Substring(start, len);
  }

  public static string SubString2(string @this, int start, int end = 0) {
    Against.ThisIsNull(@this);

    var length = @this.Length;
    if (length <= 0)
      return string.Empty;

    if (start < 0)
      start += length;

    if (start < 0)
      start = 0;

    if (end <= 0)
      end += length;

    if (start == end)
      return @this[start].ToString();

    var len = end - start;
    if (len > length)
      len = length - start;

    // when reading too less chars -> returns empty string
    if (len <= 0)
      return string.Empty;

    return @this.Substring(start, len);
  }

}

[Config(typeof(BenchmarkConfiguration))]
[MemoryDiagnoser]
public class String_ExchangeAt {

  [Params(null, "ABC")] public string Source;

  [Params(-1, 0, 1)] public int Index;

  [Params(null, "DEF")] public string Replacement;

  [Benchmark(Baseline = true)]
  public string CurrentImplementation() => this.Source.ExchangeAt(this.Index, this.Replacement);

  [Benchmark]
  public string NaiveImplementation() => _Naive(this.Source, this.Index, this.Replacement);

  [Benchmark]
  public string SelfAllocImplementation() => _SelfAlloc(this.Source, this.Index, this.Replacement);

  private static string _Naive(string @this, int index, string replacement) {
    if (@this == null || index <= 0)
      return replacement;

    return @this[..index] + replacement;
  }

  private static string _SelfAlloc(string @this, int index, string replacement) {
    if (@this == null || index <= 0)
      return replacement;

    if (index > @this.Length)
      index = @this.Length;

    if (replacement is not { Length: > 0 })
      return @this[..index];

    var result = new char[index + replacement.Length];
    @this.CopyTo(0, result, 0, index);
    replacement.CopyTo(0, result, index, replacement.Length);

    return new(result);
  }


}