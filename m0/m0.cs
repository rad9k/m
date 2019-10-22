using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.Store;
using m0.Store.FileSystem;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace m0
{
    public class MinusZero : IStoreUniverse, IDisposable
    {
        public static bool flag = false; // for debug

        public static MinusZero Instance = new MinusZero();

        public bool IsInitialized = false;

        public AccessLevelEnum[] GetStoreDefaultAccessLevelList = new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions };


        IList<IStore> stores = new List<IStore>();

        public IList<IStore> Stores { get { return stores; } }


        IStore tempstore;

        public IStore TempStore { get { return tempstore; } }


        IVertex root;

        public IVertex Root { get { return root; } }

        IVertex inherits;

        public IVertex Inherits { get { return inherits; } }

        IVertex stackFrameInherits;

        public IVertex StackFrameInherits { get { return stackFrameInherits; } }

        IVertex empty;

        public IVertex Empty { get { return empty; } }

        IUserInteraction _DefaultUserInteraction;

        public IUserInteraction DefaultUserInteraction { get { return _DefaultUserInteraction; } }

        IParser _DefaultParser;

        public IParser DefaultParser { get { return _DefaultParser; } }


        IExecuter _DefaultExecuter;

        public IExecuter DefaultExecuter { get { return _DefaultExecuter; } }

        IVertex _DefaultFormalTextLanguage;

        //

        IParser New_DefaultParser;

        public IParser NewDefaultParser { get { return New_DefaultParser; } }


        IExecuter New_DefaultExecuter;

        public IExecuter NewDefaultExecuter { get { return New_DefaultExecuter; } }

        //

        public IVertex DefaultFormalTextLanguage { get { return _DefaultFormalTextLanguage; } }


        IVertex _DefaultLanguageDefinition_ForOldParser;

        public IVertex DefaultLanguageDefinition_ForOldParser { get { return _DefaultLanguageDefinition_ForOldParser; } }


        IVertex _MetaFormalTextLanguageParsedTreeVertex;

        public IVertex MetaFormalTextLanguageParsedTreeVertex { get { return _MetaFormalTextLanguageParsedTreeVertex; } }


        ICodeGenerator _DefaultCodeGenerator;

        public ICodeGenerator DefaultCodeGenerator { get { return _DefaultCodeGenerator; } }


        public IVertex EdgeTarget;
        public IVertex Is;
        public IVertex IsAggregation;

        public bool IsGUIDragging { get; set; }

        //

        public IVertex CreateTempVertex()
        {
            return new EasyVertex(this.tempstore);
        }

        public IVertex tempRoot;

        public IEdge CreateTempEdge()
        {
            return new EasyEdge(tempRoot, empty, CreateTempVertex());
        }

        void PreBootstrap()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
        }

        void Bootstrap()
        {
            IStore rootstore = new MemoryStore("$-0$ROOT$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

            root = rootstore.Root;


            Stores.Clear();

            Stores.Add(rootstore);


            tempstore = new MemoryStore("$-0$TEMP$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions }, true);


            empty = new IdentifiedVertex("$Empty", rootstore);

            empty.Value = "$Empty";


            tempRoot = CreateTempVertex();

        }

        void Init()
        {
            _DefaultUserInteraction = m0Main.Instance;

            ZeroCode.ZeroCodeEngine_OLD zeroCodeEngine_OLD = new ZeroCode.ZeroCodeEngine_OLD();

            _DefaultParser = zeroCodeEngine_OLD;

            _DefaultExecuter = zeroCodeEngine_OLD;                        
        }

        void Init_AfterZeroCodeDefintionCreated()
        {
            ZeroCode.ZeroCodeEngine zeroCodeEngine = new ZeroCode.ZeroCodeEngine();

            New_DefaultParser = zeroCodeEngine;
            New_DefaultExecuter = zeroCodeEngine;

            _DefaultCodeGenerator = zeroCodeEngine;
        }


        void CreateSystem()
        {
            IVertex system = Root.AddVertex(null, "System");

            // turned off for now
            // system.AddVertex(null,"Session").AddVertex(null,"Visualisers");

            IVertex meta = system.AddVertex(null, "Meta");

            IVertex tl = system.AddVertex(null, "FormalTextLanguage");

            IVertex mtl = meta.AddVertex(null, "FormalTextLanguage");

            IVertex sto = meta.AddVertex(null, "Store");

            // Meta\FormalTextLanguage\Parser

            IVertex mtp = mtl.AddVertex(null, "Parser");

            IVertex ptmd = mtp.AddVertex(null, "PreviousTerminalMoveDown");

            IVertex mdtpnltoce = mtp.AddVertex(null, "MoveDownToPreviousContainerTerminalOrCretedEmpty");

            IVertex ct = mtp.AddVertex(null, "ContainerTerminal");


            // Meta\FormalTextLanguage\ParsedTree

            IVertex mtpt = mtl.AddVertex(null, "ParsedTree");

            _MetaFormalTextLanguageParsedTreeVertex = mtpt;

            IVertex empty = mtpt.AddVertex(null, "$EmptyContainerTerminal");
            empty.AddVertex(ct, null);

            // FormalTextLanguage\ZeroCode

            // later


            // FormalTextLanguage\ZeroCode_OLD

            IVertex zco = tl.AddVertex(null, "ZeroCode_OLD");

            _DefaultLanguageDefinition_ForOldParser = zco;

            zco.AddVertex(null, ",");

            IVertex colon = zco.AddVertex(null, ":");
            colon.AddVertex(ptmd, 1);
            colon.AddVertex(ct, null);

            zco.AddVertex(null, "\\");
            zco.AddVertex(null, "*");
            zco.AddVertex(null, "{").AddVertex(mdtpnltoce, null);
            zco.AddVertex(null, "}").AddVertex(mdtpnltoce, null);
            zco.AddVertex(null, "=");
            zco.AddVertex(null, "!=");
        }

        void CreateSystemMeta()
        {
            GeneralUtil.ParseAndExcute(Root.Get(false, @"System\Meta"), null, "{}");
        }

        void CreatePresentation()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Presentation{$Hide,$UpdateAfterInteractionEnd}}");
        }

        void CreateSystemMetaBase()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(sm, null, "{Base{$,Vertex{$IsLink,$Inherits,$StackFrameInherits,$Is,$EdgeTarget,$VertexTarget,$IsAggregation,$MinCardinality,$MaxCardinality,$MinTargetCardinality,$MaxTargetCardinality,$DefaultValue,$DefaultViewVisualiser,$DefaultEditVisualiser,$DefaultOpenVisualiser,$Group,$Section,$Description,$ExecutableEndPoint,Author,Dependency},$Empty,$Import,$ImportMeta,$Keyword,$KeywordGroupDefinition,$$KeywordGroup,$$KeywordManyRoot,$$LocalRoot,$$StartInLocalRoot,$$EmptyKeyword,$$NewVertexKeyword,$$LinkKeyword,$$NonSelfRecursiveParameters,$NewLine,$ParseRoot,$ParseArtefacts}}");

            sm.Get(false, @"Presentation\$Hide").AddEdge(sm.Get(false, @"Base\Vertex\$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            empty = sm.Get(false, @"Base\$Empty"); // there are some bugs related to this and old zeroscript.get

            inherits = sm.Get(false, @"Base\Vertex\$Inherits");

            stackFrameInherits = sm.Get(false, @"Base\Vertex\$StackFrameInherits");


            sm.Get(false, @"Base\Vertex\$Is").AddEdge(sm.Get(false, @"Presentation\$Hide"), empty);



            //

            //IVertex _vertex_ = sm.AddVertex(null, "_Vertex_");

            // sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex")); // TO BE DONE. now there is very strange error in query mechanics

            // sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, @"Base\Vertex\$EdgeTarget"), _vertex_); // not working too...

            sm.Get(false, @"Base\Vertex\$VertexTarget").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            // sm.Get(false, @"Base\Vertex\$VertexTarget").AddEdge(sm.Get(false, @"*$EdgeTarget"), _vertex_);

            sm.Get(false, @"Base\Vertex\$Inherits").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            sm.Get(false, @"Base\Vertex\$Is").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));


            //sm.Get(false, @"Base\Vertex\$DefaultViewVisualiser").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            //sm.Get(false, @"Base\Vertex\$DefaultEditVisualiser").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            // sm.Get(false, @"Base\Vertex\$DefaultOpenVisualiser").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));  

            // hack for now

            sm.Get(false, @"Base\Vertex\$DefaultViewVisualiser").AddEdge(sm.Get(false, @"*$IsLink"), sm.Get(false, @"Base\Vertex"));

            sm.Get(false, @"Base\Vertex\$DefaultEditVisualiser").AddEdge(sm.Get(false, @"*$IsLink"), sm.Get(false, @"Base\Vertex"));

            sm.Get(false, @"Base\Vertex\$DefaultOpenVisualiser").AddEdge(sm.Get(false, @"*$IsLink"), sm.Get(false, @"Base\Vertex"));


            sm.Get(false, @"Base\Vertex\$IsAggregation").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));
        }

        void CreateSystemMetaZeroUML()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(sm, null, "{ZeroUML{Type,AtomType,StateMachine{State{Transition}},Enum{EnumValue},Selector,Class{Attribute{MinValue,MaxValue},Association,Aggregation}}}");

            GeneralUtil.ParseAndExcute(sm.Get(false, @"ZeroUML\Selector"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            sm.Get(false, @"ZeroUML\Class\Attribute").AddEdge(sm.Get(false, @"*$IsAggregation"), empty);

            sm.Get(false, @"ZeroUML\Class\Aggregation").AddEdge(sm.Get(false, @"*$IsAggregation"), empty);

            GeneralUtil.ParseAndExcute(sm.Get(false, @"ZeroUML\Enum\EnumValue"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            sm.Get(false, @"ZeroUML\Enum\EnumValue").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            sm.Get(false, @"ZeroUML\Enum\EnumValue").AddEdge(sm.Get(false, @"*$IsAggregation"), empty);

            GeneralUtil.ParseAndExcute(sm.Get(false, @"ZeroUML\StateMachine\State"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            GeneralUtil.ParseAndExcute(sm.Get(false, @"ZeroUML\StateMachine\State\Transition"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            sm.Get(false, @"ZeroUML\StateMachine\State\Transition").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroUML\StateMachine\State"));


            sm.Get(false, @"ZeroUML\Class").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroUML\Type"));

            sm.Get(false, @"ZeroUML\Class").AddEdge(null, sm.Get(false, "*$Inherits"));



            Root.Get(false, @"System\Meta\ZeroUML\Class\Attribute").AddEdge(sm.Get(false, @"*$Inherits"), sm.Get(false, @"ZeroUML\Selector"));
            Root.Get(false, @"System\Meta\ZeroUML\Class\Attribute").AddEdge(sm.Get(false, @"*$VertexTarget"), sm.Get(false, @"ZeroUML\Type"));

            Root.Get(false, @"System\Meta\ZeroUML\Class\Association").AddEdge(sm.Get(false, @"*$Inherits"), sm.Get(false, @"ZeroUML\Selector"));
            Root.Get(false, @"System\Meta\ZeroUML\Class\Association").AddEdge(sm.Get(false, @"*$VertexTarget"), sm.Get(false, @"ZeroUML\Class"));

            Root.Get(false, @"System\Meta\ZeroUML\Class\Aggregation").AddEdge(sm.Get(false, @"*$Inherits"), sm.Get(false, @"ZeroUML\Selector"));
            Root.Get(false, @"System\Meta\ZeroUML\Class\Aggregation").AddEdge(sm.Get(false, @"*$VertexTarget"), sm.Get(false, @"ZeroUML\Class"));


            // sm.Get(false, @"ZeroUML\Type").AddEdge(sm.Get(false, "*$Inherits"),sm.Get(false, @"Base\Vertex"));    // do not want it at last for now        

            sm.Get(false, @"ZeroUML\AtomType").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroUML\Type"));
            sm.Get(false, @"ZeroUML\Enum").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroUML\AtomType")); // was ZeroUML\Type
            sm.Get(false, @"ZeroUML\StateMachine").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroUML\AtomType"));
        }

        void AddDotNetEndPoint(IVertex baseVertex, string _methodName)
        {
            IVertex callableEndPoint = Root.Get(false, @"System\Meta\Base\Vertex\$ExecutableEndPoint");
            IVertex dotNetEndPoint = Root.Get(false, @"System\Meta\ZeroTypes\DotNetEndPoint");
            IVertex typeName = Root.Get(false, @"System\Meta\ZeroTypes\DotNetEndPoint\TypeName");
            IVertex methodName = Root.Get(false, @"System\Meta\ZeroTypes\DotNetEndPoint\MethodName");
            IVertex _is = Root.Get(false, @"System\Meta\Base\Vertex\$Is");

            IVertex n = baseVertex.AddVertex(callableEndPoint, null);
            n.AddEdge(_is, dotNetEndPoint);
            n.AddVertex(typeName, "m0.ZeroUML.Instructions.BaseInstructions");
            
            n.AddVertex(methodName, _methodName);
            
        }

        void AddCodeContainerEndPoint(IVertex baseVertex)
        {
            IVertex callableEndPoint = Root.Get(false, @"System\Meta\Base\Vertex\$ExecutableEndPoint");
            IVertex codeContainerEndPoint = Root.Get(false, @"System\Meta\ZeroTypes\CodeContainer");
            IVertex _is = Root.Get(false, @"System\Meta\Base\Vertex\$Is");

            IVertex n = baseVertex.AddVertex(callableEndPoint, null);
            n.AddEdge(_is, codeContainerEndPoint);            
        }

        void CreateSystemMetaZeroUML_Action_part()
        {
            IVertex smu = Root.Get(false, @"System\Meta\ZeroUML");
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex isAggregation = Root.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");

            // "\ " > "\"
            // "|" > ":"
            // "||" > "::"        

            GeneralUtil.ParseAndExcute(smu, sm,
                "{Link{Target{$MinCardinality:1,$MaxCardinality:1}},ExpressionAtom,Atom" +
                ",SingleOperator{NextExpression{$MinCardinality:1,$MaxCardinality:1}}" +
                ",SingleExpressionOperator{Expression{$MinCardinality:1,$MaxCardinality:1}}" +                
                ",DoubleOperator{LeftExpression{$MinCardinality:1,$MaxCardinality:1},RightExpression{$MinCardinality:1,$MaxCardinality:1}}" +                
                ",MultiOperator{Expression{$MinCardinality:0,$MaxCardinality:-1}}" +
                ",Query" +
                ",[]{Target{$MinCardinality:0,$MaxCardinality:1}}" +
                ",SetIndex,SetCount" +
                ",\"{}\",InnerCreation,EdgeSetAdd,EdgeSetSubstract,+,-,Mul,/,?,\"\\ \",\"|\",\"||\",CopySet,MetaToTo,(),RedirectLeftEdgesToRightVertices,AddLeftEdgesToRightVertices,AddRightEdgesIntoLeftEdges,DeleteRightVertices,DeleteRightEdgesFromLeftEdges,DeleteRightVerticesFromLeftEdges" +
                ",Equal,ExactEqual,NotEqual,Negation,And,Or,MoreThan,LessThan,MoreOrEqualThan,LessOrEqualThan" +
                ",Action,Return{Expression{$MinCardinality:0,$MaxCardinality:1}},NextOut{Next{$MinCardinality:0,$MaxCardinality:1}}" +
                ",StackFrameCreator{Do{$MinCardinality:0,$MaxCardinality:1},Variable{$MinCardinality:0,$MaxCardinality:-1},Type{$MinCardinality:0,$MaxCardinality:-1}}" +
                ",StackFrameCreatorWithInputOutput{Output{$MinCardinality:0,$MaxCardinality:1},InputParameter{$MinCardinality:0,$MaxCardinality:-1}}" +
                ",Function,Section" +
                ",If{Test{$MinCardinality:1,$MaxCardinality:1},Then{$MinCardinality:0,$MaxCardinality:1},Else{$MinCardinality:0,$MaxCardinality:1}}" +
                ",Switch{Expression{$MinCardinality:1,$MaxCardinality:1},Case{Expression{$MinCardinality:1,$MaxCardinality:1}},Default}" +
                ",While{Test{$MinCardinality:1,$MaxCardinality:1}}" +
                ",ForEach{Variable{$MinCardinality:1,$MaxCardinality:1},Set{$MinCardinality:1,$MaxCardinality:1}}" +
                ",EmptySet,Constant" +
                ",Execute,Parse,ParseWithLanguage{FormalTextLanguage{$MinCardinality:0,$MaxCardinality:1}},Generate,GenerateWithLanguage{FormalTextLanguage{$MinCardinality:0,$MaxCardinality:1}}" +
                "}");

            // CallableEndPoint

            // Link

            AddDotNetEndPoint(smu.Get(false, "Link"), "Link");

            // query

            AddDotNetEndPoint(smu.Get(false, "Query"), "QueryOperator");
            AddDotNetEndPoint(smu.Get(false, "\"{}\""), "InnerOperator");
            AddDotNetEndPoint(smu.Get(false, "?"), "QuestionMarkOperator");
            AddDotNetEndPoint(smu.Get(false, "\"\\ \""), "SlashOperator");
            AddDotNetEndPoint(smu.Get(false, "\"|\""), "ColonOperator");            

            // edge operators
            
            AddDotNetEndPoint(smu.Get(false, "RedirectLeftEdgesToRightVertices"), "RedirectLeftEdgesToRightVertices");
            AddDotNetEndPoint(smu.Get(false, "AddLeftEdgesToRightVertices"), "AddLeftEdgesToRightVertices");
            AddDotNetEndPoint(smu.Get(false, "AddRightEdgesIntoLeftEdges"), "AddRightEdgesIntoLeftEdges");
            AddDotNetEndPoint(smu.Get(false, "DeleteRightVertices"), "DeleteRightVertices");
            AddDotNetEndPoint(smu.Get(false, "DeleteRightEdgesFromLeftEdges"), "DeleteRightEdgesFromLeftEdges");
            AddDotNetEndPoint(smu.Get(false, "DeleteRightVerticesFromLeftEdges"), "DeleteRightVerticesFromLeftEdges");

            // edge set operators

            AddDotNetEndPoint(smu.Get(false, "EdgeSetAdd"), "EdgeSetAdd");
            AddDotNetEndPoint(smu.Get(false, "EdgeSetSubstract"), "EdgeSetSubstract");
            AddDotNetEndPoint(smu.Get(false, "SetIndex"), "SetIndex");
            AddDotNetEndPoint(smu.Get(false, "SetCount"), "SetCount");
            AddDotNetEndPoint(smu.Get(false, "EmptySet"), "EmptySet");

            // number algebra operators

            AddDotNetEndPoint(smu.Get(false, "+"), "Add");
            AddDotNetEndPoint(smu.Get(false, "-"), "Substract");
            AddDotNetEndPoint(smu.Get(false, "Mul"), "Multiply");
            AddDotNetEndPoint(smu.Get(false, "/"), "Divide");

            // logic operators

            AddDotNetEndPoint(smu.Get(false, "Equal"), "Equal");
            AddDotNetEndPoint(smu.Get(false, "ExactEqual"), "ExactEqual");
            AddDotNetEndPoint(smu.Get(false, "NotEqual"), "NotEqual");
            AddDotNetEndPoint(smu.Get(false, "Negation"), "Negation");
            AddDotNetEndPoint(smu.Get(false, "And"), "And");
            AddDotNetEndPoint(smu.Get(false, "Or"), "Or");
            AddDotNetEndPoint(smu.Get(false, "MoreThan"), "MoreThan");
            AddDotNetEndPoint(smu.Get(false, "LessThan"), "LessThan");
            AddDotNetEndPoint(smu.Get(false, "MoreOrEqualThan"), "MoreOrEqualThan");
            AddDotNetEndPoint(smu.Get(false, "LessOrEqualThan"), "LessOrEqualThan");

            // general operators

            AddDotNetEndPoint(smu.Get(false, "()"), "Bracket");
            AddDotNetEndPoint(smu.Get(false, "[]"), "Call");
            AddDotNetEndPoint(smu.Get(false, "Return"), "Return");
            AddDotNetEndPoint(smu.Get(false, "ForEach"), "ForEach");
            AddDotNetEndPoint(smu.Get(false, "While"), "While");

            // stack operators

            AddDotNetEndPoint(smu.Get(false, @"StackFrameCreator\Variable"), "CreateStackEdge");

            // vertex creation operators

            AddDotNetEndPoint(smu.Get(false, "\"||\""), "DoubleColonOperator");
            AddDotNetEndPoint(smu.Get(false, "InnerCreation"), "InnerCreation");
            AddDotNetEndPoint(smu.Get(false, "CopySet"), "CopySet");
            AddDotNetEndPoint(smu.Get(false, "MetaToTo"), "MetaToTo");

            // meta

            AddDotNetEndPoint(smu.Get(false, "Execute"), "Execute");
            AddDotNetEndPoint(smu.Get(false, "Parse"), "Parse");
            AddDotNetEndPoint(smu.Get(false, "Generate"), "Generate");
            AddDotNetEndPoint(smu.Get(false, "ParseWithLanguage"), "Parse");
            AddDotNetEndPoint(smu.Get(false, "GenerateWithLanguage"), "Generate");



            ////////////////////////////////////////////////////////////////////////

            // method
            IVertex method = sm.Get(false, @"ZeroUML\Class").AddVertex(null, "Method");
            method.AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "StackFrameCreatorWithInputOutput"));
            GeneralUtil.ParseAndExcute(method, sm, "{$MinCardinality: 0,$MaxCardinality: -1}");

            // cycle edges
            smu.Get(false, @"StackFrameCreator").AddEdge(null, smu.Get(false, "Function"));


            // expression inherits
            smu.Get(false, @"Constant").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Atom"));
            smu.Get(false, @"EmptySet").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Constant"));

            smu.Get(false, @"ExpressionAtom").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Atom"));
            smu.Get(false, @"ExpressionAtom").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "NextOut"));
            smu.Get(false, @"SingleOperator").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "ExpressionAtom"));            
            smu.Get(false, @"SingleExpressionOperator").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, @"DoubleOperator").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "ExpressionAtom"));
            smu.Get(false, @"MultiOperator").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "ExpressionAtom"));

            smu.Get(false, @"Query").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, @"[]").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "MultiOperator"));
            smu.Get(false, @"SetIndex").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleExpressionOperator"));
            smu.Get(false, @"SetCount").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, "InnerCreation").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "MultiOperator"));
            smu.Get(false, "\"{}\"").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, "\"{}\"").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "MultiOperator"));

            smu.Get(false, @"+").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, @"-").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "Mul").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, @"/").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));

            smu.Get(false, "Equal").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "ExactEqual").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "NotEqual").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "Negation").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleExpressionOperator"));
            smu.Get(false, "And").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "Or").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "MoreThan").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "LessThan").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "MoreOrEqualThan").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "LessOrEqualThan").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));

            smu.Get(false, @"?").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, "\"\\ \"").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, "\"|\"").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "\"||\"").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, "CopySet").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, "MetaToTo").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, @"()").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleExpressionOperator"));            
            smu.Get(false, @"RedirectLeftEdgesToRightVertices").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, @"AddLeftEdgesToRightVertices").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, @"AddRightEdgesIntoLeftEdges").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, @"DeleteRightVertices").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, @"DeleteRightEdgesFromLeftEdges").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));
            smu.Get(false, @"DeleteRightVerticesFromLeftEdges").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "DoubleOperator"));

            // rest inherits
            smu.Get(false, @"Action").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Atom"));

            smu.Get(false, @"StackFrameCreatorWithInputOutput").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "StackFrameCreator"));

            smu.Get(false, @"Return").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Action"));
            smu.Get(false, @"Return").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "NextOut"));

            smu.Get(false, @"Section").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "NextOut"));
            smu.Get(false, @"Section").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Action"));
            smu.Get(false, @"Section").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "StackFrameCreator"));

            smu.Get(false, @"If").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Action"));

            smu.Get(false, @"If\Then").AddEdge(sm.Get(false, @"*$Inherits"), smu.Get(false, @"NextOut"));
            smu.Get(false, @"If\Else").AddEdge(sm.Get(false, @"*$Inherits"), smu.Get(false, @"NextOut"));

            smu.Get(false, @"Switch").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Action"));
            smu.Get(false, @"Switch\Case").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "NextOut"));
            smu.Get(false, @"Switch\Default").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "NextOut"));

            smu.Get(false, @"While").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "NextOut"));
            smu.Get(false, @"While").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Action"));

            smu.Get(false, @"ForEach").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "NextOut"));
            smu.Get(false, @"ForEach").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "Action"));

            smu.Get(false, @"Function").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "StackFrameCreatorWithInputOutput"));

            smu.Get(false, @"Execute").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, @"Parse").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));
            smu.Get(false, @"Generate").AddEdge(sm.Get(false, "*$Inherits"), smu.Get(false, "SingleOperator"));

            //Link
            smu.Get(false, @"Link\Target").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Vertex"));

            //expression edges
            smu.Get(false, @"StackFrameCreatorWithInputOutput\InputParameter").AddEdge(sm.Get(false, @"*$VertexTarget"), smu.Get(false, @"Type"));

            smu.Get(false, @"SingleOperator\NextExpression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));
            smu.Get(false, @"SingleExpressionOperator\Expression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));
            smu.Get(false, @"DoubleOperator\LeftExpression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));
            smu.Get(false, @"DoubleOperator\RightExpression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));            
            smu.Get(false, @"MultiOperator\Expression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));

            // $IsAggregation's for expressions
            smu.Get(false, @"SingleOperator\NextExpression").AddEdge(isAggregation, Empty);
            smu.Get(false, @"SingleExpressionOperator\Expression").AddEdge(isAggregation, Empty);
            smu.Get(false, @"DoubleOperator\LeftExpression").AddEdge(isAggregation, Empty);
            smu.Get(false, @"DoubleOperator\RightExpression").AddEdge(isAggregation, Empty);
            smu.Get(false, @"MultiOperator\Expression").AddEdge(isAggregation, Empty);            

            smu.Get(false, @"[]\Target").AddEdge(isAggregation, Empty); // XXX

            //rest edges
            smu.Get(false, @"Return\Expression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));
            smu.Get(false, @"NextOut\Next").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));

            //smu.Get(false, @"[]\Target").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"StackFrameCreator"));
            smu.Get(false, @"[]\Target").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom")); // XXX

            smu.Get(false, @"StackFrameCreator\Do").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));
            smu.Get(false, @"StackFrameCreator\Variable").AddEdge(sm.Get(false, @"*$VertexTarget"), smu.Get(false, @"Type"));
            smu.Get(false, @"StackFrameCreator\Type").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Type"));
            smu.Get(false, @"StackFrameCreatorWithInputOutput\Output").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Type"));

            smu.Get(false, @"If\Test").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));

            smu.Get(false, @"Switch\Expression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));
            smu.Get(false, @"Switch\Case\Expression").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));

            smu.Get(false, @"While\Test").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));

           // smu.Get(false, @"ForEach\Variable").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Query")); // better this
            smu.Get(false, @"ForEach\Set").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));

            // meta            
            smu.Get(false, @"ParseWithLanguage\FormalTextLanguage").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));
            smu.Get(false, @"GenerateWithLanguage\FormalTextLanguage").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Atom"));

            smu.Get(false, @"ParseWithLanguage\FormalTextLanguage").AddEdge(isAggregation, Empty);
            smu.Get(false, @"GenerateWithLanguage\FormalTextLanguage").AddEdge(isAggregation, Empty);


            // $IsAggregation's for EdgeTargets
            smu.Get(false, @"Return\Expression").AddEdge(isAggregation, Empty);
            smu.Get(false, @"NextOut\Next").AddEdge(isAggregation, Empty);

            smu.Get(false, @"StackFrameCreator\Do").AddEdge(isAggregation, Empty);
            smu.Get(false, @"StackFrameCreator\Variable").AddEdge(isAggregation, Empty);
            smu.Get(false, @"StackFrameCreator\Type").AddEdge(isAggregation, Empty);
            //  smu.Get(false, @"StackFrameCreatorWithInputOutput\Output").AddEdge(isAggregation, Empty); // this - no!

            smu.Get(false, @"If\Test").AddEdge(isAggregation, Empty);

            smu.Get(false, @"Switch\Expression").AddEdge(isAggregation, Empty);
            smu.Get(false, @"Switch\Case\Expression").AddEdge(isAggregation, Empty);

            smu.Get(false, @"While\Test").AddEdge(isAggregation, Empty);

            smu.Get(false, @"ForEach\Set").AddEdge(isAggregation, Empty);

            // package
            IVertex package = smu.AddVertex(null, "Package");

            AddCodeContainerEndPoint(smu.Get(false, "Package"));

            package.AddEdge(null, smu.Get(false, "Link"));
            package.AddEdge(null, smu.Get(false, "AtomType"));
            package.AddEdge(null, smu.Get(false, "StateMachine"));
            package.AddEdge(null, smu.Get(false, "Enum"));
            package.AddEdge(null, smu.Get(false, "Class"));
            package.AddEdge(null, smu.Get(false, "Query"));
            package.AddEdge(null, smu.Get(false, "[]"));
            package.AddEdge(null, smu.Get(false, "SetIndex"));
            package.AddEdge(null, smu.Get(false, "SetCount"));
            package.AddEdge(null, smu.Get(false, "InnerCreation"));
            package.AddEdge(null, smu.Get(false, "\"{}\""));
            package.AddEdge(null, smu.Get(false, "EdgeSetAdd"));
            package.AddEdge(null, smu.Get(false, "EdgeSetSubstract"));
            package.AddEdge(null, smu.Get(false, "EmptySet"));

            package.AddEdge(null, smu.Get(false, "+"));
            package.AddEdge(null, smu.Get(false, "-"));
            package.AddEdge(null, smu.Get(false, "Mul"));
            package.AddEdge(null, smu.Get(false, "/"));

            package.AddEdge(null, smu.Get(false, "Equal"));
            package.AddEdge(null, smu.Get(false, "ExactEqual"));
            package.AddEdge(null, smu.Get(false, "NotEqual"));
            package.AddEdge(null, smu.Get(false, "Negation"));
            package.AddEdge(null, smu.Get(false, "And"));
            package.AddEdge(null, smu.Get(false, "Or"));
            package.AddEdge(null, smu.Get(false, "MoreThan"));
            package.AddEdge(null, smu.Get(false, "LessThan"));
            package.AddEdge(null, smu.Get(false, "MoreOrEqualThan"));
            package.AddEdge(null, smu.Get(false, "LessOrEqualThan"));

            package.AddEdge(null, smu.Get(false, "?"));
            package.AddEdge(null, smu.Get(false, "\"\\ \""));
            package.AddEdge(null, smu.Get(false, "\"|\""));
            package.AddEdge(null, smu.Get(false, "\"||\""));
            package.AddEdge(null, smu.Get(false, "CopySet"));
            package.AddEdge(null, smu.Get(false, "MetaToTo"));
            package.AddEdge(null, smu.Get(false, "()"));            
            package.AddEdge(null, smu.Get(false, "RedirectLeftEdgesToRightVertices"));
            package.AddEdge(null, smu.Get(false, "AddLeftEdgesToRightVertices"));
            package.AddEdge(null, smu.Get(false, "AddRightEdgesIntoLeftEdges"));
            package.AddEdge(null, smu.Get(false, "DeleteRightVertices"));
            package.AddEdge(null, smu.Get(false, "DeleteRightEdgesFromLeftEdges"));
            package.AddEdge(null, smu.Get(false, "DeleteRightVerticesFromLeftEdges"));
            package.AddEdge(null, smu.Get(false, "Section"));
            package.AddEdge(null, smu.Get(false, "Function"));
            package.AddEdge(null, smu.Get(false, "If"));
            package.AddEdge(null, smu.Get(false, "Switch"));
            package.AddEdge(null, smu.Get(false, "While"));
            package.AddEdge(null, smu.Get(false, "ForEach"));
            package.AddEdge(null, sm.Get(false, @"Base\$Import"));
            package.AddEdge(null, sm.Get(false, @"Base\$ImportMeta"));
        }

        void CreateSystemFormalTextLanguegeZeroCode_Keywords()
        {
            IVertex zc = Root.Get(false, @"System\FormalTextLanguage\ZeroCode");
            IVertex k = zc.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\FormalTextLanguage\Keywords"), "");


            IVertex smu = Root.Get(false, @"System\Meta\ZeroUML");
            IVertex smb = Root.Get(false, @"System\Meta\Base");

            IVertex keyword = smb.Get(false, @"$Keyword");
            IVertex keywordGroup = smb.Get(false, @"$$KeywordGroup");
            IVertex keywordGroupDefinition = smb.Get(false, @"$KeywordGroupDefinition");

            IVertex kgd_ColonEmptyInner2SlashMarkIndexNewLink = k.AddVertex(keywordGroupDefinition, "ColonEmptyInner2SlashMarkIndexNewLink");
            IVertex kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy = k.AddVertex(keywordGroupDefinition, "ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy");
            IVertex kgd_ColonEmptyInner2SlashMarkIndex = k.AddVertex(keywordGroupDefinition, "ColonEmptyInner2SlashMarkIndex");
            IVertex kgd_Empty2Inner = k.AddVertex(keywordGroupDefinition, "Empty2Inner");
            IVertex kgd_InnerCreation = k.AddVertex(keywordGroupDefinition, "InnerCreation");
            IVertex kgd_SlashMarkIndex = k.AddVertex(keywordGroupDefinition, "SlashMarkIndex");
            IVertex kgd_SlashMarkIndexInner2 = k.AddVertex(keywordGroupDefinition, "SlashMarkIndexInner2");
            IVertex kgd_Inner = k.AddVertex(keywordGroupDefinition, "Inner");

            IVertex isAggregation = root.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");
            IVertex empty = root.Get(false, @"System\Meta\Base\$Empty");
            IVertex _is = root.Get(false, @"System\Meta\Base\Vertex\$Is");


            IVertex any = k.AddVertex(null, "(?<ANY>)");

            IVertex emptyKeyword = smb.Get(false, "$$EmptyKeyword");
            IVertex newVertexKeyword = smb.Get(false, "$$NewVertexKeyword");
            IVertex linkKeyword = smb.Get(false, "$$LinkKeyword");
            IVertex nonSelfRecursiveParameters = smb.Get(false, @"$$NonSelfRecursiveParameters");

            string anyString = "(?<ANY>)";


            // import meta
            //
            // import meta (?<name>) (?<link>)

            IVertex importMeta = k.AddVertex(keyword, "import meta (?<name>) (?<link>)");

            IVertex importMeta_name = importMeta.AddVertex(smb.Get(false, @"$ImportMeta"), "(?<name>)");

            importMeta.AddVertex(importMeta_name, "(?<link>)");


            // import
            //
            // import (?<name>) (?<link>)

            IVertex import = k.AddVertex(keyword, "import (?<name>) (?<link>)");

            IVertex import_name = import.AddVertex(smb.Get(false, @"$Import"), "(?<name>)");

            import.AddVertex(import_name, "(?<link>)");

            // import direct 
            //
            // import direct  (?<link>)

            IVertex importDirect = k.AddVertex(keyword, "import direct (?<link>)");

            IVertex importDirect_link = importDirect.AddVertex(smb.Get(false, @"$Direct"), "(?<link>)");


            // import direct meta
            //
            // import direct meta (?<link>)

            IVertex importDirectMeta = k.AddVertex(keyword, "import direct meta (?<link>)");

            IVertex importDirectMeta_link = importDirectMeta.AddVertex(smb.Get(false, @"$DirectMeta"), "(?<link>)");


            // comment
            //
            // # (?<text>)

            IVertex comment = k.AddVertex(keyword, "# (?<text>)");

            comment.AddVertex(smb.Get(false, @"Vertex\$Description"), "(?<text>)");
            

            // attribute
            //
            // attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) <<(?<MinValue>)..(?<MaxValue>)>>

            IVertex attribute3 = k.AddVertex(keyword, "attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) <<(?<MinValue>)..(?<MaxValue>)>>");


            IVertex attribute3_attribute = attribute3.AddVertex(smu.Get(false, @"Class\Attribute"), "(?<name>)");

            attribute3_attribute.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute3_attribute.AddVertex(smu.Get(false, @"Class\Attribute\MinValue"), "(?<MinValue>)");

            attribute3_attribute.AddVertex(smu.Get(false, @"Class\Attribute\MaxValue"), "(?<MaxValue>)");

            attribute3_attribute.AddVertex(smb.Get(false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            attribute3_attribute.AddVertex(smb.Get(false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            attribute3_attribute.AddEdge(isAggregation, empty);

            attribute3_attribute.AddEdge(_is, smu.Get(false, @"Class\Attribute"));

            // attribute
            //
            // attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) 

            IVertex attribute4 = k.AddVertex(keyword, "attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");


            IVertex attribute4_attribute = attribute4.AddVertex(smu.Get(false, @"Class\Attribute"), "(?<name>)");

            attribute4_attribute.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute4_attribute.AddVertex(smb.Get(false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            attribute4_attribute.AddVertex(smb.Get(false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            attribute4_attribute.AddEdge(isAggregation, empty);

            attribute4_attribute.AddEdge(_is, smu.Get(false, @"Class\Attribute"));


            // attribute
            //
            // attribute (?<name>) (?<type>) <<(?<MinValue>)..(?<MaxValue>)>>

            IVertex attribute2 = k.AddVertex(keyword, "attribute (?<name>) (?<type>) <<(?<xMinValue>)..(?<MaxValue>)>>");


            IVertex attribute2_attribute = attribute2.AddVertex(smu.Get(false, @"Class\Attribute"), "(?<name>)");

            attribute2_attribute.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute2_attribute.AddVertex(smu.Get(false, @"Class\Attribute\MinValue"), "(?<xMinValue>)");

            attribute2_attribute.AddVertex(smu.Get(false, @"Class\Attribute\MaxValue"), "(?<MaxValue>)");

            attribute2_attribute.AddEdge(isAggregation, empty);

            attribute2_attribute.AddEdge(_is, smu.Get(false, @"Class\Attribute"));

            // attribute
            //
            // attribute (?<name>) (?<type>)

            IVertex attribute = k.AddVertex(keyword, "attribute (?<name>) (?<type>)");

            IVertex attribute_attribute = attribute.AddVertex(smu.Get(false, @"Class\Attribute"), "(?<name>)");

            attribute_attribute.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute_attribute.AddEdge(isAggregation, empty);

            attribute_attribute.AddEdge(_is, smu.Get(false, @"Class\Attribute"));

            // variable
            //
            // variable (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex variable = k.AddVertex(keyword, "variable (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");

            IVertex variable_variable = variable.AddVertex(smu.Get(false, @"StackFrameCreator\Variable"), "(?<name>)");

            variable_variable.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            variable_variable.AddVertex(smb.Get(false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            variable_variable.AddVertex(smb.Get(false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            variable_variable.AddEdge(isAggregation, empty);

            variable_variable.AddEdge(_is, smu.Get(false, @"StackFrameCreator\Variable"));

            // variable
            //
            // variable (?<name>) (?<type>)

            IVertex variable2 = k.AddVertex(keyword, "variable (?<name>) (?<type>)");

            IVertex variable2_variable = variable2.AddVertex(smu.Get(false, @"StackFrameCreator\Variable"), "(?<name>)");

            variable2_variable.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            variable2_variable.AddEdge(isAggregation, empty);

            variable2_variable.AddEdge(_is, smu.Get(false, @"StackFrameCreator\Variable"));

            // aassociation
            //
            // association (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex association2 = k.AddVertex(keyword, "association (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");


            IVertex association2_association = association2.AddVertex(smu.Get(false, @"Class\Association"), "(?<name>)");

            association2_association.AddVertex(smb.Get(false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            association2_association.AddVertex(smb.Get(false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            association2_association.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            association2_association.AddEdge(_is, smu.Get(false, @"Class\Association"));


            // aassociation
            //
            // association (?<name>) (?<type>)

            IVertex association = k.AddVertex(keyword, "association (?<name>) (?<type>)");


            IVertex association_association = association.AddVertex(smu.Get(false, @"Class\Association"), "(?<name>)");

            association_association.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            association_association.AddEdge(_is, smu.Get(false, @"Class\Association"));

            // aggregation
            //
            // aggregation (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex aggregation2 = k.AddVertex(keyword, "aggregation (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");


            IVertex aggregation2_aggregation = aggregation2.AddVertex(smu.Get(false, @"Class\Aggregation"), "(?<name>)");

            aggregation2_aggregation.AddVertex(smb.Get(false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            aggregation2_aggregation.AddVertex(smb.Get(false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            aggregation2_aggregation.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            aggregation2_aggregation.AddEdge(_is, smu.Get(false, @"Class\Aggregation"));


            // aggregation
            //
            // aggregation (?<name>) (?<type>)

            IVertex aggregation = k.AddVertex(keyword, "aggregation (?<name>) (?<type>)");


            IVertex aggregation_aggregation = aggregation.AddVertex(smu.Get(false, @"Class\Aggregation"), "(?<name>)");

            aggregation_aggregation.AddVertex(smb.Get(false, @"Vertex\$EdgeTarget"), "(?<type>)");

            aggregation_aggregation.AddEdge(_is, smu.Get(false, @"Class\Aggregation"));
            
            // function
            //
            // function (?<name>) (?<returnType>) [(*(+, +)(?<paramType>) (?<paramName>)*)]

            IVertex function = k.AddVertex(keyword, "function (?<name>) (?<returnType>)((*(+, +)(?<paramType>) (?<paramName>)*))");


            IVertex function_function = function.AddVertex(smu.Get(false, @"Function"), "(?<name>)");

            function_function.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "Function"));

            function_function.AddVertex(smu.Get(false, @"Function\Output"), "(?<returnType>)");

            IVertex ffip = function_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "(?<paramName>)");

            ffip.AddVertex(smb.Get(false, @"Vertex\$VertexTarget"), "(?<paramType>)");

            ffip.AddEdge(smb.Get(false, @"$$KeywordManyRoot"), smb.Get(false, @"$Empty"));


            // function
            //
            // function (?<name>) [(*(+, +)(?<paramType>) (?<paramName>)*)]

            IVertex function2 = k.AddVertex(keyword, "function (?<name>) ((*(+, +)(?<paramType>) (?<paramName>)*))");

            IVertex function2_function = function2.AddVertex(smu.Get(false, @"Function"), "(?<name>)");

            function2_function.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "Function"));

            IVertex f2fip = function2_function.AddVertex(smu.Get(false, @"Function\InputParameter"), "(?<paramName>)");

            f2fip.AddVertex(smb.Get(false, @"Vertex\$VertexTarget"), "(?<paramType>)");

            f2fip.AddEdge(smb.Get(false, @"$$KeywordManyRoot"), smb.Get(false, @"$Empty"));

            // while
            //
            // while ((?<test>))

            IVertex wh = k.AddVertex(keyword, "while (?<test>)");

            IVertex whwh = wh.AddVertex(smu.Get(false, @"While"), "");

            whwh.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "While"));

            whwh.AddVertex(smu.Get(false, @"While\Test"), "(?<test>)");


            /////////////////////////////////////////////////////////
            //
            // graph algebra operators
            //            
            // =  RedirectLeftEdgesToRightVertices
            // += AddLeftEdgesToRightVertices
            // +< AddRightEdgesIntoLeftEdges
            // ~= DeleteRightVertices
            // -< DeleteRightEdgesFromLeftEdges
            // ~< DeleteRightVerticesFromLeftEdges
            //
            ////////////////////////////////////////////////////////            

            // =
            //
            // (?<left>) = (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) =(?<SUB>) (?<right>)", "RedirectLeftEdgesToRightVertices");

            // +=
            //
            // (?<left>) += (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) +=(?<SUB>) (?<right>)", "AddLeftEdgesToRightVertices");

            // +<
            //
            // (?<left>) +< (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) +<(?<SUB>) (?<right>)", "AddRightEdgesIntoLeftEdges");

            // ~=
            //
            // (?<left>) ~= (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) ~=(?<SUB>) (?<right>)", "DeleteRightVertices");

            // -<
            //
            // (?<left>) -< (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) -<(?<SUB>) (?<right>)", "DeleteRightEdgesFromLeftEdges");

            // ~<
            //
            // (?<left>) ~< (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) ~<(?<SUB>) (?<right>)", "DeleteRightVerticesFromLeftEdges");

            /////////////////////////////////////////////////////////
            //
            // edge set operators
            //
            ////////////////////////////////////////////////////////

            // <+>
            //
            // (?<left>) <+> (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) <+>(?<SUB>) (?<right>)", "EdgeSetAdd");

            // <->
            //
            // (?<left>) <-> (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) <->(?<SUB>) (?<right>)", "EdgeSetSubstract");

            // <<X>>
            //
            // <<(?<expr>)>>
            IVertex o_index = k.AddVertex(keyword, "<<(?<expr>)>>");

            o_index.AddEdge(keywordGroup, kgd_SlashMarkIndex);

            o_index.AddEdge(keywordGroup, kgd_SlashMarkIndexInner2);

            o_index.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            o_index.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_index.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            IVertex o_index_any = o_index.AddVertex(any, "");

            o_index_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_index_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "SetIndex"));

             o_index_any.AddVertex(smu.Get(false, @"SingleExpressionOperator\Expression"), "(?<expr>)");           

            IVertex o_index_any_targetExpr = o_index_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_index_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndex);

            // <>
            //
            // <>

            IVertex o_setCount = k.AddVertex(keyword, "<>");

            o_setCount.AddEdge(keywordGroup, kgd_SlashMarkIndex);

            o_setCount.AddEdge(keywordGroup, kgd_SlashMarkIndexInner2);

            o_setCount.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            o_setCount.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_setCount.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            IVertex o_setCount_any = o_setCount.AddVertex(any, "");

            o_setCount_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_setCount_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "SetCount"));

            // this is error. should be out
            //IVertex o_setCount_any_targetExpr = o_index_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            //o_setCount_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndex);

            // 00
            //
            // 00

            IVertex o_emptySet = k.AddVertex(keyword, "00");

            IVertex o_emptySet_any = o_emptySet.AddVertex(any, "");

            o_emptySet_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "EmptySet"));

            

            /////////////////////////////////////////////////////////
            //
            // algebraic operators
            //
            ////////////////////////////////////////////////////////


            // +
            //
            // (?<left>) + (?<right>)

            IVertex o_plus = k.AddVertex(keyword, "(?<left>) +(?<SUB>) (?<right>)");
            //IVertex o_plus = smuk.AddVertex(keyword, "(?<left>) + (?<right>)");
            IVertex o_plus_any = o_plus.AddVertex(any, "");

            o_plus_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "+"));

            o_plus_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_plus_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right>)");

            // -
            //
            // (?<left>) - (?<right>)

            IVertex o_minus = k.AddVertex(keyword, "(?<left>) -(?<SUB>) (?<right>)");

            IVertex o_minus_any = o_minus.AddVertex(any, "");

            o_minus_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "-"));

            o_minus_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_minus_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right>)");

            // *
            //
            // (?<left>) * (?<right>)

            IVertex o_mul = k.AddVertex(keyword, "(?<left>) *(?<SUB>) (?<right>)");

            IVertex o_mul_any = o_mul.AddVertex(any, "");

            o_mul_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "Mul"));            

            o_mul_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_mul_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right>)");

            // /
            //
            // (?<left>) / (?<right>)

            IVertex o_div = k.AddVertex(keyword, "(?<left>) /(?<SUB>) (?<right>)");

            IVertex o_div_any = o_div.AddVertex(any, "");

            o_div_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "/"));

            o_div_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_div_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right>)");

            /////////////////////////////////////////////////////////
            //
            // logic operators
            //
            ////////////////////////////////////////////////////////

            // ==
            //
            // (?<left>) == (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) ==(?<SUB>) (?<right>)", "Equal");

            // ===
            //
            // (?<left>) === (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) ===(?<SUB>) (?<right>)", "ExactEqual");

            // !=
            //
            // (?<left>) != (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) !=(?<SUB>) (?<right>)", "NotEqual");

            // !
            //
            //  !(?<expr>)

            AddSingleExpressionOperator(k, smu, smb, keyword, any, "!(?<expr>)", "Negation");

            // &
            //
            // (?<left>) & (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) &(?<SUB>) (?<right>)", "And");

            // |
            //
            // (?<left>) | (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) %(?<SUB>) (?<right>)", "Or");

            // >
            //
            // (?<left>) > (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) >(?<SUB>) (?<right>)", "MoreThan");

            // <
            //
            // (?<left>) < (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) <(?<SUB>) (?<right>)", "LessThan");

            // >=
            //
            // (?<left>) >= (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) >=(?<SUB>) (?<right>)", "MoreOrEqualThan");

            // <=
            //
            // (?<left>) <= (?<right>)

            AddLeftRightOperator(k, smu, smb, keyword, any, "(?<left>) <=(?<SUB>) (?<right>)", "LessOrEqualThan");
            

            ////////////////////////////////////////////////////////////////
            //
            // vertex creation operators
            //
            ////////////////////////////////////////////////////////////////


            // :: /1
            //
            // (?<left_ColonEmptyNew>)||(?<SUB>)(?<right_ColonEmptyNew>)                         

            IVertex o_doubleColon = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexNewLink>) || (?<right_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy>)");

            //IVertex o_doubleColon = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexNewLink>)||(?<SUB>)(?<right_ColonEmptyInner2SlashMarkIndexNewLink>)");

            o_doubleColon.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleColon_any = o_doubleColon.AddVertex(any, "");            

            o_doubleColon_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "||"));

            o_doubleColon_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left_ColonEmptyInner2SlashMarkIndexNewLink>)");

            IVertex o_doubleColon_any_right = o_doubleColon_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy>)");

            IVertex o_doubleColon_any_targetExpr = o_doubleColon_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_doubleColon_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_InnerCreation);


            // :: /2
            //
            // ||(?<SUB>)(?<right_ColonEmptyNew>)                         

            IVertex o_doubleColon2 = k.AddVertex(keyword, "|| (?<right_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy>)");

            //IVertex o_doubleColon2 = k.AddVertex(keyword, "||(?<SUB>)(?<right_ColonEmptyInner2SlashMarkIndexNewLink>)");

            o_doubleColon2.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleColon2_any = o_doubleColon2.AddVertex(any, "");

            o_doubleColon2_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "||"));

            IVertex o_doubleColon2_any_right = o_doubleColon2_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy>)");

            IVertex o_doubleColon2_any_targetExpr = o_doubleColon2_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_doubleColon2_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_InnerCreation);

            // :: /3
            //
            // (?<left_ColonEmptyNew>)||(?<SUB>)                         

            IVertex o_doubleColon3 = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexNewLink>) ||");

            // Vertex o_doubleColon3 = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexNewLink>)||(?<SUB>)");

            o_doubleColon3.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleColon3_any = o_doubleColon3.AddVertex(any, "");

            o_doubleColon3_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "||"));

            o_doubleColon3_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left_ColonEmptyInner2SlashMarkIndexNewLink>)");

            IVertex o_doubleColon3_any_targetExpr = o_doubleColon3_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_doubleColon3_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_InnerCreation);            

            // : /1
            //
            // (?<left_Empty2>)|(?<SUB>)(?<right_Empty2>)            

            IVertex o_colon = k.AddVertex(keyword, "(?<left_Empty2Inner>)|(?<SUB>)(?<right_Empty2Inner>)");

            o_colon.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_colon.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            o_colon.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            IVertex o_colon_any = o_colon.AddVertex(any, "");

            o_colon_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_colon_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "|"));

            o_colon_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left_Empty2Inner>)");

            IVertex o_colon_any_right = o_colon_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right_Empty2Inner>)");

            IVertex o_colon_any_targetExpr = o_colon_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_colon_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_SlashMarkIndex);

            // : /2
            //
            // |(?<SUB>)(?<right_Empty2>)            

            IVertex o_colon2 = k.AddVertex(keyword, "|(?<SUB>)(?<right_Empty2Inner>)");

            o_colon2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_colon2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            o_colon2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            IVertex o_colon2_any = o_colon2.AddVertex(any, "");

            o_colon2_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_colon2_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "|"));

            IVertex o_colon2_any_right = o_colon2_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right_Empty2Inner>)");

            IVertex o_colon2_any_targetExpr = o_colon2_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_colon2_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_SlashMarkIndex);

            // : /3
            //
            // (?<left_Empty2>)|(?<SUB>)            

            IVertex o_colon3 = k.AddVertex(keyword, "(?<left_Empty2Inner>)|(?<SUB>)");            

            o_colon3.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_colon3.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            o_colon3.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            IVertex o_colon3_any = o_colon3.AddVertex(any, "");

            o_colon3_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_colon3_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "|"));

            o_colon3_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left_Empty2Inner>)");            

            IVertex o_colon3_any_targetExpr = o_colon3_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_colon3_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_SlashMarkIndex);


            // _
            //
            //  _(?<expr>)

            AddSingleExpressionOperator(k, smu, smb, keyword, any, "_(?<expr>)", "CopySet");

            k.Get(false, "_(?<expr>)").AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            // `
            //
            //  `(?<expr>)

            AddSingleExpressionOperator(k, smu, smb, keyword, any, "`(?<expr>)", "MetaToTo");

            //////////////////// common

            // []
            //
            // [(*(+, +) (?<expr>)*)]

            IVertex o_call = k.AddVertex(keyword, "(?<target_ColonEmptyInner2SlashMarkIndexNewLink>)[(*(+, +)(?<expr>)*)]");

              IVertex o_call_any = o_call.AddVertex(any, "");

              o_call_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "[]"));

              IVertex o_call_any_target = o_call_any.AddVertex(smu.Get(false, @"[]\Target"), "(?<target_ColonEmptyInner2SlashMarkIndexNewLink>)");

              IVertex o_call_any_param = o_call_any.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "(?<expr>)");
           
              o_call_any_param.AddEdge(smb.Get(false, @"$$KeywordManyRoot"), smb.Get(false, @"$Empty"));

            // return
            //
            // return (?<expr>)

            AddSingleExpressionOperator(k, smu, smb, keyword, any, "return (?<expr>)", "Return");

            // return
            //
            // return (?<expr>)

            AddKeyword(k, smu, smb, keyword, any, "return", "Return");

            // foreach
            //
            // foreach (?<var>) in (?<set>)

            IVertex o_foreach = k.AddVertex(keyword, "foreach (?<var>) in (?<set>)");

            IVertex o_foreach_any = o_foreach.AddVertex(any, "");

            o_foreach_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "ForEach"));

            o_foreach_any.AddVertex(smu.Get(false, @"ForEach\Variable"), "(?<var>)");

            o_foreach_any.AddVertex(smu.Get(false, @"ForEach\Set"), "(?<set>)");

            // while
            //
            // while (?<test>)

            IVertex o_while = k.AddVertex(keyword, "while (?<test>)");

            IVertex o_while_any = o_while.AddVertex(any, "");

            o_while_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "While"));

            o_while_any.AddVertex(smu.Get(false, @"While\Test"), "(?<test>)");

            // ()
            //
            // ((?<expr>))

            AddSingleExpressionOperator(k, smu, smb, keyword, any, "((?<expr>))", "()");

            k.Get(false, "((?<expr>))").AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            // \
            //
            // \                         

            IVertex o_Slash = k.AddVertex(keyword, @"\");

            o_Slash.AddEdge(keywordGroup, kgd_SlashMarkIndex);

            o_Slash.AddEdge(keywordGroup, kgd_SlashMarkIndexInner2);

            o_Slash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            o_Slash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_Slash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            IVertex o_Slash_any = o_Slash.AddVertex(any, "");

            o_Slash_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_Slash_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "\"\\ \""));

            IVertex o_Slash_any_targetExpr = o_Slash_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_Slash_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndex);

            // ?
            //
            // ?                         

            IVertex o_Mark = k.AddVertex(keyword, @" ? ");

            o_Mark.AddEdge(keywordGroup, kgd_SlashMarkIndex);

            o_Mark.AddEdge(keywordGroup, kgd_SlashMarkIndexInner2);

            o_Mark.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            o_Mark.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_Mark.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            IVertex o_Mark_any = o_Mark.AddVertex(any, "");

            o_Mark_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_Mark_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "?"));

            IVertex o_Mark_any_targetExpr = o_Mark_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_Mark_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndex);

            // {
            // }
            //
            // {(*\r\n\t(?<expr>)*)\r\n}

            IVertex o_InnerCreation = k.AddVertex(keyword, "{(*\r\n\t(?<expr>)*)\r\n}");

            o_InnerCreation.AddEdge(keywordGroup, kgd_InnerCreation);

            IVertex o_InnerCreation_any = o_InnerCreation.AddVertex(any, anyString);

            o_InnerCreation_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_InnerCreation_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "InnerCreation"));

            IVertex o_InnerCreation_any_param = o_InnerCreation_any.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "(?<expr>)");

            o_InnerCreation_any_param.AddEdge(smb.Get(false, @"$$KeywordManyRoot"), smb.Get(false, @"$Empty"));

            // {} // 2       
            //
            // {(*(+,+)(?<expr>)*)}

            IVertex o_Inner2 = k.AddVertex(keyword, "{(*(+,+)(?<expr>)*)}");

            o_Inner2.AddEdge(keywordGroup, kgd_SlashMarkIndexInner2);

            o_Inner2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            o_Inner2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            o_Inner2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            IVertex o_Inner2_any = o_Inner2.AddVertex(any, anyString);

            o_Inner2_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_Inner2_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "\"{}\""));

            IVertex o_Inner2_any_any_param = o_Inner2_any.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "(?<expr>)");

            o_Inner2_any_any_param.AddEdge(smb.Get(false, @"$$KeywordManyRoot"), smb.Get(false, @"$Empty"));

            IVertex o_Inner2_any_targetExpr = o_Inner2_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            o_Inner2_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_SlashMarkIndex);


            // {} // 1
            //
            // {(*(+,+)(?<expr>)*)}

            IVertex o_Inner = k.AddVertex(keyword, "{(*(+,+)(?<expr1>)*)}");

            o_Inner.AddEdge(keywordGroup, kgd_Inner);
            o_Inner.AddEdge(keywordGroup, kgd_Empty2Inner);

            IVertex o_Inner_any = o_Inner.AddVertex(any, anyString);

            o_Inner_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            o_Inner_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "\"{}\""));

            IVertex o_Inner_any_any_param = o_Inner_any.AddVertex(smu.Get(false, @"MultiOperator\Expression"), "(?<expr1>)");

            o_Inner_any_any_param.AddEdge(smb.Get(false, @"$$KeywordManyRoot"), smb.Get(false, @"$Empty"));


            // ""
            //
            // "\"(?<value>)\""

            IVertex newValueKeyword = k.AddVertex(keyword, "\"(?<value>)\"");

            newValueKeyword.AddVertex(newVertexKeyword, "");

            newValueKeyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            newValueKeyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            IVertex newValueKeyword_any = newValueKeyword.AddVertex(any, "(?<value>)");

           // IVertex newValueKeyword_any_targetExpr = newValueKeyword_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

           // newValueKeyword_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_InnerCreation);


            // E M P T Y :) K E Y W O R D 1
            //
            //

            IVertex empty1Keyword = k.AddVertex(keyword, "(?<value>)");

            empty1Keyword.AddVertex(emptyKeyword, "");

            empty1Keyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            empty1Keyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            empty1Keyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndex);

            IVertex empty1Keyword_any = empty1Keyword.AddVertex(any, "(?<value>)");

            empty1Keyword_any.AddVertex(smb.Get(false, "$$StartInLocalRoot"), "");

            empty1Keyword_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "Query"));

            IVertex empty1Keyword_any_targetExpr = empty1Keyword_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            empty1Keyword_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_SlashMarkIndexInner2);

            // E M P T Y :) K E Y W O R D 2
            //
            //

            IVertex empty2Keyword = k.AddVertex(keyword, "(?<value>)");

            empty2Keyword.AddVertex(emptyKeyword, "");

            empty2Keyword.AddEdge(keywordGroup, kgd_Empty2Inner);

            IVertex empty2Keyword_any = empty2Keyword.AddVertex(any, "(?<value>)");

            empty2Keyword_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, "Query"));

            IVertex empty2Keyword_any_targetExpr = empty2Keyword_any.AddVertex(smu.Get(false, @"SingleOperator\NextExpression"), "");

            empty2Keyword_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_Inner);

            // @
            //
            // @(?<value>)

            IVertex at = k.AddVertex(keyword, "@(?<value>)");

            at.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLink);

            at.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexNewLinkBracketCopy);

            at.AddVertex(linkKeyword, "");

            IVertex at_at = at.AddVertex(any, "");

            at_at.AddEdge(_is, smu.Get(false, @"Link"));

            at_at.AddVertex(smu.Get(false, @"Link\Target"), "(?<value>)");

            //////////////////// meta

            // execute
            //
            // execute((?<expr>))

            AddSingleExpressionOperator(k, smu, smb, keyword, any, "execute((?<expr>))", "Execute");

            // parse
            //
            // parse((?<expr>))

            IVertex o_parse = k.AddVertex(keyword, "parse((?<expr>))");

            IVertex o_parse_any = o_parse.AddVertex(any, "");

            o_parse_any.AddEdge(_is, smu.Get(false, @"Parse"));

            o_parse_any.AddVertex(smu.Get(false, @"SingleExpressionOperator\Expression"), "(?<expr>)");

            // parse
            //
            // parse (?<language>)((?<expr>))

            IVertex o_parse2 = k.AddVertex(keyword, "parse (?<language>)((?<expr>))");

            IVertex o_parse2_any = o_parse2.AddVertex(any, "");

            o_parse2_any.AddEdge(_is, smu.Get(false, @"ParseWithLanguage"));

            o_parse2_any.AddVertex(smu.Get(false, @"ParseWithLanguage\FormalTextLanguage"), "(?<language>)");

            o_parse2_any.AddVertex(smu.Get(false, @"SingleExpressionOperator\Expression"), "(?<expr>)");            

            // generate
            //
            // generate((?<expr>))

            IVertex o_generate = k.AddVertex(keyword, "generate((?<expr>))");

            IVertex o_generate_any = o_generate.AddVertex(any, "");

            o_generate_any.AddEdge(_is, smu.Get(false, @"Generate"));

            o_generate_any.AddVertex(smu.Get(false, @"SingleExpressionOperator\Expression"), "(?<expr>)");

            // generate
            //
            // generate (?<language>)((?<expr>))

            IVertex o_generate2 = k.AddVertex(keyword, "generate (?<language>)((?<expr>))");

            IVertex o_generate2_any = o_generate2.AddVertex(any, "");

            o_generate2_any.AddEdge(_is, smu.Get(false, @"GenerateWithLanguage"));

            o_generate2_any.AddVertex(smu.Get(false, @"SingleExpressionOperator\Expression"), "(?<expr>)");

            o_generate2_any.AddVertex(smu.Get(false, @"GenerateWithLanguage\FormalTextLanguage"), "(?<language>)");

        }

        private static void AddLeftRightOperator(IVertex k, IVertex smu, IVertex smb, IVertex keyword, IVertex any, string text, string _is)
        {
            IVertex o_copy = k.AddVertex(keyword, text);

            IVertex o_copy_any = o_copy.AddVertex(any, "");

            o_copy_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, _is));

            o_copy_any.AddVertex(smu.Get(false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_copy_any.AddVertex(smu.Get(false, @"DoubleOperator\RightExpression"), "(?<right>)");
        }

        private static void AddSingleExpressionOperator(IVertex k, IVertex smu, IVertex smb, IVertex keyword, IVertex any, string text, string _is)
        {
            IVertex o_copy = k.AddVertex(keyword, text);

            IVertex o_copy_any = o_copy.AddVertex(any, "");

            o_copy_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, _is));

            o_copy_any.AddVertex(smu.Get(false, @"SingleExpressionOperator\Expression"), "(?<expr>)");
        }

        private static void AddKeyword(IVertex k, IVertex smu, IVertex smb, IVertex keyword, IVertex any,  string text, string _is)
        {
            IVertex o_copy = k.AddVertex(keyword, text);

            IVertex o_copy_any = o_copy.AddVertex(any, "");

            o_copy_any.AddEdge(smb.Get(false, @"Vertex\$Is"), smu.Get(false, _is));
        }

        void CreateSystemFormalTextLanguageZeroCode()
        {
            IVertex zc = Root.Get(false, @"System\FormalTextLanguage").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\FormalTextLanguage"),"ZeroCode");

            zc.AddEdge(Root.Get(false, @"System\Meta\Base\Vertex\$Is"), Root.Get(false, @"System\Meta\ZeroTypes\FormalTextLanguage"));

            IVertex b = Root.Get(false, @"System\Meta\Base");

            IVertex di = zc.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\FormalTextLanguage\DefaultImports"), "");            

            //

            IVertex DirectMeta = VertexOperations.AddInstance(b, Root.Get(false, @"System\Meta\Base\$ImportMeta"));

            DirectMeta.Value = "$DirectMeta";

            DirectMeta.AddEdge(Root.Get(false, @"System\Meta\Base\Vertex\$IsLink"), Empty);

            di.AddEdge(DirectMeta, Root.Get(false, @"System\Meta\ZeroUML"));

            di.AddEdge(DirectMeta, Root.Get(false, @"System\Meta\Base"));

            di.AddEdge(DirectMeta, Root.Get(false, @"System\Meta\Base\Vertex"));

            di.AddEdge(DirectMeta, Root.Get(false, @"System\Meta\ZeroTypes"));

            di.AddEdge(DirectMeta, Root); // ROOT

            //

            IVertex Direct = VertexOperations.AddInstance(b, Root.Get(false, @"System\Meta\Base\$Import"));

            Direct.Value = "$Direct";

            Direct.AddEdge(Root.Get(false, @"System\Meta\Base\Vertex\$IsLink"), Empty);

            //di.AddEdge(Direct, Root); // :O) now its hanging XXX

            //

            IVertex System = VertexOperations.AddInstance(di, Root.Get(false, @"System\Meta\Base\$ImportMeta"));

            System.Value = "System";

            System.AddEdge(Root.Get(false, @"System\Meta\Base\Vertex\$IsLink"), Empty);

            di.AddEdge(System, Root.Get(false, @"System"));
        }

        void CreateSystemMetaZeroTypes()
        {
            IVertex sm = Root.Get(false, @"System\Meta");


            GeneralUtil.ParseAndExcute(sm, sm, "{ZeroTypes{AtomType:String,AtomType:Integer,AtomType:Decimal,AtomType:Float,AtomType:Boolean,Vertex:VertexType,Class:Edge{Association:From{$MinCardinality:0,$MaxCardinality:1},Association:Meta{$MinCardinality:1,$MaxCardinality:1},Association:To{$MinCardinality:1,$MaxCardinality:1}},Class:DateTime{Attribute:Year{$MinCardinality:1,$MaxCardinality:1},Attribute:Month{$MinCardinality:1,$MaxCardinality:1},Attribute:Day{$MinCardinality:1,$MaxCardinality:1},Attribute:Hour{$MinCardinality:1,$MaxCardinality:1},Attribute:Minute{$MinCardinality:1,$MaxCardinality:1},Attribute:Second{$MinCardinality:1,$MaxCardinality:1},Attribute:Millisecond{$MinCardinality:0,$MaxCardinality:1}},Class:FormalTextLanguage{Aggregation:DefaultImports{$MinCardinality:0,$MaxCardinality:1},Aggregation:Keywords{$MinCardinality:0,$MaxCardinality:1}},Enum:EnumBase,Class:$PlatformClass{$PlatformClassName},Class:HasBaseEdge{Attribute:BaseEdge{$MinCardinality:1,$MaxCardinality:1}},Class:HasSelectedEdges{Attribute:SelectedEdges{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:}},Class:HasFilter{Attribute:FilterQuery{$MinCardinality:0,$MaxCardinality:1}},Class:Color{Attribute:Red{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Green{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Blue{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Opacity{MinValue:0,MaxValue:255,$MinCardinality:0,$MaxCardinality:1}},Class:Exception{Attribute:Where{$MinCardinality:0,$MaxCardinality:1},Attribute:Type{$MinCardinality:0,$MaxCardinality:1},Attribute:What{$MinCardinality:1,$MaxCardinality:1}},Enum:ExceptionTypeEnum{EnumValue:Error,EnumValue:Warning,EnumValue:Info},Class:CallableEndPoint,Class:CodeContainer,Class:DotNetEndPoint{Attribute:TypeName,Attribute:MethodName}}}");

            sm.Get(false, @"Base\Vertex\$ExecutableEndPoint").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\CallableEndPoint"));

            sm.Get(false, @"ZeroTypes\String").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\AtomType"));
            sm.Get(false, @"ZeroTypes\Integer").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\AtomType"));
            sm.Get(false, @"ZeroTypes\Decimal").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\AtomType"));
            sm.Get(false, @"ZeroTypes\Float").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\AtomType"));
            sm.Get(false, @"ZeroTypes\Boolean").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\AtomType"));
            sm.Get(false, @"ZeroTypes\VertexType").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"Base\Vertex"));
            sm.Get(false, @"ZeroTypes\Edge").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\EnumBase").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Enum"));
            sm.Get(false, @"ZeroTypes\DateTime").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\HasBaseEdge").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\HasSelectedEdges").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\HasFilter").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\$PlatformClass").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Color").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));

            sm.Get(false, @"ZeroTypes\EnumBase").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            sm.Get(false, @"ZeroTypes\DateTime\Year").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\DateTime\Month").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\DateTime\Day").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\DateTime\Hour").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\DateTime\Minute").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\DateTime\Second").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\DateTime\Millisecond").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));

            sm.Get(false, @"ZeroTypes\Edge\From").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"ZeroTypes\Edge\Meta").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"ZeroTypes\Edge\To").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));

            sm.Get(false, @"ZeroTypes\HasBaseEdge\BaseEdge").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Edge"));
            sm.Get(false, @"ZeroTypes\HasBaseEdge\BaseEdge").AddVertex(sm.Get(false, @"*$Section"), "Base");

            sm.Get(false, @"ZeroTypes\HasSelectedEdges\SelectedEdges").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"ZeroTypes\HasFilter\FilterQuery").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            sm.Get(false, @"ZeroTypes\Color\Red").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\Color\Green").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\Color\Blue").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"ZeroTypes\Color\Opacity").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));

            sm.Get(false, @"ZeroTypes\ExceptionTypeEnum").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            sm.Get(false, @"ZeroTypes\Exception\Where").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            sm.Get(false, @"ZeroTypes\Exception\Type").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\ExceptionTypeEnum"));
            sm.Get(false, @"ZeroTypes\Exception\What").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            sm.Get(false, @"ZeroTypes\CodeContainer").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\CallableEndPoint"));
            sm.Get(false, @"ZeroTypes\DotNetEndPoint").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\CallableEndPoint"));

            sm.Get(false, @"ZeroTypes\DotNetEndPoint\TypeName").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            sm.Get(false, @"ZeroTypes\DotNetEndPoint\MethodName").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

        }

        void CreateSystemMetaVisualiserDiagram()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Visualiser}");

            IVertex smv = Root.Get(false, @"System\Meta\Visualiser");

            GeneralUtil.ParseAndExcute(smv, sm, "{DiagramInternal{Class:DiagramItemBase{Association:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionX{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionY{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:DiagramLine{$MinCardinality:0,$MaxCardinality:-1},OptionEdge,OptionDiagramLineDefinition},Class:DiagramItemDefinition{Attribute:DirectVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:MetaVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Association:DiagramItemClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramItemVertex{$MinCardinality:0,$MaxCardinality:1},Association:InstanceCreation{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineDefinition{$MinCardinality:0,$MaxCardinality:-1},Attribute:DoNotShowInherited{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1}},Enum:InstanceCreationEnum{EnumValue:Instance,EnumValue:InstanceAndDirect,EnumValue:Direct},Class:DiagramLineBase{Association:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Association:ToDiagramItem{$MinCardinality:1,$MaxCardinality:1}},Class:DiagramLineDefinition{Attribute:EdgeTestQuery{$MinCardinality:1,$MaxCardinality:1},Attribute:ToDiagramItemTestQuery{$MinCardinality:0,$MaxCardinality:1},Association:DiagramLineClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineVertex{$MinCardinality:0,$MaxCardinality:1},Attribute:CreateEdgeOnly{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1}},Class:DiagramImageItem{Attribute:Filename},Class:DiagramOvalItem,Class:DiagramRhombusItem,Class:DiagramRectangleItem{Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:RoundEdgeSize{MinValue:0,MaxValue:200,$MinCardinality:0,$MaxCardinality:1},Association:VisualiserClass{$MinCardinality:0,$MaxCardinality:1},Attribute:VisualiserVertex{$MinCardinality:0,$MaxCardinality:1}},Class:DiagramLine{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}},Enum:LineEndEnum{EnumValue:Straight,EnumValue:Arrow,EnumValue:Triangle,EnumValue:FilledTriangle,EnumValue:Diamond,EnumValue:FilledDiamond},Class:DiagramMetaExtendedLine{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}}}}");

            smv.Get(false, @"DiagramInternal\InstanceCreationEnum").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            smv.Get(false, @"DiagramInternal\LineEndEnum").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            smv.Get(false, @"DiagramInternal\DiagramItemBase").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));

            smv.Get(false, @"DiagramInternal\DiagramItemBase\Definition").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramItemDefinition"));
            IVertex definitionSection = smv.Get(false, @"DiagramInternal\DiagramItemBase\Definition").AddVertex(sm.Get(false, @"*$Section"), "Definition");

            smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionX").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            IVertex positionAndSizeSection = smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionX").AddVertex(sm.Get(false, @"*$Section"), "Position and size");

            smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(false, @"*$Section"), positionAndSizeSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(false, @"*$Section"), positionAndSizeSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(false, @"*$Section"), positionAndSizeSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\LineWidth").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            IVertex lookSection = smv.Get(false, @"DiagramInternal\DiagramItemBase\LineWidth").AddVertex(sm.Get(false, @"*$Section"), "Look");

            smv.Get(false, @"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Color"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Color"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(false, @"*$Section"), sm.Get(false, @"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));



            smv.Get(false, @"DiagramInternal\DiagramItemDefinition").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DirectVertexTestQuery").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\MetaVertexTestQuery").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DiagramItemClass").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DiagramItemVertex").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\InstanceCreation").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\InstanceCreationEnum"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DiagramLineDefinition").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineDefinition"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DoNotShowInherited").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\ForceShowEditForm").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            smv.Get(false, @"DiagramInternal\DiagramLineBase").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));

            smv.Get(false, @"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineDefinition"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(false, @"*$Section"), definitionSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Color"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Color"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(false, @"*$Section"), sm.Get(false, @"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));

            smv.Get(false, @"DiagramInternal\DiagramLineDefinition").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\EdgeTestQuery").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\ToDiagramItemTestQuery").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\DiagramLineClass").AddEdge(sm.Get(false, @"*$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\DiagramLineVertex").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\CreateEdgeOnly").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\ForceShowEditForm").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            smv.Get(false, @"DiagramInternal\DiagramImageItem").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramImageItem").AddEdge(sm.Get(false, "*$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramImageItem").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramImageItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smv.Get(false, @"DiagramInternal\DiagramImageItem\Filename").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            smv.Get(false, @"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get(false, "*$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramOvalItem").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramOvalItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get(false, "*$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramRhombusItem").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRhombusItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");



            smv.Get(false, @"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get(false, "*$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRectangleItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroUML\Class"));
            IVertex visualiserSection = smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddVertex(sm.Get(false, @"*$Section"), "Visualiser");

            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(false, @"*$Section"), visualiserSection);


            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLine").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramLine").AddEdge(sm.Get(false, "*$Inherits"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramLine").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(false, @"*$Section"), lookSection);


            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get(false, "*$Inherits"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramMetaExtendedLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(false, @"*$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(false, @"*$Section"), lookSection);
        }

        void CreateSystemMetaVisualiser()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex smv = Root.Get(false, @"System\Meta\Visualiser");

            GeneralUtil.ParseAndExcute(smv, sm, "{Enum:GridStyleEnum{EnumValue:None,EnumValue:Vertical,EnumValue:Horizontal,EnumValue:All,EnumValue:AllAndRound,EnumValue:Round},Class:Form{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:ExpertMode{$MinCardinality:0,$MaxCardinality:1},Attribute:ColumnNumber{$MinCardinality:0,$MaxCardinality:1,$UpdateAfterInteractionEnds:},Attribute:MetaOnLeft{$MinCardinality:0,$MaxCardinality:1},Attribute:SectionsAsTabs{$MinCardinality:0,$MaxCardinality:1},Attribute:TableVisualiserVertex{$MinCardinality:0,$MaxCardinality:1}},Class:Code{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowWhiteSpace{$MinCardinality:0,$MaxCardinality:1},Attribute:ShowLineNumbers{$MinCardinality:0,$MaxCardinality:1},Attribute:HighlightedLine{$MinCardinality:0,$MaxCardinality:1}},Class:Table{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:ExpertMode{$MinCardinality:0,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}},Class:TableFast{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}},Class:Tree{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1}},Class:Graph{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:VisualiserCircleSize{$MinCardinality:1,$MaxCardinality:1},Attribute:NumberOfCircles{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd:},Attribute:ShowOutEdges{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowInEdges{$MinCardinality:1,$MaxCardinality:1},Attribute:FastMode{$MinCardinality:1,$MaxCardinality:1},Attribute:MetaLabels{$MinCardinality:1,$MaxCardinality:1}},Class:Class,Class:String,Class:StringView,Class:Vertex,Class:Edge,Class:Integer,Class:Decimal,Class:Float,Class:Boolean,Class:Enum,Class:Diagram{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1,MinValue:0,MaxValue:200,$DefaultValue:100},Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd:},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd},Attribute:Item{$MinCardinality:0,$MaxCardinality:-1,$Hide:},Association:CreationPool{$MinCardinality:1,$MaxCardinality:1}},Class:Wrap,Class:List{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:IsMetaRightAlign{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowMeta{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1}}}");

            sm.Get(false, @"Visualiser\GridStyleEnum").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            sm.Get(false, @"Visualiser\Form").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Form").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Form").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.FormVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Form").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Form\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Form\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MinValue"), 0);
            sm.Get(false, @"Visualiser\Form\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MaxValue"), 200);
            sm.Get(false, @"Visualiser\Form\ExpertMode").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Form\ColumnNumber").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Form\ColumnNumber").AddVertex(sm.Get(false, @"*MinValue"), 1);
            sm.Get(false, @"Visualiser\Form\ColumnNumber").AddVertex(sm.Get(false, @"*MaxValue"), 8);
            sm.Get(false, @"Visualiser\Form\MetaOnLeft").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Form\SectionsAsTabs").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Form\TableVisualiserVertex").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            //sm.Get(false, @"ZeroUML\Class").AddEdge(sm.Get(false, "ZeroUML*$DefaultOpenVisualiser"), sm.Get(false, @"Visualiser\Form"));

            sm.Get(false, @"Visualiser\Code").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Code").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Code").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.CodeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Code").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Code\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            sm.Get(false, @"Visualiser\Code\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MinValue"), 1.0);
            sm.Get(false, @"Visualiser\Code\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MaxValue"), 30.0);
            sm.Get(false, @"Visualiser\Code\ShowWhiteSpace").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Code\ShowLineNumbers").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Code\HighlightedLine").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            sm.Get(false, @"Visualiser\Wrap").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Wrap").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Wrap").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.WrapVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Wrap").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));

            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasFilter"));
            sm.Get(false, @"Visualiser\List").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.ListVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\List\ShowMeta").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\List\ShowHeader").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\List\GridStyle").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Visualiser\GridStyleEnum"));
            sm.Get(false, @"Visualiser\List\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\List\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MinValue"), 0);
            sm.Get(false, @"Visualiser\List\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MaxValue"), 200);
            sm.Get(false, @"Visualiser\List\IsMetaRightAlign").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\List\IsAllVisualisersEdit").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasFilter"));
            sm.Get(false, @"Visualiser\Table").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.TableVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Table\ExpertMode").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Table\ToShowEdgesMeta").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Edge"));
            sm.Get(false, @"Visualiser\Table\ShowHeader").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Table\GridStyle").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Visualiser\GridStyleEnum"));
            sm.Get(false, @"Visualiser\Table\AlternatingRows").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Table\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Table\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MinValue"), 0);
            sm.Get(false, @"Visualiser\Table\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MaxValue"), 200);
            sm.Get(false, @"Visualiser\Table\IsAllVisualisersEdit").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasFilter"));
            sm.Get(false, @"Visualiser\TableFast").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.TableFastVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\TableFast\ToShowEdgesMeta").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Edge"));
            sm.Get(false, @"Visualiser\TableFast\ShowHeader").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\TableFast\GridStyle").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Visualiser\GridStyleEnum"));
            sm.Get(false, @"Visualiser\TableFast\AlternatingRows").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\TableFast\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\TableFast\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MinValue"), 0);
            sm.Get(false, @"Visualiser\TableFast\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MaxValue"), 200);
            sm.Get(false, @"Visualiser\TableFast\IsAllVisualisersEdit").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));


            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Tree").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.TreeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Tree\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Tree\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MinValue"), 0);
            sm.Get(false, @"Visualiser\Tree\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MaxValue"), 200);


            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Graph").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.GraphVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Graph\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Graph\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MinValue"), 0);
            sm.Get(false, @"Visualiser\Graph\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*MaxValue"), 200);
            sm.Get(false, @"Visualiser\Graph\VisualiserCircleSize").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Graph\VisualiserCircleSize").AddVertex(sm.Get(false, @"*MinValue"), 50);
            sm.Get(false, @"Visualiser\Graph\VisualiserCircleSize").AddVertex(sm.Get(false, @"*MaxValue"), 500);
            sm.Get(false, @"Visualiser\Graph\NumberOfCircles").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Graph\NumberOfCircles").AddVertex(sm.Get(false, @"*MinValue"), 1);
            sm.Get(false, @"Visualiser\Graph\NumberOfCircles").AddVertex(sm.Get(false, @"*MaxValue"), 10);
            sm.Get(false, @"Visualiser\Graph\ShowOutEdges").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Graph\ShowInEdges").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Graph\FastMode").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Graph\MetaLabels").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            sm.Get(false, @"Visualiser\Class").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Class").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Class").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.ClassVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Class").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));

            sm.Get(false, @"Visualiser\String").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\String").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\String").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.StringVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\String").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\String").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\String"));

            sm.Get(false, @"Visualiser\StringView").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\StringView").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\StringView").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.StringViewVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\StringView").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\String").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\StringView"));



            sm.Get(false, @"Visualiser\Vertex").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Vertex").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Vertex").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.VertexVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Vertex").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));

            sm.Get(false, @"ZeroTypes\VertexType").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"ZeroTypes\VertexType").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$Inherits").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$Inherits").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$VertexTarget").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$VertexTarget").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            //sm.Get(false, @"ZeroUML\Class").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\VertexVisualiser"));
            sm.Get(false, @"ZeroUML\Class").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));

            sm.Get(false, @"Visualiser\Edge").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Edge").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Edge").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.EdgeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Edge").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Edge").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Edge"));
            sm.Get(false, @"ZeroTypes\Edge").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Edge"));

            sm.Get(false, @"Visualiser\Integer").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Integer").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Integer").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.IntegerVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Integer").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Integer").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Integer"));

            sm.Get(false, @"Visualiser\Decimal").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Decimal").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Decimal").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.DecimalVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Decimal").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Decimal").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Decimal"));

            sm.Get(false, @"Visualiser\Float").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Float").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Float").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.FloatVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Float").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Float").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Float"));

            sm.Get(false, @"Visualiser\Boolean").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Boolean").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Boolean").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.BooleanVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Boolean").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Boolean").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Boolean"));
            sm.Get(false, @"ZeroTypes\Boolean").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Boolean"));

            sm.Get(false, @"Visualiser\Enum").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Enum").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Enum").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.EnumVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Enum").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\EnumBase").AddEdge(sm.Get(false, "ZeroUML*$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Enum"));

            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\$PlatformClass"));
            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, "*$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Diagram").AddVertex(sm.Get(false, "*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.Diagram, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Diagram\ZoomVisualiserContent").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Diagram\Item").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*DiagramItemBase"));
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*Float"));
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddVertex(sm.Get(false, @"*$DefaultValue"), (double)1000.0);
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddVertex(sm.Get(false, @"*MinValue"), (double)0.0);
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddVertex(sm.Get(false, @"*MaxValue"), (double)4000.0);
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*Float"));
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddVertex(sm.Get(false, @"*$DefaultValue"), (double)1000.0);
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddVertex(sm.Get(false, @"*MinValue"), (double)0.0);
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddVertex(sm.Get(false, @"*MaxValue"), (double)4000.0);
            sm.Get(false, @"Visualiser\Diagram\CreationPool").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, "ZeroUML*$DefaultOpenVisualiser"), sm.Get(false, @"Visualiser\Diagram"));

            IVertex diagramGeneralGroup = sm.Get(false, @"Visualiser\Diagram").AddVertex(sm.Get(false, @"*$Group"), "General");
            sm.Get(false, @"Visualiser\Diagram\CreationPool").AddEdge(sm.Get(false, @"*$Group"), diagramGeneralGroup);

            IVertex diagramDetailsGroup = sm.Get(false, @"Visualiser\Diagram\ZoomVisualiserContent").AddVertex(sm.Get(false, @"*$Group"), "Details");
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddEdge(sm.Get(false, @"*$Group"), diagramDetailsGroup);
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddEdge(sm.Get(false, @"*$Group"), diagramDetailsGroup);
            sm.Get(false, @"Visualiser\Diagram\SelectedEdges").AddEdge(sm.Get(false, @"*$Group"), diagramDetailsGroup);
        }

        void CreateSystemData()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex s = Root.Get(false, @"System");

            GeneralUtil.ParseAndExcute(s, sm, "{Data}");
        }

        IVertex AddDiagramItemDefinition(String Value, String DirectVertexTestQuery, String MetaVertexTestQuery, IVertex DiagramItemClass, IVertex InstanceCreation)
        {
            IVertex did = Root.Get(false, @"System\Meta*DiagramItemDefinition");

            IVertex v = Root.Get(false, @"System\Data\Visualiser\Diagram").AddVertex(did, Value);

            v.AddEdge(Root.Get(false, @"System\Meta*$Is"), Root.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemDefinition"));

            if (DirectVertexTestQuery != null)
                v.AddVertex(did.Get(false, "DirectVertexTestQuery"), DirectVertexTestQuery);

            if (MetaVertexTestQuery != null)
                v.AddVertex(did.Get(false, "MetaVertexTestQuery"), MetaVertexTestQuery);

            v.AddEdge(did.Get(false, "DiagramItemClass"), DiagramItemClass);

            v.AddEdge(did.Get(false, "InstanceCreation"), InstanceCreation);

            return v;
        }


        IVertex AddDiagramItemDefinition_Combo_RectangleItem(String Value, bool doNotShowInherited,
              String DirectVertexTestQuery,
              String MetaVertexTestQuery,
              IVertex DiagramItemClass,
              IVertex InstanceCreation,
              bool CreateDiagraItemVertex,
              double SizeX, double SizeY,
              double LineWidth,
              int BackgroundRed, int BackgroundGreen, int BackgroundBlue, int BackgroundOpacity,
              int ForegroundRed, int ForegroundGreen, int ForegroundBlue, int ForegroundOpacity,
              int RoundEdgeSize, bool showMeta,
              IVertex VisualiserClass, bool VisualiserVertex,
              bool? ForceShowEditForm = null)
        {
            IVertex v = AddDiagramItemDefinition_Combo(Value, doNotShowInherited, DirectVertexTestQuery, MetaVertexTestQuery, DiagramItemClass, InstanceCreation,
                CreateDiagraItemVertex,
              SizeX, SizeY,
              LineWidth,
              BackgroundRed, BackgroundGreen, BackgroundBlue, BackgroundOpacity,
              ForegroundRed, ForegroundGreen, ForegroundBlue, ForegroundOpacity,
              ForceShowEditForm);

            if (CreateDiagraItemVertex && RoundEdgeSize > -1)
                v.Get(false, "DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta*RoundEdgeSize"), RoundEdgeSize);

            if (CreateDiagraItemVertex && showMeta)
                v.Get(false, "DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem\ShowMeta"), "True");
            else
                v.Get(false, "DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem\ShowMeta"), "False");

            if (VisualiserClass != null)
                v.Get(false, @"DiagramItemVertex:").AddEdge(Root.Get(false, @"System\Meta*VisualiserClass"), VisualiserClass);

            if (VisualiserVertex)
                v.Get(false, @"DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta*VisualiserVertex"), null);

            return v;
        }

        IVertex AddDiagramItemDefinition_Combo(String Value, bool doNotShowInherited,
            String DirectVertexTestQuery,
            String MetaVertexTestQuery,
            IVertex DiagramItemClass,
            IVertex InstanceCreation,
            bool CreateDiagraItemVertex,
            double SizeX, double SizeY,
            double LineWidth,
            int BackgroundRed, int BackgroundGreen, int BackgroundBlue, int BackgroundOpacity,
            int ForegroundRed, int ForegroundGreen, int ForegroundBlue, int ForegroundOpacity,
            bool? ForceShowEditForm = null)
        {
            IVertex did = Root.Get(false, @"System\Meta*DiagramItemDefinition");

            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex v = AddDiagramItemDefinition(Value, DirectVertexTestQuery, MetaVertexTestQuery, DiagramItemClass, InstanceCreation);

            if (doNotShowInherited)
                v.AddVertex(sm.Get(false, @"*DoNotShowInherited"), "True");

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    v.AddVertex(sm.Get(false, @"*ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    v.AddVertex(sm.Get(false, @"*ForceShowEditForm"), "False");
            }

            if (CreateDiagraItemVertex)
            {
                IVertex iv = v.AddVertex(did.Get(false, "DiagramItemVertex"), null);

                if (SizeX > -1)
                {
                    iv.AddVertex(Root.Get(false, @"System\Meta*SizeX"), SizeX);
                    iv.AddVertex(Root.Get(false, @"System\Meta*SizeY"), SizeY);
                }

                if (LineWidth > -1)
                    iv.AddVertex(Root.Get(false, @"System\Meta*LineWidth"), LineWidth);

                if (BackgroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(iv, Root.Get(false, @"System\Meta*Color"), Root.Get(false, @"System\Meta*BackgroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Red"), BackgroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Green"), BackgroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Blue"), BackgroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Opacity"), BackgroundOpacity);
                }

                if (ForegroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(iv, Root.Get(false, @"System\Meta*Color"), Root.Get(false, @"System\Meta*ForegroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Red"), ForegroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Green"), ForegroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Blue"), ForegroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Opacity"), ForegroundOpacity);
                }
            }

            return v;
        }

        void AddDiagramLine_Combo(IVertex v,
            String name,
            String EdgeTestQuery,
            String ToDiagramTestQuery,
            IVertex DiagramLineClass,
            bool CreateDiagraLineVertex,
            IVertex startAnchor,
            IVertex endAnchor,
            double LineWidth, bool isDashed,
            int BackgroundRed, int BackgroundGreen, int BackgroundBlue, int BackgroundOpacity,
            int ForegroundRed, int ForegroundGreen, int ForegroundBlue, int ForegroundOpacity,
            bool? CreateEdgeOnly = null,
            bool? ForceShowEditForm = null)
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex dld = Root.Get(false, @"System\Meta*DiagramInternal\DiagramLineDefinition");

            IVertex lv = v.AddVertex(dld, name);

            lv.AddEdge(sm.Get(false, "*$Is"), dld);

            lv.AddVertex(dld.Get(false, "EdgeTestQuery"), EdgeTestQuery);

            lv.AddVertex(dld.Get(false, "ToDiagramItemTestQuery"), ToDiagramTestQuery);

            lv.AddEdge(dld.Get(false, "DiagramLineClass"), DiagramLineClass);

            if (CreateEdgeOnly != null)
            {
                if (CreateEdgeOnly == true)
                    lv.AddVertex(sm.Get(false, @"*CreateEdgeOnly"), "True");

                if (CreateEdgeOnly == false)
                    lv.AddVertex(sm.Get(false, @"*CreateEdgeOnly"), "False");
            }

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    lv.AddVertex(sm.Get(false, @"*ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    lv.AddVertex(sm.Get(false, @"*ForceShowEditForm"), "False");
            }

            if (CreateDiagraLineVertex)
            {
                IVertex dlv = lv.AddVertex(dld.Get(false, "DiagramLineVertex"), null);

                if (isDashed)
                    dlv.AddVertex(sm.Get(false, "*IsDashed"), "True");

                if (startAnchor != null)
                    dlv.AddEdge(sm.Get(false, "*StartAnchor"), startAnchor);

                if (endAnchor != null)
                    dlv.AddEdge(sm.Get(false, "*EndAnchor"), endAnchor);

                if (LineWidth > -1)
                    dlv.AddVertex(Root.Get(false, @"System\Meta*LineWidth"), LineWidth);

                if (BackgroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(dlv, Root.Get(false, @"System\Meta*Color"), Root.Get(false, @"System\Meta*BackgroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Red"), BackgroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Green"), BackgroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Blue"), BackgroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Opacity"), BackgroundOpacity);
                }

                if (ForegroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(dlv, Root.Get(false, @"System\Meta*Color"), Root.Get(false, @"System\Meta*ForegroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Red"), ForegroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Green"), ForegroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Blue"), ForegroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta*Opacity"), ForegroundOpacity);
                }
            }
        }

        void CreateSystemDataVisualiserDiagram()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex sd = Root.Get(false, @"System\Data");

            GeneralUtil.ParseAndExcute(sd, sm, "{Visualiser{Diagram}}");

            IVertex Instance = sm.Get(false, "*Instance");
            IVertex InstanceAndDirect = sm.Get(false, "*InstanceAndDirect");
            IVertex Direct = sm.Get(false, "*Direct");

            IVertex arrow = sm.Get(false, @"*DiagramInternal\LineEndEnum\Arrow");
            IVertex triangle = sm.Get(false, @"*DiagramInternal\LineEndEnum\Triangle");
            IVertex filledTriangle = sm.Get(false, @"*DiagramInternal\LineEndEnum\FilledTriangle");
            IVertex diamond = sm.Get(false, @"*DiagramInternal\LineEndEnum\Diamond");
            IVertex filledDiamond = sm.Get(false, @"*DiagramInternal\LineEndEnum\FilledDiamond");
            IVertex straight = sm.Get(false, @"*DiagramInternal\LineEndEnum\Straight");

            /*    /////////////////////////////////////////////////////////////////////////
                // TEST
                /////////////////////////////////////////////////////////////////////////

                IVertex v =AddDiagramItemDefinition_Combo("test", false,
                    @"", 
                    null, 
                    sm.Get(false, @"*DiagramRhombusItem"), 
                    Direct,
                    true,200,200,10,
                    255,0,0,100,
                    0,255,0,100);

                v.Get(false, "DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta*Filename"), "testimage.gif");

                AddDiagramLine_Combo(v,
                    "Edgee",
                    @"$Is:\",
                    @"",
                    sm.Get(false, @"*DiagramInternal\DiagramLine"),
                    true,
                    triangle,
                    diamond,
                    10,true,
                    255, 0, 0, 100,
                    255, 0, 255, 100);
                    */

            /////////////////////////////////////////////////////////////////////////
            // Object Rectangle
            /////////////////////////////////////////////////////////////////////////

            IVertex v2 = AddDiagramItemDefinition_Combo_RectangleItem("Object", false,
                @"{$Is:{$Is:Class}}",
                @"{$Is:Class}",
                sm.Get(false, @"*DiagramRectangleItem"),
                InstanceAndDirect,
                true, -1, 0, -1,
                -1, 0, 0, 0,
                -1, 0, 0, 0,
                -1, true,
                Root.Get(false, @"System\Meta*List"), true);

            IVertex v2vv = v2.Get(false, @"DiagramItemVertex:\VisualiserVertex:");

            v2vv.AddVertex(Root.Get(false, @"System\Meta*FilterQuery"), "{$Is:Attribute}:");

            v2vv.AddVertex(Root.Get(false, @"System\Meta*ShowHeader"), "False");

            AddDiagramLine_Combo(v2,
                "Association instance",
                @"$Is:{$Is:Class}\Association:",
                @"Definition:Object",
                sm.Get(false, @"*DiagramInternal\DiagramLine"),
                true,
                null,
                arrow,
                -1, true,
                -1, 0, 0, 0,
                -1, 0, 0, 0);

            AddDiagramLine_Combo(v2,
                "Aggregation instance",
                @"$Is:{$Is:Class}\Aggregation:",
                @"Definition:Object",
                sm.Get(false, @"*DiagramInternal\DiagramLine"),
                true,
                diamond,
                null,
                -1, true,
                -1, 0, 0, 0,
                -1, 0, 0, 0);

            /////////////////////////////////////////////////////////////////////////
            // Class
            /////////////////////////////////////////////////////////////////////////

            IVertex v3 = AddDiagramItemDefinition_Combo_RectangleItem("Class", true,
              @"{$Is:Class}",
              "Class",
              sm.Get(false, @"*DiagramRectangleItem"),
              InstanceAndDirect,
              true, -1, 0, -1,
                -1, 0, 0, 0,
                -1, 0, 0, 0,
                -1, true,
                Root.Get(false, @"System\Meta\Visualiser\Class"), true, true);

            IVertex v3vv = v3.Get(false, @"DiagramItemVertex:\VisualiserVertex:");

            v3vv.AddVertex(Root.Get(false, @"System\Meta*FilterQuery"), "Attribute:");

            v3vv.AddVertex(Root.Get(false, @"System\Meta*ShowHeader"), "False");

            AddDiagramLine_Combo(v3,
             "Association",
             @"$Is:Class\Association",
             @"Definition:Class",
             sm.Get(false, @"*DiagramInternal\DiagramLine"),
             false,
             null,
             null,
             -1, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0);

            AddDiagramLine_Combo(v3,
            "Aggregation",
            @"$Is:Class\Aggregation",
            @"Definition:Class",
            sm.Get(false, @"*DiagramInternal\DiagramLine"),
            true,
            diamond,
            null,
            -1, false,
            -1, 0, 0, 0,
            -1, 0, 0, 0);

            AddDiagramLine_Combo(v3,
            "Inheritence",
            @"$Is:Class\$Inherits",
            @"Definition:Class",
            sm.Get(false, @"*DiagramInternal\DiagramLine"),
            true,
            null,
            triangle,
            -1, false,
            -1, 0, 0, 0,
            -1, 0, 0, 0);

            /////////////////////////////////////////////////////////////////////////
            // Method 
            /////////////////////////////////////////////////////////////////////////

            IVertex vMethod = AddDiagramItemDefinition_Combo_RectangleItem("Method", false,
         @"{$Is:Method}",
         "Method",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, 5,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          -1, true,
          null, false);

            AddDiagramLine_Combo(v3,
                    "Method",
                    @"$Is:Class\Method",
                    @"Definition:Method",
                    sm.Get(false, @"*DiagramInternal\DiagramLine"),
                    true,
                    filledDiamond,
                    null,
                    -1, false,
                    -1, 0, 0, 0,
                    -1, 0, 0, 0);

            AddDiagramLine_Combo(vMethod,
             "InputParameter",
             @"$Is:Method\InputParameter",
             @"Definition:InputParameter",
             sm.Get(false, @"*DiagramInternal\DiagramLine"),
             true,
             filledDiamond,
             null,
             2, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0, true, false);

            AddDiagramLine_Combo(vMethod,
             "Output",
            @"$Is:Method\Output",
            @"BaseEdge:\To:\$Is:Type",
            sm.Get(false, @"*DiagramInternal\DiagramLine"),
            true,
            null,
            arrow,
            1, false,
            -1, 0, 0, 0,
            -1, 0, 0, 0, true, false);

            AddDiagramLine_Combo(vMethod,
          "Variable",
          @"$Is:Method\Variable",
          @"Definition:Variable",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0, true, false);

            AddDiagramLine_Combo(vMethod,
          "Type",
          @"$Is:Method\Type",
          @"BaseEdge:\To:\$Is:Type",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0);

            AddDiagramLine_Combo(vMethod,
          "Function",
          @"$Is:Method\Function",
          @"Definition:Function",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0);



            AddDiagramLine_Combo(vMethod,
           "Do",
           @"$Is:Method\Do",
            @"BaseEdge:\To:\$Is:Atom",
            sm.Get(false, @"*DiagramInternal\DiagramLine"),
           true,
            null,
           filledTriangle,
            3, false,
           -1, 0, 0, 0,
           -1, 0, 0, 0);

            // InputParameter

            IVertex vInputParameter = AddDiagramItemDefinition_Combo_RectangleItem("InputParameter", false,
         @"{$Is:InputParameter}",
         "InputParameter",
          sm.Get(false, @"*DiagramOvalItem"),
          InstanceAndDirect,
          true, 20, 20, 2,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          -1, true,
          null, false);

            AddDiagramLine_Combo(vInputParameter,
             "EdgeTarget",
             @"$EdgeTarget",
             @"BaseEdge:\To:\$Is:Type",
             sm.Get(false, @"*DiagramInternal\DiagramLine"),
             true,
             null,
             arrow,
             -1, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0);

            // Variable

            IVertex vVariable = AddDiagramItemDefinition_Combo_RectangleItem("Variable", false,
         @"{$Is:Variable}",
         "Variable",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, 2,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          10, true,
          null, false);

            AddDiagramLine_Combo(vVariable,
          "EdgeTarget",
          @"$EdgeTarget",
          @"BaseEdge:\To:\$Is:Type",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          null,
          arrow,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0);


            /////////////////////////////////////////////////////////////////////////
            // Vertex 
            /////////////////////////////////////////////////////////////////////////

            IVertex v4 = AddDiagramItemDefinition_Combo_RectangleItem("Vertex", false,
             @"",
             null,
             sm.Get(false, @"*DiagramRectangleItem"),
             Direct,
            //   false, -1,0, -1,
            // -1, 0, 0, 0,
            //  -1, 0, 0, 0);

            true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          -1, false,
          null, false);

            AddDiagramLine_Combo(v4,
               "Edge",
               @"$Is:\",
               @"",
               sm.Get(false, @"*DiagramInternal\DiagramMetaExtendedLine"),
               true,
               null,
               arrow,
               -1, false,
               -1, 0, 0, 0,
               -1, 0, 0, 100);
        }

        void AddNextLine(IVertex diagramItem)
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex filledTriangle = sm.Get(false, @"*DiagramInternal\LineEndEnum\FilledTriangle");

            AddDiagramLine_Combo(diagramItem,
             "Next",
             @"$Is:NextOut\Next",
              @"BaseEdge:\To:\$Is:Atom",
              sm.Get(false, @"*DiagramInternal\DiagramLine"),
             true,
              null,
             filledTriangle,
              3, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0);
        }

       /* void AddOutput(IVertex diagramItem) // not used now, but might :/ be inspiring in future
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex arrow = sm.Get(false, @"*DiagramInternal\LineEndEnum\Arrow");

            AddDiagramLine_Combo(diagramItem,
             "Output",
             @"$Is:Expression\Output",
              @"BaseEdge:\To:\$Is:Type",
              sm.Get(false, @"*DiagramInternal\DiagramLine"),
             true,
              null,
             arrow,
              -1, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0);
        }*/

        void CreateSystemDataVisualiserDiagram_ZeroUML()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex Instance = sm.Get(false, "*Instance");
            IVertex InstanceAndDirect = sm.Get(false, "*InstanceAndDirect");
            IVertex Direct = sm.Get(false, "*Direct");

            IVertex arrow = sm.Get(false, @"*DiagramInternal\LineEndEnum\Arrow");
            IVertex triangle = sm.Get(false, @"*DiagramInternal\LineEndEnum\Triangle");
            IVertex filledTriangle = sm.Get(false, @"*DiagramInternal\LineEndEnum\FilledTriangle");
            IVertex diamond = sm.Get(false, @"*DiagramInternal\LineEndEnum\Diamond");
            IVertex filledDiamond = sm.Get(false, @"*DiagramInternal\LineEndEnum\FilledDiamond");
            IVertex straight = sm.Get(false, @"*DiagramInternal\LineEndEnum\Straight");


            /////////////////////////////////////////////////////////////////////////
            // AtomType 
            /////////////////////////////////////////////////////////////////////////

            IVertex vAtomType = AddDiagramItemDefinition_Combo_RectangleItem("AtomType", false,
         @"{$Is:AtomType}",
         "AtomType",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          -1, true,
          null, false);

            /////////////////////////////////////////////////////////////////////////
            // StateMachine 
            /////////////////////////////////////////////////////////////////////////

            IVertex vStateMachine = AddDiagramItemDefinition_Combo_RectangleItem("StateMachine", false,
         @"{$Is:StateMachine}",
         "StateMachine",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, 2,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          10, true,
          null, false);

            AddDiagramLine_Combo(vStateMachine,
               "State",
               @"$Is:StateMachine\State",
               @"Definition:State",
               sm.Get(false, @"*DiagramInternal\DiagramLine"),
               true,
               filledDiamond,
               null,
               -1, false,
               -1, 0, 0, 0,
               -1, 0, 0, 100);

            IVertex vState = AddDiagramItemDefinition_Combo_RectangleItem("State", false,
         @"{$Is:State}",
         "State",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          10, true,
          null, false);

            AddDiagramLine_Combo(vState,
               "Transition",
               @"$Is:State\Transition",
               @"Definition:State",
               sm.Get(false, @"*DiagramInternal\DiagramLine"),
               true,
               null,
               arrow,
               -1, false,
               -1, 0, 0, 0,
               -1, 0, 0, 100);

            /////////////////////////////////////////////////////////////////////////
            // Enum
            /////////////////////////////////////////////////////////////////////////

            IVertex vEnum = AddDiagramItemDefinition_Combo_RectangleItem("Enum", false,
         @"{$Is:Enum}",
         "Enum",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          Root.Get(false, @"System\Meta*List"), true);

            IVertex vEnum_vv = vEnum.Get(false, @"DiagramItemVertex:\VisualiserVertex:");

            vEnum_vv.AddVertex(Root.Get(false, @"System\Meta*FilterQuery"), "EnumValue:");

            vEnum_vv.AddVertex(Root.Get(false, @"System\Meta\Visualiser\List\ShowHeader"), "False");
            vEnum_vv.AddVertex(Root.Get(false, @"System\Meta\Visualiser\List\ShowMeta"), "False");



            /////////////////////////////////////////////////////////////////////////
            // SingleOperator
            /////////////////////////////////////////////////////////////////////////

            IVertex vSingleOperator = AddDiagramItemDefinition_Combo_RectangleItem("SingleOperator", false,
         @"{$Is:SingleOperator}",
         "{$Inherits:SingleOperator}",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          5, true,
          null, false);

            AddDiagramLine_Combo(vSingleOperator,
       "Expression",
       @"$Is:SingleOperator\NextExpression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       arrow,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddNextLine(vSingleOperator);


            /////////////////////////////////////////////////////////////////////////
            // MultiOperator
            /////////////////////////////////////////////////////////////////////////

            IVertex vMultiOperator = AddDiagramItemDefinition_Combo_RectangleItem("MultiOperator", false,
         @"{$Is:MultiOperator}",
         "{$Inherits:MultiOperator}",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          5, true,
          null, false);

            AddDiagramLine_Combo(vMultiOperator,
       "Expression",
       @"$Is:MultiOperator\Expression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       arrow,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddNextLine(vMultiOperator);


            /////////////////////////////////////////////////////////////////////////
            // DoubleOperator
            /////////////////////////////////////////////////////////////////////////

            IVertex vDoubleOperator = AddDiagramItemDefinition_Combo_RectangleItem("DoubleOperator", false,
         @"{$Is:DoubleOperator}",
         "{$Inherits:DoubleOperator}",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          5, true,
          null, false);

            AddDiagramLine_Combo(vDoubleOperator,
       "LeftExpression",
       @"$Is:DoubleOperator\LeftExpression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       arrow,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddDiagramLine_Combo(vDoubleOperator,
       "RightExpression",
       @"$Is:DoubleOperator\RightExpression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       arrow,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddNextLine(vDoubleOperator);

            /////////////////////////////////////////////////////////////////////////
            // Return
            /////////////////////////////////////////////////////////////////////////

            IVertex vReturn = AddDiagramItemDefinition_Combo("Return", false,
                @"{$Is:Return}",
         "Return",
         sm.Get(false, @"*DiagramOvalItem"),
         InstanceAndDirect,
         true, 40, 40, -1,
         0, 0, 0, 255,
         255, 255, 255, 255);

            AddDiagramLine_Combo(vReturn,
       "Expression",
       @"$Is:Return\Expression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       arrow,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddNextLine(vReturn);

            /////////////////////////////////////////////////////////////////////////
            // Section
            /////////////////////////////////////////////////////////////////////////

            IVertex vSection = AddDiagramItemDefinition_Combo_RectangleItem("Section", false,
         @"{$Is:Section}",
         "Section",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, 3,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          -1, true,
          null, false);


            AddDiagramLine_Combo(vSection,
          "Variable",
          @"$Is:Section\Variable",
          @"Definition:Variable",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0, true, false);

            AddDiagramLine_Combo(vSection,
          "Function",
          @"$Is:Section\Function",
          @"Definition:Function",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0);

            AddDiagramLine_Combo(vSection,
          "Type",
          @"$Is:Section\Type",
          @"BaseEdge:\To:\$Is:Type",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0);

            AddDiagramLine_Combo(vSection,
         "Do",
         @"$Is:Method\Do",
          @"BaseEdge:\To:\$Is:Atom",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
         true,
          null,
         filledTriangle,
          3, false,
         -1, 0, 0, 0,
         -1, 0, 0, 0);


            AddNextLine(vSection);

            /////////////////////////////////////////////////////////////////////////
            // Function
            /////////////////////////////////////////////////////////////////////////

            IVertex vFunction = AddDiagramItemDefinition_Combo_RectangleItem("Function", false,
         @"{$Is:Function}",
         "Function",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, 5,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          -1, true,
          null, false);

            AddDiagramLine_Combo(vFunction,
             "InputParameter",
             @"$Is:Function\InputParameter",
             @"Definition:InputParameter",
             sm.Get(false, @"*DiagramInternal\DiagramLine"),
             true,
             filledDiamond,
             null,
             2, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0, true, false);

            AddDiagramLine_Combo(vFunction,
             "Output",
            @"$Is:Function\Output",
            @"BaseEdge:\To:\$Is:Type",
            sm.Get(false, @"*DiagramInternal\DiagramLine"),
            true,
            null,
            arrow,
            2, false,
            -1, 0, 0, 0,
            -1, 0, 0, 0, true, false);

            AddDiagramLine_Combo(vFunction,
          "Variable",
          @"$Is:Function\Variable",
          @"Definition:Variable",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0, true, false);

            AddDiagramLine_Combo(vFunction,
          "Type",
          @"$Is:Function\Type",
          @"BaseEdge:\To:\$Is:Type",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0);

            AddDiagramLine_Combo(vFunction,
          "Function",
          @"$Is:Function\Function",
          @"Definition:Function",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
          true,
          filledDiamond,
          null,
          -1, false,
          -1, 0, 0, 0,
          -1, 0, 0, 0);

            AddDiagramLine_Combo(vFunction,
         "Do",
         @"$Is:Method\Do",
          @"BaseEdge:\To:\$Is:Atom",
          sm.Get(false, @"*DiagramInternal\DiagramLine"),
         true,
          null,
         filledTriangle,
          3, false,
         -1, 0, 0, 0,
         -1, 0, 0, 0);


            /////////////////////////////////////////////////////////////////////////
            // If
            /////////////////////////////////////////////////////////////////////////

            IVertex vIf = AddDiagramItemDefinition_Combo("If", false,
         @"{$Is:If}",
         "If",
          sm.Get(false, @"*DiagramRhombusItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          false);

            AddDiagramLine_Combo(vIf,
       "Test",
       @"$Is:If\Test",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       triangle,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddDiagramLine_Combo(vIf,
       "Then",
       @"$Is:If\Then",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
              null,
       filledTriangle,
       3, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddDiagramLine_Combo(vIf,
       "Else",
       @"$Is:If\Else",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
              null,
       filledTriangle,
       3, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            /////////////////////////////////////////////////////////////////////////
            // Switch
            /////////////////////////////////////////////////////////////////////////

            IVertex vSwitch = AddDiagramItemDefinition_Combo_RectangleItem("Switch", false,
         @"{$Is:Switch}",
         "Switch",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          null, false);

            AddDiagramLine_Combo(vSwitch,
       "Expression",
       @"$Is:Switch\Expression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       triangle,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddDiagramLine_Combo(vSwitch,
       "Case",
       @"$Is:Switch\Case",
       @"Definition:Case",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
              null,
       filledTriangle,
       3, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddDiagramLine_Combo(vSwitch,
       "Default",
       @"$Is:Switch\Default",
       @"Definition:Default",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
             null,
       filledTriangle,
       3, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            // Case

            IVertex vCase = AddDiagramItemDefinition_Combo_RectangleItem("Case", false,
         @"{$Is:Case}",
         "Case",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          null, false);

            AddDiagramLine_Combo(vCase,
       "Expression",
       @"$Is:Case\Expression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       triangle,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            // Default

            IVertex vDefault = AddDiagramItemDefinition_Combo_RectangleItem("Default", false,
         @"{$Is:Default}",
         "Default",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          null, false);




            AddNextLine(vCase);

            AddNextLine(vDefault);


            /////////////////////////////////////////////////////////////////////////
            // While
            /////////////////////////////////////////////////////////////////////////

            IVertex vWhile = AddDiagramItemDefinition_Combo_RectangleItem("While", false,
         @"{$Is:While}",
         "While",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          null, false);

            AddDiagramLine_Combo(vWhile,
       "Test",
       @"$Is:While\Test",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       null,
       triangle,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddDiagramLine_Combo(vWhile,
       "Do",
       @"$Is:While\Do",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
              null,
       filledTriangle,
       3, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);



            AddNextLine(vWhile);

            /////////////////////////////////////////////////////////////////////////
            // ForEach
            /////////////////////////////////////////////////////////////////////////

            IVertex vForEach = AddDiagramItemDefinition_Combo_RectangleItem("ForEach", false,
         @"{$Is:ForEach}",
         "ForEach",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          null, false);

            AddDiagramLine_Combo(vForEach,
       "Variable",
       @"$Is:ForEach\Variable",
       @"",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
       diamond,
       null,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);


            AddDiagramLine_Combo(vForEach,
            "Set",
            @"$Is:ForEach\Set",
            @"BaseEdge:\To:\$Is:Atom",
            sm.Get(false, @"*DiagramInternal\DiagramLine"),
            true,
            null,
            triangle,
            -1, false,
            -1, 0, 0, 0,
            -1, 0, 0, 100);

            AddDiagramLine_Combo(vForEach,
       "Do",
       @"$Is:ForEach\Do",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"*DiagramInternal\DiagramLine"),
       true,
              null,
       filledTriangle,
       3, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);


            AddNextLine(vForEach);


            /////////////////////////////////////////////////////////////////////////
            // Package
            /////////////////////////////////////////////////////////////////////////

            IVertex vPackage = AddDiagramItemDefinition_Combo_RectangleItem("Package", false,
         @"{$Is:Package}",
         "Package",
          sm.Get(false, @"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, 5,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          null, false);

            PackageLine[] packageLines = new PackageLine[] {
                new PackageLine("AtomType","Type"),
                new PackageLine("StateMachine","StateMachine"),
                new PackageLine("Enum","Enum"),
                new PackageLine("Class","Class"),
                new PackageLine("[]","MultiOperator") ,
                new PackageLine("[[]]","MultiOperator"),
                new PackageLine("{}","MultiOperator"),
                new PackageLine("InnerCreation","MultiOperator"),
                new PackageLine("+","DoubleOperator"),
                new PackageLine("-","DoubleOperator"),
                new PackageLine("\"* \"","DoubleOperator"),
                new PackageLine("/","DoubleOperator"),
                new PackageLine("?","SingleOperator"),
                new PackageLine("\"\\ \"","SingleOperator"),
                new PackageLine("\"|\"","DoubleOperator"),
                new PackageLine("\"||\"","DoubleOperator"),
                new PackageLine("<-","DoubleOperator"),
                new PackageLine("--","DoubleOperator"),
                new PackageLine("Return","Return"),
                new PackageLine("Section","Section"),
                new PackageLine("Function","Function"),
                new PackageLine("If","If"),
                new PackageLine("Switch","Switch"),
                new PackageLine("While","While"),
                new PackageLine("ForEach","ForEach"),
                new PackageLine("Package","Package")
                };


            foreach (PackageLine what in packageLines)
                AddDiagramLine_Combo(vPackage,
            what.Is,
            @"$Is:Package\" + what.Is,
            @"Definition:" + what.Definition,
            sm.Get(false, @"*DiagramInternal\DiagramLine"),
            true,
            diamond,
            null,
            -1, false,
            -1, 0, 0, 0,
            -1, 0, 0, 100);
        }

        public struct PackageLine
        {
            public string Is;
            public string Definition;

            public PackageLine(string _Is, string _Definition)
            {
                Is = _Is;
                Definition = _Definition;
            }
        }

        void CreateSystemMetaStoreFileSystem()
        {
            FileSystemStore.FillSystemMeta();
        }

        void CreateSystemMetaCommands()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Commands{VisualiserClass,SynchronisedVisualiser}}");
        }

        void CreateUserMeta()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{User{CurrentUser,Class:NonAtomProcess{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1}},Class:Session{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1},Aggregation:Process{$MinCardinality:0,$MaxCardinality:-1}},Class:User{Attribute:CurrentSession{$MinCardinality:1,$MaxCardinality:1},Aggregation:Session{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Settings{$MinCardinality:1,$MaxCardinality:1},Aggregation:DefaultFormalTextLanguage{$MinCardinality:1,$MaxCardinality:1},Aggregation:Queries{$MinCardinality:1,$MaxCardinality:1}},Class:Settings{Attribute:CopyOnDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Association:AllowBlankAreaDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Attribute:AllowManyDiagramItemsForOneVertex{$MinCardinality:1,$MaxCardinality:1}},Enum:AllowBlankAreaDragAndDropEnum{EnumValue:No,EnumValue:OnlyEnd,EnumValue:StartAndEnd}}}");

            sm.Get(false, @"User\NonAtomProcess").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\Session").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\User").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\Settings").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"ZeroUML\Class"));

            sm.Get(false, @"User\NonAtomProcess\StartTimeStamp").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\DateTime"));
            sm.Get(false, @"User\Session\StartTimeStamp").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\DateTime"));
            sm.Get(false, @"User\Session\Process").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"User\NonAtomProcess")); // to be updated
            sm.Get(false, @"User\User\Session").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"User\Session"));
            sm.Get(false, @"User\User\CurrentSession").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"User\Session"));
            sm.Get(false, @"User\User\Settings").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"User\Settings"));
            sm.Get(false, @"User\User\DefaultFormalTextLanguage").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\FormalTextLanguage"));
            sm.Get(false, @"User\User\Queries").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"User\AllowBlankAreaDragAndDropEnum").AddEdge(sm.Get(false, @"*$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            sm.Get(false, @"User\Settings\CopyOnDragAndDrop").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"User\Settings\AllowBlankAreaDragAndDrop").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"*AllowBlankAreaDragAndDropEnum"));
            sm.Get(false, @"User\Settings\AllowManyDiagramItemsForOneVertex").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
        }

        void CreateUser(IVertex user)
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            GeneralUtil.ParseAndExcute(user, sm, "{Settings:{CopyOnDragAndDrop:False,AllowManyDiagramItemsForOneVertex:True},Queries:{String:test,String:\"test{test2}\"}}");

            user.Get(false, "Settings:").AddEdge(sm.Get(false, "*AllowBlankAreaDragAndDrop"), sm.Get(false, @"User\AllowBlankAreaDragAndDropEnum\StartAndEnd"));

            user.AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"User\User"));
            user.Get(false, "Settings:").AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"User\Settings"));

            user.AddEdge(sm.Get(false, @"User\User\DefaultFormalTextLanguage"), Root.Get(false, @"System\FormalTextLanguage\ZeroCode"));

            //IVertex cs = user.Get(false, @"CodeSettings:");
            //cs.AddEdge(sm.Get(false, @"*$Is"), sm.Get(false, @"User\CodeSettings"));

            //cs.AddEdge(sm.Get(false, @"User\CodeSettings\Keyword"), sm.Get(false, @"ZeroUML\Keyword"));

            //foreach (IEdge e in Root.GetAll(false, @"System\FormalTextLanguage\ZeroCode\DefaultImports\"))
                //if (!GraphUtil.GetValueAndCompareStrings(e.To, "$DirectMeta") && !GraphUtil.GetValueAndCompareStrings(e.To, "$Direct"))
              //  cs.AddEdge(e.Meta, e.To);
              // XXX
            IVertex session = user.AddVertex(sm.Get(false, @"User\User\Session"), null);
            user.AddEdge(sm.Get(false, @"User\User\CurrentSession"), session);
        }

        void CreateUsers()
        {
            IVertex sm = Root.Get(false, @"System\Meta\User");

            GeneralUtil.ParseAndExcute(Root, sm, "{User{User:root,User:wlodek,User:tadek}}");

            foreach (IEdge u in Root.GetAll(false, @"User\"))
                CreateUser(u.To);

            Root.Get(false, @"User").AddEdge(Root.Get(false, @"System\Meta\User\CurrentUser"), Root.Get(false, @"User\root"));
        }

        void AfterCreateUsers()
        {
            _DefaultFormalTextLanguage = MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\DefaultFormalTextLanguage:");
        }

        void AddDrives()
        {
            string[] drives = System.IO.Directory.GetLogicalDrives();

            IVertex DriveMeta = Root.Get(false, @"System\Meta\Store\FileSystem\Drive");

            foreach (string str in drives)
            {
                FileSystemStore fss = new FileSystemStore(str, this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

                //fss.IncludeFileContent = true;                

                Root.AddEdge(DriveMeta, fss.Root);
            }
        }

        public MinusZero()
        {
            //Initialize();
        }

        private System.IO.StreamWriter logFile;

        public bool DoLog = true;

        public int LogLevel = 0;

        private void InitializeLog()
        {
            if (DoLog)
            {
                logFile = new System.IO.StreamWriter("log.txt");
                logFile.AutoFlush = true;

                Log(0, "InitializeLog", "START");
            }
        }

        public void Log(int Level, string Where, string What)
        {
            if (DoLog && Level <= LogLevel)
                logFile.WriteLine(System.DateTime.Now.ToLongTimeString()+":"+ System.DateTime.Now.Millisecond+"["+Level+"]:"+" "+Where+": "+What);
                //logFile.WriteLine(What);
        }

        private void DisposeLog()
        {
            Log(0, "DisposeLog", "STOP");
            logFile.Close();
        }

        ///

        private void AddIsAttribute(string what)
        {
            AddIsAttribute_inner(@"System\Meta\ZeroTypes*" + what + ":", what);
            AddIsAttribute_inner(@"System\Meta\Visualiser*" + what + ":", what);
            AddIsAttribute_inner(@"System\Meta\User*" + what + ":", what);
        }

        private void AddIsAttribute_inner(string s, string what)
        {
            IVertex attributes = root.GetAll(false, s);
            IVertex ismeta = root.Get(false, @"System\Meta*$Is");
            IVertex ameta = root.Get(false, @"System\Meta\ZeroUML\Class\" + what);

            foreach (IEdge v in attributes)
                if (v.To.Get(false, @"$Is:" + ameta) == null)
                    v.To.AddEdge(ismeta, ameta);

        }

        private void AddIsAggregation()
        {
            AddIsAggregation_inner(@"System\Meta\ZeroTypes*Attribute:");
            AddIsAggregation_inner(@"System\Meta\ZeroTypes*Aggregation:");

            AddIsAggregation_inner(@"System\Meta\Visualiser*Attribute:");
            AddIsAggregation_inner(@"System\Meta\Visualiser*Aggregation:");

            AddIsAggregation_inner(@"System\Meta\User*Attribute:");
            AddIsAggregation_inner(@"System\Meta\User*Aggregation:");
        }

        private void AddIsAggregation_inner(string s)
        {
            IVertex isaggregationtarget = root.GetAll(false, s);

            IVertex isAggregation = root.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");
            IVertex empty = root.Get(false, @"System\Meta\Base\$Empty");

            foreach (IEdge v in isaggregationtarget)
                if (v.To.Get(false, @"$IsAggregation:") == null)
                    v.To.AddEdge(isAggregation, empty);
        }

        bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                Finalize();

                DisposeLog();

                disposed = true;
            }
        }

        void Finalize()
        {
            CommitTransaction();
        }

        public void Refresh()
        {
            List<StoreId> StoresPersistency = new List<StoreId>();

            foreach (IStore s in Stores)
            {
                StoreId e = new StoreId(s.TypeName, s.Identifier);

                StoresPersistency.Add(e);

                s.Close();
            }

            Bootstrap();

            foreach (StoreId e in StoresPersistency)
            {
                GetStore(e.TypeName, e.Identifier);
            }

        }

        public void BeginTransaction()
        {
            foreach (ITransactionRoot r in Stores)
                r.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            foreach (IStore s in Stores)
                s.Detach();

            foreach (IStore s in Stores)
                s.RollbackTransaction();

            foreach (IStore s in Stores)
                s.Attach();
        }

        public void CommitTransaction()
        {
            foreach (IStore s in Stores)
                s.Detach();

            foreach (IStore s in Stores)
                s.CommitTransaction();

            foreach (IStore s in Stores)
                s.Attach();
        }


        public IStore GetStore(string StoreTypeName, string StoreIdentifier)
        {
            IStore store = Stores.Where(s => s.TypeName == StoreTypeName & s.Identifier == StoreIdentifier).FirstOrDefault();

            if (store != null)
                return store;

            store = (IStore)Activator.CreateInstance(Type.GetType(StoreTypeName), new object[] { StoreIdentifier, this, GetStoreDefaultAccessLevelList });

            //Stores.Add(store);
            // store's constructor does this

            return store;
        }

        public void AddFastAccessVertexes()
        {
            EdgeTarget = Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget");
            Is = Root.Get(false, @"System\Meta\Base\Vertex\$Is");
            IsAggregation = Root.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");
        }


        public void Initialize()
        {
            LogLevel = -2;

            InitializeLog();

            PreBootstrap();            

            Bootstrap();

            CreateSystem();

            Init();

            CreateSystemMeta();

            CreatePresentation();

            CreateSystemMetaBase();


            AddFastAccessVertexes();


            CreateSystemMetaZeroUML();

            CreateSystemMetaZeroTypes();

            CreateSystemMetaZeroUML_Action_part();            

            CreateSystemFormalTextLanguageZeroCode();


            CreateSystemFormalTextLanguegeZeroCode_Keywords();


            CreateSystemMetaVisualiserDiagram();

            CreateSystemMetaVisualiser();

            CreateSystemData();

            CreateSystemDataVisualiserDiagram();

            CreateSystemDataVisualiserDiagram_ZeroUML();

            CreateSystemMetaStoreFileSystem();

            CreateSystemMetaCommands();

            CreateUserMeta();

            CreateUsers();

            AfterCreateUsers();
            

            Init_AfterZeroCodeDefintionCreated();



            AddIsAttribute("Attribute");

            AddIsAttribute("Association");

            AddIsAttribute("Aggregation");

            AddIsAggregation();

            AddDrives();



            UIWpf.UIWpf.InitializeUIWpf();

            IsInitialized = true;

            //

            //AutoTest.ParserTest();
        }
    }
}