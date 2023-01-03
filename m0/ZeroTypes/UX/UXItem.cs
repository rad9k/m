using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.UX;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0.ZeroTypes.UX
{
    public class UXItem : UserControl, IUXItem, IPlatformClass
    {
        public List<FrameworkElement> Anchors;        

        public double LineWidth;        

        protected List<IUXItem> DiagramToLines = new List<IUXItem>();

        protected List<ILineDecoratorBase> DiagramToAsMetaLines = new List<ILineDecoratorBase>();

        IEdge graphChangeListenerEdge;

        //

        public IUXAggregator Diagram { get; set; } // ParentAggregator

        public bool IsSelected { get; set; }

        public bool IsHighlighted { get; set; }

        List<ILineDecoratorBase> diagramLines = new List<ILineDecoratorBase>();

        public List<ILineDecoratorBase> DiagramLines
        {
            get { return diagramLines; }
        }

        public virtual void VertexSetedUp()
        {
            graphChangeListenerEdge = ExecutionFlowHelper.AddTriggerAndListener(Vertex,
                new List<string> { },
                new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                         GraphChangeFilterEnum.OutputEdgeAdded,
                         GraphChangeFilterEnum.OutputEdgeRemoved,
                         GraphChangeFilterEnum.OutputEdgeDisposed},
                "UXItem",
                VertexChange);            

            VisualiserUpdate();
        } // to be called after Vertex is setted up

        public virtual void Dispose()
        {            
            foreach (IUXItem e in Items)
                if (e is IDisposable)
                    ((IDisposable)e).Dispose();

            foreach (ILineDecoratorBase e in DiagramLines)
                if (e is IDisposable)
                    ((IDisposable)e).Dispose();

            GraphChangeTrigger.RemoveListener(graphChangeListenerEdge);

            TypedEdge.RemoveFromDictionary(this);
        }

        public Dictionary<IVertex, List<ILineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary() { return null; }

        public virtual void RemoveFromCanvas()         
        {
            Diagram.TheCanvas.Children.Remove(this);

            Unselect();

            foreach (ILineDecoratorBase l in DiagramLines)
                l.RemoveFromCanvas();

            foreach (ILineDecoratorBase l in DiagramToLines)
                l.RemoveFromCanvas();
        }

        public virtual void DoCreateDiagramLine(IUXItem toItem) 
        {
            IVertex toEdge = toItem.Vertex.Get(false, "BaseEdge:");

            IVertex r = m0.MinusZero.Instance.Root;

            IVertex v = m0.MinusZero.Instance.CreateTempVertex();


            foreach (IEdge def in Vertex.GetAll(false, @"Definition:\DiagramLineDefinition:"))
            {
                bool CreateEdgeOnly = false;

                if (GraphUtil.GetValueAndCompareStrings(def.To.Get(false, "CreateEdgeOnly:"), "True"))
                    CreateEdgeOnly = true;

                foreach (IEdge e in Vertex.GetAll(false, @"BaseEdge:\To:\" + def.To.Get(false, "EdgeTestQuery:")))
                {
                    bool canAdd = true;

                    if (def.To.Get(false, "ToDiagramItemTestQuery:") != null && toItem.Vertex.Get(false, (string)def.To.Get(false, "ToDiagramItemTestQuery:").Value) == null)
                        canAdd = false;

                    IVertex toTest_baseVertex = toEdge.Get(false, @"To:");



                    if (e.To.Get(false, @"$EdgeTarget:") != null
                        && !GeneralUtil.CompareStrings(e.To.Get(false, @"$EdgeTarget:").Value, "Vertex") // Vertices do not have $Is:Vertex
                                                                                                         //&& toEdge.Get(false, @"To:\$Is:" + (string)e.To.Get(false, @"$EdgeTarget:").Value) == null                        
                        )
                    {
                        string toTest_class = (string)e.To.Get(false, @"$EdgeTarget:").Value;

                        if (!InstructionHelpers.CheckIfIsOrInherits(toTest_baseVertex, toTest_class))
                            canAdd = false;
                    }

                    if (CreateEdgeOnly == false
                        && e.To.Get(false, @"$VertexTarget:") != null
                        //&& toEdge.Get(false, @"To:\$Is:" + (string)e.To.Get(false, @"$VertexTarget:").Value) == null                        
                        )
                    {
                        string toTest_class = (string)e.To.Get(false, @"$VertexTarget:").Value;

                        if (!InstructionHelpers.CheckIfIsOrInherits(toTest_baseVertex, toTest_class))
                            canAdd = false;
                    }

                    if (canAdd)
                        AddNewLineOption(v, def, e);
                }

                if (GeneralUtil.CompareStrings(def.To.Value, "Edge"))// Vertex\Edge
                    foreach (IEdge e in r.Get(false, @"System\Meta\Base\Vertex"))
                        AddNewLineOption(v, def, e);

                if (GeneralUtil.CompareStrings(def.To.Get(false, "EdgeTestQuery:"), "$EdgeTarget")) // $EdgeTarget is not present as there is no inheritance from Vertex
                    AddNewLineOption(v, def, GraphUtil.FindEdgeByToVertex(r.Get(false, @"System\Meta\Base\Vertex"), "$EdgeTarget"));
            }

            if (v.Count() == 0)
                UserInteractionUtil.ShowError(Diagram.Vertex.Value + " Diagram", "There is no diagram line definition matching selected source and target items.");

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();
            info.Value = "choose diagram line:";


            Point mousePosition = WpfUtil.GetMousePosition();

            IVertex a = MinusZero.Instance.DefaultUserInteraction.SelectDialog(info, v, mousePosition);

            if (a != null)
            {
                IVertex test = VertexOperations.TestIfNewEdgeValid(Vertex.Get(false, @"BaseEdge:\To:"), a.Get(false, "OptionEdge:"), toEdge.Get(false, "To:"));

                if (test == null)
                {
                    bool? ForceShowEditForm = null; // ForceShowEditForm

                    if (a.Get(false, @"OptionDiagramLineDefinition:\ForceShowEditForm:") != null)
                    {
                        if (GeneralUtil.CompareStrings(a.Get(false, @"OptionDiagramLineDefinition:\ForceShowEditForm:"), "True"))
                            ForceShowEditForm = true;

                        if (GeneralUtil.CompareStrings(a.Get(false, @"OptionDiagramLineDefinition:\ForceShowEditForm:"), "False"))
                            ForceShowEditForm = false;
                    }

                    bool CreateEdgeOnly = false; // CreateEdgeOnly

                    if (GraphUtil.GetValueAndCompareStrings(a.Get(false, @"OptionDiagramLineDefinition:\CreateEdgeOnly:"), "True"))
                        CreateEdgeOnly = true;


                    CanAutomaticallyAddEdges = false; // for VertexChange
                    IEdge edge = VertexOperations.AddEdgeOrVertexByMeta(Vertex.Get(false, @"BaseEdge:\To:"), a.Get(false, "OptionEdge:"), toEdge.Get(false, "To:"), mousePosition, CreateEdgeOnly, ForceShowEditForm);
                    CanAutomaticallyAddEdges = true;

                    AddDiagramLineVertex(edge, a.Get(false, @"OptionDiagramLineDefinition:"), toItem);
                }
                else
                    UserInteractionUtil.ShowError(Diagram.Vertex.Value + " Diagram", "Adding new diagram line  \"" + a.Value + "\" is not possible.\n\n" + test.Value);
            }
        }

        public void AddDiagramLineVertex(IEdge edge, IVertex diagramLineDefinition, IUXItem toItem) {}

        public void AddDiagramLineObject(IUXItem toItem, ILineDecoratorBase lineDecorator) {}

        public void RemoveDiagramLine(ILineDecoratorBase line) {}

        public virtual void Select() {}

        public virtual void Unselect() {}

        public virtual void Highlight() {}

        public virtual void Unhighlight() {}

        public void MoveItem(double x, double y) {}

        public void MoveAndResizeItem(double left, double top, double width, double height) {}

        public void AddToSelectedEdges() {}

        //        

        

        public DiagramItemBase(IVertex _baseEdgeVertex)
        {
            Anchors = new List<FrameworkElement>();

            this.SizeChanged += DiagramItemBase_SizeChanged;

            this.MouseEnter += DiagramItemBase_MouseEnter;

            this.MouseLeave += DiagramItemBase_MouseLeave;
        }

        
        

        

        protected virtual INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            IVertex baseEdgeTo = Vertex.Get(false, @"BaseEdge:\To:");

            if (IsVertexChange(exe.Stack, baseEdgeTo))
                VisualiserUpdate();

            foreach (IVertex edgeVertex in GetEdgesRemovedFrom(exe.Stack, baseEdgeTo))
            {
                DiagramLineBase toRemove = null;

                foreach (DiagramLineBase l in DiagramLines)
                    if (l.Vertex.Get(false, @"BaseEdge:\Meta:") == edgeVertex.Get(false, "Meta:") &&
                        l.Vertex.Get(false, @"BaseEdge:\To:") == edgeVertex.Get(false, "To:"))
                        toRemove = l;

                if (toRemove != null)
                    RemoveDiagramLine(toRemove);
            }

            if (IsEdgeAddedTo(exe.Stack, baseEdgeTo) && CanAutomaticallyAddEdges)
                Diagram.CheckAndUpdateDiagramLinesForItem(this);

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "BackgroundColor")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "ForegroundColor")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Red")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Green")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Blue")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Opacity"))
                VisualiserUpdate();

            if (IsEdgeAddedTo(exe.Stack, Vertex))
                VisualiserUpdate();

            return exe.Stack;
        }

        /*if (sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.EdgeRemoved)
        {
            DiagramLineBase toRemove = null;

            foreach (DiagramLineBase l in DiagramLines)
                if (l.Vertex.Get(false, @"BaseEdge:\Meta:") == e.Edge.Meta &&
                    l.Vertex.Get(false, @"BaseEdge:\To:") == e.Edge.To)
                    toRemove = l;

            if (toRemove != null)
                RemoveDiagramLine(toRemove);
        }*/

        //if (sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged)
        //    VertexContentChange();

        // if (sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.EdgeAdded && CanAutomaticallyAddEdges)
        // {
        //     Diagram.CheckAndUpdateDiagramLinesForItem(this);
        // }

        /*  if (sender == Vertex.Get(false, @"LineWidth:") ||
              sender == Vertex.Get(false, @"BackgroundColor:") || sender == Vertex.Get(false, @"BackgroundColor:\Red:") || sender == Vertex.Get(false, @"BackgroundColor:\Green:") || sender == Vertex.Get(false, @"BackgroundColor:\Blue:") || sender == Vertex.Get(false, @"BackgroundColor:\Opacity:") ||
              sender == Vertex.Get(false, @"ForegroundColor:") || sender == Vertex.Get(false, @"ForegroundColor:\Red:") || sender == Vertex.Get(false, @"ForegroundColor:\Green:") || sender == Vertex.Get(false, @"ForegroundColor:\Blue:") || sender == Vertex.Get(false, @"ForegroundColor:\Opacity:"))
              VisualiserUpdate();*/

        /*   if (sender == Vertex || e.Type == VertexChangeType.EdgeAdded)
               VisualiserUpdate();*/

        // OPTIMISATION START

        Dictionary<IVertex, List<DiagramLineBase>> DiagramLinesToDiagramItemDictionary = new Dictionary<IVertex, List<DiagramLineBase>>();
        Dictionary<IVertex, List<DiagramLineBase>> DiagramLinesBaseEdgeToDictionary = new Dictionary<IVertex, List<DiagramLineBase>>();

        bool needRebuildDiagramLinesDictionary = true;

        void AddToDiagramLines(DiagramLineBase line)
        {
            DiagramLines.Add(line);

            needRebuildDiagramLinesDictionary = true;
        }

        void ClearDiagramLines()
        {
            DiagramLinesToDiagramItemDictionary.Clear();
            DiagramLinesBaseEdgeToDictionary.Clear();

            needRebuildDiagramLinesDictionary = true;
        }

        void RemoveFromDiagramLines(DiagramLineBase toRemove)
        {
            DiagramLines.Remove(toRemove);

            needRebuildDiagramLinesDictionary = true;
        }

        void RebuidDiagramLinesDictionary()
        {
            DiagramLinesToDiagramItemDictionary.Clear();
            DiagramLinesBaseEdgeToDictionary.Clear();

            foreach (DiagramLineBase l in DiagramLines)
            {
                // ToDiagramItem:

                IVertex toDiagramItem = l.Vertex.Get(false, @"ToDiagramItem:");

                if (DiagramLinesToDiagramItemDictionary.ContainsKey(toDiagramItem))
                    DiagramLinesToDiagramItemDictionary[toDiagramItem].Add(l);
                else
                {
                    List<DiagramLineBase> list = new List<DiagramLineBase>();
                    list.Add(l);

                    DiagramLinesToDiagramItemDictionary.Add(toDiagramItem, list);
                }

                // BaseEdge:\To:

                IVertex BaseEdgeTo = l.Vertex.Get(false, @"BaseEdge:\To:");

                if (DiagramLinesBaseEdgeToDictionary.ContainsKey(BaseEdgeTo))
                    DiagramLinesBaseEdgeToDictionary[BaseEdgeTo].Add(l);
                else
                {
                    List<DiagramLineBase> list = new List<DiagramLineBase>();
                    list.Add(l);

                    DiagramLinesBaseEdgeToDictionary.Add(BaseEdgeTo, list);
                }
            }

            needRebuildDiagramLinesDictionary = false;
        }

        public Dictionary<IVertex, List<DiagramLineBase>> GetDiagramLinesToDiagramItemDictionary()
        {
            if (needRebuildDiagramLinesDictionary)
                RebuidDiagramLinesDictionary();

            return DiagramLinesToDiagramItemDictionary;
        }

        public Dictionary<IVertex, List<DiagramLineBase>> GetDiagramLinesBaseEdgeToDictionary()
        {
            if (needRebuildDiagramLinesDictionary)
                RebuidDiagramLinesDictionary();

            return DiagramLinesBaseEdgeToDictionary;
        }

        // OPTIMISATION END



        public void AddAsToMetaLine(DiagramLineBase line)
        {
            DiagramToAsMetaLines.Add(line);
        }

        public virtual void VisualiserUpdate()
        {

            double? sizeX = GraphUtil.GetDoubleValue(Vertex.Get(false, "SizeX:"));
            double? sizeY = GraphUtil.GetDoubleValue(Vertex.Get(false, "SizeY:"));

            if (sizeX != null && sizeY != null)
            {
                Width = (double)sizeX;
                Height = (double)sizeY;
            }

            if (Vertex.Get(false, "BackgroundColor:") != null)
                BackgroundColor = WpfUtil.GetBrushFromColorVertex(Vertex.Get(false, "BackgroundColor:"));
            else
                BackgroundColor = (Brush)FindResource("0BackgroundBrush");

            if (Vertex.Get(false, "ForegroundColor:") != null)
                ForegroundColor = WpfUtil.GetBrushFromColorVertex(Vertex.Get(false, "ForegroundColor:"));
            else
                ForegroundColor = (Brush)FindResource("0ForegroundBrush");

            double? _lineWidth = GraphUtil.GetDoubleValue(Vertex.Get(false, "LineWidth:"));

            if (_lineWidth != null)
                LineWidth = (double)_lineWidth;

            SetBackAndForeground();
        }

        public virtual void SetBackAndForeground()
        {
            this.Foreground = ForegroundColor;

            // this.Background = BackgroundColor;
        }

               

        double AnchorSize = 11;

        public virtual Point GetLineAnchorLocation(DiagramItemBase toItem, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart)
        {
            Point p = new Point();

            p.X = Canvas.GetLeft(this) + this.ActualWidth / 2;
            p.Y = Canvas.GetTop(this) + this.ActualHeight / 2;

            return p;
        }



        private static void AddNewLineOption(IVertex v, IEdge def, IEdge e)
        {
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex vv = v.AddVertex(null, e.To.Value + " (" + def.To.Value + ")");//def.To.Value + " for " + e.To.Value);

            vv.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\OptionEdge"), e.To);
            vv.AddEdge(r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\OptionDiagramLineDefinition"), def.To);
        }

        public void AddDiagramLineVertex(IEdge edge, IVertex diagramLineDefinition, DiagramItemBase toItem)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            IVertex r = MinusZero.Instance.Root;

            //((EasyVertex)Vertex).CanFireChangeEvent = false;

            IVertex l = VertexOperations.AddInstance(Vertex, diagramLineDefinition.Get(false, "DiagramLineClass:"), r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\DiagramLine"));

            GraphUtil.CreateOrReplaceEdge(l, r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramLineBase\ToDiagramItem"), toItem.Vertex);

            GraphUtil.CreateOrReplaceEdge(l, r.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramLineBase\Definition"), diagramLineDefinition);

            EdgeHelper.CreateOrReplaceEdgeVertexFromIEdgeByMeta(l, r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), edge);

            AddDiagramLineObject(toItem, l);

            //((EasyVertex)Vertex).CanFireChangeEvent = true;

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        public void AddDiagramLineObject(DiagramItemBase toItem, IVertex l)
        {
            DiagramLineBase newline = (DiagramLineBase)PlatformClass.CreatePlatformObject(l, null as IVertex); // ??? ZZZ

            newline.Diagram = this.Diagram;

            if (newline.Vertex.Get(false, @"Definition:\DiagramLineVertex:") != null)
                Diagram.AddEdgesFromDefintion(newline.Vertex, newline.Vertex.Get(false, @"Definition:\DiagramLineVertex:"));

            newline.FromDiagramItem = this;

            newline.ToDiagramItem = toItem;

            newline.AddToCanvas();

            AddToDiagramLines(newline);

            toItem.DiagramToLines.Add(newline);

            UpdateDiagramLines(toItem);
        }

        IEdge GetLineEdgeFromLineObject(DiagramLineBase line)
        {
            foreach (IEdge e in Vertex.GetAll(false, "DiagramLine:"))
                if (e.To == line.Vertex)
                    return e;

            return null;
        }

        public void RemoveDiagramLine(DiagramLineBase line)
        {
            Vertex.DeleteEdge(GetLineEdgeFromLineObject(line));

            RemoveFromDiagramLines(line);


            line.ToDiagramItem.RemoveToLine(line);

            line.RemoveFromCanvas();
        }

        protected void RemoveToLine(DiagramLineBase line)
        {
            DiagramToLines.Remove(line);
        }

        protected void UpdateDiagramLines(DiagramItemBase toItem)
        {
            List<DiagramLineBase> sameToItemLines = new List<DiagramLineBase>();

            if (GetDiagramLinesToDiagramItemDictionary().ContainsKey(toItem.Vertex))
                foreach (DiagramLineBase l in GetDiagramLinesToDiagramItemDictionary()[toItem.Vertex])
                    sameToItemLines.Add(l);

            /*foreach (DiagramLineBase l in DiagramLines) // OOO
                if (l.Vertex.Get(false, "ToDiagramItem:") == toItem.Vertex)
                    sameToItemLines.Add(l);*/

            List<DiagramLineBase> sameFromItemLinesTo = new List<DiagramLineBase>();

            if (toItem.GetDiagramLinesToDiagramItemDictionary().ContainsKey(this.Vertex))
                foreach (DiagramLineBase l in toItem.GetDiagramLinesToDiagramItemDictionary()[this.Vertex])
                    sameFromItemLinesTo.Add(l);

            /*foreach (DiagramLineBase l in toItem.DiagramLines) // OOO
                if (l.Vertex.Get(false, "ToDiagramItem:") == this.Vertex)
                    sameFromItemLinesTo.Add(l);*/

            int allCnt = sameToItemLines.Count() + sameFromItemLinesTo.Count();

            int cnt = 0;

            if (toItem == this)
                allCnt = allCnt / 2;

            foreach (DiagramLineBase l in sameToItemLines)
            {
                Point start = GetLineAnchorLocation(toItem, new Point(), allCnt, cnt, toItem == this);

                Point end = toItem.GetLineAnchorLocation(this, new Point(), allCnt, cnt, false);

                if (toItem == this)
                    l.SetPosition(start.X, start.Y, end.X, end.Y, true, Canvas.GetLeft(this) + this.ActualWidth + 25 * (allCnt - cnt), Canvas.GetTop(this) - 25 * ((allCnt - cnt)));
                else
                    l.SetPosition(start.X, start.Y, end.X, end.Y, false, 0, 0);

                cnt++;
            }

            foreach (DiagramLineBase l in sameFromItemLinesTo)
            {
                Point end = GetLineAnchorLocation(toItem, new Point(), allCnt, cnt, false);

                Point start = toItem.GetLineAnchorLocation(this, new Point(), allCnt, cnt, false);

                if (toItem != this)
                    l.SetPosition(start.X, start.Y, end.X, end.Y, false, 0, 0);

                cnt++;
            }
        }

        protected void UpdateDiagramLines()
        {
            foreach (DiagramLineBase m in DiagramToAsMetaLines)
                m.UpdateMetaPosition();

            List<DiagramItemBase> updatedItems = new List<DiagramItemBase>();

            foreach (DiagramLineBase l in DiagramLines)
                if (!updatedItems.Contains(l.ToDiagramItem))
                {
                    UpdateDiagramLines(l.ToDiagramItem);

                    updatedItems.Add(l.ToDiagramItem);
                }

            foreach (DiagramLineBase l in DiagramToLines)
                if (!updatedItems.Contains(l.FromDiagramItem))
                {
                    UpdateDiagramLines(l.FromDiagramItem);

                    updatedItems.Add(l.FromDiagramItem);
                }

        }

        public virtual void Select()
        {
            IsSelected = true;

            GeneralUtil.SetPropertyIfPresent(this.Content, "Foreground", BackgroundColor);

            Panel.SetZIndex(this, 99999);

            double left = Canvas.GetLeft(this);
            double top = Canvas.GetTop(this);
            double right = left + ActualWidth;
            double bottom = top + ActualHeight;
            double width = ActualWidth;
            double height = ActualHeight;

            AddAnchor(ClickTargetEnum.AnchorLeftTop, left - AnchorSize, top - AnchorSize);
            AddAnchor(ClickTargetEnum.AnchorMiddleTop, left - AnchorSize / 2 + width / 2, top - AnchorSize);
            AddAnchor(ClickTargetEnum.AnchorRightTop_CreateDiagramLine, right, top - AnchorSize);

            AddAnchor(ClickTargetEnum.AnchorLeftMiddle, left - AnchorSize, top - AnchorSize / 2 + height / 2);
            AddAnchor(ClickTargetEnum.AnchorRightMiddle, right, top - AnchorSize / 2 + height / 2);

            AddAnchor(ClickTargetEnum.AnchorLeftBottom, left - AnchorSize, bottom);
            AddAnchor(ClickTargetEnum.AnchorMiddleBottom, left - AnchorSize / 2 + width / 2, bottom);
            AddAnchor(ClickTargetEnum.AnchorRightBottom, right, bottom);
        }

        public virtual void Unselect()
        {
            IsSelected = false;

            GeneralUtil.SetPropertyIfPresent(this.Content, "Foreground", ForegroundColor);

            Panel.SetZIndex(this, 0);

            foreach (UIElement e in Anchors)
                Diagram.TheCanvas.Children.Remove(e);

            Anchors.Clear();
        }

        public virtual void Highlight()
        {
            IsHighlighted = true;

            //this.Foreground = (Brush)FindResource("0HighlightBrush");

            Panel.SetZIndex(this, 99999);
        }

        public virtual void Unhighlight()
        {
            IsHighlighted = false;

            Panel.SetZIndex(this, 0);

            if (IsSelected)
                Select();
            else
                Unselect();
        }

        public void HighlightThisAndAllConectedByDiagramLine()
        {
            Highlight();

            foreach (DiagramLineBase l in DiagramLines)
            {
                l.Highlight();
                l.ToDiagramItem.Highlight();
            }

            foreach (DiagramLineBase l in DiagramToLines)
            {
                l.Highlight();
                l.FromDiagramItem.Highlight();
            }
        }

        public void UnhighlightThisAndAllConectedByDiagramLine()
        {
            Unhighlight();

            foreach (DiagramLineBase l in DiagramLines)
            {
                l.Unhighlight();
                l.ToDiagramItem.Unhighlight();
            }

            foreach (DiagramLineBase l in DiagramToLines)
            {
                l.Unhighlight();
                l.FromDiagramItem.Unhighlight();
            }
        }

        private void DiagramItemBase_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Diagram.IsDrawingLine == false && Diagram.IsSelecting == false)
                UnhighlightThisAndAllConectedByDiagramLine();
        }

        private void DiagramItemBase_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Diagram.IsDrawingLine == false && Diagram.IsSelecting == false)
                HighlightThisAndAllConectedByDiagramLine();
        }

        protected void DiagramItemBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAnchors(Canvas.GetLeft(this), Canvas.GetTop(this), this.ActualWidth, this.ActualHeight);
        }

        private bool CanAutomaticallyAddEdges = true;

        public void AddToSelectedEdges()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            EdgeHelper.AddEdgeVertexEdgeByEdgeVertex(Diagram.Vertex.Get(false, "SelectedEdges:"), Vertex.Get(false, "BaseEdge:"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        public void RemoveFromSelectedEdges()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            GraphUtil.DeleteEdgeByToVertex(Diagram.Vertex.Get(false, "SelectedEdges:"), Vertex.Get(false, "BaseEdge:"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        protected void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            Diagram.SetFocus();

            Diagram.ClickPositionX_ItemCordinates = e.GetPosition(this).X;
            Diagram.ClickPositionY_ItemCordinates = e.GetPosition(this).Y;

            Diagram.ClickTarget = ClickTargetEnum.Item;
            Diagram.ClickedItem = this;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (IsSelected)
                    RemoveFromSelectedEdges();
                else
                    AddToSelectedEdges();
            }
            else
            {
                if (IsSelected)
                    RemoveFromSelectedEdges();
                else
                {
                    Diagram.UnselectAllSelectedEdges();

                    AddToSelectedEdges();
                }
            }

            e.Handled = true;
        }

        protected void UpdateAnchor(ClickTargetEnum anchorType, double left, double top)
        {
            foreach (FrameworkElement r in Anchors)
                if ((ClickTargetEnum)r.Tag == anchorType)
                {
                    Canvas.SetLeft(r, left);
                    Canvas.SetTop(r, top);

                    r.Width = AnchorSize;
                    r.Height = AnchorSize;
                }
        }

        protected FrameworkElement AddAnchor(ClickTargetEnum anchorType, double left, double top)
        {
            FrameworkElement r;

            if (anchorType == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
            {
                TextBox l = new TextBox();
                l.IsReadOnly = true;
                l.Focusable = false;
                l.Text = "*";
                l.FontSize = 22;
                l.FontFamily = new FontFamily("Times New Roman");
                l.Padding = new Thickness(-1.8, -3.5, 0, 0);
                l.Margin = new Thickness(0);
                l.BorderThickness = new Thickness(0);

                l.Background = (Brush)FindResource("0SelectionBrush");
                l.Foreground = (Brush)FindResource("0BackgroundBrush");

                r = l;
            }
            else
            {
                r = new Rectangle();
                ((Rectangle)r).Fill = (Brush)FindResource("0SelectionBrush");
            }

            Canvas.SetLeft(r, left);
            Canvas.SetTop(r, top);

            r.Width = AnchorSize;
            r.Height = AnchorSize;

            r.Tag = anchorType;


            Anchors.Add(r);

            r.MouseLeftButtonDown += AnchorMouseButtonDown;

            switch (anchorType)
            {
                case ClickTargetEnum.AnchorLeftTop:
                    r.Cursor = Cursors.SizeNWSE;
                    break;

                case ClickTargetEnum.AnchorMiddleTop:
                    r.Cursor = Cursors.SizeNS;
                    break;

                case ClickTargetEnum.AnchorRightTop_CreateDiagramLine:
                    r.Cursor = Cursors.Pen;
                    break;

                case ClickTargetEnum.AnchorLeftMiddle:
                    r.Cursor = Cursors.SizeWE;
                    break;

                case ClickTargetEnum.AnchorRightMiddle:
                    r.Cursor = Cursors.SizeWE;
                    break;

                case ClickTargetEnum.AnchorLeftBottom:
                    r.Cursor = Cursors.SizeNESW;
                    break;

                case ClickTargetEnum.AnchorMiddleBottom:
                    r.Cursor = Cursors.SizeNS;
                    break;

                case ClickTargetEnum.AnchorRightBottom:
                    r.Cursor = Cursors.SizeNWSE;
                    break;
            }

            Diagram.TheCanvas.Children.Add(r);

            return r;
        }

        public void AnchorMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            Diagram.ClickPositionX_ItemCordinates = e.GetPosition(this).X;
            Diagram.ClickPositionY_ItemCordinates = e.GetPosition(this).Y;

            Diagram.ClickPositionX_AnchorCordinates = e.GetPosition((IInputElement)sender).X;
            Diagram.ClickPositionY_AnchorCordinates = e.GetPosition((IInputElement)sender).Y;

            FrameworkElement a = (FrameworkElement)sender;

            Diagram.ClickTarget = (ClickTargetEnum)a.Tag;

            Diagram.ClickedAnchor = a;

            Diagram.ClickedItem = this;

            e.Handled = true;
        }

        public void MoveItem(double x, double y)
        {
            double? _positionX = GraphUtil.GetDoubleValue(Vertex.Get(false, "PositionX:"));
            double? _positionY = GraphUtil.GetDoubleValue(Vertex.Get(false, "PositionY:"));

            if (_positionX == null || _positionY == null)
                return;

            double positionX = (double)_positionX;
            double positionY = (double)_positionY;

            double deltax = positionX - x;
            double deltay = positionY - y;

            Vertex.Get(false, "PositionX:").Value = x;
            Vertex.Get(false, "PositionY:").Value = y;

            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);

            foreach (UIElement a in Anchors)
            {
                Canvas.SetLeft(a, Canvas.GetLeft(a) - deltax);
                Canvas.SetTop(a, Canvas.GetTop(a) - deltay);
            }

            UpdateDiagramLines();
        }

        public void MoveAndResizeItem(double left, double top, double width, double height)
        {
            if (width < 0 || height < 0)
                return;

            Vertex.Get(false, "PositionX:").Value = left;
            Vertex.Get(false, "PositionY:").Value = top;

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);

            IVertex r = m0.MinusZero.Instance.Root;

            GraphUtil.SetVertexValue(this.Vertex, r.Get(false, @"System\Meta\Visualiser\Diagram\SizeX"), width);
            GraphUtil.SetVertexValue(this.Vertex, r.Get(false, @"System\Meta\Visualiser\Diagram\SizeY"), height);

            Width = width;
            Height = height;

            UpdateAnchors(left, top, width, height);

            UpdateDiagramLines();
        }

        private void UpdateAnchors(double left, double top, double width, double height)
        {
            double right = left + width;
            double bottom = top + height;

            UpdateAnchor(ClickTargetEnum.AnchorLeftTop, left - AnchorSize, top - AnchorSize);
            UpdateAnchor(ClickTargetEnum.AnchorMiddleTop, left - AnchorSize / 2 + width / 2, top - AnchorSize);
            UpdateAnchor(ClickTargetEnum.AnchorRightTop_CreateDiagramLine, right, top - AnchorSize);

            UpdateAnchor(ClickTargetEnum.AnchorLeftMiddle, left - AnchorSize, top - AnchorSize / 2 + height / 2);
            UpdateAnchor(ClickTargetEnum.AnchorRightMiddle, right, top - AnchorSize / 2 + height / 2);

            UpdateAnchor(ClickTargetEnum.AnchorLeftBottom, left - AnchorSize, bottom);
            UpdateAnchor(ClickTargetEnum.AnchorMiddleBottom, left - AnchorSize / 2 + width / 2, bottom);
            UpdateAnchor(ClickTargetEnum.AnchorRightBottom, right, bottom);
        }

        //

        public Point GetLineAnchorLocation(UXItem toItem, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart)
        {
            Point p = new Point();

            Point pTo = new Point();

            if (toItem != null)
            {
                pTo.X = Canvas.GetLeft(toItem) + toItem.ActualWidth / 2;
                pTo.Y = Canvas.GetTop(toItem) + toItem.ActualHeight / 2;
            }
            else
                pTo = toPoint;

            double tX = Canvas.GetLeft(this) + this.ActualWidth / 2;
            double tY = Canvas.GetTop(this) + this.ActualHeight / 2;

            double testX = pTo.X - tX;
            double testY = pTo.Y - tY;

            if (testX == 0) testX = 0.001;
            if (testY == 0) testY = 0.001;

            if (toItemDiagramLinesCount > 1)
            {
                if (toItem == this)
                {
                    if (isSelfStart)
                    {
                        p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p.Y = tY - this.ActualHeight / 2;

                        return p;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = Canvas.GetTop(this) + (((double)(toItemDiagramLinesCount - toItemDiagramLinesNumber)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        return p;
                    }
                }

                if (testY <= 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY - this.ActualHeight / 2;
                }

                if (testY > 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = Canvas.GetLeft(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY + this.ActualHeight / 2;
                }

                if (testX >= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + this.ActualWidth / 2;
                    p.Y = Canvas.GetTop(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
                }

                if (testX <= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - this.ActualWidth / 2;
                    p.Y = Canvas.GetTop(this) + (((double)toItemDiagramLinesNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
                }
            }
            else
            {
                if (toItem == this)
                {
                    if (isSelfStart)
                    {
                        p.X = tX;
                        p.Y = tY - this.ActualHeight / 2;

                        return p;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = tY;

                        return p;
                    }
                }

                if (testY <= 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - (this.ActualHeight / 2 * testX / testY);
                    p.Y = tY - this.ActualHeight / 2;
                }

                if (testY > 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + (this.ActualHeight / 2 * testX / testY);
                    p.Y = tY + this.ActualHeight / 2;
                }

                if (testX >= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + this.ActualWidth / 2;
                    p.Y = tY + (this.ActualWidth / 2 * testY / testX);
                }

                if (testX <= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - this.ActualWidth / 2;
                    p.Y = tY - (this.ActualWidth / 2 * testY / testX);
                }
            }

            return p;
        }

        //

        public UXItem() { }

        public UXItem(IEdge _edge)
        {
            edge = _edge;

            vertex = _edge.To;

            TypedEdge.vertexDictionary.Add(this.Edge.To, this);
        }

        // UNDER

        // UXItem

        static IVertex Scale_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale");
        static IVertex DesignMode_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\DesignMode");
        static IVertex Size_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Size");
        static IVertex Position_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Position");
        static IVertex Layout_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Layout");
        static IVertex BackgroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BackgroundColor");
        static IVertex ForegroundColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\ForegroundColor");
        static IVertex BorderColor_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BorderColor");
        static IVertex BorderSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BorderSize");
        static IVertex Gap_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Gap");
        static IVertex UXTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\UXTemplate");
        static IVertex Decorator_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Decorator");

        static IVertex Color_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Color");
        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");
        static IVertex Position_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Position");

        public double Scale
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Scale", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Scale", null);

                if (val == null)
                    val = Vertex.AddVertex(Scale_meta, value);
                else
                    val.Value = value;
            }
        }

        public bool DesignMode
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DesignMode", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "DesignMode", null);

                if (val == null)
                    val = Vertex.AddVertex(DesignMode_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.Size Size
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "Size", null);

                if (val == null)
                    return null;

                return (UX.Size)TypedEdge.Get(val, typeof(UX.Size));
            }
        }

        public UX.Size SizeCreate()
        {
            return new UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, Size_meta));
        }

        public UX.Position Position
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "Position", null);

                if (val == null)
                    return null;

                return (UX.Position)TypedEdge.Get(val, typeof(UX.Position));
            }
        }

        public UX.Position PositionCreate()
        {
            return new UX.Position(VertexOperations.AddInstanceAndReturnEdge(Vertex, Position_type, Position_meta));
        }

        public LayoutTypeEnum Layout
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Layout", null);

                return LayoutTypeEnumHelper.GetEnum(val);
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, Layout_meta, LayoutTypeEnumHelper.GetVertex(value));
            }
        }

        public UX.Color BackgroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BackgroundColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color BackgroundColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, BackgroundColor_meta));
        }

        public UX.Color ForegroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ForegroundColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color ForegroundColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, ForegroundColor_meta));
        }

        public UX.Color BorderColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BorderColor", null);

                if (val == null)
                    return null;

                return (UX.Color)TypedEdge.Get(val, typeof(UX.Color));
            }
        }

        public UX.Color BorderColorCreate()
        {
            return new UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, BorderColor_meta));
        }

        public double BorderSize
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BorderSize", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "BorderSize", null);

                if (val == null)
                    val = Vertex.AddVertex(BorderSize_meta, value);
                else
                    val.Value = value;
            }
        }

        public double Gap
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Gap", null);

                if (val == null)
                    return 0;

                return GraphUtil.GetDoubleValueOr0(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "Gap", null);

                if (val == null)
                    val = Vertex.AddVertex(Gap_meta, value);
                else
                    val.Value = value;
            }
        }

        public UX.UXTemplate UXTemplate
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "UXTemplate", null);

                if (val == null)
                    return null;

                return (UXTemplate)TypedEdge.Get(val, typeof(UXTemplate));
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "UXTemplate", null);

                GraphUtil.CreateOrReplaceEdge(Vertex, UXTemplate_meta, value.Vertex);
            }
        }

        public IList<IUXItem> Decorators
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Decorator", null);

                IList<IUXItem> ret = new List<IUXItem>();

                foreach (IEdge e in list)
                    ret.Add((IUXItem)TypedEdge.Get(e));

                return ret;
            }
        }

        public IUXItem AddDecorator(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Item_meta);

            return (IUXItem)TypedEdge.Get(newEdge);
        }

        // Item

        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");
        static IVertex UXItem_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem");
        static IVertex UXAggregator_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator");
        static IVertex Edge_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\Edge");

        public IVertex BaseEdgeTo
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

                if (val != null)
                    return GraphUtil.GetQueryOutFirst(val.To, "To", null);

                return null;
            }
        }

        public Edge BaseEdge
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

                if (val == null)
                    return null;

                return (Edge)TypedEdge.Get(val, typeof(Edge));
            }
        }

        public Edge BaseEdgeCreate()
        {
            IEdge baseEdgeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "BaseEdge", null);

            if (baseEdgeEdge != null)
                Vertex.DeleteEdge(baseEdgeEdge);

            baseEdgeEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, Edge_type, BaseEdge_meta);

            baseEdgeEdge.To.AddVertex(ZeroTypes.Edge.From_meta, ""); // from has 0..1 multiplicity

            return new Edge(baseEdgeEdge);
        }

        public IList<IItem> Items
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<IItem> ret = new List<IItem>();

                foreach (IEdge e in list)
                    ret.Add((IItem)TypedEdge.Get(e));

                return ret;
            }
        }

        public IItem AddItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Item_meta);

            return (IItem)TypedEdge.Get(newEdge);
        }

        public void RemoveItem(IItem item)
        {
            Vertex.DeleteEdge(item.Edge);
        }

        // TypedEdge

        IEdge edge;
        public IEdge Edge { get { return edge; } }

        IVertex vertex;
        public IVertex Vertex
        {
            get { return vertex; }
            set
            {
                throw new Exception("please correct. not handling Vertex set in UXItem");
            }
        }
    }
}
