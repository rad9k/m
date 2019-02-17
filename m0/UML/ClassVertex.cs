using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;

namespace m0.UML
{
    public class ClassVertex
    {
        public static string GetStringCardinality(IVertex value)
        {
            if (value == null)
                return "1";

            if (GraphUtil.GetIntegerValue(value) == -1)
                return "*";

            return GraphUtil.GetIntegerValue(value).ToString();
        }

        public static string GetStringCardinalities(IVertex baseVertex)
        {
            string min = GetStringCardinality(baseVertex.Get(false, "$MinCardinality:"));
            string max = GetStringCardinality(baseVertex.Get(false, "$MaxCardinality:"));

            if (min == "1" && max == "1")
                return "";

            if (min == max)
                return "[" + min + "]";

            return "["+min+".."+max+"]";
        }

        public static void AddAllAttributesAndAssociationsVertexes(IVertex ObjectVertex){
            IVertex AttributeVertexes = ObjectVertex.GetAll(false, @"$Is:\Attribute:");

            foreach (IEdge e in AttributeVertexes)
                ObjectVertex.AddVertex(e.To, null);

            IVertex AssociationVertexes = ObjectVertex.GetAll(false, @"$Is:\Association:");

            foreach (IEdge e in AssociationVertexes)
                ObjectVertex.AddVertex(e.To, null);
        }

        public static void AddIsClassAndAllAttributesAndAssociations(IVertex ObjectVertex, IVertex ClassVertex)
        {
            IVertex smuv = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex");

            ObjectVertex.AddEdge(smuv.Get(false, "$Is"), ClassVertex);

            AddAllAttributesAndAssociationsVertexes(ObjectVertex);
        }
    }
}
