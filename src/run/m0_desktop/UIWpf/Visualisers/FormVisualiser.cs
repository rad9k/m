using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Commands;
using m0.ZeroUML;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using m0.UIWpf.Visualisers.Method;
using m0.UIWpf.Visualisers.Helper;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Foundation;
using System.Threading;

namespace m0.UIWpf.Visualisers
{
    public class ControlInfo
    {
        public FrameworkElement GapControl;
        public FrameworkElement MetaControl;
        public FrameworkElement DataControl;
        public int Column;        
    }

    public class SectionInfo
    {
        public Panel Panel;
        public int Column;
    }

    public class TabInfo
    {
        public int TotalNumberOfControls;
        public int CurrentNumberOfControls;
        public IDictionary<string, SectionInfo> Sections;
        public IDictionary<IVertex, ControlInfo> ControlInfos;
        public TabItem TabItem;
        public bool WidthCorrectionDone;

        public TabInfo()
        {
            Sections=new Dictionary<string,SectionInfo>();
            ControlInfos = new Dictionary<IVertex, ControlInfo>();
            TotalNumberOfControls=0;
            CurrentNumberOfControls = 0;
        }
    }

    public class FormVisualiser : ContentControl, IListVisualiser, ITypedEdge
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        bool DisplayBaseVertex = true; /////////////////////////////////////////

        bool SectionsAsTabs;
        bool MetaOnLeft;
        bool MetaAlignRight;
        bool ExpertMode;

        bool HasTabs { get; set; }
        int ColumnNumber { get; set; }
        IDictionary<string, TabInfo> TabList { get; set; }

        TabControl TabControl;

        double marginOnRight = 1;
        double marginBetweenColumns = 5;
        double sectionControlBorderWidth = 17;
        double metaVsDataSeparator = 4;
        double controlLineVsControlLineSeparator = 4;

        double lastCorrectedWidth = 0;


        TabItem TabControlSelectedItem;


        static string[] _MetaTriggeringUpdateVertex = new string[] { "ExpertMode", "ColumnNumber", "MetaOnLeft", "MetaAlignRight", "SectionsAsTabs" };
        
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void ViewAttributesUpdated() { }

        public void UnselectAllSelectedEdges() { }

        // TypedEdge START

        public FormVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public FormVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            new ListVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form"),
                this, 
                "FormVisualiser", 
                this, 
                false, 
                new List<string> { @""/*, @"BaseEdge:\To:"*/}, // currently the form does not need BaseEdge:\To:
                "AtomVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst);            

            SetVertexDefaultValues();

            this.BorderBrush = new SolidColorBrush(Colors.Red);
            this.BorderThickness = new Thickness(10);

