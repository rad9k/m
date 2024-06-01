using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LovFlov.ZeroTypes
{
    public class Value : TypedEdge
    {
        static IVertex Definition_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Value\Definition");
        static IVertex Step_meta = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Value\Step");
        static IVertex Step_type = MinusZero.Instance.root.Get(false, @"LovFlov\Meta\Step");

        public Value(IEdge edge) : base(edge) { }

        public ValueDefinition Definition
        {
            get
            {
                IEdge e = GraphUtil.GetQueryOutFirstEdge(Vertex, "Definition", null);

                if (e == null)
                    return null;

                if (InstructionHelpers.CheckIfIsOrInherits_WRONG(e.To, "ParameterDefinition"))
                {
                    return (ValueDefinition)TypedEdge.Get(e, typeof(ParameterDefinition));
                }
                else
                {
                    if (InstructionHelpers.CheckIfIsOrInherits_WRONG(e.To, "AddressDefinition"))
                    {
                        return (ValueDefinition)TypedEdge.Get(e, typeof(AddressDefinition));
                    }
                }

                return (ValueDefinition)TypedEdge.Get(e, typeof(ValueDefinition));
            }

            set
            {
                GraphUtil.CreateOrReplaceEdge(this.Vertex, Definition_meta, value.Vertex);
            }
        }

        public IList<Step> Steps
        {
            get
            {
                IList<IEdge> list = GraphUtil.GetQueryOut(Vertex, "Step", null);

                IList<Step> ret = new List<Step>();

                foreach (IEdge e in list)    
                        ret.Add((Step)TypedEdge.Get(e, typeof(Step)));
                 
                return ret;
            }
        }

        public Step AddStep()
        {
            IEdge newEdge = VertexOperations.AddInstanceAndReturnEdge(Vertex, Step_type, Step_meta);

            return new Step(newEdge);
        }
    }
}
