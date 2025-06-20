using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Bootstrap;
using m0.Store;
using m0.Store.FileSystem;
using m0.Store.Json;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroTypes;
using m0.ZeroUML.Instructions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;



namespace m0
{
    public class MinusZero : IStoreUniverse, IDisposable
    {
        public IEnumerable<IVertex> BootstrapVertexes;

        public static MinusZero Instance = new MinusZero();

        public bool IsInitialized = false;

        public AccessLevelEnum[] GetStoreDefaultAccessLevelList = new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions };


        public IList<IStore> stores = new List<IStore>(); // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IList<IStore> Stores { get { return stores; } }


        public IStore tempstore; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IStore TempStore { get { return tempstore; } }

        IStore emptystore;

        public IStore EmptyStore { get { return emptystore; } }

        public IVertex root; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex Root { get { return root; } }        

        public IVertex Inherits;

        public IVertex StackFrameInherits;


        public IVertex empty; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex Empty { get { return empty; } }


        IVertex dolar;

        public IVertex Dolar { get { return dolar; } }


        IUserInteraction _UserInteraction;

        public IUserInteraction UserInteraction { get { return _UserInteraction; } }


        public IFormalTextParser _DefaultFormalTextParser; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IFormalTextParser DefaultFormalTextParser { get { return _DefaultFormalTextParser; } }


        public IExecuter _DefaultExecuter; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IExecuter DefaultExecuter { get { return _DefaultExecuter; } }

        //

        public IVertex _DefaultFormalTextLanguage; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex DefaultFormalTextLanguage { get { return _DefaultFormalTextLanguage; } }


        public IFormalTextGenerator _DefaultFormalTextGenerator; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IFormalTextGenerator DefaultFormalTextGenerator { get { return _DefaultFormalTextGenerator; } }


        public IVertex tempRoot; // need this public hack for LegacySystem_m0 based generation in m0_SYSTEM_GENERATE

        public IVertex EdgeTarget;
        public IVertex Is;
        public IVertex IsAggregation;

        public bool IsGUIDragging { get; set; }

        //

        private ITransaction transactionStackTop = null;

        public ITransaction GetTopTransaction()
        {
            return transactionStackTop;
        }

        public void SetTopTransaction(ITransaction transaction)
        {
            transactionStackTop = transaction;
        }

        public IVertex CreateTempVertex() //edge
        {
            IEdge edge = TempStore.Root.AddVertexAndReturnEdge(empty, null);

            TempStore.Root.DeleteEdge(edge);

            return edge.To;
        }

        public IEdge CreateTempEdge()
        {
            return CreateTempVertex().AddVertexAndReturnEdge(empty, null);            
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

            emptystore = new MemoryStore("$-0$EMPTY$STORE$", this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions }, true);

            empty = new IdentifiedVertex("$Empty", emptystore);

            empty.Value = "$Empty";

            emptystore.Root.AddEdge(null, empty);

