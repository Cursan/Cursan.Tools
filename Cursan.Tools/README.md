# Cursan.Tools

**Cursan.Tools** — это набор утилит для генерации кода в проектах .NET, использующих Entity Framework Core и WPF. Инструмент автоматизирует создание CRUD‑расширений для `DbContext`, WPF‑форм, а также YAML‑схем форм на основе структуры таблиц SQL Server.

## Возможности

- Генерация partial‑расширения для `DbContext` с CRUD‑методами (`GetAll`, `GetWhile`, `Add`, `Change`, `Delete`).
- Генерация WPF‑окна формы (XAML + code‑behind) по списку полей.
- Генерация YAML‑схемы формы (формат `cursan.dev/form-v1`) по структуре таблицы SQL Server.
- Генерация WPF‑формы по готовой YAML‑схеме.
- Использование настраиваемых шаблонов для гибкого управления выходным кодом.
- Удобный интерфейс командной строки (CLI) с поддержкой опций и флагов.

## Установка

Пакет планируется к публикации в NuGet. После публикации установка будет выполняться стандартными средствами:

```bash
dotnet add package Cursan.Tools
```

Или через менеджер пакетов в Visual Studio.

> **Примечание:** На данный момент пакет находится в стадии подготовки к публикации. Следите за обновлениями.

## Требования

- .NET SDK версии 6.0 или выше.
- Проект, использующий Entity Framework Core (для команды `gen-crud`).
- Для команд `scaffold-yaml` и `gen-form-yaml` требуется доступ к серверу SQL Server и (для `gen-form-yaml`) среда WPF.

## Использование

Все команды вызываются через исполняемый файл `cursan` (или `dotnet run` в проекте CLI). Общий синтаксис:

```bash
cursan <команда> [опции]
```

Чтобы получить список доступных команд, выполните:

```bash
cursan --help
```

Для получения справки по конкретной команде:

```bash
cursan help <команда>
```

### Команда `gen-crud`

Генерирует partial‑расширение для указанного `DbContext` с CRUD‑методами.

**Синтаксис:**

```bash
cursan gen-crud --context <ИмяDbContext> --namespace <ПространствоИмён> [--output <каталог>] [--force]
```

**Параметры:**

| Опция | Обязательна | Описание |
|-------|-------------|----------|
| `--context` | Да | Имя класса `DbContext` (например, `AppDbContext`). |
| `--namespace` | Да | Пространство имён для сгенерированного кода. |
| `--output` | Нет | Каталог для сохранения файла (по умолчанию `.`). |
| `--force` | Нет | Перезаписать существующий файл. |

**Пример:**

```bash
cursan gen-crud --context AppDbContext --namespace MyApp.Data --output ./Generated
```

В результате будет создан файл `AppDbContext.Crud.g.cs` с методами `GetAll`, `GetWhile`, `Add`, `Change`, `Delete`.

### Команда `gen-form`

Генерирует WPF‑окно формы (XAML + code‑behind) по списку полей.

**Синтаксис:**

```bash
cursan gen-form --name <ИмяКласса> --namespace <ПространствоИмён> --fields "Имя:Тип;..." [--title <Заголовок>] [--output <каталог>] [--force]
```

**Параметры:**

| Опция | Обязательна | Описание |
|-------|-------------|----------|
| `--name` | Да | Имя класса формы (например, `PersonForm`). |
| `--namespace` | Да | Пространство имён для сгенерированного кода. |
| `--fields` | Да | Список полей в формате `Имя:Тип`, разделённых `;` (например, `"Name:string;Age:int"`). |
| `--title` | Нет | Заголовок окна (по умолчанию гуманизированное имя класса). |
| `--output` | Нет | Каталог для сохранения файлов (по умолчанию `.`). |
| `--force` | Нет | Перезаписать существующие файлы. |

**Пример:**

```bash
cursan gen-form --name PersonForm --namespace MyApp.Views --fields "Name:string;Age:int" --title "Редактирование персоны"
```

Будут созданы файлы `PersonForm.xaml` и `PersonForm.xaml.cs`.

### Команда `gen-form-yaml`

Генерирует WPF‑форму по готовой YAML‑схеме (формат `cursan.dev/form-v1`).

**Синтаксис:**

```bash
cursan gen-form-yaml --schema <путь_к_yaml> [--output <каталог>] [--force]
```

