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
using m0.ZeroCode.Helpers;

namespace m0.UIWpf.UX
{

    public class MetaToPair
    {
        public IVertex Meta;
        public IVertex To;
        public int LineDecoratorNumber;
        public int EdgesNumber;
    }

    public class UXVisualiser : Border, IListVisualiser, IUXVisualiser, IMouseWheelHandler
    {
        static IVertex systemMetaBaseVertex = m0.MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex");

        public int ItemNestingLevel { get; set; }

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


        public bool IsPaiting = false;

        bool IsFirstPainted = false;

        static string[] _MetaTriggeringUpdateVertex = new string[] { "Width", "Height"};
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public virtual string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public virtual void UpdateView() { }

        public virtual void UpdateVertex() { PaintDiagram(); }

        //

        public UXVisualiser(IEdge _edge)
        {
            edge = _edge;

            vertex = _edge.To;

            TypedEdge.vertexDictionary.Add(this.Edge.To, this);

            IsVisualiser = false;
        }

        //

        void SetUpCanvas()
        {            
            Canvas = new Canvas();            

            Canvas.Background = (Brush)FindResource("0BackgroundBrush");

            this.Child = Canvas;
        }

        public UXVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            if (VisualisersList.GetVisualiser(baseEdgeVertex.Get(false, "To:")) != null)
            {
                UserInteractionUtil.ShowError("Diagram Visualiser", "There is allready Diagram Visualiser opened for this Edge");

                return;
            }

            IsVisualiser = true;

            SetUpCanvas();

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

        public static IUXItem GetUXItem(IItem i)
        {
            if (GraphUtil.ExistQueryOut(i.Vertex, "$Is", "Wrap"))
                return null;

            if (i is IUXItem)
                return (IUXItem)i;

            return null;            
        }

        // OPTIMISATION START

        List<IUXItem> Items_all = new List<IUXItem>();

        Dictionary<IVertex, List<IUXItem>> ItemsDictionary = new Dictionary<IVertex, List<IUXItem>>();

        bool needRebuildItemsDictionary = true;

