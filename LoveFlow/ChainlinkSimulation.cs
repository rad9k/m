using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Text;

using LovFlov.ZeroTypes;
using m0.ZeroTypes;
using m0.Graph;

namespace LovFlov
{
    public class ChainlinkSimulation
    {
        static IVertex r = m0.MinusZero.Instance.root;

        static FlovInstance ci;
        static Flov cf;

        static public void Run(int days)
        {
            IVertex lf = r.Get(false, "LovFlov");

            IEdge ci_edge = VertexOperations.AddInstanceAndReturnEdge(lf, r.Get(false, @"LovFlov\Meta\FlovInstance"));

            ci = (FlovInstance)TypedEdge.Get(ci_edge, typeof(FlovInstance));
            ci.Days = days;

            cf = (Flov)TypedEdge.Get(GraphUtil.GetQueryOutFirstEdge(lf, "Flov", "Chainlink"), typeof(Flov));
            ci.Definition = cf;

            AddParameterInstances();
            AddAddressesInstances();
        }

        static void AddParameterInstances()
        {
            AddParameterInstances_notDerived();
            AddParameterInstances_Derived();
        }

        static void AddParameterInstances_notDerived()
        {
            foreach (IEdge e in cf.Vertex.GetAll(false, "ParameterDefinition:{IsDerived:False}"))
            {
                ParameterDefinition pd = (ParameterDefinition)TypedEdge.Get(e, typeof(ParameterDefinition));

                Parameter p = ci.AddParameter();
                p.Definition = pd;

                p.Vertex.Value = pd.Name;

                AddStepsForParameter(p);
            }
        }

        static void AddStepsForParameter(Parameter p)
        {
            double val = p.Definition.MinValue;

            for(int d=1; d <= ci.Days; d++)
            {
                Step s = p.AddStep();

                s.Day = d;
                s.Value = val;

                val += (p.Definition.MaxValue - p.Definition.MinValue) / ci.Days;
            }
        }

        static void AddParameterInstances_Derived()
        {
            foreach (IEdge e in cf.Vertex.GetAll(false, "ParameterDefinition:{IsDerived:True}"))
            {
                ParameterDefinition pd = (ParameterDefinition)TypedEdge.Get(e, typeof(ParameterDefinition));

                Parameter p = ci.AddParameter();
                p.Definition = pd;

                p.Vertex.Value = pd.Name;
            }
        }

        static void AddAddressesInstances()
        {
            foreach(IEdge e in cf.Vertex.GetAll(false, "AddressDefinition:"))
            {
                AddressDefinition ad = (AddressDefinition)TypedEdge.Get(e, typeof(AddressDefinition));

                Address a = ci.AddAddress();
                a.Definition = ad;

                a.Vertex.Value = ad.Name;

            }
        }
    }
}
