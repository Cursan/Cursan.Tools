using System;
using System.IO;
using System.Text;

namespace Cursan.Tools
{
	/// <summary>
	/// Генератор partial-расширения для DbContext с CRUD-методами.
	/// </summary>
	public static class DbContextCrudGenerator
	{
		/// <summary>
		/// Генерирует код и сохраняет его в файл.
		/// </summary>
		/// <param name="dbContextName">Имя DbContext (например, "AppDbContext").</param>
		/// <param name="targetNamespace">Пространство имён для сгенерированного класса.</param>
		/// <param name="outputDirectory">Каталог для сохранения файла.</param>
		/// <returns>Полный путь к созданному файлу.</returns>
		public static string Generate(
			string dbContextName,
			string targetNamespace,
			string outputDirectory = ".")
		{
			if (string.IsNullOrWhiteSpace(dbContextName))
				throw new ArgumentException("Имя DbContext не может быть пустым.", nameof(dbContextName));

			if (string.IsNullOrWhiteSpace(targetNamespace))
				throw new ArgumentException("Пространство имён не может быть пустым.", nameof(targetNamespace));

			Directory.CreateDirectory(outputDirectory);

			var code = BuildCode(dbContextName, targetNamespace);
			var fileName = $"{dbContextName}.Crud.g.cs";
			var filePath = Path.Combine(outputDirectory, fileName);

			File.WriteAllText(filePath, code, Encoding.UTF8);

			return filePath;
		}

		private static string BuildCode(string dbContextName, string targetNamespace)
		{
			// Используем сырую строку (C# 11+) для удобства.
			// Если проект на более старой версии C#, замените на обычную интерполяцию.
			return $$"""
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace {{targetNamespace}};

public partial class {{dbContextName}}
{
    /// <summary>
    /// Возвращает все записи указанной сущности.
    /// </summary>
    public List<T> GetAll<T>() where T : class
    {
        return Set<T>().ToList();
    }

    /// <summary>
    /// Возвращает записи, удовлетворяющие всем переданным предикатам (объединяются через AND).
    /// </summary>
    public List<T> GetWhile<T>(params Expression<Func<T, bool>>[] predicates) where T : class
    {
        IQueryable<T> query = Set<T>();

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        return query.ToList();
    }

    /// <summary>
    /// Удаляет сущность и сохраняет изменения.
    /// </summary>
    public void Delete<T>(T entity) where T : class
    {
        Set<T>().Remove(entity);
        SaveChanges();
    }

    /// <summary>
    /// Добавляет сущность и сохраняет изменения.
    /// </summary>
    public void Add<T>(T entity) where T : class
    {
        Set<T>().Add(entity);
        SaveChanges();
    }

    /// <summary>
    /// Обновляет сущность (помечает как Modified) и сохраняет изменения.
    /// </summary>
    public void Change<T>(T entity) where T : class
    {
        Set<T>().Update(entity);
        SaveChanges();
    }
}
""";
		}
	}
}