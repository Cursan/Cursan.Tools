using System.Linq;
using Cursan.Tools.Infrastructure;

namespace Cursan.Tools.Generators;

public sealed record FormField(string Name, string Type);

public static class FormTemplate
{
	public static string BuildXaml(
		string className, string targetNamespace, string title, IReadOnlyList<FormField> fields)
	{
		var rowDefinitions = string.Join(Environment.NewLine,
			fields.Select(_ => "        <RowDefinition Height=\"Auto\"/>"));

		var rows = string.Join(Environment.NewLine,
			fields.Select((f, i) =>
				$"        <TextBlock Grid.Row=\"{i}\" Grid.Column=\"0\" Text=\"{NamingHelper.Humanize(f.Name)}\" " +
				$"VerticalAlignment=\"Center\" Margin=\"0,0,12,8\"/>{Environment.NewLine}" +
				$"        <TextBox Grid.Row=\"{i}\" Grid.Column=\"1\" x:Name=\"{f.Name}TextBox\" Margin=\"0,0,0,8\"/>"));

		var height = 140 + fields.Count * 44;

		return TemplateEngine.Render(XamlTemplate, new Dictionary<string, string>
		{
			["Namespace"] = targetNamespace,
			["Name"] = className,
			["Title"] = title,
			["Height"] = height.ToString(),
			["RowDefinitions"] = rowDefinitions,
			["Rows"] = rows,
		});
	}

	public static string BuildCodeBehind(string className, string targetNamespace) =>
		TemplateEngine.Render(CodeBehindTemplate, new Dictionary<string, string>
		{
			["Namespace"] = targetNamespace,
			["Name"] = className,
		});

	private const string XamlTemplate = """
<Window x:Class="{{Namespace}}.{{Name}}"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="{{Title}}"
        Height="{{Height}}" Width="480"
        WindowStartupLocation="CenterOwner">
    <Grid Margin="16">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
{{RowDefinitions}}
        </Grid.RowDefinitions>
{{Rows}}
    </Grid>
</Window>
""";

	private const string CodeBehindTemplate = """
using System.Windows;

namespace {{Namespace}};

public partial class {{Name}} : Window
{
    public {{Name}}()
    {
        InitializeComponent();
    }

    // TODO: методы заполнения формы и чтения значений
    // (появятся после подключения модели/ViewModel).
}
""";
}