using m0.Foundation;
using m0.Graph;
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

namespace m0.UIWpf.Visualisers.Diagram
{
    /// <summary>
    /// Interaction logic for NewDiagramItem.xaml
    /// </summary>
    public partial class NewDiagramItem : Window
    {
        bool IsSet;

        IVertex baseedge;

        Point _mousePosition;

        void OnLoad(object sender, RoutedEventArgs e)
        {
            UIWpf.SetWindowPosition(this, _mousePosition);
        }

        public NewDiagramItem(IVertex _baseedge, bool isSet, Point mousePos)
        {
            InitializeComponent();

            _mousePosition = mousePos;

            baseedge = _baseedge;

            IsSet = isSet;

            if (IsSet)
                Remember.Content = "Remember choice, for current set";
            else
                Remember.Content = "Remember choice, for current session";

            BaseEdge = baseedge;

            //Owner = m0Main.Instance;

            this.Loaded += new RoutedEventHandler(OnLoad);

            ItemName.Content = baseedge.Get("To:").Value;

            if (!CheckIfThereIChoiceRemembered())
            {
                BaseEdgeSet();

                ShowDialog();
            }
        }    

        protected void NameControlsShow(){
            NameLabel.Visibility = Visibility.Visible;
            NameTextBox.Visibility = Visibility.Visible;
        }

        protected void NameControlsHide()
        {
            NameLabel.Visibility = Visibility.Hidden;
            NameTextBox.Visibility = Visibility.Hidden;
        }

        private bool testVertex(IVertex toTest, string query)
        {
            IVertex temp = MinusZero.Instance.CreateTempVertex();

            IEdge e=temp.AddEdge(null, toTest);

            IVertex res = temp.GetAll(query);

            temp.DeleteEdge(e);

            if (res.Count() > 0)
                return true;
            else
                return false;
        }

        IVertex ItemsList; 

        protected void UpdateItemList()
        {
          ItemsList = m0.MinusZero.Instance.CreateTempVertex();     

           if (InstanceRadio.IsChecked == true)
           {
                IVertex Instance = m0.MinusZero.Instance.Root.GetAll(@"System\Data\Visualiser\Diagram\{InstanceCreation:Instance}");
             
               foreach(IEdge d in Instance)
                    //if (BaseEdge.Get("To:").Get((string)GraphUtil.GetValue(d.To.Get("MetaVertexTestQuery:"))) != null)
                    if(testVertex(BaseEdge.Get("To:"),(string)GraphUtil.GetValue(d.To.Get("MetaVertexTestQuery:"))))
                         ItemsList.AddEdge(null, d.To);

                IVertex InstanceAndDirect = m0.MinusZero.Instance.Root.GetAll(@"System\Data\Visualiser\Diagram\{InstanceCreation:InstanceAndDirect}");

                foreach (IEdge d in InstanceAndDirect) 
                     //if (BaseEdge.Get("To:").Get((string)GraphUtil.GetValue(d.To.Get("MetaVertexTestQuery:"))) != null)
                     if(testVertex(BaseEdge.Get("To:"),(string)GraphUtil.GetValue(d.To.Get("MetaVertexTestQuery:"))))
                         ItemsList.AddEdge(null, d.To);        
           }
           else
           {               
               IVertex InstanceAndDirect = m0.MinusZero.Instance.Root.GetAll(@"System\Data\Visualiser\Diagram\{InstanceCreation:InstanceAndDirect}");

               foreach (IEdge d in InstanceAndDirect)
               {
                   //if (BaseEdge.Get("To:").Get((string)GraphUtil.GetValue(d.To.Get("DirectVertexTestQuery:"))) != null)
                   if (testVertex(BaseEdge.Get("To:"), (string)GraphUtil.GetValue(d.To.Get("DirectVertexTestQuery:"))))
                       ItemsList.AddEdge(null, d.To);
                   else
                       if ((string)GraphUtil.GetValue(d.To.Get("DirectVertexTestQuery:")) == "")
                           ItemsList.AddEdge(null, d.To);
               }

               IVertex Direct = m0.MinusZero.Instance.Root.GetAll(@"System\Data\Visualiser\Diagram\{InstanceCreation:Direct}");

               foreach (IEdge d in Direct)
               {
                   //if (BaseEdge.Get("To:").Get((string)GraphUtil.GetValue(d.To.Get("DirectVertexTestQuery:"))) != null)
                   if (testVertex(BaseEdge.Get("To:"), (string)GraphUtil.GetValue(d.To.Get("DirectVertexTestQuery:"))))
                       ItemsList.AddEdge(null, d.To);
                   else
                       if ((string)GraphUtil.GetValue(d.To.Get("DirectVertexTestQuery:")) == "")
                           ItemsList.AddEdge(null, d.To);
               }
             
           }

            List.ItemsSource = ItemsList;

            if (ItemsList.Count() > 0)
            {
                ListLabel.Visibility = Visibility.Visible;
                List.Visibility = Visibility.Visible;

                CreateButton.IsEnabled = true;

                if(InstanceRadio.IsChecked==true)
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
        }

        private bool CheckIfThereIChoiceRemembered()
        {
            IVertex question = GetRememberedQuestion();

            IVertex answer=Process.UI.NonAtomProcess.GetUserChoice(question);

            if (answer != null)
            {
                DiagramItemDefinition = answer;

                return true;
            }

            return false;
        }

        private IVertex GetRememberedQuestion()
        {
            IVertex question = MinusZero.Instance.CreateTempVertex();

            if (InstanceRadio.IsChecked == false)
                question.Value= "create diagram item for " + baseedge.Get("Meta:").Value;
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
        public IVertex DiagramItemDefinition;

        private IVertex _BaseEdge;
        public IVertex BaseEdge{
            set{
                _BaseEdge = value;
             }
            get { return _BaseEdge; }
        }

        private void BaseEdgeSet()
        {
            UpdateItemList();

            if ((bool)DirectRadio.IsChecked && (ItemsList.Count() == 1))
            {
                InstanceRadio.IsChecked = true;

                UpdateItemList();

                if (ItemsList.Count() == 0)
                {
                    RadioLabel_MouseDown_Instance(null, null);
                }
                else
                    this.List.SelectedIndex = 0;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem != null)
            {
                DiagramItemDefinition = ((IEdge)List.SelectedValue).To;

                if (InstanceRadio.IsChecked == true)
                {
                    InstanceOfMeta = true;
                    InstanceValue = NameTextBox.Text;
                }
               
                if(Remember.IsChecked==true) // remember choice
                { 
                    IVertex question = GetRememberedQuestion();
                  
                    m0.Process.UI.NonAtomProcess.AddUserChoice(question, DiagramItemDefinition, !IsSet);
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
