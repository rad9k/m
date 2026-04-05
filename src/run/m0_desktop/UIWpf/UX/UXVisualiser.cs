using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

/*

*** LOAD ITEM SUB ITEM

IF item.UXTemplate.ContainerEdgeMetaVertex != null
 	IF existing edge: (item.ParentItem.BaseEdgeTo <> item.BaseEdgeTo).Meta == item.UXTemplate.ContainerEdgeMetaVertex
		THAN item.ContainerEdge = edge

	IF no item.ParentItem <> item edges
        THAN NOTHING 

        comment: we do not want to make any graph modifications during load time


*** ADD NEW SUB ITEM

IF item.UXTemplate.ContainerEdgeMetaVertex != null
 	IF existing edge: (item.ParentItem.BaseEdgeTo <> item.BaseEdgeTo).Meta edge is container edge
		THAN item.ContainerEdgeMetaVertex = edge
    ELSE crete new item.UXTemplate.ContainerEdgeMetaVertex from item.ParentItem.BaseEdgeTo to item.BaseEdgeTo
 
 */

namespace m0.UIWpf.UX
{
    public class MetaToPair
    {
        public IVertex Meta;
        public IVertex To;
        public int NumberOfDecoratorsWithSameMetaTo;
        public int NumberOfEdgesWithSameMetaTo;
    }

    public class UXVisualiser : Border, IListVisualiser, IUXVisualiser, IMouseWheelHandler
    {
        public bool ForceVertexChangeOff { 
            get { return VisualiserHelper.ForceVertexChangeOff; } 
            set { VisualiserHelper.ForceVertexChangeOff = value; } }

        public event Notify SelectedEdgesChange;

        static IVertex systemMetaBaseVertex = m0.MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex");

        public IEdge ContainerEdge { get; set; }

        public int NestingLevel { get; set; }

        public IItem ParentItem { get; set; }

        bool IsVisualiser;

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        IHasScrollViewer ScrollViewerParent;

        //
        
        public Canvas Canvas { get; set; }

        public bool IsSelecting { get; set; }

        public bool IsDrawingOrMovingLine { get; set; }

        public double ClickPositionX_ItemCordinates { get; set; }
        public double ClickPositionY_ItemCordinates { get; set; }

        public double ClickPositionX_AnchorCordinates { get; set; }
        public double ClickPositionY_AnchorCordinates { get; set; }

        public IUXItem ClickedItem { get; set; }

        public ClickTargetEnum ClickTarget { get; set; }

        public FrameworkElement ClickedAnchor { get; set; }

        public ILineDecoratorBase prevSelectedLine;
        public ILineDecoratorBase SelectedLine;
        public IUXItem SelectedLine_FromItem;


        public double LineSelectionDelta { get { return 10; } }

        //

        public Line CreateOrMoveDiagramLine;

        public SelectionArea SelectionArea;

        public IUXItem HighlightedItem;


        bool IsFirstPainted = false;

        static string[] _MetaTriggeringUpdateVertex = new string[] { "Width", "Height" };
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public virtual string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public virtual void BaseEdgeToUpdated() { Paint(); }

        //

        public UXVisualiser(IEdge _edge)
        {
            Edge = _edge;

            vertex = _edge.To;

            TypedEdge.vertexDictionary.Add(this.Edge.To, this);

            IsVisualiser = false;

            NestingLevel = 0;

            ForceVertexChangeOff = false;
        }

        //

        protected Brush GetBackgroundBrush()
        {
            ZeroTypes.UX.Color backgroundColor = BackgroundColor;

            if (backgroundColor != null)
                return backgroundColor.GetBrush();
            else
                return (Brush)FindResource("0BackgroundBrush");
        }

        void SetUpCanvas()
        {            
            Canvas = new Canvas();                        

            this.Child = Canvas;
        }

        public UXVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            if (VisualisersList.GetVisualiser(baseEdgeVertex.Get(false, "To:")) != null)
            {
                UserInteractionUtil.ShowException("Diagram Visualiser", "There is allready Diagram Visualiser opened for this Edge", ExceptionLevelEnum.Warning);

                canLoad = false;

                return;
            }            

            IsVisualiser = true;

            SetUpCanvas();

            this.BorderThickness = new Thickness(1);

            this.BorderBrush = (Brush)FindResource("0LightGrayBrush");

            new ListVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\UX"),
                this,
                "UXV",
                this,
                false,
                new List<string> { @""},
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst,
                true);

            this.AllowDrop = true;
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.MouseMove += MouseMoveHandler;
            this.MouseLeave += MouseLeaveHandler;
            this.MouseLeftButtonDown += MouseButtonDownHandler;
            this.MouseLeftButtonUp += MouseButtonUpHandler;
            this.Drop += dndDrop;

            this.KeyDown += Diagram_KeyDown;

