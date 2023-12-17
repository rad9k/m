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

            createFirstEdge2KeywordVertex();
            createFirstEdgeIs2KeywordVertex();
        }

        void createFirstEdge2KeywordVertex()
        {
            IList<IEdge> keywords = GraphUtil.GetQueryOut(formalTextLangugeVertex, "Keywords", null);
        }

        void createFirstEdgeIs2KeywordVertex()
        {

        }
    }
}
