using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using System.Windows.Media;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Commands;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using m0.UIWpf.Visualisers.Code;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Xml;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.ZeroCode;

namespace m0.UIWpf.Visualisers
{
    public class CodeVisualiser : Border, IListVisualiser, IOwnScrolling
    {
        CodeControl codeControl;

        //

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        static string[] _MetaTriggeringUpdateVertex = new string[] { };
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] {"ShowWhiteSpace", "ShowLineNumbers", "HighlightedLine", "FontSize" };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void UpdateView() { codeControl.UpdateEditView(); }

        public void UnselectAllSelectedEdges() { }

        public CodeVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            new ListVisualiserHelper(parentVisualiser,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Code"),
                this,
                "CodeVisualiser",
                this,
                false,
                new List<string> { @"", @"BaseEdge:\To:" },
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

            codeControl = new CodeControl(Vertex, false, false);

            this.Child = codeControl;

            UpdateVertex();
        }

        public void OnLoad(object sender, RoutedEventArgs e) { }

        public void SelectedVerticesUpdated() { }

        public void UpdateVertex()
        {
            if (codeControl != null)
                codeControl.UpdateVertex();
        }

        public void ScaleChange() {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        public void Dispose()
        {
            VisualiserHelper.Dispose();
        }      

        public IVertex GetEdgeByPoint(System.Windows.Point point)
        {
            return Vertex.Get(false, @"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(System.Windows.FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public System.Windows.FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }
    }
}
