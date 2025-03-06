using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.LegacySystem.Graph
{
    public class EasyVertex
    {
        public static IExecuter DefaultExecuter;
        public static IFormalTextParser DefaultParser;

        private static IDictionary<String, IVertex> QueryParseChache = new Dictionary<String, IVertex>();
        private static IDictionary<String, IVertex> QueryParseChache_metaMode = new Dictionary<String, IVertex>();



        public static IVertex Get(IVertex baseVertex, bool metaMode, string query)
        {
            IVertex queryVertex = null;
            IVertex parseError = null;

            IDictionary<String, IVertex> chache;

            if (metaMode)
                chache = QueryParseChache_metaMode;
            else
                chache = QueryParseChache;

            if (chache.ContainsKey(query))
                queryVertex = chache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                IEdge baseEdge_new;
                parseError = DefaultParser.Parse(new EdgeBase(null, null, queryVertex), query, ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

                if (parseError == null)
                {
                    queryVertex.AddExternalReference();

                    chache.Add(query, queryVertex);
                }
            }

            if (parseError != null)
                return null;

            return DefaultExecuter.Get(metaMode, baseVertex, queryVertex);
        }
    }
}
