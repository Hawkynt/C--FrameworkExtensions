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

#if NET40
using System.Diagnostics.Contracts;

#endif

namespace System.Data.Linq {
  internal static partial class TableExtensions {

    /// <summary>
    /// Updates the row by re-attaching the entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="this">This Table.</param>
    /// <param name="entity">The entity.</param>
    public static void UpdateEntity<TEntity>(this Table<TEntity> @this, TEntity entity) where TEntity : class {
#if NET40
      Contract.Requires(@this != null);
      Contract.Requires(entity != null);
#endif
      @this.Attach(entity);
      @this.Context.Refresh(RefreshMode.KeepCurrentValues, entity);
    }

    public static void Reattach<TEntity>(this Table<TEntity> @this, TEntity entity) where TEntity : class {
#if NET40
      Contract.Requires(@this != null);
      Contract.Requires(entity != null);
#endif
      foreach (var pi in typeof(TEntity).GetProperties()) {
        if (pi.GetCustomAttributes(typeof(Mapping.AssociationAttribute), false).Length <= 0)
          continue;

        // Property is associated to another entity
        var propType = pi.PropertyType;
        // Invoke Empty contructor (set to default value)
        var ci = propType.GetConstructor(new Type[0]);
        if (ci != null)
          pi.SetValue(entity, ci.Invoke(null), null);
      }
      UpdateEntity(@this, entity);
    }

  }
}