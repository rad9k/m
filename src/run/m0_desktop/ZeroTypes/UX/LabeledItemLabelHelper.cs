using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Controls;
using m0.Util;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace m0.ZeroTypes.UX
{
    internal static class LabeledItemLabelHelper
    {
        public static IEdge GetLabelEdge(IEdge baseEdge, string contentQuery)
        {
            if (baseEdge == null)
                return null;

            if (contentQuery == null)
                return baseEdge;

            if (baseEdge.To == null)
                return null;

            return baseEdge.To.GetAll(false, contentQuery).FirstOrDefault();
        }

        public static StackPanel CreateRootStack()
        {
            StackPanel stack = new StackPanel();
            stack.HorizontalAlignment = HorizontalAlignment.Center;
            stack.Orientation = Orientation.Horizontal;

            return stack;
        }

        public static void AddConstantLabel(StackPanel stack, string constantLabel, System.Func<HorizontalAlignment, TextBlock> createTextBlock)
        {
            if (constantLabel == null)
                return;

            TextBlock constantTextBlock = createTextBlock(HorizontalAlignment.Center);
            constantTextBlock.FontStyle = FontStyles.Italic;
            constantTextBlock.Text = constantLabel;
            stack.Children.Add(constantTextBlock);

            TextBlock dividerTextBlock = createTextBlock(HorizontalAlignment.Center);
            dividerTextBlock.Text = " | ";
            stack.Children.Add(dividerTextBlock);
        }

        public static void AddIconIfNeeded(StackPanel stack, bool showIcons, IEdge labelEdge)
        {
            if (!showIcons)
                return;

            ImageSource iconSource = IconServer.GetIconByEdge(labelEdge);

            if (iconSource == null)
                return;

            Image iconImage = new Image();
            iconImage.Source = iconSource;
            iconImage.Width = WpfUtil.IconSize;
            iconImage.Height = WpfUtil.IconSize;
            iconImage.Margin = new Thickness(0, 0, 3, 0);
            iconImage.VerticalAlignment = VerticalAlignment.Center;
            iconImage.Stretch = Stretch.Uniform;
            iconImage.SnapsToDevicePixels = true;
            iconImage.UseLayoutRounding = true;

            RenderOptions.SetBitmapScalingMode(iconImage, BitmapScalingMode.Fant);

            stack.Children.Add(iconImage);
        }

        public static string GetLabelLeft(IEdge labelEdge, bool showMeta)
        {
            if (labelEdge == null)
                return string.Empty;

            IVertex metaVertex = labelEdge.Meta;
            IEdge isEdge = labelEdge.To == null
                ? null
                : GraphUtil.GetQueryOut(labelEdge.To, "$Is", null).FirstOrDefault();

            if (isEdge != null)
                metaVertex = isEdge.To;

            object metaValue = metaVertex?.Value;

            if (!showMeta || GeneralUtil.CompareStrings(metaValue, "$Empty"))
                return string.Empty;

            if (metaValue == null)
                return "Ø :: ";

            return metaValue.ToString() + " :: ";
        }

        public static string GetLabelRight(IEdge labelEdge)
        {
            if (labelEdge?.To?.Value == null)
                return "Ø";

            return labelEdge.To.Value.ToString();
        }

        public static bool GetShowIcons(IVertex vertex)
        {
            IVertex val = GraphUtil.GetQueryOutFirst(vertex, "ShowIcons", null);

            if (val == null)
                return false;

            return GraphUtil.GetBooleanValueOrFalse(val);
        }

        public static Grid CreateWrappingLabelGrid(VerticalAlignment verticalAlignment)
        {
            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.VerticalAlignment = verticalAlignment;
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            return grid;
        }

        public static void ApplyWrappingTextBoxLayout(TextBox textBox)
        {
            if (textBox == null)
                return;

            textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            textBox.TextAlignment = TextAlignment.Center;
            textBox.AcceptsReturn = true;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            textBox.Margin = new Thickness(0);
            textBox.Padding = new Thickness(0);

            ApplyWrappingTextBoxVerticalAlignment(textBox);

            textBox.SizeChanged += WrappingTextBox_SizeChanged;
            textBox.TextChanged += WrappingTextBox_TextChanged;
        }

        static void WrappingTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyWrappingTextBoxVerticalAlignment(sender as TextBox);
        }

        static void WrappingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyWrappingTextBoxVerticalAlignment(sender as TextBox);
        }

        public static void ApplyWrappingTextBoxVerticalAlignment(TextBox textBox)
        {
            if (textBox == null)
                return;

            bool singleLine = textBox.LineCount <= 1;
            VerticalAlignment verticalAlignment = singleLine
                ? VerticalAlignment.Center
                : VerticalAlignment.Top;

            if (textBox.VerticalAlignment != verticalAlignment)
                textBox.VerticalAlignment = verticalAlignment;

            if (textBox.VerticalContentAlignment != verticalAlignment)
                textBox.VerticalContentAlignment = verticalAlignment;
        }

        public static Grid BuildWrappingLabelControl(
            IEdge baseEdgeForLabel,
            string constantLabel,
            bool hideLabel,
            bool showIcons,
            string leftText,
            Func<HorizontalAlignment, TextBlock> createTextBlock,
            TextBox valueTextBox,
            VerticalAlignment gridVerticalAlignment)
        {
            Grid grid = CreateWrappingLabelGrid(gridVerticalAlignment);

            if (baseEdgeForLabel == null)
                return grid;

            StackPanel prefix = CreateRootStack();
            prefix.HorizontalAlignment = HorizontalAlignment.Left;
            prefix.VerticalAlignment = VerticalAlignment.Top;

            AddConstantLabel(prefix, constantLabel, createTextBlock);

            if (!hideLabel)
            {
                AddIconIfNeeded(prefix, showIcons, baseEdgeForLabel);

                if (!string.IsNullOrEmpty(leftText))
                {
                    TextBlock left = createTextBlock(HorizontalAlignment.Center);
                    left.Text = leftText;
                    prefix.Children.Add(left);
                }
            }

            if (prefix.Children.Count > 0)
            {
                Grid.SetColumn(prefix, 0);
                grid.Children.Add(prefix);
            }

            if (valueTextBox != null)
            {
                if (prefix.Children.Count == 0)
                {
                    Grid.SetColumn(valueTextBox, 0);
                    Grid.SetColumnSpan(valueTextBox, 2);
                }
                else
                    Grid.SetColumn(valueTextBox, 1);

                grid.Children.Add(valueTextBox);
            }

            return grid;
        }

        public static void LimitLabelControlToItemHeight(FrameworkElement labelControl, FrameworkElement item)
        {
            if (labelControl == null || item == null)
                return;

            Binding maxHeightBinding = new Binding("ActualHeight");
            maxHeightBinding.Source = item;

            BindingOperations.SetBinding(
                labelControl,
                FrameworkElement.MaxHeightProperty,
                maxHeightBinding);
        }

        public static void ApplyLabelContainerClipping(Border labelContainer, bool useCodeLabel)
        {
            if (labelContainer == null)
                return;

            labelContainer.ClipToBounds = !useCodeLabel;
        }

        public static void ApplyHeaderRowHeightForWrappingLabel(
            RowDefinition row,
            bool useCodeLabel,
            bool hideHeader,
            double minHeightWhenVisible)
        {
            if (row == null)
                return;

            if (hideHeader)
            {
                row.Height = new GridLength(0);
                row.MinHeight = 0;
                return;
            }

            if (useCodeLabel)
            {
                row.Height = new GridLength(minHeightWhenVisible);
                row.MinHeight = 0;
                return;
            }

            row.Height = GridLength.Auto;
            row.MinHeight = minHeightWhenVisible;
        }

        public static void RemoveIconsFromLabelControl(FrameworkElement labelControl)
        {
            Panel panel = labelControl as Panel;

            if (panel == null)
                return;

            for (int i = panel.Children.Count - 1; i >= 0; i--)
            {
                if (panel.Children[i] is Image)
                    panel.Children.RemoveAt(i);
                else if (panel.Children[i] is FrameworkElement nested)
                    RemoveIconsFromLabelControl(nested);
            }
        }
    }
}
