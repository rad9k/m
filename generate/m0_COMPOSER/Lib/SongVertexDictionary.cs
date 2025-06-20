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

        static IVertex MakeSongVertexUnique(IVertex songVertex)
        {
            //return songVertex.Get(false, "Track:"); // we can not use songVertex becouse it will be stack version of the vertex that is not the same as main IVertex

            return songVertex.Get(false, "Tempo:"); // we can not use songVertex becouse it will be stack version of the vertex that is not the same as main IVertex
        }

        public static void SetSongVisualiser(IVertex _songVertex, SongVisualiser visualiser)
        {
            IVertex songVertex = MakeSongVertexUnique(_songVertex);

            if (SongVisusliserDictionary.ContainsKey(songVertex))
                SongVisusliserDictionary.Remove(songVertex);

            SongVisusliserDictionary.Add(songVertex, visualiser);
        }

        public static IVertex GetRealSongVertex(IVertex _songVertex) // this assumes we have song visualiser running, if no, we are getting the fake songVertex (NoInEdgeInOutVertexVertex)
        {
            IVertex songVertex = MakeSongVertexUnique(_songVertex);

            if (SongVisusliserDictionary.ContainsKey(songVertex))
                return SongVisusliserDictionary[songVertex].VisualizedVertex;

            return _songVertex;
        }

        public static SongVisualiser GetSongVisualiser(IVertex _songVertex)
        {
            IVertex songVertex = MakeSongVertexUnique(_songVertex);

            if (SongVisusliserDictionary.ContainsKey(songVertex))
                return SongVisusliserDictionary[songVertex];

            return null;
        }

        public static void SetSongPlay(IVertex _songVertex, SongPlay play)
        {            
            IVertex songVertex = MakeSongVertexUnique(_songVertex);

            if (SongPlayDictionary.ContainsKey(songVertex))
            {
                SongPlay oldPlaySong = SongPlayDictionary[songVertex];

                oldPlaySong.Destroy();

                SongPlayDictionary.Remove(songVertex);
            }            

            SongPlayDictionary.Add(songVertex, play);
        }

        public static void RemoveSongPlay(IVertex _songVertex)
        {
            IVertex songVertex = MakeSongVertexUnique(_songVertex);

            if (SongPlayDictionary.ContainsKey(songVertex))
                SongPlayDictionary.Remove(songVertex);                
        }

        public static SongPlay GetSongPlay(IVertex _songVertex)
        {
            IVertex songVertex = MakeSongVertexUnique(_songVertex);            

            if (SongPlayDictionary.ContainsKey(songVertex))
                return SongPlayDictionary[songVertex];

            return null;
        }
    }
}
