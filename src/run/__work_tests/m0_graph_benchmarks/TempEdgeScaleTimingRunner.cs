using System.Diagnostics;
using System.Globalization;
using System.Text;
using m0;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0_graph_test_support;

namespace m0_graph_benchmarks;

internal static class TempEdgeScaleTimingRunner
{
    private static readonly int[] EdgeCounts =
    {
        1000,
        5000,
        10000,
        30000,
        50000
    };

    private static readonly Series[] SeriesList =
    {
        new Series("new-vertex", false),
        new Series("link", true)
    };

    internal static void Run()
    {
        RecordingUserInteraction userInteraction =
            new RecordingUserInteraction();
        MinusZero.Instance.DoLog = false;
        MinusZero.Instance.SetUserInteraction(userInteraction);
        MinusZero.Instance.Initialize();

        foreach (Series series in SeriesList)
        {
            WriteLine("warmup " + series.Name + " 100");
            Measure(series, 100, userInteraction);
        }

        List<TimingSample> samples = new List<TimingSample>();

        foreach (int edgeCount in EdgeCounts)
        {
            foreach (Series series in SeriesList)
            {
                WriteLine(
                    "measuring " + series.Name + " " + edgeCount);
                TimingSample sample = Measure(
                    series,
                    edgeCount,
                    userInteraction);
                samples.Add(sample);
                WriteLine(
                    "series=" + sample.SeriesName
                    + " edges=" + sample.EdgeCount
                    + " executeMs=" + sample.ExecuteMilliseconds
                    + " commitMs=" + sample.CommitMilliseconds
                    + " totalMs=" + sample.TotalMilliseconds
                    + " coalesceSteps=" + sample.Counters.CoalesceSteps);
                WriteLine(
                    GraphScaleDiagnostics.FormatReport(
                        sample.Counters));
            }
        }

        string directory = Environment.CurrentDirectory;
        string csvPath = Path.Combine(
            directory,
            "temp-edge-scale.csv");
        string htmlPath = Path.Combine(
            directory,
            "temp-edge-scale.html");

        File.WriteAllText(csvPath, BuildCsv(samples));
        File.WriteAllText(htmlPath, BuildHtml(samples));

        WriteLine(csvPath);
        WriteLine(htmlPath);

        try
        {
            Process.Start(
                new ProcessStartInfo(htmlPath)
                {
                    UseShellExecute = true
                });
        }
        catch (Exception openError)
        {
            WriteLine(
                "Could not open the chart: " + openError.Message);
        }
    }

    private static TimingSample Measure(
        Series series,
        int edgeCount,
        RecordingUserInteraction userInteraction)
    {
        IVertex program = ParseProgram(series, edgeCount);
        userInteraction.Clear();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        GraphScaleDiagnostics.Reset();
        GraphScaleDiagnostics.Enabled = true;
        ExecutionFlowHelper.StartTransaction();
        long executeMilliseconds;
        long commitMilliseconds;
        IVertex result;

        try
        {
            Stopwatch executeWatch = Stopwatch.StartNew();
            result = MinusZero.Instance.DefaultExecuter.Execute(
                InstructionHelpers.CreateStack(),
                program);
            executeWatch.Stop();
            executeMilliseconds = executeWatch.ElapsedMilliseconds;

            EnsureNoExecutionErrors(
                userInteraction,
                series,
                edgeCount);
            VerifyAccumulator(result, program, edgeCount);

            Stopwatch commitWatch = Stopwatch.StartNew();
            ExecutionFlowHelper.CommitTransaction();
            commitWatch.Stop();
            commitMilliseconds = commitWatch.ElapsedMilliseconds;
        }
        catch
        {
            GraphScaleDiagnostics.Enabled = false;

            if (MinusZero.Instance.GetTopTransaction() != null)
                ExecutionFlowHelper.RollbackTransaction();

            throw;
        }
        finally
        {
            GraphScaleDiagnostics.Enabled = false;
        }

        GraphScaleSnapshot counters =
            GraphScaleDiagnostics.GetSnapshot();

        if (series.LinkInside &&
            counters.LinkOperations == 0)
        {
            throw new InvalidOperationException(
                "The link program did not execute ::>. Program: "
                + DescribeDirectEdges(program));
        }

        if (!series.LinkInside &&
            counters.NewVertexOperations < edgeCount)
        {
            throw new InvalidOperationException(
                "The new-vertex program executed :: "
                + counters.NewVertexOperations
                + " times, expected at least "
                + edgeCount
                + ".");
        }

        return new TimingSample(
            series.Name,
            edgeCount,
            executeMilliseconds,
            commitMilliseconds,
            counters);
    }

