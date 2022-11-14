using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using System.Windows;
using m0.Graph;
using m0.Util;
using System.Windows.Media;
using System.Windows.Shapes;
using m0.UIWpf.Controls;
using System.Windows.Input;
using System.Diagnostics;
using m0.UIWpf.Foundation;
using m0.UIWpf.Commands;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Dialog;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.ZeroTypes.UX;

namespace m0.UIWpf.UX
{
    public enum ClickTargetEnum
    {
        MouseUpOrLeave, Selection, Item, AnchorLeftTop, AnchorMiddleTop, AnchorRightTop_CreateDiagramLine, AnchorLeftMiddle, AnchorRightMiddle, AnchorLeftBottom, AnchorMiddleBottom, AnchorRightBottom
    }

    public class MetaToPair
    {
        public IVertex Meta;
        public IVertex To;
        public int LineDecoratorNumber;
        public int EdgesNumber;
    }

    public class UXV : Border, IListVisualiser, IUXAggregator
    {
        bool IsVisualiser = false;

        //

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public Canvas TheCanvas;

        public bool IsSelecting = false;

        public bool IsDrawingLine = false;

        public Line CreatedDiagramLine;

        public FrameworkElement ClickedAnchor;

        public SelectionArea SelectionArea;

        public ClickTargetEnum ClickTarget;

        public IUXItem ClickedItem;

        public IUXItem HighlightedItem;

        public double ClickPositionX_ItemCordinates;
        public double ClickPositionY_ItemCordinates;

        public double ClickPositionX_AnchorCordinates;
        public double ClickPositionY_AnchorCordinates;
        
        public bool IsPaiting = false;

        bool IsFirstPainted = false;

        static string[] _MetaTriggeringUpdateVertex = new string[] { "Width", "Height"};
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public virtual string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public virtual void UpdateView() { }

        public virtual void UpdateVertex() { PaintDiagram(); }

        //

        public UXV(IEdge _edge)
        {
            edge = _edge;

            vertex = _edge.To;

            TypedEdge.vertexDictionary.Add(this.Edge.To, this);

            IsVisualiser = false;
        }

        //

