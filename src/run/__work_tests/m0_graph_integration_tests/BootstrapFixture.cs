using m0;
using m0_graph_test_support;

namespace m0_graph_integration_tests;

public sealed class BootstrapFixture
{
    public BootstrapFixture()
    {
        if (MinusZero.Instance.IsInitialized)
            return;

        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(new NoOpUserInteraction());
        MinusZero.Instance.Initialize();
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class BootstrappedGraphCollection : ICollectionFixture<BootstrapFixture>
{
    public const string Name = "Bootstrapped graph";
}
