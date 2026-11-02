namespace Cursan.Tools.Generators;

public sealed class ScaffoldYamlCommand : ICommand
{
	public string Name => "scaffold-yaml";

	public string Description =>
		"Создаёт YAML-схему формы (cursan.dev/form-v1) по структуре таблицы SQL Server.";

	public string Usage =>
		"cursan scaffold-yaml --connection <строка> [--table <Таблица|all>] [--schema dbo] [--form-name <Класс>] [--namespace <Ns>] [--output <каталог>] [--force]";

	public int Execute(CommandContext context)
	{
		try
		{
			var connStr = context.RequireOption("connection");
			var tableName = context.GetOption("table", "all");
			var dbSchema = context.GetOption("schema", "dbo");
			var formName = context.GetOption("form-name", string.Empty);
			var ns = context.GetOption("namespace", string.Empty);
			var output = context.GetOption("output", ".");
			var force = context.GetFlag("force");

			var reader = new SqlServerSchemaReader(connStr);

			if (tableName.Equals("all", StringComparison.OrdinalIgnoreCase))
			{
				var tables = reader.GetTableNames(dbSchema);
				if (tables.Count == 0)
				{
					context.Out.WriteLine($"В схеме [{dbSchema}] не найдено таблиц.");
					return 0;
				}

				foreach (var name in tables)
				{
					var table = reader.ReadTable(dbSchema, name);
					var options = new ScaffoldOptions
					{
						FormName = string.IsNullOrWhiteSpace(formName) ? null : formName,
						Namespace = string.IsNullOrWhiteSpace(ns) ? null : ns,
						Output = output,
					};
					var schemaModel = new SqlToFormMapper(reader, options).Map(table);
					var yaml = new FormSchemaYamlWriter().Write(schemaModel);

					var fileName = $"{name}.form.yaml";
					var path = CodeFileWriter.Write(output, fileName, yaml, force);
					context.Out.WriteLine($"Создан файл: {path}");
				}
			}
			else
			{
				var table = reader.ReadTable(dbSchema, tableName);
				var options = new ScaffoldOptions
				{
					FormName = string.IsNullOrWhiteSpace(formName) ? null : formName,
					Namespace = string.IsNullOrWhiteSpace(ns) ? null : ns,
					Output = output,
				};
				var schemaModel = new SqlToFormMapper(reader, options).Map(table);
				var yaml = new FormSchemaYamlWriter().Write(schemaModel);

				var fileName = $"{tableName}.form.yaml";
				var path = CodeFileWriter.Write(output, fileName, yaml, force);
				context.Out.WriteLine($"Создан файл: {path}");
			}
			return 0;
		}
		catch (Exception ex)
		{
			context.Error.WriteLine($"Ошибка: {ex.Message}");
			return 1;
		}
	}
}