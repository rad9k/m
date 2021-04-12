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
                foreach (IEdge e in PlatformClassVertex.GetAll(false, metaFromWatchList + ":"))
                    if (sender == e.To)
                        return true;

            return false;
        }

        public void Listener(object sender, VertexChangeEventArgs e){            
            if (CheckSender(sender) && (e.Type==VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value,"$Is")))
            {
                //IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:\Selector:");
                IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:\{$Is:{$Inherits:Selector}}");

                //IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:\Attribute:");

                //IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:{$Inherits:$PlatformClass}\Selector:");

                foreach (IEdge ed in AttributeVertices)
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
                //if ((sender == PlatformClassVertex.Get(false, metaFromWatchList+":")) && (e.Type == VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value,"$Is")))
                foreach(IEdge ee in PlatformClassVertex.GetAll(false, metaFromWatchList + ":"))
                if ((sender == ee.To) && (e.Type == VertexChangeType.EdgeAdded) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
                    GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(e.Edge.To, this.Listener);

            if (CheckSender(sender) && (e.Type == VertexChangeType.EdgeRemoved) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
            {
                //IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:\Selector:");
                IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:\{$Is:{$Inherits:Selector}}");

                //IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:\Attribute:");


                //IVertex AttributeVertices = ((IVertex)sender).GetAll(false, @"$Is:{$Inherits:$PlatformClass}\Selector:");

                foreach (IEdge ed in AttributeVertices)
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
            //if ((sender == PlatformClassVertex.Get(false, metaFromWatchList + ":")) && (e.Type == VertexChangeType.EdgeRemoved) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))
                foreach (IEdge ee in PlatformClassVertex.GetAll(false, metaFromWatchList + ":"))
                    if ((sender == ee.To) && (e.Type == VertexChangeType.EdgeRemoved) && (!GeneralUtil.CompareStrings(e.Edge.Meta.Value, "$Is")))                   
                        e.Edge.To.Change -= new VertexChange(this.Listener);                                

            if(Change!=null)
                Change(sender, e);
        }
    }

    public class PlatformClass
    {
        static DictionaryList<string, Delegate> ListenerGroupDictionary = new DictionaryList<string, Delegate>();

        public static IPlatformClass CreatePlatformObject(IVertex Vertex)
        {
            if (Vertex.Get(false, "$Is:Class") != null)
            {
                String classname = (string)Vertex.Get(false, "$PlatformClassName:").Value;

                return (IPlatformClass)Activator.CreateInstance(Type.GetType(classname), null);
            }
            else
            {
                String classname = (string)Vertex.Get(false, @"$Is:{$Inherits:$PlatformClass}\$PlatformClassName:").Value;

                IPlatformClass pc=(IPlatformClass)Activator.CreateInstance(Type.GetType(classname), null);

                pc.Vertex = Vertex;

                return pc;
            }
        }

        public static void RegisterVertexChangeListeners_byGenericVertex(IVertex baseVertex, VertexChange action, string[] watchList)
        {
            RegisterVertexChangeListeners_byGenericVertex(baseVertex, action, watchList, null);
        }

        public static void RegisterVertexChangeListeners_byGenericVertex(IVertex baseVertex, VertexChange action, string[] watchList, string listenerGroup)
        {
            PlatformClassVertexChangeListener listener = new PlatformClassVertexChangeListener(watchList);
            listener.PlatformClassVertex = baseVertex;
            listener.Change += action;

            VertexChange listenerDelegate = new VertexChange(listener.Listener);

            baseVertex.Change += listenerDelegate;

            if (listenerGroup != null)
            {
                ListenerGroupDictionary.Add(listenerGroup, action);
                ListenerGroupDictionary.Add(listenerGroup, listenerDelegate);
           }            

            IVertex AttributeVertices = baseVertex.GetAll(false, @"$Is:\{$Is:{$Inherits:Selector}}");

            foreach (IEdge e in AttributeVertices)
            {
                foreach (IEdge ee in baseVertex.GetAll(false, e.To.Value + ":"))
                {
                    GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(ee.To, listener.Listener);

                    foreach (string metaFromWatchList in listener.WatchList)
                        if (GeneralUtil.CompareStrings(ee.Meta.Value, metaFromWatchList))
                            foreach (IEdge eee in ee.To)
                                GraphUtil.AddHandlerIfDelegateListDoesNotContainsIt(eee.To, listener.Listener);
                }
            }
        }

        public static void RemoveVertexChangeListeners_byGenericVertex(IVertex metaVertex, VertexChange action)
        {
            RemoveVertexChangeListeners_byGenericVertex(metaVertex, action);
        }

        public static void RemoveVertexChangeListeners_byGenericVertex(IVertex metaVertex, VertexChange action, string listenerGroup)
        {
            RemoveVertexChangeListeners_ForVertex(metaVertex, metaVertex, action, listenerGroup);

            IVertex AttributeVertices = metaVertex.GetAll(false, @"$Is:\{$Is:{$Inherits:Selector}}");

            foreach (IEdge e in AttributeVertices)
            {
                foreach (IEdge ee in metaVertex.GetAll(false, e.To.Value + ":"))
                    RemoveVertexChangeListeners_ForVertex(e.To, metaVertex, action, listenerGroup);

                foreach (IEdge ee in metaVertex.GetAll(false, e.To.Value + @":\"))
                    RemoveVertexChangeListeners_ForVertex(e.To, metaVertex, action, listenerGroup);
            }
        }

        public static void RegisterVertexChangeListeners(IVertex PlatformClassVertex, VertexChange action, string[] watchList){
            PlatformClassVertexChangeListener listener=new PlatformClassVertexChangeListener(watchList);
            listener.PlatformClassVertex = PlatformClassVertex;
            listener.Change += action;


            PlatformClassVertex.Change += new VertexChange(listener.Listener);

            IVertex AttributeVertices = PlatformClassVertex.GetAll(false, @"$Is:{$Inherits:$PlatformClass}\{$Is:{$Inherits:Selector}}");

            foreach (IEdge e in AttributeVertices)
            {
                foreach (IEdge ee in PlatformClassVertex.GetAll(false, e.To.Value + ":"))
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
            RemoveVertexChangeListeners_ForVertex(PlatformClassVertex,PlatformClassVertex, action, null);

            IVertex AttributeVertices = PlatformClassVertex.GetAll(false, @"$Is:{$Inherits:$PlatformClass}\{$Is:{$Inherits:Selector}}");

            foreach (IEdge e in AttributeVertices)
            {
                foreach (IEdge ee in PlatformClassVertex.GetAll(false, e.To.Value + ":"))                    
                    RemoveVertexChangeListeners_ForVertex(e.To, PlatformClassVertex, action, null);

                foreach (IEdge ee in PlatformClassVertex.GetAll(false, e.To.Value + @":\"))
                    RemoveVertexChangeListeners_ForVertex(e.To, PlatformClassVertex, action, null);
            }
        }

        private static void RemoveVertexChangeListeners_ForVertex(IVertex Vertex, IVertex PlatformClassVertex, VertexChange action, string listenerGroup)
        {
            Delegate[] delegates=Vertex.GetChangeDelegateInvocationList();            

            if(delegates!=null)
            foreach (Delegate d in delegates)                            
                if (d.Target is PlatformClassVertexChangeListener)
                {
                    PlatformClassVertexChangeListener list = (PlatformClassVertexChangeListener)d.Target;

                    bool can = true;

                    if(listenerGroup != null && ListenerGroupDictionary.ContainsKey(listenerGroup))
                            if(!ListenerGroupDictionary.Contains(listenerGroup, d))
                                can = false;
                    
                    if (list.PlatformClassVertex == PlatformClassVertex && can
                            //&& action.Target == d.Target // XXX THIS CAUSES UNKNOWN PROBLEMS IN SongVisualiser Track at last
                            //&& action.Method == d.Method // XXX THIS ALSO 
                            )
                        {
                        list.Change -= action;

                        Vertex.Change -= list.Listener;
                    }
                }                
        }        

    }
}
