using m0.Foundation;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

using LovFlov.ZeroTypes;

namespace LovFlov
{
    public class ChainlinkDefinition
    {
        static IVertex r = m0.MinusZero.Instance.root;

        static Flov cf;

        static AddressDefinition End_client,
            Product_creator,
            Product_contract,
            Centralized_Oracle,
            Golem_Requestor,
            Golem_Provider;

        static ParameterDefinition GLM_ETH_rate,
            Daily_Oracle_calls,
            One_Centralized_oracle_usage_payment,
            No_of_Providers,
            Requestor_fee,
            Daily_Provider_usage_payment,
            Product_creator_fee,
            Daily_end_client_lock,
            LINK_GLM_rate,
            Daily_Golem_Oracle_usage_payment,
            Daily_Centralized_Oracle_usage_payment,
            Product_creator_income;

        static public void Create()
        {
            IVertex lf = r.Get(false, "LovFlov");

            IEdge cf_edge = VertexOperations.AddInstanceAndReturnEdge(lf, r.Get(false, @"LovFlov\Meta\Flov"));

            cf = new Flov(cf_edge);

            cf.Vertex.Value = "Chainlink";

            AddAddressesDefinitions();
            AddFlows();
            AddParametersDefinitions();
        }

        static void AddAddressesDefinitions()
        {
            End_client = AddAddressDefintion("End client", 1000000);
            Product_creator = AddAddressDefintion("Product creator", 0);
            Product_contract = AddAddressDefintion("Product contract", 1000);
            Centralized_Oracle = AddAddressDefintion("Centralized Oracle", 0);
            Golem_Requestor = AddAddressDefintion("Golem Requestor", 0);
            Golem_Provider = AddAddressDefintion("Golem Provider", 0);
        }

        static void AddFlows()
        {
            AddFlow(End_client, Product_contract, "value lock");
            AddFlow(Product_contract, Product_creator, "product creator income");
            AddFlow(Product_contract, Centralized_Oracle, "Centralized Oracle payment");
            AddFlow(Product_contract, Golem_Requestor, "Decentralized Oracle payment");
            AddFlow(Golem_Requestor, Golem_Provider, "Golem payment");
        }

        static void AddFlow(AddressDefinition from, AddressDefinition to, string name)
        {
            Flow f = from.AddFlow();

            f.Destination = to;
            f.Vertex.Value = name;
        }

        static AddressDefinition AddAddressDefintion(string Name, double InitialValue)
        {
            AddressDefinition ad = cf.AddAddressDefinition();
            ad.Name = Name;
            ad.Vertex.Value = Name;
            ad.InitialValue = InitialValue;

            return ad;
        }

        static void AddParametersDefinitions()
        {
            GLM_ETH_rate = AddParameterDefinition("GLM ETH rate", false, 0.0001956, 0.0002);
            Daily_Oracle_calls = AddParameterDefinition("Daily Oracle calls", false, 15000, 16000);
            One_Centralized_oracle_usage_payment = AddParameterDefinition("One Centralized oracle usage payment", false, 1, 1.1);
            No_of_Providers = AddParameterDefinition("No of Providers", false, 10, 20);
            Requestor_fee = AddParameterDefinition("Requestor fee", false, 90, 120);
            Daily_Provider_usage_payment = AddParameterDefinition("Daily Provider usage payment", false, 0.4056, 0.5);
            Product_creator_fee = AddParameterDefinition("Product creator fee", false, 0.1, 0.2);
            Daily_end_client_lock = AddParameterDefinition("Daily end client lock", false, 2000, 2100);

            LINK_GLM_rate = AddParameterDefinition("LINK GLM rate", true, 0, 0);
            Daily_Golem_Oracle_usage_payment = AddParameterDefinition("Daily Golem Oracle usage payment", true, 0, 0);
            Daily_Centralized_Oracle_usage_payment = AddParameterDefinition("Daily Centralized Oracle usage payment", true, 0, 0);
            Product_creator_income = AddParameterDefinition("Product creator income", true, 0, 0);
        }

       static ParameterDefinition AddParameterDefinition(string Name, bool isDerived, double MinValue, double MaxValue)
        {
            ParameterDefinition pd = cf.AddParameterDefinition();

            pd.Name = Name;
            pd.Vertex.Value = Name;

            pd.IsDerived = isDerived;
            pd.MinValue = MinValue;
            pd.MaxValue = MaxValue;

            return pd;
        }
    }
}
