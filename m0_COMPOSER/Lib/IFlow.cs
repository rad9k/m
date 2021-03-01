using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public enum FlowQuantTypeEnum { Note, ChordIndex, Pitch }

    public interface IFlowStep
    {
        IVertex StepVertex { get; set; }

        List<IFlowQuant> Quants { get; }
    }

    public interface IFlowQuant
    {
        IVertex QuantVertex { get; set; }
        IEdge QuantEdge { get; set; }
       
        int Octave { get; set; }
        
        int Note { get; set; }
        
        int Velocity { get; set; }
        
        FlowQuantTypeEnum QuantType { get; set; }        

        void Remove();

        IEdge PutOrMoveToStep(int stepPosition);
    }

    public interface IFlow
    {
        String IsQuantMeta { get; }

        String StepToQuantMeta { get;  }

        int GetNumberOfSteps();

        void AddStepAtEnd();

        IFlowStep GetStep(int stepPosition);

        void InsertStepAt(int stepPosition);

        void RemoveStep(int stepPosition);

        IFlowQuant GetQuantFromVertex(IVertex quantVertex);

        IFlowQuant GetQuantAndStepFromQuantVertex(IVertex quantVertex, out int step, out IFlowStep stepObject);

        bool GetNumberOfStepsAndCleanUp(out int newNumberOfSteps);

        IFlowQuant CreateQuant();
    }
}
