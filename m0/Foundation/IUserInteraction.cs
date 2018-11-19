using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.Foundation
{
    public interface IUserInteraction
    {
        void ShowContent(object obj);
        
        void ShowContentFloating(object obj);

        void CloseWindowByContent(object obj);

        void ShowException(IVertex exception);

        IVertex SelectDialog(IVertex info, IVertex options, Point? position);

        IVertex SelectDialogButton(IVertex info, IVertex options, Point? position);

        void EditDialog(IVertex baseVertex, Point? position);
    }
}
