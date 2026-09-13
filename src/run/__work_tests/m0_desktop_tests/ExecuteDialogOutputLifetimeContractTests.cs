using System.Reflection;
using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Dialog;
using m0.ZeroCode.Helpers;

namespace m0_desktop_tests;

[Collection(DesktopGraphCollection.Name)]
public sealed class ExecuteDialogOutputLifetimeContractTests
{
    public ExecuteDialogOutputLifetimeContractTests(
        DesktopBootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void OutputReferenceSetKeepsTemporaryResultsLive()
    {
        StaTestHost.Run(
            () =>
            {
                IVertex result =
                    MinusZero.Instance.CreateTempVertex();
                INoInEdgeInOutVertexVertex outputStack =
                    InstructionHelpers.CreateStack();
                IEdge resultEdge =
                    GraphUtil.CreateArtificialEdge(
                        null,
                        result);
                outputStack
                    .AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                        resultEdge);
                outputStack
                    .AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(
                        resultEdge);
                int initialExternalReferences =
                    result.ExternalReferenceCount;
                IDisposable referenceSet =
                    CreateOutputReferenceSet();

                try
                {
                    ReplaceOutput(
                        referenceSet,
                        outputStack);
                    CommitPendingLifecycleActions();

                    Assert.Equal(
                        DisposeStateEnum.Live,
                        result.DisposedState);
                    Assert.Equal(
                        initialExternalReferences + 1,
                        result.ExternalReferenceCount);
                }
                finally
                {
                    referenceSet.Dispose();
                }

                CommitPendingLifecycleActions();

                Assert.Equal(
                    initialExternalReferences,
                    result.ExternalReferenceCount);
            });
    }

    private static IDisposable
        CreateOutputReferenceSet()
    {
        Type referenceSetType =
            typeof(ExecuteDialog).GetNestedType(
                "OutputVertexReferenceSet",
                BindingFlags.NonPublic)!;
        return (IDisposable)Activator.CreateInstance(
            referenceSetType)!;
    }

    private static void ReplaceOutput(
        IDisposable referenceSet,
        IVertex outputStack)
    {
        MethodInfo replaceMethod =
            referenceSet.GetType().GetMethod(
                "Replace",
                BindingFlags.Instance |
                BindingFlags.Public)!;
        replaceMethod.Invoke(
            referenceSet,
            new object[] { outputStack });
    }

    private static void CommitPendingLifecycleActions()
    {
        ITransaction ambientTransaction =
            MinusZero.Instance.GetTopTransaction();
        ExecutionFlowHelper.StartTransaction();

        try
        {
            ExecutionFlowHelper.CommitTransaction();
        }
        finally
        {
            if (!ReferenceEquals(
                ambientTransaction,
                MinusZero.Instance.GetTopTransaction()))
            {
                ExecutionFlowHelper
                    .RollbackTransaction();
            }
        }
    }

}
