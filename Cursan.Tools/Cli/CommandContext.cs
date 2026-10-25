namespace Cursan.Tools.Cli;

public sealed class CommandContext
{
	private readonly IReadOnlyDictionary<string, string> _options;

	public CommandContext(ParsedCommandLine parsed, TextWriter output, TextWriter error)
	{
		_options = parsed.Options;
		Arguments = parsed.Positionals;
		Out = output;
		Error = error;
	}

	public IReadOnlyList<string> Arguments { get; }
	public TextWriter Out { get; }
	public TextWriter Error { get; }
	public IReadOnlyDictionary<string, string> Options => _options;

	public bool HasOption(string name) => _options.ContainsKey(name);

	public string? GetOption(string name) =>
		_options.TryGetValue(name, out var value) ? value : null;

	public string GetOption(string name, string defaultValue) =>
		GetOption(name) ?? defaultValue;

	public string RequireOption(string name) =>
		GetOption(name) ?? throw new CommandLineException($"Обязательная опция --{name} не указана.");

	/// <summary>Флаг: наличие опции без значения либо значение != "false".</summary>
	public bool GetFlag(string name) =>
		HasOption(name) && !string.Equals(GetOption(name), "false", StringComparison.OrdinalIgnoreCase);
}