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

            public TypeName(string _name, string _type)
            {
                Name = _name;
                Type = _type;
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
            }

            GraphUtil.AddDotNetEndPoint(f, typeName, methodName);
        }

        public static IVertex Create()
        {
            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            std = lib.AddVertex(null, "Std");

            string type = "m0.ZeroUML.Instructions.BaseInstructions, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            AddFunction("Concatenate", type, "Concatenate", "String", new TypeName[] { new TypeName("input", "String") });



            AddFunction("Split", type, "Split", "String", new TypeName[] { new TypeName("input", "String") });
            AddFunction("SplitBy", type, "SplitBy", "String", new TypeName[] { new TypeName("input", "String") });
            AddFunction("Replace", type, "Replace", "String", new TypeName[] { new TypeName("input", "String") });
            AddFunction("Contains", type, "Contains", "String", new TypeName[] { new TypeName("input", "String") });
            AddFunction("Substring", type, "Substring", "String", new TypeName[] { new TypeName("input", "String") });


            AddFunction("Sqrt", type, "Sqrt", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Pow", type, "Pow", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Abs", type, "Abs", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Celling", type, "Celling", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Floor", type, "Floor", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Sin", type, "Sin", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Cos", type, "Cos", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Exp", type, "Exp", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Log", type, "Log", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Log10", type, "Log10", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Max", type, "Max", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Min", type, "Min", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Sign", type, "Sign", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Tan", type, "Tan", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Randomize", type, "Randomize", "Double", new TypeName[] { new TypeName("input", "Double") });
            AddFunction("Random", type, "Random", "Double", new TypeName[] { new TypeName("input", "Double") });

            return std;
        }
    }
}
