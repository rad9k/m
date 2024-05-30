using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Visualisers;
using m0.User.Process.UX;
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
        EdgeVisualiser InputStackEdgeControl;
        EdgeVisualiser OutputStackEdgeControl;

        TreeVisualiser InputStackContentControl;
        TreeVisualiser OutputStackContentControl;

        IVertex baseVertex;

        IEdge inputStackEdge;
        IEdge outputStackEdge;

        enum StateEnum { NotStarted, Executing, AfterExecution};

        StateEnum State;

        public override string ToString()
        {
            return baseVertex + " execute";
        }

        IVertex localInputStackEdgeVertex = null;

        void SetState(StateEnum toBeState)
        {
            State = toBeState;

            switch (State)
            {
                case StateEnum.NotStarted:

                    CreateInputStack();

                    IVertex inputStackEdgeVertex = EdgeHelper.CreateTempEdgeVertex(
                                                   null,
                                                   null,
                        EdgeHelper.CreateTempEdgeVertex(inputStackEdge)); // ??

                    localInputStackEdgeVertex = inputStackEdgeVertex;

                    localInputStackEdgeVertex.AddExternalReference();

                    ////////////////////////////////////////
                    Interaction.BeginInteractionWithGraph();
                    ////////////////////////////////////////

                    InputStackEdgeControl = new EdgeVisualiser(inputStackEdgeVertex, null, false);
                    InputStackEdgeControl_Border.Child = InputStackEdgeControl;

                    OutputStackEdgeControl = new EdgeVisualiser(null, null, false);
                    OutputStackEdgeControl_Border.Child = OutputStackEdgeControl;

                    InputStackContentControl = new TreeVisualiser(null, null, false);
                    InputStackContentControl_Border.Child = InputStackContentControl;

                    OutputStackContentControl = new TreeVisualiser(null, null, false);
                    OutputStackContentControl_Border.Child = OutputStackContentControl;

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////


                    this.ExecuteButton.IsEnabled = true;
                    this.InputStackEdgeControl.IsEnabled = true;
                    this.InputStackContentControl.IsEnabled = true;
                    this.OutputStackEdgeControl.IsEnabled = false;
                    this.OutputStackContentControl.IsEnabled = false;

                    //CreateInputStack();
                                     

                    //IVertex InputStackEdgeControlBaseEdge = InputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:");
                    //GraphUtil.RemoveAllEdges(InputStackEdgeControlBaseEdge);
                    //Edge.AddEdgeVertexEdges(InputStackEdgeControlBaseEdge, inputStackEdge);
                    //Edge.ReplaceEdgeVertexEdges(InputStackContentControl.Vertex.Get(false, "BaseEdge:"), inputStackEdge); // ?

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
                    
                    EdgeHelper.AddOrReplaceEdgeVertexEdges(OutputStackEdgeControl.Vertex.Get(false, @"BaseEdge:"), outputStackEdge);

                    EdgeHelper.AddOrReplaceEdgeVertexEdges(OutputStackContentControl.Vertex.Get(false, @"BaseEdge:"), outputStackEdge);

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

            EdgeHelper.AddOrReplaceEdgeVertexEdges(InputStackContentControl.Vertex.Get(false, @"BaseEdge:"), inputStackEdge);
        }

        public ExecuteDialog(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            InitializeComponent();

            SetState(StateEnum.NotStarted);

            ExecutionFlowHelper.AddTriggerAndListener(InputStackEdgeControl.Vertex.Get(false, @"BaseEdge:\To:"), inputStackEdgeControl_VertexChange);

            this.Unloaded += ExecuteDialog_Unloaded;

           // PlatformClass.RegisterVertexChangeListeners(InputStackEdgeControl.Vertex, new VertexChange(inputStackEdgeControl_VertexChange), new string[] { "BaseEdge" });
        }

        private void ExecuteDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        protected virtual INoInEdgeInOutVertexVertex inputStackEdgeControl_VertexChange(IExecution exe)
        {
            updateStackEdgeFromInputStackEdgeControl();

            return exe.Stack;
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

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex outputStackVertex = m0.MinusZero.Instance.DefaultExecuter.Execute(inputStackEdge.To, baseVertex.Get(false, "To:"));

            outputStackEdge = GraphUtil.CreateArtificialEdge(null, outputStackVertex);

            SetState(StateEnum.AfterExecution);

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////            
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            InputStackContentControl.Dispose();
            InputStackEdgeControl.Dispose();

            OutputStackContentControl.Dispose();
            OutputStackEdgeControl.Dispose();
        }
        
        void Dispose()
        {
            localInputStackEdgeVertex.RemoveExternalReference();
        }
    }
}
