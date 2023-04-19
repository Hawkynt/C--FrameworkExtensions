using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

using System;

public static class Program {
  public static void Main() => BenchmarkRunner.Run<String_ExchangeAt>();
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
    AddJob(new Job { Environment = { Runtime = CoreRuntime.Core60, Platform = Platform.X64 } });
    AddJob(new Job { Environment = { Runtime = CoreRuntime.Core60, Platform = Platform.X86 } });
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