    private static IVertex ParseProgram(Series series, int edgeCount)
    {
        IVertex parseParent = MinusZero.Instance.TempStore.Root
            .AddVertex(
                MinusZero.Instance.Empty,
                "TempEdgeScale-" + series.Name + "-" + edgeCount);
        IEdge sourceEdge = parseParent.AddVertexAndReturnEdge(
            MinusZero.Instance.Empty,
            "Source");
        IVertex parseErrors =
            MinusZero.Instance.DefaultFormalTextParser.Parse(
                sourceEdge,
                BuildSource(series.LinkInside, edgeCount),
                CodeRepresentationEnum.LinearizedManyLines,
                out IEdge parsedRootEdge);

        if (parseErrors != null && parseErrors.Any())
        {
            throw new InvalidOperationException(
                "Temp program failed to parse: " +
                string.Join(
                    "; ",
                    parseErrors.Select(
                        edge => DescribeVertex(edge.To))));
        }

        if (parsedRootEdge?.To == null)
        {
            throw new InvalidOperationException(
                "Temp program did not produce a root edge.");
        }

        return parsedRootEdge.To;
    }

    private static string BuildSource(bool linkInside, int edgeCount)
    {
        string innerOperation = linkInside
            ? "::> x"
            : ":: x";

        return
            "\"Temp\"\r\n"
            + "\tvariable \"x\" @Integer\r\n"
            + "\tvariable \"r\" @Vertex\r\n"
            + "\tx = \"0\"\r\n"
            + "\twhile x < \""
            + edgeCount.ToString(CultureInfo.InvariantCulture)
            + "\"\r\n"
            + "\t\tr +< @@System :: \"kotek\"{\r\n"
            + "\t\t\t"
            + innerOperation
            + "\r\n"
            + "\t\t}\r\n"
            + "\t\tx <- x + \"1\"\r\n";
    }

    private static void VerifyAccumulator(
        IVertex result,
        IVertex program,
        int edgeCount)
    {
        IVertex accumulator = GraphUtil.GetQueryOutFirst(
            result,
            "r",
            null);

        int actualCount = accumulator?.OutEdgesRaw.Count ?? -1;

        if (actualCount != edgeCount)
        {
            throw new InvalidOperationException(
                "Variable r has "
                + actualCount
                + " outgoing edges, expected "
                + edgeCount
                + ". Program: "
                + DescribeDirectEdges(program)
                + ". Result: "
                + DescribeDirectEdges(result));
        }
    }

    private static void EnsureNoExecutionErrors(
        RecordingUserInteraction userInteraction,
        Series series,
        int edgeCount)
    {
        if (userInteraction.Exceptions.Count == 0)
            return;

        throw new InvalidOperationException(
            series.Name
            + " "
            + edgeCount
            + " failed: "
            + string.Join(
                " || ",
                userInteraction.Exceptions.Select(FormatException)));
    }

    private static string DescribeDirectEdges(IVertex vertex)
    {
        if (vertex == null)
            return "<null>";

        StringBuilder description = new StringBuilder();
        description.Append("value=");
        description.Append(vertex.Value);
        description.Append(" count=");
        description.Append(vertex.OutEdgesRaw.Count);

        foreach (IEdge edge in vertex.OutEdgesRaw)
        {
            description.Append(" [");
            description.Append(edge.Meta?.Value);
            description.Append('=');
            description.Append(edge.To?.Value);
            description.Append(" out=");
            description.Append(edge.To?.OutEdgesRaw.Count ?? -1);
            description.Append(']');
        }

        return description.ToString();
    }

    private static string DescribeVertex(IVertex vertex)
    {
        return DescribeVertex(vertex, 0);
    }

