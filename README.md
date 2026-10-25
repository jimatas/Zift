![Zift Logo](https://raw.githubusercontent.com/jimatas/Zift/master/assets/Zift_Logo_300x300.png)

[![NuGet](https://img.shields.io/nuget/v/Zift.svg?label=Zift)](https://www.nuget.org/packages/Zift)
[![NuGet](https://img.shields.io/nuget/v/Zift.EntityFrameworkCore.svg?label=Zift.EntityFrameworkCore)](https://www.nuget.org/packages/Zift.EntityFrameworkCore)
[![CI](https://github.com/jimatas/Zift/actions/workflows/ci.yml/badge.svg)](https://github.com/jimatas/Zift/actions/workflows/ci.yml)
[![Build & Publish](https://github.com/jimatas/Zift/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/jimatas/Zift/actions/workflows/nuget-publish.yml)

Zift adds dynamic filtering and pagination to `IQueryable<T>` using familiar LINQ-style methods.

- **Dynamic filtering**: string expressions with `Where(...)`
- **Pagination**: offset-based and cursor-based pagination

## Installation

```
dotnet add package Zift
```

If you use Entity Framework Core, install the EF Core package instead. It includes Zift and adds the async methods (`ToPageAsync` and `ToCursorPageAsync`):

```
dotnet add package Zift.EntityFrameworkCore
```

Zift targets .NET 8, 9, and 10. The EF Core package uses the EF Core major version that matches your target framework.

## Dynamic Filtering

Filter queries using string expressions. Zift translates them into LINQ expression trees.

```csharp
var categories = context.Categories
    .Where("Products:any(Reviews:any(Rating >= 4))")
    .ToList();
```

### Supported Syntax

- **Property navigation**
  `Name`, `Author.Email`

  Names are limited to ASCII letters, digits, and `_`, and can't start with a digit.

- **Logical operators**
  `&&`, `||`, `!`

  Example: `Price > 100 && Price < 500`

- **Grouping**
  `(...)` for evaluation order

  Example: `(Price > 100 && Price < 500) || IsFeatured == true`

- **Comparison operators**
  `==`, `!=`, `<`, `<=`, `>`, `>=`

  Example: `Rating >= 4`

- **String operators**
  `%=` (contains), `^=` (starts with), `$=` (ends with)

  Example: `Title %= "C#"`

- **`in` operator** with list literals

  Example: `Status in ["Active", "Pending"]`

- **Quantifiers on collections**
  - `:any(predicate)`
  - `:all(predicate)`
  - `:any()` (existence check)

  Example: `Reviews:any(Author.Email $= "@example.com")`

- **Collection projections**
  - `:count`

  Example: `Products:count >= 2`

A comparison is always of the form `property operator literal`, including boolean properties (`IsFeatured == true`). Comparing two properties (`Price > Cost`) and arithmetic (`Price * 2`) aren't supported.

### Literals

- **Strings** use single or double quotes: `"Active"` or `'Active'`. Supported escapes are `\\`, `\"`, and `\'`.
- **Numbers** include integers, decimals, and exponents: `42`, `-1.5`, `1e3`. A leading `+` or `-` is optional.
- **Booleans and `null`** use `true`, `false`, and `null`.
- **Dates, times, and GUIDs** use quoted strings: `"2024-03-05T10:20:30Z"`, `"2024-03-05"`, or `"10:20:30"`, depending on the property type. All standard GUID formats are supported.
- **Enums** use quoted, case-sensitive member names: `Status == "Active"`. Numeric strings aren't supported. For `[Flags]` enums, separate names with commas.

Numbers must fit the property's type. For an `int` property, `1.0` and `1e3` are valid; `1.5` and `3000000000` (exceeds `int.MaxValue`) aren't. Fractional values aren't rounded to integers.

Databases compare strings using the column's collation. In-memory sources use the current culture for `^=`, `$=`, `<`, `<=`, `>`, and `>=`, and ordinal comparison for `%=`, `==`, and `!=`.

### Null and Empty Lists

Use `== null` or `!= null` to check for null. Other direct comparisons with `null` aren't supported.

Lists can include `null` when the property is nullable:

```csharp
items.Where("Rating in [4, 5, null]");
```

An empty list matches nothing; negating it matches everything. A list containing only `null` matches null values.

With `EnableNullGuards`, a missing navigation counts as null instead of throwing, and comparisons follow normal C# rules. `Author.Name == null` matches rows without an author, `Author.Name != "Ann"` includes them, and `Author.Rating > 3` doesn't.

A null collection counts as empty: `Reviews:count == 0` matches it, `Reviews:any()` doesn't, and `Reviews:all(...)` does. A value type behind an optional navigation (`Category.Rank == null`) can only be compared with `null` when guards are on.

### Options and Limits

Pass `ExpressionBuilderOptions` to `Where(...)` to change the defaults:

```csharp
var options = new ExpressionBuilderOptions
{
    EnableNullGuards = true,
    MaxExpressionLength = 8192
};

var products = items.Where("Category.Name == \"Books\"", options);
```

| Option | Default | Description |
| --- | --- | --- |
| `EnableNullGuards` | `false` | Treats a null navigation as null instead of throwing. Useful for in-memory queries. |
| `ParameterizeValues` | `true` | Captures values so providers such as EF Core can parameterize queries and reuse query plans. |
| `MaxExpressionLength` | `4096` | Filter length limit, in characters. |
| `MaxNestingDepth` | `64` | Nesting limit for groups, `!`, and quantifier predicates. |
| `MaxListItems` | `1000` | List size limit. |

Limits must be at least 1. Keep them bounded when accepting filters from users.

### Security

Filters can access any public property, including nested ones. Zift doesn't enforce an allowlist.

When filters come from users:

- Filter over a projection that only has the properties users are allowed to filter on, not the full entity.
- Restrict the query to records the user is allowed to access before applying their filter.
- Remember that filtering reveals values. A user who can't read a property can still probe it with `Email ^= "a"`.
- Keep the limits above bounded.

### Errors

Invalid filters throw `InvalidFilterException`, which derives from `FormatException`. This includes syntax errors, unknown properties, invalid values, unsupported operators, and exceeded limits.

```csharp
try
{
    var products = items.Where(filter);
}
catch (InvalidFilterException ex)
{
    // Handle the invalid filter. ex.Message describes the problem, and ex.Position says where when it has one.
}
```

`SyntaxErrorException` and `LexicalErrorException` derive from `InvalidFilterException`. `Position` is set for those two and is `null` for the rest. When a .NET parse, conversion, or lookup was behind the error, the original exception is in `InnerException`.

A `null` filter throws `ArgumentNullException`. An empty or whitespace-only one throws `ArgumentException`.

> **Note**: Errors the database provider raises when the query runs, such as EF Core not being able to translate a property, aren't wrapped.

## Pagination

Zift supports two ways to paginate `IQueryable<T>` queries:

- **Offset-based pagination**
- **Cursor-based pagination**

### Offset Pagination

Offset pagination uses 1-based page numbers and LINQ's `Skip` / `Take`. It works best for smaller datasets where you need a total count.

```csharp
var page = await context.Categories
    .OrderBy(c => c.Name)
    .ToPageAsync(pageNumber: 1, pageSize: 25);
```

Use `ToPageAsync` with Entity Framework Core. For other sources, such as in-memory collections, use `ToPage`:

```csharp
var page = categories
    .AsQueryable()
    .OrderBy(c => c.Name)
    .ToPage(pageNumber: 1, pageSize: 25);
```

Use a consistent ordering to avoid skipped or repeated rows between pages. `pageNumber` and `pageSize` must be at least 1. An offset greater than `int.MaxValue` throws `ArgumentOutOfRangeException`.

The count and items come from separate queries, so changes between them can leave the count out of sync with the items.

#### Result Shape

```csharp
page.Items
page.PageNumber
page.PageSize
page.PageCount
page.HasNextPage
page.HasPreviousPage
page.TotalItemCount
```

Use `page.Map(...)` to convert the items while keeping the paging information, and `Page<T>.Empty(pageSize)` for an empty first page:

```csharp
var dtoPage = page.Map(category => new CategoryDto(category));
```

### Cursor Pagination

Cursor pagination (keyset pagination) uses ordering keys instead of offsets to navigate through results. It performs well even on large datasets.

Start by calling `AsCursorQuery()` on the `IQueryable<T>`.

```csharp
var query = context.Products
    .AsCursorQuery()
    .OrderBy("Name ASC, Price DESC, Id ASC");

var page = query.ToCursorPage(pageSize: 25);
```

With Entity Framework Core, use `ToCursorPageAsync` instead:

```csharp
var page = await query.ToCursorPageAsync(pageSize: 25);
```

> **Note**: Ordering for cursor pagination must be applied *after* calling `AsCursorQuery()`. Any ordering applied before `AsCursorQuery()` is ignored.

Cursor pagination needs at least one ordering clause.

#### Ordering

You can also use key selectors for ordering. Add more keys with `ThenBy`, `ThenByDescending`, or `ThenBy(string)`:

```csharp
var query = context.Products
    .AsCursorQuery()
    .OrderBy(p => p.Name)
    .ThenByDescending(p => p.Price)
    .ThenBy(p => p.Id);
```

In the string syntax, use dots for nested paths (`Category.Title`). Names aren't case-sensitive. Each entry is `Property [ASC|DESC]`. A bad entry or unknown property throws `InvalidOrderingClauseException`. For in-memory sources, pass `new OrderingOptions { EnableNullGuards = true }` to guard nested paths against null.

Ordering keys can be strings, numbers, booleans, enums, or any type with comparison operators or `IComparable<T>` (`Guid`, `DateTime`, `DateTimeOffset`). Unsupported types throw `NotSupportedException` when navigating beyond the first page.

The ordering must uniquely identify each row, so end it with a unique key such as `Id`. Rows that tie on every key can be skipped between pages.

An ordering can have up to 16 keys by default, configurable through `CursorQueryOptions.MaxOrderingKeys`. Exceeding this limit throws `InvalidOrderingClauseException`.

By default, Zift assumes nulls sort first in ascending order (SQL Server, SQLite, MySQL). For databases that sort them last (PostgreSQL, Oracle), pass `new CursorQueryOptions { NullOrdering = NullOrdering.NullsCompareHigher }` to `AsCursorQuery()`.

#### Navigating

Cursors are URL-safe tokens tied to the entity type and ordering that produced them. Using one with a different ordering or entity type throws `InvalidCursorException`. They aren't encrypted or signed.

Reuse the ordered query and pass the current page's cursors to navigate. A `null` cursor selects the first page.

```csharp
// Forward traversal
if (page.HasNextPage)
{
    var nextPage = query
        .After(page.EndCursor)
        .ToCursorPage(pageSize: 25);
}

// Backward traversal
if (page.HasPreviousPage)
{
    var previousPage = query
        .Before(page.StartCursor)
        .ToCursorPage(pageSize: 25);
}
```

On a page with items, `HasPreviousPage` is assumed `true` after `After(...)`, and `HasNextPage` after `Before(...)`. No extra query is made to verify this, so changes between requests can make these values inaccurate.

#### Page Size

`pageSize` must be between 1 and `int.MaxValue - 1`. Public APIs should enforce a much smaller maximum of their own.

#### Result Shape

```csharp
page.Items
page.StartCursor
page.EndCursor
page.HasNextPage
page.HasPreviousPage
```

Use `page.Map(...)` to convert the items and keep the cursors. `CursorPage<T>.Empty` is a page without items.
