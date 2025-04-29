<img src="docs/Zift_Logo.png" alt="Zift Logo" width="240px" />

# Zift

Zift is a lightweight and extensible library for query composition over `IQueryable` sources.  
It provides dynamic filtering, sorting, and pagination capabilities, while remaining flexible enough to support custom criteria implementations as needed.

Designed to work seamlessly with Entity Framework Core and any LINQ-compatible data source, Zift enables both runtime-defined querying (e.g., from API parameters) and compile-time query construction through fluent builders.

## 1. Features

- **Dynamic Filtering** — Parse string-based filter expressions into safe LINQ queries.
- **Predicate-Based Filtering** — Define custom filter criteria using expressions.
- **Fluent Sorting** — Compose multi-level sorts dynamically or fluently in code.
- **Dynamic Sorting** — Parse string-based sort clauses like `"Name desc, Price asc"`.
- **Pagination** — Apply paging over queries and return paginated result sets with metadata.
- **Fluent Criteria Builders** — Easily configure filtering, sorting, and pagination.
- **Seamless IQueryable Extensions** — Integrate filtering, sorting, and pagination directly over any `IQueryable<T>`.
- **Entity Framework Core Support** — Async pagination extensions with cancellation support.
- **Extensible Design** — Implement custom filter, sort, and pagination criteria types when needed.

## 2. Requirements

- **Target Framework:** .NET 8
- **Dependencies:**
    - Zift has no external dependencies.
    - Zift.EntityFrameworkCore depends on Entity Framework Core 8.x.

## 3. Getting Started

*Note:* Zift is not yet published as a NuGet package.

If you want to try it today, you can:

- Clone the repository locally
- Reference the project(s) directly from your solution

Package publishing will be available once the project reaches a stable release.

### 3.1. Basic Example

```csharp
var products = await dbContext.Products
    .Filter(new DynamicFilterCriteria<Product>("Price > 1000 && Manufacturer == 'Logitech'"))
    .SortBy(sort => sort.Descending(p => p.Price))
    .ToPaginatedListAsync(pagination => pagination.AtPage(1).WithSize(20));
```

- **Filter** products where price is greater than 1000 and manufacturer is "Logitech".
- **Sort** descending by price.
- **Paginate** to return 20 items starting from page 1.

## 4. Architecture Overview

At the core of Zift is the `ICriteria<T>` interface, which defines a simple contract for applying transformations over `IQueryable<T>` sources:

```csharp
public interface ICriteria<T>
{
    IQueryable<T> ApplyTo(IQueryable<T> query);
}
```

All Zift functionality — filtering, sorting, and pagination — builds upon this common foundation.

### 4.1. Specialized Criteria Interfaces

| Interface | Purpose |
| :--- | :--- |
| `IFilterCriteria<T>` | Defines a filtering operation (e.g., applying `.Where()`). |
| `ISortCriteria<T>` | Defines a sorting operation (e.g., applying `.OrderBy()` / `.ThenBy()`). |
| `IPaginationCriteria<T>` | Defines a pagination operation (e.g., applying `.Skip()` / `.Take()`). |

Each criteria type offers default implementations, fluent builders, and dynamic variants for runtime-defined queries.

### 4.2. Query Composition

Zift provides a set of IQueryable extensions (`Filter`, `SortBy`, `ToPaginatedList`) that allow queries to be composed fluently for filtering, sorting, and pagination:

```csharp
var query = dbContext.Products
    .Filter(new DynamicFilterCriteria<Product>("Rating >= 4"))
    .SortBy(sort => sort.Ascending(p => p.Name))
    .ToPaginatedList(pagination => pagination.WithSize(25));
```

Queries are composed using standard LINQ patterns and executed only when enumerated.

### 4.3. Design Principles

- Unified `ICriteria<T>` abstraction.
- Deferred query execution.
- Null-safe API validation.
- Lightweight, extensible core.
- Architecture-agnostic — no repository or unit-of-work assumptions.

## 5. Filtering

### 5.1. Overview

Filtering in Zift is based on the `IFilterCriteria<T>` interface.
An `IFilterCriteria<T>` applies a filtering operation over an `IQueryable<T>`, typically by adding a `.Where()` clause.

You can filter using:

- A custom criteria object
- A LINQ predicate
- A dynamic string expression

### 5.2. Predicate-Based Filtering

Use `PredicateFilterCriteria<T>` (or a direct predicate) to apply LINQ-style filters:

```csharp
var expensiveProducts = dbContext.Products
    .Filter(new PredicateFilterCriteria<Product>(p => p.Price > 1000));

// Or directly with a lambda expression
var expensiveProducts = dbContext.Products
    .Filter(p => p.Price > 1000);
```

#### 5.2.1. Custom Filter Criteria

You can implement `IFilterCriteria<T>` to create reusable, strongly-typed filters:

```csharp
public class PreferredCustomerFilter : IFilterCriteria<User>
{
    public IQueryable<User> ApplyTo(IQueryable<User> query)
    {
        var preferredSince = DateTime.UtcNow.AddYears(-2); // Registered at least 2 years ago

        return query.Where(u => u.RegistrationDate <= preferredSince);
    }
}
```

Usage:

```csharp
var preferredCustomers = dbContext.Users
    .Filter(new PreferredCustomerFilter());
```

### 5.3. Dynamic String-Based Filtering

Use `DynamicFilterCriteria<T>` (or a plain string) to apply dynamic filters:

