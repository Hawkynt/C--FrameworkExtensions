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

namespace System.Collections.Generic; 

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class StackExtensions {
  /// <summary>
  /// Inverts the specified stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">The stack to invert.</param>
  public static void Invert<TItem>(this Stack<TItem> @this) {
    Guard.Against.ThisIsNull(@this);

    Queue<TItem> helpStack = new(@this.Count);
    while (@this.Count > 0)
      helpStack.Enqueue(@this.Pop());

    while (helpStack.Count > 0)
      @this.Push(helpStack.Dequeue());
  }

  /// <summary>
  /// Adds all given items to the stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <param name="items">The items to push on top of the stack.</param>
  public static void AddRange<TItem>(this Stack<TItem> @this, IEnumerable<TItem> items) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(items);

    foreach (var item in items)
      @this.Push(item);
  }

  /// <summary>
  /// Adds a given item to the stack.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <param name="item">The item to push on top of the stack.</param>
  public static void Add<TItem>(this Stack<TItem> @this, TItem item) {
    Guard.Against.ThisIsNull(@this);

    @this.Push(item);
  }

  /// <summary>
  /// Fetches one item.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Stack.</param>
  /// <returns>The top-most item.</returns>
  public static TItem Fetch<TItem>(this Stack<TItem> @this) {
    Guard.Against.ThisIsNull(@this);

    return @this.Pop();
  }
  
#if !SUPPORTS_STACK_TRYPOP
  /// <summary>
  /// Returns a value that indicates whether there is an object at the top of the <see cref="Stack{T}"/>, and if one is present, copies it to the <paramref name="result"/> parameter, and removes it from the <see cref="Stack{T}"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This <see cref="Stack{T}"/></param>
  /// <param name="result">If present, the object at the top of the <see cref="Stack{T}"/>; otherwise, the default value of T.</param>
  /// <returns><see langword="true"/> if there is an object at the top of the <see cref="Stack{T}"/>; <see langword="false"/> if the <see cref="Stack{T}"/> is empty.</returns>
  public static bool TryPop<TItem>(this Stack<TItem> @this, out TItem result) {
    Guard.Against.ThisIsNull(@this);
    
    if(@this.Count<1) {
      result = default;
      return false;
    }
    
    result = @this.Pop();
    return true;
  }
#endif
  
#if !SUPPORTS_STACK_TRYPEEK
  /// <summary>
  /// Returns a value that indicates whether there is an object at the top of the <see cref="Stack{T}"/>, and if one is present, copies it to the <paramref name="result"/> parameter. The object is not removed from the <see cref="Stack{T}"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This <see cref="Stack{T}"/></param>
  /// <param name="result">If present, the object at the top of the <see cref="Stack{T}"/>; otherwise, the default value of T.</param>
  /// <returns><see langword="true"/> if there is an object at the top of the <see cref="Stack{T}"/>; <see langword="false"/> if the <see cref="Stack{T}"/> is empty.</returns>
  public static bool TryPeek<TItem>(this Stack<TItem> @this, out TItem result) {
    Guard.Against.ThisIsNull(@this);
    
    if(@this.Count<1) {
      result = default;
      return false;
    }
    
    result = @this.Pop();
    return true;
  }
#endif
  
}