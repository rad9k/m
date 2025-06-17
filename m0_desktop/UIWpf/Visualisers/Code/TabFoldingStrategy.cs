// based on avalon edit's sample  BraceFoldingStrategy

using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
#if NREFACTORY
using ICSharpCode.NRefactory.Editor;
#endif

namespace m0.UIWpf.Visualisers.Code
{
    /// <summary>
    /// Allows producing foldings from a document based on braces.
    /// </summary>
    public class TabFoldingStrategy
    {

        /// <summary>
        /// Creates a new BraceFoldingStrategy.
        /// </summary>
        public TabFoldingStrategy()
        {
     
        }

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            int firstErrorOffset;
            IEnumerable<NewFolding> newFoldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            List<NewFolding> newFoldings = new List<NewFolding>();

            Stack<int> startOffsets = new Stack<int>();
            int lastNewLineOffset = 0;

            int prevTabs = 0;
            int tabsInThisLine = -1;
            bool isLineTabPart = false;

            bool isOpen;
            bool isClose;

            int closeTimes = -1;
            int prevLinePosition = 0;
            int prevLineEndPosition = 0;

            bool savePrevLinePosition = false;

            for (int i = 0; i < document.TextLength; i++)
            {
                isOpen = false;
                isClose = false;

                savePrevLinePosition = false;

                char c = document.GetCharAt(i);

                if (c == '\n' || c == '\r')
                {
                    tabsInThisLine = 0;
                    isLineTabPart = true;
                    prevLineEndPosition = i;
                }

                if (c == '\t' && isLineTabPart)
                    tabsInThisLine++;
                
                if(c != '\t'&& c != '\n' && c != '\r' && isLineTabPart)
                {
                    savePrevLinePosition = true;

                    isLineTabPart = false;

                    if (tabsInThisLine > prevTabs)
                        isOpen = true;

                    if (tabsInThisLine < prevTabs)
                    {
                        isClose = true;
                        closeTimes = prevTabs - tabsInThisLine;
                    }

                    prevTabs = tabsInThisLine;                 
                }
                
                if(isOpen)
                {
                    startOffsets.Push(prevLineEndPosition-1);                    
                }
                
                else if (isClose)
                    for (int x = 0; x < closeTimes; x++)
                        if(startOffsets.Count>0) // otherwise there was Exception
                    {                    
                        int startOffset = startOffsets.Pop();

                        newFoldings.Add(new NewFolding(startOffset, prevLineEndPosition-1));                    
                    }


                if (savePrevLinePosition)
                    prevLinePosition = i;
            }

            if(startOffsets.Count>0)
                for (int x = 0; x < startOffsets.Count; x++)
                {
                    int startOffset = startOffsets.Pop();

                    newFoldings.Add(new NewFolding(startOffset, document.TextLength));
                }

            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }
}