using m0.Foundation;
using m0.Graph;
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
using System.Windows.Input;
using System.Windows.Forms.VisualStyles;
using m0.User.Process.UX;
using System.Windows.Data;

namespace m0.ZeroTypes.UX
{
    public class RectangleItem_LabeledItem : UXItem
    {        
        public RectangleItem_LabeledItem(IEdge edge) : base(edge) { }

        // BEG CODE for LabeledItem

        FrameworkElement LabelControl;

        TextBox textBox_forBaseEdge = null;        

        static IVertex BaseEdge_meta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");        

        protected virtual void UpdateLabelControl(FrameworkElement LabelControl)
        {

        }

        public override void BaseEdgeToUpdated()
        {
            base.BaseEdgeToUpdated();

            UpdateLabelControl(GetLabelControl());

        }

        public override void ViewAttributesUpdated()
        {
            base.ViewAttributesUpdated();

            UpdateLabelControl(GetLabelControl());
        }

        protected virtual void SetBaselineColors()
        {
            Brush foregroundBrush = GetForegroundBrush();

            GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", foregroundBrush);

            GeneralUtil.SetPropertyIfPresent(LabelControl, "Background", null);
        }

        public override void Select()
        {
            base.Select();

            GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", (Brush)FindResource("0BackgroundBrush"));

            GeneralUtil.SetPropertyIfPresent(LabelControl, "Cursor", Cursors.ScrollAll);

            if (textBox_forBaseEdge != null)
            {
                textBox_forBaseEdge.Background = (Brush)FindResource("0ForegroundBrush");
                textBox_forBaseEdge.Foreground = (Brush)FindResource("0BackgroundBrush");                                
            }
        }

        public override void Unselect()
        {
            Brush foregroundBrush = GetForegroundBrush();

            base.Unselect();

            SetBaselineColors();

            GeneralUtil.SetPropertyIfPresent(LabelControl, "Cursor", Cursors.Arrow);

            if (textBox_forBaseEdge != null)
            {
                textBox_forBaseEdge.Background = null;
                textBox_forBaseEdge.Foreground = foregroundBrush;
            }
        }

        public override void Highlight()
        {
            base.Highlight();

            Brush backgroundBrush = GetBackgroundBrush();

            Brush foregroundBrush = GetForegroundBrush();

            if (UseCodeLabel)
            {
                GeneralUtil.SetPropertyIfPresent(LabelControl, "Background", backgroundBrush);

                GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", foregroundBrush);
            }
            else
            {
                GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", (Brush)FindResource("0HighlightForegroundBrush"));

                if (textBox_forBaseEdge != null)
                {
                    textBox_forBaseEdge.Background = null;
                    textBox_forBaseEdge.Foreground = (Brush)FindResource("0HighlightForegroundBrush");
                }
            }
        }

        IEdge BaseEdge_forLabel;

        public FrameworkElement GetLabelControl()
        {
            BaseEdge_forLabel = BaseEdge;

            string labelQuery = ContentQuery;

            if (labelQuery != null)
                BaseEdge_forLabel = BaseEdge.To.GetAll(false, labelQuery).FirstOrDefault();

            //

            StackPanel stack = new StackPanel();
            stack.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Orientation = Orientation.Horizontal;

            if (BaseEdge_forLabel == null)
            {
                LabelControl = stack;

                return stack;
            }

            string constantLabel = ConstantLabel;

            if (constantLabel != null)
            {
                TextBlock constantTextBlock = GetTextBlock(HorizontalAlignment.Center);

                constantTextBlock.FontStyle = FontStyles.Italic;

                constantTextBlock.Text = constantLabel;

                stack.Children.Add(constantTextBlock);

                //

                TextBlock dividerTextBlock = GetTextBlock(HorizontalAlignment.Center);

                dividerTextBlock.Text = " | ";

                stack.Children.Add(dividerTextBlock);
            }

            if (!HideLabel)
                stack.Children.Add(GetLabelControl_RightPart());

            LabelControl = stack;

            return stack;
        }

        public string GetLabel_Left()
        {
            StringBuilder label = new StringBuilder();

            if (ShowMeta && BaseEdge_forLabel.Meta.Value.ToString() != "$Empty")
            {
                if (BaseEdge_forLabel.Meta.Value == null)
                    label.Append("Ø");
                else
                    label.Append(BaseEdge_forLabel.Meta.Value.ToString());

                label.Append(" :: ");
            }

            return label.ToString();
        }

        public string GetLabel_Right()
        {
            StringBuilder label = new StringBuilder();

            if (BaseEdge_forLabel.To.Value == null)
                label.Append("Ø");
            else
                label.Append(BaseEdge_forLabel.To.Value.ToString());

            return label.ToString();
        }

        FrameworkElement GetLabelControl_RightPart()
        {
            FrameworkElement labelControl;

            if (UseCodeLabel)
                labelControl = GetLabelControl_Code();
            else
                labelControl = GetLabelControl_Text();

            labelControl.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            labelControl.HorizontalAlignment = HorizontalAlignment.Center;

            return labelControl;
        }

        IVertex Vertex_forLabel = null;

