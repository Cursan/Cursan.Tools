namespace Cursan.Tools.Cli;

public sealed class CommandLineException : Exception
{
	public CommandLineException(string message) : base(message) { }
}

public sealed record ParsedCommandLine(
	string CommandName,
	IReadOnlyList<string> Positionals,
	IReadOnlyDictionary<string, string> Options);

public static class CommandLineParser
{
	/// <summary>
	/// Разбирает аргументы вида:
	///   cursan gen-crud --context AppDbContext --namespace MyApp.Data --output ./Generated --force
	///   cursan gen-form -name EmployeeEditForm -namespace MyApp.Views -fields "Name:string;Age:int"
	/// Поддерживает --key value, --key=value, -k value и флаги без значения (--force => "true").
	/// </summary>
	public static ParsedCommandLine Parse(string[] args)
	{
		string? commandName = null;
		var positionals = new List<string>();
		var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		for (var i = 0; i < args.Length; i++)
		{
			var token = args[i];

			if (IsOptionToken(token))
			{
				var body = token.TrimStart('-');
				if (body.Length == 0)
					throw new CommandLineException($"Некорректная опция: {token}");

				string key, value;
				var eq = body.IndexOf('=');
				if (eq >= 0)
				{
					key = body[..eq];
					value = body[(eq + 1)..];
				}
				else
				{
					key = body;
					value = i + 1 < args.Length && !IsOptionToken(args[i + 1])
						? args[++i]
						: "true";
				}

				options[key] = value;
			}
			else if (commandName is null)
			{
				commandName = token;
			}
			else
			{
				positionals.Add(token);
			}
		}

		if (commandName is null)
			throw new CommandLineException("Не указана команда. Запустите без аргументов для справки.");

		return new ParsedCommandLine(commandName, positionals, options);
	}

	private static bool IsOptionToken(string token) =>
		token.Length > 1 && token[0] == '-' && !char.IsDigit(token[1]);
}