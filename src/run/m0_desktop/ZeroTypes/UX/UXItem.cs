using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf;
using m0.UIWpf.UX;
using m0.User.Process.UX;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static m0.Graph.ExecutionFlow.ExecutionFlowHelper;

namespace m0.ZeroTypes.UX
{
    public class CursorAndClickTarget
    {
        public Cursor Cursor;
        public ClickTargetEnum ClickTarget;
        public IUXItem SubItem;
    }

    public class UXItem : UserControl, IUXItem, IPlatformClass
    {
        public bool ForceVertexChangeOff { get; set; } = false;

        public virtual string[] SubVertexesTriggeringItemVisualUpdate { get; }

        //

        public IEdge ContainerEdge { get; set; }

        public int NestingLevel { get; set; }

        public IItem ParentItem { get; set; }

        public List<FrameworkElement> Anchors;

        IEdge graphChangeListenerEdge;

        protected double AnchorSize = 11;

        //

        public IUXVisualiser OwningVisualiser { get; set; } // ParentAggregator

        public bool IsSelected { get; set; }

        public bool IsHighlighted { get; set; }

        public List<ILineDecoratorBase> DiagramToLines { get; } = new List<ILineDecoratorBase>();

        public List<ILineDecoratorBase> DiagramToAsMetaLines { get; } = new List<ILineDecoratorBase>();

        public UXItem(IEdge _edge): this(_edge, false)
        {
        }

        public UXItem(IEdge _edge, bool noAnchors)
        {
            NestingLevel = 0;

            Edge = _edge;

            vertex = _edge.To;

            TypedEdge.vertexDictionary.Add(this.Edge.To, this);

            //

            if (!noAnchors)
            {
                Anchors = new List<FrameworkElement>();

                //this.SizeChanged += DiagramItemBase_SizeChanged;

                this.MouseEnter += UXItem_MouseEnter;

                this.MouseLeave += UXItem_MouseLeave;

                this.MouseLeftButtonDown += MouseLeftButtonDownHandler;
            }

            this.SizeChanged += UXItem_SizeChanged;
        }

        private void UXItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            UpdateDiagramLines();
        }

        protected Brush GetBackgroundBrush()
        {
            Color backgroundColor = BackgroundColor;

            if (backgroundColor != null)
                return backgroundColor.GetBrush();
            else
                return (Brush)FindResource("0BackgroundBrush");
        }

        protected Brush GetForegroundBrush()
        {
            Color foregroundColor = ForegroundColor;

            if (foregroundColor != null)
                return foregroundColor.GetBrush();
            else
                return (Brush)FindResource("0ForegroundBrush");
        }

        protected Brush GetBorderBrush()
        {
            Color borderColor = BorderColor;

            if (borderColor != null)
                return borderColor.GetBrush();
            else
                return (Brush)FindResource("0ForegroundBrush");
        }


        // PUBLIC

        public virtual void VertexSetedUp()
        {
            graphChangeListenerEdge = ExecutionFlowHelper.AddTriggerAndListener(Vertex,
                new List<string> { @"", @"\" },
                new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                         GraphChangeFilterEnum.OutputEdgeAdded,
                         GraphChangeFilterEnum.OutputEdgeRemoved,
                         GraphChangeFilterEnum.OutputEdgeDisposed},
                "UXItem",
                VertexChange);

            BaseEdgeToUpdated();

            ViewAttributesUpdated();
        } // to be called after Vertex is setted up

        public bool IsDisposed = false;

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                foreach (ITypedEdge e in Items)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();

                foreach (ITypedEdge e in VolatileItems)
                {
                    Vertex.DeleteEdge(e.Edge);

                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();
                }

                foreach (IUXItem e in Decorators)
                    if (e is IDisposable)
                        ((IDisposable)e).Dispose();

                GraphChangeTrigger.RemoveListener(graphChangeListenerEdge);

