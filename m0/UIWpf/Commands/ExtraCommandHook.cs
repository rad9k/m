using m0.Foundation;
using m0.UIWpf.Controls;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using m0.ZeroTypes;
using m0.Graph;
using m0.User.Process.UX;

namespace m0.UIWpf.Commands
{
    public class ExtraCommandHook
    {
        static IVertex r = MinusZero.Instance.Root;

        static IVertex fileMeta = r.Get(false, @"System\Meta\Store\FileSystem\Directory\File");

        m0ContextMenu contextMenu;

        public ExtraCommandHook(m0ContextMenu _contextMenu)
        {
            contextMenu = _contextMenu;
        }

        public void CheckAndAddExtraCommand()
        {
            IVertex baseVertex = contextMenu.EdgeVertex;

            if (baseVertex == null)
                return;

            IVertex metaVertex = baseVertex.Get(false, "Meta:");

            if (GeneralUtil.CompareStrings(metaVertex.Value, "Directory") ||
                GeneralUtil.CompareStrings(metaVertex.Value, "Drive"))
            {
                MenuItem newStoreMenuItem = m0ContextMenu.createMenuItem("New music space store");

                newStoreMenuItem.Click += OnNewMusicSpaceStore;

                contextMenu.Items.Add(newStoreMenuItem);

                //

                MenuItem newStoreMenuItem2 = m0ContextMenu.createMenuItem("New store");

                newStoreMenuItem2.Click += OnNewStore;

                contextMenu.Items.Add(newStoreMenuItem2);

                contextMenu.AddSeparator();
            }
        }

        void OnNewMusicSpaceStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("please enter new music space store name");

            if (storeName != "")
            {
                if (!storeName.EndsWith(".m0"))
                    storeName += ".m0";

                IVertex baseVertex = contextMenu.EdgeVertex.Get(false, "To:");

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                IVertex fileVertex = baseVertex.AddVertex(fileMeta, storeName);

                IVertex store = fileVertex.Get(false, "$Store:");

                NewMusicSpaceStore(store);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }
        }

        void OnNewStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0"))
                    storeName += ".m0";

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                IVertex baseVertex = contextMenu.EdgeVertex.Get(false, "To:");

                baseVertex.AddVertex(fileMeta, storeName);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }
        }        

        static IVertex musicSpaceMeta = r.Get(false, @"System\Lib\Music\MusicSpace");
        static IVertex songMeta = r.Get(false, @"System\Lib\Music\Song");
        static IVertex diagramMeta = r.Get(false, @"System\Meta\Visualiser\Diagram");
        static IVertex creationPoolMeta = r.Get(false, @"System\Meta\Visualiser\Diagram\CreationPool");

        static IVertex classMeta = r.Get(false, @"System\Meta\ZeroUML\Class");

        static IVertex trackMeta = r.Get(false, @"System\Lib\Music\Track");
        static IVertex sequenceEventMeta = r.Get(false, @"System\Lib\Music\SequenceEvent");
        static IVertex sequenceMeta = r.Get(false, @"System\Lib\Music\Sequence");
        static IVertex melodyFlowMeta = r.Get(false, @"System\Lib\Music\Generator\MelodyFlow");
        static IVertex triggerSetMeta = r.Get(false, @"System\Lib\Music\Generator\TriggerSet");
        static IVertex chordProgressionMeta = r.Get(false, @"System\Lib\Music\Generator\ChordProgression");

        public static void NewMusicSpaceStore(IVertex baseVertex)
        {
            IVertex ms = VertexOperations.AddInstance(baseVertex, musicSpaceMeta);

            ms.Value = "New Music Space";
            

            IVertex diagram = VertexOperations.AddInstance(ms, diagramMeta);

            diagram.Value = "Mew Music Space Diagram";


            IVertex msm = ms.AddVertex(null, "Meta");

            msm.AddEdge(classMeta, songMeta);
            msm.AddEdge(classMeta, trackMeta);
            msm.AddEdge(classMeta, sequenceEventMeta);
            msm.AddEdge(classMeta, sequenceMeta);
            msm.AddEdge(classMeta, melodyFlowMeta);
            msm.AddEdge(classMeta, triggerSetMeta);
            msm.AddEdge(classMeta, chordProgressionMeta);

            IVertex zm = ms.AddVertex(null, "ZeroMachines");
            zm.AddEdge(classMeta, m0.MinusZero.Instance.root.Get(false, @"System\Lib\Music\Generator\'HarmonyMelodyTimeGenerator'"));
            zm.AddEdge(classMeta, m0.MinusZero.Instance.root.Get(false, @"System\Lib\Music\Generator\'SimpleTransformer'"));

            ms.AddEdge(null, MinusZero.Instance.root.Get(false, @"System\Lib\Music\Instrument"));

            ms.AddEdge(null, MinusZero.Instance.root.Get(false, @"System\Lib\Music\Chord"));


            GraphUtil.CreateOrReplaceEdge(diagram, creationPoolMeta, ms);

            IVertex song = VertexOperations.AddInstance(ms, songMeta);

            song.Value = "New Song";
        }
    }
}
