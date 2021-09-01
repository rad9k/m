using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using m0.Foundation;
using m0.UIWpf.Visualisers;
using m0.Graph;
using m0.ZeroTypes;
using m0.Util;
using m0.Graph.ExecutionFlow;

namespace m0.UIWpf
{
    public class VisualiserTransactedEditWrapper : VisualiserEditWrapper
    {
        public VisualiserTransactedEditWrapper()
        {
            TriggerNewTransaction = true;
        }
    }
}
