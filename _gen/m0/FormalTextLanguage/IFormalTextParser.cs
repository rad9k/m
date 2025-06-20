using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;
using m0.ZeroTypes.UX;

namespace m0.FormalTextLanguage
{
    public interface IFormalTextParser
    {
        IVertex Parse(IEdge rootEdge, string text, CodeRepresentationEnum codeRepresentation, out IEdge rootEdge_new);

        IVertex Parse(IVertex formalTextLanguage, IEdge rootEdge, string text, CodeRepresentationEnum codeRepresentation, out IEdge rootEdge_new);
    }
}
