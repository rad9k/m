using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public interface IZoomScrollView
    {
        void SetHorizontalAxisDecorator(IZoomScrollViewAxisDecorator decorator);

        void SetVerticalAxisDecorator(IZoomScrollViewAxisDecorator decorator);

        void SetMainContent(FrameworkElement control);

        void SetDownContent(FrameworkElement downDecoratorContent, FrameworkElement downMainContent);

        void SetHost(IZoomScrollViewerHost host);  
        
        double InitialDownHeight { get; set; }

        void SetLeftDownCornerControl(FrameworkElement control);
    }
}