```csharp
var filteredCategories = dbContext.Categories
    .Filter(new DynamicFilterCriteria<Category>("Name ^= 'Gaming' && Products:count > 0"));

// Or directly with a string expression
var filteredCategories = dbContext.Categories
    .Filter("Name ^= 'Gaming' && Products:count > 0");
```

### 5.4. Expression Syntax Overview

Supports:

- Scalar comparisons (`==`, `>`, `<`, etc.)
- Nested properties (e.g., `Products.Manufacturer`)
- Collection quantifiers (`:any`, `:all`) and projections (`:count`)
- Logical operators (`&&`, `||`, `!()`)

Example:

```csharp
// Find products where (price > 1000 OR name contains "Pro") AND at least one review has rating >= 4
var products = dbContext.Products
    .Filter(new DynamicFilterCriteria<Product>(
        "(Price > 1000 || Name %= 'Pro') && Reviews.Rating >= 4"));
```

For a more comprehensive reference on dynamic filtering expressions, see the [Dynamic Filtering Documentation](docs/Filtering.Dynamic.md).

## 6. Sorting

### 6.1. Overview

Sorting in Zift is based on the `ISortCriteria<T>` interface, supporting multiple sort operations applied in order.

### 6.2. Fluent Sorting

Sort using the fluent `SortCriteriaBuilder<T>`:

#### 6.2.1. Expression-Based

```csharp
var sortedProducts = dbContext.Products
    .SortBy(sort => sort
        .Descending(p => p.Price)
        .Ascending(p => p.Name));
```

#### 6.2.2. String-Based

```csharp
var sortedProducts = dbContext.Products
    .SortBy(sort => sort
        .Ascending("Name")
        .Descending("Price"));
```

### 6.3. Manual Sort Criteria

```csharp
var sortCriteria = new SortCriteria<Product>();

sortCriteria.Add(new SortCriterion<Product, decimal>(p => p.Price, SortDirection.Descending));
sortCriteria.Add(new SortCriterion<Product>("Name", SortDirection.Ascending));

var sortedProducts = dbContext.Products
    .SortBy(sortCriteria);
```

### 6.4. Dynamic String-Based Sorting

Sort using a SQL-style ORDER BY string:

```csharp
var sortedProducts = dbContext.Products
    .SortBy(sort => sort.Clause("Price DESC, Name ASC"));
```

> Default sort direction is ascending if omitted.

You can also specify a custom `ISortDirectiveParser<T>` implementation if you need different parsing behavior:

```csharp
var parser = new CustomSortDirectiveParser<Product>();

var sortedProducts = dbContext.Products
    .SortBy(sort => sort.Clause("Price DESC, Name ASC", parser));
```

## 7. Pagination

### 7.1. Overview

Pagination in Zift is based on `IPaginationCriteria<T>`, applying `.Skip()` and `.Take()` operations.

### 7.2. Fluent Pagination

```csharp
var paginatedProducts = await dbContext.Products
    .ToPaginatedListAsync(pagination => pagination
        .AtPage(1)
        .WithSize(20));
```

Or manually:

```csharp
var paginationCriteria = new PaginationCriteria<Product> { PageNumber = 1, PageSize = 20 };

var paginatedProducts = await dbContext.Products
    .ToPaginatedListAsync(paginationCriteria);
```

### 7.3. Paginated List Result

Paginated results implement `IPaginatedList<T>`, exposing:

- `PageNumber`
- `PageSize`
- `PageCount`
- `TotalCount`

Example:

```csharp
if (paginatedProducts.HasNextPage())
{
    // Load next page...
}
```

## 8. IQueryable Extensions

Zift extends `IQueryable<T>` with:

| Method | Purpose |
|:---|:---|
| `Filter(IFilterCriteria<T>)` | Apply filtering using a criteria object |
| `Filter(Expression<Func<T, bool>>)` | Apply filtering using a predicate |
| `Filter(string)` | Apply filtering using a dynamic expression |
| `SortBy(ISortCriteria<T>)` | Apply sorting using a criteria object |
| `SortBy(Action<SortCriteriaBuilder<T>>)` | Apply sorting using a fluent builder |
| `ToPaginatedList(IPaginationCriteria<T>)` | Apply pagination using a criteria object |
| `ToPaginatedList(Action<PaginationCriteriaBuilder<T>>)` | Apply pagination using a fluent builder |
| `ToPaginatedListAsync(IPaginationCriteria<T>)` | Apply pagination using a criteria object (EF Core) |
| `ToPaginatedListAsync(Action<PaginationCriteriaBuilder<T>>)` | Apply pagination using a fluent builder (EF Core) |

## 9. Advanced Topics

### 9.1. Extending Zift

Create custom filtering, sorting, or pagination criteria by implementing the respective interfaces.

Example custom filter:

```csharp
public class ActiveReviewFilter : IFilterCriteria<Review>
{
    public IQueryable<Review> ApplyTo(IQueryable<Review> query)
    {
        return query.Where(r => r.DatePosted != null);
    }
}
```

## 10. Roadmap

Zift is under active development and not yet considered stable.

Potential future enhancements:

- **Extensibility for Dynamic Filtering:** Improve parser extensibility.
- **Case-Insensitive Filtering:** Explore case-insensitivity options (string comparisons currently respect database collation settings.)

## 11. License

Zift is licensed under the [MIT License](https://opensource.org/licenses/MIT).
