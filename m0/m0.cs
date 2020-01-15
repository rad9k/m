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


        public IList<IStore> stores = new List<IStore>(); // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IList<IStore> Stores { get { return stores; } }


        public IStore tempstore; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IStore TempStore { get { return tempstore; } }


        public IVertex root; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex Root { get { return root; } }


        IVertex inherits;

        public IVertex Inherits { get { return inherits; } }


        IVertex stackFrameInherits;

        public IVertex StackFrameInherits { get { return stackFrameInherits; } }


        public IVertex empty; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex Empty { get { return empty; } }


        IVertex dolar;

        public IVertex Dolar { get { return dolar; } }


        IUserInteraction _DefaultUserInteraction;

        public IUserInteraction DefaultUserInteraction { get { return _DefaultUserInteraction; } }
        

        public IParser _DefaultParser; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IParser DefaultParser { get { return _DefaultParser; } }


        public IExecuter _DefaultExecuter; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IExecuter DefaultExecuter { get { return _DefaultExecuter; } }

        //

        public IVertex _DefaultFormalTextLanguage; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex DefaultFormalTextLanguage { get { return _DefaultFormalTextLanguage; } }
        

        public ICodeGenerator _DefaultCodeGenerator; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public ICodeGenerator DefaultCodeGenerator { get { return _DefaultCodeGenerator; } }


        public IVertex tempRoot; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex EdgeTarget;
        public IVertex Is;
        public IVertex IsAggregation;

        public bool IsGUIDragging { get; set; }

        //

        public IVertex CreateTempVertex()
        {
            return new EasyVertex(this.tempstore);
        }        

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
       
        void InitRootVariables()
        {
            IVertex System = GraphUtil.GetQueryOutFirst(Root, null, "System");

            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");

            IVertex Base = GraphUtil.GetQueryOutFirst(Meta, null, "Base");
            
            IVertex Vertex = GraphUtil.GetQueryOutFirst(Base, null, "Vertex");

            //

            empty = GraphUtil.GetQueryOutFirst(Base, null, "$Empty"); // there are some bugs related to this and old zeroscript.get ???

            inherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$Inherits");

            stackFrameInherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$StackFrameInherits");

            dolar = GraphUtil.GetQueryOutFirst(Base, null, "$");

            _DefaultFormalTextLanguage = MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\DefaultFormalTextLanguage:");

            EdgeTarget = GraphUtil.GetQueryOutFirst(Vertex, null, "$EdgeTarget");

            Is = GraphUtil.GetQueryOutFirst(Vertex, null, "$Is");

            IsAggregation = GraphUtil.GetQueryOutFirst(Vertex, null, "$IsAggregation");
        }        

        void CreateSystemMetaStoreFileSystem()
        {
            FileSystemStore.FillSystemMeta();
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

        public void InitializeLog()   // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE
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
        }

        private void DisposeLog()
        {
            Log(0, "DisposeLog", "STOP");
            logFile.Close();
        }

        ///
        
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

        public void Initialize()
        {
            if (IsInitialized)
                return;

            LogLevel = -2;

            InitializeLog();

            PreBootstrap();

            Bootstrap();

            Init();

            InitRootVariables();

            Init_AfterZeroCodeDefintionCreated();

            FileSystemStore.FillSystemMeta();

            AddDrives();



            UIWpf.UIWpf.InitializeUIWpf();

            IsInitialized = true;



            

            
        }
    }
}