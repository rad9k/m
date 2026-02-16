using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Controls;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.UIWpf.Commands
{
    public class ExtraCommandHook
    {
        static IVertex r = MinusZero.Instance.Root;
        
        static IVertex baseVertex_parameter_meta = r.Get(false, @"System\Meta\UserCommands\OnUserCommand\baseVertex");

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

            IVertex metaVertex = GraphUtil.GetQueryOutFirst(baseVertex, "Meta", null);

            IList<IEdge> userCommands = GraphUtil.GetQueryOut(metaVertex, "UserCommand", null);

            bool anythingAdded = false;

            foreach (IEdge userCommand in userCommands)
            {
                IVertex commandVertex = userCommand.To;
                string name = GraphUtil.GetStringValue(commandVertex);

                IVertex nameVertex = GraphUtil.GetQueryOutFirst(commandVertex, "$Name", null);

                if (nameVertex != null)
                    name = GraphUtil.GetStringValue(nameVertex);

                MenuItem newMenuItem = m0ContextMenu.createMenuItem(name);

                newMenuItem.Tag = commandVertex;

                newMenuItem.Click += (sender, e) =>
                {
                    OnExtraCommandClick(sender, e);
                };

                contextMenu.Items.Add(newMenuItem);

                anythingAdded = true;
            }

            if (anythingAdded)
                contextMenu.Items.Add(new Separator());

            return;            
        }

        void OnExtraCommandClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                IVertex commandVertex = menuItem.Tag as IVertex;

                if (commandVertex != null)
                {
                    IVertex baseVertex = contextMenu.EdgeVertex;

                    IVertex parameters = InstructionHelpers.CreateStack();

                    parameters.AddEdge(baseVertex_parameter_meta, baseVertex);

                    INoInEdgeInOutVertexVertex ret = ZeroCodeExecutonUtil.FuncionCall(commandVertex, parameters);                    
                }
            }
        }
    }
}
