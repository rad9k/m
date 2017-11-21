using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.UML;
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

namespace m0.UIWpf.Visualisers.Diagram
{
    public enum ClickTargetEnum
    {
        MouseUpOrLeave, Selection, Item, AnchorLeftTop, AnchorMiddleTop, AnchorRightTop_CreateDiagramLine, AnchorLeftMiddle, AnchorRightMiddle, AnchorLeftBottom, AnchorMiddleBottom, AnchorRightBottom
    }

    public class MetaToPair
    {
        public IVertex Meta;
        public IVertex To;
        public int DiagramLinesNumber;
        public int EdgesNumber;
    }

    public class Diagram : Border, IPlatformClass, IDisposable, IHasLocalizableEdges, IHasSelectableEdges
    {
        public Canvas TheCanvas;

        Rectangle SelectionArea;

        public bool IsSelecting = false;

        public bool IsDrawingLine = false;

        public Line CreatedDiagramLine;

        public FrameworkElement ClickedAnchor;

        int SelectionAreaLeft, SelectionAreaTop;

        public List<DiagramItemBase> Items;

        public ClickTargetEnum ClickTarget;

        public DiagramItemBase ClickedItem;

        public DiagramItemBase HighlightedItem;

        public double ClickPositionX_ItemCordinates;
        public double ClickPositionY_ItemCordinates;

        public double ClickPositionX_AnchorCordinates;
        public double ClickPositionY_AnchorCordinates;
        
        public bool IsPaiting = false;

        bool IsFirstPainted = false;


        // OPTIMISATION START

        Dictionary<IVertex, List<DiagramItemBase>> ItemsDictionary = new Dictionary<IVertex, List<DiagramItemBase>>();

        bool needRebuildItemsDictionary = true;

        void AddToItems(DiagramItemBase item)
        {
            Items.Add(item);

            needRebuildItemsDictionary = true;
        }

        void RemoveFromItems(DiagramItemBase item)
        {
            Items.Remove(item);

            needRebuildItemsDictionary = true;
        }

        void ClearItems()
          {
            Items.Clear();

            needRebuildItemsDictionary = true;
        }

        void RebuidItemsDictionary()
        {
            ItemsDictionary.Clear();

            foreach(DiagramItemBase i in Items)
            {
                IVertex baseEdgeTo = i.Vertex.Get(@"BaseEdge:\To:");

                if (ItemsDictionary.ContainsKey(baseEdgeTo))
                    ItemsDictionary[baseEdgeTo].Add(i);
                else
                {
                    List<DiagramItemBase> list = new List<DiagramItemBase>();
                    list.Add(i);

                    ItemsDictionary.Add(baseEdgeTo, list);
                }
            }

            needRebuildItemsDictionary = false;
        }

        public Dictionary<IVertex, List<DiagramItemBase>> GetItemsDictionary()
        {
            if (needRebuildItemsDictionary)
                RebuidItemsDictionary();

            return ItemsDictionary;
        }

        // OPTIMISATION END

        public void RemoveItem(DiagramItemBase item)
        {
            RemoveFromItems(item);

            item.RemoveFromCanvas();
        }

