using Zift.Filtering.ExpressionBuilding;

namespace Zift.Filtering.E2E;

public sealed class NullPropagationTests
{
    private sealed class Node
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? Rank { get; set; }
        public int Score { get; set; }
        public Node? Child { get; set; }
        public List<string?>? Tags { get; set; }
        public List<Node?>? Children { get; set; }
    }

    private static readonly Node[] Nodes =
    [
        new() { Id = 1, Name = null, Rank = null, Score = 0, Child = null, Tags = null, Children = null },
        new()
        {
            Id = 2, Name = "a", Rank = 5, Score = 7, Tags = [], Children = [],
            Child = new() { Id = 20, Name = "bc", Rank = 8, Score = 9, Tags = ["x"] }
        },
        new()
        {
            Id = 3, Name = "b", Rank = null, Score = 3, Tags = ["x", "y"],
            Child = new() { Id = 30, Name = null, Rank = null, Score = 0, Tags = null },
            Children = [null, new() { Id = 40, Name = "k", Score = 11 }]
        },
        new()
        {
            Id = 4, Name = "bc", Rank = 8, Score = 9, Tags = [null, "x"],
            Child = new() { Id = 50, Name = "bc", Score = 9, Child = new() { Id = 60, Name = "z", Score = 1 } },
            Children = [new() { Id = 70, Name = null, Score = 2 }]
        }
    ];

    private static readonly Dictionary<string, Func<Node, bool>> Oracles = new()
    {
        ["Name != 'a'"] = n => n.Name != "a",
        ["Name == 'a'"] = n => n.Name == "a",
        ["Name == null"] = n => n.Name == null,
        ["Name != null"] = n => n.Name != null,
        ["Name %= 'b'"] = n => n.Name != null && n.Name.Contains('b'),
        ["Name ^= 'a'"] = n => n.Name != null && n.Name.StartsWith('a'),
        ["Name $= 'c'"] = n => n.Name != null && n.Name.EndsWith('c'),
        ["Name > 'a'"] = n => n.Name != null && n.Name.CompareTo("a") > 0,
        ["Name <= 'b'"] = n => n.Name != null && n.Name.CompareTo("b") <= 0,

        ["Rank == 5"] = n => n.Rank == 5,
        ["Rank != 5"] = n => n.Rank != 5,
        ["Rank > 5"] = n => n.Rank > 5,
        ["Rank == null"] = n => n.Rank == null,

        ["Child.Name == 'bc'"] = n => n.Child?.Name == "bc",
        ["Child.Name != 'bc'"] = n => n.Child?.Name != "bc",
        ["Child.Name == null"] = n => n.Child?.Name == null,
        ["Child.Name != null"] = n => n.Child?.Name != null,
        ["Child.Name %= 'b'"] = n => n.Child?.Name?.Contains('b') == true,
        ["Child.Name > 'a'"] = n => n.Child?.Name != null && n.Child.Name.CompareTo("a") > 0,
        ["!(Child.Name == 'bc')"] = n => !(n.Child?.Name == "bc"),
        ["Child.Child.Name == 'z'"] = n => n.Child?.Child?.Name == "z",
        ["Child.Child.Name != 'z'"] = n => n.Child?.Child?.Name != "z",

        ["Child.Score == 9"] = n => n.Child?.Score == 9,
        ["Child.Score != 9"] = n => n.Child?.Score != 9,
        ["Child.Score > 8"] = n => n.Child?.Score > 8,
        ["Child.Score <= 0"] = n => n.Child?.Score <= 0,
        ["Child.Score == null"] = n => n.Child?.Score == null,
        ["Child.Score != null"] = n => n.Child?.Score != null,

        ["Child.Rank in [8]"] = n => n.Child?.Rank == 8,
        ["Child.Rank in [8, null]"] = n => n.Child?.Rank is null or 8,
        ["Child.Rank in [null]"] = n => n.Child?.Rank is null,
        ["Child.Rank in []"] = _ => false,
        ["Child.Name in ['bc', 'z']"] = n => n.Child?.Name is "bc" or "z",
        ["Child.Name in ['bc', null]"] = n => n.Child?.Name is "bc" or null,

        ["Tags:count == 0"] = n => (n.Tags?.Count ?? 0) == 0,
        ["Tags:count != 2"] = n => (n.Tags?.Count ?? 0) != 2,
        ["Tags:count > 0"] = n => (n.Tags?.Count ?? 0) > 0,
        ["Tags:any()"] = n => n.Tags != null && n.Tags.Any(),
        ["Tags:any(Length == 1)"] = n => n.Tags != null && n.Tags.Any(t => t?.Length == 1),
        ["Tags:all(Length == 1)"] = n => n.Tags == null || n.Tags.All(t => t?.Length == 1),
        ["!Tags:any()"] = n => !(n.Tags != null && n.Tags.Any()),

        ["Child.Tags:any()"] = n => n.Child?.Tags != null && n.Child.Tags.Any(),
        ["Child.Tags:all(Length == 1)"] = n => n.Child?.Tags == null || n.Child.Tags.All(t => t?.Length == 1),
        ["Child.Tags:count == 0"] = n => (n.Child?.Tags?.Count ?? 0) == 0,

        ["Children:any(Name == 'k')"] = n => n.Children != null && n.Children.Any(c => c?.Name == "k"),
        ["Children:any(Name == null)"] = n => n.Children != null && n.Children.Any(c => c?.Name == null),
        ["Children:all(Score > 5)"] = n => n.Children == null || n.Children.All(c => c?.Score > 5),
        ["Children:any(Child.Name == 'x')"] = n => n.Children != null && n.Children.Any(c => c?.Child?.Name == "x"),
        ["Children:count == 2"] = n => (n.Children?.Count ?? 0) == 2
    };

    public static IEnumerable<object[]> Filters =>
        Oracles.Keys.Select(filter => new object[] { filter });

    [Theory]
    [MemberData(nameof(Filters))]
    public void Where_WithNullGuards_FollowsCSharpNullConditionalSemantics(string filter)
    {
        var options = new ExpressionBuilderOptions { EnableNullGuards = true };

        var actual = Nodes.AsQueryable().Where(filter, options).Select(n => n.Id).ToArray();
        var expected = Nodes.Where(Oracles[filter]).Select(n => n.Id).ToArray();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Filters))]
    public void Where_WithNullGuards_AndConstantValues_GivesSameResult(string filter)
    {
        var constants = new ExpressionBuilderOptions { EnableNullGuards = true, ParameterizeValues = false };

        var actual = Nodes.AsQueryable().Where(filter, constants).Select(n => n.Id).ToArray();
        var expected = Nodes.Where(Oracles[filter]).Select(n => n.Id).ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Where_WithoutNullGuards_StillThrowsOnNullNavigation()
    {
        var options = new ExpressionBuilderOptions { EnableNullGuards = false };

        Assert.Throws<NullReferenceException>(() =>
            Nodes.AsQueryable().Where("Child.Name == 'bc'", options).ToArray());
    }

    [Theory]
    [InlineData("Name %= 'b'")]
    [InlineData("Name ^= 'a'")]
    [InlineData("Name $= 'c'")]
    [InlineData("Name > 'a'")]
    [InlineData("Name <= 'b'")]
    public void Where_WithoutNullGuards_StringOperatorsStillThrowOnNullValue(string filter)
    {
        var options = new ExpressionBuilderOptions { EnableNullGuards = false };

        Assert.Throws<NullReferenceException>(() =>
            Nodes.AsQueryable().Where(filter, options).ToArray());
    }

    [Theory]
    [InlineData("Name %= 'b'")]
    [InlineData("Name ^= 'a'")]
    [InlineData("Name $= 'c'")]
    [InlineData("Name > 'a'")]
    [InlineData("Name <= 'b'")]
    public void Where_WithNullGuards_StringOperatorsDoNotMatchNullValue(string filter)
    {
        var options = new ExpressionBuilderOptions { EnableNullGuards = true };

        var ids = Nodes.AsQueryable().Where(filter, options).Select(n => n.Id).ToArray();

        Assert.DoesNotContain(1, ids);
    }

    [Theory]
    [InlineData("Name != 'a'")]
    [InlineData("Rank != 5")]
    [InlineData("Name == null")]
    [InlineData("Rank in [5, null]")]
    public void Where_OnRootProperties_GivesSameResultWithAndWithoutNullGuards(string filter)
    {
        var on = new ExpressionBuilderOptions { EnableNullGuards = true };
        var off = new ExpressionBuilderOptions { EnableNullGuards = false };

        var withGuards = Nodes.AsQueryable().Where(filter, on).Select(n => n.Id).ToArray();
        var withoutGuards = Nodes.AsQueryable().Where(filter, off).Select(n => n.Id).ToArray();

        Assert.Equal(withoutGuards, withGuards);
    }
}
