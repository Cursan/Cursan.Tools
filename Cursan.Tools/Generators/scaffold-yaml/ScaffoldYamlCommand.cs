namespace Cursan.Tools.Generators;

public sealed class ScaffoldYamlCommand : ICommand
{
	public string Name => "scaffold-yaml";

	public string Description =>
		"Создаёт YAML-схему формы (cursan.dev/form-v1) по структуре таблицы SQL Server.";

	public string Usage =>
		"cursan scaffold-yaml --connection <строка> --table <Таблица> [--schema dbo] " +
		"[--form-name <Класс>] [--namespace <Ns>] [--output <каталог>] [--force]";

	public int Execute(CommandContext context)
	{
		try
		{
			var connStr = context.RequireOption("connection");
			var tableName = context.RequireOption("table");
			var dbSchema = context.GetOption("schema", "dbo");
			var formName = context.GetOption("form-name", string.Empty);
			var ns = context.GetOption("namespace", string.Empty);
			var output = context.GetOption("output", ".");
			var force = context.GetFlag("force");

			var reader = new SqlServerSchemaReader(connStr);
			var table = reader.ReadTable(dbSchema, tableName);

			var options = new ScaffoldOptions
			{
				FormName = string.IsNullOrWhiteSpace(formName) ? null : formName,
				Namespace = string.IsNullOrWhiteSpace(ns) ? null : ns,
				Output = output,
			};

			var schema = new SqlToFormMapper(reader, options).Map(table);
			var yaml = new FormSchemaYamlWriter().Write(schema);

			var fileName = $"{tableName}.form.yaml";
			var path = CodeFileWriter.Write(output, fileName, yaml, force);

			context.Out.WriteLine($"Создан файл: {path}");
			return 0;
		}
		catch (Exception ex)
		{
			context.Error.WriteLine($"Ошибка: {ex.Message}");
			return 1;
		}
	}
}