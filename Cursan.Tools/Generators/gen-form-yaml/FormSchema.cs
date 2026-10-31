namespace Cursan.Tools.Generators;

// Корневая модель
public sealed class FormSchema
{
	public int Version { get; set; }
	public Metadata Metadata { get; set; } = new();
	public FormSettings Form { get; set; } = new();
	public ViewModelSettings ViewModel { get; set; } = new();
	public CodeBehindSettings CodeBehind { get; set; } = new();
	public List<FieldSchema> Fields { get; set; } = new();
	public List<ActionSchema> Actions { get; set; } = new();
}

public sealed class Metadata
{
	public string Name { get; set; } = string.Empty;
	public string Namespace { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Output { get; set; } = "./";
	public bool Overwrite { get; set; } = false;
}

public sealed class FormSettings
{
	public string Type { get; set; } = "Window"; // Window | UserControl | Page
	public string? Title { get; set; }
	public string Width { get; set; } = "auto";
	public string Height { get; set; } = "auto";
	public int MinWidth { get; set; } = 0;
	public int MinHeight { get; set; } = 0;
	public string StartupLocation { get; set; } = "Manual";
	public bool Resizable { get; set; } = true;
	public FormLayout Layout { get; set; } = new();
}

public sealed class FormLayout
{
	public List<string> Columns { get; set; } = new() { "Auto", "*" };
	public int RowHeight { get; set; } = 28;
	public int Margin { get; set; } = 0;
	public int Spacing { get; set; } = 4;
	public bool TwoColumns { get; set; } = false;
}

public sealed class ViewModelSettings
{
	public bool Generate { get; set; } = true;
	public string? Name { get; set; }
	public string? Namespace { get; set; }
	public string BaseType { get; set; } = "ObservableObject";
	public string Toolkit { get; set; } = "CommunityToolkit.Mvvm";
	public List<string> Commands { get; set; } = new();
	public List<ServiceSchema> Services { get; set; } = new();
}

public sealed class ServiceSchema
{
	public string Name { get; set; } = string.Empty;
	public string? Type { get; set; }
	public string? InjectAs { get; set; }
}

public sealed class CodeBehindSettings
{
	public bool Generate { get; set; } = true;
	public string DataContext { get; set; } = "viewModel"; // viewModel | self | null
}

// Общие свойства поля
public class FieldSchema
{
	public string Name { get; set; } = string.Empty;
	public string? Label { get; set; }
	public string Type { get; set; } = "string";
	public string? Control { get; set; }
	public bool Required { get; set; } = false;
	public object? Default { get; set; }
	public string? Placeholder { get; set; }
	public int MaxLength { get; set; } = 0;
	public double? Min { get; set; }
	public double? Max { get; set; }
	public bool Multiline { get; set; } = false;
	public string? ItemsSource { get; set; }
	public string? DisplayMember { get; set; }
	public string? ValueMember { get; set; }
	public ValidationSchema? Validation { get; set; }
	public FieldLayoutSchema? Layout { get; set; }
}

// Конкретные типы полей (можно использовать один класс с полиморфизмом, но для наглядности – отдельные)
public sealed class TextBoxField : FieldSchema { }
public sealed class NumericUpDownField : FieldSchema { }
public sealed class DatePickerField : FieldSchema { }
public sealed class ComboBoxField : FieldSchema { }
public sealed class CheckBoxField : FieldSchema { }

public sealed class ValidationSchema
{
	public string? Pattern { get; set; }
	public string? Message { get; set; }
}

public sealed class FieldLayoutSchema
{
	public int Span { get; set; } = 1;
	public int? Row { get; set; }
}

public sealed class ActionSchema
{
	public string Name { get; set; } = string.Empty;
	public string? Text { get; set; }
	public string? Command { get; set; }
	public bool IsDefault { get; set; } = false;
	public bool IsCancel { get; set; } = false;
	public string? IsEnabledBinding { get; set; }
	public string? XIcon { get; set; } // Соответствует 'x-icon' в YAML
}