using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Util;
using m0.Graph;

namespace m0.ZeroTypes
{
    class PlatformClassVertexChangeListener{
        public PlatformClassVertexChangeListener(string[] watchList)
        {
            foreach (string w in watchList)
                WatchList.Add(w);
        }

        public List<string> WatchList = new List<string>();

        public event VertexChange Change;

        public virtual Delegate[] GetChangeDelegateInvocationList()
        {
            return Change.GetInvocationList();
        }

        public IVertex PlatformClassVertex;

        private bool CheckSender(object sender)
        {
            if (sender == PlatformClassVertex)
                return true;

            foreach (string metaFromWatchList in WatchList)
                foreach (IEdge e in PlatformClassVertex.GetAll(metaFromWatchList + ":"))
                    if (sender == e.To)
                        return true;

            return false;
        }

        public void Listener(object sender, VertexChangeEventArgs e){            
            if (CheckSender(sender) && (e.Type==VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value,"$Is")))
            {
                IVertex AttributeVertexes = ((IVertex)sender).GetAll(@"$Is:\Selector:");
                //IVertex AttributeVertexes = ((IVertex)sender).GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

                foreach (IEdge ed in AttributeVertexes)
                    if (e.Edge.Meta == ed.To)                    
                        GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(e.Edge.To, this.Listener);                        

                foreach (string metaFromWatchList in WatchList)
                    if (((string)e.Edge.Meta.Value) == metaFromWatchList)
                    {
                        GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(e.Edge.To, this.Listener);
                        
                        foreach (IEdge ee in e.Edge.To)
                            GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(ee.To, this.Listener);
                    }
            }

            foreach (string metaFromWatchList in WatchList)
                if ((sender == PlatformClassVertex.Get(metaFromWatchList+":")) && (e.Type == VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value,"$Is")))
                    GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(e.Edge.To, this.Listener);

            if (CheckSender(sender) && (e.Type == VertexChangeType.EdgeRemoved) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
            {
                IVertex AttributeVertexes = ((IVertex)sender).GetAll(@"$Is:\Selector:");
                //IVertex AttributeVertexes = ((IVertex)sender).GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

                foreach (IEdge ed in AttributeVertexes)
                    if (e.Edge.Meta == ed.To)
                        e.Edge.To.Change -= new VertexChange(this.Listener);

                foreach (string metaFromWatchList in WatchList)
                    if (((string)e.Edge.Meta.Value) == metaFromWatchList)
                    {
                        e.Edge.To.Change -= new VertexChange(this.Listener);

                        foreach (IEdge ee in e.Edge.To)
                            ee.To.Change -= new VertexChange(this.Listener);
                    }
            }


            foreach (string metaFromWatchList in WatchList)
                if ((sender == PlatformClassVertex.Get(metaFromWatchList + ":")) && (e.Type == VertexChangeType.EdgeRemoved) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
                        e.Edge.To.Change -= new VertexChange(this.Listener);                                

            if(Change!=null)
                Change(sender, e);
        }
    }

    public class PlatformClass
    {
        public static IPlatformClass CreatePlatformObject(IVertex Vertex)
        {
            if (Vertex.Get("$Is:Class") != null)
            {
                String classname = (string)Vertex.Get("$PlatformClassName:").Value;

                return (IPlatformClass)Activator.CreateInstance(Type.GetType(classname), null);
            }
            else
            {
                String classname = (string)Vertex.Get(@"$Is:{$Inherits:$PlatformClass}\$PlatformClassName:").Value;

                IPlatformClass pc=(IPlatformClass)Activator.CreateInstance(Type.GetType(classname), null);

                pc.Vertex = Vertex;

                return pc;
            }
        }

        public static void RegisterVertexChangeListeners(IVertex PlatformClassVertex, VertexChange action, string[] watchList){
            PlatformClassVertexChangeListener listener=new PlatformClassVertexChangeListener(watchList);
            listener.PlatformClassVertex = PlatformClassVertex;
            listener.Change += action;


            PlatformClassVertex.Change += new VertexChange(listener.Listener);

            IVertex AttributeVertexes = PlatformClassVertex.GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

            foreach (IEdge e in AttributeVertexes)
            {
                foreach (IEdge ee in PlatformClassVertex.GetAll(e.To.Value + ":"))
                {
                    GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(ee.To, listener.Listener);

                    foreach (string metaFromWatchList in listener.WatchList)
                        if (GeneralUtil.CompareStrings(ee.Meta.Value, metaFromWatchList))                    
                            foreach (IEdge eee in ee.To)
                                GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(eee.To, listener.Listener);                             
                }
            }
        }

        public static void RemoveVertexChangeListeners(IVertex PlatformClassVertex, VertexChange action)
        {
            RemoveVertexChangeListeners_ForVertex(PlatformClassVertex,PlatformClassVertex, action);

            IVertex AttributeVertexes = PlatformClassVertex.GetAll(@"$Is:{$Inherits:$PlatformClass}\Selector:");

            foreach (IEdge e in AttributeVertexes)
            {
                foreach (IEdge ee in PlatformClassVertex.GetAll(e.To.Value + ":"))                    
                    RemoveVertexChangeListeners_ForVertex(e.To, PlatformClassVertex,action);

                foreach (IEdge ee in PlatformClassVertex.GetAll(e.To.Value + @":\"))
                    RemoveVertexChangeListeners_ForVertex(e.To, PlatformClassVertex, action);
            }
        }

        private static void RemoveVertexChangeListeners_ForVertex(IVertex Vertex, IVertex PlatformClassVertex, VertexChange action)
        {
            Delegate[] delegates=Vertex.GetChangeDelegateInvocationList();

            if(delegates!=null)
            foreach (Delegate d in delegates)                            
                if (d.Target is PlatformClassVertexChangeListener)
                {
                    PlatformClassVertexChangeListener list = (PlatformClassVertexChangeListener)d.Target;

                    if (list.PlatformClassVertex == PlatformClassVertex)
                    {
                        list.Change -= action;

                        Vertex.Change -= list.Listener;
                    }
                }                
            }            
        
    }
}
