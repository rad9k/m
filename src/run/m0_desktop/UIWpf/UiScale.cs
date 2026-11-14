using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

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

        double scale = 1.0;

        static UiScale()
        {
            EventManager.RegisterClassHandler(
                typeof(Popup),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnPopupLoaded));
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
            if (child != null)
                EnsureScaleTransform(child);
        }

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
