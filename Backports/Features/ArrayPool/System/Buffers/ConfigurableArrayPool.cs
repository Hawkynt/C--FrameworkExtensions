// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software: 
// you can redistribute and/or modify it under the terms 
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that 
// it will be useful, but WITHOUT ANY WARRANTY without even the implied 
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !SUPPORTS_ARRAYPOOL

using System.Threading;

namespace System.Buffers;

internal sealed class ConfigurableArrayPool<T> : ArrayPool<T> {
  private const int DEFAULT_MAX_ARRAY_LENGTH = 1024 * 1024; // 1MB
  private const int DEFAULT_MAX_ARRAYS_PER_BUCKET = 50;

  private readonly Bucket[] _buckets;
  private readonly int _maxArrayLength;
  
  [ThreadStatic]
  private static ThreadLocalCache<T> _tlsBuckets;

  public ConfigurableArrayPool(int maxArrayLength, int maxArraysPerBucket) {
    if (maxArrayLength < 1)
      throw new ArgumentOutOfRangeException(nameof(maxArrayLength));
    if (maxArraysPerBucket < 1)
      throw new ArgumentOutOfRangeException(nameof(maxArraysPerBucket));

    this._maxArrayLength = maxArrayLength;
    
    // Determine number of buckets
    var maxBuckets = 0;
    var maxSize = 1;

    while (maxSize <= maxArrayLength) {
      ++maxBuckets;
      maxSize <<= 1;
    }

    this._buckets = new Bucket[maxBuckets];
    for (var i = 0; i < this._buckets.Length; ++i)
      this._buckets[i] = new(1 << i, maxArraysPerBucket);
  }

  public ConfigurableArrayPool() : this(DEFAULT_MAX_ARRAY_LENGTH, DEFAULT_MAX_ARRAYS_PER_BUCKET) { }

  #region Overrides of ArrayPool<T>

  /// <inheritdoc />
  public override T[] Rent(int minimumLength) {
    switch (minimumLength) {
      case < 0:
        throw new ArgumentOutOfRangeException(nameof(minimumLength));
      case 0:
        return [];
      default:
        if (minimumLength > this._maxArrayLength)
          return new T[minimumLength];
        
        break;
    }
    
    var bucketIndex = this.GetBucketIndex(minimumLength);

    // First try thread-local storage
    var tlsBuckets = _tlsBuckets ??= new(this._buckets.Length);
    var rentedArray = tlsBuckets.Rent(bucketIndex);
    if (rentedArray != null)
      return rentedArray;

    if (bucketIndex >= this._buckets.Length)
      return new T[GetSizeForBucket(bucketIndex)];

    // Try from shared buckets
    rentedArray = this._buckets[bucketIndex].Rent();
    if (rentedArray != null)
      return rentedArray;

    // Create new array of exact power of 2 size
    return new T[GetSizeForBucket(bucketIndex)];
  }

  /// <inheritdoc />
  public override void Return(T[] array, bool clearArray = false) {
    if (array == null)
      throw new ArgumentNullException(nameof(array));

    var arrayLength = array.Length;
    if (arrayLength == 0)
      throw new ArgumentException("The buffer is not associated with this pool and may not be returned to it. (Parameter 'array')");

    if (arrayLength > this._maxArrayLength)
      return;

    if ((arrayLength & (arrayLength-1)) != 0)
      throw new ArgumentException("The buffer is not associated with this pool and may not be returned to it. (Parameter 'array')");

    var bucketIndex = this.GetBucketIndex(arrayLength);
    if (clearArray)
      Array.Clear(array, 0, array.Length);

    // Try to return to thread-local first
    var tlsBuckets = _tlsBuckets ??= new(this._buckets.Length);
    if (tlsBuckets.Return(array, bucketIndex))
      return;

    // If thread-local is full, try the shared bucket
    if (bucketIndex < this._buckets.Length)
      this._buckets[bucketIndex].Return(array);

    // If both are full, let GC collect it
  }
  #endregion

  private int GetBucketIndex(int length) {
    var index = 0;
    var size = 1;

    while (size < length && index < this._buckets.Length) {
      ++index;
      size <<= 1;
    }

    return index;
  }

  private static int GetSizeForBucket(int bucketIndex) => 1 << bucketIndex;

  private sealed class ThreadLocalCache<TItem> {
    private readonly TItem[][] _buckets;

    internal ThreadLocalCache(int size) => this._buckets = new TItem[size][];

    internal TItem[] Rent(int bucketIndex) {
      if (bucketIndex < 0 || bucketIndex >= this._buckets.Length)
        return null;

      var array = this._buckets[bucketIndex];
      if (array == null)
        return null;

      this._buckets[bucketIndex] = null;
      return array;
    }

    internal bool Return(TItem[] array, int bucketIndex) {
      if (bucketIndex < 0 || bucketIndex >= this._buckets.Length)
        return false;

      if (this._buckets[bucketIndex] != null)
        return false;

      this._buckets[bucketIndex] = array;
      return true;
    }
  }

  private sealed class Bucket {
    private readonly T[][] _arrays;
    private readonly int _bufferLength;
    private volatile int _index;

    internal Bucket(int bufferLength, int numberOfArrays) {
      this._bufferLength = bufferLength;
      this._arrays = new T[numberOfArrays][];
      this._index = 0;
    }

    internal T[] Rent() {
      // Try to atomically decrement the index
      while (true) {
        var currentIndex = this._index;

        // Check if there are arrays available
        if (currentIndex <= 0)
          return null;

        var newIndex = currentIndex - 1;
        var array = this._arrays[newIndex];
        if (Interlocked.CompareExchange(ref this._index, newIndex, currentIndex) != currentIndex)
          continue;

        // Clear the reference in case no one else already returned another array onto this index
        Interlocked.CompareExchange(ref this._arrays[newIndex], null, array);
        return array;
      }
    }

    internal void Return(T[] array) {
      // Verify this is the right size array
      if (array.Length != this._bufferLength)
        return;

      // Try to atomically increment the index
      while (true) {
        var currentIndex = this._index;

        // Check if the bucket is full
        if (currentIndex >= this._arrays.Length)
          return; // Let GC collect it

        // Try to store the array and increment the index atomically
        // If CAS failed, someone else modified _index, try again
        if (Interlocked.CompareExchange(ref this._index, currentIndex + 1, currentIndex) != currentIndex)
          continue;

        this._arrays[currentIndex] = array;
        return;
      }
    }
  }

}
#endif