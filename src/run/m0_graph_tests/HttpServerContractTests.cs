using m0.Lib.Net;
using m0.Network.Server;

namespace m0_graph_tests;

public sealed class HttpServerContractTests
{
    [Theory]
    [InlineData("OPTIONS")]
    [InlineData("options")]
    public void OptionsRequestKeepsOptionsAction(
        string method)
    {
        Assert.Equal(
            HttpActionEnum.OPTIONS,
            HttpServer.GetHttpAction(method));
    }
}
