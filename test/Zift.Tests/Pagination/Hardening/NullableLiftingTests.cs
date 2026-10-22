using System.Linq.Expressions;
using Zift.Pagination.Cursor;
using Zift.Pagination.Cursor.Ordering;

namespace Zift.Pagination.Tests;

public class NullableLiftingTests
{
    private enum Mode { Off, On }

    private struct Pair
    {
        public int A { get; set; }
    }

    private sealed class Leaf
    {
        public int Value { get; set; }
        public int? Maybe { get; set; }
        public bool Flag { get; set; }
        public Mode Mode { get; set; }
        public Pair Pair { get; set; }
        public Leaf? Next { get; set; }
        public int FieldValue = 1;

        public int Compute() => Value * 2;
    }

    private sealed class Node
    {
        public int Id { get; set; }
        public int? Score { get; set; }
        public string? Text { get; set; }
        public Pair Pair { get; set; }
        public Leaf? Leaf { get; set; }
        public Leaf Required { get; set; } = new();
    }

    private static OrderingClause<Node> Clause<TKey>(Expression<Func<Node, TKey>> selector)
    {
        return new(selector, OrderingDirection.Ascending);
    }

    public static TheoryData<string, LambdaExpression, Type> LiftedSelectors => new()
    {
        { "value leaf", (Expression<Func<Node, int>>)(n => n.Leaf!.Value), typeof(int?) },
        { "two navigations", (Expression<Func<Node, int>>)(n => n.Leaf!.Next!.Value), typeof(int?) },
        { "bool leaf", (Expression<Func<Node, bool>>)(n => n.Leaf!.Flag), typeof(bool?) },
        { "enum leaf", (Expression<Func<Node, Mode>>)(n => n.Leaf!.Mode), typeof(Mode?) },
        { "struct member after navigation", (Expression<Func<Node, int>>)(n => n.Leaf!.Pair.A), typeof(int?) },
        { "string member", (Expression<Func<Node, int>>)(n => n.Text!.Length), typeof(int?) }
    };

    [Theory]
    [MemberData(nameof(LiftedSelectors))]
    public void PropertyChains_ThroughReferenceNavigations_AreLiftedToNullableKeys(
        string description, LambdaExpression selector, Type expectedKeyType)
    {
        var clause = new OrderingClause<Node>(selector, OrderingDirection.Ascending);

        Assert.Equal(expectedKeyType, clause.KeyType);
        var conversion = Assert.IsAssignableFrom<UnaryExpression>(clause.KeySelector.Body);
        Assert.Equal(ExpressionType.Convert, conversion.NodeType);
        Assert.Same(selector.Body, conversion.Operand);
        Assert.Same(selector.Parameters[0], clause.KeySelector.Parameters[0]);
        Assert.False(string.IsNullOrEmpty(description));
    }

    public static TheoryData<string, LambdaExpression> UnchangedSelectors => new()
    {
        { "root value property", (Expression<Func<Node, int>>)(n => n.Id) },
        { "root nullable property", (Expression<Func<Node, int?>>)(n => n.Score) },
        { "root struct member", (Expression<Func<Node, int>>)(n => n.Pair.A) },
        { "root string", (Expression<Func<Node, string?>>)(n => n.Text) },
        { "reference leaf", (Expression<Func<Node, Leaf?>>)(n => n.Leaf) },
        { "already nullable leaf", (Expression<Func<Node, int?>>)(n => n.Leaf!.Maybe) },
        { "explicit nullable cast", (Expression<Func<Node, int?>>)(n => (int?)n.Leaf!.Value) },
        { "explicit widening cast", (Expression<Func<Node, long>>)(n => (long)n.Leaf!.Value) },
        { "nullable Value access", (Expression<Func<Node, int>>)(n => n.Leaf!.Maybe!.Value) },
        { "nullable HasValue access", (Expression<Func<Node, bool>>)(n => n.Leaf!.Maybe.HasValue) },
        { "conditional", (Expression<Func<Node, int>>)(n => n.Leaf == null ? 0 : n.Leaf.Value) },
        { "coalesce", (Expression<Func<Node, int>>)(n => n.Leaf!.Maybe ?? 0) },
        { "arithmetic", (Expression<Func<Node, int>>)(n => n.Leaf!.Value + 1) },
        { "method call", (Expression<Func<Node, int>>)(n => n.Leaf!.Compute()) },
        { "field leaf", (Expression<Func<Node, int>>)(n => n.Leaf!.FieldValue) }
    };

