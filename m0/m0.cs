using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Store;
using m0.Graph;
using m0.TextLanguage;
using m0.Store.FileSystem;
using m0.Util;
using m0.ZeroTypes;
using System.Text.RegularExpressions;

namespace m0
{
    public class MinusZero:IStoreUniverse, IDisposable
    {
        public static bool flag = false; // for debug

        public static MinusZero Instance=new MinusZero();

        public bool IsInitialized = false;

        public AccessLevelEnum[] GetStoreDefaultAccessLevelList=new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions };

        
        IList<IStore> stores=new List<IStore>();

        public IList<IStore> Stores { get { return stores; } }


        IStore tempstore;

        public IStore TempStore { get { return tempstore; } }

        
        IVertex root;

        public IVertex Root { get { return root; } }

        
        IVertex empty;

        public IVertex Empty { get { return empty; } }
        
        IShow _DefaultShow;

        public IShow DefaultShow { get { return _DefaultShow; } }
        
        IParser _DefaultParser;

        public IParser DefaultParser { get { return _DefaultParser; } }


        IExecuter _DefaultExecuter;

        public IExecuter DefaultExecuter { get { return _DefaultExecuter; } }

        IVertex _DefaultLanguageDefinition;

        public IVertex DefaultLanguageDefinition { get { return _DefaultLanguageDefinition; } }


        IVertex _DefaultLanguageDefinition_OLD;

        public IVertex DefaultLanguageDefinition_OLD { get { return _DefaultLanguageDefinition_OLD; } }


        IVertex _MetaTextLanguageParsedTreeVertex;

        public IVertex MetaTextLanguageParsedTreeVertex {get {return _MetaTextLanguageParsedTreeVertex; } }

        
        IZeroCodeGraph2String _DefaultZeroCode2String;

        public IZeroCodeGraph2String DefaultZeroCode2String  { get { return _DefaultZeroCode2String; } }

        public bool IsGUIDragging { get; set; }


        public IVertex CreateTempVertex()
        {
            return new EasyVertex(this.tempstore);
        }

        void Bootstrap()
        {
            IStore rootstore = new MemoryStore("$-0$ROOT$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

            root = rootstore.Root;


            Stores.Clear();

            Stores.Add(rootstore);


            tempstore = new MemoryStore("$-0$TEMP$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions },true);


            empty = new IdentifiedVertex("$Empty", rootstore);

            empty.Value = "$Empty";

            
        }

        void Init(){
            ZeroCode.ZeroCodeEngine_OLD zeroCodeEngine_OLD = new ZeroCode.ZeroCodeEngine_OLD();

            _DefaultShow = m0Main.Instance;

            _DefaultParser = zeroCodeEngine_OLD;

            _DefaultExecuter = zeroCodeEngine_OLD;

            ZeroCode.ZeroCodeEngine zeroCodeEngine = new ZeroCode.ZeroCodeEngine();

            _DefaultZeroCode2String = zeroCodeEngine;            
        }


        void CreateSystem()
        {
            IVertex system=Root.AddVertex(null, "System");

            // turned off for now
            // system.AddVertex(null,"Session").AddVertex(null,"Visualisers");

            IVertex meta = system.AddVertex(null, "Meta");
            
            IVertex tl=system.AddVertex(null, "TextLanguage");

            IVertex mtl = meta.AddVertex(null, "TextLanguage");

            IVertex sto = meta.AddVertex(null, "Store");

            // Meta\TextLanguage\Parser

            IVertex mtp =mtl.AddVertex(null,"Parser");

            IVertex ptmd=mtp.AddVertex(null, "PreviousTerminalMoveDown");

            IVertex mdtpnltoce = mtp.AddVertex(null, "MoveDownToPreviousContainerTerminalOrCretedEmpty");

            IVertex ct = mtp.AddVertex(null, "ContainerTerminal");


            // Meta\TextLanguage\ParsedTree

            IVertex mtpt = mtl.AddVertex(null,"ParsedTree");

            _MetaTextLanguageParsedTreeVertex = mtpt;

            IVertex empty=mtpt.AddVertex(null, "$EmptyContainerTerminal");
            empty.AddVertex(ct, null);

            // TextLanguage\ZeroCode

            IVertex zc = tl.AddVertex(null, "ZeroCode");

            _DefaultLanguageDefinition = zc;


            // TextLanguage\ZeroCode_OLD

            IVertex zco=tl.AddVertex(null, "ZeroCode_OLD");

            _DefaultLanguageDefinition_OLD = zco;
                       
            zco.AddVertex(null, ",");

            IVertex colon=zco.AddVertex(null, ":");
            colon.AddVertex(ptmd,1);
            colon.AddVertex(ct,null);

            zco.AddVertex(null, "\\");
            zco.AddVertex(null, "*");
            zco.AddVertex(null, "{").AddVertex(mdtpnltoce,null);
            zco.AddVertex(null, "}").AddVertex(mdtpnltoce,null);
            zco.AddVertex(null, "=");
            zco.AddVertex(null, "!=");
        }

        void CreateSystemMeta()
        {
            GeneralUtil.ParseAndExcute(Root.Get(@"System\Meta"), null, "{}");
        }

        void CreatePresentation()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Presentation{$Hide,$UpdateAfterInteractionEnd}}");
        }

