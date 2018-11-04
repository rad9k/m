using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    // IStore has to have constructor with following parameters:
    // - Identifier
    // - IStoreUniverse
    // - AccessLavels[]

    public interface IStore:ITransactionRoot
    {
        IStoreUniverse StoreUniverse { get; }

        String TypeName { get; }
        
        String Identifier { get; }
               

        void Detach();

        // Deletes all incoming edges that has e.store==InDetachStore
        void InDetach(IStore InDetachStore);
        
        void Attach();

        void Close();

        DetachStateEnum DetachState { get; }
        

        IList<AccessLevelEnum> AccessLevel { get; }


        void StoreVertexIdentifier(IVertex Vertex);

        void RemoveVertexIdentifier(IVertex Vertex);

        IVertex GetVertexByIdentifier(string VertexIdentidier);
    }
}
