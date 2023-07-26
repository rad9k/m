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
    public class CodeControl : TextEditor
    {
        public bool BaseEdgeInsteadBaseVertex;

        public IVertex Vertex;

        public bool NoVertexForTextMemory = false;

        //

        IList<string> TextMemory;

        public void UpdateView() { UpdateEditView(); }

        public void UnselectAllSelectedEdges() { }

        public CodeControl(IVertex _Vertex, bool _NoVertexForTextMemory, bool _BaseEdgeInsteadBaseVertex)
        {
            Vertex = _Vertex;

            NoVertexForTextMemory = _NoVertexForTextMemory;

            BaseEdgeInsteadBaseVertex = _BaseEdgeInsteadBaseVertex;

            //

            SetVertexDefaultValues();

            TextMemory = new List<string>();

            EditSetup();

            UpdateEditView();

            this.PreviewKeyDown += CodeVisualiser_KeyDown;
        }

        int _TextMemoryMax;
        int TextMemoryMax
        {
            get {
                if (NoVertexForTextMemory)
                    return _TextMemoryMax;
                else
                    return (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryMax:"));
            }

            set {
                if (NoVertexForTextMemory)
                    _TextMemoryMax = value;
                else
                {
                    ////////////////////////////////////////
                    Interaction.BeginInteractionWithGraph();
                    ////////////////////////////////////////
                    
                    Vertex.Get(false, "TextMemoryMax:").Value = value;

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////
                }
            }
        }

        int _TextMemoryCurrent;
        int TextMemoryCurrent
        {
            get
            {
                if (NoVertexForTextMemory)
                    return _TextMemoryCurrent;
                else
                    return (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryCurrent:"));
            }

            set
            {
                if (NoVertexForTextMemory)
                    _TextMemoryCurrent = value;
                else
                {
                    ////////////////////////////////////////
                    Interaction.BeginInteractionWithGraph();
                    ////////////////////////////////////////
                    
                    Vertex.Get(false, "TextMemoryCurrent:").Value = value;

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////
                }
            }
        }

        private void ExecuteParse()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            TextMemory.Add(Text);            

            IVertex BaseEdgeToVertex = Vertex.Get(false, @"BaseEdge:\To:");            

            MinusZero.Instance.DefaultParser.Parse(BaseEdgeToVertex, Text);

            int currentTextMemory = TextMemory.Count;

            TextMemoryMax = currentTextMemory;
            TextMemoryCurrent = currentTextMemory;
            
            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        private void ReferenceTextMemoryLeft()
        {                   
            if (TextMemoryCurrent > 0)
            {
                if (TextMemoryCurrent > 1)
                    TextMemoryCurrent--;

                Text = TextMemory[TextMemoryCurrent - 1];
            }
        }

        private void ReferenceTextMemoryRight()
        {
            if (TextMemoryCurrent < TextMemoryMax)
            {
                TextMemoryCurrent++;

                Text = TextMemory[TextMemoryCurrent - 1];
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

        public void UpdateEditView()
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

            //

            double fontSize = ((double)GraphUtil.GetDoubleValue(Vertex.Get(false, "FontSize:")));

            this.FontSize = fontSize;
        }

        void EditSetup()
        {
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

        public virtual void SetVertexDefaultValues()
        {
            TextMemoryCurrent = 0;
            TextMemoryMax = 0;
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
    }
}
