using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Foundation;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.UIWpf.Commands
{
    public class Dnd
    {
        public static double MinimumHorizontalDragDistance;
            
        public static double MinimumVerticalDragDistance;

        public static void DoDragDrop(DependencyObject o, DataObject dragData)
        {
            if (MinusZero.Instance.IsGUIDragging)
                return;

            MinusZero.Instance.IsGUIDragging = true;
            dragData.SetData("DragSource", o);

            try
            {
                DragDrop.DoDragDrop(o, dragData, DragDropEffects.Copy);
            }
            finally
            {
                MinusZero.Instance.IsGUIDragging = false;
            }
        }

        public static void DoDropForEdgeVisualiser(object orgin, IVertex baseEdge, DragEventArgs e)
        {
            IVertex r = MinusZero.Instance.Root;

            object sender = e.Data.GetData("DragSource");

            if (sender == null || orgin == sender)
                return;

            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                IEdge ee = dndVertex.FirstOrDefault();

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                if (ee != null)
                {
                    GraphUtil.CreateOrReplaceEdge(baseEdge.Get(false, "To:"), r.Get(false, @"System\Meta\ZeroTypes\Edge\From"), ee.To.Get(false, "From:"));
                    GraphUtil.CreateOrReplaceEdge(baseEdge.Get(false, "To:"), r.Get(false, @"System\Meta\ZeroTypes\Edge\Meta"), ee.To.Get(false, "Meta:"));
                    GraphUtil.CreateOrReplaceEdge(baseEdge.Get(false, "To:"), r.Get(false, @"System\Meta\ZeroTypes\Edge\To"), ee.To.Get(false, "To:"));
                 
                    //GraphUtil.ReplaceEdge(baseEdge.Get(false, "To:"), "Meta", ee.To.Get(false, "Meta:"));
                    //GraphUtil.ReplaceEdge(baseEdge.Get(false, "To:"), "To", ee.To.Get(false, "To:"));
                }

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(dndVertex);

                dndVertex.RemoveExternalReference();

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

            }

            MinusZero.Instance.IsGUIDragging = false;
        }

        public static void DoDropForVertexVisualiser(object orgin, IVertex baseEdge, DragEventArgs e)
        {
            IVertex r = MinusZero.Instance.Root;

            object sender = e.Data.GetData("DragSource");

            if (sender == null || orgin == sender)
                return;                    

            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                IEdge ee = dndVertex.FirstOrDefault();

                if (ee != null)
                {
                    IVertex toMeta = r.Get(false, @"System\Meta\ZeroTypes\Edge\To");
                    GraphUtil.CreateOrReplaceEdge(baseEdge, toMeta, ee.To.Get(false, "To:")); // this is needed for some update scenarios ??? YYY
                    GraphUtil.CreateOrReplaceEdge(baseEdge.Get(false, "From:"), baseEdge.Get(false, "Meta:"), ee.To.Get(false, "To:"));
                    
                }
             

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(dndVertex);

                dndVertex.RemoveExternalReference();

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }

            MinusZero.Instance.IsGUIDragging = false;
        }

        public static void DoFormDrop(object orgin, IVertex baseVertex, IVertex metaVertex, DragEventArgs e)
        {
            object sender = e.Data.GetData("DragSource");

            if (sender == null || orgin == sender)
                return;

            if (e.Data.GetDataPresent("Vertex"))
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                IVertex maxCardinality = metaVertex.Get(false, "$MaxCardinality:");

                if (maxCardinality != null && (GraphUtil.GetIntegerValue(maxCardinality) == -1 || GraphUtil.GetIntegerValue(maxCardinality) > 1)) // ADD
                    foreach (IEdge ee in dndVertex)
                        baseVertex.AddEdge(metaVertex, ee.To.Get(false, "To:"));
                else // REPLACE
                    GraphUtil.ReplaceEdge(baseVertex, metaVertex, dndVertex.First().To.Get(false, "To:"));
                    

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(dndVertex);

                dndVertex.RemoveExternalReference();

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }

            MinusZero.Instance.IsGUIDragging = false;
        }

        public static void DoDrop(object orgin, IVertex baseVertex, DragEventArgs e)
        {
            object sender = e.Data.GetData("DragSource");

            if (sender == null /*|| orgin == sender*/)
            {
                MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.Dnd.DoDrop",
                    "skip=nullSender baseVertex=" + FormatVertexForLog(baseVertex));
                return;
            }

            bool doCopy = false;

            IVertex copyOnDragAndDropSetting = MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\CopyOnDragAndDrop:");

            if (GeneralUtil.CompareStrings(copyOnDragAndDropSetting.Value, "True"))
                doCopy = true;

            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.Dnd.DoDrop",
                    "enter doCopy=" + doCopy
                    + " senderType=" + sender.GetType().Name
                    + " payloadCount=" + (dndVertex == null ? -1 : dndVertex.Count())
                    + " dropBaseVertex=" + FormatVertexForLog(baseVertex));

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                foreach (IEdge ee in dndVertex)
                {
                    IVertex eeFrom = ee.To.Get(false, "From:");
                    IVertex eeMeta = ee.To.Get(false, "Meta:");
                    IVertex eeTo   = ee.To.Get(false, "To:");

                    bool selfDrop = (eeTo == baseVertex);

                    MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.Dnd.DoDrop",
                        "edge doCopy=" + doCopy
                        + " selfDrop=" + selfDrop
                        + " From=" + FormatVertexForLog(eeFrom)
                        + " Meta=" + FormatVertexForLog(eeMeta)
                        + " To=" + FormatVertexForLog(eeTo)
                        + " dropBaseVertex=" + FormatVertexForLog(baseVertex));

                    if (doCopy)
                    {
                        baseVertex.AddEdge(eeMeta, eeTo);
                    }
                    else
                    {
                        if (!selfDrop) // do not want to cut and paste to itself
                        {
                            baseVertex.AddEdge(eeMeta, eeTo);
                            GraphUtil.DeleteEdge(eeFrom, eeMeta, eeTo); // YYY
                        }
                    }
                }

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(dndVertex);

                dndVertex.RemoveExternalReference();

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.Dnd.DoDrop",
                    "done Interaction.EndInteractionWithGraph"
                    + " dropBaseVertex=" + FormatVertexForLog(baseVertex));
            }

            MinusZero.Instance.IsGUIDragging = false;
        }

        private static string FormatVertexForLog(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            if (vertex.DisposedState != DisposeStateEnum.Live)
                return "<disposed:" + vertex.DisposedState + ">";

            object value = vertex.Value;

            if (value == null)
                return "<nullValue@" + vertex.GetHashCode() + ">";

            string text = value.ToString();

            if (text.Length > 48)
                text = text.Substring(0, 48) + "...";

            return text + "@" + vertex.GetHashCode();
        }        
    }
}
