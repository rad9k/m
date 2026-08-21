using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using m0;
using m0_graph_test_support;

namespace m0_desktop_tests;

public sealed class DesktopBootstrapFixture
{
    public DesktopBootstrapFixture()
    {
        if (MinusZero.Instance.IsInitialized)
            return;

        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(
            new NoOpUserInteraction());
        MinusZero.Instance.Initialize();
    }
}

[CollectionDefinition(
    Name,
    DisableParallelization = true)]
public sealed class DesktopGraphCollection :
    ICollectionFixture<DesktopBootstrapFixture>
{
    public const string Name =
        "Bootstrapped desktop graph";
}

internal static class StaTestHost
{
    private static readonly BlockingCollection<Action>
        workItems = new();

    private static readonly ManualResetEventSlim
        startupCompleted = new();

    private static ExceptionDispatchInfo?
        startupException;

    private static readonly Thread workerThread =
        CreateWorkerThread();

    internal static void Run(Action action)
    {
        startupCompleted.Wait();
        startupException?.Throw();

        ExceptionDispatchInfo? capturedException = null;
        using var completed = new ManualResetEventSlim();

        workItems.Add(
            () =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    capturedException =
                        ExceptionDispatchInfo.Capture(
                            exception);
                }
                finally
                {
                    completed.Set();
                }
            });

        completed.Wait();
        capturedException?.Throw();
    }

    private static Thread CreateWorkerThread()
    {
        var thread = new Thread(
            () =>
            {
                try
                {
                    var application =
                        new Application();
                    application.Resources
                        .MergedDictionaries.Add(
                            new ResourceDictionary
                            {
                                Source = new Uri(
                                    "/m0_desktop;component/ResourceDictionaryWhite.xaml",
                                    UriKind.Relative)
                            });
                }
                catch (Exception exception)
                {
                    startupException =
                        ExceptionDispatchInfo.Capture(
                            exception);
                    return;
                }
                finally
                {
                    startupCompleted.Set();
                }

                foreach (var workItem in
                    workItems.GetConsumingEnumerable())
                {
                    workItem();
                }
            });

        thread.IsBackground = true;
        thread.Name = "m0 desktop test STA";
        thread.SetApartmentState(
            ApartmentState.STA);
        thread.Start();
        return thread;
    }
}
