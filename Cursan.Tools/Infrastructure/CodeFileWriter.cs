using System.Text;

namespace Cursan.Tools.Infrastructure;

public static class CodeFileWriter
{
	public static string Write(string directory, string fileName, string content, bool overwrite = false)
	{
		Directory.CreateDirectory(directory);

		var path = Path.Combine(directory, fileName);

		if (File.Exists(path) && !overwrite)
			throw new InvalidOperationException(
				$"Файл уже существует: {path}. Используйте --force для перезаписи.");

		File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		return path;
	}
}