using System.Diagnostics;
using System.Globalization;
using System.Text;
using m0;
using m0.Foundation;
using m0.Graph.ExecutionFlow;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

internal static class PasteDeepCopyTimingRunner
{
    private static readonly int[] EdgeCounts = { 1000, 5000, 10000, 30000, 50000 };

    private static readonly CopyShape[] Shapes =
    {
        new CopyShape("star", 0),
        new CopyShape("three-children", 3)
    };

    internal static void Run()
    {
        MinusZero.Instance.SetUserInteraction(new NoOpUserInteraction());
        MinusZero.Instance.Initialize();

        foreach (CopyShape shape in Shapes)
        {
            WriteLine("warmup " + shape.Name + " 100");
            Measure(shape, 100);
        }

        List<TimingSample> samples = new List<TimingSample>();

        foreach (int edgeCount in EdgeCounts)
        {
            foreach (CopyShape shape in Shapes)
            {
                WriteLine("measuring " + shape.Name + " " + edgeCount);
                TimingSample sample = Measure(shape, edgeCount);
                samples.Add(sample);
                WriteLine(
                    "shape=" + sample.ShapeName
                    + " topEdges=" + sample.EdgeCount
                    + " totalEdges=" + sample.TotalEdges
                    + " copyMs=" + sample.CopyMilliseconds
                    + " commitMs=" + sample.CommitMilliseconds
                    + " totalMs=" + sample.TotalMilliseconds);
            }
        }

        string directory = Environment.CurrentDirectory;
        string csvPath = Path.Combine(directory, "paste-deep-copy-timing.csv");
        string htmlPath = Path.Combine(directory, "paste-deep-copy-timing.html");

        File.WriteAllText(csvPath, BuildCsv(samples));
        File.WriteAllText(htmlPath, BuildHtml(samples));

        WriteLine(csvPath);
        WriteLine(htmlPath);

        Process.Start(new ProcessStartInfo(htmlPath) { UseShellExecute = true });
    }

    private static void WriteLine(string line)
    {
        Console.WriteLine(line);
        Console.Out.Flush();
    }

    private static TimingSample Measure(CopyShape shape, int edgeCount)
    {
        WriteLine("building " + shape.Name + " " + edgeCount);

        GraphFixture fixture = new GraphFixture();
        IVertex itemMeta = fixture.CreateVertex("Item");
        IVertex rootMeta = fixture.CreateVertex("Root");
        IVertex hub = fixture.CreateVertex("Hub");
        IVertex parent = fixture.CreateVertex("Parent");
        IEdge rootEdge = parent.AddEdge(rootMeta, hub);
        IReadOnlyList<IEdge> hubEdges = fixture.AddEdges(hub, itemMeta, edgeCount);

        if (shape.ChildEdgesPerItem > 0)
        {
            IVertex childMeta = fixture.CreateVertex("Child");

            foreach (IEdge hubEdge in hubEdges)
                fixture.AddEdges(hubEdge.To, childMeta, shape.ChildEdgesPerItem);
        }

        IVertex destination = fixture.CreateVertex("Destination");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        ExecutionFlowHelper.StartTransaction();
        long copyMilliseconds;
        long commitMilliseconds;

        try
        {
            WriteLine("copying " + shape.Name + " " + edgeCount);
            Stopwatch copyWatch = Stopwatch.StartNew();
            VertexOperations.CopyEdgesSet(new[] { rootEdge }, destination);
            copyWatch.Stop();
            copyMilliseconds = copyWatch.ElapsedMilliseconds;

            WriteLine("committing " + shape.Name + " " + edgeCount);
            Stopwatch commitWatch = Stopwatch.StartNew();
            ExecutionFlowHelper.CommitTransaction();
            commitWatch.Stop();
            commitMilliseconds = commitWatch.ElapsedMilliseconds;
        }
        catch
        {
            if (MinusZero.Instance.GetTopTransaction() != null)
                ExecutionFlowHelper.RollbackTransaction();

            throw;
        }

        VerifyCopiedHub(destination, edgeCount, shape.ChildEdgesPerItem);

        return new TimingSample(
            shape.Name,
            edgeCount,
            shape.TotalEdges(edgeCount),
            copyMilliseconds,
            commitMilliseconds);
    }

