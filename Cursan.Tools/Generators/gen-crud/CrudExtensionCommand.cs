using Cursan.Tools.Cli;
using Cursan.Tools.Infrastructure;

namespace Cursan.Tools.Generators;

public sealed class CrudExtensionCommand : ICommand
{
	public string Name => "gen-crud";

	public string Description =>
		"Partial-расширение DbContext: GetAll / GetWhile / Add / Change / Delete.";

	public string Usage =>
		"cursan gen-crud --context <ИмяDbContext> --namespace <Namespace> [--output <каталог>] [--force]";

	public int Execute(CommandContext context)
	{
		var dbContext = NamingHelper.EnsureValidIdentifier(
			context.RequireOption("context"), "context");

		var targetNamespace = context.RequireOption("namespace");
		var outputDir = context.GetOption("output", ".");
		var force = context.GetFlag("force");

		var code = CrudExtensionTemplate.Build(dbContext, targetNamespace);
		var path = CodeFileWriter.Write(outputDir, $"{dbContext}.Crud.g.cs", code, force);

		context.Out.WriteLine($"Создан файл: {path}");
		return 0;
	}
}