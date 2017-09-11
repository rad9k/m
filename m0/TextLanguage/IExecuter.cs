using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;

namespace m0.TextLanguage
{
    public interface IExecuter
    {
        IVertex Execute(IVertex baseVertex, IVertex inputVertex, IVertex expression);

        IVertex Get(IVertex baseVertex, IVertex expression);

        IVertex GetAll(IVertex baseVertex, IVertex expression);
    }
}
