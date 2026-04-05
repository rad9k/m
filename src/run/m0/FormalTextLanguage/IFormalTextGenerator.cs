using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;
using m0.ZeroTypes;

namespace m0.FormalTextLanguage
{
    public interface IFormalTextGenerator
    {
        string Generate(IEdge graphBaseEdge, CodeRepresentationEnum codeRepresentation);

        string Generate(IVertex formalTextLanguage, IEdge graphBaseEdge, CodeRepresentationEnum codeRepresentation);

    }
}
