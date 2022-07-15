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

namespace System.Collections.Generic {
  /// <summary>
  /// Interface for Queues
  /// </summary>
  /// <typeparam name="T">the type of items in the queue</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  interface IQueue<T> {
    /// <summary>
    /// Dequeues an item or blocks until there is one.
    /// </summary>
    /// <returns>the item</returns>
    T Dequeue();
    /// <summary>
    /// Enqueues an item.
    /// </summary>
    /// <param name="item">The item.</param>
    void Enqueue(T item);
    /// <summary>
    /// Tries to dequeue an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    bool TryDequeue(out T item);
    /// <summary>
    /// Gets the number of items contained.
    /// </summary>
    /// <value>The number of items</value>
    int Count {
      get;
    }
    /// <summary>
    /// Gets a value indicating whether this queue is empty.
    /// </summary>
    /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
    bool IsEmpty {
      get;
    }
    /// <summary>
    /// Clears this instance.
    /// </summary>
    void Clear();
  }
}