using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode.Helpers
{
    public class FormalTextLanguageDictinaries_Graph2Text
    {
        IVertex formalTextLangugeVertex;

        public IDictionary<string, IVertex> firstEdge2KeywordVertex;
        public IDictionary<string, IVertex> firstEdgeIs2KeywordVertex;

        public FormalTextLanguageDictinaries_Graph2Text(IVertex _formalTextLangugeVertex)
        {
            formalTextLangugeVertex = _formalTextLangugeVertex;

            firstEdge2KeywordVertex = new Dictionary<string, IVertex>();
            firstEdgeIs2KeywordVertex = new Dictionary<string, IVertex>();

            IVertex keywordsVertex = GraphUtil.GetQueryOutFirst(formalTextLangugeVertex, "Keywords", null);

            foreach(IVertex keyword in GraphUtil.GetQueryOut(keywordsVertex, "$Keyword", null))
            {
                IEdge firstEdge = null;

                foreach(IEdge e in keyword)
                    if (!GraphUtil.IsMetaDoubleDollar(e))
                    {
                        firstEdge = e;
                        break;
                    }

                if (firstEdge != null)
                {
                    string firstEdgeToValue = firstEdge.To.Value.ToString();

                    firstEdge2KeywordVertex.Add(firstEdgeToValue, keyword);

                    IEdge firstEdgeIs = GraphUtil.GetQueryOutFirstEdge(firstEdge.To, "$Is", null);

                    if (firstEdgeIs != null)
                        firstEdge2KeywordVertex.Add(firstEdgeToValue + "|" + firstEdgeIs.To.Value.ToString(), keyword);
                }
            }
        }

        void createFirstEdgeIs2KeywordVertex()
        {

        }
    }
}