        void RebuidItemsDictionary()
        {
            ItemsDictionary.Clear();

            foreach(IUXItem ui in Items_all)
                {                    
                    IVertex ui_BaseEdgeTo = ui.BaseEdgeTo;

                    if (ItemsDictionary.ContainsKey(ui_BaseEdgeTo))
                        ItemsDictionary[ui_BaseEdgeTo].Add(ui);
                    else
                    {
                        List<IUXItem> list = new List<IUXItem>();
                        list.Add(ui);

                        ItemsDictionary.Add(ui_BaseEdgeTo, list);
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
            item.ParentItem.RemoveItem(item);

            Items_all.Remove(item);

            needRebuildItemsDictionary = true;

            item.RemoveFromCanvas();

            item.Dispose(); // check if will not cause problems
        }

        // TOO
        protected List<IUXItem> GetItemsByBaseEdge(IVertex edgeToVertex)
        {
           return GetItemsDictionary()[GraphUtil.GetQueryOutFirst(edgeToVertex, "To", null)];
        }        

        public void AddEdgesFromDefintion(IVertex baseVertex, IVertex definitionEdges)
        {
            if (definitionEdges == null)
                return;

            foreach (IEdge e in definitionEdges)
            {
                if(VertexOperations.IsAtomicVertex(e.To))
                    GraphUtil.SetVertexValue(baseVertex, e.Meta, e.To.Value); // shallow copy
                else
                    GraphUtil.CreateOrReplaceEdge(baseVertex, e.Meta, e.To); // deep BUT NOT COPY
            }
        }

        public void HostItem(IUXContainer host, IUXItem item){
            if (!(item is UIElement))
                return;

            Items_all.Add(item);

            //

            UIElement item_UIElement = (UIElement)item;

            item.Diagram = this;                

            item.VertexSetedUp();

            needRebuildItemsDictionary = true;            
                
            Panel.SetZIndex(item_UIElement, 1);
               
            Canvas.SetLeft(item_UIElement, item.Position.X);
            Canvas.SetTop(item_UIElement, item.Position.Y);

            
            host.Canvas.Children.Add(item_UIElement);            

            //                     

            if (item is IUXContainer)
            {
                IUXContainer container = (IUXContainer)item;

                if (container.Canvas != null)
                    container.Canvas.Children.Clear();

                foreach (IItem _i in container.Items)
                {
                    IUXItem i = GetUXItem(_i);

                    if (i == null)
                        continue;

                    HostItem(container, i);
                }
            }
            //

            item_UIElement.UpdateLayout(); 
        }

        // TOO
        
        public void AddLineObjects()
        {
            List<MetaToPair> metatopairs = new List<MetaToPair>();

           foreach(IItem _item in Items_all)
               {
                   IUXItem item = GetUXItem(_item);

                   if (item == null)
                      continue;

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

            foreach(IItem _i in Items_all)                
                {
                    IUXItem i = GetUXItem(_i);

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

        public void PaintDiagram()
        {
            if (ActualHeight != 0 || IsFirstPainted)
            {                       
                IsPaiting = true;                

                Canvas.Children.Clear();

                Width = Size.Width ;
                Height = Size.Height;

                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 200));

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                //////////////////////////////////////// 

                Items_all.Clear();

                //

                foreach (IItem _i in Items)
                {
                    IUXItem i = GetUXItem(_i);

                    if (i == null)
                        continue;

                    HostItem(this, i);
                } 
                
                //
                        
                UpdateLayout(); // here

                AddLineObjects();

                SelectionArea = new SelectionArea(Canvas);
                

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
            if (IsFirstPainted)
                return;

            SetFocus();

            PaintDiagram();

            if (IsFirstPainted)
                this.Loaded -= OnLoad;

            VisualiserHelper.AddContextMenu();

            ScrollViewerParent = GetScrollViewerParent(this);
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

            IVertex option = MinusZero.Instance.DefaultUserInteraction.SelectDialogButton(info, options, null);

            if (option == optionCancel)
                return;

            SelectedLine.FromDiagramItem.RemoveDiagramLine(SelectedLine);

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
        }

        protected void MouseButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            SelectionArea.StartSelection(e.GetPosition(Canvas));

            ClickTarget = ClickTargetEnum.Selection;

            UnselectAllSelectedEdges();
        }

        protected void CreateAndUpdateCreateDiagramLine(double ToX, double ToY)
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

                CreateOrMoveDiagramLine.X1 = Canvas.GetLeft(ClickedItem_FrameworkElement) + ClickedItem_FrameworkElement.ActualWidth;
                CreateOrMoveDiagramLine.Y1 = Canvas.GetTop(ClickedItem_FrameworkElement);
            }

            CreateOrMoveDiagramLine.X2 = ToX;
            CreateOrMoveDiagramLine.Y2 = ToY;

            Point p = new Point(ToX, ToY);            

            foreach (IItem _i in Items_all)
                {
                    IUXItem i = GetUXItem(_i);

                    if (i == null || !(i is UIElement))
                        continue;

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

            foreach (IItem _i in Items_all)
            {
                IUXItem i = GetUXItem(_i);

                if (i == null || !(i is UIElement))
                    continue;

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
                foreach (IUXItem item in GetItemsByBaseEdge(ed.To))
                        item.MoveItem(item.Position.X + x, item.Position.Y + y);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        protected void MouseMoveHandler(object sender, MouseEventArgs e)
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

                double ClickedItem_left;
                double ClickedItem_top;

                if (ClickedItem.ParentItem is IUXVisualiser)
                {
                    ClickedItem_left = Canvas.GetLeft(ClickedItem_FrameworkElement);
                    ClickedItem_top = Canvas.GetTop(ClickedItem_FrameworkElement);
                }
                else
                {
                    Point mainCanvasPosition = ((IUXContainer)ClickedItem.ParentItem).Canvas.TranslatePoint(
                        new Point(Canvas.GetLeft(ClickedItem_FrameworkElement), 
                        Canvas.GetTop(ClickedItem_FrameworkElement)), 
                        Canvas);

                    ClickedItem_left = mainCanvasPosition.X;
                    ClickedItem_top = mainCanvasPosition.Y;
                }

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
                            (e.GetPosition(Canvas).Y - ClickPositionY_ItemCordinates));
                    }
                }
            }else
            {
                CheckIfLineNeedsSelection(e.GetPosition(Canvas));
            }
        }        