        // TOO
        protected List<DiagramItemBase> GetItemsByBaseEdge(IVertex to)
        {
           return GetItemsDictionary()[to.Get("To:")];
            
            /*List<DiagramItemBase> r = new List<DiagramItemBase>();

            foreach (DiagramItemBase i in GetItemsDictionary)
               if (i.Vertex.Get(@"BaseEdge:\To:") == to.Get("To:"))
                    r.Add(i);

            return r;*/
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

        protected void AddItem(IVertex ItemVertex){
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(ItemVertex);

            if (pc is DiagramItemBase)
            {
                DiagramItemBase item = (DiagramItemBase)pc;

                item.Diagram = this;                

                item.VertexSetedUp();


                AddToItems(item);
                

                Panel.SetZIndex(item, 1);
                Canvas.SetLeft(item, GraphUtil.GetDoubleValue(item.Vertex.Get("PositionX:")));
                Canvas.SetTop(item, GraphUtil.GetDoubleValue(item.Vertex.Get("PositionY:")));        

                TheCanvas.Children.Add(item);

                item.UpdateLayout(); 
            }
        }

        // TOO
        public void AddLineObjects()
        {
            List<MetaToPair> metatopairs = new List<MetaToPair>();

           foreach(DiagramItemBase item in Items){
               metatopairs.Clear();

               foreach (IEdge l in item.Vertex.GetAll("DiagramLine:")) // calculate DiagramLines number and Edges number for each Meta/To edge pair
               {
                   MetaToPair found = null;

                   foreach (MetaToPair pair in metatopairs)
                       if (pair.Meta == l.To.Get(@"BaseEdge:\Meta:") && pair.To == l.To.Get(@"BaseEdge:\To:"))
                           found = pair;

                   if (found == null)
                   {
                       MetaToPair newpair = new MetaToPair();
                       newpair.Meta = l.To.Get(@"BaseEdge:\Meta:");
                       newpair.To = l.To.Get(@"BaseEdge:\To:");
                       newpair.DiagramLinesNumber = 1;
                       newpair.EdgesNumber = 0;

                       foreach(IEdge e in item.Vertex.GetAll(@"BaseEdge:\To:\"))
                           if (newpair.Meta == e.Meta && newpair.To == e.To)
                               newpair.EdgesNumber++;

                       metatopairs.Add(newpair);
                       
                   }else
                       found.DiagramLinesNumber++;
               }

               foreach(MetaToPair pair in metatopairs){ // delete DiagramLines for edges that been deleted
                   if(pair.DiagramLinesNumber>pair.EdgesNumber)
                       foreach(IEdge e in item.Vertex.GetAll("DiagramLine:")){
                           if(pair.Meta == e.To.Get(@"BaseEdge:\Meta:") && pair.To == e.To.Get(@"BaseEdge:\To:") && pair.DiagramLinesNumber>pair.EdgesNumber){
                               item.Vertex.DeleteEdge(e);
                               pair.DiagramLinesNumber--;

                                IVertex c = m0.MinusZero.Instance.Root.Get(@"TEST\Counter:");
                                c.Value=((int)c.Value)+1;
                           }
                       }
               }

               foreach (IEdge l in item.Vertex.GetAll("DiagramLine:")) // add diagram line objects
                   item.AddDiagramLineObject(GetToDiagramItemFromLineVertex(l.To), l.To);
               }
                            
        }

        // TOO
        public DiagramItemBase GetToDiagramItemFromLineVertex(IVertex lineVertex)
        {
            IVertex toFind = null;

            if (lineVertex.Get(@"BaseEdge:\Meta:\$VertexTarget:") != null
                && !GraphUtil.GetValueAndCompareStrings(lineVertex.Get(@"Definition:\CreateEdgeOnly:"), "True"))
                toFind = lineVertex.Get(@"BaseEdge:\To:\$EdgeTarget:");
            else
                toFind = lineVertex.Get(@"BaseEdge:\To:");

            if (toFind != null)
                foreach(DiagramItemBase i in GetItemsDictionary()[toFind])
                    if (!(lineVertex.Get(@"Definition:\ToDiagramItemTestQuery:") != null && i.Vertex.Get((string)lineVertex.Get(@"Definition:\ToDiagramItemTestQuery:").Value) == null))
                        return i;

               /* foreach (DiagramItemBase i in Items)
                {
                    bool canReturn = true;

                    if (lineVertex.Get(@"Definition:\ToDiagramItemTestQuery:") != null && i.Vertex.Get((string)lineVertex.Get(@"Definition:\ToDiagramItemTestQuery:").Value) == null)
                        canReturn = false;

                    if (i.Vertex.Get(@"BaseEdge:\To:") == toFind && canReturn)
                        return i;
                }*/

            return null;
        }

        // TOO
        public List<DiagramItemBase> GetToDiagramItemFromEdge(IEdge edge)
        {
            IVertex toFind = null;

            List<DiagramItemBase> list=new List<DiagramItemBase>();

            if (edge.Meta.Get(@"$VertexTarget:") != null)
                toFind = edge.To.Get(@"$EdgeTarget:");
            else
                toFind = edge.To;

            return GetItemsDictionary()[toFind];

           /* if (toFind != null)
                foreach (DiagramItemBase i in Items)
                {                  
                    if (i.Vertex.Get(@"BaseEdge:\To:") == toFind)
                        list.Add(i);
                }

            return list;*/
        }

        public void HideSelectionArea()
        {
            Canvas.SetLeft(SelectionArea, 0);
            Canvas.SetTop(SelectionArea, 0);
            SelectionArea.Width = 0;
            SelectionArea.Height = 0;

            IsSelecting = false;
        }

        public void SetSelectionArea(double left, double top, double right, double bottom) {
            double _left, _right, _top, _bottom;

            SelectionArea_RemapCordinates(left, top, right, bottom, out _left, out _right, out _top, out _bottom);

            Canvas.SetLeft(SelectionArea, _left);
            Canvas.SetTop(SelectionArea, _top);
            SelectionArea.Width = _right - _left;
            SelectionArea.Height = _bottom - _top;

            IsSelecting = true;
        }

        private static void SelectionArea_RemapCordinates(double left, double top, double right, double bottom, out double _left, out double _right, out double _top, out double _bottom)
        {
            if (left > right)
            {
                _left = right;
                _right = left;
            }
            else
            {
                _left = left;
                _right = right;
            }

            if (top > bottom)
            {
                _top = bottom;
                _bottom = top;
            }
            else
            {
                _top = top;
                _bottom = bottom;
            }
        }

        private void TurnOnSelectedEdgesFireChange()
        {
            if (Vertex.Get("SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get("SelectedEdges:")).CanFireChangeEvent = true;
        }

        private void TurnOffSelectedEdgesFireChange()
        {
            if (Vertex.Get("SelectedEdges:") is VertexBase)
                ((VertexBase)Vertex.Get("SelectedEdges:")).CanFireChangeEvent = false;
        }

        void SelectItemsBySelectionArea(double left, double top, double right, double bottom)
        {
            double _left, _right, _top, _bottom;

            SelectionArea_RemapCordinates(left, top, right, bottom, out _left, out _right, out _top, out _bottom);

            UnselectAllSelectedEdges();

            TurnOffSelectedEdgesFireChange();

            foreach(DiagramItemBase i in Items){
                int ileft, itop, iright, ibottom;

                ileft = (int)Canvas.GetLeft(i);
                itop = (int)Canvas.GetTop(i);
                iright = ileft + (int)i.ActualWidth;
                ibottom = itop + (int)i.ActualHeight;

                if (_left <= ileft && _right >= iright && _top <= itop && _bottom >= ibottom)
                    i.AddToSelectedEdges();
            }

            TurnOnSelectedEdgesFireChange();

            SelectedVertexesUpdated();
        }

        private void VertexChangeListenOff()
        {
           ((EasyVertex)Vertex).CanFireChangeEvent = false;

            PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));            
        }

        private void VertexChangeListenOn()
        {
            ((EasyVertex)Vertex).CanFireChangeEvent = true;

            PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });            
        }

        public void PaintDiagram()
        {
            if (ActualHeight != 0 || IsFirstPainted)
            {
                //MinusZero.Instance.Log(1, "Diagram", "");

                // turn off Vertex.Change listener

                VertexChangeListenOff();

                //                                

                IsPaiting = true;                

                TheCanvas.Children.Clear();

                foreach (DiagramItemBase e in Items)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();

                ClearItems();

                Width = GraphUtil.GetDoubleValue(Vertex.Get("SizeX:"));
                Height = GraphUtil.GetDoubleValue(Vertex.Get("SizeY:"));

                Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));

                foreach (IEdge ie in Vertex.GetAll("Item:"))
                    AddItem(ie.To);

                UpdateLayout(); // here


                AddLineObjects();

                SelectionArea = new Rectangle(); // use old one
                TheCanvas.Children.Add(SelectionArea);

                SelectionArea.Stroke = (Brush)FindResource("0HighlightBrush");
                SelectionArea.StrokeDashArray=new DoubleCollection(new double[]{3,3});

                HideSelectionArea();

               

                SelectWrappersForSelectedVertexes();

                IsFirstPainted = true;

                IsPaiting = false;

                CheckAndUpdateDiagramLines();

                // turn on Vertex.Change listener

                VertexChangeListenOn();

                //

             
            }
        }

