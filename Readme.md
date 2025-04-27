<img src="docs/Zift_Logo.png" alt="Zift Logo" width="240px" />

# Zift

Zift is a lightweight and extensible library for query composition over `IQueryable` sources.  
It provides dynamic filtering, sorting, and pagination capabilities, while remaining flexible enough to support custom criteria types tailored to your application's needs.

Designed to work seamlessly with Entity Framework Core and any LINQ-compatible data source, Zift enables both runtime-driven querying (e.g., from API parameters) and compile-time query construction through fluent builders.

---

## Features

- **Dynamic Filtering** — Parse string-based filter expressions into safe LINQ queries.
- **Predicate-Based Filtering** — Define custom filter criteria using expressions.
- **Fluent Sorting** — Compose multi-level sorts dynamically or fluently in code.
- **Dynamic Sorting** — Parse string-based sort clauses like `"Name desc, Price asc"`.
- **Pagination** — Apply paging over queries and return paginated result sets with metadata.
- **Fluent Criteria Builders** — Easily configure filtering, sorting, and pagination.
- **Seamless IQueryable Extensions** — Integrate filtering, sorting, and pagination directly over any `IQueryable<T>`.
- **Entity Framework Core Support** — Async pagination extensions with cancellation support.
- **Extensible Design** — Implement custom filter, sort, and pagination criteria types when needed.

---

## Getting Started

Install the main Zift package:

```bash
dotnet add package Zift
```

If you're using Entity Framework Core and want async pagination support, install the EF Core package (which includes Zift automatically):

```bash
dotnet add package Zift.EntityFrameworkCore
```

---

### Basic Example

```csharp
var products = await dbContext.Products
    .FilterBy(new DynamicFilterCriteria<Product>("Price > 1000 && Manufacturer == 'Logitech'"))
    .SortBy(sort => sort.Descending(p => p.Price))
    .ToPaginatedListAsync(pagination => pagination.StartAt(1).WithPageSize(20));
```

- **Filter** products where price is greater than 1000 and manufacturer is "Logitech".
- **Sort** descending by price.
- **Paginate** to return 20 items starting from page 1.

---

# Architecture Overview

At the core of Zift is the `ICriteria<T>` interface, which defines a simple contract for applying transformations over `IQueryable<T>` sources:

```csharp
public interface ICriteria<T>
{
    IQueryable<T> ApplyTo(IQueryable<T> query);
}
```

All Zift functionality — filtering, sorting, and pagination — builds upon this common foundation.

---

## Specialized Criteria Interfaces

| Interface | Purpose |
| :--- | :--- |
| `IFilterCriteria<T>` | Defines a filtering operation (e.g., applying `.Where()`). |
| `ISortCriteria<T>` | Defines a sorting operation (e.g., applying `.OrderBy()` / `.ThenBy()`). |
| `IPaginationCriteria<T>` | Defines a pagination operation (e.g., applying `.Skip()` / `.Take()`). |

Each criteria type offers default implementations, fluent builders, and dynamic variants for runtime-defined queries.

---

## Query Composition

Zift provides a set of IQueryable extensions (`FilterBy`, `SortBy`, `ToPaginatedList`) that allow queries to be composed fluently for filtering, sorting, and pagination:

```csharp
var query = dbContext.Products
    .FilterBy(new DynamicFilterCriteria<Product>("Rating >= 4"))
    .SortBy(sort => sort.Ascending(p => p.Name))
    .ToPaginatedList(pagination => pagination.WithPageSize(25));
```

Queries are composed naturally and are only executed when enumerated.

---

# Filtering

## Overview

Filtering in Zift is based on the `IFilterCriteria<T>` interface.
An `IFilterCriteria<T>` applies a filtering operation over an `IQueryable<T>`, typically by adding a `.Where()` clause.

---

## Predicate-Based Filtering

Use `PredicateFilterCriteria<T>` to filter using regular LINQ expressions:

```csharp
var expensiveProducts = dbContext.Products
    .FilterBy(new PredicateFilterCriteria<Product>(p => p.Price > 1000));
```

