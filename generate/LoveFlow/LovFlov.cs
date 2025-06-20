using m0.Foundation;
using m0.Graph;
using System;

namespace LovFlov
{
    public class LovFlov
    {
        static IVertex r = m0.MinusZero.Instance.root;
        static IVertex String = r.Get(false, @"System\Meta\ZeroTypes\String");
        static IVertex Integer = r.Get(false, @"System\Meta\ZeroTypes\Integer");
        static IVertex Boolean = r.Get(false, @"System\Meta\ZeroTypes\Boolean");
        static IVertex Float = r.Get(false, @"System\Meta\ZeroTypes\Float");
        static IVertex VertexType = r.Get(false, @"System\Meta\ZeroTypes\VertexType");
        static IVertex Color = r.Get(false, @"System\Meta\ZeroTypes\UX\Color");

        static void CreateLoweFlov()
        {
            IVertex lf = r.AddVertex(null, "LovFlov");

            IVertex meta = lf.AddVertex(null, "Meta");

            IVertex flovDefinitionClass = GraphUtil.AddClass(meta, "FlovDefinition");

            IVertex flovClass = GraphUtil.AddClass(meta, "Flov");

            // Step
            IVertex stepClass = GraphUtil.AddClass(meta, "Step");
            GraphUtil.AddAttribute(stepClass, "Day", Integer, 1, 1, 0);
            GraphUtil.AddAttribute(stepClass, "Value", Float, 1, 1, 0);

            // ValueDefinition
            IVertex valueDefinitionClass = GraphUtil.AddClass(meta, "ValueDefinition");
            GraphUtil.AddAttribute(valueDefinitionClass, "Name", String, 0, 1);
            GraphUtil.AddAttribute(valueDefinitionClass, "MinValue", Float, 0, 1);
            GraphUtil.AddAttribute(valueDefinitionClass, "MaxValue", Float, 0, 1);
            GraphUtil.AddAttribute(valueDefinitionClass, "Unit", String, 0, 1);

            // Value
            IVertex valueClass = GraphUtil.AddClass(meta, "Value");
            GraphUtil.AddAssociation(valueClass, "Definition", valueDefinitionClass, 1, 1);
            GraphUtil.AddAttribute(valueClass, "Step", stepClass, 0, -1);

            IVertex flowClass = GraphUtil.AddClass(meta, "Flow");

            // ParameterDefinition
            IVertex parameterDefinitionClass = GraphUtil.AddClass(meta, "ParameterDefinition");
            GraphUtil.AddInherits(parameterDefinitionClass, valueDefinitionClass);
            GraphUtil.AddAttribute(parameterDefinitionClass, "IsDerived", Boolean, 0, 1);

            // Parameter
            IVertex parameterClass = GraphUtil.AddClass(meta, "Parameter");
            GraphUtil.AddInherits(parameterClass, valueClass);

            // AddressDefinition
            IVertex addressDefinitionClass = GraphUtil.AddClass(meta, "AddressDefinition");
            GraphUtil.AddInherits(addressDefinitionClass, valueDefinitionClass);
            GraphUtil.AddAttribute(addressDefinitionClass, "InstancesCount", Integer, 0, 1);
            GraphUtil.AddAttribute(addressDefinitionClass, "InitialValue", Float, 0, 1);
            GraphUtil.AddAggregation(addressDefinitionClass, "Flow", flowClass, 0, -1);

            // Address
            IVertex addressClass = GraphUtil.AddClass(meta, "Address");
            GraphUtil.AddInherits(addressClass, valueClass);

            // Flow
            GraphUtil.AddAssociation(flowClass, "Destination", addressDefinitionClass, 1, 1);
            GraphUtil.AddAssociation(flowClass, "Alghoritm", VertexType, 1, 1);

            // FlovInstance
            IVertex flovInstanceClass = GraphUtil.AddClass(meta, "FlovInstance");
            GraphUtil.AddAttribute(flovInstanceClass, "Days", Integer, 0, 1);
            GraphUtil.AddAggregation(flovInstanceClass, "Parameter", parameterClass, 0, -1);
            GraphUtil.AddAggregation(flovInstanceClass, "Address", addressClass, 0, -1);
            GraphUtil.AddAssociation(flovInstanceClass, "Definition", flovClass, 1, 1);

            // Flov
            GraphUtil.AddAggregation(flovClass, "ParameterDefinition", parameterDefinitionClass, 0, -1);
            GraphUtil.AddAggregation(flovClass, "AddressDefinition", addressDefinitionClass, 0, -1);
        }

        static public void Execute()
        {
            CreateLoweFlov();

            ChainlinkDefinition.Create();
            ChainlinkSimulation.Run(90);
        }
    }
}