        private void CheckIfLineNeedsSelection(System.Windows.Point p)
        {
            double best = 999999;
            ILineDecoratorBase bestLine = null;
            IUXItem bestLine_FromItem = null;

            foreach (IItem _i in Items_all)
                {
                    IUXItem i = GetUXItem(_i);

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
            if (ClickTarget == ClickTargetEnum.Selection)
            {
                SelectItemsBySelectionArea();

                SelectionArea.HideSelectionArea();
            }
            if (!(ClickedItem is FrameworkElement))
                return;

            FrameworkElement ClickedItem_FrameworkElemet = (FrameworkElement)ClickedItem;

            SetFocus();

            if (ClickTarget == ClickTargetEnum.Item && IsMultiSelectionMoving)
                RemoveMultiSelectionMovingSprites(e.GetPosition(ClickedItem_FrameworkElemet).X - ClickPositionX_ItemCordinates,
                            e.GetPosition(ClickedItem_FrameworkElemet).Y - ClickPositionY_ItemCordinates);            

            if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
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

            foreach (IEdge e in fromItem.BaseEdgeTo.GetAll(false, tem.EdgeTestQuery))            
                if(CanAddLineByDecoratorTemplateAndFromItemBaseEdgeToQuery(toItem, toItem.BaseEdge, tem, e));
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

                if(!onlyDelete)
                    fromItemBaseEdgeTo.AddEdge(meta, toItem.BaseEdgeTo);

                fromItem.RemoveDecorator(line);                
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 

            PaintDiagram();
        }

        protected void UnselectAll()
        {            
            foreach (IItem _i in Items_all)
            {
                IUXItem i = GetUXItem(_i);

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

                if (IsVisualiser)
                {
                    VisualisersList.RemoveVisualiser(this);

                    GraphChangeTrigger.RemoveListener(VisualiserHelper.graphChangeListenerEdge);

                    foreach (IItem _i in Items)
                    {
                        IUXItem i = GetUXItem(_i);

                        if (i == null)
                            continue;

                        i.Dispose(); // that will iterate i.Items and will do Dispose for each of them
                    }
                }
                else
                {
                    TypedEdge.RemoveFromDictionary(this);
                }
            }
        }

        public IUXItem GetItemByPoint(Point p)
        {
            IUXItem itemToReturn = null;

            int highestNestingLevel = -1;

            foreach (IItem _i in Items_all)
            {
                IUXItem i = GetUXItem(_i);

                if (i == null)
                    continue;

                UIElement i_UIElement = (UIElement)i;

                if (VisualTreeHelper.HitTest(i_UIElement, TranslatePoint(p, i_UIElement)) != null)                                  
                    if (i.ItemNestingLevel > highestNestingLevel)
                    {
                        itemToReturn = i;
                        highestNestingLevel = i.ItemNestingLevel;
                    }
                
            }

            return itemToReturn;
        }

        void CheckAndUpdateItemComposition(IUXItem item)
        {

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

        //

        public virtual bool DoCreateDiagramLine(IUXItem fromItem, IUXItem toItem)
        {
            IEdge toEdge = toItem.BaseEdge;

            

            IVertex v = m0.MinusZero.Instance.CreateTempVertex();

            IVertex fromItemBaseEdgeTo = fromItem.BaseEdge.To;

            foreach (UXDecoratorTemplate tem in fromItem.UXTemplate.UXDecoratorTemplates)
            {
                foreach (IEdge e in fromItemBaseEdgeTo.GetAll(false, tem.EdgeTestQuery))
                {
                    bool canAdd = CanAddLineByDecoratorTemplateAndFromItemBaseEdgeToQuery(toItem, toEdge, tem, e);

                    if (canAdd)
                        AddNewLineOption(v, tem, e);
                }

                if (GeneralUtil.CompareStrings(tem.Vertex.Value, "Edge"))// Vertex\Edge
                    foreach (IEdge e in systemMetaBaseVertex)
                        AddNewLineOption(v, tem, e);

                if (tem.EdgeTestQuery == "$EdgeTarget") // $EdgeTarget is not present as there is no inheritance from Vertex
                    AddNewLineOption(v, tem, GraphUtil.FindEdgeByToVertex(systemMetaBaseVertex, "$EdgeTarget"));
            }

            if (v.Count() == 0)
                UserInteractionUtil.ShowError(Vertex.Value + " Diagram", "There is no diagram line definition matching selected source and target items.");

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();
            info.Value = "choose diagram line:";


            Point mousePosition = WpfUtil.GetMousePosition();

            IVertex a = MinusZero.Instance.DefaultUserInteraction.SelectDialog(info, v, mousePosition);

            if (a != null)
            {
                IVertex test = VertexOperations.TestIfNewEdgeValid(fromItemBaseEdgeTo, a.Get(false, "OptionEdge:"), toEdge.To);

                if (test == null)
                {
                    //UXDecoratorTemplate chosenTemplate = new UXDecoratorTemplate(a.GetAll(false, "OptionDiagramLineDefinition:").FirstOrDefault());

                    UXDecoratorTemplate chosenTemplate = (UXDecoratorTemplate)TypedEdge.Get(
                        a.GetAll(false, "OptionDiagramLineDefinition:").FirstOrDefault(), 
                        typeof(UXDecoratorTemplate));
                        
                    IEdge edge = VertexOperations.AddEdgeOrVertexByMeta(fromItemBaseEdgeTo,
                        a.Get(false, "OptionEdge:"),
                        toEdge.To,
                        mousePosition,
                        chosenTemplate.CreateEdgeOnly,
                        chosenTemplate.ForceShowEditForm);

                    AddDiagramLineVertex(fromItem, edge, chosenTemplate, toItem);

                    return true;
                }
                else
                    UserInteractionUtil.ShowError(Diagram.Vertex.Value + " Diagram", "Adding new diagram line  \"" + a.Value + "\" is not possible.\n\n" + test.Value);
            }

            return false;
        }

        private static bool CanAddLineByDecoratorTemplateAndFromItemBaseEdgeToQuery(IUXItem toItem, IEdge toEdge, UXDecoratorTemplate tem, IEdge e)
        {
            bool canAdd = true;

            if (tem.ToDiagramItemTestQuery != null && toItem.Vertex.Get(false, tem.ToDiagramItemTestQuery) == null)
                canAdd = false;

            string eToEdgeTarget = (string)GraphUtil.GetValue(e.To.Get(false, @"$EdgeTarget:"));
            string eToVertexTarget = (string)GraphUtil.GetValue(e.To.Get(false, @"$VertexTarget:"));

            if (eToEdgeTarget != null
                && eToEdgeTarget != "Vertex" // Vertices do not have $Is:Vertex     
                && !InstructionHelpers.CheckIfIsOrInherits(toEdge.To, eToEdgeTarget))
                canAdd = false;

            if (tem.CreateEdgeOnly
                && eToVertexTarget != null
                && !InstructionHelpers.CheckIfIsOrInherits(toEdge.To, eToVertexTarget)
                )
                canAdd = false;

            return canAdd;
        }

        private static void AddNewLineOption(IVertex v, UXDecoratorTemplate def, IEdge e)
        {
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex vv = v.AddVertex(null, e.To.Value + " (" + def.Vertex.Value + ")");

            vv.AddEdge(r.Get(false, @"System\Meta\ZeroTypes\UX\OptionEdge"), e.To);
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

            fromItem.AddDiagramLineObject(toItem, newLine);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }



        //

        private void dndDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex r = m0.MinusZero.Instance.Root;

                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                double x = e.GetPosition(Canvas).X, y = e.GetPosition(Canvas).Y;

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

            i.PositionCreate();
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

            HostItem(this, i);            
        }

        public void AddDiagramItem(double x, double y, UXTemplate UXTemplate, IVertex metaVertex,IVertex newVertex)
        {
            IUXItem i = AddDiagramItem_Base(x, y, UXTemplate);

            IVertex be = i.Vertex.Get(false, "BaseEdge:");

            EdgeHelper.AddEdgeVertexEdgesOnlyMetaTo(be, metaVertex, newVertex);

            HostItem(this, i);
        }

        public void CheckAndUpdateDiagramLines()
        {        
            foreach(IItem _i in Items_all)
            {
                IUXItem item = GetUXItem(_i);

                if (item == null)
                    continue;

                CheckAndUpdateDiagramLinesForItem((IUXItem)item);                
            }
        }

        public void CheckAndUpdateDiagramLinesForItem(IUXItem item)
        {
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
                List<IUXItem> toDiagramItems = null;

                bool needAdding = true;

                if(item.GetDiagramLinesBaseEdgeToDictionary().ContainsKey(e.To))
                foreach (ILineDecoratorBase l in item.GetDiagramLinesBaseEdgeToDictionary()[e.To])
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
                        UXDecoratorTemplate lineDef = GetLineDefinition(e, item, toDiagramItem);

                        if (lineDef != null)
                            AddDiagramLineVertex(item, e, lineDef, toDiagramItem);
                    }
                }           
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        protected List<IUXItem> GetItemsByBaseEdgeTo_ForLines(IEdge toEdge) // MAX TOO
        {
            List<IUXItem> r = new List<IUXItem>();

            if(GetItemsDictionary().ContainsKey(toEdge.To))
            foreach (IUXItem i in GetItemsDictionary()[toEdge.To])
                r.Add(i);

            IVertex toEdgeToEdgeTarget = GraphUtil.GetQueryOutFirst(toEdge.To, "$EdgeTarget", null);

            if (GraphUtil.ExistQueryOut(toEdge.Meta, "$VertexTarget", null) && toEdgeToEdgeTarget != null)

            if (toEdge.Meta.Get(false, "$VertexTarget:") != null && toEdge.To.Get(false, "$EdgeTarget:")!=null)
                if (GetItemsDictionary().ContainsKey(toEdgeToEdgeTarget))
                foreach (IUXItem i in GetItemsDictionary()[toEdgeToEdgeTarget])
                    r.Add(i);
            
            return r;
        }

