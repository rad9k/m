using m0.Foundation;
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
        public static void CheckAndAddExtraCommand(IVertex baseVertex, ContextMenu menu)
        {
            if (baseVertex == null)
                return;

            IVertex metaVertex = baseVertex.Get(false, "Meta:");

            if(GeneralUtil.CompareStrings(metaVertex.Value, "Directory"))
            {

                menu.Items.Add()
            }
        }
    }
}
