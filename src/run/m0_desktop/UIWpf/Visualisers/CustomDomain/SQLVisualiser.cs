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
using m0.Lib;

namespace m0.UIWpf.Visualisers.CustomDomain
{
    class SQLVisualiser : Border, IVisualiser, ITypedEdge
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public bool SelectionProphibited { get; set; }

        List<IVertex> manuallyAddedVertexChangeListeners = new List<IVertex>();

        // TypedEdge START
        public SQLVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        StackPanel stackPanel;

        public SQLVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            this.Padding = new Thickness(2);

            stackPanel = new StackPanel();

            stackPanel.Orientation = Orientation.Vertical;

            this.Child = stackPanel;

            new AtomVisualiserHelper(
               parentVisualiser,
               isVolatile,
               MinusZero.Instance.Root.Get(false, @"System\Meta\CustomDomain\SQL\Visualiser\SQL"), 
               this, 
               "SQLVisualiser", 
               this, 
               false, 
               new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\"}, 
               "ListVisualiser",
               baseEdgeVertex,
               UpdateBaseEdgeCallSchemeEnum.OmmitSecond);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        public void BaseEdgeToUpdated()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            stackPanel.Children.Clear();

            if (bv != null && bv.Value != null)
            {
                Brush foregroundBrush = (Brush)FindResource("0ForegroundBrush");

                List<IEdge> primaryKeyColumnEdges = new List<IEdge>();
                List<IEdge> regularColumnEdges = new List<IEdge>();

                foreach (IEdge columnEdge in GraphUtil.GetQueryOut(bv, "Column", null))
                {
                    if (IsColumnFlagTrue(columnEdge.To, "IsPK"))
                        primaryKeyColumnEdges.Add(columnEdge);
                    else
                        regularColumnEdges.Add(columnEdge);
                }

                Grid tableGrid = new Grid();
                tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                int rowIndex = 0;

                foreach (IEdge columnEdge in primaryKeyColumnEdges)
                {
                    tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    AddColumnRow(tableGrid, columnEdge, rowIndex, foregroundBrush, "PK");
                    rowIndex++;
                }

                if (primaryKeyColumnEdges.Count > 0 && regularColumnEdges.Count > 0)
                {
                    tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Border sectionSeparator = new Border();
                    sectionSeparator.BorderBrush = foregroundBrush;
                    sectionSeparator.BorderThickness = new Thickness(0, 1, 0, 0);
                    Grid.SetRow(sectionSeparator, rowIndex);
                    Grid.SetColumn(sectionSeparator, 0);
                    Grid.SetColumnSpan(sectionSeparator, 2);
                    tableGrid.Children.Add(sectionSeparator);

                    rowIndex++;
                }

                foreach (IEdge columnEdge in regularColumnEdges)
                {
                    tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    string leftLabel = IsColumnFlagTrue(columnEdge.To, "IsFK") ? "FK" : "";
                    AddColumnRow(tableGrid, columnEdge, rowIndex, foregroundBrush, leftLabel);
                    rowIndex++;
                }

                stackPanel.Children.Add(tableGrid);
            }
            else
            {
                TextBlock tb = new TextBlock();

                tb.Text = "Ø";

                stackPanel.Children.Add(tb);
            }                
        }

        // Reads a "True"/"False" flag (e.g. IsPK, IsFK) from a Column vertex.
        static bool IsColumnFlagTrue(IVertex columnVertex, string flagMetaName)
        {
            IVertex flagVertex = GraphUtil.GetQueryOutFirst(columnVertex, flagMetaName, null);

            return GraphUtil.GetBooleanValueOrFalse(flagVertex);
        }

        void AddColumnRow(Grid tableGrid, IEdge columnEdge, int rowIndex, Brush foregroundBrush, string leftLabel)
        {
            IVertex columnVertex = columnEdge.To;

            Border leftCell = new Border();
            leftCell.BorderBrush = foregroundBrush;
            leftCell.BorderThickness = new Thickness(0, 0, 1, 0);
            leftCell.Padding = new Thickness(2, 0, 2, 0);

            TextBlock leftLabelTextBlock = new TextBlock();
            leftLabelTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            leftLabelTextBlock.Text = leftLabel;
            leftCell.Child = leftLabelTextBlock;

            Grid.SetRow(leftCell, rowIndex);
            Grid.SetColumn(leftCell, 0);
            tableGrid.Children.Add(leftCell);

            StackPanel rightStack = new StackPanel();
            rightStack.Orientation = Orientation.Horizontal;
            rightStack.Margin = new Thickness(2, 0, 2, 0);

            string columnName = columnVertex.Value != null ? columnVertex.Value.ToString() : "";

            TextBlock nameTextBlock = new TextBlock();
            nameTextBlock.FontWeight = FontWeights.Bold;

            IVertex columnTypeVertex = GraphUtil.GetQueryOutFirst(columnVertex, "$EdgeTarget", null);

            if (columnTypeVertex != null)
            {
                nameTextBlock.Text = columnName + " : ";
                rightStack.Children.Add(nameTextBlock);

                TextBlock typeTextBlock = new TextBlock();
                typeTextBlock.FontStyle = FontStyles.Italic;
                typeTextBlock.Text = GraphUtil.GetStringValue(columnTypeVertex);
                rightStack.Children.Add(typeTextBlock);
            }
            else
            {
                nameTextBlock.Text = columnName;
                rightStack.Children.Add(nameTextBlock);
            }

            Grid.SetRow(rightStack, rowIndex);
            Grid.SetColumn(rightStack, 1);
            tableGrid.Children.Add(rightStack);
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
