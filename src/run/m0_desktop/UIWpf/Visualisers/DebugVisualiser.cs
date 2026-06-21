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
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class DebugVisualiser : StackPanel, IVisualiser, ITypedEdge, IKeyboardHighlight
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public bool SelectionProhibited { get; set; }
        private readonly List<FrameworkElement> keyboardHighlightElements = new List<FrameworkElement>();
        private readonly List<IEdge> keyboardHighlightEdges = new List<IEdge>();
        private int currentHighlightPosition = -1;
        private bool isBeforeFirstPosition;
        private bool isAfterLastPosition;

        // TypedEdge START

        public DebugVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public DebugVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {            
            new AtomVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Debug"), 
                this, 
                "DebugVisualiser", 
                this, 
                false, 
                new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" }, 
                "ListVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        public int CurrentHighlightPosition { get { return currentHighlightPosition; } }

        public bool IsBeforeFirstPosition { get { return isBeforeFirstPosition; } }

        public bool IsAfterLastPosition { get { return isAfterLastPosition; } }

        public bool IsFirstPosition
        {
            get { return currentHighlightPosition == 0 && !isBeforeFirstPosition && !isAfterLastPosition; }
            set { if (value) SetKeyboardHighlightPosition(0); }
        }

        public bool IsLastPosition
        {
            get { return keyboardHighlightElements.Count > 0 && currentHighlightPosition == keyboardHighlightElements.Count - 1 && !isBeforeFirstPosition && !isAfterLastPosition; }
            set { if (value) SetKeyboardHighlightPosition(keyboardHighlightElements.Count - 1); }
        }

        public bool CanGoBeforeFirstPosition { get { return true; } }

        public bool CanGoAfterLastPosition { get { return true; } }

        public bool HasKeyboardHighlightItems
        {
            get { return keyboardHighlightElements.Count > 0; }
        }

        public bool IsVertexCommanderKeyboardHighlightEnabled { get; set; }

        public IEdge KeyboardHighlightedEdge
        {
            get
            {
                if (currentHighlightPosition < 0 || currentHighlightPosition >= keyboardHighlightEdges.Count)
                    return null;

                return keyboardHighlightEdges[currentHighlightPosition];
            }
        }

        public event EventHandler KeyboardHighlightActivated;

        public event EventHandler KeyboardHighlightEnterPressed;

        public event EventHandler GoneBeforeFirstPosition;

        public event EventHandler GoneAfterLastPosition;

        string GetEdgeString(IVertex meta, IVertex to)
        {
            string ret = "";

            if (meta != null && meta.Value != null)
                ret += meta.Value.ToString();

            ret += " :: ";

            if (to != null && to.Value != null)
                ret += to.Value.ToString();

            ret += " [";

            if (meta != null)
                ret += meta.Store.Identifier + " " + meta.Identifier;
                    
            ret += " :: ";

            ret += to.Store.Identifier + " " + to.Identifier + "]";

            return ret;
        }

        private void AddLine(StringBuilder sb)
        {
            TextBlock tb = new TextBlock();
                
            tb.FontFamily = new FontFamily("Consolas");

            tb.Text = sb.ToString();

            this.Children.Add(tb);
        }

        private void AddVertexVertexLine(string s, IVertex v1, IVertex v2, IEdge edge)
        {
            StackPanel sp = new StackPanel();

            sp.Orientation = Orientation.Horizontal;
            sp.MouseLeftButtonDown += DebugLine_MouseLeftButtonDown;

            TextBlock tb = new TextBlock();

            tb.FontFamily = new FontFamily("Consolas");

            tb.Text = " " + s;

            //meta

            Button b_m = new Button();

            b_m.Padding = new Thickness(0);

            b_m.FontSize = 6;

            b_m.Tag = v1;

            b_m.Width = 25;
            b_m.Height = 10;

            b_m.Content = "go meta";

            b_m.Click += GoEvent;

            sp.Children.Add(b_m);

            //to

            Button b = new Button();

            b.Padding = new Thickness(0);

            b.FontSize = 6;

            b.Tag = v2;

            b.Width = 20;
            b.Height = 10;

            b.Content = "go to";

            b.Click += GoEvent;

            sp.Children.Add(b);


            sp.Children.Add(tb);

            this.Children.Add(sp);
            keyboardHighlightElements.Add(sp);
            keyboardHighlightEdges.Add(edge);
        }        

        private void DebugLine_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled || e.ClickCount != 2)
                return;

            int index = keyboardHighlightElements.IndexOf((FrameworkElement)sender);

            if (index < 0)
                return;

            SetKeyboardHighlightPosition(index);
            RaiseKeyboardHighlightActivated();
            e.Handled = true;
        }

        private void GoEvent(object sender, RoutedEventArgs e)
        {
            IVertex v = (IVertex)((Button)e.Source).Tag;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            GraphUtil.ReplaceEdge(Vertex.Get(false, @"BaseEdge:"), EdgeHelper.MetaMeta, MinusZero.Instance.Empty);
            GraphUtil.ReplaceEdge(Vertex.Get(false, @"BaseEdge:"), EdgeHelper.ToMeta, v);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public void ClearKeyboardHighlight()
        {
            if (currentHighlightPosition >= 0 && currentHighlightPosition < keyboardHighlightElements.Count)
                SetElementHighlight(keyboardHighlightElements[currentHighlightPosition], false);

            currentHighlightPosition = -1;
            isBeforeFirstPosition = false;
            isAfterLastPosition = false;
        }

        public void MoveKeyboardHighlight(int positionDelta)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            if (keyboardHighlightElements.Count == 0)
            {
                if (positionDelta < 0)
                    GoBeforeFirstKeyboardHighlightPosition();
                else
                    GoAfterLastKeyboardHighlightPosition();

                return;
            }

            int newPosition = currentHighlightPosition + positionDelta;

            if (currentHighlightPosition < 0)
                newPosition = positionDelta < 0 ? keyboardHighlightElements.Count - 1 : 0;

            if (newPosition < 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else if (newPosition >= keyboardHighlightElements.Count)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightPosition(newPosition);
        }

        public void MoveKeyboardHighlight(KeyboardHighlightMoveDirection direction)
        {
            if (direction == KeyboardHighlightMoveDirection.Up)
                MoveKeyboardHighlight(-1);
            else if (direction == KeyboardHighlightMoveDirection.Down)
                MoveKeyboardHighlight(1);
        }

        public void ToggleKeyboardHighlightedEdgeSelection() { }

        private void SetKeyboardHighlightPosition(int position)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            ClearKeyboardHighlight();

            if (position < 0 || position >= keyboardHighlightElements.Count)
                return;

            currentHighlightPosition = position;
            SetElementHighlight(keyboardHighlightElements[position], true);
            keyboardHighlightElements[position].BringIntoView();
        }

        private void GoBeforeFirstKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();
            isBeforeFirstPosition = true;

            if (GoneBeforeFirstPosition != null)
                GoneBeforeFirstPosition(this, EventArgs.Empty);
        }

        private void GoAfterLastKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();
            isAfterLastPosition = true;

            if (GoneAfterLastPosition != null)
                GoneAfterLastPosition(this, EventArgs.Empty);
        }

        private void SetElementHighlight(FrameworkElement element, bool isHighlighted)
        {
            Panel panel = element as Panel;

            if (panel == null)
                return;

            panel.Background = isHighlighted
                ? (Brush)FindResource("0HighlightBrush")
                : (Brush)FindResource("0BackgroundBrush");
        }

        private void RaiseKeyboardHighlightActivated()
        {
            if (KeyboardHighlightedEdge == null)
                return;

            if (KeyboardHighlightActivated != null)
                KeyboardHighlightActivated(this, EventArgs.Empty);

            if (KeyboardHighlightEnterPressed != null)
                KeyboardHighlightEnterPressed(this, EventArgs.Empty);
        }

        public void BaseEdgeToUpdated()
        {
            IVertex mv = Vertex.Get(false, @"BaseEdge:\Meta:");
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null)
            {
                this.Children.Clear();
                keyboardHighlightElements.Clear();
                keyboardHighlightEdges.Clear();
                currentHighlightPosition = -1;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(GetEdgeString(mv, bv));

                sb.AppendLine();

                sb.AppendLine("INPUT EDGES RAW [" + bv.InEdgesRaw.Count + "]:");

                //sb.AppendLine();

                AddLine(sb);

                foreach (IEdge e in bv.InEdgesRaw)
                    AddVertexVertexLine(GetEdgeString(e.Meta, e.From), e.Meta, e.From, e);
                

                sb.Clear();

                sb.AppendLine();

                sb.AppendLine("OUTPUT EDGES RAW [" + bv.OutEdgesRaw.Count + "]:");

                //sb.AppendLine();

                AddLine(sb);

                foreach (IEdge e in bv.OutEdgesRaw)
                    AddVertexVertexLine(GetEdgeString(e.Meta, e.To), e.Meta, e.To, e);                       

                return;
            }
            else
            {
                this.Children.Clear();
                keyboardHighlightElements.Clear();
                keyboardHighlightEdges.Clear();
                currentHighlightPosition = -1;

                return;
            }        
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                VisualiserHelper.Dispose();
            }
        }

        public IVertex GetEdgeByPoint(System.Windows.Point point)
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
    }
}
