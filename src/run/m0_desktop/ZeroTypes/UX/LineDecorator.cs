using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf;
using m0.UIWpf.Controls;
using m0.UIWpf.UX;
using m0.Util;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static m0.Graph.ExecutionFlow.ExecutionFlowHelper;

namespace m0.ZeroTypes.UX
{
    public class LineDecorator: LineDecoratorBase
    {
        protected ArrowPolyline LineEndings = new ArrowPolyline();
        protected Path Line = new Path();

        protected TextBlock Label = new TextBlock();

        double currentSelfRelationX;
        double currentSelfRelationY;

        IEdge graphChangeListenerEdge;

        public override void VertexSetedUp()
        {
            UpdateLineEnds();
            VertexUpdated();
            UpdateLabelVisibility();

            if (graphChangeListenerEdge != null)
                return;

            if (OwningVisualiser is UXVisualiser uxVisualiser
                && uxVisualiser.TryDeferLineDecoratorListenerRegistration(this))
            {
                return;
            }

            RegisterGraphChangeListener();
        }

        internal void RegisterDeferredGraphChangeListener()
        {
            RegisterGraphChangeListener();
        }

        void RegisterGraphChangeListener()
        {
            if (graphChangeListenerEdge != null)
                return;

            graphChangeListenerEdge = ExecutionFlowHelper.AddTriggerAndListener(Vertex,
                 new List<string>
                 {
                     @"BaseEdge:",
                     @"BaseEdge:\From:",
                     @"BaseEdge:\Meta:",
                     @"BaseEdge:\To:",
                     @"StartAnchor:",
                     @"EndAnchor:",
                     @"IsDashed:",
                     @"LineWidth:",
                     @"HideLabel:",
                     @"ConstantLabel:",
                     @"BackgroundColor:",
                     @"BackgroundColor:\",
                     @"ForegroundColor:",
                     @"ForegroundColor:\"
                 },
                 new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                         GraphChangeFilterEnum.OutputEdgeAdded,
                         GraphChangeFilterEnum.OutputEdgeRemoved,
                        GraphChangeFilterEnum.OutputEdgeDisposed},
                "DiagramLine",
                VertexChange);
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                GraphChangeTrigger.RemoveListener(graphChangeListenerEdge);
                graphChangeListenerEdge = null;
            }

