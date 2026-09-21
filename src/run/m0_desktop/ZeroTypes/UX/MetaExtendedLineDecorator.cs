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
            if (MetaDiagramItem == null ||
                CurrentRoute == null ||
                CurrentRoute.Segments.Count == 0 ||
                OwningVisualiser == null ||
                OwningVisualiser.Canvas == null)
            {
                MetaLine.Points = new PointCollection();
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

            if ((metaAnchor - routeMiddle).Length > 0.001)
            {
                MetaLine.Points = new PointCollection
                {
                    routeMiddle,
                    metaAnchor
                };
            }
            else
                MetaLine.Points = new PointCollection();

            UpdateLabelVisibility();
        }
        
        public override void AddToCanvas()
        {
            IEdge baseEdge = BaseEdge;
            IUXItem previousMetaDiagramItem = MetaDiagramItem;
            MetaDiagramItem = null;

            OwningVisualiser.Canvas.Children.Add(LineEndings);
            OwningVisualiser.Canvas.Children.Add(Line);

            if (baseEdge.Meta != MinusZero.Instance.Empty)
            {
                Dictionary<IVertex, List<IUXItem>> itemsByBaseEdgeTo =
                    OwningVisualiser.GetItemsDictionaryByBaseEdgeTo();
                if (itemsByBaseEdgeTo.TryGetValue(
                    baseEdge.Meta,
                    out List<IUXItem> matchingItems))
                {
                    MetaDiagramItem = matchingItems.FirstOrDefault();
                }
            }

            if (previousMetaDiagramItem != MetaDiagramItem
                && previousMetaDiagramItem is UXItem previousMetaItem)
            {
                previousMetaItem.RemoveAsToMetaLine(this);
            }

            if (MetaDiagramItem != null &&
                MetaLine.Points != null &&
                MetaLine.Points.Count >= 2)
            {
                OwningVisualiser.Canvas.Children.Add(MetaLine);
                MetaDiagramItem.AddAsToMetaLine(this);
            }
            else
            {
                if (!HideLabel)
                    OwningVisualiser.Canvas.Children.Add(Label);
            }
            
            VertexSetedUp();
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
    }
}
