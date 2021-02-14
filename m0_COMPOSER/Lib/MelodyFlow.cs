using m0;
using m0.Foundation;
using m0.Graph;
using m0.Util;
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
                    ql.Add(new MelodyFlowQuant(MelodyFlow, e.To));

                return ql;
            }
        }               
    }

    public class MelodyFlowQuant
    {        
        public IVertex QuantVertex;        

        public MelodyFlow MelodyFlow;

        static IVertex r = MinusZero.Instance.Root;

        static IVertex quantMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowStep\Quant");
        static IVertex octaveMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\Octave");
        static IVertex noteMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\Note");
        static IVertex velocityMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\Velocity");
        static IVertex quantTypeMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuant\QuantType");

        static IVertex noteEnumValue = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuantType\Note");
        static IVertex chordIndexEnumValue = r.Get(false, @"System\Lib\Music\Generator\MelodyFlowQuantType\ChordIndex");


        public int Octave
        {
            get
            {
                return GraphUtil.GetIntegerValueOr0(QuantVertex.Get(false, "Octave:"));
            }
            set
            {                
                GraphUtil.SetVertexValue(QuantVertex, octaveMeta, value);
            }
        }

        public int Note
        {
            get
            {
                return GraphUtil.GetIntegerValueOr0(QuantVertex.Get(false, "Note:"));
            }
            set
            {
                GraphUtil.SetVertexValue(QuantVertex, noteMeta, value);
            }
        }

        public int Velocity
        {
            get
            {
                return GraphUtil.GetIntegerValueOr0(QuantVertex.Get(false, "Velocity:"));
            }
            set
            {
                GraphUtil.SetVertexValue(QuantVertex, velocityMeta, value);
            }
        }

        public MelodyFlowQuantTypeEnum QuantType
        {
            get
            {
                IVertex quantTypeVertex = QuantVertex.Get(false, "QuantType:");

                if (quantTypeVertex != null && GeneralUtil.CompareStrings(quantTypeVertex.Value, "ChordIndex"))
                    return MelodyFlowQuantTypeEnum.ChordIndex;

                return MelodyFlowQuantTypeEnum.Note;
            }
            set
            {
                if (value == MelodyFlowQuantTypeEnum.Note)
                    GraphUtil.CreateOrReplaceEdge(QuantVertex, quantTypeMeta, noteEnumValue);

                if (value == MelodyFlowQuantTypeEnum.ChordIndex)
                    GraphUtil.CreateOrReplaceEdge(QuantVertex, quantTypeMeta, chordIndexEnumValue);
            }
        }

        public MelodyFlowQuant(MelodyFlow _MelodyFlow, IVertex _QuantVertex)
        {
            MelodyFlow = _MelodyFlow;
            QuantVertex = _QuantVertex;            
        }

        public MelodyFlowQuant(MelodyFlow _MelodyFlow)
        {
            MelodyFlow = _MelodyFlow;                        
        }

        IVertex GetParentStepVertex()
        {
            return GraphUtil.GetQueryInFirst(QuantVertex, "Step", null);
        }

        public void Remove()
        {
            IVertex stepVertex = GetParentStepVertex();

            IEdge quantEdge = GraphUtil.FindEdge(stepVertex, quantMeta, QuantVertex);

            stepVertex.DeleteEdge(quantEdge);
        }

        public void MoveToStep(int stepPosition)
        {
            Remove();

            IVertex toStepVertex = MelodyFlow.GetStep(stepPosition).StepVertex;

            toStepVertex.AddEdge(quantMeta, QuantVertex);            
        }               
    }

    public class MelodyFlow
    {
        IVertex baseVertex;

        public bool IsDrum;

        public MelodyFlow(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            if (GraphUtil.GetBooleanValueOrFalse(baseVertex.Get(false, "IsDrum:")))
                IsDrum = true;
        }

        public int GetNumberOfSteps()
        {
            return baseVertex.GetAll(false, "Step:").Count();
        }

        public MelodyFlowStep GetStep(int stepPosition)
        {
            return new MelodyFlowStep(this, baseVertex.Get(false, "Step:<<"+stepPosition+">>"));
        }
        
        public MelodyFlowStep InsertStepAt(int stepPosition)
        {
            return null;
        }
    }
}
