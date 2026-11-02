namespace Cursan.Tools.Generators;

public sealed class SqlTableSchema
{
	public string Schema { get; init; } = "dbo";
	public string Name { get; init; } = string.Empty;
	public IReadOnlyList<SqlColumnSchema> Columns { get; init; } = Array.Empty<SqlColumnSchema>();
	public IReadOnlyList<SqlForeignKey> ForeignKeys { get; init; } = Array.Empty<SqlForeignKey>();
}

public sealed class SqlColumnSchema
{
	public string Name { get; init; } = string.Empty;
	public string SqlType { get; init; } = string.Empty;   // в нижнем регистре: "nvarchar", "int", ...
	public int MaxLength { get; init; }                    // для nvarchar — байты, -1 = MAX
	public int Precision { get; init; }
	public int Scale { get; init; }
	public bool IsNullable { get; init; }
	public bool IsIdentity { get; init; }
	public bool IsComputed { get; init; }
	public bool IsPrimaryKey { get; init; }
	public string? DefaultDefinition { get; init; }        // "(0)", "('x')", "(getdate())", ...
}

public sealed class SqlForeignKey
{
	public string ColumnName { get; init; } = string.Empty;
	public string ReferencedSchema { get; init; } = string.Empty;
	public string ReferencedTable { get; init; } = string.Empty;
	public string ReferencedColumn { get; init; } = string.Empty;
}