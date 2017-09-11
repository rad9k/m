using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;

namespace m0.TextLanguage
{
    public interface IParser
    {
        IVertex Parse(IVertex rootVertex, string text);
    }
}
