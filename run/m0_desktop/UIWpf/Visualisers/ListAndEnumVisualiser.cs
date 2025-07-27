using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using m0.UIWpf.Foundation;
using System.Windows;
using m0.UIWpf.Commands;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class ListAndEnumVisualiser : ComboBox, IVisualiser, ITypedEdge
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        // TypedEdge START

        public ListAndEnumVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public ListAndEnumVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {            
            new AtomVisualiserHelper(
               parentVisualiser,
               isVolatile,
               MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\ListAndEnum"),
               this,
               "EnumVisualiser",
               this,
               false,
               new List<string> { @"", @"BaseEdge:\From:" },
               "EnumVisualiser",
               baseEdgeVertex,
               UpdateBaseEdgeCallSchemeEnum.OmmitSecond);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        bool DoingSelectionChanged = false;

        protected bool CanProceedUIUpdateEvent = true;

        protected override void OnSelectionChanged(SelectionChangedEventArgs _e)
        {
            if (!CanProceedUIUpdateEvent)
                return;

            if (DoingSelectionChanged == false)
            {
                DoingSelectionChanged = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                if (this.SelectedItem != null && ((ComboBoxItem)this.SelectedItem).Tag is IVertex)
                {
                    IVertex tag = (IVertex)((ComboBoxItem)this.SelectedItem).Tag;

                    IVertex bev = Vertex.Get(false, "BaseEdge:");

                    if (bev != null)
                    {
                        IVertex fromv = bev.Get(false, "From:");
                        IVertex metav = bev.Get(false, "Meta:");
                        IVertex tov = bev.Get(false, "To:");

                        if (tov != tag) // is there any change ?
                        {
                            //GraphUtil.ReplaceEdge(fromv, metav, tag);

                            GraphUtil.CreateOrReplaceEdge(fromv, metav, tag);

                            GraphUtil.CreateOrReplaceEdge(bev, MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\To"), tag);                            
                        }
                    }                    
                }

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                DoingSelectionChanged = false;
            }

            base.OnSelectionChanged(_e);
        }

        public void BaseEdgeToUpdated()
        {
            IVertex bev = Vertex.Get(false, "BaseEdge:");

            if (bev == null)
                return;

            IVertex fromv;
            IVertex metav;
            IVertex tov;

            fromv = bev.Get(false, "From:");

            if (fromv == null) // happens on dispose?
                return;

            metav = bev.Get(false, "Meta:");

            //

            bool hasTargetQuery = false;
            string targetQuery = null;

            IVertex targetQueryVertex = GraphUtil.GetQueryOutFirst(metav, "$TargetQuery", null);

            targetQuery = GraphUtil.GetStringValueOrNull(targetQueryVertex);

            if (targetQuery != null)
                hasTargetQuery = true;

            //

            tov = fromv.Get(false, metav.Value.ToString()+":");
            
            if (fromv != null && metav != null /*&& tov!=null*/)
            {
                CanProceedUIUpdateEvent = false;

                int ToBeSelectedIndex = -1;

                List<ComboBoxItem> valuesList = new List<ComboBoxItem>();

                ComboBoxItem ToBeComboBoxItem = null;
                string ToBeString = null;

                if (hasTargetQuery)
                    GetListValuesFromTargetQuery(targetQuery, tov, ref ToBeSelectedIndex, valuesList, ref ToBeComboBoxItem, ref ToBeString);
                else
                    GetListValuesFromEnum(metav, tov, ref ToBeSelectedIndex, valuesList, ref ToBeComboBoxItem, ref ToBeString);

                this.ItemsSource = valuesList;

                if (ToBeSelectedIndex != -1)
                {
                    this.IsEditable = true;

                    this.SelectedIndex = ToBeSelectedIndex;
                    this.SelectedItem = ToBeComboBoxItem;
                    this.Text = ToBeString;

                    this.IsEditable = false;
                }

                CanProceedUIUpdateEvent = true;
            }

        }

        private static void GetListValuesFromEnum(IVertex metav, IVertex tov, ref int ToBeSelectedIndex, List<ComboBoxItem> valuesList, ref ComboBoxItem ToBeComboBoxItem, ref string ToBeString)
        {
            int cnt = 0;

            foreach (IEdge e in metav.GetAll(false, @"$EdgeTarget:\EnumValue:"))
            {
                string value = e.To.Value.ToString();

                ComboBoxItem i = new ComboBoxItem();
                i.Content = value;
                i.Tag = e.To;

                valuesList.Add(i);

                if (tov != null && tov.Value.ToString() == value)
                {
                    ToBeSelectedIndex = cnt;
                    ToBeComboBoxItem = i;
                    ToBeString = value;
                }

                cnt++;
            }
        }

        private static void GetListValuesFromTargetQuery(string targetQuery, IVertex tov, ref int ToBeSelectedIndex, List<ComboBoxItem> valuesList, ref ComboBoxItem ToBeComboBoxItem, ref string ToBeString)
        {
            int cnt = 0;

            foreach (IEdge e in MinusZero.Instance.root.GetAll(false, targetQuery))
            {
                string value = e.To.Value.ToString();

                ComboBoxItem i = new ComboBoxItem();
                i.Content = value;
                i.Tag = e.To;

                valuesList.Add(i);

                if (tov != null && tov.Value.ToString() == value)
                {
                    ToBeSelectedIndex = cnt;
                    ToBeComboBoxItem = i;
                    ToBeString = value;
                }

                cnt++;
            }
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                VisualiserHelper.Dispose();
            }
        }

        public IVertex GetEdgeByPoint(Point point)
        {
            return Vertex.Get(false, @"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex edge)
        {
            throw new NotImplementedException();
        }
    }

}
