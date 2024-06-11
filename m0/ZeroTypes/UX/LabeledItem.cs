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

namespace m0.ZeroTypes.UX
{
    public class LabeledItem : UXItem
    {
        // CODE for LabeledItem

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
                label.Append(edge.Meta.Value.ToString());
                label.Append(" :: ");
            }

            label.Append(edge.To.Value.ToString());

            return label.ToString();
        }

        public FrameworkElement GetLabelControl()
        {
            if (UseCodeLabel)
                return GetLabelControl_Code();
            else
                return GetLabelControl_TextBlock();
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


            //CodeControl code = new CodeControl();


            return null;
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
