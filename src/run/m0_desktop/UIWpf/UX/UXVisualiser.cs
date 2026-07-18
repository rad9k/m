using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers;
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
        public bool ForceVertexChangeOff
        {
            get { return VisualiserHelper.ForceVertexChangeOff; }
            set { VisualiserHelper.ForceVertexChangeOff = value; }
        }

        public event Notify SelectedEdgesChange;

        public bool SelectionProhibited { get; set; }

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
            IVertex baseEdgeTo = baseEdgeVertex == null ? null : baseEdgeVertex.Get(false, "To:");
            IVisualiser alreadyOpened = VisualisersList.GetVisualiser(baseEdgeTo);

            if (alreadyOpened != null)
            {
                MinusZero.Instance.Log(1, "UXVisualiser.DisposeNesting",
                    "ctor blocked alreadyOpened type=" + alreadyOpened.GetType().Name
                    + " baseEdgeTo=" + DescribeVertexForDisposeNestingLog(baseEdgeTo)
                    + " parentVisualiser=" + DescribeVertexForDisposeNestingLog(parentVisualiser)
                    + " isVolatile=" + isVolatile
                    + " alreadyOpenedIsDisposed=" + DescribeDisposedFlag(alreadyOpened));

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
                new List<string> { @"" },
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst,
                true);

            MinusZero.Instance.Log(1, "UXVisualiser.DisposeNesting",
                "ctor created baseEdgeTo=" + DescribeVertexForDisposeNestingLog(baseEdgeTo)
                + " registeredVertex=" + DescribeVertexForDisposeNestingLog(Vertex)
                + " parentVisualiser=" + DescribeVertexForDisposeNestingLog(parentVisualiser)
                + " isVolatile=" + isVolatile
                + " note=AddVertexFalse_noItemEdgeOnParent");

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

            try
            {

                sv.ScrollToHorizontalOffset((scrollBarPosAbstract_horizontal * Canvas.ActualWidth * scale) - half_horizontal);
                sv.ScrollToVerticalOffset((scrollBarPosAbstract_vertical * Canvas.ActualHeight * scale) - half_vertical);
            }
            catch (Exception e) { }

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

            foreach (IUXItem ui in Items_all)
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

            foreach (IUXItem i in Items_all)
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

            //foreach (IEdge e in definitionEdges.OutEdgesRaw)
            foreach (IEdge e in definitionEdges.OutEdges)
                if (e.Meta.Value.ToString()[0]!='$' && (!(GraphUtil.ExistQueryOut(e.Meta, "$NoCopy", null) || GraphUtil.ExistQueryOut(e.To, "$NoCopy", null))))
                {
                    if (!VertexOperations.IsLink(e) && VertexOperations.IsAtomicVertex(e.To))
                        GraphUtil.SetVertexValue(baseVertex, e.Meta, e.To.Value); // shallow copy
                    else

                        GraphUtil.CreateOrReplaceEdge_DeepCopy(baseVertex, e.Meta, e.To);

                    /*if (VertexOperations.IsAtomicVertex(e.To))
                        GraphUtil.SetVertexValue(baseVertex, e.Meta, e.To.Value); // shallow copy
                    else
                    {
                        if (e.Meta.Value.ToString() == "$Inherits")
                            baseVertex.AddEdge(e.Meta, e.To);
                        else        
                            GraphUtil.CreateOrReplaceEdge_DeepCopy(baseVertex, e.Meta, e.To);
                    }*/
                }
        }

        public void HostItem(IUXContainer host, IUXItem item, bool newItemCreation)
        {
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

            if (itemParentItem.UXTemplate != null)
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
                foreach (IUXItem i in itemParentItem.Decorators)
                    if (i is LineDecoratorBase)
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

            foreach (ITypedEdge _item in Items_all)
            {
                IUXItem item = UXItem.GetUXItem(this, _item);

                if (item == null)
                    continue;

                metatopairs.Clear();

                foreach (IUXItem decorator in item.Decorators) // calculate LineDecorator number and Edges number for each Meta/To edge pair
                                                               //foreach (IEdge l in item.Vertex.GetAll(false, "DiagramLine:")) // calculate DiagramLines number and Edges number for each Meta/To edge pair
                {
                    if (decorator is ILineDecoratorBase)
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

                        foreach (IEdge e in GetEdgesForDiagramLineDecorators(item))
                            if (newpair.Meta == e.Meta && newpair.To == e.To)
                                newpair.NumberOfEdgesWithSameMetaTo++;

                        metatopairs.Add(newpair);

                    }
                    else
                        found.NumberOfDecoratorsWithSameMetaTo++;
                }

                foreach (MetaToPair pair in metatopairs)
                { // delete DiagramLines for edges that been deleted
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

                if (dict.ContainsKey(toFind))
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

            foreach (ITypedEdge _i in Items_all)
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

                Width = Size.Width;
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

        List<Rectangle> MovingSprites = new List<Rectangle>();

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
                        r.Stroke = (Brush)FindResource("0ForegroundBrush");
                        r.StrokeDashArray = new DoubleCollection(new double[] { 1, 4 });
                        r.Tag = item;
                        Panel.SetZIndex(r, 99999);

                        Canvas.Children.Add(r);
                        MovingSprites.Add(r);
                    }
            }
            else
            {
                foreach (Rectangle r in MovingSprites)
                {
                    IUXItem i = (IUXItem)r.Tag;

                    Canvas.SetLeft(r, i.Position.X + x);
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

                    if ((selectedEdgesCount > 0 && ClickedItem.IsSelected == false) ||
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
            }
            else
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
                    if (_line is ILineDecoratorBase)
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
            }
            else
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
                if (CanAddLineByDecoratorTemplateAndFromItemBaseEdgeToQuery(toItem, toItem.BaseEdge, tem, e)) ;
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

            foreach (IEdge e in GetEdgesForDiagramLineDecorators(fromItem))
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

                foreach (IUXItem d in i.Decorators)
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

                MinusZero.Instance.Log(1, "UXVisualiser.DisposeNesting",
                    "Dispose begin IsVisualiser=" + IsVisualiser
                    + " vertex=" + DescribeVertexForDisposeNestingLog(Vertex));

                if (IsVisualiser)
                {
                    VisualisersList.RemoveVisualiser(this);

                    GraphChangeTrigger.RemoveListener(VisualiserHelper.graphChangeListenerEdge);

                    DisposeAllItems();

                    SaveDiagram();

                    MinusZero.Instance.Log(1, "UXVisualiser.DisposeNesting",
                        "Dispose end removedFromVisualisersList vertex=" + DescribeVertexForDisposeNestingLog(Vertex));
                }
                else
                {
                    TypedEdge.RemoveFromDictionary(this);

                    MinusZero.Instance.Log(1, "UXVisualiser.DisposeNesting",
                        "Dispose end TypedEdge path vertex=" + DescribeVertexForDisposeNestingLog(Vertex));
                }
            }
            else
            {
                MinusZero.Instance.Log(1, "UXVisualiser.DisposeNesting",
                    "Dispose skipped alreadyDisposed vertex=" + DescribeVertexForDisposeNestingLog(Vertex));
            }
        }

        private static string DescribeVertexForDisposeNestingLog(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            return "val=" + (vertex.Value == null ? "null" : vertex.Value.ToString())
                + " hash=" + vertex.GetHashCode();
        }

        private static string DescribeDisposedFlag(IVisualiser visualiser)
        {
            UXVisualiser uxVisualiser = visualiser as UXVisualiser;

            if (uxVisualiser != null)
                return uxVisualiser.IsDisposed.ToString();

            return "unknown";
        }

        private void SaveDiagram()
        {
            string path = GraphUtil.GetQueryBetweenVertexes_byInEdges(this.Vertex, MinusZero.Instance.Root);
            string pathEncoded = Lib.StdView.Html.DiagramQueryToDiagramId_internal(path);
            string startFullFilename = GraphUtil.GetStringValue(MinusZero.Instance.Root.Get(false, @"Start:\FullFilename:"));
            string diagramsDirectoryPath = System.IO.Path.Combine(startFullFilename, "diagrams");
            string diagramFilePath = System.IO.Path.Combine(diagramsDirectoryPath, pathEncoded + ".png");

            try
            {
                System.IO.Directory.CreateDirectory(diagramsDirectoryPath);
                CanvasToPng.SaveCanvasToPng(Canvas, diagramFilePath);
            }
            catch (Exception ex)
            {
                MinusZero.Instance.Log(1, "UXVisualiser.SaveDiagram",
                    "Failed to save diagram to " + diagramFilePath + ". " + ex);
            }
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

                    if (GetItemsDictionaryByBaseEdgeTo().ContainsKey(ndi.BaseEdge.Get(false, "To:")))
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
                            UserInteractionUtil.ShowException(Vertex.Value + "UXAggregtor", "There is allready UX Item, that visualises dropped vertex.\n\nNow, it is not possible to add second representation of same vertex.\n\nOne can change this limitation by changing \"User\\CurrentUser:\\Settings:\\AllowManyUXItemsWithSameBaseEdgeTo:\" setting."
                                , ExceptionLevelEnum.Warning);

                    }
                    else
                        UserInteractionUtil.ShowException(Vertex.Value + "UXAggregator", "There is allready \"" + ndi.UXTemplate.Vertex.Value + "\" UX Item, that visualises dropped vertex.\n\nIt is not possible to add second representation of same vertex, with the same UX Item type."
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

                if (tem.EdgeTestQuery == "$Inherits"
                    && (tem.ToDiagramItemTestQuery == null
                        || toItem.Vertex.Get(false, tem.ToDiagramItemTestQuery) != null))
                    AddNewLineOption(v, tem, MinusZero.Instance.Inherits);
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
                && !VertexOperations.CheckIfIsOrInherits(toEdge.To, eToEdgeTarget))
                return false;

            // 2025.04.17 - we use CreateEdgeOnly for Variables. the code below seems to be not needed now
            /*
            if (!tem.CreateEdgeOnly &&// ZZZ added !
            //if (tem.CreateEdgeOnly && // normally we have CreateEdgeOnly being FALSE, so... we want to activatge this only if CreateEdgeOnly = True
              //above is WRONG for sure
                eToVertexTarget != null
                && !VertexOperations.CheckIfIsOrInherits(toEdge.To, eToVertexTarget))
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
                    IUXItem newUXItem = AddDiagramItemDialog(p, eee.To, isSet, e);

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

        private IUXItem AddDiagramItem_Base(IUXContainer host, Point p, UXTemplate UXTemplate)
        {
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

        public IUXItem AddDiagramItem(Point p, UXTemplate UXTemplate, IVertex BaseEdge)
        {
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
            foreach (ITypedEdge _i in Items_all)
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

        private static IEnumerable<IEdge> GetEdgesForDiagramLineDecorators(IUXItem item)
        {
            if (item == null || item.BaseEdgeTo == null)
                return Enumerable.Empty<IEdge>();

            if (item.UXTemplate != null && item.UXTemplate.DoNotShowInherited)
                return item.BaseEdgeTo.OutEdgesRaw;

            return item.BaseEdgeTo.OutEdges;
        }

        private void RemoveDiagramLineDecoratorsWithoutMatchingEdges(IUXItem item, IEnumerable<IEdge> edges)
        {
            List<IEdge> unmatchedEdges = edges.Where(e => !IsContainerEdge(e)).ToList();

            foreach (IUXItem decorator in item.Decorators)
            {
                if (!(decorator is ILineDecoratorBase))
                    continue;

                ILineDecoratorBase lineDecorator = (ILineDecoratorBase)decorator;
                Edge decoratorBaseEdge = lineDecorator.BaseEdge;

                if (decoratorBaseEdge == null)
                    continue;

                IEdge matchingEdge = unmatchedEdges.FirstOrDefault(e =>
                    e.Meta == decoratorBaseEdge.Meta && e.To == decoratorBaseEdge.To);

                if (matchingEdge != null)
                    unmatchedEdges.Remove(matchingEdge);
                else
                    item.RemoveDiagramLine(lineDecorator);
            }
        }

        public void CheckAndUpdateDiagramLinesForItem(IUXItem item)
        {
            if (item == this) // currently support for Visualiser lines is limited
                return;

            List<IEdge> edges = GetEdgesForDiagramLineDecorators(item).ToList();

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            RemoveDiagramLineDecoratorsWithoutMatchingEdges(item, edges);

            foreach (IEdge e in edges)
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

                if (needAdding && CanAddLine(item, e))
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

        public UXDecoratorTemplate GetLineDefinition(IEdge e, IUXItem item, IUXItem toItem)
        {
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



                if (edgeTestQuery != null && edgeTestQuery != "")
                {
                    canReturn = false;

                    if (edgeTestQuery == "$EdgeTarget" && e.Meta.Value.ToString() == "$EdgeTarget")
                        canReturn = true;

                    if (edgeTestQuery == "$Inherits" && e.Meta == MinusZero.Instance.Inherits)
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

        public Dictionary<IUXItem, List<ILineDecoratorBase>> GetDiagramLinesToDiagramItemDictionary()
        {
            return new Dictionary<IUXItem, List<ILineDecoratorBase>>();
        }

        public Dictionary<IVertex, List<ILineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary()
        {
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

        // Visualiser is not a renderable shape entity; return fromPoint as a
        // safe no-op so any accidental call doesn't produce a (0,0) artifact.
        public Point GetLineEdgeIntersection(Point fromPoint, Vector direction) { return fromPoint; }

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

                    if (i != null && i is IUXItem)
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

        // REPOSITION

        public static INoInEdgeInOutVertexVertex Reposition(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex AlgorithmVertex = GraphUtil.GetQueryOutFirst(stack, "Algorithm", null);

            RepositionAlgorithmEnum Reposition = RepositionAlgorithmEnumHelper.GetEnum(AlgorithmVertex);

            IVertex visualiserVertex = GraphUtil.GetQueryOutFirst(stack, "this", null);

            UXVisualiser visualiser = (UXVisualiser)VisualisersList.GetVisualiser(visualiserVertex);

            if (visualiser == null)
            {
                UserInteractionUtil.ShowException("UXVisualiser", "UXVisualiser instance not found for baseVertex", ExceptionLevelEnum.Error);

                return stack;
            }

            visualiser.Dispatcher.Invoke(() => visualiser.RepositionGraph(Reposition));

            return stack;
        }

        public void RepositionGraph(RepositionAlgorithmEnum algorithm)
        {
            List<IUXItem> items = GetTopLevelRepositionItems();
            if (items.Count == 0) return;

            foreach (IUXItem i in items)
                if (i is UIElement ue) ue.UpdateLayout();

            MinusZero.Instance.Log(1, "UXVisualiser.RepositionGraph", algorithm.ToString() + " on " + items.Count + " items");

            Interaction.BeginInteractionWithGraph();
            try
            {
                switch (algorithm)
                {
                    case RepositionAlgorithmEnum.Radial:   ApplyRadialLayoutUX(items);      break;
                    case RepositionAlgorithmEnum.Force:    ApplyForceLayoutUX(items);       break;
                    case RepositionAlgorithmEnum.Sugiyama: ApplySugiyamaLayoutUX(items);    break;
                    case RepositionAlgorithmEnum.Kamada:   ApplyKamadaKawaiLayoutUX(items); break;
                    case RepositionAlgorithmEnum.Tree:     ApplyTreeLayoutUX(items);        break;
                    default:                               ApplyRadialLayoutUX(items);      break;
                }

                // Post-process (6): rectangle overlap removal - applied for every algorithm
                ApplyOverlapRemovalUX(items);

                CommitItemPositions(items);
            }
            finally
            {
                Interaction.EndInteractionWithGraph();
            }

            CheckAndUpdateDiagramLines();
        }

        // HELPERS =============================================================

        private List<IUXItem> GetTopLevelRepositionItems()
        {
            List<IUXItem> result = new List<IUXItem>();
            HashSet<IUXItem> seen = new HashSet<IUXItem>();
            if (Items_all == null) return result;
            foreach (IUXItem i in Items_all)
            {
                if (i == null) continue;
                if (i is IUXDecorator) continue;          // lines
                if (i is ILineDecoratorBase) continue;    // lines (double guard)
                if (!(i.ParentItem is IUXVisualiser)) continue; // only top-level nodes
                if (!seen.Add(i)) continue;
                result.Add(i);
            }
            return result;
        }

        private double GetItemWidthEffective(IUXItem item)
        {
            // Prefer the user-set Size on the vertex, then fall back to WPF rendered
            // size, then a sensible constant so algorithms always have a non-zero box.
            m0.ZeroTypes.UX.Size s = item.Size;
            if (s != null && s.Width > 0) return s.Width;
            if (item is FrameworkElement fe && fe.ActualWidth > 0) return fe.ActualWidth;
            return 100;
        }

        private double GetItemHeightEffective(IUXItem item)
        {
            m0.ZeroTypes.UX.Size s = item.Size;
            if (s != null && s.Height > 0) return s.Height;
            if (item is FrameworkElement fe && fe.ActualHeight > 0) return fe.ActualHeight;
            return 40;
        }

        private Point GetItemCenterEffective(IUXItem item)
        {
            m0.ZeroTypes.UX.Position p = item.Position;
            double x = p != null ? p.X : 0;
            double y = p != null ? p.Y : 0;
            return new Point(x + GetItemWidthEffective(item) / 2, y + GetItemHeightEffective(item) / 2);
        }

        // Computed centers accumulate here so we write to the graph at the very end
        // in one batch - avoids triggering a full re-layout for every single move.
        private Dictionary<IUXItem, Point> _pendingCenters;

        private void ResetPendingCenters(List<IUXItem> items)
        {
            _pendingCenters = new Dictionary<IUXItem, Point>(items.Count);
            foreach (IUXItem i in items) _pendingCenters[i] = GetItemCenterEffective(i);
        }

        private Point GetPendingCenter(IUXItem item)
        {
            if (_pendingCenters != null && _pendingCenters.TryGetValue(item, out Point p)) return p;
            return GetItemCenterEffective(item);
        }

        private void SetPendingCenter(IUXItem item, double cx, double cy)
        {
            if (_pendingCenters == null) _pendingCenters = new Dictionary<IUXItem, Point>();
            _pendingCenters[item] = new Point(cx, cy);
        }

        private void CommitItemPositions(List<IUXItem> items)
        {
            if (_pendingCenters == null) return;
            foreach (IUXItem i in items)
            {
                if (!_pendingCenters.TryGetValue(i, out Point c)) continue;
                double left = c.X - GetItemWidthEffective(i) / 2;
                double top  = c.Y - GetItemHeightEffective(i) / 2;
                i.MoveItem(left, top, false);
            }
            _pendingCenters = null;
        }

        private Dictionary<IUXItem, List<IUXItem>> BuildUndirectedAdjacencyUX(List<IUXItem> items)
        {
            HashSet<IUXItem> set = new HashSet<IUXItem>(items);
            Dictionary<IUXItem, List<IUXItem>> adj = new Dictionary<IUXItem, List<IUXItem>>();
            foreach (IUXItem i in items) adj[i] = new List<IUXItem>();

            // DiagramToLines on UXItem contains incoming lines. Iterating all items and
            // walking their incoming lines covers every edge exactly once.
            foreach (IUXItem i in items)
                foreach (ILineDecoratorBase line in i.DiagramToLines)
                {
                    IUXItem from = line.FromDiagramItem;
                    if (from == null || from == i || !set.Contains(from)) continue;
                    if (!adj[i].Contains(from)) adj[i].Add(from);
                    if (!adj[from].Contains(i)) adj[from].Add(i);
                }
            return adj;
        }

        private void BuildDirectedAdjacencyUX(List<IUXItem> items,
            out Dictionary<IUXItem, List<IUXItem>> outAdj,
            out Dictionary<IUXItem, List<IUXItem>> inAdj)
        {
            HashSet<IUXItem> set = new HashSet<IUXItem>(items);
            outAdj = new Dictionary<IUXItem, List<IUXItem>>();
            inAdj = new Dictionary<IUXItem, List<IUXItem>>();
            foreach (IUXItem i in items)
            {
                outAdj[i] = new List<IUXItem>();
                inAdj[i] = new List<IUXItem>();
            }

            foreach (IUXItem i in items)
                foreach (ILineDecoratorBase line in i.DiagramToLines)
                {
                    IUXItem from = line.FromDiagramItem;
                    if (from == null || from == i || !set.Contains(from)) continue;
                    if (!outAdj[from].Contains(i))
                    {
                        outAdj[from].Add(i);
                        inAdj[i].Add(from);
                    }
                }
        }

        private IUXItem PickRootItem(List<IUXItem> items,
            Dictionary<IUXItem, List<IUXItem>> adj)
        {
            IUXItem best = items[0];
            int bestDeg = -1;
            foreach (IUXItem i in items)
            {
                int deg = adj.ContainsKey(i) ? adj[i].Count : 0;
                if (deg > bestDeg) { bestDeg = deg; best = i; }
            }
            return best;
        }

        private double GetUXCanvasWidth()
        {
            if (this.Canvas != null)
            {
                if (this.Canvas.ActualWidth > 0) return this.Canvas.ActualWidth;
                if (this.Canvas.Width > 0) return this.Canvas.Width;
            }
            if (this.ActualWidth > 0) return this.ActualWidth;
            return 1200;
        }

        private double GetUXCanvasHeight()
        {
            if (this.Canvas != null)
            {
                if (this.Canvas.ActualHeight > 0) return this.Canvas.ActualHeight;
                if (this.Canvas.Height > 0) return this.Canvas.Height;
            }
            if (this.ActualHeight > 0) return this.ActualHeight;
            return 800;
        }

        // 1) RADIAL (improved BFS) ===========================================

        private void ApplyRadialLayoutUX(List<IUXItem> items)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            ResetPendingCenters(items);

            Dictionary<IUXItem, List<IUXItem>> adj = BuildUndirectedAdjacencyUX(items);
            IUXItem root = PickRootItem(items, adj);

            Dictionary<IUXItem, int> level = new Dictionary<IUXItem, int>();
            Queue<IUXItem> q = new Queue<IUXItem>();
            q.Enqueue(root);
            level[root] = 0;
            while (q.Count > 0)
            {
                IUXItem c = q.Dequeue();
                foreach (IUXItem n in adj[c])
                    if (!level.ContainsKey(n)) { level[n] = level[c] + 1; q.Enqueue(n); }
            }
            int maxLevel = level.Count > 0 ? level.Values.Max() : 0;
            foreach (IUXItem i in items)
                if (!level.ContainsKey(i)) level[i] = maxLevel + 1;

            double baseCircleSize = 220;

            double cx = GetUXCanvasWidth() / 2;
            double cy = GetUXCanvasHeight() / 2;

            List<IGrouping<int, KeyValuePair<IUXItem, int>>> byLevel =
                level.GroupBy(kv => kv.Value).OrderBy(g => g.Key).ToList();

            Dictionary<int, double> maxHalfHeight = new Dictionary<int, double>();
            foreach (IGrouping<int, KeyValuePair<IUXItem, int>> g in byLevel)
            {
                double mh = 0;
                foreach (KeyValuePair<IUXItem, int> kv in g)
                {
                    double h = GetItemHeightEffective(kv.Key) / 2;
                    if (h > mh) mh = h;
                }
                maxHalfHeight[g.Key] = mh;
            }

            const double angularPadding = 20;
            const double ringPadding = 25;
            double previousRadius = 0;
            double previousHalfHeight = 0;

            foreach (IGrouping<int, KeyValuePair<IUXItem, int>> g in byLevel)
            {
                int lvl = g.Key;
                List<IUXItem> atLevel = g.Select(kv => kv.Key).ToList();

                if (lvl == 0)
                {
                    foreach (IUXItem i in atLevel) SetPendingCenter(i, cx, cy);
                    previousRadius = 0;
                    previousHalfHeight = maxHalfHeight[lvl];
                    continue;
                }

                double totalWeight = atLevel.Sum(i => GetItemWidthEffective(i) + angularPadding);
                if (totalWeight <= 0) totalWeight = atLevel.Count;

                double radiusFromNodes = totalWeight / (2 * Math.PI);
                double radiusFromPrevious = previousRadius + previousHalfHeight + maxHalfHeight[lvl] + ringPadding;
                double radius = Math.Max(Math.Max(baseCircleSize * lvl, radiusFromNodes), radiusFromPrevious);

                double angleAcc = 0;
                foreach (IUXItem i in atLevel)
                {
                    double weight = GetItemWidthEffective(i) + angularPadding;
                    double share = weight / totalWeight;
                    double mid = angleAcc + share / 2;
                    double a = mid * 2 * Math.PI;
                    double x = cx + Math.Cos(a) * radius;
                    double y = cy + Math.Sin(a) * radius;
                    SetPendingCenter(i, x, y);
                    angleAcc += share;
                }

                previousRadius = radius;
                previousHalfHeight = maxHalfHeight[lvl];
            }

            sw.Stop();
            MinusZero.Instance.Log(1, "UXVisualiser.ApplyRadialLayoutUX",
                "items=" + items.Count + " rings=" + byLevel.Count + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        // 2) FORCE-DIRECTED ===================================================

        private void ApplyForceLayoutUX(List<IUXItem> items)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            ResetPendingCenters(items);

            Dictionary<IUXItem, List<IUXItem>> adj = BuildUndirectedAdjacencyUX(items);

            double width = GetUXCanvasWidth();
            double height = GetUXCanvasHeight();
            int n = items.Count;

            // Ideal edge length derived from average node size rather than from
            // canvas area. With the canvas-based formula k = sqrt(area/n), two
            // connected nodes settle about 'k' pixels apart (repulsion = attraction
            // at dist = k). For a 1000x500 canvas with 25 items that is ~140 px,
            // which spreads the cluster over half the canvas. Sizing k from the
            // node itself gives a compact, readable layout regardless of canvas.
            double avgItemSize = items.Average(i => Math.Max(GetItemWidthEffective(i), GetItemHeightEffective(i)));
            if (avgItemSize <= 0) avgItemSize = 80;
            double k = avgItemSize * 1.3;

            Random rand = new Random(42);
            Dictionary<IUXItem, Point> pos = new Dictionary<IUXItem, Point>();
            foreach (IUXItem i in items)
            {
                Point c = GetPendingCenter(i);
                double px = c.X;
                double py = c.Y;
                if (double.IsNaN(px) || (px == 0 && py == 0))
                {
                    px = rand.NextDouble() * width;
                    py = rand.NextDouble() * height;
                }
                pos[i] = new Point(px, py);
            }

            List<KeyValuePair<IUXItem, IUXItem>> edges = new List<KeyValuePair<IUXItem, IUXItem>>();
            HashSet<string> seen = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                IUXItem a = items[i];
                foreach (IUXItem b in adj[a])
                {
                    int bi = items.IndexOf(b);
                    if (bi < 0) continue;
                    string key = i < bi ? i + ":" + bi : bi + ":" + i;
                    if (seen.Add(key)) edges.Add(new KeyValuePair<IUXItem, IUXItem>(a, b));
                }
            }

            int iterations = 200;
            double temperature = Math.Max(width, height) / 10.0;
            double cooling = Math.Pow(0.02, 1.0 / iterations);

            // Gravity is applied only to nodes that have NO edges at all.
            // The connected cluster organises itself through attractive springs,
            // and pulling it toward centre too just dissolves the nice layout.
            // Isolated nodes, by contrast, have nothing attracting them and would
            // otherwise drift to the canvas borders. Centre of gravity is the
            // cluster's centroid (recomputed each iteration) - this way isolated
            // items follow the cluster wherever it settles, not the canvas centre.
            double isolatedGravityStrength = 0.25;

            // Repulsion cutoff: nodes further than this do not repel each other.
            // Prevents the main cluster from constantly kicking isolated items
            // toward the frame.
            double repulsionCutoff = 3.0 * k;

            HashSet<IUXItem> isolatedItems = new HashSet<IUXItem>();
            foreach (IUXItem i in items)
                if (!adj.ContainsKey(i) || adj[i].Count == 0) isolatedItems.Add(i);

            Dictionary<IUXItem, Vector> disp = new Dictionary<IUXItem, Vector>();

            for (int iter = 0; iter < iterations; iter++)
            {
                foreach (IUXItem i in items) disp[i] = new Vector(0, 0);

                for (int i = 0; i < items.Count; i++)
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        IUXItem v = items[i];
                        IUXItem u = items[j];

                        bool vIsolated = isolatedItems.Contains(v);
                        bool uIsolated = isolatedItems.Contains(u);

                        double dx = pos[v].X - pos[u].X;
                        double dy = pos[v].Y - pos[u].Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist < 0.01) { dx = (rand.NextDouble() - 0.5) * 0.1; dy = (rand.NextDouble() - 0.5) * 0.1; dist = 0.01; }

                        double requiredDx = (GetItemWidthEffective(v) + GetItemWidthEffective(u)) / 2 + 10;
                        double requiredDy = (GetItemHeightEffective(v) + GetItemHeightEffective(u)) / 2 + 10;
                        double overlapX = requiredDx - Math.Abs(dx);
                        double overlapY = requiredDy - Math.Abs(dy);
                        double rectPush = 0;
                        if (overlapX > 0 && overlapY > 0)
                            rectPush = Math.Min(overlapX, overlapY) * 5;

                        // Skip the long-range inverse-square repulsion when either of
                        // the pair is isolated. Isolated nodes must be driven purely
                        // by gravity, otherwise the cluster keeps kicking them toward
                        // the frame and gravity never wins. Short-range rectangle
                        // overlap push is still applied so they do not sit on top of
                        // cluster nodes.
                        double inverseSquare = 0;
                        if (!vIsolated && !uIsolated && dist < repulsionCutoff)
                            inverseSquare = (k * k) / dist;

                        double force = inverseSquare + rectPush;
                        double ux = dx / dist;
                        double uy = dy / dist;
                        disp[v] = new Vector(disp[v].X + ux * force, disp[v].Y + uy * force);
                        disp[u] = new Vector(disp[u].X - ux * force, disp[u].Y - uy * force);
                    }

                foreach (KeyValuePair<IUXItem, IUXItem> e in edges)
                {
                    double dx = pos[e.Key].X - pos[e.Value].X;
                    double dy = pos[e.Key].Y - pos[e.Value].Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < 0.01) dist = 0.01;
                    double force = (dist * dist) / k;
                    double ux = dx / dist;
                    double uy = dy / dist;
                    disp[e.Key]   = new Vector(disp[e.Key].X   - ux * force, disp[e.Key].Y   - uy * force);
                    disp[e.Value] = new Vector(disp[e.Value].X + ux * force, disp[e.Value].Y + uy * force);
                }

                // Compute centroid of the connected (non-isolated) cluster. If there
                // is no connected cluster at all we fall back to the canvas centre.
                double gravityCenterX;
                double gravityCenterY;
                if (isolatedItems.Count < items.Count)
                {
                    double sumX = 0, sumY = 0;
                    int cnt = 0;
                    foreach (IUXItem i in items)
                    {
                        if (isolatedItems.Contains(i)) continue;
                        sumX += pos[i].X;
                        sumY += pos[i].Y;
                        cnt++;
                    }
                    gravityCenterX = sumX / cnt;
                    gravityCenterY = sumY / cnt;
                }
                else
                {
                    gravityCenterX = width / 2;
                    gravityCenterY = height / 2;
                }

                // Gravity applied ONLY to isolated nodes, pulled toward the
                // connected cluster's centroid.
                foreach (IUXItem i in isolatedItems)
                {
                    double gx = gravityCenterX - pos[i].X;
                    double gy = gravityCenterY - pos[i].Y;
                    disp[i] = new Vector(disp[i].X + gx * isolatedGravityStrength,
                                         disp[i].Y + gy * isolatedGravityStrength);
                }

                foreach (IUXItem i in items)
                {
                    Vector d = disp[i];
                    double dlen = Math.Sqrt(d.X * d.X + d.Y * d.Y);
                    if (dlen > 0)
                    {
                        double move = Math.Min(dlen, temperature);
                        pos[i] = new Point(pos[i].X + (d.X / dlen) * move, pos[i].Y + (d.Y / dlen) * move);
                    }
                }

                temperature *= cooling;
            }

            NormalizePositionsUX(items, pos);
            foreach (KeyValuePair<IUXItem, Point> kv in pos) SetPendingCenter(kv.Key, kv.Value.X, kv.Value.Y);

            sw.Stop();
            MinusZero.Instance.Log(1, "UXVisualiser.ApplyForceLayoutUX",
                "items=" + items.Count + " edges=" + edges.Count +
                " isolated=" + isolatedItems.Count +
                " k=" + k.ToString("F1") + " avgItemSize=" + avgItemSize.ToString("F1") +
                " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        // 3) SUGIYAMA (layered) ==============================================

        private void ApplySugiyamaLayoutUX(List<IUXItem> items)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            ResetPendingCenters(items);

            Dictionary<IUXItem, List<IUXItem>> outAdj;
            Dictionary<IUXItem, List<IUXItem>> inAdj;
            BuildDirectedAdjacencyUX(items, out outAdj, out inAdj);

            // Layer assignment via Kahn + cycle breaking.
            Dictionary<IUXItem, int> remainingInDeg = new Dictionary<IUXItem, int>();
            foreach (IUXItem i in items) remainingInDeg[i] = inAdj[i].Count;

            Dictionary<IUXItem, int> layer = new Dictionary<IUXItem, int>();
            Queue<IUXItem> readyQueue = new Queue<IUXItem>();
            HashSet<IUXItem> processed = new HashSet<IUXItem>();

            foreach (IUXItem i in items)
                if (remainingInDeg[i] == 0) { readyQueue.Enqueue(i); layer[i] = 0; }

            int maxAllowedLayer = Math.Max(1, items.Count - 1);
            int cyclesBroken = 0;

            while (processed.Count < items.Count)
            {
                if (readyQueue.Count == 0)
                {
                    IUXItem pick = null;
                    int minDeg = int.MaxValue;
                    foreach (IUXItem i in items)
                    {
                        if (processed.Contains(i)) continue;
                        if (remainingInDeg[i] < minDeg) { minDeg = remainingInDeg[i]; pick = i; }
                    }
                    if (pick == null) break;

                    int maxP = -1;
                    foreach (IUXItem p in inAdj[pick])
                        if (layer.TryGetValue(p, out int lp) && lp > maxP) maxP = lp;
                    int lw = Math.Min(maxP + 1, maxAllowedLayer);
                    layer[pick] = lw;
                    readyQueue.Enqueue(pick);
                    cyclesBroken++;
                }

                IUXItem v = readyQueue.Dequeue();
                if (!processed.Add(v)) continue;

                foreach (IUXItem u in outAdj[v])
                {
                    if (processed.Contains(u)) continue;
                    int currentLayer = layer.ContainsKey(u) ? layer[u] : 0;
                    int candidate = Math.Min(layer[v] + 1, maxAllowedLayer);
                    if (candidate > currentLayer) layer[u] = candidate;
                    else if (!layer.ContainsKey(u)) layer[u] = currentLayer;

                    remainingInDeg[u]--;
                    if (remainingInDeg[u] <= 0) readyQueue.Enqueue(u);
                }
            }

            foreach (IUXItem i in items)
                if (!layer.ContainsKey(i)) layer[i] = 0;

            Dictionary<int, List<IUXItem>> layers = layer
                .GroupBy(kv => kv.Value)
                .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

            int maxLayer = layers.Keys.Max();

            for (int sweep = 0; sweep < 16; sweep++)
            {
                for (int L = 1; L <= maxLayer; L++)
                {
                    if (!layers.ContainsKey(L)) continue;
                    List<IUXItem> prev = layers.ContainsKey(L - 1) ? layers[L - 1] : new List<IUXItem>();
                    layers[L].Sort((a, b) => BarycenterUX(a, inAdj, prev).CompareTo(BarycenterUX(b, inAdj, prev)));
                }
                for (int L = maxLayer - 1; L >= 0; L--)
                {
                    if (!layers.ContainsKey(L)) continue;
                    List<IUXItem> next = layers.ContainsKey(L + 1) ? layers[L + 1] : new List<IUXItem>();
                    layers[L].Sort((a, b) => BarycenterUX(a, outAdj, next).CompareTo(BarycenterUX(b, outAdj, next)));
                }
            }

            double xPadding = 40;
            double canvasH = GetUXCanvasHeight();

            double maxLayerHeight = 0;
            foreach (KeyValuePair<int, List<IUXItem>> kv in layers)
                foreach (IUXItem i in kv.Value)
                {
                    double h = GetItemHeightEffective(i);
                    if (h > maxLayerHeight) maxLayerHeight = h;
                }

            double topMargin = Math.Max(40, maxLayerHeight / 2 + 20);
            double bottomMargin = Math.Max(40, maxLayerHeight / 2 + 20);
            double available = Math.Max(100, canvasH - topMargin - bottomMargin);
            double preferredYPadding = 160;
            double yPadding = maxLayer > 0 ? Math.Min(preferredYPadding, available / maxLayer) : preferredYPadding;
            if (yPadding < maxLayerHeight + 20) yPadding = maxLayerHeight + 20;

            double cx = GetUXCanvasWidth() / 2;
            double startY = topMargin;

            foreach (KeyValuePair<int, List<IUXItem>> kv in layers.OrderBy(k => k.Key))
            {
                int L = kv.Key;
                List<IUXItem> nodes = kv.Value;
                double totalW = nodes.Sum(i => GetItemWidthEffective(i) + xPadding);
                double curX = cx - totalW / 2;
                foreach (IUXItem i in nodes)
                {
                    double w = GetItemWidthEffective(i);
                    double nodeX = curX + (w + xPadding) / 2;
                    double nodeY = startY + L * yPadding;
                    SetPendingCenter(i, nodeX, nodeY);
                    curX += w + xPadding;
                }
            }

            sw.Stop();
            MinusZero.Instance.Log(1, "UXVisualiser.ApplySugiyamaLayoutUX",
                "items=" + items.Count + " maxLayer=" + maxLayer + " cyclesBroken=" + cyclesBroken +
                " yPadding=" + yPadding.ToString("F1") + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        private double BarycenterUX(IUXItem w,
            Dictionary<IUXItem, List<IUXItem>> adj,
            List<IUXItem> referenceLayer)
        {
            if (!adj.ContainsKey(w) || adj[w].Count == 0 || referenceLayer.Count == 0)
                return referenceLayer.IndexOf(w);

            double sum = 0;
            int count = 0;
            foreach (IUXItem n in adj[w])
            {
                int idx = referenceLayer.IndexOf(n);
                if (idx >= 0) { sum += idx; count++; }
            }
            if (count == 0) return 0;
            return sum / count;
        }

        // 4) KAMADA-KAWAI ====================================================

        private void ApplyKamadaKawaiLayoutUX(List<IUXItem> items)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            ResetPendingCenters(items);

            int n = items.Count;
            Dictionary<IUXItem, List<IUXItem>> adj = BuildUndirectedAdjacencyUX(items);

            Dictionary<IUXItem, Dictionary<IUXItem, int>> dist =
                new Dictionary<IUXItem, Dictionary<IUXItem, int>>();
            foreach (IUXItem s in items)
            {
                Dictionary<IUXItem, int> d = new Dictionary<IUXItem, int>();
                Queue<IUXItem> q = new Queue<IUXItem>();
                q.Enqueue(s); d[s] = 0;
                while (q.Count > 0)
                {
                    IUXItem c = q.Dequeue();
                    foreach (IUXItem nn in adj[c])
                        if (!d.ContainsKey(nn)) { d[nn] = d[c] + 1; q.Enqueue(nn); }
                }
                dist[s] = d;
            }

            int diameter = 1;
            foreach (KeyValuePair<IUXItem, Dictionary<IUXItem, int>> kv in dist)
                foreach (int v in kv.Value.Values) if (v > diameter) diameter = v;

            // Ideal edge length (distance 1 in graph space) = a small multiple of
            // the average node size. Previously L was derived from canvas size /
            // diameter, which for small-diameter graphs produced huge gaps
            // (e.g. diameter=2 on a 1500 px canvas -> L ~= 600 px).
            double avgItemSize = items.Average(i => Math.Max(GetItemWidthEffective(i), GetItemHeightEffective(i)));
            if (avgItemSize <= 0) avgItemSize = 80;
            double L = avgItemSize * 1.6;
            double K = 1.0;

            Dictionary<IUXItem, Point> pos = new Dictionary<IUXItem, Point>();
            double cx = GetUXCanvasWidth() / 2;
            double cy = GetUXCanvasHeight() / 2;
            // Seed circle radius chosen so initial neighbor distance on the ring
            // is close to L - the algorithm then only nudges, never has to move
            // nodes hundreds of pixels.
            double R = Math.Max(L, (L * n) / (2 * Math.PI));
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * i / Math.Max(1, n);
                pos[items[i]] = new Point(cx + R * Math.Cos(a), cy + R * Math.Sin(a));
            }

            int iterations = 150;
            for (int iter = 0; iter < iterations; iter++)
            {
                double maxDelta = 0;
                foreach (IUXItem m in items)
                {
                    double dxSum = 0, dySum = 0;
                    foreach (IUXItem i in items)
                    {
                        if (i == m) continue;
                        if (!dist[m].ContainsKey(i)) continue;
                        int dmi = dist[m][i];
                        if (dmi == 0) continue;
                        double lmi = L * dmi;
                        double kmi = K / (dmi * dmi);
                        double dx = pos[m].X - pos[i].X;
                        double dy = pos[m].Y - pos[i].Y;
                        double dd = Math.Sqrt(dx * dx + dy * dy);
                        if (dd < 0.01) dd = 0.01;
                        dxSum += kmi * (dx - lmi * dx / dd);
                        dySum += kmi * (dy - lmi * dy / dd);
                    }
                    double delta = Math.Sqrt(dxSum * dxSum + dySum * dySum);
                    if (delta > 0.01)
                    {
                        double step = Math.Min(delta * 0.1, 20);
                        pos[m] = new Point(pos[m].X - (dxSum / delta) * step,
                                           pos[m].Y - (dySum / delta) * step);
                        if (delta > maxDelta) maxDelta = delta;
                    }
                }
                if (maxDelta < 0.5) break;
            }

            NormalizePositionsUX(items, pos);
            foreach (KeyValuePair<IUXItem, Point> kv in pos) SetPendingCenter(kv.Key, kv.Value.X, kv.Value.Y);

            sw.Stop();
            MinusZero.Instance.Log(1, "UXVisualiser.ApplyKamadaKawaiLayoutUX",
                "items=" + items.Count + " diameter=" + diameter +
                " L=" + L.ToString("F1") + " avgItemSize=" + avgItemSize.ToString("F1") +
                " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        // 5) TREE (Reingold-Tilford style) ==================================

        private void ApplyTreeLayoutUX(List<IUXItem> items)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            ResetPendingCenters(items);

            Dictionary<IUXItem, List<IUXItem>> adj = BuildUndirectedAdjacencyUX(items);
            IUXItem root = PickRootItem(items, adj);

            Dictionary<IUXItem, List<IUXItem>> children = new Dictionary<IUXItem, List<IUXItem>>();
            foreach (IUXItem i in items) children[i] = new List<IUXItem>();

            HashSet<IUXItem> visited = new HashSet<IUXItem> { root };
            Queue<IUXItem> q = new Queue<IUXItem>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                IUXItem c = q.Dequeue();
                foreach (IUXItem nb in adj[c])
                    if (visited.Add(nb)) { children[c].Add(nb); q.Enqueue(nb); }
            }
            foreach (IUXItem i in items)
                if (visited.Add(i)) children[root].Add(i);

            double xGap = 40;
            double yGap = 160;

            Dictionary<IUXItem, double> subtreeWidth = new Dictionary<IUXItem, double>();
            ComputeSubtreeWidthUX(root, children, subtreeWidth, xGap);

            double startX = GetUXCanvasWidth() / 2 - subtreeWidth[root] / 2;
            PlaceTreeNodeUX(root, children, subtreeWidth, startX, 80, yGap);

            sw.Stop();
            MinusZero.Instance.Log(1, "UXVisualiser.ApplyTreeLayoutUX",
                "items=" + items.Count + " rootChildren=" + children[root].Count + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        private double ComputeSubtreeWidthUX(IUXItem w,
            Dictionary<IUXItem, List<IUXItem>> children,
            Dictionary<IUXItem, double> cache,
            double xGap)
        {
            if (cache.ContainsKey(w)) return cache[w];
            double own = GetItemWidthEffective(w) + xGap;
            if (children[w].Count == 0) { cache[w] = own; return own; }
            double sum = 0;
            foreach (IUXItem c in children[w]) sum += ComputeSubtreeWidthUX(c, children, cache, xGap);
            double result = Math.Max(own, sum);
            cache[w] = result;
            return result;
        }

        private void PlaceTreeNodeUX(IUXItem w,
            Dictionary<IUXItem, List<IUXItem>> children,
            Dictionary<IUXItem, double> subtreeWidth,
            double xLeft, double y, double yGap)
        {
            double nodeCx = xLeft + subtreeWidth[w] / 2;
            SetPendingCenter(w, nodeCx, y);

            if (children[w].Count == 0) return;

            double totalChildren = 0;
            foreach (IUXItem c in children[w]) totalChildren += subtreeWidth[c];

            double childX = xLeft + (subtreeWidth[w] - totalChildren) / 2;
            foreach (IUXItem c in children[w])
            {
                PlaceTreeNodeUX(c, children, subtreeWidth, childX, y + yGap, yGap);
                childX += subtreeWidth[c];
            }
        }

        // 6) POST-PROCESS: OVERLAP REMOVAL ==================================

        private void ApplyOverlapRemovalUX(List<IUXItem> items)
        {
            if (items.Count < 2) return;

            const double margin = 8;
            const int maxIter = 40;
            const double damping = 0.5;

            double convergenceThreshold = 0.25 * items.Count;

            int iterationsUsed = 0;
            double totalMovement = 0;

            for (int iter = 0; iter < maxIter; iter++)
            {
                iterationsUsed = iter + 1;
                totalMovement = 0;

                for (int i = 0; i < items.Count; i++)
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        IUXItem a = items[i];
                        IUXItem b = items[j];

                        Point ac = GetPendingCenter(a);
                        Point bc = GetPendingCenter(b);
                        double aw = GetItemWidthEffective(a),  ah = GetItemHeightEffective(a);
                        double bw = GetItemWidthEffective(b),  bh = GetItemHeightEffective(b);

                        double dx = bc.X - ac.X;
                        double dy = bc.Y - ac.Y;
                        double overlapX = (aw + bw) / 2 + margin - Math.Abs(dx);
                        double overlapY = (ah + bh) / 2 + margin - Math.Abs(dy);

                        if (overlapX > 0 && overlapY > 0)
                        {
                            double push;
                            if (overlapX < overlapY)
                            {
                                push = overlapX / 2 * damping;
                                if (dx >= 0) { ac.X -= push; bc.X += push; }
                                else         { ac.X += push; bc.X -= push; }
                            }
                            else
                            {
                                push = overlapY / 2 * damping;
                                if (dy >= 0) { ac.Y -= push; bc.Y += push; }
                                else         { ac.Y += push; bc.Y -= push; }
                            }
                            SetPendingCenter(a, ac.X, ac.Y);
                            SetPendingCenter(b, bc.X, bc.Y);
                            totalMovement += push * 2;
                        }
                    }

                if (totalMovement < convergenceThreshold) break;
            }

            MinusZero.Instance.Log(1, "UXVisualiser.ApplyOverlapRemovalUX",
                "iterations=" + iterationsUsed + " lastTotalMovement=" + totalMovement.ToString("F2") +
                " items=" + items.Count);
        }

        // Normalization shared by Force and Kamada-Kawai =====================

        private void NormalizePositionsUX(List<IUXItem> items, Dictionary<IUXItem, Point> positions)
        {
            if (positions.Count == 0) return;

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            double maxHalfW = 0, maxHalfH = 0;
            foreach (IUXItem i in items)
            {
                Point p = positions[i];
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
                double hw = GetItemWidthEffective(i) / 2;
                double hh = GetItemHeightEffective(i) / 2;
                if (hw > maxHalfW) maxHalfW = hw;
                if (hh > maxHalfH) maxHalfH = hh;
            }

            double margin = 60;
            double canvasW = GetUXCanvasWidth();
            double canvasH = GetUXCanvasHeight();

            double spanX = Math.Max(1, maxX - minX);
            double spanY = Math.Max(1, maxY - minY);
            double availableX = canvasW - 2 * (margin + maxHalfW);
            double availableY = canvasH - 2 * (margin + maxHalfH);

            double scaleX = availableX / spanX;
            double scaleY = availableY / spanY;
            double scale = Math.Min(scaleX, scaleY);
            if (double.IsInfinity(scale) || double.IsNaN(scale) || scale <= 0) scale = 1;
            if (scale > 1) scale = 1; // only shrink

            List<IUXItem> keys = new List<IUXItem>(positions.Keys);
            foreach (IUXItem i in keys)
            {
                Point p = positions[i];
                double nx = margin + maxHalfW + (p.X - minX) * scale;
                double ny = margin + maxHalfH + (p.Y - minY) * scale;
                positions[i] = new Point(nx, ny);
            }
        }
    }
}