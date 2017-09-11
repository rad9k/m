using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0;


namespace m0.TextLanguage.GoldParser
{
    class GoldGenericParser: IParser
    {
        GoldGenericParser_BackEnd goldParser;

        IVertex langDef;

        IVertex MetaTextLanguageParsedTree;

        public bool EscapeQuotas;

        public bool RemoveBeginEndQuotas;

        public IVertex Parse(IVertex rootVertex, string text)
        {

            if (goldParser.Parse(new System.IO.StringReader(text)))
            {
                Parse_Reccurent(goldParser.Root, rootVertex, new List<IVertex>());

                return null;
            }
            else
            {
                IVertex error = MinusZero.Instance.CreateTempVertex();
                                
                IVertex error2 = MinusZero.Instance.CreateTempVertex();
                error2.Value = goldParser.FailMessage;

                error.AddEdge(null, error2);                

                return error;
            }

            
        }

        string ParseLeaf(string leaf)
        {
            if (RemoveBeginEndQuotas && leaf[0] == '"' && leaf[leaf.Length - 1] == '"')
                leaf = leaf.Substring(1, leaf.Length - 2);

            if (EscapeQuotas)
                return leaf.Replace("\\\"", "\"");
            else
                return leaf;
        }

        void Parse_Reccurent(GOLD.Reduction reduction, IVertex v, List<IVertex> generatedVertexList)
        {
            IVertex current=v;

            for (int x = 0; x < reduction.Count(); x++)
                if (reduction[x].Type() == GOLD.SymbolType.Nonterminal)
                {
                    GOLD.Reduction branch = (GOLD.Reduction)reduction[x].Data;

                    Parse_Reccurent(branch, current, generatedVertexList);
                }
                else
                {
                    string leaf = ParseLeaf( (string)reduction[x].Data );

                    //IVertex def= langDef.Get("\"" + leaf + "\"");                    
                    IVertex def = GraphUtil.FindOneByValue(langDef, leaf);

                    if (def != null)
                    {

                        //if (def.Get("PreviousTerminalMoveDown:") != null)
                        if (GraphUtil.FindOneByMeta(def, "PreviousTerminalMoveDown") != null)
                        {
                            IEdge previousEdge = v.OutEdges.Last();

                            v.DeleteEdge(previousEdge);

                            current = v.AddVertex(def, leaf);

                            generatedVertexList.Add(current);

                            current.AddEdge(previousEdge.Meta, previousEdge.To);

                            // }else if(def.Get("MoveDownToPreviousContainerTerminalOrCretedEmpty:")!=null){

                        }
                        else if (GraphUtil.FindOneByMeta(def, "MoveDownToPreviousContainerTerminalOrCretedEmpty") != null)
                        {
                            IEdge previousEdge = v.OutEdges.LastOrDefault();

                            //if ((previousEdge!=null)&&((GeneralUtil.CompareStrings(previousEdge.Meta.Value,"$Empty"))||(previousEdge.Meta.Get("ContainerTerminal:")!=null)))
                            if (
                                ((previousEdge != null) && generatedVertexList.Contains(previousEdge.To)) && (
                                 ((GeneralUtil.CompareStrings(previousEdge.Meta.Value, "$Empty"))
                                || (GraphUtil.FindOneByMeta(previousEdge.Meta, "ContainerTerminal") != null))                                
                                ))
                            {
                                IVertex previousVertex = previousEdge.To;

                                current = previousVertex.AddVertex(def, leaf);

                                generatedVertexList.Add(current);
                            }
                            else
                            {
                                //current = v.AddVertex(MetaTextLanguageParsedTree.Get("$EmptyContainerTerminal"), null);

                                current = v.AddVertex(GraphUtil.FindOneByValue(MetaTextLanguageParsedTree, "$EmptyContainerTerminal"), null);

                                generatedVertexList.Add(current);

                                current = current.AddVertex(def, leaf);
                            }

                        }
                        else
                        {
                            current = v.AddVertex(def, leaf);

                            generatedVertexList.Add(current);
                        }
                    }
                    else
                    {
                        current = v.AddVertex(def, leaf);

                        generatedVertexList.Add(current);
                    }
                }
        }     

        public GoldGenericParser(string fileName, IVertex languageDefinitionRoot)
        {
            EscapeQuotas = true;
            RemoveBeginEndQuotas = true;
            
            goldParser=new GoldGenericParser_BackEnd();
            goldParser.Setup(fileName);

            langDef = languageDefinitionRoot;

            MetaTextLanguageParsedTree = MinusZero.Instance.MetaTextLanguageParsedTreeVertex;
        }
    }
}