    private static string DescribeVertex(IVertex vertex, int depth)
    {
        if (vertex == null)
            return "<null>";

        if (depth > 3)
            return vertex.Value?.ToString() ?? "";

        StringBuilder description = new StringBuilder();
        description.Append(vertex.Value?.ToString() ?? "");

        foreach (IEdge edge in vertex.OutEdgesRaw)
        {
            description.Append(" [");
            description.Append(edge.Meta?.Value);
            description.Append('=');
            description.Append(DescribeVertex(edge.To, depth + 1));
            description.Append(']');
        }

        return description.ToString();
    }

    private static string FormatException(IVertex exception)
    {
        IEnumerable<string> values = exception
            .OutEdgesRaw
            .Select(
                edge =>
                    edge.Meta?.Value + ": " + edge.To?.Value);
        return string.Join(" | ", values);
    }

    private static string BuildCsv(IReadOnlyList<TimingSample> samples)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine(
            "series,edges,executeMs,commitMs,totalMs,"
            + "coalesceOutSteps,coalesceInSteps,coalesceMetaSteps,"
            + "coalesceOutMax,coalesceInMax,coalesceMetaMax,"
            + "outEdgesScanned,inEdgesScanned,addEdge,"
            + "newVertex,link,linkSameTarget");

        foreach (TimingSample sample in samples)
        {
            GraphScaleSnapshot counters = sample.Counters;
            csv.Append(sample.SeriesName);
            csv.Append(',');
            csv.Append(sample.EdgeCount.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(sample.ExecuteMilliseconds.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(sample.CommitMilliseconds.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(sample.TotalMilliseconds.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.CoalesceOutSteps.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.CoalesceInSteps.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.CoalesceMetaSteps.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.CoalesceOutMaxList.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.CoalesceInMaxList.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.CoalesceMetaMaxList.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.OutIndexRebuildEdges.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.InIndexRebuildEdges.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.AddEdgeCalls.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.NewVertexOperations.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.Append(counters.LinkOperations.ToString(
                CultureInfo.InvariantCulture));
            csv.Append(',');
            csv.AppendLine(counters.LinkSameTargetCount.ToString(
                CultureInfo.InvariantCulture));
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
        long yMaxMilliseconds = samples.Max(
            sample => sample.TotalMilliseconds);
        if (yMaxMilliseconds < 1)
            yMaxMilliseconds = 1;
        const double xMaxEdges = 50000;

        StringBuilder svg = new StringBuilder();
        svg.Append(
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"960\" height=\"520\" viewBox=\"0 0 960 520\">");
        svg.Append("<rect width=\"960\" height=\"520\" fill=\"#ffffff\"/>");

        for (int tick = 0; tick <= 4; tick++)
        {
            double fraction = tick / 4.0;
            double y = top + plotHeight - fraction * plotHeight;
            long tickMilliseconds = (long)Math.Round(
                yMaxMilliseconds * fraction);
            svg.Append("<line x1=\"")
                .Append(left.ToString(CultureInfo.InvariantCulture))
                .Append("\" y1=\"")
                .Append(y.ToString(CultureInfo.InvariantCulture))
                .Append("\" x2=\"")
                .Append((left + plotWidth).ToString(
                    CultureInfo.InvariantCulture))
                .Append("\" y2=\"")
                .Append(y.ToString(CultureInfo.InvariantCulture))
                .Append("\" stroke=\"#e6e6e6\"/>");
            svg.Append("<text x=\"64\" y=\"")
                .Append((y + 4).ToString(CultureInfo.InvariantCulture))
                .Append("\" text-anchor=\"end\" font-size=\"12\" fill=\"#444\">")
                .Append(tickMilliseconds.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</text>");
        }

        foreach (int edgeCount in EdgeCounts)
        {
            double x = left + edgeCount / xMaxEdges * plotWidth;
            svg.Append("<line x1=\"")
                .Append(x.ToString(CultureInfo.InvariantCulture))
                .Append("\" y1=\"")
                .Append(top.ToString(CultureInfo.InvariantCulture))
                .Append("\" x2=\"")
                .Append(x.ToString(CultureInfo.InvariantCulture))
                .Append("\" y2=\"")
                .Append((top + plotHeight).ToString(
                    CultureInfo.InvariantCulture))
                .Append("\" stroke=\"#f3f3f3\"/>");
            svg.Append("<text x=\"")
                .Append(x.ToString(CultureInfo.InvariantCulture))
                .Append("\" y=\"")
                .Append((top + plotHeight + 22).ToString(
                    CultureInfo.InvariantCulture))
                .Append("\" text-anchor=\"middle\" font-size=\"12\" fill=\"#444\">")
                .Append(edgeCount.ToString(CultureInfo.InvariantCulture))
                .Append("</text>");
        }

        AppendSeries(
            svg,
            SamplesForSeries(samples, "new-vertex"),
            left,
            top,
            plotWidth,
            plotHeight,
            xMaxEdges,
            yMaxMilliseconds,
            "#1f77b4");
        AppendSeries(
            svg,
            SamplesForSeries(samples, "link"),
            left,
            top,
            plotWidth,
            plotHeight,
            xMaxEdges,
            yMaxMilliseconds,
            "#d62728");

        svg.Append(
            "<text x=\"480\" y=\"508\" text-anchor=\"middle\" font-size=\"13\" fill=\"#222\">edges</text>");
        svg.Append(
            "<text x=\"16\" y=\"250\" text-anchor=\"middle\" font-size=\"13\" fill=\"#222\" transform=\"rotate(-90 16 250)\">milliseconds</text>");
        svg.Append(
            "<rect x=\"88\" y=\"32\" width=\"150\" height=\"52\" fill=\"#ffffff\" stroke=\"#dddddd\"/>");
        AppendLegendRow(svg, 48, "#1f77b4", "new vertex ::");
        AppendLegendRow(svg, 70, "#d62728", "link ::&gt;");
        svg.Append("</svg>");

        StringBuilder html = new StringBuilder();
        html.Append(
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Temp edge scale</title></head><body>");
        html.Append("<h1>Temp loop time by edge count</h1>");
        html.Append("<p>");
        html.Append(FormatRatio(samples));
        html.Append(
            " From 10000 to 50000 edges, linear growth is about 5 and quadratic growth is about 25. Time is execute plus commit.</p>");
        html.Append(svg);
        html.Append(
            "<table><tr><th>edges</th><th>newVertexMs</th><th>linkMs</th><th>linkOverNew</th><th>newExecuteMs</th><th>newCommitMs</th><th>linkExecuteMs</th><th>linkCommitMs</th><th>newCoalesceSteps</th><th>linkCoalesceSteps</th></tr>");

        foreach (int edgeCount in EdgeCounts)
        {
            TimingSample newVertex = FindSample(
                samples,
                "new-vertex",
                edgeCount);
            TimingSample link = FindSample(
                samples,
                "link",
                edgeCount);
            html.Append("<tr><td>")
                .Append(edgeCount.ToString(CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(newVertex.TotalMilliseconds.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(link.TotalMilliseconds.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(FormatGrowth(
                    link.TotalMilliseconds,
                    newVertex.TotalMilliseconds))
                .Append("</td><td>")
                .Append(newVertex.ExecuteMilliseconds.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(newVertex.CommitMilliseconds.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(link.ExecuteMilliseconds.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(link.CommitMilliseconds.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(newVertex.Counters.CoalesceSteps.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td><td>")
                .Append(link.Counters.CoalesceSteps.ToString(
                    CultureInfo.InvariantCulture))
                .Append("</td></tr>");
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
        string color)
    {
        StringBuilder points = new StringBuilder();

        foreach (TimingSample sample in samples)
        {
            double x = left + sample.EdgeCount / xMaxEdges * plotWidth;
            double y = top + plotHeight
                - sample.TotalMilliseconds
                    / (double)yMaxMilliseconds
                    * plotHeight;

            if (points.Length > 0)
                points.Append(' ');

            points.Append(x.ToString(CultureInfo.InvariantCulture));
            points.Append(',');
            points.Append(y.ToString(CultureInfo.InvariantCulture));
        }

        svg.Append("<polyline fill=\"none\" stroke=\"")
            .Append(color)
            .Append("\" stroke-width=\"2\" points=\"")
            .Append(points)
            .Append("\"/>");

        foreach (TimingSample sample in samples)
        {
            double x = left + sample.EdgeCount / xMaxEdges * plotWidth;
            double y = top + plotHeight
                - sample.TotalMilliseconds
                    / (double)yMaxMilliseconds
                    * plotHeight;
            svg.Append("<circle cx=\"")
                .Append(x.ToString(CultureInfo.InvariantCulture))
                .Append("\" cy=\"")
                .Append(y.ToString(CultureInfo.InvariantCulture))
                .Append("\" r=\"3.5\" fill=\"")
                .Append(color)
                .Append("\"/>");
        }
    }

    private static void AppendLegendRow(
        StringBuilder svg,
        double y,
        string color,
        string label)
    {
        svg.Append("<line x1=\"100\" y1=\"")
            .Append(y.ToString(CultureInfo.InvariantCulture))
            .Append("\" x2=\"124\" y2=\"")
            .Append(y.ToString(CultureInfo.InvariantCulture))
            .Append("\" stroke=\"")
            .Append(color)
            .Append("\" stroke-width=\"2\"/>");
        svg.Append("<text x=\"132\" y=\"")
            .Append((y + 4).ToString(CultureInfo.InvariantCulture))
            .Append("\" font-size=\"12\" fill=\"#222\">")
            .Append(label)
            .Append("</text>");
    }

    private static List<TimingSample> SamplesForSeries(
        IReadOnlyList<TimingSample> samples,
        string seriesName)
    {
        return samples
            .Where(sample => sample.SeriesName == seriesName)
            .ToList();
    }

    private static TimingSample FindSample(
        IReadOnlyList<TimingSample> samples,
        string seriesName,
        int edgeCount)
    {
        return samples.First(
            sample => sample.SeriesName == seriesName
                && sample.EdgeCount == edgeCount);
    }

    private static string FormatRatio(IReadOnlyList<TimingSample> samples)
    {
        TimingSample newTen = FindSample(samples, "new-vertex", 10000);
        TimingSample newFifty = FindSample(samples, "new-vertex", 50000);
        TimingSample linkTen = FindSample(samples, "link", 10000);
        TimingSample linkFifty = FindSample(samples, "link", 50000);

        return "new-vertex total(50000) / total(10000) = "
            + FormatGrowth(
                newFifty.TotalMilliseconds,
                newTen.TotalMilliseconds)
            + ". link total(50000) / total(10000) = "
            + FormatGrowth(
                linkFifty.TotalMilliseconds,
                linkTen.TotalMilliseconds)
            + ".";
    }

    private static string FormatGrowth(long larger, long smaller)
    {
        if (smaller <= 0)
            return "n/a";

        double ratio = larger / (double)smaller;
        return ratio.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static void WriteLine(string line)
    {
        Console.WriteLine(line);
        Console.Out.Flush();
    }

    private sealed class Series
    {
        internal Series(string name, bool linkInside)
        {
            Name = name;
            LinkInside = linkInside;
        }

        internal string Name { get; }

        internal bool LinkInside { get; }
    }

    private sealed class TimingSample
    {
        internal TimingSample(
            string seriesName,
            int edgeCount,
            long executeMilliseconds,
            long commitMilliseconds,
            GraphScaleSnapshot counters)
        {
            SeriesName = seriesName;
            EdgeCount = edgeCount;
            ExecuteMilliseconds = executeMilliseconds;
            CommitMilliseconds = commitMilliseconds;
            Counters = counters;
        }

        internal string SeriesName { get; }

        internal int EdgeCount { get; }

        internal long ExecuteMilliseconds { get; }

        internal long CommitMilliseconds { get; }

        internal GraphScaleSnapshot Counters { get; }

        internal long TotalMilliseconds =>
            ExecuteMilliseconds + CommitMilliseconds;
    }

    private sealed class RecordingUserInteraction : NoOpUserInteraction
    {
        internal List<IVertex> Exceptions { get; } = new List<IVertex>();

        public override void InteractionOutputException(IVertex exception)
        {
            Exceptions.Add(exception);
        }

        internal void Clear()
        {
            Exceptions.Clear();
        }
    }
}
