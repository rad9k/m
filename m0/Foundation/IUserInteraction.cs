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

        void EditEdge(IVertex baseVertex, Point? position);

        //

        void InteractionOutputException(IVertex exception);

        void InteractionOutput(string info);

        IVertex InteractionSelect(IVertex info, IList<IEdge> options, bool firstSelected, Point? position);

        IVertex InteractionSelectButton(IVertex info, IList<IEdge> options, Point? position);

        string InteractionInput(string question, Point? position);

        //

        IVertex OpenVisualiser(IVertex baseVertex, IVertex inputVertex, bool isFloating);

        IVertex OpenFormVisualiser(IVertex baseVertex, bool isFloating);
    }
}
