using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using m0;
using System.Windows.Shapes;
using m0.UIWpf;
using System.Windows.Media;
using m0.Graph;
using m0.ZeroTypes;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    class PitchSetAxisDecorator : AxisDecoratorBase, IZoomScrollViewAxisDecorator
    {
        public double ValueSpaceMin { get; set; }

        public double ValueSpaceMax { get; set; }

        public double ScreenToValueSpace(double screenPosition) { return 0; }

        public double ValueSpaceToScreen(double valueSpacePosition) { return 0; }

        public bool isHorizontal { get; set; }

        double FontSize = 12;

        double segmentSize;

        private void Draw()
        {
            Children.Clear();

            double maxWidth = 60;

            List<TextBlock> tbl = new List<TextBlock>();

            foreach (AxisSegment s in Segments)
            {
                bool isSelected = false;

                if (Selection != null && ((AxisSegment)Selection).BaseVertex == s.BaseVertex)
                    isSelected = true;

                TextBlock t = new TextBlock();

                t.Text = s.BaseVertex.Get(false, "Name:").Value.ToString();

                Color segmentColor = s.Color;
                Color negativeSegmentColor = WpfUtil.GetNegativeColorWhiteOrBlack(segmentColor);

                if (isSelected)
                {
                    segmentColor = (Color)WpfUtil.FindResource("0Highlight");
                    negativeSegmentColor = (Color)WpfUtil.FindResource("0Background");
                }

                if (segmentColor != null)
                {
                    t.Background = new SolidColorBrush(segmentColor);

                    if (segmentSize > 13)
                        t.Foreground = new SolidColorBrush(negativeSegmentColor);
                    else
                        t.Foreground = new SolidColorBrush(segmentColor);
                }

                t.FontSize = FontSize;

                t.Height = segmentSize;

                WpfUtil.SetPosition(t, 0, s.StartPosition);

                tbl.Add(t);

                Children.Add(t);

                //

                Size si = WpfUtil.MeasureTextBlock(t);

                if (si.Width > maxWidth)
                    maxWidth = si.Width;
            }

            Size newSize = new Size();
            newSize.Width = maxWidth;

            if (newSize.Width < 50)
                newSize.Width += 10;

            newSize.Height = Size.Height;

            Size = newSize;

            Width = Size.Width;
            Height = Size.Height;

            foreach (TextBlock tb in tbl)
                tb.Width = Size.Width;

            foreach (AxisSegment s in Segments)
            {
                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Size.Width, s.StartPosition);

                s.LineStyle.SetStyle(l);

                Children.Add(l);

                //

                l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.EndPosition, Size.Width, s.EndPosition);

                s.LineStyle.SetStyle(l);

                Children.Add(l);
            }

            //

            Line lr = new Line();

            WpfUtil.SetLinePosition(lr, Size.Width, 0, Size.Width, Size.Height);

            lr.StrokeThickness = 5;

            lr.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            Children.Add(lr);
        }

        private void Update()
        {
            Segments = new List<AxisSegment>();

            int cnt = 0;

            //segmentSize = FontSize * 1.5;

            double maxHeight = 0;

            int prevOctave = -9;

            foreach (IEdge e in baseVertex.GetAll(false, "VisualisedPitch:"))
            {
                AxisSegment segment = new AxisSegment();

                segment.LineStyle = new LineStyle();

                segment.StartPosition = cnt * segmentSize;
                segment.EndPosition = (cnt + 1) * segmentSize;

                if (segment.EndPosition > maxHeight)
                    maxHeight = segment.EndPosition;

                segment.BaseVertex = e.To;

                //

                IVertex colorVertex = segment.BaseVertex.Get(false, "PitchColor:");

                if (colorVertex != null)
                    segment.Color = ColorHelper_desktop.GetColorFromColorVertex(colorVertex);

                //

                IVertex noteBackgroundColorVertex = segment.BaseVertex.Get(false, "NoteBackgroundColor:");

                if (noteBackgroundColorVertex != null)
                {
                    segment.UseBackgroundColor = true;
                    segment.BackgroundColor = ColorHelper_desktop.GetColorFromColorVertex(noteBackgroundColorVertex);
                }


                int? thisOctave = GraphUtil.GetIntegerValue(segment.BaseVertex.Get(false, "Octave:"));

                if (thisOctave != null && thisOctave != prevOctave)
                {
                    prevOctave = (int)thisOctave;

                    segment.LineStyle.StrokeThickness = 3;
                }


                Segments.Add(segment);

                cnt++;
            }

            Size s = new Size();
            s.Height = maxHeight;

            Size = s;

            Draw();
        }

        public void SetBaseVertex(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            Update();
        }

        public void SetZoomFactor(double _zoomFactor)
        {
            zoomFactor = _zoomFactor;

            segmentSize = 3 + (15 * (zoomFactor / 40));

            Update();
        }

        public void SetValueSpaceMax(double length)
        {

        }

        public PitchSetAxisDecorator()
        {
            this.MouseDown += PitchSetAxisDecorator_MouseDown;
        }

        private void PitchSetAxisDecorator_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            foreach (AxisSegment s in Segments)
                if (s.StartPosition <= p.Y && p.Y <= s.EndPosition)
                {
                    if (Selection == s)
                        Selection = null;
                    else
                        Selection = s;

                    if (SelectionChanged != null)
                        SelectionChanged(sender, null);

                    Draw();
                }
        }

        public event EventHandler SelectionChanged;

        public object Selection { get; set; }
    }
}
