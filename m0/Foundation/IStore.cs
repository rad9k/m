using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public class StoreId
    {
        public string TypeName;

        public string Identifier;

        public StoreId(string _TypeName, string _Identifier)
        {
            TypeName = _TypeName;
            Identifier = _Identifier;
        }

        public StoreId() // for deserialisation
        {

        }
    }
    
    
    
    // IStore has to have constructor with following parameters:
    // - Identifier
    // - IStoreUniverse
    // - AccessLavels[]

    public interface IStore: ITransactionRoot
    {
        IStoreUniverse StoreUniverse { get; }

        long VertexIdentifierCount { get; set; }

        String TypeName { get; }
        
        String Identifier { get; }
               
        bool AlwaysPresent { get; } // if AlwaysPresent, do not have to move its vertexes in the ZeroUMLInstructionHelper.MoveEdgesIntoVertex        

        void UpdateDetachStateData();

        void Detach();

        // Deletes all incoming edges that has e.store==InDetachStore
        void InDetach(IStore InDetachStore);
        
        void Attach();

        void Close();

        DetachStateEnum DetachState { get; }
        

        IList<AccessLevelEnum> AccessLevel { get; }


        void StoreVertexIdentifier(IVertex Vertex);

        void RemoveVertexIdentifier(IVertex Vertex);

        IVertex GetVertexByIdentifier(object VertexIdentidier);

        object GetRootIdentifier();

        void Backup();
    }
}
