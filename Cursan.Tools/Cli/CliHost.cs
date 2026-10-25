using System.Linq;

namespace Cursan.Tools.Cli;

public sealed class CliHost
{
	private readonly CommandRegistry _registry;
	private readonly TextWriter _out;
	private readonly TextWriter _error;

	public CliHost(CommandRegistry registry, TextWriter output, TextWriter error)
	{
		_registry = registry;
		_out = output;
		_error = error;
	}

	public int Run(string[] args)
	{
		try
		{
			if (args.Length == 0)
			{
				PrintHelp(_out);
				return 0;
			}

			var parsed = CommandLineParser.Parse(args);

			if (string.Equals(parsed.CommandName, "help", StringComparison.OrdinalIgnoreCase))
			{
				if (parsed.Positionals.Count > 0 && _registry.TryGet(parsed.Positionals[0], out var helpTarget))
					PrintCommandHelp(helpTarget, _out);
				else
					PrintHelp(_out);
				return 0;
			}

			if (!_registry.TryGet(parsed.CommandName, out var command))
			{
				_error.WriteLine($"Неизвестная команда: {parsed.CommandName}");
				PrintHelp(_error);
				return 2;
			}

			if (parsed.Options.ContainsKey("help"))
			{
				PrintCommandHelp(command, _out);
				return 0;
			}

			var context = new CommandContext(parsed, _out, _error);
			return command.Execute(context);
		}
		catch (CommandLineException ex)
		{
			_error.WriteLine($"Ошибка аргументов: {ex.Message}");
			return 2;
		}
		catch (Exception ex)
		{
			_error.WriteLine($"Непредвиденная ошибка: {ex.Message}");
			return 1;
		}
	}

	private void PrintHelp(TextWriter writer)
	{
		writer.WriteLine("Cursan Tools — генераторы кода");
		writer.WriteLine();
		writer.WriteLine("Использование: cursan <команда> [опции]");
		writer.WriteLine();
		writer.WriteLine("Команды:");

		foreach (var command in _registry.Commands.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
			writer.WriteLine($"  {command.Name,-14} {command.Description}");

		writer.WriteLine();
		writer.WriteLine("Подробнее: cursan help <команда>");
	}

	private static void PrintCommandHelp(ICommand command, TextWriter writer)
	{
		writer.WriteLine(command.Name);
		writer.WriteLine(command.Description);
		writer.WriteLine();
		writer.WriteLine($"Использование: {command.Usage}");
	}
}