using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jil;
using m0.Foundation;

namespace m0.Store.Json
{
    public class JsonSerializationData
    {
        public Dictionary<int, StoreId> StoreIdDictionary;
        public List<JsonVertex> Vertices;
    }

    public class JsonVertex
    {
        [JilDirective(Name = "Id", IsUnion = true)]
        public string IdString;

        [JilDirective(Name = "Id", IsUnion = true)]
        public long IdLong;

        
        [JilDirective(Name = "Value", IsUnion = true)]
        public string ValueString;

        [JilDirective(Name = "Value", IsUnion = true)]
        public int? ValueInt;

        [JilDirective(Name = "ValueDouble")]
        public double? ValueDouble;

        [JilDirective(Name = "ValueDecimal")]
        public decimal? ValueDecimal;
        

        public List<JsonEdge> Edges;
    }

    public class JsonEdge
    {
        public int MetaStoreId;

        [JilDirective(Name = "MetaId", IsUnion = true)]
        public string MetaIdString;

        [JilDirective(Name = "MetaId", IsUnion = true)]
        public long MetaIdLong;

        public int ToStoreId;

        [JilDirective(Name = "ToId", IsUnion = true)]
        public string ToIdString;

        [JilDirective(Name = "ToId", IsUnion = true)]
        public long ToIdLong;
    }
}
