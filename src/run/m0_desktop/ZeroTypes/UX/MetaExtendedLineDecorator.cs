using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0.ZeroTypes.UX
{
    public class MetaExtendedLineDecorator : LineDecorator
    {        
        public IUXItem MetaDiagramItem;

        protected ArrowPolyline MetaLine = new ArrowPolyline();

        public MetaExtendedLineDecorator(IEdge edge) : base(edge)
        {
            MetaLine.IsEndings = false;
            MetaLine.StrokeThickness = 1;
            MetaLine.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");
            MetaLine.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
        }        
        
        public override void SetPosition(double _FromX, double _FromY, double _ToX, double _ToY, bool isSelfRelation, double selfRelationX, double selfRelationY)
        {
            base.SetPosition(
                _FromX,
                _FromY,
                _ToX,
                _ToY,
                isSelfRelation,
                selfRelationX,
                selfRelationY);

            UpdateMetaLinePosition();
        }

        public override void UpdateMetaPosition()
        {
            base.UpdateMetaPosition();

            UpdateMetaLinePosition();
        }

        void UpdateMetaLinePosition()
        {
            if (MetaDiagramItem == null)
            {
                MetaLine.Points = new PointCollection();
                LogMetaLine(
                    "MetaExtendedLineDecorator.UpdateMetaLinePosition",
                    "MetaDiagramItem == null");
                return;
            }

            if (CurrentRoute == null)
            {
                MetaLine.Points = new PointCollection();
                LogMetaLine(
                    "MetaExtendedLineDecorator.UpdateMetaLinePosition",
                    "CurrentRoute == null");
                return;
            }

            if (CurrentRoute.Segments.Count == 0)
            {
                MetaLine.Points = new PointCollection();
                LogMetaLine(
                    "MetaExtendedLineDecorator.UpdateMetaLinePosition",
                    "CurrentRoute.Segments.Count == 0");
                return;
            }

            if (OwningVisualiser == null ||
                OwningVisualiser.Canvas == null)
            {
                MetaLine.Points = new PointCollection();
                LogMetaLine(
                    "MetaExtendedLineDecorator.UpdateMetaLinePosition",
                    "OwningVisualiser or Canvas == null");
                return;
            }

            Point routeMiddle;
            Vector routeTangent;
            CurrentRoute.GetPointAndTangentAtFraction(
                0.5,
                out routeMiddle,
                out routeTangent);

            Rect metaBounds;
            Point metaAnchor = routeMiddle;
            string clearReason = null;

            if (DiagramLineRouter.TryGetVisibleBounds(
                    MetaDiagramItem,
                    OwningVisualiser.Canvas,
                    out metaBounds))
            {
                if (metaBounds.Contains(routeMiddle))
                {
                    IReadOnlyList<Point> flattenedPoints =
                        CurrentRoute.FlattenedPoints;
                    int middleIndex =
                        flattenedPoints.Count / 2;
                    bool foundOutsidePoint = false;

                    for (int offset = 0;
                        offset < flattenedPoints.Count;
                        offset++)
                    {
                        int beforeIndex =
                            middleIndex - offset;
                        int afterIndex =
                            middleIndex + offset;

                        if (beforeIndex >= 0 &&
                            !metaBounds.Contains(
                                flattenedPoints[beforeIndex]))
                        {
                            routeMiddle =
                                flattenedPoints[beforeIndex];
                            foundOutsidePoint = true;
                            break;
                        }

                        if (afterIndex <
                                flattenedPoints.Count &&
                            !metaBounds.Contains(
                                flattenedPoints[afterIndex]))
                        {
                            routeMiddle =
                                flattenedPoints[afterIndex];
                            foundOutsidePoint = true;
                            break;
                        }
                    }

                    if (!foundOutsidePoint)
                    {
                        MetaLine.Points =
                            new PointCollection();
                        UpdateLabelVisibility();
                        LogMetaLine(
                            "MetaExtendedLineDecorator.UpdateMetaLinePosition",
                            "foundOutsidePoint == false bounds="
                                + FormatRect(metaBounds));
                        return;
                    }
                }

                Point metaCenter = new Point(
                    metaBounds.Left + metaBounds.Width / 2,
                    metaBounds.Top + metaBounds.Height / 2);
                Vector direction = metaCenter - routeMiddle;

                if (direction.Length > 0.001)
                    metaAnchor =
                        MetaDiagramItem.GetLineEdgeIntersection(
                            routeMiddle,
                            direction);
            }
            else
            {
                clearReason = "TryGetVisibleBounds == false "
                    + DescribeMetaItemLayout(MetaDiagramItem);
            }

            if ((metaAnchor - routeMiddle).Length > 0.001)
            {
                MetaLine.Points = new PointCollection
                {
                    routeMiddle,
                    metaAnchor
                };
                UpdateLabelVisibility();
                LogMetaLine(
                    "MetaExtendedLineDecorator.UpdateMetaLinePosition",
                    "points set "
                        + FormatPoint(routeMiddle)
                        + " -> "
                        + FormatPoint(metaAnchor));
                return;
            }

            MetaLine.Points = new PointCollection();
            UpdateLabelVisibility();
            if (clearReason == null)
            {
                clearReason = "metaAnchor distance <= 0.001 routeMiddle="
                    + FormatPoint(routeMiddle)
                    + " metaAnchor="
                    + FormatPoint(metaAnchor);
            }

            LogMetaLine(
                "MetaExtendedLineDecorator.UpdateMetaLinePosition",
                clearReason);
        }
        
        public override void AddToCanvas()
        {
            IEdge baseEdge = BaseEdge;
            IUXItem previousMetaDiagramItem = MetaDiagramItem;
            MetaDiagramItem = null;

            OwningVisualiser.Canvas.Children.Add(LineEndings);
            OwningVisualiser.Canvas.Children.Add(Line);
            OwningVisualiser.Canvas.Children.Add(
                CardinalityLabel);

            string lookupDetail;

            if (baseEdge.Meta != MinusZero.Instance.Empty)
            {
                Dictionary<IVertex, List<IUXItem>> itemsByBaseEdgeTo =
                    OwningVisualiser.GetItemsDictionaryByBaseEdgeTo();
                if (itemsByBaseEdgeTo.TryGetValue(
                    baseEdge.Meta,
                    out List<IUXItem> matchingItems))
                {
                    MetaDiagramItem = matchingItems.FirstOrDefault();
                    IVertex chosenBaseEdgeTo = MetaDiagramItem == null
                        ? null
                        : MetaDiagramItem.BaseEdgeTo;
                    lookupDetail = "hit count=" + matchingItems.Count
                        + " chosen=" + (MetaDiagramItem == null
                            ? "null"
                            : MetaDiagramItem.GetType().Name)
                        + " chosenBaseEdgeTo=" + DescribeVertex(chosenBaseEdgeTo)
                        + " refEq=" + object.ReferenceEquals(
                            chosenBaseEdgeTo,
                            baseEdge.Meta);
                }
                else
                {
                    lookupDetail = "miss "
                        + DescribeSameValueDictionaryKeys(
                            itemsByBaseEdgeTo,
                            baseEdge.Meta);
                }
            }
            else
                lookupDetail = "meta is Empty, lookup skipped";

            if (previousMetaDiagramItem != MetaDiagramItem
                && previousMetaDiagramItem is UXItem previousMetaItem)
            {
                previousMetaItem.RemoveAsToMetaLine(this);
            }

            int pointsAtDecision = MetaLine.Points == null
                ? 0
                : MetaLine.Points.Count;
            string metaDiagramItemState = MetaDiagramItem == null
                ? "null"
                : "set";
            string branch;

            if (MetaDiagramItem != null &&
                MetaLine.Points != null &&
                MetaLine.Points.Count >= 2)
            {
                OwningVisualiser.Canvas.Children.Add(MetaLine);
                MetaDiagramItem.AddAsToMetaLine(this);
                branch = "added MetaLine pointsAtDecision="
                    + pointsAtDecision;
            }
            else
            {
                if (!HideLabel)
                    OwningVisualiser.Canvas.Children.Add(Label);

                branch = (!HideLabel ? "added Label" : "label hidden")
                    + " pointsAtDecision=" + pointsAtDecision
                    + " metaDiagramItem=" + metaDiagramItemState;
            }
            
            VertexSetedUp();
            LogMetaLine(
                "MetaExtendedLineDecorator.AddToCanvas",
                lookupDetail + " " + branch);
        }

        protected override void UpdateLabelVisibility()
        {
            if (MetaDiagramItem != null)
            {
                if (OwningVisualiser != null &&
                    OwningVisualiser.Canvas != null)
                {
                    OwningVisualiser.Canvas.Children.Remove(Label);
                }

                return;
            }

            base.UpdateLabelVisibility();
        }

        public override double GetMouseDistance(Point point)
        {
            double minimumDistance =
                base.GetMouseDistance(point);

            if (MetaLine.Points == null ||
                MetaLine.Points.Count < 2)
            {
                return minimumDistance;
            }

            for (int pointIndex = 0;
                pointIndex < MetaLine.Points.Count - 1;
                pointIndex++)
            {
                minimumDistance = Math.Min(
                    minimumDistance,
                    GetPointToSegmentDistance(
                        point,
                        MetaLine.Points[pointIndex],
                        MetaLine.Points[pointIndex + 1]));
            }

            return minimumDistance;
        }

        static double GetPointToSegmentDistance(
            Point point,
            Point segmentStart,
            Point segmentEnd)
        {
            Vector segment = segmentEnd - segmentStart;
            double squaredLength =
                segment.X * segment.X +
                segment.Y * segment.Y;

            if (squaredLength <= 0.000001)
                return (point - segmentStart).Length;

            Vector fromStart = point - segmentStart;
            double projection =
                (fromStart.X * segment.X +
                 fromStart.Y * segment.Y) /
                squaredLength;
            projection = Math.Max(
                0,
                Math.Min(1, projection));

            Point closest = new Point(
                segmentStart.X +
                    segment.X * projection,
                segmentStart.Y +
                    segment.Y * projection);

            return (point - closest).Length;
        }

        public override void Dispose()
        {
            if (!IsDisposed && MetaDiagramItem is UXItem metaItem)
                metaItem.RemoveAsToMetaLine(this);

            MetaDiagramItem = null;
            base.Dispose();
        }
        
        public override void RemoveFromCanvas()
        {
            if (OwningVisualiser == null || OwningVisualiser.Canvas == null)
                return;

            OwningVisualiser.Canvas.Children.Remove(MetaLine);
            OwningVisualiser.Canvas.Children.Remove(LineEndings);
            OwningVisualiser.Canvas.Children.Remove(Line);
            OwningVisualiser.Canvas.Children.Remove(Label);
            OwningVisualiser.Canvas.Children.Remove(
                CardinalityLabel);
        }

        protected override void UpdateLine()
        {
            base.UpdateLine();

            MetaLine.StrokeThickness = LineWidth;
        }

        protected override void UpdateLineEnds()
        {
            base.UpdateLineEnds();

            Brush foregroundBrush = GetForegroundBrush();

            MetaLine.Stroke = foregroundBrush;
        }

        public override void Highlight()
        {
            base.Highlight();

            MetaLine.Stroke = (Brush)LineEndings.FindResource("0LightHighlightBrush");

            Panel.SetZIndex(MetaLine, 99999);
        }

        public override void Unhighlight()
        {
            base.Unhighlight();

            Brush foregroundBrush = GetForegroundBrush();

            MetaLine.Stroke = foregroundBrush;

            Panel.SetZIndex(MetaLine, 0);
        }

        void LogMetaLine(string where, string what)
        {
            MinusZero.Instance.Log(
                1,
                where,
                DescribeBaseEdge()
                    + " "
                    + what
                    + " "
                    + DescribeMetaLineCanvasState());
        }

        string DescribeBaseEdge()
        {
            Edge baseEdge = BaseEdge;
            if (baseEdge == null)
                return "baseEdge=null";

            return "meta " + DescribeVertex(baseEdge.Meta)
                + " from " + DescribeVertex(baseEdge.From)
                + " to " + DescribeVertex(baseEdge.To);
        }

        static string DescribeVertex(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            string value = vertex.Value == null
                ? "null"
                : vertex.Value.ToString();
            string identifier = vertex.Identifier == null
                ? "null"
                : vertex.Identifier.ToString();

            return "\"" + value + "\" id=" + identifier;
        }

        string DescribeMetaLineCanvasState()
        {
            int pointCount = MetaLine.Points == null
                ? 0
                : MetaLine.Points.Count;
            bool onCanvas = false;

            if (OwningVisualiser != null &&
                OwningVisualiser.Canvas != null)
            {
                onCanvas = OwningVisualiser.Canvas.Children.Contains(
                    MetaLine);
            }

            string stroke = MetaLine.Stroke == null
                ? "null"
                : MetaLine.Stroke.ToString();

            return "points=" + pointCount
                + " onCanvas=" + onCanvas
                + " thickness=" + MetaLine.StrokeThickness.ToString("0.##")
                + " stroke=" + stroke
                + " z=" + Panel.GetZIndex(MetaLine);
        }

        static string DescribeSameValueDictionaryKeys(
            Dictionary<IVertex, List<IUXItem>> itemsByBaseEdgeTo,
            IVertex metaVertex)
        {
            string metaValue = metaVertex == null || metaVertex.Value == null
                ? null
                : metaVertex.Value.ToString();
            int sameValueCount = 0;
            StringBuilder sameValueKeys = new StringBuilder();

            foreach (IVertex key in itemsByBaseEdgeTo.Keys)
            {
                string keyValue = key == null || key.Value == null
                    ? null
                    : key.Value.ToString();

                if (!string.Equals(
                    metaValue,
                    keyValue,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                sameValueCount++;
                if (sameValueCount > 8)
                    continue;

                if (sameValueKeys.Length > 0)
                    sameValueKeys.Append("; ");

                sameValueKeys.Append(DescribeVertex(key));
                sameValueKeys.Append(" refEq=");
                sameValueKeys.Append(
                    object.ReferenceEquals(key, metaVertex));
            }

            return "dictCount=" + itemsByBaseEdgeTo.Count
                + " sameValueKeys=" + sameValueCount
                + " [" + sameValueKeys + "]";
        }

        static string DescribeMetaItemLayout(IUXItem item)
        {
            FrameworkElement element = item as FrameworkElement;
            if (element == null)
            {
                return "not FrameworkElement type="
                    + (item == null ? "null" : item.GetType().Name);
            }

            return "type=" + element.GetType().Name
                + " ActualWidth=" + element.ActualWidth.ToString("0.##")
                + " ActualHeight=" + element.ActualHeight.ToString("0.##")
                + " IsVisible=" + element.IsVisible
                + " Visibility=" + element.Visibility;
        }

        static string FormatPoint(Point point)
        {
            return point.X.ToString("0.##")
                + ","
                + point.Y.ToString("0.##");
        }

        static string FormatRect(Rect rect)
        {
            return FormatPoint(rect.TopLeft)
                + " "
                + rect.Width.ToString("0.##")
                + "x"
                + rect.Height.ToString("0.##");
        }
    }
}
