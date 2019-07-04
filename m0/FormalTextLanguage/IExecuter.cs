using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;

namespace m0.FormalTextLanguage
{
    public interface IExecuter
    {
        IVertex Execute(IVertex baseVertex, IVertex expression);

        IVertex Get(bool metaMode, IVertex baseVertex, IVertex expression);

        IVertex GetAll(bool metaMode, IVertex baseVertex, IVertex expression);
    }
}
