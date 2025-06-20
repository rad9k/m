using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;

namespace m0.Store
{
    public class MemoryStore:StoreBase
    {
        public bool IsTemporaryStore;

        public override string Identifier
        {
            get {
                if (_Identifier != null)
                    return _Identifier;
                else
                    return AppDomain.CurrentDomain.GetHashCode().ToString() + "|" + this.GetHashCode().ToString(); 
            }
        }
     
        public MemoryStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
            :base(identifier, storeUniverse, accessLeveList)
        {
            root = new EasyVertex(this);
        }

        public MemoryStore(String identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList, bool _IsTemporaryStore)
            : base(identifier, storeUniverse, accessLeveList)
        {
            IsTemporaryStore = _IsTemporaryStore;

            if (identifier == "$-0$EMPTY$STORE$") // hack
                alwaysPresent = true;

            root = new EasyVertex(this);

            root.IsRoot = true;
        }
    }
}
