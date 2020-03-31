using m0.Foundation;
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

        static void AddFunction(string name, string ret, IList<TypeName> pars)
        {
            IVertex zu = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroUML");

            IVertex zt = m0.MinusZero.Instance.root.Get(false, "System\\Meta\\ZeroTypes");

            IVertex f = std.AddVertex(zu.Get(false, "Function"), name);

            if (ret != null)
                f.AddEdge(zu.Get(false, "Function\\Output"), zt.Get(false, ret));

            foreach(TypeName tn in pars)
            {
                IVertex ip = f.AddVertex(zu.Get(false, "Function\\InputParameter"), tn.Name);
                ip.AddEdge(zu.Get(false, "$Edge"))
            }


        }

        public static IVertex Create()
        {
            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            std = lib.AddVertex(null, "Std");

            AddFunction("StringConcat", "String", new TypeName[] { new TypeName("input", "String") });

            return std;
        }
    }
}
