using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Foundation;
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
            if (!MinusZero.Instance.IsGUIDragging)
            {
                MinusZero.Instance.IsGUIDragging = true;
                dragData.SetData("DragSource", o);
                DragDrop.DoDragDrop(o, dragData, DragDropEffects.Copy);                
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

                if (ee != null)
                {
                    GraphUtil.CreateOrReplaceEdge(baseEdge.Get("To:"), r.Get(@"System\Meta\ZeroTypes\Edge\Meta"), ee.To.Get("Meta:"));
                    GraphUtil.CreateOrReplaceEdge(baseEdge.Get("To:"), r.Get(@"System\Meta\ZeroTypes\Edge\To"), ee.To.Get("To:"));                    

                    //GraphUtil.ReplaceEdge(baseEdge.Get("To:"), "Meta", ee.To.Get("Meta:"));
                    //GraphUtil.ReplaceEdge(baseEdge.Get("To:"), "To", ee.To.Get("To:"));
                }

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges(dndVertex);
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

                IEdge ee = dndVertex.FirstOrDefault();

                if (ee != null)
                {
                    IVertex toMeta = r.Get(@"System\Meta\ZeroTypes\Edge\To");
                    GraphUtil.CreateOrReplaceEdge(baseEdge.Get("From:"), baseEdge.Get("Meta:"), ee.To.Get("To:"));
                    GraphUtil.CreateOrReplaceEdge(baseEdge, toMeta, ee.To.Get("To:")); // this is needed for some update scenarios
                }
             

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges(dndVertex);
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
                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                IVertex maxCardinality = metaVertex.Get("$MaxCardinality:");

                if (maxCardinality != null && (GraphUtil.GetIntegerValue(maxCardinality) == -1 || GraphUtil.GetIntegerValue(maxCardinality) > 1)) // ADD
                    foreach (IEdge ee in dndVertex)
                        baseVertex.AddEdge(metaVertex, ee.To.Get("To:"));
                else // REPLACE
                    GraphUtil.ReplaceEdge(baseVertex, metaVertex, dndVertex.First().To.Get("To:"));
                    

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges(dndVertex);
            }

            MinusZero.Instance.IsGUIDragging = false;
        }

        public static void DoDrop(object orgin, IVertex baseVertex, DragEventArgs e)
        {
            object sender = e.Data.GetData("DragSource");

            if (sender==null || orgin == sender)
                return;

            bool doCopy = false;

            if(GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(@"User\CurrentUser:\Settings:\CopyOnDragAndDrop:").Value, "True"))
                doCopy=true;


            if (e.Data.GetDataPresent("Vertex"))
            {
                IVertex dndVertex = e.Data.GetData("Vertex") as IVertex;

                foreach (IEdge ee in dndVertex)
                    if (doCopy)
                        baseVertex.AddEdge(ee.To.Get("Meta:"), ee.To.Get("To:"));
                    else
                    {
                        if (ee.To.Get("To:") != baseVertex) // do not want to cut and paste to itself
                        {
                            GraphUtil.DeleteEdge(ee.To.Get("From:"), ee.To.Get("Meta:"), ee.To.Get("To:"));
                            baseVertex.AddEdge(ee.To.Get("Meta:"), ee.To.Get("To:"));
                        }
                    }                        

                if (sender is IHasSelectableEdges)
                    ((IHasSelectableEdges)sender).UnselectAllSelectedEdges();

                GraphUtil.RemoveAllEdges(dndVertex);
            }

            MinusZero.Instance.IsGUIDragging = false;
        }        
    }
}
