using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Controls;
using m0.Util;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
    }
}
