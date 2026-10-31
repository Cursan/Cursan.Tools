namespace Cursan.Tools.Generators;

public sealed class YamlFormCommand : ICommand
{
	public string Name => "gen-form-yaml";
	public string Description => "Генерирует WPF-форму по YAML-схеме (cursan.dev/form-v1).";
	public string Usage => "cursan gen-form-yaml --schema <путь_к_yaml> [--output <каталог>] [--force]";

	public int Execute(CommandContext context)
	{
		string schemaPath = context.RequireOption("schema");
		string outputDir = context.GetOption("output", ".");
		bool force = context.GetFlag("force");

		try
		{
			var parser = new FormSchemaParser();
			var schema = parser.ParseFile(schemaPath);

			var generator = new YamlFormGenerator(schema);
			generator.Generate(outputDir, force);

			context.Out.WriteLine("Генерация завершена успешно.");
			return 0;
		}
		catch (Exception ex)
		{
			context.Error.WriteLine($"Ошибка: {ex.Message}");
			return 1;
		}
	}
}