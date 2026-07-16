using m0;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class TransactionContractTests
{
    [Fact]
    public void QueryReadsEachMutationBeforeTransactionCompletion()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Before");
        var transaction = StartTransaction();

        try
        {
            var edge = source.AddEdge(meta, target);
            Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "Before"));

            target.Value = "After";
            Assert.Empty(GraphUtil.GetQueryOut(source, "Meta", "Before"));
            Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "After"));

            source.DeleteEdge(edge);
            Assert.Empty(GraphUtil.GetQueryOut(source, "Meta", null));
        }
        finally
        {
            RestorePreviousTransaction(transaction);
        }
    }

    [Fact]
    public void RollbackRestoresAddedEdgeState()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var execution = new GraphTestExecution(fixture);
        var transaction = StartTransaction();

        try
        {
            source.AddEdge(meta, target);
            transaction.Rollback(execution);

            Assert.Empty(GraphUtil.GetQueryOut(source, "Meta", null));
        }
        finally
        {
            RestorePreviousTransaction(transaction);
        }
    }

    [Fact]
    public void RollbackRestoresChangedValueAndWarmIndexes()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Before");
        var edge = source.AddEdge(meta, target);
        var execution = new GraphTestExecution(fixture);
        Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "Before"));
        var transaction = StartTransaction();

        try
        {
            target.Value = "After";
            transaction.Rollback(execution);

            Assert.Equal("Before", target.Value);
            Assert.Equal(new[] { edge }, GraphUtil.GetQueryOut(source, "Meta", "Before"));
            Assert.Empty(GraphUtil.GetQueryOut(source, "Meta", "After"));
        }
        finally
        {
            RestorePreviousTransaction(transaction);
        }
    }

    [Fact]
    public void RepeatedValueChangesCoalesceButRollbackFullJournal()
    {
        var fixture = new GraphFixture();
        var target = fixture.CreateVertex("Before");
        var execution = new GraphTestExecution(fixture);
        var transaction = StartTransaction();

        try
        {
            target.Value = "First";
            target.Value = "Second";

            var listenerAtom = Assert.Single(
                transaction
                    .graphChangeTransactionAtoms_OutEdgeValueChange[
                        target]);
            Assert.Equal("Before", listenerAtom.OldValue);
            Assert.Equal("Second", listenerAtom.NewValue);

            transaction.Rollback(execution);

            Assert.Equal("Before", target.Value);
        }
        finally
        {
            RestorePreviousTransaction(transaction);
        }
    }

    [Fact]
    public void AddedThenRemovedEdgeProducesNoNetChangeEvent()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var execution = new GraphTestExecution(fixture);
        var transaction = StartTransaction();

        try
        {
            var edge = source.AddEdge(meta, target);
            source.DeleteEdge(edge);

            Assert.False(
                transaction
                    .graphChangeTransactionAtoms_OutEdgeValueChange
                    .ContainsKey(source));
            Assert.False(
                transaction.graphChangeTransactionAtoms_InEdge
                    .ContainsKey(target));
            Assert.False(
                transaction.graphChangeTransactionAtoms_MetaEdge
                    .ContainsKey(meta));

            transaction.Rollback(execution);

            Assert.Empty(source.OutEdgesRaw);
        }
        finally
        {
            RestorePreviousTransaction(transaction);
        }
    }

    [Fact]
    public void RemovedThenAddedEquivalentEdgePreservesIdentityChange()
    {
        var fixture = new GraphFixture();
        var source = fixture.CreateVertex("Source");
        var meta = fixture.CreateVertex("Meta");
        var target = fixture.CreateVertex("Target");
        var originalEdge = source.AddEdge(meta, target);
        var execution = new GraphTestExecution(fixture);
        var transaction = StartTransaction();

        try
        {
            source.DeleteEdge(originalEdge);
            var replacementEdge =
                source.AddEdge(meta, target);

            var listenerAtoms = transaction
                .graphChangeTransactionAtoms_OutEdgeValueChange[
                    source];
            Assert.Collection(
                listenerAtoms,
                atom =>
                {
                    Assert.Equal(
                        AtomGraphChangeTypeEnum.EdgeRemoved,
                        atom.Type);
                    Assert.Same(originalEdge, atom.Edge);
                },
                atom =>
                {
                    Assert.Equal(
                        AtomGraphChangeTypeEnum.EdgeAdded,
                        atom.Type);
                    Assert.Same(replacementEdge, atom.Edge);
                });

            transaction.Rollback(execution);

            var restoredEdge = Assert.Single(
                source.OutEdgesRaw);
            Assert.Same(meta, restoredEdge.Meta);
            Assert.Same(target, restoredEdge.To);
        }
        finally
        {
            RestorePreviousTransaction(transaction);
        }
    }

    private static Transaction StartTransaction()
    {
        var previousTransaction = MinusZero.Instance.GetTopTransaction();
        var transaction = new Transaction(previousTransaction);
        transaction.Start();
        MinusZero.Instance.SetTopTransaction(transaction);
        return transaction;
    }

    private static void RestorePreviousTransaction(Transaction transaction)
    {
        MinusZero.Instance.SetTopTransaction(transaction.Previous);
    }
}
