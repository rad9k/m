using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using m0.Graph;
using m0.Foundation;
using m0.ZeroTypes;
using m0.Util;
using System.Xml.Linq;
using m0.User.Process.UX;
using m0.UIWpf.Controls;
using System.Windows.Forms;

namespace m0.ZeroTypes.UX
{
    /// <summary>
    /// Interaction logic for DiagramRectangleItem.xaml
    /// </summary>
    public partial class CodeItem : RectangleItem_LabeledItem
    {
        CodeControl codeControl;
        bool codeControlAdded = false;

        //

        static string[] _SubVertexesTriggeringItemVisualUpdate = new string[] {
            "RoundEdgeSize", "HideHeader", "BorderSize", "ConstantLabel", "LabelQuery", "ShowMeta", "UseCodeLabel", "ContentQuery", "FontSize", "FormalTextLanguage", "CodeRepresentation", "ShowMeta", "HideLabel"};
        public override string[] SubVertexesTriggeringItemVisualUpdate { get { return _SubVertexesTriggeringItemVisualUpdate; } }

        //        

        public CodeItem() : base(new ZeroTypes.Edge(null))
        {
            InitializeComponent();
        }

        public CodeItem(IEdge edge) : base(edge) {
            InitializeComponent();
        }

        protected override void UpdateLabelControl(FrameworkElement LabelControl)
        {
            LabelContainer.Child = LabelControl;
        }

        public override void VertexSetedUp()
        {
            base.VertexSetedUp();            

            AddCodeControl();
        }

        void AddCodeControl()
        {
            if (codeControl != null && !codeControlAdded)
            {
                Grid.SetRow(codeControl, 2);

                codeControl.Padding = new Thickness(1, 0, 1, 0);

                TheGrid.Children.Add(codeControl);

                codeControlAdded = true;
            }
        }

        public override void BaseEdgeToUpdated()
        {
            base.BaseEdgeToUpdated();

            if (codeControl != null)
            {
                TheGrid.Children.Remove((UIElement)codeControl);
                codeControlAdded = false;
            }

            codeControl = new CodeControl(Vertex, true);
            //codeControl.NoBackgroundWorkOnGenerate = true;
            codeControl.GenerateAfterParse = false;
            codeControl.BaseEdgeToUpdated();

            AddCodeControl();
        }

        public override void ViewAttributesUpdated()
        {
            base.ViewAttributesUpdated();            

            //

            if (codeControl != null)
                codeControl.ViewAttributesUpdated();

            //         
            
            double BorderSize_nonZero = BorderSize; ;

            if (BorderSize_nonZero == 0)
                BorderSize_nonZero = 1;

            this.Frame.BorderThickness = new Thickness(BorderSize_nonZero);

            int headerHeight;

            if (HideHeader)
            {                                                                                                                                                                                                 
                this.TheGrid.RowDefinitions[0].Height = new GridLength(0);
                this.TheGrid.RowDefinitions[1].Height = new GridLength(0);

                headerHeight = 0;
            }
            else
            {
                headerHeight = 16;

                this.TheGrid.RowDefinitions[0].Height = new GridLength(headerHeight);
                this.TheGrid.RowDefinitions[1].Height = new GridLength(BorderSize_nonZero);
            }

            //

            
            this.Frame.CornerRadius = new CornerRadius(RoundEdgeSize);


            this.LabelContainer.Margin = new Thickness(RoundEdgeSize, RoundEdgeSize, RoundEdgeSize, 1);

            ((FrameworkElement)this.codeControl).Margin = new Thickness(RoundEdgeSize, 0, RoundEdgeSize, RoundEdgeSize);

            
            TheGrid.RowDefinitions[0].Height = new GridLength(headerHeight + RoundEdgeSize);
                       
            SetBaselineColors();
        }        

        protected override void SetBaselineColors()
        {
            base.SetBaselineColors();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            Brush borderBrush = GetBorderBrush();


            this.Frame.Background = backgroundBrush;

            this.Foreground = foregroundBrush;

            this.InternalFrame.Background = borderBrush;

            this.Frame.BorderBrush = borderBrush;

            if (codeControl != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(codeControl, "Foreground", foregroundBrush);
                GeneralUtil.SetPropertyIfPresent(codeControl, "Background", backgroundBrush);
            }
        }

        public override void Select()
        {
            base.Select();

            this.InternalFrame.Background = (Brush)FindResource("0SelectionBrush");
            this.Frame.BorderBrush = (Brush)FindResource("0SelectionBrush");

            this.Foreground = (Brush)FindResource("0BackgroundBrush");

            this.Frame.Background = (Brush)FindResource("0SelectionBrush");

            //

            if (codeControl != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(codeControl, "Foreground", (Brush)FindResource("0BackgroundBrush"));
                GeneralUtil.SetPropertyIfPresent(codeControl, "Background", (Brush)FindResource("0ForegroundBrush")); 
            }
        }

        public override void Highlight()
        {
            base.Highlight();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();
            
            this.InternalFrame.Background = (Brush)FindResource("0HighlightBrush");
            this.Frame.BorderBrush = (Brush)FindResource("0HighlightBrush");

            this.Foreground = (Brush)FindResource("0HighlightForegroundBrush"); 
           
            this.Frame.Background = (Brush)FindResource("0HighlightBrush");

            if (codeControl != null) // not always works, but can
            {
                GeneralUtil.SetPropertyIfPresent(codeControl, "Foreground", foregroundBrush);
                GeneralUtil.SetPropertyIfPresent(codeControl, "Background", backgroundBrush);
            }
        }     
    }
}