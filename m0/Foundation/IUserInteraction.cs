using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.Foundation
{
    public enum FloatingWindowSize {Micro, Small, Medium, Large };

    public interface IUserInteraction
    {        
        void ShowContent(object obj);
        
        void ShowContentFloating(object obj, FloatingWindowSize size);

        void CloseWindowByContent(object obj);

        //

        void EditEdge(IVertex baseVertex);

        //

        void InteractionOutputException(IVertex exception);

        void InteractionOutput(string info);

        IVertex InteractionSelect(IVertex info, IList<IEdge> options, bool firstSelected);

        IVertex InteractionSelectButton(IVertex info, IList<IEdge> options);

        string InteractionInput(string question);

        //

        void OpenDefaultVisualiser(IVertex baseVertex, bool isFloating);

        void OpenVisualiser(IVertex baseVertex, IVertex inputVertex, bool isFloating);

        void OpenCodeVisualiser(IVertex baseVertex,  bool isFloating);

        void OpenFormVisualiser(IVertex baseVertex, bool isFloating);

        //

        void UserInteractionInitialize();

        void UserInteractionFinalize();

        //

        bool TypedEdge_Get_Test(Type[] interfacesInToCreateType);
    }
}
