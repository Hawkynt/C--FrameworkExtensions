namespace System.Collections.Generic {
#if !NET45_OR_GREATER && !NETCOREAPP && !NETSTANDARD

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  interface IReadOnlyDictionary<K, V> : IDictionary<K, V> { }

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class ReadOnlyDictionary<K, V> : Dictionary<K, V>, IReadOnlyDictionary<K, V> {

    public ReadOnlyDictionary(IDictionary<K,V> dictionary) : base(dictionary) {}
  }
#endif
}
