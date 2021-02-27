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
    public enum MelodyFlowQuantTypeEnum { Note, ChordIndex }

    public class MelodyFlowStep
    {
        public IVertex StepVertex;

        MelodyFlow MelodyFlow;

        public MelodyFlowStep(MelodyFlow _MelodyFlow, IVertex _StepVertex)
        {
            MelodyFlow = _MelodyFlow;
            StepVertex = _StepVertex;
        }

        public List<MelodyFlowQuant> Quants
        {
            get
            {
                List<MelodyFlowQuant> ql = new List<MelodyFlowQuant>();

                foreach (IEdge e in StepVertex.GetAll(false, "Quant:"))
                    ql.Add(new MelodyFlowQuant(MelodyFlow, e));

                return ql;
            }
        }               
    }

    public class MelodyFlowQuant
    {        
        public IVertex QuantVertex;
        public IEdge QuantEdge;

        bool isAttached = false;

        public MelodyFlow MelodyFlow;

        static IVertex r = MinusZero.Instance.Root;

        static IVertex melodyFlowQuantMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant");
        static IVertex quantMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowStep\Quant");
        static IVertex octaveMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\Octave");
        static IVertex noteMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\Note");
        static IVertex velocityMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\Velocity");
        static IVertex quantTypeMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\QuantType");

        static IVertex noteEnumValue = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuantTypeEnum\Note");
        static IVertex chordIndexEnumValue = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuantTypeEnum\ChordIndex");


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

        int velocity;
        public int Velocity
        {
            get
            {
                if (QuantVertex == null)
                    return velocity;
                else
                    return GraphUtil.GetIntegerValueOr0(QuantVertex.Get(false, "Velocity:"));
            }
            set
            {
                if (QuantVertex == null)
                    velocity = value;
                else
                    GraphUtil.SetVertexValue(QuantVertex, velocityMeta, value);
            }
        }

        MelodyFlowQuantTypeEnum quantType;
        public MelodyFlowQuantTypeEnum QuantType
        {
            get
            {
                if (QuantVertex == null)
                    return quantType;
                else
                {
                    IVertex quantTypeVertex = QuantVertex.Get(false, "QuantType:");

                    if (quantTypeVertex != null && GeneralUtil.CompareStrings(quantTypeVertex.Value, "ChordIndex"))
                        return MelodyFlowQuantTypeEnum.ChordIndex;

                    return MelodyFlowQuantTypeEnum.Note;
                }
            }
            set
            {
                if (QuantVertex == null)
                    quantType = value;
                else
                {
                    if (value == MelodyFlowQuantTypeEnum.Note)
                        GraphUtil.CreateOrReplaceEdge(QuantVertex, quantTypeMeta, noteEnumValue);

                    if (value == MelodyFlowQuantTypeEnum.ChordIndex)
                        GraphUtil.CreateOrReplaceEdge(QuantVertex, quantTypeMeta, chordIndexEnumValue);
                }
            }
        }        

        public MelodyFlowQuant(MelodyFlow _MelodyFlow, IEdge _QuantEdge)
        {
            MelodyFlow = _MelodyFlow;
            QuantEdge = _QuantEdge;
            QuantVertex = _QuantEdge.To;

            if(GetParentStepVertex() != null)
                isAttached = true;
        }

        public MelodyFlowQuant(MelodyFlow _MelodyFlow)
        {
            MelodyFlow = _MelodyFlow;

            isAttached = false;
        }

        IVertex GetParentStepVertex()
        {
            return GraphUtil.GetQueryInFirst(QuantVertex, "Step", null);
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

            IVertex toStepVertex = MelodyFlow.GetStep(stepPosition).StepVertex;

            bool needToInsert = false;

            if (!MelodyFlow.IsDrum && toStepVertex.Get(false, @"\QuantType:" + QuantType.ToString()) != null)
                needToInsert = true;

            if (needToInsert)
            {
                MelodyFlow.InsertStepAt(stepPosition);
                toStepVertex = MelodyFlow.GetStep(stepPosition).StepVertex;
            }

            IEdge newEdge = null;

            if (QuantVertex == null)
            {
                QuantEdge = VertexOperations.AddInstanceAndReturnEdge(toStepVertex, melodyFlowQuantMeta, quantMeta);

                QuantVertex = QuantEdge.To;                 

                Note = note;
                Octave = octave;
                Velocity = velocity;
                QuantType = quantType;
            }
            else
                newEdge = toStepVertex.AddEdge(quantMeta, QuantVertex);

            isAttached = true;

            return newEdge;
        }               
    }

    public class MelodyFlow
    {
        IVertex baseVertex;

        public bool IsDrum;        

        static IVertex r = MinusZero.Instance.Root;

        static IVertex melodyFlowStepMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowStep");
        static IVertex stepMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlow\Step");

        public MelodyFlow(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            if (GraphUtil.GetBooleanValueOrFalse(baseVertex.Get(false, "IsDrum:")))
                IsDrum = true;

            if (baseVertex.GetAll(false, "Step:").Count() == 0)
                VertexOperations.AddInstance(baseVertex, melodyFlowStepMeta, stepMeta);
        }

        public int GetNumberOfSteps()
        {
            return baseVertex.GetAll(false, "Step:").Count();
        }

        public void AddStepAtEnd()
        {
            VertexOperations.AddInstance(baseVertex, melodyFlowStepMeta, stepMeta);
        }

        public MelodyFlowStep GetStep(int stepPosition)
        {
            int actualNumberOfSteps = GetNumberOfSteps();

            int stepPosition_zeroScript = stepPosition + 1;

            if(stepPosition_zeroScript > actualNumberOfSteps)
                for (int x = actualNumberOfSteps; x < stepPosition_zeroScript; x++)
                    AddStepAtEnd();

            return new MelodyFlowStep(this, baseVertex.Get(false, "Step:<<\""+stepPosition_zeroScript+"\">>"));
        }                       

        public void InsertStepAt(int stepPosition)
        {
            List<IEdge> edges = new List<IEdge>();

            foreach (IEdge e in baseVertex.GetAll(false, "Step:"))
                edges.Add(e);

            foreach (IEdge e in edges)
                baseVertex.DeleteEdge(e);

            int cnt = 0;
            foreach (IEdge e in edges)
            {                
                if (cnt == stepPosition)
                    VertexOperations.AddInstance(baseVertex, melodyFlowStepMeta, stepMeta);

                baseVertex.AddEdge(stepMeta, e.To);

                cnt++;
            }                
        }

        public void RemoveStep(int stepPosition)
        {
            IList<IEdge> steps = GraphUtil.GetQueryOut(baseVertex, "Step", null);

            IEdge toDeleteEdge = steps[stepPosition];

            baseVertex.DeleteEdge(toDeleteEdge);
        }

        public MelodyFlowQuant GetQuantAndStepFromQuantVertex(IVertex quantVertex, out int step, out MelodyFlowStep stepObject)
        {
            step = -1;
            stepObject = null;

            int cntStep = 0;
            foreach (IEdge stepEdge in baseVertex.GetAll(false, "Step:")) {
                
                int cntQuant = 0;

                foreach (IEdge quantEdge in stepEdge.To.GetAll(false, "Quant:")) {
                    if (quantEdge.To == quantVertex)
                    {
                        stepObject = new MelodyFlowStep(this, stepEdge.To);
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
            int numberOfSteps = GetNumberOfSteps(); // check if last step is empty, if not, add empty step

            IVertex stepVertex = GetStep(numberOfSteps - 1).StepVertex;
            
            if (stepVertex.Get(false, @"Quant:") != null)
            {
                AddStepAtEnd();
                newNumberOfSteps = numberOfSteps + 1;

                return true;
            }

            newNumberOfSteps = numberOfSteps;

            return false;
        }
    }
}
