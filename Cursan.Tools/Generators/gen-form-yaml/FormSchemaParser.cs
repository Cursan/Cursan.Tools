using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cursan.Tools.Generators;

public sealed class FormSchemaParser
{
	private readonly IDeserializer _deserializer;

	public FormSchemaParser()
	{
		_deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.IgnoreUnmatchedProperties() // Строгий режим можно включить при необходимости
			.Build();
	}

	public FormSchema Parse(string yamlContent)
	{
		var schema = _deserializer.Deserialize<FormSchema>(yamlContent);
		if (schema == null)
			throw new InvalidOperationException("Не удалось десериализовать YAML-схему.");

		Validate(schema);
		return schema;
	}

	public FormSchema ParseFile(string filePath)
	{
		if (!File.Exists(filePath))
			throw new FileNotFoundException($"Файл схемы не найден: {filePath}");

		var yaml = File.ReadAllText(filePath);
		return Parse(yaml);
	}

	private void Validate(FormSchema schema)
	{
		// 1. Обязательные ключи
		if (schema.Version == 0)
			throw new InvalidOperationException("Отсутствует обязательный ключ 'version'.");
		if (string.IsNullOrWhiteSpace(schema.Metadata.Name))
			throw new InvalidOperationException("Отсутствует обязательный ключ 'metadata.name'.");
		if (string.IsNullOrWhiteSpace(schema.Metadata.Namespace))
			throw new InvalidOperationException("Отсутствует обязательный ключ 'metadata.namespace'.");
		if (schema.Fields == null || schema.Fields.Count == 0)
			throw new InvalidOperationException("Список 'fields' не может быть пустым.");

		// 2. Уникальность имён полей и действий
		var fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var field in schema.Fields)
		{
			if (string.IsNullOrWhiteSpace(field.Name))
				throw new InvalidOperationException("У поля отсутствует обязательный ключ 'name'.");
			if (!fieldNames.Add(field.Name))
				throw new InvalidOperationException($"Имя поля '{field.Name}' не уникально.");
			if (string.IsNullOrWhiteSpace(field.Control))
				throw new InvalidOperationException($"У поля '{field.Name}' отсутствует обязательный ключ 'control'.");
		}

		var actionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var action in schema.Actions)
		{
			if (string.IsNullOrWhiteSpace(action.Name))
				throw new InvalidOperationException("У действия отсутствует обязательный ключ 'name'.");
			if (!actionNames.Add(action.Name))
				throw new InvalidOperationException($"Имя действия '{action.Name}' не уникально.");
		}

		// 3. Согласованность type ↔ control
		foreach (var field in schema.Fields)
		{
			var control = field.Control;
			var type = field.Type;

			bool valid = control switch
			{
				"TextBox" => type == "string",
				"NumericUpDown" => type is "int" or "double" or "decimal",
				"DatePicker" => type == "DateTime",
				"CheckBox" => type == "bool",
				"ComboBox" => type is "int" or "string" or "Guid",
				_ => false
			};

			if (!valid)
				throw new InvalidOperationException($"Несовместимая пара control='{control}' и type='{type}' для поля '{field.Name}'.");

			// Дополнительные проверки для NumericUpDown
			if (control == "NumericUpDown")
			{
				if (field.Min.HasValue && field.Max.HasValue && field.Min > field.Max)
					throw new InvalidOperationException($"Для поля '{field.Name}' min ({field.Min}) больше max ({field.Max}).");

				if (field.Default != null && field.Min.HasValue && field.Max.HasValue)
				{
					if (!double.TryParse(field.Default.ToString(), out var defVal) ||
						defVal < field.Min || defVal > field.Max)
					{
						throw new InvalidOperationException($"Значение по умолчанию для поля '{field.Name}' выходит за пределы [min, max].");
					}
				}
			}

			// Проверка ComboBox
			if (control == "ComboBox")
			{
				if (string.IsNullOrWhiteSpace(field.ItemsSource))
					throw new InvalidOperationException($"Для ComboBox-поля '{field.Name}' не указан 'itemsSource'.");
				if (string.IsNullOrWhiteSpace(field.DisplayMember))
					throw new InvalidOperationException($"Для ComboBox-поля '{field.Name}' не указан 'displayMember'.");
				if (string.IsNullOrWhiteSpace(field.ValueMember))
					throw new InvalidOperationException($"Для ComboBox-поля '{field.Name}' не указан 'valueMember'.");
			}

			// Проверка span
			if (field.Layout != null && field.Layout.Span > schema.Form.Layout.Columns.Count)
				throw new InvalidOperationException($"Для поля '{field.Name}' span ({field.Layout.Span}) превышает количество колонок ({schema.Form.Layout.Columns.Count}).");
		}

		// 4. Согласованность команд ViewModel и действий
		if (schema.ViewModel.Generate)
		{
			foreach (var cmd in schema.ViewModel.Commands)
			{
				if (!actionNames.Contains(cmd))
					throw new InvalidOperationException($"Команда '{cmd}' в viewModel.commands не найдена среди actions.");
			}

			// Проверка injectAs на валидность идентификатора C#
			foreach (var service in schema.ViewModel.Services)
			{
				if (string.IsNullOrWhiteSpace(service.InjectAs)) continue;
				try
				{
					NamingHelper.EnsureValidIdentifier(service.InjectAs, "viewModel.services[].injectAs");
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException($"Некорректный injectAs '{service.InjectAs}': {ex.Message}");
				}
			}
		}

		// 5. Проверка default для DateTime
		foreach (var field in schema.Fields.Where(f => f.Control == "DatePicker"))
		{
			if (field.Default != null)
			{
				var defStr = field.Default.ToString();
				if (defStr != "today" && defStr != "now")
				{
					if (!DateTime.TryParse(defStr, out _))
						throw new InvalidOperationException($"Некорректное значение default для DatePicker-поля '{field.Name}': {defStr}");
				}
			}
		}
	}
}