        public UXV(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            if(VisualisersList.GetVisualiser(baseEdgeVertex) != null)
            {
                UserInteractionUtil.ShowError("Diagram Visualiser", "There is allready Diagram Visualiser opened for this Edge");

                return;
            }

            IsVisualiser = true;

            TheCanvas = new Canvas();

            TheCanvas.Background = (Brush)FindResource("0BackgroundBrush");

            this.Child = TheCanvas;

            this.BorderThickness = new Thickness(1);

            this.BorderBrush = (Brush)FindResource("0LightGrayBrush");

            new ListVisualiserHelper(parentVisualiser,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\UXV"),
                 this,
                "UXV",
                this,
                false,
                new List<string> { @"", @"BaseEdge:\To:" },
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

        Dictionary<IVertex, List<IUXItem>> ItemsDictionary = new Dictionary<IVertex, List<IUXItem>>();

        bool needRebuildItemsDictionary = true;

        void AddToItems(IUXItem item)
        {
            Items.Add(item);

            needRebuildItemsDictionary = true;
        }

        void RebuidItemsDictionary()
        {
            ItemsDictionary.Clear();

            foreach(IItem i in Items)
                if(i is IUXItem)
                {
                    IUXItem ui = (UXItem)i;

                    IVertex i_BaseEdgeTo = i.BaseEdgeTo;

                    if (ItemsDictionary.ContainsKey(i_BaseEdgeTo))
                        ItemsDictionary[i_BaseEdgeTo].Add(ui);
                    else
                    {
                        List<IUXItem> list = new List<IUXItem>();
                        list.Add(ui);

                        ItemsDictionary.Add(i_BaseEdgeTo, list);
                    }
                }

            needRebuildItemsDictionary = false;
        }

        public Dictionary<IVertex, List<IUXItem>> GetItemsDictionary()
        {
            if (needRebuildItemsDictionary)
                RebuidItemsDictionary();

            return ItemsDictionary;
        }

        // OPTIMISATION END

        public void RemoveUXItem(IUXItem item)
        {
            RemoveItem(item);

            needRebuildItemsDictionary = true;

            item.RemoveFromCanvas();
        }

        // TOO
        protected List<IUXItem> GetItemsByBaseEdge(IVertex edgeToVertex)
        {
           return GetItemsDictionary()[GraphUtil.GetQueryOutFirst(edgeToVertex, "To", null)];
        }        

        public void AddEdgesFromDefintion(IVertex baseVertex, IVertex definitionEdges)
        {
            foreach (IEdge e in definitionEdges)
            {
                if(VertexOperations.IsAtomicVertex(e.To))
                    GraphUtil.SetVertexValue(baseVertex, e.Meta, e.To.Value); // shallow copy
                else
                    GraphUtil.CreateOrReplaceEdge(baseVertex, e.Meta, e.To); // deep BUT NOT COPY
            }
        }

        public void HostItem(IUXItem item){
            if (!(item is UIElement))
                return;

            UIElement item_UIElement = (UIElement)item;

            item.Diagram = this;                

            item.VertexSetedUp();

            needRebuildItemsDictionary = true;

            AddToItems(item);
                
            Panel.SetZIndex(item_UIElement, 1);
               
            Canvas.SetLeft(item_UIElement, item.Position.X);
            Canvas.SetTop(item_UIElement, item.Position.Y);     

            TheCanvas.Children.Add(item_UIElement);

            item_UIElement.UpdateLayout(); 
        }

        // TOO
        public void AddLineObjects()
        {
            List<MetaToPair> metatopairs = new List<MetaToPair>();

           foreach(IItem _item in Items)
               if(_item is IUXItem)
               {
                   IUXItem item = (IUXItem)_item;

                   metatopairs.Clear();

                   foreach(IUXItem decorator in item.Decorators) // calculate LineDecorator number and Edges number for each Meta/To edge pair
                                                                //foreach (IEdge l in item.Vertex.GetAll(false, "DiagramLine:")) // calculate DiagramLines number and Edges number for each Meta/To edge pair
                   { 
                       MetaToPair found = null;

                       Edge decorator_BaseEdge= decorator.BaseEdge;

                       foreach (MetaToPair pair in metatopairs)
                           if (pair.Meta == decorator_BaseEdge.Meta && pair.To == decorator_BaseEdge.To)
                               found = pair;

                       if (found == null)
                       {
                           MetaToPair newpair = new MetaToPair();
                           newpair.Meta = decorator_BaseEdge.Meta;
                           newpair.To = decorator_BaseEdge.To;
                           newpair.LineDecoratorNumber = 1;
                           newpair.EdgesNumber = 0;

                           foreach (IEdge e in item.BaseEdge.To)
                               if (newpair.Meta == e.Meta && newpair.To == e.To)
                                   newpair.EdgesNumber++;

                           metatopairs.Add(newpair);
                       
                       }else
                           found.LineDecoratorNumber++;
                   }

                   foreach(MetaToPair pair in metatopairs){ // delete DiagramLines for edges that been deleted
                       if(pair.LineDecoratorNumber > pair.EdgesNumber)
                            foreach (IUXItem decorator in item.Decorators)
                            {
                                Edge decorator_BaseEdge = decorator.BaseEdge;

                                if (pair.Meta == decorator_BaseEdge.Meta 
                                    && pair.To == decorator_BaseEdge.To 
                                    && pair.LineDecoratorNumber > pair.EdgesNumber)
                                {
                                   item.Vertex.DeleteEdge(decorator.Edge);
                                   pair.LineDecoratorNumber--;
                                }
                            }
                   }

                    foreach (IUXItem decorator in item.Decorators)
                    // add diagram line objects
                        if(decorator is LineDecorator)
                        {
                            LineDecorator lineDecorator = (LineDecorator)decorator;

                            item.AddDiagramLineObject(GetToDiagramItemFromLineVertex(lineDecorator), lineDecorator);
                        }
               }
           
        }

        // TOO
        public IUXItem GetToDiagramItemFromLineVertex(LineDecorator lineDecorator)
        {
            IVertex toFind = null;

            Edge lineDecorator_BaseEdge = lineDecorator.BaseEdge;

            if (GraphUtil.ExistQueryOut(lineDecorator_BaseEdge.Meta, "$VertexTarget", null)
            && ((UXDecoratorTemplate)lineDecorator.UXTemplate).CreateEdgeOnly)
                toFind = GraphUtil.GetQueryOutFirst(lineDecorator_BaseEdge.To, "$EdgeTarget", null);
            else
                toFind = lineDecorator_BaseEdge.To;

            if (toFind != null)
                foreach (IUXItem i in GetItemsDictionary()[toFind]) {
                    string tdtq = ((UXDecoratorTemplate)lineDecorator.UXTemplate).ToDiagramItemTestQuery;

                    if (!(tdtq != null && i.Vertex.Get(false, tdtq) == null))
                        return i;
                }

            return null;
        }

        // TOO

        void SelectItemsBySelectionArea()
        {
            double left = SelectionArea.Left;
            double top = SelectionArea.Top;
            double right = SelectionArea.Right;
            double bottom = SelectionArea.Bottom;            

            UnselectAllSelectedEdges();

            foreach(IItem _i in Items)
                if(_i is IUXItem && _i is FrameworkElement)
                {
                    IUXItem i = (IUXItem)_i;

                    FrameworkElement i_FrameworkElement = (FrameworkElement)i;

                    int ileft, itop, iright, ibottom;

                    ileft = (int)Canvas.GetLeft(i_FrameworkElement);
                    itop = (int)Canvas.GetTop(i_FrameworkElement);
                    iright = ileft + (int)i_FrameworkElement.ActualWidth;
                    ibottom = itop + (int)i_FrameworkElement.ActualHeight;

                    if (left <= ileft && right >= iright && top <= itop && bottom >= ibottom)
                        i.AddToSelectedEdges();
                }

            SelectedVerticesUpdated();
        }

        public void PaintDiagram()
        {
            if (ActualHeight != 0 || IsFirstPainted)
            {                       
                IsPaiting = true;                

                TheCanvas.Children.Clear();

                foreach (IItem i in Items)
                    if (i is IDisposable)
                        ((IDisposable)i).Dispose();

                Width = Size.Width ;
                Height = Size.Height;

                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 200));

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                //////////////////////////////////////// 

                foreach (IItem i in Items)
                    if(i is IUXItem)
                        HostItem((IUXItem)i);

                UpdateLayout(); // here

                AddLineObjects();

                SelectionArea = new SelectionArea(TheCanvas);
                

                SelectionArea.HideSelectionArea();

               

                SelectWrappersForSelectedVertices();

                IsFirstPainted = true;

                IsPaiting = false;

                CheckAndUpdateDiagramLines();

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////    
            }
        }

