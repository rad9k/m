using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Visualisers;
using m0.Util;
using m0.ZeroTypes;
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

namespace m0.UIWpf.Dialog
{    
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class ExecuteDialog : UserControl
    {
        IVertex baseVertex;

        IEdge inputStackEdge;
        IEdge outputStackEdge;

        enum StateEnum { NotStarted, Executing, AfterExecution};

        StateEnum State;

        public override string ToString()
        {
            return baseVertex + " execute";
        }

        void SetState(StateEnum toBeState)
        {
            State = toBeState;

            switch (State)
            {
                case StateEnum.NotStarted:
                    this.ExecuteButton.IsEnabled = true;
                    this.InputStackEdgeControl.IsEnabled = true;
                    this.InputStackContentControl.IsEnabled = true;
                    this.OutputStackEdgeControl.IsEnabled = false;
                    this.OutputStackContentControl.IsEnabled = false;

                    CreateInputStack();
                    
                    Edge.AddEdgeEdges(InputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:"), inputStackEdge);
                    Edge.ReplaceEdgeEdges(InputStackContentControl.Vertex.Get(false, "BaseEdge:"), inputStackEdge);

                    break;

                case StateEnum.Executing:
                    this.ExecuteButton.IsEnabled = false;
                    this.InputStackEdgeControl.IsEnabled = false;
                    this.InputStackContentControl.IsEnabled = false;
                    this.OutputStackEdgeControl.IsEnabled = false;
                    this.OutputStackContentControl.IsEnabled = false;
                    break;

                case StateEnum.AfterExecution:
                    this.ExecuteButton.IsEnabled = true;
                    this.InputStackEdgeControl.IsEnabled = true;
                    this.InputStackContentControl.IsEnabled = true;
                    this.OutputStackEdgeControl.IsEnabled = true;
                    this.OutputStackContentControl.IsEnabled = true;
                    
                    Edge.AddEdgeEdges(OutputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:"), outputStackEdge);
                    Edge.ReplaceEdgeEdges(OutputStackContentControl.Vertex.Get(false, "BaseEdge:"), outputStackEdge);

                    break;
            }
        }

        void CreateInputStack()
        {
            inputStackEdge = m0.MinusZero.Instance.CreateTempEdge();
        }

        void updateStackEdgeFromInputStackEdgeControl()
        {
            inputStackEdge = new EasyEdge(InputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:\From:"),
                                          InputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:\Meta:"),
                                          InputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:\To:"));

            Edge.ReplaceEdgeEdges(InputStackContentControl.Vertex.Get(false, "BaseEdge:"), inputStackEdge);
        }

        public ExecuteDialog(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            InitializeComponent();

            SetState(StateEnum.NotStarted);

            PlatformClass.RegisterVertexChangeListeners(InputStackEdgeControl.Vertex, new VertexChange(inputStackEdgeControl_VertexChange), new string[] { "BaseEdge" });
        }

        protected void inputStackEdgeControl_VertexChange(object sender, VertexChangeEventArgs e)
        {
            //if ((sender == InputStackEdgeControl.Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))                         

            if ((sender == InputStackEdgeControl.Vertex.Get(false, "BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))
                || (sender == InputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:") && e.Type == VertexChangeType.EdgeAdded))
                updateStackEdgeFromInputStackEdgeControl();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            SetState(StateEnum.Executing);

            outputStackEdge = m0.MinusZero.Instance.CreateTempEdge();


            outputStackEdge.To.AddVertex(null, "TEST");

            SetState(StateEnum.AfterExecution);
        }
    }
}
