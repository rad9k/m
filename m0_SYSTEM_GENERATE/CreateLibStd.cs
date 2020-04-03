using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_SYSTEM_GENERATE
{
    public class CreateLibStd
    {
        static IVertex std;

        class TypeName
        {
            public string Name;

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
        }

        static void AddFunction(string name, string typeName, string methodName, string ret, IList<TypeName> pars)
        {
            IVertex zu = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroUML");

            IVertex zt = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroTypes");

            IVertex bv = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\Base\\Vertex");

            IVertex f = std.AddVertex(zu.Get(false, "Function"), name);

            if (ret != null)
                f.AddEdge(zu.Get(false, "Function\\Output"), zt.Get(false, ret));

            foreach(TypeName tn in pars)
            {
                IVertex ip = f.AddVertex(zu.Get(false, "Function\\InputParameter"), tn.Name);

                ip.AddEdge(bv.Get(false, "$EdgeTarget"), zt.Get(false, tn.Type));
                ip.AddVertex(bv.Get(false, "$MinCardinality"), tn.MinCardinality);
                ip.AddVertex(bv.Get(false, "$MaxCardinality"), tn.MaxCardinality);
            }

            GraphUtil.AddDotNetEndPoint(f, typeName, methodName);
        }

        public static IVertex Create()
        {
            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            std = lib.AddVertex(null, "Std");

            string type = "m0.Lib.Std, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            AddFunction("Concatenate", type, "Concatenate", "String", new TypeName[] { new TypeName("input", "String", 0, -1) });
            AddFunction("Split", type, "Split", "String", new TypeName[] { new TypeName("input", "String", 0, -1) });
            AddFunction("SplitBy", type, "SplitBy", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("by", "String", 1, -1) });
            AddFunction("Replace", type, "Replace", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("from", "String"), new TypeName("to", "String") });
            AddFunction("IndexOf", type, "IndexOf", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("test", "String") });
            AddFunction("Substring", type, "Substring", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("from", "Integer"), new TypeName("to", "Integer") });


            AddFunction("Sqrt", type, "Sqrt", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Pow", type, "Pow", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1), new TypeName("power", "Float") });
            AddFunction("Abs", type, "Abs", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Celling", type, "Celling", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Floor", type, "Floor", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Sin", type, "Sin", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Cos", type, "Cos", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Exp", type, "Exp", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Log", type, "Log", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Log10", type, "Log10", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Max", type, "Max", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Min", type, "Min", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Sign", type, "Sign", "Integer", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Tan", type, "Tan", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction("Randomize", type, "Randomize", "Float", new TypeName[] { new TypeName("value", "Float") });
            AddFunction("Random", type, "Random", "Float", new TypeName[] { new TypeName("max", "Float", 0, -1) });

            return std;
        }
    }
}
