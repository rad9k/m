using m0.Foundation;
using m0.UIWpf.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using m0.UIWpf.Commands;
using m0.Util;
using m0.ZeroTypes;

namespace m0.UIWpf.Controls
{
    public class m0ContextMenu : ContextMenu
    {
        IPlatformClass PlatformClassOfVisualiserMenuHasBeenOpenedOn;
        IVertex root;
        public IVertex EdgeVertex;

        public m0ContextMenu(IPlatformClass pc)
        {
            root = MinusZero.Instance.Root;

            PlatformClassOfVisualiserMenuHasBeenOpenedOn = pc;                        

            this.Opened += m0ContextMenu_Opened;
        }

        void AddStandardMenuItems()
        {
            AddOpen();

            AddExecute();

            AddSeparator();

            AddOpenFormCode();

            AddSeparator();

            AddOpenAs();

            AddOpenAsSpecial();

            AddSeparator();

            //AddNewDiagram();

            AddNewUX();

            AddSeparator();

            AddNew();

            AddSeparator();

            AddQuery();

            AddSeparator();

            AddCutCopyPasteItems();
        }

        void m0ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            EdgeVertex=null;

            if (PlatformClassOfVisualiserMenuHasBeenOpenedOn is IHasLocalizableEdges && PlatformClassOfVisualiserMenuHasBeenOpenedOn is IInputElement)
            {
                Point p = Mouse.GetPosition((IInputElement)PlatformClassOfVisualiserMenuHasBeenOpenedOn);
                EdgeVertex = ((IHasLocalizableEdges)PlatformClassOfVisualiserMenuHasBeenOpenedOn).GetEdgeByPoint(p);
                

                //EnableMenuItems();

                this.Items.Clear();

                ExtraCommandHook ech = new ExtraCommandHook(this);

                ech.CheckAndAddExtraCommand();

                AddStandardMenuItems();

                if (EdgeVertex == null)
                {
                    DisableMenuItems();
                    return;
                }

                FillNewVertexAndEdgeBySchemaMenu();
            }

            if (EdgeVertex == null)
            {
                DisableMenuItems();
                return;
            }
        }

