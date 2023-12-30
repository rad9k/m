using m0.FormalTextLanguage;
using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Store;
using m0.Store.FileSystem;
using m0.UIWpf.Visualisers.Helper;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Xml.Linq;


namespace m0
{
    public class LegacySystem_MinusZero : IStoreUniverse, IDisposable
    {
        public static LegacySystem_MinusZero Instance = new LegacySystem_MinusZero();

        public bool IsInitialized = false;

        public AccessLevelEnum[] GetStoreDefaultAccessLevelList = new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions };


        IList<IStore> stores = new List<IStore>();

        public IList<IStore> Stores { get { return stores; } }


        IStore tempstore;

        public IStore TempStore { get { return tempstore; } }

        IStore emptystore;

        public IStore EmptyStore { get { return emptystore; } }


        IVertex root;

        public IVertex Root { get { return root; } }

        public IVertex Inherits;

        public IVertex StackFrameInherits;

        IVertex empty;

        public IVertex Empty { get { return empty; } }

        IVertex dolar;

        public IVertex Dolar { get { return dolar; } }

        IUserInteraction _DefaultUserInteraction;

        public IUserInteraction DefaultUserInteraction { get { return _DefaultUserInteraction; } }


        //

        IFormalTextParser _DefaultParser;

        public IFormalTextParser DefaultParser { get { return _DefaultParser; } }


        IExecuter _DefaultExecuter;

        public IExecuter DefaultExecuter { get { return _DefaultExecuter; } }

        //

        private IVertex _DefaultFormalTextLanguage;

        public IVertex DefaultFormalTextLanguage { get { return _DefaultFormalTextLanguage; } }



        IFormalTextGenerator _DefaultCodeGenerator;

        public IFormalTextGenerator DefaultCodeGenerator { get { return _DefaultCodeGenerator; } }


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
            MinusZero.Instance.stores = stores;

            IStore rootstore = new MemoryStore("$-0$ROOT$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

            root = rootstore.Root;

            MinusZero.Instance.root = root;

            Stores.Clear();

            Stores.Add(rootstore);


            tempstore = new MemoryStore("$-0$TEMP$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions }, true);

            MinusZero.Instance.tempstore = tempstore;

            emptystore = new MemoryStore("$-0$EMPTY$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions }, true);

            empty = new IdentifiedVertex("$Empty", emptystore);

            empty.Value = "$Empty";

            MinusZero.Instance.empty = empty;

            emptystore.Root.AddEdge(null, empty);


            tempRoot = CreateTempVertex();

            MinusZero.Instance.tempRoot = tempRoot;

        }

        void Init()
        {
            _DefaultUserInteraction = null;
        }

        void Init_AfterZeroCodeDefintionCreated()
        {
            ZeroCode.ZeroCodeEngine zeroCodeEngine = new ZeroCode.ZeroCodeEngine();

            _DefaultParser = zeroCodeEngine;
            _DefaultExecuter = zeroCodeEngine;
            _DefaultCodeGenerator = zeroCodeEngine;

            MinusZero.Instance._DefaultFormalTextParser = DefaultParser;
            MinusZero.Instance._DefaultExecuter = DefaultExecuter;
            MinusZero.Instance._DefaultFormalTextGenerator = DefaultCodeGenerator;
        }


        void CreateSystem()
        {
            IVertex system = Root.AddVertex(null, "System");

            // turned off for now
            // system.AddVertex(null,"Session").AddVertex(null,"Visualisers");

            IVertex meta = system.AddVertex(null, "Meta");

            IVertex tl = system.AddVertex(null, "FormalTextLanguage");

            IVertex sto = meta.AddVertex(null, "Store");
        }

        void CreatePresentation()
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, sm, "{Presentation{$Hide,$DisplayLarger,$UpdateAfterInteractionEnd}}");
        }

