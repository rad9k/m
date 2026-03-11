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
using m0.UIWpf.Commands;
using m0.User.Process.UX;
using m0.ZeroTypes.UX;

namespace m0.UIWpf.Commands
{
    public class BaseCommands
    {
        static IVertex ClipboardEntry_meta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\User\ClipboardEntry");
        public static void Execute(IVertex baseVertex, IVertex inputVertex)
        {
            ExecuteDialog e = new ExecuteDialog(baseVertex);

            MinusZero.Instance.UserInteraction.ShowContentFloating(e, FloatingWindowSize.Medium);
        }

       public static void NewVertex(IVertex baseVertex,IVertex inputVertex){
            NewVertex d = new NewVertex(baseVertex.Get(false, "To:"));

            MinusZero.Instance.UserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);
        }

        public static void NewVertexBySchema(IVertex baseVertex, IVertex inputVertex)
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
                MinusZero.Instance.UserInteraction.EditEdge(v);
            else
            {
                NewVertexBySchema d = new NewVertexBySchema(v, MetaVertex);

                MinusZero.Instance.UserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void NewEdge(IVertex baseVertex, IVertex inputVertex)
        {
            NewEdge d = new NewEdge(baseVertex.Get(false, "To:"));

            MinusZero.Instance.UserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);
        }

        public static void NewEdgeBySchema(IVertex baseVertex, IVertex inputVertex)
        {
            NewEdgeBySchema d = new NewEdgeBySchema(baseVertex.Get(false, "To:"), inputVertex);

            MinusZero.Instance.UserInteraction.ShowContentFloating(d, FloatingWindowSize.Micro);
        }

