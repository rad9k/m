using m0;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.ZeroUML.Instructions;
using m0_graph_test_support;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class ZeroCodePerformanceWorkloadTests
{
    public ZeroCodePerformanceWorkloadTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void FreshPerformanceWorkloadParsesEveryProgram()
    {
        IEdge parsedRootEdge = ParseWorkload();
        HashSet<string> programNames = parsedRootEdge.To
            .OutEdgesRaw
            .Select(edge => edge.To?.Value?.ToString())
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal("code", parsedRootEdge.To.Value);
        Assert.All(
            ZeroCodePerformanceWorkload.ProgramNames,
            programName => Assert.Contains(programName, programNames));
    }

    [Fact]
    public void FreshPerformanceWorkloadExecutesWithoutReportedErrors()
    {
        IEdge parsedRootEdge = ParseWorkload();
        RecordingUserInteraction userInteraction =
            new RecordingUserInteraction();
        IUserInteraction previousUserInteraction =
            MinusZero.Instance.UserInteraction;
        MinusZero.Instance.SetUserInteraction(userInteraction);

        try
        {
            foreach (string programName in
                ZeroCodePerformanceWorkload.ProgramNames)
            {
                IVertex program = parsedRootEdge.To
                    .OutEdgesRaw
                    .Single(
                        edge => string.Equals(
                            edge.To?.Value?.ToString(),
                            programName,
                            StringComparison.Ordinal))
                    .To;
                int previousErrorCount =
                    userInteraction.Exceptions.Count;
                IVertex result =
                    MinusZero.Instance.DefaultExecuter.Execute(
                        InstructionHelpers.CreateStack(),
                        program);

                Assert.NotNull(result);
                Assert.Equal(
                    previousErrorCount,
                    userInteraction.Exceptions.Count);

                if (programName == "Code7")
                    Assert.Equal(
                        4369,
                        GraphUtil.GetQueryOutCount(
                            result,
                            "E",
                            null));
                else if (programName == "Code7b")
                    Assert.Equal(
                        new[] { "1", "2", "a", "b" },
                        GraphUtil.GetQueryOut(
                                result,
                                "B",
                                null)
                            .Select(
                                edge => Convert.ToString(
                                    edge.To.Value))
                            .OrderBy(
                                value => value,
                                StringComparer.Ordinal)
                            .ToArray());
                else if (programName == "Code8")
                    Assert.Equal(
                        100001,
                        Convert.ToInt32(
                            GraphUtil.GetQueryOutFirst(
                                result,
                                "A",
                                null)
                                ?.Value));
            }
        }
        finally
        {
            MinusZero.Instance.SetUserInteraction(
                previousUserInteraction);
        }
    }

    [Fact]
    public void ScalarRedirectAddPreservesNumericResultAndFallback()
    {
        const string source =
            "\"ScalarRedirectAdd\"\r\n" +
            "\tvariable \"IntegerValue\" @String\r\n" +
            "\tvariable \"FallbackValue\" @String\r\n" +
            "\tvariable \"NumericFallbackValue\" @String\r\n" +
            "\tIntegerValue = \"1\"\r\n" +
            "\tFallbackValue = \"not-a-number\"\r\n" +
            "\tNumericFallbackValue = \"not-a-number\"\r\n" +
            "\tIntegerValue = IntegerValue + \"2\"\r\n" +
            "\tIntegerValue = IntegerValue + \"3\"\r\n" +
            "\tFallbackValue = FallbackValue + \"right\"\r\n" +
            "\tNumericFallbackValue = " +
                "NumericFallbackValue + \"2\"";
        IEdge parsedRootEdge = ParseSource(source);

        IVertex result =
            MinusZero.Instance.DefaultExecuter.Execute(
                InstructionHelpers.CreateStack(),
                parsedRootEdge.To);

        Assert.Equal(
            6,
            Convert.ToInt32(
                GraphUtil.GetQueryOutFirst(
                    result,
                    "IntegerValue",
                    null)?.Value));
        Assert.Equal(
            "right",
            Convert.ToString(
                GraphUtil.GetQueryOutFirst(
                    result,
                    "FallbackValue",
                    null)?.Value));
        Assert.IsType<string>(
            GraphUtil.GetQueryOutFirst(
                result,
                "NumericFallbackValue",
                null)?.Value);
        Assert.Equal(
            "2",
            GraphUtil.GetQueryOutFirst(
                result,
                "NumericFallbackValue",
                null)?.Value);
    }

    [Fact]
    public void ScalarRedirectDoesNotMutateAliasedValue()
    {
        const string source =
            "\"ScalarRedirectAlias\"\r\n" +
            "\tvariable \"First\" @String\r\n" +
            "\tvariable \"Second\" @String\r\n" +
            "\tFirst = \"1\"\r\n" +
            "\tSecond = First\r\n" +
            "\tFirst = First + \"1\"";
        IEdge parsedRootEdge = ParseSource(source);

        IVertex result =
            MinusZero.Instance.DefaultExecuter.Execute(
                InstructionHelpers.CreateStack(),
                parsedRootEdge.To);
        IVertex first =
            GraphUtil.GetQueryOutFirst(
                result,
                "First",
                null);
        IVertex second =
            GraphUtil.GetQueryOutFirst(
                result,
                "Second",
                null);

        Assert.Equal(
            2,
            Convert.ToInt32(first?.Value));
        Assert.Equal(
            1,
            Convert.ToInt32(second?.Value));
        Assert.NotSame(
            first,
            second);
    }

    [Fact]
    public void ScalarExpressionPlanInvalidatesAfterExpressionMutation()
    {
        const string source =
            "\"ScalarPlanInvalidation\"\r\n" +
            "\tvariable \"Value\" @String\r\n" +
            "\tValue = \"1\"\r\n" +
            "\tValue = Value + \"2\"";
        IEdge parsedRootEdge = ParseSource(source);

        IVertex firstResult =
            MinusZero.Instance.DefaultExecuter.Execute(
                InstructionHelpers.CreateStack(),
                parsedRootEdge.To);
        Assert.Equal(
            3,
            Convert.ToInt32(
                GraphUtil.GetQueryOutFirst(
                    firstResult,
                    "Value",
                    null)?.Value));

        IVertex literal = Descendants(
                parsedRootEdge.To)
            .Single(
                vertex =>
                    string.Equals(
                        vertex.Value?.ToString(),
                        "2",
                        StringComparison.Ordinal) &&
                    InstructionHelpers.GetIs(vertex) == null);
        literal.Value = "4";

        IVertex secondResult =
            MinusZero.Instance.DefaultExecuter.Execute(
                InstructionHelpers.CreateStack(),
                parsedRootEdge.To);
        Assert.Equal(
            5,
            Convert.ToInt32(
                GraphUtil.GetQueryOutFirst(
                    secondResult,
                    "Value",
                    null)?.Value));

        IVertex addExpression = Descendants(
                parsedRootEdge.To)
            .Single(
                vertex => string.Equals(
                    InstructionHelpers.GetIs(vertex)
                        ?.Value?.ToString(),
                    "+",
                    StringComparison.Ordinal));
        IEdge rightEdge =
            GraphUtil.GetQueryOutFirstEdge(
                addExpression,
                "RightExpression",
                null);
        Assert.NotNull(rightEdge);
        IVertex rightMeta = rightEdge.Meta;
        addExpression.DeleteEdge(rightEdge);
        addExpression.AddVertex(
            rightMeta,
            "6");

        IVertex thirdResult =
            MinusZero.Instance.DefaultExecuter.Execute(
                InstructionHelpers.CreateStack(),
                parsedRootEdge.To);
        Assert.Equal(
            7,
            Convert.ToInt32(
                GraphUtil.GetQueryOutFirst(
                    thirdResult,
                    "Value",
                    null)?.Value));
    }

    [Fact]
    public void WhileReassignsDynamicPathAccumulator()
    {
        const string source =
            "\"DynamicPathWhile\"\r\n" +
            "\tvariable \"accumulator\" @VertexType\r\n" +
            "\tvariable \"segment\" @String\r\n" +
            "\tvariable \"counter\" @Integer\r\n" +
            "\taccumulator = root\r\n" +
            "\tcounter = \"1\"\r\n" +
            "\twhile counter < \"3\"\r\n" +
            "\t\tsegment = segments<<counter>>\r\n" +
            "\t\taccumulator = accumulator\\:'(segment)'\r\n" +
            "\t\tcounter = counter + \"1\"";
        IEdge parsedRootEdge = ParseSource(source);

        IVertex CreateVertex(string value)
        {
            return MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                value);
        }

        IVertex root = CreateVertex("Root");
        IVertex first = CreateVertex("First");
        IVertex second = CreateVertex("Second");
        root.AddEdge(MinusZero.Instance.Empty, first);
        first.AddEdge(MinusZero.Instance.Empty, second);

        IVertex segments = CreateVertex("Segments");
        segments.AddVertex(MinusZero.Instance.Empty, "First");
        segments.AddVertex(MinusZero.Instance.Empty, "Second");

        IVertex inputStack = InstructionHelpers.CreateStack();
        inputStack.AddEdge(CreateVertex("root"), root);
        inputStack.AddEdge(CreateVertex("segments"), segments);

        IVertex result = MinusZero.Instance.DefaultExecuter.Execute(
            inputStack,
            parsedRootEdge.To);

        Assert.Same(
            second,
            GraphUtil.GetQueryOutFirst(
                result,
                "accumulator",
                null));
    }

    [Fact]
    public void RedirectPropagationPlanInvalidatesAfterTypeHierarchyMutation()
    {
        const string source =
            "\"PropagationPlanInvalidation\"\r\n" +
            "\tvariable \"Value\" @String\r\n" +
            "\tValue = \"1\"\r\n" +
            "\tValue = Value + \"1\"";
        IEdge parsedRootEdge = ParseSource(source);
        IVertex redirectInstruction =
            Descendants(parsedRootEdge.To)
                .Single(
                    vertex =>
                        string.Equals(
                            InstructionHelpers.GetIs(vertex)
                                ?.Value?.ToString(),
                            "RedirectLeftEdgesToRightVertices",
                            StringComparison.Ordinal) &&
                        string.Equals(
                            InstructionHelpers.GetIs(
                                    InstructionHelpers.GetRight(
                                        vertex))
                                ?.Value?.ToString(),
                            "+",
                            StringComparison.Ordinal));
        IVertex leftExpression =
            InstructionHelpers.GetLeft(
                redirectInstruction);
        IVertex customType =
            MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                $"PropagationType-{Guid.NewGuid():N}");
        leftExpression.AddEdge(
            MinusZero.Instance.Is,
            customType);

        var execution =
            new ZeroCodeExecution();
        ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(
            execution,
            execution.Stack,
            parsedRootEdge.To,
            out _);
        IVertex parentFrame = execution.Stack;
        Assert.Contains(
            parentFrame.OutEdgesRaw,
            edge => string.Equals(
                edge.Meta?.Value?.ToString(),
                "Value",
                StringComparison.Ordinal));

        execution.AddStackFrame();
        IVertex propagationType =
            MinusZero.Instance.TempStore.Root.AddVertex(
                MinusZero.Instance.Empty,
                "PropagateToStackExpression");
        IEdge inheritanceEdge =
            customType.AddEdge(
                MinusZero.Instance.Inherits,
                propagationType);

        try
        {
            BaseInstructions
                .RedirectLeftEdgesToRightVertices(
                    execution,
                    execution.Stack,
                    redirectInstruction,
                    out _);

            Assert.DoesNotContain(
                parentFrame.OutEdgesRaw,
                edge => string.Equals(
                    edge.Meta?.Value?.ToString(),
                    "Value",
                    StringComparison.Ordinal));
            Assert.Contains(
                execution.Stack.OutEdgesRaw,
                edge => string.Equals(
                    edge.Meta?.Value?.ToString(),
                    "Value",
                    StringComparison.Ordinal));
        }
        finally
        {
            customType.DeleteEdge(
                inheritanceEdge);
        }
    }

    private static IEnumerable<IVertex> Descendants(
        IVertex root)
    {
        HashSet<IVertex> visited =
            new HashSet<IVertex>(
                ReferenceEqualityComparer.Instance);
        Stack<IVertex> pending =
            new Stack<IVertex>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            IVertex current = pending.Pop();
            if (!visited.Add(current))
                continue;

            yield return current;
            foreach (IEdge edge in current.OutEdgesRaw)
                if (edge.To != null &&
                    ReferenceEquals(
                        edge.To.Store,
                        root.Store))
                    pending.Push(edge.To);
        }
    }

    private static IEdge ParseWorkload()
    {
        return ParseSource(
            ZeroCodePerformanceWorkload.Source);
    }

    private static IEdge ParseSource(
        string source)
    {
        IVertex parseParent = MinusZero.Instance.TempStore.Root
            .AddVertex(
                MinusZero.Instance.Empty,
                $"ZeroCodePerformance-{Guid.NewGuid():N}");
        IEdge sourceEdge = parseParent.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "Source");
        IVertex parseErrors =
            MinusZero.Instance.DefaultFormalTextParser.Parse(
                sourceEdge,
                source,
                CodeRepresentationEnum.LinearizedManyLines,
                out IEdge parsedRootEdge);

        Assert.True(
            parseErrors == null || !parseErrors.Any(),
            parseErrors == null
                ? "The workload parser did not return a root edge."
                : string.Join(
                    Environment.NewLine,
                    parseErrors.Select(
                        edge => edge.To?.Value?.ToString() ??
                            "<error without value>")));
        Assert.NotNull(parsedRootEdge);
        return parsedRootEdge;
    }

    private sealed class RecordingUserInteraction
        : NoOpUserInteraction
    {
        internal List<IVertex> Exceptions { get; } =
            new List<IVertex>();

        public override void InteractionOutputException(
            IVertex exception)
        {
            Exceptions.Add(exception);
        }
    }
}
