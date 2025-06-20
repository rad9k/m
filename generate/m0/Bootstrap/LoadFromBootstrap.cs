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
            JsonSerializationStore bootstrapStore = new JsonSerializationStore("_bootstrap.m0j", MinusZero.Instance, new AccessLevelEnum[] { });

            IVertex root = MinusZero.Instance.root;

            bool isSystem = true;

            IEnumerable<IVertex> system = null;

            foreach (IEdge e in bootstrapStore.Root)
            {
                string importVertexPath = e.To.Value.ToString();
                string importFilePath = e.To.OutEdges[0].To.Value.ToString();

                IVertex importRoot = GraphUtil.DivideQueryAndGetByPart(root, importVertexPath);

                if (importRoot == null)
                    importRoot = GraphUtil.SimpleCreateVertexPath(root, importVertexPath);

                JsonSerializationStore loadedStore;

                if (isSystem)
                {
                    loadedStore = new JsonSerializationStore(importFilePath, MinusZero.Instance, new AccessLevelEnum[] { });

                    ZeroUMLInstructionHelpers.MoveEdgesIntoVertex(loadedStore.Root, importRoot);

                    isSystem = false;

                    system = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(importRoot);
                }
                else
                {
                    loadedStore = new JsonSerializationStore(importFilePath, MinusZero.Instance, new AccessLevelEnum[] { });

                    ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_IncludeEverythingBesidesList(loadedStore.Root, importRoot, new HashSet<IVertex>(system));
                }

                MinusZero.Instance.RemoveStore(loadedStore);
            }

            MinusZero.Instance.RemoveStore(bootstrapStore);
        }
    }
}
