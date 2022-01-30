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

namespace m0.UIWpf.Visualisers
{
    public class CodeVisualiser : TextEditor, IListVisualiser, IOwnScrolling
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        IList<string> TextMemory;

        static string[] _MetaTriggeringBaseEdgeUpdate = new string[] { };
        public string[] MetaTriggeringUpdateBaseEdge { get { return _MetaTriggeringBaseEdgeUpdate; } }

        static string[] _MetaTriggeringUpdateViewSettings = new string[] {"ShowWhiteSpace", "ShowLineNumbers", "HighlightedLine"  };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateViewSettings; } }

        public void UpdateView() { UpdateEditView(); }

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

            //((ListVisualiserHelper)VisualiserHelper).CustomVertexChangeEvent += CustomVertexChange;

            SetVertexDefaultValues();

            ZoomVisualiserContentChange();

            TextMemory = new List<string>();

            EditSetup();

            UpdateEditView();

            this.PreviewKeyDown += CodeVisualiser_KeyDown;
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            //VisualiserHelper.AddContextMenu();
        }

        public void SelectedVerticesUpdated() { }

        private void ExecuteParse()
        {            
            TextMemory.Add(Text);

            MinusZero.Instance.DefaultParser.Parse(Vertex.Get(false, @"BaseEdge:\To:"), Text);

            int currentTextMemory = TextMemory.Count;

            Vertex.Get(false, "TextMemoryMax:").Value = currentTextMemory;
            Vertex.Get(false, "TextMemoryCurrent:").Value = currentTextMemory;
        }

        private void ReferenceTextMemoryLeft()
        {            
            int TextMemoryCurrent = (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryCurrent:"));        

            if (TextMemoryCurrent > 0)
            {
                if (TextMemoryCurrent > 1)
                    TextMemoryCurrent--;

                Text = TextMemory[TextMemoryCurrent - 1];

                Vertex.Get(false, "TextMemoryCurrent:").Value = TextMemoryCurrent;
            }            
        }

        private void ReferenceTextMemoryRight()
        {
            int TextMemoryMax = (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryMax:"));
            int TextMemoryCurrent = (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryCurrent:"));            

            if (TextMemoryCurrent < TextMemoryMax)
            {
                TextMemoryCurrent++;

                Text = TextMemory[TextMemoryCurrent - 1];

                Vertex.Get(false, "TextMemoryCurrent:").Value = TextMemoryCurrent;
            }
        }

        private void CodeVisualiser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ExecuteParse();

            if (e.Key == Key.Left && Keyboard.IsKeyDown(Key.RightAlt))
                ReferenceTextMemoryLeft();

            if (e.Key == Key.Right && Keyboard.IsKeyDown(Key.RightAlt))
                ReferenceTextMemoryRight();
        }

        TabFoldingStrategy foldingStrategy;
        FoldingManager foldingManager;

        void UpdateEditView()
        {
            if(GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, @"ShowWhiteSpace:"),"True"))
                Options.ShowTabs = true;
            else
                Options.ShowTabs = false;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, @"ShowLineNumbers:"), "True"))
                this.ShowLineNumbers = true;
            else
                this.ShowLineNumbers = false;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, @"HighlightedLine:"), "True"))
                Options.HighlightCurrentLine = true;
            else
                Options.HighlightCurrentLine = false;
        }
        void EditSetup()
        {
            UpdateEditView();

            this.FontFamily = new FontFamily("Consolas");
            this.FontWeight = FontWeight.FromOpenTypeWeight(1);
            
            Foreground = new SolidColorBrush(Color.FromRgb(0X2B, 0X91, 0XAF));

            this.LineNumbersForeground = new SolidColorBrush(Colors.LightGray);

            foldingManager = FoldingManager.Install(TextArea);
            foldingStrategy = new TabFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, Document);

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
            foldingUpdateTimer.Start();

            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(m0.MinusZero).Assembly.GetManifestResourceStream("m0.ZeroCodeHighlighting.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            SyntaxHighlighting = customHighlighting;
        }

        void UpdateFoldings()
        {           
            foldingStrategy.UpdateFoldings(foldingManager, Document);            
        }

        protected virtual void SetVertexDefaultValues()
        {
            Vertex.Get(false, "ZoomVisualiserContent:").Value = 15.0;
            Vertex.Get(false, "ShowWhiteSpace:").Value = "False";
            Vertex.Get(false, "ShowLineNumbers:").Value = "False";
            Vertex.Get(false, "HighlightedLine:").Value = "True";
            Vertex.Get(false, "TextMemoryCurrent:").Value = 0;
            Vertex.Get(false, "TextMemoryMax:").Value = 0;
        }

        public void UpdateVertex()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null /*&& bv.Value != null && ((String)bv.Value)!="$Empty"*/)
            {
                EdgeBase ee = new EdgeBase(Vertex.Get(false, @"BaseEdge:\From:"), Vertex.Get(false, @"BaseEdge:\Meta:"), Vertex.Get(false, @"BaseEdge:\To:"));
                this.Text = this.Text = MinusZero.Instance.DefaultCodeGenerator.Generate(ee);
            }
            else
                this.Text = "Ø";
        }

        public void ZoomVisualiserContentChange()
        {
            double scale = ((double)GraphUtil.GetDoubleValue(Vertex.Get(false, "ZoomVisualiserContent:")));

            this.FontSize = scale;
        }

        protected INoInEdgeInOutVertexVertex CustomVertexChange(IExecution exe)
        {
            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                if (GraphUtil.ExistQueryIn(changedVertex, "ZoomVisualiserContent", null))
                {
                    ZoomVisualiserContentChange();
                    return exe.Stack;
                }

                if (GraphUtil.ExistQueryIn(changedVertex, "ShowWhiteSpace", null) 
                    || GraphUtil.ExistQueryIn(changedVertex, "ShowLineNumbers", null)
                    || GraphUtil.ExistQueryIn(changedVertex, "HighlightedLine", null))
                {
                    UpdateEditView();
                    return exe.Stack;
                }                
            }            

            UpdateVertex();

            return exe.Stack;
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

        public IVertex GetEdgeByLocation(System.Windows.Point point)
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
