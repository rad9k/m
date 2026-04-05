using System;

namespace m0.UIWpf.Controls.Fast
{
    /// <summary>
    ///     Indicates which end of the line has an arrow.
    /// </summary>
    [Flags]
    public enum ArrowEnds
    {
        None = 0,
        Start = 1,
        End = 2,
        Both = 3
    }

    public enum LineEndEnum
    {
        Straight, Arrow, Triangle, FilledTriangle, Diamond, FilledDiamond
    }
}