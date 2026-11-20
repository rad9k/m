using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace m0.UIWpf
{
    /// <summary>
    /// Application-wide interface scale. The main window binds its content to
    /// <see cref="Scale"/> and keeps the slider outside that transform.
    /// Other windows receive the same scale from the implicit Window style.
    /// Popups are separate windows, so they get their own transform.
    /// </summary>
    public sealed class UiScale : INotifyPropertyChanged
    {
        public static UiScale Instance { get; } = new UiScale();

        public const double MinimumScale = 0.75;
        public const double MaximumScale = 1.75;
        public const double ScaleStep = 0.05;

        // Screen pixels for the AvalonDock splitter column. Layout units are this
        // value divided by the interface scale.
        const double DockSplitterScreenHitThickness = 8;

        static readonly List<LayoutGridResizerControl> LiveSplitters = new List<LayoutGridResizerControl>();
        static bool RefreshingSplitters;

        static readonly DependencyProperty ManagedScaleTransformProperty = DependencyProperty.RegisterAttached(
            "ManagedScaleTransform",
            typeof(ScaleTransform),
            typeof(UiScale),
            new PropertyMetadata(null));

        static readonly DependencyProperty PopupScaleHookedProperty = DependencyProperty.RegisterAttached(
            "PopupScaleHooked",
            typeof(bool),
            typeof(UiScale),
            new PropertyMetadata(false));

        static readonly DependencyProperty WindowScaleStateProperty = DependencyProperty.RegisterAttached(
            "WindowScaleState",
            typeof(WindowScaleState),
            typeof(UiScale),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TrackWindowSizeProperty = DependencyProperty.RegisterAttached(
            "TrackWindowSize",
            typeof(bool),
            typeof(UiScale),
            new PropertyMetadata(false, OnTrackWindowSizeChanged));

        static readonly Style UnscaledWindowStyle = new Style(typeof(Window));
        static readonly Dictionary<LayoutGridResizerControl, SplitterDragStart> SplitterDragStarts =
            new Dictionary<LayoutGridResizerControl, SplitterDragStart>();

        double scale = 1.0;

        static UiScale()
        {
            EventManager.RegisterClassHandler(
                typeof(Popup),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnPopupLoaded));
            EventManager.RegisterClassHandler(
                typeof(Window),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnWindowLoaded));
            EventManager.RegisterClassHandler(
                typeof(LayoutGridResizerControl),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnSplitterLoaded));
            EventManager.RegisterClassHandler(
                typeof(LayoutGridResizerControl),
                Thumb.DragStartedEvent,
                new DragStartedEventHandler(OnSplitterDragStarted));
        }

        UiScale()
        {
        }

        public double Scale
        {
            get => scale;
            set
            {
                double clampedScale = value;
                if (clampedScale < MinimumScale)
                    clampedScale = MinimumScale;
                if (clampedScale > MaximumScale)
                    clampedScale = MaximumScale;

                if (Math.Abs(scale - clampedScale) < 0.0001)
                    return;

                scale = clampedScale;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scale)));
                UpdateLiveSplitterHitMargins();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static bool GetTrackWindowSize(DependencyObject element)
        {
            return (bool)element.GetValue(TrackWindowSizeProperty);
        }

        public static void SetTrackWindowSize(DependencyObject element, bool value)
        {
            element.SetValue(TrackWindowSizeProperty, value);
        }

        static void OnTrackWindowSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Window window = sender as Window;
            if (window == null || (bool)e.NewValue != true)
                return;

            if (window.GetValue(WindowScaleStateProperty) is WindowScaleState)
                return;

            WindowScaleState state = new WindowScaleState();
            bool widthIsFixed = window.SizeToContent == SizeToContent.Manual || window.SizeToContent == SizeToContent.Height;
            bool heightIsFixed = window.SizeToContent == SizeToContent.Manual || window.SizeToContent == SizeToContent.Width;
            state.AdjustWidth = widthIsFixed && !double.IsNaN(window.Width);
            state.AdjustHeight = heightIsFixed && !double.IsNaN(window.Height);
            if (state.AdjustWidth)
                state.BaseWidth = window.Width;
            if (state.AdjustHeight)
                state.BaseHeight = window.Height;

            window.SetValue(WindowScaleStateProperty, state);
            ApplyWindowSize(window, state);

            PropertyChangedEventHandler scaleChangedHandler = null;
            scaleChangedHandler = (scaleSender, args) =>
            {
                if (args.PropertyName == nameof(Scale))
                    ApplyWindowSize(window, state);
            };
            Instance.PropertyChanged += scaleChangedHandler;
            window.Closed += (closedSender, closedArgs) => Instance.PropertyChanged -= scaleChangedHandler;
        }

        static void ApplyWindowSize(Window window, WindowScaleState state)
        {
            if (state.Applying)
                return;

            state.Applying = true;
            try
            {
                double currentScale = Instance.Scale;
                if (state.AdjustWidth)
                    window.Width = state.BaseWidth * currentScale;
                if (state.AdjustHeight)
                    window.Height = state.BaseHeight * currentScale;
            }
            finally
            {
                state.Applying = false;
            }
        }

        static void OnPopupLoaded(object sender, RoutedEventArgs e)
        {
            Popup popup = sender as Popup;
            if (popup == null || (bool)popup.GetValue(PopupScaleHookedProperty))
                return;

            popup.SetValue(PopupScaleHookedProperty, true);
            popup.Opened += OnPopupOpened;
            ApplyPopupScale(popup);
        }

        static void OnPopupOpened(object sender, EventArgs e)
        {
            ApplyPopupScale((Popup)sender);
        }

        static void ApplyPopupScale(Popup popup)
        {
            FrameworkElement child = popup.Child as FrameworkElement;
            if (child == null)
                return;

            // A popup inside the scaled tree already receives that scale from WPF.
            // Scaling the child again makes the popup too large, and WPF then
            // slides it to the edge of the screen so that it stays visible.
            if (PopupIsInsideScaledTree(popup))
                ClearManagedScaleTransform(child);
            else
                EnsureScaleTransform(child);

            if (popup.IsOpen)
                popup.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => RepositionOpenPopup(popup)));
        }

        static bool PopupIsInsideScaledTree(Popup popup)
        {
            return VisualCarriesUiScale(popup);
        }

        static bool VisualCarriesUiScale(Visual visual)
        {
            if (visual == null)
                return false;

            PresentationSource source = PresentationSource.FromVisual(visual);
            if (source == null || source.RootVisual == null)
                return false;

            try
            {
                GeneralTransform transform = visual.TransformToAncestor(source.RootVisual);
                Point origin = transform.Transform(new Point(0, 0));
                Point unit = transform.Transform(new Point(1, 0));
                double scaleX = Math.Abs(unit.X - origin.X);
                return Math.Abs(scaleX - Instance.Scale) < 0.05;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        static void ClearManagedScaleTransform(FrameworkElement element)
        {
            if (element.ReadLocalValue(ManagedScaleTransformProperty) == DependencyProperty.UnsetValue)
                return;

            ScaleTransform managedTransform = element.GetValue(ManagedScaleTransformProperty) as ScaleTransform;
            if (element.LayoutTransform == managedTransform)
                element.LayoutTransform = null;
            else if (element.LayoutTransform is TransformGroup transformGroup && managedTransform != null)
            {
                transformGroup.Children.Remove(managedTransform);
                if (transformGroup.Children.Count == 1)
                    element.LayoutTransform = transformGroup.Children[0];
                else if (transformGroup.Children.Count == 0)
                    element.LayoutTransform = null;
            }

            element.ClearValue(ManagedScaleTransformProperty);
        }

        static void RepositionOpenPopup(Popup popup)
        {
            if (!popup.IsOpen)
                return;

            FrameworkElement child = popup.Child as FrameworkElement;
            if (child == null || child.ActualWidth <= 0 || child.ActualHeight <= 0)
                return;

            Point actualOrigin = child.PointToScreen(new Point(0, 0));
            if (!TryGetExpectedPopupOrigin(popup, child, out Point expectedOrigin))
                return;

            Point childExtent = child.PointToScreen(new Point(child.ActualWidth, child.ActualHeight));
            double childWidth = Math.Abs(childExtent.X - actualOrigin.X);
            double childHeight = Math.Abs(childExtent.Y - actualOrigin.Y);
            if (PointIsNearRectangle(expectedOrigin, actualOrigin, childWidth, childHeight))
                return;

            double deltaX = expectedOrigin.X - actualOrigin.X;
            double deltaY = expectedOrigin.Y - actualOrigin.Y;
            double dipPerPixelX = 1;
            double dipPerPixelY = 1;
            PresentationSource source = PresentationSource.FromVisual(child);
            if (source != null && source.CompositionTarget != null)
            {
                dipPerPixelX = source.CompositionTarget.TransformFromDevice.M11;
                dipPerPixelY = source.CompositionTarget.TransformFromDevice.M22;
            }

            double offsetX = deltaX * dipPerPixelX;
            double offsetY = deltaY * dipPerPixelY;
            PlacementMode placement = popup.Placement;
            if (placement != PlacementMode.Mouse &&
                placement != PlacementMode.MousePoint &&
                placement != PlacementMode.Absolute &&
                placement != PlacementMode.AbsolutePoint)
            {
                double currentScale = Instance.Scale;
                if (currentScale > 0.01)
                {
                    offsetX /= currentScale;
                    offsetY /= currentScale;
                }
            }

            popup.HorizontalOffset += offsetX;
            popup.VerticalOffset += offsetY;
        }

        static bool PointIsNearRectangle(Point point, Point origin, double width, double height)
        {
            const double margin = 24;
            return point.X >= origin.X - margin
                && point.X <= origin.X + width + margin
                && point.Y >= origin.Y - margin
                && point.Y <= origin.Y + height + margin;
        }

        static bool TryGetExpectedPopupOrigin(Popup popup, FrameworkElement child, out Point originPhysical)
        {
            originPhysical = new Point();
            PlacementMode placement = popup.Placement;
            if (placement == PlacementMode.Absolute ||
                placement == PlacementMode.AbsolutePoint ||
                placement == PlacementMode.Custom)
                return false;

            if (placement == PlacementMode.Mouse || placement == PlacementMode.MousePoint)
            {
                if (!GetCursorPos(out NativeCursorPoint cursor))
                    return false;

                originPhysical = new Point(cursor.X, cursor.Y);
                return true;
            }

            FrameworkElement target = popup.PlacementTarget as FrameworkElement;
            if (target == null)
                return false;

            Point anchor = target.PointToScreen(new Point(popup.HorizontalOffset, popup.VerticalOffset));
            Point targetExtent = target.PointToScreen(new Point(
                target.ActualWidth + popup.HorizontalOffset,
                target.ActualHeight + popup.VerticalOffset));
            Point childOrigin = child.PointToScreen(new Point(0, 0));
            Point childExtent = child.PointToScreen(new Point(child.ActualWidth, child.ActualHeight));
            double childWidth = childExtent.X - childOrigin.X;
            double childHeight = childExtent.Y - childOrigin.Y;

            switch (placement)
            {
                case PlacementMode.Bottom:
                    originPhysical = new Point(anchor.X, targetExtent.Y);
                    return true;
                case PlacementMode.Top:
                    originPhysical = new Point(anchor.X, anchor.Y - childHeight);
                    return true;
                case PlacementMode.Right:
                    originPhysical = new Point(targetExtent.X, anchor.Y);
                    return true;
                case PlacementMode.Left:
                    originPhysical = new Point(anchor.X - childWidth, anchor.Y);
                    return true;
                case PlacementMode.Center:
                    originPhysical = new Point(
                        (anchor.X + targetExtent.X) / 2 - childWidth / 2,
                        (anchor.Y + targetExtent.Y) / 2 - childHeight / 2);
                    return true;
                case PlacementMode.Relative:
                case PlacementMode.RelativePoint:
                    originPhysical = anchor;
                    return true;
                default:
                    return false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NativeCursorPoint
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out NativeCursorPoint point);

        static void EnsureScaleTransform(FrameworkElement element)
        {
            if (element.ReadLocalValue(ManagedScaleTransformProperty) != DependencyProperty.UnsetValue)
                return;

            ScaleTransform scaleTransform = CreateBoundScaleTransform();
            Transform existingTransform = element.LayoutTransform;
            if (existingTransform == null || existingTransform == Transform.Identity)
                element.LayoutTransform = scaleTransform;
            else
            {
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(existingTransform);
                transformGroup.Children.Add(scaleTransform);
                element.LayoutTransform = transformGroup;
            }

            element.SetValue(ManagedScaleTransformProperty, scaleTransform);
        }

        static ScaleTransform CreateBoundScaleTransform()
        {
            ScaleTransform scaleTransform = new ScaleTransform();
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, CreateScaleBinding());
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, CreateScaleBinding());
            return scaleTransform;
        }

        static Binding CreateScaleBinding()
        {
            return new Binding(nameof(Scale))
            {
                Source = Instance,
                Mode = BindingMode.OneWay
            };
        }

        static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Window window = sender as Window;
            if (!IsDockSplitterPreview(window))
                return;

            // AvalonDock already sizes this window in post-scale screen units.
            // The implicit Window style would scale that size and the preview again.
            if (window.GetValue(WindowScaleStateProperty) is WindowScaleState state)
            {
                if (state.AdjustWidth)
                    window.Width = state.BaseWidth;
                if (state.AdjustHeight)
                    window.Height = state.BaseHeight;
                state.AdjustWidth = false;
                state.AdjustHeight = false;
            }

            window.Style = UnscaledWindowStyle;
            MatchPreviewThicknessToScale(window);
        }

        static bool IsDockSplitterPreview(Window window)
        {
            return window != null
                && window.WindowStyle == WindowStyle.None
                && window.AllowsTransparency
                && window.ShowInTaskbar == false
                && window.ResizeMode == ResizeMode.NoResize
                && window.ShowActivated == false
                && window.Content is Canvas;
        }

        static void MatchPreviewThicknessToScale(Window window)
        {
            double currentScale = Instance.Scale;
            if (currentScale <= 0 || Math.Abs(currentScale - 1) < 0.0001)
                return;

            Canvas canvas = window.Content as Canvas;
            if (canvas == null)
                return;

            foreach (object child in canvas.Children)
            {
                Border preview = child as Border;
                if (preview == null || double.IsNaN(preview.Width) || double.IsNaN(preview.Height))
                    continue;

                if (preview.Width < preview.Height)
                {
                    double extraWidth = preview.Width * (currentScale - 1);
                    preview.Width *= currentScale;
                    if (!double.IsNaN(window.Width))
                        window.Width += extraWidth;
                }
                else if (preview.Height < preview.Width)
                {
                    double extraHeight = preview.Height * (currentScale - 1);
                    preview.Height *= currentScale;
                    if (!double.IsNaN(window.Height))
                        window.Height += extraHeight;
                }
            }
        }

        public static void WidenDockSplitters(DependencyObject root)
        {
            if (RefreshingSplitters || root == null)
                return;

            RefreshingSplitters = true;
            try
            {
                foreach (LayoutGridResizerControl splitter in FindVisualChildren<LayoutGridResizerControl>(root))
                {
                    AttachSplitter(splitter);
                    PaintSplitterGrip(splitter);
                    ApplySplitterHitThickness(splitter);
                }
            }
            finally
            {
                RefreshingSplitters = false;
            }
        }

        static void AttachSplitter(LayoutGridResizerControl splitter)
        {
            if (splitter == null || LiveSplitters.Contains(splitter))
                return;

            LiveSplitters.Add(splitter);
            splitter.Unloaded += OnSplitterUnloaded;
            Panel.SetZIndex(splitter, 100);
            PaintSplitterGrip(splitter);
        }

        static void PaintSplitterGrip(LayoutGridResizerControl splitter)
        {
            Brush gripBrush = splitter.TryFindResource("0GripBrush") as Brush;
            if (gripBrush == null)
                return;

            if (!ReferenceEquals(splitter.Background, gripBrush))
                splitter.SetResourceReference(Control.BackgroundProperty, "0GripBrush");
            if (!ReferenceEquals(splitter.BackgroundWhileDragging, gripBrush))
                splitter.SetResourceReference(LayoutGridResizerControl.BackgroundWhileDraggingProperty, "0GripBrush");
            if (Math.Abs(splitter.OpacityWhileDragging - 1) > 0.001)
                splitter.OpacityWhileDragging = 1;
        }

        static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, childIndex);
                T match = child as T;
                if (match != null)
                    yield return match;

                foreach (T descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        static void OnSplitterLoaded(object sender, RoutedEventArgs e)
        {
            LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
            if (splitter == null || LiveSplitters.Contains(splitter))
                return;

            AttachSplitter(splitter);

            if (!ApplySplitterHitThickness(splitter))
            {
                splitter.Dispatcher.BeginInvoke(
                    new Action(() => ApplySplitterHitThickness(splitter)),
                    DispatcherPriority.Loaded);
            }
        }

        static void OnSplitterUnloaded(object sender, RoutedEventArgs e)
        {
            LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
            if (splitter == null)
                return;

            splitter.Unloaded -= OnSplitterUnloaded;
            LiveSplitters.Remove(splitter);
        }

        static void UpdateLiveSplitterHitMargins()
        {
            LayoutGridResizerControl[] splitters = LiveSplitters.ToArray();
            for (int index = 0; index < splitters.Length; index++)
                ApplySplitterHitThickness(splitters[index]);
        }

        static bool ApplySplitterHitThickness(LayoutGridResizerControl splitter)
        {
            if (splitter == null)
                return false;

            bool? resizesWidth = SplitterResizesWidth(splitter);
            if (resizesWidth == null)
                return false;

            double currentScale = Instance.Scale;
            if (currentScale <= 0)
                currentScale = 1;

            double layoutHitThickness = DockSplitterScreenHitThickness / currentScale;
            DockingManager manager = FindVisualParent<DockingManager>(splitter);
            if (manager != null)
            {
                if (resizesWidth.Value)
                {
                    if (Math.Abs(manager.GridSplitterWidth - layoutHitThickness) > 0.1)
                        manager.GridSplitterWidth = layoutHitThickness;
                }
                else if (Math.Abs(manager.GridSplitterHeight - layoutHitThickness) > 0.1)
                    manager.GridSplitterHeight = layoutHitThickness;
            }

            // The grip's grid slot is the hit target. AvalonDock does not rebuild
            // that slot when GridSplitterWidth changes, so write the row or column directly.
            Grid grid = splitter.Parent as Grid;
            if (grid == null)
                return false;

            if (resizesWidth.Value)
            {
                int column = Grid.GetColumn(splitter);
                if (column < 0 || column >= grid.ColumnDefinitions.Count)
                    return false;

                ColumnDefinition columnDefinition = grid.ColumnDefinitions[column];
                if (!columnDefinition.Width.IsAbsolute || Math.Abs(columnDefinition.Width.Value - layoutHitThickness) > 0.1)
                    columnDefinition.Width = new GridLength(layoutHitThickness);
            }
            else
            {
                int row = Grid.GetRow(splitter);
                if (row < 0 || row >= grid.RowDefinitions.Count)
                    return false;

                RowDefinition rowDefinition = grid.RowDefinitions[row];
                if (!rowDefinition.Height.IsAbsolute || Math.Abs(rowDefinition.Height.Value - layoutHitThickness) > 0.1)
                    rowDefinition.Height = new GridLength(layoutHitThickness);
            }

            if (splitter.Margin != new Thickness(0))
                splitter.Margin = new Thickness(0);
            return true;
        }

        static bool? SplitterResizesWidth(LayoutGridResizerControl splitter)
        {
            if (splitter.Cursor == Cursors.SizeWE)
                return true;
            if (splitter.Cursor == Cursors.SizeNS)
                return false;

            object parent = splitter.Parent;
            if (parent == null)
                return null;

            PropertyInfo orientationProperty = parent.GetType().GetProperty("Orientation");
            if (orientationProperty == null || orientationProperty.PropertyType != typeof(Orientation))
                return null;

            Orientation orientation = (Orientation)orientationProperty.GetValue(parent);
            return orientation == Orientation.Horizontal;
        }

        static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject current = child;
            while (current != null)
            {
                T match = current as T;
                if (match != null)
                    return match;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        static void OnSplitterDragStarted(object sender, DragStartedEventArgs e)
        {
            LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
            if (splitter == null || SplitterDragStarts.ContainsKey(splitter))
                return;

            SplitterDragStart start = CaptureSplitterDragStart(splitter);
            if (start == null)
                return;

            SplitterDragStarts[splitter] = start;
            splitter.DragCompleted += OnSplitterDragCompletedAfterDock;
        }

        static void OnSplitterDragCompletedAfterDock(object sender, DragCompletedEventArgs e)
        {
            LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
            if (splitter == null)
                return;

            splitter.DragCompleted -= OnSplitterDragCompletedAfterDock;
            if (!SplitterDragStarts.TryGetValue(splitter, out SplitterDragStart start))
                return;

            SplitterDragStarts.Remove(splitter);

            // AvalonDock adds the drag in screen units. DockWidth is in pre-scale layout units.
            double currentScale = Instance.Scale;
            if (currentScale <= 0 || Math.Abs(currentScale - 1) < 0.0001)
                return;

            CorrectDockLength(start.Before, "DockWidth", "DockMinWidth", start.BeforeWidth, currentScale);
            CorrectDockLength(start.Before, "DockHeight", "DockMinHeight", start.BeforeHeight, currentScale);
            CorrectDockLength(start.After, "DockWidth", "DockMinWidth", start.AfterWidth, currentScale);
            CorrectDockLength(start.After, "DockHeight", "DockMinHeight", start.AfterHeight, currentScale);
        }

        static SplitterDragStart CaptureSplitterDragStart(LayoutGridResizerControl splitter)
        {
            Panel panel = splitter.Parent as Panel;
            if (panel == null)
                return null;

            int index = panel.Children.IndexOf(splitter);
            if (index <= 0)
                return null;

            object before = ModelOf(panel.Children[index - 1] as FrameworkElement);
            object after = null;
            for (int childIndex = index + 1; childIndex < panel.Children.Count; childIndex++)
            {
                FrameworkElement child = panel.Children[childIndex] as FrameworkElement;
                if (child != null && child.Visibility == Visibility.Visible)
                {
                    after = ModelOf(child);
                    break;
                }
            }

            if (before == null)
                return null;

            SplitterDragStart start = new SplitterDragStart();
            start.Before = before;
            start.After = after;
            TryReadDockLength(before, "DockWidth", out start.BeforeWidth);
            TryReadDockLength(before, "DockHeight", out start.BeforeHeight);
            if (after != null)
            {
                TryReadDockLength(after, "DockWidth", out start.AfterWidth);
                TryReadDockLength(after, "DockHeight", out start.AfterHeight);
            }

            return start;
        }

        static object ModelOf(FrameworkElement element)
        {
            ILayoutControl layoutControl = element as ILayoutControl;
            if (layoutControl == null)
                return null;

            return layoutControl.Model;
        }

        static bool TryReadDockLength(object model, string propertyName, out GridLength length)
        {
            length = new GridLength();
            PropertyInfo property = DockLengthProperty(model, propertyName);
            if (property == null)
                return false;

            length = (GridLength)property.GetValue(model);
            return true;
        }

        static void CorrectDockLength(object model, string lengthPropertyName, string minimumPropertyName, GridLength startLength, double currentScale)
        {
            if (model == null || !startLength.IsAbsolute)
                return;

            PropertyInfo lengthProperty = DockLengthProperty(model, lengthPropertyName);
            if (lengthProperty == null || !lengthProperty.CanWrite)
                return;

            GridLength currentLength = (GridLength)lengthProperty.GetValue(model);
            if (!currentLength.IsAbsolute)
                return;

            double corrected = startLength.Value + (currentLength.Value - startLength.Value) / currentScale;
            PropertyInfo minimumProperty = model.GetType().GetProperty(minimumPropertyName);
            if (minimumProperty != null && minimumProperty.PropertyType == typeof(double))
            {
                double minimum = (double)minimumProperty.GetValue(model);
                if (corrected < minimum)
                    corrected = minimum;
            }

            if (Math.Abs(corrected - currentLength.Value) < 0.01)
                return;

            lengthProperty.SetValue(model, new GridLength(corrected, GridUnitType.Pixel));
        }

        static PropertyInfo DockLengthProperty(object model, string propertyName)
        {
            if (model == null)
                return null;

            PropertyInfo property = model.GetType().GetProperty(propertyName);
            if (property == null || property.PropertyType != typeof(GridLength))
                return null;

            return property;
        }

        sealed class SplitterDragStart
        {
            public object Before;
            public object After;
            public GridLength BeforeWidth;
            public GridLength BeforeHeight;
            public GridLength AfterWidth;
            public GridLength AfterHeight;
        }

        sealed class WindowScaleState
        {
            public double BaseWidth;
            public double BaseHeight;
            public bool AdjustWidth;
            public bool AdjustHeight;
            public bool Applying;
        }
    }

    public sealed class UiScalePercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double scaleValue)
                return Math.Round(scaleValue * 100).ToString(CultureInfo.InvariantCulture) + "%";

            return "100%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
