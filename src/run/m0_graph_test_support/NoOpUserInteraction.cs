using m0.Foundation;
using m0.ZeroTypes;

namespace m0_graph_test_support;

public sealed class NoOpUserInteraction : IUserInteraction
{
    public PlatformTypeEnum GetPlatformType()
    {
        return PlatformTypeEnum.Console;
    }

    public void ShowContent(object obj)
    {
    }

    public void ShowContentFloating(object obj, FloatingWindowSize size)
    {
    }

    public void CloseWindowByContent(object obj)
    {
    }

    public void EditEdge(IVertex baseVertex)
    {
    }

    public void InteractionOutputException(IVertex exception)
    {
    }

    public void InteractionOutput(string info)
    {
    }

    public IVertex? InteractionSelect(IVertex info, IList<IEdge> options, bool firstSelected)
    {
        return options.FirstOrDefault()?.To;
    }

    public IVertex? InteractionSelectButton(IVertex info, IList<IEdge> options)
    {
        return options.FirstOrDefault()?.To;
    }

    public string InteractionInput(string question)
    {
        return string.Empty;
    }

    public void OpenDefaultVisualiser(IVertex baseVertex, bool isFloating)
    {
    }

    public void OpenVisualiser(IVertex baseVertex, IVertex inputVertex, bool isFloating)
    {
    }

    public void OpenCodeVisualiser(IVertex baseVertex, bool isFloating)
    {
    }

    public void OpenFormVisualiser(IVertex baseVertex, bool isFloating)
    {
    }

    public void UserInteractionInitialize()
    {
    }

    public void UserInteractionFinalize()
    {
    }

    public bool TypedEdge_Get_Test(Type[] interfacesInToCreateType)
    {
        return false;
    }
}