            tempRoot = TempStore.Root;
        }

        void Init_AfterZeroCodeDefintionCreated()
        {
            ZeroCode.ZeroCodeEngine zeroCodeEngine = new ZeroCode.ZeroCodeEngine();

            _DefaultFormalTextParser = zeroCodeEngine;
            _DefaultExecuter = zeroCodeEngine;

            _DefaultFormalTextGenerator = zeroCodeEngine;
        }

        void InitRootVariables()
        {
            IVertex System = GraphUtil.GetQueryOutFirst(Root, null, "System");

            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");

            IVertex Base = GraphUtil.GetQueryOutFirst(Meta, null, "Base");
            
            IVertex Vertex = GraphUtil.GetQueryOutFirst(Base, null, "Vertex");

            //

            empty = GraphUtil.GetQueryOutFirst(Base, null, "$Empty"); // there are some bugs related to this and old zeroscript.get ???

            Inherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$Inherits");

            StackFrameInherits = GraphUtil.GetQueryOutFirst(Vertex, null, "$StackFrameInherits");

            dolar = GraphUtil.GetQueryOutFirst(Base, null, "$");

            //

            IVertex User = GraphUtil.GetQueryOutFirst(Root, null, "User");

            IVertex CurrentUser = GraphUtil.GetQueryOutFirst(User, "CurrentUser", null);

            _DefaultFormalTextLanguage = GraphUtil.GetQueryOutFirst(CurrentUser, "DefaultFormalTextLanguage", null);

            //_DefaultFormalTextLanguage = MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\DefaultFormalTextLanguage:");

            //

            EdgeTarget = GraphUtil.GetQueryOutFirst(Vertex, null, "$EdgeTarget");

            Is = GraphUtil.GetQueryOutFirst(Vertex, null, "$Is");

            IsAggregation = GraphUtil.GetQueryOutFirst(Vertex, null, "$IsAggregation");
        }        
   

        void AddDrives()
        {
            IVertex localComputer = root.Get(false, @"Hardware\LocalComputer:");

            string[] drives = System.IO.Directory.GetLogicalDrives();

            IVertex DriveMeta = Root.Get(false, @"System\Meta\Store\FileSystem\Drive");

            IVertex ComputerDrive = Root.Get(false, @"System\Meta\Hardware\Computer\Drive");

            foreach (string str in drives)
            {
                FileSystemStore fss = new FileSystemStore(str, this, new AccessLevelEnum[] { AccessLevelEnum.NoRestrictions });

                //fss.IncludeFileContent = true;                

                Root.AddEdge(DriveMeta, fss.Root);

                localComputer.AddEdge(ComputerDrive, fss.Root);
            }
        }

        public MinusZero()
        {
            //Initialize();
        }

        private System.IO.StreamWriter logFile;

        public bool DoLog = true;

        public int LogLevel;

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
            Log(Level, 0, Where, What);
        }

        public void Log(int Level, int nesting, string Where, string What)
        {
            StringBuilder pre = new StringBuilder();

            for (int x = 0; x < nesting; x++)
                pre.Append(" . ");

            if (DoLog && Level <= LogLevel)
                logFile.WriteLine(System.DateTime.Now.ToLongTimeString() + ":" + System.DateTime.Now.Millisecond + "[" + Level + "]:" + " " + pre + Where + ": " + What);
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
            UserInteraction.UserInteractionFinalize();
            

            GraphChangeTriggerWatcher.RemoveAllGraphChangeTriggers();

            ExecutionFlowHelper.CommitTransaction();

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
                if (s is ICommintBeforeGlobalDetachStore)
                    s.CommitTransaction();

            foreach (IStore s in Stores)
                s.Detach();

            foreach (IStore s in Stores)
                if (!(s is ICommintBeforeGlobalDetachStore))
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

        public IStore GetStore(string StoreIdentifier)
        {
            IStore store = Stores.Where(s => s.Identifier == StoreIdentifier).FirstOrDefault();

            if (store != null)
                return store;

            return null;
        }

        public void RemoveStore(IStore store)
        {
            Stores.Remove(store);
        }

        void FillBootstrapVertexes()
        {
            BootstrapVertexes = GraphUtil.GetSubGraphWithoutLinksAsList(root);
        }

        void AddHardware()
        {
            IVertex Hardware = root.AddVertex(null, "Hardware");

            IVertex localComputer = VertexOperations.AddInstance(Hardware, root.Get(false, @"System\Meta\Hardware\Computer"));

            localComputer.Value = "my";

            Hardware.AddEdge(root.Get(false, @"System\Meta\Hardware\LocalComputer"), localComputer);
        }

        public void StaticMetaInitialize()
        {
            GraphChangeTrigger.Initialize();
            ExecutionFlowHelper.Initialize();
            GraphChangeTransactionAtom.Initialize();
            Transaction.Initialize();
            //AtomVisualiserHelper.Initialize();
        }

        void LoadFromFileSystem()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            
            string autostartPath = Path.Combine(currentDirectory, "autostart");

            FileSystemLoader fsl = new FileSystemLoader(autostartPath);

            IVertex autostartVertex = root.AddVertex(null, "Autostart");

            fsl.Load(autostartVertex);        
        }

        void Autostart()
        {
            IVertex autostartVertex = root.Get(false, "Autostart");

            IExecution exe = new ZeroCodeExecution();

            foreach (IEdge e in autostartVertex)
                e.To.Execute(exe);
        }

        public void SetUserInteraction(IUserInteraction userInteraction)
        {
            _UserInteraction = userInteraction;
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            LogLevel = 2;

            InitializeLog();

            PreBootstrap();

            

            Bootstrap();

            ExecutionFlowHelper.StartTransaction();            


            LoadFromBootstrap.Execute();

            InitRootVariables();

            Init_AfterZeroCodeDefintionCreated();

            FillBootstrapVertexes();

            AddHardware();


            UserInteraction.UserInteractionInitialize();
            

            StaticMetaInitialize();

            LoadFromFileSystem();



            AddDrives();



            ExecutionFlowHelper.CommitTransaction();

            IsInitialized = true;

            ExecutionFlowHelper.StartTransaction();
        }

        public void Initialize_AfterUXInitialized()
        {
            Autostart();
        }
    }
}