    private static void VerifyCopiedHub(
        IVertex destination,
        int edgeCount,
        int childEdgesPerItem)
    {
        if (destination.OutEdgesRaw.Count != 1)
        {
            throw new InvalidOperationException(
                "Destination has " + destination.OutEdgesRaw.Count
                + " outgoing edges, expected the single copied root edge.");
        }

        IVertex copiedHub = destination.OutEdgesRaw[0].To;

        if (copiedHub.OutEdgesRaw.Count != edgeCount)
        {
            throw new InvalidOperationException(
                "Copied hub has " + copiedHub.OutEdgesRaw.Count
                + " outgoing edges, expected " + edgeCount
                + ". The Item meta was treated as a link or the copy stopped early.");
        }

        foreach (IEdge edge in copiedHub.OutEdgesRaw)
        {
            int childEdgeCount = edge.To.OutEdgesRaw.Count;

            if (childEdgeCount != childEdgesPerItem)
            {
                throw new InvalidOperationException(
                    "Copied child has " + childEdgeCount
                    + " outgoing edges, expected " + childEdgesPerItem
                    + ".");
            }
        }
    }

    private static string BuildCsv(IReadOnlyList<TimingSample> samples)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("shape,topEdges,totalEdges,copyMs,commitMs,totalMs");

