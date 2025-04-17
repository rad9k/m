using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace m0.UIWpf.UX
{
    /// <summary>
    /// Interaction logic for NewDiagramItem.xaml
    /// </summary>
    public partial class NewUXItem : Window
    {
        bool IsSet;

        IVertex baseedge;

        Point _mousePosition;

        void OnLoad(object sender, RoutedEventArgs e)
        {
            WpfUtil.SetWindowPosition(this, _mousePosition); 
        }

        IUXVisualiser visualiser;

        Boolean showDialog = true;

        public NewUXItem(IUXVisualiser _visualiser, IVertex _baseEdge, bool isSet, Point mousePos)
        {
            visualiser = _visualiser;

            InitializeComponent();

          //  this.Owner = m0Main.Instance;

            _mousePosition = mousePos;

            baseedge = _baseEdge;

            IsSet = isSet;

            if (IsSet)
                Remember.Content = "Remember choice, for current set";
            else
                Remember.Content = "Remember choice, for current session";

            BaseEdge = baseedge;

            //Owner = m0Main.Instance;

            this.Loaded += new RoutedEventHandler(OnLoad);

            ItemName.Content = baseedge.Get(false, "To:").Value;

            if (!CheckIfThereIsChoiceRemembered())
            {
                BaseEdgeSet();

                if (showDialog)
                    ShowDialog();
            }
        }    

        protected void NameControlsShow(){
            if (!is_EmptyValueInstance)
            {
                NameLabel.Visibility = Visibility.Visible;
                NameTextBox.Visibility = Visibility.Visible;
                NameTextBox.Focus();
            }
        }

        protected void NameControlsHide()
        {
            NameLabel.Visibility = Visibility.Hidden;
            NameTextBox.Visibility = Visibility.Hidden;
        }

        private bool testVertex(IVertex toTestEdge, string query)
        {
            if (query == null)
                return false;

            IVertex temp = MinusZero.Instance.CreateTempVertex();

            IEdge e = temp.AddEdge(toTestEdge.Get(false, "Meta:"), toTestEdge.Get(false, "To:"));

            IVertex res = temp.GetAll(false, query);

            temp.DeleteEdge(e);

            if (res.Count() > 0)
                return true;
            else
                return false;
        }

        IVertex ItemsList; 

        protected void UpdateItemList()
        {
            if (ItemsList != null)
                ItemsList.RemoveExternalReference();            

           ItemsList = m0.MinusZero.Instance.CreateTempVertex();

            if (visualiser.NewItemUXTemplate == null)
                return;

            if (InstanceRadio.IsChecked == true)
           {
                IVertex Instance = visualiser.NewItemUXTemplate.Vertex.GetAll(false, @"{InstanceCreation:Instance}");
             
               foreach(IEdge d in Instance)
                    //if (BaseEdge.Get(false, "To:").Get(false, (string)GraphUtil.GetValue(d.To.Get(false, "MetaVertexTestQuery:"))) != null)
                    if (testVertex(BaseEdge,(string)GraphUtil.GetValue(d.To.Get(false, "MetaVertexTestQuery:"))))
                         ItemsList.AddEdge(null, d.To);

                IVertex InstanceAndDirect = visualiser.NewItemUXTemplate.Vertex.GetAll(false, @"{InstanceCreation:InstanceAndDirect}");

                foreach (IEdge d in InstanceAndDirect) 
                     //if (BaseEdge.Get(false, "To:").Get(false, (string)GraphUtil.GetValue(d.To.Get(false, "MetaVertexTestQuery:"))) != null)
                     if (testVertex(BaseEdge,(string)GraphUtil.GetValue(d.To.Get(false, "MetaVertexTestQuery:"))))
                         ItemsList.AddEdge(null, d.To);        
           }
           else
            {               
               IVertex InstanceAndDirect = visualiser.NewItemUXTemplate.Vertex.GetAll(false, @"{InstanceCreation:InstanceAndDirect}");

               foreach (IEdge d in InstanceAndDirect)
               {
                   //if (BaseEdge.Get(false, "To:").Get(false, (string)GraphUtil.GetValue(d.To.Get(false, "DirectVertexTestQuery:"))) != null)
                   if (testVertex(BaseEdge, (string)GraphUtil.GetValue(d.To.Get(false, "DirectVertexTestQuery:"))))
                       ItemsList.AddEdge(null, d.To);
                   else
                       if ((string)GraphUtil.GetValue(d.To.Get(false, "DirectVertexTestQuery:")) == "")
                           ItemsList.AddEdge(null, d.To);
               }

               IVertex Direct = visualiser.NewItemUXTemplate.Vertex.GetAll(false, @"{InstanceCreation:Direct}");

               foreach (IEdge d in Direct)
               {
                   //if (BaseEdge.Get(false, "To:").Get(false, (string)GraphUtil.GetValue(d.To.Get(false, "DirectVertexTestQuery:"))) != null)
                   if (testVertex(BaseEdge, (string)GraphUtil.GetValue(d.To.Get(false, "DirectVertexTestQuery:"))))
                       ItemsList.AddEdge(null, d.To);
                   else
                       if ((string)GraphUtil.GetValue(d.To.Get(false, "DirectVertexTestQuery:")) == "")
                           ItemsList.AddEdge(null, d.To);
               }
             
           }

            ItemsList.AddExternalReference();
            List.ItemsSource = ItemsList;

            if (ItemsList.Count() > 0)
            {
                ListLabel.Visibility = Visibility.Visible;
                List.Visibility = Visibility.Visible;

                CreateButton.IsEnabled = true;

                if (InstanceRadio.IsChecked==true)
                    NameControlsShow();
                else
                    NameControlsHide();               
            }
            else
            {
                ListLabel.Visibility = Visibility.Hidden;
                List.Visibility = Visibility.Hidden;

                CreateButton.IsEnabled = false;

                NameControlsHide();
            }

            if (ItemsList.Count() > 0)
                this.List.SelectedIndex = 0;
        }

        private bool CheckIfThereIsChoiceRemembered()
        {
            IVertex question = GetRememberedQuestion();

            IVertex answer = User.Process.UX.NonAtomProcess.GetUserChoice(question);

            if (answer != null)
            {
                UXTemplate = (UXTemplate)TypedEdge.GetFromDictionary(answer);

                return true;
            }

            return false;
        }

        private IVertex GetRememberedQuestion()
        {
            IVertex question = MinusZero.Instance.CreateTempVertex();

            if (InstanceRadio.IsChecked == false)
                question.Value= "create diagram item for " + baseedge.Get(false, "Meta:").Value;
            else
                question.Value = "create diagram item for " + ItemName.Content;

            return question;
        }

        private void DirectInstanceRadio_Click(object sender, RoutedEventArgs e)
        {
            UpdateItemList();
        }

        public bool InstanceOfMeta;
        public string InstanceValue;
        public UXTemplate UXTemplate;

        private IVertex _BaseEdge;
        public IVertex BaseEdge{
            set{
                _BaseEdge = value;
             }
            get { return _BaseEdge; }
        }

        bool is_InstanceCreationPriority = false;
        bool is_EmptyValueInstance = false;

        private void BaseEdgeSet()
        {            
            if (BaseEdge.Get(false, @"To:\$InstanceCreationPriority:") != null)
            {
                InstanceRadio.IsChecked = true;
                is_InstanceCreationPriority = true;
            }

            if (BaseEdge.Get(false, @"To:\$EmptyValueInstance:") != null)
            {
                is_EmptyValueInstance = true;
            }

            UpdateItemList();

            if (is_InstanceCreationPriority)
                this.List.SelectedIndex = 0;

            if ((bool)InstanceRadio.IsChecked && ItemsList.Count() == 1 && is_EmptyValueInstance)
            {
                InstanceOfMeta = true;
                this.List.SelectedIndex = 0;
                UXTemplate = (UXTemplate)TypedEdge.Get((IEdge)List.SelectedValue, typeof(UXTemplate));
                showDialog = false;
            }

            if ((bool)DirectRadio.IsChecked && (ItemsList.Count() == 1))
            {                
                InstanceRadio.IsChecked = true;

                UpdateItemList();

                if (ItemsList.Count() == 0)
                {
                    RadioLabel_MouseDown_Instance(null, null);
                }
                //else
                  //  this.List.SelectedIndex = 0;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem != null)
            {
                UXTemplate = (UXTemplate)TypedEdge.Get((IEdge)List.SelectedValue, typeof(UXTemplate));

                if (InstanceRadio.IsChecked == true)
                {
                    InstanceOfMeta = true;
                    InstanceValue = NameTextBox.Text;
                }
               
                if (Remember.IsChecked == true) // remember choice
                { 
                    IVertex question = GetRememberedQuestion();
                  
                    m0.User.Process.UX.NonAtomProcess.AddUserChoice(question, UXTemplate.Vertex, !IsSet);
                }

                this.Close();
            }
        }

        private void RadioLabel_MouseDown_Instance(object sender, MouseButtonEventArgs e)
        {
            InstanceRadio.IsChecked = true;

            UpdateItemList();
        }

        private void RadioLabel_MouseDown_Direct(object sender, MouseButtonEventArgs e)
        {
            DirectRadio.IsChecked = true;

            UpdateItemList();
        }
    }
}
