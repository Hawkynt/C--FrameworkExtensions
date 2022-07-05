#region (c)2010-2020 Hawkynt
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

// as long as fake db is defined, this will not execute any statement on the sql server, instead it will print statement to the console and do not use sql parameters, so values can be seen
#undef FAKE_DB


#if FAKE_DB
#define DO_NOT_USE_PARAMETERS
#else
#undef DO_NOT_USE_PARAMETERS
#endif


using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Data.SqlClient {
  /// <summary>
  /// Extensions for the sql command.
  /// </summary>
  internal static partial class CommandExtensions {
    /// <summary>
    /// Executes something that is not a query on the given command.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="commandText">The command text.</param>
    /// <returns>The number of rows that where affected.</returns>
    public static int ExecuteNonQuery(this SqlCommand This, string commandText) {
      Contract.Requires(This != null);
      Contract.Requires(!string.IsNullOrWhiteSpace(commandText));
#if FAKE_DB
      Console.WriteLine(commandText);
      return (1);
#else
      This.CommandText = commandText;
      return This.ExecuteNonQuery();
#endif
    }

    /// <summary>
    /// Executes a query and returns the first row and the first columns' value.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="commandText">The command text.</param>
    /// <returns>The single result.</returns>
    public static object ExecuteScalar(this SqlCommand This, string commandText) {
      Contract.Requires(This != null);
      Contract.Requires(!string.IsNullOrWhiteSpace(commandText));
#if FAKE_DB
      Console.WriteLine(commandText);
      return (null);
#else
      This.CommandText = commandText;
      return This.ExecuteScalar();
#endif
    }

    /// <summary>
    /// Executes an sql statement and returns a reader to the ResultSet.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="commandText">The command text.</param>
    /// <returns>The reader for reading the ResultSet.</returns>
    public static SqlDataReader ExecuteReader(this SqlCommand This, string commandText) {
      Contract.Requires(This != null);
      Contract.Requires(!string.IsNullOrWhiteSpace(commandText));
#if FAKE_DB
      return (null);
#else
      This.CommandText = commandText;
      return This.ExecuteReader();
#endif
    }

    /// <summary>
    /// Executes an update.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="values">The values.</param>
    /// <param name="whereStatement">The where statement; if any.</param>
    /// <param name="dontUseParameters">if set to <c>true</c> doesn't use parameters.</param>
    /// <returns>
    /// The number of updated rows.
    /// </returns>
    public static int ExecuteUpdate(this SqlCommand This, string tableName, IEnumerable<KeyValuePair<string, object>> values, string whereStatement = null, bool dontUseParameters = false) {
      Contract.Requires(This != null);
      Contract.Requires(!string.IsNullOrWhiteSpace(tableName));
      Contract.Requires(values != null);

      // TODO: conflicts with existing parameters
      var sqlCommand = new StringBuilder();
      sqlCommand.Append("UPDATE " + tableName.MsSqlIdentifierEscape());
      var realValues = (
        from i in values
        select new { ColumnName = i.Key, Value = i.Value == null ? null : i.Value.GetType().IsEnum ? Convert.ToInt64(i.Value) : i.Value }
      ).ToArray();

      sqlCommand.Append(" SET ");

      var paramIndex = 0;
      foreach (var kvp in realValues) {
        if (paramIndex > 0)
          sqlCommand.Append(", ");
        if (dontUseParameters)
          sqlCommand.Append(kvp.ColumnName.MsSqlIdentifierEscape() + " = " + kvp.Value.MsSqlDataEscape());
        else {
          sqlCommand.Append(kvp.ColumnName.MsSqlIdentifierEscape() + " = @val" + paramIndex);
          This.Parameters.AddWithValue("@val" + paramIndex, kvp.Value ?? DBNull.Value);
        }
        paramIndex++;
      }

      if (whereStatement != null)
        sqlCommand.Append(" WHERE " + whereStatement);

      var result = This.ExecuteNonQuery(sqlCommand.ToString());

      // remove all parameters we added
      if (!dontUseParameters)
        This.Parameters.Clear();

      return result;
    }

    /// <summary>
    /// Executes a single insert statement.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="values">The values to insert.</param>
    /// <param name="tableContainsId">if set to <c>true</c> the table contains an identity column which will be returned.</param>
    /// <returns>The value of the identity column or the number of affected rows.</returns>
    public static long ExecuteSingleInsert(this SqlCommand This, string tableName, bool tableContainsId = false, params IEnumerable<KeyValuePair<string, object>>[] values) {
      var allValues = values.Where(v => v != null).ConcatAll();
      return This.ExecuteSingleInsert(tableName, allValues, tableContainsId);
    }

    /// <summary>
    /// Executes a single insert statement.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="values">The values to insert.</param>
    /// <param name="tableContainsId">if set to <c>true</c> the table contains an identity column which will be returned.</param>
    /// <returns>The value of the identity column or the number of affected rows.</returns>
    public static long ExecuteSingleInsert(this SqlCommand This, string tableName, IEnumerable<KeyValuePair<string, object>> values = null, bool tableContainsId = false) {
      Contract.Requires(This != null);
      Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

      var sqlCommandTextWithParameters = new StringBuilder();
      var sqlCommandTextWithoutParameters = new StringBuilder();
      _AppendToBuilders($"INSERT INTO {tableName.MsSqlIdentifierEscape()}", sqlCommandTextWithParameters, sqlCommandTextWithoutParameters);

      var realValues = values == null ? null : (
        from i in values
        select new {
          ColumnName = i.Key,
          Value = _ConvertDataValue(i.Value)
        }
      ).ToArray();

      // if there are any columns to insert
      if (realValues != null && realValues.Any()) {
        _AppendToBuilders(" (", sqlCommandTextWithParameters, sqlCommandTextWithoutParameters);
        _AppendToBuilders(string.Join(", ", realValues.Select((i, n) => i.ColumnName.MsSqlIdentifierEscape())), sqlCommandTextWithParameters, sqlCommandTextWithoutParameters);
        _AppendToBuilders(") VALUES (", sqlCommandTextWithParameters, sqlCommandTextWithoutParameters);
        _AppendToBuilders(string.Join(", ", realValues.Select((i, n) => i.Value.MsSqlDataEscape())), sqlCommandTextWithoutParameters);

#if !DO_NOT_USE_PARAMETERS
        _AppendToBuilders(string.Join(", ", realValues.Select(i => {
          var name = "@" + _ConvertParameterName(i.ColumnName);

          // find a name for the param if it already exists using a counter postfix
          var index = 1;
          while (This.Parameters.Contains(name + (index < 2 ? string.Empty : index.ToString())))
            index++;

          // add value and mark "allow null" when the value is null
          This.Parameters.AddWithValue(name, i.Value ?? DBNull.Value);
          if (i.Value == null)
            This.Parameters[name].IsNullable = true;

          return name;
        })), sqlCommandTextWithParameters);
#endif
        _AppendToBuilders(")", sqlCommandTextWithParameters, sqlCommandTextWithoutParameters);
      }

      // no identity column, return number of rows affected, usually 1
      if (!tableContainsId) {

#if DO_NOT_USE_PARAMETERS
        var numberOfAffectedRows = This.ExecuteNonQuery(sqlCommandNoParams.ToString());
#else
        var numberOfAffectedRows = This.ExecuteNonQuery(sqlCommandTextWithParameters.ToString());

        // clear parameter list
        This.Parameters.Clear();
#endif

        return numberOfAffectedRows;
      }

      // return scope if any
      _AppendToBuilders(";SELECT SCOPE_IDENTITY() AS [ID]", sqlCommandTextWithParameters, sqlCommandTextWithoutParameters);

#if DO_NOT_USE_PARAMETERS
      var result = This.ExecuteScalar(sqlCommandNoParams.ToString()) ?? 0;
#else
      var result = This.ExecuteScalar(sqlCommandTextWithParameters.ToString()) ?? 0;

      // clear parameter list
      This.Parameters.Clear();
#endif

      var identityResult = (long)(decimal)result;
      return identityResult;
    }

    private static void _AppendToBuilders(string text, StringBuilder sb1, StringBuilder sb2 = null) {
      sb1.Append(text);
      sb2?.Append(text);
    }

    /// <summary>
    /// Gets the records.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="whereStatement">The where statement.</param>
    /// <returns></returns>
    public static IEnumerable<Dictionary<string, object>> GetRecords(this SqlCommand This, string tableName, string whereStatement = null) {
      Contract.Requires(!string.IsNullOrWhiteSpace(whereStatement));
      var sqlCommand = $"SELECT * FROM {tableName.MsSqlIdentifierEscape()}";
      sqlCommand += " WHERE " + whereStatement;

      using (var results = This.ExecuteReader(sqlCommand)) {
        foreach (IDataRecord record in results) {
          var result = (
            from i in Enumerable.Range(0, results.FieldCount)
            let k = results.GetName(i)
            let v = record.GetValue(i)
            select new KeyValuePair<string, object>(k, v)
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
          yield return result;
        }
      }
    }

    /// <summary>
    /// Converts parameter names.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    private static string _ConvertParameterName(string name) {
      return new Regex("[^a-z0-9]+", RegexOptions.IgnoreCase).Replace(name, string.Empty);
    }

    /// <summary>
    /// Converts all enum values to longs, all bools to 1/0
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The converted value</returns>
    private static object _ConvertDataValue(object value) {
      if (value == null)
        return null;

      if (!value.GetType().IsEnum) {

        // convert bools to ints
        if (value is bool)
          return (bool)value ? 1 : 0;

        // let the rest as it is
        return value;
      }

      // enum derived from ulong
      if (value is ulong)
        return Convert.ToUInt64(value);

      // all other enum bases
      return Convert.ToInt64(value);
    }

    /// <summary>
    /// Checks if the given column in a table exists.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <returns></returns>
    public static bool ColumnExists(this SqlCommand This, string tableName, string columnName) {
      Contract.Requires(This != null);
      Contract.Requires(tableName != null);
      Contract.Requires(columnName != null);
#if DO_NOT_USE_PARAMETERS
      var result = This.ExecuteScalar("SELECT COUNT(0) FROM [sysobjects] AS [so] INNER JOIN [syscolumns] AS [sc] ON [sc].[id]=[so].[id] WHERE ([so].[xtype]='U' OR [so].[xtype]='V') AND ([so].[name] = " + tableName.MsSqlDataEscape() + ") AND ([sc].[name] = " + columnName.MsSqlDataEscape() + ")");
#else
      This.Parameters.AddWithValue("@val0", tableName);
      This.Parameters.AddWithValue("@val1", columnName);
      var result = This.ExecuteScalar("SELECT COUNT(0) FROM [sysobjects] AS [so] INNER JOIN [syscolumns] AS [sc] ON [sc].[id]=[so].[id] WHERE ([so].[xtype]='U' OR [so].[xtype]='V') AND ([so].[name] = @val0) AND ([sc].[name] = @val1)");
#endif


#if !DO_NOT_USE_PARAMETERS
      // clear parameter list
      This.Parameters.Clear();
#endif

      return (int)(result ?? 0) > 0;
    }

    /// <summary>
    /// Adds the column to an existing table.
    /// </summary>
    /// <param name="This">This SqlCommand.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="dbType">Type of the column.</param>
    /// <param name="isNotNull">if set to <c>true</c> this column will not allow NULL-values.</param>
    /// <param name="defaultValue">The default value; if any.</param>
    /// <param name="useDefaultValueForExistingRecords">if set to <c>true</c> the default value will be immediately applied to existing rows.</param>
    /// <param name="charLength">Length of the char/varchar/text column, 0=max, &lt;0 means use database default.</param>
    /// <param name="totalDigits">The total digits in a decimal datatype.</param>
    /// <param name="decimalDigits">The decimal digits in a decimal datatype.</param>
    public static void AddColumnToTable(this SqlCommand This, string tableName, string columnName, SqlDbType dbType, bool isNotNull = false, object defaultValue = null, bool useDefaultValueForExistingRecords = false, int charLength = 0, uint totalDigits = 0, uint decimalDigits = 0) {
      Contract.Requires(This != null);
      Contract.Requires(!string.IsNullOrWhiteSpace(tableName));
      Contract.Requires(!string.IsNullOrWhiteSpace(columnName));
      Contract.Requires(decimalDigits <= totalDigits);
      Contract.Requires(!isNotNull || defaultValue != null);
      var sql = new StringBuilder();
      sql.Append("ALTER TABLE ");
      sql.Append(tableName.MsSqlIdentifierEscape());
      sql.Append(" ADD ");
      sql.Append(columnName.MsSqlIdentifierEscape());
      sql.Append(" ");
      sql.Append(dbType.ToString());

      if (charLength >= 0 && (dbType == SqlDbType.Char || dbType == SqlDbType.VarChar || dbType == SqlDbType.NChar || dbType == SqlDbType.NVarChar)) {

        if (charLength == 0) {
          var version = This.Connection.ServerVersion == null ? null : This.Connection.ServerVersion.Split('.');

          // HACK: work-around for sql server <2005(<v9.0.0), limit string columns to 1024 chars
          sql.Append(version == null || version.Length < 1 || int.Parse(version[0]) < 9 ? "(1024)" : "(MAX)");
        } else {
          sql.Append("(" + charLength + ")");
        }
      } else if ((decimalDigits != 0 || totalDigits != 0) && dbType == SqlDbType.Decimal)
        sql.Append("(" + totalDigits + "," + decimalDigits + ")");

      if (isNotNull)
        sql.Append(" NOT");
      sql.Append(" NULL");

      sql.Append(" DEFAULT ");
      sql.Append(defaultValue.MsSqlDataEscape());

      if (useDefaultValueForExistingRecords)
        sql.Append(" WITH VALUE");

      This.ExecuteNonQuery(sql.ToString());
    }
  }
}