        public FrameworkElement GetLabelControl_Code()
        {
            CodeControl codeControl;

            codeControl = new CodeControl(Vertex, true);

            codeControl.BaseEdgeToUpdated();

            return codeControl;
        }

        private TextBlock GetTextBlock(HorizontalAlignment horlizontalAlignment)
        {
            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = horlizontalAlignment;
            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;

            if (FontSize != 0)
                textBlock.FontSize = this.FontSize;

            return textBlock;
        }

        private void GetTextBox(HorizontalAlignment horlizontalAlignment)
        {            
            textBox_forBaseEdge = new TextBox();
            
            textBox_forBaseEdge.BorderThickness = new Thickness(0);
            textBox_forBaseEdge.Background = null;
            textBox_forBaseEdge.HorizontalAlignment = horlizontalAlignment;
            textBox_forBaseEdge.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            textBox_forBaseEdge.TextWrapping = TextWrapping.Wrap;

            if (FontSize != 0)
                textBox_forBaseEdge.FontSize = this.FontSize;

            textBox_forBaseEdge.PreviewMouseMove += TextBox_forBaseEdge_MouseMove;
            textBox_forBaseEdge.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
            textBox_forBaseEdge.PreviewMouseLeftButtonUp += TextBox_forBaseEdge_PreviewMouseLeftButtonUp;
            textBox_forBaseEdge.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseLeftButtonDownHandler(sender, e);

            e.Handled = false;
        }

        private void TextBox_forBaseEdge_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool SuspendSetFocus_prev = OwningVisualiser.SuspendSetFocus;

            OwningVisualiser.SuspendSetFocus = true;

            this.OwningVisualiser.MouseButtonUpHandler(sender, e);

            OwningVisualiser.SuspendSetFocus = SuspendSetFocus_prev;

            e.Handled = false;
        }

        private void TextBox_forBaseEdge_MouseMove(object sender, MouseEventArgs e)
        {
            OwningVisualiser.MouseMoveHandler(sender, e);
        }

        string TextBox_TextChanged_memory = null;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newText = textBox_forBaseEdge.Text;

            if (TextBox_TextChanged_memory == null || TextBox_TextChanged_memory != newText)
            {
                bool ForceVertexChangeOff_prev = ForceVertexChangeOff;

                ForceVertexChangeOff = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                BaseEdge.To.Value = newText;

                //////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                //////////////////////////////////////

                ForceVertexChangeOff = ForceVertexChangeOff_prev;

                TextBox_TextChanged_memory = newText;
            }
        }        

        private FrameworkElement GetLabelControl_Text()
        {
            string leftText = GetLabel_Left();
            string rightText = GetLabel_Right();

            GetTextBox(HorizontalAlignment.Center);
            textBox_forBaseEdge.Text = rightText;

            if (leftText == "")
            {
                return textBox_forBaseEdge;
            }
            else
            {
                StackPanel stack = new StackPanel();
                stack.HorizontalAlignment = HorizontalAlignment.Center;
                stack.Orientation = Orientation.Horizontal;

                TextBlock left = GetTextBlock(HorizontalAlignment.Center);
                left.Text = leftText;

                stack.Children.Add(left);
                stack.Children.Add(textBox_forBaseEdge);

                return stack;
            }
        }

        public new bool IsDisposed = false;

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                if (Vertex_forLabel != null)
                    Vertex_forLabel.RemoveExternalReference();

                base.Dispose();
            }
        }

        // END CODE for LabeledItem 

        // UNDER for RectangleItem

        static IVertex RoundEdgeSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\RoundEdgeSize");
        static IVertex HideHeader_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\HideHeader");

        public int RoundEdgeSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetIntegerValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "RoundEdgeSize", null);

                if (val == null)
                    val = Vertex.AddVertex(RoundEdgeSize_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool HideHeader
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideHeader", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "HideHeader", null);

                if (val == null)
                    val = Vertex.AddVertex(HideHeader_meta, value);
                else
                    val.Value = value;
            }
        }

        // UNDER for LabeledItem

        static IVertex FontSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\FontSize");
        static IVertex ConstantLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ConstantLabel");
        static IVertex ContentQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ContentQuery");
        static IVertex UseCodeLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\UseCodeLabel");
        static IVertex FormalTextLanguageProcessing_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\FormalTextLanguageProcessing");
        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ShowMeta");
        static IVertex HideLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\HideLabel");


        public new double FontSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "FontSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "FontSize", null);

                if (val == null)
                    val = Vertex.AddVertex(FontSize_meta, value);
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

                return GraphUtil.GetStringValue(val);
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

        public string ContentQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContentQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContentQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(ContentQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool UseCodeLabel
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "UseCodeLabel", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "UseCodeLabel", null);

                if (val == null)
                    val = Vertex.AddVertex(UseCodeLabel_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool ShowMeta
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ShowMeta", null);

                if (val == null)
                    val = Vertex.AddVertex(ShowMeta_meta, value);
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

        public IVertex FormalTextLanguageProcessing
        {
            get
            {
                return GraphUtil.GetQueryOutFirst(Vertex, "FormalTextLanguageProcessing", null);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, FormalTextLanguageProcessing_meta, value);
            }
        }


    }
}
