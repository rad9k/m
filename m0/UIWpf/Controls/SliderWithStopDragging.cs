using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace m0.UIWpf.Controls
{
    
    public class SliderWithStopDragging : Slider
    {
        public bool IsDragging { get; protected set; }

        public bool FireValueChangedOnlyIfDraggingFinished;

        protected override void OnThumbDragCompleted(System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            IsDragging = false;
            base.OnThumbDragCompleted(e);

            OnValueChanged(this.Value,this.Value);
        }

        protected override void OnThumbDragStarted(System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            IsDragging = true;
            base.OnThumbDragStarted(e);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (FireValueChangedOnlyIfDraggingFinished)
            {
                if (!IsDragging)
                {
                    base.OnValueChanged(oldValue, newValue);
                }
            }else
                base.OnValueChanged(oldValue, newValue);
        }

    }
}
