using m0.ZeroCode.Helpers;
using m0_graph_test_support;

namespace m0_graph_tests;

public sealed class InstructionHelpersContractTests
{
    [Theory]
    [InlineData("0.0000000001", true)]
    [InlineData("0", false)]
    [InlineData("-1", false)]
    public void DecimalBooleanConversionMatchesVertexTruth(
        string value,
        bool expected)
    {
        var fixture = new GraphFixture();
        var vertex = fixture.CreateVertex(
            decimal.Parse(
                value,
                System.Globalization.CultureInfo
                    .InvariantCulture));

        Assert.Equal(
            expected,
            InstructionHelpers.IsTrue_Vertex(vertex));
        Assert.Equal(
            expected
                ? InstructionHelpers.BooleanEnum.True
                : InstructionHelpers.BooleanEnum.False,
            InstructionHelpers.GetBolleanValue(vertex));
    }
}
