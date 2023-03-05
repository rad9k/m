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
using m0.User.Process.UX;

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
                  //      PlatformClass.RemoveVertexChangeListeners(selectSynchronisedVisualiser, new VertexChange(this.SynchronisedVisualiserChange));
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

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex v;

            IVertex targetVertex = GraphUtil.GetQueryOutFirst(MetaVertex, "$EdgeTarget", null);

            if(targetVertex != null)            
                v = VertexOperations.AddInstance(Vertex, targetVertex, MetaVertex);
            else
                v = VertexOperations.AddInstance(Vertex, MetaVertex);

            if (VertexOperations.GetChildEdges(MetaVertex).Count() > 0)
                MinusZero.Instance.DefaultUserInteraction.Edit(v, null);
            else
            {
                NewVertexBySchema d = new NewVertexBySchema(v, MetaVertex);

                MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

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
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IVertex dv = VertexOperations.AddInstance(baseVertex.Get(false, "To:"), MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class:Diagram"));

            GraphUtil.CreateOrReplaceEdge(dv, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class:Diagram\CreationPool"), baseVertex.Get(false, "To:"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            MinusZero.Instance.DefaultUserInteraction.Edit(dv, null);           

            return null;
        }

        public static IVertex NewUX(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex dv = VertexOperations.AddInstance(baseVertex.Get(false, "To:"), MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class:UX"));

            GraphUtil.CreateOrReplaceEdge(dv, MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Class:HasBaseEdge\BaseEdge"), baseVertex.Get(false, "To:"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            MinusZero.Instance.DefaultUserInteraction.Edit(dv, null);

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

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            CutPasteStore.Clear();

            if (inputVertex.Get(false, "SelectedEdges:")==null || inputVertex.Get(false, "SelectedEdges:").Count() == 0)
                CutPasteStore.Add(baseVertex);
            else
                foreach (IEdge e in inputVertex.Get(false, "SelectedEdges:"))
                    CutPasteStore.Add(e.To);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            return null;
        }

        public static IVertex Paste(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            foreach (IVertex v in CutPasteStore)
            {
                if(DoCut)
                    VertexOperations.DeleteOneEdge(v.Get(false, "From:"), v.Get(false, "Meta:"), v.Get(false, "To:"));

                baseVertex.Get(false, "To:").AddEdge(v.Get(false, "Meta:"), v.Get(false, "To:"));
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

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

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (inputVertex.Get(false, "SelectedEdges:").Count() == 0)
                if (allEdgesDelete)
                    //VertexOperations.DeleteAllInOutEdges(baseVertex.Get(false, "To:"));
                    baseVertex.Get(false, "To:").Dispose();
                else
                    VertexOperations.DeleteOneEdge(baseVertex.Get(false, "From:"), baseVertex.Get(false, "Meta:"), baseVertex.Get(false, "To:"));
            else
            {
                IList<IEdge> selected = GeneralUtil.CreateAndCopyList(inputVertex.Get(false, "SelectedEdges:"));
                foreach (IEdge v in selected)
                    if (allEdgesDelete)
                        //VertexOperations.DeleteAllInOutEdges(v.To.Get(false, "To:"));
                        v.To.Get(false, "To:").Dispose();
                    else
                        VertexOperations.DeleteOneEdge(v.To.Get(false, "From:"), v.To.Get(false, "Meta:"), v.To.Get(false, "To:"));
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

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
                DefaultVis = baseVertex.Get(false, @"Meta:\$EdgeTarget:\$DefaultOpenVisualiser:");

            if (DefaultVis == null)
                DefaultVis = baseVertex.Get(false, @"Meta:\$EdgeTarget:\$DefaultEditVisualiser:");

            if (DefaultVis == null)
                DefaultVis = MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form");

            IVertex toReturn = null; 

            if (GeneralUtil.CompareStrings(DefaultVis.Value, "Diagram"))
                toReturn =  OpenDiagram(baseVertex, DefaultVis);
            else
                toReturn =  OpenVisualiser(baseVertex, DefaultVis);

            return toReturn;
        }

        public static IVertex OpenFormVisualiser(IVertex baseVertex)
        {
            IVertex toReturn;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            toReturn = OpenVisualiser(baseVertex, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            return toReturn;
        }

        public static IVertex OpenDiagram(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex visualiserVertex = baseVertex.Get(false, "To:");


            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(visualiserVertex, visualiserVertex);

            //GraphUtil.ReplaceEdge(sv.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContent(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            return null;
        }

        public static IVertex OpenVisualiser(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex, baseVertex);
            
            //Edge.CopyAndReplaceEdgeVertexByEdgeVertex(sv.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContent(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            return null;            
        }

        public static IVertex OpenMetaVisualiser(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IEdge edge = new EasyEdge(null, null, baseVertex.Get(false, "Meta:"));

            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex, edge);

            //GraphUtil.ReplaceEdge(sv.Vertex.Get(false, "BaseEdge:"), "To", baseVertex.Get(false, "Meta:"));            

            MinusZero.Instance.DefaultUserInteraction.ShowContent(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            return null;
        }

        public static IVertex OpenVisualiserFloating(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex, baseVertex);

            //GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", baseVertex);

            //Edge.CopyAndReplaceEdgeVertexByEdgeVertex(pc.Vertex, "BaseEdge", baseVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContentFloating(pc, FloatingWindowSize.Medium);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            return null;
        }

        public static IVertex OpenVisualiserSelectedBase(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, baseVertex.Get(false, "Meta:"), baseVertex.Get(false, "To:"));

            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get(false, "VisualiserClass:"), baseEdgeVertex);

            //GraphUtil.ReplaceEdge(pc.Vertex.Get(false, "BaseEdge:"),"Meta", baseVertex.Get(false, "Meta:"));

            //GraphUtil.ReplaceEdge(pc.Vertex.Get(false, "BaseEdge:"), "To", baseVertex.Get(false, "To:"));

            IVertex synchronisedVisualiser = inputVertex.Get(false, "SynchronisedVisualiser:");

            BaseSelectedSynchronisedHelper helper = new BaseSelectedSynchronisedHelper(pc.Vertex, synchronisedVisualiser);

            //PlatformClass.RegisterVertexChangeListeners(synchronisedVisualiser,new VertexChange(helper.SynchronisedVisualiserChange), new string[]{"BaseEdge","SelectedEdges"});
            
            IVertex firstSelectedVertex = synchronisedVisualiser.Get(false, @"SelectedEdges:\");

            if (firstSelectedVertex != null)
                GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", firstSelectedVertex);

            MinusZero.Instance.DefaultUserInteraction.ShowContent(pc);

            return null;     
        }

        public static IVertex OpenVisualiserSelectedSelected(IVertex baseVertex, IVertex inputVertex)
        {
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get(false, "VisualiserClass:"), baseVertex);

            //GraphUtil.ReplaceEdge(pc.Vertex, "BaseEdge", baseVertex);

            GraphUtil.ReplaceEdge(pc.Vertex, "SelectedEdges", inputVertex.Get(false, @"SynchronisedVisualiser:\SelectedEdges:"));

            MinusZero.Instance.DefaultUserInteraction.ShowContent(pc);

            return null;     
        }


    }
}
