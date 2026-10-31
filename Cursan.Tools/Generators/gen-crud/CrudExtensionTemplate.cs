using Cursan.Tools.Infrastructure;

namespace Cursan.Tools.Generators;

public static class CrudExtensionTemplate
{
	public static string Build(string dbContextName, string targetNamespace) =>
		TemplateEngine.Render(RawTemplate, new Dictionary<string, string>
		{
			["Namespace"] = targetNamespace,
			["Name"] = dbContextName,
		});

	private const string RawTemplate = """
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace {{Namespace}};

public partial class {{Name}}
{
    /// <summary>Возвращает все записи указанной сущности.</summary>
    public List<T> GetAll<T>() where T : class
        => Set<T>().ToList();

    /// <summary>Возвращает записи, удовлетворяющие всем предикатам (AND).</summary>
    public List<T> GetWhile<T>(params Expression<Func<T, bool>>[] predicates) where T : class
    {
        IQueryable<T> query = Set<T>();
        foreach (var predicate in predicates)
            query = query.Where(predicate);
        return query.ToList();
    }

    /// <summary>Удаляет сущность и сохраняет изменения.</summary>
    public void Delete<T>(T entity) where T : class
    {
        Set<T>().Remove(entity);
        SaveChanges();
    }

    /// <summary>Добавляет сущность и сохраняет изменения.</summary>
    public void Add<T>(T entity) where T : class
    {
        Set<T>().Add(entity);
        SaveChanges();
    }

    /// <summary>Помечает сущность как Modified и сохраняет изменения.</summary>
    public void Change<T>(T entity) where T : class
    {
        Set<T>().Update(entity);
        SaveChanges();
    }
}
""";
}