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
            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "IsDashed")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "LineWidth"))
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

            if (IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:"))
                 || IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:\To:")))
                VertexUpdated();

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "HideLabel"))
                UpdateLabelVisibility();

            return exe.Stack;
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
        }

        public override double GetMouseDistance(Point p)
        {
            if (OwningVisualiser == null)
                return double.MaxValue;

            if (!isSelfRelation)
            {
                return GetMouseDistance_Helper(p, FromX, FromY, ToX, ToY);
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

                return min;
            }
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
