using System.Reflection;
using m0;
using m0.Foundation;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes.UX;

namespace m0_desktop_tests;

[Collection(DesktopGraphCollection.Name)]
public sealed class UXVisualiserContractTests
{
    public UXVisualiserContractTests(
        DesktopBootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void HostedItemValueChangesDoNotRequireFullPaint()
    {
        IVertex hostedVertex = CreateRetainedVertex(
            "HostedValue");
        IVertex stack = InstructionHelpers.CreateStack();
        IVertex eventVertex = CreateRetainedVertex(
            "ValueChangeEvent");
        eventVertex.AddEdge(
            CreateRetainedVertex("Type"),
            CreateRetainedVertex("ValueChange"));
        eventVertex.AddEdge(
            CreateRetainedVertex("ChangedVertex"),
            hostedVertex);
        stack.AddEdge(
            CreateRetainedVertex("event"),
            eventVertex);
        var itemsByBaseEdgeTo =
            new Dictionary<IVertex, List<IUXItem>>
            {
                [hostedVertex] = new List<IUXItem>()
            };
        var itemsByVertex =
            new Dictionary<IVertex, IUXItem>();
        MethodInfo method = typeof(UXVisualiser)
            .GetMethod(
                "ContainsOnlyValueChangesForHostedItems",
                BindingFlags.Static |
                BindingFlags.NonPublic)!;

        Assert.True(
            (bool)method.Invoke(
                null,
                new object[]
                {
                    stack,
                    itemsByBaseEdgeTo,
                    itemsByVertex
                })!);

        itemsByBaseEdgeTo.Clear();

        Assert.False(
            (bool)method.Invoke(
                null,
                new object[]
                {
                    stack,
                    itemsByBaseEdgeTo,
                    itemsByVertex
                })!);
    }

    private static IVertex CreateRetainedVertex(
        string value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(
            MinusZero.Instance.Empty,
            value);
    }
}