        private void FillNewVertexAndEdgeBySchemaMenu()
        {
            IVertex baseVertex = EdgeVertex.Get(false, @"To:");

            if (baseVertex == null)
                return;

            IVertex r = baseVertex.GetAll(false, @"$Is:");

            if (r.Count() == 0)
                r = EdgeVertex.GetAll(false, @"Meta:"); ;

            if (r.Count() == 0 || r.FirstOrDefault().To.Value==null || GeneralUtil.CompareStrings(r.FirstOrDefault().To.Value, "$Empty"))
            {
                NewVertexBySchema.IsEnabled = false;
                NewEdgeBySchema.IsEnabled = false;
                return;
            }
            
            NewVertexBySchema.Items.Clear();
            NewEdgeBySchema.Items.Clear();

            foreach (IEdge e in r)
                NewVertexAndEdgeBySchema_FillForMeta(baseVertex, e.To, VertexOperations.GetChildEdges(e.To));

            NewVertexAndEdgeBySchema_FillForMeta(baseVertex, MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex"), MinusZero.Instance.Root.GetAll(false, @"System\Meta\Base\Vertex\"));

            NewVertexBySchema.IsEnabled = true;
            NewEdgeBySchema.IsEnabled = true;
        }

        private void NewVertexAndEdgeBySchema_FillForMeta(IVertex baseVertex, IVertex meta, IVertex metaEdges)
        {
            //if (meta.Value != null && !GeneralUtil.CompareStrings(meta.Value, "$Empty"))
            //{
                MenuItem i = createMenuItem(meta.Value.ToString());
                NewVertexBySchema.Items.Add(i);

                MenuItem ie = createMenuItem(meta.Value.ToString());
                NewEdgeBySchema.Items.Add(ie);

            foreach (IEdge ee in metaEdges)
                    if (ee.To.Value != null && !GeneralUtil.CompareStrings(ee.To.Value, "$Empty"))
                    {
                        MenuItem ii = createMenuItem(ee.To.Value.ToString());                        

                        ii.Tag = ee.To;
                        i.Items.Add(ii);

                        MenuItem iie = createMenuItem(ee.To.Value.ToString());
                        iie.Tag = ee.To;
                        ie.Items.Add(iie);

                        if (VertexOperations.TestIfNewEdgeValid(baseVertex, ee.To, null) != null)
                        {
                            ii.IsEnabled = false;
                            iie.IsEnabled = false;
                        }

                        ii.Click += OnNewVertexBySchema;
                       iie.Click += OnNewEdgeBySchema;
                    }
            //}
        }

        private void DisableMenuItems(){        
            ChangeActiveState_Reccurent(this, false);
        }

        private void EnableMenuItems()
        {
            ChangeActiveState_Reccurent(this, true);
        }

        private void ChangeActiveState_Reccurent(ItemsControl control, bool state){
            foreach (object o in control.Items)
                if (o is ItemsControl)
                {
                    ItemsControl c = (ItemsControl)o;

                    c.IsEnabled = state;

                    ChangeActiveState_Reccurent(c, state);
                }
            
        }

        public static MenuItem createMenuItem(string header)
        {
            MenuItem m = new MenuItem();
            m.Header = header;
            return m;
        }

        public void AddSeparator(){
            this.Items.Add(new Separator());
        }

        private MenuItem NewVertexBySchema;
        private MenuItem NewEdgeBySchema;

        private void AddOpen()
        {
            MenuItem NewVertex = createMenuItem("Open");
            NewVertex.Click += OnOpen;
            this.Items.Add(NewVertex);
        }

        private void AddNew()
        {
            MenuItem NewVertex = createMenuItem("New Vertex");
            NewVertex.Click += OnNewVertex;
            this.Items.Add(NewVertex);

            NewVertexBySchema = createMenuItem("New Vertex by Meta Schema");            
            this.Items.Add(NewVertexBySchema);

            MenuItem NewEdge = createMenuItem("New Edge");
            NewEdge.Click += OnNewEdge;
            this.Items.Add(NewEdge);

            NewEdgeBySchema = createMenuItem("New Edge by Meta Schema");
            this.Items.Add(NewEdgeBySchema);
        }

        private void AddNewDiagram()
        {
            MenuItem NewVertex = createMenuItem("New Diagram");
            NewVertex.Click += OnNewDiagram;
            this.Items.Add(NewVertex);
        }

        private void AddNewUX()
        {
            MenuItem NewVertex = createMenuItem("New Diagram / UX");
            NewVertex.Click += OnNewUX;
            this.Items.Add(NewVertex);
        }

        private void AddCutCopyPasteItems()
        {
            MenuItem Cut = createMenuItem("Cut");
            Cut.Click += OnCut;
            this.Items.Add(Cut);

            MenuItem Copy = createMenuItem("Copy");
            Copy.Click += OnCopy;
            this.Items.Add(Copy);

            MenuItem Paste = createMenuItem("Paste");
            Paste.Click += OnPaste;
            this.Items.Add(Paste);

            MenuItem Delete = createMenuItem("Delete");
            Delete.Click += OnDelete;
            this.Items.Add(Delete);
        }

        private void AddQuery()
        {
            MenuItem Query = createMenuItem("Query");
            Query.Click += OnQuery;
            this.Items.Add(Query);            
        }

        void AddOpenFormCode()
        {           
            IVertex formVis = root.Get(false, @"System\Meta\Visualiser\Form");

            IVertex codeVis = root.Get(false, @"System\Meta\Visualiser\Code");

           
            MenuItem formMenuItem = createMenuItem("Open Form");

            formMenuItem.Tag = formVis;

            formMenuItem.Click += OnOpenVisualiser;

            Items.Add(formMenuItem);


            MenuItem codeMenuItem = createMenuItem("Open Code");

            codeMenuItem.Tag = codeVis;

            codeMenuItem.Click += OnOpenVisualiser;

            Items.Add(codeMenuItem);

        }

        private void AddExecute()
        {
            MenuItem Execute = createMenuItem("Execute");
            Execute.Click += OnExecute;
            this.Items.Add(Execute);
        }

        void AddOpenAs()
        {
            MenuItem OpenVisualiser = createMenuItem("Open As");

            this.Items.Add(OpenVisualiser);

            // IVertex vislist = root.GetAll(false, @"System\Meta\Visualiser\"); BaseEdge ones currently not supported

            IVertex vislist = root.GetAll(false, @"System\Meta\Visualiser\Class:{$Inherits:UXItem,BaseEdgeTarget:Any}");

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenVisualiser;

                OpenVisualiser.Items.Add(v);
            }
        }

