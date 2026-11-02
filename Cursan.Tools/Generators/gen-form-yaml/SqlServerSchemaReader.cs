namespace Cursan.Tools.Generators;

public sealed class SqlServerSchemaReader
{
	private readonly string _connectionString;

	public SqlServerSchemaReader(string connectionString) => _connectionString = connectionString;

	public SqlTableSchema ReadTable(string schema, string table)
	{
		using var conn = new SqlConnection(_connectionString);
		conn.Open();

		var columns = ReadColumns(conn, schema, table);
		if (columns.Count == 0)
			throw new InvalidOperationException($"Таблица [{schema}].[{table}] не найдена или пуста.");

		var fks = ReadForeignKeys(conn, schema, table);
		return new SqlTableSchema { Schema = schema, Name = table, Columns = columns, ForeignKeys = fks };
	}

	public string? FindDisplayColumn(string schema, string table)
	{
		using var conn = new SqlConnection(_connectionString);
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = @"
            SELECT TOP 1 c.name
            FROM sys.columns c
            JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID(@tbl)
              AND t.name IN ('nvarchar','varchar','nchar','char')
              AND c.max_length BETWEEN 2 AND 200
            ORDER BY c.column_id";
		cmd.Parameters.AddWithValue("@tbl", $"[{schema}].[{table}]");
		return cmd.ExecuteScalar() as string;
	}

	private static IReadOnlyList<SqlColumnSchema> ReadColumns(SqlConnection conn, string schema, string table)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = @"
            SELECT c.name, t.name, c.max_length, c.precision, c.scale,
                   c.is_nullable, c.is_identity, c.is_computed,
                   CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END,
                   dc.definition
            FROM sys.columns c
            JOIN sys.types t ON c.user_type_id = t.user_type_id
            LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
            LEFT JOIN (
                SELECT ic.object_id, ic.column_id
                FROM sys.index_columns ic
                JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                WHERE i.is_primary_key = 1
            ) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
            WHERE c.object_id = OBJECT_ID(@tbl)
            ORDER BY c.column_id";
		cmd.Parameters.AddWithValue("@tbl", $"[{schema}].[{table}]");

		var result = new List<SqlColumnSchema>();
		using var r = cmd.ExecuteReader();
		while (r.Read())
		{
			result.Add(new SqlColumnSchema
			{
				Name = r.GetString(0),
				SqlType = r.GetString(1).ToLowerInvariant(),
				MaxLength = r.GetInt16(2),
				Precision = r.GetByte(3),
				Scale = r.GetByte(4),
				IsNullable = r.GetBoolean(5),
				IsIdentity = r.GetBoolean(6),
				IsComputed = r.GetBoolean(7),
				IsPrimaryKey = r.GetInt32(8) == 1,
				DefaultDefinition = r.IsDBNull(9) ? null : r.GetString(9),
			});
		}
		return result;
	}

	private static IReadOnlyList<SqlForeignKey> ReadForeignKeys(SqlConnection conn, string schema, string table)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = @"
            SELECT pc.name, rs.name, rt.name, rc.name
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            JOIN sys.columns pc ON fkc.parent_object_id    = pc.object_id AND fkc.parent_column_id    = pc.column_id
            JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
            JOIN sys.tables rt  ON fk.referenced_object_id  = rt.object_id
            JOIN sys.schemas rs ON rt.schema_id             = rs.schema_id
            WHERE fk.parent_object_id = OBJECT_ID(@tbl)";
		cmd.Parameters.AddWithValue("@tbl", $"[{schema}].[{table}]");

		var result = new List<SqlForeignKey>();
		using var r = cmd.ExecuteReader();
		while (r.Read())
		{
			result.Add(new SqlForeignKey
			{
				ColumnName = r.GetString(0),
				ReferencedSchema = r.GetString(1),
				ReferencedTable = r.GetString(2),
				ReferencedColumn = r.GetString(3),
			});
		}
		return result;
	}
}