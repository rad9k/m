using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using m0.Foundation;
using System.Windows.Controls;
using m0.Util;
using m0.Graph;
using System.Windows.Shapes;
using m0.UIWpf;
using System.Windows.Media;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    class FloatSpanAxisDecorator : AxisDecoratorBase, IZoomScrollViewAxisDecorator
    {
        double valueSpaceMin;

        public double ValueSpaceMin { get { return valueSpaceMin; }
            set {
                valueSpaceMin = value;

                Update();
            }
        }

        double valueSpaceMax;

        public double ValueSpaceMax {
            get { return valueSpaceMax; }
            set {
                valueSpaceMax = value;

                Update();
            }
        }

        public double ScreenToValueSpace(double screenPosition) { return 0; }

        public double ValueSpaceToScreen(double valueSpacePosition) { return 0; }



        public bool isHorizontal { get; set; }

        public FloatSpanAxisDecorator(ZoomScrollViewBasedVisualiserBase _visualiser) : base()
        {
            visualiser = _visualiser;
        }
            

        double FontSize = 10;

        double timeSpanHeight;

        class timeSpanLevel
        {
            public int BaseMusicTimeSpanLevelCountForThisLevel;
            public int length;
            public IVertex timeSpanLevelVertex;
        }

        List<timeSpanLevel> timeSpanStructure;

        int timeSpanLevels;        

        double Length;

        double segmentStep;
        double segmentStart;
        double segmentStop;

        public event EventHandler SelectionChanged;

        //

        public override void PositionMarkUpdate() { }

        private void SegmentStepUpdate()
        {
            segmentStep = 10;
        }

        private void SegmentStartStopUpdate()
        {
            segmentStart = valueSpaceMin;
            segmentStop = ValueSpaceMax;
        }

        private void Draw()
        { 
            Size s = new Size();

            double decoratorSize = 20;

            double valueSpaceSize = ValueSpaceMax - ValueSpaceMin;

            if (isHorizontal)
            {
                s.Width = valueSpaceSize * baseUnitSize;
                s.Height = decoratorSize;
            }
            else
            {
                s.Width = decoratorSize;
                s.Height = valueSpaceSize * baseUnitSize;
            }

            Size = s;

            Width = Size.Width;
            Height = Size.Height;

            //

            Children.Clear();

            DrawBackground();

            foreach (AxisSegment ax in Segments)
                {
                    TextBlock t = new TextBlock();

                    t.Foreground = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                    t.Text = ax.Tag.ToString();

                    t.FontSize = FontSize;

                    if (isHorizontal)
                        WpfUtil.SetPosition(t, ax.StartPosition + 2, 0);
                    else
                        WpfUtil.SetPosition(t, 2, ax.StartPosition);

                    Children.Add(t);

                    //

                    Line l = new Line();

                    if (isHorizontal)
                        WpfUtil.SetLinePosition(l, ax.StartPosition, 0, ax.StartPosition, decoratorSize);
                    else
                        WpfUtil.SetLinePosition(l, 0, ax.StartPosition, decoratorSize, ax.StartPosition);

                    l.StrokeThickness = 1;

                    l.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                    Children.Add(l);
                }
        }

        private void Update()
        {

            SegmentStepUpdate();
            SegmentStartStopUpdate();

            SegmentsUpdate();

            Draw();
        }

        private void SegmentsUpdate() { 
            Segments = new List<AxisSegment>();            


            for (double position = segmentStart; position < segmentStop ; position += segmentStep)
            {
                AxisSegment segment = new AxisSegment();

                segment.LineStyle = new LineStyle();

                segment.StartPosition = position * baseUnitSize;
                segment.EndPosition = (position + segmentStep) * baseUnitSize;

                segment.Tag = position;

                Segments.Add(segment);
            }                        
        }

        public void SetBaseVertex(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            Update();
        }

        public void SetZoomFactor(double _zoomFactor)
        {
            zoomFactor = _zoomFactor;

            baseUnitSize = 10;

            Update();
        }


        public object Selection { get; set; }

        protected void DrawBackground()
        {
            Border b = new Border();

            b.Background = (Brush)WpfUtil.FindResource("0BackgroundBrush");

            WpfUtil.SetPosition(b, 0, 0, Width, Height);

            this.Children.Add(b);

            b.PreviewMouseDown += MouseDownHandler;
        }
    }
}
