namespace Cursan.Tools.Cli;

public interface ICommand
{
	/// <summary>Имя команды, например "gen-crud".</summary>
	string Name { get; }

	/// <summary>Короткое описание для общего --help.</summary>
	string Description { get; }

	/// <summary>Строка использования для --help конкретной команды.</summary>
	string Usage { get; }

	/// <summary>Выполнить команду. Возвращает код выхода (0 — успех).</summary>
	int Execute(CommandContext context);
}