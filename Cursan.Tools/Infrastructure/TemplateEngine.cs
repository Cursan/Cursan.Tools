using System.Text;

namespace Cursan.Tools.Infrastructure;

/// <summary>
/// Минимальный шаблонизатор: заменяет плейсхолдеры {{Ключ}} значениями.
/// Для сложных шаблонов (циклы, условия) стоит перейти на Scriban.
/// </summary>
public static class TemplateEngine
{
	public static string Render(string template, IReadOnlyDictionary<string, string> values)
	{
		var sb = new StringBuilder(template);
		foreach (var (key, value) in values)
			sb.Replace("{{" + key + "}}", value ?? string.Empty);
		return sb.ToString();
	}
}