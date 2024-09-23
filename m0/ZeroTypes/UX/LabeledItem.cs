using m0.Foundation;
using m0.Graph;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using m0.UIWpf.Controls;
using m0.Util;
using System.Windows.Media;
using System.Windows.Input;

namespace m0.ZeroTypes.UX
{
    public class LabeledItem : UXItem
    {
        // CODE for LabeledItem

        static IVertex BaseEdge_meta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");

        protected FrameworkElement LabelControl;

        public override void BaseEdgeToUpdated()
        {
            base.BaseEdgeToUpdated();

            LabelControl = GetLabelControl();
        }

        public override void ViewAttributesUpdated()
        {
            base.ViewAttributesUpdated();

            LabelControl = GetLabelControl();
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
        }

        public override void Unselect()
        {
            base.Unselect();

            SetBaselineColors();

            GeneralUtil.SetPropertyIfPresent(LabelControl, "Cursor", Cursors.Arrow);
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
                GeneralUtil.SetPropertyIfPresent(LabelControl, "Foreground", (Brush)FindResource("0HighlightForegroundBrush"));
        }

        IEdge BaseEdge_forLabel;

        public FrameworkElement GetLabelControl()
        {
            BaseEdge_forLabel = BaseEdge;

            string labelQuery = LabelQuery;

            if (labelQuery != null)
                BaseEdge_forLabel = BaseEdge.To.GetAll(false, labelQuery).FirstOrDefault();

            //

            StackPanel stack = new StackPanel();
            stack.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Orientation = Orientation.Horizontal;

            if (BaseEdge_forLabel == null)
                return stack;

            string constantLabel = ConstantLabel;

            if (constantLabel != null)
            {
                TextBlock constantTextBlock = getTextBlock(HorizontalAlignment.Center);

                constantTextBlock.FontStyle = FontStyles.Italic;

                constantTextBlock.Text = constantLabel;

                stack.Children.Add(constantTextBlock);

                //

                TextBlock dividerTextBlock = getTextBlock(HorizontalAlignment.Center);

                dividerTextBlock.Text = " | ";

                stack.Children.Add(dividerTextBlock);
            }

            if (!HideLabel)
                stack.Children.Add(GetLabelControl_RightPart());

            return stack;
        }

        public string GetLabel()
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
                labelControl = GetLabelControl_TextBlock();

            labelControl.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            labelControl.HorizontalAlignment = HorizontalAlignment.Center;

            return labelControl;
        }

        IVertex Vertex_forLabel = null;

        public FrameworkElement GetLabelControl_Code()
        {
            CodeControl codeControl;

            if (LabelQuery != null)
            {
                if (Vertex_forLabel == null)
                {
                    Vertex_forLabel = MinusZero.Instance.CreateTempVertex();
                    Vertex_forLabel.AddExternalReference();
                }
                else
                    GraphUtil.RemoveAllEdges(Vertex_forLabel);

                GraphUtil.CopyShallow(Vertex, Vertex_forLabel);

                EdgeHelper.CreateOrReplaceEdgeVertexFromIEdgeByMeta(Vertex_forLabel, BaseEdge_meta, BaseEdge_forLabel);

                codeControl = new CodeControl(Vertex_forLabel, true);
            }else
                codeControl = new CodeControl(Vertex, true);



            codeControl.BaseEdgeToUpdated();

            return codeControl;
        }

        private TextBlock getTextBlock(HorizontalAlignment horlizontalAlignment)
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

        private FrameworkElement GetLabelControl_TextBlock()
        {
            TextBlock textBlock = getTextBlock(HorizontalAlignment.Center);

            textBlock.Text = GetLabel();
            return textBlock;
        }

        public bool IsDisposed = false;

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                if (Vertex_forLabel != null)
                    Vertex_forLabel.RemoveExternalReference();
            }
        }

        // UNDER for LabeledItem

        static IVertex FontSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\FontSize");
        static IVertex ConstantLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ConstantLabel");
        static IVertex LabelQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\LabelQuery");        
        static IVertex UseCodeLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\UseCodeLabel");
        static IVertex FormalTextLanguage_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\FormalTextLanguage");
        static IVertex CodeRepresentation_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\CodeRepresentation");
        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ShowMeta");
        static IVertex HideLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\HideLabel");
        

        public LabeledItem(IEdge edge) : base(edge) { }

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

        public string LabelQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "LabelQuery", null);

                if (val == null)
                    return null;

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "LabelQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(LabelQuery_meta, value);
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

        public CodeRepresentationEnum CodeRepresentation
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "CodeRepresentation", null);

                return CodeRepresentationEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, CodeRepresentation_meta, CodeRepresentationEnumHelper.GetVertex(value));
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

        public IVertex FormalTextLanguage
        {
            get
            {
                return GraphUtil.GetQueryOutFirst(Vertex, "FormalTextLanguage", null);                
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, FormalTextLanguage_meta, value);
            }
        }

    }
}
