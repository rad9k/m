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

            SimulateFlows();
        }

        static void AddParameterInstances()
        {
            AddParameterInstances_Base();

            AddParameterSteps_LINK_GLM_rate();
            AddParameterSteps_Daily_Golem_Oracle_usage_payment();
            AddParameterSteps_Daily_Centralized_Oracle_usage_payment();
            AddParameterSteps_Product_creator_income();
        }

        static void AddParameterInstances_Base()
        {
            foreach (IEdge e in cf.Vertex.GetAll(false, "ParameterDefinition:"))
            {
                ParameterDefinition pd = (ParameterDefinition)TypedEdge.Get(e, typeof(ParameterDefinition));

                Parameter p = ci.AddParameter();
                p.Definition = pd;

                p.Vertex.Value = pd.Name;

                if (!pd.IsDerived)
                    AddStepsForParameter(p);
            }
        }

        static void AddParameterSteps_LINK_GLM_rate()
        {
            Parameter LINK_GLM_rate = null;
            Parameter GLM_ETH_rate = null;


            foreach (Parameter tp in ci.Parameters)
            {
                if (tp.Vertex.Value.ToString() == "LINK GLM rate")
                    LINK_GLM_rate = tp;

                if (tp.Vertex.Value.ToString() == "GLM ETH rate")
                    GLM_ETH_rate = tp;
            }

            IList<Step> GLM_ETH_rate_Steps = GLM_ETH_rate.Steps;



            for (int d = 1; d <= ci.Days; d++)
            {
                Step s = LINK_GLM_rate.AddStep();

                s.Day = d;

                double GLM_ETH_rate_Value = GLM_ETH_rate_Steps[d - 1].Value;

                s.Value = (1/0.00005706) * GLM_ETH_rate_Value;
            }
        }

        static void AddParameterSteps_Daily_Golem_Oracle_usage_payment()
        {
            Parameter Daily_Golem_Oracle_usage_payment = null;
            Parameter No_of_Providers = null;
            Parameter Requestor_fee = null;
            Parameter Daily_Provider_usage_payment = null;

            foreach (Parameter tp in ci.Parameters)
            {
                if (tp.Vertex.Value.ToString() == "Daily Golem Oracle usage payment")
                    Daily_Golem_Oracle_usage_payment = tp;

                if (tp.Vertex.Value.ToString() == "No of Providers")
                    No_of_Providers = tp;

                if (tp.Vertex.Value.ToString() == "Requestor fee")
                    Requestor_fee = tp;

                if (tp.Vertex.Value.ToString() == "Daily Provider usage payment")
                    Daily_Provider_usage_payment = tp;
            }

            IList<Step> No_of_Providers_Steps = No_of_Providers.Steps;
            IList<Step> Requestor_fee_Steps = Requestor_fee.Steps;
            IList<Step> Daily_Provider_usage_payment_Steps = Daily_Provider_usage_payment.Steps;

            for (int d = 1; d <= ci.Days; d++)
            {
                Step s = Daily_Golem_Oracle_usage_payment.AddStep();

                s.Day = d;

                double No_of_Providers_Value = No_of_Providers_Steps[d - 1].Value;
                double Requestor_fee_Steps_Value = Requestor_fee_Steps[d - 1].Value;
                double Daily_Provider_usage_payment_Value = Daily_Provider_usage_payment_Steps[d - 1].Value;

                s.Value = No_of_Providers_Value * Requestor_fee_Steps_Value * Daily_Provider_usage_payment_Value;
            }
        }

        static void AddParameterSteps_Daily_Centralized_Oracle_usage_payment()
        {
            Parameter Daily_Centralized_Oracle_usage_payment = null;
            Parameter Daily_Oracle_calls = null;
            Parameter One_Centralized_oracle_usage_payment = null;

            foreach (Parameter tp in ci.Parameters)
            {
                if (tp.Vertex.Value.ToString() == "Daily Centralized Oracle usage payment")
                    Daily_Centralized_Oracle_usage_payment = tp;

                if (tp.Vertex.Value.ToString() == "Daily Oracle calls")
                    Daily_Oracle_calls = tp;

                if (tp.Vertex.Value.ToString() == "One Centralized oracle usage payment")
                    One_Centralized_oracle_usage_payment = tp;
            }

            IList<Step> Daily_Oracle_calls_Steps = Daily_Oracle_calls.Steps;
            IList<Step> One_Centralized_oracle_usage_payment_Steps = One_Centralized_oracle_usage_payment.Steps;


            for (int d = 1; d <= ci.Days; d++)
            {
                Step s = Daily_Centralized_Oracle_usage_payment.AddStep();

                s.Day = d;

                double Daily_Oracle_calls_Value = Daily_Oracle_calls_Steps[d - 1].Value;
                double One_Centralized_oracle_usage_payment_Value = One_Centralized_oracle_usage_payment_Steps[d - 1].Value;

                s.Value = Daily_Oracle_calls_Value * One_Centralized_oracle_usage_payment_Value;
            }
        }

        static void AddParameterSteps_Product_creator_income()
        {
            Parameter Product_creator_income = null;
            Parameter Product_creator_fee = null;
            Parameter Daily_end_client_lock = null;

            foreach (Parameter tp in ci.Parameters)
            {
                if (tp.Vertex.Value.ToString() == "Product creator income")
                    Product_creator_income = tp;

                if (tp.Vertex.Value.ToString() == "Product creator fee")
                    Product_creator_fee = tp;

                if (tp.Vertex.Value.ToString() == "Daily end client lock")
                    Daily_end_client_lock = tp;
            }

            IList<Step> Product_creator_fee_Steps = Product_creator_fee.Steps;
            IList<Step> Daily_end_client_lock_Steps = Daily_end_client_lock.Steps;


            for (int d = 1; d <= ci.Days; d++)
            {
                Step s = Product_creator_income.AddStep();

                s.Day = d;

                double Product_creator_fee_Value = Product_creator_fee_Steps[d - 1].Value;
                double Daily_end_client_lock_Value = Daily_end_client_lock_Steps[d - 1].Value;

                s.Value = Product_creator_fee_Value * Daily_end_client_lock_Value;
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

                val += (p.Definition.MaxValue - p.Definition.MinValue) / (ci.Days - 1);
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

        static public void SimulateFlows()
        {

        }
    }
}
