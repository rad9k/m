using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0.UIWpf.Visualisers.Diagram
{
    public class DiagramLine: DiagramLineBase, IPlatformClass
    {
        private IVertex _Vertex;
        public override IVertex Vertex { get { return _Vertex; }
            set {
                _Vertex = value;

                VertexUpdated();

                PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges", "ForegroundColor", "BackgroundColor" });
            }
        }

        private void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex.Get(@"BaseEdge:\To:") || sender == Vertex.Get(@"BaseEdge:\Meta:"))
                && e.Type == VertexChangeType.ValueChanged)
                VertexUpdated();

            if ((e.Type == VertexChangeType.EdgeAdded && (GeneralUtil.CompareStrings(e.Edge.Meta.Value,"IsDashed")||GeneralUtil.CompareStrings(e.Edge.Meta.Value,"LineWidth")))
                || (e.Type == VertexChangeType.ValueChanged && (sender == Vertex.Get(@"IsDashed:")||sender == Vertex.Get(@"LineWidth:"))))
            {
                UpdateLine();
            }

            if ((e.Type == VertexChangeType.EdgeAdded && (GeneralUtil.CompareStrings(e.Edge.Meta.Value,"StartAnchor") || GeneralUtil.CompareStrings(e.Edge.Meta.Value,"EndAnchor")))
                || (e.Type == VertexChangeType.ValueChanged && (sender == Vertex.Get(@"StartAnchor:")||sender == Vertex.Get(@"EndAnchor:"))))
            {
                UpdateLineEnds();
            }

            if ((e.Type == VertexChangeType.EdgeAdded && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BackgroundColor") || GeneralUtil.CompareStrings(e.Edge.Meta.Value, "ForegroundColor")))
                || (e.Type == VertexChangeType.ValueChanged && (
                  sender == Vertex.Get(@"BackgroundColor:") || sender == Vertex.Get(@"BackgroundColor:\Red:") || sender == Vertex.Get(@"BackgroundColor:\Green:") || sender == Vertex.Get(@"BackgroundColor:\Blue:") || sender == Vertex.Get(@"BackgroundColor:\Opacity:") ||
                   sender == Vertex.Get(@"ForegroundColor:") || sender == Vertex.Get(@"ForegroundColor:\Red:") || sender == Vertex.Get(@"ForegroundColor:\Green:") || sender == Vertex.Get(@"ForegroundColor:\Blue:") || sender == Vertex.Get(@"ForegroundColor:\Opacity:")       
                )))
            {
                UpdateLineEnds();
            }
        }

        protected virtual void UpdateLine()
        {
            if (GraphUtil.GetDoubleValue(Vertex.Get("LineWidth:")) != GraphUtil.NullDouble)
                LineWidth = GraphUtil.GetDoubleValue(Vertex.Get("LineWidth:"));
            else
                LineWidth = 1;

            Line.StrokeThickness = LineWidth;
            LineEndings.StrokeThickness = LineWidth;

            if (GeneralUtil.CompareStrings(Vertex.Get("IsDashed:"), "True"))
                Line.StrokeDashArray = new DoubleCollection(new double[] { 5, 3 });
            else
                Line.StrokeDashArray = null;
        }

        protected virtual void UpdateLineEnds()
        {
            if (Vertex.Get("BackgroundColor:") != null)
                BackgroundColor = UIWpf.GetBrushFromColorVertex(Vertex.Get("BackgroundColor:"));
            else
                BackgroundColor = (Brush)Line.FindResource("0BackgroundBrush");

            if (Vertex.Get("ForegroundColor:") != null)
                ForegroundColor = UIWpf.GetBrushFromColorVertex(Vertex.Get("ForegroundColor:"));
            else
                ForegroundColor = (Brush)Line.FindResource("0ForegroundBrush");

            LineEndings.Stroke = ForegroundColor;
            Line.Stroke = ForegroundColor;
            Label.Foreground = ForegroundColor;


            string StartAnchor = (string)GraphUtil.GetValue(Vertex.Get(@"StartAnchor:"));
            string EndAnchor = (string)GraphUtil.GetValue(Vertex.Get(@"EndAnchor:"));

            if (StartAnchor == "Straight")
            {
                LineEndings.StartEnding = LineEndEnum.Straight;
                Line.StartEnding = LineEndEnum.Straight;
            }

            if (EndAnchor == "Straight")
            {
                LineEndings.EndEnding = LineEndEnum.Straight;
                Line.EndEnding = LineEndEnum.Straight;
            }

            if (StartAnchor == "Arrow")
            {
                LineEndings.StartEnding = LineEndEnum.Arrow;
                Line.StartEnding = LineEndEnum.Arrow;
            }

            if (EndAnchor == "Arrow")
            {
                LineEndings.EndEnding = LineEndEnum.Arrow;
                Line.EndEnding = LineEndEnum.Arrow;
            }

            if (StartAnchor == "Triangle")
            {
                LineEndings.StartEnding = LineEndEnum.Triangle;
                Line.StartEnding = LineEndEnum.Triangle;

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (EndAnchor == "Triangle")
            {
                LineEndings.EndEnding = LineEndEnum.Triangle;
                Line.EndEnding = LineEndEnum.Triangle;

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (StartAnchor == "FilledTriangle")
            {
                LineEndings.StartEnding = LineEndEnum.FilledTriangle;
                Line.StartEnding = LineEndEnum.FilledTriangle;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (EndAnchor == "FilledTriangle")
            {
                LineEndings.EndEnding = LineEndEnum.FilledTriangle;
                Line.EndEnding = LineEndEnum.FilledTriangle;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (StartAnchor == "Diamond")
            {
                LineEndings.StartEnding = LineEndEnum.Diamond;
                Line.StartEnding = LineEndEnum.Diamond;

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (EndAnchor == "Diamond")
            {
                LineEndings.EndEnding = LineEndEnum.Diamond;
                Line.EndEnding = LineEndEnum.Diamond;

                FillBrush = BackgroundColor;
                HighlightFillBrush = BackgroundColor;
            }

            if (StartAnchor == "FilledDiamond")
            {
                LineEndings.StartEnding = LineEndEnum.FilledDiamond;
                Line.StartEnding = LineEndEnum.FilledDiamond;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }

            if (EndAnchor == "FilledDiamond")
            {
                LineEndings.EndEnding = LineEndEnum.FilledDiamond;
                Line.EndEnding = LineEndEnum.FilledDiamond;

                FillBrush = ForegroundColor;
                HighlightFillBrush = (Brush)LineEndings.FindResource("0LightHighlightBrush");
            }


            if (FillBrush != null)
                LineEndings.Fill = FillBrush;
            
            LineEndings.ArrowLength=0;
            Line.ArrowLength = 0;
            LineEndings.ArrowLength=15;
            Line.ArrowLength = 15;            

        }

        Brush FillBrush=null;
        Brush HighlightFillBrush = null;

        private void VertexUpdated(){
            if (Vertex.Get(@"Definition:Inheritence") != null) // not to display $Inherits
                return;

            if (Vertex.Get(@"BaseEdge:\Meta:\$VertexTarget:")!=null
                &&!GraphUtil.GetValueAndCompareStrings(Vertex.Get(@"Definition:\CreateEdgeOnly:"),"True"))
            {
                IVertex v=Vertex.Get(@"BaseEdge:\To:");
                if(v.Value!=null&&!GeneralUtil.CompareStrings(v.Value,"$Empty"))
                      Label.Text = (string)v.Value;
            }
            else
            {
                IVertex v = Vertex.Get(@"BaseEdge:\Meta:");
                if (v.Value != null && !GeneralUtil.CompareStrings(v.Value, "$Empty"))
                    Label.Text = (string)v.Value;
            }        
        }

        protected ArrowPolyline LineEndings=new ArrowPolyline();
        protected ArrowPolyline Line = new ArrowPolyline();

        protected TextBlock Label = new TextBlock();

       

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
                pc.Add(new Point(selfRelationX,  selfRelationY));
                pc.Add(new Point(selfRelationX, ToY));

                Canvas.SetLeft(Label,  selfRelationX + 3);
                Canvas.SetTop(Label,  selfRelationY);
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
            

            if (!isSelfRelation)
            {
                return GetMouseDistance_Helper(p,FromX,FromY,ToX,ToY);
            }
            else
            {
                double min = 99999;

                for(int x = 0; x < Line.Points.Count - 1; x++)
                {
                    Point A = Line.Points[x];
                    Point B = Line.Points[x + 1];

                    double distance= GetMouseDistance_Helper(p, A.X, A.Y, B.X, B.Y);

                    if (distance < min)
                        min = distance;
                }

                return min;
            }
        }

        private double GetMouseDistance_Helper(Point p,double _FromX, double _FromY, double _ToX, double _ToY)
        {
            double max = 99999;

            Line2D l2d = Geometry2D.GetLine2DFromPoints(_FromX, _FromY, _ToX, _ToY);

            if (p.X + Diagram.lineSelectionDelta < Math.Min(_FromX, _ToX) ||
                p.X - Diagram.lineSelectionDelta > Math.Max(_FromX, _ToX) ||
                p.Y + Diagram.lineSelectionDelta < Math.Min(_FromY, _ToY) ||
                p.Y - Diagram.lineSelectionDelta > Math.Max(_FromY, _ToY))
                return max;

            return Geometry2D.GetPointDistanceFrom2DLine(l2d, p);
        }

        public override void AddToCanvas()
        {
            Diagram.TheCanvas.Children.Add(LineEndings);
            Diagram.TheCanvas.Children.Add(Line);
            Diagram.TheCanvas.Children.Add(Label);
        }

        public override void RemoveFromCanvas()
        {
            Diagram.TheCanvas.Children.Remove(LineEndings);
            Diagram.TheCanvas.Children.Remove(Line);
            Diagram.TheCanvas.Children.Remove(Label);
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
        }

        public override void Unhighlight()
        {
            IsHighlighted = false;

            LineEndings.Stroke = ForegroundColor;
            Line.Stroke = ForegroundColor;

            if (FillBrush != null)
                LineEndings.Fill = FillBrush;

            Label.Foreground = ForegroundColor;

            Panel.SetZIndex(LineEndings, 0);
            Panel.SetZIndex(Label, 0); 
        }

        public DiagramLine(){
            ForegroundColor = (Brush)Line.FindResource("0ForegroundBrush");
            BackgroundColor = (Brush)Line.FindResource("0BackgroundBrush");

            LineEndings.IsEndings = true;
            LineEndings.StrokeThickness = 1;
            LineEndings.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");

            LineEndings.ArrowLength = 15;
            LineEndings.ArrowAngle = 60;

            Line.IsEndings = false;
            Line.StrokeThickness = 1;
            Line.Stroke = (Brush)LineEndings.FindResource("0ForegroundBrush");

            Line.ArrowLength = 15;
            Line.ArrowAngle = 60;

            Label.Foreground = (Brush)LineEndings.FindResource("0ForegroundBrush");
        }
    }
}

