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
        static double DoNotWantNewBias = 0.01;

        static IVertex r = m0.MinusZero.Instance.root;

        static FlovInstance ci;
        static Flov cf;

        static Address End_client = null,
            Product_creator = null,
            Product_contract = null,
            Centralized_Oracle = null,
            Golem_Requestor = null,
            Golem_Provider = null;

        static IList<Step> GLM_ETH_rate_Steps = null,
            Daily_Oracle_calls_Steps = null,
            One_Centralized_oracle_usage_payment_Steps = null,
            No_of_Providers_Steps = null,
            Requestor_fee_Steps = null,
            Daily_Provider_usage_payment_Steps = null,
            Product_creator_fee_Steps = null,
            Daily_end_client_lock_Steps = null,
            LINK_GLM_rate_Steps = null,
            Daily_Golem_Oracle_usage_payment_Steps = null,
            Daily_Centralized_Oracle_usage_payment_Steps = null,
            Product_creator_income_Steps = null;

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

            GetParameterSteps();

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

                s.Value = (1/0.005706) * GLM_ETH_rate_Value;
            }
        }

        static void AddParameterSteps_Daily_Golem_Oracle_usage_payment()
        {
            Parameter Daily_Golem_Oracle_usage_payment = null;
            Parameter No_of_Providers = null;
            Parameter Requestor_fee = null;
            Parameter Daily_Provider_usage_payment = null;
            Parameter LINK_GLM_rate = null;

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

                if (tp.Vertex.Value.ToString() == "LINK GLM rate")
                    LINK_GLM_rate = tp;
            }

            IList<Step> No_of_Providers_Steps = No_of_Providers.Steps;
            IList<Step> Requestor_fee_Steps = Requestor_fee.Steps;
            IList<Step> Daily_Provider_usage_payment_Steps = Daily_Provider_usage_payment.Steps;
            IList<Step> LINK_GLM_rate_Steps = LINK_GLM_rate.Steps;

            for (int d = 1; d <= ci.Days; d++)
            {
                Step s = Daily_Golem_Oracle_usage_payment.AddStep();

                s.Day = d;

                double No_of_Providers_Value = No_of_Providers_Steps[d - 1].Value;
                double Requestor_fee_Steps_Value = Requestor_fee_Steps[d - 1].Value;
                double Daily_Provider_usage_payment_Value = Daily_Provider_usage_payment_Steps[d - 1].Value;
                double LINK_GLM_rate_Value = LINK_GLM_rate_Steps[d - 1].Value;

                s.Value = No_of_Providers_Value * (1 + Requestor_fee_Steps_Value) * Daily_Provider_usage_payment_Value * LINK_GLM_rate_Value;
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

                switch (ad.Name)
                {
                    case ("End client"):
                        End_client = a; break;

                    case ("Product creator"):
                        Product_creator = a; break;

                    case ("Product contract"):
                        Product_contract = a; break;

                    case ("Centralized Oracle"):
                        Centralized_Oracle = a; break;

                    case ("Golem Requestor"):
                        Golem_Requestor = a; break;

                    case ("Golem Provider"):
                        Golem_Provider = a; break;
                }

            }
        }

        static public void GetParameterSteps()
        {
            foreach(Parameter p in ci.Parameters)
            {
                switch (p.Definition.Name)
                {
                    case ("GLM ETH rate"):
                        GLM_ETH_rate_Steps = p.Steps; break;

                    case ("Daily Oracle calls"):
                        Daily_Oracle_calls_Steps = p.Steps; break;

                    case ("One Centralized oracle usage payment"):
                        One_Centralized_oracle_usage_payment_Steps = p.Steps; break;

                    case ("No of Providers"):
                        No_of_Providers_Steps = p.Steps; break;

                    case ("Requestor fee"):
                        Requestor_fee_Steps = p.Steps; break;

                    case ("Daily Provider usage payment"):
                        Daily_Provider_usage_payment_Steps = p.Steps; break;

                    case ("Product creator fee"):
                        Product_creator_fee_Steps = p.Steps; break;

                    case ("Daily end client lock"):
                        Daily_end_client_lock_Steps = p.Steps; break;

                    case ("LINK GLM rate"):
                        LINK_GLM_rate_Steps = p.Steps; break;

                    case ("Daily Golem Oracle usage payment"):
                        Daily_Golem_Oracle_usage_payment_Steps = p.Steps; break;

                    case ("Daily Centralized Oracle usage payment"):
                        Daily_Centralized_Oracle_usage_payment_Steps = p.Steps; break;

                    case ("Product creator income"):
                        Product_creator_income_Steps = p.Steps; break;
                }
            }
        }

        static public void SimulateFlows()
        {

            SetAddressPrevInitialValues();

            for (int d = 1; d <= ci.Days; d++)
            {
                Simulate_value_lock(d);
                Simulate_product_creator_income(d);
                Simulate_Centralized_Oracle_payment(d);
                Simulate_Decentralized_Oracle_payment(d);
                Simulate_Golem_payment(d);
            }
        }

        static double End_client_prev = Double.NegativeInfinity;
        static double Product_contract_prev = Double.NegativeInfinity;
        static double Product_creator_prev = Double.NegativeInfinity;
        static double Centralized_Oracle_prev = Double.NegativeInfinity;
        static double Golem_Requestor_prev = Double.NegativeInfinity;
        static double Golem_Provider_prev = Double.NegativeInfinity;

        static void SetAddressPrevInitialValues()
        {
            End_client_prev = ((AddressDefinition)End_client.Definition).InitialValue;

            Product_contract_prev = ((AddressDefinition)Product_contract.Definition).InitialValue;
            Product_creator_prev = ((AddressDefinition)Product_creator.Definition).InitialValue;
            Centralized_Oracle_prev = ((AddressDefinition)Centralized_Oracle.Definition).InitialValue;
            Golem_Requestor_prev = ((AddressDefinition)Golem_Requestor.Definition).InitialValue;
            Golem_Provider_prev = ((AddressDefinition)Golem_Provider.Definition).InitialValue;
        }

        static Step sProduct_contract;

        static void Simulate_value_lock(int day)
        {
            double v = Daily_end_client_lock_Steps[day - 1].Value;

            sProduct_contract = Product_contract.AddStep();
            sProduct_contract.Day = day;
            sProduct_contract.Value = Product_contract_prev + v;
            Product_contract_prev = sProduct_contract.Value;

            Step sEnd_client = End_client.AddStep();
            sEnd_client.Day = day;
            sEnd_client.Value = End_client_prev - v;
            End_client_prev = sEnd_client.Value;
        }

        static void Simulate_product_creator_income(int day)
        {
            double lock_ = Daily_end_client_lock_Steps[day - 1].Value;
            double fee = Product_creator_fee_Steps[day - 1].Value;

            double v = lock_ * fee;

            sProduct_contract.Value -= v;
            Product_contract_prev = sProduct_contract.Value;

            Step sProduct_creator = Product_creator.AddStep();
            sProduct_creator.Day = day;
            sProduct_creator.Value = Product_creator_prev + v;
            Product_creator_prev = sProduct_creator.Value;
        }

        static void Simulate_Centralized_Oracle_payment(int day)
        {
            double v_LINK = Daily_Centralized_Oracle_usage_payment_Steps[day - 1].Value * (1-DoNotWantNewBias);

            sProduct_contract.Value -= v_LINK;
            Product_contract_prev = sProduct_contract.Value;

            Step sCentralized_Oracle = Centralized_Oracle.AddStep();
            sCentralized_Oracle.Day = day;
            sCentralized_Oracle.Value = Centralized_Oracle_prev + v_LINK;
            Centralized_Oracle_prev = sCentralized_Oracle.Value;
        }

        static Step sGolem_Requestor;

        static void Simulate_Decentralized_Oracle_payment(int day)
        {
            double LINK_GLM_rate = LINK_GLM_rate_Steps[day - 1].Value;
            double v_LINK = Daily_Golem_Oracle_usage_payment_Steps[day - 1].Value * DoNotWantNewBias;

            sProduct_contract.Value -= v_LINK;
            Product_contract_prev = sProduct_contract.Value;

            sGolem_Requestor = Golem_Requestor.AddStep();
            sGolem_Requestor.Day = day;
            sGolem_Requestor.Value = Golem_Requestor_prev + v_LINK * (1/LINK_GLM_rate);
            Golem_Requestor_prev = sGolem_Requestor.Value;
        }

        static void Simulate_Golem_payment(int day)
        {
            double v_GLM = No_of_Providers_Steps[day - 1].Value * Daily_Provider_usage_payment_Steps[day - 1].Value;

            sGolem_Requestor.Value -= v_GLM;
            Golem_Requestor_prev = sGolem_Requestor.Value;

            Step sGolem_Provider = Golem_Provider.AddStep();
            sGolem_Provider.Day = day;
            sGolem_Provider.Value = Golem_Provider_prev + v_GLM;
            Golem_Provider_prev = sGolem_Provider.Value;
        }


    }
}
