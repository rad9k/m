using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroUML.Instructions;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class ZeroCodeStackIntegrationTests
{
    public ZeroCodeStackIntegrationTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void AddedFrameSupportsParentLookupLocalShadowingAndReturn()
    {
        var execution = new ZeroCodeExecution();
        var originalFrame = execution.Stack;
        var localMeta = CreateTempVertex("Local");
        var parentValue = CreateTempVertex("ParentValue");
        var childValue = CreateTempVertex("ChildValue");
        originalFrame.AddEdge(localMeta, parentValue);

        execution.AddStackFrame();
        var childFrame = execution.Stack;
        Assert.Same(
            parentValue,
            Assert.Single(GraphUtil.GetQueryOut(childFrame, "Local", null)).To);

        childFrame.AddEdge(localMeta, childValue);
        Assert.Same(
            childValue,
            Assert.Single(GraphUtil.GetQueryOut(childFrame, "Local", null)).To);

        execution.RemoveStackFrame();

        Assert.Same(originalFrame, execution.Stack);
        Assert.Same(
            parentValue,
            Assert.Single(GraphUtil.GetQueryOut(execution.Stack, "Local", null)).To);
    }

    [Fact]
    public void ReturnedStackRemainsUsableAfterFactoryScopeEnds()
    {
        var stack = CreateReturnedStack();
        var meta = CreateTempVertex("Returned");
        var target = CreateTempVertex("StillUsable");

        stack.AddEdge(meta, target);

        Assert.Same(
            target,
            Assert.Single(GraphUtil.GetQueryOut(stack, "Returned", null)).To);
    }

    [Fact]
    public void FrameLifecycleDoesNotGrowTempStoreRegistry()
    {
        const int frameCount = 100;
        var tempStore = (m0.Store.StoreBase)
            MinusZero.Instance.TempStore;
        var initialStoreVertexCount =
            tempStore.VertexIdentifiersDictionary.Count;
        var execution = new ZeroCodeExecution();
        var rootFrame = execution.Stack;

        for (var index = 0; index < frameCount; index++)
            execution.AddStackFrame();

        for (var index = 0; index < frameCount; index++)
            execution.RemoveStackFrame();

        Assert.Same(rootFrame, execution.Stack);
        Assert.Equal(
            initialStoreVertexCount,
            tempStore.VertexIdentifiersDictionary.Count);
    }

    [Fact]
    public void LinkWithoutTargetCreatesOnlyReturnedStack()
    {
        var execution = new ZeroCodeExecution();
        var instructionVertex = CreateTempVertex("Link");

        GraphPerformanceCounters.Reset();
        GraphPerformanceCounters.Enabled = true;

        try
        {
            var result = BaseInstructions.Link(
                execution,
                execution.Stack,
                instructionVertex,
                out var isStackFrameReturn);
            var snapshot =
                GraphPerformanceCounters.GetSnapshot();

            Assert.NotNull(result);
            Assert.False(isStackFrameReturn);
            Assert.Equal(1, snapshot.CreatedStacks);
            Assert.Equal(
                1,
                snapshot.CreatedTempStoreStacks);
        }
        finally
        {
            GraphPerformanceCounters.Enabled = false;
        }
    }

    private static INoInEdgeInOutVertexVertex CreateReturnedStack()
    {
        return InstructionHelpers.CreateStack();
    }

    private static IVertex CreateTempVertex(object value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(MinusZero.Instance.Empty, value);
    }
}