**Параметры:**

| Опция | Обязательна | Описание |
|-------|-------------|----------|
| `--schema` | Да | Путь к YAML‑файлу со схемой формы. |
| `--output` | Нет | Каталог для сохранения сгенерированных файлов (по умолчанию `.`). |
| `--force` | Нет | Перезаписать существующие файлы. |

**Пример:**

```bash
cursan gen-form-yaml --schema ./person.form.yaml --output ./Views
```

Команда создаст XAML‑файл и code‑behind на основе схемы.

### Команда `scaffold-yaml`

Создаёт YAML‑схему формы (`cursan.dev/form-v1`) по структуре таблицы SQL Server. Начиная с последней версии, по умолчанию генерируются схемы для **всех таблиц** указанной схемы.

**Синтаксис:**

```bash
cursan scaffold-yaml --connection <строка_подключения> [--table <Таблица|all>] [--schema dbo] [--form-name <Класс>] [--namespace <ПространствоИмён>] [--output <каталог>] [--force]
```

**Параметры:**

| Опция | Обязательна | Описание |
|-------|-------------|----------|
| `--connection` | Да | Строка подключения к SQL Server. |
| `--table` | Нет | Имя таблицы или `all` (по умолчанию `all` — все таблицы схемы). |
| `--schema` | Нет | Схема базы данных (по умолчанию `dbo`). |
| `--form-name` | Нет | Имя класса формы (если не указано, выводится из имени таблицы). |
| `--namespace` | Нет | Пространство имён для генерируемых схем. |
| `--output` | Нет | Каталог для сохранения YAML‑файлов (по умолчанию `.`). |
| `--force` | Нет | Перезаписать существующие файлы. |

**Примеры:**

Сгенерировать YAML‑схемы для **всех таблиц** схемы `dbo`:

```bash
cursan scaffold-yaml --connection "Server=.;Database=MyDb;Trusted_Connection=True;"
```

Сгенерировать схему только для таблицы `Products`:

```bash
cursan scaffold-yaml --connection "Server=.;Database=MyDb;Trusted_Connection=True;" --table Products --output ./Forms
```

Для каждой таблицы будет создан файл `<ИмяТаблицы>.form.yaml`.

## Архитектура CLI

CLI построен на четырёх ключевых компонентах:

- **`ICommand`** — контракт, который реализует каждая команда.
- **`CommandContext`** — объект, передаваемый в команду; содержит аргументы, опции и потоки вывода.
- **`CommandRegistry`** — реестр для регистрации и поиска команд.
- **`CliHost`** — точка входа, которая парсит аргументы, находит команду и запускает её.

Все команды регистрируются в `Entry.cs`:

```csharp
var registry = new CommandRegistry()
    .Register(new CrudExtensionCommand())
    .Register(new FormCommand())
    .Register(new YamlFormCommand())
    .Register(new ScaffoldYamlCommand());
return new CliHost(registry, Console.Out, Console.Error).Run(args);
```



## Структура проекта

| Проект / каталог | Назначение |
|------------------|------------|
| `Cursan.Tools.Cli` | Реализация интерфейса командной строки: `ICommand`, `CommandContext`, `CommandRegistry`, `CliHost`. |
| `Cursan.Tools.Generators` | Генераторы кода и связанные с ними шаблоны: `gen-crud`, `gen-form`, `gen-form-yaml`, `scaffold-yaml`. |
| `Cursan.Tools.Infrastructure` | Вспомогательные классы: `CodeFileWriter` (запись файлов), `NamingHelper` (работа с именами), `TemplateEngine` (шаблонизатор). |
| `DbContextCrudGenerator` | Основной класс для генерации CRUD‑кода на основе `DbContext` (используется командой `gen-crud`). |



## Разработка

Для сборки проекта выполните:

```bash
dotnet build Cursan.Tools.slnx
```

Запуск CLI из исходников:

```bash
dotnet run --project Cursan.Tools.Cli -- <команда> [опции]
```

## Лицензия

Проект распространяется под лицензией **MIT**. Подробности см. в файле [LICENSE.txt](https://github.com/Cursan/Cursan.Tools/blob/master/Cursan.Tools/LICENSE.txt).

## Вклад

Если вы хотите внести свой вклад, создайте issue или отправьте pull request. Перед отправкой PR убедитесь, что код соответствует стилю проекта и проходит существующие тесты.