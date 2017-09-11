using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Dialog;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0.UIWpf.Visualisers.Diagram
{
    public class DiagramItemBase : UserControl, IPlatformClass, IDisposable
    {
        public Diagram Diagram;

        public List<FrameworkElement> Anchors;

        public virtual IVertex Vertex { get; set; }

        public double LineWidth;

        public Brush BackgroundColor;

        public Brush ForegroundColor;

        public List<DiagramLineBase> DiagramLines = new List<DiagramLineBase>();

        protected List<DiagramLineBase> DiagramToLines = new List<DiagramLineBase>();

        protected List<DiagramLineBase> DiagramToAsMetaLines = new List<DiagramLineBase>();

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

                IVertex toDiagramItem = l.Vertex.Get(@"ToDiagramItem:");

                if (DiagramLinesToDiagramItemDictionary.ContainsKey(toDiagramItem))
                    DiagramLinesToDiagramItemDictionary[toDiagramItem].Add(l);
                else
                {
                    List<DiagramLineBase> list = new List<DiagramLineBase>();
                    list.Add(l);

                    DiagramLinesToDiagramItemDictionary.Add(toDiagramItem, list);
                }

               // BaseEdge:\To:

                    IVertex BaseEdgeTo= l.Vertex.Get(@"BaseEdge:\To:");

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

        public void RemoveFromCanvas()
        {
            Diagram.TheCanvas.Children.Remove(this);

            Unselect();

            foreach (DiagramLineBase l in DiagramLines)
                l.RemoveFromCanvas();

            foreach (DiagramLineBase l in DiagramToLines)
                l.RemoveFromCanvas();
        }


        public void AddAsToMetaLine(DiagramLineBase line)
        {
            DiagramToAsMetaLines.Add(line);
        }

        public virtual void VertexSetedUp() {
            PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges", "ForegroundColor", "BackgroundColor" });

            VisualiserUpdate();            
        } // to be called after Vertex is setted up

        public virtual void VisualiserUpdate()       {

            double sizeX = GraphUtil.GetDoubleValue(Vertex.Get("SizeX:"));
            double sizeY = GraphUtil.GetDoubleValue(Vertex.Get("SizeY:"));

            if (sizeX != GraphUtil.NullDouble && sizeY != GraphUtil.NullDouble)
            {
                Width = sizeX;
                Height = sizeY;
            }

            if(Vertex.Get("BackgroundColor:")!=null)
                BackgroundColor = UIWpf.GetBrushFromColorVertex(Vertex.Get("BackgroundColor:"));
            else
                BackgroundColor = (Brush)FindResource("0BackgroundBrush");

            if (Vertex.Get("ForegroundColor:") != null)
                ForegroundColor = UIWpf.GetBrushFromColorVertex(Vertex.Get("ForegroundColor:"));
            else
                ForegroundColor = (Brush)FindResource("0ForegroundBrush");

            if (GraphUtil.GetDoubleValue(Vertex.Get("LineWidth:")) != GraphUtil.NullDouble)
                LineWidth = GraphUtil.GetDoubleValue(Vertex.Get("LineWidth:"));

            SetBackAndForeground();
        }

        public virtual void SetBackAndForeground()
        {
            this.Foreground = ForegroundColor;

           // this.Background = BackgroundColor;
        }

        public bool IsSelected;

        public bool IsHighlighted;

        double AnchorSize = 11;

        public virtual Point GetLineAnchorLocation(DiagramItemBase toItem, Point toPoint, int toItemDiagramLinesCount, int toItemDiagramLinesNumber, bool isSelfStart){
            Point p = new Point();
           
            p.X = Canvas.GetLeft(this) + this.ActualWidth / 2;
            p.Y = Canvas.GetTop(this) + this.ActualHeight / 2;       

            return p;
        }

        public virtual void DoCreateDiagramLine(DiagramItemBase toItem)
        {
            IVertex toEdge=toItem.Vertex.Get("BaseEdge:");

            IVertex r=m0.MinusZero.Instance.Root;

            IVertex v = m0.MinusZero.Instance.CreateTempVertex();


            foreach (IEdge def in Vertex.GetAll(@"Definition:\DiagramLineDefinition:")) 
            {
                bool CreateEdgeOnly = false;

                if (GraphUtil.GetValueAndCompareStrings(def.To.Get("CreateEdgeOnly:"), "True"))
                    CreateEdgeOnly = true;
                                   
                foreach (IEdge e in Vertex.GetAll(@"BaseEdge:\To:\" + def.To.Get("EdgeTestQuery:")))
                {
                    bool canAdd = true;

                    if (def.To.Get("ToDiagramItemTestQuery:") != null && toItem.Vertex.Get((string)def.To.Get("ToDiagramItemTestQuery:").Value) == null)
                        canAdd = false;

                    if (e.To.Get(@"$EdgeTarget:") != null 
                        && !GeneralUtil.CompareStrings(e.To.Get(@"$EdgeTarget:").Value,"Vertex") // Vertexes do not have $Is:Vertex
                        && toEdge.Get(@"To:\$Is:" + (string)e.To.Get(@"$EdgeTarget:").Value) == null)
                        canAdd = false;

                    if (CreateEdgeOnly==false 
                        && e.To.Get(@"$VertexTarget:") != null 
                        && toEdge.Get(@"To:\$Is:" + (string)e.To.Get(@"$VertexTarget:").Value) == null)
                        canAdd = false;

                    if (canAdd)
                        AddNewLineOption(v, def, e);
                }

                if (GeneralUtil.CompareStrings(def.To.Value, "Edge"))// Vertex\Edge
                    foreach (IEdge e in r.Get(@"System\Meta\Base\Vertex"))
                        AddNewLineOption(v, def, e);

                if (GeneralUtil.CompareStrings(def.To.Get("EdgeTestQuery:"), "$EdgeTarget")) // $EdgeTarget is not present as there is no inheritance from Vertex
                    AddNewLineOption(v, def, GraphUtil.FindEdgeByToVertex(r.Get(@"System\Meta\Base\Vertex"),"$EdgeTarget"));
            }

                if (v.Count() == 0)
                    MinusZero.Instance.DefaultShow.ShowInfo("There is no diagram line definition matching selected source and target items.");

                IVertex info = m0.MinusZero.Instance.CreateTempVertex();
                info.Value = "choose diagram line:";

                
            Point mousePosition=UIWpf.GetMousePosition();

            IVertex a = MinusZero.Instance.DefaultShow.SelectDialog(info, v, mousePosition);

                if (a != null){
                    IVertex test = VertexOperations.TestIfNewEdgeValid(Vertex.Get(@"BaseEdge:\To:"), a.Get("OptionEdge:"), toEdge.Get("To:"));

                    if (test == null)
                    {
                        bool? ForceShowEditForm = null; // ForceShowEditForm

                        if (a.Get(@"OptionDiagramLineDefinition:\ForceShowEditForm:") != null)
                        {
                            if (GeneralUtil.CompareStrings(a.Get(@"OptionDiagramLineDefinition:\ForceShowEditForm:"), "True"))
                                ForceShowEditForm = true;

                            if (GeneralUtil.CompareStrings(a.Get(@"OptionDiagramLineDefinition:\ForceShowEditForm:"), "False"))
                                ForceShowEditForm = false;
                        }

                        bool CreateEdgeOnly = false; // CreateEdgeOnly

                        if (GraphUtil.GetValueAndCompareStrings(a.Get(@"OptionDiagramLineDefinition:\CreateEdgeOnly:"), "True"))
                           CreateEdgeOnly = true;


                        CanAutomaticallyAddEdges = false; // for VertexChange
                        IEdge edge = VertexOperations.AddEdgeOrVertexByMeta(Vertex.Get(@"BaseEdge:\To:"), a.Get("OptionEdge:"), toEdge.Get("To:"), mousePosition, CreateEdgeOnly, ForceShowEditForm);
                        CanAutomaticallyAddEdges = true; 

                        AddDiagramLineVertex(edge, a.Get(@"OptionDiagramLineDefinition:"), toItem);
                    }
                    else
                        MinusZero.Instance.DefaultShow.ShowInfo("Adding new diagram line  \"" + a.Value + "\" is not possible.\n\n" + test.Value);
                }
                 
            
        }

        private static void AddNewLineOption(IVertex v, IEdge def, IEdge e)
        {
            IVertex r = m0.MinusZero.Instance.Root;

            IVertex vv = v.AddVertex(null, e.To.Value + " (" + def.To.Value + ")");//def.To.Value + " for " + e.To.Value);

            vv.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\OptionEdge"), e.To);
            vv.AddEdge(r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\OptionDiagramLineDefinition"), def.To);
        }        

        public void AddDiagramLineVertex(IEdge edge, IVertex diagramLineDefinition, DiagramItemBase toItem)
        {
            IVertex r = MinusZero.Instance.Root;

            ((EasyVertex)Vertex).CanFireChangeEvent = false;

            IVertex l = VertexOperations.AddInstance(Vertex, diagramLineDefinition.Get("DiagramLineClass:"), r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemBase\DiagramLine"));

            GraphUtil.CreateOrReplaceEdge(l, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramLineBase\ToDiagramItem"), toItem.Vertex);

            GraphUtil.CreateOrReplaceEdge(l, r.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramLineBase\Definition"), diagramLineDefinition);
            
            Edge.CreateEdgeAndCreateOrReplaceEdgeByMeta(l, r.Get(@"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge"), edge);

            AddDiagramLineObject(toItem, l);

            ((EasyVertex)Vertex).CanFireChangeEvent = true;
        }

        public void AddDiagramLineObject(DiagramItemBase toItem, IVertex l)
        {
            DiagramLineBase newline = (DiagramLineBase)PlatformClass.CreatePlatformObject(l);

            newline.Diagram = this.Diagram;

            if (newline.Vertex.Get(@"Definition:\DiagramLineVertex:") != null)
                Diagram.AddEdgesFromDefintion(newline.Vertex, newline.Vertex.Get(@"Definition:\DiagramLineVertex:"));        

            newline.FromDiagramItem = this;

            newline.ToDiagramItem = toItem;

            newline.AddToCanvas();            

            AddToDiagramLines(newline);

            toItem.DiagramToLines.Add(newline);

            UpdateDiagramLines(toItem);
        }

        IEdge GetLineEdgeFromLineObject(DiagramLineBase line)
        {
            foreach (IEdge e in Vertex.GetAll("DiagramLine:"))
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

            if(GetDiagramLinesToDiagramItemDictionary().ContainsKey(toItem.Vertex))
            foreach (DiagramLineBase l in GetDiagramLinesToDiagramItemDictionary()[toItem.Vertex])
                sameToItemLines.Add(l);

            /*foreach (DiagramLineBase l in DiagramLines) // OOO
                if (l.Vertex.Get("ToDiagramItem:") == toItem.Vertex)
                    sameToItemLines.Add(l);*/

            List<DiagramLineBase> sameFromItemLinesTo = new List<DiagramLineBase>();

            if(toItem.GetDiagramLinesToDiagramItemDictionary().ContainsKey(this.Vertex))
            foreach (DiagramLineBase l in toItem.GetDiagramLinesToDiagramItemDictionary()[this.Vertex])
                sameFromItemLinesTo.Add(l);

            /*foreach (DiagramLineBase l in toItem.DiagramLines) // OOO
                if (l.Vertex.Get("ToDiagramItem:") == this.Vertex)
                    sameFromItemLinesTo.Add(l);*/

            int allCnt = sameToItemLines.Count() + sameFromItemLinesTo.Count();

            int cnt = 0;

            if (toItem == this)
                allCnt = allCnt/2;

            foreach (DiagramLineBase l in sameToItemLines)
            {
                Point start = GetLineAnchorLocation(toItem, new Point(), allCnt, cnt,toItem==this);

                Point end = toItem.GetLineAnchorLocation(this, new Point(), allCnt, cnt, false);
                
                if(toItem==this)
                    l.SetPosition(start.X, start.Y, end.X, end.Y, true, Canvas.GetLeft(this)+ this.ActualWidth+25*(allCnt-cnt),Canvas.GetTop(this) - 25*((allCnt-cnt)));
                else
                    l.SetPosition(start.X, start.Y, end.X, end.Y,false,0,0);

                cnt++;
            }

            foreach (DiagramLineBase l in sameFromItemLinesTo)
            {
                Point end = GetLineAnchorLocation(toItem, new Point(), allCnt, cnt, false);

                Point start = toItem.GetLineAnchorLocation(this, new Point(), allCnt, cnt, false);

                if (toItem != this)
                    l.SetPosition(start.X, start.Y, end.X, end.Y,false,0,0);

                cnt++;
            }
        }

        protected void UpdateDiagramLines()
        {
            foreach (DiagramLineBase m in DiagramToAsMetaLines)
                m.UpdateMetaPosition();

            List<DiagramItemBase> updatedItems = new List<DiagramItemBase>();

            foreach(DiagramLineBase l in DiagramLines)
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

            foreach (DiagramLineBase l in DiagramLines){
                l.Unhighlight();
                l.ToDiagramItem.Unhighlight();
            }

            foreach (DiagramLineBase l in DiagramToLines)
            {
                l.Unhighlight();
                l.FromDiagramItem.Unhighlight();
            }
        }

        public DiagramItemBase() 
        {
            Anchors = new List<FrameworkElement>();

            if (Vertex != null)
                PlatformClass.RegisterVertexChangeListeners(Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges","ForegroundColor","BackgroundColor" });

            this.SizeChanged+=DiagramItemBase_SizeChanged; 

            this.MouseEnter+=DiagramItemBase_MouseEnter;

            this.MouseLeave+=DiagramItemBase_MouseLeave;
        }

        private void DiagramItemBase_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Diagram.IsDrawingLine==false&&Diagram.IsSelecting==false)
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

        public virtual void VertexContentChange()
        {
            VisualiserUpdate();
        }

        public virtual void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.EdgeRemoved)
            {
                DiagramLineBase toRemove=null;

                foreach (DiagramLineBase l in DiagramLines)
                    if (l.Vertex.Get(@"BaseEdge:\Meta:") == e.Edge.Meta &&
                        l.Vertex.Get(@"BaseEdge:\To:") == e.Edge.To)
                        toRemove = l;

                if(toRemove!=null)
                    RemoveDiagramLine(toRemove);
            }

            if (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged)
                VertexContentChange();

            if (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.EdgeAdded && CanAutomaticallyAddEdges)
            {
                Diagram.CheckAndUpdateDiagramLinesForItem(this); 
            }

            if (sender == Vertex.Get(@"LineWidth:") ||
                sender == Vertex.Get(@"BackgroundColor:") || sender == Vertex.Get(@"BackgroundColor:\Red:") || sender == Vertex.Get(@"BackgroundColor:\Green:") || sender == Vertex.Get(@"BackgroundColor:\Blue:") || sender == Vertex.Get(@"BackgroundColor:\Opacity:") ||
                sender == Vertex.Get(@"ForegroundColor:") || sender == Vertex.Get(@"ForegroundColor:\Red:") || sender == Vertex.Get(@"ForegroundColor:\Green:") || sender == Vertex.Get(@"ForegroundColor:\Blue:") || sender == Vertex.Get(@"ForegroundColor:\Opacity:"))
                    VisualiserUpdate();

            if (sender == Vertex || e.Type == VertexChangeType.EdgeAdded)
                    VisualiserUpdate();
        }

        public void AddToSelectedEdges()
        {
            Edge.AddEdge(Diagram.Vertex.Get("SelectedEdges:"), Vertex.Get("BaseEdge:"));
        }

        public void RemoveFromSelectedEdges()
        {
            GraphUtil.DeleteEdgeByToVertex(Diagram.Vertex.Get("SelectedEdges:"), Vertex.Get("BaseEdge:"));
        }

        protected void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            Diagram.SetFocus();

            Diagram.ClickPositionX_ItemCordinates = e.GetPosition(this).X;
            Diagram.ClickPositionY_ItemCordinates = e.GetPosition(this).Y;

            Diagram.ClickTarget = ClickTargetEnum.Item;
            Diagram.ClickedItem = this;

            IVertex selectedEdges = Vertex.Get("SelectedEdges:");

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
            foreach(FrameworkElement r in Anchors)
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
                l.Text= "*";
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
                r= new Rectangle();
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
            double deltax = GraphUtil.GetDoubleValue(Vertex.Get("PositionX:")) - x;
            double deltay = GraphUtil.GetDoubleValue(Vertex.Get("PositionY:")) - y;

            Vertex.Get("PositionX:").Value = x;
            Vertex.Get("PositionY:").Value = y;

            Canvas.SetLeft(this, GraphUtil.GetDoubleValue(Vertex.Get("PositionX:")));
            Canvas.SetTop(this, GraphUtil.GetDoubleValue(Vertex.Get("PositionY:")));

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

            Vertex.Get("PositionX:").Value = left;
            Vertex.Get("PositionY:").Value = top;

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);

            IVertex r=m0.MinusZero.Instance.Root;

            GraphUtil.SetVertexValue(this.Vertex,r.Get(@"System\Meta\Visualiser\Diagram\SizeX"), width);
            GraphUtil.SetVertexValue(this.Vertex, r.Get(@"System\Meta\Visualiser\Diagram\SizeY"), height);

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




        public void Dispose()
        {
            foreach (DiagramLineBase e in DiagramLines)
                if (e is IDisposable)
                    ((IDisposable)e).Dispose();

            PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));
        }
    }
}
