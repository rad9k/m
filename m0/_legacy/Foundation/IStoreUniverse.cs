using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public interface IStoreUniverse:ITransactionRoot
    {
        IList<IStore> Stores { get; }

        IStore GetStore(string StoreTypeName, string StoreIdentifier);
                        
        IVertex Empty { get; }
    }
}
