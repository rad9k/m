using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf;
using m0.UIWpf.Controls;
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
using static m0.Graph.ExecutionFlow.ExecutionFlowHelper;

namespace m0.ZeroTypes.UX
{
    public class LineDecorator: LineDecoratorBase
    {
        protected ArrowPolyline LineEndings = new ArrowPolyline();
        protected ArrowPolyline Line = new ArrowPolyline();

        protected TextBlock Label = new TextBlock();

        IEdge graphChangeListenerEdge;

        public override void VertexSetedUp()
        {
            UpdateLineEnds();
            VertexUpdated();
            UpdateLabelVisibility();

            graphChangeListenerEdge = ExecutionFlowHelper.AddTriggerAndListener(Vertex,
                 new List<string> { "", @"\" },
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

            Line.IsEndings = false;
            Line.StrokeThickness = 1;
            Line.Stroke = GetForegroundBrush();

            Line.ArrowLength = 15;
            Line.ArrowAngle = 60;

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
        }

        protected virtual void UpdateLabelVisibility()
        {
            if (HideLabel)
                OwningVisualiser.Canvas.Children.Remove(Label);
            else
                if (!OwningVisualiser.Canvas.Children.Contains(Label))
                    OwningVisualiser.Canvas.Children.Add(Label);
        }

        protected virtual void UpdateLineEnds()
        {
            Brush backgroundBrush = GetBackgroundBrush();
            Brush foregroundBrush = GetForegroundBrush();

            LineEndings.Stroke = foregroundBrush;
            Line.Stroke = foregroundBrush;
            Label.Foreground = foregroundBrush;

            LineEndings.StartEnding = StartAnchor;
            Line.StartEnding = StartAnchor;

            LineEndings.EndEnding = EndAnchor;
            Line.EndEnding = EndAnchor;            

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
            Line.ArrowLength = 0;
            LineEndings.ArrowLength = 15;
            Line.ArrowLength = 15;

            // Anchor type may have just become CrowFoot without items moving;
            // refresh the side prong tips so DefiningGeometry sees them on the
            // very next render.
            ComputeCrowFootSideTips();
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
        }

        public override void SetPosition(double _FromX, double _FromY, double _ToX, double _ToY, bool _isSelfRelation, double selfRelationX, double selfRelationY)
        {
            long t0 = UXPerfLog.Timestamp();

            FromX = _FromX;
            FromY = _FromY;
            ToX = _ToX;
            ToY = _ToY;

            isSelfRelation = _isSelfRelation;

            PointCollection pc = new PointCollection();

            pc.Add(new Point(FromX, FromY));

            if (isSelfRelation)
            {
                pc.Add(new Point(FromX, selfRelationY));
                pc.Add(new Point(selfRelationX, selfRelationY));
                pc.Add(new Point(selfRelationX, ToY));

                Canvas.SetLeft(Label, selfRelationX + 3);
                Canvas.SetTop(Label, selfRelationY);
            }
            else
            {
                Canvas.SetLeft(Label, FromX + ((ToX - FromX) / 2));
                Canvas.SetTop(Label, FromY + ((ToY - FromY) / 2));
            }

            pc.Add(new Point(ToX, ToY));

            LineEndings.Points = pc;
            Line.Points = pc;

            long tCrow = UXPerfLog.Timestamp();
            ComputeCrowFootSideTips();
            UXPerfLog.Record("LineDecorator.SetPosition.ComputeCrowFootSideTips", UXPerfLog.Timestamp() - tCrow);

            UXPerfLog.Record("LineDecorator.SetPosition", UXPerfLog.Timestamp() - t0, isSelfRelation ? 1 : 0, "self");
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
            long t0 = UXPerfLog.Timestamp();

            if (OwningVisualiser == null)
            {
                UXPerfLog.Record("LineDecorator.GetMouseDistance", UXPerfLog.Timestamp() - t0);
                return double.MaxValue;
            }

            double result;

            if (!isSelfRelation)
            {
                result = GetMouseDistance_Helper(p, FromX, FromY, ToX, ToY);
            }
            else
            {
                double min = 99999;

                for (int x = 0; x < Line.Points.Count - 1; x++)
                {
                    Point A = Line.Points[x];
                    Point B = Line.Points[x + 1];

                    double distance = GetMouseDistance_Helper(p, A.X, A.Y, B.X, B.Y);

                    if (distance < min)
                        min = distance;
                }

                result = min;
            }

            UXPerfLog.Record("LineDecorator.GetMouseDistance", UXPerfLog.Timestamp() - t0);
            return result;
        }

        private double GetMouseDistance_Helper(Point p, double _FromX, double _FromY, double _ToX, double _ToY)
        {
            double max = 99999;

            Line2D l2d = Geometry2D.GetLine2DFromPoints(_FromX, _FromY, _ToX, _ToY);

             if (p.X + OwningVisualiser.LineSelectionDelta < Math.Min(_FromX, _ToX) ||
                p.X - OwningVisualiser.LineSelectionDelta > Math.Max(_FromX, _ToX) ||
                p.Y + OwningVisualiser.LineSelectionDelta < Math.Min(_FromY, _ToY) ||
                p.Y - OwningVisualiser.LineSelectionDelta > Math.Max(_FromY, _ToY))
                return max;

            return Geometry2D.GetPointDistanceFrom2DLine(l2d, p);
        }

        public override void AddToCanvas()
        {
            OwningVisualiser.Canvas.Children.Add(LineEndings);
            OwningVisualiser.Canvas.Children.Add(Line);

            if (!HideLabel)
                OwningVisualiser.Canvas.Children.Add(Label);

            VertexSetedUp(); 
        }

        public override void RemoveFromCanvas()
        {
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
            IsSelected = true;

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
