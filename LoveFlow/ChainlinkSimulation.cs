using m0.Foundation;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Text;

using LovFlov.ZeroTypes;

namespace LovFlov
{
    public class ChainlinkSimulation
    {
        static IVertex r = m0.MinusZero.Instance.root;

        static Flov cf;

        static string[] Address_Names = { "End client", "Product creator", "Product contract", "Centralized wallet", "Golem Requestor wallet", "Provider wallet" };

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

            AddAddresses();
            AddParameters();
        }

        static public void Run()
        {

        }

        static void AddAddresses()
        {
            foreach (string s in Address_Names)
            {
                AddressDefinition ad = cf.AddAddressDefinition();
                ad.Name = s;
                ad.Vertex.Value = s;
            }
        }

        static void AddParameters()
        {
            GLM_ETH_rate = AddParameter("GLM ETH rate", false, 1, 10);
            Daily_Oracle_calls = AddParameter("Daily Oracle calls", false, 1, 10);
            One_Centralized_oracle_usage_payment = AddParameter("One Centralized oracle usage payment", false, 1, 10);
            No_of_Providers = AddParameter("No of Providers", false, 1, 10);
            Requestor_fee = AddParameter("Requestor fee", false, 1, 10);
            Daily_Provider_usage_payment = AddParameter("Daily Provider usage payment", false, 1, 10);
            Product_creator_fee = AddParameter("Product creator fee", false, 1, 10);
            Daily_end_client_lock = AddParameter("Daily end client lock", false, 1, 10);

            LINK_GLM_rate = AddParameter("LINK GLM rate", true, 1, 10);
            Daily_Golem_Oracle_usage_payment = AddParameter("Daily Golem Oracle usage_payment", true, 1, 10);
            Daily_Centralized_Oracle_usage_payment = AddParameter("Daily Centralized Oracle usage payment", true, 1, 10);
            Product_creator_income = AddParameter("Product creator income", true, 1, 10);
        }

       static ParameterDefinition AddParameter(string Name, bool isDerived, double MinValue, double MaxValue)
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
