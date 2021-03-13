using m0.Foundation;
using m0_COMPOSER.UIWpf.Visualisers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{    
    public class SongVertexDictionary
    {
        static IDictionary<IVertex, SongVisualiser> SongVisusliserDictionary = new Dictionary<IVertex, SongVisualiser>();

        static IDictionary<IVertex, SongPlay> SongPlayDictionary = new Dictionary<IVertex, SongPlay>();

        public static void SetSongVisualiser(IVertex songVertex, SongVisualiser visualiser)
        {
            if (SongVisusliserDictionary.ContainsKey(songVertex))
                SongVisusliserDictionary.Remove(songVertex);

            SongVisusliserDictionary.Add(songVertex, visualiser);
        }

        public static SongVisualiser GetSongVisualiser(IVertex songVertex)
        {
            if (SongVisusliserDictionary.ContainsKey(songVertex))
                return SongVisusliserDictionary[songVertex];

            return null;
        }

        public static void SetSongPlay(IVertex songVertex, SongPlay play)
        {
            IVertex firstTrackVertex = songVertex.Get(false, "Track:"); // we can not use songVertex becouse it will be stack version of the vertex that is not the same as main IVertex

            if (SongPlayDictionary.ContainsKey(firstTrackVertex))
            {
                SongPlay oldPlaySong = SongPlayDictionary[firstTrackVertex];

                oldPlaySong.Destroy();

                SongPlayDictionary.Remove(firstTrackVertex);
            }            

            SongPlayDictionary.Add(firstTrackVertex, play);
        }

        public static SongPlay GetSongPlay(IVertex songVertex)
        {
            IVertex firstTrackVertex = songVertex.Get(false, "Track:"); // we can not use songVertex becouse it will be stack version of the vertex that is not the same as main IVertex

            if (SongPlayDictionary.ContainsKey(firstTrackVertex))
                return SongPlayDictionary[firstTrackVertex];

            return null;
        }
    }
}
