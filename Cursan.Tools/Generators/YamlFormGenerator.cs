namespace Cursan.Tools.Generators;

public sealed class YamlFormGenerator
{
	private readonly FormSchema _schema;

	public YamlFormGenerator(FormSchema schema)
	{
		_schema = schema;
	}

	public void Generate(string outputDirectory, bool force = false)
	{
		Metadata metadata = _schema.Metadata;
		var className = metadata.Name;
		var targetNamespace = metadata.Namespace;
		var outputDir = string.IsNullOrWhiteSpace(metadata.Output) ? outputDirectory : metadata.Output;
		var overwrite = force || metadata.Overwrite;

		Directory.CreateDirectory(outputDir);

		// Генерация XAML
		var xamlContent = BuildXaml();
		var xamlPath = CodeFileWriter.Write(outputDir, $"{className}.xaml", xamlContent, overwrite);

		// Генерация Code-Behind
		if (_schema.CodeBehind.Generate)
		{
			var codeBehindContent = BuildCodeBehind();
			var codeBehindPath = CodeFileWriter.Write(outputDir, $"{className}.xaml.cs", codeBehindContent, overwrite);
		}

		// Генерация ViewModel
		if (_schema.ViewModel.Generate)
		{
			var vmName = _schema.ViewModel.Name ?? $"{className}Model";
			var vmNamespace = _schema.ViewModel.Namespace ?? $"{targetNamespace}.ViewModels";
			var vmContent = BuildViewModel(vmName, vmNamespace);
			CodeFileWriter.Write(outputDir, $"{vmName}.cs", vmContent, overwrite);
		}
	}

