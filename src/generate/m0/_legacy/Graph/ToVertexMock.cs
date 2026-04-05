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

        public ToVertexMock(object _mockData): base(m0.MinusZero.Instance.TempStore)
        {
            mockData = _mockData;

            Value = _mockData.ToString();
        }
    }
}
