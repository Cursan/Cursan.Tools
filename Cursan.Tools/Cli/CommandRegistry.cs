namespace Cursan.Tools.Cli;

public sealed class CommandRegistry
{
	private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

	public CommandRegistry Register(ICommand command)
	{
		if (!_commands.TryAdd(command.Name, command))
			throw new InvalidOperationException($"Команда '{command.Name}' уже зарегистрирована.");
		return this;
	}

	public bool TryGet(string name, out ICommand command) =>
		_commands.TryGetValue(name, out command!);

	public IEnumerable<ICommand> Commands => _commands.Values;
}