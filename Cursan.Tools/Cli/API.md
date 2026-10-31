## 🏗️ Архитектура CLI

CLI состоит из четырёх ключевых компонентов:

- **`ICommand`** — контракт, который должна реализовать каждая команда.
- **`CommandContext`** — объект, передаваемый в команду при выполнении; содержит аргументы, опции и потоки вывода.
- **`CommandRegistry`** — реестр для регистрации и поиска команд.
- **`CliHost`** — точка входа, которая парсит аргументы, находит команду и запускает её.

---

## 📜 Интерфейс `ICommand`

Каждая команда обязана реализовать этот интерфейс:

```csharp
namespace Cursan.Tools.Cli;

public interface ICommand
{
    /// Имя команды, например "gen-crud".
    string Name { get; }

    /// Короткое описание для общего --help.
    string Description { get; }

    /// Строка использования для --help конкретной команды.
    string Usage { get; }

    /// Выполнить команду. Возвращает код выхода (0 — успех).
    int Execute(CommandContext context);
}
```

### Свойства

| Свойство | Тип | Назначение |
|----------|-----|------------|
| `Name` | `string` | Уникальное имя команды (регистронезависимое). |
| `Description` | `string` | Краткое описание, отображается в общем списке `--help`. |
| `Usage` | `string` | Строка с синтаксисом вызова, показывается в `cursan help <команда>`. |

### Метод `Execute`

Принимает `CommandContext` и возвращает целочисленный код выхода:
- `0` — успешное выполнение;
- `2` — ошибка аргументов (например, отсутствует обязательная опция);
- `1` — непредвиденная ошибка.

---

## 📦 Класс `CommandContext`

`CommandContext` инкапсулирует всё, что нужно команде для работы: позиционные аргументы, опции и потоки вывода.

### Свойства и методы

| Член | Тип | Описание |
|------|-----|----------|
| `Arguments` | `IReadOnlyList<string>` | Позиционные аргументы командной строки. |
| `Out` | `TextWriter` | Поток для стандартного вывода (результаты работы). |
| `Error` | `TextWriter` | Поток для вывода ошибок. |
| `Options` | `IReadOnlyDictionary<string, string>` | Словарь всех переданных опций (ключ — имя без префикса `--`). |
| `HasOption(string name)` | `bool` | Проверяет, была ли указана опция (даже без значения). |
| `GetOption(string name)` | `string?` | Возвращает значение опции или `null`, если опция не задана. |
| `GetOption(string name, string defaultValue)` | `string` | Возвращает значение опции или `defaultValue`, если опция отсутствует. |
| `RequireOption(string name)` | `string` | Возвращает значение опции или выбрасывает `CommandLineException`, если опция не указана. |
| `GetFlag(string name)` | `bool` | Интерпретирует опцию как флаг: `true`, если опция присутствует и её значение не равно `"false"` (без учёта регистра). |

### Пример использования в команде

```csharp
public int Execute(CommandContext context)
{
    // Обязательная опция --schema
    var schemaPath = context.RequireOption("schema");

    // Опция --output со значением по умолчанию "."
    var outputDir = context.GetOption("output", ".");

    // Флаг --force
    var force = context.GetFlag("force");

    context.Out.WriteLine($"Генерация из {schemaPath} в {outputDir} (force={force})");
    return 0;
}
```

---

## 🗂️ Класс `CommandRegistry`

Реестр команд обеспечивает регистрацию и поиск по имени (без учёта регистра).

### Методы

| Метод | Описание |
|-------|----------|
| `Register(ICommand command)` | Добавляет команду в реестр. Если команда с таким именем уже зарегистрирована, выбрасывается `InvalidOperationException`. Возвращает сам реестр для цепочки вызовов. |
| `TryGet(string name, out ICommand command)` | Пытается найти команду по имени. Возвращает `true`, если команда найдена. |
| `Commands` | Свойство, возвращающее перечисление всех зарегистрированных команд (для вывода справки). |

