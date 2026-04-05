using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public class MusicUtil
    {
        public static IVertex GetNoteFromPitchSet(IVertex pitchSet, int? Octave, int? Note)
        {
            foreach(IEdge e in pitchSet)
            {
                IVertex noteVertex = e.To.Get(false, "Note:");
                IVertex octaveVertex = e.To.Get(false, "Octave:");

                if(noteVertex != null && octaveVertex != null)
                {
                    int? noteValue = GraphUtil.GetIntegerValue(noteVertex);
                    int? octaveValue = GraphUtil.GetIntegerValue(octaveVertex);

                    if (noteValue != null && octaveValue != null && Octave == (int)octaveValue && Note == (int)noteValue)
                        return e.To;
                }
            }

            return null;
        }
    }
}
