using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers.Diagram;
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
            FromX = _FromX;
            FromY = _FromY;
            ToX = _ToX;
            ToY = _ToY;

            base.SetPosition(FromX, FromY, ToX, ToY, isSelfRelation, selfRelationX, selfRelationY);            

            if (MetaDiagramItem == null)
                return;

            PointCollection pc = new PointCollection();

            if (!isSelfRelation) 
            {
                Point p = new Point(FromX + ((ToX - FromX) / 2), FromY + ((ToY - FromY) / 2));
                pc.Add(p);
                pc.Add(MetaDiagramItem.GetLineAnchorLocation(this, 1, 1, false));                    

                MetaLine.Points = pc;
            }
        }

        public override void UpdateMetaPosition()
        {
            base.UpdateMetaPosition();

            PointCollection pc = new PointCollection();

            Point p = new Point(FromX + ((ToX - FromX) / 2), FromY + ((ToY - FromY) / 2));
            pc.Add(p);
            pc.Add(MetaDiagramItem.GetLineAnchorLocation(this, 1, 1, false));

            MetaLine.Points = pc;
        }

        public override void AddToCanvas()
        {
            IEdge baseEdge = BaseEdge;

            Diagram.Canvas.Children.Add(LineEndings);
            Diagram.Canvas.Children.Add(Line);

            if (baseEdge.Meta == MinusZero.Instance.Empty)
                return;           

            if (Diagram.GetItemsDictionaryByBaseEdgeTo().ContainsKey(baseEdge.Meta))
                MetaDiagramItem = Diagram.GetItemsDictionaryByBaseEdgeTo()[baseEdge.Meta].FirstOrDefault();

            if (MetaDiagramItem != null)
            {
                Diagram.Canvas.Children.Add(MetaLine);
                MetaDiagramItem.AddAsToMetaLine(this);
            }
            else
                Diagram.Canvas.Children.Add(Label);
            
            VertexSetedUp();
        }

        public override void RemoveFromCanvas()
        {
            Diagram.Canvas.Children.Remove(MetaLine);
            Diagram.Canvas.Children.Remove(LineEndings);
            Diagram.Canvas.Children.Remove(Line);
            Diagram.Canvas.Children.Remove(Label);
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
