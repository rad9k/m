using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    static IVertex r = MinusZero.Instance.Root;

    static IVertex fileMeta = r.Get(false, @"System\Meta\Store\FileSystem\Directory\File");

    public class MusicSpace
    {
        public static void NewMusicSpaceStore(IVertex baseVertex)
        {
            baseVertex.AddEdge()
        }
    }
}
