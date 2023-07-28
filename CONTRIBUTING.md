# Contributing guidelines

## Foreword

These extensions are meant to be extended by everyone who is firm in [C#](https://dotnet.microsoft.com/en-us/learn/csharp).

I appreciate every idea making these classes

* go faster
* use less memory
* have more throughput
* less worst/average-case-[complexity](https://en.wikipedia.org/wiki/Big_O_notation)
* fix things Microsoft has [overseen](https://softwareengineering.stackexchange.com/questions/296445/whats-the-use-of-any-in-a-c-list)
* or simply teaching them new tricks
  
However, this is not my daily business and I kindly ask you for some time to read through your pull-requests and issues.
Please make it as easy for me as you can by avoiding things like

* different code style
* hard-to-figure-out-stuff without meaningful comments
* huge changesets (try to push more often, that helps)
* endless discussions (I'm the captain and the captain is in charge of this spaceship)

## So, you are brave enough?

There are some guidelines for extensions which have proven one's worth:

* Every referenced assembly/package should have its own project/assembly
* Use folders for every part of the namespace
* Every file in there should have a name that is build like this: "**Type** *.cs*"
* The classname is always "**Type***Extensions*". The class is always `internal/public static partial`, thus allowing us to extend it further in a given project by adding another partial class with the same name
* All **public** methods must be **static**
* For extensions to static classes like **Math** or **Activator**,
  there is no **This**-parameter
* Get a test for your contribution under "**Tests**"-*folder* following the examples already in place
* Do not write [too many](https://en.wikipedia.org/wiki/Equivalence_partitioning) tests
* But do write [enough](https://en.wikipedia.org/wiki/Boundary-value_analysis) tests

## Are you keen on refactoring?

You can go on and refactor whatever you think is necessary to make the code more readable or adapt more recent .Net versions.
However don't make the code slower or more memory-hungry during refactoring.
Pay kind attention to details, escpecially all that compiler-sugar ([async](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async)/[await](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/await), [yield](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/yield), [lambdas](https://medium.com/criteo-engineering/beware-lambda-captures-383efe3a4345), [Patterns](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns), [LINQ](https://www.youtube.com/watch?v=Dv_nsoEmC7s&list=PLzQZKn8ki7X1XhXSjaSQpRr4Am1uFK4fo)) and [boxing](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing).
C# is doing a lot under the hood which you only see when using [dotPeek](https://www.jetbrains.com/decompiler/), [dnSpy](https://github.com/dnSpy/dnSpy), [Reflector](https://www.red-gate.com/products/reflector/), [ILSpy](https://github.com/icsharpcode/ILSpy), [SharpLab](https://sharplab.io/) or any other decompilation tool.
You should make yourself comfortable with the [difference](https://www.c-sharpcorner.com/article/stack-vs-heap-memory-c-sharp/) between [heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals) and [stack](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc) allocations, know the [large object heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap) and its size ([~82KB](https://devblogs.microsoft.com/dotnet/large-object-heap-uncovered-from-an-old-msdn-article/)).

Ask if unsure and [learn](https://www.youtube.com/watch?v=Tb2Fx9qku_o) about [micro-optimizations](https://www.specbranch.com/posts/intro-to-micro-optimization/) and [why they are needed](https://medium.com/google-developers/the-truth-about-preventative-optimizations-ccebadfd3eb5).
If you really feel it, create benchmark code under "Tests" like the one already in place.

BTW: I am using [JetBrains](https://www.jetbrains.com) [ReSharper](https://www.jetbrains.com/resharper/) so don't wonder upon specific comments for it.

## Hardcore-Fixer?

Everyone can learn new things and nobody is perfect.
Some things just don't work (yet) like I like them to be.
If you gonna fix actions, code, tests, docs or whatever just let me know.

## Code-Conventions

### Naming Style

* based loosely on Microsoft [guidelines](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
* using [camelCase](https://www.theserverside.com/answer/Pascal-case-vs-camel-case-Whats-the-difference) `changeTheWorld` and [PascalCase](https://www.freecodecamp.org/news/snake-case-vs-camel-case-vs-pascal-case-vs-kebab-case-whats-the-difference/) `LeaveAsIs`
* everything that is a variable is camelCase: `int myInt = 40;`
* everything private/protected is prefixed by underscore: `private string _myText;`
* constants cry for help: `private const int _MY_SECRET_ID = 0xdeadbeef;`
* pseudo-consts also cry if they are in a *private* room: `private static readonly string MY_SECRET_API_KEY = Settings.Default.ApiKey;`
* in *public* areas pseudo-consts give the impression of being a get-only property but with more access speed: `public static readonly string TheBigBadWolf = "What does the fox say?";`
* methods want to *start doing* something *big*: `public void InsertStuffIntoDatabase() { }`
* interfaces are self**I**sh: `public interface IKnowBetter { }`
* abstract classes prefix with **A**: `public abstract class AKnowBetterBase { }`
* generic type parameters do **T**-poses: `public void DoThatThing<TItem, TResult>(Func<TItem, TResult> renderer) { }`
* enums, enum-members, classes, namespaces, structs, records, properties all use **PascalCase**: `public class Car { }`
* avoid acronyms if they're not globally known (e.g. Id)
* if "proper names" are used, convert them to camelCase/PascalCase: `public static void DoForIetfLanguageTag`
* *bool*s want to start with something like *Has*, *Can*, *Exists*, *Contains*, *Try*
* anything doing [P/Invoke](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke) is in a **private nested class** named **NativeMethods**

``` cs
public static partial class IntPtrExtensions {
  private static class NativeMethods {
    [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
    public static extern IntPtr _MemoryCopy(IntPtr dst, IntPtr src, int count);
  }

  public static void CopyTo(this IntPtr @this, IntPtr other, int count)
    => NativeMethods._MemoryCopy(other, @this, count)
    ;
}
```

* The first parameter of all *public* methods that extend a given class must be the type itself and is called **@this** or alternatively *This* (only for old legacy code)

### Formatting Style

* there is no tab, only two spaces for indendation
* brackets are [K&R](https://en.wikipedia.org/wiki/Indentation_style#K&R_style)-style
* try to avoid nesting using multiple return-statements for shortcuts even though this means code duplication

``` cs
// BAD:
public void CheckInFile(File file) {
  if (file != null) {
    if (file.IsCheckedOut) {
      if(file.IsOpen) {
        file.Close();
      }

      file.CheckIn();
    }
  }
}

// GOOD:
public void CheckInFile(File file) {
  if (file == null)
    return;

  if (!file.IsCheckedOut)
    return;
  
  if(file.IsOpen)
    file.Close();

  file.CheckIn();
}
```

* indent statements in a bracketed scope and also if they are single-statement blocks

``` cs
public void TestStuff(bool assertionFailed) {
  if (assertionFailed)
    FailHard();
  else {
    KillingMeSoftly();
    // with his words
  }
}
```

* when, for whatever reason, you need to shorten lines, do so at operators, brackets or after commas and indent

``` cs
public void MarkStuff(
  string text,
  int appendCount,
  bool zOrderReversal,
  IntPtr dangerousThingy
) {
  
  var linqResult = (
    from character in text
    where zOrderReversal
    select character
  ).ToArray();

  var lastResort = text
    .Where(c => c < ' ')
    .Select(c => (char)(c + 65))
    ;

  Func<int, bool> lambda = l 
    => (l < appendCount)
    && zOrderReversal
    ;

  var whatEverFits = text.Any()
    ? appendCount
    : linqResult.Length
    ;

}

public void ShowMeExpressions()
  => DoItNow()
  ;
```

* regions go where code goes

``` cs
class TaxiDriver {

  // BAD:
#region ctor
  public TaxiDriver() { }
#endregion

  // GOOD:
  #region dtor
  ~TaxiDriver() { }
  #endregion

}
```

* pre-processor directives always start left

``` cs
class TaxiDriver {

  // BAD:
  #if ALLOW_TAXI_CTOR
  public TaxiDriver() { }
  #endif

  // GOOD:
#if ALLOW_TAXI_DTOR
  ~TaxiDriver() { }
#endif

}

```

* the only valid way to indent switch-statements is this

``` cs
switch (onWhateverVariable) {
  case 0:   // single line statement
    break;
  case 1: { // scope with explicit "fall-through"
    var text = "I need a dollar";
    goto case 2;
  }
  case 2: { // block statement with scope
    var text = "I need a block";
    return "later";
  }
  default:  // early return
    return "early";
}
```

* there is lots of space(s) but never more than one contiguous blank line

``` cs
var spacesBefore = "andAfterEqualSigns";
var spaces = "BeforeAnd" + "AfterOperators";
var spaceBag = new [] { "spaces", "after", "commas" }

// this also applies to for, foreach, while, lock, using, fixed, catch, when, etc.
if (spaceIsNotEnclosingConditions) {

  // just here to make it multiline
  throw new InvalidOperationException("I'm gonna cry" + ((99 + 1) * 10) + " times")
}

return "successfully typed spaces";
```

* put spaces around operators but beware of these special cases

``` cs
++i;
j++;
k = -k;
l = +k;
pointer = &value;
copy = *pointer;
result = !input;
```

* try to get methods on one screen page (ca. 60 lines of code, max. 120 characters per line)

### File Layout

* usings first
* put nested classes up or separate them into partials named *Class.NestedClass.cs*
* put constants below
* try to get fields now
* now props
* now cctor/ctor/dtor/Dispose
* now methods
* if a field is only needed for a single method (e.g. backing field, memoization cache), put it directly in front of the method
* partial classes are OK, especially when class-files grow big and have logically-connected blocks - name them **ClassName.BlockName.cs**
* when having static stuff, move that before the instance members of the same type
* when attributing stuff, make sure each attribute gets its own line and brackets - merge with parameters if single attribute

``` cs
// BAD:
[NotNullWhen(true), MethodImpl(MethodImplOptions.AggressiveInlining)]
bool DoStuff( [NotNull, Localizable] string a) { }

[MethodImpl(MethodImplOptions.AggressiveInlining)]bool DoStuff2(
  [NotNull] string b
) { }

// GOOD:
[NotNullWhen(true)]
[MethodImpl(MethodImplOptions.AggressiveInlining)]
bool DoStuff(
  [NotNull]
  [Localizable] 
  string a
) { }

[MethodImpl(MethodImplOptions.AggressiveInlining)]
bool DoStuff2([NotNull] string b) { }
```

### Namespaces and Usings

* The namespace in the file is always the same namespace as the original type is in
* use file based namespaces if applicable
* only one namespace per file, multiple types however are perfectly fine
* order usings alphabetically (if not logically coherent in a #if-directive)
* no *global usings* file
* no **global::** prefix
* try to avoid *static aliasing*

### Syntax Style

* use `var` for all declarations possible
* spare the type after new if the compiler knows what you doing, use together with var to your maximum saving of typing

``` cs
// BAD:
Dictionary<string, List<string>> cache = new Dictionary<string, List<string>>();

// BETTER:
var cache = new Dictionary<string, List<string>>();

// GOOD:
Dictionary<string, List<string>> cache = new();
```

* when assigning `null` or `default`, prefer explicit type over `var`

``` cs
// BAD:
var cache = (Dictionary<string, List<string>>)null;

// WORSE:
Dictionary<string, List<string>> cache = (Dictionary<string, List<string>>) null;
Dictionary<string, List<string>> cache = default(Dictionary<string, List<string>>);

// GOOD:
Dictionary<string, List<string>> cache = null;

// ALSO GOOD:
Dictionary<string, List<string>> cache = default;
```

* use `this.` for everything that accesses an instance member
* use keywords for types when available (like `string`, `int`, `float`, `bool`)
* use explicit access modifiers (like `public`, `protected`, `private`, `internal`)
* don't forget to dispose stuff -  use `using`-Blocks if possible, use the right `try { } finally { }`-pattern when needed

``` cs
// BAD:
var myDisposable = new DisposableClass();
...
myDisposeable.Dispose();

// WORSE:
var myDisposable = new DisposableClass();
...

// BETTER: if needed because something hasn't IDisposable implemented
DisposableClass myDisposable = null;
try {
  myDisposable = new DisposableClass();
  ...
} finally {
  myDisposable?.Dispose();
}

// GOOD:
using (var myDisposable = new DisposableClass()) {
  ...
}

// ALSO GOOD: auto-dipose upon end of current scope
using var myDisposable = new DisposableClass();
...
```

* use expression bodies when possible

``` cs
// BAD:
public int CalculateStuff() {
  return (int)(15 * 2 + Math.PI);
}

// GOOD:
public int CalculateOtherStuff() => (int)(15 * 2 + Math.PI);

// ALSO GOOD:
public double CalculateTheThirdStuff() 
  => 15 * 2 + Math.PI
  ;
```

* use anonymous/calculated properties when possible

``` cs
//BAD:
private int _chargeBackingField;

public int Charge {
  get {
    return this._chargeBackingField;
  }
  set {
    this._chargeBackingField = value;
  }
}

// WORSE: C# is not Java
private int _chargeBackingField;

public int GetCharge() {
  return this._chargeBackingField;
}

public void SetCharge(int value) {
  this._chargeBackingField = value;
}

// BETTER:
private int _chargeBackingField;

public int Charge {
  get => this._chargeBackingField;
  set => this._chargeBackingField = value;
}

// GOOD:
public int Charge { get; set; }
```

* use property and collection initializer syntax when applicable

``` cs
// BAD:
var guest = new User();
guest.Name = "Alex";

var guest2 = new User();
guest2.Name = "Michael";
guest2.Age = 32;

var guests = new List<User>();
guests.Add(guest);
guests.Add(guest2);

// GOOD:
var guest = new User { Name = "Alex" };
var guest2 = new User { Name = "Michael", Age = 32 };
var guests = new List<User> { guest, guest2 };

// GOOD: combining with inlining on single line in simple statements is OK
var alexOnly = new List<User> { new User { Name = "Alex" } };

// GOOD: using line-breaks to see what's going on
var guests = new List<User> { 
  new User { Name = "Alex" },
  new User { Name = "Michael", Age = 32 }
};

// ALSO GOOD: sometimes property lists are longer than in this example
var guests = new List<User> { 
  new User { 
    Name = "Alex" 
  },
  new User { 
    Name = "Michael", 
    Age = 32 
  }
};
```

* learn about postfix and prefix operators if you don't know the [difference](https://riptutorial.com/csharp/example/8183/postfix-and-prefix-increment-and-decrement) yet
* seal nested classes to improve [performance](https://www.meziantou.net/performance-benefits-of-sealed-class.htm)
* use ternary-operator if possible

``` cs
// BAD:
if ( c < 15)
  return "below";
else
  return "above";

// GOOD:
return c < 15 ? "below" : "above";
```

* don't reserve empty blocks to make room for the "future"

``` cs
// BAD:
if ( i > 25) {
  ...
} else { 
  // in case we need the else someday
}

switch (j) {
  case 1:
    // in case we need this someday
    break;
  case 2:
    return "OK";
  default:
    // in case we need this someday
    break;
}

// GOOD:
if ( i > 25) {
  ...
}

switch (j) {
  case 2:
    return "OK";
  case 1:
  default:
    throw new NotSupportedException();
}

// in this case it would be even better to use this code
if (j == 2)
  return "OK";

throw new NotSupportedException();
```

* try to avoid methods with more than 8 parameters and using *Tuples* with more than 5 type-parameters
* instead of using [optional values](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments) in parameters, prefer overloads (optional values are baked into [caller-site](https://stackoverflow.com/questions/30317625/does-adding-optional-parameters-change-method-signatures-and-would-it-trigger-me) upon compilation and are hard to change afterwards)

``` cs
// BAD:
public static void DoRandomStuff(this string @this, int startAt = 0, int count = -1) {
  ...
}

// GOOD:
public static void DoRandomStuff(this string @this) 
  => DoRandomStuff(@this, 0, -1)
  ;

public static void DoRandomStuff(this string @this, int startAt)
  => DoRandomStuff(@this, startAt, -1)
  ;

public static void DoRandomStuff(this string @this, int startAt, int count) {
  ...
}
```

### Null-Checking and validation

* always throw the [most concise](https://stackoverflow.com/questions/774104/what-exceptions-should-be-thrown-for-invalid-or-unexpected-parameters-in-net) [exception](https://learn.microsoft.com/en-us/dotnet/api/system.exception)
* throw [ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception) when something is wrong with a given argument
* throw [ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception) when a given argument is null but it shouldn't
* throw [ArgumentOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception) when an indexer, number or count is simply not within a valid range
* throw [InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception) when an argument might be valid but the object is currently in a state where that value is not accepted
* throw [NotSupportedException](https://learn.microsoft.com/en-us/dotnet/api/system.notsupportedexception) when an argument is technically valid, but the code to handle it was not (yet) implemented
* use [Guard](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/Corlib.Extensions/Guard/Against.cs)-clauses instead of checking yourself (they are already tuned for performance in most cases)

``` cs
public static void DoSomething(this string @this, string other, int count) {
  Guard.Against.ThisIsNull(@this);
  Guard.Against.ArgumentIsNull(other);
  Guard.Against.CountBelowOrEqualZero(count);
  ...
}
```

* [guard-clauses](https://maximegel.medium.com/what-are-guard-clauses-and-how-to-use-them-350c8f1b6fd2) come first and they always check given parameters - they're meant to be used as preconditions upon entering a method - do not use them inside the method

``` cs
public static void DoSomething(this string @this) {
  ...
  // BAD: late-checking preconditions
  Guard.Against.ThisIsNull(@this);
  ...
  var index = ...; // someindex

  // BAD: check on local variable
  Guard.Against.IndexOutOfRange(count);
  ...
}
```

* validate arguments in order of appearance

``` cs
// BAD:
public static void DoSomething(this string @this, string other, int count) {
  Guard.Against.CountBelowOrEqualZero(count);
  Guard.Against.ThisIsNull(@this);
  Guard.Against.ArgumentIsNull(other);
  ...
}

// GOOD:
public static void DoSomething(this string @this, string other, int count) {
  Guard.Against.ThisIsNull(@this);
  Guard.Against.ArgumentIsNull(other);
  Guard.Against.CountBelowOrEqualZero(count);
  ...
}
```

* do validate all public data ([taint-mode](https://en.wikipedia.org/wiki/Taint_checking)), avoid duplicate validations in private code

``` cs
public static void DoSomething(this string @this) {
  Guard.Against.ThisIsNull(@this);
  _DoSpecificStuff(@this);
}

private static void _DoSpecificStuff(string @this) {
  // BAD: double-checking
  Guard.Against.ThisIsNull(@this);
  ...
}
```

* use throw-helpers if possible from the [AlwaysThrow](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/Corlib.Extensions/Guard/AlwaysThrow.cs)-class

``` cs
public static void DoSomething<T>(this IEnumerable<T> @this) {
  ...
  if (!@this.Any())
    AlwaysThrow.NoElements();
}
```

* if no suitable checks or throw-helpers exist (yet), add your own into the Guard-class taking existing code as an example

``` cs
/* Goes into Against.cs */
// GOOD: better debug experience because method is stepped through
[DebuggerHidden]
// GOOD: compiler will try to inline the call and optimize it away entirely if not needed
#if SUPPORTS_INLINING
[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
public static void EarlyMornings(DateTime date) {
  if(date.Hour < 7)
    AlwaysThrow.NoCokeYetException();
}

/* Goes into AlwaysThrow.cs */
[DebuggerHidden]
// GOOD: never inline throw-helpers, it would hurt performance by preventing the calling method to get inlined
[MethodImpl(MethodImplOptions.NoInlining)]
// GOOD: tell the static code analyzer that we won't return ever
[DoesNotReturn]
// GOOD: name like the exception you're gonna throw
public static void NoCokeYetException() => throw new NoCokeYetException();
```

* when adding specific stuff for numbers use the [Against.T4.tt](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/Corlib.Extensions/Guard/Against.T4.tt) to avoid writing nearly identical code for each number type

``` cs
<#foreach (var type in new[]{"sbyte", "short", "int", "long", "float", "double", "decimal"}) {#>
  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  // GOOD: use CallerMemberName and CallerArgumentExpression to let the compiler work for you
  public static void NegativeValues(<#=type#> value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.ArgumentBelowRangeException(expression ?? nameof(value), value, 0, caller);
  }
<#}#>
```

* wrap P/Invoke methods inside the *NativeMethods* class and handle exceptions and type conversion there

``` cs
public static partial class IntPtrExtensions {

  private static class NativeMethods {

    public interface IWindowHandle { }

    private readonly record struct WindowHandle(IntPtr Handle) : IWindowHandle {
      public static bool IsInvalid(IntPtr handle) => handle == IntPtr.Zero || (long)handle < 0;
    }
    
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "FindWindow")]
    private static extern IntPtr _FindWindow(string lpClassName, string lpWindowName);

    public static IWindowHandle FindWindow(string lpClassName, string lpWindowName) {
      var result = _FindWindow( lpClassName, lpWindowName);
      if (WindowHandle.IsInvalid(result))
        throw new Win32Exception();

      return new WindowHandle(result);
    }

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetForegroundWindow")]
    private static extern bool _SetForegroundWindow(IntPtr hWnd);

    public static void SetForegroundWindow(IWindowHandle hWnd) {
      if (hWnd is not WindowHandle handle)
        throw new InvalidOperationException("Window handle not created from within this class");

      if (_SetForegroundWindow(handle.Handle))
        return;

      throw new Win32Exception();
    }

  }
}
```

### Other stuff

* use [T4](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates) to save you from writing nearly identical code in general, name the [text-templating](https://learn.microsoft.com/en-us/visualstudio/modeling/guidelines-for-writing-t4-text-templates) files **ClassName.T4.tt**
* the endless loop lets the *for* cry

``` cs
for (;;) {
  ...
}
```

* *for*-[loops](https://softwareengineering.stackexchange.com/a/164554/33478) use prefix increment/decrement

``` cs
// BAD:
for (var i = 0; i < 100; i++) { ... }

// GOOD:
for (var i = 0; i < 100; ++i) { ... }
```

* some tricky stuff is welcome as an exercise for the reader

``` cs
// for without init
for (; j != 20 ; ++j) { ... }

// multi statements
for (int i = 5, j = 20; i < j ; ++i, --j) { ... }

// no footer
for (int i = 5; i > 0 ;) { ... }

// multi-assignments
var a = b = 20;

// multiline-multi-assignments
var a 
    = b 
    = 20
    ;

// even in conditions
while ( (c = 20) < b ) { ... }
```

* don't trust in the intelligence of the compiler and read about

  * [Block-Processing](https://www.c-sharpcorner.com/article/fast-equality-comparison/)
  * [Branchless-Assignments](https://blog.joberty.com/branchless-programming-why-your-cpu-will-thank-you/)
  * [Bounds-Checks](https://www.codeproject.com/articles/844781/digging-into-net-loop-performance-bounds-checking) and how to [get rid](https://tooslowexception.com/getting-rid-of-array-bound-checks-ref-returns-and-net-5/) of them
  * [Cache-Blocking](https://www.intel.com/content/www/us/en/developer/articles/technical/cache-blocking-techniques.html)
  * [Duff's-Device](https://en.wikipedia.org/wiki/Duff%27s_device)
  * [Branch-Tables](https://en.wikipedia.org/wiki/Branch_table)
  * [Loop Optimization](https://en.wikipedia.org/wiki/Loop_optimization) and [Unrolling](https://en.wikipedia.org/wiki/Loop_unrolling) (T4 can assist here)
  * [Out-of-order execution](https://en.wikipedia.org/wiki/Register_renaming)
  * [Non-Blocking](https://en.wikipedia.org/wiki/Non-blocking_algorithm)
  * [Pre-Allocation](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.-ctor#system-collections-generic-list-1-ctor(system-int32)) and [Object-Pooling](https://en.wikipedia.org/wiki/Object_pool_pattern)
  * [Spans](https://www.codemag.com/Article/2207031/Writing-High-Performance-Code-Using-SpanT-and-MemoryT-in-C)
  * how to turn [Recursion to Iteration](https://www.baeldung.com/cs/convert-recursion-to-iteration) using [Stacks](https://www.cs.odu.edu/~zeil/cs361/latest/Public/recursionConversion/index.html), [Queues](https://stackoverflow.com/questions/159590/way-to-go-from-recursion-to-iteration) and [Tail-Calls](https://thomaslevesque.com/2011/09/02/tail-recursion-in-c/)
  * [Tail-Calls](https://github.com/dotnet/runtime/issues/2191) in general

* do not depend on more than [Backports](https://www.nuget.org/packages/FrameworkExtensions.Backports) and [Corlib.Extensions](https://www.nuget.org/packages/FrameworkExtensions.Corlib)

### Architecture

* use [design-patterns](https://en.wikipedia.org/wiki/Software_design_pattern)
* use [SOLID](https://en.wikipedia.org/wiki/SOLID)
* [YAGNI](https://en.wikipedia.org/wiki/You_aren%27t_gonna_need_it) does not apply here, you are enabling people to do more than you know right now
* however [KISS](https://en.wikipedia.org/wiki/KISS_principle) for the public interface is totally welcomed
* [over-engineering](https://en.wikipedia.org/wiki/Overengineering) is OK for performance and memory usage because you don't know the context in which this code is gonna be used later on
* when you violate stuff on purpose, comment why you did so

### Try to have fun

* not everything is set in stone
* there are the "it depends"-cases
* there's always a "does-not-fit-here"-case
* and there will always be code not up-to-date to these standards for whatever boring reasons
* just leaving the files cleaner than before will help steering into the right direction (so look-around +/- 10 lines from where you made changes and tidy them if you can)