        public void SetFocus()
        {
            this.Focusable = true;

            Keyboard.Focus(this);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            SetFocus();

            PaintDiagram();

            if (IsFirstPainted)
                this.Loaded -= OnLoad;

            VisualiserHelper.AddContextMenu();
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

            IVertex option = MinusZero.Instance.DefaultUserInteraction.SelectDialogButton(info, options, null);

            if (option == null || option == optionCancel)
                return;

            IList<IEdge> selectedEdges_copy = GeneralUtil.CreateAndCopyList<IEdge>(selectedEdges);

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            UnselectAllSelectedEdges();

            foreach (IEdge e in selectedEdges_copy)
                foreach (IUXItem i in GetItemsDictionary()[e.To.Get(false, "To:")])
                {  // what about multiple items for same BaseEdge:\To: ?

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

            if (selectedLine.Vertex.Get(false, @"BaseEdge:\Meta:\$VertexTarget:") != null)
                onlyEdge = false;

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();

            if (onlyEdge)
                info.Value = "DELETE line's edge?";
            else
                info.Value = "DELETE line's vertex?";

            IVertex options = m0.MinusZero.Instance.CreateTempVertex();

            options.AddVertex(null, "Yes");
          
            IVertex optionCancel = options.AddVertex(null, "Cancel");

            IVertex option = MinusZero.Instance.DefaultUserInteraction.SelectDialogButton(info, options, null);

            if (option == optionCancel)
                return;

            selectedLine.FromDiagramItem.RemoveDiagramLine(selectedLine);

            if (onlyEdge)
            {             
                GraphUtil.DeleteEdge(selectedLine.FromDiagramItem.Vertex.Get(false, @"BaseEdge:\To:"), 
                    selectedLine.Vertex.Get(false, @"BaseEdge:\Meta:"),
                    selectedLine.Vertex.Get(false, @"BaseEdge:\To:"));
            }
            else
            {
                GraphUtil.DeleteEdge(selectedLine.Vertex.Get(false, @"BaseEdge:\To:"),
               MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"),
               selectedLine.Vertex.Get(false, @"BaseEdge:\To:\$EdgeTarget:"));

                GraphUtil.DeleteEdge(selectedLine.FromDiagramItem.Vertex.Get(false, @"BaseEdge:\To:"),
                  selectedLine.Vertex.Get(false, @"BaseEdge:\Meta:"),
                  selectedLine.Vertex.Get(false, @"BaseEdge:\To:"));
            }
        }

        protected void MouseButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            SelectionArea.StartSelection(e.GetPosition(TheCanvas));            

            ClickTarget = ClickTargetEnum.Selection;

            UnselectAllSelectedEdges();
        }

