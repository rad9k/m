using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Lib;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static m0_SYSTEM_GENERATE.Program;
using static m0_SYSTEM_GENERATE.Util.GenerateUtil;

namespace m0_SYSTEM_GENERATE.Lib
{
    public class CreateLib
    {
        static IVertex r = m0.MinusZero.Instance.root;

        static IVertex StringMeta = r.Get(false, @"System\Meta\ZeroTypes\String");
        static IVertex IntegerMeta = r.Get(false, @"System\Meta\ZeroTypes\Integer");
        static IVertex BooleanMeta = r.Get(false, @"System\Meta\ZeroTypes\Boolean");
        static IVertex FloatMeta = r.Get(false, @"System\Meta\ZeroTypes\Float");
        static IVertex ColorMeta = r.Get(false, @"System\Meta\ZeroTypes\UX\Color");
        static IVertex ExecutableMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\Executable");

        static IVertex LibStd;
        static IVertex LibSys;
        static IVertex LibStdUI;
        static IVertex LibStdView;
        static IVertex LibNet;

        public static void CreateLibStd()
        {
            print("* creating Lib\\Std");

            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            LibStd = lib.AddVertex(null, "Std");

            string type = "m0.Lib.Std, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            AddFunction(LibStd, "AlphabeticalSortByQuery", type, "AlphabeticalSortByQuery", "VertexType", new TypeName[] { new TypeName("toSortVertex", "VertexType", 0, -1), new TypeName("sortVertexQueryString", "String", 0, -1) });
            AddFunction(LibStd, "NumericSortByQuery", type, "NumericSortByQuery", "VertexType", new TypeName[] { new TypeName("toSortVertex", "VertexType", 0, -1), new TypeName("sortVertexQueryString", "String", 0, -1) });
            AddFunction(LibStd, "AlphabeticalSort", type, "AlphabeticalSort", "VertexType", new TypeName[] { new TypeName("toSortVertex", "VertexType", 0, -1) });
            AddFunction(LibStd, "NumericSort", type, "NumericSort", "VertexType", new TypeName[] { new TypeName("toSortVertex", "VertexType", 0, -1) });

            AddFunction(LibStd, "Concatenate", type, "Concatenate", "String", new TypeName[] { new TypeName("input", "String", 0, -1) });
            AddFunction(LibStd, "Split", type, "Split", "String", new TypeName[] { new TypeName("input", "String", 0, -1) });
            AddFunction(LibStd, "SplitBy", type, "SplitBy", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("by", "String", 1, -1) });
            AddFunction(LibStd, "Replace", type, "Replace", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("from", "String"), new TypeName("to", "String") });
            AddFunction(LibStd, "IndexOf", type, "IndexOf", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("test", "String") });
            AddFunction(LibStd, "Substring", type, "Substring", "String", new TypeName[] { new TypeName("input", "String", 0, -1), new TypeName("from", "Integer"), new TypeName("to", "Integer") });

            AddFunction(LibStd, "IsNumeric", type, "IsNumeric", "Boolean", new TypeName[] { new TypeName("input", "VertexType", 0, -1)});


            AddFunction(LibStd, "Sqrt", type, "Sqrt", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Pow", type, "Pow", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1), new TypeName("power", "Float") });
            AddFunction(LibStd, "Abs", type, "Abs", "Integer", new TypeName[] { new TypeName("value", "Float", 0, -1) });
            AddFunction(LibStd, "Truncate", type, "Truncate", "Float", new TypeName[] { new TypeName("value", "Float", 0, -1) });
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
        }

        public static void CreateLibSys()
        {
            print("* creating Lib\\Sys");

            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            LibSys = lib.AddVertex(null, "Sys");

            string type = "m0.Lib.Sys, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            AddFunction(LibSys, "StartTransaction", type, "StartTransaction", null, new TypeName[] { });
            AddFunction(LibSys, "CommitTransaction", type, "CommitTransaction", null, new TypeName[] { });
            AddFunction(LibSys, "RollbackTransaction", type, "RollbackTransaction", null, new TypeName[] { });
        }

        public static void CreateLibStdUI()
        {
            print("* creating Lib\\StdUI");

            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            LibStdUI = lib.AddVertex(null, "StdUI");

            string type = "m0.Lib.StdUI, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            AddFunction(LibStdUI, "InteractionOutput", type, "InteractionOutput", null, new TypeName[] { new TypeName("output", "String", 1, 1) });
            AddFunction(LibStdUI, "InteractionInput", type, "InteractionInput", "String", new TypeName[] { new TypeName("output", "String", 1, 1) });
            AddFunction(LibStdUI, "InteractionSelect", type, "InteractionSelect", "VertexType", new TypeName[] { new TypeName("output", "String", 0, 1), new TypeName("option", "VertexType", 0, -1) });
            AddFunction(LibStdUI, "InteractionSelectButton", type, "InteractionSelectButton", "VertexType", new TypeName[] { new TypeName("output", "String", 0, 1), new TypeName("option", "VertexType", 0, -1) });
            AddFunction(LibStdUI, "OpenDefaultVisualiser", type, "OpenDefaultVisualiser", null, new TypeName[] { new TypeName("baseEdge", "Edge", 0, 1) });
            AddFunction(LibStdUI, "OpenVisualiser", type, "OpenVisualiser", null, new TypeName[] { new TypeName("baseEdge", "Edge", 1, 1), new TypeName("visualiser", "VertexType", 1, 1) });
            AddFunction(LibStdUI, "OpenFormVisualiser", type, "OpenFormVisualiser", null, new TypeName[] { new TypeName("baseEdge", "Edge", 1, 1) });
            AddFunction(LibStdUI, "OpenCodeVisualiser", type, "OpenCodeVisualiser", null, new TypeName[] { new TypeName("baseEdge", "Edge", 1, 1) });
        }

        public static void CreateLibStdView()
        {
            print("* creating Lib\\StdView");

            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            LibStdView = lib.AddVertex(null, "StdView");

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Lib\StdView\StdView.txt", LibStdView);

            IVertex VertexToJson_Transform_Vertex = LibStdView.Get(false, "VertexToJson_Transform");
            IVertex JsonToVertex_Transform_Vertex = LibStdView.Get(false, "JsonToVertex_Transform");

            ExecutionFlowHelper.DecorateWithDotNetStaticMethod(VertexToJson_Transform_Vertex,
                "m0.Lib.StdView.VertexToJson, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                "VertexToJson_Transform");

            ExecutionFlowHelper.DecorateWithDotNetStaticMethod(JsonToVertex_Transform_Vertex,
                "m0.Lib.StdView.JsonToVertex, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", 
                "JsonToVertex_Transform");

            LibStdView.AddVertex(null, "$IsJsonArray");
        }

        public static void CreateLibNet()
        {
            print("* creating Lib\\Net");

            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, "System").AddVertex(null, "Lib");

            IVertex IntegerType = root.Get(false, @"System\Meta\ZeroTypes\Integer");
            IVertex BooleanType = root.Get(false, @"System\Meta\ZeroTypes\Boolean");
            IVertex StringType = root.Get(false, @"System\Meta\ZeroTypes\String");

            IVertex default_meta = root.Get(false, @"System\Meta\Base\Vertex\$DefaultValue");

            LibNet = lib.AddVertex(null, "Net");

            string type = "m0.Lib.Net.Net, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            // HttpActionEnum

            IVertex HttpActionEnumVertex = GraphUtil.AddEnum(LibNet, "HttpActionEnum", new string[] { "GET", "PUT", "POST", "DELETE", "PATCH", "HEADOPTIONS", "TRACE" });

            // HttpMappig

            IVertex httpMapingVertex = GraphUtil.AddClass(LibNet, "HttpMapping");

            IVertex httpMapingEntryVertex = GraphUtil.AddClass(LibNet, "HttpMappingEntry");

            GraphUtil.AddAggregation(httpMapingVertex, "HttpMappingEntry", httpMapingEntryVertex, 1, -1);

            GraphUtil.AddAssociation(httpMapingEntryVertex, "Action", HttpActionEnumVertex, 1, 1);
            GraphUtil.AddAttribute(httpMapingEntryVertex, "PathMask", StringMeta, 1, 1);
            GraphUtil.AddAssociation(httpMapingEntryVertex, "Handler", ExecutableMeta, 1, 1);

            // HttpServer

            IVertex httpServerVertex = GraphUtil.AddClass(LibNet, "HttpServer");

            GraphUtil.AddAttribute(httpServerVertex, "Mapping", httpMapingVertex, 1, 1);
            GraphUtil.AddAttribute(httpServerVertex, "Port", IntegerType, 1, 1);

            IVertex doLogAttributeVertex = GraphUtil.AddAttribute(httpServerVertex, "DoLog", BooleanType, 1, 1);
            doLogAttributeVertex.AddVertex(default_meta, "True");

            IVertex logFilenameVertex = GraphUtil.AddAttribute(httpServerVertex, "LogFilename", BooleanType, 1, 1);
            logFilenameVertex.AddVertex(default_meta, "");

            IVertex httpServer_InitVertex = AddMethod(httpServerVertex, "HttpServer", null, new TypeName[] { new TypeName("p_mapping", httpMapingVertex, 0, -1), new TypeName("p_port", "Integer", 0, 1) });

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Lib\Net\HttpServer_Init.txt", httpServer_InitVertex);

            AddMethod(httpServerVertex, "Start", type, "HttpServer_Start", null, new TypeName[] {});
            AddMethod(httpServerVertex, "Stop", type, "HttpServer_Stop", null, new TypeName[] {});

            AddFunction(LibNet, "HttpHandler", null, null, "String", new TypeName[] { new TypeName("url", "String") });

        }

        public static void Save(IEnumerable<IVertex> systemSubGraphWithLinks, Dictionary<string, StoreId> storeOverride)
        {            
            print("* saving Lib\\Std");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_std.m0j", LibStd, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* saving Lib\\Sys");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_sys.m0j", LibSys, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* saving Lib\\StdUI");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_stdui.m0j", LibStdUI, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* saving Lib\\StdView");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_stdview.m0j", LibStdView, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);

            print("* saving Lib\\Net");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_net.m0j", LibNet, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);
        }

    }
}
