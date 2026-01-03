#region (c)2010-2042 Hawkynt

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

#endregion

// System.Dynamic was introduced in .NET 4.0
// Only polyfill for net20/net35 where no DLR exists
#if !SUPPORTS_DYNAMIC

using System.Threading;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides a cache for rules produced by <see cref="CallSiteBinder"/> instances.
/// </summary>
/// <typeparam name="T">The delegate type for the cached rules.</typeparam>
/// <remarks>
/// <para>
/// <see cref="RuleCache{T}"/> is used by <see cref="CallSite{T}"/> to store binding
/// results for reuse. This enables the DLR to avoid repeated binding operations
/// when the same types of arguments are encountered.
/// </para>
/// <para>
/// The cache uses a least-recently-used (LRU) eviction policy to manage memory.
/// </para>
/// </remarks>
internal sealed class RuleCache<T> where T : class {

  /// <summary>
  /// The maximum number of rules to cache per binder.
  /// </summary>
  private const int MaxRules = 10;

  /// <summary>
  /// The cached rules.
  /// </summary>
  private T[] _rules;

  /// <summary>
  /// Lock for thread-safe access.
  /// </summary>
  private readonly object _lock = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="RuleCache{T}"/> class.
  /// </summary>
  internal RuleCache() => this._rules = Array.Empty<T>();

  /// <summary>
  /// Gets the cached rules.
  /// </summary>
  /// <returns>An array of cached rule delegates.</returns>
  internal T[] GetRules() {
    lock (this._lock)
      return this._rules;
  }

  /// <summary>
  /// Adds a rule to the cache.
  /// </summary>
  /// <param name="newRule">The rule to add.</param>
  internal void AddRule(T newRule) {
    lock (this._lock) {
      var rules = this._rules;

      // Check if already cached
      for (var i = 0; i < rules.Length; ++i)
        if (ReferenceEquals(rules[i], newRule))
          return;

      // Add new rule
      T[] newRules;
      if (rules.Length < MaxRules) {
        newRules = new T[rules.Length + 1];
        newRules[0] = newRule;
        Array.Copy(rules, 0, newRules, 1, rules.Length);
      } else {
        // Cache full, replace oldest (last) rule
        newRules = new T[MaxRules];
        newRules[0] = newRule;
        Array.Copy(rules, 0, newRules, 1, MaxRules - 1);
      }

      this._rules = newRules;
    }
  }

  /// <summary>
  /// Moves a rule to the front of the cache (most recently used).
  /// </summary>
  /// <param name="rule">The rule to move.</param>
  internal void MoveRule(T rule) {
    lock (this._lock) {
      var rules = this._rules;
      if (rules.Length < 2)
        return;

      // Find the rule
      var index = -1;
      for (var i = 0; i < rules.Length; ++i) {
        if (ReferenceEquals(rules[i], rule)) {
          index = i;
          break;
        }
      }

      // If found and not already first, move to front
      if (index > 0) {
        var newRules = new T[rules.Length];
        newRules[0] = rule;
        Array.Copy(rules, 0, newRules, 1, index);
        Array.Copy(rules, index + 1, newRules, index + 1, rules.Length - index - 1);
        this._rules = newRules;
      }
    }
  }

}

#endif
