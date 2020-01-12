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

        IVertex dolar;

        public IVertex Dolar { get { return dolar; } }

        IUserInteraction _DefaultUserInteraction;

        public IUserInteraction DefaultUserInteraction { get { return _DefaultUserInteraction; } }
       

        //

        IParser _DefaultParser;

        public IParser DefaultParser { get { return _DefaultParser; } }


        IExecuter _DefaultExecuter;

        public IExecuter DefaultExecuter { get { return _DefaultExecuter; } }

        //

        private IVertex _DefaultFormalTextLanguage;

        public IVertex DefaultFormalTextLanguage { get { return _DefaultFormalTextLanguage; } }
        


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
        }

        void Init_AfterZeroCodeDefintionCreated()
        {
            ZeroCode.ZeroCodeEngine zeroCodeEngine = new ZeroCode.ZeroCodeEngine();

            _DefaultParser = zeroCodeEngine;
            _DefaultExecuter = zeroCodeEngine;

            _DefaultCodeGenerator = zeroCodeEngine;
        }


       
        void RootVariableVertexLinksCreate()
        {
            IVertex System = GraphUtil.GetQueryOutFirst(Root, null, "System");

            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");

            IVertex Base = GraphUtil.GetQueryOutFirst(Meta, null, "Base");

            empty = GraphUtil.GetQueryOutFirst(Base, null, "$Empty"); // there are some bugs related to this and old zeroscript.get

            IVertex Vertex = GraphUtil.GetQueryOutFirst(Base, null, "Vertex");

            inherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$Inherits");

            stackFrameInherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$StackFrameInherits");

            dolar = GraphUtil.GetQueryOutFirst(Base, null, "$");
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

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(sm, sm, "{User{CurrentUser,Class:NonAtomProcess{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1}},Class:Session{Attribute:StartTimeStamp{$MinCardinality:1,$MaxCardinality:1},Aggregation:Process{$MinCardinality:0,$MaxCardinality:-1}},Class:User{Attribute:CurrentSession{$MinCardinality:1,$MaxCardinality:1},Aggregation:Session{$MinCardinality:0,$MaxCardinality:-1},Aggregation:Settings{$MinCardinality:1,$MaxCardinality:1},Aggregation:DefaultFormalTextLanguage{$MinCardinality:1,$MaxCardinality:1},Aggregation:Queries{$MinCardinality:1,$MaxCardinality:1}},Class:Settings{Attribute:CopyOnDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Association:AllowBlankAreaDragAndDrop{$MinCardinality:1,$MaxCardinality:1},Attribute:AllowManyDiagramItemsForOneVertex{$MinCardinality:1,$MaxCardinality:1}},Enum:AllowBlankAreaDragAndDropEnum{EnumValue:No,EnumValue:OnlyEnd,EnumValue:StartAndEnd}}}");

            sm.Get(false, @"User\NonAtomProcess").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\Session").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\User").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));
            sm.Get(false, @"User\Settings").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"ZeroUML\Class"));

            sm.Get(false, @"User\NonAtomProcess\StartTimeStamp").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\DateTime"));
            sm.Get(false, @"User\Session\StartTimeStamp").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\DateTime"));
            sm.Get(false, @"User\Session\Process").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\NonAtomProcess")); // to be updated
            sm.Get(false, @"User\User\Session").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\Session"));
            sm.Get(false, @"User\User\CurrentSession").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\Session"));
            sm.Get(false, @"User\User\Settings").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"User\Settings"));
            sm.Get(false, @"User\User\DefaultFormalTextLanguage").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\FormalTextLanguage"));
            sm.Get(false, @"User\User\Queries").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\VertexType"));
            sm.Get(false, @"User\AllowBlankAreaDragAndDropEnum").AddEdge(sm.Get(false, @"?$Inherits"), sm.Get(false, @"ZeroTypes\EnumBase"));

            sm.Get(false, @"User\Settings\CopyOnDragAndDrop").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
            sm.Get(false, @"User\Settings\AllowBlankAreaDragAndDrop").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"?AllowBlankAreaDragAndDropEnum"));
            sm.Get(false, @"User\Settings\AllowManyDiagramItemsForOneVertex").AddEdge(sm.Get(false, @"?$EdgeTarget"), sm.Get(false, @"ZeroTypes\Boolean"));
        }

        void CreateUser(IVertex user)
        {
            IVertex sm = Root.Get(false, @"System\Meta");

            m0.LegacySystem.Util.GeneralUtil.ParseAndExcute(user, sm, "{Settings:{CopyOnDragAndDrop:False,AllowManyDiagramItemsForOneVertex:True},Queries:{String:test,String:\"test{test2}\"}}");

            user.Get(false, "Settings:").AddEdge(sm.Get(false, "?AllowBlankAreaDragAndDrop"), sm.Get(false, @"User\AllowBlankAreaDragAndDropEnum\StartAndEnd"));

            user.AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"User\User"));
            user.Get(false, "Settings:").AddEdge(sm.Get(false, @"?$Is"), sm.Get(false, @"User\Settings"));

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

        private void AddFastAccessVertexes()
        {
            EdgeTarget = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$EdgeTarget");
            Is = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$Is");
            IsAggregation = LegacySystem.Graph.EasyVertex.Get(Root, false, @"System\Meta\Base\Vertex\$IsAggregation");
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

            CreateSystemMetaBase();


            AddFastAccessVertexes();


            CreateSystemMetaZeroUML();

            CreateSystemMetaZeroTypes();

            CreateSystemMetaZeroUML_ZeroCode_part();

            CreateSystemFormalTextLanguageZeroCode();


            CreateSystemFormalTextLanguegeZeroCode_Keywords();
        }

        private void Initialize_PostParserReady()
        {
            Init_AfterZeroCodeDefintionCreated();



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



            AddIsAttribute("Attribute");

            AddIsAttribute("Association");

            AddIsAttribute("Aggregation");

            AddIsAggregation();

            AddDrives();



            UIWpf.UIWpf.InitializeUIWpf();

            IsInitialized = true;
        }

        public void Initialize()
        {
            Initialize_PreParserReady();

            // PARSER READY

            Initialize_PostParserReady();


            RootVariableVertexLinksCreate();
        }
    }
}