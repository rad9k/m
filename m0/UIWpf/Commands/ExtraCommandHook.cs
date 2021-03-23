using m0.Foundation;
using m0.UIWpf.Controls;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using m0_COMPOSER.Lib;
using m0.ZeroTypes;

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

            if (!storeName.EndsWith(".m0"))
                storeName += ".m0";

            IVertex baseVertex = contextMenu.EdgeVertex.Get(false, "To:");

            IVertex fileVertex = baseVertex.AddVertex(fileMeta, storeName);

            IVertex store
        
            NewMusicSpaceStore()
        }

        void OnNewStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("please enter new store name");

            if (!storeName.EndsWith(".m0"))
                storeName += ".m0";

            IVertex baseVertex = contextMenu.EdgeVertex.Get(false, "To:");

            baseVertex.AddVertex(fileMeta, storeName);
        }        

        static IVertex musicSpaceMeta = r.Get(false, @"System\Lib\Music\MusicSpace");
        static IVertex songMeta = r.Get(false, @"System\Lib\Music\Song");
        static IVertex diagramMeta = r.Get(false, @"System\Meta\ZeroTypes\Diagram");

        public static void NewMusicSpaceStore(IVertex baseVertex)
        {
            IVertex ms = VertexOperations.AddInstance(baseVertex, musicSpaceMeta);

            ms.Value = "New Music Space";

            IVertex diagram = VertexOperations.AddInstance(ms, diagramMeta);

            diagram.Value = "Mew Music Space Diagram";

            IVertex song = VertexOperations.AddInstance(ms, songMeta);

            song.Value = "New Song";
        }
    }
}