        void CreateSystemMetaBase_Part1()
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, null, "{Base{$,Vertex{$IsLink,$Inherits,$NoInherit,$NoCopy,$StackFrameInherits,$Is,$EdgeTarget,$VertexTarget,$IsAggregation,$MinCardinality,$MaxCardinality,$MaxTargetCardinality,$DefaultValue,$DefaultViewVisualiser,$DefaultEditVisualiser,$DefaultOpenVisualiser,$Group,$Section,$Description,$ExecutableEndPoint,$GraphChangeTrigger,$VertexEval,MetaEdge,Author,Dependency},$Import,$ImportMeta,$Keyword,$KeywordGroupDefinition,$$KeywordGroup,$$KeywordManyRoot,$$LocalRoot,$$StartInLocalRoot,$$EmptyKeyword,$$NewVertexKeyword,$$LinkKeyword,$$NonSelfRecursiveParameters,$$Import,$$ImportDirect,$$ImportMeta,$$ImportDirectMeta,$$NoSequentialExecution,$$NextAtomRoot,$NewLine,$ParseRoot,$ParseArtefacts}}");

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base").AddEdge(
                null,
                Empty);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Presentation\$Hide").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex"));

            RootVariableVertexLinksCreate();


            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$Is").AddVertex(LegacySystem.Graph.EasyVertex.Get(sm, false, @"Presentation\$Hide"), "0");



            //

            // sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex")); // TO BE DONE. now there is very strange error in query mechanics

            // sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, @"Base\Vertex\$EdgeTarget"), _vertex_); // not working too...

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$VertexTarget").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex"));

            // sm.Get(false, @"Base\Vertex\$VertexTarget").AddEdge(sm.Get(false, @"*$EdgeTarget"), _vertex_);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$Inherits").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$Inherits").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$NoInherit"),
                Empty);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$Is").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex"));


            //sm.Get(false, @"Base\Vertex\$DefaultViewVisualiser").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            //sm.Get(false, @"Base\Vertex\$DefaultEditVisualiser").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));

            // sm.Get(false, @"Base\Vertex\$DefaultOpenVisualiser").AddEdge(sm.Get(false, @"*$EdgeTarget"), sm.Get(false, @"Base\Vertex"));  

            // hack for now

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$DefaultViewVisualiser").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$IsLink"),
                Empty);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$DefaultEditVisualiser").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$IsLink"),
                Empty);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$DefaultOpenVisualiser").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$IsLink"),
                Empty);


            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$IsAggregation").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex"));            
        }

        void CreateSystemMetaBase_Part2()
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");            

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$MinCardinality").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$MaxCardinality").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            //

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$ExecutableEndPoint").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\ExecutionFlow\Executable"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$ExecutableEndPoint").AddEdge( // XXX ????
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$IsAggregation"), Empty);            

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$GraphChangeTrigger").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\ExecutionFlow\GraphChangeTrigger"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$GraphChangeTrigger").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$NoInherit"),
                Empty);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$GraphChangeTrigger").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$NoCopy"),
                Empty);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$GraphChangeTrigger").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Presentation\$Hide"),
                "0");

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$GraphChangeTrigger").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Presentation\$Hide"),
                "1");

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$VertexEval").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\ExecutionFlow\VertexEval"));
        }


        void RootVariableVertexLinksCreate()
        {
            IVertex System = GraphUtil.GetQueryOutFirst(Root, null, "System");

            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");

            IVertex Base = GraphUtil.GetQueryOutFirst(Meta, null, "Base");

            //empty = GraphUtil.GetQueryOutFirst(Base, null, "$Empty"); // there are some bugs related to this and old zeroscript.get
            // we want to use $-0$EMPTY$STORE$

            IVertex Vertex = GraphUtil.GetQueryOutFirst(Base, null, "Vertex");

            Inherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$Inherits");

            StackFrameInherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$StackFrameInherits");

            dolar = GraphUtil.GetQueryOutFirst(Base, null, "$");
        }

        void CreateSystemMetaZeroUML()
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, null, "{ZeroUML{Type,AtomType,StateMachine{State{Transition}},Enum{EnumValue},Selector,Class{Attribute{MinValue,MaxValue},Association,Aggregation,$PlatformClassName}}}");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Selector"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class\Attribute").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$IsAggregation"), empty);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class\Aggregation").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$IsAggregation"), empty);

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Enum\EnumValue"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Enum\EnumValue").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Enum\EnumValue").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$IsAggregation"), empty);

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\StateMachine\State"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\StateMachine\State\Transition"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\StateMachine\State\Transition").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\StateMachine\State"));


            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Type"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class").AddEdge(
                null,
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"));



            LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Class\Attribute").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Selector"));
            LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Class\Attribute").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm,false, @"*$VertexTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Type"));

            LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Class\Association").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Selector"));
            LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Class\Association").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$VertexTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Class\Aggregation").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Selector"));
            LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Class\Aggregation").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$VertexTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));


            // sm.Get(false, @"ZeroUML\Type").AddEdge(sm.Get(false, "*$Inherits"),sm.Get(false, @"Base\Vertex"));    // do not want it at last for now        

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Type"));
            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Enum").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType")); // was ZeroUML\Type
            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\StateMachine").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType"));
        }

        void AddDotNetStaticMethodAsExecutableEndpoint(IVertex baseVertex, string _methodName)
        {
            IVertex callableEndPoint = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$ExecutableEndPoint");
            IVertex dotNetEndPoint = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod");
            IVertex typeName = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod\DotNetTypeName");
            IVertex methodName = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod\DotNetMethodName");
            IVertex _is = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Is");

            IVertex n = baseVertex.AddVertex(callableEndPoint, null);
            n.AddEdge(_is, dotNetEndPoint);
            n.AddVertex(typeName, "m0.ZeroUML.Instructions.BaseInstructions, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            n.AddVertex(methodName, _methodName);            
        }

        void CreateSystemMetaZeroUML_ZeroCode_part()
        {
            IVertex smu = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML");
            IVertex smzt = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes");
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            IVertex nse = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\$$NoSequentialExecution");

            IVertex isAggregation = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsAggregation");

            // "\ " > "\"
            // "|" > ":"
            // "||" > "::"        

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(smu, sm,
                // NextExpression{$MinCardinality:1 ?????? it was like this. maybe on a purpose :) :/
                "{Link{Target{$MinCardinality:1,$MaxCardinality:1}},ExpressionAtom{NextExpression{$MinCardinality:0,$MaxCardinality:1}},Atom" +
                ",PropagateToStackExpression,ZeroOperator" +
                ",SingleOperator{Expression{$MinCardinality:1,$MaxCardinality:1}}" +                
                ",DoubleOperator{LeftExpression{$MinCardinality:1,$MaxCardinality:1},RightExpression{$MinCardinality:1,$MaxCardinality:1}}" +                
                ",MultiOperator{Expression{$MinCardinality:0,$MaxCardinality:-1}}" +
                ",Query" +
                ",FunctionCall{Target{$MinCardinality:1,$MaxCardinality:1}}" +
                ",MethodCall{Target{$MinCardinality:1,$MaxCardinality:1}},New" +
                ",SetIndex,SetCount" +
                ",\"{}\",InnerCreation,EdgeSetAdd,EdgeSetSubstract,+,-,Mul,/,?,\"\\ \",InEdgesSlash,Colon,DoubleColon,DoubleSemicolon,CopySet,MetaToTo,()"+
                ",RedirectLeftEdgesToRightVertices,AddLeftEdgesToRightVertices,AddRightEdgesIntoLeftEdges,DeleteRightVertices,DeleteRightEdgesFromLeftEdges,DeleteRightVerticesFromLeftEdges" +
                ",SetLeftVertexesToFirstRightVertexValue,AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex,AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex" +
                ",Equal,ExactEqual,VertexEqual,NotEqual,Negation,And,Or,MoreThan,LessThan,MoreOrEqualThan,LessOrEqualThan" +
                ",Action,Return{Expression{$MinCardinality:0,$MaxCardinality:1}},NextOut{Next{$MinCardinality:0,$MaxCardinality:1}}" +
                ",StackFrameCreator{$$NextAtomRoot:" +
                //Do{$MinCardinality:0,$MaxCardinality:1}
                ",Variable{$MinCardinality:0,$MaxCardinality:-1},Type{$$NoSequentialExecution:,$MinCardinality:0,$MaxCardinality:-1}}" +
                ",StackFrameCreatorWithInputOutput{Output{$$NoSequentialExecution:,$MinCardinality:0,$MaxCardinality:1},InputParameter{$$NoSequentialExecution:,$MinCardinality:0,$MaxCardinality:-1}}" +
                ",Function{$$NoSequentialExecution:},Section" +
                ",While{Test{$$NoSequentialExecution:,$MinCardinality:1,$MaxCardinality:1}}" +
                ",ForEach{Variable{$$NoSequentialExecution:,$MinCardinality:1,$MaxCardinality:1},Set{$$NoSequentialExecution:,$MinCardinality:1,$MaxCardinality:1}}" +
                ",If{Test{$$NoSequentialExecution:,$MinCardinality:1,$MaxCardinality:1}},Test{Expression{$$NoSequentialExecution:,$MinCardinality:1,$MaxCardinality:1}},Case{Test{$$NoSequentialExecution:,$MinCardinality:1,$MaxCardinality:1}},Fallback" +
                ",EmptySet,Constant" +
                ",Execute,Parse,ParseWithLanguage{FormalTextLanguage{$MinCardinality:0,$MaxCardinality:1}},Generate,GenerateWithLanguage{FormalTextLanguage{$MinCardinality:0,$MaxCardinality:1}}" +
                ",this" +
                "}");            

            // Link

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Link"), "Link");

            // query

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Query"), "QueryOperator");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "\"{}\""), "InnerOperator");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "?"), "QuestionMarkOperator");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "\"\\ \""), "SlashOperator");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "InEdgesSlash"), "InEdgesSlashOperator");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "\"Colon\""), "ColonOperator");            

            // edge operators
            
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "RedirectLeftEdgesToRightVertices"), "RedirectLeftEdgesToRightVertices");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "AddLeftEdgesToRightVertices"), "AddLeftEdgesToRightVertices");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "AddRightEdgesIntoLeftEdges"), "AddRightEdgesIntoLeftEdges");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "DeleteRightVertices"), "DeleteRightVertices");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "DeleteRightEdgesFromLeftEdges"), "DeleteRightEdgesFromLeftEdges");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "DeleteRightVerticesFromLeftEdges"), "DeleteRightVerticesFromLeftEdges");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "SetLeftVertexesToFirstRightVertexValue"), "SetLeftVertexesToFirstRightVertexValue");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex"), "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex"), "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex");

            // edge set operators

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "EdgeSetAdd"), "EdgeSetAdd");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "EdgeSetSubstract"), "EdgeSetSubstract");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "SetIndex"), "SetIndex");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "SetCount"), "SetCount");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "EmptySet"), "EmptySet");

            // number algebra operators

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "+"), "Add");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "-"), "Substract");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Mul"), "Multiply");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "/"), "Divide");

            // logic operators

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Equal"), "Equal");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "ExactEqual"), "ExactEqual");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "VertexEqual"), "VertexEqual");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "NotEqual"), "NotEqual");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Negation"), "Negation");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "And"), "And");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Or"), "Or");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "MoreThan"), "MoreThan");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "LessThan"), "LessThan");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "MoreOrEqualThan"), "MoreOrEqualThan");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "LessOrEqualThan"), "LessOrEqualThan");

            // general operators

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "()"), "Bracket");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "FunctionCall"), "FunctionCall");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Return"), "Return");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "ForEach"), "ForEach");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "While"), "While");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "If"), "If");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Test"), "Test");

            // oo operators

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "MethodCall"), "MethodCall");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "New"), "New");

            // stack operators

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Variable"), "CreateStackEdge");

            // vertex creation operators

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleColon"), "DoubleColonOperator");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleSemicolon"), "DoubleSemicolonOperator");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "InnerCreation"), "InnerCreation");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "CopySet"), "CopySet");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "MetaToTo"), "MetaToTo");

            // meta

            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Execute"), "Execute");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Parse"), "Parse");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "Generate"), "Generate");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu, false, "ParseWithLanguage"), "Parse");
            AddDotNetStaticMethodAsExecutableEndpoint(LegacySystem.Graph.EasyVertex.Get(smu,false, "GenerateWithLanguage"), "Generate");



            ////////////////////////////////////////////////////////////////////////

            // method
            IVertex method = LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class").AddVertex(null, "Method");
            method.AddEdge(LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "StackFrameCreatorWithInputOutput"));

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(method, sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            // cycle edges
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator").AddEdge(
                null,
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Function"));


            // expression inherits
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Constant").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"EmptySet").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Constant"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ZeroOperator").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ExpressionAtom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ExpressionAtom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ExpressionAtom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ExpressionAtom"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Query").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm,false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"FunctionCall").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "MultiOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"SetIndex").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "SingleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"SetCount").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "InnerCreation").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "MultiOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "\"{}\"").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "\"{}\"").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "MultiOperator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"MethodCall").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "MultiOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"New").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "SingleOperator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"EdgeSetAdd").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"EdgeSetSubstract").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"+").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"-").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "Mul").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"/").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, "Equal").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "ExactEqual").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"), 
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "VertexEqual").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "NotEqual").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "Negation").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "SingleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "And").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "Or").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "MoreThan").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "LessThan").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "MoreOrEqualThan").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "LessOrEqualThan").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"?").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "\"\\ \"").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "Colon").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleColon").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"), 
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleSemicolon").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "CopySet").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "MetaToTo").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"()").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "SingleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"RedirectLeftEdgesToRightVertices").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"AddLeftEdgesToRightVertices").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"AddRightEdgesIntoLeftEdges").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DeleteRightVertices").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DeleteRightEdgesFromLeftEdges").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DeleteRightVerticesFromLeftEdges").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"SetLeftVertexesToFirstRightVertexValue").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleOperator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleColon").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "PropagateToStackExpression"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleSemicolon").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "PropagateToStackExpression"));

            // rest inherits
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Action").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Atom"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreatorWithInputOutput").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "StackFrameCreator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Return").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Return").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Section").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Section").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Section").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "StackFrameCreator"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"While").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"While").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"If").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"If").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Test").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Test").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Case").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut")); // XXX got to think
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Case").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Fallback").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Fallback").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ForEach").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "NextOut"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ForEach").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Action"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Function").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "StackFrameCreatorWithInputOutput"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Execute").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Parse").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Generate").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ZeroOperator"));

            //Link
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Link\Target").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smzt, false, @"VertexType"));

            //expression edges
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreatorWithInputOutput\InputParameter").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$VertexTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Type"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator\Expression").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            // $IsAggregation's for expressions
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator\Expression").AddEdge(isAggregation, Empty);

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"FunctionCall\Target").AddEdge(isAggregation, Empty); // XXX
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"MethodCall\Target").AddEdge(isAggregation, Empty); // XXX

            //rest edges
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Return\Expression").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"NextOut\Next").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            //smu.Get(false, @"FunctionCall\Target").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"StackFrameCreator"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"FunctionCall\Target").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom")); // XXX
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"MethodCall\Target").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom")); // XXX

            //LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Do").AddEdge(
                //LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                //LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Variable").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$VertexTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Type"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Type").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Type"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreatorWithInputOutput\Output").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Type"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"While\Test").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"If\Test").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Test\Expression").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Case\Test").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            // smu.Get(false, @"ForEach\Variable").AddEdge(sm.Get(false, @"*$EdgeTarget"), smu.Get(false, @"Query")); // better this
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ForEach\Set").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            // meta            
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ParseWithLanguage\FormalTextLanguage").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"GenerateWithLanguage\FormalTextLanguage").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Atom"));

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ParseWithLanguage\FormalTextLanguage").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"GenerateWithLanguage\FormalTextLanguage").AddEdge(isAggregation, Empty);


            // $IsAggregation's for EdgeTargets
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Return\Expression").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"NextOut\Next").AddEdge(isAggregation, Empty);

            //LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Do").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Variable").AddEdge(isAggregation, Empty);
            LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Type").AddEdge(isAggregation, Empty);
            //  smu.Get(false, @"StackFrameCreatorWithInputOutput\Output").AddEdge(isAggregation, Empty); // this - no!            

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"While\Test").AddEdge(isAggregation, Empty);

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"If\Test").AddEdge(isAggregation, Empty);

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Test\Expression").AddEdge(isAggregation, Empty);

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"Case\Test").AddEdge(isAggregation, Empty);

            LegacySystem.Graph.EasyVertex.Get(smu, false, @"ForEach\Set").AddEdge(isAggregation, Empty);

            // package
            IVertex package = smu.AddVertex(null, "Package");            

            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Link"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "AtomType"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "StateMachine"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Enum"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Class"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Query"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "FunctionCall"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "SetIndex"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "SetCount"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "InnerCreation"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "\"{}\""));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "EdgeSetAdd"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "EdgeSetSubstract"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "EmptySet"));

            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "+"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "-"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Mul"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "/"));

            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Equal"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "ExactEqual"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "VertexEqual"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "NotEqual"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Negation"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "And"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Or"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "MoreThan"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "LessThan"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "MoreOrEqualThan"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "LessOrEqualThan"));

            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "?"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "\"\\ \""));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Colon"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleColon"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleSemicolon"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "CopySet"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "MetaToTo"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "()"));            
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "RedirectLeftEdgesToRightVertices"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "AddLeftEdgesToRightVertices"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "AddRightEdgesIntoLeftEdges"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "DeleteRightVertices"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "DeleteRightEdgesFromLeftEdges"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "DeleteRightVerticesFromLeftEdges"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "SetLeftVertexesToFirstRightVertexValue"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Section"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Function"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "If"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "Test"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "While"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "ForEach"));

            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "MethodCall"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(smu, false, "New"));

            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\$Import"));
            package.AddEdge(null, LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\$ImportMeta"));
        }

        void CreateSystemFormalTextLanguegeZeroCode_Keywords()
        {
            IVertex zc = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\FormalTextLanguage\ZeroCode");
            IVertex zt = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes");
            IVertex k = zc.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\FormalTextLanguage\Keywords"), "");


            IVertex smu = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML");
            IVertex smb = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base");

            IVertex keyword = LegacySystem.Graph.EasyVertex.Get(smb, false, @"$Keyword");
            IVertex keywordGroup = LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordGroup");
            IVertex keywordGroupDefinition = LegacySystem.Graph.EasyVertex.Get(smb, false, @"$KeywordGroupDefinition");

            IVertex kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink = k.AddVertex(keywordGroupDefinition, "ColonEmptyInner2SlashMarkIndexMethodNewLink");
            IVertex kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy = k.AddVertex(keywordGroupDefinition, "ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy");
            IVertex kgd_ColonEmptyInner2SlashMarkIndexMethod = k.AddVertex(keywordGroupDefinition, "ColonEmptyInner2SlashMarkIndexMethod");
            IVertex kgd_Empty2Inner = k.AddVertex(keywordGroupDefinition, "Empty2Inner");
            IVertex kgd_InnerCreation = k.AddVertex(keywordGroupDefinition, "InnerCreation");
            IVertex kgd_SlashMarkIndexMethod = k.AddVertex(keywordGroupDefinition, "SlashMarkIndexMethod");
            IVertex kgd_SlashMarkIndexMethodInner2 = k.AddVertex(keywordGroupDefinition, "SlashMarkIndexMethodInner2");
            IVertex kgd_Inner = k.AddVertex(keywordGroupDefinition, "Inner");

            IVertex isAggregation = LegacySystem.Graph.EasyVertex.Get(root, false, @"System\Meta\Base\Vertex\$IsAggregation");
            //IVertex empty = LegacySystem.Graph.EasyVertex.Get(root, false, @"System\Meta\Base\$Empty");
            // we wanto to use instance.empty
            IVertex _is = LegacySystem.Graph.EasyVertex.Get(root, false, @"System\Meta\Base\Vertex\$Is");


            IVertex any = k.AddVertex(null, "(?<ANY>)");

            IVertex last = k.AddVertex(null, "(?<LAST>)");

            IVertex emptyKeyword = LegacySystem.Graph.EasyVertex.Get(smb, false, "$$EmptyKeyword");
            IVertex newVertexKeyword = LegacySystem.Graph.EasyVertex.Get(smb, false, "$$NewVertexKeyword");
            IVertex linkKeyword = LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LinkKeyword");
            IVertex nonSelfRecursiveParameters = LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$NonSelfRecursiveParameters");

            string anyString = "(?<ANY>)";

            
            // import meta
            //
            // import meta (?<name>) (?<link>)

            IVertex importMeta = k.AddVertex(keyword, "import (?<name>) (?<link>) meta");

            importMeta.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$ImportMeta"), "import[ ]+\"(?<name>.*)\"[ ]+@(?<link>[^ ]+)[ ]+meta[ ]*\\r");

            IVertex importMeta_name = importMeta.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$ImportMeta"), "(?<name>)");

            importMeta_name.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$IsLink"), null);

            importMeta_name.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smb, false, @"$ImportMeta"));

            importMeta.AddVertex(last, "(?<link>)");            


            // import
            //
            // import (?<name>) (?<link>)

            IVertex import = k.AddVertex(keyword, "import (?<name>) (?<link>)");

            import.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$Import"), "import[ ]+\"(?<name>.*)\"[ ]+@(?<link>[^ ]+)[ ]*\\r");

            IVertex import_name = import.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$Import"), "(?<name>)");

            import_name.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$IsLink"), Empty);

            import_name.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smb, false, @"$Import"));

            import.AddVertex(last, "(?<link>)");

            //import.AddVertex(last, "(?<link>)");
            

            // import direct 
            //
            // import direct  (?<link>)

            IVertex importDirect = k.AddVertex(keyword, "import (?<link>) direct");

            importDirect.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$ImportDirect"), "import[ ]+@(?<link>[^ ]+)[ ]+direct[ ]*\\r");

            IVertex importDirect_link = importDirect.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$ImportDirect"), "(?<link>)");


            // import direct meta
            //
            // import direct meta (?<link>)

            IVertex importDirectMeta = k.AddVertex(keyword, "import (?<link>) direct meta");

            importDirectMeta.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$ImportDirectMeta"), "import[ ]+@(?<link>[^ ]+)[ ]+direct[ ]+meta[ ]*\\r");

            IVertex importDirectMeta_link = importDirectMeta.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$ImportDirectMeta"), "(?<link>)");


          /*  // comment
            //
            // # (?<text>)

            IVertex comment = k.AddVertex(keyword, "REM (?<text>)");

            comment.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Description"), "(?<text>)");
            */

            // default
            //
            // default (?<expr>)

            IVertex _default = k.AddVertex(keyword, "default (?<expr>)");

            _default.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$DefaultValue"), "(?<expr>)");

            // class
            //
            // class (?<name>)

            IVertex _class = k.AddVertex(keyword, "class (?<name>)");

            IVertex class_class = _class.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class"), "(?<name>)");

            class_class.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class"));

            // attribute
            //
            // attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) <<(?<MinValue>):(?<MaxValue>)>>

            IVertex attribute3 = k.AddVertex(keyword, "attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) <<(?<MinValue>):(?<MaxValue>)>>");

            IVertex attribute3_attribute = attribute3.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"), "(?<name>)");

            attribute3_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute3_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute\MinValue"), "(?<MinValue>)");

            attribute3_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute\MaxValue"), "(?<MaxValue>)");

            attribute3_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            attribute3_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            attribute3_attribute.AddEdge(isAggregation, empty);

            attribute3_attribute.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"));

            // attribute
            //
            // attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) 

            IVertex attribute4 = k.AddVertex(keyword, "attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");

            IVertex attribute4_attribute = attribute4.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"), "(?<name>)");

            attribute4_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute4_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            attribute4_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            attribute4_attribute.AddEdge(isAggregation, empty);

            attribute4_attribute.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"));

            // attribute
            //
            // attribute (?<name>) (?<type>) <<(?<MinValue>):(?<MaxValue>)>>

            IVertex attribute2 = k.AddVertex(keyword, "attribute (?<name>) (?<type>) <<(?<xMinValue>):(?<MaxValue>)>>");

            IVertex attribute2_attribute = attribute2.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"), "(?<name>)");

            attribute2_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute2_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute\MinValue"), "(?<xMinValue>)");

            attribute2_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute\MaxValue"), "(?<MaxValue>)");

            attribute2_attribute.AddEdge(isAggregation, empty);

            attribute2_attribute.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"));

            // attribute
            //
            // attribute (?<name>) (?<type>)

            IVertex attribute = k.AddVertex(keyword, "attribute (?<name>) (?<type>)");

            IVertex attribute_attribute = attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"), "(?<name>)");

            attribute_attribute.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            attribute_attribute.AddEdge(isAggregation, empty);

            attribute_attribute.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Attribute"));

            // variable
            //
            // variable (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex variable = k.AddVertex(keyword, "variable (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");

            IVertex variable_variable = variable.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Variable"), "(?<name>)");

            variable_variable.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            variable_variable.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            variable_variable.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            variable_variable.AddEdge(isAggregation, empty);

            variable_variable.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Variable"));

            // variable
            //
            // variable (?<name>) (?<type>)

            IVertex variable2 = k.AddVertex(keyword, "variable (?<name>) (?<type>)");

            IVertex variable2_variable = variable2.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Variable"), "(?<name>)");

            variable2_variable.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            variable2_variable.AddEdge(isAggregation, empty);

            variable2_variable.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"StackFrameCreator\Variable"));

            // aassociation
            //
            // association (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex association2 = k.AddVertex(keyword, "association (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");

            IVertex association2_association = association2.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Association"), "(?<name>)");

            association2_association.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            association2_association.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            association2_association.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            association2_association.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Association"));

            // aassociation
            //
            // association (?<name>) (?<type>)

            IVertex association = k.AddVertex(keyword, "association (?<name>) (?<type>)");

            IVertex association_association = association.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Association"), "(?<name>)");

            association_association.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            association_association.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Association"));

            // aggregation
            //
            // aggregation (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex aggregation2 = k.AddVertex(keyword, "aggregation (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");

            IVertex aggregation2_aggregation = aggregation2.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Aggregation"), "(?<name>)");

            aggregation2_aggregation.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            aggregation2_aggregation.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            aggregation2_aggregation.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            aggregation2_aggregation.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Aggregation"));

            aggregation2_aggregation.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsAggregation"), empty);

            // aggregation
            //
            // aggregation (?<name>) (?<type>)

            IVertex aggregation = k.AddVertex(keyword, "aggregation (?<name>) (?<type>)");

            IVertex aggregation_aggregation = aggregation.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Aggregation"), "(?<name>)");

            aggregation_aggregation.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$EdgeTarget"), "(?<type>)");

            aggregation_aggregation.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Aggregation"));

            aggregation_aggregation.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsAggregation"), empty);

            // method
            //
            // method (?<name>) (?<returnType>) [(*(+, +)(?<paramType>) (?<paramName>)*)]

            IVertex method = k.AddVertex(keyword, "method (?<name>) (?<returnType>)((*(+, +)(?<paramType>) (?<paramName>)*))");

            IVertex method_method = method.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Method"), "(?<name>)");

            method_method.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"), 
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Method"));

            method_method.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Method\Output"), "(?<returnType>)");

            IVertex mmip = method_method.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Method\InputParameter"), "(?<paramName>)");

            mmip.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$VertexTarget"), "(?<paramType>)");

            mmip.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                Empty);

            // method
            //
            // method (?<name>) [(*(+, +)(?<paramType>) (?<paramName>)*)]

            IVertex method2 = k.AddVertex(keyword, "method (?<name>) ((*(+, +)(?<paramType>) (?<paramName>)*))");

            IVertex method2_method = method2.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Method"), "(?<name>)");

            method2_method.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Method"));

            IVertex m2fip = method2_method.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Class\Method\InputParameter"), "(?<paramName>)");

            m2fip.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$VertexTarget"), "(?<paramType>)");

            m2fip.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                Empty);

            // function
            //
            // function (?<name>) (?<returnType>) [(*(+, +)(?<paramType>) (?<paramName>)*)]

            IVertex function = k.AddVertex(keyword, "function (?<name>) (?<returnType>)((*(+, +)(?<paramType>) (?<paramName>)*))");

            IVertex function_function = function.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Function"), "(?<name>)");

            function_function.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Function"));

            function_function.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Function\Output"), "(?<returnType>)");

            IVertex ffip = function_function.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Function\InputParameter"), "(?<paramName>)");

            ffip.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$VertexTarget"), "(?<paramType>)");

            ffip.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"), 
                Empty);

            // function
            //
            // function (?<name>) [(*(+, +)(?<paramType>) (?<paramName>)*)]

            IVertex function2 = k.AddVertex(keyword, "function (?<name>) ((*(+, +)(?<paramType>) (?<paramName>)*))");

            IVertex function2_function = function2.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Function"), "(?<name>)");

            function2_function.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Function"));

            IVertex f2fip = function2_function.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Function\InputParameter"), "(?<paramName>)");

            f2fip.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$VertexTarget"), "(?<paramType>)");

            f2fip.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                Empty);

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
            // <- SetLeftVertexesToFirstRightVertexValue
            // <+< AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex
            // <<< AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex 
            //
            ////////////////////////////////////////////////////////            

            // =
            //
            // (?<left>) = (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) =(?<SUB>) (?<right>)", "RedirectLeftEdgesToRightVertices");

            // +=
            //
            // (?<left>) += (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) +=(?<SUB>) (?<right>)", "AddLeftEdgesToRightVertices");

            // +<
            //
            // (?<left>) +< (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) +<(?<SUB>) (?<right>)", "AddRightEdgesIntoLeftEdges");

            // ~=
            //
            // (?<left>) ~= (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) ~=(?<SUB>) (?<right>)", "DeleteRightVertices");

            // -<
            //
            // (?<left>) -< (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) -<(?<SUB>) (?<right>)", "DeleteRightEdgesFromLeftEdges");

            // ~<
            //
            // (?<left>) ~< (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) ~<(?<SUB>) (?<right>)", "DeleteRightVerticesFromLeftEdges");

            // <-
            //
            // (?<left>) <- (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) <-(?<SUB>) (?<right>)", "SetLeftVertexesToFirstRightVertexValue");

            // <+<
            //
            // (?<left>) <+< (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) <+<(?<SUB>) (?<right>)", "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex");

            // <<<
            //
            // (?<left>) <<< (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) <<<(?<SUB>) (?<right>)", "AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex");

            /////////////////////////////////////////////////////////
            //
            // edge set operators
            //
            ////////////////////////////////////////////////////////

            // <+>
            //
            // (?<left>) <+> (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) <+>(?<SUB>) (?<right>)", "EdgeSetAdd");

            // <->
            //
            // (?<left>) <-> (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) <->(?<SUB>) (?<right>)", "EdgeSetSubstract");

            // <<X>>
            //
            // <<(?<expr>)>>
            IVertex o_index = k.AddVertex(keyword, "<<(?<expr>)>>");

            o_index.AddEdge(keywordGroup, kgd_SlashMarkIndexMethod);

            o_index.AddEdge(keywordGroup, kgd_SlashMarkIndexMethodInner2);

            o_index.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            o_index.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_index.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            IVertex o_index_any = o_index.AddVertex(any, "");

            o_index_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_index_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "SetIndex"));

             o_index_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression"), "(?<expr>)");           

            IVertex o_index_any_targetExpr = o_index_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_index_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndexMethod);

            // <>
            //
            // <>

            IVertex o_setCount = k.AddVertex(keyword, "<>");

            o_setCount.AddEdge(keywordGroup, kgd_SlashMarkIndexMethod);

            o_setCount.AddEdge(keywordGroup, kgd_SlashMarkIndexMethodInner2);

            o_setCount.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            o_setCount.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_setCount.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            IVertex o_setCount_any = o_setCount.AddVertex(any, "");

            o_setCount_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_setCount_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"), 
                LegacySystem.Graph.EasyVertex.Get(smu, false, "SetCount"));

            // this is error. should be out
            //IVertex o_setCount_any_targetExpr = o_index_any.AddVertex(smu.Get(false, @"ExpressionAtom\NextExpression"), "");

            //o_setCount_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndexMethod);

            // ~00
            //
            // ~00

            IVertex o_emptySet = k.AddVertex(keyword, "~00");

            IVertex o_emptySet_any = o_emptySet.AddVertex(any, "");

            o_emptySet_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "EmptySet"));

            

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

            o_plus_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "+"));

            o_plus_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_plus_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right>)");

            // -
            //
            // (?<left>) - (?<right>)

            IVertex o_minus = k.AddVertex(keyword, "(?<left>) -(?<SUB>) (?<right>)");

            IVertex o_minus_any = o_minus.AddVertex(any, "");

            o_minus_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "-"));

            o_minus_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_minus_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right>)");

            // *
            //
            // (?<left>) * (?<right>)

            IVertex o_mul = k.AddVertex(keyword, "(?<left>) *(?<SUB>) (?<right>)");

            IVertex o_mul_any = o_mul.AddVertex(any, "");

            o_mul_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Mul"));            

            o_mul_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_mul_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right>)");

            // /
            //
            // (?<left>) / (?<right>)

            IVertex o_div = k.AddVertex(keyword, "(?<left>) /(?<SUB>) (?<right>)");

            IVertex o_div_any = o_div.AddVertex(any, "");

            o_div_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "/"));

            o_div_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_div_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right>)");

            /////////////////////////////////////////////////////////
            //
            // logic operators
            //
            ////////////////////////////////////////////////////////

            // ==
            //
            // (?<left>) == (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) ==(?<SUB>) (?<right>)", "Equal");

            // ===
            //
            // (?<left>) === (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) ===(?<SUB>) (?<right>)", "ExactEqual");

            // ====
            //
            // (?<left>) ==== (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) ====(?<SUB>) (?<right>)", "VertexEqual");

            // !=
            //
            // (?<left>) != (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) !=(?<SUB>) (?<right>)", "NotEqual");

            // !
            //
            //  !(?<expr>)

            AddSingleOperator(k, smu, smb, keyword, any, "!(?<expr>)", "Negation");

            // &
            //
            // (?<left>) & (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) &(?<SUB>) (?<right>)", "And");

            // |
            //
            // (?<left>) | (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) |(?<SUB>) (?<right>)", "Or");

            // >
            //
            // (?<left>) > (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) >(?<SUB>) (?<right>)", "MoreThan");

            // <
            //
            // (?<left>) < (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) <(?<SUB>) (?<right>)", "LessThan");

            // >=
            //
            // (?<left>) >= (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) >=(?<SUB>) (?<right>)", "MoreOrEqualThan");

            // <=
            //
            // (?<left>) <= (?<right>)

            AddDoubleOperator(k, smu, smb, keyword, any, "(?<left>) <=(?<SUB>) (?<right>)", "LessOrEqualThan");


            ////////////////////////////////////////////////////////////////
            //
            // vertex creation operators
            //
            ////////////////////////////////////////////////////////////////

            // !!!!!!!!!!!!!!!!!!!! D O U B L E C O L O N

            // :: /1
            //
            // (?<left_ColonEmptyNew>)||(?<SUB>)(?<right_ColonEmptyNew>)                         

            IVertex o_doubleColon = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>) :: (?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            //IVertex o_doubleColon = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLink>)::(?<SUB>)(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLink>)");

            o_doubleColon.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleColon_any = o_doubleColon.AddVertex(any, "");

            o_doubleColon_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleColon"));

            o_doubleColon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleColon_any_right = o_doubleColon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleColon_any_targetExpr = o_doubleColon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_doubleColon_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_InnerCreation);


            // :: /2
            //
            // ||(?<SUB>)(?<right_ColonEmptyNew>)                         

            IVertex o_doubleColon2 = k.AddVertex(keyword, ":: (?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            //IVertex o_doubleColon2 = k.AddVertex(keyword, "::(?<SUB>)(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLink>)");

            o_doubleColon2.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleColon2_any = o_doubleColon2.AddVertex(any, "");

            o_doubleColon2_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleColon"));

            IVertex o_doubleColon2_any_right = o_doubleColon2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleColon2_any_targetExpr = o_doubleColon2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_doubleColon2_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_InnerCreation);

            // :: /3
            //
            // (?<left_ColonEmptyNew>)||(?<SUB>)                         

            IVertex o_doubleColon3 = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>) ::");

            // Vertex o_doubleColon3 = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLink>)::(?<SUB>)");

            o_doubleColon3.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleColon3_any = o_doubleColon3.AddVertex(any, "");

            o_doubleColon3_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleColon"));

            o_doubleColon3_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleColon3_any_targetExpr = o_doubleColon3_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_doubleColon3_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_InnerCreation);

            // !!!!!!!!!!!!!!!!!!!! D O U B L E S E M I C O L O N

            // ;; /1
            //
            // (?<left_ColonEmptyNew>);;(?<SUB>)(?<right_ColonEmptyNew>)                         

            IVertex o_doubleSemicolon = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>) ;; (?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            //IVertex o_doubleSemicolon = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLink>);;(?<SUB>)(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLink>)");

            o_doubleSemicolon.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleSemicolon_any = o_doubleSemicolon.AddVertex(any, "");

            o_doubleSemicolon_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleSemicolon"));

            o_doubleSemicolon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleSemicolon_any_right = o_doubleSemicolon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleSemicolon_any_targetExpr = o_doubleSemicolon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_doubleSemicolon_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_InnerCreation);


            // ;; /2
            //
            // ;;(?<SUB>)(?<right_ColonEmptyNew>)                         

            IVertex o_doubleSemicolon2 = k.AddVertex(keyword, ";; (?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            //IVertex o_doubleSemicolon2 = k.AddVertex(keyword, ";;(?<SUB>)(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLink>)");

            o_doubleSemicolon2.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleSemicolon2_any = o_doubleSemicolon2.AddVertex(any, "");

            o_doubleSemicolon2_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleSemicolon"));

            IVertex o_doubleSemicolon2_any_right = o_doubleSemicolon2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleSemicolon2_any_targetExpr = o_doubleSemicolon2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_doubleSemicolon2_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_InnerCreation);

            // ;; /3
            //
            // (?<left_ColonEmptyNew>);;(?<SUB>)                         

            IVertex o_doubleSemicolon3 = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>) ;;");

            // Vertex o_doubleSemicolon3 = k.AddVertex(keyword, "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLink>);;(?<SUB>)");

            o_doubleSemicolon3.AddVertex(nonSelfRecursiveParameters, "");

            IVertex o_doubleSemicolon3_any = o_doubleSemicolon3.AddVertex(any, "");

            o_doubleSemicolon3_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "DoubleSemicolon"));

            o_doubleSemicolon3_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)");

            IVertex o_doubleSemicolon3_any_targetExpr = o_doubleSemicolon3_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_doubleSemicolon3_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_InnerCreation);

            // !!!!!!!!!!!!!!!!!!!! C O L O N

            // : /1
            //
            // (?<left_Empty2>)|(?<SUB>)(?<right_Empty2>)            

            IVertex o_colon = k.AddVertex(keyword, "(?<left_Empty2Inner>):(?<SUB>)(?<right_Empty2Inner>)");

            o_colon.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_colon.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            o_colon.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            IVertex o_colon_any = o_colon.AddVertex(any, "");

            o_colon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_colon_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Colon"));

            o_colon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left_Empty2Inner>)");

            IVertex o_colon_any_right = o_colon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right_Empty2Inner>)");

            IVertex o_colon_any_targetExpr = o_colon_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_colon_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_SlashMarkIndexMethod);

            // : /2
            //
            // |(?<SUB>)(?<right_Empty2>)            

            IVertex o_colon2 = k.AddVertex(keyword, ":(?<SUB>)(?<right_Empty2Inner>)");

            o_colon2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_colon2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            o_colon2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            IVertex o_colon2_any = o_colon2.AddVertex(any, "");

            o_colon2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_colon2_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Colon"));

            IVertex o_colon2_any_right = o_colon2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right_Empty2Inner>)");

            IVertex o_colon2_any_targetExpr = o_colon2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_colon2_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_SlashMarkIndexMethod);

            // : /3
            //
            // (?<left_Empty2>)|(?<SUB>)            

            IVertex o_colon3 = k.AddVertex(keyword, "(?<left_Empty2Inner>):(?<SUB>)");            

            o_colon3.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_colon3.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            o_colon3.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            IVertex o_colon3_any = o_colon3.AddVertex(any, "");

            o_colon3_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_colon3_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Colon"));

            o_colon3_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left_Empty2Inner>)");            

            IVertex o_colon3_any_targetExpr = o_colon3_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_colon3_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_SlashMarkIndexMethod);


            // _
            //
            //  _(?<expr>)

            AddSingleOperator(k, smu, smb, keyword, any, "_(?<expr>)", "CopySet");

            LegacySystem.Graph.EasyVertex.Get(k, false, "_(?<expr>)").AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            // `
            //
            //  `(?<expr>)

            AddSingleOperator(k, smu, smb, keyword, any, "`(?<expr>)", "MetaToTo");

            //////////////////// common

            // []
            //
            // [(*(+, +) (?<expr>)*)]

            IVertex o_call = k.AddVertex(keyword, "(?<target_ColonEmptyInner2SlashMarkIndexMethodNewLink>)[(*(+, +)(?<expr>)*)]");

              IVertex o_call_any = o_call.AddVertex(any, "");

              o_call_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                  LegacySystem.Graph.EasyVertex.Get(smu, false, "FunctionCall"));

              IVertex o_call_any_target = o_call_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"FunctionCall\Target"), "(?<target_ColonEmptyInner2SlashMarkIndexMethodNewLink>)");

              IVertex o_call_any_param = o_call_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator\Expression"), "(?<expr>)");
           
              o_call_any_param.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                  Empty);

            // return
            //
            // return (?<expr>)

            AddSingleOperator(k, smu, smb, keyword, any, "return (?<expr>)", "Return");

            // return
            //
            // return (?<expr>)

            AddKeyword(k, smu, smb, keyword, any, "return", "Return");

            // foreach
            //
            // foreach (?<var>) in (?<set>)

            IVertex o_foreach = k.AddVertex(keyword, "foreach (?<var>) in (?<set>)");

            IVertex o_foreach_any = o_foreach.AddVertex(any, "");

            o_foreach_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "ForEach"));

            o_foreach_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ForEach\Variable"), "(?<var>)");

            o_foreach_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ForEach\Set"), "(?<set>)");

            // while
            //
            // while (?<test>)

            IVertex o_while = k.AddVertex(keyword, "while (?<test>)");

            IVertex o_while_any = o_while.AddVertex(any, "");

            o_while_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "While"));

            o_while_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"While\Test"), "(?<test>)");

            // if
            //
            // if (?<test>)

            IVertex o_if = k.AddVertex(keyword, "if (?<test>)");

            IVertex o_if_any = o_if.AddVertex(any, "");

            o_if_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "If"));

            o_if_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"If\Test"), "(?<test>)");

            // test
            //
            // test (?<test>)

            IVertex o_test = k.AddVertex(keyword, "test (?<expr>)");

            IVertex o_test_any = o_test.AddVertex(any, "");

            o_test_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Test"));

            o_test_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Test\Expression"), "(?<expr>)");

            // case
            //
            // case (?<test>)

            IVertex o_case = k.AddVertex(keyword, "case (?<test>)");

            IVertex o_case_any = o_case.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, "Case"), "");

            o_case_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Case"));

            o_case_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Case\Test"), "(?<test>)");

            // fallback
            //
            // fallback

            IVertex o_default = k.AddVertex(keyword, "fallback");

            IVertex o_default_any = o_default.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, "Fallback"), "");

            o_default_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Fallback"));
            

            // ()
            //
            // ((?<expr>))

            AddSingleOperator(k, smu, smb, keyword, any, "((?<expr>))", "()");

            LegacySystem.Graph.EasyVertex.Get(k, false, "((?<expr>))").AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);
            
            // \
            //
            // \                         

            IVertex o_Slash = k.AddVertex(keyword, @"\");

            o_Slash.AddEdge(keywordGroup, kgd_SlashMarkIndexMethod);

            o_Slash.AddEdge(keywordGroup, kgd_SlashMarkIndexMethodInner2);

            o_Slash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            o_Slash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_Slash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            IVertex o_Slash_any = o_Slash.AddVertex(any, "");

            o_Slash_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_Slash_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "\"\\ \""));

            IVertex o_Slash_any_targetExpr = o_Slash_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_Slash_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndexMethod);

            // #
            //
            // #                         

            IVertex o_InSlash = k.AddVertex(keyword, @"#");

            o_InSlash.AddEdge(keywordGroup, kgd_SlashMarkIndexMethod);

            o_InSlash.AddEdge(keywordGroup, kgd_SlashMarkIndexMethodInner2);

            o_InSlash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            o_InSlash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_InSlash.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            IVertex o_InSlash_any = o_InSlash.AddVertex(any, "");

            o_InSlash_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_InSlash_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "InEdgesSlash"));

            IVertex o_InSlash_any_targetExpr = o_InSlash_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_InSlash_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndexMethod);

            // ?
            //
            // ?                         

            IVertex o_Mark = k.AddVertex(keyword, @" ? ");

            o_Mark.AddEdge(keywordGroup, kgd_SlashMarkIndexMethod);

            o_Mark.AddEdge(keywordGroup, kgd_SlashMarkIndexMethodInner2);

            o_Mark.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            o_Mark.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_Mark.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            IVertex o_Mark_any = o_Mark.AddVertex(any, "");

            o_Mark_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_Mark_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "?"));

            IVertex o_Mark_any_targetExpr = o_Mark_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_Mark_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_ColonEmptyInner2SlashMarkIndexMethod);

            // {
            // }
            //
            // {(*\r\n\t(?<expr>)*)\r\n}

            IVertex o_InnerCreation = k.AddVertex(keyword, "{(*\r\n\t(?<expr>)*)\r\n}");

            o_InnerCreation.AddEdge(keywordGroup, kgd_InnerCreation);

            IVertex o_InnerCreation_any = o_InnerCreation.AddVertex(any, anyString);

            o_InnerCreation_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_InnerCreation_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "InnerCreation"));

            IVertex o_InnerCreation_any_param = o_InnerCreation_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator\Expression"), "(?<expr>)");

            o_InnerCreation_any_param.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                Empty);

            // {} // 2       
            //
            // {(*(+,+)(?<expr>)*)}

            IVertex o_Inner2 = k.AddVertex(keyword, "{(*(+,+)(?<expr>)*)}");

            o_Inner2.AddEdge(keywordGroup, kgd_SlashMarkIndexMethodInner2);

            o_Inner2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_Inner2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            o_Inner2.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            IVertex o_Inner2_any = o_Inner2.AddVertex(any, anyString);

            o_Inner2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_Inner2_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "\"{}\""));

            IVertex o_Inner2_any_any_param = o_Inner2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator\Expression"), "(?<expr>)");

            o_Inner2_any_any_param.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                Empty);

            IVertex o_Inner2_any_targetExpr = o_Inner2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            o_Inner2_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_SlashMarkIndexMethod);


            // {} // 1
            //
            // {(*(+,+)(?<expr>)*)}

            IVertex o_Inner = k.AddVertex(keyword, "{(*(+,+)(?<expr1>)*)}");

            o_Inner.AddEdge(keywordGroup, kgd_Inner);
            o_Inner.AddEdge(keywordGroup, kgd_Empty2Inner);

            IVertex o_Inner_any = o_Inner.AddVertex(any, anyString);

            o_Inner_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_Inner_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"), LegacySystem.Graph.EasyVertex.Get(smu, false, "\"{}\""));

            IVertex o_Inner_any_any_param = o_Inner_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator\Expression"), "(?<expr1>)");

            o_Inner_any_any_param.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                Empty);
                

            // ""
            //
            // "\"(?<value>)\""

            IVertex newValueKeyword = k.AddVertex(keyword, "\"(?<value>)\"");

            newValueKeyword.AddVertex(newVertexKeyword, "");

            newValueKeyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            newValueKeyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            IVertex newValueKeyword_any = newValueKeyword.AddVertex(any, "(?<value>)");

            // IVertex newValueKeyword_any_targetExpr = newValueKeyword_any.AddVertex(smu.Get(false, @"ExpressionAtom\NextExpression"), "");

            // newValueKeyword_any_targetExpr.AddEdge(smb.Get(false, "$$LocalRoot"), kgd_InnerCreation);

            
            // E M P T Y :) K E Y W O R D 1
            //
            //

            IVertex empty1Keyword = k.AddVertex(keyword, "(?<value>)");

            empty1Keyword.AddVertex(emptyKeyword, "");

            empty1Keyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            empty1Keyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            empty1Keyword.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            IVertex empty1Keyword_any = empty1Keyword.AddVertex(any, "(?<value>)");

            empty1Keyword_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            empty1Keyword_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Query"));

            IVertex empty1Keyword_any_targetExpr = empty1Keyword_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            empty1Keyword_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_SlashMarkIndexMethodInner2);
            
            // E M P T Y :) K E Y W O R D 2
            //
            //

            IVertex empty2Keyword = k.AddVertex(keyword, "(?<value>)");

            empty2Keyword.AddVertex(emptyKeyword, "");

            empty2Keyword.AddEdge(keywordGroup, kgd_Empty2Inner);

            IVertex empty2Keyword_any = empty2Keyword.AddVertex(any, "(?<value>)");

            empty2Keyword_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "Query"));

            IVertex empty2Keyword_any_targetExpr = empty2Keyword_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ExpressionAtom\NextExpression"), "");

            empty2Keyword_any_targetExpr.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$LocalRoot"), kgd_Inner);

            // @
            //
            // @(?<value>)

            IVertex at = k.AddVertex(keyword, "@(?<value>)");

            at.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            at.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            at.AddVertex(linkKeyword, "");

            IVertex at_at = at.AddVertex(any, "");

            at_at.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Link"));

            at_at.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"Link\Target"), "(?<value>)");

            //////////////////// meta

            // execute
            //
            // execute((?<expr>))

            AddSingleOperator(k, smu, smb, keyword, any, "execute((?<expr>))", "Execute");

            // parse
            //
            // parse((?<expr>))

            IVertex o_parse = k.AddVertex(keyword, "parse((?<expr>))");

            IVertex o_parse_any = o_parse.AddVertex(any, "");

            o_parse_any.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Parse"));

            o_parse_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression"), "(?<expr>)");

            // parse
            //
            // parse (?<language>) ((?<expr>))

            IVertex o_parse2 = k.AddVertex(keyword, "parse (?<language>) ((?<expr>))");

            IVertex o_parse2_any = o_parse2.AddVertex(any, "");

            o_parse2_any.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"ParseWithLanguage"));

            o_parse2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"ParseWithLanguage\FormalTextLanguage"), "(?<language>)");

            o_parse2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression"), "(?<expr>)");            

            // generate
            //
            // generate((?<expr>))

            IVertex o_generate = k.AddVertex(keyword, "generate((?<expr>))");

            IVertex o_generate_any = o_generate.AddVertex(any, "");

            o_generate_any.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"Generate"));

            o_generate_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression"), "(?<expr>)");

            // generate
            //
            // generate (?<language>) ((?<expr>))

            IVertex o_generate2 = k.AddVertex(keyword, "generate (?<language>) ((?<expr>))");

            IVertex o_generate2_any = o_generate2.AddVertex(any, "");

            o_generate2_any.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"GenerateWithLanguage"));

            o_generate2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression"), "(?<expr>)");

            o_generate2_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"GenerateWithLanguage\FormalTextLanguage"), "(?<language>)");


            //////////////////// oo

            // new 
            //
            // new (?<expr>))

            IVertex o_new = k.AddVertex(keyword, "new (?<expr>)");

            IVertex o_new_any = o_new.AddVertex(any, "");

            o_new_any.AddEdge(_is, LegacySystem.Graph.EasyVertex.Get(smu, false, @"New"));

            o_new_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression"), "(?<expr>)");


            // .[]
            //
            // [(*(+, +) (?<expr>)*)]

            IVertex o_methodCall = k.AddVertex(keyword, ".(?<target_ColonEmptyInner2SlashMarkIndexMethodNewLink>)[(*(+, +)(?<expr>)*)]");

            o_methodCall.AddEdge(keywordGroup, kgd_SlashMarkIndexMethod);

            o_methodCall.AddEdge(keywordGroup, kgd_SlashMarkIndexMethodInner2);

            o_methodCall.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethod);

            o_methodCall.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLink);

            o_methodCall.AddEdge(keywordGroup, kgd_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy);

            IVertex o_methodCall_any = o_methodCall.AddVertex(any, "");

            o_methodCall_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smb, false, "$$StartInLocalRoot"), "");

            o_methodCall_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, "MethodCall"));

            IVertex o_methodCall_any_target = o_methodCall_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"MethodCall\Target"), "(?<target_ColonEmptyInner2SlashMarkIndexMethodNewLink>)");

            IVertex o_methodCall_any_param = o_methodCall_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"MultiOperator\Expression"), "(?<expr>)");

            o_methodCall_any_param.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"$$KeywordManyRoot"),
                Empty);
                
        }

        private static void AddDoubleOperator(IVertex k, IVertex smu, IVertex smb, IVertex keyword, IVertex any, string text, string _is)
        {
            IVertex o_copy = k.AddVertex(keyword, text);

            IVertex o_copy_any = o_copy.AddVertex(any, "");

            o_copy_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, _is));

            o_copy_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\LeftExpression"), "(?<left>)");

            o_copy_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"DoubleOperator\RightExpression"), "(?<right>)");
        }

        private static void AddSingleOperator(IVertex k, IVertex smu, IVertex smb, IVertex keyword, IVertex any, string text, string _is)
        {
            IVertex o_copy = k.AddVertex(keyword, text);

            IVertex o_copy_any = o_copy.AddVertex(any, "");

            o_copy_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, _is));

            o_copy_any.AddVertex(LegacySystem.Graph.EasyVertex.Get(smu, false, @"SingleOperator\Expression"), "(?<expr>)");
        }

        private static void AddKeyword(IVertex k, IVertex smu, IVertex smb, IVertex keyword, IVertex any,  string text, string _is)
        {
            IVertex o_copy = k.AddVertex(keyword, text);

            IVertex o_copy_any = o_copy.AddVertex(any, "");

            o_copy_any.AddEdge(LegacySystem.Graph.EasyVertex.Get(smb, false, @"Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(smu, false, _is));
        }

        public void DefaultFormalTextLanguageVertexSetup()
        {
            IVertex System = GraphUtil.GetQueryOutFirst(Root, null, "System");
            IVertex FormalTextLanguage = GraphUtil.GetQueryOutFirst(System, null, "FormalTextLanguage");

            _DefaultFormalTextLanguage = GraphUtil.GetQueryOutFirst(FormalTextLanguage, null, "ZeroCode");

            MinusZero.Instance._DefaultFormalTextLanguage = DefaultFormalTextLanguage;
        }

        void CreateSystemFormalTextLanguageZeroCodeBase()
        {
            IVertex ftl = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\FormalTextLanguage");

            IVertex zcb = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\FormalTextLanguage").AddVertex(ftl, "ZeroCodeBase");

            zcb.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\FormalTextLanguage"));

            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "CRLFoperator"), @"{");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "MetaSeparator"), ":");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "CodeGraphVertexPrefix"), "<");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "CodeGraphVertexSuffix"), ">");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "LineContinuationPrefix"), "^");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "CodeGraphLinkPrefix"), "@");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "CodeGraphLinkKeywordPrefix"), "@@");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "NewVertexPrefix"), "\"");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "NewVertexSuffix"), "\"");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "EscapedSequencePrefix"), "\'");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "EscapedSequenceSuffix"), "\'");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "EscapeCharacter"), "%");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "SetIndexPrefix"), "<<");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "SetIndexPostfix"), ">>");
            zcb.AddVertex(LegacySystem.Graph.EasyVertex.Get(ftl, false, "QuerySlash"), "\\");
            zcb.AddEdge(LegacySystem.Graph.EasyVertex.Get(ftl, false, "NextAtomEdge"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\NextOut\Next"));

            IVertex CodeViewTimeLinkKeywordPart = LegacySystem.Graph.EasyVertex.Get(ftl, false, "CodeViewTimeLinkKeywordPart");

            zcb.AddVertex(CodeViewTimeLinkKeywordPart, "\\");
            zcb.AddVertex(CodeViewTimeLinkKeywordPart, "{");
            zcb.AddVertex(CodeViewTimeLinkKeywordPart, "}");
            zcb.AddVertex(CodeViewTimeLinkKeywordPart, ":");
            zcb.AddVertex(CodeViewTimeLinkKeywordPart, "::");
            zcb.AddVertex(CodeViewTimeLinkKeywordPart, "<<");
            zcb.AddVertex(CodeViewTimeLinkKeywordPart, ">>");
            zcb.AddVertex(CodeViewTimeLinkKeywordPart, ",");

        }
        
        public void DecorateWithDotNetStaticMethod(IVertex baseVertex, string _typeName, string _methodName)
        {            
            baseVertex.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod"));

            baseVertex.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod\DotNetTypeName"), _typeName);

            baseVertex.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\DotNetStaticMethod\DotNetMethodName"), _methodName);
        }

        void CreateSystemFormalTextLanguageZeroCode()
        {
            IVertex zc = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\FormalTextLanguage").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\FormalTextLanguage"),"ZeroCode");

            DefaultFormalTextLanguageVertexSetup();

            zc.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\FormalTextLanguage\ZeroCodeBase"));


            IVertex b = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base");

            IVertex di = zc.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\FormalTextLanguage\DefaultImports"), "");            

            //

            IVertex DirectMeta = VertexOperations.AddInstance(b, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\$ImportMeta"));

            DirectMeta.Value = "$ImportDirectMeta";

            DirectMeta.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsLink"), Empty);

            di.AddEdge(DirectMeta, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML"));

            di.AddEdge(DirectMeta, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base"));

            di.AddEdge(DirectMeta, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex"));

            di.AddEdge(DirectMeta, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes"));

            di.AddEdge(DirectMeta, Root); // ROOT

            //

            IVertex Direct = VertexOperations.AddInstance(b, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\$Import"));

            Direct.Value = "$ImportDirect";

            Direct.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsLink"), Empty);

            //di.AddEdge(Direct, Root); // :O) now its hanging XXX

            //

            IVertex System = VertexOperations.AddInstance(di, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\$ImportMeta"));

            System.Value = "System";

            System.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsLink"), Empty);

            di.AddEdge(System, LegacySystem.Graph.EasyVertex.Get(Root, false, @"System"));

            //   ZeroCodeViewGraphChange         

            IVertex view = zc.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\FormalTextLanguage\FormalTextLanguageView"), 
                "ZeroCodeView");

            IVertex graphChangeTrigger = view.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$GraphChangeTrigger"),
                "ZeroCodeViewGraphChange");            

            graphChangeTrigger.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Is"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger"));

            graphChangeTrigger.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\ChangeTypeFilter"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OnlyNonTransactedRootVertexEvents"));

            graphChangeTrigger.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\ChangeTypeFilter"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\MetaEdgeAdded"));

            graphChangeTrigger.AddEdge(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\ChangeTypeFilter"),
                LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\MetaEdgeRemoved"));
            
            IVertex listener = graphChangeTrigger.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeTrigger\Listener"),
                "ZeroCodeViewListener");

            DecorateWithDotNetStaticMethod(listener,
                "m0.ZeroCode.ZeroCodeView, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                "ZeroCodeViewListener");

        }

        void AddAttribute(IVertex baseVertex, string name, int MinCardinality, int MaxCardinality, IVertex edgeTarget)
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            IVertex atr = baseVertex.AddVertex(LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class\Attribute"), name);

            atr.AddVertex(LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$MinCardinality"), MinCardinality);

            atr.AddVertex(LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$MaxCardinality"), MaxCardinality);

            atr.AddEdge(LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"), edgeTarget);
        }

        void AddAssociation(IVertex baseVertex, string name, int MinCardinality, int MaxCardinality, IVertex edgeTarget)
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            IVertex atr = baseVertex.AddVertex(LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class\Association"), name);

            atr.AddVertex(LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$MinCardinality"), MinCardinality);

            atr.AddVertex(LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex\$MaxCardinality"), MaxCardinality);

            atr.AddEdge(LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"), edgeTarget);
        }

        void CreateSystemMetaZeroTypes()
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");


            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, sm, "{ZeroTypes{AtomType:String,AtomType:Integer,AtomType:Decimal,AtomType:Float,AtomType:Boolean,Type:VertexType,Class:Edge{Association:From{$MinCardinality:0,$MaxCardinality:1},Association:Meta{$MinCardinality:1,$MaxCardinality:1},Association:To{$MinCardinality:1,$MaxCardinality:1}},Class:DateTime{Attribute:Year{$MinCardinality:1,$MaxCardinality:1},Attribute:Month{$MinCardinality:1,$MaxCardinality:1},Attribute:Day{$MinCardinality:1,$MaxCardinality:1},Attribute:Hour{$MinCardinality:1,$MaxCardinality:1},Attribute:Minute{$MinCardinality:1,$MaxCardinality:1},Attribute:Second{$MinCardinality:1,$MaxCardinality:1},Attribute:Millisecond{$MinCardinality:0,$MaxCardinality:1}},Class:FormalTextLanguage,Enum:EnumBase,Class:HasBaseEdge{Attribute:BaseEdge{$MinCardinality:1,$MaxCardinality:1}},Class:HasSelectedEdges{Attribute:SelectedEdges{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:},Attribute:ShowSelectedEdgesBaseEdge{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:}},Class:HasFilter{Attribute:FilterQuery{$MinCardinality:0,$MaxCardinality:1}},Class:HasExecutableVertex{Attribute:ExecutableVertex{$MinCardinality:1,$MaxCardinality:1}},Class:Exception{Attribute:Where{$MinCardinality:0,$MaxCardinality:1},Attribute:Type{$MinCardinality:0,$MaxCardinality:1},Attribute:What{$MinCardinality:1,$MaxCardinality:1}},Enum:ExceptionTypeEnum{EnumValue:Error,EnumValue:Warning,EnumValue:Info}}}");

            IVertex FormalTextLanguage = LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\FormalTextLanguage");

            IVertex vertexType = LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\VertexType");

            AddAssociation(FormalTextLanguage, "DefaultImports", 0, 1, vertexType);
            AddAssociation(FormalTextLanguage, "Keywords", 0, 1, vertexType);

            AddAttribute(FormalTextLanguage, "CRLFoperator", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "MetaSeparator", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "CodeGraphVertexPrefix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "CodeGraphVertexSuffix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "LineContinuationPrefix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "CodeGraphLinkPrefix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "CodeGraphLinkKeywordPrefix", 1, 1, vertexType); 
            AddAttribute(FormalTextLanguage, "NewVertexPrefix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "NewVertexSuffix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "EscapedSequencePrefix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "EscapedSequenceSuffix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "EscapeCharacter", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "SetIndexPrefix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "SetIndexPostfix", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "QuerySlash", 1, 1, vertexType);
            AddAttribute(FormalTextLanguage, "NextAtomEdge", 1, 1, vertexType);

            AddAttribute(FormalTextLanguage, "CodeViewTimeLinkKeywordPart", 0, -1, vertexType);

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\String").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Decimal").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Float").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Boolean").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\AtomType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\VertexType").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Type"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Edge").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\EnumBase").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Enum"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasBaseEdge").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasSelectedEdges").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasExecutableVertex").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasFilter").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));                        
            
            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\EnumBase").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"Base\Vertex"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime\Year").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime\Month").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime\Day").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime\Hour").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime\Minute").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime\Second").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\DateTime\Millisecond").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Edge\From").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\VertexType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Edge\Meta").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\VertexType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Edge\To").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\VertexType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasBaseEdge\BaseEdge").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Edge"));
            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasBaseEdge\BaseEdge").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Section"), "Base");

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasSelectedEdges\SelectedEdges").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\VertexType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasSelectedEdges\ShowSelectedEdgesBaseEdge").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Boolean"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasFilter\FilterQuery").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\String"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasExecutableVertex\ExecutableVertex").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\VertexType"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\ExceptionTypeEnum").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\EnumBase"));

            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Exception\Where").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\String"));
            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Exception\Type").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\ExceptionTypeEnum"));
            LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Exception\What").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"), 
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\String"));
        }

        void CreateSystemMetaZeroTypes_UX()
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            IVertex smz = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes");


            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(smz, sm, "{UX{"
                +"Class:Color{Attribute:Red{MinValue:0,MaxValue:255,$DefaultValue:0,$MinCardinality:1,$MaxCardinality:1},Attribute:Green{MinValue:0,MaxValue:255,$DefaultValue:0,$MinCardinality:1,$MaxCardinality:1},Attribute:Blue{MinValue:0,MaxValue:255,$DefaultValue:0,$MinCardinality:1,$MaxCardinality:1},Attribute:Opacity{MinValue:0,MaxValue:255,$MinCardinality:0,$MaxCardinality:1}}"
                +",Class:HasColor{Attribute:Color{$MinCardinality:0,$MaxCardinality:1}}"
                + ",Class:Item{Aggregation:Item{$MinCardinality:0,$MaxCardinality:-1}}"
                + ",Class:UXItem{Attribute:Scale{$MinCardinality:1,$MaxCardinality:1,$DisplayLarger:,$DefaultValue:100},Attribute:DesignMode{$MinCardinality:0,$MaxCardinality:1},Attribute:Size{$MinCardinality:0,$MaxCardinality:1},Attribute:Position{$MinCardinality:0,$MaxCardinality:1},Attribute:Layout{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BorderColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BorderSize{MinValue:0,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:Gap{$MinCardinality:0,$MaxCardinality:1},Aggregation:Decorator{$MinCardinality:0,$MaxCardinality:-1},Association:UXTemplate{$MinCardinality:0,$MaxCardinality:1}}"
                + ",Class:UXContainer{Attribute:IsExpanded{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:CollapsedSize{$MinCardinality:0,$MaxCardinality:1},Attribute:ExpandedSize{$MinCardinality:0,$MaxCardinality:1},Association:NewItemUXTemplate{$MinCardinality:0,$MaxCardinality:1}}"
                + ",Class:Size{Attribute:Width,Attribute:Height}"
                + ",Class:Position{Attribute:X,Attribute:Y}"
                + ",Enum:LayoutTypeEnum{EnumValue:Vertical,EnumValue:Horizontal,EnumValue:Wrap,EnumValue:Manual,EnumValue:Auto,EnumValue:Hidden}"
                +"}}");
            
            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Color").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"), LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Color\Red").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));
            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Color\Green").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));
            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Color\Blue").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));
            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Color\Opacity").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\HasColor").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Is"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroUML\Class"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\HasColor\Color").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Color"));

            // Item

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Item").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasBaseEdge"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Item\Item").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Item"));

            // UXItem

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Item"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Scale").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Integer"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Scale").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*MinValue"), 0);

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Scale").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*MaxValue"), 200);

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Size").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Size"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\DesignMode").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Boolean"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Position").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Position"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Layout").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\LayoutTypeEnum"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\BackgroundColor").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Color"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\ForegroundColor").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Color"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\BorderColor").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Color"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\BorderSize").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Float"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Gap").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Float"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXItem\Decorator").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\UXItem"));

            // UXContainer

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXContainer").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\UXItem"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXContainer").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\HasSelectedEdges"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXContainer\IsExpanded").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Boolean"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXContainer\CollapsedSize").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Size"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\UXContainer\ExpandedSize").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\UX\Size"));            

            // Size

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Size\Width").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Float"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Size\Height").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Float"));


            // Position

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Position\X").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Float"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\Position\Y").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\Float"));

            // LayoutTypeEnum

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"UX\LayoutTypeEnum").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(sm, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(sm, false, @"ZeroTypes\EnumBase"));
        }

        void CreateSystemMetaZeroTypesExecutionFlow_Part1()
        {
            IVertex sm = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta");

            IVertex smuml = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML");

            IVertex smz = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes");

            IVertex smze = smz.AddVertex(LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Package"),
                "ExecutionFlow");


            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(smze, sm, "{Enum:GraphChangeEnum{EnumValue:ValueChange,EnumValue:OutputEdgeAdded,EnumValue:OutputEdgeRemoved,EnumValue:InputEdgeAdded,EnumValue:InputEdgeRemoved,EnumValue:MetaEdgeRemoved,EnumValue:MetaEdgeAdded,EnumValue:OutputEdgeDisposed}"
                + ",Enum:GraphChangeFilterEnum{EnumValue:FilterOutRootVertexEvents,EnumValue:OnlyNonTransactedRootVertexEvents}"
                + ",Class:EventTrigger{Association:Listener{$MinCardinality:0,$MaxCardinality:-1}}"
                + ",Class:Event{Association:Trigger{$MinCardinality:1,$MaxCardinality:1},Association:Source{$MinCardinality:0,$MaxCardinality:1}}"
                + ",Class:GraphChangeTrigger{$Hide:0,$Hide:1,Attribute:ScopeQuery{$MinCardinality:0,$MaxCardinality:-1},Attribute:ChangeTypeFilter{$MinCardinality:0,$MaxCardinality:-1}}"
                + ",Class:GraphChangeEvent{Attribute:ChangedVertex{$MinCardinality:0,$MaxCardinality:1},Attribute:Type{$MinCardinality:1,$MaxCardinality:1},Attribute:OldValue{$MinCardinality:0,$MaxCardinality:1},Attribute:NewValue{$MinCardinality:0,$MaxCardinality:1},Attribute:Edge{$MinCardinality:0,$MaxCardinality:1}}"
                + ",Class:Executable"
                + ",Class:Delegate{Attribute:Object{$MinCardinality:1,$MaxCardinality:1},Attribute:Method{$MinCardinality:1,$MaxCardinality:1}}"
                + ",Class:DotNetStaticMethod{Attribute:DotNetTypeName{$MinCardinality:1,$MaxCardinality:1},Attribute:DotNetMethodName{$MinCardinality:1,$MaxCardinality:1}}"
                + ",Class:DotNetDelegate{Attribute:DotNetDelegatePointer{$MinCardinality:1,$MaxCardinality:1}}"
                + ",Class:VertexEval"
                + "}");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEnum").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$Inherits"),
                 LegacySystem.Graph.EasyVertex.Get(smz, false, @"EnumBase"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeFilterEnum").AddEdge(
             LegacySystem.Graph.EasyVertex.Get(smz, false, "*$Inherits"),
             LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEnum"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\EventTrigger\Listener").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Executable"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Event\Trigger").AddEdge(
             LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
             LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\EventTrigger"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Event\Source").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeTrigger").AddEdge(
             LegacySystem.Graph.EasyVertex.Get(smz, false, "*$Inherits"),
             LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\EventTrigger"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeTrigger\ScopeQuery").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"String"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeTrigger\ChangeTypeFilter").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeFilterEnum"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEvent").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$Inherits"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Event"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEvent\ChangedVertex").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEvent\Type").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEnum"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEvent\OldValue").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEvent\NewValue").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GraphChangeEvent\Edge").AddEdge(
                 LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
                 LegacySystem.Graph.EasyVertex.Get(smz, false, @"Edge"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Delegate").AddEdge(
                  LegacySystem.Graph.EasyVertex.Get(smz, false, "*$Inherits"),
                 LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Executable"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Delegate\Object").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Delegate\Method").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\DotNetStaticMethod").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(smz, false, "*$Inherits"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Executable"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\DotNetStaticMethod\DotNetTypeName").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"String"));
            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\DotNetStaticMethod\DotNetMethodName").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"String"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\DotNetDelegate").AddEdge(
                  LegacySystem.Graph.EasyVertex.Get(smz, false, "*$Inherits"),
                  LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Executable"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\DotNetDelegate\DotNetDelegatePointer").AddEdge(
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"*$EdgeTarget"),
                LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));
        }

        void CreateSystemMetaZeroTypesExecutionFlow_Part2()
        {
            IVertex smuml = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML");

            IVertex smz = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroTypes");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\EventTrigger").AddVertex(
                 LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                 "Fire");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\EventTrigger\Fire").AddVertex(
                 LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                 "event");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\EventTrigger\Fire\event").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Event"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow").AddVertex(
                 LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Function"),
                 "GenericEventHandler");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GenericEventHandler").AddVertex(
                 LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                 "event");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GenericEventHandler\event").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\Event"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GenericEventHandler\event").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smz, false, "*$MinCardinality"),
                "0");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\GenericEventHandler\event").AddVertex(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$MaxCardinality"),
              "-1");

            // object GetValue

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "GetValue");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetValue").AddEdge(
                Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetValue").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\Output"),
                "");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetValue\Output:").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$VertexTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            // SetValue(object value)

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "SetValue");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\SetValue").AddEdge(
                Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\SetValue").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                "value");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\SetValue\value").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            // IList<IEdge> GetOutEdges

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "GetOutEdges");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetOutEdges").AddEdge(
                Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetOutEdges").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\Output"),
                "");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetOutEdges\Output:").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$VertexTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            // IList<IEdge> GetInEdges

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "GetInEdges");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetInEdges").AddEdge(
                Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetInEdges").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\Output"),
                "");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\GetInEdges\Output:").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$VertexTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            // IEdge AddVertexAndReturnEdge(IVertex metaVertex, object val)

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "AddVertexAndReturnEdge");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddVertexAndReturnEdge").AddEdge(
                Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddVertexAndReturnEdge").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                "metaVertex");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddVertexAndReturnEdge\metaVertex").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddVertexAndReturnEdge").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                "val");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddVertexAndReturnEdge\val").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddVertexAndReturnEdge").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\Output"),
                 "");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddVertexAndReturnEdge\Output:").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$VertexTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"Edge"));

            // IEdge AddEdge(IVertex metaVertex, IVertex destVertex)

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "AddEdge");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddEdge").AddEdge(
                       Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddEdge").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                "metaVertex");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddEdge\metaVertex").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddEdge").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                "destVertex");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\AddEdge\destVertex").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            // void DeleteEdge(IEdge edge)

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "DeleteEdge");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\DeleteEdge").AddEdge(
                  Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\DeleteEdge").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method\InputParameter"),
                "edge");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\DeleteEdge\edge").AddEdge(
              LegacySystem.Graph.EasyVertex.Get(smz, false, "*$EdgeTarget"),
              LegacySystem.Graph.EasyVertex.Get(smz, false, @"VertexType"));

            // Execute()

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval").AddVertex(
                LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"),
                "Execute");

            LegacySystem.Graph.EasyVertex.Get(smz, false, @"ExecutionFlow\VertexEval\Execute").AddEdge(
                Is, LegacySystem.Graph.EasyVertex.Get(smuml, false, @"Class\Method"));
        }

        void CreateSystemMetaVisualiserDiagram()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, sm, "{Visualiser}");

            IVertex smv = Root.Get(false, @"System\Meta\Visualiser");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(smv, sm, "{DiagramInternal{" +
                "Class:DiagramItemBase{Association:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionX{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionY{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:DiagramLine{$MinCardinality:0,$MaxCardinality:-1},OptionEdge,OptionDiagramLineDefinition},"+
                "Class:DiagramItemDefinition{Attribute:DirectVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:MetaVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Association:DiagramItemClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramItemVertex{$MinCardinality:0,$MaxCardinality:1},Association:InstanceCreation{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineDefinition{$MinCardinality:0,$MaxCardinality:-1},Attribute:DoNotShowInherited{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1}},"+
                "Enum:InstanceCreationEnum{EnumValue:Instance,EnumValue:InstanceAndDirect,EnumValue:Direct}," +
                "Class:DiagramLineBase{Association:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Association:ToDiagramItem{$MinCardinality:1,$MaxCardinality:1}},"+
                "Class:DiagramLineDefinition{Attribute:EdgeTestQuery{$MinCardinality:1,$MaxCardinality:1},Attribute:ToDiagramItemTestQuery{$MinCardinality:0,$MaxCardinality:1},Association:DiagramLineClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineVertex{$MinCardinality:0,$MaxCardinality:1},Attribute:CreateEdgeOnly{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1}},"+
                "Class:DiagramImageItem{Attribute:Filename}," +
                "Class:DiagramOvalItem,"+
                "Class:DiagramRhombusItem,"+
                "Class:DiagramRectangleItem{Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:RoundEdgeSize{MinValue:0,MaxValue:200,$MinCardinality:0,$MaxCardinality:1},Association:VisualiserClass{$MinCardinality:0,$MaxCardinality:1},Attribute:VisualiserVertex{$MinCardinality:0,$MaxCardinality:1}},"+
                "Class:DiagramLine{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}},"+
                "Enum:LineEndEnum{EnumValue:Straight,EnumValue:Arrow,EnumValue:Triangle,EnumValue:FilledTriangle,EnumValue:Diamond,EnumValue:FilledDiamond},"+
                "Class:DiagramMetaExtendedLine{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}}"+
                "}}");

            smv.Get(false, @"DiagramInternal\InstanceCreationEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            smv.Get(false, @"DiagramInternal\LineEndEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            smv.Get(false, @"DiagramInternal\DiagramItemBase").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));            
            smv.Get(false, @"DiagramInternal\DiagramItemBase").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));

            smv.Get(false, @"DiagramInternal\DiagramItemBase\Definition").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramItemDefinition"));
            IVertex definitionSection = smv.Get(false, @"DiagramInternal\DiagramItemBase\Definition").AddVertex(sm.Get(false, @"?$Section"), "Definition");

            smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionX").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            IVertex positionAndSizeSection = smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionX").AddVertex(sm.Get(false, @"?$Section"), "Position and size");

            smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(false, @"?$Section"), positionAndSizeSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(false, @"?$Section"), positionAndSizeSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(false, @"?$Section"), positionAndSizeSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\LineWidth").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            IVertex lookSection = smv.Get(false, @"DiagramInternal\DiagramItemBase\LineWidth").AddVertex(sm.Get(false, @"?$Section"), "Look");

            smv.Get(false, @"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\Color"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\Color"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(false, @"?$Section"), sm.Get(false, @"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));



            smv.Get(false, @"DiagramInternal\DiagramItemDefinition").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DirectVertexTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\MetaVertexTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DiagramItemClass").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DiagramItemVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\InstanceCreation").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\InstanceCreationEnum"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DiagramLineDefinition").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineDefinition"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\DoNotShowInherited").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramItemDefinition\ForceShowEditForm").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            smv.Get(false, @"DiagramInternal\DiagramLineBase").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));            
            smv.Get(false, @"DiagramInternal\DiagramLineBase").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));

            smv.Get(false, @"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineDefinition"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(false, @"?$Section"), definitionSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\Color"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\Color"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(false, @"?$Section"), sm.Get(false, @"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));

            smv.Get(false, @"DiagramInternal\DiagramLineDefinition").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\EdgeTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\ToDiagramItemTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\DiagramLineClass").AddEdge(sm.Get(false, @"?$EdgeTarget"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\DiagramLineVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\CreateEdgeOnly").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramLineDefinition\ForceShowEditForm").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            smv.Get(false, @"DiagramInternal\DiagramImageItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramImageItem").AddEdge(sm.Get(false, "?$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramImageItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramImageItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smv.Get(false, @"DiagramInternal\DiagramImageItem\Filename").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            smv.Get(false, @"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get(false, "?$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramOvalItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramOvalItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get(false, "?$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramRhombusItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRhombusItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");



            smv.Get(false, @"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get(false, "?$Inherits"), smv.Get(false, @"DiagramInternal\DiagramItemBase"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRectangleItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroUML\Class"));
            IVertex visualiserSection = smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddVertex(sm.Get(false, @"?$Section"), "Visualiser");

            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(false, @"?$Section"), visualiserSection);


            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLine").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramLine").AddEdge(sm.Get(false, "?$Inherits"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramLine").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(false, @"?$Section"), lookSection);


            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get(false, "?$Inherits"), smv.Get(false, @"DiagramInternal\DiagramLineBase"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramMetaExtendedLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            smv.Get(false, @"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(false, @"?$Section"), lookSection);
        }

        void CreateSystemMetaZeroTypes_UX_Additional()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex smzu = Root.Get(false, @"System\Meta\ZeroTypes\UX");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(smzu, sm, "{" +
                "OptionEdge,"+
                "OptionDiagramLineDefinition,"+
                "Class:UXTemplate{Attribute:Name{$MinCardinality:0,$MaxCardinality:1},Aggregation:UXTemplate{$MinCardinality:0,$MaxCardinality:-1},Attribute:DirectVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:MetaVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Association:ItemClass{$MinCardinality:0,$MaxCardinality:1},Attribute:ItemVertex{$MinCardinality:0,$MaxCardinality:1},Association:InstanceCreation{$MinCardinality:0,$MaxCardinality:1},Aggregation:UXDecoratorTemplate{$MinCardinality:0,$MaxCardinality:-1},Attribute:DoNotShowInherited{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1},Attribute:ContainerEdgeMetaVertex{$MinCardinality:0,$MaxCardinality:1},Attribute:BaseEdgeQuery{$MinCardinality:0,$MaxCardinality:1}}," +
                "Enum:InstanceCreationEnum{EnumValue:Instance,EnumValue:InstanceAndDirect,EnumValue:Direct}," +
                "Class:LineDecoratorBase{Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Association:ToItem{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:UXDecoratorTemplate{Attribute:EdgeTestQuery{$MinCardinality:1,$MaxCardinality:1},Attribute:ToDiagramItemTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:CreateEdgeOnly{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1},Attribute:DecoratorClass{$MinCardinality:0,$MaxCardinality:1},Attribute:DecoratorVertex{$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:ImageItem{Attribute:Filename{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowName{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:OvalItem{Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:RhombusItem{Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:CodeItem{Attribute:HideHeader{$MinCardinality:0,$MaxCardinality:1},Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:FontSize{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:15,MinValue:1,MaxValue:40,$DisplayLarger:},Attribute:ShowWhiteSpace{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowLineNumbers{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowFolding{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:HighlightedLine{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowScrollBars{$MinCardinality:0,$MaxCardinality:1},Attribute:FormalTextLanguage{$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:RectangleItem{Attribute:HideHeader{$MinCardinality:0,$MaxCardinality:1},Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:RoundEdgeSize{MinValue:0,MaxValue:200,$MinCardinality:0,$MaxCardinality:1},Association:VisualiserClass{$MinCardinality:0,$MaxCardinality:1},Attribute:VisualiserVertex{$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:LineDecorator{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}}," +
                "Enum:LineEndEnum{EnumValue:Straight,EnumValue:Arrow,EnumValue:Triangle,EnumValue:FilledTriangle,EnumValue:Diamond,EnumValue:FilledDiamond}," +
                "Class:MetaExtendedLineDecorator{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:ContainerItem{Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:RoundEdgeSize{MinValue:0,MaxValue:200,$MinCardinality:0,$MaxCardinality:1}}," +
                "Enum:OrientationEnum{EnumValue:Horizontal,EnumValue:Vertical},"+
                "Class:MultiContainerItem{Attribute:Orientation{$MinCardinality:0,$MaxCardinality:1},Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:RoundEdgeSize{MinValue:0,MaxValue:200,$MinCardinality:0,$MaxCardinality:1}}," +
                "Class:MultiContainerSubItem" +
                "}");

            // UXItem [EXTENSION]

            smzu.Get(false, @"UXItem\UXTemplate").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\UXTemplate"));

            // UXContainer [EXTENSION]

            smzu.Get(false, @"UXContainer\NewItemUXTemplate").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\UXTemplate"));

            // enums

            smzu.Get(false, @"InstanceCreationEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            smzu.Get(false, @"LineEndEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            smzu.Get(false, @"OrientationEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            // UXTemplate

            smzu.Get(false, @"UXTemplate").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"UXTemplate").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.UXTemplate, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smzu.Get(false, @"UXTemplate\Name").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smzu.Get(false, @"UXTemplate\UXTemplate").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\UXTemplate"));
            smzu.Get(false, @"UXTemplate\DirectVertexTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smzu.Get(false, @"UXTemplate\MetaVertexTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smzu.Get(false, @"UXTemplate\ItemClass").AddEdge(sm.Get(false, @"?$EdgeTarget"), smzu.Get(false, @"Class,"));
            smzu.Get(false, @"UXTemplate\ItemVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smzu.Get(false, @"UXTemplate\InstanceCreation").AddEdge(sm.Get(false, @"?$EdgeTarget"), smzu.Get(false, @"InstanceCreationEnum"));
            smzu.Get(false, @"UXTemplate\UXDecoratorTemplate").AddEdge(sm.Get(false, @"?$EdgeTarget"), smzu.Get(false, @"UXDecoratorTemplate"));
            smzu.Get(false, @"UXTemplate\DoNotShowInherited").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"UXTemplate\ForceShowEditForm").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"UXTemplate\ContainerEdgeMetaVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smzu.Get(false, @"UXTemplate\BaseEdgeQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));

            // LineDecoratorBase

            smzu.Get(false, @"LineDecoratorBase").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"LineDecoratorBase").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXItem"));
            smzu.Get(false, @"LineDecoratorBase\LineWidth").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smzu.Get(false, @"LineDecoratorBase\ToItem").AddEdge(sm.Get(false, @"?$EdgeTarget"), smzu.Get(false, @"UXItem"));

            // UXDecoratorTemplate

            smzu.Get(false, @"UXDecoratorTemplate").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"UXDecoratorTemplate").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXTemplate"));
            smzu.Get(false, @"UXDecoratorTemplate\EdgeTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smzu.Get(false, @"UXDecoratorTemplate\ToDiagramItemTestQuery").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smzu.Get(false, @"UXDecoratorTemplate\CreateEdgeOnly").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"UXDecoratorTemplate\ForceShowEditForm").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"UXDecoratorTemplate\DecoratorClass").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"UXDecoratorTemplate\DecoratorVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Vertex"));

            // ImageItem

            smzu.Get(false, @"ImageItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"ImageItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXItem"));
            smzu.Get(false, @"ImageItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.ImageItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smzu.Get(false, @"ImageItem\Filename").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\String"));
            smzu.Get(false, @"ImageItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"ImageItem\ShowName").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            // OvalItem

            smzu.Get(false, @"OvalItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"OvalItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXItem"));
            smzu.Get(false, @"OvalItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.OvalItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smzu.Get(false, @"OvalItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            // RhombusItem

            smzu.Get(false, @"RhombusItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"RhombusItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXItem"));
            smzu.Get(false, @"RhombusItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.RhombusItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smzu.Get(false, @"RhombusItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            // CodeItem

            smzu.Get(false, @"CodeItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"CodeItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXItem"));
            smzu.Get(false, @"CodeItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.CodeItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smzu.Get(false, @"CodeItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            //smzu.Get(false, @"CodeItem\ShowMeta").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"CodeItem\HideHeader").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            //smzu.Get(false, @"CodeItem\ShowHeader").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"CodeItem\FontSize").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            smzu.Get(false, @"CodeItem\ShowWhiteSpace").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"CodeItem\ShowLineNumbers").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"CodeItem\HighlightedLine").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"CodeItem\ShowFolding").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            smzu.Get(false, @"CodeItem\ShowScrollBars").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            smzu.Get(false, @"CodeItem\FormalTextLanguage").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));


            // RectangleItem

            smzu.Get(false, @"RectangleItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"RectangleItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXItem"));
            smzu.Get(false, @"RectangleItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.RectangleItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smzu.Get(false, @"RectangleItem\VisualiserClass").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroUML\Class"));
            //IVertex visualiserSection = smzu.Get(false, @"RectangleItem\VisualiserClass").AddVertex(sm.Get(false, @"?$Section"), "Visualiser");

            smzu.Get(false, @"RectangleItem\VisualiserVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            //smzu.Get(false, @"RectangleItem\VisualiserVertex").AddEdge(sm.Get(false, @"?$Section"), visualiserSection);

            smzu.Get(false, @"RectangleItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            //smzu.Get(false, @"RectangleItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"RectangleItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            //smzu.Get(false, @"RectangleItem\ShowMeta").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"RectangleItem\HideHeader").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            //smzu.Get(false, @"RectangleItem\ShowHeader").AddEdge(sm.Get(false, @"?$Section"), lookSection);


            // LineDecorator

            smzu.Get(false, @"LineDecorator").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"LineDecorator").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"LineDecoratorBase"));
            smzu.Get(false, @"LineDecorator").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.LineDecorator, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smzu.Get(false, @"LineDecorator\StartAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));
            // smzu.Get(false, @"LineDecorator\StartAnchor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"LineDecorator\EndAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));
            // smzu.Get(false, @"LineDecorator\EndAnchor").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"LineDecorator\IsDashed").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            //smzu.Get(false, @"LineDecorator\IsDashed").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            // MetaExtendedLineDecorator

            smzu.Get(false, @"MetaExtendedLineDecorator").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"MetaExtendedLineDecorator").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"LineDecorator"));
            smzu.Get(false, @"MetaExtendedLineDecorator").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.MetaExtendedLineDecorator, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smzu.Get(false, @"MetaExtendedLineDecorator\StartAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));

            smzu.Get(false, @"MetaExtendedLineDecorator\EndAnchor").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?LineEndEnum"));

            smzu.Get(false, @"MetaExtendedLineDecorator\IsDashed").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));


            // ContatinerItem

            smzu.Get(false, @"ContainerItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"ContainerItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXContainer"));
            smzu.Get(false, @"ContainerItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.ContainerItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            smzu.Get(false, @"ContainerItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            //smzu.Get(false, @"ContainerItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"ContainerItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            //smzu.Get(false, @"ContainerItem\ShowMeta").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            // MultiContainerItem

            smzu.Get(false, @"MultiContainerItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"MultiContainerItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXContainer"));
            smzu.Get(false, @"MultiContainerItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.MultiContainerItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smzu.Get(false, @"MultiContainerItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            //smzu.Get(false, @"ContainerItem\RoundEdgeSize").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"MultiContainerItem\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            //smzu.Get(false, @"ContainerItem\ShowMeta").AddEdge(sm.Get(false, @"?$Section"), lookSection);

            smzu.Get(false, @"MultiContainerItem\Orientation").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\OrientationEnum"));

            // MultiContainerSubItem

            smzu.Get(false, @"MultiContainerSubItem").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            smzu.Get(false, @"MultiContainerSubItem").AddEdge(sm.Get(false, "?$Inherits"), smzu.Get(false, @"UXContainer"));
            smzu.Get(false, @"MultiContainerSubItem").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.ZeroTypes.UX.MultiContainerSubItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");


        }

        void CreateSystemMetaVisualiser()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex smv = Root.Get(false, @"System\Meta\Visualiser");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(smv, sm, "{Enum:BaseEdgeTarget{EnumValue:Any,EnumValue:Specyfic},"+
                "Enum:GridStyleEnum{EnumValue:None,EnumValue:Vertical,EnumValue:Horizontal,EnumValue:All,EnumValue:AllAndRound,EnumValue:Round},"+
                "Enum:SongSnapToGridEnum{EnumValue:1 bar,EnumValue:1/2 bar,EnumValue:1/4 bar,EnumValue:1/8 bar,EnumValue:1/16 bar,EnumValue:1/32 bar,EnumValue:no snap},"+
                "Enum:SnapToGridEnum{EnumValue:1/16 bar,EnumValue:1/32 bar,EnumValue:1/64 bar,EnumValue:1/128 bar,EnumValue:1/256 bar,EnumValue:1/512 bar,EnumValue:no snap}," +
                "Class:Form{Attribute:ExpertMode{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ColumnNumber{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:1},Attribute:MetaOnLeft{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:MetaAlignLeft{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:SectionsAsTabs{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False}}," +
                "Class:Code{Attribute:FontSize{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:15,MinValue:1,MaxValue:40,$DisplayLarger:},Attribute:ShowWhiteSpace{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowLineNumbers{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:HighlightedLine{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},,Attribute:ShowFolding{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:FormalTextLanguage{$MinCardinality:0,$MaxCardinality:1},Attribute:TextMemoryCurrent{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:0},Attribute:TextMemoryMax{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:0}}," +
                "Class:Table{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:ExpertMode{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:TableFast{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:Tree," +
                "Class:Graph{Attribute:VisualiserCircleSize{$MinCardinality:1,$MaxCardinality:1},Attribute:NumberOfCircles{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd:},Attribute:ShowOutEdges{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowInEdges{$MinCardinality:1,$MaxCardinality:1},Attribute:FastMode{$MinCardinality:1,$MaxCardinality:1},Attribute:MetaLabels{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:Class,Class:String,Class:StringView,Class:Vertex,Class:Edge,Class:Integer,Class:Decimal,Class:Float,Class:Boolean,Class:Enum,Class:Debug," +
                "Class:Diagram{Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd:},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd},Attribute:Item{$MinCardinality:0,$MaxCardinality:-1,$Hide:0},Association:CreationPool{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:Wrap,"+
                "Class:List{Attribute:IsMetaRightAlign{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowMeta{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:Set2D{Attribute:CanEdit{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ConnectPoints{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowArrowLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ScaleLinesDensity{$MinCardinality:1,$MaxCardinality:1,MinValue:1,MaxValue:5,$DefaultValue:1},Attribute:VerticalAxisMax{$MinCardinality:1,$MaxCardinality:1},Attribute:VerticalAxisMin{$MinCardinality:1,$MaxCardinality:1},Attribute:HorizontalAxisMin{$MinCardinality:1,$MaxCardinality:1},Attribute:HorizontalAxisMax{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowToolbarNames{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False}}," +
                "Class:Sequence{Attribute:SnapToGrid{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowVelocity{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowLabel{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowArrowLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowSnapLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:DefaultVelocity{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:127,MinValue:0,MaxValue:127}}," +
                "Class:MelodyFlow{Attribute:ShowVelocity{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowLabel{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowArrowLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:DefaultVelocity{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:127,MinValue:0,MaxValue:127}}," +                
                "Class:TriggerSet{Attribute:SnapToGrid{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowVelocity{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False},Attribute:ShowSnapLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:DefaultVelocity{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:127,MinValue:-1,MaxValue:127}}," +
                "Class:ChordProgression{Attribute:ShowLabel{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowArrowLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True}}," +
                "Class:Song{Attribute:SnapToGrid{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowLabel{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowArrowLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowSnapLines{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowToolbarNames{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:False}}," +
                "Class:UX," +
                //"Class:UXTest," +
                "Class:Test}");

            sm.Get(false, @"Visualiser\BaseEdgeTarget").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            sm.Get(false, @"Visualiser\GridStyleEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            sm.Get(false, @"Visualiser\SnapToGridEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            sm.Get(false, @"Visualiser\SongSnapToGridEnum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));
            
            sm.Get(false, @"Visualiser\Form").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Form").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.FormVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Form").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Form\ExpertMode").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Form\ColumnNumber").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Form\ColumnNumber").AddVertex(sm.Get(false, @"?MinValue"), 1);
            sm.Get(false, @"Visualiser\Form\ColumnNumber").AddVertex(sm.Get(false, @"?MaxValue"), 8);
            sm.Get(false, @"Visualiser\Form\MetaOnLeft").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Form\MetaAlignLeft").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Form\SectionsAsTabs").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));            
            sm.Get(false, @"Visualiser\Form").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));
            //sm.Get(false, @"ZeroUML\Class").AddEdge(sm.Get(false, "ZeroUML*$DefaultOpenVisualiser"), sm.Get(false, @"Visualiser\Form"));

            sm.Get(false, @"Visualiser\Code").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Code").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.CodeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Code").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Code\FontSize").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Float"));
            sm.Get(false, @"Visualiser\Code\ShowWhiteSpace").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Code\ShowLineNumbers").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Code\HighlightedLine").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Code\ShowFolding").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));

            sm.Get(false, @"Visualiser\Code\FormalTextLanguage").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"Visualiser\Code\TextMemoryCurrent").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Code\TextMemoryMax").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Code").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Wrap").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Wrap").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.WrapVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Wrap").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Wrap").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasFilter"));
            sm.Get(false, @"Visualiser\List").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.ListVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\List\ShowMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));            
            sm.Get(false, @"Visualiser\List\GridStyle").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Visualiser\GridStyleEnum"));
            sm.Get(false, @"Visualiser\List\IsMetaRightAlign").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\List\IsAllVisualisersEdit").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\List").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasFilter"));
            sm.Get(false, @"Visualiser\Table").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.TableVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Table\ExpertMode").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Table\ToShowEdgesMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Edge"));
            sm.Get(false, @"Visualiser\Table\ShowHeader").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Table\GridStyle").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Visualiser\GridStyleEnum"));
            sm.Get(false, @"Visualiser\Table\GridStyle").AddEdge(sm.Get(false, @"?$DefaultValue"), sm.Get(false, @"Visualiser\GridStyleEnum\Round"));
            sm.Get(false, @"Visualiser\Table\AlternatingRows").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Table\IsAllVisualisersEdit").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Table").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasFilter"));
            sm.Get(false, @"Visualiser\TableFast").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.TableFastVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\TableFast\ToShowEdgesMeta").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Edge"));
            sm.Get(false, @"Visualiser\TableFast\ShowHeader").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\TableFast\GridStyle").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Visualiser\GridStyleEnum"));
            sm.Get(false, @"Visualiser\TableFast\GridStyle").AddEdge(sm.Get(false, @"?$DefaultValue"), sm.Get(false, @"Visualiser\GridStyleEnum\Round"));
            sm.Get(false, @"Visualiser\TableFast\AlternatingRows").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\TableFast\IsAllVisualisersEdit").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\TableFast").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Tree").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.TreeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Tree").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Graph").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.GraphVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Graph\VisualiserCircleSize").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Graph\VisualiserCircleSize").AddVertex(sm.Get(false, @"?MinValue"), 50);
            sm.Get(false, @"Visualiser\Graph\VisualiserCircleSize").AddVertex(sm.Get(false, @"?MaxValue"), 500);
            sm.Get(false, @"Visualiser\Graph\NumberOfCircles").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Integer"));
            sm.Get(false, @"Visualiser\Graph\NumberOfCircles").AddVertex(sm.Get(false, @"?MinValue"), 1);
            sm.Get(false, @"Visualiser\Graph\NumberOfCircles").AddVertex(sm.Get(false, @"?MaxValue"), 10);
            sm.Get(false, @"Visualiser\Graph\ShowOutEdges").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Graph\ShowInEdges").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Graph\FastMode").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Graph\MetaLabels").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"Visualiser\Graph").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Class").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Class").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.ClassVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Class").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Class").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));

            sm.Get(false, @"Visualiser\String").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\String").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.StringVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\String").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\String").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\String"));
            sm.Get(false, @"Visualiser\String").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\StringView").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\StringView").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.StringViewVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\StringView").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));            
            sm.Get(false, @"ZeroTypes\String").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\StringView"));
            sm.Get(false, @"Visualiser\StringView").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));


            sm.Get(false, @"Visualiser\Vertex").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Vertex").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.VertexVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Vertex").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Vertex").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"ZeroTypes\VertexType").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"ZeroTypes\VertexType").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$Inherits").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$Inherits").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$VertexTarget").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            sm.Get(false, @"Base\Vertex\$VertexTarget").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
            //sm.Get(false, @"ZeroUML\Class").AddEdge(sm.Get(false, "ZeroUML*$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\VertexVisualiser"));
            sm.Get(false, @"ZeroUML\Class").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Vertex"));
           
            sm.Get(false, @"Visualiser\Edge").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Edge").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.EdgeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Edge").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Edge").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Edge"));
            sm.Get(false, @"ZeroTypes\Edge").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Edge"));
            sm.Get(false, @"Visualiser\Edge").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));            

            sm.Get(false, @"Visualiser\Integer").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Integer").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.IntegerVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Integer").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Integer").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Integer"));
            sm.Get(false, @"Visualiser\Integer").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Decimal").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Decimal").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.DecimalVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Decimal").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Decimal").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Decimal"));
            sm.Get(false, @"Visualiser\Decimal").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Float").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Float").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.FloatVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Float").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Float").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Float"));
            sm.Get(false, @"Visualiser\Float").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Boolean").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Boolean").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.BooleanVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Boolean").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\Boolean").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Boolean"));
            sm.Get(false, @"ZeroTypes\Boolean").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\Boolean"));
            sm.Get(false, @"Visualiser\Boolean").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Enum").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Enum").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.EnumVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Enum").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"ZeroTypes\EnumBase").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\Enum"));
            sm.Get(false, @"Visualiser\Enum").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));

            sm.Get(false, @"Visualiser\Debug").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Debug").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.DebugVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Debug").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Debug").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));            

            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));
            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Diagram").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.Diagram, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Diagram\Item").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?DiagramItemBase"));
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Integer"));
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddVertex(sm.Get(false, @"?$DefaultValue"), 1000);
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddVertex(sm.Get(false, @"?MinValue"), 0);
            sm.Get(false, @"Visualiser\Diagram\SizeX").AddVertex(sm.Get(false, @"?MaxValue"), 4000);
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Integer"));
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddVertex(sm.Get(false, @"?$DefaultValue"), 1000);
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddVertex(sm.Get(false, @"?MinValue"), 0);
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddVertex(sm.Get(false, @"?MaxValue"), 4000);
            sm.Get(false, @"Visualiser\Diagram\CreationPool").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, "ZeroUML?$DefaultOpenVisualiser"), sm.Get(false, @"Visualiser\Diagram"));

            IVertex diagramGeneralGroup = sm.Get(false, @"Visualiser\Diagram").AddVertex(sm.Get(false, @"?$Group"), "General");
            sm.Get(false, @"Visualiser\Diagram\CreationPool").AddEdge(sm.Get(false, @"?$Group"), diagramGeneralGroup);

            //IVertex diagramDetailsGroup = sm.Get(false, @"Visualiser\Diagram\Scale").AddVertex(sm.Get(false, @"?$Group"), "Details");
            IVertex diagramDetailsGroup = sm.Get(false, @"Visualiser\Diagram\SizeX").AddVertex(sm.Get(false, @"?$Group"), "Details");
            sm.Get(false, @"Visualiser\Diagram\SizeY").AddEdge(sm.Get(false, @"?$Group"), diagramDetailsGroup);
            sm.Get(false, @"Visualiser\Diagram\SelectedEdges").AddEdge(sm.Get(false, @"?$Group"), diagramDetailsGroup);

            sm.Get(false, @"Visualiser\Diagram").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));

            sm.Get(false, @"Visualiser\Set2D").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));
            sm.Get(false, @"Visualiser\Set2D").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Set2D").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0_COMPOSER.UIWpf.Visualisers.Set2DVisualiser, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Set2D").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Set2D\CanEdit").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Set2D\ConnectPoints").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Set2D\ShowArrowLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Set2D\ScaleLinesDensity").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Float"));
            sm.Get(false, @"Visualiser\Set2D\HorizontalAxisMin").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Float"));
            sm.Get(false, @"Visualiser\Set2D\HorizontalAxisMax").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Float"));
            sm.Get(false, @"Visualiser\Set2D\VerticalAxisMin").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Float"));
            sm.Get(false, @"Visualiser\Set2D\VerticalAxisMax").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Float"));
            sm.Get(false, @"Visualiser\Set2D\ShowToolbarNames").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Set2D").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            sm.Get(false, @"Visualiser\Sequence").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Sequence").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Sequence").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0_COMPOSER.UIWpf.Visualisers.SequenceVisualiser, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Sequence").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Sequence").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));
            sm.Get(false, @"Visualiser\Sequence\ShowLabel").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Sequence\ShowVelocity").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Sequence\ShowArrowLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Sequence\ShowSnapLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Sequence\DefaultVelocity").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Integer"));
            sm.Get(false, @"Visualiser\Sequence\SnapToGrid").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Visualiser\SnapToGridEnum"));
            //  sm.Get(false, @"Visualiser\Sequence\SnapToGrid").AddEdge(sm.Get(false, @"?$DefaultValue"), sm.Get(false, @"Visualiser\SnapToGridEnum\'1 bar'"));

            sm.Get(false, @"Visualiser\MelodyFlow").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\MelodyFlow").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\MelodyFlow").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0_COMPOSER.UIWpf.Visualisers.MelodyFlowVisualiser, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\MelodyFlow").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\MelodyFlow").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));
            sm.Get(false, @"Visualiser\MelodyFlow\ShowLabel").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\MelodyFlow\ShowVelocity").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\MelodyFlow\ShowArrowLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));            
            sm.Get(false, @"Visualiser\MelodyFlow\DefaultVelocity").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Integer"));
            
            sm.Get(false, @"Visualiser\TriggerSet").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\TriggerSet").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\TriggerSet").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0_COMPOSER.UIWpf.Visualisers.TriggerSetVisualiser, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\TriggerSet").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\TriggerSet").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));            
            sm.Get(false, @"Visualiser\TriggerSet\ShowVelocity").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));            
            sm.Get(false, @"Visualiser\TriggerSet\ShowSnapLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\TriggerSet\DefaultVelocity").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Integer"));
            sm.Get(false, @"Visualiser\TriggerSet\SnapToGrid").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Visualiser\SnapToGridEnum"));

            sm.Get(false, @"Visualiser\ChordProgression").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\ChordProgression").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\ChordProgression").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0_COMPOSER.UIWpf.Visualisers.ChordProgressionVisualiser, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\ChordProgression").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\ChordProgression").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));
            sm.Get(false, @"Visualiser\ChordProgression\ShowLabel").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));            
            sm.Get(false, @"Visualiser\ChordProgression\ShowArrowLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));            

            sm.Get(false, @"Visualiser\Song").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Song").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasSelectedEdges"));
            sm.Get(false, @"Visualiser\Song").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0_COMPOSER.UIWpf.Visualisers.SongVisualiser, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Song").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Song").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Specyfic"));
            sm.Get(false, @"Visualiser\Song\ShowLabel").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Song\ShowArrowLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Song\ShowSnapLines").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Song\ShowToolbarNames").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?Boolean"));
            sm.Get(false, @"Visualiser\Song\SnapToGrid").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Visualiser\SongSnapToGridEnum"));

            sm.Get(false, @"Visualiser\UX").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXContainer"));            
            //sm.Get(false, @"Visualiser\UX").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.UX.UXVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            sm.Get(false, @"Visualiser\UX").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.UX.UXVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            sm.Get(false, @"Visualiser\UX").AddEdge(sm.Get(false, @" ?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\UX").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));
            //sm.Get(false, @"ZeroTypes\UX\UXItem").AddEdge(sm.Get(false, "ZeroUML?$DefaultOpenVisualiser"), sm.Get(false, @"Visualiser\UX"));
            sm.Get(false, @"ZeroTypes\UX\UXContainer").AddEdge(sm.Get(false, "ZeroUML?$DefaultOpenVisualiser"), sm.Get(false, @"Visualiser\UX"));
            //sm.Get(false, @"ZeroTypes\UX\UXItem").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\UX"));
            sm.Get(false, @"ZeroTypes\UX\UXContainer").AddEdge(sm.Get(false, "ZeroUML?$DefaultViewVisualiser"), sm.Get(false, @"Visualiser\UX"));
            //sm.Get(false, @"ZeroTypes\UX\UXItem").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\UX"));
            sm.Get(false, @"ZeroTypes\UX\UXContainer").AddEdge(sm.Get(false, "ZeroUML?$DefaultEditVisualiser"), sm.Get(false, @"Visualiser\UX"));

            sm.Get(false, @"Visualiser\Test").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));            
            sm.Get(false, @"Visualiser\Test").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.TestVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Test").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\Test").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));

            /*sm.Get(false, @"Visualiser\UXTest").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\UX\UXItem"));
            sm.Get(false, @"Visualiser\UXTest").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.UX.Generic.UXTestVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\UXTest").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"Visualiser\UXTest").AddEdge(sm.Get(false, @"Visualiser\BaseEdgeTarget"), sm.Get(false, @"Visualiser\BaseEdgeTarget\Any"));*/
        }

        void CreateSystemMetaMethodVisualiser()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex smv = Root.Get(false, @"System\Meta\Visualiser");

            IVertex smvm = smv.AddVertex(null, "Method");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(smvm, sm, "{" +
                "Class:VoidVoidMethod{}," +            
                "}");            
            
            sm.Get(false, @"Visualiser\Method\VoidVoidMethod").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasBaseEdge"));
            sm.Get(false, @"Visualiser\Method\VoidVoidMethod").AddEdge(sm.Get(false, "?$Inherits"), sm.Get(false, @"ZeroTypes\HasExecutableVertex"));
            sm.Get(false, @"Visualiser\Method\VoidVoidMethod").AddVertex(sm.Get(false, "?$PlatformClassName"), @"m0.UIWpf.Visualisers.Method.VoidVoidMethod, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(false, @"Visualiser\Method\VoidVoidMethod").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));            
        }

            void CreateSystemData()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex s = Root.Get(false, @"System");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(s, sm, "{Data}");
        }

        IVertex AddDiagramItemDefinition(String Value, String DirectVertexTestQuery, String MetaVertexTestQuery, IVertex DiagramItemClass, IVertex InstanceCreation)
        {
            IVertex did = Root.Get(false, @"System\Meta?DiagramItemDefinition");

            IVertex v = Root.Get(false, @"System\Data\Visualiser\Diagram").AddVertex(did, Value);

            v.AddEdge(Root.Get(false, @"System\Meta?$Is"), Root.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramItemDefinition"));

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
                v.Get(false, "DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta?RoundEdgeSize"), RoundEdgeSize);

            if (CreateDiagraItemVertex && showMeta)
                v.Get(false, "DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem\ShowMeta"), "True");
            else
                v.Get(false, "DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem\ShowMeta"), "False");

            if (VisualiserClass != null)
                v.Get(false, @"DiagramItemVertex:").AddEdge(Root.Get(false, @"System\Meta?VisualiserClass"), VisualiserClass);

            if (VisualiserVertex)
                v.Get(false, @"DiagramItemVertex:").AddVertex(Root.Get(false, @"System\Meta?VisualiserVertex"), null);

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
            IVertex did = Root.Get(false, @"System\Meta?DiagramItemDefinition");

            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex v = AddDiagramItemDefinition(Value, DirectVertexTestQuery, MetaVertexTestQuery, DiagramItemClass, InstanceCreation);

            if (doNotShowInherited)
                v.AddVertex(sm.Get(false, @"?DoNotShowInherited"), "True");

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    v.AddVertex(sm.Get(false, @"?ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    v.AddVertex(sm.Get(false, @"?ForceShowEditForm"), "False");
            }

            if (CreateDiagraItemVertex)
            {
                IVertex iv = v.AddVertex(did.Get(false, "DiagramItemVertex"), null);

                if (SizeX > -1)
                {
                    iv.AddVertex(Root.Get(false, @"System\Meta?SizeX"), SizeX);
                    iv.AddVertex(Root.Get(false, @"System\Meta?SizeY"), SizeY);
                }

                if (LineWidth > -1)
                    iv.AddVertex(Root.Get(false, @"System\Meta?LineWidth"), LineWidth);

                if (BackgroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(iv, Root.Get(false, @"System\Meta?Color"), Root.Get(false, @"System\Meta?BackgroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Red"), BackgroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Green"), BackgroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Blue"), BackgroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Opacity"), BackgroundOpacity);
                }

                if (ForegroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(iv, Root.Get(false, @"System\Meta?Color"), Root.Get(false, @"System\Meta?ForegroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Red"), ForegroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Green"), ForegroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Blue"), ForegroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Opacity"), ForegroundOpacity);
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

            IVertex dld = Root.Get(false, @"System\Meta?DiagramInternal\DiagramLineDefinition");

            IVertex lv = v.AddVertex(dld, name);

            lv.AddEdge(sm.Get(false, "?$Is"), dld);

            lv.AddVertex(dld.Get(false, "EdgeTestQuery"), EdgeTestQuery);

            lv.AddVertex(dld.Get(false, "ToDiagramItemTestQuery"), ToDiagramTestQuery);

            lv.AddEdge(dld.Get(false, "DiagramLineClass"), DiagramLineClass);

            if (CreateEdgeOnly != null)
            {
                if (CreateEdgeOnly == true)
                    lv.AddVertex(sm.Get(false, @"?CreateEdgeOnly"), "True");

                if (CreateEdgeOnly == false)
                    lv.AddVertex(sm.Get(false, @"?CreateEdgeOnly"), "False");
            }

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    lv.AddVertex(sm.Get(false, @"?ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    lv.AddVertex(sm.Get(false, @"?ForceShowEditForm"), "False");
            }

            if (CreateDiagraLineVertex)
            {
                IVertex dlv = lv.AddVertex(dld.Get(false, "DiagramLineVertex"), null);

                if (isDashed)
                    dlv.AddVertex(sm.Get(false, "?IsDashed"), "True");

                if (startAnchor != null)
                    dlv.AddEdge(sm.Get(false, "?StartAnchor"), startAnchor);

                if (endAnchor != null)
                    dlv.AddEdge(sm.Get(false, "?EndAnchor"), endAnchor);

                if (LineWidth > -1)
                    dlv.AddVertex(Root.Get(false, @"System\Meta?LineWidth"), LineWidth);

                if (BackgroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(dlv, Root.Get(false, @"System\Meta?Color"), Root.Get(false, @"System\Meta?BackgroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Red"), BackgroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Green"), BackgroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Blue"), BackgroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Opacity"), BackgroundOpacity);
                }

                if (ForegroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(dlv, Root.Get(false, @"System\Meta?Color"), Root.Get(false, @"System\Meta?ForegroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Red"), ForegroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Green"), ForegroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Blue"), ForegroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(false, @"System\Meta?Opacity"), ForegroundOpacity);
                }
            }
        }

        void CreateSystemDataVisualiserDiagram()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            IVertex sd = Root.Get(false, @"System\Data");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sd, sm, "{Visualiser{Diagram}}");

            IVertex Instance = sm.Get(false, "?Instance");
            IVertex InstanceAndDirect = sm.Get(false, "?InstanceAndDirect");
            IVertex Direct = sm.Get(false, "?Direct");

            IVertex arrow = sm.Get(false, @"?DiagramInternal\LineEndEnum\Arrow");
            IVertex triangle = sm.Get(false, @"?DiagramInternal\LineEndEnum\Triangle");
            IVertex filledTriangle = sm.Get(false, @"?DiagramInternal\LineEndEnum\FilledTriangle");
            IVertex diamond = sm.Get(false, @"?DiagramInternal\LineEndEnum\Diamond");
            IVertex filledDiamond = sm.Get(false, @"?DiagramInternal\LineEndEnum\FilledDiamond");
            IVertex straight = sm.Get(false, @"?DiagramInternal\LineEndEnum\Straight");

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
                sm.Get(false, @"?DiagramRectangleItem"),
                InstanceAndDirect,
                true, -1, 0, -1,
                -1, 0, 0, 0,
                -1, 0, 0, 0,
                -1, true,
                Root.Get(false, @"System\Meta?List"), true);

            IVertex v2vv = v2.Get(false, @"DiagramItemVertex:\VisualiserVertex:");

            v2vv.AddVertex(Root.Get(false, @"System\Meta?FilterQuery"), "{$Is:Attribute}:");

            v2vv.AddVertex(Root.Get(false, @"System\Meta?ShowHeader"), "False");

            AddDiagramLine_Combo(v2,
                "Association instance",
                @"$Is:{$Is:Class}\Association:",
                @"Definition:Object",
                sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
                sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
              sm.Get(false, @"?DiagramRectangleItem"),
              InstanceAndDirect,
              true, -1, 0, -1,
                -1, 0, 0, 0,
                -1, 0, 0, 0,
                -1, true,
                Root.Get(false, @"System\Meta\Visualiser\Class"), true, true);

            IVertex v3vv = v3.Get(false, @"DiagramItemVertex:\VisualiserVertex:");

            v3vv.AddVertex(Root.Get(false, @"System\Meta?FilterQuery"), "Attribute:");

            v3vv.AddVertex(Root.Get(false, @"System\Meta?ShowHeader"), "False");

            AddDiagramLine_Combo(v3,
             "Association",
             @"$Is:Class\Association",
             @"Definition:Class",
             sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
            sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
            sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
                    sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
             sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
            sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
            sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramOvalItem"),
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
             sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
             sm.Get(false, @"?DiagramRectangleItem"),
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
               sm.Get(false, @"?DiagramInternal\DiagramMetaExtendedLine"),
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

            IVertex filledTriangle = sm.Get(false, @"?DiagramInternal\LineEndEnum\FilledTriangle");

            AddDiagramLine_Combo(diagramItem,
             "Next",
             @"$Is:NextOut\Next",
              @"BaseEdge:\To:\$Is:Atom",
              sm.Get(false, @"?DiagramInternal\DiagramLine"),
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

            IVertex Instance = sm.Get(false, "?Instance");
            IVertex InstanceAndDirect = sm.Get(false, "?InstanceAndDirect");
            IVertex Direct = sm.Get(false, "?Direct");

            IVertex arrow = sm.Get(false, @"?DiagramInternal\LineEndEnum\Arrow");
            IVertex triangle = sm.Get(false, @"?DiagramInternal\LineEndEnum\Triangle");
            IVertex filledTriangle = sm.Get(false, @"?DiagramInternal\LineEndEnum\FilledTriangle");
            IVertex diamond = sm.Get(false, @"?DiagramInternal\LineEndEnum\Diamond");
            IVertex filledDiamond = sm.Get(false, @"?DiagramInternal\LineEndEnum\FilledDiamond");
            IVertex straight = sm.Get(false, @"?DiagramInternal\LineEndEnum\Straight");


            /////////////////////////////////////////////////////////////////////////
            // AtomType 
            /////////////////////////////////////////////////////////////////////////

            IVertex vAtomType = AddDiagramItemDefinition_Combo_RectangleItem("AtomType", false,
         @"{$Is:AtomType}",
         "AtomType",
          sm.Get(false, @"?DiagramRectangleItem"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
               sm.Get(false, @"?DiagramInternal\DiagramLine"),
               true,
               filledDiamond,
               null,
               -1, false,
               -1, 0, 0, 0,
               -1, 0, 0, 100);

            IVertex vState = AddDiagramItemDefinition_Combo_RectangleItem("State", false,
         @"{$Is:State}",
         "State",
          sm.Get(false, @"?DiagramRectangleItem"),
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
               sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          Root.Get(false, @"System\Meta?List"), true);

            IVertex vEnum_vv = vEnum.Get(false, @"DiagramItemVertex:\VisualiserVertex:");

            vEnum_vv.AddVertex(Root.Get(false, @"System\Meta?FilterQuery"), "EnumValue:");

            vEnum_vv.AddVertex(Root.Get(false, @"System\Meta\Visualiser\List\ShowHeader"), "False");
            vEnum_vv.AddVertex(Root.Get(false, @"System\Meta\Visualiser\List\ShowMeta"), "False");



            /////////////////////////////////////////////////////////////////////////
            // ZeroOperator
            /////////////////////////////////////////////////////////////////////////

            IVertex vZeroOperator = AddDiagramItemDefinition_Combo_RectangleItem("ZeroOperator", false,
         @"{$Is:ZeroOperator}",
         "{$Inherits:ZeroOperator}",
          sm.Get(false, @"?DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          5, true,
          null, false);

            AddDiagramLine_Combo(vZeroOperator,
       "Expression",
       @"$Is:ExpressionAtom\NextExpression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
       true,
       null,
       arrow,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddNextLine(vZeroOperator);


            /////////////////////////////////////////////////////////////////////////
            // MultiOperator
            /////////////////////////////////////////////////////////////////////////

            IVertex vMultiOperator = AddDiagramItemDefinition_Combo_RectangleItem("MultiOperator", false,
         @"{$Is:MultiOperator}",
         "{$Inherits:MultiOperator}",
          sm.Get(false, @"?DiagramRectangleItem"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
         sm.Get(false, @"?DiagramOvalItem"),
         InstanceAndDirect,
         true, 40, 40, -1,
         0, 0, 0, 255,
         255, 255, 255, 255);

            AddDiagramLine_Combo(vReturn,
       "Expression",
       @"$Is:Return\Expression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
             sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
            sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRhombusItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          false);

            AddDiagramLine_Combo(vIf,
       "Test",
       @"$Is:If\Test",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
       true,
              null,
       filledTriangle,
       3, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddDiagramLine_Combo(vSwitch,
       "Fallback",
       @"$Is:Switch\Fallback",
       @"Definition:Fallback",
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
       true,
       null,
       triangle,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            // Default

            IVertex vDefault = AddDiagramItemDefinition_Combo_RectangleItem("Fallback", false,
         @"{$Is:Fallback}",
         "Fallback",
          sm.Get(false, @"?DiagramRectangleItem"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
            sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
       sm.Get(false, @"?DiagramInternal\DiagramLine"),
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
          sm.Get(false, @"?DiagramRectangleItem"),
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
                new PackageLine("FunctionCall","MultiOperator") ,                
                new PackageLine("{}","MultiOperator"),
                new PackageLine("InnerCreation","MultiOperator"),
                new PackageLine("+","DoubleOperator"),
                new PackageLine("-","DoubleOperator"),
                new PackageLine("\"* \"","DoubleOperator"),
                new PackageLine("/","DoubleOperator"),
                new PackageLine("?","ZeroOperator"),
                new PackageLine("\"\\ \"","ZeroOperator"),
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
            sm.Get(false, @"?DiagramInternal\DiagramLine"),
            true,
            diamond,
            null,
            -1, false,
            -1, 0, 0, 0,
            -1, 0, 0, 100);
        }

        struct PackageLine
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

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, sm, "{Commands{VisualiserClass,SynchronisedVisualiser}}");
        }

        void CreateUserMeta()
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, sm, "{User{CurrentUser,"+
                "Class:NonAtomProcess{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1}},"+
                "Class:Session{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1},Aggregation:ClipboardCopy{$MinCardinality:0,$MaxCardinality:-1},Aggregation:ClipboardCut{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Process{$MinCardinality:0,$MaxCardinality:-1},Attribute:Visualisers{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:User{Attribute:CurrentSession{$MinCardinality:1,$MaxCardinality:1},Aggregation:Session{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Settings{$MinCardinality:1,$MaxCardinality:1},Association:DefaultFormalTextLanguage{$MinCardinality:1,$MaxCardinality:1},Association:DefaultUXTemplate{$MinCardinality:1,$MaxCardinality:1},Aggregation:Queries{$MinCardinality:1,$MaxCardinality:1}}," +
                "Class:Settings{Attribute:CopyOnDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Association:AllowBlankAreaDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Attribute:AllowManyUXItemsWithSameBaseEdgeTo{$MinCardinality:1,$MaxCardinality:1}},Enum:AllowBlankAreaDragAndDropEnum{EnumValue:No,EnumValue:OnlyEnd,EnumValue:StartAndEnd},"+
                "Class:VisualiserList{Association:Visualiser{$MinCardinality:0,$MaxCardinality:-1}}}}");

            sm.Get(false, @"User\NonAtomProcess").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\Session").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\User").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\Settings").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));

            sm.Get(false, @"User\NonAtomProcess\StartTimeStamp").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\DateTime"));

            sm.Get(false, @"User\VisualiserList\Visualiser").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\UXItem"));

            sm.Get(false, @"User\Session\StartTimeStamp").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\DateTime"));
            sm.Get(false, @"User\Session\ClipboardCopy").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Base\Vertex"));
            sm.Get(false, @"User\Session\ClipboardCut").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"Base\Vertex"));
            sm.Get(false, @"User\Session\Process").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\NonAtomProcess")); // to be updated
            sm.Get(false, @"User\Session\Visualisers").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\VisualiserList")); 

            sm.Get(false, @"User\User\Session").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\Session"));
            sm.Get(false, @"User\User\CurrentSession").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\Session"));
            sm.Get(false, @"User\User\Settings").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\Settings"));
            sm.Get(false, @"User\User\DefaultFormalTextLanguage").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\FormalTextLanguage"));
            sm.Get(false, @"User\User\DefaultUXTemplate").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\UX\UXTemplate"));
            sm.Get(false, @"User\User\Queries").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));

            sm.Get(false, @"User\AllowBlankAreaDragAndDropEnum").AddEdge(sm.Get(false, @"?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            sm.Get(false, @"User\Settings\CopyOnDragAndDrop").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"User\Settings\AllowBlankAreaDragAndDrop").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?AllowBlankAreaDragAndDropEnum"));
            sm.Get(false, @"User\Settings\AllowManyUXItemsWithSameBaseEdgeTo").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
        }

        void CreateUser(IVertex user)
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(user, sm, "{Settings:{CopyOnDragAndDrop:False,AllowManyUXItemsWithSameBaseEdgeTo:True},Queries:{String:test,String:\"test{test2}\"}}");

            user.Get(false, "Settings:").AddEdge(sm.Get(false, "?AllowBlankAreaDragAndDrop"), sm.Get(false, @"User\AllowBlankAreaDragAndDropEnum\StartAndEnd"));

            user.AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"User\User"));
            user.Get(false, "Settings:").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"User\Settings"));

            user.AddEdge(sm.Get(false, @"User\User\DefaultFormalTextLanguage"), Root.Get(false, @"System\FormalTextLanguage\ZeroCode"));

            IVertex session = VertexOperations.AddInstance(user, sm.Get(false, @"User\Session"));

            user.AddEdge(sm.Get(false, @"User\User\CurrentSession"), session);
        }

        void CreateUsers()
        {
            IVertex sm = Root.Get(false, @"System\Meta\User");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(Root, sm, "{User{User:root,User:wlodek,User:tadek}}");

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

        public LegacySystem_MinusZero()
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
                logFile = new System.IO.StreamWriter("LegacySystem_log.txt");
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
            AddIsAttribute_inner(@"System\Meta\ZeroTypes?" + what + ":", what);
            AddIsAttribute_inner(@"System\Meta\Visualiser?" + what + ":", what);
            AddIsAttribute_inner(@"System\Meta\User?" + what + ":", what);
        }

        private void AddIsAttribute_inner(string s, string what)
        {
            IVertex attributes = root.GetAll(false, s);
            IVertex ismeta = root.Get(false, @"System\Meta?$Is");
            IVertex ameta = root.Get(false, @"System\Meta\ZeroUML\Class\" + what);

            foreach (IEdge v in attributes)
                if (v.To.Get(false, @"$Is:" + ameta) == null)
                    v.To.AddEdge(ismeta, ameta);
        }

        private void AddIsAggregation()
        {
            AddIsAggregation_inner(@"System\Meta\Store?Attribute:");
            AddIsAggregation_inner(@"System\Meta\Store?Aggregation:");

            AddIsAggregation_inner(@"System\Meta\ZeroTypes?Attribute:");
            AddIsAggregation_inner(@"System\Meta\ZeroTypes?Aggregation:");

            AddIsAggregation_inner(@"System\Meta\Visualiser?Attribute:");
            AddIsAggregation_inner(@"System\Meta\Visualiser?Aggregation:");

            AddIsAggregation_inner(@"System\Meta\User?Attribute:");
            AddIsAggregation_inner(@"System\Meta\User?Aggregation:");
        }

        private void AddIsAggregation_inner(string s)
        {
            IVertex isaggregationtarget = root.GetAll(false, s);

            IVertex isAggregation = root.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");

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

        public void StartTransaction()
        {
            foreach (ITransactionRoot r in Stores)
                r.StartTransaction();
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
            EdgeTarget = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$EdgeTarget");
            Is = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Is");
            IsAggregation = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsAggregation");
            Inherits = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Inherits");

            MinusZero.Instance.EdgeTarget = EdgeTarget;
            MinusZero.Instance.Is = Is;
            MinusZero.Instance.IsAggregation = IsAggregation;
            MinusZero.Instance.Inherits = Inherits;
        }

        private void CheckAndCorrecIsClass()
        {
            IVertex systemClasses = root.GetAll(false, "System?Class:");

            IVertex Class = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\ZeroUML\Class");

            foreach (IEdge classEdge in systemClasses)
            {
                IVertex classVertex = classEdge.To;

                if (classVertex.Get(false, "$Is:Class") == null)
                    classVertex.AddEdge(Is, Class);
            }
        }

        private void CreateSystemHardware()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex Meta = r.Get(false, @"System\Meta");

            IVertex Hardware = Meta.AddVertex(null, "Hardware");

            IVertex Computer = GraphUtil.AddClass(Hardware, "Computer");

            GraphUtil.AddAttribute(Computer, "Drive", r.Get(false, @"System\Meta\Store\FileSystem\Drive"), 0, -1);
            

            IVertex DefaultComputer = Hardware.AddVertex(null, "LocalComputer");

            DefaultComputer.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), Computer);
        }

        int ScaleUp(int inputValue, double Value)
        {
            if (inputValue == 0)
                inputValue = 30;

            double toAddSpace = 255 - inputValue;


            int o = (int)((toAddSpace * Value) + inputValue);

            if (o > 255)
                return 255;

            return o;
        }

        public void CreateSystemDataUXColor()
        {
            IVertex r = MinusZero.Instance.root;

            IVertex Data = r.Get(false, @"System\Data");

            IVertex UX = Data.AddVertex(null, "UX");

            IVertex Colors = UX.AddVertex(null, "Colors");

            ColorHelper.AddColor(Colors, "White", 255, 255, 255, 255);
            ColorHelper.AddColor(Colors, "Black", 0, 0, 0, 255);
            ColorHelper.AddColor(Colors, "Gray", 127, 127, 127, 255);
            ColorHelper.AddColor(Colors, "LightGray", 160, 160, 160, 255);
            ColorHelper.AddColor(Colors, "VeryLightGray", 200, 200, 200, 255);
            ColorHelper.AddColor(Colors, "VeryVeryLightGray", 220, 220, 220, 255);            

            for (int x = 0; x < 12; x++)
                ColorHelper.AddColor(Colors, "Gray" + x, x * 23, x * 23, x * 23, 255);

            var baseColors = new Dictionary<string, int[]> {
                ["Red"] = new int[] {255, 0, 0},
                ["Green"] = new int[] { 0, 255, 0 },
                ["Blue"] = new int[] { 0, 0, 255 },
                ["Orange"] = new int[] {255, 106, 0},
                ["Yellow"] = new int[] {255, 255, 0},
                ["Citron"] = new int[] {255, 216, 0},
                ["LimeGreen"] = new int[] {182, 255, 0},
                ["YellowGreen"] = new int[] {76, 255, 0},
                ["CentralGreen"] = new int[] {0, 255, 33},
                ["BlueGreen"] = new int[] {0, 255, 144},
                ["Cyan"] = new int[] {0, 255, 255},
                ["Azure"] = new int[] {0, 148, 255},
                ["CentralBlue"] = new int[] {0, 38, 255},
                ["VioletBlue"] = new int[] {72, 0, 255},
                ["Violet"] = new int[] {178, 0, 255},
                ["Pink"] = new int[] {255, 0, 220},
                ["Magenta"] = new int[] {255, 0, 255},
                ["Rose"] = new int[] {255, 0, 110}
            };

            foreach (var de in baseColors)
                ColorHelper.AddColor(Colors, de.Key, de.Value[0], de.Value[1], de.Value[2], 255);

            foreach (var de in baseColors)
                ColorHelper.AddColor(Colors, "Light"+de.Key, ScaleUp(de.Value[0], 0.3), ScaleUp(de.Value[1], 0.3), ScaleUp(de.Value[2], 0.3), 255);

            foreach (var de in baseColors)
                ColorHelper.AddColor(Colors, "VeryLight"+de.Key, ScaleUp(de.Value[0], 0.6), ScaleUp(de.Value[1], 0.6), ScaleUp(de.Value[2], 0.6), 255);

            foreach (var de in baseColors)
                ColorHelper.AddColor(Colors, "VeryVeryLight"+de.Key, ScaleUp(de.Value[0], 0.8), ScaleUp(de.Value[1], 0.8), ScaleUp(de.Value[2], 0.8), 255);

        }

        IVertex AddUXTemplate(IVertex where, String Value, bool doNotShowInherited,
            String DirectVertexTestQuery,
            String MetaVertexTestQuery,
            IVertex ItemClass,
            IVertex InstanceCreation,
            bool CreateItemVertex,
            double BorderWidth,
            IVertex BackgroundColor, IVertex ForegroundColor,
            bool? ForceShowEditForm = null)
        {
            IVertex ut = Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate");

            IVertex sm = Root.Get(false, @"System\Meta");
            
            //

            IVertex v = where.AddVertex(ut, Value);

            v.AddEdge(Root.Get(false, @"System\Meta?$Is"), ut);

            if (DirectVertexTestQuery != null)
                v.AddVertex(ut.Get(false, "DirectVertexTestQuery"), DirectVertexTestQuery);

            if (MetaVertexTestQuery != null)
                v.AddVertex(ut.Get(false, "MetaVertexTestQuery"), MetaVertexTestQuery);

            v.AddEdge(ut.Get(false, "ItemClass"), ItemClass);

            v.AddEdge(ut.Get(false, "InstanceCreation"), InstanceCreation);


            //

            if (doNotShowInherited)
                v.AddVertex(ut.Get(false, @"DoNotShowInherited"), "True");

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    v.AddVertex(ut.Get(false, @"ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    v.AddVertex(ut.Get(false, @"ForceShowEditForm"), "False");
            }

            if (CreateItemVertex)
            {
                IVertex iv = v.AddVertex(ut.Get(false, "ItemVertex"), null);

                if (BorderWidth > -1)
                    iv.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BorderWidth"), BorderWidth);

                if (ForegroundColor != null)
                    iv.AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\ForegroundColor"), ForegroundColor);

                if (BackgroundColor != null)
                    iv.AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BackgroundColor"), BackgroundColor);
            }

            return v;
        }

        IVertex AddUXTemplate_RectangleItem(IVertex where, String Value, bool doNotShowInherited,
              String DirectVertexTestQuery,
              String MetaVertexTestQuery,
              IVertex InstanceCreation,
              bool CreateItemVertex,
              double BorderWidth,
              IVertex BackgroundColor, IVertex ForegroundColor,
              int RoundEdgeSize, bool showMeta,
              IVertex VisualiserClass, bool VisualiserVertex,
              bool? ForceShowEditForm = null)
        {
               IVertex v = AddUXTemplate(where, Value, doNotShowInherited, DirectVertexTestQuery, MetaVertexTestQuery,
                   Root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem"), InstanceCreation,
                CreateItemVertex,
              BorderWidth,
              ForegroundColor, BackgroundColor,
              ForceShowEditForm);

            if (CreateItemVertex)
            {
                if (RoundEdgeSize > -1)
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\RoundEdgeSize"), RoundEdgeSize);

                if (showMeta)
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\ShowMeta"), "True");
                else
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\ShowMeta"), "False");
            }

            if (VisualiserClass != null)
                v.Get(false, @"ItemVertex:").AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserClass"), VisualiserClass);

            if (VisualiserVertex)
                v.Get(false, @"ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserVertex"), null);

            return v;
        }

        IVertex AddUXTemplate_ContainerItem(IVertex where, String Value, bool doNotShowInherited,
              String DirectVertexTestQuery,
              String MetaVertexTestQuery,
              IVertex InstanceCreation,
              double BorderWidth,
              IVertex BackgroundColor, IVertex ForegroundColor,
              int RoundEdgeSize, bool showMeta,
              IVertex ContainerEdgeMetaVertex,
              bool? ForceShowEditForm = null)
        {
            IVertex v = AddUXTemplate(where, Value, doNotShowInherited, DirectVertexTestQuery, MetaVertexTestQuery,
                Root.Get(false, @"System\Meta\ZeroTypes\UX\ContainerItem"), InstanceCreation, true/*CreateItemVertex*/,
           BorderWidth,
           ForegroundColor, BackgroundColor,
           ForceShowEditForm);

            //if (CreateItemVertex)
            {
                if (RoundEdgeSize > -1)
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\ContainerItem\RoundEdgeSize"), RoundEdgeSize);

                if (showMeta)
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\ContainerItem\ShowMeta"), "True");
                else
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\ContainerItem\ShowMeta"), "False");
            }

            if (ContainerEdgeMetaVertex!= null)
                v.AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\ContainerEdgeMetaVertex"), 
                    ContainerEdgeMetaVertex);

            v.Get(false, "ItemVertex:").AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXContainer\NewItemUXTemplate"),
              Root.Get(false, @"System\Data\UX\Templates\ZeroUML"));


            return v;
        }

        IVertex AddUXTemplate_MultiContainerItem(IVertex where, String Value, bool doNotShowInherited,
              String DirectVertexTestQuery,
              String MetaVertexTestQuery,
              IVertex InstanceCreation,
              bool CreateItemVertex,
              double BorderWidth,
              IVertex BackgroundColor, IVertex ForegroundColor,
              int RoundEdgeSize, bool showMeta, OrientationEnum orientation,       
              bool? ForceShowEditForm = null)
        {
            IVertex v = AddUXTemplate(where, Value, doNotShowInherited, DirectVertexTestQuery, MetaVertexTestQuery,
                Root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem"), InstanceCreation, CreateItemVertex,
           BorderWidth,
           ForegroundColor, BackgroundColor,
           ForceShowEditForm);

            if (CreateItemVertex)
            {
                if (RoundEdgeSize > -1)
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\RoundEdgeSize"), RoundEdgeSize);

                if (showMeta)
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\ShowMeta"), "True");
                else
                    v.Get(false, "ItemVertex:").AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\ShowMeta"), "False");
                
                v.Get(false, "ItemVertex:").AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\MultiContainerItem\Orientation"),
                    OrientationEnumHelper.GetVertex(orientation));
            }

            return v;
        }

        IVertex MultiContainerItem_AddUXTemplate(IVertex where, string Name, IVertex ContainerEdgeMetaVertex, string baseEdgeQuery, double Width, double Height)
        {
            IVertex t = where.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate"), null);

            t.AddEdge(Is, Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate"));

            t.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\Name"), Name);
            t.AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\ContainerEdgeMetaVertex"), ContainerEdgeMetaVertex);
            t.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\BaseEdgeQuery"), baseEdgeQuery);

            IVertex t_ItemVertex = t.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXTemplate\ItemVertex"), null);

            IVertex t_ItemVertex_Size = t_ItemVertex.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\Size"), null);
            t_ItemVertex_Size.AddEdge(Root.Get(false, @"System\Meta\Base\Vertex\$Is"),
                Root.Get(false, @"System\Meta\ZeroTypes\UX\Size"));

            t_ItemVertex_Size.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\Size\Width"), Width);
            t_ItemVertex_Size.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\Size\Height"), Height);

            return t;
        }

        void AddLineDecorator(IVertex v,
          String name,
          String EdgeTestQuery,
          String ToDiagramTestQuery,
          IVertex DecoratorClass,
          bool CreateLineVertex,
          IVertex startAnchor,
          IVertex endAnchor,
          double LineWidth, bool isDashed,
          IVertex BackgroundColor, IVertex ForegroundColor,
          bool? CreateEdgeOnly = null,
          bool? ForceShowEditForm = null)
        {
            IVertex uld = Root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator");

            IVertex udt = Root.Get(false, @"System\Meta\ZeroTypes\UX\UXDecoratorTemplate");

            IVertex lv = v.AddVertex(udt, name);

            lv.AddEdge(Root.Get(false, @"System\Meta\Base\Vertex\$Is"), udt);

            lv.AddVertex(udt.Get(false, "EdgeTestQuery"), EdgeTestQuery);

            lv.AddVertex(udt.Get(false, "ToDiagramItemTestQuery"), ToDiagramTestQuery);

            lv.AddEdge(udt.Get(false, "DecoratorClass"), DecoratorClass);

            if (CreateEdgeOnly != null)
            {
                if (CreateEdgeOnly == true)
                    lv.AddVertex(udt.Get(false, @"CreateEdgeOnly"), "True");

                if (CreateEdgeOnly == false)
                    lv.AddVertex(udt.Get(false, @"CreateEdgeOnly"), "False");
            }

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    lv.AddVertex(udt.Get(false, @"ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    lv.AddVertex(udt.Get(false, @"ForceShowEditForm"), "False");
            }

            if (CreateLineVertex)
            {
                IVertex dlv = lv.AddVertex(udt.Get(false, "DecoratorVertex"), null);

                if (isDashed)
                    dlv.AddVertex(uld.Get(false, "IsDashed"), "True");

                if (startAnchor != null)
                    dlv.AddEdge(uld.Get(false, "StartAnchor"), startAnchor);

                if (endAnchor != null)
                    dlv.AddEdge(uld.Get(false, "EndAnchor"), endAnchor);

                if (LineWidth > -1)
                    dlv.AddVertex(uld.Get(false, @"LineWidth"), LineWidth);

                if (ForegroundColor != null)
                    dlv.AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\ForegroundColor"), ForegroundColor);

                if (BackgroundColor != null)
                    dlv.AddEdge(Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\BackgroundColor"), BackgroundColor);
            }
        }

        void AddLineDecorator(IVertex v,
            String name,
            String EdgeTestQuery,
            String ToDiagramTestQuery,
            IVertex startAnchor,
            IVertex endAnchor,
            double LineWidth, bool isDashed,
            IVertex BackgroundColor, IVertex ForegroundColor,
            bool? CreateEdgeOnly = null,
            bool? ForceShowEditForm = null)
        {
            AddLineDecorator(v, name, EdgeTestQuery, ToDiagramTestQuery,
                Root.Get(false, @"System\Meta\ZeroTypes\UX\LineDecorator"), true,
                startAnchor, endAnchor, LineWidth, isDashed, BackgroundColor, ForegroundColor, CreateEdgeOnly, ForceShowEditForm);
        }

        void AddItemVertex(IVertex v, IVertex meta, object value)
        {
            v.Get(false, "ItemVertex:").AddVertex(meta, value);
        }

        void AddVisualiserVertex(IVertex v, IVertex meta, object value)
        {
            IVertex itemVertex = v.Get(false, "ItemVertex:");

            if (itemVertex.Get(false, "VisualiserVertex:") == null)
                itemVertex.AddVertex(Root.Get(false, @"System\Meta\ZeroTypes\UX\RectangleItem\VisualiserVertex"), null);

            IVertex visualiserVertex = itemVertex.Get(false, "VisualiserVertex:");

            visualiserVertex.AddVertex(meta, value);
        }

        void CreateSystemDataUXZeroUMLTemplate()
        {
            IVertex smzu = Root.Get(false, @"System\Meta\ZeroTypes\UX");

            IVertex sdu = Root.Get(false, @"System\Data\UX");

            IVertex colors = Root.Get(false, @"System\Data\UX\Colors");

            IVertex sdut = sdu.AddVertex(null, "Templates");

            IVertex sdutz = VertexOperations.AddInstance(sdut, smzu.Get(false, "UXTemplate"));

            sdutz.Value = "ZeroUML";

            //IVertex sduz = sdu.AddVertex(null, "ZeroUMLTemplate");

            IVertex Instance = smzu.Get(false, "?Instance");
            IVertex InstanceAndDirect = smzu.Get(false, "?InstanceAndDirect");
            IVertex Direct = smzu.Get(false, "?Direct");

            IVertex arrow = smzu.Get(false, @"LineEndEnum\Arrow");
            IVertex triangle = smzu.Get(false, @"LineEndEnum\Triangle");
            IVertex filledTriangle = smzu.Get(false, @"LineEndEnum\FilledTriangle");
            IVertex diamond = smzu.Get(false, @"LineEndEnum\Diamond");
            IVertex filledDiamond = smzu.Get(false, @"LineEndEnum\FilledDiamond");
            IVertex straight = smzu.Get(false, @"LineEndEnum\Straight");

            IVertex v5 = AddUXTemplate_MultiContainerItem(/*where*/sdutz, /*name*/"DoubleOperator TEST",/*doNotShowInherited*/ false,
                /*DirectVertexTestQuery*/ @"{{$Inherits:DoubleOperator}}",
                /*MetaVertexTestQuery*/ @"{$Inherits:DoubleOperator}",
                /*InstanceCreation*/ InstanceAndDirect,
                /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
                /*RoundEdgeSize*/-1, /*ShowMeta*/ false, /*Orientation*/ OrientationEnum.Horizontal);


            IVertex lt = MultiContainerItem_AddUXTemplate(/*where*/v5, /*name*/"Left",
                /*containerEdgeMetaVertex*/smzu,
                /*baseEdgeQuery*/"LeftExpression:",
                /*width*/0, /*height*/10);

            AddLineDecorator(/*where*/lt, /*name*/ "edge",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);

            MultiContainerItem_AddUXTemplate(v5, "Right",
                smzu,
                "RightExpression:",
                0, 20);            
            

            AddLineDecorator(/*where*/v5, /*name*/ "edge",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);


            //


            IVertex v4 = AddUXTemplate_MultiContainerItem(/*where*/sdutz, /*name*/"DoubleOperator",/*doNotShowInherited*/ false,
                /*DirectVertexTestQuery*/ @"{{$Inherits:DoubleOperator}}",
                /*MetaVertexTestQuery*/ @"{$Inherits:DoubleOperator}",
                /*InstanceCreation*/ InstanceAndDirect,
                /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
                /*RoundEdgeSize*/-1, /*ShowMeta*/ false, /*Orientation*/ OrientationEnum.Vertical);

            IVertex v4_lt = MultiContainerItem_AddUXTemplate(v4, "Left",
                smzu,
                "LeftExpression:",
                10, 0);

            AddLineDecorator(/*where*/v4_lt, /*name*/ "edge",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);

            IVertex v4_rt = MultiContainerItem_AddUXTemplate(v4, "Right",
                smzu,
                "RightExpression:",
                20, 0);

            AddLineDecorator(/*where*/v4_rt, /*name*/ "edge",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/filledTriangle,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);


            AddLineDecorator(/*where*/v4, /*name*/ "edge",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);


            //return;


            IVertex v = AddUXTemplate(/*where*/sdutz, /*name*/"Vertex", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ @"",
            /*ItemClass*/ smzu.Get(false, @"?RectangleItem"), /*InstanceCreation*/ InstanceAndDirect,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null,/*ForegroundColor*/ null);

            IVertex vi = AddUXTemplate(/*where*/sdutz, /*name*/"Image", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ null,
            /*ItemClass*/ smzu.Get(false, @"?ImageItem"), /*InstanceCreation*/ Direct,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null,/*ForegroundColor*/ null);

            AddItemVertex(vi, Root.Get(false, @"System\Meta\ZeroTypes\UX\ImageItem\Filename"), "prezes.jpg");
            AddItemVertex(vi, Root.Get(false, @"System\Meta\ZeroTypes\UX\ImageItem\ShowName"), "False");
            AddItemVertex(vi, Root.Get(false, @"System\Meta\ZeroTypes\UX\ImageItem\BorderSize"), "0");

            IVertex vo = AddUXTemplate(/*where*/sdutz, /*name*/"Oval", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ null,
            /*ItemClass*/ smzu.Get(false, @"?OvalItem"), /*InstanceCreation*/ Direct,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null,/*ForegroundColor*/ null);

            AddLineDecorator(/*where*/vo, /*name*/ "edge",
   /*EdgeTestQuery*/@"",
   /*ToDiagramTestQuery*/@"",
   //smzu.Get(false, @"MetaExtendedLineDecorator"),               
   /*StartAnchor*/null,
   /*EndAnchor*/arrow,
   /*LineWidth*/-1, /*IsDashed*/false,
   /*BackgroundColor*/null, null);

            IVertex vc = AddUXTemplate(/*where*/sdutz, /*name*/"Code hidden", /*doNotShowInherited*/ false,
         /*DirectVertexTestQuery*/ @"",
         /*MetaVertexTestQuery*/ null,
         /*ItemClass*/ smzu.Get(false, @"?CodeItem"), /*InstanceCreation*/ Direct,
         /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null,/*ForegroundColor*/ null);


            AddItemVertex(vc, Root.Get(false, @"System\Meta\ZeroTypes\UX\CodeItem\HideHeader"), "true");
            AddItemVertex(vc, Root.Get(false, @"System\Meta\ZeroTypes\UX\CodeItem\ShowScrollBars"), "true");

            AddLineDecorator(/*where*/vc, /*name*/ "any edge",
   /*EdgeTestQuery*/@"",
   /*ToDiagramTestQuery*/@"",
   //smzu.Get(false, @"MetaExtendedLineDecorator"),               
   /*StartAnchor*/null,
   /*EndAnchor*/arrow,
   /*LineWidth*/-1, /*IsDashed*/false,
   /*BackgroundColor*/null, null);


            AddUXTemplate(/*where*/sdutz, /*name*/"Code", /*doNotShowInherited*/ false,
/*DirectVertexTestQuery*/ @"",
/*MetaVertexTestQuery*/ null,
/*ItemClass*/ smzu.Get(false, @"?CodeItem"), /*InstanceCreation*/ Direct,
/*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null,/*ForegroundColor*/ null);

            IVertex vr = AddUXTemplate(/*where*/sdutz, /*name*/"Rhombus", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ null,
            /*ItemClass*/ smzu.Get(false, @"?RhombusItem"), /*InstanceCreation*/ Direct,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null,/*ForegroundColor*/ null);

            AddLineDecorator(/*where*/vr, /*name*/ "edge",
   /*EdgeTestQuery*/@"",
   /*ToDiagramTestQuery*/@"",
   //smzu.Get(false, @"MetaExtendedLineDecorator"),               
   /*StartAnchor*/null,
   /*EndAnchor*/arrow,
   /*LineWidth*/-1, /*IsDashed*/false,
   /*BackgroundColor*/null, null);

            

            AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Vertex RectangleItem LIST", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ null,
            /*InstanceCreation*/ Direct,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
            /*RoundEdgeSize*/-1,/*ShowMeta*/ false,
            /*VisualiserClass*/Root.Get(false, @"System\Meta?List"),/*VisualiserVertex*/ false);


            IVertex v2 = AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Vertex RectangleItem", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ null,
            /*InstanceCreation*/ Direct,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
            /*RoundEdgeSize*/-1,/*ShowMeta*/ false,
            /*VisualiserClass*/null,/*VisualiserVertex*/ false);

            IVertex vci = AddUXTemplate_ContainerItem(/*where*/sdutz, /*name*/"Vertex ContainerItem",/*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ null,
            /*InstanceCreation*/ Direct,
            /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
            /*RoundEdgeSize*/-1,/*ShowMeta*/ false, /*ContainerEdgeMetaVertex*/smzu);

            AddLineDecorator(/*where*/vci, /*name*/ "any edge",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);



            AddLineDecorator(/*where*/v, /*name*/ "VERTEX EDGE",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               /*DecoratorClass*/smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*CreateeLineVertex*/true,
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);

            AddLineDecorator(/*where*/v2, /*name*/ "edge",
               /*EdgeTestQuery*/@"",
               /*ToDiagramTestQuery*/@"",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/false,
               /*BackgroundColor*/null, null);

        }

        void CreateSystemDataUXZeroUMLTemplate_MSX()
        {
            IVertex smzu = Root.Get(false, @"System\Meta\ZeroTypes\UX");

            IVertex sdu = Root.Get(false, @"System\Data\UX");

            IVertex colors = Root.Get(false, @"System\Data\UX\Colors");

            IVertex sdut = sdu.AddVertex(null, "Templates");

            IVertex sdutz = VertexOperations.AddInstance(sdut, smzu.Get(false, "UXTemplate"));

            sdutz.Value = "ZeroUML";

            //IVertex sduz = sdu.AddVertex(null, "ZeroUMLTemplate");

            IVertex Instance = smzu.Get(false, "?Instance");
            IVertex InstanceAndDirect = smzu.Get(false, "?InstanceAndDirect");
            IVertex Direct = smzu.Get(false, "?Direct");

            IVertex arrow = smzu.Get(false, @"LineEndEnum\Arrow");
            IVertex triangle = smzu.Get(false, @"LineEndEnum\Triangle");
            IVertex filledTriangle = smzu.Get(false, @"LineEndEnum\FilledTriangle");
            IVertex diamond = smzu.Get(false, @"LineEndEnum\Diamond");
            IVertex filledDiamond = smzu.Get(false, @"LineEndEnum\FilledDiamond");
            IVertex straight = smzu.Get(false, @"LineEndEnum\Straight");


            /////////////////////////////////////////////////////////////////////////
            // Object
            /////////////////////////////////////////////////////////////////////////

            

            IVertex obj = AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Object", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"{$Is:{$Is:Class}}",
            /*MetaVertexTestQuery*/ @"{$Is: Class}",
            /*InstanceCreation*/ InstanceAndDirect,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
            /*RoundEdgeSize*/-1,/*ShowMeta*/ false,
            /*VisualiserClass*/Root.Get(false, @"System\Meta?List"), /*ItemVertex*/ true);

            IVertex vv = obj.Get(false, @"ItemVertex:\VisualiserVertex:");

            vv.AddVertex(Root.Get(false, @"System\Meta?FilterQuery"), "{$Is:Attribute}:");

            vv.AddVertex(Root.Get(false, @"System\Meta?ShowHeader"), "False");

            AddLineDecorator(/*where*/obj, /*name*/ "Association instance",
               /*EdgeTestQuery*/@"$Is:{$Is:Class}\Association:",
               /*ToDiagramTestQuery*/@"UXTemplate:Object",               
               /*StartAnchor*/null,
               /*EndAnchor*/arrow,
               /*LineWidth*/-1, /*IsDashed*/true,
               /*BackgroundColor*/null, null);

            AddLineDecorator(/*where*/obj, /*name*/ "Aggregation instance",
               /*EdgeTestQuery*/@"$Is:{$Is:Class}\Aggregation:",
               /*ToDiagramTestQuery*/@"UXTemplate:Object",
               //smzu.Get(false, @"MetaExtendedLineDecorator"),               
               /*StartAnchor*/null,
               /*EndAnchor*/diamond,
               /*LineWidth*/-1, /*IsDashed*/true,
               /*BackgroundColor*/null, null);


        }

        void CreateSystemDataUXZeroUMLTemplate_Parent()
        {
            IVertex smzu = Root.Get(false, @"System\Meta\ZeroTypes\UX");

            IVertex sdu = Root.Get(false, @"System\Data\UX");

            IVertex colors = Root.Get(false, @"System\Data\UX\Colors");

            IVertex sdut = sdu.AddVertex(null, "Templates");

            IVertex sdutz = VertexOperations.AddInstance(sdut, smzu.Get(false, "UXTemplate"));

            sdutz.Value = "ZeroUML";

            //IVertex sduz = sdu.AddVertex(null, "ZeroUMLTemplate");

            IVertex Instance = smzu.Get(false, "?Instance");
            IVertex InstanceAndDirect = smzu.Get(false, "?InstanceAndDirect");
            IVertex Direct = smzu.Get(false, "?Direct");

            IVertex arrow = smzu.Get(false, @"LineEndEnum\Arrow");
            IVertex triangle = smzu.Get(false, @"LineEndEnum\Triangle");
            IVertex filledTriangle = smzu.Get(false, @"LineEndEnum\FilledTriangle");
            IVertex diamond = smzu.Get(false, @"LineEndEnum\Diamond");
            IVertex filledDiamond = smzu.Get(false, @"LineEndEnum\FilledDiamond");
            IVertex straight = smzu.Get(false, @"LineEndEnum\Straight");


            IVertex vi_code = AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Vertex RectangleItem CODE", /*doNotShowInherited*/ false,
            /*DirectVertexTestQuery*/ @"",
            /*MetaVertexTestQuery*/ null,
            /*InstanceCreation*/ Direct,
            /*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
            /*RoundEdgeSize*/-1,/*ShowMeta*/ false,
            /*VisualiserClass*/Root.Get(false, @"System\Meta\Visualiser\Code"),/*VisualiserVertex*/ false);

            AddVisualiserVertex(vi_code, Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale"), 50);

            IVertex vi_tree = AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Vertex RectangleItem TREE", /*doNotShowInherited*/ false,
/*DirectVertexTestQuery*/ @"",
/*MetaVertexTestQuery*/ null,
/*InstanceCreation*/ Direct,
/*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
/*RoundEdgeSize*/-1,/*ShowMeta*/ false,
/*VisualiserClass*/Root.Get(false, @"System\Meta\Visualiser\Tree"),/*VisualiserVertex*/ false);

            AddVisualiserVertex(vi_tree, Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale"), 50);

            IVertex vi_graph = AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Vertex RectangleItem GRAPH", /*doNotShowInherited*/ false,
/*DirectVertexTestQuery*/ @"",
/*MetaVertexTestQuery*/ null,
/*InstanceCreation*/ Direct,
/*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
/*RoundEdgeSize*/-1,/*ShowMeta*/ false,
/*VisualiserClass*/Root.Get(false, @"System\Meta\Visualiser\Graph"),/*VisualiserVertex*/ false);

            AddVisualiserVertex(vi_graph, Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale"), 50);

            IVertex vi_list = AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Vertex RectangleItem LIST", /*doNotShowInherited*/ false,
/*DirectVertexTestQuery*/ @"",
/*MetaVertexTestQuery*/ null,
/*InstanceCreation*/ Direct,
/*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
/*RoundEdgeSize*/-1,/*ShowMeta*/ false,
/*VisualiserClass*/Root.Get(false, @"System\Meta\Visualiser\List"),/*VisualiserVertex*/ false);

            AddVisualiserVertex(vi_list, Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale"), 50);


            IVertex vi_form = AddUXTemplate_RectangleItem(/*where*/sdutz, /*name*/"Vertex RectangleItem FORM", /*doNotShowInherited*/ false,
/*DirectVertexTestQuery*/ @"",
/*MetaVertexTestQuery*/ null,
/*InstanceCreation*/ Direct,
/*CreateItemVertex*/ true, /*BorderWidth*/ -1, /*BackgroundColor*/null, /*ForegroundColor*/ null,
/*RoundEdgeSize*/-1,/*ShowMeta*/ false,
/*VisualiserClass*/Root.Get(false, @"System\Meta\Visualiser\Form"),/*VisualiserVertex*/ false);

            AddVisualiserVertex(vi_form, Root.Get(false, @"System\Meta\ZeroTypes\UX\UXItem\Scale"), 50);




        }

        private void Initialize_PreParserReady()
        {
            LogLevel = -2;

            InitializeLog();

            PreBootstrap();

            Bootstrap();

            CreateSystem();            

            Init();

            ///////////

            LegacySystem.LegacySystem.LegacyInit();

            ///////////

            CreatePresentation();

            CreateSystemMetaBase_Part1();


            AddFastAccessVertexes();


            CreateSystemMetaZeroUML();

            CreateSystemMetaZeroTypes();

            CreateSystemMetaZeroTypes_UX();

            CreateSystemMetaZeroTypesExecutionFlow_Part1();

            CreateSystemMetaZeroUML_ZeroCode_part();

            CreateSystemMetaZeroTypesExecutionFlow_Part2();

            CreateSystemMetaBase_Part2();

            CreateSystemFormalTextLanguageZeroCodeBase();

            CreateSystemFormalTextLanguageZeroCode();


            CreateSystemFormalTextLanguegeZeroCode_Keywords();
        }

        private void Initialize_PostParserReady()
        {
            Init_AfterZeroCodeDefintionCreated();



            CreateSystemMetaVisualiserDiagram();

            CreateSystemMetaZeroTypes_UX_Additional();

            CreateSystemMetaVisualiser();

            CreateSystemMetaMethodVisualiser();

            CreateSystemData();

            CreateSystemDataVisualiserDiagram(); // out

            CreateSystemDataVisualiserDiagram_ZeroUML(); // out

            CreateSystemMetaStoreFileSystem();

            CreateSystemMetaCommands();

            CreateUserMeta();

            CreateUsers();

            AfterCreateUsers();


            CreateSystemHardware();

            CreateSystemDataUXColor();

            //CreateSystemDataUXZeroUMLTemplate(); // <<<<<<<<<<<<<<<<<<<

            CreateSystemDataUXZeroUMLTemplate_Parent();

            //CreateSystemDataUXZeroUMLTemplate_MSX();


            AddIsAttribute("Attribute");

            AddIsAttribute("Association");

            AddIsAttribute("Aggregation");

            AddIsAggregation();

            CheckAndCorrecIsClass();

            IsInitialized = true;
        }

        public void Initialize()
        {
            MinusZero.Instance.InitializeLog();

            Initialize_PreParserReady();

            // PARSER READY

            Initialize_PostParserReady();

        }
        
    }
}