---

## 🚀 Хост `CliHost`

`CliHost` — это точка входа, которая связывает всё вместе.

### Конструктор

```csharp
public CliHost(CommandRegistry registry, TextWriter output, TextWriter error)
```

### Метод `Run`

```csharp
public int Run(string[] args)
```

Алгоритм работы:
1. Если аргументы не переданы — выводит общую справку и возвращает `0`.
2. Парсит аргументы с помощью `CommandLineParser.Parse`.
3. Если первая команда — `help`:
   - при наличии позиционного аргумента выводит справку по указанной команде;
   - иначе выводит общую справку.
4. Если команда не найдена — выводит сообщение об ошибке в `Error` и возвращает `2`.
5. Если среди опций есть `--help` — выводит справку по команде и возвращает `0`.
6. Создаёт `CommandContext` и вызывает `command.Execute(context)`.
7. Обрабатывает исключения:
   - `CommandLineException` → сообщение об ошибке аргументов, код `2`;
   - любое другое исключение → сообщение о непредвиденной ошибке, код `1`.

---

## 🛠️ Пошаговое руководство по созданию команды

### 1. Реализуйте интерфейс `ICommand`

Создайте класс, который будет выполнять нужную логику.

```csharp
using Cursan.Tools.Cli;

namespace MyApp.Commands;

public sealed class GenYamlCommand : ICommand
{
    public string Name => "gen-yaml";
    public string Description => "Генерирует WPF-форму из YAML-схемы.";
    public string Usage => "cursan gen-yaml --schema <путь_к_yaml> [--output <каталог>] [--force]";

    public int Execute(CommandContext context)
    {
        var schemaPath = context.RequireOption("schema");
        var outputDir = context.GetOption("output", ".");
        var force = context.GetFlag("force");

        // Здесь размещается логика генерации.
        // Например, вызов соответствующего генератора из Cursan.Tools.Generators.

        context.Out.WriteLine($"Форма сгенерирована в {outputDir}");
        return 0;
    }
}
```

### 2. Зарегистрируйте команду в `CommandRegistry`

При инициализации приложения добавьте команду в реестр.

```csharp
var registry = new CommandRegistry();
registry.Register(new GenYamlCommand());
// При необходимости зарегистрируйте другие команды.
```

### 3. Передайте реестр в `CliHost` и запустите

```csharp
var host = new CliHost(registry, Console.Out, Console.Error);
int exitCode = host.Run(args);
Environment.Exit(exitCode);
```

После этого команда станет доступна как `cursan gen-yaml ...`. Справка по ней будет выводиться по запросу `cursan help gen-yaml`.

---

## ⚠️ Обработка ошибок и коды возврата

| Ситуация | Исключение / поведение | Код выхода |
|----------|------------------------|------------|
| Отсутствует обязательная опция | `CommandLineException` (генерируется `RequireOption`) | `2` |
| Неизвестная команда | Сообщение в `Error` | `2` |
| Ошибка парсинга аргументов | `CommandLineException` | `2` |
| Любая другая ошибка в команде | Неперехваченное исключение | `1` |
| Успешное выполнение | — | `0` |

Рекомендуется в командах выбрасывать `CommandLineException` для ошибок, связанных с аргументами, чтобы хост мог корректно обработать их и вернуть код `2`.

---

## 💡 Рекомендации

- **Имена команд** делайте краткими и в kebab-case (`gen-crud`, `gen-form-yaml`).
- **Описание** должно быть понятным в одну строку — оно выводится в общем списке `--help`.
- **Строка `Usage`** должна точно отражать синтаксис, включая обязательные и необязательные опции.
- **Вывод результатов** направляйте в `context.Out`, а ошибки — в `context.Error`, чтобы обеспечить корректное перенаправление потоков.
- **Флаги** обрабатывайте через `GetFlag`, чтобы опция `--force` без значения трактовалась как `true`.