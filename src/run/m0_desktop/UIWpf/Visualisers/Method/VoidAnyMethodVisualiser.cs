using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Foundation;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroUML;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace m0.UIWpf.Visualisers.Method
{
    public class VoidAnyMethodVisualiser : Border, IPlatformClass, IDisposable, IHasLocalizableEdges
    {
        class ParameterVisualiserInfo
        {
            public IVertex InputParameter;
            public TextBlock Label;
            public VisualiserEditWrapper Wrapper;
        }

        readonly Grid contentGrid;
        readonly StackPanel parametersPanel;
        readonly Button runButton;
        readonly List<ParameterVisualiserInfo> parameterVisualisers = new List<ParameterVisualiserInfo>();

        IVertex parametersStack;

        public VoidAnyMethodVisualiser()
        {
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();
                Vertex.AddExternalReference();
                Vertex.Value = "VoidAnyMethod" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\Method\VoidAnyMethod"));
                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                parametersStack = InstructionHelpers.CreateStack();
                parametersStack.AddExternalReference();
            }

            contentGrid = new Grid();
            parametersPanel = new StackPanel();
            runButton = new Button();

            this.Child = contentGrid;
            this.Padding = new Thickness(0);
            this.BorderThickness = new Thickness(0);
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetIsSharedSizeScope(contentGrid, true);

            contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            contentGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            parametersPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

            Grid.SetColumn(parametersPanel, 0);
            contentGrid.Children.Add(parametersPanel);

            runButton.Content = "run";
            runButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            runButton.VerticalAlignment = VerticalAlignment.Stretch;
            runButton.VerticalContentAlignment = VerticalAlignment.Center;
            runButton.Margin = new Thickness(6, 0, 0, 0);
            Grid.SetColumn(runButton, 1);
            runButton.Click += RunButton_Click;

            contentGrid.Children.Add(runButton);
        }

        public void RefreshParameters()
        {
            ClearParameterEditors();

            IVertex methodVertex = Vertex.Get(false, @"ExecutableVertex:");

            if (methodVertex == null)
                return;

            IList<IEdge> inputParameters = GraphUtil.GetQueryOut(methodVertex, "InputParameter", null);

            foreach (IEdge inputParameterEdge in inputParameters)
            {
                AddParameterEditor(inputParameterEdge.To);
            }
        }

        private void AddParameterEditor(IVertex inputParameter)
        {
            Grid row = new Grid();
            row.Margin = new Thickness(0, 0, 0, 4);
            row.HorizontalAlignment = HorizontalAlignment.Stretch;

            row.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto, SharedSizeGroup = "ParameterLabel" });
            row.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock label = CreateParameterLabel(inputParameter);
            Border separator = CreateParameterSeparator();
            VisualiserEditWrapper wrapper = CreateParameterWrapper(inputParameter);

            Grid.SetColumn(label, 0);
            Grid.SetColumn(separator, 1);
            Grid.SetColumn(wrapper, 2);

            row.Children.Add(label);
            row.Children.Add(separator);
            row.Children.Add(wrapper);

            parametersPanel.Children.Add(row);

            parameterVisualisers.Add(new ParameterVisualiserInfo()
            {
                InputParameter = inputParameter,
                Label = label,
                Wrapper = wrapper
            });
        }

        private TextBlock CreateParameterLabel(IVertex inputParameter)
        {
            TextBlock label = new TextBlock();
            label.Text = (string)inputParameter.Value;
            label.FontStyle = FontStyles.Italic;
            label.FontWeight = WpfUtil.MetaWeight;
            label.Foreground = (Brush)FindResource("0GrayBrush");
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Margin = new Thickness(0, 0, 0, 0);
            label.TextAlignment = TextAlignment.Right;
            label.HorizontalAlignment = HorizontalAlignment.Stretch;

            return label;
        }

        private Border CreateParameterSeparator()
        {
            Border separator = new Border();
            separator.BorderThickness = new Thickness(4, 0, 0, 0);

            return separator;
        }

        private VisualiserEditWrapper CreateParameterWrapper(IVertex inputParameter)
        {
            VisualiserEditWrapper wrapper = new VisualiserEditWrapper(Vertex);
            wrapper.VerticalAlignment = VerticalAlignment.Center;
            wrapper.HorizontalAlignment = HorizontalAlignment.Stretch;
            wrapper.MinWidth = 0;

            IEdge existingParameterEdge = GraphUtil.GetQueryOutFirstEdge(parametersStack, inputParameter, null);

            if (existingParameterEdge == null)
                wrapper.BaseEdge = new EasyEdge(parametersStack, inputParameter, null);
            else
                wrapper.BaseEdge = existingParameterEdge;

            return wrapper;
        }

        private void ClearParameterEditors()
        {
            foreach (ParameterVisualiserInfo parameterVisualiser in parameterVisualisers)
                parameterVisualiser.Wrapper.Dispose();

            parameterVisualisers.Clear();

            parametersPanel.Children.Clear();
        }

        void ButtonVisualChange(bool isActive)
        {
            Thread thread = new Thread(new ThreadStart(delegate ()
            {
                Thread.Sleep(200);
                try
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                        new Action(delegate ()
                        {
                            runButton.IsEnabled = isActive;
                        }));
                }
                catch { }
            }));
            thread.Name = "ThreadName";
            thread.Start();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");
            IVertex methodVertex = Vertex.Get(false, @"ExecutableVertex:");

            ButtonVisualChange(false);

            if (baseVertex != null && methodVertex != null && parametersStack != null)
            {
                IExecution exe = new ZeroCodeExecution();
                ZeroCodeExecutonUtil.MethodCall(exe, methodVertex, baseVertex, parametersStack);
            }

            ButtonVisualChange(true);
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set { _Vertex = value; }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                runButton.Click -= RunButton_Click;
                ClearParameterEditors();

                if (parametersStack is IDisposable)
                    ((IDisposable)parametersStack).Dispose();

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        public IVertex GetEdgeByPoint(Point point)
        {
            foreach (ParameterVisualiserInfo parameterVisualiser in parameterVisualisers)
            {
                Point translatedPoint = TranslatePoint(point, parameterVisualiser.Wrapper);

                if (VisualTreeHelper.HitTest(parameterVisualiser.Wrapper, translatedPoint) != null)
                {
                    if (parameterVisualiser.Wrapper.Content is IHasLocalizableEdges localizableEdges)
                        return localizableEdges.GetEdgeByPoint(
                            parameterVisualiser.Wrapper.TranslatePoint(translatedPoint, (UIElement)parameterVisualiser.Wrapper.Content));

                    return Vertex.Get(false, @"BaseEdge:");
                }
            }

            return Vertex.Get(false, @"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            foreach (ParameterVisualiserInfo parameterVisualiser in parameterVisualisers)
                if (visualElement == parameterVisualiser.Wrapper || visualElement == parameterVisualiser.Label)
                {
                    if (parameterVisualiser.Wrapper.Content is IHasLocalizableEdges localizableEdges)
                        return localizableEdges.GetEdgeByVisualElement(visualElement);

                    return Vertex.Get(false, @"BaseEdge:");
                }

            if (visualElement == runButton)
                return Vertex.Get(false, @"BaseEdge:");

            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            if (vertex == null)
                throw new NotImplementedException();

            IVertex edgeMeta = vertex.Get(false, @"Meta:");

            if (edgeMeta != null)
            {
                ParameterVisualiserInfo parameterVisualiser =
                    parameterVisualisers.FirstOrDefault(v => v.InputParameter == edgeMeta);

                if (parameterVisualiser != null)
                    return parameterVisualiser.Wrapper;
            }

            return runButton;
        }
    }
}
