![Zift Logo](https://raw.githubusercontent.com/jimatas/Zift/master/assets/Zift_Logo_300x300.png)

[![NuGet](https://img.shields.io/nuget/v/Zift.svg?label=Zift)](https://www.nuget.org/packages/Zift)
[![NuGet](https://img.shields.io/nuget/v/Zift.EntityFrameworkCore.svg?label=Zift.EntityFrameworkCore)](https://www.nuget.org/packages/Zift.EntityFrameworkCore)
[![Build & Publish](https://github.com/jimatas/Zift/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/jimatas/Zift/actions/workflows/nuget-publish.yml)

Zift is a lightweight, composable query library for .NET that adds dynamic filtering and pagination on top of `IQueryable<T>`.

It builds on existing `IQueryable<T>` queries using familiar LINQ operators such as `Where(...)` and `OrderBy(...)`.

The library focuses on two core capabilities:

- **Dynamic querying**: string-based filtering via `Where(...)`
- **Pagination**: offset-based and cursor-based pagination

## Dynamic Querying

Zift allows dynamic filtering using a compact, expressive string syntax that is parsed and translated into LINQ expression trees.

```csharp
var categories = context.Categories
    .Where("Products:any(Reviews:any(Rating >= 4))")
    .ToList();
```

### Supported Syntax

- **Property navigation**
  `Name`, `Author.Email`

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
  
## Pagination

Zift supports two pagination strategies that operate directly on `IQueryable<T>` and can be combined with dynamic querying:

- **Offset-based pagination**
- **Cursor-based pagination**

### Offset Pagination

Offset pagination is 1-based and uses `Skip` / `Take` under the hood. It is best suited for small to medium result sets where total counts are required.

```csharp
var page = await context.Categories
    .OrderBy(c => c.Name)
    .ToPageAsync(pageNumber: 1, pageSize: 25);
```

#### Result shape

```csharp
page.Items
page.PageNumber
page.PageSize
page.PageCount
page.HasNextPage
page.HasPreviousPage
page.TotalItemCount
```

### Cursor Pagination

Cursor pagination (keyset pagination) is designed for stable and efficient traversal over ordered data, particularly for large or unbounded result sets.

It is enabled by calling `AsCursorQuery()` on the `IQueryable<T>`.

```csharp
var page = context.Products
    .AsCursorQuery()
    .OrderBy("Name ASC, Price DESC, Id ASC")
    .ToCursorPage(pageSize: 25);
```

> **Note**: Ordering for cursor pagination must be applied *after* calling `AsCursorQuery()`. Any ordering applied before `AsCursorQuery()` is ignored.

Cursor pagination requires at least one ordering clause to be provided; otherwise execution fails.

The combined ordering keys must uniquely identify each row, so end the ordering with a unique key such as `Id`. Rows that tie on every key can be skipped between pages.

Null keys are assumed to sort first when ascending (SQL Server, SQLite, MySQL). For sources that sort nulls last, such as PostgreSQL and Oracle, pass `new CursorQueryOptions { NullOrdering = NullOrdering.NullsCompareHigher }` to `AsCursorQuery()`.

Cursors are URL-safe tokens tied to the ordering that produced them; using one with a different ordering throws `InvalidCursorException`. They are not encrypted or signed.

To continue navigation from a previously retrieved page, use the cursor values exposed by that page as anchors for further traversal.

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

#### Result shape

```csharp
page.Items
page.StartCursor
page.EndCursor
page.HasNextPage
page.HasPreviousPage
```