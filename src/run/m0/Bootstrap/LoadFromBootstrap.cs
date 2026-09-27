using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store;
using m0.Store.Binary;
using m0.Store.Json;
using m0.Util;
using m0.ZeroUML.Instructions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Bootstrap
{
    public class LoadFromBootstrap
    {
        class PendingBootstrapImport
        {
            public string ImportFilePath;
            public IVertex ImportRoot;
        }

        static HashSet<IVertex> SystemVerticesForLaterImport;

        static Dictionary<IVertex, PendingBootstrapImport> PendingImports = new Dictionary<IVertex, PendingBootstrapImport>();

        public static bool HasPendingImport(IVertex vertex)
        {
            return vertex != null && PendingImports.ContainsKey(vertex);
        }

        public static void EnsureImported(IVertex vertex)
        {
            if (vertex == null)
                return;

            if (!PendingImports.TryGetValue(vertex, out PendingBootstrapImport pendingImport))
                return;

            PendingImports.Remove(vertex);

            ExecutionFlowHelper.GraphChangeWatchOff();

            try
            {
                HashSet<IVertex> excludeList = SystemVerticesForLaterImport ?? new HashSet<IVertex>();

                string importFilePath = pendingImport.ImportFilePath;

                StoreBase loadedStore = OpenImportStore(importFilePath);

                if (loadedStore != null)
                {
                    object mountName = pendingImport.ImportRoot.Value;

                    ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_IncludeEverythingBesidesList(loadedStore.Root, pendingImport.ImportRoot, excludeList);

                    if (pendingImport.ImportFilePath.EndsWith(".m0x") &&
                        (GeneralUtil.CompareStrings(pendingImport.ImportRoot.Value, "") || GeneralUtil.CompareStrings(pendingImport.ImportRoot.Value, "$Empty")))
                        pendingImport.ImportRoot.Value = mountName;

                    MinusZero.Instance.RemoveStore(loadedStore);

                    if (MinusZero.Instance.BootstrapVertexes != null)
                    {
                        HashSet<IVertex> updatedBootstrapVertexes = new HashSet<IVertex>(MinusZero.Instance.BootstrapVertexes);

                        foreach (IVertex importedVertex in GraphUtil.GetSubGraphWithoutLinksAsList(pendingImport.ImportRoot))
                            updatedBootstrapVertexes.Add(importedVertex);

                        MinusZero.Instance.BootstrapVertexes = updatedBootstrapVertexes;
                    }
                }
            }
            finally
            {
                ExecutionFlowHelper.GraphChangeWatchOn();
            }
        }

        static StoreBase OpenImportStore(string importFilePath)
        {
            AccessLevelEnum[] accessLevelList = new AccessLevelEnum[] { };

            if (importFilePath.EndsWith(".m0j"))
                return new JsonSerializationStore(importFilePath, MinusZero.Instance, accessLevelList);

            if (importFilePath.EndsWith(".m0x"))
                return new BinaryStore(importFilePath, MinusZero.Instance, accessLevelList);

            return null;
        }

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

                StoreBase loadedStore = null;

                string importFileName = Path.GetFileName(importFilePath);

                if (MinusZero.Instance.DeferBinaryBootstrapImports &&
                    !isSystem &&
                    importFileName.StartsWith("_"))
                {
                    PendingImports[importRoot] = new PendingBootstrapImport
                    {
                        ImportFilePath = importFilePath,
                        ImportRoot = importRoot
                    };
                }
                else if (isSystem)
                {
                    loadedStore = new JsonSerializationStore(importFilePath, MinusZero.Instance, new AccessLevelEnum[] { });

                    ZeroUMLInstructionHelpers.MoveEdgesIntoVertex(loadedStore.Root, importRoot);

                    isSystem = false;

                    system = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(importRoot);

                    SystemVerticesForLaterImport = new HashSet<IVertex>(system);
                }
                else
                {
                    if (importFilePath.EndsWith(".m0j"))
                    {
                        loadedStore = new JsonSerializationStore(importFilePath, MinusZero.Instance, new AccessLevelEnum[] { });

                        ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_IncludeEverythingBesidesList(loadedStore.Root, importRoot, new HashSet<IVertex>(system));
                    }

                    if (importFilePath.EndsWith(".m0x"))
                    {
                        loadedStore = new BinaryStore(importFilePath, MinusZero.Instance, new AccessLevelEnum[] { });

                        object mountName = importRoot.Value;

                        ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_IncludeEverythingBesidesList(loadedStore.Root, importRoot, new HashSet<IVertex>(system));

                        if (GeneralUtil.CompareStrings(importRoot.Value, "") || GeneralUtil.CompareStrings(importRoot.Value, "$Empty"))
                            importRoot.Value = mountName;
                    }

                }

                if (loadedStore != null)
                    MinusZero.Instance.RemoveStore(loadedStore);
            }

            MinusZero.Instance.RemoveStore(bootstrapStore);
        }
    }
}