### Custom Filter Criteria

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
    .FilterBy(new PreferredCustomerFilter());
```

---

## Dynamic String-Based Filtering

Use `DynamicFilterCriteria<T>` for runtime-defined string expressions:

```csharp
var filteredCategories = dbContext.Categories
    .FilterBy(new DynamicFilterCriteria<Category>("Name ^= 'Gaming' && Products:count > 0"));
```

### Expression Syntax Overview

Supports:

- Scalar comparisons (`==`, `>`, `<`, etc.)
- Nested properties (e.g., `Products.Manufacturer`)
- Collection quantifiers (`:any`, `:all`) and projections (`:count`)
- Logical operators (`&&`, `||`, `!()`)

Example:

```csharp
// Find products where (price > 1000 OR name contains "Pro") AND at least one review has rating >= 4
var products = dbContext.Products
    .FilterBy(new DynamicFilterCriteria<Product>(
        "(Price > 1000 || Name %= 'Pro') && Reviews.Rating >= 4"));
```

For a more comprehensive reference on dynamic filtering expressions, see the [Dynamic Filtering Documentation](docs/Filtering.Dynamic.md).

---

# Sorting

## Overview

Sorting in Zift is based on the `ISortCriteria<T>` interface, supporting multiple sort operations applied in order.

---

## Fluent Sorting

Sort using the fluent `SortCriteriaBuilder<T>`:

### Expression-Based

```csharp
var sortedProducts = dbContext.Products
    .SortBy(sort => sort
        .Descending(p => p.Price)
        .Ascending(p => p.Name));
```

### String-Based

```csharp
var sortedProducts = dbContext.Products
    .SortBy(sort => sort
        .Ascending("Name")
        .Descending("Price"));
```

---

## Manual Sort Criteria

```csharp
var sortCriteria = new SortCriteria<Product>();

sortCriteria.Add(new SortCriterion<Product, decimal>(p => p.Price, SortDirection.Descending));
sortCriteria.Add(new SortCriterion<Product>("Name", SortDirection.Ascending));

var sortedProducts = dbContext.Products
    .SortBy(sortCriteria);
```

---

## Dynamic String-Based Sorting

Sort using a raw string directive:

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

---

# Pagination

## Overview

Pagination in Zift is based on `IPaginationCriteria<T>`, applying `.Skip()` and `.Take()` operations.

---

## Fluent Pagination

```csharp
var paginatedProducts = await dbContext.Products
    .ToPaginatedListAsync(pagination => pagination
        .StartAt(1)
        .WithPageSize(20));
```

Or manually:

```csharp
var paginationCriteria = new PaginationCriteria<Product> { PageNumber = 1, PageSize = 20 };

var paginatedProducts = await dbContext.Products
    .ToPaginatedListAsync(paginationCriteria);
```

## Paginated List Result

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

---

# IQueryable Extensions

Zift extends `IQueryable<T>` with:

| Method | Purpose |
|:---|:---|
| `FilterBy(IFilterCriteria<T>)` | Apply filtering |
| `SortBy(ISortCriteria<T>)` | Apply sorting |
| `SortBy(Action<SortCriteriaBuilder<T>>)` | Fluent sorting |
| `ToPaginatedList(IPaginationCriteria<T>)` | Apply pagination (sync) |
| `ToPaginatedListAsync(IPaginationCriteria<T>)` | Apply pagination (async, EF Core) |

---

# Advanced Topics

## Extending Zift

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

## Design Notes

- Unified `ICriteria<T>` abstraction.
- Deferred query execution.
- Null-safe API validation.
- Lightweight, extensible core.
- Architecture-agnostic — no repository or unit-of-work assumptions.

---

# Roadmap

Zift is under active development and not yet considered stable.

Potential future enhancements:

- **Extensibility for Dynamic Filtering:** Improve parser extensibility.
- **Case-Insensitive Filtering:** Explore case-insensitivity options (string comparisons currently respect database collation settings.)

---

# License

Zift is licensed under the [MIT License](https://opensource.org/licenses/MIT).