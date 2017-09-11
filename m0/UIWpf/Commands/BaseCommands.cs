using m0.Foundation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Graph;
using m0.ZeroTypes;
using m0.Util;
using m0.UIWpf.Dialog;

namespace m0.UIWpf.Commands
{
    class BaseSelectedSynchronisedHelper
    {
        IVertex baseSynchronisedVertex;
        IVertex selectSynchronisedVisualiser;

        public BaseSelectedSynchronisedHelper(IVertex BaseSynchronisedVertex, IVertex SelectSynchronisedVisualiser)
        {
            this.baseSynchronisedVertex = BaseSynchronisedVertex;
            this.selectSynchronisedVisualiser = SelectSynchronisedVisualiser;
        }

        public void SynchronisedVisualiserChange(object sender, VertexChangeEventArgs e){
            if (
                ((sender == selectSynchronisedVisualiser) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
            ||
            (sender is IVertex && GraphUtil.FindEdgeByToVertex(selectSynchronisedVisualiser.GetAll(@"SelectedEdges:\"),(IVertex)sender)!=null && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
             ||
            (sender is IVertex && selectSynchronisedVisualiser.Get(@"SelectedEdges:")==(IVertex)sender && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
                ){
                    if (baseSynchronisedVertex.Get(@"BaseEdge:\To:") == null) // if Disposed
                    {
                        PlatformClass.RemoveVertexChangeListeners(selectSynchronisedVisualiser, new VertexChange(this.SynchronisedVisualiserChange));
                    }
                    else
                    {
                        IVertex selEdgesFirst = selectSynchronisedVisualiser.Get(@"SelectedEdges:\");

                        if (selEdgesFirst != null)
                        {
                            IVertex firstSelectedVertexEdgeTo = selEdgesFirst.Get("To:");

                            if (firstSelectedVertexEdgeTo != null)
                                GraphUtil.ReplaceEdge(baseSynchronisedVertex.Get("BaseEdge:"), "To", firstSelectedVertexEdgeTo);

                            IVertex firstSelectedVertexEdgeMeta = selEdgesFirst.Get("Meta:");

                            if (firstSelectedVertexEdgeMeta != null)
                                GraphUtil.ReplaceEdge(baseSynchronisedVertex.Get("BaseEdge:"), "Meta", firstSelectedVertexEdgeMeta);
                        }                        
                    }
            }                
        }
    }

    public class BaseCommands
    {
        public static IVertex NewVertex(IVertex baseVertex,IVertex inputVertex){
            NewVertex d = new NewVertex(baseVertex.Get("To:"));

            MinusZero.Instance.DefaultShow.ShowContentFloating(d);

            return null;
        }

        public static IVertex NewVertexBySchema(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex Vertex = baseVertex.Get("To:");
            IVertex MetaVertex = inputVertex;

            IVertex v = VertexOperations.AddInstance(Vertex, MetaVertex);

            if (VertexOperations.GetChildEdges(MetaVertex).Count() > 0)
                MinusZero.Instance.DefaultShow.EditDialog(v, null);
            else
            {
                NewVertexBySchema d = new NewVertexBySchema(v, MetaVertex);

                MinusZero.Instance.DefaultShow.ShowContentFloating(d);
            }

            return null;
        }

        public static IVertex NewEdge(IVertex baseVertex, IVertex inputVertex)
        {
            NewEdge d = new NewEdge(baseVertex.Get("To:"));

            MinusZero.Instance.DefaultShow.ShowContentFloating(d);

            return null;
        }

        public static IVertex NewEdgeBySchema(IVertex baseVertex, IVertex inputVertex)
        {
            NewEdgeBySchema d = new NewEdgeBySchema(baseVertex.Get("To:"), inputVertex);

            MinusZero.Instance.DefaultShow.ShowContentFloating(d);

            return null;
        }

        public static IVertex NewDiagram(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex dv = VertexOperations.AddInstance(baseVertex.Get("To:"), MinusZero.Instance.Root.Get(@"System\Meta\Visualiser\Class:Diagram"));

            GraphUtil.CreateOrReplaceEdge(dv, MinusZero.Instance.Root.Get(@"System\Meta\Visualiser\Class:Diagram\CreationPool"), baseVertex.Get("To:"));

            MinusZero.Instance.DefaultShow.EditDialog(dv, null);           

            return null;
        }

        protected static IList<IVertex> CutPasteStore = new List<IVertex>();

        protected static bool DoCut;
 
        public static IVertex Cut(IVertex baseVertex, IVertex inputVertex)
        {
            Copy(baseVertex, inputVertex);
            
            DoCut = true;

            return null;
        }

        public static IVertex Copy(IVertex baseVertex, IVertex inputVertex)
        {
            DoCut = false;

            CutPasteStore.Clear();

            if (inputVertex.Get("SelectedEdges:").Count() == 0)
                CutPasteStore.Add(baseVertex);
            else
                foreach (IEdge e in inputVertex.Get("SelectedEdges:"))
                    CutPasteStore.Add(e.To);

            return null;
        }

        public static IVertex Paste(IVertex baseVertex, IVertex inputVertex)
        {
            foreach (IVertex v in CutPasteStore)
            {
                if(DoCut)
                    VertexOperations.DeleteOneEdge(v.Get("From:"), v.Get("Meta:"), v.Get("To:"));

                baseVertex.Get("To:").AddEdge(v.Get("Meta:"), v.Get("To:"));
            }

            return null;
        }

        public static IVertex Delete(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex info = m0.MinusZero.Instance.CreateTempVertex();
            info.Value = "DELETE vertex";

            IVertex options = m0.MinusZero.Instance.CreateTempVertex();

            options.AddVertex(null, "Edge delete");
            options.AddVertex(null, "Remove from repository");
            options.AddVertex(null, "Cancel");

          

            IVertex option = MinusZero.Instance.DefaultShow.SelectDialogButton(info, options, null);

            bool allEdgesDelete = false;


            if (option == null || GeneralUtil.CompareStrings(option.Value, "Cancel"))
                return null;

            if (GeneralUtil.CompareStrings(option.Value, "Remove from repository"))
                allEdgesDelete = true;

            if (inputVertex.Get("SelectedEdges:").Count() == 0)
                if (allEdgesDelete)
                    VertexOperations.DeleteAllEdges(baseVertex.Get("To:"));
                else
                    VertexOperations.DeleteOneEdge(baseVertex.Get("From:"), baseVertex.Get("Meta:"), baseVertex.Get("To:"));
            else
            {
                IList<IEdge> selected=GeneralUtil.CreateAndCopyList(inputVertex.Get("SelectedEdges:"));
                foreach (IEdge v in selected)
                    if(allEdgesDelete)
                        VertexOperations.DeleteAllEdges(v.To.Get("To:"));
                    else
                        VertexOperations.DeleteOneEdge(v.To.Get("From:"), v.To.Get("Meta:"), v.To.Get("To:"));
            }

            return null;
        }

        public static IVertex Query(IVertex baseVertex, IVertex inputVertex)
        {
            QueryDialog d = new QueryDialog(baseVertex.Get("To:"));

            MinusZero.Instance.DefaultShow.ShowContentFloating(d);

            return null;
        }

        public static IVertex Open(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex DefaultVis;

            DefaultVis=baseVertex.Get(@"Meta:\$DefaultOpenVisualiser:");

            if(DefaultVis==null)
                DefaultVis=baseVertex.Get(@"To:\$Is:\$Is:\$DefaultOpenVisualiser:"); // yes. bad but it is

            if (DefaultVis == null)
                DefaultVis = baseVertex.Get(@"Meta:\$EdgeTarget:\$DefaultEditVisualiser:");

            if (DefaultVis == null)
                DefaultVis = MinusZero.Instance.Root.Get(@"System\Meta\Visualiser\Form");

            if (GeneralUtil.CompareStrings(DefaultVis.Value, "Diagram"))
                return OpenDiagram(baseVertex, DefaultVis);

            return OpenVisualiser(baseVertex,DefaultVis);
        }

        public static IVertex OpenFormVisualiser(IVertex baseVertex)
        {            
            return OpenVisualiser(baseVertex, MinusZero.Instance.Root.Get(@"System\Meta\Visualiser\Form"));
        }

        public static IVertex OpenDiagram(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(baseVertex.Get("To:"));

            //GraphUtil.ReplaceEdge(sv.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultShow.ShowContent(sv);

            return null;
        }

        public static IVertex OpenVisualiser(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex);
            
            //GraphUtil.ReplaceEdge(sv.Vertex, "BaseEdge", baseVertex); very wrong, will not work with GraphVisualiser becouse of changing its BaseEdge
            Edge.CopyAndReplaceEdge(sv.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultShow.ShowContent(sv);

            return null;            
        }

        public static IVertex OpenMetaVisualiser(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex);

            GraphUtil.ReplaceEdge(sv.Vertex.Get("BaseEdge:"), "To", baseVertex.Get("Meta:"));            

            MinusZero.Instance.DefaultShow.ShowContent(sv);

            return null;
        }

        public static IVertex OpenVisualiserFloating(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex);

            //GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", baseVertex);

            Edge.CopyAndReplaceEdge(pc.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultShow.ShowContentFloating(pc);

            return null;
        }

        public static IVertex OpenVisualiserSelectedBase(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get("VisualiserClass:"));

            GraphUtil.ReplaceEdge(pc.Vertex.Get("BaseEdge:"),"Meta", baseVertex.Get("Meta:"));

            GraphUtil.ReplaceEdge(pc.Vertex.Get("BaseEdge:"), "To", baseVertex.Get("To:"));

            IVertex synchronisedVisualiser = inputVertex.Get("SynchronisedVisualiser:");

            BaseSelectedSynchronisedHelper helper = new BaseSelectedSynchronisedHelper(pc.Vertex, synchronisedVisualiser);

            PlatformClass.RegisterVertexChangeListeners(synchronisedVisualiser,new VertexChange(helper.SynchronisedVisualiserChange), new string[]{"BaseEdge","SelectedEdges"});
            
            IVertex firstSelectedVertex = synchronisedVisualiser.Get(@"SelectedEdges:\");

            if (firstSelectedVertex != null)
                GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", firstSelectedVertex);

            MinusZero.Instance.DefaultShow.ShowContent(pc);

            return null;     
        }

        public static IVertex OpenVisualiserSelectedSelected(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get("VisualiserClass:"));

            GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", baseVertex);

            GraphUtil.ReplaceEdge(pc.Vertex, "SelectedEdges", inputVertex.Get(@"SynchronisedVisualiser:\SelectedEdges:"));

            MinusZero.Instance.DefaultShow.ShowContent(pc);

            return null;     
        }


    }
}
