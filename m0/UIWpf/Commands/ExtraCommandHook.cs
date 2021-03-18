using m0.Foundation;
using m0.UIWpf.Controls;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.UIWpf.Commands
{
    public class ExtraCommandHook
    {
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

            if(GeneralUtil.CompareStrings(metaVertex.Value, "Directory"))
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
            string storeName = UserInteractionUtil.Ask("music space store name");

           // BaseCommands.Open(this.EdgeVertex, null);
        }

        void OnNewStore(object sender, System.Windows.RoutedEventArgs e)
        {
            string storeName = UserInteractionUtil.Ask("store name");
        }
    }
}