            ForceVertexChangeOff = false;
        }

        public void MouseWheelAction(MouseWheelEventArgs e)
        {            
            double scale = Scale;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 
            
            double toBeScale = 0;

            if (e.Delta > 0)
            {
                if (scale >= 0 && scale < 10)
                    toBeScale = scale + 1;

                if (scale >= 10 && scale < 20)
                    toBeScale = scale + 2;

                if (scale >= 20 && scale < 40)
                    toBeScale = scale + 5;

                if (scale >= 40)
                    toBeScale = scale + 10;
            }
            else
            {                
                if (scale >= 0 && scale < 10)
                    toBeScale = scale - 1;

                if (scale >= 10 && scale < 20)
                    toBeScale = scale - 2;

                if (scale >= 20 && scale < 40)
                    toBeScale = scale - 5;

                if (scale >= 40)
                    toBeScale = scale - 10;                
            }

            toBeScale = Math.Abs(toBeScale);

            if (toBeScale < 0)
                Scale = 0.001;
            else
                Scale = toBeScale;

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        double prev_Scale = -1;

        public void ScaleChange()
        {
            if (prev_Scale == -1)
                prev_Scale = 1;

            //

            double scale = Scale / 100;

            ScrollViewer sv = ScrollViewerParent.GetScrollViewer();

            double half_horizontal = sv.ActualWidth / 2;
            double half_vertical = sv.ActualHeight / 2;

            double scrollBarPosAbstract_horizontal = (sv.HorizontalOffset + half_horizontal) / (Canvas.ActualWidth * prev_Scale);
            double scrollBarPosAbstract_vertical = (sv.VerticalOffset + half_vertical) / (Canvas.ActualHeight * prev_Scale);

            //

            if (scale != 1.0)
            {
                if (ActualHeight != 0)
                {
                    this.LayoutTransform = new ScaleTransform(scale, scale, ActualWidth / 2, ActualHeight / 2);
                }
            }
            else
                this.LayoutTransform = null;

            //

            sv.ScrollToHorizontalOffset((scrollBarPosAbstract_horizontal * Canvas.ActualWidth * scale) - half_horizontal);
            sv.ScrollToVerticalOffset((scrollBarPosAbstract_vertical * Canvas.ActualHeight * scale) - half_vertical);

            prev_Scale = scale;
        }        
    
        IVertex vertex = null;

        public IVertex Vertex
        {
            get
            {
                if (IsVisualiser && VisualiserHelper != null)
                    return VisualiserHelper.Vertex;
                else
                    return vertex;
            }
            set
            {
                if (IsVisualiser && VisualiserHelper != null)
                    VisualiserHelper.SetVertex(value);
                else
                    vertex = value;
            }
        }

        // OPTIMISATION START

        List<IUXItem> Items_all = new List<IUXItem>();

        Dictionary<IVertex, List<IUXItem>> ItemsDictionaryByBaseEdgeTo = new Dictionary<IVertex, List<IUXItem>>();

        bool _needRebuildItemsDictionary = true;

        void needRebuildItemsDictionary()
        {
            _needRebuildItemsDictionary = true;
            _needRebuildItemsDictionaryByVertex = true;
        }

        void RebuidItemsDictionary()
        {
            ItemsDictionaryByBaseEdgeTo.Clear();

            foreach(IUXItem ui in Items_all)
                {                    
                    IVertex ui_BaseEdgeTo = ui.BaseEdgeTo;

                    if (ItemsDictionaryByBaseEdgeTo.ContainsKey(ui_BaseEdgeTo))
                        ItemsDictionaryByBaseEdgeTo[ui_BaseEdgeTo].Add(ui);
                    else
                    {
                        List<IUXItem> list = new List<IUXItem>();
                        list.Add(ui);

                        ItemsDictionaryByBaseEdgeTo.Add(ui_BaseEdgeTo, list);
                    }
                }

            _needRebuildItemsDictionary = false;
        }

        public Dictionary<IVertex, List<IUXItem>> GetItemsDictionaryByBaseEdgeTo()
        {
            if (_needRebuildItemsDictionary)
                RebuidItemsDictionary();

            return ItemsDictionaryByBaseEdgeTo;
        }

        Dictionary<IVertex, IUXItem> ItemsDictionaryByVertex = new Dictionary<IVertex, IUXItem>();

        bool _needRebuildItemsDictionaryByVertex = true;

        void RebuidItemsByVertexDictionary()
        {
            ItemsDictionaryByVertex.Clear();

            foreach (IUXItem ui in Items_all)
            {
                IVertex ui_Vertex = ui.Vertex;

                ItemsDictionaryByVertex[ui.Vertex] = ui;
            }

            _needRebuildItemsDictionaryByVertex = false;
        }

        public Dictionary<IVertex, IUXItem> GetItemsDictionaryByVertex()
        {
            if (_needRebuildItemsDictionaryByVertex)
                RebuidItemsByVertexDictionary();

            return ItemsDictionaryByVertex;
        }

        // OPTIMISATION END

        public void RemoveUXItem(IUXItem item)
        {
            if (item is IUXContainer)
            {
                IUXContainer itemContainer = (IUXContainer)item;

                foreach (ITypedEdge typedEdge in itemContainer.Items)
                {
                    if (typedEdge is IUXItem)
                        RemoveUXItem((IUXItem)typedEdge);
                }
            }

            item.ParentItem.RemoveItem(item);

            Items_all.Remove(item);

            needRebuildItemsDictionary();

            item.RemoveFromCanvas();

            item.Dispose(); // check if will not cause problems

            RemoveAllDecoratorsWithGivenBaseEdgeTo(item);
        }

        private void RemoveAllDecoratorsWithGivenBaseEdgeTo(IUXItem item)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            foreach (IUXItem i in  Items_all)
                foreach (IUXItem decorator in i.Decorators)
                    if (decorator is ILineDecoratorBase)
                    {
                        ILineDecoratorBase lineDecorator = (ILineDecoratorBase)decorator;

                        if (lineDecorator.ToItem.BaseEdgeTo == item.BaseEdgeTo)
                            i.RemoveDecorator(lineDecorator);
                    }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        // TOO
        protected List<IUXItem> GetItemsByBaseEdge(IVertex edgeVertex)
        {
            Dictionary<IVertex, List<IUXItem>> dict = GetItemsDictionaryByBaseEdgeTo();

            IVertex to = GraphUtil.GetQueryOutFirst(edgeVertex, "To", null);

            if (dict.ContainsKey(to))
                return dict[to];

           return new List<IUXItem>();
        }

        protected List<IUXItem> GetItemsByVertex(IVertex edgeVertex)
        {
            Dictionary<IVertex, IUXItem> dict = GetItemsDictionaryByVertex();

            IVertex to = GraphUtil.GetQueryOutFirst(edgeVertex, "To", null);

            if (dict.ContainsKey(to))
            {
                List<IUXItem> l = new List<IUXItem>();
                l.Add(dict[to]);
                return l;
            }

            return new List<IUXItem>();
        }

        public static void AddEdgesFromDefintion(IVertex baseVertex, IVertex definitionEdges)
        {
            if (definitionEdges == null)
                return;

            foreach (IEdge e in definitionEdges.OutEdgesRaw)
                if(!(GraphUtil.ExistQueryOut(e.Meta, "$NoCopy", null) || GraphUtil.ExistQueryOut(e.To, "$NoCopy", null)))
                {
                    if(VertexOperations.IsAtomicVertex(e.To))
                        GraphUtil.SetVertexValue(baseVertex, e.Meta, e.To.Value); // shallow copy
                    else
                        GraphUtil.CreateOrReplaceEdge_DeepCopy(baseVertex, e.Meta, e.To); // deep BUT NOT COPY
                }
        }

        public void HostItem(IUXContainer host, IUXItem item, bool newItemCreation){
            if (!(item is UIElement))
                return;

            Items_all.Add(item);

            if (item is IUXMultiContainerSubItem)
            {                
                IUXMultiContainerSubItem item_multiContainerSubItem = (IUXMultiContainerSubItem)item;

                if (!item_multiContainerSubItem.SubItemsNotVisible) // not adding items when we are about to display code here
                    foreach (ITypedEdge _i in item.Items)
                    {
                        IUXItem i = UXItem.GetUXItem(this, _i);

                        if (i == null)
                            continue;
                    
                        HostItem(item_multiContainerSubItem, i, newItemCreation);
                    }

                return;
            }                        

            //

            UIElement item_UIElement = (UIElement)item;

            item.OwningVisualiser = this;                

            item.VertexSetedUp();

            needRebuildItemsDictionary();            
                
            Panel.SetZIndex(item_UIElement, 1);
               
            Canvas.SetLeft(item_UIElement, item.Position.X);
            Canvas.SetTop(item_UIElement, item.Position.Y);

            try
            {
                if (host.Canvas != null)
                {
                    var parent = VisualTreeHelper.GetParent(item_UIElement) as Canvas;
                    if (parent != null)
                    {
                        parent.Children.Remove(item_UIElement);
                    }

                    host.Canvas.Children.Add(item_UIElement);
                }
            }
            catch (Exception ex) 
            {
                UserInteractionUtil.ShowException("UXVisualiser", "Item allready opened in another visualiser instance", ExceptionLevelEnum.Warning);
            }

            item.NestingLevel = host.NestingLevel + 1;

            //                     

            if (item is IUXContainer)
            {
                IUXContainer container = (IUXContainer)item;

                if (container.Canvas != null)
                    container.Canvas.Children.Clear();

                foreach (ITypedEdge _i in container.Items)
                {
                    IUXItem i = UXItem.GetUXItem(this, _i);

                    if (i == null)
                        continue;

                    HostItem(container, i, newItemCreation);
                }
            }
            //

            item_UIElement.UpdateLayout();

            FindAndOrCreateContainerEdge(item, newItemCreation);
        }

        void FindAndOrCreateContainerEdge(IUXItem item, bool userDirectInteraction)
        {
            IUXContainer itemParentItem = (IUXContainer)item.ParentItem;

            IVertex ContainerEdgeMetaVertex = null;
            
            if(itemParentItem.UXTemplate != null)
                ContainerEdgeMetaVertex = itemParentItem.UXTemplate.ContainerEdgeMetaVertex;

            IVertex itemBaseEdgeTo = item.BaseEdgeTo;

            IVertex itemParentItemBaseEdgeTo = item.ParentItem.BaseEdgeTo;

            item.ContainerEdge = null;

            if (ContainerEdgeMetaVertex != null)
            {
                IEdge foundEdge = null;

                foreach (IEdge e in GraphUtil.GetQueryOut(itemParentItemBaseEdgeTo, ContainerEdgeMetaVertex.Value, itemBaseEdgeTo.Value))
                    if (e.To == itemBaseEdgeTo)
                        foundEdge = e;

                if (foundEdge != null)
                    item.ContainerEdge = foundEdge;

                if (foundEdge == null && userDirectInteraction)                                                        
                    item.ContainerEdge = itemParentItemBaseEdgeTo.AddEdge(ContainerEdgeMetaVertex, itemBaseEdgeTo);                
            }

            //

            if (item.ContainerEdge != null)
            {  // check if need to remove line                
                foreach(IUXItem i in itemParentItem.Decorators)                
                    if(i is LineDecoratorBase)
                    {
                        LineDecoratorBase line = (LineDecoratorBase)i;

                        IEdge lineBaseEdge = line.BaseEdge;

                        if (lineBaseEdge.Meta == item.ContainerEdge.Meta &&
                            lineBaseEdge.To == item.ContainerEdge.To)                        
                            itemParentItem.RemoveDiagramLine(line);                                                    
                    }                
            }
        }

        // TOO
        
        public void AddLineObjects()
        {
           List<MetaToPair> metatopairs = new List<MetaToPair>();

           foreach(ITypedEdge _item in Items_all)
               {
                   IUXItem item = UXItem.GetUXItem(this, _item);

                   if (item == null)
                      continue;

                   metatopairs.Clear();

                   foreach (IUXItem decorator in item.Decorators) // calculate LineDecorator number and Edges number for each Meta/To edge pair
                                                                //foreach (IEdge l in item.Vertex.GetAll(false, "DiagramLine:")) // calculate DiagramLines number and Edges number for each Meta/To edge pair
                   {
                       if (!(decorator is ILineDecoratorBase))
                       {
                            ILineDecoratorBase line_decorator = (ILineDecoratorBase)decorator;

                            if (!Items_all.Contains(line_decorator.ToItem))
                                continue;
                       }
                       else
                           continue;
                       
                       MetaToPair found = null;

                       Edge decorator_BaseEdge = decorator.BaseEdge;

                       foreach (MetaToPair pair in metatopairs)
                           if (pair.Meta == decorator_BaseEdge.Meta && pair.To == decorator_BaseEdge.To)
                               found = pair;

                       if (found == null)
                       {
                           MetaToPair newpair = new MetaToPair();
                           newpair.Meta = decorator_BaseEdge.Meta;
                           newpair.To = decorator_BaseEdge.To;
                           newpair.NumberOfDecoratorsWithSameMetaTo = 1;
                           newpair.NumberOfEdgesWithSameMetaTo = 0;

                           foreach (IEdge e in item.BaseEdge.To)
                               if (newpair.Meta == e.Meta && newpair.To == e.To)
                                   newpair.NumberOfEdgesWithSameMetaTo++;

                           metatopairs.Add(newpair);
                       
                       } else
                           found.NumberOfDecoratorsWithSameMetaTo++;
                   }

                   foreach (MetaToPair pair in metatopairs) { // delete DiagramLines for edges that been deleted
                       if (pair.NumberOfDecoratorsWithSameMetaTo > pair.NumberOfEdgesWithSameMetaTo)
                            foreach (IUXItem decorator in item.Decorators)
                            {
                                Edge decorator_BaseEdge = decorator.BaseEdge;

                                if (pair.Meta == decorator_BaseEdge.Meta 
                                    && pair.To == decorator_BaseEdge.To 
                                    && pair.NumberOfDecoratorsWithSameMetaTo > pair.NumberOfEdgesWithSameMetaTo)
                                {
                                   item.Vertex.DeleteEdge(decorator.Edge);
                                   pair.NumberOfDecoratorsWithSameMetaTo--;
                                }
                            }
                   }

                    foreach (IUXItem decorator in item.Decorators)
                    // add diagram line objects
                        if (decorator is LineDecorator)
                        {                        
                            LineDecorator lineDecorator = (LineDecorator)decorator;

                            if (!Items_all.Contains(lineDecorator.ToItem))
                                continue;

                        item.AddDiagramLineObject(lineDecorator.ToItem, lineDecorator, false); 
                        }                    
               }
           
        }

        // TOO
        public IUXItem GetToDiagramItemFromLineVertex(LineDecorator lineDecorator) // this one is probably NOT needed
        {
            IVertex toFind = null;

            Edge lineDecorator_BaseEdge = lineDecorator.BaseEdge;

            if (GraphUtil.ExistQueryOut(lineDecorator_BaseEdge.Meta, "$VertexTarget", null)
            //&& !((UXDecoratorTemplate)lineDecorator.UXTemplate).CreateEdgeOnly // ???? ZZZ added ! hope this is ok
            )
                toFind = GraphUtil.GetQueryOutFirst(lineDecorator_BaseEdge.To, "$EdgeTarget", null);
            else
                toFind = lineDecorator_BaseEdge.To;

            if (toFind != null)
            {
                Dictionary<IVertex, List<IUXItem>> dict = GetItemsDictionaryByBaseEdgeTo();

                if(dict.ContainsKey(toFind))
                    foreach (IUXItem i in dict[toFind])
                    {
                        string tdtq = ((UXDecoratorTemplate)lineDecorator.UXTemplate).ToDiagramItemTestQuery;

                        if (!(tdtq != null && i.Vertex.Get(false, tdtq) == null))
                            return i;
                    }
            }

            return null;
        }

        // TOO

        Point GetItemAbsolutePosition(IUXItem item)
        {            
            UIElement uie = (UIElement)item;

            if (item.ParentItem != this)
                return uie.TranslatePoint(new Point(0, 0), Canvas);
            else                 
                return new Point(Canvas.GetLeft(uie), Canvas.GetTop(uie));            
        }
               
       void SelectItemsBySelectionArea()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 
            

            double left = SelectionArea.Left;
            double top = SelectionArea.Top;
            double right = SelectionArea.Right;
            double bottom = SelectionArea.Bottom;

            UnselectAllSelectedEdges_NoSelectedVerticesUpdated();

            foreach(ITypedEdge _i in Items_all)                
                {
                    IUXItem i = UXItem.GetUXItem(this, _i);

                    if (i == null || !(_i is FrameworkElement))
                        continue;

                    FrameworkElement i_FrameworkElement = (FrameworkElement)i;

                    Point itemLeftTop = GetItemAbsolutePosition(i);

                    int ileft, itop, iright, ibottom;

                    ileft = (int)itemLeftTop.X;
                    itop = (int)itemLeftTop.Y;
                    
                    iright = ileft + (int)i_FrameworkElement.ActualWidth;
                    ibottom = itop + (int)i_FrameworkElement.ActualHeight;

                    if (left <= ileft && right >= iright && top <= itop && bottom >= ibottom)
                        i.AddToSelectedEdges();
                }

            SelectedVerticesUpdated();

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        public virtual void ViewAttributesUpdated()        
        {
            Paint();
        }

        public void Paint()
        {
            if (ActualHeight != 0 || IsFirstPainted)
            {
                ScaleChange();

                Canvas.Children.Clear();

                Width = Size.Width ;
                Height = Size.Height;

                Background = GetBackgroundBrush();
                    //new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 200));

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                //////////////////////////////////////// 

                Items_all.Clear();

                //

                foreach (ITypedEdge _i in Items)
                {
                    IUXItem i = UXItem.GetUXItem(this, _i);

                    if (i == null)
                        continue;

                    if (CheckIfItemIsValidAndRemoveIfInvalid(i))
                        HostItem(this, i, false);
                } 
                
                //
                        
                UpdateLayout(); // here

                AddLineObjects();

                SelectionArea = new SelectionArea(Canvas);
                

                SelectionArea.HideSelectionArea();

               
                SelectWrappersForSelectedVertices();

                IsFirstPainted = true;

                
                CheckAndUpdateDiagramLines();
                

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////    
            }
        }

        private bool CheckIfItemIsValidAndRemoveIfInvalid(IUXItem item)
        {
            if (item.BaseEdgeTo == null)
            {
                item.ParentItem.RemoveItem(item);
                return false;
            }

            return true;
        }
        

        public void SetFocus()
        {
            if (SuspendSetFocus)
                return;

            this.Focusable = true;

            Keyboard.Focus(this);
        }

        bool canLoad = true;

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            if (IsFirstPainted || !canLoad)
                return;

            SetFocus();

            ScrollViewerParent = GetScrollViewerParent(this);

            Paint();

            if (IsFirstPainted)
                this.Loaded -= OnLoad;

            VisualiserHelper.AddContextMenu();            
        }             

        IHasScrollViewer GetScrollViewerParent(DependencyObject e)
        {
            if (e == null || !(e is FrameworkElement))
                return null;

            if (e is IHasScrollViewer)
                return (IHasScrollViewer)e;

            return GetScrollViewerParent(((FrameworkElement)e).Parent);
        }

        bool IsLineSelected = false;  

        private void Diagram_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                if (IsLineSelected)
                    DeleteLine();
                else
                    DeleteSelectedItems();
        }

        private void DeleteSelectedItems()
        {
            IVertex selectedEdges = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");
            if (selectedEdges.Count() == 0)
                return;

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();
            info.Value = "DELETE UX item / underlying vertex?";

            IVertex options = m0.MinusZero.Instance.CreateTempVertex();

            IVertex optionUXItemDelete = options.AddVertex(null, "UX Item only delete");
            IVertex optionUnderlyingEdgeDelete = options.AddVertex(null, "Underlying Edge delete");
            IVertex optionUnderlyingVertexDelete = options.AddVertex(null, "Underlying Vertex remove from repository");
            IVertex optionCancel = options.AddVertex(null, "Cancel");

            IVertex option = MinusZero.Instance.UserInteraction.InteractionSelectButton(info, options.OutEdges);

            if (option == null || option == optionCancel)
                return;

            IList<IEdge> selectedEdges_copy = GeneralUtil.CreateAndCopyList<IEdge>(selectedEdges);

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            UnselectAllSelectedEdges();

            foreach (IEdge e in selectedEdges_copy)
            {  // what about multiple items for same BaseEdge:\To: ?

                IUXItem i = GetItemsDictionaryByVertex()[e.To.Get(false, "To:")];

                if (option == optionUXItemDelete)
                {
                    GraphUtil.DeleteEdgeByToVertex(i.Edge.From, i.Vertex);
                    RemoveUXItem(i);
                }

                if (option == optionUnderlyingEdgeDelete)
                {
                    GraphUtil.DeleteEdgeByToVertex(i.Edge.From, i.Vertex);
                    RemoveUXItem(i);

                    Edge i_BaseEdge = i.BaseEdge;
                        
                    VertexOperations.DeleteOneEdge(i_BaseEdge.From,
                        i_BaseEdge.Meta,
                        i_BaseEdge.To);
                }

                if (option == optionUnderlyingVertexDelete)
                {
                    GraphUtil.DeleteEdgeByToVertex(i.Edge.From, i.Vertex);
                    RemoveUXItem(i);

                    i.BaseEdge.To.Dispose();
                }
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        private void DeleteLine()
        {
            bool onlyEdge = true;

            if (SelectedLine.BaseEdge.Meta.Get(false, @"$VertexTarget:") != null)
                onlyEdge = false;

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();

            if (onlyEdge)
                info.Value = "DELETE line's edge?";
            else
                info.Value = "DELETE line's vertex?";

            IVertex options = m0.MinusZero.Instance.CreateTempVertex();

            options.AddVertex(null, "Yes");
          
            IVertex optionCancel = options.AddVertex(null, "Cancel");

            IVertex option = MinusZero.Instance.UserInteraction.InteractionSelectButton(info, options.OutEdges);

            if (option == optionCancel || option == null)
                return;

            SelectedLine.FromDiagramItem.RemoveDiagramLine(SelectedLine);

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////            

            if (onlyEdge)
            {             
                GraphUtil.DeleteEdge(SelectedLine.FromDiagramItem.Vertex.Get(false, @"BaseEdge:\To:"), 
                    SelectedLine.Vertex.Get(false, @"BaseEdge:\Meta:"),
                    SelectedLine.Vertex.Get(false, @"BaseEdge:\To:"));
            }
            else
            {
                GraphUtil.DeleteEdge(SelectedLine.Vertex.Get(false, @"BaseEdge:\To:"),
               MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"),
               SelectedLine.Vertex.Get(false, @"BaseEdge:\To:\$EdgeTarget:"));

                GraphUtil.DeleteEdge(SelectedLine.FromDiagramItem.Vertex.Get(false, @"BaseEdge:\To:"),
                  SelectedLine.Vertex.Get(false, @"BaseEdge:\Meta:"),
                  SelectedLine.Vertex.Get(false, @"BaseEdge:\To:"));
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////            
        }

        protected void CreateAndUpdateCreateDiagramLine(double ToX, double ToY)
        {
            if (CreateOrMoveDiagramLine == null)
            {
                CreateOrMoveDiagramLine = new Line();

                IsDrawingOrMovingLine = true;

                Panel.SetZIndex(CreateOrMoveDiagramLine, 100000);

                CreateOrMoveDiagramLine.Stroke = (Brush)FindResource("0VeryLightHighlightBrush");
                CreateOrMoveDiagramLine.Stroke = (Brush)FindResource("0VeryLightHighlightBrush");

                CreateOrMoveDiagramLine.StrokeThickness = 2;

                Canvas.Children.Add(CreateOrMoveDiagramLine);

                FrameworkElement ClickedItem_FrameworkElement = (FrameworkElement)ClickedItem;

                Point ClickedItem_absolutePosition = GetItemAbsolutePosition(ClickedItem);

                CreateOrMoveDiagramLine.X1 = ClickedItem_absolutePosition.X + ClickedItem_FrameworkElement.ActualWidth;
                CreateOrMoveDiagramLine.Y1 = ClickedItem_absolutePosition.Y;
            }

            CreateOrMoveDiagramLine.X2 = ToX;
            CreateOrMoveDiagramLine.Y2 = ToY;

            Point p = new Point(ToX, ToY);

            UnhighlightAllSelectedEdges_noDecorators();
            
            IUXItem toHighlightItem = GetItemByPoint(p);

            if (toHighlightItem != null)
            {
                toHighlightItem.Highlight();
                HighlightedItem = toHighlightItem;
            }            
        }

        protected void CreateAndUpdateMoveDiagramLine(double ToX, double ToY)
        {
            if (CreateOrMoveDiagramLine == null)
            {
                CreateOrMoveDiagramLine = new Line();

                IsDrawingOrMovingLine = true;

                Panel.SetZIndex(CreateOrMoveDiagramLine, 100000);

                CreateOrMoveDiagramLine.Stroke = (Brush)FindResource("0HighlightBrush");

                CreateOrMoveDiagramLine.StrokeThickness = 2;

                Canvas.Children.Add(CreateOrMoveDiagramLine);

                FrameworkElement ClickedItem_FrameworkElement = (FrameworkElement)ClickedItem;

                CreateOrMoveDiagramLine.X1 = SelectedLine.FromX;
                CreateOrMoveDiagramLine.Y1 = SelectedLine.FromY;
            }

            CreateOrMoveDiagramLine.X2 = ToX;
            CreateOrMoveDiagramLine.Y2 = ToY;
            
            Point p = new Point(ToX, ToY);

            UnhighlightAllSelectedEdges_noDecorators();

            IUXItem toHighlightItem = GetItemByPoint(p);

            if (toHighlightItem != null)
            {
                toHighlightItem.Highlight();
                HighlightedItem = toHighlightItem;
            }            
        }

        bool IsMultiSelectionMoving = false;

        List<Rectangle> MovingSprites=new List<Rectangle>();

        void AddOrMoveMultiSelectionMovingSprites(double x, double y)
        {
            if (IsMultiSelectionMoving == false)
            {
                IsMultiSelectionMoving = true;

                MovingSprites.Clear();

                foreach (IEdge ed in Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"))
                    foreach (IUXItem item in GetItemsByVertex(ed.To))
                    {
                        if (!(item is FrameworkElement) || item.NestingLevel != 1)
                            continue;

                        FrameworkElement item_FrameworkElement = (FrameworkElement)item;

                        double rx = Canvas.GetLeft(item_FrameworkElement);
                        double ry = Canvas.GetTop(item_FrameworkElement);
                        double rwidth = item_FrameworkElement.ActualWidth;
                        double rheight = item_FrameworkElement.ActualHeight;

                        Rectangle r = new Rectangle();
                        Canvas.SetLeft(r, rx);
                        Canvas.SetTop(r, ry);
                        r.Width = rwidth;
                        r.Height = rheight;
                        r.Stroke= (Brush)FindResource("0ForegroundBrush");
                        r.StrokeDashArray = new DoubleCollection(new double[] { 1, 4 });
                        r.Tag = item;
                        Panel.SetZIndex(r, 99999);

                        Canvas.Children.Add(r);
                        MovingSprites.Add(r);
                    }
            }else
            {
                foreach(Rectangle r in MovingSprites)
                {
                    IUXItem i = (IUXItem)r.Tag;
                    
                    Canvas.SetLeft(r, i.Position.X +x);
                    Canvas.SetTop(r, i.Position.Y + y);        
                }
            }
        }

        void RemoveMultiSelectionMovingSprites(double x, double y)
        {
            IsMultiSelectionMoving = false;

            foreach (Rectangle r in MovingSprites)
                Canvas.Children.Remove(r);

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////            

            foreach (IEdge ed in Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"))
                foreach (IUXItem item in GetItemsByVertex(ed.To))
                    if (item.NestingLevel == 1)
                        item.MoveItem(item.Position.X + x, item.Position.Y + y, false);
                    else
                        item.MoveItem(x, y, true);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        //protected void MouseButtonDownHandler(object sender, MouseButtonEventArgs e)
        public void MouseButtonDownHandler(object sender, MouseButtonEventArgs e)
        {            
            SelectionArea.StartSelection(e.GetPosition(Canvas));

            ClickTarget = ClickTargetEnum.Selection;         

            UnselectAllSelectedEdges();
        }

        protected void MouseLeaveHandler(object sender, MouseEventArgs e)
        {            
            MouseUpOrLeave(false, e);
        }

        public void MouseButtonUpHandler(object sender, MouseButtonEventArgs e)
        {         
            MouseUpOrLeave(true, e);
        }

        protected void MouseUpOrLeave(bool IsUp, MouseEventArgs e)
        {
            if (ClickTarget == ClickTargetEnum.Selection)
            {
                SelectItemsBySelectionArea();

                SelectionArea.HideSelectionArea();
            }
            if (!(ClickedItem is FrameworkElement))
                return;

            FrameworkElement ClickedItem_FrameworkElemet = (FrameworkElement)ClickedItem;

            SetFocus();

            if (ClickTarget == ClickTargetEnum.Item)
                if (IsMultiSelectionMoving)
                    RemoveMultiSelectionMovingSprites(e.GetPosition(ClickedItem_FrameworkElemet).X - ClickPositionX_ItemCordinates,
                            e.GetPosition(ClickedItem_FrameworkElemet).Y - ClickPositionY_ItemCordinates);
                else
                    CheckAndUpdateItemParent(ClickedItem, false);


            if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine
                || ClickTarget == ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine)
            {
                if (HighlightedItem != null)
                {
                    HighlightedItem.Unhighlight();

                    if (IsUp)
                        DoCreateDiagramLine(ClickedItem, HighlightedItem);
                }

                HighlightedItem = null;
                Canvas.Children.Remove(CreateOrMoveDiagramLine);
                CreateOrMoveDiagramLine = null;

                IsDrawingOrMovingLine = false;
            }

            if (ClickTarget == ClickTargetEnum.AnchorRightTop_MoveDiagramLine)
            {
                if (HighlightedItem != null)
                {
                    HighlightedItem.Unhighlight();

                    if (IsUp)
                        DoMoveLineProcess(SelectedLine_FromItem, HighlightedItem, SelectedLine);
                }

                HighlightedItem = null;
                Canvas.Children.Remove(CreateOrMoveDiagramLine);
                CreateOrMoveDiagramLine = null;

                IsDrawingOrMovingLine = false;
            }

            ClickTarget = ClickTargetEnum.MouseUpOrLeave;         
        }

        //protected void MouseMoveHandler(object sender, MouseEventArgs e)
        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ClickTarget == ClickTargetEnum.Selection) // selection
                {
                    SelectionArea.MoveSelectionArea(e.GetPosition(Canvas));

                    //SelectItemsBySelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                    // too slow
                }

                if (!(ClickedItem is FrameworkElement))
                    return;

                FrameworkElement ClickedItem_FrameworkElement = (FrameworkElement)ClickedItem;

                //

                Point clickedItem_absolute = GetItemAbsolutePosition(ClickedItem);

                double ClickedItem_left = clickedItem_absolute.X;
                double ClickedItem_top = clickedItem_absolute.Y;

                //

                if (ClickTarget == ClickTargetEnum.AnchorLeftTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(Canvas).X - ClickPositionX_ItemCordinates),
                        (e.GetPosition(Canvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem_FrameworkElement.ActualWidth - ((e.GetPosition(Canvas).X - ClickPositionX_ItemCordinates) - ClickedItem_left),
                        ClickedItem_FrameworkElement.ActualHeight - ((e.GetPosition(Canvas).Y - ClickPositionY_ItemCordinates) - ClickedItem_top)); 
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        ClickedItem_left,
                        (e.GetPosition(Canvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem_FrameworkElement.ActualWidth,
                       ClickedItem_FrameworkElement.ActualHeight - ((e.GetPosition(Canvas).Y - ClickPositionY_ItemCordinates) - ClickedItem_top));
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
                    CreateAndUpdateCreateDiagramLine(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);

                if (ClickTarget == ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine)
                    CreateAndUpdateCreateDiagramLine(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);

                if (ClickTarget == ClickTargetEnum.AnchorRightTop_MoveDiagramLine)
                    CreateAndUpdateMoveDiagramLine(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);

                if (ClickTarget == ClickTargetEnum.AnchorLeftMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(Canvas).X - ClickPositionX_ItemCordinates),
                        ClickedItem_top,
                        ClickedItem_FrameworkElement.ActualWidth - ((e.GetPosition(Canvas).X - ClickPositionX_ItemCordinates) - ClickedItem_left),
                        ClickedItem_FrameworkElement.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        ClickedItem_left,
                        ClickedItem_top,
                        e.GetPosition(Canvas).X - ClickedItem_left - ClickPositionX_AnchorCordinates,
                        ClickedItem_FrameworkElement.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorLeftBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      (e.GetPosition(Canvas).X - ClickPositionX_ItemCordinates),
                      ClickedItem_top,
                      ClickedItem_FrameworkElement.ActualWidth - ((e.GetPosition(Canvas).X - ClickPositionX_ItemCordinates) - ClickedItem_left),
                     e.GetPosition(Canvas).Y - ClickedItem_top - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      ClickedItem_left,
                      ClickedItem_top,
                      ClickedItem_FrameworkElement.ActualWidth,
                    e.GetPosition(Canvas).Y - ClickedItem_top - ClickPositionY_AnchorCordinates); ;
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      ClickedItem_left,
                      ClickedItem_top,
                      e.GetPosition(Canvas).X - ClickedItem_left - ClickPositionX_AnchorCordinates,
                    e.GetPosition(Canvas).Y - ClickedItem_top - ClickPositionY_AnchorCordinates);
                }                

                if (ClickTarget == ClickTargetEnum.Item) // item move
                {
                    int selectedEdgesCount = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}").Count();

                    if (( selectedEdgesCount > 0 && ClickedItem.IsSelected == false) ||
                        selectedEdgesCount > 1)
                    {
                        if (ClickedItem.IsSelected == false)
                            ClickedItem.AddToSelectedEdges();

                        AddOrMoveMultiSelectionMovingSprites(e.GetPosition(ClickedItem_FrameworkElement).X - ClickPositionX_ItemCordinates,
                            e.GetPosition(ClickedItem_FrameworkElement).Y - ClickPositionY_ItemCordinates);
                    }
                    else
                    {                        
                        if (ClickedItem.IsSelected == false)
                        {
                            UnselectAllSelectedEdges();

                            ClickedItem.AddToSelectedEdges();
                        }

                        ClickedItem.MoveItem((e.GetPosition(Canvas).X - ClickPositionX_ItemCordinates), 
                            (e.GetPosition(Canvas).Y - ClickPositionY_ItemCordinates), false);
                    }
                }
            } else
            {
                CheckIfLineNeedsSelection(e.GetPosition(Canvas));
            }
        }        

        private void CheckIfLineNeedsSelection(System.Windows.Point p)
        {
            double best = 999999;
            ILineDecoratorBase bestLine = null;
            IUXItem bestLine_FromItem = null;

            foreach (ITypedEdge _i in Items_all)
                {
                    IUXItem i = UXItem.GetUXItem(this, _i);

                    if (i == null)
                        continue;

                    foreach (IUXItem _line in i.Decorators)
                        if(_line is ILineDecoratorBase)
                        {
                            ILineDecoratorBase line = (ILineDecoratorBase)_line;

                            double len = line.GetMouseDistance(p);

                            if (len < best)
                            {
                                bestLine = line;
                                best = len;
                                bestLine_FromItem = i;
                            }
                        }
                }

            if (best < LineSelectionDelta && bestLine != null) 
            {
                if (bestLine != prevSelectedLine)
                {
                    if (prevSelectedLine != null)
                    {
                        prevSelectedLine.Unhighlight();

                        prevSelectedLine.Unselect();
                    }

                    bestLine.Highlight();

                    bestLine.Select();

                    SelectedLine = bestLine;
                    SelectedLine_FromItem = bestLine_FromItem;

                    prevSelectedLine = bestLine;

                  //  UnselectAllSelectedEdges(); need to comment it

                    IsLineSelected = true;
                }
            }else
                if (IsLineSelected)
                {
                    IsLineSelected = false;

                    prevSelectedLine.Unhighlight();

                    prevSelectedLine.Unselect();

                    SelectedLine = null;

                    prevSelectedLine = null;
                }
        }        

        private void DoMoveLineProcess(IUXItem fromItem, IUXItem toItem, ILineDecoratorBase line)
        {
            if (CheckIfCanMove(fromItem, toItem, line))
                MoveLine(fromItem, toItem, line, false);
            else
                if (DoCreateDiagramLine(fromItem, toItem))
                    MoveLine(fromItem, toItem, line, true);            
        }

        private bool CheckIfCanMove(IUXItem fromItem, IUXItem toItem, ILineDecoratorBase line)
        {
            bool canAdd = false;

            UXDecoratorTemplate tem = (UXDecoratorTemplate)line.UXTemplate;

            IVertex edgesToTest = fromItem.BaseEdgeTo;

            if (tem.EdgeTestQuery != null)
                edgesToTest = fromItem.BaseEdgeTo.GetAll(false, tem.EdgeTestQuery);

            foreach (IEdge e in edgesToTest)
                if (CanAddLineByDecoratorTemplateAndFromItemBaseEdgeToQuery(toItem, toItem.BaseEdge, tem, e));
                    canAdd = true;

            return canAdd;
        }

        private void MoveLine(IUXItem fromItem, IUXItem toItem, ILineDecoratorBase line, bool onlyDelete)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 
            
            IEdge toMoveEdge = null;

            IVertex meta = line.BaseEdge.Meta;
            IVertex to = line.BaseEdge.To;

            IVertex fromItemBaseEdgeTo = fromItem.BaseEdgeTo;

            foreach (IEdge e in fromItemBaseEdgeTo)
                if (e.Meta == meta && e.To == to)
                    toMoveEdge = e;

            if (toMoveEdge != null)
            {
                fromItemBaseEdgeTo.DeleteEdge(toMoveEdge);

                fromItem.RemoveDiagramLine(line);

                if (!onlyDelete)
                {
                    IEdge newEdge = fromItemBaseEdgeTo.AddEdge(meta, toItem.BaseEdgeTo);

                    AddDiagramLineVertex(fromItem, newEdge, (UXDecoratorTemplate)line.UXTemplate, toItem);
                }
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////                        
        }

        protected void UnselectAll()
        {            
            foreach (ITypedEdge _i in Items_all)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.Unselect();
            }
        }

        public void UnhighlightAllSelectedEdges()
        {
            foreach (IUXItem i in Items_all)
            {
                i.Unhighlight();

                foreach(IUXItem d in i.Decorators)
                    d.Unhighlight();
            }
        }

        public void UnhighlightAllSelectedEdges_noDecorators()
        {
            foreach (IUXItem i in Items_all)
                i.Unhighlight();
        }

        public void UnselectAllSelectedEdges()
        {
            UnselectAllSelectedEdges_NoSelectedVerticesUpdated();

            SelectedVerticesUpdated();
        }

        private void UnselectAllSelectedEdges_NoSelectedVerticesUpdated()
        {
            IVertex sv = Vertex.Get(false, @"SelectedEdges:");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv); // XXX
                                                          //GraphUtil.RemoveAllEdges(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        public void SelectedVerticesUpdated()
        {
            if (IsFirstPainted)
            {
                UnselectAll();

                SelectWrappersForSelectedVertices();
            }

            if (SelectedEdgesChange != null)
                SelectedEdgesChange();
        }

        protected void SelectWrappersForSelectedVertices()
        {
            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge e in sv)
                if (GetItemsDictionaryByVertex().ContainsKey(e.To.Get(false, "To:")))
                    GetItemsDictionaryByVertex()[e.To.Get(false, "To:")].Select();
        }

        public bool IsDisposed = false;

        private void DisposeAllItems()
        {
            foreach (ITypedEdge e in Items)
                if (e is IDisposable)
                    ((IDisposable)e).Dispose();

            foreach (ITypedEdge e in VolatileItems)
            {
                Vertex.DeleteEdge(e.Edge);

                if (e is IDisposable)
                    ((IDisposable)e).Dispose();
            }
        }

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                if (IsVisualiser)
                {
                    VisualisersList.RemoveVisualiser(this);

                    GraphChangeTrigger.RemoveListener(VisualiserHelper.graphChangeListenerEdge);

                    DisposeAllItems();

                    SaveDiagram();
                }
                else
                {
                    TypedEdge.RemoveFromDictionary(this);
                }
            }
        }

        private void SaveDiagram()
        {
            return;

            string path = GraphUtil.GetQueryBetweenVertexes_byInEdges(this.Vertex, MinusZero.Instance.Root);

            string pathEncoded = Lib.StdView.Html.DiagramQueryToDiagramId_internal(path);

            CanvasToPng.SaveCanvasToPng(Canvas, MinusZero.Instance.Root.Get(false, @"Start:\FullFilename:") + @"\diagrams\" + pathEncoded + ".png");
        }

        public IUXContainer GetItemByPoint_ByCanvas(Point p)
        {
            IUXContainer itemToReturn = this;

            int highestNestingLevel = -1;

            foreach (ITypedEdge _i in Items_all)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null || !(i is IUXContainer) || ((IUXContainer)i).Canvas == null)
                    continue;

                if (i is IUXMultiContainerSubItem && ((IUXMultiContainerSubItem)i).SubItemsNotVisible)
                    continue;

                Canvas item_canvas = ((IUXContainer)i).Canvas;
                
                Point item_absolute = item_canvas.TranslatePoint(new Point(0, 0), Canvas);
                FrameworkElement item_FrameworkElement = (FrameworkElement)i;        

                if (item_absolute.X <= p.X &&
                    item_absolute.Y <= p.Y &&
                    p.X <= item_absolute.X + item_FrameworkElement.ActualWidth &&
                    p.Y <= item_absolute.Y + item_FrameworkElement.ActualHeight)
                    if (i.NestingLevel > highestNestingLevel
                        && !itemToReturn.SubItemsNotVisible)
                    {
                        itemToReturn = (IUXContainer)i;
                        highestNestingLevel = i.NestingLevel;
                    }               
            }
            
            return itemToReturn;
        }

        public IUXItem GetItemByPoint(Point p)
        {
            IUXItem itemToReturn = this;

            int highestNestingLevel = -1;

            foreach (ITypedEdge _i in Items_all)
            {
                IUXItem i = UXItem.GetUXItem(this, _i);

                if (i == null)
                    continue;

                FrameworkElement item_FrameworkElement = (FrameworkElement)i;

                Point item_absolute = item_FrameworkElement.TranslatePoint(new Point(0, 0), Canvas);
                
                if (item_absolute.X <= p.X &&
                    item_absolute.Y <= p.Y &&
                    p.X <= item_absolute.X + item_FrameworkElement.ActualWidth &&
                    p.Y <= item_absolute.Y + item_FrameworkElement.ActualHeight)
                    if (i.NestingLevel > highestNestingLevel)
                    {
                        itemToReturn = i;
                        highestNestingLevel = i.NestingLevel;
                    }
            }

            return itemToReturn;
        }        

        public void CheckAndUpdateItemParent(IUXItem item, bool fastMode)
        {
            Point itemPosition_absolute = GetItemAbsolutePosition(item);

          /*  if (fastMode) // will not use it as seems not to be needed
            {
                Position itemPosition_relative = item.Position;

                FrameworkElement item_FrameworkElement = (FrameworkElement)item;

                if (//item.ParentItem != this && // this will make simple multi selected (move)-> container scenario not working
                    item.ParentItem != null)                
                {
                    FrameworkElement itemParent_FrameworkElement = (FrameworkElement)item.ParentItem;

                    if (itemPosition_relative.X < 0 || itemPosition_relative.Y < 0 ||
                        (itemPosition_relative.X + item_FrameworkElement.ActualWidth) > itemParent_FrameworkElement.ActualWidth ||
                        (itemPosition_relative.Y + item_FrameworkElement.Height) > itemParent_FrameworkElement.ActualHeight)
                    {
                        IUXContainer toBeParentItem = GetItemByPoint_ByCanvas(itemPosition_absolute);

                        if (toBeParentItem == null)
                            toBeParentItem = this;

                        if (toBeParentItem != item.ParentItem && toBeParentItem != item)
                            MoveToParentItem(item, toBeParentItem);
                    }
                }
            }
            else*/
            {
                IUXContainer toBeParentItem = GetItemByPoint_ByCanvas(itemPosition_absolute); // can take some time, especially when moving

                if (toBeParentItem != item.ParentItem 
                    && toBeParentItem.ParentItem != item
                    && toBeParentItem != item)
                    MoveToParentItem(item, toBeParentItem);
            }
        }

        private void MoveToParentItem(IUXItem item, IUXContainer NewParentItem)
        {
            /*if (NewParentItem is IUXMultiContainerItem)
            {
                NewParentItem = ((IUXMultiContainerItem)NewParentItem).GetContainerSubItem(item);

                if (NewParentItem == null)
                    return;
            }*/
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IUXContainer OldParentItem = (IUXContainer)item.ParentItem;            

            if (OldParentItem == null)
                OldParentItem = this;

            OldParentItem.Canvas.Children.Remove((UIElement)item);

            NewParentItem.MoveExistingItemAsThisItemsSubItem(item);
            
            NewParentItem.Canvas.Children.Add((UIElement)item);

            FindAndOrCreateContainerEdge(item, true);

            Point newPosition = OldParentItem.Canvas.TranslatePoint(item.Position.GetPoint(), NewParentItem.Canvas);

            Position p = item.Position;
            p.X = newPosition.X;
            p.Y = newPosition.Y;

            Canvas.SetLeft((UIElement)item, p.X);
            Canvas.SetTop((UIElement)item, p.Y);

            needRebuildItemsDictionary();

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            CheckAndUpdateDiagramLinesForItem(OldParentItem); // container edge might need to be shown
        }

        // IHasLocalizableEdges

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByPoint(Point p) 
        {
            IUXItem item = GetItemByPoint(p);

            IVertex v = MinusZero.Instance.CreateTempVertex();

            IEdge iBaseEdge = item.BaseEdge;

            EdgeHelper.AddEdgeVertexEdges(v, iBaseEdge.From, iBaseEdge.Meta, iBaseEdge.To);            

            return v;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }
        public void AddAsToMetaLine(ILineDecoratorBase line)
        {
            throw new NotImplementedException();
        }

        /////////////////////////////

        private IUXItem AddDiagramItemDialog(Point p, IVertex droppedVertex, bool isSet, DragEventArgs e)
        {
            IUXItem newUXItem = null;

            IVertex r = m0.MinusZero.Instance.Root;

            NewUXItem ndi = new NewUXItem(this, droppedVertex, isSet, WpfUtil.GetMousePositionDnd(e));

            if (ndi.UXTemplate != null)
            {
                if (ndi.InstanceOfMeta)
                {
                    IUXItem clickedItem = GetItemByPoint(p);

                      IEdge ve = VertexOperations.AddInstanceAndReturnEdge(
                          clickedItem.BaseEdge.To                      
                          , ndi.BaseEdge.Get(false, "To:"));

                      IVertex newVertex = ve.To;

                      newVertex.Value = ndi.InstanceValue;

                      if (ndi.UXTemplate.ForceShowEditForm)
                          MinusZero.Instance.UserInteraction.EditEdge(newVertex);

                    IVertex newEdgeVertex = EdgeHelper.CreateTempEdgeVertex(/*ve.From*/null, ve.Meta, ve.To);

                      newUXItem = AddDiagramItem(p,
                                     ndi.UXTemplate,
                                     newEdgeVertex);
                  }
                  else
                  {
                      bool ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = false;
                      bool ThereIsDiagramItemOfThisBaseEdgeTo = false;

                      IVertex DiagramItemOfThisDiagramItemDefinition = Vertex.GetAll(false, @"Item:{UXTemplate:" + ndi.UXTemplate.Vertex.Value + "}");

                      foreach (IEdge ee in DiagramItemOfThisDiagramItemDefinition)
                          if (ee.To.Get(false, @"BaseEdge:\To:") == ndi.BaseEdge.Get(false, "To:"))
                              ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = true;

                      if(GetItemsDictionaryByBaseEdgeTo().ContainsKey(ndi.BaseEdge.Get(false, "To:")))
                      foreach (IUXItem b in GetItemsDictionaryByBaseEdgeTo()[ndi.BaseEdge.Get(false, "To:")])
                          ThereIsDiagramItemOfThisBaseEdgeTo = true;

                      /*if (b.Vertex.Get(false, @"BaseEdge:\To:") == ndi.BaseEdge.Get(false, "To:"))
                          ThereIsDiagramItemOfThisBaseEdgeTo = true;*/

                    if (ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo == false)
                    {
                        if (ThereIsDiagramItemOfThisBaseEdgeTo == false ||
                            GeneralUtil.CompareStrings(r.Get(false, @"Home:\CurrentUser:\Settings:\AllowManyUXItemsWithSameBaseEdgeTo:").Value, "True"))
                        {
                            newUXItem = AddDiagramItem(p,
                                        ndi.UXTemplate,
                                        ndi.BaseEdge);
                        }
                        else
                            UserInteractionUtil.ShowException(Vertex.Value + "UXAggregtor","There is allready UX Item, that visualises dropped vertex.\n\nNow, it is not possible to add second representation of same vertex.\n\nOne can change this limitation by changing \"User\\CurrentUser:\\Settings:\\AllowManyUXItemsWithSameBaseEdgeTo:\" setting."
                                , ExceptionLevelEnum.Warning);
                        
                    }
                    else
                        UserInteractionUtil.ShowException(Vertex.Value + "UXAggregator","There is allready \"" + ndi.UXTemplate.Vertex.Value + "\" UX Item, that visualises dropped vertex.\n\nIt is not possible to add second representation of same vertex, with the same UX Item type."
                            , ExceptionLevelEnum.Warning);
                }
            }

            return newUXItem;
        }

        //

        public virtual bool DoCreateDiagramLine(IUXItem fromItem, IUXItem toItem)
        {
            if (toItem == this)
                return false;

            DoCreateDiagramLine_toUse_count = 0;
            DoCreateDiagramLine_Edge_toUse = null;
            DoCreateDiagramLine_DiagramLineDefinition_toUse = null;

            IEdge toEdge = toItem.BaseEdge;
         

            IVertex v = m0.MinusZero.Instance.CreateTempVertex();

            IVertex fromItemBaseEdgeTo = fromItem.BaseEdge.To;

            foreach (UXDecoratorTemplate tem in fromItem.UXTemplate.UXDecoratorTemplates)
            {
                string tem_EdgeTestQuery = tem.EdgeTestQuery;

                if (tem_EdgeTestQuery != null)
                    foreach (IEdge e in fromItemBaseEdgeTo.GetAll(false, tem_EdgeTestQuery))
                    {
                        bool canAdd = CanAddLineByDecoratorTemplateAndFromItemBaseEdgeToQuery(toItem, toEdge, tem, e);

                        if (canAdd)
                            AddNewLineOption(v, tem, e.To);
                    }

                if (tem.SupportEmptyMetaEdge)
                    AddNewLineOption(v, tem, MinusZero.Instance.Empty);

                if (GeneralUtil.CompareStrings(tem.Vertex.Value, "VERTEX EDGE"))// Vertex\Edge
                    foreach (IEdge e in systemMetaBaseVertex)
                        AddNewLineOption(v, tem, e.To);

                if (tem.EdgeTestQuery == "$EdgeTarget") // $EdgeTarget is not present as there is no inheritance from Vertex                    
                    AddNewLineOption(v, tem, GraphUtil.GetQueryOutFirstEdge(systemMetaBaseVertex, null, "$EdgeTarget").To);
            }

            if (v.Count() == 0)
                UserInteractionUtil.ShowException(Vertex.Value + "Diagram", "There is no diagram line definition matching selected source and target items."
                    , ExceptionLevelEnum.Warning);

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();
            info.Value = "choose diagram line:";


            Point mousePosition = WpfUtil.GetMousePosition();

            IVertex selected = null;

            if (DoCreateDiagramLine_toUse_count > 1)
            {
                DoCreateDiagramLine_DiagramLineDefinition_toUse = null;
                DoCreateDiagramLine_Edge_toUse = null;
            }

            if (DoCreateDiagramLine_Edge_toUse == null)
            {
                m0Main.Instance.PositionForUserInteraction = WpfUtil.GetMousePosition();

                selected = MinusZero.Instance.UserInteraction.InteractionSelect(info, v.OutEdges, true);
            }

            if (selected != null || DoCreateDiagramLine_Edge_toUse != null)
            {
                IEdge DoCreateDiagramLine_DiagramLineDefinition_toUse_Edge = null;

                if (DoCreateDiagramLine_Edge_toUse == null)
                {
                    DoCreateDiagramLine_Edge_toUse = selected.Get(false, "OptionEdge:");
                    DoCreateDiagramLine_DiagramLineDefinition_toUse_Edge = selected.GetAll(false, "OptionDiagramLineDefinition:").FirstOrDefault();
                }
                else
                    DoCreateDiagramLine_DiagramLineDefinition_toUse_Edge = new EasyEdge(null, null, DoCreateDiagramLine_DiagramLineDefinition_toUse);

                IVertex test = VertexOperations.TestIfNewEdgeValid(fromItemBaseEdgeTo, DoCreateDiagramLine_Edge_toUse, toEdge.To);

                if (test == null)
                {
                    //UXDecoratorTemplate chosenTemplate = new UXDecoratorTemplate(a.GetAll(false, "OptionDiagramLineDefinition:").FirstOrDefault());

                    UXDecoratorTemplate chosenTemplate = (UXDecoratorTemplate)TypedEdge.Get(
                        DoCreateDiagramLine_DiagramLineDefinition_toUse_Edge, 
                        typeof(UXDecoratorTemplate));

                    ////////////////////////////////////////
                    Interaction.BeginInteractionWithGraph();
                    ////////////////////////////////////////            

                    IEdge edge = VertexOperations.AddEdgeOrVertexByMeta(fromItemBaseEdgeTo,
                        DoCreateDiagramLine_Edge_toUse,
                        toEdge.To,
                        chosenTemplate.CreateEdgeOnly,
                        chosenTemplate.ForceShowEditForm);

                    AddDiagramLineVertex(fromItem, edge, chosenTemplate, toItem);

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////            

                    return true;
                }
                else
                    UserInteractionUtil.ShowException(Vertex.Value + "Diagram", "Adding new diagram line  \"" + selected.Value + "\" is not possible.\n\n" + test.Value
                        , ExceptionLevelEnum.Warning);
            }

            return false;
        }

        private static bool CanAddLineByDecoratorTemplateAndFromItemBaseEdgeToQuery(IUXItem toItem, IEdge toEdge, UXDecoratorTemplate tem, IEdge e)
        {
            string eMetaValue = e.Meta.Value.ToString();

            if (eMetaValue.Length > 0 && eMetaValue[0] == '$' && eMetaValue != "$Empty") // we do not want to limit
                return false;

            if (tem.ToDiagramItemTestQuery != null && toItem.Vertex.Get(false, tem.ToDiagramItemTestQuery) == null)
                return false;

            string eToEdgeTarget = (string)GraphUtil.GetValue(e.To.Get(false, @"$EdgeTarget:"));
            string eToVertexTarget = (string)GraphUtil.GetValue(e.To.Get(false, @"$VertexTarget:"));

            if (eToEdgeTarget != null
                && eToEdgeTarget != "Vertex" // Vertices do not have $Is:Vertex     
                && !InstructionHelpers.CheckIfIsOrInherits(toEdge.To, eToEdgeTarget))
                return false;

            // 2025.04.17 - we use CreateEdgeOnly for Variables. the code below seems to be not needed now
            /*
            if (!tem.CreateEdgeOnly &&// ZZZ added !
            //if (tem.CreateEdgeOnly && // normally we have CreateEdgeOnly being FALSE, so... we want to activatge this only if CreateEdgeOnly = True
              //above is WRONG for sure
                eToVertexTarget != null
                && !InstructionHelpers.CheckIfIsOrInherits(toEdge.To, eToVertexTarget))
                return false;*/

            return true;
        }

        int DoCreateDiagramLine_toUse_count;
        IVertex DoCreateDiagramLine_Edge_toUse = null;
        IVertex DoCreateDiagramLine_DiagramLineDefinition_toUse = null;

        private void AddNewLineOption(IVertex v, UXDecoratorTemplate def, IVertex edgeVertex)
        {
           if (def.EdgeTestQuery != null && def.EdgeTestQuery != ""
                && def.ToDiagramItemTestQuery != null && def.ToDiagramItemTestQuery != "")
            {
                DoCreateDiagramLine_Edge_toUse = edgeVertex;
                DoCreateDiagramLine_DiagramLineDefinition_toUse = def.Vertex;
                DoCreateDiagramLine_toUse_count++;
            }
            
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex vv = v.AddVertex(null, edgeVertex.Value + " (" + def.Vertex.Value + ")");

            vv.AddEdge(r.Get(false, @"System\Meta\ZeroTypes\UX\OptionEdge"), edgeVertex);
            vv.AddEdge(r.Get(false, @"System\Meta\ZeroTypes\UX\OptionDiagramLineDefinition"), def.Vertex);
        }

        public void AddDiagramLineVertex(IUXItem fromItem, IEdge edge, UXDecoratorTemplate diagramLineDefinition, IUXItem toItem)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////             

            ILineDecoratorBase newLine = (LineDecoratorBase)fromItem.AddDecorator(diagramLineDefinition.DecoratorClass);

            newLine.ToItem = toItem;

            newLine.UXTemplate = diagramLineDefinition;

            newLine.BaseEdgeSet(edge);

            fromItem.AddDiagramLineObject(toItem, newLine, true);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        //

        IList<IUXItem> NewUXItemsList = new List<IUXItem>();

        private void dndDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex r = m0.MinusZero.Instance.Root;

                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                Point p = e.GetPosition(Canvas);                

                bool isSet = false;

                if (dndVertex.Count() > 1)
                    isSet = true;

                if (isSet)
                    User.Process.UX.NonAtomProcess.StartNonAtomProcess();

                NewUXItemsList.Clear();

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                //////////////////////////////////////// 

                foreach (IEdge eee in dndVertex)
                {
                    IUXItem newUXItem = AddDiagramItemDialog(p, eee.To,isSet,e);

                    if (newUXItem != null)
                        NewUXItemsList.Add(newUXItem);

                    p.X += 25;
                    p.Y += 25;
                }

                foreach (IUXItem i in NewUXItemsList)
                    i.ForceVertexChangeOff = true;

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                //////////////////////////////////////// 

                foreach (IUXItem i in NewUXItemsList)
                    i.ForceVertexChangeOff = false;

                NewUXItemsList.Clear();


                CheckAndUpdateDiagramLines();

                UpdateLayout();


                if (isSet)
                    User.Process.UX.NonAtomProcess.StopNonAtomProcess();

                if (e.Data.GetData("DragSource") is IHasSelectableEdges)
                    ((IHasSelectableEdges)e.Data.GetData("DragSource")).UnselectAllSelectedEdges();

                //GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(dndVertex);
                GraphUtil.RemoveAllEdges(dndVertex);
            }
        }

        private IUXItem AddDiagramItem_Base(IUXContainer host, Point p, UXTemplate UXTemplate){
            ITypedEdge _i = host.AddItem(UXTemplate.ItemClass);

            if (!(_i is IUXItem))
                return null;


            IUXItem i = (IUXItem)_i;

            i.PositionCreate();
            i.Position.X = p.X;
            i.Position.Y = p.Y;
            i.UXTemplate = UXTemplate;
             
            if (UXTemplate.ItemVertex != null)
                AddEdgesFromDefintion(i.Vertex, UXTemplate.ItemVertex);

            return i;
        }

        public IUXItem AddDiagramItem(Point p, UXTemplate UXTemplate, IVertex BaseEdge){
            IUXContainer host = GetItemByPoint_ByCanvas(p);

            Point p_translated = new Point(p.X, p.Y);

            if (host == null)
                host = this;
            else
                p_translated = Canvas.TranslatePoint(p, host.Canvas);

            //

            IUXItem i = AddDiagramItem_Base(host, p_translated, UXTemplate);

            IVertex edge = GraphUtil.CreateOrReplaceEdgeByValue(i.Vertex, BaseEdge_meta, "");

            EdgeHelper.AddEdgeVertexEdgesByEdgeVertex(edge, BaseEdge);      

            //

            HostItem(host, i, true);

            return i;
        }        

        public void CheckAndUpdateDiagramLines()
        {        
            foreach(ITypedEdge _i in Items_all)
            {
                IUXItem item = UXItem.GetUXItem(this, _i);

                if (item == null)
                    continue;

                CheckAndUpdateDiagramLinesForItem((IUXItem)item);                
            }
        }

        bool IsContainerEdge(IEdge e)
        {
            Dictionary<IVertex, List<IUXItem>> idbbet = GetItemsDictionaryByBaseEdgeTo();

            if (idbbet.ContainsKey(e.To))
                foreach (IUXItem i in idbbet[e.To])
                    if (e == i.ContainerEdge)
                        return true;

            return false;
        }

        public void CheckAndUpdateDiagramLinesForItem(IUXItem item)
        {
            if (item == this) // currently support for Visualiser lines is limited
                return;

            IEnumerable<IEdge> edges;

            IVertex item_BaseEdgeTo = item.BaseEdgeTo;

            if (item.UXTemplate.DoNotShowInherited)
                edges = item_BaseEdgeTo.OutEdgesRaw;
            else
                edges = item_BaseEdgeTo;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            foreach (IEdge e in item_BaseEdgeTo)
            {
                if (IsContainerEdge(e))
                    continue;

                bool needAdding = true;

                if (item.GetDiagramLinesBaseEdgeToDictionary().ContainsKey(e.To))
                foreach (ILineDecoratorBase l in item.GetDiagramLinesBaseEdgeToDictionary()[e.To])
                    {
                        IVertex l_BaseEdge = GraphUtil.GetQueryOutFirst(l.Vertex, "BaseEdge", null);

                        if (l_BaseEdge != null &&
                            GraphUtil.GetQueryOutFirst(l_BaseEdge, "Meta", null) == e.Meta)
                            needAdding = false;
                    }

                if (needAdding && CanAddLine(item,e))
                {
                    List<IUXItem> toDiagramItems = GetItemsByBaseEdgeTo_ForLines(e);

                    TryAddDiagramLineVertexForListOfItems(item, e, toDiagramItems, false);

                    List<IUXItem> toDiagramItems_EdgeTargetInEdgePointingToTargetItemBaseEdgeTo = GetItemsByBaseEdgeTo_ForLines_EdgeTargetInEdgePointingToTargetItemBaseEdgeTo(e);

                    TryAddDiagramLineVertexForListOfItems(item, e, toDiagramItems_EdgeTargetInEdgePointingToTargetItemBaseEdgeTo, true);
                }
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        private bool CanAddLine(IUXItem item, IEdge e)
        {
            if (item is IUXMultiContainerSubItem)
            {
                IUXMultiContainerItem parentItem = (IUXMultiContainerItem)item.ParentItem;

                foreach (IUXDecorator dec in parentItem.Decorators)
                    if (EdgeHelper.CompareIEdges(dec.BaseEdge, e))
                        return false;
            }

            return true;
        }

        private void TryAddDiagramLineVertexForListOfItems(IUXItem item, IEdge e, List<IUXItem> toDiagramItems, bool isEdgeTargetInEdgePointingToTargetItemBaseEdgeTo)
        {
            foreach (IUXItem toDiagramItem in toDiagramItems)
            {
                if (item is IUXMultiContainerItem
                    && toDiagramItem is IUXMultiContainerSubItem
                    && toDiagramItem.ParentItem == item)
                    continue;

                UXDecoratorTemplate lineDef = GetLineDefinition(e, item, toDiagramItem);

                if (lineDef != null && 
                    !(toDiagramItem is IUXMultiContainerSubItem) &&
                    (isEdgeTargetInEdgePointingToTargetItemBaseEdgeTo == false ||
                    lineDef.EdgeTargetInEdgePointingToTargetItemBaseEdgeTo
                    ))                
                    AddDiagramLineVertex(item, e, lineDef, toDiagramItem);
            }
        }

        protected List<IUXItem> GetItemsByBaseEdgeTo_ForLines(IEdge toEdge)
        {
            List<IUXItem> r = new List<IUXItem>();

            if (GetItemsDictionaryByBaseEdgeTo().ContainsKey(toEdge.To))
                foreach (IUXItem i in GetItemsDictionaryByBaseEdgeTo()[toEdge.To])
                    r.Add(i);
            return r;
        }

        protected List<IUXItem> GetItemsByBaseEdgeTo_ForLines_EdgeTargetInEdgePointingToTargetItemBaseEdgeTo(IEdge toEdge)
        // in order Associations to work 
        {
            List<IUXItem> r = new List<IUXItem>();

            IVertex toEdgeToEdgeTarget = GraphUtil.GetQueryOutFirst(toEdge.To, "$EdgeTarget", null); 

            if (toEdgeToEdgeTarget != null && GraphUtil.ExistQueryOut(toEdge.Meta, "$VertexTarget", null)) // toEdgeToEdgeTarget is instance of GraphUtil.GetQueryOut(toEdge.Meta, "$VertexTarget", null)  ??
                if (GetItemsDictionaryByBaseEdgeTo().ContainsKey(toEdgeToEdgeTarget))
                    foreach (IUXItem i in GetItemsDictionaryByBaseEdgeTo()[toEdgeToEdgeTarget])
                        r.Add(i);
            
            return r;
        }

        public UXDecoratorTemplate GetLineDefinition(IEdge e, IUXItem item, IUXItem toItem){
            // Vertex / Edge handling << that was replaced by if(edgeTestQuery != null && edgeTestQuery != ""){ below
            //if (GraphUtil.GetValueAndCompareStrings(item.UXTemplate.Vertex, "Vertex"))
            //  return new UXDecoratorTemplate(item.Vertex.GetAll(false, @"UXTemplate:\UXDecoratorTemplate:Edge").FirstOrDefault());

            UXDecoratorTemplate tem_found_EmptyMetaEdge = null;
            UXDecoratorTemplate tem_found_AnyMetaEdge = null;
            UXDecoratorTemplate tem_found_EdgeTestQueries = null;

            foreach (UXDecoratorTemplate tem in item.UXTemplate.UXDecoratorTemplates)            
            {
                string edgeTestQuery = tem.EdgeTestQuery;

                bool canReturn = true;

                if (tem.SupportEmptyMetaEdge)
                {
                    if (e.Meta.Value.ToString() == "$Empty")
                        tem_found_EmptyMetaEdge = tem;

                    canReturn = false;
                }

                if (tem.SupportAnyMetaEdge)
                    tem_found_AnyMetaEdge = tem;

                

                if (edgeTestQuery != null && edgeTestQuery != ""){
                    canReturn = false;

                    if (edgeTestQuery == "$EdgeTarget" && e.Meta.Value.ToString() == "$EdgeTarget")
                        canReturn = true;

                    foreach (IEdge toTest in item.BaseEdgeTo.GetAll(false, edgeTestQuery))
                        if (toTest.To == e.Meta)
                            canReturn = true;
                }

                if (canReturn)
                {
                    string toDiagramItemTestQuery = tem.ToDiagramItemTestQuery;

                    if (toDiagramItemTestQuery != null
                        && toItem.Vertex.Get(false, toDiagramItemTestQuery) != null)
                    {
                        if (edgeTestQuery != null && edgeTestQuery != "")
                            tem_found_EdgeTestQueries = tem;
                        else
                            if (tem_found_EmptyMetaEdge == null)
                                tem_found_EmptyMetaEdge = tem;
                    }                    
                }
            }

            if (tem_found_EdgeTestQueries != null)
                return tem_found_EdgeTestQueries;

            if (tem_found_EmptyMetaEdge != null)
                return tem_found_EmptyMetaEdge;

            return tem_found_AnyMetaEdge; // can be null and that is ok
        }

        // UNDERPINNINGS

        // UXContainer

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");
        static IVertex ContainerEdgeMetaVertex_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ContainerEdgeMetaVertex");
        static IVertex SubItemsNotVisible_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\SubItemsNotVisible");
        static IVertex NewItemUXTemplate_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\NewItemUXTemplate");

        static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");

        public bool IsExpanded
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "IsExpanded", null);

                if (val == null)
                    val = Vertex.AddVertex(IsExpanded_meta, value);
                else
                    val.Value = value;
            }
        }

        public ZeroTypes.UX.Size ExpandedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ExpandedSize", null);

                if (val == null)
                    return null;

                return (ZeroTypes.UX.Size)TypedEdge.Get(val, typeof(ZeroTypes.UX.Size));
            }
        }

        public ZeroTypes.UX.Size ExpandedSizeCreate()
        {
            return new ZeroTypes.UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, ExpandedSize_meta));
        }

        public ZeroTypes.UX.Size CollapsedSize
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "CollapsedSize", null);

                if (val == null)
                    return null;

                return (ZeroTypes.UX.Size)TypedEdge.Get(val, typeof(ZeroTypes.UX.Size));
            }
        }

        public ZeroTypes.UX.Size CollapsedSizeCreate()
        {
            return new ZeroTypes.UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, CollapsedSize_meta));
        }

        public bool SubItemsNotVisible
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "SubItemsNotVisible", null);

                if (val == null)
                    return false;

                return GraphUtil.GetBooleanValueOrFalse(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "SubItemsNotVisible", null);

                if (val == null)
                    val = Vertex.AddVertex(SubItemsNotVisible_meta, value);
                else
                    val.Value = value;
            }
        }

        public ZeroTypes.UX.UXTemplate NewItemUXTemplate
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "NewItemUXTemplate", null);

                if (val == null)
                    return null;

                ITypedEdge _i = TypedEdge.Get(val);

                if (_i != null && _i is UXTemplate)
                    return (UXTemplate)_i;

                return null;
            }
            set
            {
                GraphUtil.CreateOrReplaceEdge(Vertex, NewItemUXTemplate_meta, value.Vertex);
            }
        }

        //

        public IUXVisualiser OwningVisualiser { get; set; } // ParentAggregator

        public bool IsSelected { get; set; }

        public bool IsHighlighted { get; set; }

        public List<ILineDecoratorBase> DiagramLines { get; } = new List<ILineDecoratorBase>();

        public List<ILineDecoratorBase> DiagramToLines { get; } = new List<ILineDecoratorBase>();

        public List<ILineDecoratorBase> DiagramToAsMetaLines { get; } = new List<ILineDecoratorBase>();

        public virtual void VertexSetedUp() { }

        public Dictionary<IUXItem, List<ILineDecoratorBase>> GetDiagramLinesToDiagramItemDictionary() { 
            return new Dictionary<IUXItem, List<ILineDecoratorBase>>(); 
        }

        public Dictionary<IVertex, List<ILineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary() {
            return new Dictionary<IVertex, List<ILineDecoratorBase>>();
        }

        public virtual void RemoveFromCanvas() { }

        public void AddDiagramLineObject(IUXItem toItem, ILineDecoratorBase lineDecorator, bool AddDecoratorVertex) { }

        public void RemoveDiagramLine(ILineDecoratorBase line) { }

        public virtual void Select() { }

        public virtual void Unselect() { }

        public virtual void Highlight() { }

        public virtual void Unhighlight() { }

        public void MoveItem(double x, double y, bool onlyAnchors) { }

        public void MoveAndResizeItem(double left, double top, double width, double height) { }

        public void AddToSelectedEdges() { }

        public Point GetLineAnchorLocation(IUXItem toItem, bool useToPoint, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart) { return new Point(); }

        public void UpdateDiagramLines() { }        

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
        //static IVertex Size_type = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Size");
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

        public ZeroTypes.UX.Size Size
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "Size", null);

                if (val == null)
                    return null;

                return (ZeroTypes.UX.Size)TypedEdge.Get(val, typeof(ZeroTypes.UX.Size));
            }
        }

        public ZeroTypes.UX.Size SizeCreate()
        {
            return new ZeroTypes.UX.Size(VertexOperations.AddInstanceAndReturnEdge(Vertex, Size_type, Size_meta));
        }

        public ZeroTypes.UX.Position Position
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "Position", null);

                if (val == null)
                    return null;

                return (ZeroTypes.UX.Position)TypedEdge.Get(val, typeof(ZeroTypes.UX.Position));
            }
        }

        public ZeroTypes.UX.Position PositionCreate()
        {
            return new ZeroTypes.UX.Position(VertexOperations.AddInstanceAndReturnEdge(Vertex, Position_type, Position_meta));
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

        public ZeroTypes.UX.Color BackgroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BackgroundColor", null);

                if (val == null)
                    return null;

                return (ZeroTypes.UX.Color)TypedEdge.Get(val, typeof(ZeroTypes.UX.Color));
            }
        }

        public ZeroTypes.UX.Color BackgroundColorCreate()
        {
            return new ZeroTypes.UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, BackgroundColor_meta));
        }

        public ZeroTypes.UX.Color ForegroundColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "ForegroundColor", null);

                if (val == null)
                    return null;

                return (ZeroTypes.UX.Color)TypedEdge.Get(val, typeof(ZeroTypes.UX.Color));
            }
        }

        public ZeroTypes.UX.Color ForegroundColorCreate()
        {
            return new ZeroTypes.UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, ForegroundColor_meta));
        }

        public ZeroTypes.UX.Color BorderColor
        {
            get
            {
                IEdge val = GraphUtil.GetQueryOutFirstEdge(Vertex, "BorderColor", null);

                if (val == null)
                    return null;

                return (ZeroTypes.UX.Color)TypedEdge.Get(val, typeof(ZeroTypes.UX.Color));
            }
        }

        public ZeroTypes.UX.Color BorderColorCreate()
        {
            return new ZeroTypes.UX.Color(VertexOperations.AddInstanceAndReturnEdge(Vertex, Color_type, BorderColor_meta));
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

        public ZeroTypes.UX.UXTemplate UXTemplate
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
                {
                    ITypedEdge i = TypedEdge.Get(e);                    

                    if(i != null && i is IUXItem)
                        ret.Add((IUXItem)i);
                }

                return ret;
            }
        }

        public IUXItem AddDecorator(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Decorator_meta);            

            return (IUXItem)TypedEdge.Get(newEdge);
        }
        public void RemoveDecorator(IUXItem decorator)
        {
            Vertex.DeleteEdge(decorator.Edge);
        }

        // Item

        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");
        static IVertex VolatileItem_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\VolatileItem");
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

        public void BaseEdgeSet(IEdge value)
        {
            Edge baseEdge = BaseEdge;

            if (value.From != null)
                baseEdge.From = value.From;

            if (value.Meta != null)
                baseEdge.Meta = value.Meta;

            if (value.To != null)
                baseEdge.To = value.To;
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

        public IList<ITypedEdge> Items
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<ITypedEdge> ret = new List<ITypedEdge>();

                foreach (IEdge e in list)
                {
                    ITypedEdge item = TypedEdge.Get(e);

                    if (item != null)
                    {
                        if (item is IItem)
                            ((IItem)item).ParentItem = this;

                        ret.Add(item);
                    }
                }

                return ret;
            }
        }

        public ITypedEdge AddItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Item_meta);

            ITypedEdge item = TypedEdge.Get(newEdge);

            if (item != null)
            {
                if (item is IItem)
                    ((IItem)item).ParentItem = this;

                if (item is IUXItem)
                    ((IUXItem)item).NestingLevel = NestingLevel + 1;

                return item;
            }

            return null;
        }

        public IList<ITypedEdge> VolatileItems
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "VolatileItem", null);

                IList<ITypedEdge> ret = new List<ITypedEdge>();

                foreach (IEdge e in list)
                {
                    ITypedEdge item = TypedEdge.Get(e);

                    if (item != null)
                    {
                        if (item is IItem)
                            ((IItem)item).ParentItem = this;

                        ret.Add(item);
                    }
                }

                return ret;
            }
        }

        public ITypedEdge AddVolatileItem(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, VolatileItem_meta);

            ITypedEdge item = TypedEdge.Get(newEdge);

            if (item != null)
            {
                if (item is IItem)
                    ((IItem)item).ParentItem = this;

                if (item is IUXItem)
                    ((IUXItem)item).NestingLevel = NestingLevel + 1;

                return item;
            }

            return null;
        }


        public void MoveExistingItemAsThisItemsSubItem(IItem item)
        {
            item.Edge.From.DeleteEdge(item.Edge);

            IEdge e = Vertex.AddEdge(Item_meta, item.Vertex);
            item.Edge = e;

            item.ParentItem = this;

            if (item is IUXItem)
                ((IUXItem)item).NestingLevel = NestingLevel + 1;
        }

        public void RemoveItem(IItem item)
        {
            Vertex.DeleteEdge(item.Edge);
        }        

        // TypedEdge        
        public IEdge Edge { get; set; }
        public bool SuspendSetFocus { get; set; }
    }
}