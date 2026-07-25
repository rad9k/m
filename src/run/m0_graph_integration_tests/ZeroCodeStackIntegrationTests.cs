using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
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
    public void StackValueCreationIsNotTransactional()
    {
        var meta =
            CreateTempVertex("Value");
        var tempStore =
            (m0.Store.StoreBase)
                MinusZero.Instance.TempStore;
        var initialStoreVertexCount =
            tempStore.VertexIdentifiersDictionary.Count;
        var ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        ExecutionFlowHelper.StartTransaction();

        try
        {
            var stack =
                InstructionHelpers.CreateStack();
            var valueEdge =
                stack.AddVertexAndReturnEdge(
                    meta,
                    "Temporary");
            var transaction =
                Assert.IsType<Transaction>(
                    MinusZero.Instance
                        .GetTopTransaction());

            Assert.False(
                transaction
                    .graphChangeTransactionAtoms_OutEdgeValueChange
                    .ContainsKey(valueEdge.To));
            Assert.False(
                tempStore.VertexIdentifiersDictionary
                    .ContainsKey(valueEdge.To.Identifier));
            Assert.Equal(
                initialStoreVertexCount,
                tempStore.VertexIdentifiersDictionary.Count);

            ExecutionFlowHelper.RollbackTransaction();

            Assert.Equal(
                "Temporary",
                valueEdge.To.Value);
            Assert.Contains(
                valueEdge,
                stack.OutEdgesRaw);
            Assert.Same(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction());
        }
        finally
        {
            if (!ReferenceEquals(
                    ambientTransaction,
                    MinusZero.Instance.GetTopTransaction()))
            {
                ExecutionFlowHelper.RollbackTransaction();
            }
        }
    }

    [Fact]
    public void ExclusiveScalarAssignmentUpdatesTargetInPlace()
    {
        var execution =
            new ZeroCodeExecution();
        var valueMeta =
            CreateTempVertex("ExclusiveValue");
        var valueEdge =
            execution.Stack.AddVertexAndReturnEdge(
                valueMeta,
                1);
        var originalTarget =
            valueEdge.To;

        Assert.True(
            execution
                .TryUpdateExclusiveScalarAssignmentTarget(
                    valueEdge,
                    EasyVertex.ScalarNumericValue
                        .FromInteger(2)));
        Assert.Same(
            originalTarget,
            valueEdge.To);
        Assert.Equal(
            2,
            valueEdge.To.Value);
    }

    [Fact]
    public void ExclusiveScalarAssignmentPreservesNumericTypes()
    {
        var execution =
            new ZeroCodeExecution();
        var valueMeta =
            CreateTempVertex("ExclusiveNumericValue");
        var valueEdge =
            execution.Stack.AddVertexAndReturnEdge(
                valueMeta,
                1);

        Assert.True(
            execution
                .TryUpdateExclusiveScalarAssignmentTarget(
                    valueEdge,
                    EasyVertex.ScalarNumericValue
                        .FromDouble(1.5)));
        Assert.IsType<double>(
            valueEdge.To.Value);
        Assert.Equal(
            1.5,
            valueEdge.To.Value);

        Assert.True(
            execution
                .TryUpdateExclusiveScalarAssignmentTarget(
                    valueEdge,
                    EasyVertex.ScalarNumericValue
                        .FromDecimal(2.25m)));
        Assert.IsType<decimal>(
            valueEdge.To.Value);
        Assert.Equal(
            2.25m,
            valueEdge.To.Value);
    }

    [Fact]
    public void ExclusiveScalarAssignmentRejectsAliasesAndSubgraphs()
    {
        var execution =
            new ZeroCodeExecution();
        var valueMeta =
            CreateTempVertex("ExclusiveValue");
        var aliasMeta =
            CreateTempVertex("ExclusiveAlias");
        var childMeta =
            CreateTempVertex("ExclusiveChild");
        var valueEdge =
            execution.Stack.AddVertexAndReturnEdge(
                valueMeta,
                1);
        var aliasEdge =
            execution.Stack.AddEdge(
                aliasMeta,
                valueEdge.To);

        Assert.False(
            execution
                .TryUpdateExclusiveScalarAssignmentTarget(
                    valueEdge,
                    2));
        Assert.Equal(
            1,
            valueEdge.To.Value);

        execution.Stack.DeleteEdge(
            aliasEdge);
        valueEdge.To.AddVertex(
            childMeta,
            "Nested");

        Assert.False(
            execution
                .TryUpdateExclusiveScalarAssignmentTarget(
                    valueEdge,
                    2));
        Assert.Equal(
            1,
            valueEdge.To.Value);
    }

    [Fact]
    public void CachedExclusiveScalarTargetInvalidatesAfterBatchAlias()
    {
        var execution =
            new ZeroCodeExecution();
        var instruction =
            CreateTempVertex("CachedRedirect");
        var valueMeta =
            CreateTempVertex("CachedExclusiveValue");
        var aliasMeta =
            CreateTempVertex("CachedExclusiveAlias");
        var valueEdge =
            execution.Stack.AddVertexAndReturnEdge(
                valueMeta,
                1);
        execution.CacheRedirectAssignmentTarget(
            instruction,
            valueEdge,
            null,
            null,
            null);

        Assert.True(
            execution.TryGetCachedRedirectAssignmentTarget(
                instruction,
                out _,
                out _,
                out _,
                out bool initiallyExclusive));
        Assert.True(initiallyExclusive);

        var aliasSource =
            InstructionHelpers.CreateStack();
        var aliasEdge =
            aliasSource.AddEdge(
                aliasMeta,
                valueEdge.To);
        execution.Stack.AddRangeOriginalEdges(
            new[] { aliasEdge });

        Assert.True(
            execution.TryGetCachedRedirectAssignmentTarget(
                instruction,
                out _,
                out _,
                out _,
                out bool exclusiveAfterAlias));
        Assert.False(exclusiveAfterAlias);
    }

    [Fact]
    public void ExclusiveScalarAssignmentRejectsPromotedTarget()
    {
        var execution =
            new ZeroCodeExecution();
        var valueMeta =
            CreateTempVertex("ExclusiveValue");
        var container =
            CreateTempVertex("ExclusiveContainer");
        var valueEdge =
            execution.Stack.AddVertexAndReturnEdge(
                valueMeta,
                1);
        Assert.True(
            execution
                .TryUpdateExclusiveScalarAssignmentTarget(
                    valueEdge,
                    EasyVertex.ScalarNumericValue
                        .FromInteger(2)));
        var persistentEdge =
            container.AddEdge(
                valueMeta,
                valueEdge.To);

        Assert.False(
            execution
                .TryUpdateExclusiveScalarAssignmentTarget(
                    valueEdge,
                    EasyVertex.ScalarNumericValue
                        .FromInteger(3)));
        Assert.Equal(
            2,
            valueEdge.To.Value);

        container.DeleteEdge(
            persistentEdge);
    }

    [Fact]
    public void StackValueSubgraphRegistersWhenAttachedToRegularGraph()
    {
        var meta =
            CreateTempVertex("Value");
        var childMeta =
            CreateTempVertex("Child");
        var container =
            CreateTempVertex("Container");
        var tempStore =
            (m0.Store.StoreBase)
                MinusZero.Instance.TempStore;
        var stack =
            InstructionHelpers.CreateStack();
        var valueEdge =
            stack.AddVertexAndReturnEdge(
                meta,
                "Temporary");
        var child =
            valueEdge.To.AddVertex(
                childMeta,
                "Nested");

        Assert.Null(
            Assert.IsType<EasyVertex>(
                valueEdge.To)._Identifier);
        Assert.Null(
            Assert.IsType<EasyVertex>(
                child)._Identifier);
        Assert.DoesNotContain(
            tempStore.VertexIdentifiersDictionary.Values,
            vertex => ReferenceEquals(
                vertex,
                valueEdge.To));
        Assert.DoesNotContain(
            tempStore.VertexIdentifiersDictionary.Values,
            vertex => ReferenceEquals(
                vertex,
                child));

        var persistentEdge =
            container.AddEdge(
                meta,
                valueEdge.To);

        Assert.Same(
            valueEdge.To,
            tempStore.GetVertexByIdentifier(
                valueEdge.To.Identifier));
        Assert.Same(
            child,
            tempStore.GetVertexByIdentifier(
                child.Identifier));

        container.DeleteEdge(
            persistentEdge);
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
    public void SequentialExecutionPreservesDirectAndNextAtomOrder()
    {
        var nextAtomMeta =
            MinusZero.Instance.Root.Get(
                false,
                @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");
        var executionRoot =
            CreateTempVertex("ExecutionRoot");
        var ignoredDirectInstruction =
            CreateTempVertex("Ignored");
        var directInstruction =
            CreateTempVertex("Direct");
        var nextInstruction =
            CreateTempVertex("Next");
        executionRoot.AddEdge(
            CreateTempVertex("$Ignored"),
            ignoredDirectInstruction);
        executionRoot.AddEdge(
            CreateTempVertex("$Empty"),
            directInstruction);
        directInstruction.AddEdge(
            nextAtomMeta,
            nextInstruction);
        var execution =
            new RecordingExecution();

        _ = ZeroCodeExecutonUtil
            .SequentiallyExecuteInstructions(
                execution,
                execution.Stack,
                executionRoot,
                out var isStackFrameReturn);

        Assert.False(isStackFrameReturn);
        Assert.Equal(
            new object[] { "Direct", "Next" },
            execution.ExecutedValues);
    }

    [Fact]
    public void SequentialExecutionObservesNextAtomAddedByDirectInstruction()
    {
        var nextAtomMeta =
            MinusZero.Instance.Root.Get(
                false,
                @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");
        var executionRoot =
            CreateTempVertex("ExecutionRoot");
        var nextAtomRoot =
            CreateTempVertex("NextAtomRoot");
        var directInstruction =
            CreateTempVertex("Direct");
        var dynamicallyAddedInstruction =
            CreateTempVertex("Dynamic");
        executionRoot.AddEdge(
            CreateTempVertex("$Ignored"),
            nextAtomRoot);
        executionRoot.AddEdge(
            CreateTempVertex("$Empty"),
            directInstruction);
        var execution =
            new RecordingExecution
            {
                OnExecute =
                    instruction =>
                    {
                        if (ReferenceEquals(
                                instruction,
                                directInstruction))
                        {
                            nextAtomRoot.AddEdge(
                                nextAtomMeta,
                                dynamicallyAddedInstruction);
                        }
                    }
            };

        _ = ZeroCodeExecutonUtil
            .SequentiallyExecuteInstructions(
                execution,
                execution.Stack,
                executionRoot,
                out var isStackFrameReturn);

        Assert.False(isStackFrameReturn);
        Assert.Equal(
            new object[] { "Direct", "Dynamic" },
            execution.ExecutedValues);
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

        public Action<IVertex>? OnExecute { get; set; }

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
            OnExecute?.Invoke(
                instructionVertex);
            isStackFrameReturn =
                Equals(
                    instructionVertex.Value,
                    ReturnOnValue);
            return Stack;
        }
    }
}
