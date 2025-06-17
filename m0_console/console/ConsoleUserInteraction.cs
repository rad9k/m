using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_console.console
{
    public class ConsoleUserInteraction : IUserInteraction
    {
        public void CloseWindowByContent(object obj)
        {

        }

        public void EditEdge(IVertex baseVertex)
        {
  
        }

        public string InteractionInput(string question)
        {
            return "";
        }

        public void InteractionOutput(string info)
        {
            
        }

        public void InteractionOutputException(IVertex exception)
        {
            throw new NotImplementedException();
        }

        public IVertex InteractionSelect(IVertex info, IList<IEdge> options, bool firstSelected)
        {
            return null;
        }

        public IVertex InteractionSelectButton(IVertex info, IList<IEdge> options)
        {
            return null;
        }

        public void OpenCodeVisualiser(IVertex baseVertex, bool isFloating)
        {

        }

        public void OpenDefaultVisualiser(IVertex baseVertex, bool isFloating)
        {

        }

        public void OpenFormVisualiser(IVertex baseVertex, bool isFloating)
        {

        }

        public void OpenVisualiser(IVertex baseVertex, IVertex inputVertex, bool isFloating)
        {

        }

        public void ShowContent(object obj)
        {

        }

        public void ShowContentFloating(object obj, FloatingWindowSize size)
        {

        }

        //

        public void UserInteractionInitialize()
        {

        }

        public void UserInteractionFinalize()
        {

        }

        //

        public bool TypedEdge_Get_Test(Type[] interfacesInToCreateType)
        {
            return false;
        }
    }
}