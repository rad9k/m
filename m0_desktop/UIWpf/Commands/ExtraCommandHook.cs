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
using m0.ZeroTypes.UX;

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

                MenuItem newStoreMenuItem4 = m0ContextMenu.createMenuItem("New M0X store (binary)");

                newStoreMenuItem4.Click += OnNewM0XStore;

                contextMenu.Items.Add(newStoreMenuItem4);

                //

                MenuItem newStoreMenuItem3 = m0ContextMenu.createMenuItem("New M0T store (text language)");

                newStoreMenuItem3.Click += OnNewM0TStore;

                contextMenu.Items.Add(newStoreMenuItem3);

                //

                MenuItem newStoreMenuItem2 = m0ContextMenu.createMenuItem("New M0J store (json)");

                newStoreMenuItem2.Click += OnNewM0JStore;

                contextMenu.Items.Add(newStoreMenuItem2);


                contextMenu.AddSeparator();
            }
        }

        void OnNewMusicSpaceStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("please enter new music space store name");

            if (storeName !=null && storeName != "")
            {
                if (!storeName.EndsWith(".m0j"))
                    storeName += ".m0j";

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

        void OnNewM0JStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0j"))
                    storeName += ".m0j";

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

        void OnNewM0XStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0x"))
                    storeName += ".m0x";

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

        void OnNewM0TStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (storeName != null && storeName != "")
            {
                if (!storeName.EndsWith(".m0t"))
                    storeName += ".m0t";

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
        static IVertex creationPoolMeta = r.Get(false, @"System\Meta\Visualiser\Diagram\CreationPool");

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


            GraphUtil.CreateOrReplaceEdge(uxcontainer, creationPoolMeta, ms);

            IVertex song = VertexOperations.AddInstance(ms, songMeta);

            song.Value = "New Song";
        }        
    }
}