            base.Dispose();
        }
     
        public LineDecorator(IEdge _edge) : base(_edge)
        {
            LineEndings.IsEndings = true;
            LineEndings.StrokeThickness = 1;
            LineEndings.Stroke = GetForegroundBrush();

            LineEndings.ArrowLength = 15;
            LineEndings.ArrowAngle = 60;

            Line.StrokeThickness = 1;
            Line.Stroke = GetForegroundBrush();

            Label.Foreground = GetForegroundBrush();

            Panel.SetZIndex(Label, 99999);
            Panel.SetZIndex(LineEndings, 99999);
            Panel.SetZIndex(Line, 99999);
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (ShouldRemoveLineAfterBaseEdgeTargetDisposed(exe.Stack))
            {
                if (FromDiagramItem != null)
                    FromDiagramItem.RemoveDiagramLine(this);

                return exe.Stack;
            }

            bool willCallUpdateLine =
                   IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "IsDashed")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "LineWidth");

            if (willCallUpdateLine)
                UpdateLine();

            bool needToUpdateLineEnds = false;

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "StartAnchor")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "EndAnchor"))
                needToUpdateLineEnds = true;

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "BackgroundColor")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "ForegroundColor"))
                needToUpdateLineEnds = true;

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Red")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Green")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Blue")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Opacity"))
                needToUpdateLineEnds = true;

            if (needToUpdateLineEnds)
                UpdateLineEnds();

            bool willCallVertexUpdated =
                   IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:"))
                || IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:\To:"));

            if (willCallVertexUpdated)
                VertexUpdated();

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "HideLabel"))
                UpdateLabelVisibility();

            return exe.Stack;
        }

        private bool ShouldRemoveLineAfterBaseEdgeTargetDisposed(IVertex stack)
        {
            if (FromDiagramItem == null || IsDisposed)
                return false;

            IVertex edgeStub = Vertex != null ? Vertex.Get(false, "BaseEdge:") : null;
            if (edgeStub == null)
                return false;

            IVertex baseEdgeFrom = edgeStub.Get(false, "From:");
            IVertex baseEdgeMeta = edgeStub.Get(false, "Meta:");
            IVertex baseEdgeTo = edgeStub.Get(false, "To:");

            if (baseEdgeTo != null && baseEdgeTo.DisposedState != DisposeStateEnum.Live)
                return true;

            foreach (IEdge ev in stack.GetAll(false, "event:"))
            {
                IVertex eventVertex = ev.To;
                IVertex type = eventVertex.Get(false, "Type:");
                if (type == null)
                    continue;

                bool isRelevantEventType =
                    GraphUtil.GetValueAndCompareStrings(type, "OutputEdgeRemoved")
                    || GraphUtil.GetValueAndCompareStrings(type, "OutputEdgeDisposed");

                if (!isRelevantEventType)
                    continue;

                IVertex edge = eventVertex.Get(false, "Edge:");
                if (edge == null)
                    continue;

                IVertex edgeFrom = edge.Get(false, "From:");
                IVertex edgeMeta = edge.Get(false, "Meta:");
                IVertex edgeTo = edge.Get(false, "To:");

                if (edgeFrom == edgeStub && GraphUtil.GetValueAndCompareStrings(edgeMeta, "To"))
                    return true;

                if (baseEdgeFrom != null
                    && baseEdgeMeta != null
                    && baseEdgeTo != null
                    && edgeFrom == baseEdgeFrom
                    && edgeMeta == baseEdgeMeta
                    && edgeTo == baseEdgeTo)
                    return true;
            }

            return false;
        }

        protected virtual void UpdateLine()
        {
            double thickness = LineWidth;

            if (thickness == 0)
                thickness = 1;

            Line.StrokeThickness = thickness;
            LineEndings.StrokeThickness = thickness;

            if (IsDashed)            
                Line.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
            else
                Line.StrokeDashArray = null;

            if (CurrentRoute != null)
            {
                SetPosition(
                    FromX,
                    FromY,
                    ToX,
                    ToY,
                    isSelfRelation,
                    currentSelfRelationX,
                    currentSelfRelationY);
            }
        }

        protected virtual void UpdateLabelVisibility()
        {
            if (HideLabel)
                OwningVisualiser.Canvas.Children.Remove(Label);
            else
                if (!OwningVisualiser.Canvas.Children.Contains(Label))
                {
                    OwningVisualiser.Canvas.Children.Add(Label);
                }
        }

        protected virtual void UpdateLineEnds()
        {
            Brush backgroundBrush = GetBackgroundBrush();
            Brush foregroundBrush = GetForegroundBrush();
            FillBrush = null;
            HighlightFillBrush = null;

            LineEndings.Stroke = foregroundBrush;
            Line.Stroke = foregroundBrush;
            Label.Foreground = foregroundBrush;

            LineEndings.StartEnding = StartAnchor;

            LineEndings.EndEnding = EndAnchor;

            if (StartAnchor == LineEndEnum.Triangle)
            {                
                FillBrush = backgroundBrush;
                HighlightFillBrush = backgroundBrush;
            }

            if (EndAnchor == LineEndEnum.Triangle)
            {                
                FillBrush = backgroundBrush;
                HighlightFillBrush = backgroundBrush;
            }

            if (StartAnchor == LineEndEnum.FilledTriangle)
            {                
                FillBrush = foregroundBrush;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (EndAnchor == LineEndEnum.FilledTriangle)
            {                
                FillBrush = foregroundBrush;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (StartAnchor == LineEndEnum.Diamond)
            {                
                FillBrush = backgroundBrush;
                HighlightFillBrush = backgroundBrush;
            }

            if (EndAnchor == LineEndEnum.Diamond)
            {                
                FillBrush = backgroundBrush;
                HighlightFillBrush = backgroundBrush;
            }

            if (StartAnchor == LineEndEnum.FilledDiamond)
            {                
                FillBrush = foregroundBrush;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (EndAnchor == LineEndEnum.FilledDiamond)
            {                
                FillBrush = foregroundBrush;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }


            if (FillBrush != null)
                LineEndings.Fill = FillBrush;

            LineEndings.ArrowLength = 0;
            LineEndings.ArrowLength = 15;

            // Anchor type may have just become CrowFoot without items moving;
            // refresh the side prong tips so DefiningGeometry sees them on the
            // very next render.
            UpdateRenderedRoute();
        }

        Brush FillBrush = null;
        Brush HighlightFillBrush = null;

        private void VertexUpdated()
        {
            if (GraphUtil.GetValueAndCompareStrings(UXTemplate.Vertex, "Inheritence")) // not to display "$Inherits"                 
                return;

            IEdge baseEdge = BaseEdge;

            if (baseEdge.Meta == null) // during disposing
                return;

            string constantLabel = ConstantLabel;

            if (constantLabel != null)
            {
                Label.Text = constantLabel;
                UpdateLabelPosition();
                return;
            }

            if (baseEdge.Meta.Get(false, "$VertexTarget:") != null
                //&& !((UXDecoratorTemplate)UXTemplate).CreateEdgeOnly) // ZZZ       
                && ((UXDecoratorTemplate)UXTemplate).EdgeTargetInEdgePointingToTargetItemBaseEdgeTo) // ZZZ       
            {
                IVertex v = baseEdge.To;

                if (v.Value != null && !GeneralUtil.CompareStrings(v.Value, "$Empty"))
                    Label.Text = (string)v.Value;
            }
            else
            {
                IVertex v = baseEdge.Meta;

                if (v.Value != null && !GeneralUtil.CompareStrings(v.Value, "$Empty"))
                    Label.Text = (string)v.Value;
            }

            UpdateLabelPosition();
        }

        public override void SetPosition(double _FromX, double _FromY, double _ToX, double _ToY, bool _isSelfRelation, double selfRelationX, double selfRelationY)
        {
            FromX = _FromX;
            FromY = _FromY;
            ToX = _ToX;
            ToY = _ToY;

            isSelfRelation = _isSelfRelation;
            currentSelfRelationX = selfRelationX;
            currentSelfRelationY = selfRelationY;

            DiagramLineRoute previousRoute = CurrentRoute;
            CurrentRoute = DiagramLineRouter.CreateRoute(
                FromDiagramItem,
                ToItem,
                OwningVisualiser,
                new Point(_FromX, _FromY),
                new Point(_ToX, _ToY),
                _isSelfRelation,
                selfRelationX,
                selfRelationY,
                LineWidth,
                GetEndingRequiredLength(StartAnchor),
                GetEndingRequiredLength(EndAnchor),
                previousRoute);

            if (CurrentRoute != null)
            {
                FromX = CurrentRoute.StartPoint.X;
                FromY = CurrentRoute.StartPoint.Y;
                ToX = CurrentRoute.EndPoint.X;
                ToY = CurrentRoute.EndPoint.Y;
            }

            UpdateRenderedRoute();
            UpdateLabelPosition();
        }

        protected virtual void UpdateRenderedRoute()
        {
            if (CurrentRoute == null ||
                CurrentRoute.Segments.Count == 0)
            {
                Line.Data = Geometry.Empty;
                LineEndings.Points = new PointCollection();
                Label.Visibility = Visibility.Collapsed;
                return;
            }

            Label.Visibility = Visibility.Visible;

            double startInset = GetEndingInset(StartAnchor);
            double endInset = GetEndingInset(EndAnchor);
            Line.Data = CurrentRoute.CreateBodyGeometry(
                startInset,
                endInset);

            Point start = CurrentRoute.StartPoint;
            Point end = CurrentRoute.EndPoint;
            Vector startTangent =
                CurrentRoute.GetStartTangent();
            Vector endTangent =
                CurrentRoute.GetEndTangent();
            const double markerDirectionProbeLength = 10;

            PointCollection endingPoints =
                new PointCollection
                {
                    start,
                    start +
                        startTangent *
                        markerDirectionProbeLength,
                    end -
                        endTangent *
                        markerDirectionProbeLength,
                    end
                };

            LineEndings.Points = endingPoints;

            ComputeCrowFootSideTips();
        }

        protected virtual void UpdateLabelPosition()
        {
            if (CurrentRoute == null ||
                CurrentRoute.Segments.Count == 0)
            {
                return;
            }

            Label.Measure(
                new System.Windows.Size(
                    double.PositiveInfinity,
                    double.PositiveInfinity));

            double labelWidth = Math.Max(
                1,
                Label.DesiredSize.Width);
            double labelHeight = Math.Max(
                1,
                Label.DesiredSize.Height);
            double[] fractions = { 0.5, 0.35, 0.65 };
            double bestScore = double.MaxValue;
            Point bestLeftTop = new Point();

            foreach (double fraction in fractions)
            {
                Point routePoint;
                Vector tangent;
                CurrentRoute.GetPointAndTangentAtFraction(
                    fraction,
                    out routePoint,
                    out tangent);

                Vector normal = new Vector(
                    -tangent.Y,
                    tangent.X);

                if (normal.Length < 0.001)
                    normal = new Vector(0, -1);
                else
                    normal.Normalize();

                for (int sideIndex = 0;
                    sideIndex < 2;
                    sideIndex++)
                {
                    double side = sideIndex == 0 ? -1 : 1;
                    double offset = labelHeight / 2 + 5;
                    Point leftTop = new Point(
                        routePoint.X +
                            normal.X * offset * side -
                            labelWidth / 2,
                        routePoint.Y +
                            normal.Y * offset * side -
                            labelHeight / 2);
                    Rect labelBounds = new Rect(
                        leftTop,
                        new System.Windows.Size(
                            labelWidth,
                            labelHeight));
                    double score =
                        Math.Abs(fraction - 0.5) * 4 +
                        sideIndex * 0.05;

                    score += GetLabelCollisionScore(
                        labelBounds);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestLeftTop = leftTop;
                    }
                }
            }

            Canvas.SetLeft(Label, bestLeftTop.X);
            Canvas.SetTop(Label, bestLeftTop.Y);
        }

        double GetLabelCollisionScore(Rect labelBounds)
        {
            UXVisualiser visualiser =
                OwningVisualiser as UXVisualiser;

            if (visualiser == null ||
                OwningVisualiser.Canvas == null)
            {
                return 0;
            }

            if (visualiser.IsItemMoveGraphInteractionActive)
                return 0;

            double score = 0;

            foreach (IUXItem item in
                visualiser.GetDiagramRoutingItems())
            {
                Rect itemBounds;
                if (!DiagramLineRouter.TryGetVisibleBounds(
                        item,
                        OwningVisualiser.Canvas,
                        out itemBounds) ||
                    !itemBounds.IntersectsWith(labelBounds))
                {
                    continue;
                }

                Rect intersection = Rect.Intersect(
                    itemBounds,
                    labelBounds);
                score += 100 +
                    intersection.Width *
                    intersection.Height;
            }

            foreach (UIElement child in
                OwningVisualiser.Canvas.Children)
            {
                TextBlock otherLabel =
                    child as TextBlock;

                if (otherLabel == null ||
                    object.ReferenceEquals(
                        otherLabel,
                        Label) ||
                    otherLabel.Visibility !=
                        Visibility.Visible)
                {
                    continue;
                }

                double left = Canvas.GetLeft(otherLabel);
                double top = Canvas.GetTop(otherLabel);

                if (double.IsNaN(left) ||
                    double.IsNaN(top))
                {
                    continue;
                }

                otherLabel.Measure(
                    new System.Windows.Size(
                        double.PositiveInfinity,
                        double.PositiveInfinity));
                double otherLabelWidth = Math.Max(
                    otherLabel.ActualWidth,
                    otherLabel.DesiredSize.Width);
                double otherLabelHeight = Math.Max(
                    otherLabel.ActualHeight,
                    otherLabel.DesiredSize.Height);

                if (otherLabelWidth <= 0 ||
                    otherLabelHeight <= 0)
                {
                    continue;
                }

                Rect otherLabelBounds = new Rect(
                    left,
                    top,
                    otherLabelWidth,
                    otherLabelHeight);

                if (otherLabelBounds.IntersectsWith(
                        labelBounds))
                {
                    score += 25;
                }
            }

            return score;
        }

        double GetEndingInset(LineEndEnum ending)
        {
            switch (ending)
            {
                case LineEndEnum.Triangle:
                case LineEndEnum.FilledTriangle:
                    return LineEndings.ArrowLength *
                        Math.Cos(
                            LineEndings.ArrowAngle /
                            2 *
                            Math.PI /
                            180);

                case LineEndEnum.CrowFoot:
                    return LineEndings.ArrowLength;

                case LineEndEnum.Diamond:
                case LineEndEnum.FilledDiamond:
                    return 17.5;

                default:
                    return 0;
            }
        }

        double GetEndingRequiredLength(LineEndEnum ending)
        {
            switch (ending)
            {
                case LineEndEnum.Diamond:
                case LineEndEnum.FilledDiamond:
                    return 17.5;

                case LineEndEnum.Arrow:
                case LineEndEnum.Triangle:
                case LineEndEnum.FilledTriangle:
                case LineEndEnum.CrowFoot:
                    return LineEndings.ArrowLength;

                default:
                    return 0;
            }
        }

        // Computes the two side prong tips of CrowFoot at each end (only for
        // ends with StartAnchor / EndAnchor == CrowFoot), using a shape-aware
        // ray/edge intersection on the relevant UXItem. The tip overrides are
        // pushed onto LineEndings so ArrowLineBase.DefiningGeometry uses them
        // instead of the angular fallback. Falls back to the angular formula
        // (override left null) when the item reference is missing or the
        // geometry is degenerate, so this method is safe to call eagerly.
        protected virtual void ComputeCrowFootSideTips()
        {
            LineEndings.StartCrowFootSide1Tip = null;
            LineEndings.StartCrowFootSide2Tip = null;
            LineEndings.EndCrowFootSide1Tip = null;
            LineEndings.EndCrowFootSide2Tip = null;

            if (StartAnchor != LineEndEnum.CrowFoot
                && EndAnchor != LineEndEnum.CrowFoot)
            {
                return;
            }


            PointCollection pc = LineEndings.Points;
            if (pc == null || pc.Count < 2)
                return;

            double arrowLen = LineEndings.ArrowLength;
            double halfAngle = LineEndings.ArrowAngle / 2.0;

            if (StartAnchor == LineEndEnum.CrowFoot && FromDiagramItem != null)
            {
                Point pt2 = pc[0];
                Point pt1 = pc[1];

                Vector vect = pt2 - pt1;

                if (vect.Length > 0.001)
                {
                    vect.Normalize();
                    Point convergence = pt2 - vect * arrowLen;

                    Vector dir1 = RotateVectorDegrees(vect, +halfAngle);
                    Vector dir2 = RotateVectorDegrees(vect, -halfAngle);

                    LineEndings.StartCrowFootSide1Tip = FromDiagramItem.GetLineEdgeIntersection(convergence, dir1);
                    LineEndings.StartCrowFootSide2Tip = FromDiagramItem.GetLineEdgeIntersection(convergence, dir2);
                }
            }

            if (EndAnchor == LineEndEnum.CrowFoot && ToItem != null)
            {
                int n = pc.Count;
                Point pt1 = pc[n - 2];
                Point pt2 = pc[n - 1];

                Vector vect = pt2 - pt1;

                if (vect.Length > 0.001)
                {
                    vect.Normalize();
                    Point convergence = pt2 - vect * arrowLen;

                    Vector dir1 = RotateVectorDegrees(vect, +halfAngle);
                    Vector dir2 = RotateVectorDegrees(vect, -halfAngle);

                    LineEndings.EndCrowFootSide1Tip = ToItem.GetLineEdgeIntersection(convergence, dir1);
                    LineEndings.EndCrowFootSide2Tip = ToItem.GetLineEdgeIntersection(convergence, dir2);
                }
            }
        }

        // Same rotation convention as Matrix.Rotate (degrees, screen Y-down
        // coordinate system); kept as a static helper to avoid allocating
        // a Matrix per call.
        static Vector RotateVectorDegrees(Vector v, double angleDegrees)
        {
            double rad = angleDegrees * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            return new Vector(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        public override double GetMouseDistance(Point p)
        {
            if (OwningVisualiser == null ||
                CurrentRoute == null ||
                CurrentRoute.FlattenedPoints.Count < 2)
            {
                return double.MaxValue;
            }

            Rect hitBounds = CurrentRoute.Bounds;
            hitBounds.Inflate(
                OwningVisualiser.LineSelectionDelta,
                OwningVisualiser.LineSelectionDelta);

            if (!hitBounds.Contains(p))
                return double.MaxValue;

            return CurrentRoute.GetDistanceFromPoint(p);
        }

        public override void AddToCanvas()
        {
            OwningVisualiser.Canvas.Children.Add(LineEndings);
            OwningVisualiser.Canvas.Children.Add(Line);

            if (!HideLabel)
            {
                OwningVisualiser.Canvas.Children.Add(Label);
            }

            VertexSetedUp(); 
        }

        public override void RemoveFromCanvas()
        {
            if (OwningVisualiser == null || OwningVisualiser.Canvas == null)
                return;

            OwningVisualiser.Canvas.Children.Remove(LineEndings);
            OwningVisualiser.Canvas.Children.Remove(Line);
            OwningVisualiser.Canvas.Children.Remove(Label);
        }

        public override void Highlight()
        {
            IsHighlighted = true;

            LineEndings.Stroke = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            Line.Stroke = (Brush)LineEndings.FindResource("0LightHighlightBrush");

            if (HighlightFillBrush != null)
                LineEndings.Fill = HighlightFillBrush;


            Label.Foreground = (Brush)LineEndings.FindResource("0HighlightBrush");

            Panel.SetZIndex(LineEndings, 99999);
            Panel.SetZIndex(Label, 99999);

            //

            double thickness = LineWidth + 2;            

            Line.StrokeThickness = thickness;
            LineEndings.StrokeThickness = thickness;

            NotifyMiniaturesHighlightChanged();
        }

        public override void Unhighlight()
        {
            Brush foregroundBrush = GetForegroundBrush();

            IsHighlighted = false;

            LineEndings.Stroke = foregroundBrush;
            Line.Stroke = foregroundBrush;

            if (FillBrush != null)
                LineEndings.Fill = FillBrush;

            Label.Foreground = foregroundBrush;

            Panel.SetZIndex(LineEndings, 0);
            Panel.SetZIndex(Label, 0);

            //

            double thickness = LineWidth;

            if (thickness == 0)
                thickness = 1;

            Line.StrokeThickness = thickness;
            LineEndings.StrokeThickness = thickness;

            NotifyMiniaturesHighlightChanged();
        }

        public override void Select()
        {            
            IsSelected = true;

            GeneralUtil.SetPropertyIfPresent(this.Content, "Foreground", GetBackgroundBrush());

            Panel.SetZIndex(this, 99999);
                        
            AddAnchor(ClickTargetEnum.AnchorRightTop_MoveDiagramLine, ToX - (AnchorSize/2), ToY - (AnchorSize/2));
        }

        public override void Unselect()
        {
            IsSelected = false;

            Panel.SetZIndex(this, 0);

            foreach (UIElement e in Anchors)
                OwningVisualiser.Canvas.Children.Remove(e);

            Anchors.Clear();
        }

        // UNDER

        static IVertex StartAnchor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\StartAnchor");
        static IVertex EndAnchor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\EndAnchor");
        static IVertex IsDashed_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\IsDashed");
        static IVertex HideLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\HideLabel");
        static IVertex ConstantLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator\ConstantLabel");

        public LineEndEnum StartAnchor
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "StartAnchor", null);

                return LineEndEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, StartAnchor_meta, LineEndEnumHelper.GetVertex(value));
            }
        }

        public LineEndEnum EndAnchor
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "EndAnchor", null);

                return LineEndEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, EndAnchor_meta, LineEndEnumHelper.GetVertex(value));
            }
        }

        public bool IsDashed
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsDashed", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsDashed", null);

                if (val == null)
                    val = Vertex.AddVertex(IsDashed_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool HideLabel
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideLabel", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideLabel", null);

                if (val == null)
                    val = Vertex.AddVertex(HideLabel_meta, value);
                else
                    val.Value = value;
            }
        }

        public string ConstantLabel
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ConstantLabel", null);

                if (val == null)
                    return null;

                return val.Value.ToString();                
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ConstantLabel", null);

                if (val == null)
                    val = Vertex.AddVertex(ConstantLabel_meta, value);
                else
                    val.Value = value;
            }
        }
    }
}
