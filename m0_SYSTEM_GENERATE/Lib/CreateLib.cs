using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Util;

using static m0_SYSTEM_GENERATE.Util.GenerateUtil;

using static m0_SYSTEM_GENERATE.Program;

namespace m0_SYSTEM_GENERATE.Lib
{
    public class CreateLib
    {
        public static IVertex LibStd;        

        public static void Create()
        {
            print("* creating Lib\\Std");

            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            LibStd = lib.AddVertex(null, "Std");

            string type = "m0.Lib.Std, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            AddFunction(LibStd, "Concatenate", type, "Concatenate", "String", new TypeName[] { new TypeName("input", "String", 0, -1) });
            AddFunction(LibStd, "Split", type, "Split", "String", new TypeName[] { new TypeName("input", "String", 0, -1) });
            AddFunction(LibStd, "SplitBy", type, "SplitBy", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("by", "String", 1, -1) });
            AddFunction(LibStd, "Replace", type, "Replace", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("from", "String"), new TypeName("to", "String") });
            AddFunction(LibStd, "IndexOf", type, "IndexOf", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("test", "String") });
            AddFunction(LibStd, "Substring", type, "Substring", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("from", "Integer"), new TypeName("to", "Integer") });


            AddFunction(LibStd, "Sqrt", type, "Sqrt", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Pow", type, "Pow", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1), new TypeName("power", "Float") });
            AddFunction(LibStd, "Abs", type, "Abs", "Integer", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Celling", type, "Celling", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Floor", type, "Floor", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Sin", type, "Sin", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Cos", type, "Cos", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Exp", type, "Exp", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Log", type, "Log", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Log10", type, "Log10", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Max", type, "Max", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Min", type, "Min", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Sign", type, "Sign", "Integer", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Tan", type, "Tan", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Randomize", type, "Randomize", null, new TypeName[] { new TypeName("seed", "Integer") });
            AddFunction(LibStd, "Random", type, "Random", "Float", new TypeName[] { new TypeName("max", "Float", 0, -1) });

            AddFunction(LibStd, "Sequence", type, "Sequence", "Integer", new TypeName[] { new TypeName("min", "Integer", 1, 1), new TypeName("max", "Integer", 1, 1) });
            AddFunction(LibStd, "StepSequence", type, "StepSequence", "Integer", new TypeName[] { new TypeName("min", "Float", 1, 1), new TypeName("max", "Float", 1, 1), new TypeName("step", "Float", 1, 1) });

            AddFunction(LibStd, "Sleep", type, "Sleep", null, new TypeName[] {new TypeName("miliseconds", "Integer", 1, 1) });

            GraphUtil.AddClass(LibStd, "View");            
        }

        public static void Save(List<IVertex> systemSubGraphWithLinks, Dictionary<string, StoreId> storeOverride)
        {            
            print("* saving Lib\\Std");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_std.m0", LibStd, systemSubGraphWithLinks, storeOverride);            
        }

    }
}
