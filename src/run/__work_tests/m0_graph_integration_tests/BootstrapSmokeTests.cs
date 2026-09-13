using m0;
using m0.Foundation;
using m0.Store;
using m0.ZeroCode.Helpers;

namespace m0_graph_integration_tests;

[Collection(BootstrappedGraphCollection.Name)]
public sealed class BootstrapSmokeTests
{
    public BootstrapSmokeTests(BootstrapFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public void BootstrapProvidesCoreGraphAndMetaVertices()
    {
        Assert.True(MinusZero.Instance.IsInitialized);
        Assert.NotNull(MinusZero.Instance.Root);
        Assert.NotNull(MinusZero.Instance.TempStore);
        Assert.NotNull(MinusZero.Instance.Empty);
        Assert.NotNull(MinusZero.Instance.Inherits);
        Assert.NotNull(MinusZero.Instance.Is);
        Assert.NotNull(MinusZero.Instance.StackFrameInherits);
        Assert.True(MinusZero.Instance.Root.IsRoot);
    }

    [Fact]
    public void ProductionStackFactoryCreatesEphemeralUsableStack()
    {
        var tempStore = (m0.Store.StoreBase)
            MinusZero.Instance.TempStore;
        var initialStoreVertexCount =
            tempStore.VertexIdentifiersDictionary.Count;

        var stack = InstructionHelpers.CreateStack();
        var secondStack = InstructionHelpers.CreateStack();

        Assert.NotNull(stack);
        Assert.Same(MinusZero.Instance.TempStore, stack.Store);
        Assert.NotNull(stack.Identifier);
        Assert.NotEqual(stack.Identifier, secondStack.Identifier);
        Assert.False(
            tempStore.VertexIdentifiersDictionary.ContainsKey(
                stack.Identifier));
        Assert.Equal(
            initialStoreVertexCount,
            tempStore.VertexIdentifiersDictionary.Count);
    }

    [Fact]
    public void RepeatedProductionStackCreationDoesNotGrowTempStore()
    {
        var tempStore = (m0.Store.StoreBase)
            MinusZero.Instance.TempStore;
        var initialStoreVertexCount =
            tempStore.VertexIdentifiersDictionary.Count;

        for (var index = 0; index < 1000; index++)
            _ = InstructionHelpers.CreateStack();

        Assert.Equal(
            initialStoreVertexCount,
            tempStore.VertexIdentifiersDictionary.Count);
    }

    [Fact]
    public void StoreLookupReturnsExistingInstanceWithoutAddingDuplicate()
    {
        var initialStoreCount =
            MinusZero.Instance.Stores.Count;
        var identifier =
            $"store-lookup-{Guid.NewGuid():N}";
        var store = new MemoryStore(
            identifier,
            MinusZero.Instance,
            new[] { AccessLevelEnum.NoRestrictions },
            true);

        try
        {
            Assert.Same(
                store,
                MinusZero.Instance.GetStore(
                    store.TypeName,
                    identifier));
            Assert.Same(
                store,
                MinusZero.Instance.GetStore(identifier));
            Assert.Equal(
                initialStoreCount + 1,
                MinusZero.Instance.Stores.Count);
        }
        finally
        {
            MinusZero.Instance.RemoveStore(store);
        }
    }
}