        void CreateSystemMetaBase()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, null, "{Base{Vertex{$$IsLink,$Inherits,$Is,$EdgeTarget,$VertexTarget,$IsAggregation,$MinCardinality,$MaxCardinality,$MinTargetCardinality,$MaxTargetCardinality,$DefaultValue,$DefaultViewVisualiser,$DefaultEditVisualiser,$DefaultOpenVisualiser,$Group,$Section,$Description,Author,Dependency},$Empty,$Import,$ImportMeta,$Keyword,$KeywordManyRoot,$NewLine}}");

            sm.Get(@"Presentation\$Hide").AddEdge(sm.Get(@"Base\Vertex\$EdgeTarget"), sm.Get(@"Base\Vertex"));

            empty = sm.Get(@"Base\$Empty"); // there are some bugs related to this and old zeroscript.get
      

            sm.Get(@"Base\Vertex\$Is").AddEdge(sm.Get(@"Presentation\$Hide"), empty);

      

            //

            //IVertex _vertex_ = sm.AddVertex(null, "_Vertex_");

            // sm.Get(@"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex")); // TO BE DONE. now there is very strange error in query mechanics

            // sm.Get(@"Base\Vertex\$EdgeTarget").AddEdge(sm.Get(@"Base\Vertex\$EdgeTarget"), _vertex_); // not working too...

            sm.Get(@"Base\Vertex\$VertexTarget").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));

            // sm.Get(@"Base\Vertex\$VertexTarget").AddEdge(sm.Get(@"*$EdgeTarget"), _vertex_);

            sm.Get(@"Base\Vertex\$Inherits").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));

            sm.Get(@"Base\Vertex\$Is").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));


            //sm.Get(@"Base\Vertex\$DefaultViewVisualiser").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));

            //sm.Get(@"Base\Vertex\$DefaultEditVisualiser").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));

            // sm.Get(@"Base\Vertex\$DefaultOpenVisualiser").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));  

            // hack for now

            sm.Get(@"Base\Vertex\$DefaultViewVisualiser").AddEdge(sm.Get(@"*$$IsLink"), sm.Get(@"Base\Vertex"));

            sm.Get(@"Base\Vertex\$DefaultEditVisualiser").AddEdge(sm.Get(@"*$$IsLink"), sm.Get(@"Base\Vertex"));

            sm.Get(@"Base\Vertex\$DefaultOpenVisualiser").AddEdge(sm.Get(@"*$$IsLink"), sm.Get(@"Base\Vertex"));        


            sm.Get(@"Base\Vertex\$IsAggregation").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));  
        }

        void CreateSystemMetaUml()
        {
            IVertex sm=Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, null, "{UML{Type,AtomType,StateMachine{State{Transition}},Enum{EnumValue},Selector,Class{Attribute{MinValue,MaxValue},Association,Aggregation}}}");

            GeneralUtil.ParseAndExcute(sm.Get(@"UML\Selector"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            sm.Get(@"UML\Class\Attribute").AddEdge(sm.Get(@"*$IsAggregation"), empty);

            sm.Get(@"UML\Class\Aggregation").AddEdge(sm.Get(@"*$IsAggregation"), empty);

            GeneralUtil.ParseAndExcute(sm.Get(@"UML\Enum\EnumValue"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            sm.Get(@"UML\Enum\EnumValue").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));

            sm.Get(@"UML\Enum\EnumValue").AddEdge(sm.Get(@"*$IsAggregation"), empty);

            GeneralUtil.ParseAndExcute(sm.Get(@"UML\StateMachine\State"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            GeneralUtil.ParseAndExcute(sm.Get(@"UML\StateMachine\State\Transition"), sm, "{$MinCardinality:0,$MaxCardinality:-1}");

            sm.Get(@"UML\StateMachine\State\Transition").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"UML\StateMachine\State"));


            sm.Get(@"UML\Class").AddEdge(sm.Get("*$Inherits"), sm.Get(@"UML\Type"));

            sm.Get(@"UML\Class").AddEdge(null,sm.Get("*$Inherits"));
            
           

            Root.Get(@"System\Meta\UML\Class\Attribute").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"UML\Selector"));
            Root.Get(@"System\Meta\UML\Class\Attribute").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"UML\Type"));

            Root.Get(@"System\Meta\UML\Class\Association").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"UML\Selector"));            
            Root.Get(@"System\Meta\UML\Class\Association").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"UML\Class"));

            Root.Get(@"System\Meta\UML\Class\Aggregation").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"UML\Selector"));
            Root.Get(@"System\Meta\UML\Class\Aggregation").AddEdge(sm.Get(@"*$VertexTarget"), sm.Get(@"UML\Class"));


           // sm.Get(@"UML\Type").AddEdge(sm.Get("*$Inherits"),sm.Get(@"Base\Vertex"));    // do not want it at last for now        

            sm.Get(@"UML\AtomType").AddEdge(sm.Get("*$Inherits"),sm.Get(@"UML\Type"));
            sm.Get(@"UML\Enum").AddEdge(sm.Get("*$Inherits"),sm.Get(@"UML\AtomType")); // was UML\Type
            sm.Get(@"UML\StateMachine").AddEdge(sm.Get("*$Inherits"), sm.Get(@"UML\AtomType"));
        }

        void CreateSystemMetaUml_Action_part()
        {
            IVertex smu = Root.Get(@"System\Meta\UML");
            IVertex sm = Root.Get(@"System\Meta");

            IVertex isAggregation = Root.Get(@"System\Meta\Base\Vertex\$IsAggregation");
            
            // "\ " > "\"
            // "|" > ":"
            // "||" > "::"        

            GeneralUtil.ParseAndExcute(smu, sm,
                "{Expression,Atom"+
                ",SingleOperator{TargetExpression{$MinCardinality:1,$MaxCardinality:1}}"+
                ",DoubleOperator{LeftExpression{$MinCardinality:1,$MaxCardinality:1},RightExpression{$MinCardinality:1,$MaxCardinality:1}}" +
                ",MultiOperator{TargetExpression{$MinCardinality:1,$MaxCardinality:1}}" +
                ",Value,NewVertex" +
                ",[]" +
                ",[[]]" +
                ",\"{}\"{TargetExpression},+,-,\"* \",/,?,\"\\ \",\"|\",\"||\",<-,--" +
                ",Action,Return{Expression},NextOut{Next{$MinCardinality:0,$MaxCardinality:1}}"+
                ",StackFrameCreator{Do{$MinCardinality:0,$MaxCardinality:1},Variable{$MinCardinality:0,$MaxCardinality:-1},Type{$MinCardinality:0,$MaxCardinality:-1}}" +
                ",StackFrameCreatorWithInputOutput{Output{$MinCardinality:0,$MaxCardinality:1},InputParameter{$MinCardinality:0,$MaxCardinality:-1}}"+
                ",Function,Section" +      
                ",If{Test{$MinCardinality:1,$MaxCardinality:1},Then{$MinCardinality:0,$MaxCardinality:1},Else{$MinCardinality:0,$MaxCardinality:1}}"+
                ",Switch{Expression{$MinCardinality:1,$MaxCardinality:1},Case{Expression{$MinCardinality:1,$MaxCardinality:1}},Default}"+
                ",While{Test{$MinCardinality:1,$MaxCardinality:1},Do{$MinCardinality:0,$MaxCardinality:1}}"+
                ",ForEach{Variable{$MinCardinality:0,$MaxCardinality:1},Set{$MinCardinality:1,$MaxCardinality:1},Do{$MinCardinality:0,$MaxCardinality:1}}" + 
                "}");
            
            // method
            IVertex method=sm.Get(@"UML\Class").AddVertex(null, "Method");
            method.AddEdge(sm.Get("*$Inherits"), smu.Get("StackFrameCreatorWithInputOutput"));
            GeneralUtil.ParseAndExcute(method,sm,"{$MinCardinality: 0,$MaxCardinality: -1}");

            // cycle edges
            smu.Get(@"StackFrameCreator").AddEdge(null, smu.Get("Function"));


            // expression inherits
            smu.Get(@"Expression").AddEdge(sm.Get("*$Inherits"), smu.Get("Atom"));
            smu.Get(@"Expression").AddEdge(sm.Get("*$Inherits"), smu.Get("NextOut"));
            smu.Get(@"SingleOperator").AddEdge(sm.Get("*$Inherits"), smu.Get("Expression"));
            smu.Get(@"DoubleOperator").AddEdge(sm.Get("*$Inherits"), smu.Get("Expression"));
            smu.Get(@"MultiOperator").AddEdge(sm.Get("*$Inherits"), smu.Get("Expression"));

            smu.Get(@"Value").AddEdge(sm.Get("*$Inherits"), smu.Get("SingleOperator"));
            smu.Get(@"NewVertex").AddEdge(sm.Get("*$Inherits"), smu.Get("SingleOperator"));
            smu.Get(@"[]").AddEdge(sm.Get("*$Inherits"), smu.Get("MultiOperator"));
            smu.Get(@"[[]]").AddEdge(sm.Get("*$Inherits"), smu.Get("MultiOperator"));
            smu.Get("\"{}\"").AddEdge(sm.Get("*$Inherits"), smu.Get("MultiOperator"));
            smu.Get(@"+").AddEdge(sm.Get("*$Inherits"), smu.Get("DoubleOperator"));
            smu.Get(@"-").AddEdge(sm.Get("*$Inherits"), smu.Get("DoubleOperator"));
            smu.Get("\"* \"").AddEdge(sm.Get("*$Inherits"), smu.Get("DoubleOperator"));
            smu.Get(@"/").AddEdge(sm.Get("*$Inherits"), smu.Get("DoubleOperator"));
            smu.Get(@"?").AddEdge(sm.Get("*$Inherits"), smu.Get("SingleOperator"));
            smu.Get("\"\\ \"").AddEdge(sm.Get("*$Inherits"), smu.Get("SingleOperator"));
            smu.Get("\"|\"").AddEdge(sm.Get("*$Inherits"), smu.Get("SingleOperator"));
            smu.Get("\"||\"").AddEdge(sm.Get("*$Inherits"), smu.Get("DoubleOperator"));
            smu.Get(@"<-").AddEdge(sm.Get("*$Inherits"), smu.Get("DoubleOperator"));
            smu.Get(@"--").AddEdge(sm.Get("*$Inherits"), smu.Get("DoubleOperator"));

           // string s = Regex.Escape(@"[]{}+-*/?\:::<---()");
            
            // rest inherits
            smu.Get(@"Action").AddEdge(sm.Get("*$Inherits"), smu.Get("Atom"));

            smu.Get(@"StackFrameCreatorWithInputOutput").AddEdge(sm.Get("*$Inherits"), smu.Get("StackFrameCreator"));

            smu.Get(@"Return").AddEdge(sm.Get("*$Inherits"), smu.Get("Action"));
            smu.Get(@"Return").AddEdge(sm.Get("*$Inherits"), smu.Get("NextOut"));

            smu.Get(@"Section").AddEdge(sm.Get("*$Inherits"), smu.Get("NextOut"));
            smu.Get(@"Section").AddEdge(sm.Get("*$Inherits"), smu.Get("Action"));
            smu.Get(@"Section").AddEdge(sm.Get("*$Inherits"), smu.Get("StackFrameCreator")); 
                       
            smu.Get(@"If").AddEdge(sm.Get("*$Inherits"), smu.Get("Action"));
         
            smu.Get(@"If\Then").AddEdge(sm.Get(@"*$Inherits"), smu.Get(@"NextOut"));
            smu.Get(@"If\Else").AddEdge(sm.Get(@"*$Inherits"), smu.Get(@"NextOut"));

            smu.Get(@"Switch").AddEdge(sm.Get("*$Inherits"), smu.Get("Action"));
            smu.Get(@"Switch\Case").AddEdge(sm.Get("*$Inherits"), smu.Get("NextOut"));
            smu.Get(@"Switch\Default").AddEdge(sm.Get("*$Inherits"), smu.Get("NextOut"));

            smu.Get(@"While").AddEdge(sm.Get("*$Inherits"), smu.Get("NextOut"));
            smu.Get(@"While").AddEdge(sm.Get("*$Inherits"), smu.Get("Action"));

            smu.Get(@"ForEach").AddEdge(sm.Get("*$Inherits"), smu.Get("NextOut"));
            smu.Get(@"ForEach").AddEdge(sm.Get("*$Inherits"), smu.Get("Action"));

            smu.Get(@"Function").AddEdge(sm.Get("*$Inherits"), smu.Get("StackFrameCreatorWithInputOutput"));

            //expression edges
            smu.Get(@"StackFrameCreatorWithInputOutput\InputParameter").AddEdge(sm.Get(@"*$VertexTarget"), smu.Get(@"Type"));
            
            smu.Get(@"SingleOperator\TargetExpression").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"DoubleOperator\LeftExpression").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"DoubleOperator\RightExpression").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"MultiOperator\TargetExpression").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));

            // $IsAggregation's for EdgeTargets
            smu.Get(@"SingleOperator\TargetExpression").AddEdge(isAggregation, Empty);
            smu.Get(@"DoubleOperator\LeftExpression").AddEdge(isAggregation, Empty);
            smu.Get(@"DoubleOperator\RightExpression").AddEdge(isAggregation, Empty);
            smu.Get(@"MultiOperator\TargetExpression").AddEdge(isAggregation, Empty);


            //rest edges
            smu.Get(@"Return\Expression").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"NextOut\Next").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));

            smu.Get(@"StackFrameCreator\Do").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"StackFrameCreator\Variable").AddEdge(sm.Get(@"*$VertexTarget"), smu.Get(@"Type"));
            smu.Get(@"StackFrameCreator\Type").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Type"));
            smu.Get(@"StackFrameCreatorWithInputOutput\Output").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Type"));

            smu.Get(@"If\Test").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));

            smu.Get(@"Switch\Expression").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"Switch\Case\Expression").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            
            smu.Get(@"While\Test").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"While\Do").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));

            // smu.Get(@"ForEach\Variable").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Value")); // NO NO. NO NO NO NO
            smu.Get(@"ForEach\Set").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));
            smu.Get(@"ForEach\Do").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Atom"));

            // $IsAggregation's for EdgeTargets
            smu.Get(@"Return\Expression").AddEdge(isAggregation, Empty);
            smu.Get(@"NextOut\Next").AddEdge(isAggregation, Empty);

            smu.Get(@"StackFrameCreator\Do").AddEdge(isAggregation, Empty);
            smu.Get(@"StackFrameCreator\Variable").AddEdge(isAggregation, Empty);
            smu.Get(@"StackFrameCreator\Type").AddEdge(isAggregation, Empty);
          //  smu.Get(@"StackFrameCreatorWithInputOutput\Output").AddEdge(isAggregation, Empty); // this - no!

            smu.Get(@"If\Test").AddEdge(isAggregation, Empty);

            smu.Get(@"Switch\Expression").AddEdge(isAggregation, Empty);
            smu.Get(@"Switch\Case\Expression").AddEdge(isAggregation, Empty);

            smu.Get(@"While\Test").AddEdge(isAggregation, Empty);
            smu.Get(@"While\Do").AddEdge(isAggregation, Empty);

            // smu.Get(@"ForEach\Variable").AddEdge(sm.Get(@"*$EdgeTarget"), smu.Get(@"Value")); // NO NO. NO NO NO NO
            smu.Get(@"ForEach\Set").AddEdge(isAggregation, Empty);
            smu.Get(@"ForEach\Do").AddEdge(isAggregation, Empty);

            // package
            IVertex package = smu.AddVertex(null, "Package");

           
            package.AddEdge(null, smu.Get("AtomType"));
            package.AddEdge(null, smu.Get("StateMachine"));
            package.AddEdge(null, smu.Get("Enum"));
            package.AddEdge(null, smu.Get("Class"));
            package.AddEdge(null, smu.Get("Value"));
            package.AddEdge(null, smu.Get("NewVertex"));
            package.AddEdge(null, smu.Get("[]"));
            package.AddEdge(null, smu.Get("[[]]"));
            package.AddEdge(null, smu.Get("\"{}\""));
            package.AddEdge(null, smu.Get("+"));
            package.AddEdge(null, smu.Get("-"));
            package.AddEdge(null, smu.Get("* "));
            package.AddEdge(null, smu.Get("/"));
            package.AddEdge(null, smu.Get("?"));
            package.AddEdge(null, smu.Get("\"\\ \""));
            package.AddEdge(null, smu.Get("\"|\""));
            package.AddEdge(null, smu.Get("\"||\""));
            package.AddEdge(null, smu.Get("<-"));
            package.AddEdge(null, smu.Get("--"));            
            package.AddEdge(null, smu.Get("Section"));
            package.AddEdge(null, smu.Get("Function"));
            package.AddEdge(null, smu.Get("If"));
            package.AddEdge(null, smu.Get("Switch"));
            package.AddEdge(null, smu.Get("While"));
            package.AddEdge(null, smu.Get("ForEach"));
            package.AddEdge(null, sm.Get(@"Base\$Import"));
            package.AddEdge(null, sm.Get(@"Base\$ImportMeta"));
        }

        void CreateSystemUMLKeywords()
        {
            IVertex smu = Root.Get(@"System\Meta\UML");
            IVertex smb = Root.Get(@"System\Meta\Base");

            IVertex keyword = smb.Get(@"$Keyword");

            IVertex isAggregation = root.Get(@"System\Meta\Base\Vertex\$IsAggregation");
            IVertex empty = root.Get(@"System\Meta\Base\$Empty");
            IVertex _is = root.Get(@"System\Meta\Base\Vertex\$Is");

            IVertex smuk = smu.AddVertex(null, "Keyword");

            IVertex any = smuk.AddVertex(null, "(?<ANY>)");
            
            // import meta
            //
            // import meta (?<name>) (?<link>)
            
            IVertex importMeta = smuk.AddVertex(keyword, "import meta (?<name>) (?<link>)");

            IVertex importMeta_name=importMeta.AddVertex(smb.Get(@"$ImportMeta"), "(?<name>)");

            importMeta.AddVertex(importMeta_name, "(?<link>)");


            // import
            //
            // import (?<name>) (?<link>)

            IVertex import = smuk.AddVertex(keyword, "import (?<name>) (?<link>)");

            IVertex import_name = import.AddVertex(smb.Get(@"$Import"), "(?<name>)");

            import.AddVertex(import_name, "(?<link>)");

            // import direct 
            //
            // import direct  (?<link>)

            IVertex importDirect = smuk.AddVertex(keyword, "import direct (?<link>)");

            IVertex importDirect_link = importDirect.AddVertex(smb.Get(@"$Direct"), "(?<link>)");




            // import direct meta
            //
            // import direct meta (?<link>)

            IVertex importDirectMeta = smuk.AddVertex(keyword, "import direct meta (?<link>)");

            IVertex importDirectMeta_link = importDirectMeta.AddVertex(smb.Get(@"$DirectMeta"), "(?<link>)");
                        

            // comment
            //
            // # (?<text>)

            IVertex comment = smuk.AddVertex(keyword, "# (?<text>)");

            comment.AddVertex(smb.Get(@"Vertex\$Description"), "(?<text>)");

            
            // attribute
            //
            // attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) <(?<MinValue>):(?<MaxValue>)>

            IVertex attribute3 = smuk.AddVertex(keyword, "attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) <(?<MinValue>):(?<MaxValue>)>");


            IVertex attribute3_attribute = attribute3.AddVertex(smu.Get(@"Class\Attribute"), "(?<name>)");

            attribute3_attribute.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            attribute3_attribute.AddVertex(smu.Get(@"Class\Attribute\MinValue"), "(?<MinValue>)");

            attribute3_attribute.AddVertex(smu.Get(@"Class\Attribute\MaxValue"), "(?<MaxValue>)");

            attribute3_attribute.AddVertex(smb.Get(@"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            attribute3_attribute.AddVertex(smb.Get(@"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            attribute3_attribute.AddEdge(isAggregation, empty);

            attribute3_attribute.AddEdge(_is, smu.Get(@"Class\Attribute"));

            // attribute
            //
            // attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) 

            IVertex attribute4 = smuk.AddVertex(keyword, "attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");


            IVertex attribute4_attribute = attribute4.AddVertex(smu.Get(@"Class\Attribute"), "(?<name>)");

            attribute4_attribute.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            attribute4_attribute.AddVertex(smb.Get(@"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            attribute4_attribute.AddVertex(smb.Get(@"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            attribute4_attribute.AddEdge(isAggregation, empty);

            attribute4_attribute.AddEdge(_is, smu.Get(@"Class\Attribute"));


            // attribute
            //
            // attribute (?<name>) (?<type>) <(?<MinValue>):(?<MaxValue>)>

            IVertex attribute2 = smuk.AddVertex(keyword, "attribute (?<name>) (?<type>) <(?<MinValue>):(?<MaxValue>)>");


            IVertex attribute2_attribute = attribute2.AddVertex(smu.Get(@"Class\Attribute"), "(?<name>)");

            attribute2_attribute.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            attribute2_attribute.AddVertex(smu.Get(@"Class\Attribute\MinValue"), "(?<MinValue>)");

            attribute2_attribute.AddVertex(smu.Get(@"Class\Attribute\MaxValue"), "(?<MaxValue>)");

            attribute2_attribute.AddEdge(isAggregation, empty);

            attribute2_attribute.AddEdge(_is, smu.Get(@"Class\Attribute"));
            
            // attribute
            //
            // attribute (?<name>) (?<type>)

            IVertex attribute = smuk.AddVertex(keyword, "attribute (?<name>) (?<type>)");


            IVertex attribute_attribute = attribute.AddVertex(smu.Get(@"Class\Attribute"), "(?<name>)");

            attribute_attribute.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            attribute_attribute.AddEdge(isAggregation, empty);

            attribute_attribute.AddEdge(_is, smu.Get(@"Class\Attribute"));
            /*
            // aassociation
            //
            // association (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex association2 = smuk.AddVertex(keyword, "association (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");


            IVertex association2_association = association2.AddVertex(smu.Get(@"Class\Association"), "(?<name>)");

            association2_association.AddVertex(smb.Get(@"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            association2_association.AddVertex(smb.Get(@"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            association2_association.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            association2_association.AddEdge(_is, smu.Get(@"Class\Association"));


            // aassociation
            //
            // association (?<name>) (?<type>)

            IVertex association = smuk.AddVertex(keyword, "association (?<name>) (?<type>)");


            IVertex association_association = association.AddVertex(smu.Get(@"Class\Association"), "(?<name>)");

            association_association.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            association_association.AddEdge(_is, smu.Get(@"Class\Association"));

            // aggregation
            //
            // aggregation (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)

            IVertex aggregation2 = smuk.AddVertex(keyword, "aggregation (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>)");


            IVertex aggregation2_aggregation = aggregation2.AddVertex(smu.Get(@"Class\Aggregation"), "(?<name>)");

            aggregation2_aggregation.AddVertex(smb.Get(@"Vertex\$MinCardinality"), "(?<MinCardinality>)");

            aggregation2_aggregation.AddVertex(smb.Get(@"Vertex\$MaxCardinality"), "(?<MaxCardinality>)");

            aggregation2_aggregation.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            aggregation2_aggregation.AddEdge(_is, smu.Get(@"Class\Aggregation"));


            // aggregation
            //
            // aggregation (?<name>) (?<type>)

            IVertex aggregation = smuk.AddVertex(keyword, "aggregation (?<name>) (?<type>)");


            IVertex aggregation_aggregation = aggregation.AddVertex(smu.Get(@"Class\Aggregation"), "(?<name>)");

            aggregation_aggregation.AddVertex(smb.Get(@"Vertex\$EdgeTarget"), "(?<type>)");

            aggregation_aggregation.AddEdge(_is, smu.Get(@"Class\Aggregation"));
            
            // function
            //
            // function (?<name>) (?<returnType>) [(*(+, +)(?<paramType>) (?<paramName>)*)]

            IVertex function = smuk.AddVertex(keyword, "function (?<name>) (?<returnType>) [(*(+, +)(?<paramType>) (?<paramName>)*)]");


            IVertex function_function = function.AddVertex(smu.Get(@"Function"), "(?<name>)");

            function_function.AddVertex(smu.Get(@"Function\Output"), "(?<returnType>)");

            IVertex ffip = function_function.AddVertex(smu.Get(@"Function\InputParameter"), "(?<paramName>)");

            ffip.AddVertex(smb.Get(@"Vertex\$VertexTarget"), "(?<paramType>)");

            ffip.AddEdge(smb.Get(@"$KeywordManyRoot"), smb.Get(@"$Empty"));


            // function
            //
            // function (?<name>) [(*(+, +)(?<paramType>) (?<paramName>)*)]
            
            IVertex function2 = smuk.AddVertex(keyword, "function (?<name>) [(*(+, +)(?<paramType>) (?<paramName>)*)]");


            IVertex function2_function = function2.AddVertex(smu.Get(@"Function"), "(?<name>)");            

            IVertex f2fip = function2_function.AddVertex(smu.Get(@"Function\InputParameter"), "(?<paramName>)");

            f2fip.AddVertex(smb.Get(@"Vertex\$VertexTarget"), "(?<paramType>)");            

            f2fip.AddEdge(smb.Get(@"$KeywordManyRoot"),smb.Get(@"$Empty"));

                        

            // while
            //
            // while ((?<test>))

            IVertex wh = smuk.AddVertex(keyword, "while (?<test>)");

            IVertex whwh = wh.AddVertex(smu.Get(@"While"), null);

            whwh.AddEdge(smb.Get(@"Vertex\$Is"), smu.Get("While"));

            whwh.AddVertex(smu.Get(@"While\Test"), "(?<test>)");
            */
            // +
            //
            // (?<left>) + (?<right>)

            // IVertex o_plus = smuk.AddVertex(keyword, "(?<left>) +(?<SUB>) (?<right>)");

            IVertex o_plus = smuk.AddVertex(keyword, "(?<left>) + (?<right>)");

            IVertex o_plus_any = o_plus.AddVertex(any, "");

            o_plus_any.AddEdge(smb.Get(@"Vertex\$Is"), smu.Get("+"));

            o_plus_any.AddVertex(smu.Get(@"DoubleOperator\LeftExpression"), "(?<left>)");

            o_plus_any.AddVertex(smu.Get(@"DoubleOperator\RightExpression"), "(?<right>)");
            /*
            // -
            //
            // (?<left>) - (?<right>)

            IVertex o_minus = smuk.AddVertex(keyword, "(?<left>) -(?<SUB>) (?<right>)");

            IVertex o_minus_any = o_minus.AddVertex(any, "");

            o_minus_any.AddEdge(smb.Get(@"Vertex\$Is"), smu.Get("-"));

            o_minus_any.AddVertex(smu.Get(@"DoubleOperator\LeftExpression"), "(?<left>)");

            o_minus_any.AddVertex(smu.Get(@"DoubleOperator\RightExpression"), "(?<right>)");



            // []
            //
            // [(*(+, +) (?<expr>)*)]

            IVertex o_call = smuk.AddVertex(keyword, "[(*(+, +)(?<expr>)*)]");

            IVertex o_call_any = o_call.AddVertex(any, "");

            o_call_any.AddEdge(smb.Get(@"Vertex\$Is"), smu.Get("[]"));

            IVertex o_call_any_param=o_call_any.AddVertex(smu.Get(@"MultiOperator\Expression"), "(?<expr>)");

            o_call_any_param.AddEdge(smb.Get(@"$KeywordManyRoot"), smb.Get(@"$Empty"));
            */
        }

        void CreateSystemTextLanguageZeroCode()
        {
            IVertex zc = Root.Get(@"System\TextLanguage\ZeroCode");

            IVertex b = Root.Get(@"System\Meta\Base");

            IVertex di=zc.AddVertex(null, "DefaultImports");

            //

            IVertex DirectMeta = VertexOperations.AddInstance(b,Root.Get(@"System\Meta\Base\$ImportMeta"));

            DirectMeta.Value = "$DirectMeta";

            DirectMeta.AddEdge(Root.Get(@"System\Meta\Base\Vertex\$$IsLink"), Empty);

            di.AddEdge(DirectMeta, Root.Get(@"System\Meta\UML"));

            di.AddEdge(DirectMeta, Root.Get(@"System\Meta\Base"));

            di.AddEdge(DirectMeta, Root.Get(@"System\Meta\Base\Vertex"));

            di.AddEdge(DirectMeta, Root.Get(@"System\Meta\ZeroTypes"));

            //

            IVertex Direct = VertexOperations.AddInstance(b, Root.Get(@"System\Meta\Base\$Import"));

            Direct.Value = "$Direct";

            Direct.AddEdge(Root.Get(@"System\Meta\Base\Vertex\$$IsLink"), Empty);

            //di.AddEdge(Direct, Root); // :O) now its hanging

            //

            IVertex System = VertexOperations.AddInstance(di, Root.Get(@"System\Meta\Base\$ImportMeta"));

            System.Value = "System";

            System.AddEdge(Root.Get(@"System\Meta\Base\Vertex\$$IsLink"), Empty);

            di.AddEdge(System, Root.Get(@"System"));
        }

        void CreateSystemMetaZeroTypes()
        {
            IVertex sm = Root.Get(@"System\Meta");

            
            GeneralUtil.ParseAndExcute(sm, sm, "{ZeroTypes{AtomType:String,AtomType:Integer,AtomType:Decimal,AtomType:Float,AtomType:Boolean,Vertex:VertexType,Class:Edge{Association:From{$MinCardinality:0,$MaxCardinality:1},Association:Meta{$MinCardinality:1,$MaxCardinality:1},Association:To{$MinCardinality:1,$MaxCardinality:1}},Class:DateTime{Attribute:Year{$MinCardinality:1,$MaxCardinality:1},Attribute:Month{$MinCardinality:1,$MaxCardinality:1},Attribute:Day{$MinCardinality:1,$MaxCardinality:1},Attribute:Hour{$MinCardinality:1,$MaxCardinality:1},Attribute:Minute{$MinCardinality:1,$MaxCardinality:1},Attribute:Second{$MinCardinality:1,$MaxCardinality:1},Attribute:Millisecond{$MinCardinality:0,$MaxCardinality:1}},Enum:EnumBase,Class:$PlatformClass{$PlatformClassName},Class:HasBaseEdge{Attribute:BaseEdge{$MinCardinality:1,$MaxCardinality:1}},Class:HasSelectedEdges{Attribute:SelectedEdges{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:}},Class:HasFilter{Attribute:FilterQuery{$MinCardinality:0,$MaxCardinality:1}},Class:Color{Attribute:Red{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Green{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Blue{MinValue:0,MaxValue:255,$MinCardinality:1,$MaxCardinality:1},Attribute:Opacity{MinValue:0,MaxValue:255,$MinCardinality:0,$MaxCardinality:1}}}}");

            sm.Get(@"ZeroTypes\String").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Integer").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Decimal").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Float").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));
            sm.Get(@"ZeroTypes\Boolean").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\AtomType"));            
            sm.Get(@"ZeroTypes\VertexType").AddEdge(sm.Get(@"*$Is"), sm.Get(@"Base\Vertex"));            
            sm.Get(@"ZeroTypes\Edge").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\EnumBase").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Enum"));
            sm.Get(@"ZeroTypes\DateTime").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));            
            sm.Get(@"ZeroTypes\HasBaseEdge").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));            
            sm.Get(@"ZeroTypes\HasSelectedEdges").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\HasFilter").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\$PlatformClass").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Color").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));

            sm.Get(@"ZeroTypes\EnumBase").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Base\Vertex"));

            sm.Get(@"ZeroTypes\DateTime\Year").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Month").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Day").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Hour").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Minute").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Second").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\DateTime\Millisecond").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));

            sm.Get(@"ZeroTypes\Edge\From").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            sm.Get(@"ZeroTypes\Edge\Meta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            sm.Get(@"ZeroTypes\Edge\To").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
           
            sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Edge"));
            sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge").AddVertex(sm.Get(@"*$Section"), "Base");

            sm.Get(@"ZeroTypes\HasSelectedEdges\SelectedEdges").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            sm.Get(@"ZeroTypes\HasFilter\FilterQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));

            sm.Get(@"ZeroTypes\Color\Red").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\Color\Green").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\Color\Blue").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"ZeroTypes\Color\Opacity").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
        }

        void CreateSystemMetaVisualiserDiagram()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Visualiser}");

            IVertex smv = Root.Get(@"System\Meta\Visualiser");

            GeneralUtil.ParseAndExcute(smv, sm, "{DiagramInternal{Class:DiagramItemBase{Association:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionX{$MinCardinality:1,$MaxCardinality:1},Attribute:PositionY{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:DiagramLine{$MinCardinality:0,$MaxCardinality:-1},OptionEdge,OptionDiagramLineDefinition},Class:DiagramItemDefinition{Attribute:DirectVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Attribute:MetaVertexTestQuery{$MinCardinality:0,$MaxCardinality:1},Association:DiagramItemClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramItemVertex{$MinCardinality:0,$MaxCardinality:1},Association:InstanceCreation{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineDefinition{$MinCardinality:0,$MaxCardinality:-1},Attribute:DoNotShowInherited{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1}},Enum:InstanceCreationEnum{EnumValue:Instance,EnumValue:InstanceAndDirect,EnumValue:Direct},Class:DiagramLineBase{Association:Definition{$MinCardinality:1,$MaxCardinality:1},Attribute:LineWidth{MinValue:1,MaxValue:10,$MinCardinality:0,$MaxCardinality:1},Attribute:ForegroundColor{$MinCardinality:0,$MaxCardinality:1},Attribute:BackgroundColor{$MinCardinality:0,$MaxCardinality:1},Association:ToDiagramItem{$MinCardinality:1,$MaxCardinality:1}},Class:DiagramLineDefinition{Attribute:EdgeTestQuery{$MinCardinality:1,$MaxCardinality:1},Attribute:ToDiagramItemTestQuery{$MinCardinality:0,$MaxCardinality:1},Association:DiagramLineClass{$MinCardinality:1,$MaxCardinality:1},Attribute:DiagramLineVertex{$MinCardinality:0,$MaxCardinality:1},Attribute:CreateEdgeOnly{$MinCardinality:0,$MaxCardinality:1},Attribute:ForceShowEditForm{$MinCardinality:0,$MaxCardinality:1}},Class:DiagramImageItem{Attribute:Filename},Class:DiagramOvalItem,Class:DiagramRhombusItem,Class:DiagramRectangleItem{Attribute:ShowMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:RoundEdgeSize{MinValue:0,MaxValue:200,$MinCardinality:0,$MaxCardinality:1},Association:VisualiserClass{$MinCardinality:0,$MaxCardinality:1},Attribute:VisualiserVertex{$MinCardinality:0,$MaxCardinality:1}},Class:DiagramLine{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}},Enum:LineEndEnum{EnumValue:Straight,EnumValue:Arrow,EnumValue:Triangle,EnumValue:FilledTriangle,EnumValue:Diamond,EnumValue:FilledDiamond},Class:DiagramMetaExtendedLine{Association:StartAnchor{$MinCardinality:0,$MaxCardinality:1},Association:EndAnchor{$MinCardinality:0,$MaxCardinality:1},Attribute:IsDashed{$MinCardinality:0,$MaxCardinality:1}}}}");

            smv.Get(@"DiagramInternal\InstanceCreationEnum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));
            smv.Get(@"DiagramInternal\LineEndEnum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));

            smv.Get(@"DiagramInternal\DiagramItemBase").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramItemBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            smv.Get(@"DiagramInternal\DiagramItemBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            
            smv.Get(@"DiagramInternal\DiagramItemBase\Definition").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramItemDefinition"));
            IVertex definitionSection = smv.Get(@"DiagramInternal\DiagramItemBase\Definition").AddVertex(sm.Get(@"*$Section"), "Definition");
            
            smv.Get(@"DiagramInternal\DiagramItemBase\PositionX").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            IVertex positionAndSizeSection = smv.Get(@"DiagramInternal\DiagramItemBase\PositionX").AddVertex(sm.Get(@"*$Section"), "Position and size");

            smv.Get(@"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramItemBase\PositionY").AddEdge(sm.Get(@"*$Section"), positionAndSizeSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramItemBase\SizeX").AddEdge(sm.Get(@"*$Section"), positionAndSizeSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramItemBase\SizeY").AddEdge(sm.Get(@"*$Section"), positionAndSizeSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\LineWidth").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            IVertex lookSection = smv.Get(@"DiagramInternal\DiagramItemBase\LineWidth").AddVertex(sm.Get(@"*$Section"), "Look");

            smv.Get(@"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramItemBase\BackgroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramItemBase\ForegroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramItemBase\DiagramLine").AddEdge(sm.Get(@"*$Section"), sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));
            


            smv.Get(@"DiagramInternal\DiagramItemDefinition").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DirectVertexTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\MetaVertexTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DiagramItemClass").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DiagramItemVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\InstanceCreation").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\InstanceCreationEnum"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DiagramLineDefinition").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineDefinition"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\DoNotShowInherited").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            smv.Get(@"DiagramInternal\DiagramItemDefinition\ForceShowEditForm").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            smv.Get(@"DiagramInternal\DiagramLineBase").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramLineBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            smv.Get(@"DiagramInternal\DiagramLineBase").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));

            smv.Get(@"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineDefinition"));
            smv.Get(@"DiagramInternal\DiagramLineBase\Definition").AddEdge(sm.Get(@"*$Section"), definitionSection);
            
            smv.Get(@"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            smv.Get(@"DiagramInternal\DiagramLineBase\LineWidth").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramLineBase\BackgroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Color"));
            smv.Get(@"DiagramInternal\DiagramLineBase\ForegroundColor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramLineBase\ToDiagramItem").AddEdge(sm.Get(@"*$Section"), sm.Get(@"ZeroTypes\HasBaseEdge\BaseEdge\$Section:"));

            smv.Get(@"DiagramInternal\DiagramLineDefinition").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\EdgeTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\ToDiagramItemTestQuery").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\DiagramLineClass").AddEdge(sm.Get(@"*$EdgeTarget"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\DiagramLineVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\CreateEdgeOnly").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            smv.Get(@"DiagramInternal\DiagramLineDefinition\ForceShowEditForm").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            smv.Get(@"DiagramInternal\DiagramImageItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramImageItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramImageItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramImageItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            smv.Get(@"DiagramInternal\DiagramImageItem\Filename").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\String"));

            smv.Get(@"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramOvalItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramOvalItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramOvalItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(@"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramRhombusItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramRhombusItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRhombusItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramItemBase"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramRectangleItem, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"UML\Class"));
            IVertex visualiserSection = smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserClass").AddVertex(sm.Get(@"*$Section"), "Visualiser");
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem\VisualiserVertex").AddEdge(sm.Get(@"*$Section"), visualiserSection);
            
            
            smv.Get(@"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem\RoundEdgeSize").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            smv.Get(@"DiagramInternal\DiagramRectangleItem\ShowMeta").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramLine").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramLine").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramLine").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            smv.Get(@"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramLine\StartAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);
            
            smv.Get(@"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramLine\EndAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);
            
            smv.Get(@"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Boolean"));
            smv.Get(@"DiagramInternal\DiagramLine\IsDashed").AddEdge(sm.Get(@"*$Section"), lookSection);


            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine").AddEdge(sm.Get("*$Inherits"), smv.Get(@"DiagramInternal\DiagramLineBase"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.DiagramMetaExtendedLine, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\StartAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*LineEndEnum"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\EndAnchor").AddEdge(sm.Get(@"*$Section"), lookSection);

            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Boolean"));
            smv.Get(@"DiagramInternal\DiagramMetaExtendedLine\IsDashed").AddEdge(sm.Get(@"*$Section"), lookSection);
        }

        void CreateSystemMetaVisualiser()
        {
            IVertex sm = Root.Get(@"System\Meta");

            IVertex smv = Root.Get(@"System\Meta\Visualiser");

            GeneralUtil.ParseAndExcute(smv, sm, "{Enum:GridStyleEnum{EnumValue:None,EnumValue:Vertical,EnumValue:Horizontal,EnumValue:All,EnumValue:AllAndRound,EnumValue:Round},Class:Form{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:ExpertMode{$MinCardinality:0,$MaxCardinality:1},Attribute:ColumnNumber{$MinCardinality:0,$MaxCardinality:1,$UpdateAfterInteractionEnds:},Attribute:MetaOnLeft{$MinCardinality:0,$MaxCardinality:1},Attribute:SectionsAsTabs{$MinCardinality:0,$MaxCardinality:1},Attribute:TableVisualiserVertex{$MinCardinality:0,$MaxCardinality:1}},Class:Code{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowWhiteSpace{$MinCardinality:0,$MaxCardinality:1},Attribute:HighlightedLine{$MinCardinality:0,$MaxCardinality:1}},Class:Table{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:ExpertMode{$MinCardinality:0,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}},Class:TableFast{Attribute:ToShowEdgesMeta{$MinCardinality:0,$MaxCardinality:1},Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1},Attribute:AlternatingRows{$MinCardinality:1,$MaxCardinality:1}},Class:Tree{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1}},Class:Graph{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:VisualiserCircleSize{$MinCardinality:1,$MaxCardinality:1},Attribute:NumberOfCircles{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd:},Attribute:ShowInEdges{$MinCardinality:1,$MaxCardinality:1},Attribute:FastMode{$MinCardinality:1,$MaxCardinality:1},Attribute:MetaLabels{$MinCardinality:1,$MaxCardinality:1}},Class:Class,Class:String,Class:StringView,Class:Vertex,Class:Edge,Class:Integer,Class:Decimal,Class:Float,Class:Boolean,Class:Enum,Class:Diagram{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1,MinValue:0,MaxValue:200,$DefaultValue:100},Attribute:SizeX{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd:},Attribute:SizeY{$MinCardinality:1,$MaxCardinality:1,$UpdateAfterInteractionEnd},Attribute:Item{$MinCardinality:0,$MaxCardinality:-1,$Hide:},Association:CreationPool{$MinCardinality:1,$MaxCardinality:1}},Class:Wrap,Class:List{Attribute:ZoomVisualiserContent{$MinCardinality:1,$MaxCardinality:1},Attribute:IsMetaRightAlign{$MinCardinality:1,$MaxCardinality:1},Attribute:IsAllVisualisersEdit{$MinCardinality:1,$MaxCardinality:1},Attribute:ShowMeta{$MinCardinality:1,$MaxCardinality:1,$DefaultValue:True},Attribute:ShowHeader{$MinCardinality:1,$MaxCardinality:1},Association:GridStyle{$MinCardinality:1,$MaxCardinality:1}}}");

            sm.Get(@"Visualiser\GridStyleEnum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));

            sm.Get(@"Visualiser\Form").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Form").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Form").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.FormVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Form").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\Form\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\Form\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\Form\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\Form\ExpertMode").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Form\ColumnNumber").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\Form\ColumnNumber").AddVertex(sm.Get(@"*MinValue"), 1);
            sm.Get(@"Visualiser\Form\ColumnNumber").AddVertex(sm.Get(@"*MaxValue"), 8);
            sm.Get(@"Visualiser\Form\MetaOnLeft").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Form\SectionsAsTabs").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Form\TableVisualiserVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            //sm.Get(@"UML\Class").AddEdge(sm.Get("UML*$DefaultOpenVisualiser"), sm.Get(@"Visualiser\Form"));

            sm.Get(@"Visualiser\Code").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Code").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Code").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.CodeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Code").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\Code\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Float"));
            sm.Get(@"Visualiser\Code\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 1.0);
            sm.Get(@"Visualiser\Code\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 30.0);
            sm.Get(@"Visualiser\Code\ShowWhiteSpace").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Code\HighlightedLine").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            sm.Get(@"Visualiser\Wrap").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Wrap").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Wrap").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.WrapVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Wrap").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));

            sm.Get(@"Visualiser\List").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\List").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\List").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\List").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasFilter"));
            sm.Get(@"Visualiser\List").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.ListVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\List").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\List\ShowMeta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\List\ShowHeader").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\List\GridStyle").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Visualiser\GridStyleEnum"));
            sm.Get(@"Visualiser\List\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\List\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\List\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\List\IsMetaRightAlign").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\List\IsAllVisualisersEdit").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            
            sm.Get(@"Visualiser\Table").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Table").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Table").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\Table").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasFilter"));
            sm.Get(@"Visualiser\Table").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.TableVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Table").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\Table\ExpertMode").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Table\ToShowEdgesMeta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Edge"));
            sm.Get(@"Visualiser\Table\ShowHeader").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Table\GridStyle").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Visualiser\GridStyleEnum"));
            sm.Get(@"Visualiser\Table\AlternatingRows").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Table\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\Table\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\Table\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\Table\IsAllVisualisersEdit").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            sm.Get(@"Visualiser\TableFast").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\TableFast").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\TableFast").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\TableFast").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasFilter"));
            sm.Get(@"Visualiser\TableFast").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.TableFastVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\TableFast").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\TableFast\ToShowEdgesMeta").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Edge"));
            sm.Get(@"Visualiser\TableFast\ShowHeader").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\TableFast\GridStyle").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"Visualiser\GridStyleEnum"));
            sm.Get(@"Visualiser\TableFast\AlternatingRows").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\TableFast\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\TableFast\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\TableFast\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\TableFast\IsAllVisualisersEdit").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
           

            sm.Get(@"Visualiser\Tree").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Tree").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Tree").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\Tree").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.TreeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Tree").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\Tree\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\Tree\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\Tree\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);


            sm.Get(@"Visualiser\Graph").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Graph").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Graph").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));            
            sm.Get(@"Visualiser\Graph").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.GraphVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Graph").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\Graph\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\Graph\ZoomVisualiserContent").AddVertex(sm.Get(@"*MinValue"), 0);
            sm.Get(@"Visualiser\Graph\ZoomVisualiserContent").AddVertex(sm.Get(@"*MaxValue"), 200);
            sm.Get(@"Visualiser\Graph\VisualiserCircleSize").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\Graph\VisualiserCircleSize").AddVertex(sm.Get(@"*MinValue"), 50);
            sm.Get(@"Visualiser\Graph\VisualiserCircleSize").AddVertex(sm.Get(@"*MaxValue"), 500);
            sm.Get(@"Visualiser\Graph\NumberOfCircles").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));
            sm.Get(@"Visualiser\Graph\NumberOfCircles").AddVertex(sm.Get(@"*MinValue"), 1);
            sm.Get(@"Visualiser\Graph\NumberOfCircles").AddVertex(sm.Get(@"*MaxValue"), 10);
            sm.Get(@"Visualiser\Graph\ShowInEdges").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Graph\FastMode").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"Visualiser\Graph\MetaLabels").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));

            sm.Get(@"Visualiser\Class").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Class").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Class").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.ClassVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Class").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));

            sm.Get(@"Visualiser\String").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\String").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\String").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.StringVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\String").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\String").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\String"));

            sm.Get(@"Visualiser\StringView").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\StringView").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\StringView").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.StringViewVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\StringView").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\String").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\StringView"));
     


            sm.Get(@"Visualiser\Vertex").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Vertex").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Vertex").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.VertexVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Vertex").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));

            sm.Get(@"ZeroTypes\VertexType").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\Vertex"));
            sm.Get(@"ZeroTypes\VertexType").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Vertex"));
            sm.Get(@"Base\Vertex\$Inherits").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Vertex"));
            sm.Get(@"Base\Vertex\$Inherits").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\Vertex"));
            sm.Get(@"Base\Vertex\$EdgeTarget").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Vertex"));
            sm.Get(@"Base\Vertex\$EdgeTarget").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\Vertex"));
            sm.Get(@"Base\Vertex\$VertexTarget").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Vertex"));
            sm.Get(@"Base\Vertex\$VertexTarget").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\Vertex"));
            //sm.Get(@"UML\Class").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\VertexVisualiser"));
            sm.Get(@"UML\Class").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Vertex"));

            sm.Get(@"Visualiser\Edge").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Edge").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Edge").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.EdgeVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Edge").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Edge").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\Edge"));
            sm.Get(@"ZeroTypes\Edge").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Edge"));

            sm.Get(@"Visualiser\Integer").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Integer").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));                        
            sm.Get(@"Visualiser\Integer").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.IntegerVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Integer").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Integer").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Integer"));

            sm.Get(@"Visualiser\Decimal").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Decimal").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));            
            sm.Get(@"Visualiser\Decimal").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.DecimalVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Decimal").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Decimal").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Decimal"));

            sm.Get(@"Visualiser\Float").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Float").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));            
            sm.Get(@"Visualiser\Float").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.FloatVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Float").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Float").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Float"));

            sm.Get(@"Visualiser\Boolean").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Boolean").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));
            sm.Get(@"Visualiser\Boolean").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.BooleanVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Boolean").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\Boolean").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Boolean"));
            sm.Get(@"ZeroTypes\Boolean").AddEdge(sm.Get("UML*$DefaultViewVisualiser"), sm.Get(@"Visualiser\Boolean"));

            sm.Get(@"Visualiser\Enum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Enum").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasBaseEdge"));                        
            sm.Get(@"Visualiser\Enum").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.EnumVisualiser, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Enum").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"ZeroTypes\EnumBase").AddEdge(sm.Get("UML*$DefaultEditVisualiser"), sm.Get(@"Visualiser\Enum"));

            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\$PlatformClass"));
            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get("*$Inherits"), sm.Get(@"ZeroTypes\HasSelectedEdges"));
            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"Visualiser\Diagram").AddVertex(sm.Get("*$PlatformClassName"), @"m0.UIWpf.Visualisers.Diagram.Diagram, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            sm.Get(@"Visualiser\Diagram\ZoomVisualiserContent").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Integer"));            
            sm.Get(@"Visualiser\Diagram\Item").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*DiagramItemBase"));
            sm.Get(@"Visualiser\Diagram\SizeX").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Float"));
            sm.Get(@"Visualiser\Diagram\SizeX").AddVertex(sm.Get(@"*$DefaultValue"), (double)1000.0);
            sm.Get(@"Visualiser\Diagram\SizeX").AddVertex(sm.Get(@"*MinValue"), (double)0.0);
            sm.Get(@"Visualiser\Diagram\SizeX").AddVertex(sm.Get(@"*MaxValue"), (double)4000.0);
            sm.Get(@"Visualiser\Diagram\SizeY").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*Float"));
            sm.Get(@"Visualiser\Diagram\SizeY").AddVertex(sm.Get(@"*$DefaultValue"), (double)1000.0);
            sm.Get(@"Visualiser\Diagram\SizeY").AddVertex(sm.Get(@"*MinValue"), (double)0.0);
            sm.Get(@"Visualiser\Diagram\SizeY").AddVertex(sm.Get(@"*MaxValue"), (double)4000.0);
            sm.Get(@"Visualiser\Diagram\CreationPool").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            sm.Get(@"Visualiser\Diagram").AddEdge(sm.Get("UML*$DefaultOpenVisualiser"), sm.Get(@"Visualiser\Diagram"));   
            
            IVertex diagramGeneralGroup = sm.Get(@"Visualiser\Diagram").AddVertex(sm.Get(@"*$Group"), "General");
            sm.Get(@"Visualiser\Diagram\CreationPool").AddEdge(sm.Get(@"*$Group"),diagramGeneralGroup);

            IVertex diagramDetailsGroup = sm.Get(@"Visualiser\Diagram\ZoomVisualiserContent").AddVertex(sm.Get(@"*$Group"), "Details");
            sm.Get(@"Visualiser\Diagram\SizeX").AddEdge(sm.Get(@"*$Group"), diagramDetailsGroup);
            sm.Get(@"Visualiser\Diagram\SizeY").AddEdge(sm.Get(@"*$Group"), diagramDetailsGroup);
            sm.Get(@"Visualiser\Diagram\SelectedEdges").AddEdge(sm.Get(@"*$Group"), diagramDetailsGroup);
        }        

        void CreateSystemData(){
            IVertex sm = Root.Get(@"System\Meta");

            IVertex s = Root.Get(@"System");

            GeneralUtil.ParseAndExcute(s,sm,"{Data}");
        }

        IVertex AddDiagramItemDefinition(String Value, String DirectVertexTestQuery, String MetaVertexTestQuery, IVertex DiagramItemClass,  IVertex InstanceCreation)
        {
            IVertex did = Root.Get(@"System\Meta*DiagramItemDefinition");

            IVertex v = Root.Get(@"System\Data\Visualiser\Diagram").AddVertex(did, Value);

            v.AddEdge(Root.Get(@"System\Meta*$Is"), Root.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramItemDefinition"));

            if(DirectVertexTestQuery!=null)
                v.AddVertex(did.Get("DirectVertexTestQuery"), DirectVertexTestQuery);

            if (MetaVertexTestQuery != null)
                v.AddVertex(did.Get("MetaVertexTestQuery"), MetaVertexTestQuery);

            v.AddEdge(did.Get("DiagramItemClass"), DiagramItemClass);  

            v.AddEdge(did.Get("InstanceCreation"), InstanceCreation);

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
                v.Get("DiagramItemVertex:").AddVertex(Root.Get(@"System\Meta*RoundEdgeSize"), RoundEdgeSize);

            if (CreateDiagraItemVertex && showMeta)
                v.Get("DiagramItemVertex:").AddVertex(Root.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem\ShowMeta"), "True");
            else
                v.Get("DiagramItemVertex:").AddVertex(Root.Get(@"System\Meta\Visualiser\DiagramInternal\DiagramRectangleItem\ShowMeta"), "False");

            if (VisualiserClass != null)
                v.Get(@"DiagramItemVertex:").AddEdge(Root.Get(@"System\Meta*VisualiserClass"), VisualiserClass);

            if (VisualiserVertex)
                v.Get(@"DiagramItemVertex:").AddVertex(Root.Get(@"System\Meta*VisualiserVertex"), null);

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
            IVertex did = Root.Get(@"System\Meta*DiagramItemDefinition");

            IVertex sm = Root.Get(@"System\Meta");

            IVertex v = AddDiagramItemDefinition(Value, DirectVertexTestQuery, MetaVertexTestQuery, DiagramItemClass, InstanceCreation);

            if (doNotShowInherited)
                v.AddVertex(sm.Get(@"*DoNotShowInherited"), "True");

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    v.AddVertex(sm.Get(@"*ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    v.AddVertex(sm.Get(@"*ForceShowEditForm"), "False");
            }

            if (CreateDiagraItemVertex)
            {
                IVertex iv = v.AddVertex(did.Get("DiagramItemVertex"), null);

                if (SizeX > -1)
                {
                    iv.AddVertex(Root.Get(@"System\Meta*SizeX"), SizeX);
                    iv.AddVertex(Root.Get(@"System\Meta*SizeY"), SizeY);
                }

                if (LineWidth > -1)
                    iv.AddVertex(Root.Get(@"System\Meta*LineWidth"), LineWidth);

                if (BackgroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(iv, Root.Get(@"System\Meta*Color"), Root.Get(@"System\Meta*BackgroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Red"), BackgroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Green"), BackgroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Blue"), BackgroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Opacity"), BackgroundOpacity);
                }

                if (ForegroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(iv, Root.Get(@"System\Meta*Color"), Root.Get(@"System\Meta*ForegroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Red"), ForegroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Green"), ForegroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Blue"), ForegroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Opacity"), ForegroundOpacity);
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
            IVertex sm = Root.Get(@"System\Meta");

            IVertex dld = Root.Get(@"System\Meta*DiagramInternal\DiagramLineDefinition");

            IVertex lv = v.AddVertex(dld, name);

            lv.AddEdge(sm.Get("*$Is"), dld);

            lv.AddVertex(dld.Get("EdgeTestQuery"), EdgeTestQuery);

            lv.AddVertex(dld.Get("ToDiagramItemTestQuery"), ToDiagramTestQuery);

            lv.AddEdge(dld.Get("DiagramLineClass"), DiagramLineClass);

            if (CreateEdgeOnly != null)
            {
                if (CreateEdgeOnly == true)
                    lv.AddVertex(sm.Get(@"*CreateEdgeOnly"), "True");

                if (CreateEdgeOnly == false)
                    lv.AddVertex(sm.Get(@"*CreateEdgeOnly"), "False");
            }

            if (ForceShowEditForm != null)
            {
                if (ForceShowEditForm == true)
                    lv.AddVertex(sm.Get(@"*ForceShowEditForm"), "True");

                if (ForceShowEditForm == false)
                    lv.AddVertex(sm.Get(@"*ForceShowEditForm"), "False");
            }

            if (CreateDiagraLineVertex)
            {
                IVertex dlv = lv.AddVertex(dld.Get("DiagramLineVertex"), null);

                if (isDashed)
                    dlv.AddVertex(sm.Get("*IsDashed"), "True");

                if (startAnchor != null)
                    dlv.AddEdge(sm.Get("*StartAnchor"), startAnchor);

                if (endAnchor != null)
                    dlv.AddEdge(sm.Get("*EndAnchor"), endAnchor);

                if (LineWidth > -1)
                    dlv.AddVertex(Root.Get(@"System\Meta*LineWidth"), LineWidth);

                if (BackgroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(dlv, Root.Get(@"System\Meta*Color"), Root.Get(@"System\Meta*BackgroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Red"), BackgroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Green"), BackgroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Blue"), BackgroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Opacity"), BackgroundOpacity);
                }

                if (ForegroundRed > -1)
                {
                    IVertex b = VertexOperations.AddInstance(dlv, Root.Get(@"System\Meta*Color"), Root.Get(@"System\Meta*ForegroundColor"));
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Red"), ForegroundRed);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Green"), ForegroundGreen);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Blue"), ForegroundBlue);
                    GraphUtil.SetVertexValue(b, Root.Get(@"System\Meta*Opacity"), ForegroundOpacity);
                }
            }
        }

        void CreateSystemDataVisualiserDiagram()
        {
            IVertex sm = Root.Get(@"System\Meta");

            IVertex sd = Root.Get(@"System\Data");

            GeneralUtil.ParseAndExcute(sd, sm, "{Visualiser{Diagram}}");

            IVertex Instance = sm.Get("*Instance");
            IVertex InstanceAndDirect = sm.Get("*InstanceAndDirect");
            IVertex Direct = sm.Get("*Direct");

            IVertex arrow = sm.Get(@"*DiagramInternal\LineEndEnum\Arrow");
            IVertex triangle = sm.Get(@"*DiagramInternal\LineEndEnum\Triangle");
            IVertex filledTriangle = sm.Get(@"*DiagramInternal\LineEndEnum\FilledTriangle");
            IVertex diamond = sm.Get(@"*DiagramInternal\LineEndEnum\Diamond");
            IVertex filledDiamond = sm.Get(@"*DiagramInternal\LineEndEnum\FilledDiamond");
            IVertex straight = sm.Get(@"*DiagramInternal\LineEndEnum\Straight");

        /*    /////////////////////////////////////////////////////////////////////////
            // TEST
            /////////////////////////////////////////////////////////////////////////

            IVertex v =AddDiagramItemDefinition_Combo("test", false,
                @"", 
                null, 
                sm.Get(@"*DiagramRhombusItem"), 
                Direct,
                true,200,200,10,
                255,0,0,100,
                0,255,0,100);

            v.Get("DiagramItemVertex:").AddVertex(Root.Get(@"System\Meta*Filename"), "testimage.gif");

            AddDiagramLine_Combo(v,
                "Edgee",
                @"$Is:\",
                @"",
                sm.Get(@"*DiagramInternal\DiagramLine"),
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
                sm.Get(@"*DiagramRectangleItem"),
                InstanceAndDirect,
                true, -1, 0, -1,
                -1, 0, 0, 0,
                -1, 0, 0, 0,
                -1,true,
                Root.Get(@"System\Meta*List"),true);

            IVertex v2vv = v2.Get(@"DiagramItemVertex:\VisualiserVertex:");

            v2vv.AddVertex(Root.Get(@"System\Meta*FilterQuery"), "{$Is:Attribute}:");

            v2vv.AddVertex(Root.Get(@"System\Meta*ShowHeader"), "False");

            AddDiagramLine_Combo(v2,
                "Association instance",
                @"$Is:{$Is:Class}\Association:",
                @"Definition:Object",
                sm.Get(@"*DiagramInternal\DiagramLine"),
                true,
                null,
                arrow,
                -1,true,
                -1, 0, 0, 0,
                -1, 0, 0, 0);

            AddDiagramLine_Combo(v2,
                "Aggregation instance",
                @"$Is:{$Is:Class}\Aggregation:",
                @"Definition:Object",
                sm.Get(@"*DiagramInternal\DiagramLine"),
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
              sm.Get(@"*DiagramRectangleItem"),
              InstanceAndDirect,
              true, -1, 0, -1,
                -1, 0, 0, 0,
                -1, 0, 0, 0,
                -1, true,
                Root.Get(@"System\Meta\Visualiser\Class"), true, true);

            IVertex v3vv = v3.Get(@"DiagramItemVertex:\VisualiserVertex:");

            v3vv.AddVertex(Root.Get(@"System\Meta*FilterQuery"), "Attribute:");

            v3vv.AddVertex(Root.Get(@"System\Meta*ShowHeader"), "False");

            AddDiagramLine_Combo(v3,
             "Association",
             @"$Is:Class\Association",
             @"Definition:Class",             
             sm.Get(@"*DiagramInternal\DiagramLine"),
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
            sm.Get(@"*DiagramInternal\DiagramLine"),
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
            sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
                    sm.Get(@"*DiagramInternal\DiagramLine"),
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
             sm.Get(@"*DiagramInternal\DiagramLine"),
             true,
             filledDiamond,
             null,
             2, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0,true,false);

            AddDiagramLine_Combo(vMethod,
             "Output",
            @"$Is:Method\Output",
            @"BaseEdge:\To:\$Is:Type",
            sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
            sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramOvalItem"),
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
             sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
             sm.Get(@"*DiagramRectangleItem"),
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
               sm.Get(@"*DiagramInternal\DiagramMetaExtendedLine"),
               true,
               null,
               arrow,
               -1, false,
               -1, 0, 0, 0,
               -1, 0, 0, 100);
        }

        void AddNextLine(IVertex diagramItem)
        {
            IVertex sm = Root.Get(@"System\Meta");

            IVertex filledTriangle = sm.Get(@"*DiagramInternal\LineEndEnum\FilledTriangle");

            AddDiagramLine_Combo(diagramItem,
             "Next",
             @"$Is:NextOut\Next",
              @"BaseEdge:\To:\$Is:Atom",
              sm.Get(@"*DiagramInternal\DiagramLine"),
             true,
              null,
             filledTriangle,
              3, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0);
        }

        void AddOutput(IVertex diagramItem)
        {
            IVertex sm = Root.Get(@"System\Meta");

            IVertex arrow = sm.Get(@"*DiagramInternal\LineEndEnum\Arrow");

            AddDiagramLine_Combo(diagramItem,
             "Output",
             @"$Is:Expression\Output",
              @"BaseEdge:\To:\$Is:Type",
              sm.Get(@"*DiagramInternal\DiagramLine"),
             true,
              null,
             arrow,
              -1, false,
             -1, 0, 0, 0,
             -1, 0, 0, 0);
        }

        void CreateSystemDataVisualiserDiagram_Uml()
        {
            IVertex sm = Root.Get(@"System\Meta");

            IVertex Instance = sm.Get("*Instance");
            IVertex InstanceAndDirect = sm.Get("*InstanceAndDirect");
            IVertex Direct = sm.Get("*Direct");

            IVertex arrow = sm.Get(@"*DiagramInternal\LineEndEnum\Arrow");
            IVertex triangle = sm.Get(@"*DiagramInternal\LineEndEnum\Triangle");
            IVertex filledTriangle = sm.Get(@"*DiagramInternal\LineEndEnum\FilledTriangle");
            IVertex diamond = sm.Get(@"*DiagramInternal\LineEndEnum\Diamond");
            IVertex filledDiamond = sm.Get(@"*DiagramInternal\LineEndEnum\FilledDiamond");
            IVertex straight = sm.Get(@"*DiagramInternal\LineEndEnum\Straight");

 
            /////////////////////////////////////////////////////////////////////////
            // AtomType 
            /////////////////////////////////////////////////////////////////////////

            IVertex vAtomType = AddDiagramItemDefinition_Combo_RectangleItem("AtomType", false,
         @"{$Is:AtomType}",
         "AtomType",
          sm.Get(@"*DiagramRectangleItem"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
               sm.Get(@"*DiagramInternal\DiagramLine"),
               true,
               filledDiamond,
               null,
               -1, false,
               -1, 0, 0, 0,
               -1, 0, 0, 100);

            IVertex vState = AddDiagramItemDefinition_Combo_RectangleItem("State", false,
         @"{$Is:State}",
         "State",
          sm.Get(@"*DiagramRectangleItem"),
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
               sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          0, true,
          Root.Get(@"System\Meta*List"), true);

            IVertex vEnum_vv = vEnum.Get(@"DiagramItemVertex:\VisualiserVertex:");

            vEnum_vv.AddVertex(Root.Get(@"System\Meta*FilterQuery"), "EnumValue:");

            vEnum_vv.AddVertex(Root.Get(@"System\Meta\Visualiser\List\ShowHeader"), "False");
            vEnum_vv.AddVertex(Root.Get(@"System\Meta\Visualiser\List\ShowMeta"), "False");



            /////////////////////////////////////////////////////////////////////////
            // SingleOperator
            /////////////////////////////////////////////////////////////////////////

            IVertex vSingleOperator = AddDiagramItemDefinition_Combo_RectangleItem("SingleOperator", false,
         @"{$Is:SingleOperator}",
         "{$Inherits:SingleOperator}",
          sm.Get(@"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          5, true,
          null, false);

            AddDiagramLine_Combo(vSingleOperator,
       "Expression",
       @"$Is:SingleOperator\TargetExpression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          5, true,
          null, false);

            AddDiagramLine_Combo(vMultiOperator,
       "Expression",
       @"$Is:MultiOperator\TargetExpression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(@"*DiagramInternal\DiagramLine"),
       true,
       null,
       arrow,
       -1, false,
       -1, 0, 0, 0,
       -1, 0, 0, 100);

            AddNextLine(vSingleOperator);


            /////////////////////////////////////////////////////////////////////////
            // DoubleOperator
            /////////////////////////////////////////////////////////////////////////

            IVertex vDoubleOperator = AddDiagramItemDefinition_Combo_RectangleItem("DoubleOperator", false,
         @"{$Is:DoubleOperator}",
         "{$Inherits:DoubleOperator}",
          sm.Get(@"*DiagramRectangleItem"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
         sm.Get(@"*DiagramOvalItem"),
         InstanceAndDirect,
         true, 40, 40, -1,
         0, 0, 0, 255,
         255, 255, 255, 255);

            AddDiagramLine_Combo(vReturn,
       "Expression",
       @"$Is:Return\Expression",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
             sm.Get(@"*DiagramInternal\DiagramLine"),
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
            sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRhombusItem"),
          InstanceAndDirect,
          true, -1, 0, -1,
          -1, 0, 0, 0,
          -1, 0, 0, 0,
          false);

            AddDiagramLine_Combo(vIf,
       "Test",
       @"$Is:If\Test",
       @"BaseEdge:\To:\$Is:Atom",
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
       sm.Get(@"*DiagramInternal\DiagramLine"),
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
          sm.Get(@"*DiagramRectangleItem"),
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


            foreach(PackageLine what in packageLines)
                AddDiagramLine_Combo(vPackage,
            what.Is,
            @"$Is:Package\"+what.Is,
            @"Definition:"+what.Definition,
            sm.Get(@"*DiagramInternal\DiagramLine"),
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

        void CreateStoresMeta()
        {
            FileSystemStore.FillSystemMeta();
        }

        void CreateSystemMetaCommands()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{Commands{VisualiserClass,SynchronisedVisualiser}}");
        }

        void CreateUserMeta()
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(sm, sm, "{User{CurrentUser,Class:NonAtomProcess{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1}},Class:Session{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1},Aggregation:Process{$MinCardinality:0,$MaxCardinality:-1}},Class:User{Attribute:CurrentSession{$MinCardinality:1,$MaxCardinality:1},Aggregation:Session{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Settings{$MinCardinality:1,$MaxCardinality:1},Aggregation:CodeSettings{$MinCardinality:1,$MaxCardinality:1},Aggregation:Queries{$MinCardinality:1,$MaxCardinality:1}},Class:CodeSettings{Association:Keyword{$MinCardinality:0,$MaxCardinality:-1}},Class:Settings{Attribute:CopyOnDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Association:AllowBlankAreaDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Attribute:AllowManyDiagramItemsForOneVertex{$MinCardinality:1,$MaxCardinality:1}},Enum:AllowBlankAreaDragAndDropEnum{EnumValue:No,EnumValue:OnlyEnd,EnumValue:StartAndEnd}}}");

            sm.Get(@"User\NonAtomProcess").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"User\Session").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"User\User").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"User\Settings").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));
            sm.Get(@"User\CodeSettings").AddEdge(sm.Get(@"*$Is"), sm.Get(@"UML\Class"));


            sm.Get(@"User\NonAtomProcess\StartTimeStamp").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\DateTime"));
            sm.Get(@"User\Session\StartTimeStamp").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\DateTime"));
            sm.Get(@"User\Session\Process").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\NonAtomProcess")); // to be updated
            sm.Get(@"User\User\Session").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\Session"));
            sm.Get(@"User\User\CurrentSession").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\Session"));
            sm.Get(@"User\User\Settings").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"User\Settings"));
            sm.Get(@"User\User\Queries").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\VertexType"));
            sm.Get(@"User\AllowBlankAreaDragAndDropEnum").AddEdge(sm.Get(@"*$Inherits"), sm.Get(@"ZeroTypes\EnumBase"));

            sm.Get(@"User\Settings\CopyOnDragAndDrop").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));
            sm.Get(@"User\Settings\AllowBlankAreaDragAndDrop").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"*AllowBlankAreaDragAndDropEnum"));
            sm.Get(@"User\Settings\AllowManyDiagramItemsForOneVertex").AddEdge(sm.Get(@"*$EdgeTarget"), sm.Get(@"ZeroTypes\Boolean"));            
        }

        void CreateUser(IVertex user)
        {
            IVertex sm = Root.Get(@"System\Meta");

            GeneralUtil.ParseAndExcute(user, sm, "{Settings:{CopyOnDragAndDrop:False,AllowManyDiagramItemsForOneVertex:True},CodeSettings:,Queries:{String:test,String:\"test{test2}\"}}");
            
            user.Get("Settings:").AddEdge(sm.Get("*AllowBlankAreaDragAndDrop"),sm.Get(@"User\AllowBlankAreaDragAndDropEnum\StartAndEnd"));            

            user.AddEdge(sm.Get(@"*$Is"),sm.Get(@"User\User"));
            user.Get("Settings:").AddEdge(sm.Get(@"*$Is"),sm.Get(@"User\Settings"));

            IVertex cs = user.Get(@"CodeSettings:");
            cs.AddEdge(sm.Get(@"*$Is"), sm.Get(@"User\CodeSettings"));

            cs.AddEdge(sm.Get(@"User\CodeSettings\Keyword"), sm.Get(@"UML\Keyword"));

            foreach (IEdge e in Root.GetAll(@"System\TextLanguage\ZeroCode\DefaultImports\"))
                //if (!GraphUtil.GetValueAndCompareStrings(e.To, "$DirectMeta") && !GraphUtil.GetValueAndCompareStrings(e.To, "$Direct"))
                    cs.AddEdge(e.Meta, e.To);

            IVertex session = user.AddVertex(sm.Get(@"User\User\Session"), null);
            user.AddEdge(sm.Get(@"User\User\CurrentSession"), session);
        }

        void CreateUsers()
        {
            IVertex sm = Root.Get(@"System\Meta\User");

            GeneralUtil.ParseAndExcute(Root, sm, "{User{User:root,User:wlodek,User:tadek}}");

            foreach (IEdge u in Root.GetAll(@"User\"))
                CreateUser(u.To);

            Root.Get(@"User").AddEdge(Root.Get(@"System\Meta\User\CurrentUser"), Root.Get(@"User\root"));
        }

        void AddDrives()
        {
            string[] drives = System.IO.Directory.GetLogicalDrives();

            IVertex DriveMeta=Root.Get(@"System\Meta\Store\FileSystem\Drive");

            foreach (string str in drives)
            {
                FileSystemStore fss = new FileSystemStore(str, this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

                //fss.IncludeFileContent = true;

                Root.AddEdge(DriveMeta,fss.Root);
            }
        }
        
        public MinusZero(){
            //Initialize();
        }       

        private System.IO.StreamWriter logFile;

        public bool DoLog=false;

        public int LogLevel=1;

        private void InitializeLog()
        {
            if (DoLog)
            {
                logFile = new System.IO.StreamWriter("log.txt");
                logFile.AutoFlush = true;

                Log(0,"InitializeLog", "START");
            }
        }

        public void Log(int Level,string Where, string What)
        {
            if(DoLog&&Level<=LogLevel)
                logFile.WriteLine(System.DateTime.Now.ToLongTimeString()+":"+ System.DateTime.Now.Millisecond+" "+Where+": "+What);
        }

        private void DisposeLog()
        {
            Log(0,"DisposeLog", "STOP");
            logFile.Close();
        }

        private void AddIsAttribute(string what)
        {
            AddIsAttribute_inner(@"System\Meta\ZeroTypes*"+what+":",what);
            AddIsAttribute_inner(@"System\Meta\Visualiser*" + what + ":", what);
            AddIsAttribute_inner(@"System\Meta\User*" + what + ":", what);
        }

        private void AddIsAttribute_inner(string s, string what) { 
            IVertex attributes = root.GetAll(s);
            IVertex ismeta = root.Get(@"System\Meta*$Is");
            IVertex ameta=root.Get(@"System\Meta\UML\Class\"+what);

            foreach (IEdge v in attributes)
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

        private void AddIsAggregation_inner(string s) { 
        /*    IVertex isaggregationtarget = root.GetAll(s);

            IVertex isAggregation = root.Get(@"System\Meta\Base\Vertex\$IsAggregation");
            IVertex empty = root.Get(@"System\Meta\Base\$Empty");

            foreach (IEdge v in isaggregationtarget)
                v.To.AddEdge(isAggregation, empty);*/
        }

        public void Initialize(){
            InitializeLog();            

            Bootstrap();

            CreateSystem();            

            Init();

            CreateSystemMeta();

            CreatePresentation();

            CreateSystemMetaBase();

            CreateSystemMetaUml();

            CreateSystemMetaUml_Action_part();

            

            CreateSystemMetaZeroTypes();

            CreateSystemTextLanguageZeroCode();


            CreateSystemUMLKeywords();


            CreateSystemMetaVisualiserDiagram();

            CreateSystemMetaVisualiser();            

            CreateSystemData();

            CreateSystemDataVisualiserDiagram();

            CreateSystemDataVisualiserDiagram_Uml();

            CreateStoresMeta();            

            CreateSystemMetaCommands();

            CreateUserMeta();

            CreateUsers();

            AddIsAttribute("Attribute");

            AddIsAttribute("Association");

            AddIsAttribute("Aggregation");

            AddIsAggregation();

            AddDrives();            

            UIWpf.UIWpf.InitializeUIWpf();

            IsInitialized = true;
        }

        public void Dispose()
        {
           // DisposeLog();
        }

        public void Refresh()
        {
            List<string[]> StoresPersistency=new List<string[]>();

            foreach (IStore s in Stores)
            {
                string[] e=new string[2];

                e[0] = s.TypeName;
                e[1] = s.Identifier;

                StoresPersistency.Add(e);

                s.Close();
            }

            Bootstrap();

            foreach (string[] e in StoresPersistency)
            {
                GetStore(e[0], e[1]);
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

            if(store!=null)
                return store;

            store=(IStore)Activator.CreateInstance(Type.GetType(StoreTypeName), new object[] { StoreIdentifier, this, GetStoreDefaultAccessLevelList });

            //Stores.Add(store);
            // store's constructor does this

            return store;
        }
    }
}