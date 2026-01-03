# Zift

Zift is a lightweight, composable query library for .NET that adds dynamic filtering and pagination on top of `IQueryable<T>`.

It builds on existing `IQueryable<T>` queries to provide these capabilities using familiar LINQ operators such as `Where(...)` and `OrderBy(...)`.

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
page.HasNext
page.HasPrevious
```

### Cursor Pagination

Cursor pagination (keyset pagination) is designed for stable and efficient traversal over ordered data, particularly for large or unbounded result sets.

It is enabled by calling `AsCursorQuery()` on the `IQueryable<T>`.

```csharp
var page = context.Products
    .AsCursorQuery()
    .OrderBy("Name ASC, Price DESC")
    .ToCursorPage(pageSize: 25);
```

> **Note**: Ordering for cursor pagination must be applied *after* calling `AsCursorQuery()`. Any ordering applied before `AsCursorQuery()` is ignored.

Cursor pagination requires at least one ordering clause to be provided; otherwise execution fails.

To continue navigation from a previously retrieved page, use the cursor values returned by that page.

```csharp
// Forward
query.After(page.NextCursor);

// Backward
query.Before(page.PreviousCursor);
```

#### Result shape

```csharp
page.Items
page.HasNext
page.HasPrevious
page.NextCursor      // opaque cursor for forward traversal
page.PreviousCursor  // opaque cursor for backward traversal
```