                TypedEdge.RemoveFromDictionary(this);
            }
        }

        public Dictionary<IUXItem, List<ILineDecoratorBase>> GetDiagramLinesToDiagramItemDictionary()
        {
            if (needRebuildDiagramLinesDictionary)
                RebuidDiagramLinesDictionary();

            return DiagramLinesToDiagramItemDictionary;
        }

        public Dictionary<IVertex, List<ILineDecoratorBase>> GetDiagramLinesBaseEdgeToDictionary()
        {
            if (needRebuildDiagramLinesDictionary)
                RebuidDiagramLinesDictionary();

            return DiagramLinesBaseEdgeToDictionary;
        }

        public virtual void RemoveFromCanvas()
        {
            if (ParentItem is IUXContainer && !(ParentItem is IUXMultiContainerItem))
            {
                IUXContainer PerentItem_UXContainer = (IUXContainer)ParentItem;

                PerentItem_UXContainer.Canvas.Children.Remove(this);
            }
            else
                OwningVisualiser.Canvas.Children.Remove(this);

            Unselect();

            foreach (IUXItem _l in Decorators)
                if (_l is ILineDecoratorBase)
                {
                    ILineDecoratorBase l = (ILineDecoratorBase)_l;

                    l.RemoveFromCanvas();
                }

            foreach (ILineDecoratorBase l in DiagramToLines)
                l.RemoveFromCanvas();
        }

        public void AddDiagramLineObject(IUXItem toItem, ILineDecoratorBase newline, bool AddDecoratorVertex)
        {
            if (toItem == null)
                return;

            newline.OwningVisualiser = this.OwningVisualiser;

            if (newline.UXTemplate != null && AddDecoratorVertex)
            {
                IVertex DecoratorVertex = ((UXDecoratorTemplate)newline.UXTemplate).DecoratorVertex;                

                UXVisualiser.AddEdgesFromDefintion(newline.Vertex, DecoratorVertex);
            }

            newline.FromDiagramItem = this;

            newline.ToItem = toItem;

            newline.AddToCanvas();

            needRebuildDiagramLinesDictionary = true;

            toItem.DiagramToLines.Add(newline);

            UpdateDiagramLines(toItem);
        }

        public void RemoveDiagramLine(ILineDecoratorBase line)
        {
            needRebuildDiagramLinesDictionary = true;

            line.ToItem.DiagramToLines.Remove(line);

            line.RemoveFromCanvas();

            Vertex.DeleteEdge(line.Edge);
        }

        public virtual void Select()
        {
            if (IsSelected)
                return;

            IsSelected = true;

            GeneralUtil.SetPropertyIfPresent(this.Content, "Foreground", GetBackgroundBrush());

            Panel.SetZIndex(this, 99999);

            Point thisLeftTop = TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

            double left = thisLeftTop.X; // Canvas.GetLeft(this);
            double top = thisLeftTop.Y; //Canvas.GetTop(this);
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

            GeneralUtil.SetPropertyIfPresent(this.Content, "Foreground", GetForegroundBrush());

            Panel.SetZIndex(this, 0);

            foreach (UIElement e in Anchors)
                OwningVisualiser.Canvas.Children.Remove(e);

            Anchors.Clear();
        }

        public virtual void Highlight()
        {
            IsHighlighted = true;

            Panel.SetZIndex(this, 99998);
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

        public void MoveItem(double x, double y, bool onlyAnchors)
        {
            if (Position == null)
                return;

            if (!(ParentItem is IUXVisualiser) && ParentItem != null && !onlyAnchors)
            {
                Point localCanvasPosition = localCanvasPosition = OwningVisualiser.Canvas.TranslatePoint(new Point(x, y), ((IUXContainer)ParentItem).Canvas);

                x = localCanvasPosition.X;
                y = localCanvasPosition.Y;
            }

            Position position = this.Position;

            double deltax = position.X - x;
            double deltay = position.Y - y;

            if (!onlyAnchors)
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                //////////////////////////////////////// 

                position.X = x;
                position.Y = y;

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////             

                Canvas.SetLeft(this, x);
                Canvas.SetTop(this, y);
            }
            else
            {
                deltax = -x;
                deltay = -y;
            }

            foreach (UIElement a in Anchors)
            {
                Canvas.SetLeft(a, Canvas.GetLeft(a) - deltax);
                Canvas.SetTop(a, Canvas.GetTop(a) - deltay);
            }

            UpdateLayout();

            UpdateDiagramLinesInSubItems();

            UpdateDiagramLines();

            OwningVisualiser.CheckAndUpdateItemParent(this, true);
        }

        static public IUXItem GetUXItem(IItem parent, ITypedEdge i)
        {            
            if (parent is IUXContainer)
            {
                if (i is IUXItem)
                    return (IUXItem)i;
            }
            else
            {
                if (i is IUXDecorator)
                {
                    throw new Exception("decorator as item");
                    return (IUXItem)i;
                }
            }

            return null;
        }

        public void UpdateDiagramLinesInSubItems()
        {
            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.UpdateDiagramLines();
            }
        }

        public void MoveAndResizeItem(double x_orginal, double y_orginal, double width, double height)
        {
            if (width < 0 || height < 0)
                return;

            double x = x_orginal;
            double y = y_orginal;

            if (!(ParentItem is IUXVisualiser) && ParentItem != null)
            {
                Point localCanvasPosition = OwningVisualiser.Canvas.TranslatePoint(new Point(x, y), ((IUXContainer)ParentItem).Canvas);

                x = localCanvasPosition.X;
                y = localCanvasPosition.Y;
            }

            Position position = this.Position;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            position.X = x;
            position.Y = y;

            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);

            Size size = Size;

            if (size == null)
                size = SizeCreate();

            size.Width = width;
            size.Height = height;

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 

            Width = width;
            Height = height;

            UpdateAnchors(x_orginal, y_orginal, width, height);

            //UpdateDiagramLines(); //On SizeChanged

            OwningVisualiser.CheckAndUpdateItemParent(this, true);
        }

        public void AddToSelectedEdges()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            //EdgeHelper.AddEdgeVertexEdgeByEdgeVertex(OwningVisualiser.Vertex.Get(false, "SelectedEdges:"), Vertex.Get(false, "BaseEdge:")); // we can have alg here that gets ShowSelectedEdgesBaseEdge:

            EdgeHelper.AddEdgeVertex(OwningVisualiser.Vertex.Get(false, "SelectedEdges:"), Edge.From, Edge.Meta, Edge.To);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        /////// NON PUBLIC:        

        protected virtual INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (ForceVertexChangeOff)
                return exe.Stack;

            bool do_ViewAttributesUpdated = false;

            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                bool trigger = false;

                foreach (string s in SubVertexesTriggeringItemVisualUpdate)
                    if (GraphUtil.ExistQueryIn(changedVertex, s, null))
                        trigger = true;

                if (trigger)
                    do_ViewAttributesUpdated = true;
            }

            //

            IVertex baseEdgeTo = BaseEdgeTo;

            if (IsVertexChange(exe.Stack, baseEdgeTo))
                BaseEdgeToUpdated();

            foreach (IVertex edgeVertex in GetEdgesRemovedFrom(exe.Stack, baseEdgeTo))
            {
                ILineDecoratorBase toRemove = null;

                foreach (IUXItem _l in Decorators)
                    if (_l is ILineDecoratorBase)
                    {
                        ILineDecoratorBase l = (ILineDecoratorBase)_l;

                        IEdge l_baseEdge = l.BaseEdge;
                        if (l_baseEdge.Meta == edgeVertex.Get(false, "Meta:") &&
                            l_baseEdge.To == edgeVertex.Get(false, "To:"))
                            toRemove = l;
                    }

                if (toRemove != null)
                    RemoveDiagramLine(toRemove);
            }

            if (IsEdgeAddedTo(exe.Stack, baseEdgeTo) && CanAutomaticallyAddEdges)
                OwningVisualiser.CheckAndUpdateDiagramLinesForItem(this);

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "BackgroundColor")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "ForegroundColor")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "BorderColor")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Red")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Green")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Blue")
                || IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Vertex, "Opacity"))
                do_ViewAttributesUpdated = true;

            if (IsEdgeAddedTo(exe.Stack, Vertex))
                do_ViewAttributesUpdated = true;

            if (do_ViewAttributesUpdated)
                ViewAttributesUpdated();

            return exe.Stack;
        }

        // OPTIMISATION START

        Dictionary<IUXItem, List<ILineDecoratorBase>> DiagramLinesToDiagramItemDictionary = new Dictionary<IUXItem, List<ILineDecoratorBase>>();
        Dictionary<IVertex, List<ILineDecoratorBase>> DiagramLinesBaseEdgeToDictionary = new Dictionary<IVertex, List<ILineDecoratorBase>>();

        bool needRebuildDiagramLinesDictionary = true;

        void ClearDiagramLines()
        {
            DiagramLinesToDiagramItemDictionary.Clear();
            DiagramLinesBaseEdgeToDictionary.Clear();

            needRebuildDiagramLinesDictionary = true;
        }

        void RebuidDiagramLinesDictionary()
        {
            DiagramLinesToDiagramItemDictionary.Clear();
            DiagramLinesBaseEdgeToDictionary.Clear();

            foreach (IUXItem _l in Decorators)
                if (_l is ILineDecoratorBase)
                {
                    ILineDecoratorBase l = (ILineDecoratorBase)_l;

                    // ToDiagramItem:

                    IUXItem toDiagramItem = l.ToItem;

                    if (DiagramLinesToDiagramItemDictionary.ContainsKey(toDiagramItem))
                        DiagramLinesToDiagramItemDictionary[toDiagramItem].Add(l);
                    else
                    {
                        List<ILineDecoratorBase> list = new List<ILineDecoratorBase>();
                        list.Add(l);

                        DiagramLinesToDiagramItemDictionary.Add(toDiagramItem, list);
                    }

                    // BaseEdge:\To:

                    IVertex BaseEdgeTo = l.BaseEdgeTo;

                    if (DiagramLinesBaseEdgeToDictionary.ContainsKey(BaseEdgeTo))
                        DiagramLinesBaseEdgeToDictionary[BaseEdgeTo].Add(l);
                    else
                    {
                        List<ILineDecoratorBase> list = new List<ILineDecoratorBase>();
                        list.Add(l);

                        DiagramLinesBaseEdgeToDictionary.Add(BaseEdgeTo, list);
                    }
                }

            needRebuildDiagramLinesDictionary = false;
        }

        // OPTIMISATION END

        public void AddAsToMetaLine(ILineDecoratorBase line) // metaextendedline uses that
        {
            DiagramToAsMetaLines.Add(line);
        }

        public virtual void BaseEdgeToUpdated() { }

        public virtual void ViewAttributesUpdated()
        {
            Size size = Size;

            if (size != null)
            {
                Width = size.Width;
                Height = size.Height;
            }

            //

            GeneralUtil.SetPropertyIfPresent(this.Content, "Foreground", GetForegroundBrush());

            //

            BorderBrush = GetBorderBrush();
        }

        protected void UpdateDiagramLines(IUXItem toItem)
        {
            if (toItem.OwningVisualiser == null)
                return;

            List<ILineDecoratorBase> sameToItemLines = new List<ILineDecoratorBase>();

            Dictionary<IUXItem, List<ILineDecoratorBase>> DiagramLinesToDiagramItemDictionary = GetDiagramLinesToDiagramItemDictionary();

            if (DiagramLinesToDiagramItemDictionary.ContainsKey(toItem))
                foreach (ILineDecoratorBase l in DiagramLinesToDiagramItemDictionary[toItem])
                    sameToItemLines.Add(l);

            List<ILineDecoratorBase> sameFromItemLinesTo = new List<ILineDecoratorBase>();

            Dictionary<IUXItem, List<ILineDecoratorBase>> toItemDiagramLinesToDiagramItemDictionary = toItem.GetDiagramLinesToDiagramItemDictionary();

            if (toItemDiagramLinesToDiagramItemDictionary.ContainsKey(this))
                foreach (ILineDecoratorBase l in toItemDiagramLinesToDiagramItemDictionary[this])
                    sameFromItemLinesTo.Add(l);

            int allCnt = sameToItemLines.Count() + sameFromItemLinesTo.Count();

            int cnt = 0;

            if (toItem == this)
                allCnt = allCnt / 2;

            foreach (ILineDecoratorBase l in sameToItemLines)
            {
                Point start = GetLineAnchorLocation(toItem, false, new Point(), allCnt, cnt, toItem == this);

                Point end = toItem.GetLineAnchorLocation(this, false, new Point(), allCnt, cnt, false);

                if (toItem == this)
                    l.SetPosition(start.X, start.Y, end.X, end.Y, true, Canvas.GetLeft(this) + this.ActualWidth + 25 * (allCnt - cnt), Canvas.GetTop(this) - 25 * ((allCnt - cnt)));
                else
                    l.SetPosition(start.X, start.Y, end.X, end.Y, false, 0, 0);

                cnt++;
            }

            foreach (ILineDecoratorBase l in sameFromItemLinesTo)
            {
                Point end = GetLineAnchorLocation(toItem, false, new Point(), allCnt, cnt, false);

                Point start = toItem.GetLineAnchorLocation(this, false, new Point(), allCnt, cnt, false);

                if (toItem != this)
                    l.SetPosition(start.X, start.Y, end.X, end.Y, false, 0, 0);

                cnt++;
            }
        }

        public void UpdateDiagramLines()
        {
            foreach (ILineDecoratorBase m in DiagramToAsMetaLines)
                m.UpdateMetaPosition();

            List<IUXItem> updatedItems = new List<IUXItem>();


            foreach (IUXItem _l in Decorators)
                if (_l is ILineDecoratorBase)
                {
                    ILineDecoratorBase l = (ILineDecoratorBase)_l;

                    if (!updatedItems.Contains(l.ToItem))
                    {
                        UpdateDiagramLines(l.ToItem);

                        updatedItems.Add(l.ToItem);
                    }
                }

            foreach (ILineDecoratorBase l in DiagramToLines)
                if (!updatedItems.Contains(l.FromDiagramItem))
                {
                    UpdateDiagramLines(l.FromDiagramItem);

                    updatedItems.Add(l.FromDiagramItem);
                }

            //

            foreach (ITypedEdge _i in Items)
            {
                IUXItem i = GetUXItem(this, _i);

                if (i == null)
                    continue;

                i.UpdateDiagramLines();
            }
        }

        public void HighlightThisAndAllConectedByDiagramLine()
        {
            Highlight();

            foreach (IUXItem _l in Decorators)
                if (_l is ILineDecoratorBase)
                {
                    ILineDecoratorBase l = (ILineDecoratorBase)_l;

                    l.Highlight();
                    l.ToItem.Highlight();
                }

            foreach (ILineDecoratorBase l in DiagramToLines)
            {
                l.Highlight();
                l.FromDiagramItem.Highlight();
            }
        }

        public void UnhighlightThisAndAllConectedByDiagramLine()
        {
            Unhighlight();

            foreach (IUXItem _l in Decorators)
                if (_l is ILineDecoratorBase)
                {
                    ILineDecoratorBase l = (ILineDecoratorBase)_l;

                    l.Unhighlight();
                    l.ToItem.Unhighlight();
                }

            foreach (ILineDecoratorBase l in DiagramToLines)
            {
                l.Unhighlight();
                l.FromDiagramItem.Unhighlight();
            }
        }

        private void UXItem_MouseLeave(object sender, MouseEventArgs e)
        {            
            if (OwningVisualiser.IsDrawingOrMovingLine == false && OwningVisualiser.IsSelecting == false)
                UnhighlightThisAndAllConectedByDiagramLine();
        }

        //private void UXItem_MouseEnter(object sender, MouseEventArgs e)
        public void UXItem_MouseEnter(object sender, MouseEventArgs e)
        {            
            OwningVisualiser.UnhighlightAllSelectedEdges();

            if (OwningVisualiser.IsDrawingOrMovingLine == false && OwningVisualiser.IsSelecting == false)
                HighlightThisAndAllConectedByDiagramLine();
        }

        private bool CanAutomaticallyAddEdges = true;

        public void RemoveFromSelectedEdges()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            GraphUtil.DeleteEdgeByToVertex(OwningVisualiser.Vertex.Get(false, "SelectedEdges:"), Vertex.Get(false, "BaseEdge:"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        protected void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {            
            OwningVisualiser.SetFocus();

            OwningVisualiser.ClickPositionX_ItemCordinates = e.GetPosition(this).X;
            OwningVisualiser.ClickPositionY_ItemCordinates = e.GetPosition(this).Y;
         
            OwningVisualiser.ClickTarget = ClickTargetEnum.Item;
            OwningVisualiser.ClickedItem = this;

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
                    OwningVisualiser.UnselectAllSelectedEdges();

                    AddToSelectedEdges();
                }
            }

            e.Handled = true;
        }

        protected void UpdateAnchor(ClickTargetEnum anchorType, double left, double top)
        {
            foreach (FrameworkElement r in Anchors)
                if (GetAnchorsClickTarget(r) == anchorType)
                {
                    Canvas.SetLeft(r, left);
                    Canvas.SetTop(r, top);

                    r.Width = AnchorSize;
                    r.Height = AnchorSize;
                }
        }

        protected FrameworkElement AddAnchor(ClickTargetEnum anchorType, double left, double top)
        {
            return AddAnchor(anchorType, left, top, null);
        }

        protected FrameworkElement AddAnchor(ClickTargetEnum anchorType, double left, double top, IUXItem subItem)
        {
            FrameworkElement r;            

            if (anchorType == ClickTargetEnum.AnchorRightTop_CreateDiagramLine
                || anchorType == ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine
                || anchorType == ClickTargetEnum.AnchorRightTop_MoveDiagramLine)
            {
                TextBox l = new TextBox();
                l.IsReadOnly = true;
                l.Focusable = false;
                l.BorderThickness = new Thickness(0);

                if (anchorType == ClickTargetEnum.AnchorRightTop_CreateDiagramLine
                    || anchorType == ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine)
                {
                    l.FontSize = 22;
                    l.Text = "*";
                    l.Padding = new Thickness(-2, -3.8, 0, 0);
                }

                if (anchorType == ClickTargetEnum.AnchorRightTop_MoveDiagramLine)
                {
                    l.Text = "*";
                    l.FontSize = 15;
                    l.Padding = new Thickness(-3.5, -3.7, 0, 0);

                    l.BorderBrush = (Brush)this.FindResource("0LightHighlightBrush");
                    l.BorderThickness = new Thickness(2);
                }

                l.FontFamily = new FontFamily("Times New Roman");

                l.Margin = new Thickness(0);

                l.Background = (Brush)FindResource("0SelectionBrush");
                l.Foreground = (Brush)FindResource("0BackgroundBrush");

                r = l;
            }
            else
            {
                r = new Rectangle();

                ((Rectangle)r).Fill = (Brush)FindResource("0SelectionBrush");
            }

            DecorateWithCursor(r);

            Canvas.SetLeft(r, left);
            Canvas.SetTop(r, top);

            r.Width = AnchorSize;
            r.Height = AnchorSize;

            SetAnchorsClickTarget(r, anchorType);

            if (subItem != null)
                SetAnchorsSubItem(r, subItem);


            Anchors.Add(r);

            r.MouseLeftButtonDown += AnchorMouseButtonDown;


            switch (anchorType)
            {
                case ClickTargetEnum.AnchorLeftTop:
                    SetAnchorsCursor(r, Cursors.SizeNWSE);
                    break;

                case ClickTargetEnum.AnchorMiddleTop:
                    SetAnchorsCursor(r, Cursors.SizeNS);
                    break;

                case ClickTargetEnum.AnchorRightTop_CreateDiagramLine:
                    SetAnchorsCursor(r, Cursors.Pen);
                    break;

                case ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine:
                    SetAnchorsCursor(r, Cursors.Pen);
                    break;

                case ClickTargetEnum.AnchorRightTop_MoveDiagramLine:
                    SetAnchorsCursor(r, Cursors.Pen);
                    break;

                case ClickTargetEnum.AnchorLeftMiddle:
                    SetAnchorsCursor(r, Cursors.SizeWE);
                    break;

                case ClickTargetEnum.AnchorRightMiddle:
                    SetAnchorsCursor(r, Cursors.SizeWE);
                    break;

                case ClickTargetEnum.AnchorLeftBottom:
                    SetAnchorsCursor(r, Cursors.SizeNESW);
                    break;

                case ClickTargetEnum.AnchorMiddleBottom:
                    SetAnchorsCursor(r, Cursors.SizeNS);
                    break;

                case ClickTargetEnum.AnchorRightBottom:
                    SetAnchorsCursor(r, Cursors.SizeNWSE);
                    break;
            }

            Panel.SetZIndex(r, 99999);

            OwningVisualiser.Canvas.Children.Add(r);

            return r;
        }

        //

        public FrameworkElement DecorateWithCursor(FrameworkElement e)
        {
            e.Tag = new CursorAndClickTarget();

            e.MouseEnter += R_MouseEnter;
            e.MouseLeave += R_MouseLeave;

            return e;
        }

        public void SetAnchorsCursor(FrameworkElement r, Cursor c)
        {
            ((CursorAndClickTarget)r.Tag).Cursor = c;
        }

        public void SetAnchorsClickTarget(FrameworkElement r, ClickTargetEnum c)
        {
            ((CursorAndClickTarget)r.Tag).ClickTarget = c;
        }

        public void SetAnchorsSubItem(FrameworkElement r, IUXItem i)
        {
            ((CursorAndClickTarget)r.Tag).SubItem = i;
        }

        public IUXItem GetAnchorsSubItem(FrameworkElement r)
        {
            return ((CursorAndClickTarget)r.Tag).SubItem;
        }

        public ClickTargetEnum GetAnchorsClickTarget(FrameworkElement r)
        {
            return ((CursorAndClickTarget)r.Tag).ClickTarget;
        }

        private void R_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            WpfUtil.SetCursor(Cursors.Arrow);
        }

        private void R_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement rectangle = (FrameworkElement)sender;

            WpfUtil.SetCursor(((CursorAndClickTarget)rectangle.Tag).Cursor);
        }

        //

        public void AnchorMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            OwningVisualiser.ClickPositionX_ItemCordinates = e.GetPosition(this).X;
            OwningVisualiser.ClickPositionY_ItemCordinates = e.GetPosition(this).Y;

            OwningVisualiser.ClickPositionX_AnchorCordinates = e.GetPosition((IInputElement)sender).X;
            OwningVisualiser.ClickPositionY_AnchorCordinates = e.GetPosition((IInputElement)sender).Y;

            FrameworkElement anchor = (FrameworkElement)sender;

            OwningVisualiser.ClickTarget = GetAnchorsClickTarget(anchor);            

            OwningVisualiser.ClickedAnchor = anchor;

            if (OwningVisualiser.ClickTarget == ClickTargetEnum.AnchorRightTop_SubItem_CreateDiagramLine)
                OwningVisualiser.ClickedItem = GetAnchorsSubItem(anchor);
            else
                OwningVisualiser.ClickedItem = this;

            e.Handled = true;
        }

        protected virtual void UpdateAnchors(double left, double top, double width, double height)
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

        bool IsInParentHierarchy(IItem toItem)
        {
            if (toItem == null)
                return false;

            if (ParentItem == toItem)
                return true;

            return IsInParentHierarchy(toItem.ParentItem);
        }

        public virtual Point GetLineAnchorLocation(IUXItem _toItem, bool useToPoint, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLineNumber, bool isSelfStart)
        {
            if (!(_toItem is FrameworkElement) || OwningVisualiser == null)
                return new Point();

            FrameworkElement toItem = (FrameworkElement)_toItem;

            //

            Point toItemLeftTop = new Point();

            Point thisLeftTop = new Point();


            toItemLeftTop = toItem.TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

            thisLeftTop = TranslatePoint(new Point(0, 0), OwningVisualiser.Canvas);

            //

            Point p = new Point();

            Point pTo = new Point();

            if (!useToPoint && toItem != null)
            {
                if (IsInParentHierarchy(_toItem))
                {
                    pTo.X = toItemLeftTop.X + toItem.ActualWidth / 2;
                    pTo.Y = toItemLeftTop.Y;
                }
                else
                {
                    pTo.X = toItemLeftTop.X + toItem.ActualWidth / 2;
                    pTo.Y = toItemLeftTop.Y + toItem.ActualHeight / 2;
                }
            }
            else
                pTo = toPoint;



            double tX = thisLeftTop.X + this.ActualWidth / 2;
            double tY = thisLeftTop.Y + this.ActualHeight / 2;

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
                        p.X = thisLeftTop.X + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                        p.Y = tY - this.ActualHeight / 2;

                        return p;
                    }
                    else
                    {
                        p.X = tX + this.ActualWidth / 2;
                        p.Y = thisLeftTop.Y + (((double)(toItemDiagramLinesCount - toItemDiagramLineNumber)) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);

                        return p;
                    }
                }

                if (testY <= 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = thisLeftTop.X + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY - this.ActualHeight / 2;
                }

                if (testY > 0 && Math.Abs(testX * this.ActualHeight) <= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = thisLeftTop.X + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualWidth);
                    p.Y = tY + this.ActualHeight / 2;
                }

                if (testX >= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX + this.ActualWidth / 2;
                    p.Y = thisLeftTop.Y + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
                }

                if (testX <= 0 && Math.Abs(testX * this.ActualHeight) >= Math.Abs(testY * this.ActualWidth))
                {
                    p.X = tX - this.ActualWidth / 2;
                    p.Y = thisLeftTop.Y + (((double)toItemDiagramLineNumber + 1) / ((double)toItemDiagramLinesCount + 1) * this.ActualHeight);
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
            IEdge sizeEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "Size", null);

            if (sizeEdge != null)
                Vertex.DeleteEdge(sizeEdge);

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
            IEdge positionEdge = GraphUtil.GetQueryOutFirstEdge(Vertex, "Position", null);

            if (positionEdge != null)
                Vertex.DeleteEdge(positionEdge);

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

                ITypedEdge _i = TypedEdge.Get(val);

                if (_i != null && _i is UXTemplate)
                    return (UXTemplate)_i;

                return null;                                
            }
            set
            {
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
                    ITypedEdge _i = TypedEdge.Get(e);

                    if (_i != null && _i is IUXItem)                    
                        ret.Add((IUXItem)_i);                    
                }

                return ret;
            }
        }

        public IUXItem AddDecorator(IVertex typeVertex)
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, typeVertex, Decorator_meta);

            ITypedEdge _i = TypedEdge.Get(newEdge);

            if (_i != null && _i is IUXItem)
                return (IUXItem)_i;

            return null;
        }

        public void RemoveDecorator(IUXItem decorator)
        {
            Vertex.DeleteEdge(decorator.Edge);
        }

        // Item

        static IVertex BaseEdge_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        static IVertex Item_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item");
        static IVertex VolatileItem_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\VolatileItem");        
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
            set
            {
                Edge baseEdge = BaseEdge;

                if (value.From != null)
                    baseEdge.From = value.From;

                if (value.Meta != null)
                    baseEdge.Meta = value.Meta;

                if (value.To != null)
                    baseEdge.To = value.To;
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