        foreach (TimingSample sample in samples)
        {
            csv.Append(sample.ShapeName);
            csv.Append(',');
            csv.Append(sample.EdgeCount.ToString(CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(sample.TotalEdges.ToString(CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(sample.CopyMilliseconds.ToString(CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(sample.CommitMilliseconds.ToString(CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.AppendLine(sample.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }

        return csv.ToString();
    }

    private static string BuildHtml(IReadOnlyList<TimingSample> samples)
    {
        const double width = 960;
        const double height = 520;
        const double left = 72;
        const double top = 24;
        const double right = 24;
        const double bottom = 56;
        double plotWidth = width - left - right;
        double plotHeight = height - top - bottom;
        long yMaxMilliseconds = samples.Max(sample => sample.CopyMilliseconds);
        if (yMaxMilliseconds < 1)
            yMaxMilliseconds = 1;
        const double xMaxEdges = 50000;

        StringBuilder svg = new StringBuilder();
        svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"960\" height=\"520\" viewBox=\"0 0 960 520\">");
        svg.Append("<rect width=\"960\" height=\"520\" fill=\"#ffffff\"/>");

        for (int tick = 0; tick <= 4; tick++)
        {
            double fraction = tick / 4.0;
            double y = top + plotHeight - fraction * plotHeight;
            long tickMilliseconds = (long)Math.Round(yMaxMilliseconds * fraction);
            svg.Append("<line x1=\"");
            svg.Append(left.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" y1=\"");
            svg.Append(y.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" x2=\"");
            svg.Append((left + plotWidth).ToString(CultureInfo.InvariantCulture));
            svg.Append("\" y2=\"");
            svg.Append(y.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" stroke=\"#e6e6e6\"/>");
            svg.Append("<text x=\"64\" y=\"");
            svg.Append((y + 4).ToString(CultureInfo.InvariantCulture));
            svg.Append("\" text-anchor=\"end\" font-size=\"12\" fill=\"#444\">");
            svg.Append(tickMilliseconds.ToString(CultureInfo.InvariantCulture));
            svg.Append("</text>");
        }

        foreach (int edgeCount in EdgeCounts)
        {
            double x = left + edgeCount / xMaxEdges * plotWidth;
            svg.Append("<line x1=\"");
            svg.Append(x.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" y1=\"");
            svg.Append(top.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" x2=\"");
            svg.Append(x.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" y2=\"");
            svg.Append((top + plotHeight).ToString(CultureInfo.InvariantCulture));
            svg.Append("\" stroke=\"#f3f3f3\"/>");
            svg.Append("<text x=\"");
            svg.Append(x.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" y=\"");
            svg.Append((top + plotHeight + 22).ToString(CultureInfo.InvariantCulture));
            svg.Append("\" text-anchor=\"middle\" font-size=\"12\" fill=\"#444\">");
            svg.Append(edgeCount.ToString(CultureInfo.InvariantCulture));
            svg.Append("</text>");
        }

        AppendSeries(svg, SamplesForShape(samples, "star"), left, top, plotWidth, plotHeight, xMaxEdges, yMaxMilliseconds, sample => sample.CopyMilliseconds, "#1f77b4");
        AppendSeries(svg, SamplesForShape(samples, "three-children"), left, top, plotWidth, plotHeight, xMaxEdges, yMaxMilliseconds, sample => sample.CopyMilliseconds, "#2ca02c");

        svg.Append("<text x=\"480\" y=\"508\" text-anchor=\"middle\" font-size=\"13\" fill=\"#222\">top edges</text>");
        svg.Append("<text x=\"16\" y=\"250\" text-anchor=\"middle\" font-size=\"13\" fill=\"#222\" transform=\"rotate(-90 16 250)\">milliseconds</text>");
        AppendLegend(svg);
        svg.Append("</svg>");

        StringBuilder html = new StringBuilder();
        html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Paste deep copy timing</title></head><body>");
        html.Append("<h1>Paste subgraph copy time</h1>");
        html.Append("<p>star is one hub with N edges. three-children adds three edges under each of those targets, each to its own vertex, so the copied subgraph has 4N edges.</p>");
        html.Append(svg);
        html.Append("<p>");
        html.Append(FormatRatio(samples));
        html.Append(" From 10000 to 50000 top edges, linear growth is about 5 and quadratic growth is about 25. At the same top-edge count, three-children has 4 times as many edges, so a linear cost is about 4 times star and a cost quadratic in the edge count is about 16 times star.</p>");
        html.Append("<table><tr><th>topEdges</th><th>starCopyMs</th><th>threeChildrenCopyMs</th><th>threeOverStar</th><th>starCommitMs</th><th>threeCommitMs</th></tr>");

        foreach (int edgeCount in EdgeCounts)
        {
            TimingSample star = FindSample(samples, "star", edgeCount);
            TimingSample threeChildren = FindSample(samples, "three-children", edgeCount);
            html.Append("<tr><td>");
            html.Append(edgeCount.ToString(CultureInfo.InvariantCulture));
            html.Append("</td><td>");
            html.Append(star.CopyMilliseconds.ToString(CultureInfo.InvariantCulture));
            html.Append("</td><td>");
            html.Append(threeChildren.CopyMilliseconds.ToString(CultureInfo.InvariantCulture));
            html.Append("</td><td>");
            html.Append(FormatGrowth(threeChildren.CopyMilliseconds, star.CopyMilliseconds));
            html.Append("</td><td>");
            html.Append(star.CommitMilliseconds.ToString(CultureInfo.InvariantCulture));
            html.Append("</td><td>");
            html.Append(threeChildren.CommitMilliseconds.ToString(CultureInfo.InvariantCulture));
            html.Append("</td></tr>");
        }

        html.Append("</table></body></html>");
        return html.ToString();
    }

    private static void AppendSeries(
        StringBuilder svg,
        IReadOnlyList<TimingSample> samples,
        double left,
        double top,
        double plotWidth,
        double plotHeight,
        double xMaxEdges,
        long yMaxMilliseconds,
        Func<TimingSample, long> value,
        string color)
    {
        StringBuilder points = new StringBuilder();

        foreach (TimingSample sample in samples)
        {
            double x = left + sample.EdgeCount / xMaxEdges * plotWidth;
            double y = top + plotHeight - value(sample) / (double)yMaxMilliseconds * plotHeight;

            if (points.Length > 0)
                points.Append(' ');

            points.Append(x.ToString(CultureInfo.InvariantCulture));
            points.Append(',');
            points.Append(y.ToString(CultureInfo.InvariantCulture));
        }

        svg.Append("<polyline fill=\"none\" stroke=\"");
        svg.Append(color);
        svg.Append("\" stroke-width=\"2\" points=\"");
        svg.Append(points);
        svg.Append("\"/>");

        foreach (TimingSample sample in samples)
        {
            double x = left + sample.EdgeCount / xMaxEdges * plotWidth;
            double y = top + plotHeight - value(sample) / (double)yMaxMilliseconds * plotHeight;
            svg.Append("<circle cx=\"");
            svg.Append(x.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" cy=\"");
            svg.Append(y.ToString(CultureInfo.InvariantCulture));
            svg.Append("\" r=\"3.5\" fill=\"");
            svg.Append(color);
            svg.Append("\"/>");
        }
    }

    private static void AppendLegend(StringBuilder svg)
    {
        svg.Append("<rect x=\"88\" y=\"32\" width=\"168\" height=\"52\" fill=\"#ffffff\" stroke=\"#dddddd\"/>");
        AppendLegendRow(svg, 48, "#1f77b4", "star");
        AppendLegendRow(svg, 70, "#2ca02c", "three children");
    }

    private static void AppendLegendRow(StringBuilder svg, double y, string color, string label)
    {
        svg.Append("<line x1=\"100\" y1=\"");
        svg.Append(y.ToString(CultureInfo.InvariantCulture));
        svg.Append("\" x2=\"124\" y2=\"");
        svg.Append(y.ToString(CultureInfo.InvariantCulture));
        svg.Append("\" stroke=\"");
        svg.Append(color);
        svg.Append("\" stroke-width=\"2\"/>");
        svg.Append("<text x=\"132\" y=\"");
        svg.Append((y + 4).ToString(CultureInfo.InvariantCulture));
        svg.Append("\" font-size=\"12\" fill=\"#222\">");
        svg.Append(label);
        svg.Append("</text>");
    }

    private static List<TimingSample> SamplesForShape(
        IReadOnlyList<TimingSample> samples,
        string shapeName)
    {
        return samples.Where(sample => sample.ShapeName == shapeName).ToList();
    }

    private static TimingSample FindSample(
        IReadOnlyList<TimingSample> samples,
        string shapeName,
        int edgeCount)
    {
        return samples.First(
            sample => sample.ShapeName == shapeName && sample.EdgeCount == edgeCount);
    }

    private static string FormatRatio(IReadOnlyList<TimingSample> samples)
    {
        TimingSample starTenThousand = FindSample(samples, "star", 10000);
        TimingSample starFiftyThousand = FindSample(samples, "star", 50000);
        TimingSample threeTenThousand = FindSample(samples, "three-children", 10000);
        TimingSample threeFiftyThousand = FindSample(samples, "three-children", 50000);

        return "star copy(50000) / copy(10000) = "
            + FormatGrowth(starFiftyThousand.CopyMilliseconds, starTenThousand.CopyMilliseconds)
            + ". three-children copy(50000) / copy(10000) = "
            + FormatGrowth(threeFiftyThousand.CopyMilliseconds, threeTenThousand.CopyMilliseconds)
            + ". At 50000 top edges, three-children / star = "
            + FormatGrowth(threeFiftyThousand.CopyMilliseconds, starFiftyThousand.CopyMilliseconds)
            + ".";
    }

    private static string FormatGrowth(long larger, long smaller)
    {
        if (smaller <= 0)
            return "n/a";

        double ratio = larger / (double)smaller;
        return ratio.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private sealed class CopyShape
    {
        internal CopyShape(string name, int childEdgesPerItem)
        {
            Name = name;
            ChildEdgesPerItem = childEdgesPerItem;
        }

        internal string Name { get; }

        internal int ChildEdgesPerItem { get; }

        internal int TotalEdges(int topEdgeCount)
        {
            return topEdgeCount + topEdgeCount * ChildEdgesPerItem;
        }
    }

    private sealed class TimingSample
    {
        internal TimingSample(
            string shapeName,
            int edgeCount,
            int totalEdges,
            long copyMilliseconds,
            long commitMilliseconds)
        {
            ShapeName = shapeName;
            EdgeCount = edgeCount;
            TotalEdges = totalEdges;
            CopyMilliseconds = copyMilliseconds;
            CommitMilliseconds = commitMilliseconds;
        }

        internal string ShapeName { get; }

        internal int EdgeCount { get; }

        internal int TotalEdges { get; }

        internal long CopyMilliseconds { get; }

        internal long CommitMilliseconds { get; }

        internal long TotalMilliseconds => CopyMilliseconds + CommitMilliseconds;
    }
}
