namespace Cursan.Tools.Generators;

public sealed class ScaffoldOptions
{
	public string? FormName { get; init; }
	public string? Namespace { get; init; }
	public string? Output { get; init; }
}

public sealed class SqlToFormMapper
{
	private readonly SqlServerSchemaReader _reader;
	private readonly ScaffoldOptions _options;

	public SqlToFormMapper(SqlServerSchemaReader reader, ScaffoldOptions options)
	{
		_reader = reader;
		_options = options;
	}

	public FormSchema Map(SqlTableSchema table)
	{
		var fkByColumn = table.ForeignKeys
			.GroupBy(fk => fk.ColumnName, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

		var multiColumnPk = table.Columns.Count(c => c.IsPrimaryKey) > 1;
		var fields = new List<FieldSchema>();

		foreach (var col in table.Columns)
		{
			if (col.IsComputed) continue;
			if (col.IsIdentity) continue;
			if (col.SqlType is "rowversion" or "timestamp") continue;
			if (col.IsPrimaryKey && !multiColumnPk) continue;   // единственный PK = технический id

			FieldSchema? field = fkByColumn.TryGetValue(col.Name, out var fk)
				? BuildForeignKeyField(col, fk)
				: BuildField(col);

			if (field != null) fields.Add(field);
		}

		if (fields.Count == 0)
			throw new InvalidOperationException(
				$"После фильтрации (identity/PK/rowversion/бинарные) у таблицы [{table.Schema}].[{table.Name}] не осталось полей для формы.");

		var formName = string.IsNullOrWhiteSpace(_options.FormName) ? $"{table.Name}Form" : _options.FormName!;
		var ns = string.IsNullOrWhiteSpace(_options.Namespace) ? $"{table.Schema}.{table.Name}.Forms" : _options.Namespace!;

		return new FormSchema
		{
			Version = 1,
			Metadata = new Metadata
			{
				Name = formName,
				Namespace = ns,
				Description = $"Форма редактирования записи [{table.Schema}].[{table.Name}]",
				Output = _options.Output ?? "./Generated",
				Overwrite = false,
			},
			Form = new FormSettings
			{
				Type = "Window",
				Title = NamingHelper.Humanize(table.Name),
				Width = "720",
				Height = "auto",
				MinWidth = 480,
				MinHeight = 320,
				StartupLocation = "CenterOwner",
				Resizable = true,
				Layout = new FormLayout
				{
					Columns = new List<string> { "Auto", "*" },
					RowHeight = 30,
					Margin = 16,
					Spacing = 6,
				},
			},
			ViewModel = new ViewModelSettings
			{
				Generate = true,
				Name = formName + "Model",
				Namespace = ns + ".ViewModels",
				BaseType = "ObservableObject",
				Toolkit = "CommunityToolkit.Mvvm",
				Commands = new List<string> { "Save", "Cancel" },
			},
			CodeBehind = new CodeBehindSettings
			{
				Generate = true,
				DataContext = "viewModel",
			},
			Fields = fields,
			Actions = new List<ActionSchema>
			{
				new() { Name = "Save",   Text = "Сохранить", Command = "SaveCommand",   IsDefault = true, XIcon = "save"  },
				new() { Name = "Cancel", Text = "Отмена",    Command = "CancelCommand", IsCancel  = true, XIcon = "close" },
			},
		};
	}

	private FieldSchema? BuildField(SqlColumnSchema col)
	{
		var baseField = new FieldSchema
		{
			Name = col.Name,
			Label = NamingHelper.Humanize(col.Name),
			Required = !col.IsNullable,
		};

		switch (col.SqlType)
		{
			case "nvarchar" or "varchar" or "nchar" or "char" or "ntext" or "text":
				baseField.Control = "TextBox";
				baseField.Type = "string";
				baseField.MaxLength = col.MaxLength == -1
					? 0
					: (col.SqlType is "nvarchar" or "nchar" ? col.MaxLength / 2 : col.MaxLength);
				baseField.Multiline = col.MaxLength == -1 || col.MaxLength > 500;
				baseField.Default = StripDefaultLiteral(col.DefaultDefinition);
				return baseField;

			case "int" or "bigint" or "smallint" or "tinyint":
				baseField.Control = "NumericUpDown";
				baseField.Type = "int";
				baseField.Default = ParseNumeric(col.DefaultDefinition);
				return baseField;

			case "decimal" or "numeric" or "money" or "smallmoney":
				baseField.Control = "NumericUpDown";
				baseField.Type = "decimal";
				baseField.Default = ParseNumeric(col.DefaultDefinition);
				return baseField;

			case "float" or "real":
				baseField.Control = "NumericUpDown";
				baseField.Type = "double";
				return baseField;

			case "bit":
				baseField.Control = "CheckBox";
				baseField.Type = "bool";
				baseField.Required = false;
				baseField.Default = ParseBit(col.DefaultDefinition);
				return baseField;

			case "datetime" or "datetime2" or "date" or "smalldatetime" or "datetimeoffset":
				baseField.Control = "DatePicker";
				baseField.Type = "DateTime";
				baseField.Default = ParseDateDefault(col.DefaultDefinition);
				return baseField;

			case "uniqueidentifier":
				baseField.Control = "TextBox";
				baseField.Type = "string";
				baseField.Validation = new ValidationSchema
				{
					Pattern = "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
					Message = "Ожидается GUID.",
				};
				return baseField;

			case "xml":
				baseField.Control = "TextBox";
				baseField.Type = "string";
				baseField.Multiline = true;
				return baseField;

			case "time":
				baseField.Control = "TextBox";
				baseField.Type = "string";
				baseField.Placeholder = "HH:mm:ss";
				baseField.Validation = new ValidationSchema
				{
					Pattern = @"^\d{2}:\d{2}(:\d{2})?$",
					Message = "Ожидается время в формате HH:mm:ss.",
				};
				return baseField;

			default:   // binary, varbinary, geography, sql_variant, ...
				return null;
		}
	}

	private FieldSchema BuildForeignKeyField(SqlColumnSchema col, SqlForeignKey fk)
	{
		var display = _reader.FindDisplayColumn(fk.ReferencedSchema, fk.ReferencedTable)
					  ?? fk.ReferencedColumn;

		var fieldType = col.SqlType switch
		{
			"int" or "bigint" or "smallint" or "tinyint" => "int",
			"uniqueidentifier" => "Guid",
			_ => "string",
		};

		return new FieldSchema
		{
			Name = col.Name,
			Label = NamingHelper.Humanize(col.Name),
			Control = "ComboBox",
			Type = fieldType,
			Required = !col.IsNullable,
			ItemsSource = Pluralize(fk.ReferencedTable),
			DisplayMember = display,
			ValueMember = fk.ReferencedColumn,
		};
	}

	// ─── парсеры default-констрейнтов ──────────────────────────
	private static string? StripDefaultLiteral(string? def)
	{
		if (string.IsNullOrWhiteSpace(def)) return null;
		var s = def.Trim();
		while (s.StartsWith("(") && s.EndsWith(")")) s = s[1..^1].Trim();
		return s.Trim('\'');
	}

	private static object? ParseNumeric(string? def)
	{
		var s = StripDefaultLiteral(def);
		if (s == null) return null;
		return decimal.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : null;
	}

	private static bool? ParseBit(string? def) => StripDefaultLiteral(def) switch
	{
		"1" => true,
		"0" => false,
		_ => null,
	};

	private static string? ParseDateDefault(string? def)
	{
		var s = StripDefaultLiteral(def);
		if (s == null) return null;
		if (s.Contains("getdate", StringComparison.OrdinalIgnoreCase) ||
			s.Contains("sysdatetime", StringComparison.OrdinalIgnoreCase))
			return "now";
		return DateTime.TryParse(s, out _) ? s : null;
	}

	private static string Pluralize(string name)
	{
		if (name.Length > 1 && name.EndsWith("y") && !"aeiou".Contains(char.ToLowerInvariant(name[^2])))
			return name[..^1] + "ies";
		if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("z") ||
			name.EndsWith("ch") || name.EndsWith("sh"))
			return name + "es";
		return name + "s";
	}
}