        public UXDecoratorTemplate GetLineDefinition(IEdge e, IUXItem item, IUXItem toItem){
            // Vertex / Edge handling
            if (GraphUtil.GetValueAndCompareStrings(item.UXTemplate.Vertex, "Vertex"))
                return new UXDecoratorTemplate(item.Vertex.GetAll(false, @"UXTemplate:\UXDecoratorTemplate:Edge").FirstOrDefault());
           
            foreach (UXDecoratorTemplate tem in item.UXTemplate.UXDecoratorTemplates)            
            {
                bool canReturn=true;

                string edgeTestQuery = tem.EdgeTestQuery;

                if(edgeTestQuery != null){
                    canReturn=false;

                    foreach (IEdge toTest in item.BaseEdgeTo.GetAll(false, edgeTestQuery))
                        if (toTest.To == e.Meta)
                            canReturn = true;
                }

                if (canReturn)
                {
                    string toDiagramItemTestQuery = tem.ToDiagramItemTestQuery;

                    if (toDiagramItemTestQuery != null
                        && toItem.Vertex.Get(false, toDiagramItemTestQuery) != null)
                        return tem;
                }
            }

            return null;           
        }

        // UNDERPINNINGS

        // UXAggregator

        static IVertex IsExpanded_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\IsExpanded");
        static IVertex ExpandedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ExpandedSize");
        static IVertex CollapsedSize_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\CollapsedSize");
        static IVertex ContainerEdgeQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\ContainerEdgeQuery");

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

