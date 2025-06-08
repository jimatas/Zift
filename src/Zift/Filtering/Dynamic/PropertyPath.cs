namespace Zift.Filtering.Dynamic;

/// <summary>
/// Represents a navigable path to a property, including optional quantifiers or projections on collection segments.
/// </summary>
public class PropertyPath
{
    private readonly IReadOnlyList<PropertyPathSegment> _segments;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyPath"/> class from a sequence of segments.
    /// </summary>
    /// <param name="segments">The property path segments to compose the path.</param>
    /// <exception cref="ArgumentException">Thrown if the segment sequence is <see langword="null"/>, empty,
    /// or contains invalid segment combinations.</exception>
    public PropertyPath(IEnumerable<PropertyPathSegment> segments)
    {
        _segments = segments.ThrowIfNullOrEmpty().ToArray();
        ValidateSegments(nameof(segments));
    }

    /// <summary>
    /// The number of segments in the property path.
    /// </summary>
    public int Count => _segments.Count;

    /// <summary>
    /// The segment at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the segment.</param>
    /// <returns>The corresponding <see cref="PropertyPathSegment"/>.</returns>
    public PropertyPathSegment this[int index] => _segments[index];

    /// <summary>
    /// Returns the string representation of the property path, optionally including modifiers.
    /// </summary>
    /// <param name="includeModifiers">Whether to include quantifier or projection modifiers.</param>
    /// <returns>The formatted property path string.</returns>
    public string ToString(bool includeModifiers)
    {
        return string.Join('.', _segments.Select(segment => segment.ToString(includeModifiers)));
    }

    /// <inheritdoc/>
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
