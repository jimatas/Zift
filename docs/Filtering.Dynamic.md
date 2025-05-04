# Dynamic String-Based Filtering

Zift provides dynamic filtering via string-based filter expressions using the `DynamicFilterCriteria<T>` class.

This allows filters to be defined at runtime — for example, based on API query parameters, user selections, or configuration files — and translated into type-safe LINQ queries.

The primary use case is external configuration of filtering conditions, such as via API query parameters or admin tools.

## 1. Overview

Filter expressions are parsed into a logical model (`FilterTerm`, `FilterCondition`, `FilterGroup`), then translated into expression trees using standard LINQ operators.

This allows validated, type-safe querying over nested object graphs, including collections.

## 2. Usage

To apply a dynamic filter:

```csharp
var expression = "Name ^= 'Gaming' && Products:count > 0";
var criteria = new DynamicFilterCriteria<Category>(expression);

var categories = await dbContext.Categories
    .Filter(criteria)
    .ToListAsync(cancellationToken);
```

## 3. Expression Examples

### 3.1. Scalar Filtering

```text
"Name == 'Electronics'"
"Price > 1000"
```

### 3.2. Nested Properties

```text
"Products.Manufacturer == 'Logitech'"
"Products.Reviews.Author.Name == 'Alice'"
```

### 3.3. Collection Properties

```text
// Projected count of related items
"Products:count > 5"
"Products.Reviews:count >= 10"

// Any product with at least one review rated >= 4
"Products.Reviews.Rating >= 4"

// Any product where all reviews have rating >= 4
"Products.Reviews:all.Rating >= 4"
```

## 4. Supported Operators

| Operator | Description |
| :--- | :--- |
| `==` | Equal (supports `:i`) |
| `!=` | Not equal (supports `:i`) |
| `>` | Greater than |
| `<` | Less than |
| `>=` | Greater than or equal |
| `<=` | Less than or equal |
| `%=` | String contains (supports `:i`) |
| `^=` | String starts with (supports `:i`) |
| `$=` | String ends with (supports `:i`) |
| `&&` | Logical AND |
| `\|\|` | Logical OR |
| `!()` | Logical NOT |

Example:

```text
"!(Name == 'Obsolete' || IsArchived == true)"
```

*Note:* The `!` operator must precede a parenthesized expression. Standalone negations like `!IsArchived` are not supported.

## 5. Supported Literals

### 5.1. Strings

Single or double-quoted, allows escaping embedded quotes.

```text
"Name == 'Gaming Mouse'"
"Manufacturer == 'O\'Reilly'"
```

Complex types like `DateTime`, `TimeSpan`, and `Guid` can be expressed as strings using their standard .NET formats.
They will be converted automatically during query translation.

```text
"CreatedOn > '2024-02-13'"
"UserId == 'd9b25756-62d4-4c59-b1f5-9e1048385c63'"
```

You can apply modifiers to string literals. The `:i` modifier enables case-insensitive string comparisons:

```text
"Name == 'smartphone':i"        // matches "Smartphone"
"Name != 'REFRIGERATOR':i"      // excludes "Refrigerator"
"Name ^= 'lap':i"               // matches "Laptop"
"Name $= 'SHIRT':i"             // matches "T-Shirt"
"Name %= 'great':i"             // matches "The Great Gatsby"
```
> Only string literals support modifiers. Using a modifier on non-string values results in a syntax error.

### 5.2. Numbers

Supports integers, decimals, and scientific notation:

```text
"Rating > 4"
"Price <= 49.99"
"Discount > 1.5e2"
```

### 5.3. Keywords

The following keywords are supported (case-insensitive):

- `true`
- `false`
- `null`

Examples:

```text
"IsPublished == true"
"DeletedAt == null"
```

*Note:* Boolean comparisons must be explicit, as shown above. An expression like this will not work:

```text
"IsPublished"
```

## 6. Collections

### 6.1. Quantifiers

Use quantifiers to control how conditions apply to collection elements:

- `:any` — at least one element must match *(default if omitted)*
- `:all` — all elements must match

```text
"Products:any.Reviews:all.Rating >= 3"
```

The following two expressions are equivalent, since `:any` is the default:

```text
"Products.Reviews.Rating >= 4"
"Products:any.Reviews:any.Rating >= 4"
```

### 6.2. Projections

Use projections to operate on collection metadata (e.g., count):

```text
"Products.Reviews:count >= 5"
```

Projections must appear at the end of the path.

## 7. Expression Structure

Internally, expressions are parsed into:

- `FilterCondition`: a single property/operator/value check
- `FilterGroup`: a logical grouping of terms, which can be either filter conditions or nested groups.
- `PropertyPath`: supports nesting, quantifiers, and projections
- `DynamicFilterCriteria<T>`: compiles a parsed structure into an `Expression<Func<T, bool>>`
