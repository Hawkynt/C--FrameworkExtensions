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

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

using Guard;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System;

partial class SpanHelper {

#pragma warning disable CS8500
  internal sealed unsafe class SharedPin<TElement> : IDisposable {
    private readonly object _source;
    private GCHandle? _handle;

    private const int IS_ALIVE = 0;
    private const int IS_DISPOSED = -1;
    private int _disposed = IS_ALIVE;
    private int _accessCounter;

    private const int AccessThreshold = 16;
    private const int SizeThreshold = 81920;
    private const int ItemsThreshold = 1024;

    public SharedPin(TElement[] array) {
      if (array == null)
        AlwaysThrow.ArgumentNullException(nameof(array));

      this._source = array;
    }

    public SharedPin(string str) {
      if (typeof(TElement) != typeof(char))
        AlwaysThrow.InvalidOperationException("TElement must be char when wrapping a string");

      if (str == null)
        AlwaysThrow.ArgumentNullException(nameof(str));
      
      this._source = str;
    }

    public bool IsPinned => this._handle != null;

    public void TrackAccess() {
      if (!MetaInfo<TElement>.IsPinable || this.IsPinned)
        return;

      var size = Unsafe.SizeOf<TElement>();
      var len = this._source switch {
        string s => s.Length,
        TElement[] a => a.Length,
        _ => 0
      };
      size *= len;

      if (len < ItemsThreshold && size < SizeThreshold && Interlocked.Increment(ref this._accessCounter) <= AccessThreshold)
        return;

      // forces immediate pinning
      _ = this.Pointer;
    }

    public TElement* Pointer {
      get {
        if (this._disposed != IS_ALIVE)
          AlwaysThrow.ObjectDisposedException(nameof(SharedPin<TElement>));

        if (this._handle is { } handle1)
          return (TElement*)handle1.AddrOfPinnedObject();
        
        lock (this) {
          if (this._disposed != IS_ALIVE)
            AlwaysThrow.ObjectDisposedException(nameof(SharedPin<TElement>));

          if (this._handle is { } handle2)
            return (TElement*)handle2.AddrOfPinnedObject();

          try {
            var handle = GCHandle.Alloc(this._source, GCHandleType.Pinned);
            this._handle = handle;
            return (TElement*)handle.AddrOfPinnedObject();
          } catch (ArgumentException) {
            return (TElement*)(this._source switch {
              TElement[] a => GetArrayPointer(a),
              _ => throw new NotSupportedException($"Can not get a pointer for type {typeof(TElement)}")
            });
          }
        }

        IntPtr GetArrayPointer(TElement[] a) {
          if (MetaInfo<TElement>.IsPinable)
            return Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);

          fixed (TElement* ptr = a)
            return (IntPtr)ptr; // WARNING: only valid within fixed
        }

      }
    }

    public void Dispose() {
      if (Interlocked.CompareExchange(ref this._disposed, IS_DISPOSED, IS_ALIVE) != IS_ALIVE)
        return;

      if (this._handle is { IsAllocated: true } handle)
        handle.Free();
    }

    ~SharedPin() {
      try {
        this.Dispose();
      } catch { }
    }

    private static class MetaInfo<T> {
      public static readonly bool IsPinable = typeof(T) == typeof(string) || typeof(T).IsPrimitive;
    }

  }
}
#endif