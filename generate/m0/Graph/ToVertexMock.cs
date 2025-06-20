using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph
{
    public class ToVertexMock : EasyVertex
    {
        public object mockData;

        public IVertex parentVertex;

       /* public ToVertexMock(IStore store) : base(store)
        {

        }*/

        public ToVertexMock(object _mockData, IVertex _parentVertex): base(m0.MinusZero.Instance.TempStore)
        {
            mockData = _mockData;

            parentVertex = _parentVertex;

            Value = _mockData.ToString();
        }
    }
}