        public string ContainerEdgeQuery
        {
            get
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    return "";

                return GraphUtil.GetStringValue(val);
            }
            set
            {
                IVertex val = GraphUtil.GetQueryOutFirst(Vertex, "ContainerEdgeQuery", null);

                if (val == null)
                    val = Vertex.AddVertex(ContainerEdgeQuery_meta, value);
                else
                    val.Value = value;
            }
        }

        //

        public IUXVisualiser Diagram { get; set; } // ParentAggregator

        public bool IsSelected { get; set; }

        public bool IsHighlighted { get; set; }

        public List<ILineDecoratorBase> DiagramLines { get; } = new List<ILineDecoratorBase>();

        public List<ILineDecoratorBase> DiagramToLines { get; } = new List<ILineDecoratorBase>();

        public List<ILineDecoratorBase> DiagramToAsMetaLines { get; } = new List<ILineDecoratorBase>();

        public virtual void VertexSetedUp() { }

        public Dictionary<IUXItem, List<ILineDecoratorBase>> GetDiagramLinesToDiagramItemDictionary() { return null; }

        public Dictionary<IVertex, List<ILineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary() { return null; }

        public virtual void RemoveFromCanvas() { }

        public void AddDiagramLineObject(IUXItem toItem, ILineDecoratorBase lineDecorator) { }

        public void RemoveDiagramLine(ILineDecoratorBase line) { }

        public virtual void Select() { }

        public virtual void Unselect() { }

        public virtual void Highlight() { }

        public virtual void Unhighlight() { }

        public void MoveItem(double x, double y) { }

        public void MoveAndResizeItem(double left, double top, double width, double height) { }

        public void AddToSelectedEdges() { }

        public Point GetLineAnchorLocation(IUXItem toItem, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart) { return new Point(); }

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
                    ret.Add((IUXItem)TypedEdge.Get(e));

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

        public IList<IItem> Items
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Item", null);

                IList<IItem> ret = new List<IItem>();

                foreach (IEdge e in list)
                {
                    IItem i = (IItem)TypedEdge.Get(e);
                    i.ParentItem = this;
                    ret.Add(i);                    
                }

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
