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

        IVertex SelectDialog(IVertex info, IVertex options, bool firstSelected, Point? position);

        IVertex SelectDialogButton(IVertex info, IVertex options, Point? position);

        void Edit(IVertex baseVertex, Point? position);

        string StringQuestionDialog(string question, Point? position);
    }
}
