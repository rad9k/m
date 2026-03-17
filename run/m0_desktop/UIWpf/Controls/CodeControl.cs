using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers.Code;
using m0.User.Process.UX;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace m0.UIWpf.Controls
{
    public class CodeControl : Border
    {
        static object lockObject = new object();

        public bool GenerateAfterParse = true;

        public bool NoBackgroundWorkOnGenerate = false;

        public IVertex Vertex;

        public bool NoVertexForTextMemory = false;

        //

        public TextEditor editor = new TextEditor();

        bool doNotParse = false;



        //

        IList<string> TextMemory;

        public void UpdateView() { ViewAttributesUpdated(); }

        public void UnselectAllSelectedEdges() { }

        public CodeControl(IVertex _Vertex) : this(_Vertex, true) { }

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

            editor.TextChanged += Editor_TextChanged;
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
           editor.Background = (Brush)FindResource("0EditorChangedBrush");
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

        private IEdge GetBaseEdge()
        {
            if (baseEdgeFinal != null)
                return baseEdgeFinal;

            IEdge BaseEdge = EdgeHelper.CreateIEdgeFromEdgeVertex(Vertex.Get(false, @"BaseEdge:"));

            string ContentQuery = GraphUtil.GetStringValueOrNull(Vertex.Get(false, @"ContentQuery:"));

            if (ContentQuery != null)
                BaseEdge = BaseEdge.To.GetAll(false, ContentQuery).FirstOrDefault();

            return BaseEdge;
        }

        IEdge baseEdgeFinal = null;

        private void SetBaseEdgeFinal(IEdge baseEdge)
        {
            //ExecutionFlowHelper.GraphChangeWatchOff();

            //EdgeHelper.ReplaceEdgeVertexEdges(Vertex.Get(false, @"BaseEdge:"), baseEdge);

            baseEdgeFinal = baseEdge;

            //ExecutionFlowHelper.GraphChangeWatchOn();
        }

        private IVertex GetBaseEdgeTo()
        {
            return GetBaseEdge().To;
        }

        private CodeRepresentationEnum GetCodeRepresentation()
        {
            IVertex CodeRepresentationVertex = Vertex.Get(false, @"CodeRepresentation:");

            if (CodeRepresentationVertex != null)
                return CodeRepresentationEnumHelper.GetEnum(CodeRepresentationVertex);
            else
                return CodeRepresentationEnum.VertexAndManyLines;
        }

        public void ExecuteParse()
        {            
            editor_Text = editor.Text;

            if (NoBackgroundWorkOnGenerate)
            {
                ExecuteParse_SeparateThread();
            }
            else
            {
                Thread thread = new Thread(ExecuteParse_SeparateThread);
                thread.IsBackground = true;
                thread.Start();
            }
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

                IEdge BaseEdge = GetBaseEdge();

                IVertex ftlp = GraphUtil.GetQueryOutFirst(Vertex, "FormalTextLanguageProcessing", null);

                //

                m0Main.Instance.Dispatcher.Invoke(() =>
                {
                    editor.Background = (Brush)FindResource("0ProcessingBrush");
                });

                //

                IVertex errorList = null;

                IEdge baseEdge_new = null;

                //if (ftl == null)
                //  errorList = MinusZero.Instance.DefaultFormalTextParser.Parse(BaseEdge, editor_Text, GetCodeRepresentation(), out baseEdge_new);
                //else
                //  errorList = MinusZero.Instance.DefaultFormalTextParser.Parse(ftl, BaseEdge, editor_Text, GetCodeRepresentation(), out baseEdge_new);

                if (ftlp != null)
                    errorList = ZeroCodeProcessingHelper.Parse(ftlp, BaseEdge, editor_Text, out baseEdge_new);
                else
                    errorList = ZeroCodeProcessingHelper.Parse(BaseEdge, editor_Text, out baseEdge_new);
                

                if (baseEdge_new != null)
                    SetBaseEdgeFinal(baseEdge_new);
              
                //

                int errorLine = -1;

                string generated = null;

                if (errorList != null && errorList.OutEdges.Count == 0 && GenerateAfterParse)
                    generated = ExecuteGenerate_SeparateThread_internal();

                m0Main.Instance.Dispatcher.Invoke(() =>
                {
                    if (errorList != null)
                    {
                        if (errorList.OutEdges.Count == 0)
                        {
                            if (generated != null)
                                editor.Text = generated;

                            editor.Background = null;
                        }
                        else
                        {
                            editor.Background = (Brush)FindResource("0LightErrorBrush");

                            errorLine = GraphUtil.GetIntegerValueOr0(errorList.OutEdges[0].To.Get(false, "Where:"));
                        }
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

        private static Key GetEffectiveKey(KeyEventArgs e)
        {
            if (e.Key == Key.System)
                return e.SystemKey;

            return e.Key;
        }

        private static bool IsAltPressed(KeyEventArgs e)
        {
            return (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
        }

        private void CodeVisualiser_KeyDown(object sender, KeyEventArgs e)
        {
            Key effectiveKey = GetEffectiveKey(e);

            if (effectiveKey == Key.Escape)
            {
                ExecuteParse();
                e.Handled = true;
                return;
            }

            if (effectiveKey == Key.Left && IsAltPressed(e))
            {
                ReferenceTextMemoryLeft();
                e.Handled = true;
                return;
            }

            if (effectiveKey == Key.Right && IsAltPressed(e))
            {
                ReferenceTextMemoryRight();
                e.Handled = true;
                return;
            }
        }

        TabFoldingStrategy foldingStrategy;
        FoldingManager foldingManager;
        DispatcherTimer foldingUpdateTimer;

        IVertex FormalTextLanguageProcessingVertex_prev;

        bool FormalTextLanguageProcessingVertex_prev_calculated = false;

        public void ViewAttributesUpdated()
        {
            IVertex FormalTextLanguageProcessingVertex = Vertex.Get(false, @"FormalTextLanguageProcessing:");

            if (FormalTextLanguageProcessingVertex != null)
            {                
                if (FormalTextLanguageProcessingVertex_prev_calculated)
                {
                    if (FormalTextLanguageProcessingVertex_prev != FormalTextLanguageProcessingVertex)
                        ExecuteGenerate();
                }
                else
                    FormalTextLanguageProcessingVertex_prev_calculated = true;

                FormalTextLanguageProcessingVertex_prev = FormalTextLanguageProcessingVertex;
            }

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, @"ShowWhiteSpace:"),"True"))
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
            editor.Background = null;

            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            editor.FontFamily = new FontFamily("Consolas");
            editor.FontWeight = FontWeight.FromOpenTypeWeight(1);

            editor.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0X2B, 0X91, 0XAF));

            editor.LineNumbersForeground = new SolidColorBrush(Colors.LightGray);

           
            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(m0.UIWpf.WpfUtil).Assembly.GetManifestResourceStream("m0_desktop.ZeroCodeHighlighting.xshd"))
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
            editor.TextArea.TextView.NonPrintableCharacterBrush = new SolidColorBrush(Colors.Black);
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


        public void BaseEdgeToUpdated()
        {
            if (doNotParse)
                return;

            IVertex bv = GetBaseEdgeTo();

            if (bv != null)
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

        void ExecuteGenerate()
        {
            if (NoBackgroundWorkOnGenerate)
            {
                editor.Text = ExecuteGenerate_SeparateThread_internal();
                editor.Background = null;
            }
            else
            {                                              
                Thread thread = new Thread(ExecuteGenerate_SeparateThread);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private string ExecuteGenerate_SeparateThread_internal()
        {
            lock (lockObject)            
            {
                if (Vertex.DisposedState != DisposeStateEnum.Live)
                    return "";

                IEdge ee = GetBaseEdge();

                IVertex ftlp = GraphUtil.GetQueryOutFirst(Vertex, "FormalTextLanguageProcessing", null);

                string generated;

                //if (ftl == null)
                  //  generated = MinusZero.Instance.DefaultFormalTextGenerator.Generate(ee, GetCodeRepresentation());
                //else
                  //  generated = MinusZero.Instance.DefaultFormalTextGenerator.Generate(ftl, ee, GetCodeRepresentation());

                if (ftlp == null)
                    generated = ZeroCodeProcessingHelper.Generate(ee);
                else                    
                    generated = ZeroCodeProcessingHelper.Generate(ftlp, ee);


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

                editor.Background = null;
            });

            
        }
    }
}
