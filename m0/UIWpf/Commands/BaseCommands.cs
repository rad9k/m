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
            (sender is IVertex && GraphUtil.FindEdgeByToVertex(selectSynchronisedVisualiser.GetAll(false, @"SelectedEdges:\"),(IVertex)sender)!=null && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
             ||
            (sender is IVertex && selectSynchronisedVisualiser.Get(false, @"SelectedEdges:")==(IVertex)sender && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
                ){
                    if (baseSynchronisedVertex.Get(false, @"BaseEdge:\To:") == null) // if Disposed
                    {
                        PlatformClass.RemoveVertexChangeListeners(selectSynchronisedVisualiser, new VertexChange(this.SynchronisedVisualiserChange));
                    }
                    else
                    {
                        IVertex selEdgesFirst = selectSynchronisedVisualiser.Get(false, @"SelectedEdges:\");

                        if (selEdgesFirst != null)
                        {
                            IVertex firstSelectedVertexEdgeTo = selEdgesFirst.Get(false, "To:");

                            if (firstSelectedVertexEdgeTo != null)
                                GraphUtil.ReplaceEdge(baseSynchronisedVertex.Get(false, "BaseEdge:"), "To", firstSelectedVertexEdgeTo);

                            IVertex firstSelectedVertexEdgeMeta = selEdgesFirst.Get(false, "Meta:");

                            if (firstSelectedVertexEdgeMeta != null)
                                GraphUtil.ReplaceEdge(baseSynchronisedVertex.Get(false, "BaseEdge:"), "Meta", firstSelectedVertexEdgeMeta);
                        }                        
                    }
            }                
        }
    }

    public class BaseCommands
    {
        public static IVertex Execute(IVertex baseVertex, IVertex inputVertex)
        {
            ExecuteDialog e = new ExecuteDialog(baseVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(e, FloatingWindowSize.Medium);

            return null;
        }

       public static IVertex NewVertex(IVertex baseVertex,IVertex inputVertex){
            NewVertex d = new NewVertex(baseVertex.Get(false, "To:"));

            MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);

            return null;
        }

        public static IVertex NewVertexBySchema(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex Vertex = baseVertex.Get(false, "To:");
            IVertex MetaVertex = inputVertex;

            IVertex v = VertexOperations.AddInstance(Vertex, MetaVertex);

            if (VertexOperations.GetChildEdges(MetaVertex).Count() > 0)
                MinusZero.Instance.DefaultUserInteraction.EditDialog(v, null);
            else
            {
                NewVertexBySchema d = new NewVertexBySchema(v, MetaVertex);

                MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);
            }

            return null;
        }

        public static IVertex NewEdge(IVertex baseVertex, IVertex inputVertex)
        {
            NewEdge d = new NewEdge(baseVertex.Get(false, "To:"));

            MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);

            return null;
        }

        public static IVertex NewEdgeBySchema(IVertex baseVertex, IVertex inputVertex)
        {
            NewEdgeBySchema d = new NewEdgeBySchema(baseVertex.Get(false, "To:"), inputVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);

            return null;
        }

        public static IVertex NewDiagram(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex dv = VertexOperations.AddInstance(baseVertex.Get(false, "To:"), MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class:Diagram"));

            GraphUtil.CreateOrReplaceEdge(dv, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class:Diagram\CreationPool"), baseVertex.Get(false, "To:"));

            MinusZero.Instance.DefaultUserInteraction.EditDialog(dv, null);           

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

            if (inputVertex.Get(false, "SelectedEdges:")==null || inputVertex.Get(false, "SelectedEdges:").Count() == 0)
                CutPasteStore.Add(baseVertex);
            else
                foreach (IEdge e in inputVertex.Get(false, "SelectedEdges:"))
                    CutPasteStore.Add(e.To);

            return null;
        }

        public static IVertex Paste(IVertex baseVertex, IVertex inputVertex)
        {
            foreach (IVertex v in CutPasteStore)
            {
                if(DoCut)
                    VertexOperations.DeleteOneEdge(v.Get(false, "From:"), v.Get(false, "Meta:"), v.Get(false, "To:"));

                baseVertex.Get(false, "To:").AddEdge(v.Get(false, "Meta:"), v.Get(false, "To:"));
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

          

            IVertex option = MinusZero.Instance.DefaultUserInteraction.SelectDialogButton(info, options, null);

            bool allEdgesDelete = false;


            if (option == null || GeneralUtil.CompareStrings(option.Value, "Cancel"))
                return null;

            if (GeneralUtil.CompareStrings(option.Value, "Remove from repository"))
                allEdgesDelete = true;

            if (inputVertex.Get(false, "SelectedEdges:").Count() == 0)
                if (allEdgesDelete)
                    //VertexOperations.DeleteAllInOutEdges(baseVertex.Get(false, "To:"));
                    baseVertex.Get(false, "To:").Destroy();
                else
                    VertexOperations.DeleteOneEdge(baseVertex.Get(false, "From:"), baseVertex.Get(false, "Meta:"), baseVertex.Get(false, "To:"));
            else
            {
                IList<IEdge> selected = GeneralUtil.CreateAndCopyList(inputVertex.Get(false, "SelectedEdges:"));
                foreach (IEdge v in selected)
                    if (allEdgesDelete)
                        //VertexOperations.DeleteAllInOutEdges(v.To.Get(false, "To:"));
                        v.To.Get(false, "To:").Destroy();
                    else
                        VertexOperations.DeleteOneEdge(v.To.Get(false, "From:"), v.To.Get(false, "Meta:"), v.To.Get(false, "To:"));
            }

            return null;
        }

        public static IVertex Query(IVertex baseVertex, IVertex inputVertex)
        {
            QueryDialog d = new QueryDialog(baseVertex.Get(false, "To:"));

            MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(d, FloatingWindowSize.Small);

            return null;
        }

        public static IVertex Open(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex DefaultVis;

            DefaultVis=baseVertex.Get(false, @"Meta:\$DefaultOpenVisualiser:");

            if(DefaultVis==null)
                DefaultVis=baseVertex.Get(false, @"To:\$Is:\$Is:\$DefaultOpenVisualiser:"); // yes. bad but it is

            if (DefaultVis == null)
                DefaultVis = baseVertex.Get(false, @"Meta:\$EdgeTarget:\$DefaultEditVisualiser:");

            if (DefaultVis == null)
                DefaultVis = MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form");

            if (GeneralUtil.CompareStrings(DefaultVis.Value, "Diagram"))
                return OpenDiagram(baseVertex, DefaultVis);

            return OpenVisualiser(baseVertex,DefaultVis);
        }

        public static IVertex OpenFormVisualiser(IVertex baseVertex)
        {            
            return OpenVisualiser(baseVertex, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form"));
        }

        public static IVertex OpenDiagram(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(baseVertex.Get(false, "To:"));

            //GraphUtil.ReplaceEdge(sv.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContent(sv);

            return null;
        }

        public static IVertex OpenVisualiser(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex);
            
            //GraphUtil.ReplaceEdge(sv.Vertex, "BaseEdge", baseVertex); very wrong, will not work with GraphVisualiser becouse of changing its BaseEdge
            Edge.CopyAndReplaceEdgeVertexByEdgeVertex(sv.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContent(sv);

            return null;            
        }

        public static IVertex OpenMetaVisualiser(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex);

            GraphUtil.ReplaceEdge(sv.Vertex.Get(false, "BaseEdge:"), "To", baseVertex.Get(false, "Meta:"));            

            MinusZero.Instance.DefaultUserInteraction.ShowContent(sv);

            return null;
        }

        public static IVertex OpenVisualiserFloating(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex);

            //GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", baseVertex);

            Edge.CopyAndReplaceEdgeVertexByEdgeVertex(pc.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(pc, FloatingWindowSize.Medium);

            return null;
        }

        public static IVertex OpenVisualiserSelectedBase(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get(false, "VisualiserClass:"));

            GraphUtil.ReplaceEdge(pc.Vertex.Get(false, "BaseEdge:"),"Meta", baseVertex.Get(false, "Meta:"));

            GraphUtil.ReplaceEdge(pc.Vertex.Get(false, "BaseEdge:"), "To", baseVertex.Get(false, "To:"));

            IVertex synchronisedVisualiser = inputVertex.Get(false, "SynchronisedVisualiser:");

            BaseSelectedSynchronisedHelper helper = new BaseSelectedSynchronisedHelper(pc.Vertex, synchronisedVisualiser);

            PlatformClass.RegisterVertexChangeListeners(synchronisedVisualiser,new VertexChange(helper.SynchronisedVisualiserChange), new string[]{"BaseEdge","SelectedEdges"});
            
            IVertex firstSelectedVertex = synchronisedVisualiser.Get(false, @"SelectedEdges:\");

            if (firstSelectedVertex != null)
                GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", firstSelectedVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContent(pc);

            return null;     
        }

        public static IVertex OpenVisualiserSelectedSelected(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get(false, "VisualiserClass:"));

            GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", baseVertex);

            GraphUtil.ReplaceEdge(pc.Vertex, "SelectedEdges", inputVertex.Get(false, @"SynchronisedVisualiser:\SelectedEdges:"));

            MinusZero.Instance.DefaultUserInteraction.ShowContent(pc);

            return null;     
        }


    }
}
