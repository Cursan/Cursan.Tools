namespace Cursan.Tools.Generators;

public sealed class FormCommand : ICommand
{
	public string Name => "gen-form";

	public string Description => "Генерирует WPF-окно формы (XAML + code-behind) по списку полей.";

	public string Usage =>
		"cursan gen-form --name <ИмяКласса> --namespace <Namespace> --fields \"Name:string;Age:int\" [--title <Заголовок>] [--output <каталог>] [--force]";

	public int Execute(CommandContext context)
	{
		var className = NamingHelper.EnsureValidIdentifier(context.RequireOption("name"), "name");
		var targetNamespace = context.RequireOption("namespace");
		var fields = ParseFields(context.RequireOption("fields"));
		var title = context.GetOption("title", NamingHelper.Humanize(className));
		var outputDir = context.GetOption("output", ".");
		var force = context.GetFlag("force");

		var xaml = FormTemplate.BuildXaml(className, targetNamespace, title, fields);
		var codeBehind = FormTemplate.BuildCodeBehind(className, targetNamespace);

		var xamlPath = CodeFileWriter.Write(outputDir, $"{className}.xaml", xaml, force);
		var csPath = CodeFileWriter.Write(outputDir, $"{className}.xaml.cs", codeBehind, force);

		context.Out.WriteLine($"Создан файл: {xamlPath}");
		context.Out.WriteLine($"Создан файл: {csPath}");
		return 0;
	}

	private static IReadOnlyList<FormField> ParseFields(string spec)
	{
		var result = new List<FormField>();

		foreach (var part in spec.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
		{
			var pieces = part.Split(':', 2);
			if (pieces.Length != 2)
				throw new CommandLineException(
					$"Неверный формат поля '{part}'. Ожидается 'Имя:Тип', например 'Name:string'.");

			var name = NamingHelper.EnsureValidIdentifier(pieces[0].Trim(), "fields");
			var type = pieces[1].Trim();

			if (type.Length == 0)
				throw new CommandLineException($"Не указан тип для поля '{name}'.");

			result.Add(new FormField(name, type));
		}

		if (result.Count == 0)
			throw new CommandLineException("Список полей пуст.");

		return result;
	}
}