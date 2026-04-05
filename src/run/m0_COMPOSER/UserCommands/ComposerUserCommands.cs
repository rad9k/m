using m0;
using m0.Foundation;
using m0.Graph;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0_COMPOSER.UserCommands
{
    public class ComposerUserCommands
    {
        static IVertex r = MinusZero.Instance.Root;

        static IVertex file_meta = r.Get(false, @"System\Meta\Store\FileSystem\Directory\File");

        public static INoInEdgeInOutVertexVertex OnNewMusicSpaceStore(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseVertexEdge = GraphUtil.GetQueryOutFirst(stack, "baseVertex", null);
            IVertex baseVertex = GraphUtil.GetQueryOutFirst(baseVertexEdge, "To", null);

            if (baseVertex == null)
                return exe.Stack;

            string storeName = UserInteractionUtil.Ask("please enter new music space store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0j"))
                    storeName += ".m0j";                

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                IVertex fileVertex = baseVertex.AddVertex(file_meta, storeName);

                IVertex store = fileVertex.Get(false, "$Store:");

                NewMusicSpaceStore(store);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }

            return exe.Stack;
        }


        static IVertex musicSpaceMeta = r.Get(false, @"System\Lib\Music\MusicSpace");
        static IVertex songMeta = r.Get(false, @"System\Lib\Music\Song");        

        static IVertex classMeta = r.Get(false, @"System\Meta\ZeroUML\Class");
        static IVertex linkMeta = r.Get(false, @"System\Meta\Base\Link");

        static IVertex trackMeta = r.Get(false, @"System\Lib\Music\Track");
        static IVertex sequenceEventMeta = r.Get(false, @"System\Lib\Music\SequenceEvent");
        static IVertex sequenceMeta = r.Get(false, @"System\Lib\Music\Sequence");
        static IVertex melodyFlowMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlow");
        static IVertex triggerSetMeta = r.Get(false, @"System\Lib\Music\Generator\TriggerSet");
        static IVertex chordProgressionMeta = r.Get(false, @"System\Lib\Music\Generator\ChordProgression");

        public static void NewMusicSpaceStore(IVertex baseVertex)
        {
            IEdge ms_Edge = VertexOperations.AddInstanceAndReturnEdge(baseVertex, musicSpaceMeta);

            IVertex ms = ms_Edge.To;
            ms.Value = "New Music Space";


            IVertex uxcontainer = UXContainer.CreateDefaultContainer(EdgeHelper.CreateTempEdgeVertex(ms_Edge));

            uxcontainer.Value = "Music Space";


            IVertex msm = ms.AddVertex(null, "Meta");

            msm.AddEdge(linkMeta, songMeta);
            msm.AddEdge(linkMeta, trackMeta);
            msm.AddEdge(linkMeta, sequenceEventMeta);
            msm.AddEdge(linkMeta, sequenceMeta);
            msm.AddEdge(linkMeta, melodyFlowMeta);
            msm.AddEdge(linkMeta, triggerSetMeta);
            msm.AddEdge(linkMeta, chordProgressionMeta);

            IVertex zm = ms.AddVertex(null, "ZeroMachines");
            zm.AddEdge(linkMeta, m0.MinusZero.Instance.root.Get(false, @"System\Lib\Music\Generator\'HarmonyMelodyTimeGenerator'"));
            zm.AddEdge(linkMeta, m0.MinusZero.Instance.root.Get(false, @"System\Lib\Music\Generator\'SimpleTransformer'"));

            ms.AddEdge(linkMeta, MinusZero.Instance.root.Get(false, @"System\Lib\Music\Instrument"));

            ms.AddEdge(linkMeta, MinusZero.Instance.root.Get(false, @"System\Lib\Music\Chord"));
            

            IVertex song = VertexOperations.AddInstance(ms, songMeta);

            song.Value = "New Song";
        }
    }
}
