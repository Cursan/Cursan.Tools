namespace Cursan.Tools.Generators;

public sealed class FormSchemaYamlWriter
{
	private readonly ISerializer _serializer;

	public FormSchemaYamlWriter()
	{
		_serializer = new SerializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
			.Build();
	}

	public string Write(FormSchema schema) => _serializer.Serialize(schema);
}