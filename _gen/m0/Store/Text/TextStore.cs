using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;
using Jil;
using m0.Util;
using m0.Store.FileSystem;
using m0.Store.Json;
using static System.Net.WebRequestMethods;

namespace m0.Store.Text
{
    public class TextStore : StoreBase, ICommintBeforeGlobalDetachStore
    {
        bool canWrite = true;

        string pathToLanguageDefinition = null;
        string body = null;
        IVertex formalTextLanguageVertex = null;

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

                        pathToLanguageDefinition = readStream.ReadLine();
                        body = readStream.ReadToEnd();

                        if (pathToLanguageDefinition != null && pathToLanguageDefinition != "")
                            formalTextLanguageVertex = MinusZero.Instance.Root.Get(false, pathToLanguageDefinition);
                        else
                            pathToLanguageDefinition = @"System\FormalTextLanguage\ZeroCode";

                        if (formalTextLanguageVertex == null)
                            formalTextLanguageVertex = MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\ZeroCode");

                        root = new EasyVertex(this);

                        IEdge baseEdge_new;
                        IVertex errorList = MinusZero.Instance.DefaultFormalTextParser.Parse(formalTextLanguageVertex, new EdgeBase(null, null, root), body, m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

                        if (errorList.OutEdges.Count > 0)
                            canWrite = false;

                        root.IsRoot = true; 
                    }
                }

            }
            else
            { // create new
                EasyVertex __root = new EasyVertex(this);

                pathToLanguageDefinition = @"System\FormalTextLanguage\ZeroCode";
                formalTextLanguageVertex = MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\ZeroCode");

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
            if (!canWrite)
            {
                UserInteractionUtil.ShowError("Text serlialisation to " + fileName, "As text file " + fileName + " has not been properly loaded, commit (saving) is disabled for the file. This will protect existing file content.");
                return;
            }

            bool wasDetached = false;

            if (this.DetachState == DetachStateEnum.Detached)
            {
                wasDetached = true;
                Attach();
            }

            //

            if (pathToLanguageDefinition == null)
                pathToLanguageDefinition = @"System\FormalTextLanguage\ZeroCode";

            if (formalTextLanguageVertex == null)
                formalTextLanguageVertex = MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\ZeroCode");

            //

            StreamWriter writeStream = new StreamWriter(fileName);

            writeStream.WriteLine(pathToLanguageDefinition);

            EasyEdge e = new EasyEdge(MinusZero.Instance.Empty, null, root);

            string generated = MinusZero.Instance.DefaultFormalTextGenerator.Generate(formalTextLanguageVertex, e, ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines);

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


                string fileName = FileSystemUtil.getFileName(Identifier);

                string extension = FileSystemUtil.getExtension(Identifier);

                string pathPart = FileSystemUtil.getPathPart(Identifier);

                string backupFileName = pathPart + fileName + "." + extension + ".backup";


                CommitTransaction(backupFileName);
            }
        }
    }
}