            this.Foreground = new SolidColorBrush(Colors.Purple);
        }
        
        public void OnLoad(object sender, RoutedEventArgs e)
        {            
            // DO NOT WANT CONTEXTMENU HERE
        }

        public void SelectedVerticesUpdated() { }

        private TabInfo getActiveTabInfo()
        {
            if (HasTabs)
            {
                TabItem i = TabControlSelectedItem;

                if (i == null && TabControl != null)
                    i = TabControl.SelectedItem as TabItem;

                if (i == null)
                    return TabList.Values.FirstOrDefault();

                foreach (TabInfo tie in TabList.Values)
                    if (tie.TabItem.Header == i.Header)
                        return tie;

                return null;
            }
            else
                return TabList[""];
        }

        private IVertex getMetaForForm()
        {
            if (Vertex.Get(false, @"BaseEdge:\Meta:") == null/* || Vertex.Get(false, @"BaseEdge:\Meta:").Count() == 0*/)
                return null;

            IVertex v = GraphUtil.GetMostInheritedMeta(Vertex.Get(false, @"BaseEdge:\To:"), Vertex.Get(false, @"BaseEdge:\Meta:"));
            // XXX there is error in GetMostInheritedMeta - see it 

            if (v != null && v.Get(false, @"$EdgeTarget:") != null)
                return v.Get(false, @"$EdgeTarget:");
            else
                return v;
        }

        private string getGroup(IVertex meta)
        {
            if (SectionsAsTabs) {
                if (meta == null)
                    return " | ";

                string _section = (string)GraphUtil.GetValue(meta.Get(false, "$Section:"));
                string _group = (string)GraphUtil.GetValue(meta.Get(false, "$Group:"));

                if (_group == null && _section == null)
                    return "";

                if (_group == null)
                    return "| " + _section;

                if (_section == null)
                    return _group;

                return _group + " | " + _section;
            }
            else
            {
                if (meta == null)
                    return "";

                string _group = (string)GraphUtil.GetValue(meta.Get(false, "$Group:"));

                if (_group == null)
                    return "";
                else
                    return _group;
            }
        }

        private string getSection(IVertex meta)
        {
            if (meta == null)
                return null;

            if (SectionsAsTabs)
                return null;
            else
                return (string)GraphUtil.GetValue(meta.Get(false, "$Section:"));
        }

        bool BaseVertexEdgeAdded_PreFill = false;

        private void PreFillFormAnalyseEdge(IVertex meta, bool isSet)
        {
            if (DisplayBaseVertex && BaseVertexEdgeAdded_PreFill == false)
            {
                BaseVertexEdge = getMetaForForm();
                BaseVertexEdgeAdded_PreFill = true;
                PreFillFormAnalyseEdge(BaseVertexEdge, false);
            }

            string group = getGroup(meta);
            string section = getSection(meta);

            TabInfo t;

            if (group != null && group != "")
                HasTabs = true;

            if (TabList.ContainsKey(group))
                t = TabList[group];
            else
            {
                t = new TabInfo();
                TabList.Add(group, t);
            }

            //if(isSet==false)
            t.TotalNumberOfControls++;

        }

        private void PreFillForm(IVertex metaForForm)
        {
            TabList = new Dictionary<string, TabInfo>();

            IVertex basTo = Vertex.Get(false, @"BaseEdge:\To:");

            List<IEdge> childs = new List<IEdge>();            

            if (metaForForm == null || metaForForm.Count() == 0) // if Form is not typed
            {
                IList<IVertex> visited = new List<IVertex>();

                foreach (IEdge e in basTo)
                {
                    childs.Add(e);
                    if (!visited.Contains(e.Meta) && e.Meta.Get(false, "$Hide:") == null)
                        if (basTo.GetAll(false, e.Meta + ":").Count() > 1)
                        {
                            PreFillFormAnalyseEdge(e.Meta, true);
                            visited.Add(e.Meta);
                        }
                        else
                            PreFillFormAnalyseEdge(e.Meta, false);
                }
            }
            else // Form is typed
            {
                foreach (IEdge e in VertexOperations.GetChildEdges(metaForForm))
                {
                    childs.Add(e);

                    if (e.To.Get(false, "$Hide:") == null)
                        if (GraphUtil.GetIntegerValue(e.To.Get(false, "$MaxCardinality:")) > 1 || GraphUtil.GetIntegerValue(e.To.Get(false, "$MaxCardinality:")) == -1)
                            PreFillFormAnalyseEdge(e.To, true);
                        else
                            PreFillFormAnalyseEdge(e.To, false);
                }
            }

            if (ExpertMode)
            {
                foreach (IEdge e in MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex"))
                {
                    bool contains = false;

                    foreach (IEdge ee in childs)
                        if (GeneralUtil.CompareStrings(ee.To, e.To))
                            contains = true;

                    if (contains == false)
                        PreFillFormAnalyseEdge(e.To, false);
                }
            }

            if (ExecutableVisualiserFactory.IsOfExecutableMeta(metaForForm))
                foreach(IEdge e in ExecutableVisualiserFactory.GetExecutableEdges(metaForForm))
                    if (e.To.Get(false, "$Hide:") == null)
                        PreFillFormAnalyseEdge(e.To, false);
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                this.SizeChanged -= FormVisualiser_SizeChanged;

                VisualiserHelper.Dispose();
            }
        }

        IVertex BaseVertexEdge = null;
        

        public void BaseEdgeToUpdated()
        {
           // Content = new Button();
            VisualiserHelper.ForceVertexChangeOff = true;

            //ExecutionFlowHelper.

            this.SizeChanged -= FormVisualiser_SizeChanged;

            VisualiserHelper.DisposeAllChildVisualisersExceptWrap();

            BaseVertexEdgeAdded_PreFill = false;
            BaseVertexEdgeAdded = false;

            IVertex basTo = Vertex.Get(false, @"BaseEdge:\To:");            

            if (basTo != null)
            {
                if ((string)Vertex.Get(false, @"SectionsAsTabs:").Value == "True")
                    SectionsAsTabs = true;
                else
                    SectionsAsTabs = false;

                if ((string)Vertex.Get(false, @"MetaOnLeft:").Value == "True")
                    MetaOnLeft = true;
                else
                    MetaOnLeft = false;

                if ((string)Vertex.Get(false, @"MetaAlignRight:").Value == "True")
                    MetaAlignRight = true;
                else
                    MetaAlignRight = false;

                if ((string)Vertex.Get(false, @"ExpertMode:").Value == "True")
                    ExpertMode = true;
                else
                    ExpertMode = false;

                int? _columnNumber = GraphUtil.GetIntegerValue(Vertex.Get(false, @"ColumnNumber:"));

                if (_columnNumber != null)
                    ColumnNumber = (int)_columnNumber;

                IVertex metaForForm = getMetaForForm();

                PreFillForm(metaForForm);

                InitializeControlContent();


                List<IEdge> childs = new List<IEdge>();

                if (metaForForm==null||metaForForm.Count()==0) // if Form is not typed
                {
                    IList<IVertex> visited = new List<IVertex>();

                    foreach (IEdge e in basTo)
                    {
                        childs.Add(e);
                       
                        if (!visited.Contains(e.Meta)&&e.Meta.Get(false, "$Hide:") == null)
                            if (basTo.GetAll(false, e.Meta + ":").Count() > 1)
                            {
                                AddEdge(e.Meta, true); 
                                visited.Add(e.Meta);
                            }
                            else
                                AddEdge(e.Meta, false);
                    }
                }
                else // Form is typed
                {
                    foreach (IEdge e in VertexOperations.GetChildEdges(metaForForm))
                    {
                        childs.Add(e);
                        
                        if (e.To.Get(false, "$Hide:") == null)
                            if (GraphUtil.GetIntegerValue(e.To.Get(false, "$MaxCardinality:")) > 1 || GraphUtil.GetIntegerValue(e.To.Get(false, "$MaxCardinality:")) == -1)
                                AddEdge(e.To, true);
                            else
                                AddEdge(e.To, false);

                    }
                }
                
                if (ExpertMode)
                {
                    foreach (IEdge e in MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex"))
                    {
                        bool contains = false;

                        foreach (IEdge ee in childs)
                            if (GeneralUtil.CompareStrings(ee.To, e.To))
                                contains = true;

                        if (contains == false)
                            AddEdge(e.To, false);
                    }
                }
                
                if (ExecutableVisualiserFactory.IsOfExecutableMeta(metaForForm))
                    foreach (IEdge e in ExecutableVisualiserFactory.GetExecutableEdges(metaForForm))
                        if (e.To.Get(false, "$Hide:") == null)
                            AddEdge(e.To, false);
                
                if (MetaOnLeft){
                    if (HasTabs)
                    {
                        ScheduleActiveTabCorrection();
                    }
                    else if (TabList.ContainsKey(""))
                    {
                        this.SizeChanged -= FormVisualiser_SizeChanged;
                        CorrectWidth(TabList[""]);
                        lastCorrectedWidth = this.ActualWidth;
                        this.SizeChanged += FormVisualiser_SizeChanged;
                    }
                }
                
            }
            //return;
            VisualiserHelper.ForceVertexChangeOff = false;
        }

        protected void CorrectWidth(TabInfo i, bool allowUpdateLayout = true)
        {
            if (i.ControlInfos.Count() == 0)
                return;

            if (allowUpdateLayout && !HasTabs && !i.WidthCorrectionDone)
                this.UpdateLayout();

           if (i.ControlInfos.First().Value.MetaControl.ActualWidth == 0)
                return;
            
            i.WidthCorrectionDone = true;
           

            double oneColumnWidth = Math.Floor(((this.ActualWidth - marginOnRight) / ColumnNumber) - marginBetweenColumns);
                      
                    double[] maxMetaWidthInColumn = new double[ColumnNumber];

                    foreach (ControlInfo ci in i.ControlInfos.Values)
                        if (ci.MetaControl.ActualWidth > maxMetaWidthInColumn[ci.Column])
                            maxMetaWidthInColumn[ci.Column] = Math.Floor(ci.MetaControl.ActualWidth);

                    for (int c = 0; c < ColumnNumber; c++)
                        if (maxMetaWidthInColumn[c] > oneColumnWidth - metaVsDataSeparator - 5)
                            maxMetaWidthInColumn[c] = Math.Floor(oneColumnWidth * 0.5);

            if(i.Sections.Count()==0)
            foreach (KeyValuePair<IVertex, ControlInfo> ci in i.ControlInfos) // if there are no sections
                    {                       
                        ci.Value.MetaControl.Width = maxMetaWidthInColumn[ci.Value.Column];
                        ci.Value.GapControl.Width = 0;

                    double ci_Value_DataControl_Width_to_be = Math.Floor(oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - 5);

                    if (ci_Value_DataControl_Width_to_be < 0)
                        ci_Value_DataControl_Width_to_be = 0;

                    ci.Value.DataControl.Width = ci_Value_DataControl_Width_to_be;
                    }
            else
                foreach (KeyValuePair<IVertex, ControlInfo> ci in i.ControlInfos) // if there are sections
                {
                    ci.Value.MetaControl.Width = maxMetaWidthInColumn[ci.Value.Column];

                    if (getSection(ci.Key) == null)
                    {
                        ci.Value.GapControl.Width = (sectionControlBorderWidth / 2) - 2;
                        ci.Value.DataControl.Width = Math.Max(0, Math.Floor(oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - 9 - sectionControlBorderWidth / 2));
                    }
                    else
                    {
                        ci.Value.GapControl.Width = 0;
                        ci.Value.DataControl.Width = Math.Max(0, Math.Floor(oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - sectionControlBorderWidth));
                    }
                }
        }

        protected object CreateColumnedContent()
        {
            Grid g = new Grid();

            bool notFirstColumn = false;

            int columnCount = 0;

            for (int i = 0; i < ColumnNumber; i++)
            {
                if (notFirstColumn)
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(marginBetweenColumns);
                    g.ColumnDefinitions.Add(cd);

                    columnCount++;
                }
                
                g.ColumnDefinitions.Add(new ColumnDefinition());
                StackPanel s = new StackPanel();
                Grid.SetColumn(s, columnCount);
                g.Children.Add(s);

                columnCount++;

                notFirstColumn = true;
            }

            ColumnDefinition cdd = new ColumnDefinition();
            cdd.Width = new GridLength(marginOnRight);
            g.ColumnDefinitions.Add(cdd);

            columnCount++;

            return g;
        }

        private void InitializeControlContent()
        {               
            if (HasTabs)
            {
                TabControl = new TabControl();

                TabControl.SelectionChanged += TabControl_SelectionChanged;

                Content = TabControl;

                foreach (KeyValuePair<string,TabInfo> t in TabList)
                {
                    TabItem i = new TabItem();
                    i.Header = t.Key ;
                    TabControl.Items.Add(i);
                    t.Value.TabItem = i;
                    i.Tag = t.Value;

                    i.Content = CreateColumnedContent();
                }

                if (TabControl.Items.Count > 0)
                    TabControl.SelectedIndex = 0;

                TabControlSelectedItem = TabControl.SelectedItem as TabItem;

                if (MetaOnLeft)
                    ScheduleActiveTabCorrection();
            }
            else
                Content = CreateColumnedContent();

            if (MetaOnLeft)
                this.SizeChanged += FormVisualiser_SizeChanged;

           // Content = new Button();
        }

        private void ScheduleActiveTabCorrection()
        {
            if (!HasTabs || !MetaOnLeft || isDisposed || TabList == null)
                return;

            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    if (!HasTabs || !MetaOnLeft || isDisposed || TabList == null)
                        return;

                    TabInfo activeTab = getActiveTabInfo();
                    if (activeTab == null)
                        return;

                    this.SizeChanged -= FormVisualiser_SizeChanged;
                    try
                    {
                        CorrectWidth(activeTab, false);
                        lastCorrectedWidth = this.ActualWidth;
                    }
                    finally
                    {
                        this.SizeChanged += FormVisualiser_SizeChanged;
                    }
                }),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControlSelectedItem = (TabItem)TabControl.SelectedItem;

            if (MetaOnLeft && TabControlSelectedItem != null && TabControlSelectedItem.Tag is TabInfo tabInfo)
            {
                this.SizeChanged -= FormVisualiser_SizeChanged;
                CorrectWidth(tabInfo);
                lastCorrectedWidth = this.ActualWidth;
                this.SizeChanged += FormVisualiser_SizeChanged;

                ScheduleActiveTabCorrection();
            }
        }

        private void FormVisualiser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
                return;

            if (TabList == null)
                return;

            if (this.ActualWidth < 1)
                return;

            if (Math.Abs(this.ActualWidth - lastCorrectedWidth) < 3)
                return;

            lastCorrectedWidth = this.ActualWidth;

            this.SizeChanged -= FormVisualiser_SizeChanged;
            try
            {
                if (HasTabs)
                {
                    TabInfo activeTab = getActiveTabInfo();
                    if (activeTab != null)
                        CorrectWidth(activeTab, false);
                }
                else
                {
                    if (TabList.ContainsKey(""))
                        CorrectWidth(TabList[""], false);
                }
            }
            finally
            {
                this.SizeChanged += FormVisualiser_SizeChanged;
            }
        }

        protected Panel GetUIPlace(string group,string section, ControlInfo ci)
        {
            TabInfo t = TabList[group];

            
            int targetColumn = (int)((double)t.CurrentNumberOfControls * (double)ColumnNumber / (double)t.TotalNumberOfControls);

            if (targetColumn >= ColumnNumber)
                targetColumn = ColumnNumber-1;

            t.CurrentNumberOfControls++;

            ci.Column = targetColumn;

            if (section != null)
            {
                if (t.Sections.ContainsKey(section))
                {
                    ci.Column = t.Sections[section].Column;

                    return ((Panel)t.Sections[section].Panel);
                }

                Panel toAdd;

                if (HasTabs)
                    toAdd=(Panel)((Grid)t.TabItem.Content).Children[targetColumn];
                else
                    toAdd=(Panel)((Grid)this.Content).Children[0];

                GroupBox g = new GroupBox();

                //Expander g = new Expander();

                g.BorderBrush = (Brush)FindResource("0ForegroundBrush");

                TextBlock Header = new TextBlock();
                Header.FontWeight = WpfUtil.BoldWeight;
                Header.Text = section;
                g.Header = Header;

                g.BorderThickness = new Thickness(2); // can be 1, but 2 is more separated

                toAdd.Children.Add(g);

                Border b = new Border(); // separator

                b.BorderThickness = new System.Windows.Thickness(0, controlLineVsControlLineSeparator, 0, 0);

                toAdd.Children.Add(b);

                StackPanel gp = new StackPanel();

                g.Content = gp;

                SectionInfo si = new SectionInfo();
                si.Panel = gp;
                si.Column = targetColumn;

                t.Sections.Add(section, si);

                return gp;
            }


            if(HasTabs)
                return (Panel)((Grid)t.TabItem.Content).Children[targetColumn];
            else
                return (Panel)((Grid)this.Content).Children[targetColumn];
        }

        bool BaseVertexEdgeAdded = false;

        protected void AddEdge(IVertex meta, bool isSet)
        {
            if (DisplayBaseVertex && BaseVertexEdgeAdded == false) { 
                BaseVertexEdge = getMetaForForm();
                BaseVertexEdgeAdded = true;
                AddEdge(BaseVertexEdge, false);
            }
            
            string group = getGroup(meta);
            string section = getSection(meta);  

            IVertex r = MinusZero.Instance.Root;

            TextBlock metaControl = new TextBlock();

            if (meta == null)
            {
                metaControl.Text = "XXX";
                metaControl.Height = 0;
            }else
                metaControl.Text = (string)meta.Value;

            metaControl.FontStyle = FontStyles.Italic;
            metaControl.FontWeight = WpfUtil.MetaWeight;
            metaControl.Foreground = (Brush)FindResource("0GrayBrush");
            metaControl.FontStyle = FontStyles.Italic;

            System.Windows.FrameworkElement dataControl = null;
            
            if (isSet)
            {
                IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, Vertex.Get(false, @"BaseEdge:\To:"));

                TableVisualiser tableVisualiser = new TableVisualiser(baseEdgeVertex, Vertex, false);

                if (ExpertMode)
                    GraphUtil.SetVertexValue(tableVisualiser.Vertex, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Table\ExpertMode"), "True");               

                // need to remove and add to have "transaction" // THIS DOES NOT WORK
                GraphUtil.CreateOrReplaceEdge(tableVisualiser.Vertex.Get(false, "ToShowEdgesMeta:"), r.Get(false, @"System\Meta\ZeroTypes\Edge\Meta"), meta);

               // IVertex v = tableVisualiser.Vertex.Get(false, "ToShowEdgesMeta:"); /////////////// this ToShowEdgesMeta is a trash bin XXX

                //tableVisualiser.Vertex.AddEdge(MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Table\ToShowEdgesMeta"), v);

                //GraphUtil.DeleteEdgeByMeta(tableVisualiser.Vertex, "ToShowEdgesMeta");                

                // no need for this

                dataControl = tableVisualiser; 
            }
            else
            {
                if (meta == BaseVertexEdge)
                {
                    IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(Vertex.GetAll(false, @"BaseEdge:\To:").FirstOrDefault());

                    StringVisualiser sv = new StringVisualiser(baseEdgeVertex, this.Vertex, false);

                    //Edge.ReplaceEdgeVertexEdges(sv.Vertex.Get(false, "BaseEdge:"), Vertex.GetAll(false, @"BaseEdge:\To:").FirstOrDefault());

                    baseEdgeVertex.AddExternalReference();

                    dataControl = sv;
                }
                else
                if (ExecutableVisualiserFactory.IsExecutableVertex(meta))
                    dataControl = ExecutableVisualiserFactory.CreateExecutableVisualiser(Vertex.GetAll(false, @"BaseEdge:\To:").FirstOrDefault(), meta);
                else
                {
                    VisualiserEditWrapper w = new VisualiserEditWrapper(Vertex);

                    IEdge e;

                    e = Vertex.GetAll(false, @"BaseEdge:\To:\" + (string)meta.Value + ":").FirstOrDefault();

                    if (e == null) // no edge in data vertex
                    {
                        w.BaseEdge = new EasyEdge(Vertex.Get(false, @"BaseEdge:\To:"), meta, null);
                    }
                    else
                        w.BaseEdge = e;

                    dataControl = w;
                }
                
           
            }

            ControlInfo ci = new ControlInfo();

            ci.MetaControl = metaControl;
            ci.DataControl = dataControl;

            if (meta == null)
            { // BaseEdgeVertex
                IVertex metaVertex = MinusZero.Instance.CreateTempVertex();

                metaVertex.AddExternalReference();

                TabList[group].ControlInfos.Add(metaVertex, ci);
            }
            else
            {
                if (TabList[group].ControlInfos.ContainsKey(meta))
                {
                    int x = 0; // same meta sub vertex two times in meta vertex
                }
                else
                    TabList[group].ControlInfos.Add(meta, ci);
            }
            
            Panel place = GetUIPlace(group,section,ci);

            if (MetaAlignRight)
                metaControl.TextAlignment = TextAlignment.Right;
            else
                metaControl.TextAlignment = TextAlignment.Left;


            if (MetaOnLeft)
            {                
                StackPanel s=new StackPanel();
                s.Orientation=Orientation.Horizontal;

                ci.GapControl = new StackPanel();
                
                s.Children.Add(ci.GapControl);

                s.Children.Add(metaControl);

                Border b2 = new Border();

                b2.BorderThickness = new System.Windows.Thickness(metaVsDataSeparator, 0, 0, 0);

                s.Children.Add(b2);

                s.Children.Add(dataControl);

                place.Children.Add(s);
            }
            else
            {
                place.Children.Add(metaControl);
                
                place.Children.Add(dataControl);
            }


            Border b = new Border();

            b.BorderThickness = new System.Windows.Thickness(0, controlLineVsControlLineSeparator, 0, 0);

            place.Children.Add(b);
        }

        protected virtual void SetVertexDefaultValues()
        {
            Vertex.Get(false, "Scale:").Value = 100;
            Vertex.Get(false, "ColumnNumber:").Value = 1;
            Vertex.Get(false, "SectionsAsTabs:").Value = "False";
            Vertex.Get(false, "MetaOnLeft:").Value = "True";            
        }        

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        // LOCATION STUFF

        public IVertex GetEdgeByPoint(Point p)
        {
            TabInfo t = getActiveTabInfo();

            foreach(KeyValuePair<IVertex,ControlInfo> kvp in t.ControlInfos)
                if (VisualTreeHelper.HitTest(kvp.Value.MetaControl, TranslatePoint(p, kvp.Value.MetaControl)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    EdgeHelper.AddEdgeVertexEdgesOnlyTo(v, kvp.Key);
                    return(v);
                }
               
            return null;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }
    }
}
