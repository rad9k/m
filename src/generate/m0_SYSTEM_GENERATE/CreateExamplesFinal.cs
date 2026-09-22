using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;

// this one is a bit of trash

namespace m0_SYSTEM_GENERATE
{
    public class CreateExamplesFinal
    {
        
        public static void CreateTestData()
        {            
            IVertex r = MinusZero.Instance.Root;
            
            IVertex tr = MinusZero.Instance.Root.AddVertex(null, "Examples");

            

            IVertex EnterpriseArchitecture = tr.AddVertex(null, "Enterprise AI Architecture");
            

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Examples\Enterprise_AI_Architecture.txt", EnterpriseArchitecture);
            
            IVertex OnlineRetailModel = tr.AddVertex(null, "Online retail model");

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Examples\Online_retail_model.txt", OnlineRetailModel);


            IVertex OnlineRetailBackend = tr.AddVertex(null, "Online retail backend");

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Examples\Online_retail_backend.txt", OnlineRetailBackend);


            IVertex LLMOrchestrator = tr.AddVertex(null, "LLM orchestrator");

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Examples\LLM_orchestrator.txt", LLMOrchestrator);


            IVertex AIModelPipeline = tr.AddVertex(null, "AI model pipeline");

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Examples\AI_model_pipeline.txt", AIModelPipeline);
            

            IVertex composer = tr.AddVertex(null, "-composer");

            GraphUtil.LoadTXTParseAndMove_ChildEdges(@"_RES\Examples\-composer.txt", composer);
        }
    }
}