	private string BuildXaml()
	{
		var sb = new StringBuilder();
		FormSettings form = _schema.Form;
		FormLayout layout = form.Layout;

		sb.AppendLine($"<{form.Type} x:Class=\"{_schema.Metadata.Namespace}.{_schema.Metadata.Name}\"");
		sb.AppendLine("        xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
		sb.AppendLine("        xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");

		if (!string.IsNullOrWhiteSpace(form.Title))
			sb.AppendLine($"        Title=\"{form.Title}\"");

		if (form.Width != "auto")
			sb.AppendLine($"        Width=\"{form.Width}\"");
		if (form.Height != "auto")
			sb.AppendLine($"        Height=\"{form.Height}\"");
		if (form.MinWidth > 0)
			sb.AppendLine($"        MinWidth=\"{form.MinWidth}\"");
		if (form.MinHeight > 0)
			sb.AppendLine($"        MinHeight=\"{form.MinHeight}\"");
		if (!form.Resizable)
			sb.AppendLine("        ResizeMode=\"NoResize\"");
		if (form.StartupLocation != "Manual")
			sb.AppendLine($"        WindowStartupLocation=\"{form.StartupLocation}\"");

		sb.AppendLine("        >");

		// Сетка
		sb.AppendLine("    <Grid>");
		sb.AppendLine("        <Grid.ColumnDefinitions>");
		foreach (var col in layout.Columns)
		{
			var width = col switch
			{
				"Auto" => "Auto",
				"*" => "*",
				_ when int.TryParse(col, out _) => col,
				_ => "Auto"
			};
			sb.AppendLine($"            <ColumnDefinition Width=\"{width}\"/>");
		}
		sb.AppendLine("        </Grid.ColumnDefinitions>");

		sb.AppendLine("        <Grid.RowDefinitions>");
		for (int i = 0; i < _schema.Fields.Count; i++)
		{
			sb.AppendLine($"            <RowDefinition Height=\"{layout.RowHeight}\"/>");
		}
		sb.AppendLine("        </Grid.RowDefinitions>");

		// Размещение полей
		var rowIndex = 0;
		var colIndex = 0;
		foreach (FieldSchema field in _schema.Fields)
		{
			var span = field.Layout?.Span ?? 1;
			if (span > layout.Columns.Count) span = layout.Columns.Count;

			// Учет twoColumns
			if (layout.TwoColumns && span > 1) span = 1;

			if (field.Layout?.Row != null)
				rowIndex = field.Layout.Row.Value;

			var label = field.Label ?? field.Name;
			var placeholder = field.Placeholder ?? "";
			var requiredMark = field.Required ? " *" : "";

			// Label
			sb.AppendLine($"        <TextBlock Text=\"{label}{requiredMark}\" Grid.Row=\"{rowIndex}\" Grid.Column=\"{colIndex}\" VerticalAlignment=\"Center\"/>");

			// Control
			var control = field.Control;
			var controlXaml = control switch
			{
				"TextBox" => BuildTextBox(field, placeholder),
				"NumericUpDown" => BuildNumericUpDown(field),
				"DatePicker" => BuildDatePicker(field),
				"ComboBox" => BuildComboBox(field),
				"CheckBox" => BuildCheckBox(field),
				_ => "<!-- Неизвестный контрол -->"
			};

			var controlCol = colIndex + 1;
			var controlSpan = span;
			sb.AppendLine($"        {controlXaml} Grid.Row=\"{rowIndex}\" Grid.Column=\"{controlCol}\" Grid.ColumnSpan=\"{controlSpan}\"");

			rowIndex++;
			// Простое размещение: каждое поле на новой строке, если не указано иное
		}

		// Кнопки действий
		if (_schema.Actions.Count > 0)
		{
			sb.AppendLine("        <StackPanel Orientation=\"Horizontal\" HorizontalAlignment=\"Right\" Grid.Row=\"" + rowIndex + "\" Grid.Column=\"0\" Grid.ColumnSpan=\"" + layout.Columns.Count + "\">");
			foreach (ActionSchema action in _schema.Actions)
			{
				var text = action.Text ?? action.Name;
				var command = action.Command ?? $"{action.Name}Command";
				var isDefault = action.IsDefault ? " IsDefault=\"True\"" : "";
				var isCancel = action.IsCancel ? " IsCancel=\"True\"" : "";
				var isEnabled = !string.IsNullOrWhiteSpace(action.IsEnabledBinding) ? $" IsEnabled=\"{{Binding {action.IsEnabledBinding}}}\"" : "";
				sb.AppendLine($"            <Button Content=\"{text}\" Command=\"{{Binding {command}}}\"{isDefault}{isCancel}{isEnabled} Margin=\"5\"/>");
			}
			sb.AppendLine("        </StackPanel>");
		}

		sb.AppendLine("    </Grid>");
		sb.AppendLine($"</{form.Type}>");

		return sb.ToString();
	}

	private string BuildTextBox(FieldSchema field, string placeholder)
	{
		var sb = new StringBuilder();
		sb.Append("<TextBox Text=\"{Binding " + field.Name + ", UpdateSourceTrigger=PropertyChanged}\"");
		if (field.MaxLength > 0)
			sb.Append($" MaxLength=\"{field.MaxLength}\"");
		if (field.Multiline)
			sb.Append(" AcceptsReturn=\"True\" TextWrapping=\"Wrap\"");
		if (!string.IsNullOrWhiteSpace(placeholder) && !field.Multiline)
			sb.Append($" ToolTip=\"{placeholder}\"");
		sb.Append(" />");
		return sb.ToString();
	}

	private string BuildNumericUpDown(FieldSchema field)
	{
		var sb = new StringBuilder();
		sb.Append("<NumericUpDown Value=\"{Binding " + field.Name + "}\"");
		if (field.Min.HasValue)
			sb.Append($" Minimum=\"{field.Min}\"");
		if (field.Max.HasValue)
			sb.Append($" Maximum=\"{field.Max}\"");
		sb.Append(" />");
		return sb.ToString();
	}

	private string BuildDatePicker(FieldSchema field)
	{
		return "<DatePicker SelectedDate=\"{Binding " + field.Name + "}\" />";
	}

	private string BuildComboBox(FieldSchema field)
	{
		var sb = new StringBuilder();
		sb.Append("<ComboBox ItemsSource=\"{Binding " + field.ItemsSource + "}\"");
		sb.Append($" DisplayMemberPath=\"{field.DisplayMember}\"");
		sb.Append($" SelectedValuePath=\"{field.ValueMember}\"");
		sb.Append($" SelectedValue=\"{{Binding {field.Name}}}\"");
		sb.Append(" />");
		return sb.ToString();
	}

	private string BuildCheckBox(FieldSchema field)
	{
		return "<CheckBox IsChecked=\"{Binding " + field.Name + "}\" />";
	}

	private string BuildCodeBehind()
	{
		var className = _schema.Metadata.Name;
		var ns = _schema.Metadata.Namespace;
		var vmName = _schema.ViewModel.Name ?? $"{className}Model";
		var dataContext = _schema.CodeBehind.DataContext;

		var sb = new StringBuilder();
		sb.AppendLine("using System.Windows;");
		if (dataContext == "viewModel")
			sb.AppendLine($"using {(_schema.ViewModel.Namespace ?? $"{ns}.ViewModels")};");
		sb.AppendLine();
		sb.AppendLine($"namespace {ns};");
		sb.AppendLine();
		sb.AppendLine($"public partial class {className} : {_schema.Form.Type}");
		sb.AppendLine("{");
		sb.AppendLine($"    public {className}()");
		sb.AppendLine("    {");
		sb.AppendLine("        InitializeComponent();");
		if (dataContext == "viewModel")
		{
			sb.AppendLine($"        DataContext = new {vmName}();");
		}
		else if (dataContext == "self")
		{
			sb.AppendLine("        DataContext = this;");
		}
		sb.AppendLine("    }");
		sb.AppendLine("}");

		return sb.ToString();
	}

	private string BuildViewModel(string vmName, string vmNamespace)
	{
		var className = _schema.Metadata.Name;
		var toolkit = _schema.ViewModel.Toolkit;
		var baseType = _schema.ViewModel.BaseType;

		var sb = new StringBuilder();
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Collections.ObjectModel;");
		sb.AppendLine("using System.ComponentModel;");
		sb.AppendLine("using System.Runtime.CompilerServices;");
		sb.AppendLine("using System.Windows.Input;");

		if (toolkit == "CommunityToolkit.Mvvm")
		{
			sb.AppendLine("using CommunityToolkit.Mvvm.ComponentModel;");
			sb.AppendLine("using CommunityToolkit.Mvvm.Input;");
		}

		sb.AppendLine();
		sb.AppendLine($"namespace {vmNamespace};");
		sb.AppendLine();

		var baseClass = baseType switch
		{
			"ObservableObject" => "ObservableObject",
			"ViewModelBase" => "ViewModelBase",
			"none" => "",
			_ => "ObservableObject"
		};

		sb.AppendLine($"public partial class {vmName}" + (string.IsNullOrEmpty(baseClass) ? "" : $" : {baseClass}"));
		sb.AppendLine("{");

		// Свойства полей
		foreach (FieldSchema field in _schema.Fields)
		{
			var propertyType = field.Type switch
			{
				"string" => "string",
				"int" => "int",
				"double" => "double",
				"decimal" => "decimal",
				"bool" => "bool",
				"DateTime" => "DateTime",
				"Guid" => "Guid",
				_ => "string"
			};

			var backingField = $"_{char.ToLowerInvariant(field.Name[0])}{field.Name[1..]}";
			sb.AppendLine($"    private {propertyType} {backingField};");
			sb.AppendLine($"    public {propertyType} {field.Name}");
			sb.AppendLine("    {");
			sb.AppendLine($"        get => {backingField};");
			sb.AppendLine($"        set => SetProperty(ref {backingField}, value);");
			sb.AppendLine("    }");
			sb.AppendLine();
		}

		// Сервисы
		foreach (ServiceSchema service in _schema.ViewModel.Services)
		{
			var serviceType = service.Type ?? service.Name;
			var injectAs = service.InjectAs ?? $"_{char.ToLowerInvariant(service.Name[0])}{service.Name[1..]}";
			sb.AppendLine($"    private readonly {serviceType} {injectAs};");
		}
		sb.AppendLine();

		// Конструктор
		sb.AppendLine($"    public {vmName}(");
		var parameters = _schema.ViewModel.Services.Select(s =>
		{
			var serviceType = s.Type ?? s.Name;
			var paramName = char.ToLowerInvariant(s.Name[0]) + s.Name[1..];
			return $"{serviceType} {paramName}";
		}).ToList();
		sb.AppendLine(string.Join(",\n        ", parameters));
		sb.AppendLine("    )");
		sb.AppendLine("    {");
		foreach (ServiceSchema service in _schema.ViewModel.Services)
		{
			var injectAs = service.InjectAs ?? $"_{char.ToLowerInvariant(service.Name[0])}{service.Name[1..]}";
			var paramName = char.ToLowerInvariant(service.Name[0]) + service.Name[1..];
			sb.AppendLine($"        {injectAs} = {paramName};");
		}
		sb.AppendLine("    }");
		sb.AppendLine();

		// Команды
		foreach (ActionSchema action in _schema.Actions)
		{
			var commandName = action.Command ?? $"{action.Name}Command";
			var methodName = action.Name;
			sb.AppendLine($"    [RelayCommand]");
			sb.AppendLine($"    private void {methodName}()");
			sb.AppendLine("    {");
			sb.AppendLine("        // TODO: реализовать логику");
			sb.AppendLine("    }");
			sb.AppendLine();
		}

		sb.AppendLine("}");

		return sb.ToString();
	}
}