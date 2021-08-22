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
using System.Windows;
using System.Windows.Media;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Commands;
using m0.Graph.ExecutionFlow;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers.Helper
{
    public class ListVisualiserHelper : AtomVisualiserHelper
    {
        IListVisualiser listVisualiser;

        public ListVisualiserHelper(IVertex _visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement)
            : this(_visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  true,
                  new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" },
                  "ListVisualiser")
        {

        }

        public ListVisualiserHelper(IVertex visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement,
            bool _dndSupport,
            IList<string> _scopeQueries,
            string _scopeQueriesName)
            : base(visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  _dndSupport,
                  _scopeQueries,
                  _scopeQueriesName)
        {
            listVisualiser = (IListVisualiser)_visualiser;
        }

        protected new INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            IVertex sourceVertex = exe.Stack.Get(false, @"event:\Source:");

            if (GraphUtil.ExistQueryIn(sourceVertex, "ZoomVisualiserContent", null))
            {
                listVisualiser.ZoomVisualiserContentChange();
                return exe.Stack;
            }

            if (GraphUtil.ExistQueryIn(sourceVertex, "SelectedEdges", null))
            {
                listVisualiser.SelectedVerticesUpdated();

                return exe.Stack;
            }                

            visualiser.UpdateBaseEdge();

            return exe.Stack;
        }
    }
}
