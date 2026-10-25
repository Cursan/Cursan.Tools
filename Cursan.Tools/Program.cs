/*using System;
using Cursan.Tools;

namespace Cursan.Cli
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			// Ожидаемые аргументы:
			// [0] — имя DbContext (обязательно)
			// [1] — пространство имён для сгенерированного кода (обязательно)
			// [2] — каталог вывода (необязательно, по умолчанию текущий)

			if (args.Length < 2)
			{
				Console.WriteLine("Использование: Cursan.Cli <DbContextName> <Namespace> [OutputDirectory]");
				Console.WriteLine("Пример: Cursan.Cli AppDbContext MyApp.Data ./Generated");
				return;
			}

			var dbContextName = args[0];
			var targetNamespace = args[1];
			var outputDir = args.Length >= 3 ? args[2] : ".";

			try
			{
				var filePath = DbContextCrudGenerator.Generate(
					dbContextName,
					targetNamespace,
					outputDir);

				Console.WriteLine($"Файл сгенерирован: {filePath}");
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Ошибка: {ex.Message}");
				Environment.ExitCode = 1;
			}
		}
	}
}*/