using m0;
using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{    
    public class ChordProgressionStep: IFlowStep
    {
        public IVertex StepVertex { get; set; }

        ChordProgressionFlow Flow;

        public ChordProgressionStep(ChordProgressionFlow _MelodyFlow, IVertex _StepVertex)
        {
            Flow = _MelodyFlow;
            StepVertex = _StepVertex;
        }

        public List<IFlowQuant> Quants
        {
            get
            {
                List<IFlowQuant> ql = new List<IFlowQuant>();

                foreach (IEdge e in StepVertex.GetAll(false, "Pitch:"))
                    ql.Add(new ChordProgressionQuant(Flow, e));

                return ql;
            }
        }               
    }

    public class ChordProgressionQuant : IFlowQuant
    {
        public IVertex QuantVertex { get; set; }
        public IEdge QuantEdge { get; set; }


        bool isAttached = false;

        public ChordProgressionFlow Flow;

        static IVertex r = MinusZero.Instance.Root;

        static IVertex melodyFlowQuantMeta = r.Get(false, @"System\Lib\Music\Pitch");
        static IVertex quantMeta = r.Get(false, @"System\Lib\Music\PitchSet\Pitch");
        static IVertex octaveMeta = r.Get(false, @"System\Lib\Music\Pitch\Octave");
        static IVertex noteMeta = r.Get(false, @"System\Lib\Music\Pitch\Note");        


        int octave;
        public int Octave
        {
            get
            {
                if (QuantVertex == null)
                    return octave;
                else
                    return GraphUtil.GetIntegerValueOr0(QuantVertex.Get(false, "Octave:"));
            }
            set
            {
                if (QuantVertex == null)
                    octave = value;
                else
                    GraphUtil.SetVertexValue(QuantVertex, octaveMeta, value);
            }
        }

        int note;
        public int Note
        {
            get
            {
                if (QuantVertex == null)
                    return note;
                else
                    return GraphUtil.GetIntegerValueOr0(QuantVertex.Get(false, "Note:"));
            }
            set
            {
                if (QuantVertex == null)
                    note = value;
                else
                    GraphUtil.SetVertexValue(QuantVertex, noteMeta, value);
            }
        }
       
        public int Velocity
        {
            get
            {
                return 0;
            }
            set { }
        }
        
        public FlowQuantTypeEnum QuantType
        {
            get
            {
                return FlowQuantTypeEnum.Pitch;             
            }
            set { }
        }        

        public ChordProgressionQuant(IFlow _MelodyFlow, IEdge _QuantEdge)
        {
            Flow = (ChordProgressionFlow)_MelodyFlow;
            QuantEdge = _QuantEdge;
            QuantVertex = _QuantEdge.To;

            if(GetParentStepVertex() != null)
                isAttached = true;
        }

        public ChordProgressionQuant(IFlow _MelodyFlow)
        {
            Flow = (ChordProgressionFlow)_MelodyFlow;

            isAttached = false;
        }

        IVertex GetParentStepVertex()
        {
            return GraphUtil.GetQueryInFirst(QuantVertex, "Pitch", null);
        }

        public void Remove()
        {
            if (isAttached)
            {
                IVertex stepVertex = GetParentStepVertex();

                IEdge quantEdge = GraphUtil.FindEdge(stepVertex, quantMeta, QuantVertex);

                stepVertex.DeleteEdge(quantEdge);

                isAttached = false;
            }
        }

        public IEdge PutOrMoveToStep(int stepPosition)
        {
            Remove();

            IVertex toStepVertex = Flow.GetStep(stepPosition).StepVertex;
            
            if (QuantVertex == null)
            {
                QuantEdge = VertexOperations.AddInstanceAndReturnEdge(toStepVertex, melodyFlowQuantMeta, quantMeta);

                QuantVertex = QuantEdge.To;                 

                Note = note;
                Octave = octave;
            }
            else
                QuantEdge = toStepVertex.AddEdge(quantMeta, QuantVertex);

            isAttached = true;

            return QuantEdge;
        }               
    }

    public class ChordProgressionFlow : IFlow
    {
        public String IsQuantMeta { get { return "$Is:Pitch"; } }

        public String StepToQuantMeta { get { return "Pitch"; } }

        IVertex baseVertex;

        public bool IsDrum;        

        static IVertex r = MinusZero.Instance.Root;

        static IVertex melodyFlowStepMeta = r.Get(false, @"System\Lib\Music\PitchSet");
        static IVertex stepMeta = r.Get(false, @"System\Lib\Music\Generator\ChordProgression\Chord");

        public ChordProgressionFlow(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;            

            if (baseVertex.GetAll(false, "Chord:").Count() == 0)
                VertexOperations.AddInstance(baseVertex, melodyFlowStepMeta, stepMeta);
        }

        public int GetNumberOfSteps()
        {
            return baseVertex.GetAll(false, "Chord:").Count();
        }

        public void AddStepAtEnd()
        {
            VertexOperations.AddInstance(baseVertex, melodyFlowStepMeta, stepMeta);
        }

        public IFlowStep GetStep(int stepPosition)
        {
            int actualNumberOfSteps = GetNumberOfSteps();

            int stepPosition_zeroScript = stepPosition + 1;

            if(stepPosition_zeroScript > actualNumberOfSteps)
                for (int x = actualNumberOfSteps; x < stepPosition_zeroScript; x++)
                    AddStepAtEnd();

            return new ChordProgressionStep(this, baseVertex.Get(false, "Chord:<<\""+stepPosition_zeroScript+"\">>"));
        }                       

        public void InsertStepAt(int stepPosition)
        {
            List<IEdge> edges = new List<IEdge>();

            foreach (IEdge e in baseVertex.GetAll(false, "Chord:"))
                edges.Add(e);

            int cnt = 0;
            foreach (IEdge e in edges)
            {                
                if (cnt == stepPosition)
                    VertexOperations.AddInstance(baseVertex, melodyFlowStepMeta, stepMeta);

                baseVertex.AddEdge(stepMeta, e.To);

                cnt++;
            }

            foreach (IEdge e in edges)
                baseVertex.DeleteEdge(e);
        }

        public void RemoveStep(int stepPosition)
        {
            IList<IEdge> steps = GraphUtil.GetQueryOut(baseVertex, "Chord", null);

            IEdge toDeleteEdge = steps[stepPosition];

            baseVertex.DeleteEdge(toDeleteEdge);
        }

        public IFlowQuant GetQuantFromVertex(IVertex quantVertex)
        {
            int step;
            IFlowStep stepObject;

            return GetQuantAndStepFromQuantVertex(quantVertex, out step, out stepObject);
        }

        public IFlowQuant GetQuantAndStepFromQuantVertex(IVertex quantVertex, out int step, out IFlowStep stepObject)
        {
            step = -1;
            stepObject = null;

            int cntStep = 0;
            foreach (IEdge stepEdge in baseVertex.GetAll(false, "Chord:")) {
                
                int cntQuant = 0;

                foreach (IEdge quantEdge in stepEdge.To.GetAll(false, "Pitch:")) {
                    if (quantEdge.To == quantVertex)
                    {
                        stepObject = new ChordProgressionStep(this, stepEdge.To);
                        step = cntStep;
                        return stepObject.Quants[cntQuant];
                    }
                    cntQuant++;
                }

                cntStep++;
            }

            return null;
        }        

        public bool GetNumberOfStepsAndCleanUp(out int newNumberOfSteps)
        {
            bool needToCleanUp = false;

            int numberOfSteps = 0;

            int stepsCount = baseVertex.GetAll(false, "Chord:").OutEdges.Count;

            foreach (IEdge e in baseVertex.GetAll(false, "Chord:"))
            {
                numberOfSteps++;

                if(numberOfSteps < stepsCount)
                    if (!GraphUtil.ExistQueryOut(e.To, "Pitch", null))
                        needToCleanUp = true;
            }            

            if (needToCleanUp)
            {
                numberOfSteps = 0;

                List<IEdge> edges = new List<IEdge>();

                foreach (IEdge e in baseVertex.GetAll(false, "Chord:"))
                    edges.Add(e);                

                foreach (IEdge e in edges)
                {
                    IVertex chordVertex = e.To;
                    
                    if(GraphUtil.ExistQueryOut(chordVertex, "Pitch", null))
                        baseVertex.AddEdge(stepMeta, chordVertex);

                    numberOfSteps++;
                }

                foreach (IEdge e in edges)
                    baseVertex.DeleteEdge(e);
            }
            //            

            IVertex stepVertex = GetStep(numberOfSteps - 1).StepVertex;
            
            if (stepVertex.Get(false, @"Pitch:") != null)
            {
                AddStepAtEnd();
                newNumberOfSteps = numberOfSteps + 1;

                return true;
            }

            newNumberOfSteps = numberOfSteps;

            return false;
        }

        public IFlowQuant CreateQuant()
        {
            return new ChordProgressionQuant(this);
        }
    }
}
