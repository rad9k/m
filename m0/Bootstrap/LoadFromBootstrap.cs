using m0.Foundation;
using m0.Graph;
using m0.Store.Json;
using m0.ZeroUML.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Bootstrap
{
    class LoadFromBootstrap
    {
        public static void Execute()
        {
            JsonSerializationStore bootstrap = new JsonSerializationStore("_bootstrap.m0", MinusZero.Instance, new AccessLevelEnum[] { });

            IVertex root = MinusZero.Instance.root;

            bool isSystem = true;


            IList<IVertex> system = null;

            foreach(IEdge e in bootstrap.Root)
            {
                string importVertexPath = e.To.Value.ToString();
                string importFilePath = e.To.OutEdges[0].To.Value.ToString();

                IVertex importRoot = GraphUtil.DivideQueryAndGetByPart(root, importVertexPath);

                if (importRoot == null)
                    importRoot = GraphUtil.SimpleCreateVertexPath(root, importVertexPath);                        

                if (isSystem)
                {
                    JsonSerializationStore imp = new JsonSerializationStore(importFilePath, MinusZero.Instance, new AccessLevelEnum[] { });

                    ZeroUMLInstructionHelpers.MoveEdgesIntoVertex(imp.Root, importRoot);

                    isSystem = false;

                    system = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(importRoot);
                }
                else
                {
                    JsonSerializationStore imp = new JsonSerializationStore(importFilePath, MinusZero.Instance, new AccessLevelEnum[] { });

                    ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_IncludeEverythingBesidesList(imp.Root, importRoot, system);
                }
            }
        }
    }
}
