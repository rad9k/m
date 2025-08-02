using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.Store.FileSystem;
using m0.Store.Json;
using static System.Net.WebRequestMethods;
using m0.ZeroTypes;
using m0.FormalTextLanguage;

namespace m0.Store.Text
{
    public class TextStore : StoreBase, ICommitBeforeGlobalDetachStore
    {
        bool properlyLoaded = true;

        static string defaultFormalTextLanguageProcessing_Query = @"System\FormalTextLanguage\ZeroCode_VertexAndManyLines";

        string formalTextLanguageProcessing_Query = null;
        string body = null;
        IVertex formalTextLanguageProcessing_Vertex = null;

        void Load()
        {
            if (System.IO.File.Exists(Identifier))
            {            
                using (StreamReader readStream = new StreamReader(Identifier))
                {
                    if (readStream.EndOfStream)
                    { // create new sub graph
                        EasyVertex __root = new EasyVertex(this);

                        root = __root;

                        root.IsRoot = true;                        
                    }
                    else
                    { // load graph from store

                        formalTextLanguageProcessing_Query = readStream.ReadLine();
                        body = readStream.ReadToEnd();

                        if (formalTextLanguageProcessing_Query == null || formalTextLanguageProcessing_Query != "")
                            formalTextLanguageProcessing_Query = defaultFormalTextLanguageProcessing_Query;

                        formalTextLanguageProcessing_Vertex = MinusZero.Instance.Root.Get(false, formalTextLanguageProcessing_Query);
                      
                        root = new EasyVertex(this);

                        IEdge baseEdge_new;

                        //IVertex errorList = MinusZero.Instance.DefaultFormalTextParser.Parse(formalTextLanguageProcessing_Vertex, new EdgeBase(null, null, root), body, CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

                        IVertex errorList = ZeroCodeProcessingHelper.Parse(formalTextLanguageProcessing_Vertex,
                            new EdgeBase(null, null, root),
                            body,
                            out baseEdge_new);
                            

                        if (errorList.OutEdges.Count > 0)
                            properlyLoaded = false;

                        root.IsRoot = true; 
                    }
                }

            }
            else
            { // create new
                EasyVertex __root = new EasyVertex(this);

                formalTextLanguageProcessing_Query = defaultFormalTextLanguageProcessing_Query;
                formalTextLanguageProcessing_Vertex = MinusZero.Instance.Root.Get(false, formalTextLanguageProcessing_Query);

                root = __root;

                root.IsRoot = true;                
            }          
        }

        public override void CommitTransaction()
        {
            CommitTransaction(Identifier);
        }

        public void CommitTransaction(string fileName)
        {            
            if (!properlyLoaded)
            {
                UserInteractionUtil.ShowError("Text serlialisation to " + fileName, "As text file " + fileName + " has not been properly loaded, commit (saving) is disabled for the file. This will protect existing file content.");
                return;
            }

            if (ReadOnly)
                return;

            bool wasDetached = false;

            if (this.DetachState == DetachStateEnum.Detached)
            {
                wasDetached = true;
                Attach();
            }

            //

            if (formalTextLanguageProcessing_Query == null)
                formalTextLanguageProcessing_Query = defaultFormalTextLanguageProcessing_Query;

            if (formalTextLanguageProcessing_Vertex == null)
                formalTextLanguageProcessing_Vertex = MinusZero.Instance.Root.Get(false, formalTextLanguageProcessing_Query);

            //

            StreamWriter writeStream = new StreamWriter(fileName);

            writeStream.WriteLine(formalTextLanguageProcessing_Query);

            EasyEdge e = new EasyEdge(MinusZero.Instance.Empty, null, root);

            string generated = MinusZero.Instance.DefaultFormalTextGenerator.Generate(formalTextLanguageProcessing_Vertex, e, CodeRepresentationEnum.VertexAndManyLines);

            writeStream.Write(generated);

            writeStream.Close();

            base.CommitTransaction();

            if (wasDetached)
                Detach();
        }

        public TextStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
            : base(identifier, storeUniverse, accessLeveList)
        {
            Load();

            Attach();
        }

        public override void Backup()
        {
            if (DetachState == DetachStateEnum.Attached)
            {
                UpdateDetachStateData();


                string fileName = FileSystemUtil.GetFileName(Identifier);

                string extension = FileSystemUtil.GetExtension(Identifier);

                string pathPart = FileSystemUtil.GetPathPart(Identifier);

                string backupFileName = pathPart + fileName + "." + extension + ".backup";


                CommitTransaction(backupFileName);
            }
        }
    }
}
