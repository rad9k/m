using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace m0.UIWpf.Visualisers.Method
{
    public static class MethodRunSignal
    {
        static readonly Color StartedColor = Color.FromRgb(0xFF, 0x9F, 0x1C);
        static readonly Color ReturnedColor = Color.FromRgb(0x2E, 0xC2, 0x7E);
        static readonly Color FailedColor = Color.FromRgb(0xE0, 0x1B, 0x24);

        sealed class SignalState
        {
            public int Generation;
        }

        static readonly ConditionalWeakTable<Button, SignalState> States =
            new ConditionalWeakTable<Button, SignalState>();

        public static void ShowStarted(Button button)
        {
            Apply(button, "…", StartedColor, false);

            // The click handler keeps the UI thread inside the method call,
            // so the started color has to be painted before that call begins.
            try
            {
                button.Dispatcher.Invoke(
                    new Action(delegate { }),
                    DispatcherPriority.Render);
            }
            catch
            {
            }
        }

        public static void ShowReturned(Button button)
        {
            Apply(button, "ok", ReturnedColor, true);
        }

        public static void ShowFailed(Button button)
        {
            Apply(button, "err", FailedColor, false);
        }

        static void Apply(Button button, string text, Color color, bool restoreAfterFade)
        {
            SignalState state = States.GetOrCreateValue(button);
            int generation = ++state.Generation;

            SolidColorBrush brush = new SolidColorBrush(color);
            Border capsule = CreateCapsule(text, brush);

            button.Background = brush;
            button.Foreground = Brushes.White;
            button.Content = capsule;

            if (!restoreAfterFade)
                return;

            DoubleAnimation contentFade = new DoubleAnimation();
            contentFade.From = 1;
            contentFade.To = 0;
            contentFade.BeginTime = TimeSpan.FromMilliseconds(400);
            contentFade.Duration = new Duration(TimeSpan.FromMilliseconds(350));
            contentFade.Completed += delegate
            {
                try
                {
                    if (state.Generation != generation)
                        return;

                    capsule.BeginAnimation(UIElement.OpacityProperty, null);
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    button.ClearValue(Control.BackgroundProperty);
                    button.ClearValue(Control.ForegroundProperty);
                    button.Content = "run";
                }
                catch
                {
                }
            };

            ColorAnimation backgroundFade = new ColorAnimation();
            backgroundFade.To = Colors.Transparent;
            backgroundFade.BeginTime = TimeSpan.FromMilliseconds(400);
            backgroundFade.Duration = new Duration(TimeSpan.FromMilliseconds(350));

            capsule.BeginAnimation(UIElement.OpacityProperty, contentFade);
            brush.BeginAnimation(SolidColorBrush.ColorProperty, backgroundFade);
        }

        static Border CreateCapsule(string text, Brush background)
        {
            TextBlock label = new TextBlock();
            label.Text = text;
            label.Foreground = Brushes.White;
            label.FontWeight = FontWeights.SemiBold;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.TextAlignment = TextAlignment.Center;

            Border capsule = new Border();
            capsule.Background = background;
            capsule.CornerRadius = new CornerRadius(2);
            capsule.Padding = new Thickness(8, 2, 8, 2);
            capsule.HorizontalAlignment = HorizontalAlignment.Stretch;
            capsule.VerticalAlignment = VerticalAlignment.Stretch;
            capsule.Child = label;

            return capsule;
        }
    }
}
