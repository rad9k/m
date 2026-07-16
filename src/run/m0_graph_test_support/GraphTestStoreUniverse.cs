using m0.Foundation;

namespace m0_graph_test_support;

public sealed class GraphTestStoreUniverse : IStoreUniverse
{
    private readonly List<IStore> stores = new();

    public IList<IStore> Stores => stores;

    public IVertex Root =>
        stores.Count > 0
            ? stores[0].Root
            : throw new InvalidOperationException("The test store universe has no stores.");

    public IVertex Empty => Root;

    public IStore? GetStore(string storeTypeName, string storeIdentifier)
    {
        return stores.FirstOrDefault(
            store => store.TypeName == storeTypeName && store.Identifier == storeIdentifier);
    }

    public void RemoveStore(IStore store)
    {
        stores.Remove(store);
    }

    public void Refresh()
    {
    }

    public void StartTransaction()
    {
    }

    public void RollbackTransaction()
    {
    }

    public void CommitTransaction()
    {
    }
}