        protected void CreateAndUpdateDiagramLine(double ToX, double ToY)
        {
            if (CreatedDiagramLine == null)
            {
                CreatedDiagramLine = new Line();

                IsDrawingLine = true;

                Panel.SetZIndex(CreatedDiagramLine, 100000);

                CreatedDiagramLine.Stroke = (Brush)FindResource("0HighlightBrush");

                CreatedDiagramLine.StrokeThickness = 2;

                TheCanvas.Children.Add(CreatedDiagramLine);

                FrameworkElement ClickedItem_FrameworkElement = (FrameworkElement)ClickedItem;

                CreatedDiagramLine.X1 = Canvas.GetLeft(ClickedItem_FrameworkElement) + ClickedItem_FrameworkElement.ActualWidth;
                CreatedDiagramLine.Y1 = Canvas.GetTop(ClickedItem_FrameworkElement);
            }

            CreatedDiagramLine.X2 = ToX;
            CreatedDiagramLine.Y2 = ToY;

            Point p = new Point(ToX, ToY);            

            foreach (IItem _i in Items)
                if(_i is IUXItem && _i is UIElement)
                {
                    IUXItem i = (IUXItem)_i;

                    UIElement i_UIElement = (UIElement)i;

                    if (VisualTreeHelper.HitTest(i_UIElement, TranslatePoint(p, i_UIElement)) != null)
                    {
                        if (HighlightedItem == null)
                        {
                            i.Highlight();

                            HighlightedItem = i;
                        }
                    }
                    else
                    {
                        if (HighlightedItem == i)
                        {
                            HighlightedItem = null;

                            i.Unhighlight();
                        }
                    }
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
                    foreach (IUXItem item in GetItemsByBaseEdge(ed.To))
                    {
                        if (!(item is FrameworkElement))
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

                        TheCanvas.Children.Add(r);
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
                TheCanvas.Children.Remove(r);

            foreach (IEdge ed in Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"))
                foreach (IUXItem item in GetItemsByBaseEdge(ed.To))
                        item.MoveItem(item.Position.X + x, item.Position.Y + y);
        }

        protected void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!(ClickedItem is FrameworkElement))
                return;

            FrameworkElement ClickedItem_FrameworkElement = (FrameworkElement) ClickedItem;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ClickTarget == ClickTargetEnum.AnchorLeftTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                        (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem_FrameworkElement.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem_FrameworkElement)),
                       ClickedItem_FrameworkElement.ActualHeight - ((e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates) - Canvas.GetTop(ClickedItem_FrameworkElement)) );                        
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        Canvas.GetLeft(ClickedItem_FrameworkElement),
                        (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem_FrameworkElement.ActualWidth,
                       ClickedItem_FrameworkElement.ActualHeight - ((e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates) - Canvas.GetTop(ClickedItem_FrameworkElement)));
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
                    CreateAndUpdateDiagramLine(e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);

