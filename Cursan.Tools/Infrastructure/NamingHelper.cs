namespace Cursan.Tools.Infrastructure;

public static partial class NamingHelper
{
	[GeneratedRegex(@"^@?[A-Za-z_][A-Za-z0-9_]*$")]
	private static partial Regex IdentifierRegex();

	public static string EnsureValidIdentifier(string value, string paramName)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentException("Идентификатор не может быть пустым.", paramName);

		if (!IdentifierRegex().IsMatch(value))
			throw new ArgumentException(
				$"'{value}' не является корректным идентификатором C# (параметр: {paramName}).", paramName);

		return value;
	}

	/// <summary>"HiredAt" -> "Hired At" — для подписей на форме.</summary>
	public static string Humanize(string name)
	{
		if (string.IsNullOrEmpty(name)) return name;

		var sb = new StringBuilder(name.Length + 4);
		for (var i = 0; i < name.Length; i++)
		{
			if (i > 0 && char.IsUpper(name[i]))
				sb.Append(' ');
			sb.Append(name[i]);
		}
		return sb.ToString();
	}
}