using m0.Foundation;
using m0.Graph;

namespace m0_graph_test_support;

public sealed class GraphTestExecution : IExecution
{
    private readonly GraphFixture fixture;

    public GraphTestExecution(GraphFixture fixture)
    {
        this.fixture = fixture;
        CreateEmptyStack();
        NewVertexCreationSpace = Stack;
    }

    public INoInEdgeInOutVertexVertex Stack { get; set; } = null!;

    public IVertex NewVertexCreationSpace { get; set; } = null!;

    public bool MetaMode { get; set; }

    public void AddStackFrame()
    {
        throw new NotSupportedException();
    }

    public void AddStackFrame(IVertex newStackFrame)
    {
        throw new NotSupportedException();
    }

    public void RemoveStackFrame()
    {
        throw new NotSupportedException();
    }

    public void CreateEmptyStack()
    {
        Stack = new NoInEdgeInOutVertexVertex(fixture.Store);
    }

    public INoInEdgeInOutVertexVertex ExecuteInstructionByMontevideoPrinciples(
        IVertex inputQs,
        IVertex instructionVertex,
        out bool isStackFrameReturn)
    {
        throw new NotSupportedException();
    }

    public INoInEdgeInOutVertexVertex ExecuteInstruction(
        IVertex inputQs,
        IVertex instructionVertex,
        out bool isStackFrameReturn)
    {
        throw new NotSupportedException();
    }
}