        public static void NewDiagram(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IVertex dv = VertexOperations.AddInstance(baseVertex.Get(false, "To:"), MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class:Diagram"));

            GraphUtil.CreateOrReplaceEdge(dv, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Class:Diagram\CreationPool"), baseVertex.Get(false, "To:"));

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            MinusZero.Instance.UserInteraction.EditEdge(dv);           
        }

        public static void NewUX(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex Visualiser_Vertex = UXContainer.CreateDefaultContainer(baseVertex);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            MinusZero.Instance.UserInteraction.EditEdge(Visualiser_Vertex);
        }
 
        public static void Cut(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            User.Clipboard.ClearClipboard();

            //

            if (inputVertex.Get(false, "SelectedEdges:") == null || inputVertex.Get(false, "SelectedEdges:").Count() == 0)
                User.Clipboard.PutToClipboard(baseVertex, true);
            else
                foreach (IEdge e in inputVertex.Get(false, "SelectedEdges:"))
                    User.Clipboard.PutToClipboard(e.To, true);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void Copy(IVertex baseVertex, IVertex inputVertex)
        {            
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            User.Clipboard.ClearClipboard();            

            //

            if (inputVertex.Get(false, "SelectedEdges:") == null || inputVertex.Get(false, "SelectedEdges:").Count() == 0)
                User.Clipboard.PutToClipboard(baseVertex, false);            
            else
                foreach (IEdge e in inputVertex.Get(false, "SelectedEdges:"))
                    User.Clipboard.PutToClipboard(e.To, false);            

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }



        public static void Paste(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            foreach (IEdge e in User.Clipboard.GetFromClipboard())
            {
                IVertex v = e.To;

                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCut"))
                {
                    IVertex v_From = GraphUtil.GetQueryOutFirst(v, "From", null);
                    IVertex v_Meta = GraphUtil.GetQueryOutFirst(v, "Meta", null);
                    IVertex v_To = GraphUtil.GetQueryOutFirst(v, "To", null);

                    VertexOperations.DeleteOneEdge(v_From, v_Meta, v_To);

                    IVertex baseVertex_To = GraphUtil.GetQueryOutFirst(baseVertex, "To", null);

                    baseVertex_To.AddEdge(v_Meta, v_To);
                }

                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCopy"))
                {
                    IVertex baseVertex_To = GraphUtil.GetQueryOutFirst(baseVertex, "To", null);

                    IVertex v_To = GraphUtil.GetQueryOutFirst(v, "To", null);

                    IEdge v_To_Edge = EdgeHelper.GetIEdgeByEdgeVertex(v);

                    VertexOperations.CopyVertex(v_To_Edge, baseVertex_To);
                }
            }            

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void Delete(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex info = m0.MinusZero.Instance.CreateTempVertex();
            info.Value = "DELETE vertex";

            IVertex options = m0.MinusZero.Instance.CreateTempVertex();

            options.AddVertex(null, "Edge delete");
            options.AddVertex(null, "Remove from repository");
            options.AddVertex(null, "Cancel");

            IVertex option = MinusZero.Instance.UserInteraction.InteractionSelectButton(info, options.OutEdges);

            bool allEdgesDelete = false;


            if (option == null || GeneralUtil.CompareStrings(option.Value, "Cancel"))
                return;

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
        }

        public static void Query(IVertex baseVertex, IVertex inputVertex)
        {
            QueryDialog d = new QueryDialog(baseVertex.Get(false, "To:"));

            MinusZero.Instance.UserInteraction.ShowContentFloating(d, FloatingWindowSize.Medium);
        }

        public static void OpenDefaultVisualiser(IVertex baseVertex, bool isFloating)
        {
            IVertex DefaultVis;

            DefaultVis=baseVertex.Get(false, @"Meta:\$DefaultOpenVisualiser:");

            if (DefaultVis == null)
                DefaultVis = baseVertex.Get(false, @"To:\$Is:\$DefaultOpenVisualiser:"); // yes. bad but it is

            if (DefaultVis==null)
                DefaultVis=baseVertex.Get(false, @"To:\$Is:\$Is:\$DefaultOpenVisualiser:"); // yes. bad but it is

            if (DefaultVis == null)
                DefaultVis = baseVertex.Get(false, @"Meta:\$EdgeTarget:\$DefaultOpenVisualiser:");

            if (DefaultVis == null)
                DefaultVis = baseVertex.Get(false, @"Meta:\$EdgeTarget:\$DefaultEditVisualiser:");

            if (DefaultVis == null)
                DefaultVis = MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form");

            if (GeneralUtil.CompareStrings(DefaultVis.Value, "Diagram"))
                OpenDiagram(baseVertex, DefaultVis, isFloating);
            else
                OpenVisualiser(baseVertex, DefaultVis, isFloating);
        }

        public static void OpenFormVisualiser(IVertex baseVertex, bool isFloating)
        {            
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            OpenVisualiser(baseVertex, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form"), isFloating);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void OpenDiagram(IVertex baseVertex, IVertex inputVertex, bool isFloating)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex visualiserVertex = baseVertex.Get(false, "To:");


            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(visualiserVertex, visualiserVertex);            

            if (isFloating)
                MinusZero.Instance.UserInteraction.ShowContentFloating(sv, FloatingWindowSize.Medium);
            else
                MinusZero.Instance.UserInteraction.ShowContent(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void OpenVisualiser(IVertex baseVertex, IVertex inputVertex, bool isFloating)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex, baseVertex);                        

            if (isFloating)
                MinusZero.Instance.UserInteraction.ShowContentFloating(sv, FloatingWindowSize.Medium);
            else
                MinusZero.Instance.UserInteraction.ShowContent(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////        
        }

        public static void OpenMetaVisualiser(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IEdge edge = new EasyEdge(null, null, baseVertex.Get(false, "Meta:"));

            IPlatformClass sv = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex, edge);            

            MinusZero.Instance.UserInteraction.ShowContent(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void OpenVisualiserFloating(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex, baseVertex);            

            MinusZero.Instance.UserInteraction.ShowContentFloating(pc, FloatingWindowSize.Medium);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void OpenVisualiserFirstSelectedEdgeSynchronised(IVertex baseVertex, IVertex inputVertex)
        {
            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, baseVertex.Get(false, "Meta:"), baseVertex.Get(false, "To:"));

            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get(false, "VisualiserClass:"), baseEdgeVertex);
            
            IVertex masterVisualiser = inputVertex.Get(false, "MasterVisualiser:");

            FirstSelectedEdgeSynchronisedHelper helper = new FirstSelectedEdgeSynchronisedHelper(masterVisualiser, pc.Vertex);            

            MinusZero.Instance.UserInteraction.ShowContent(pc);

            helper.DoSynchronise();
        }

        public static void OpenVisualiserSelectedSelected(IVertex baseVertex, IVertex inputVertex)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, baseVertex.Get(false, "Meta:"), baseVertex.Get(false, "To:"));

            IPlatformClass pc = (IPlatformClass)PlatformClass.CreatePlatformObject(inputVertex.Get(false, "VisualiserClass:"), baseEdgeVertex);


            GraphUtil.ReplaceEdge(pc.Vertex, "SelectedEdges", inputVertex.Get(false, @"MasterVisualiser:\SelectedEdges:"));

            MinusZero.Instance.UserInteraction.ShowContent(pc);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }
    }
}
