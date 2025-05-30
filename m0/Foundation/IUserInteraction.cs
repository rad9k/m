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

        void ShowException(IVertex exception);

        void ShowInfo(string info);

        IVertex SelectDialog(IVertex info, IList<IEdge> options, bool firstSelected, Point? position);

        IVertex SelectButtonDialog(IVertex info, IList<IEdge> options, Point? position);

        void Edit(IVertex baseVertex, Point? position);

        string StringQuestionDialog(string question, Point? position);
    }
}
