using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroUML.Instructions;
using m0.ZeroTypes;

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

        var result = BaseInstructions.Link(
            execution,
            execution.Stack,
            instructionVertex,
            out var isStackFrameReturn);

        Assert.NotNull(result);
        Assert.False(isStackFrameReturn);
    }

    [Fact]
    public void ParsedFunctionCallPreservesNestedFramesAndReturnedValue()
    {
        const string code =
            "function \"choose\" @String()\r\n" +
            "\treturn \"selected\"";
        var codeVertex =
            CreateTempVertex("StackFunctionConformance");
        var codeParent =
            CreateTempVertex("StackFunctionConformanceParent");
        var sourceEdge =
            codeParent.AddEdge(
                MinusZero.Instance.Empty,
                codeVertex);
        var parseErrors =
            MinusZero.Instance.DefaultFormalTextParser.Parse(
                sourceEdge,
                code,
                CodeRepresentationEnum.LinearizedManyLines,
                out var parsedRootEdge);

        Assert.True(
            parseErrors == null ||
            !parseErrors.Any(),
            "The ZeroCode function conformance program " +
            "must parse without errors.");

        var execution = new ZeroCodeExecution();
        var originalFrame = execution.Stack;
        var callInstruction =
            CreateTempVertex("FunctionCallInstruction");
        callInstruction.AddEdge(
            CreateTempVertex("Target"),
            parsedRootEdge.To);
        var returnedStack =
            BaseInstructions.FunctionCall(
                execution,
                execution.Stack,
                callInstruction,
                out var isStackFrameReturn);

        Assert.NotNull(returnedStack);
        Assert.False(isStackFrameReturn);
        Assert.Same(originalFrame, execution.Stack);
        Assert.True(
            returnedStack.OutEdgesRaw.Any(
                edge => Equals(
                    edge.To?.Value,
                    "selected")),
            MinusZero.Instance.DefaultFormalTextGenerator.Generate(
                parsedRootEdge,
                CodeRepresentationEnum.EdgeAndManyLines));
    }

    [Fact]
    public void SequentialNextAtomsHandleDepthTenThousand()
    {
        const int depth = 10_000;
        var nextAtomMeta =
            MinusZero.Instance.Root.Get(
                false,
                @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");
        var root =
            CreateTempVertex(
                "ExecutionRoot");
        IVertex current = root;

        for (var index = 0;
            index < depth;
            index++)
        {
            var next =
                CreateTempVertex(index);
            current.AddEdge(
                nextAtomMeta,
                next);
            current = next;
        }

        var execution =
            new RecordingExecution();
        var result =
            ZeroCodeExecutonUtil
                .SequentiallyExecuteInstructions_NextEdges(
                    execution,
                    execution.Stack,
                    root,
                    out var isStackFrameReturn);

        Assert.False(isStackFrameReturn);
        Assert.Same(
            execution.Stack,
            result);
        Assert.Equal(
            depth,
            execution.ExecutedValues.Count);
        Assert.Equal(
            0,
            execution.ExecutedValues[0]);
        Assert.Equal(
            depth - 1,
            execution.ExecutedValues[^1]);
    }

    [Fact]
    public void SequentialNextAtomsPreserveDfsOrderAndTerminateCycle()
    {
        var nextAtomMeta =
            MinusZero.Instance.Root.Get(
                false,
                @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");
        var root = CreateTempVertex("Root");
        var first = CreateTempVertex("First");
        var firstChild =
            CreateTempVertex("FirstChild");
        var second = CreateTempVertex("Second");
        root.AddEdge(nextAtomMeta, first);
        root.AddEdge(nextAtomMeta, second);
        first.AddEdge(
            nextAtomMeta,
            firstChild);
        firstChild.AddEdge(
            nextAtomMeta,
            root);
        var execution =
            new RecordingExecution();

        _ = ZeroCodeExecutonUtil
            .SequentiallyExecuteInstructions_NextEdges(
                execution,
                execution.Stack,
                root,
                out var isStackFrameReturn);

        Assert.False(isStackFrameReturn);
        Assert.Equal(
            new object[]
            {
                "First",
                "FirstChild",
                "Root",
                "Second"
            },
            execution.ExecutedValues);
    }

    [Fact]
    public void SequentialNextAtomsPreserveEarlyReturn()
    {
        var nextAtomMeta =
            MinusZero.Instance.Root.Get(
                false,
                @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");
        var root = CreateTempVertex("Root");
        var first = CreateTempVertex("First");
        var second = CreateTempVertex("Second");
        root.AddEdge(nextAtomMeta, first);
        root.AddEdge(nextAtomMeta, second);
        var execution =
            new RecordingExecution
            {
                ReturnOnValue = "First"
            };

        var result =
            ZeroCodeExecutonUtil
                .SequentiallyExecuteInstructions_NextEdges(
                    execution,
                    execution.Stack,
                    root,
                    out var isStackFrameReturn);

        Assert.True(isStackFrameReturn);
        Assert.Same(
            execution.Stack,
            result);
        Assert.Equal(
            new object[] { "First" },
            execution.ExecutedValues);
    }

    private static INoInEdgeInOutVertexVertex CreateReturnedStack()
    {
        return InstructionHelpers.CreateStack();
    }

    private static IVertex CreateTempVertex(object value)
    {
        return MinusZero.Instance.TempStore.Root.AddVertex(MinusZero.Instance.Empty, value);
    }

    private sealed class RecordingExecution
        : IExecution
    {
        public RecordingExecution()
        {
            Stack = InstructionHelpers.CreateStack();
            NewVertexCreationSpace = Stack;
        }

        public List<object> ExecutedValues
        {
            get;
        } = new List<object>();

        public object? ReturnOnValue { get; set; }

        public INoInEdgeInOutVertexVertex Stack
        {
            get;
            set;
        }

        public IVertex NewVertexCreationSpace
        {
            get;
            set;
        }

        public bool MetaMode { get; set; }

        public void AddStackFrame()
        {
            throw new NotSupportedException();
        }

        public void AddStackFrame(
            IVertex newStackFrame)
        {
            throw new NotSupportedException();
        }

        public void RemoveStackFrame()
        {
            throw new NotSupportedException();
        }

        public void CreateEmptyStack()
        {
            throw new NotSupportedException();
        }

        public INoInEdgeInOutVertexVertex
            ExecuteInstructionByMontevideoPrinciples(
                IVertex inputQs,
                IVertex instructionVertex,
                out bool isStackFrameReturn)
        {
            return ExecuteInstruction(
                inputQs,
                instructionVertex,
                out isStackFrameReturn);
        }

        public INoInEdgeInOutVertexVertex
            ExecuteInstruction(
                IVertex inputQs,
                IVertex instructionVertex,
                out bool isStackFrameReturn)
        {
            ExecutedValues.Add(
                instructionVertex.Value);
            isStackFrameReturn =
                Equals(
                    instructionVertex.Value,
                    ReturnOnValue);
            return Stack;
        }
    }
}
