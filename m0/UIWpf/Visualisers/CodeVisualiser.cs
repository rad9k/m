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

namespace m0.UIWpf.Visualisers
{
    public class CodeVisualiser : TextEditor, IPlatformClass, IDisposable, IHasLocalizableEdges, IOwnScrolling
    {
        IList<string> TextMemory;

        public CodeVisualiser()
        {
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();

                Vertex.Value = "CodeVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\Code"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                SetVertexDefaultValues();

                // no dnd here
                // this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                // this.PreviewMouseMove += dndPreviewMouseMove;
                //this.Drop+=dndDrop;
                // this.AllowDrop = true;

                // this.MouseEnter += dndMouseEnter;

                TextMemory = new List<string>();

                this.Loaded += new RoutedEventHandler(OnLoad);

                //this.KeyDown += CodeVisualiser_KeyDown;

                this.PreviewKeyDown += CodeVisualiser_KeyDown;

                editSetup();

                UpdateEditView();
            }
        }

        private void ExecuteParse()
        {
            int TextMemoryMax = (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryMax:"));
            TextMemoryMax++;

            TextMemory.Add(Text);

            MinusZero.Instance.DefaultParser.Parse(Vertex.Get(false, @"BaseEdge:\To:"), Text);

            TextMemoryMax++;

            Vertex.Get(false, "TextMemoryMax:").Value = TextMemoryMax - 1;
            Vertex.Get(false, "TextMemoryCurrent:").Value = TextMemoryMax;
        }

        private void ReferenceTextMemoryLeft()
        {            
            int TextMemoryCurrent = (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryCurrent:"));

        /*    if (TextMemoryCurrent == 1)
            {            
                Text = TextMemory[TextMemoryCurrent - 1];

                Vertex.Get(false, "TextMemoryCurrent:").Value = TextMemoryCurrent;
            }*/

            if (TextMemoryCurrent > 1)
            {
                TextMemoryCurrent--;

                Text = TextMemory[TextMemoryCurrent - 1];

                Vertex.Get(false, "TextMemoryCurrent:").Value = TextMemoryCurrent;
            }

            
        }

        private void ReferenceTextMemoryRight()
        {
            int TextMemoryMax = (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryMax:"));
            int TextMemoryCurrent = (int)GraphUtil.GetIntegerValue(Vertex.Get(false, "TextMemoryCurrent:"));

            TextMemoryCurrent++;

            if (TextMemoryCurrent < (TextMemoryMax - 1))
            {
                TextMemoryCurrent++;

                Text = TextMemory[TextMemoryCurrent - 1];

                Vertex.Get(false, "TextMemoryCurrent:").Value = TextMemoryCurrent - 1; ;
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
        void editSetup()
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

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!WpfUtil.HasParentsGotContextMenu(this))
                this.ContextMenu = new m0ContextMenu(this);
        }

        private void UpdateBaseEdge()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null /*&& bv.Value != null && ((String)bv.Value)!="$Empty"*/)
            {
                NonActingEdge ee = new NonActingEdge(Vertex.Get(false, @"BaseEdge:\From:"), Vertex.Get(false, @"BaseEdge:\Meta:"), Vertex.Get(false, @"BaseEdge:\To:"));
                this.Text = this.Text = MinusZero.Instance.DefaultCodeGenerator.Generate(ee);
            }
            else
                this.Text = "Ø";
        }

        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetDoubleValue(Vertex.Get(false, "ZoomVisualiserContent:")));

            this.FontSize = scale;
        }


        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();                        

            if ((sender == Vertex.Get(false, "BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))
                || (sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged))            
                UpdateBaseEdge();

            if (sender == Vertex.Get(false, "ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
                ChangeZoomVisualiserContent();

            if (sender == Vertex.Get(false, "ShowWhiteSpace:") && e.Type == VertexChangeType.ValueChanged)
                UpdateEditView();

            if (sender == Vertex.Get(false, "ShowLineNumbers:") && e.Type == VertexChangeType.ValueChanged)
                UpdateEditView();

            if (sender == Vertex.Get(false, "HighlightedLine:") && e.Type == VertexChangeType.ValueChanged)
                UpdateEditView();
        }        

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
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

        ///// DRAG AND DROP

        Point dndStartPoint;        

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);

            MinusZero.Instance.IsGUIDragging = false;

            hasButtonBeenDown = true;
        }

        bool isDraggin = false;
        bool hasButtonBeenDown;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown && isDraggin == false && (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                if (Vertex.Get(false, @"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();
                 
                    dndVertex.AddEdge(null, Vertex.Get(false, @"BaseEdge:"));

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    isDraggin = false;
                }
            }           
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            Dnd.DoDrop(this,Vertex.Get(false, @"BaseEdge:\To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
