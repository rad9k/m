using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.UML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using m0.UIWpf.Controls;
using m0.UIWpf.Commands;
using m0.UIWpf.Foundation;

namespace m0.UIWpf.Visualisers
{
    class MySlider : SliderWithStopDragging
    {
        public MySlider()
        {
            this.MouseMove += MySlider_PreviewMouseMove;
        }

        void MySlider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }
    }

    public class NumberVisualiser<T> : Grid, IPlatformClass, IDisposable, IHasLocalizableEdges where T : new()    {
        bool IsRanged = false;
        T MinValue, MaxValue;
        TextBox TextBox;
        MySlider Slider;

        bool _IsNull;

        bool IsNull
        {
            get { return _IsNull; }
            set
            {
                _IsNull = value;

                if (IsNull)
                    this.TextBox.Background = (Brush)FindResource("0VeryLightGrayBrush");
                else
                    this.TextBox.Background = (Brush)FindResource("0BackgroundBrush");
            }
        }

        protected void SetIsValid(bool IsValid)
        {
            if (TextBox != null)
            {
                if (IsValid)
                    TextBox.Background = (Brush)FindResource("0BackgroundBrush");
                else
                    TextBox.Background = (Brush)FindResource("0ErrorBrush");
            }
        }


        protected T Parse(object val)
        {
            T ret=new T();

            if (val is T)
                return (T)val;

            if (val is string) {
                if (ret is int)
                {
                    int _ret = 0;
                    int _MinValue = (int)(object)MinValue;
                    int _MaxValue = (int)(object)MaxValue;

                    if (Int32.TryParse((string)val, out _ret) == false)
                    {
                        SetIsValid(false);
                        return MinValue;
                    }

                    if (!GraphUtil.IsNullNumber<T>(MinValue) && _MinValue > _ret)
                    {
                        SetIsValid(false);
                        return (T)(object)_MinValue;
                    }

                    if (!GraphUtil.IsNullNumber<T>(MaxValue) && _MaxValue < _ret)
                    {
                        SetIsValid(false);
                        return (T)(object)_MaxValue;
                    }

                    SetIsValid(true);

                    return (T)(object)_ret;
                }

                if (ret is decimal)
                {
                    decimal _ret = 0;
                    decimal _MinValue = (decimal)(object)MinValue;
                    decimal _MaxValue = (decimal)(object)MaxValue;

                    if (Decimal.TryParse((string)val, out _ret) == false)
                    {
                        SetIsValid(false);
                        return MinValue;
                    }

                    if (!GraphUtil.IsNullNumber<T>(MinValue) && _MinValue > _ret)
                    {
                        SetIsValid(false);
                        return (T)(object)_MinValue;
                    }

                    if (!GraphUtil.IsNullNumber<T>(MaxValue) && _MaxValue < _ret)
                    {
                        SetIsValid(false);
                        return (T)(object)_MaxValue;
                    }

                    SetIsValid(true);

                    return (T)(object)_ret;
                }

                if (ret is double)
                {
                    double _ret = 0;
                    double _MinValue = (double)(object)MinValue;
                    double _MaxValue = (double)(object)MaxValue;

                    if (Double.TryParse((string)val, out _ret) == false)
                    {
                        SetIsValid(false);
                        return MinValue;
                    }

                    if (!GraphUtil.IsNullNumber<T>(MinValue) && _MinValue > _ret)
                    {
                        SetIsValid(false);
                        return (T)(object)_MinValue;
                    }

                    if (!GraphUtil.IsNullNumber<T>(MaxValue) && _MaxValue < _ret)
                    {
                        SetIsValid(false);
                        return (T)(object)_MaxValue;
                    }

                    SetIsValid(true);

                    return (T)(object)_ret;
                }
            }

            return ret;            
        }

        protected void CreateComposite(){
            Children.Clear();

            if (!GraphUtil.IsNullNumber<T>(MinValue) && !GraphUtil.IsNullNumber<T>(MaxValue))
            {
                IsRanged = true;

                ColumnDefinitions.Clear();

                ColumnDefinition col0 = new ColumnDefinition();

                T test=default(T);

                if (test is float)
                    col0.Width = new GridLength(UIWpf.GetHorizontalSizeOfCharacterString((int)Math.Ceiling(2*Math.Log10(Math.Max(GraphUtil.ToInt<T>(MinValue), GraphUtil.ToInt<T>(MaxValue))))));
                else
                    col0.Width = new GridLength(UIWpf.GetHorizontalSizeOfCharacterString((int)Math.Ceiling(Math.Log10(Math.Max(GraphUtil.ToInt<T>(MinValue), GraphUtil.ToInt<T>(MaxValue))))));

                this.ColumnDefinitions.Add(col0);

                ColumnDefinition col1 = new ColumnDefinition();
                col1.Width = new GridLength(5);
                this.ColumnDefinitions.Add(col1);

                ColumnDefinition col2 = new ColumnDefinition();
                col2.Width = new GridLength(1,GridUnitType.Star);                
                this.ColumnDefinitions.Add(col2);
                
                TextBox = new TextBox();
                Grid.SetColumn(TextBox, 0);
                TextBox.TextChanged += new TextChangedEventHandler(OnBoxTextChanged);
                Children.Add(TextBox);
                
                Slider = new MySlider();                
                Slider.MinWidth = 60; //////////////////////////////////////// !!!!!!!!!!!
                Slider.Minimum = GraphUtil.ToDouble<T>(MinValue);
                Slider.Maximum = GraphUtil.ToDouble<T>(MaxValue);
                Slider.IsSnapToTickEnabled = true;
                Slider.Foreground = (Brush)FindResource("0GrayBrush");
                Slider.TickFrequency = 1;
                Slider.TickPlacement = TickPlacement.BottomRight;
                Grid.SetColumn(Slider, 2);                
                Slider.ValueChanged+=new RoutedPropertyChangedEventHandler<double>(OnSliderValueChanged);

                if(Vertex.Get(@"BaseEdge:\Meta:\$UpdateAfterInteractionEnd:") !=null)
                    Slider.FireValueChangedOnlyIfDraggingFinished = true;      


                Children.Add(Slider);
            }
            else
            {
                TextBox = new TextBox();                
                TextBox.TextChanged += new TextChangedEventHandler(OnBoxTextChanged);
                Children.Add(TextBox);
            }
            
        }

        

        public NumberVisualiser()
        {
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();

                Vertex.Value = "NumberVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(@"System\Meta\Visualiser\Integer"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

                this.Loaded += new RoutedEventHandler(OnLoad);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.PreviewMouseMove += dndPreviewMouseMove;
               // this.MouseMove += dndPreviewMouseMove;
                this.Drop+=dndDrop;
                this.AllowDrop = true;

                this.MouseEnter += dndMouseEnter;
            }            
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!UIWpf.HasParentsGotContextMenu(this))
                this.ContextMenu = new m0ContextMenu(this);
        }

        protected void OnBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ValueChangeing == false)
            {
                ValueChangeing = true;

                IVertex bv = Vertex.Get(@"BaseEdge:\To:");

                if (bv == null || bv.Value == null)
                {
                    IVertex r = MinusZero.Instance.Root;

                    IVertex from = Vertex.Get(@"BaseEdge:\From:");
                    IVertex meta = Vertex.Get(@"BaseEdge:\Meta:");
                    IVertex toMeta = r.Get(@"System\Meta\ZeroTypes\Edge\To");

                    if (from != null && meta != null)
                    {
                        //GraphUtil.SetVertexValue(from, meta, Parse(TextBox.Text)); // this is not enough. BaseEdge:\To: is not set

                        GraphUtil.CreateOrReplaceEdge(Vertex.Get("BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, Parse(TextBox.Text)));

                        IsNull = false;
                    }
                }else
                //if (bv != null)
                {
                    bv.Value = Parse(TextBox.Text);

                    if (IsRanged)
                        Slider.Value = GraphUtil.ToDouble<T>((T)bv.Value);
                }

                ValueChangeing = false;
            }
        }

        protected void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ValueChangeing == false)
            {
                ValueChangeing = true;

                IVertex bv = Vertex.Get(@"BaseEdge:\To:");

                if (bv == null || bv.Value == null)
                {
                    IVertex r = MinusZero.Instance.Root;

                    IVertex from = Vertex.Get(@"BaseEdge:\From:");
                    IVertex meta = Vertex.Get(@"BaseEdge:\Meta:");
                    IVertex toMeta = r.Get(@"System\Meta\ZeroTypes\Edge\To");

                    if (from != null && meta != null)
                    {
                        //GraphUtil.SetVertexValue(from, meta, GraphUtil.FromDouble<T>(Slider.Value)); // this is not enough. BaseEdge:\To: is not se

                        GraphUtil.CreateOrReplaceEdge(Vertex.Get("BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, GraphUtil.FromDouble<T>(Slider.Value)));

                        IsNull = false;
                    }

                    TextBox.Text = Slider.Value.ToString();
                }else
                {
                    bv.Value = GraphUtil.FromDouble<T>(Slider.Value);
                    TextBox.Text = Slider.Value.ToString();
                }

                ValueChangeing = false;
            }
        }

        bool ValueChangeing = false;

        private void UpdateBaseEdgeOrMetaVertex()
        {
            IVertex bv = Vertex.Get(@"BaseEdge:\To:");
            IVertex bmv = Vertex.Get(@"BaseEdge:\Meta:");

            if(ValueChangeing)
                return;

            if (bv == null) { }
            else if (bv.Value == null)
            {
                MinValue = GraphUtil.GetNumberValue<T>(bmv.Get("MinValue:"));
                MaxValue = GraphUtil.GetNumberValue<T>(bmv.Get("MaxValue:"));
                CreateComposite();
                IsNull = true;
            }
            else
            //if (bv != null && bv.Value != null)
            {
                IsNull = false;

                ValueChangeing = true;

                MinValue = GraphUtil.GetNumberValue<T>(bmv.Get("MinValue:"));
                MaxValue = GraphUtil.GetNumberValue<T>(bmv.Get("MaxValue:"));


                T value = Parse(bv.Value);

                CreateComposite();

                TextBox.Text = value.ToString();

                if (IsRanged)
                    Slider.Value = GraphUtil.ToDouble<T>(value);

                ValueChangeing = false;
            }
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {            
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")
            || (sender == Vertex.Get("BaseEdge:") && e.Type == VertexChangeType.EdgeAdded && GeneralUtil.CompareStrings(e.Edge.Meta.Value,"To"))
            || (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged)
            || (sender == Vertex.Get("BaseEdge:") && e.Type == VertexChangeType.EdgeAdded && GeneralUtil.CompareStrings(e.Edge.Meta.Value, "Meta"))
            || (sender == Vertex.Get(@"BaseEdge:\Meta:") && e.Type == VertexChangeType.ValueChanged)                                
            )            
                UpdateBaseEdgeOrMetaVertex();            
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

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange),new string[]{"BaseEdge","SelectedEdges"});

                UpdateBaseEdgeOrMetaVertex();
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

        public IVertex GetEdgeByLocation(Point point)
        {
            return Vertex.Get(@"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

        ///// DRAG AND DROP

        Point dndStartPoint;
        bool isValidPreDragStart;

        private bool IsMouseOnSlider(MouseEventArgs e)
        {
            if (!IsRanged)
                return false;

            if (VisualTreeHelper.HitTest(Slider, e.GetPosition(Slider)) == null)
                return false;
            
            return true;
        }

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOnSlider(e))
            {
                dndStartPoint = e.GetPosition(this);
                isValidPreDragStart = true;
            }
            else
                isValidPreDragStart = false;

            MinusZero.Instance.IsGUIDragging = false;

            hasButtonBeenDown = true;
        }

        bool isDraggin = false;
        bool hasButtonBeenDown;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown && isDraggin == false && (e.LeftButton == MouseButtonState.Pressed) &&
                isValidPreDragStart &&
                !IsMouseOnSlider(e) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                if (Vertex.Get(@"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                    dndVertex.AddEdge(null, Vertex.Get(@"BaseEdge:"));

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    isDraggin = false;

                    e.Handled = true;
                }
            } 

            
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            Dnd.DoDrop(this, Vertex.Get(@"BaseEdge:\To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