                if (ClickTarget == ClickTargetEnum.AnchorLeftMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                        Canvas.GetTop(ClickedItem_FrameworkElement),
                        ClickedItem_FrameworkElement.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem_FrameworkElement)),
                       ClickedItem_FrameworkElement.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        Canvas.GetLeft(ClickedItem_FrameworkElement),
                        Canvas.GetTop(ClickedItem_FrameworkElement),
                        e.GetPosition(TheCanvas).X - Canvas.GetLeft(ClickedItem_FrameworkElement) - ClickPositionX_AnchorCordinates,
                       ClickedItem_FrameworkElement.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorLeftBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                      Canvas.GetTop(ClickedItem_FrameworkElement),
                      ClickedItem_FrameworkElement.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem_FrameworkElement)),
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem_FrameworkElement) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      Canvas.GetLeft(ClickedItem_FrameworkElement),
                      Canvas.GetTop(ClickedItem_FrameworkElement),
                      ClickedItem_FrameworkElement.ActualWidth,
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem_FrameworkElement) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      Canvas.GetLeft(ClickedItem_FrameworkElement),
                      Canvas.GetTop(ClickedItem_FrameworkElement),
                      e.GetPosition(TheCanvas).X - Canvas.GetLeft(ClickedItem_FrameworkElement) - ClickPositionX_AnchorCordinates,
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem_FrameworkElement) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.Selection) // selection
                {
                    SelectionArea.MoveSelectionArea(e.GetPosition(TheCanvas));
                    
                    //SelectItemsBySelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                    // too slow
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

                        ClickedItem.MoveItem((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates), 
                            (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates));
                    }
                }
            }else
            {
                CheckIfLineNeedsSelection(e.GetPosition(TheCanvas));
            }
        }

        ILineDecoratorBase prevSelected;
        ILineDecoratorBase selectedLine;

        public double LineSelectionDelta { get { return 10; } }

        private void CheckIfLineNeedsSelection(System.Windows.Point p)
        {
            double best = 999999;
            ILineDecoratorBase bestLine = null;

            foreach (IItem _i in Items)
                if (_i is IUXItem) {
                    IUXItem i = (IUXItem)_i;

                    foreach (IUXItem _line in i.Decorators)
                        if(_line is ILineDecoratorBase)
                        {
                            ILineDecoratorBase line = (ILineDecoratorBase)_line;

                            double len = line.GetMouseDistance(p);

                            if (len < best)
                            {
                                bestLine = line;
                                best = len;
                            }
                        }
                }

            if (best < LineSelectionDelta && bestLine != null) 
            {
                if (bestLine != prevSelected)
                {
                    if (prevSelected != null)
                        prevSelected.Unhighlight();

                    bestLine.Highlight();

                    selectedLine = bestLine;

                    prevSelected = bestLine;

                  //  UnselectAllSelectedEdges(); need to comment it

                    IsLineSelected = true;
                }
            }else
                if (IsLineSelected)
                {
                    IsLineSelected = false;

                    prevSelected.Unhighlight();

                    selectedLine = null;

                    prevSelected = null;
                }
        }

        protected void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            MouseUpOrLeave(false, e);
        }

        protected void MouseButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            MouseUpOrLeave(true, e);
        }

        protected void MouseUpOrLeave(bool IsUp, MouseEventArgs e)
        {
            if (!(ClickedItem is FrameworkElement))
                return;

            FrameworkElement ClickedItem_FrameworkElemet = (FrameworkElement)ClickedItem;

            SetFocus();

            if (ClickTarget == ClickTargetEnum.Item && IsMultiSelectionMoving)
                RemoveMultiSelectionMovingSprites(e.GetPosition(ClickedItem_FrameworkElemet).X - ClickPositionX_ItemCordinates,
                            e.GetPosition(ClickedItem_FrameworkElemet).Y - ClickPositionY_ItemCordinates);

            if (ClickTarget == ClickTargetEnum.Selection)
            {
                SelectItemsBySelectionArea();

                SelectionArea.HideSelectionArea();
            }

            if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
            {
                if (HighlightedItem != null)
                {
                    HighlightedItem.Unhighlight();

                    if (IsUp)
                        ClickedItem.DoCreateDiagramLine(HighlightedItem);
                }

                HighlightedItem = null;
                TheCanvas.Children.Remove(CreatedDiagramLine);
                CreatedDiagramLine = null;

                IsDrawingLine = false;
            }

            ClickTarget = ClickTargetEnum.MouseUpOrLeave;
        }

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100;

            if (scale != 1.0)
            {
                if (ActualHeight != 0)
                {
                    this.LayoutTransform = new ScaleTransform(scale, scale, ActualWidth / 2, ActualHeight / 2);
                }
            }
            else
                this.LayoutTransform = null;
        }               
                
        protected void UnselectAll()
        {
            foreach (IItem i in Items)
                if(i is IUXItem)
                    ((IUXItem)i).Unselect();
        }

        public void UnselectAllSelectedEdges()
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

            SelectedVerticesUpdated();
        }

        public void SelectedVerticesUpdated()
        {
            if (IsFirstPainted)
            {
                UnselectAll();

                SelectWrappersForSelectedVertices();
            }
        }

        protected void SelectWrappersForSelectedVertices()
        {
            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge e in sv)                
                if(GetItemsDictionary().ContainsKey(e.To.Get(false, "To:")))
                    foreach (IUXItem i in GetItemsDictionary()[e.To.Get(false, "To:")])
                        i.Select();
        }

        public bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                VisualisersList.RemoveVisualiser(this);

                GraphChangeTrigger.RemoveListener(VisualiserHelper.graphChangeListenerEdge);

                foreach (IItem i in Items)
                    if(i is IUXItem)
                        ((IUXItem)i).Dispose();

                if (!IsVisualiser)
                    TypedEdge.RemoveFromDictionary(this);
            }
        }
     
        // IHasLocalizableEdges

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByLocation(Point p)
        {
            vertexByLocationToReturn = null;

            foreach(IItem i in Items)
                if(i is UIElement)
                {
                    UIElement i_UIElement = (UIElement)i;

                    if (VisualTreeHelper.HitTest(i_UIElement, TranslatePoint(p, i_UIElement)) != null)
                    {
                        IVertex v = MinusZero.Instance.CreateTempVertex();
                        //Edge.AddEdgeEdgesOnlyTo(v, i.Vertex.Get(false, @"BaseEdge:\To:"));
                        EdgeHelper.AddEdgeVertexEdges(v,i.Vertex.Get(false, @"BaseEdge:\From:"),i.Vertex.Get(false, @"BaseEdge:\Meta:"), i.Vertex.Get(false, @"BaseEdge:\To:"));
                        vertexByLocationToReturn = v;
                    }
                }          

            return vertexByLocationToReturn;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

       /////////////////////////////

        private void AddDiagramItemDialog(double x, double y, IVertex vv, bool isSet, DragEventArgs e)
        {
            IVertex r = m0.MinusZero.Instance.Root;

            NewUXItem ndi = new NewUXItem(vv, isSet, WpfUtil.GetMousePositionDnd(e));

            if (ndi.UXTemplate != null)
            {
                if (ndi.InstanceOfMeta)
                {
                    IEdge ve = VertexOperations.AddInstanceAndReturnEdge(
                        BaseEdge.From
                        //Vertex.Get(false, "CreationPool:")
                        , ndi.BaseEdge.Get(false, "To:"));
                    IVertex v = ve.To;

                    v.Value = ndi.InstanceValue;

                    if (ndi.UXTemplate.ForceShowEditForm)
                        MinusZero.Instance.DefaultUserInteraction.Edit(ve.To, WpfUtil.GetMousePositionDnd(e));
         
                    AddDiagramItem(x,
                                   y,
                                   ndi.UXTemplate,
                                   ndi.BaseEdge.Get(false, "To:"), v);
                }
                else
                {
                    bool ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = false;
                    bool ThereIsDiagramItemOfThisBaseEdgeTo = false;

                    IVertex DiagramItemOfThisDiagramItemDefinition = Vertex.GetAll(false, @"Item:{UXTemplate:" + ndi.UXTemplate.Vertex.Value + "}");

                    foreach (IEdge ee in DiagramItemOfThisDiagramItemDefinition)
                        if (ee.To.Get(false, @"BaseEdge:\To:") == ndi.BaseEdge.Get(false, "To:"))
                            ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = true;

                    if(GetItemsDictionary().ContainsKey(ndi.BaseEdge.Get(false, "To:")))
                    foreach (IUXItem b in GetItemsDictionary()[ndi.BaseEdge.Get(false, "To:")])
                        ThereIsDiagramItemOfThisBaseEdgeTo = true;

                    /*if (b.Vertex.Get(false, @"BaseEdge:\To:") == ndi.BaseEdge.Get(false, "To:"))
                        ThereIsDiagramItemOfThisBaseEdgeTo = true;*/

                    if (ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo == false)
                    {
                        if (ThereIsDiagramItemOfThisBaseEdgeTo == false ||
                            GeneralUtil.CompareStrings(r.Get(false, @"User\CurrentUser:\Settings:\AllowManyUXItemsWithSameBaseEdgeTo:").Value, "True"))
                        {
                            AddDiagramItem(x,
                                        y,
                                        ndi.UXTemplate,
                                        ndi.BaseEdge);
                        }
                        else
                            UserInteractionUtil.ShowError(Vertex.Value + " UXAggregtor","There is allready UX Item, that visualises dropped vertex.\n\nNow, it is not possible to add second representation of same vertex.\n\nOne can change this limitation by changing \"User\\CurrentUser:\\Settings:\\AllowManyUXItemsWithSameBaseEdgeTo:\" setting.");
                        
                    }
                    else
                        UserInteractionUtil.ShowError(Vertex.Value + " UXAggregator","There is allready \"" + ndi.UXTemplate.Vertex.Value + "\" UX Item, that visualises dropped vertex.\n\nIt is not possible to add second representation of same vertex, with the same UX Item type.");
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex r = m0.MinusZero.Instance.Root;

                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                double x = e.GetPosition(TheCanvas).X, y = e.GetPosition(TheCanvas).Y;

                bool isSet = false;

                if (dndVertex.Count() > 1)
                    isSet = true;

                if(isSet)
                    User.Process.UX.NonAtomProcess.StartNonAtomProcess();

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                //////////////////////////////////////// 

                foreach (IEdge eee in dndVertex)
                {
                    AddDiagramItemDialog(x,y, eee.To,isSet,e);
                    x += 25;
                    y += 25;
                }

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                //////////////////////////////////////// 

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

        private IUXItem AddDiagramItem_Base(double x, double y , UXTemplate UXTemplate){
            IItem _i = AddItem(UXTemplate.ItemClass);

            if (!(_i is IUXItem))
                return null;

            IUXItem i = (IUXItem)_i;

            i.Position.X = x;
            i.Position.Y = y;
            i.UXTemplate = UXTemplate;
             
            if (UXTemplate.ItemVertex != null)
                AddEdgesFromDefintion(i.Vertex, UXTemplate.ItemVertex);

            return i;
        }

        public void AddDiagramItem(double x, double y, UXTemplate UXTemplate, IVertex BaseEdge){
            IUXItem i = AddDiagramItem_Base(x, y, UXTemplate);

            IVertex edge = GraphUtil.CreateOrReplaceEdgeByValue(i.Vertex, BaseEdge_meta, "");

            EdgeHelper.AddEdgeVertexEdgesByEdgeVertex(edge, BaseEdge);      

            HostItem(i);            
        }

        public void AddDiagramItem(double x, double y, UXTemplate UXTemplate, IVertex metaVertex,IVertex newVertex)
        {
            IUXItem i = AddDiagramItem_Base(x, y, UXTemplate);

            IVertex be = i.Vertex.Get(false, "BaseEdge:");

            EdgeHelper.AddEdgeVertexEdgesOnlyMetaTo(be, metaVertex, newVertex);

            HostItem(i);
        }

        public void CheckAndUpdateDiagramLines()
        {
            foreach(IItem item in Items)
                if(item is IUXItem)
                    CheckAndUpdateDiagramLinesForItem((IUXItem)item);
        }

        public void CheckAndUpdateDiagramLinesForItem(IUXItem item)
        {
            IEnumerable<IEdge> edges;

            IVertex item_BaseEdgeTo = item.BaseEdgeTo;

            if (item.UXTemplate.DoNotShowInherited)
                edges = item_BaseEdgeTo.OutEdgesRaw;
            else
                edges = item_BaseEdgeTo;
            
            foreach (IEdge e in item_BaseEdgeTo)
            {
                List<IUXItem> toDiagramItems = null;

                bool needAdding = true;

                if(item.GetDiagramLinesBaseEdgeToDictionary().ContainsKey(e.To))
                foreach (LineDecoratorBase l in item.GetDiagramLinesBaseEdgeToDictionary()[e.To])
                    {
                        IVertex l_BaseEdge = GraphUtil.GetQueryOutFirst(l.Vertex, "BaseEdge", null);

                        if (l_BaseEdge != null &&
                            GraphUtil.GetQueryOutFirst(l_BaseEdge, "Meta", null) == e.Meta)
                            needAdding = false;
                    }

                if (needAdding) {
                    toDiagramItems = GetItemsByBaseEdgeTo_ForLines(e);

                    foreach (IUXItem toDiagramItem in toDiagramItems)
                    {
                        IVertex lineDef = GetLineDefinition(e, item.Vertex, toDiagramItem);

                        if (lineDef != null)
                            item.AddDiagramLineVertex(e, lineDef, toDiagramItem);
                        
                    }
                }           
            }
        }

        protected List<IUXItem> GetItemsByBaseEdgeTo_ForLines(IEdge toEdge) // MAX TOO
        {
            List<DiagramItemBase> r = new List<DiagramItemBase>();

            if(GetItemsDictionary().ContainsKey(toEdge.To))
            foreach (DiagramItemBase i in GetItemsDictionary()[toEdge.To])
                r.Add(i);

            if (toEdge.Meta.Get(false, "$VertexTarget:") != null && toEdge.To.Get(false, "$EdgeTarget:")!=null)
            if(GetItemsDictionary().ContainsKey(toEdge.To.Get(false, "$EdgeTarget:")))
            foreach (DiagramItemBase i in GetItemsDictionary()[toEdge.To.Get(false, "$EdgeTarget:")])
                r.Add(i);

            

            return r;
        }

        public IVertex GetLineDefinition(IEdge e,IVertex Vertex, IUXItem toItem){
            if (GeneralUtil.CompareStrings(Vertex.Get(false, "Definition:"), "Vertex")) // Vertex / Edge
                return Vertex.Get(false, @"Definition:\DiagramLineDefinition:Edge");

            foreach (IEdge def in Vertex.GetAll(false, @"Definition:\DiagramLineDefinition:"))
            {
                bool canReturn=true;

                if(def.To.Get(false, "EdgeTestQuery:")!=null){
                    canReturn=false;

                    foreach (IEdge toTest in Vertex.GetAll(false, @"BaseEdge:\To:\" + def.To.Get(false, "EdgeTestQuery:")))
                        if (toTest.To == e.Meta)
                            canReturn = true;
                }

                if (canReturn && def.To.Get(false, "ToDiagramItemTestQuery:") != null && toItem.Vertex.Get(false, (string)def.To.Get(false, "ToDiagramItemTestQuery:").Value) != null)
                    return def.To;
            }

            return null;           
        }

        // UNDERPINNINGS

        // UXAggregator

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXAggregator\CollapsedSize");

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

        //

        public IUXAggregator Diagram { get; set; } // ParentAggregator

        public bool IsSelected { get; set; }

        public bool IsHighlighted { get; set; }

        List<LineDecoratorBase> diagramLines = new List<LineDecoratorBase>();

        public List<LineDecoratorBase> DiagramLines
        {
            get { return diagramLines; }
        }

        public virtual void VertexSetedUp() { }

        public Dictionary<IVertex, List<LineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary() { return null; }

        public virtual void RemoveFromCanvas() { }

        public virtual void DoCreateDiagramLine(IUXItem toItem) { }

        public void AddDiagramLineVertex(IEdge edge, IVertex diagramLineDefinition, IUXItem toItem) { }

        public void AddDiagramLineObject(IUXItem toItem, LineDecorator lineDecorator) { }

        public void RemoveDiagramLine(LineDecoratorBase line) { }

        public virtual void Select() { }

        public virtual void Unselect() { }

        public virtual void Highlight() { }

        public virtual void Unhighlight() { }

        public void MoveItem(double x, double y) { }

        public void MoveAndResizeItem(double left, double top, double width, double height) { }

        public void AddToSelectedEdges() { }

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

                if (val == null)
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

        // AddItem is higher

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
    }
}
