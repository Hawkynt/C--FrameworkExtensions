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

#if NETFRAMEWORK

using System.Data.Linq.Mapping;
using Guard;

namespace System.Data.Linq {
  internal static partial class TableExtensions {
    /// <summary>
    ///   Updates the row by re-attaching the entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="this">This Table.</param>
    /// <param name="entity">The entity.</param>
    public static void UpdateEntity<TEntity>(this Table<TEntity> @this, TEntity entity) where TEntity : class {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNull(entity);

      @this.Attach(entity);
      @this.Context.Refresh(RefreshMode.KeepCurrentValues, entity);
    }

    public static void Reattach<TEntity>(this Table<TEntity> @this, TEntity entity) where TEntity : class {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNull(entity);

      foreach (var pi in typeof(TEntity).GetProperties()) {
        if (pi.GetCustomAttributes(typeof(AssociationAttribute), false).Length <= 0)
          continue;

        // Property is associated to another entity
        var propType = pi.PropertyType;
        // Invoke Empty contructor (set to default value)
        var ci = propType.GetConstructor(Array.Empty<Type>());
        if (ci != null)
          pi.SetValue(entity, ci.Invoke(null), null);
      }

      UpdateEntity(@this, entity);
    }
  }
}

#endif
