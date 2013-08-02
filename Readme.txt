This is the folder where all extensions to .NET classes go.

There are some rules for extensions which have proven one's worth:

* Every file in here should have a name that is build like this: "<Namespace>.<Type>Extensions.cs"
* The namespace in the files is always the same namespace as the original type is in.
* The classname is always "<Type>Extensions". The class is always "internal static partial", thus allowing us to extend it further in a given project
  by adding another partial class with the same name and preventing us from accidentially using the extension in another project in the same solution.
* All methods must be static.
* The first parameter of all "public" methods must be the type itself and is called "This". For extensions to static classes like "Math" or "Activator",
  there is no "This"-parameter.