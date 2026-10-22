namespace Zift.Pagination.Cursor.Execution;

internal sealed record KeyedItem<T>(T Item, object?[] Keys);
