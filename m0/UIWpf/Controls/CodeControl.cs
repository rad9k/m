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
using Xceed.Wpf.Toolkit.Core.Converters;
using ICSharpCode.AvalonEdit.Editing;
using System.Threading;
using m0.UIWpf.Dialog;
using System.Diagnostics;

namespace m0.UIWpf.Controls
{
    public class CodeControl : Border
    {
        static object lockObject = new object();

        public bool GenerateAfterParse = true;        

        public IVertex Vertex;

        public bool NoVertexForTextMemory = false;        

        //

        TextEditor editor = new TextEditor();

        bool doNotParse = false;

        

        //

        IList<string> TextMemory;

        public void UpdateView() { ViewAttributesUpdated(); }

        public void UnselectAllSelectedEdges() { }

        public CodeControl(IVertex _Vertex, bool _NoVertexForTextMemory)
        {
            Vertex = _Vertex;

            NoVertexForTextMemory = _NoVertexForTextMemory;            

            //

            SetVertexDefaultValues();

            TextMemory = new List<string>();

            EditSetup();

            ViewAttributesUpdated();

            this.PreviewKeyDown += CodeVisualiser_KeyDown;

            this.Child = editor;

            editor.Background = null;
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

        string editor_Text;

        private void ExecuteParse()
        {            
            editor_Text = editor.Text;

            Thread thread = new Thread(ExecuteParse_SeparateThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private void ExecuteParse_SeparateThread()
        {
            lock (lockObject)
            {
                if (Vertex.DisposedState != DisposeStateEnum.Live)
                    return;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                TextMemory.Add(editor_Text);

                IVertex BaseEdgeToVertex = Vertex.Get(false, @"BaseEdge:\To:");

                IVertex ftl = GraphUtil.GetQueryOutFirst(Vertex, "FormalTextLanguage", null);

                //

                m0Main.Instance.Dispatcher.Invoke(() =>
                {
                    editor.Background = (Brush)FindResource("0ProcessingBrush");
                });

                //

                IVertex errorList;

                if (ftl == null)
                    errorList = MinusZero.Instance.DefaultFormalTextParser.Parse(BaseEdgeToVertex, editor_Text);
                else
                    errorList = MinusZero.Instance.DefaultFormalTextParser.Parse(ftl, BaseEdgeToVertex, editor_Text);

                //

                int errorLine = -1;

                string generated = null;

                if (errorList.OutEdges.Count == 0 && GenerateAfterParse)
                    generated = ExecuteGenerate_SeparateThread_internal();

                m0Main.Instance.Dispatcher.Invoke(() =>
                {
                    if (errorList.OutEdges.Count == 0)
                    {
                        if (generated != null)
                            editor.Text = generated;

                        //editor.Background = (Brush)FindResource("0BackgroundBrush");

                        editor.Background = null;
                    }
                    else
                    {
                        editor.Background = (Brush)FindResource("0LightErrorBrush");

                        errorLine = GraphUtil.GetIntegerValueOr0(errorList.OutEdges[0].To.Get(false, "Where:"));
                    }
                });

                //

                int currentTextMemory = TextMemory.Count;

                TextMemoryMax = currentTextMemory;
                TextMemoryCurrent = currentTextMemory;

                m0Main.Instance.Dispatcher.Invoke(() =>
                {
                    doNotParse = true;

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////

                    doNotParse = false;

                    if (errorLine != -1)
                        editor.TextArea.Caret.Line = errorLine + 1;
                });
            }
        }

        private void ReferenceTextMemoryLeft()
        {                   
            if (TextMemoryCurrent > 0)
            {
                if (TextMemoryCurrent > 1)
                    TextMemoryCurrent--;

                editor.Text = TextMemory[TextMemoryCurrent - 1];
            }
        }

        private void ReferenceTextMemoryRight()
        {
            if (TextMemoryCurrent < TextMemoryMax)
            {
                TextMemoryCurrent++;

                editor.Text = TextMemory[TextMemoryCurrent - 1];
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
        DispatcherTimer foldingUpdateTimer;

        public void ViewAttributesUpdated()
        {
            if(GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, @"ShowWhiteSpace:"),"True"))
                editor.Options.ShowTabs = true;
            else
                editor.Options.ShowTabs = false;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, @"ShowLineNumbers:"), "True"))
                editor.ShowLineNumbers = true;
            else
                editor.ShowLineNumbers = false;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, @"HighlightedLine:"), "True"))
                editor.Options.HighlightCurrentLine = true;
            else
                editor.Options.HighlightCurrentLine = false;

            //

            double? fontSize = GraphUtil.GetDoubleValue(Vertex.Get(false, "FontSize:"));

            if (fontSize == null)
                editor.FontSize = 15;
            else
                editor.FontSize = (double)fontSize;

            //

            bool showFolding = GraphUtil.GetBooleanValueOrFalse(GraphUtil.GetQueryOutFirst(Vertex, "ShowFolding", null));

            if (showFolding)
            {
                if (foldingManager == null)
                    foldingManager = FoldingManager.Install(editor.TextArea);

                foldingStrategy = new TabFoldingStrategy();
                foldingStrategy.UpdateFoldings(foldingManager, editor.Document);

                if (foldingUpdateTimer == null)
                    foldingUpdateTimer = new DispatcherTimer();
                foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
                foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
                foldingUpdateTimer.Start();
            }
            else
            {      
                if(foldingManager != null)
                    foldingManager.Clear();
                
                if (foldingUpdateTimer != null)
                    foldingUpdateTimer.Stop();
            }

        }

        void EditSetup()
        {
            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            editor.Background = new SolidColorBrush(Colors.Green);

            editor.FontFamily = new FontFamily("Consolas");
            editor.FontWeight = FontWeight.FromOpenTypeWeight(1);

            editor.Foreground = new SolidColorBrush(Color.FromRgb(0X2B, 0X91, 0XAF));

            editor.LineNumbersForeground = new SolidColorBrush(Colors.LightGray);

           
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

            editor.SyntaxHighlighting = customHighlighting;
        }

        void UpdateFoldings()
        {           
            foldingStrategy.UpdateFoldings(foldingManager, editor.Document);            
        }

        public virtual void SetVertexDefaultValues()
        {
            TextMemoryCurrent = 0;
            TextMemoryMax = 0;
        }

        bool isFirstParse = true;

        //static object lockObject2 = new object();

        public void BaseEdgeToUpdated()
        {
            //lock (lockObject2)
            {
                if (doNotParse)
                    return;

                IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

                if (bv != null /*&& bv.Value != null && ((String)bv.Value)!="$Empty"*/)
                {
                    ExecuteGenerate();

                    if (isFirstParse)
                    {
                        TextMemory.Add(editor.Text);

                        int currentTextMemory = TextMemory.Count;

                        TextMemoryMax = currentTextMemory;
                        TextMemoryCurrent = currentTextMemory;

                        isFirstParse = false;
                    }
                }
                else
                    editor.Text = "Ø";
            }
        }

        void ExecuteGenerate()
        {
            Thread thread = new Thread(ExecuteGenerate_SeparateThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private string ExecuteGenerate_SeparateThread_internal()
        {
            lock (lockObject)
            {
                if (Vertex.DisposedState != DisposeStateEnum.Live)
                    return "";

                EdgeBase ee = new EdgeBase(Vertex.Get(false, @"BaseEdge:\From:"), Vertex.Get(false, @"BaseEdge:\Meta:"), Vertex.Get(false, @"BaseEdge:\To:"));

                IVertex ftl = GraphUtil.GetQueryOutFirst(Vertex, "FormalTextLanguage", null);

                string generated;

                if (ftl == null)
                    generated = MinusZero.Instance.DefaultFormalTextGenerator.Generate(ee);
                else
                    generated = MinusZero.Instance.DefaultFormalTextGenerator.Generate(ftl, ee);

                return generated;
            }
        }

        private void ExecuteGenerate_SeparateThread()
        {
            m0Main.Instance.Dispatcher.Invoke(() =>
            {
                editor.Background = (Brush)FindResource("0ProcessingBrush");
            });

            
            string generated = ExecuteGenerate_SeparateThread_internal();


            m0Main.Instance.Dispatcher.Invoke(() =>
            {
                editor.Text = generated;

                //editor.Background = (Brush)FindResource("0BackgroundBrush");

                editor.Background = null;
            });

            
        }
    }
}
