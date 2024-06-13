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
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using m0.UIWpf.Controls;
using m0.UIWpf.Commands;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;

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

    public class NumberVisualiser<T> : Grid, IVisualiser where T : new()    {
        protected virtual string visualiserName { get; set; }
        protected virtual IVertex visualiserMetaVertex { get; set; }

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        bool isContinous = false;
        bool IsRanged = false;
        T MinValue, MaxValue;
        TextBox TextBox;
        MySlider Slider;

        public NumberVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            new AtomVisualiserHelper(parentVisualiser,
                isVolatile,
                visualiserMetaVertex,
                this,
                visualiserName,
                this,
                false,
                new List<string> { @"BaseEdge:\To:" },
                "AtomVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

            // need custom dnd becouse of slider / mouse move
            this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
            this.PreviewMouseMove += dndPreviewMouseMove;

            this.Drop += dndDrop;
            this.AllowDrop = true;

            this.MouseEnter += dndMouseEnter;

            if (typeof(T) == typeof(double?))
                isContinous = true;
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }


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

        protected T Parse(object _val)
        {
            if (_val is T)
                return (T)_val;

            string val = _val.ToString();

            if (val is string) {
                if (typeof(T) == typeof(int?))
                {
                    int _ret = 0;
                    int? _MinValue = (int?)(object)MinValue;
                    int? _MaxValue = (int?)(object)MaxValue;

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

                if (typeof(T) == typeof(decimal?))
                {
                    decimal _ret = 0;
                    decimal? _MinValue = (decimal?)(object)MinValue;
                    decimal? _MaxValue = (decimal?)(object)MaxValue;

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

                if (typeof(T) == typeof(double?))
                {
                    double _ret = 0;
                    double? _MinValue = (double?)(object)MinValue;
                    double? _MaxValue = (double?)(object)MaxValue;

                    if (Double.TryParse((string)val, out _ret) == false)
                    {
                        SetIsValid(false);

                        if (!GraphUtil.IsNullNumber<T>(MinValue))
                            return MinValue;
                        else
                            return (T)(object)new double?(0);
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

            return new T();            
        }

        protected void CreateComposite(){
            Children.Clear();

            if (!GraphUtil.IsNullNumber<T>(MinValue) && !GraphUtil.IsNullNumber<T>(MaxValue))
            {
                IsRanged = true;

                ColumnDefinitions.Clear();

                ColumnDefinition col0 = new ColumnDefinition();                

                int additionalCharacterBecouseOfCanBeNegative = 0;

                if ((int)GraphUtil.ToInt<T>(MinValue) < 0)
                    additionalCharacterBecouseOfCanBeNegative = 1;

                if (isContinous)
                    col0.Width = new GridLength(WpfUtil.GetHorizontalSizeOfCharacterString(additionalCharacterBecouseOfCanBeNegative + (int)Math.Ceiling(2*Math.Log10(Math.Max((int)GraphUtil.ToInt<T>(MinValue), (int)GraphUtil.ToInt<T>(MaxValue))))));
                else
                    col0.Width = new GridLength(WpfUtil.GetHorizontalSizeOfCharacterString(additionalCharacterBecouseOfCanBeNegative + (int)Math.Ceiling(Math.Log10(Math.Max((int)GraphUtil.ToInt<T>(MinValue), (int)GraphUtil.ToInt<T>(MaxValue))))));

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

                if(MinValue != null && MaxValue!=null)
                Slider.Minimum = (double)GraphUtil.ToDouble<T>(MinValue);
                Slider.Maximum = (double)GraphUtil.ToDouble<T>(MaxValue);

                if(isContinous)
                    Slider.IsSnapToTickEnabled = false;
                else
                    Slider.IsSnapToTickEnabled = true;

                Slider.Foreground = (Brush)FindResource("0GrayBrush");
                Slider.TickFrequency = 1;
                Slider.TickPlacement = TickPlacement.BottomRight;
                Grid.SetColumn(Slider, 2);                
                Slider.ValueChanged+=new RoutedPropertyChangedEventHandler<double>(OnSliderValueChanged);

                if(Vertex.Get(false, @"BaseEdge:\Meta:\$UpdateAfterInteractionEnd:") !=null)
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

        protected void OnBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ValueChangeing == false)
            {
                ValueChangeing = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

                if (bv == null || bv.Value == null || bv == MinusZero.Instance.Empty)
                {
                    IVertex r = MinusZero.Instance.Root;

                    IVertex from = Vertex.Get(false, @"BaseEdge:\From:");
                    IVertex meta = Vertex.Get(false, @"BaseEdge:\Meta:");
                    IVertex toMeta = r.Get(false, @"System\Meta\ZeroTypes\Edge\To");

                    if (from != null && meta != null)
                    {
                        //GraphUtil.SetVertexValue(from, meta, Parse(TextBox.Text)); // this is not enough. BaseEdge:\To: is not set

                        GraphUtil.CreateOrReplaceEdge(Vertex.Get(false, "BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, Parse(TextBox.Text)));

                        IsNull = false;
                    }
                }else
                //if (bv != null)
                {
                    bv.Value = Parse(TextBox.Text);

                    if (IsRanged)
                        Slider.Value = (double)GraphUtil.ToDouble<T>((T)bv.Value);
                }

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                ValueChangeing = false;
            }
        }

        protected void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ValueChangeing == false)
            {
                ValueChangeing = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

                if (bv == null || bv.Value == null || bv == MinusZero.Instance.Empty)
                {
                    IVertex r = MinusZero.Instance.Root;

                    IVertex from = Vertex.Get(false, @"BaseEdge:\From:");
                    IVertex meta = Vertex.Get(false, @"BaseEdge:\Meta:");
                    IVertex toMeta = r.Get(false, @"System\Meta\ZeroTypes\Edge\To");

                    if (from != null && meta != null)
                    {
                        //GraphUtil.SetVertexValue(from, meta, GraphUtil.FromDouble<T>(Slider.Value)); // this is not enough. BaseEdge:\To: is not se

                        GraphUtil.CreateOrReplaceEdge(Vertex.Get(false, "BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, GraphUtil.FromDouble<T>(Slider.Value)));

                        IsNull = false;
                    }

                    TextBox.Text = Slider.Value.ToString();
                }else
                {
                    bv.Value = GraphUtil.FromDouble<T>(Slider.Value);
                    TextBox.Text = Slider.Value.ToString();
                }

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                ValueChangeing = false;
            }
        }

        bool ValueChangeing = false;

        public void BaseEdgeToUpdated()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");
            IVertex bmv = Vertex.Get(false, @"BaseEdge:\Meta:");

            if(ValueChangeing)
                return;

            if (bv == null) { }
            else if (bv.Value == null || (bv.Value is String && (String)bv.Value == "") || bv == MinusZero.Instance.Empty)
            {
                T _minValue = GraphUtil.GetNumberValue<T>(bmv.Get(false, "MinValue:"));
                T _maxValue = GraphUtil.GetNumberValue<T>(bmv.Get(false, "MaxValue:"));

                if (_minValue != null && _minValue != null) {
                    MinValue = _minValue;
                    MaxValue = _maxValue;
                }
                CreateComposite();
                IsNull = true;
            }
            else
            //if (bv != null && bv.Value != null)
            {
                //IsNull = false;

                ValueChangeing = true;

                MinValue = GraphUtil.GetNumberValue<T>(bmv.Get(false, "MinValue:"));
                MaxValue = GraphUtil.GetNumberValue<T>(bmv.Get(false, "MaxValue:"));


                T value = Parse(bv.Value);

                if (value == null)
                    try
                    {
                        value = (T)bv.Value;
                    }catch(Exception e) { }
                
                CreateComposite();
                IsNull = false;

                TextBox.Text = value.ToString();

                if (IsRanged)
                    Slider.Value = (double)GraphUtil.ToDouble<T>(value);

                ValueChangeing = false;
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

        public IVertex GetEdgeByPoint(Point point)
        {
            return Vertex.Get(false, @"BaseEdge:");
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
                if (Vertex.Get(false, @"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                    dndVertex.AddEdge(null, Vertex.Get(false, @"BaseEdge:"));

                    dndVertex.AddExternalReference();

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
            Dnd.DoDrop(this, Vertex.Get(false, @"BaseEdge:\To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
