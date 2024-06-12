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

        protected FrameworkElement LabelControl;

        public override void ItemVisualUpdate()
        {
            base.ItemVisualUpdate();

            LabelControl = GetLabelControl();

            if (LabelControl is CodeControl)
                ((CodeControl)LabelControl).UpdateVertex();
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

        public string GetLabel()
        {
            StringBuilder label = new StringBuilder();

            string constantLabel = ConstantLabel;

            if (constantLabel != null)
            {
                label.Append(constantLabel);
                label.Append(" | ");
            }

            IEdge edge = BaseEdge;

            string labelQuery = LabelQuery;

            if (labelQuery != null)
            {
                edge = edge.To.GetAll(false, labelQuery).FirstOrDefault();

                if (edge == null)
                    return "[empty query result]";
            }

            if (ShowMeta && edge.Meta.Value.ToString() != "$Empty")
            {
                if (edge.Meta.Value == null)
                    label.Append("Ø");
                else
                    label.Append(edge.Meta.Value.ToString());

                label.Append(" :: ");
            }

            if (edge.To.Value == null)
                label.Append("Ø");
            else
                label.Append(edge.To.Value.ToString());

            return label.ToString();
        }

        public FrameworkElement GetLabelControl()
        {
            FrameworkElement labelControl;

            if (UseCodeLabel)
                labelControl = GetLabelControl_Code();
            else
                labelControl = GetLabelControl_TextBlock();

            labelControl.VerticalAlignment = VerticalAlignment.Center;
            labelControl.HorizontalAlignment = HorizontalAlignment.Center;

            return labelControl;
        }

        public FrameworkElement GetLabelControl_Code()
        {
            IEdge edge = BaseEdge;

            string labelQuery = LabelQuery;

            if (labelQuery != null)
            {
                edge = edge.To.GetAll(false, labelQuery).FirstOrDefault();

                if (edge == null)
                {
                    TextBlock textBlock = getTextBlock();
                    textBlock.Text = "[empty query result]";
                    return textBlock;
                }
            }


            CodeControl codeControl = new CodeControl(Vertex, true);
            
            return codeControl;
        }

        private TextBlock getTextBlock()
        {
            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;

            return textBlock;
        }

        public FrameworkElement GetLabelControl_TextBlock()
        {
            TextBlock textBlock = getTextBlock();

            if (HideLabel)
                textBlock.Text = "";
            else
                textBlock.Text = GetLabel();

            return textBlock;
        }


        // UNDER

        static IVertex ConstantLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ConstantLabel");
        static IVertex LabelQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\LabelQuery");        
        static IVertex UseCodeLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\UseCodeLabel");
        static IVertex FormalTextLanguage_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\FormalTextLanguage");
        static IVertex ShowMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\ShowMeta");
        static IVertex HideLabel_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LabeledItem\HideLabel");
        

        public LabeledItem(IEdge edge) : base(edge) { }

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
