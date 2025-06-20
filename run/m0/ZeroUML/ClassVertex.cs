using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;

namespace m0.ZeroUML
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

        public static string GetCardinalitiesString(IVertex baseVertex)
        {
            string min = GetStringCardinality(baseVertex.Get(false, "$MinCardinality:"));
            string max = GetStringCardinality(baseVertex.Get(false, "$MaxCardinality:"));

            if (min == "1" && max == "1")
                return "";

            if (min == max)
                return "[" + min + "]";

            return "[" + min + ".." + max + "]";
        }

        public static string GetValueRangeString(IVertex baseVertex)
        {
            string min = GraphUtil.GetStringValue(GraphUtil.GetQueryOutFirst(baseVertex, "MinValue", null));
            string max = GraphUtil.GetStringValue(GraphUtil.GetQueryOutFirst(baseVertex, "MaxValue", null));

            if (min == "" || max == "")
                return "";            

            return "<" + min + ":" + max + ">";
        }

        public static void AddAllAttributesAndAssociationsVertices(IVertex ObjectVertex){
            IVertex AttributeVertices = ObjectVertex.GetAll(false, @"$Is:\Attribute:");

            foreach (IEdge e in AttributeVertices)
                if(e.To.Get(false,"$DefaultValue:") != null)
                    ObjectVertex.AddVertex(e.To, e.To.Get(false, "$DefaultValue:").Value);
                else if (GraphUtil.GetIntegerValueOr0(e.To.Get(false, "$MinCardinality:")) != 0)
                    ObjectVertex.AddVertex(e.To, null);

            IVertex AssociationVertices = ObjectVertex.GetAll(false, @"$Is:\Association:");

            foreach (IEdge e in AssociationVertices)
                if (e.To.Get(false, "$DefaultValue:") != null)
                    ObjectVertex.AddVertex(e.To, e.To.Get(false, "$DefaultValue:").Value);
                else
                    ObjectVertex.AddVertex(e.To, null);
        }

        public static void AddIsClassAndAllAttributesAndAssociations(IVertex ObjectVertex, IVertex ClassVertex)
        {
            IVertex smuv = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex");

            ObjectVertex.AddEdge(smuv.Get(false, "$Is"), ClassVertex);

            AddAllAttributesAndAssociationsVertices(ObjectVertex);
        }
    }
}