        void AddOpenAsSpecial()
        { 
            MenuItem Special = createMenuItem("Open special");

            this.Items.Add(Special);

            /////////////////////// meta

            MenuItem OpenMetaVisualiser = createMenuItem("Open Visualiser for Meta");
            Special.Items.Add(OpenMetaVisualiser);

            IVertex vislist = root.GetAll(false, @"System\Meta\Visualiser\Class:{$Inherits:UXItem,BaseEdgeTarget:Any}");

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenMetaVisualiser;

                OpenMetaVisualiser.Items.Add(v);
            }

            /*
            /////////////////////// floating

            MenuItem OpenVisualiserFloating = createMenuItem("Open Floating Visualiser");
            Special.Items.Add(OpenVisualiserFloating);

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenVisualiserFloating;

                OpenVisualiserFloating.Items.Add(v);
            } */           

            /////////////////////// base synchronised

            MenuItem OpenVisualiserSelectedBase = createMenuItem("Open Master-Detail (SelectedEdges<>BaseEdge synchronised) Visualiser");
            Special.Items.Add(OpenVisualiserSelectedBase);

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenVisualiserFirstSelectedEdge;

                OpenVisualiserSelectedBase.Items.Add(v);
            }

            /////////////////////// selected synchronised

            vislist = root.GetAll(false, @"System\Meta\Visualiser\Class:{$Inherits:HasSelectedEdges,BaseEdgeTarget:Any}");

            MenuItem OpenVisualiserSelectedSelected = createMenuItem("Open SelectedEdges<>SelectedEdges synchronised Visualiser");
            Special.Items.Add(OpenVisualiserSelectedSelected);            

            foreach (IEdge vis in vislist)
            {
                MenuItem v = createMenuItem(vis.To.Value.ToString());

                v.Tag = vis.To;

                v.Click += OnOpenVisualiserSelectedSelectedSynchronised;

                OpenVisualiserSelectedSelected.Items.Add(v);
            }
        }

        /*m0ContextMenu getMenu(MenuItem i)
        {
            if (i.Parent is m0ContextMenu)
                return (m0ContextMenu)i.Parent;

            return (m0ContextMenu)getMenu((MenuItem)i.Parent);
        }*/
        


        void OnNewVertex(object sender, System.Windows.RoutedEventArgs e)
        {           
            BaseCommands.NewVertex(this.EdgeVertex, null);
        }
        
        void OnNewVertexBySchema(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is MenuItem)
                BaseCommands.NewVertexBySchema(this.EdgeVertex, (IVertex)((MenuItem)sender).Tag);            
        }

        void OnNewEdgeBySchema(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is MenuItem)
                BaseCommands.NewEdgeBySchema(this.EdgeVertex, (IVertex)((MenuItem)sender).Tag);
        }

        void OnNewEdge(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.NewEdge(this.EdgeVertex, null);
        }

        void OnNewDiagram(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.NewDiagram(this.EdgeVertex, null);
        }

        void OnNewUX(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.NewUX(this.EdgeVertex, null);
        }

        void OnCut(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Cut(this.EdgeVertex, PlatformClassOfVisualiserMenuHasBeenOpenedOn.Vertex);

            FromCopyPlatformClass = PlatformClassOfVisualiserMenuHasBeenOpenedOn;
        }


        IPlatformClass FromCopyPlatformClass;

        void OnCopy(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Copy(this.EdgeVertex, PlatformClassOfVisualiserMenuHasBeenOpenedOn.Vertex);

            FromCopyPlatformClass = PlatformClassOfVisualiserMenuHasBeenOpenedOn;
        }

        void OnPaste(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Paste(this.EdgeVertex, PlatformClassOfVisualiserMenuHasBeenOpenedOn.Vertex);

            if (FromCopyPlatformClass is IHasSelectableEdges)
                ((IHasSelectableEdges)FromCopyPlatformClass).UnselectAllSelectedEdges();
        }

        void OnDelete(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Delete(this.EdgeVertex, PlatformClassOfVisualiserMenuHasBeenOpenedOn.Vertex);

            if (PlatformClassOfVisualiserMenuHasBeenOpenedOn is IHasSelectableEdges)
                ((IHasSelectableEdges)PlatformClassOfVisualiserMenuHasBeenOpenedOn).UnselectAllSelectedEdges();
        }

        void OnQuery(object sender, System.Windows.RoutedEventArgs e)
        {            
            BaseCommands.Query(this.EdgeVertex, null);
        }  

        void OnOpen(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.OpenDefaultVisualiser(this.EdgeVertex, false);
        }

        void OnExecute(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.Execute(this.EdgeVertex, null);
        }

        void OnOpenVisualiser(object sender, System.Windows.RoutedEventArgs e)
        {            
            BaseCommands.OpenVisualiser(this.EdgeVertex, ((IVertex)((MenuItem)sender).Tag), false);
        }

        void OnOpenMetaVisualiser(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.OpenMetaVisualiser(this.EdgeVertex, ((IVertex)((MenuItem)sender).Tag));
        }

        void OnOpenVisualiserFloating(object sender, System.Windows.RoutedEventArgs e)
        {
            BaseCommands.OpenVisualiserFloating(this.EdgeVertex, ((IVertex)((MenuItem)sender).Tag));
        }

        void OnOpenVisualiserFirstSelectedEdge(object sender, System.Windows.RoutedEventArgs e)
        {            
            IVertex input = MinusZero.Instance.CreateTempVertex();

            IVertex root = MinusZero.Instance.Root;

            input.AddEdge(root.Get(false, @"System\Meta\UserCommands\VisualiserClass"), ((IVertex)((MenuItem)sender).Tag));
            input.AddEdge(root.Get(false, @"System\Meta\UserCommands\MasterVisualiser"), PlatformClassOfVisualiserMenuHasBeenOpenedOn.Vertex);

            BaseCommands.OpenVisualiserFirstSelectedEdgeSynchronised(this.EdgeVertex, input);
        }

        void OnOpenVisualiserSelectedSelectedSynchronised(object sender, System.Windows.RoutedEventArgs e)
        {            
            IVertex input = MinusZero.Instance.CreateTempVertex();

            IVertex root=MinusZero.Instance.Root;

            input.AddEdge(root.Get(false, @"System\Meta\UserCommands\VisualiserClass"), ((IVertex)((MenuItem)sender).Tag));
            input.AddEdge(root.Get(false, @"System\Meta\UserCommands\MasterVisualiser"), PlatformClassOfVisualiserMenuHasBeenOpenedOn.Vertex);

            BaseCommands.OpenVisualiserSelectedSelected(this.EdgeVertex, input);
        }

    }
}