        public void SetFocus()
        {
            this.Focusable = true;
            //this.Focus();

            Keyboard.Focus(this);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            SetFocus();

            PaintDiagram();

            if (IsFirstPainted)
                this.Loaded -= OnLoad;
        }        

        public Diagram()
        {
            MinusZero mz = MinusZero.Instance;

            Items = new List<DiagramItemBase>();

            TheCanvas = new Canvas();

            TheCanvas.Background = (Brush)FindResource("0BackgroundBrush");

            this.Child = TheCanvas;

            this.BorderThickness = new Thickness(1);

            this.BorderBrush = (Brush)FindResource("0LightGrayBrush");


            this.AllowDrop = true;

            if (mz != null && mz.IsInitialized)
            {                       
                this.ContextMenu = new m0ContextMenu(this);
            }

            this.Loaded += new RoutedEventHandler(OnLoad);
            this.MouseMove += MouseMoveHandler;
            this.MouseLeave+=MouseLeaveHandler;
            this.MouseLeftButtonDown += MouseButtonDownHandler;
            this.MouseLeftButtonUp+=MouseButtonUpHandler;
            this.Drop += dndDrop;

            this.KeyDown += Diagram_KeyDown;                      
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
            if (Vertex.GetAll(@"SelectedEdges:\").Count() == 0)
                return;

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();
            info.Value = "DELETE diagram item / vertex";

            IVertex options = m0.MinusZero.Instance.CreateTempVertex();

            IVertex optionDiagramItemDelete = options.AddVertex(null, "Diagram only delete");
            IVertex optionLocalEdgeDelete = options.AddVertex(null, "Edge delete");
            IVertex optionAllEdgesDelete = options.AddVertex(null, "Remove from repository");
            IVertex optionCancel = options.AddVertex(null, "Cancel");

            IVertex option = MinusZero.Instance.DefaultShow.SelectDialogButton(info, options, null);

            if (option == null || option == optionCancel)
                return;

            IList<IEdge> selectedEdges = GeneralUtil.CreateAndCopyList<IEdge>(Vertex.GetAll(@"SelectedEdges:\"));

            UnselectAllSelectedEdges();

            foreach (IEdge e in selectedEdges)
                foreach (DiagramItemBase i in GetItemsDictionary()[e.To.Get("To:")])
                {  // what about multiple items for same BaseEdge:\To: ?

                    if (option == optionDiagramItemDelete)
                    {
                        GraphUtil.DeleteEdgeByToVertex(Vertex, i.Vertex);
                        RemoveItem(i);
                    }

                    if (option == optionLocalEdgeDelete)
                    {
                        GraphUtil.DeleteEdgeByToVertex(Vertex, i.Vertex);
                        RemoveItem(i);
                        VertexOperations.DeleteOneEdge(i.Vertex.Get(@"BaseEdge:\From:"), i.Vertex.Get(@"BaseEdge:\Meta:"),i.Vertex.Get(@"BaseEdge:\To:"));
                    }

                    if (option == optionAllEdgesDelete)
                    {
                        GraphUtil.DeleteEdgeByToVertex(Vertex, i.Vertex);
                        RemoveItem(i);
                        VertexOperations.DeleteAllEdges(i.Vertex.Get(@"BaseEdge:\To:"));
                    }

                }
        }

        private void DeleteLine()
        {
            bool onlyEdge = true;

            if (selectedLine.Vertex.Get(@"BaseEdge:\Meta:\$VertexTarget:") != null)
                onlyEdge = false;

            IVertex info = m0.MinusZero.Instance.CreateTempVertex();

            if (onlyEdge)
                info.Value = "DELETE line's edge";
            else
                info.Value = "DELETE line's vertex";

            IVertex options = m0.MinusZero.Instance.CreateTempVertex();

            options.AddVertex(null, "Yes");
          
            IVertex optionCancel = options.AddVertex(null, "Cancel");

            IVertex option = MinusZero.Instance.DefaultShow.SelectDialogButton(info, options, null);

            if (option == optionCancel)
                return;

            selectedLine.FromDiagramItem.RemoveDiagramLine(selectedLine);

            if (onlyEdge)
            {             
                GraphUtil.DeleteEdge(selectedLine.FromDiagramItem.Vertex.Get(@"BaseEdge:\To:"), 
                    selectedLine.Vertex.Get(@"BaseEdge:\Meta:"),
                    selectedLine.Vertex.Get(@"BaseEdge:\To:"));
            }
            else
            {
                GraphUtil.DeleteEdge(selectedLine.Vertex.Get(@"BaseEdge:\To:"),
               MinusZero.Instance.Root.Get(@"System\Meta\Base\Vertex\$EdgeTarget"),
               selectedLine.Vertex.Get(@"BaseEdge:\To:\$EdgeTarget:"));

                GraphUtil.DeleteEdge(selectedLine.FromDiagramItem.Vertex.Get(@"BaseEdge:\To:"),
                  selectedLine.Vertex.Get(@"BaseEdge:\Meta:"),
                  selectedLine.Vertex.Get(@"BaseEdge:\To:"));
            }
        }

        protected void MouseButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            SelectionAreaLeft = (int)e.GetPosition(TheCanvas).X;
            SelectionAreaTop = (int)e.GetPosition(TheCanvas).Y;

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

                CreatedDiagramLine.X1 = Canvas.GetLeft(ClickedItem)+ClickedItem.ActualWidth;
                CreatedDiagramLine.Y1 = Canvas.GetTop(ClickedItem);
            }

            CreatedDiagramLine.X2= ToX;
            CreatedDiagramLine.Y2=ToY;

            Point p = new Point(ToX, ToY);            

            foreach (DiagramItemBase i in Items)
            {
                if (VisualTreeHelper.HitTest(i, TranslatePoint(p, i)) != null)
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

        void AddOrMoveMultiSelectionMovingSprites(double x,double y)
        {
            if (IsMultiSelectionMoving == false)
            {
                IsMultiSelectionMoving = true;

                MovingSprites.Clear();

                foreach (IEdge ed in Vertex.GetAll(@"SelectedEdges:\"))
                    foreach (DiagramItemBase item in GetItemsByBaseEdge(ed.To))
                    {
                        double rx = Canvas.GetLeft(item);
                        double ry = Canvas.GetTop(item);
                        double rwidth = item.ActualWidth;
                        double rheight = item.ActualHeight;

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
                    Canvas.SetLeft(r, GraphUtil.GetDoubleValue(((DiagramItemBase)r.Tag).Vertex.Get(@"PositionX:"))+x);
                    Canvas.SetTop(r, GraphUtil.GetDoubleValue(((DiagramItemBase)r.Tag).Vertex.Get(@"PositionY:")) + y);                 
                }
            }
        }

        void RemoveMultiSelectionMovingSprites(double x, double y)
        {
            IsMultiSelectionMoving = false;

            foreach (Rectangle r in MovingSprites)
                TheCanvas.Children.Remove(r);

            foreach (IEdge ed in Vertex.GetAll(@"SelectedEdges:\"))
                foreach (DiagramItemBase item in GetItemsByBaseEdge(ed.To))
                    item.MoveItem(GraphUtil.GetDoubleValue(item.Vertex.Get("PositionX:")) + x,
                        GraphUtil.GetDoubleValue(item.Vertex.Get("PositionY:")) + y);
        }

        protected void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ClickTarget == ClickTargetEnum.AnchorLeftTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                        (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem)),                       
                       ClickedItem.ActualHeight - ( (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates) - Canvas.GetTop(ClickedItem)) );                        
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleTop)
                {
                    ClickedItem.MoveAndResizeItem(
                        Canvas.GetLeft(ClickedItem),
                        (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates),
                        ClickedItem.ActualWidth,
                       ClickedItem.ActualHeight - ((e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates) - Canvas.GetTop(ClickedItem)));
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightTop_CreateDiagramLine)
                {                    
                    CreateAndUpdateDiagramLine(e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                }

                if (ClickTarget == ClickTargetEnum.AnchorLeftMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                        Canvas.GetTop(ClickedItem),
                        ClickedItem.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem)),
                       ClickedItem.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightMiddle)
                {
                    ClickedItem.MoveAndResizeItem(
                        Canvas.GetLeft(ClickedItem),
                        Canvas.GetTop(ClickedItem),
                        e.GetPosition(TheCanvas).X - Canvas.GetLeft(ClickedItem) - ClickPositionX_AnchorCordinates,
                       ClickedItem.ActualHeight);
                }

                if (ClickTarget == ClickTargetEnum.AnchorLeftBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      (e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates),
                      Canvas.GetTop(ClickedItem),
                      ClickedItem.ActualWidth - ((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates) - Canvas.GetLeft(ClickedItem)),
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.AnchorMiddleBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      Canvas.GetLeft(ClickedItem),
                      Canvas.GetTop(ClickedItem),
                      ClickedItem.ActualWidth,
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.AnchorRightBottom)
                {
                    ClickedItem.MoveAndResizeItem(
                      Canvas.GetLeft(ClickedItem),
                      Canvas.GetTop(ClickedItem),
                      e.GetPosition(TheCanvas).X - Canvas.GetLeft(ClickedItem) - ClickPositionX_AnchorCordinates,
                    e.GetPosition(TheCanvas).Y - Canvas.GetTop(ClickedItem) - ClickPositionY_AnchorCordinates);
                }

                if (ClickTarget == ClickTargetEnum.Selection) // selection
                {
                    SetSelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                    //SelectItemsBySelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);
                    // too slow
                }

                if (ClickTarget == ClickTargetEnum.Item) // item move
                {
                    if ((Vertex.GetAll(@"SelectedEdges:\").Count() > 0 && ClickedItem.IsSelected == false) ||
                        Vertex.GetAll(@"SelectedEdges:\").Count() > 1)
                    {
                        if (ClickedItem.IsSelected == false)
                            ClickedItem.AddToSelectedEdges();

                        AddOrMoveMultiSelectionMovingSprites(e.GetPosition(ClickedItem).X - ClickPositionX_ItemCordinates,
                            e.GetPosition(ClickedItem).Y - ClickPositionY_ItemCordinates);

                        /*foreach (IEdge ed in Vertex.GetAll(@"SelectedEdges:\"))
                            foreach (DiagramItemBase item in GetItemsByBaseEdge(ed.To))
                                item.MoveItem(GraphUtil.GetDoubleValue(item.Vertex.Get("PositionX:")) + (e.GetPosition(ClickedItem).X - ClickPositionX_ItemCordinates),
                                    GraphUtil.GetDoubleValue(item.Vertex.Get("PositionY:")) + (e.GetPosition(ClickedItem).Y - ClickPositionY_ItemCordinates));
                                    */
                    }
                    else
                    {                        
                        if (ClickedItem.IsSelected == false)
                        {
                            UnselectAllSelectedEdges();

                            ClickedItem.AddToSelectedEdges();
                        }

                        ClickedItem.MoveItem((e.GetPosition(TheCanvas).X - ClickPositionX_ItemCordinates), (e.GetPosition(TheCanvas).Y - ClickPositionY_ItemCordinates));
                    }

                }
            }else
            {
                CheckIfLineNeedsSelection(e.GetPosition(TheCanvas));
            }
        }

        DiagramLineBase prevSelected;
        DiagramLineBase selectedLine;

        public double lineSelectionDelta = 10;

        private void CheckIfLineNeedsSelection(Point p)
        {
            double best = 999999;
            DiagramLineBase bestLine = null;

            foreach(DiagramItemBase i in Items)
                foreach(DiagramLineBase line in i.DiagramLines)
                {
                    double len = line.GetMouseDistance(p);

                    if (len < best)
                    {
                        bestLine = line;
                        best = len;
                    }                    
                }

            if (best< lineSelectionDelta && bestLine != null) 
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
            MouseUpOrLeave(false,e);
        }

        protected void MouseButtonUpHandler(object sender, MouseButtonEventArgs e){
            MouseUpOrLeave(true,e);
        }

        protected void MouseUpOrLeave(bool IsUp, MouseEventArgs e)
        {
            SetFocus();

            if (ClickTarget == ClickTargetEnum.Item && IsMultiSelectionMoving)
                RemoveMultiSelectionMovingSprites(e.GetPosition(ClickedItem).X - ClickPositionX_ItemCordinates,
                            e.GetPosition(ClickedItem).Y - ClickPositionY_ItemCordinates);

            if (ClickTarget == ClickTargetEnum.Selection)
            {
                SelectItemsBySelectionArea(SelectionAreaLeft, SelectionAreaTop, e.GetPosition(TheCanvas).X, e.GetPosition(TheCanvas).Y);

                HideSelectionArea();
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

        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get("ZoomVisualiserContent:"))) / 100;

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
            foreach (DiagramItemBase i in Items)
                i.Unselect();
        }

        public void UnselectAllSelectedEdges()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            TurnOffSelectedEdgesFireChange();

            GraphUtil.RemoveAllEdges(sv);

            TurnOnSelectedEdgesFireChange();

            SelectedVertexesUpdated();
        }

        protected void SelectedVertexesUpdated()
        {
            if (IsFirstPainted)
            {
                UnselectAll();

                SelectWrappersForSelectedVertexes();
            }
        }

        protected void SelectWrappersForSelectedVertexes()
        {
            IVertex sv = Vertex.Get("SelectedEdges:");

            foreach (IEdge e in sv)
                if(GetItemsDictionary().ContainsKey(e.To.Get("To:")))
                foreach (DiagramItemBase i in GetItemsDictionary()[e.To.Get("To:")])
                    i.Select();

            /*if (i.Vertex.Get(@"BaseEdge:\To:") == e.To.Get("To:"))
                i.Select();    */
        }

        public void VertexChange(object sender, VertexChangeEventArgs e)
        {            
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
            { SelectedVertexesUpdated(); return; }

            if ((sender == Vertex.Get("SelectedEdges:")) && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
            { SelectedVertexesUpdated(); return; }

            if (sender is IVertex && GraphUtil.FindEdgeByToVertex(Vertex.GetAll(@"SelectedEdges:\"), (IVertex)sender) != null)
            { SelectedVertexesUpdated(); return; }

            if (sender == Vertex.Get("ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
            { ChangeZoomVisualiserContent(); return; }

            if ((sender == Vertex.Get("SizeX:") || sender == Vertex.Get("SizeY:")) && e.Type == VertexChangeType.ValueChanged)
            { PaintDiagram(); return; }   
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                MinusZero mz = MinusZero.Instance;

                if (_Vertex != null)
                {
                    GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(@"System\Session\Visualisers"), Vertex);

                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));
                }

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                //mz.Root.Get(@"System\Session\Visualisers").AddEdge(null, Vertex);

                PaintDiagram();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                MinusZero mz = MinusZero.Instance;

                IsDisposed = true;

                foreach (DiagramItemBase e in Items)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();
                
                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(@"System\Session\Visualisers"), Vertex);

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                //if (Vertex is IDisposable) HELLO NO - Vertex stayes!!! this is not normal Visualiser where its Vertex disapears @ Dispose
                 //   ((IDisposable)Vertex).Dispose();
                
            }
        }


        // IHasLocalizableEdges

        private IVertex vertexByLocationToReturn;

        public IVertex GetEdgeByLocation(Point p)
        {
            vertexByLocationToReturn = null;

            foreach(DiagramItemBase i in Items)
            {
                if (VisualTreeHelper.HitTest(i, TranslatePoint(p, i)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    //Edge.AddEdgeEdgesOnlyTo(v, i.Vertex.Get(@"BaseEdge:\To:"));
                    Edge.AddEdgeEdges(v,i.Vertex.Get(@"BaseEdge:\From:"),i.Vertex.Get(@"BaseEdge:\Meta:"), i.Vertex.Get(@"BaseEdge:\To:"));
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

            NewDiagramItem ndi = new NewDiagramItem(vv, isSet, UIWpf.GetMousePositionDnd(e));

            if (ndi.DiagramItemDefinition != null)
            {
                if (ndi.InstanceOfMeta)
                {
                    IEdge ve = VertexOperations.AddInstanceAndReturnEdge(Vertex.Get("CreationPool:"), ndi.BaseEdge.Get("To:"));
                    IVertex v = ve.To;

                    v.Value = ndi.InstanceValue;

                    bool? ForceShowEditForm = null;

                    if (ndi.DiagramItemDefinition.Get(@"ForceShowEditForm:") != null)
                    {
                        if (GeneralUtil.CompareStrings(ndi.DiagramItemDefinition.Get(@"ForceShowEditForm:"), "True"))
                            ForceShowEditForm = true;

                        if (GeneralUtil.CompareStrings(ndi.DiagramItemDefinition.Get(@"ForceShowEditForm:"), "False"))
                            ForceShowEditForm = false;
                    }

                    if (ForceShowEditForm.HasValue == true && ForceShowEditForm == true)
                        MinusZero.Instance.DefaultShow.EditDialog(ve.To, UIWpf.GetMousePositionDnd(e));

                  
                    AddDiagramItem(x,
                                   y,
                                   ndi.DiagramItemDefinition,
                                   ndi.BaseEdge.Get("To:"), v);
                }
                else
                {
                    bool ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = false;
                    bool ThereIsDiagramItemOfThisBaseEdgeTo = false;

                    IVertex DiagramItemOfThisDiagramItemDefinition = Vertex.GetAll(@"Item:{Definition:" + ndi.DiagramItemDefinition.Value + "}");

                    foreach (IEdge ee in DiagramItemOfThisDiagramItemDefinition)
                        if (ee.To.Get(@"BaseEdge:\To:") == ndi.BaseEdge.Get("To:"))
                            ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo = true;

                    if(GetItemsDictionary().ContainsKey(ndi.BaseEdge.Get("To:")))
                    foreach (DiagramItemBase b in GetItemsDictionary()[ndi.BaseEdge.Get("To:")])
                        ThereIsDiagramItemOfThisBaseEdgeTo = true;

                    /*if (b.Vertex.Get(@"BaseEdge:\To:") == ndi.BaseEdge.Get("To:"))
                        ThereIsDiagramItemOfThisBaseEdgeTo = true;*/


                    if (ThereIsDiagramItemOfThisClassAndThisBaseEdgeTo == false)
                    {
                        if (ThereIsDiagramItemOfThisBaseEdgeTo == false ||
                            GeneralUtil.CompareStrings(r.Get(@"User\CurrentUser:\Settings:\AllowManyDiagramItemsForOneVertex:").Value, "True"))
                        {
                            AddDiagramItem(x,
                                        y,
                                        ndi.DiagramItemDefinition,
                                        ndi.BaseEdge);
                        }
                        else
                            m0.MinusZero.Instance.DefaultShow.ShowInfo("There is allready diagram item, that visualises dropped vertex.\n\nNow, it is not possible to add second representation of same vertex.\n\nOne can change this limitation by changing \"User\\CurrentUser:\\Settings:\\AllowManyDiagramItemsForOneVertex:\" setting.");
                    }
                    else
                        m0.MinusZero.Instance.DefaultShow.ShowInfo("There is allready \"" + ndi.DiagramItemDefinition.Value + "\" diagram item, that visualises dropped vertex.\n\nIt is not possible to add second representation of same vertex, with the same diagram item type.");
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex r=m0.MinusZero.Instance.Root;

                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                double x = e.GetPosition(TheCanvas).X, y = e.GetPosition(TheCanvas).Y;

                bool isSet = false;

                if (dndVertex.Count() > 1)
                    isSet = true;

                if(isSet)
                    Process.UI.NonAtomProcess.StartNonAtomProcess();


                VertexChangeListenOff();

                foreach (IEdge eee in dndVertex)
                {
                    AddDiagramItemDialog(x,y, eee.To,isSet,e);
                    x += 25;
                    y += 25;
                }

                CheckAndUpdateDiagramLines();

                VertexChangeListenOn();

                UpdateLayout();

                

                if (isSet)
                    Process.UI.NonAtomProcess.StopNonAtomProcess();

                if (e.Data.GetData("DragSource") is IHasSelectableEdges)
                    ((IHasSelectableEdges)e.Data.GetData("DragSource")).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges(dndVertex);
            }
        }

        private IVertex AddDiagramItem_Base(double x,double y, IVertex DiagramItemDefinition){
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex v = VertexOperations.AddInstance(Vertex, DiagramItemDefinition.Get("DiagramItemClass:"), r.Get(@"System\Meta\Visualiser\Diagram\Item"));

            GraphUtil.SetVertexValue(v, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\PositionX"), x);
            GraphUtil.SetVertexValue(v, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\PositionY"), y);

            GraphUtil.CreateOrReplaceEdge(v, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\Definition"), DiagramItemDefinition);

            //GraphUtil.CreateOrReplaceEdge(v, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), null);

            v.AddVertex(r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), null);

            if (v.Get(@"Definition:\DiagramItemVertex:") != null)
                AddEdgesFromDefintion(v, v.Get(@"Definition:\DiagramItemVertex:"));

            return v;
        }

        public void AddDiagramItem(double x,double y, IVertex DiagramItemDefinition, IVertex BaseEdge){
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex v = AddDiagramItem_Base(x, y, DiagramItemDefinition);
            
            GraphUtil.CreateOrReplaceEdge(v, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), BaseEdge);

            AddItem(v);            
        }

        public void AddDiagramItem(double x, double y, IVertex DiagramItemDefinition, IVertex metaVertex,IVertex newVertex)
        {
            IVertex v = AddDiagramItem_Base(x, y, DiagramItemDefinition);

            IVertex be = v.Get("BaseEdge:");

            Edge.AddEdgeEdgesOnlyMetaTo(be, metaVertex, newVertex);

            AddItem(v);

         //   CheckAndUpdateDiagramLines();
        }

        public void CheckAndUpdateDiagramLines()
        {
            foreach(DiagramItemBase item in Items)
                CheckAndUpdateDiagramLinesForItem(item);
        }

        public void CheckAndUpdateDiagramLinesForItem(DiagramItemBase item)
        {
            IEnumerable<IEdge> edges;

            if (item.Vertex.Get(@"Definition:\DoNotShowInherited:True") != null)
                edges = item.Vertex.Get(@"BaseEdge:\To:").OutEdgesRaw;
            else
                edges = item.Vertex.Get(@"BaseEdge:\To:");

            foreach (IEdge e in item.Vertex.Get(@"BaseEdge:\To:"))
            {
                List<DiagramItemBase> toDiagramItems = null;


                bool needAdding = true;

                if(item.GetDiagramLinesBaseEdgeToDictionary().ContainsKey(e.To))
                foreach (DiagramLineBase l in item.GetDiagramLinesBaseEdgeToDictionary()[e.To])
                    if (l.Vertex.Get(@"BaseEdge:\Meta:") == e.Meta)
                        needAdding = false;
                        
               // foreach (IEdge ee in item.Vertex.GetAll("DiagramLine:")) ////////////// OOO
                 //   if (ee.To.Get(@"BaseEdge:\Meta:") == e.Meta && ee.To.Get(@"BaseEdge:\To:") == e.To)
                 //       needAdding = false;

                if (needAdding) {
                    toDiagramItems = GetItemsByBaseEdgeTo_ForLines(e);

                    foreach (DiagramItemBase toDiagramItem in toDiagramItems)
                    {
                        IVertex lineDef = GetLineDefinition(e, item.Vertex, toDiagramItem);

                        if (lineDef != null)
                        {
                            bool canAdd = true;

                           // if (item.Vertex.Get(@"Definition:\DoNotShowInherited:True") != null)
                            //    if (VertexOperations.IsInheritedEdge(item.Vertex.Get(@"BaseEdge:\To:"), e.Meta))
                             //       canAdd = false;
                             //
                             // done upper. left to check if done correctly

                            if (canAdd)
                                item.AddDiagramLineVertex(e, lineDef, toDiagramItem);
                        }

                    }
                }           
            }
        }

        protected List<DiagramItemBase> GetItemsByBaseEdgeTo_ForLines(IEdge toEdge) // MAX TOO
        {
            List<DiagramItemBase> r = new List<DiagramItemBase>();

            if(GetItemsDictionary().ContainsKey(toEdge.To))
            foreach (DiagramItemBase i in GetItemsDictionary()[toEdge.To])
                r.Add(i);

            if (toEdge.Meta.Get("$VertexTarget:") != null && toEdge.To.Get("$EdgeTarget:")!=null)
            if(GetItemsDictionary().ContainsKey(toEdge.To.Get("$EdgeTarget:")))
            foreach (DiagramItemBase i in GetItemsDictionary()[toEdge.To.Get("$EdgeTarget:")])
                r.Add(i);

            /*foreach (DiagramItemBase i in Items)
            {
                if (i.Vertex.Get(@"BaseEdge:\To:") == toEdge.To)
                    r.Add(i);

                if (toEdge.Meta.Get("$VertexTarget:") != null && toEdge.To.Get("$EdgeTarget:") == i.Vertex.Get(@"BaseEdge:\To:"))
                    r.Add(i);
            }*/

            return r;
        }

        public IVertex GetLineDefinition(IEdge e,IVertex Vertex, DiagramItemBase toItem){
            if (GeneralUtil.CompareStrings(Vertex.Get("Definition:"), "Vertex")) // Vertex / Edge
                return Vertex.Get(@"Definition:\DiagramLineDefinition:Edge");

            foreach (IEdge def in Vertex.GetAll(@"Definition:\DiagramLineDefinition:"))
            {
                bool canReturn=true;

                if(def.To.Get("EdgeTestQuery:")!=null){
                    canReturn=false;

                    foreach (IEdge toTest in Vertex.GetAll(@"BaseEdge:\To:\" + def.To.Get("EdgeTestQuery:")))
                        if (toTest.To == e.Meta)
                            canReturn = true;
                }

                if (canReturn && def.To.Get("ToDiagramItemTestQuery:") != null && toItem.Vertex.Get((string)def.To.Get("ToDiagramItemTestQuery:").Value) != null)
                    return def.To;
            }

            return null;           
        }

    }
}