    [Theory]
    [MemberData(nameof(UnchangedSelectors))]
    public void OtherSelectorShapes_AreLeftUnchanged(string description, LambdaExpression selector)
    {
        var clause = new OrderingClause<Node>(selector, OrderingDirection.Ascending);

        Assert.Same(selector, clause.KeySelector);
        Assert.Equal(selector.ReturnType, clause.KeyType);
        Assert.False(string.IsNullOrEmpty(description));
    }

    [Fact]
    public void NavigationDeclaredNonNullable_IsStillLifted()
    {
        var clause = Clause(n => n.Required.Value);

        Assert.Equal(typeof(int?), clause.KeyType);
        Assert.Equal(ExpressionType.Convert, clause.KeySelector.Body.NodeType);
    }

    [Fact]
    public void RootProperties_KeepTheirKeyTypes()
    {
        Assert.Equal(typeof(int), Clause(n => n.Id).KeyType);
        Assert.Equal(typeof(int?), Clause(n => n.Score).KeyType);
        Assert.Equal(typeof(int), Clause(n => n.Pair.A).KeyType);
    }

    [Fact]
    public void Reverse_KeepsTheLiftedSelector()
    {
        var clause = Clause(n => n.Leaf!.Value);

        var reversed = clause.Reverse();

        Assert.Same(clause.KeySelector, reversed.KeySelector);
        Assert.Equal(typeof(int?), reversed.KeyType);
    }

    [Theory]
    [InlineData("Leaf.Value")]
    [InlineData("Leaf.Next.Value")]
    [InlineData("Leaf.Flag")]
    [InlineData("Leaf.Mode")]
    [InlineData("Text.Length")]
    public void LiftedTypedSelectors_HaveTheSameFingerprintShape_AsParsedPaths(string path)
    {
        var typed = path switch
        {
            "Leaf.Value" => Clause(n => n.Leaf!.Value),
            "Leaf.Next.Value" => Clause(n => n.Leaf!.Next!.Value),
            "Leaf.Flag" => Clause(n => n.Leaf!.Flag),
            "Leaf.Mode" => Clause(n => n.Leaf!.Mode),
            _ => Clause(n => n.Text!.Length)
        };

        var parsed = OrderingParser.Parse<Node>(path, new OrderingOptions())[0];

        Assert.Equal(parsed.KeyType, typed.KeyType);
        Assert.Equal(parsed.Describe(), typed.Describe());
    }

    [Fact]
    public void ExplicitNullableSelector_HasTheSameFingerprintShape_AsLiftedSelector()
    {
        Assert.Equal(
            Clause(n => (int?)n.Leaf!.Value).Describe(),
            Clause(n => n.Leaf!.Value).Describe());
    }

    [Fact]
    public void GuardedParsedPath_StillLiftsBeforeGuarding()
    {
        var clause = OrderingParser.Parse<Node>("Leaf.Value", new OrderingOptions { EnableNullGuards = true })[0];

        Assert.Equal(typeof(int?), clause.KeyType);
        var conditional = Assert.IsAssignableFrom<ConditionalExpression>(clause.KeySelector.Body);
        Assert.Equal(typeof(int?), conditional.IfTrue.Type);
        Assert.Equal(ExpressionType.Convert, conditional.IfTrue.NodeType);
    }

    [Fact]
    public void LinqToObjects_LiftedSelector_StillRequiresCallerToGuardMissingNavigations()
    {
        var nodes = new[]
        {
            new Node { Id = 1, Leaf = new Leaf { Value = 5 } },
            new Node { Id = 2, Leaf = new Leaf { Value = -2 } }
        }.AsQueryable();

        var page = nodes.AsCursorQuery().OrderBy(n => n.Leaf!.Value).ThenBy(n => n.Id).ToCursorPage(10);
        Assert.Equal([2, 1], page.Items.Select(n => n.Id));

        var withMissing = new[] { new Node { Id = 3 }, new Node { Id = 4, Leaf = new Leaf() } }.AsQueryable();
        Assert.Throws<NullReferenceException>(
            () => withMissing.AsCursorQuery().OrderBy(n => n.Leaf!.Value).ThenBy(n => n.Id).ToCursorPage(10));

        var guarded = withMissing.AsCursorQuery()
            .OrderBy(n => n.Leaf == null ? (int?)null : n.Leaf.Value).ThenBy(n => n.Id).ToCursorPage(10);
        Assert.Equal([3, 4], guarded.Items.Select(n => n.Id));
    }
}
