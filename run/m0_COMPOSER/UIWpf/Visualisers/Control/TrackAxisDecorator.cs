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
using m0.UIWpf.Visualisers.Controls;
using m0.User.Process.UX;
using m0.ZeroTypes;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    class TrackAxisDecorator : AxisDecoratorBase, IZoomScrollViewAxisDecorator
    {
        public double ValueSpaceMin { get; set; }

        public double ValueSpaceMax { get; set; }

        public double ScreenToValueSpace(double screenPosition) { return 0; }

        public double ValueSpaceToScreen(double valueSpacePosition) { return 0; }

        public bool isHorizontal { get; set; }

        double FontSize = 12;

        double segmentSize;        
       

        List<LitButton> MuteButtons;
        List<LitButton> SoloButtons;

        private void DrawAdditionalSegmentControls(AxisSegment s)
        {
            bool isNull = false; 

            LitButton m = new LitButton(new SolidColorBrush(Colors.DarkRed), new SolidColorBrush(Colors.Red), 
                new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.White), "M");

            m.Width = 13;

            WpfUtil.SetPosition(m, Width - 17, s.StartPosition + 2);

            this.Children.Add(m);

            m.Tag = s.BaseVertex;

            if (GraphUtil.GetBooleanValue(s.BaseVertex.Get(false, "IsMuted:"), ref isNull))
                m.On();

            m.MouseDown += M_MouseDown;

            MuteButtons.Add(m);

            //

            LitButton so = new LitButton(new SolidColorBrush(Colors.DarkKhaki), new SolidColorBrush(Colors.Yellow),
                new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.Black), "S");

            so.Width = 13;

            WpfUtil.SetPosition(so, Width - 32, s.StartPosition + 2);

            this.Children.Add(so);

            so.Tag = s.BaseVertex;

            if (GraphUtil.GetBooleanValue(s.BaseVertex.Get(false, "IsSolo:"), ref isNull))
                so.On();

            so.MouseDown += So_MouseDown;

            SoloButtons.Add(so);

            //

            InfoButton ib = new InfoButton();

            ib.NewEditWindow = true;

            ib.BaseEdge = s.BaseEdge;

            WpfUtil.SetPosition(ib, Width - 49, s.StartPosition + 4);

            this.Children.Add(ib);

            //

            DeleteButton db = new DeleteButton();            

            db.BaseEdge = s.BaseEdge;

            WpfUtil.SetPosition(db, Width - 64, s.StartPosition + 4);

            this.Children.Add(db);
        }

        private void M_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {            
            LitButton senderButton = (LitButton)sender;
            IVertex trackBaseVertex = (IVertex)senderButton.Tag;

            bool isNull = false;
            bool isMuted = GraphUtil.GetBooleanValue(trackBaseVertex.Get(false, "IsMuted:"), ref isNull);

            IVertex metaVertex = m0.MinusZero.Instance.root.Get(false, @"System\Lib\Music\Track\IsMuted");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (!isMuted)
            {   
                GraphUtil.SetVertexValue(trackBaseVertex, metaVertex, "True");

                senderButton.On();
            }
            else
            {
                GraphUtil.SetVertexValue(trackBaseVertex, metaVertex, "False");

                senderButton.Off();
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        private void So_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {            
            LitButton senderButton = (LitButton)sender;
            IVertex trackBaseVertex = (IVertex)senderButton.Tag;

            bool isNull = false;
            bool isSolo = GraphUtil.GetBooleanValue(trackBaseVertex.Get(false, "IsSolo:"), ref isNull);

            IVertex metaVertex = m0.MinusZero.Instance.root.Get(false, @"System\Lib\Music\Track\IsSolo");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (!isSolo)
            {               
                GraphUtil.SetVertexValue(trackBaseVertex, metaVertex, "True");

                senderButton.On();
            }
            else
            {
                GraphUtil.SetVertexValue(trackBaseVertex, metaVertex, "False");

                senderButton.Off();
            }

            foreach (IEdge ee in baseVertex.GetAll(false, @"Track:\IsSolo:"))
                if(ee.To != trackBaseVertex.Get(false, "IsSolo:"))
                    ee.To.Value = "False";

            foreach (LitButton b in SoloButtons)
                if(b != senderButton)
                    b.Off();

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        private void Draw()
        {
            Children.Clear();

            Width = 120;

            List<TextBlock> tbl = new List<TextBlock>();

            MuteButtons = new List<LitButton>();
            SoloButtons = new List<LitButton>();

            foreach (AxisSegment s in Segments)
            {
                bool isSelected = false;

                if (Selection != null && ((AxisSegment)Selection).BaseVertex == s.BaseVertex)
                    isSelected = true;

                Border segment = new Border();

                WpfUtil.SetPosition(segment, 0, s.StartPosition, Width, s.EndPosition);

                segment.Background = new SolidColorBrush((Color)WpfUtil.FindResource("0Background"));

                Children.Add(segment);

                TextBlock text = new TextBlock();
                
                text.Text = s.BaseVertex.Value.ToString();

                text.Foreground = new SolidColorBrush((Color)WpfUtil.FindResource("0Foreground"));
                text.Background = new SolidColorBrush((Color)WpfUtil.FindResource("0Background"));

                Color segmentColor = (Color)WpfUtil.FindResource("0Background");

                Color trackColor = s.Color;

                if (s.BaseVertex.Get(false, "Color:") != null)
                {                    
                    Brush trackColorBrush = new SolidColorBrush(trackColor);

                    segment.Background = trackColorBrush;                    

                    text.Background = trackColorBrush;

                    text.Foreground = new SolidColorBrush(WpfUtil.GetNegativeColorWhiteOrBlack(trackColor));
                }

                if (segmentSize < 13)                        
                    text.Foreground = segment.Background;

                text.FontSize = FontSize;

                text.Height = segmentSize;

                text.Padding = new Thickness(2, 0, 3, 0);

                text.Width = 56;

                WpfUtil.SetPosition(text, 0, s.StartPosition);            

                Children.Add(text);

                //                

                Line l = new Line();

                WpfUtil.SetLinePosition(l, 0, s.StartPosition, Width, s.StartPosition);

                s.LineStyle.SetStyle(l);

                Children.Add(l);

                //

                DrawAdditionalSegmentControls(s);
            }

            Size newSize = new Size();
            newSize.Width = Width;            

            newSize.Height = Size.Height;

            Size = newSize;
            
            Height = Size.Height;

            //

            WpfUtil.DrawLine(this, 0, 0, 0, Size.Height, 1, (Brush)WpfUtil.FindResource("0ForegroundBrush"));

            WpfUtil.DrawLine(this, Size.Width, 0, Size.Width, Size.Height, 5, (Brush)WpfUtil.FindResource("0ForegroundBrush"));            

            //

            if(Segments.Count > 0)
            {
                Line ld = new Line();

                WpfUtil.SetLinePosition(ld, 0, Size.Height, Size.Width, Height);

                ld.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                Children.Add(ld);
            }            
        }

        private void Update()
        {
            Segments = new List<AxisSegment>();

            int cnt = 0;            

            double maxHeight = 0;            

            foreach(IEdge e in baseVertex.GetAll(false, "Track:"))                
            {                
                AxisSegment segment = new AxisSegment();

                segment.LineStyle = new LineStyle();

                segment.StartPosition = cnt * segmentSize;
                segment.EndPosition = (cnt + 1) * segmentSize;

                if (segment.EndPosition > maxHeight)
                    maxHeight = segment.EndPosition;

                segment.BaseVertex = e.To;
                segment.BaseEdge = e;

                //

                IVertex colorVertex = segment.BaseVertex.Get(false, "Color:");

                if (colorVertex != null)
                {
                    segment.Color = ColorHelper_desktop.GetColorFromColorVertex(colorVertex);

                    //segment.UseBackgroundColor = true;

                    //segment.BackgroundColor = segment.Color;
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

            segmentSize = 3 +  (15 * (zoomFactor / 40) );

            Update();
        }

        public void SetValueSpaceMax(double length)
        {

        }

        public TrackAxisDecorator()
        {
            //this.MouseDown += TrackAxisDecorator_MouseDown;
        }

        private void TrackAxisDecorator_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            foreach (AxisSegment s in Segments)
                if (s.StartPosition <= p.Y && p.Y <= s.EndPosition)
                {
                    if (Selection == s)
                        Selection = null;
                    else
                        Selection = s;

                    if(SelectionChanged!=null)
                        SelectionChanged(sender, null);

                    Draw();
                }
        }

        public event EventHandler SelectionChanged;

        public object Selection { get; set; }
    }
}
