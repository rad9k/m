using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_SYSTEM_GENERATE.Util
{
    public class GenerateUtil
    {
        public class TypeName
        {
            public string Name;

            public IVertex TypeVertex;

            public string Type;

            public int MinCardinality;

            public int MaxCardinality;

            public TypeName(string _name, string _type)
            {
                Name = _name;
                Type = _type;

                MinCardinality = 1;

                MaxCardinality = 1;
            }

            public TypeName(string _name, string _type, int _MinCardinality, int _MaxCardinality)
            {
                Name = _name;
                Type = _type;

                MinCardinality = _MinCardinality;

                MaxCardinality = _MaxCardinality;
            }

            public TypeName(string _name, IVertex _typeVertex, int _MinCardinality, int _MaxCardinality)
            {
                Name = _name;
                TypeVertex = _typeVertex;

                MinCardinality = _MinCardinality;

                MaxCardinality = _MaxCardinality;
            }
        }

        public static IVertex AddFunction(IVertex baseVertex, string name, string typeName, string methodName, string ret, IList<TypeName> pars)
        {
            IVertex zu = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroUML");

            IVertex zt = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroTypes");

            IVertex bv = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\Base\\Vertex");

            IVertex f = baseVertex.AddVertex(zu.Get(false, "Function"), name);

            f.AddEdge(m0.MinusZero.Instance.Is, zu.Get(false, "Function"));

            if (ret != null)
                f.AddEdge(zu.Get(false, "Function\\Output"), zt.Get(false, ret));

            foreach (TypeName tn in pars)
            {
                IVertex ip = f.AddVertex(zu.Get(false, "Function\\InputParameter"), tn.Name);

                if (tn.Type !=null)
                    ip.AddEdge(bv.Get(false, "$EdgeTarget"), zt.Get(false, tn.Type));
                else
                    ip.AddEdge(bv.Get(false, "$EdgeTarget"), tn.TypeVertex);

              //  ip.AddVertex(bv.Get(false, "$MinCardinality"), tn.MinCardinality); // must be a reson for not using those now
              //  ip.AddVertex(bv.Get(false, "$MaxCardinality"), tn.MaxCardinality);
            }

            if (methodName != null)
                ExecutionFlowHelper.DecorateWithDotNetStaticMethod(f, typeName, methodName);

            return f;
        }

        public static IVertex AddMethod(IVertex baseVertex, string name, string typeName, string methodName, string ret, IList<TypeName> pars)
        {
            IVertex zu = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroUML");

            IVertex zt = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroTypes");

            IVertex bv = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\Base\\Vertex");

            IVertex f = baseVertex.AddVertex(zu.Get(false, @"Class\Method"), name);

            IVertex methodVertex = f.AddEdge(m0.MinusZero.Instance.Is, zu.Get(false, @"Class\Method")).To;

            if (ret != null)
                f.AddEdge(zu.Get(false, "Function\\Output"), zt.Get(false, ret));

            foreach (TypeName tn in pars)
            {
                IVertex ip = f.AddVertex(zu.Get(false, "Class\\Method\\InputParameter"), tn.Name);

                if(tn.Type !=null)
                    ip.AddEdge(bv.Get(false, "$EdgeTarget"), zt.Get(false, tn.Type));
                else
                    ip.AddEdge(bv.Get(false, "EdgeTarget"), tn.TypeVertex);

                //ip.AddVertex(bv.Get(false, "$MinCardinality"), tn.MinCardinality); // must be a reson for not using those now
                // ip.AddVertex(bv.Get(false, "$MaxCardinality"), tn.MaxCardinality);
            }

            if (typeName != null)
                ExecutionFlowHelper.DecorateWithDotNetStaticMethod(f, typeName, methodName);

            return methodVertex;
        }

        public static IVertex AddMethod(IVertex baseVertex, string name, string ret, IList<TypeName> pars)
        {
            IVertex zu = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroUML");

            IVertex zt = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroTypes");

            IVertex bv = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\Base\\Vertex");

            IVertex f = baseVertex.AddVertex(zu.Get(false, @"Class\Method"), name);

            f.AddEdge(m0.MinusZero.Instance.Is, zu.Get(false, @"Class\Method"));

            if (ret != null)
                f.AddEdge(zu.Get(false, "Function\\Output"), zt.Get(false, ret));

            foreach (TypeName tn in pars)
            {
                IVertex ip = f.AddVertex(zu.Get(false, "Class\\Method\\InputParameter"), tn.Name);

                if (tn.Type != null)
                    ip.AddEdge(bv.Get(false, "$EdgeTarget"), zt.Get(false, tn.Type));
                else
                    ip.AddEdge(bv.Get(false, "$EdgeTarget"), tn.TypeVertex);

                //ip.AddVertex(bv.Get(false, "$MinCardinality"), tn.MinCardinality);
                // ip.AddVertex(bv.Get(false, "$MaxCardinality"), tn.MaxCardinality);
            }

            return f;
        }
    }
}
