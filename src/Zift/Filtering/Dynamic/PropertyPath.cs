namespace Zift.Filtering.Dynamic;

public class PropertyPath
{
    private readonly IReadOnlyList<PropertyPathSegment> _segments;

    public PropertyPath(IEnumerable<PropertyPathSegment> segments)
    {
        _segments = segments.ThrowIfNullOrEmpty().ToArray();
        ValidateSegments(nameof(segments));
    }

    public int Count => _segments.Count;
    public PropertyPathSegment this[int index] => _segments[index];

    public string ToString(bool includeModifiers)
    {
        return string.Join('.', _segments.Select(segment => segment.ToString(includeModifiers)));
    }
    
    public override string ToString() => ToString(includeModifiers: false);

    private void ValidateSegments(string paramName)
    {
        for (var i = 0; i < _segments.Count; i++)
        {
            try
            {
                _segments[i].Validate(isLastSegment: i == _segments.Count - 1);
            }
            catch (InvalidOperationException exception)
            {
                throw new ArgumentException(exception.Message, paramName);
            }
        }
    }
}
