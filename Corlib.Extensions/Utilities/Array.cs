namespace Utilities;
internal class Array {

#if !SUPPORTS_ARRAY_EMPTY

  private static class EmptyArray<T> {
    public static readonly T[] Empty = new T[0];
  }
  
#endif

  public static T[] Empty<T>()
#if SUPPORTS_ARRAY_EMPTY
    => System.Array.Empty<T>()
#else
    => EmptyArray<T>.Empty
#endif
    ;
  
}
