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

        }

        public static IVertex Create()
        {
            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            std = lib.AddVertex(null, "Std");

            AddFunction("StringConcat", "String", new TypeName[] { new TypeName("input", "sting") });

            return std;
        }
    }
}
