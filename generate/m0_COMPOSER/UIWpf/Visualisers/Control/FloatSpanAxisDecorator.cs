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
    class FloatSpanAxisDecorator : AxisDecoratorBase, IZoomScrollViewAxisDecorator // dedicated to Set2DVisualiser
    {
        Set2DVisualiser Visualiser;


        double decoratorSize = 20;
        
        double valueSpaceSize;

        double valueSpaceMin = double.NegativeInfinity;
        double valueSpaceMax = double.NegativeInfinity;

        bool valueSpaceMinSet = false;
        bool valueSpaceMaxSet = false;

        //

        double FontSize = 10;

        double segmentsStep_valueSpace;
        double segmentsStart_valueSpace;
        double segmentsStop_valueSpace;
        double segmentsSize_valueSpace;

        int segmentDigits;

        public double ValueSpaceMin {
            get { return valueSpaceMin; }
            set {
                if (value != valueSpaceMin)
                {
                    valueSpaceMin = value;                    

                    valueSpaceMinSet = true;

                    if (valueSpaceMinSet && valueSpaceMaxSet)
                    {
                        valueSpaceSize = ValueSpaceMax - ValueSpaceMin;
                        Update();
                    }
                }
            }
        }        

        public double ValueSpaceMax {
            get { return valueSpaceMax; }
            set {
                if (value != valueSpaceMax)
                {
                    valueSpaceMax = value;                    

                    valueSpaceMaxSet = true;

                    if (valueSpaceMinSet && valueSpaceMaxSet)
                    {
                        valueSpaceSize = ValueSpaceMax - ValueSpaceMin;
                        Update();
                    }
                }
            }
        }

        public double ScreenToValueSpace(double screenPosition) {            
            if (!isHorizontal)
                screenPosition = Size.Height - screenPosition;

            return (screenPosition / BaseUnitSize) + segmentsStart_valueSpace;
        }

        public double ValueSpaceToScreen(double valueSpacePosition) {
            valueSpacePosition -= segmentsStart_valueSpace;

            double ret = valueSpacePosition * BaseUnitSize;

            if (!isHorizontal)
                ret = Size.Height - ret;

            return ret;
        }

        public bool isHorizontal { get; set; }

        public FloatSpanAxisDecorator(ZoomScrollViewBasedVisualiserBase _visualiser, bool _isHorizontal)
        {
            isHorizontal = _isHorizontal;
        
            visualiser = _visualiser;

            if (!isHorizontal)
                decoratorSize = 35;

            Width = decoratorSize;
            Height = decoratorSize;

            Segments = new List<AxisSegment>();
        }
            
        public event EventHandler SelectionChanged;

        //

        public override void PositionMarkUpdate() { }

        private void Update()
        {
            UpdateBaseUntSize(false);

            SegmentStepStartStopUpdate();

            UpdateBaseUntSize(true);

            SegmentsUpdate();

            if (Segments.Count > 0)
                Draw();
        }

        private void SegmentStepStartStopUpdate()
        {
            if (baseUnitSize <= 0 || baseUnitSize == double.PositiveInfinity)
                return;

            double scaleLinesDensity = 1;

            if(visualiser is Set2DVisualiser)
            {
                Set2DVisualiser set2Dvisualiser = (Set2DVisualiser)visualiser;
                scaleLinesDensity = set2Dvisualiser.ScaleLinesDensity;
            }

            double baseUnitSize_log = Math.Log10(baseUnitSize * scaleLinesDensity);
            int baseUnitSize_log_round = (int)Math.Round(baseUnitSize_log);

            int segmentStep_log = 2 - baseUnitSize_log_round;

            if ((segmentStep_log - 1) < 0)
                segmentDigits = -1 * (segmentStep_log - 1);
            else
                segmentDigits = 0;

            segmentsStep_valueSpace = Math.Pow(10, segmentStep_log);

            segmentsStart_valueSpace = MathUtil.RoundDown(ValueSpaceMin, segmentStep_log);
            segmentsStop_valueSpace = MathUtil.RoundUp(ValueSpaceMax, segmentStep_log);

            segmentsSize_valueSpace = Math.Abs(segmentsStop_valueSpace - segmentsStart_valueSpace);
        }

        private void Draw()
        { 
            Size s = new Size();           

            if (isHorizontal)
            {
                //s.Width = valueSpaceSize * baseUnitSize;
                s.Width = segmentsSize_valueSpace * baseUnitSize;
                s.Height = decoratorSize;
            }
            else
            {
                s.Width = decoratorSize;
                //s.Height = valueSpaceSize * baseUnitSize;
                s.Height = segmentsSize_valueSpace * baseUnitSize;
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

                    if(segmentDigits == 0)
                        t.Text = ax.Tag.ToString();
                    else
                    {
                        double value = (double)ax.Tag;

                        value = Math.Round(value, segmentDigits);

                        t.Text = value.ToString();
                    }

                    t.FontSize = FontSize;

                    if (isHorizontal)
                        WpfUtil.SetPosition(t, ax.StartPosition + 2, 0);
                    else
                        WpfUtil.SetPosition(t, 2, ax.StartPosition - FontSize - 4);

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

        private void SegmentsUpdate() { 
            Segments = new List<AxisSegment>();            


            for (double position = segmentsStart_valueSpace; position < segmentsStop_valueSpace ; position += segmentsStep_valueSpace)
            {
                AxisSegment segment = new AxisSegment();

                segment.LineStyle = new LineStyle();

                segment.LineStyle.Stroke = (Brush)WpfUtil.FindResource("0VeryLightForegroundBrush");

                segment.StartPosition = ValueSpaceToScreen(position);
                segment.EndPosition = ValueSpaceToScreen(position + segmentsStep_valueSpace);

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
            //UpdateBaseUntSize();
            Update();
        }

        void UpdateBaseUntSize(bool useSegments) { 
            FrameworkElement wholeBox = visualiser.ZoomScrollView;

            FrameworkElement horizontalSlider = visualiser.ZoomScrollView.HorizontalZoomSlider;

            FrameworkElement verticalSlider = visualiser.ZoomScrollView.VerticalZoomSlider;

            FrameworkElement horizontalDecorator = (FrameworkElement)visualiser.ZoomScrollView.HorizontalAxisDecoratorScrollViewer.Content;

            FrameworkElement verticalDecorator = (FrameworkElement)visualiser.ZoomScrollView.VerticalAxisDecoratorScrollViewer.Content;

            if (wholeBox == null || horizontalSlider == null || horizontalDecorator == null)
                return;


            double scale = 1 + (zoomFactor/4);

            double mainSize;

            if (isHorizontal)
                mainSize = wholeBox.ActualWidth - verticalDecorator.Width - verticalSlider.ActualWidth - 2;
            else
                mainSize = wholeBox.ActualHeight - horizontalDecorator.Height - horizontalSlider.ActualHeight - 2;

            if (mainSize < 0)
                mainSize = 0;

            if(useSegments)
                baseUnitSize = (mainSize / segmentsSize_valueSpace) * scale;
            else
                baseUnitSize = (mainSize / valueSpaceSize) * scale;
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
