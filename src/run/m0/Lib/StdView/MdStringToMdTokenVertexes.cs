using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class MdStringToMdTokenVertexes
    {                
        static IVertex Md = MinusZero.Instance.Root.Get(false, @"System\Lib\StdView\Md");

        static IVertex Text = Md.Get(false, "Text");
        static IVertex HardBreak = Md.Get(false, "HardBreak");
        static IVertex ParagraphBreak = Md.Get(false, "ParagraphBreak");
        static IVertex HorizontalRule = Md.Get(false, "HorizontalRule");
        static IVertex Header1Start = Md.Get(false, "Header1Start");
        static IVertex Header2Start = Md.Get(false, "Header2Start");
        static IVertex Header3Start = Md.Get(false, "Header3Start");
        static IVertex Header4Start = Md.Get(false, "Header4Start");
        static IVertex Header5Start = Md.Get(false, "Header5Start");
        static IVertex Header6Start = Md.Get(false, "Header6Start");
        static IVertex Header1End = Md.Get(false, "Header1End");
        static IVertex Header2End = Md.Get(false, "Header2End");
        static IVertex Header3End = Md.Get(false, "Header3End");
        static IVertex Header4End = Md.Get(false, "Header4End");
        static IVertex Header5End = Md.Get(false, "Header5End");
        static IVertex Header6End = Md.Get(false, "Header6End");
        static IVertex BoldStart = Md.Get(false, "BoldStart");
        static IVertex BoldEnd = Md.Get(false, "BoldEnd");
        static IVertex ItalicStart = Md.Get(false, "ItalicStart");
        static IVertex ItalicEnd = Md.Get(false, "ItalicEnd");
        static IVertex CodeBlock = Md.Get(false, "CodeBlock");
        static IVertex FormalTextLanguageName = Md.Get(false, @"CodeBlock\FormalTextLanguageName");
        static IVertex InlineCodeStart = Md.Get(false, "InlineCodeStart");
        static IVertex InlineCodeEnd = Md.Get(false, "InlineCodeEnd");
        static IVertex Link = Md.Get(false, "Link");
        static IVertex LinkText = Md.Get(false, @"Link\Text");
        static IVertex LinkUrl = Md.Get(false, @"Link\Url");
        static IVertex Image = Md.Get(false, "Image");
        static IVertex ImageText = Md.Get(false, @"Image\Text");
        static IVertex ImageUrl = Md.Get(false, @"Image\Url");
        static IVertex Generic = Md.Get(false, "Generic");
        static IVertex GenericNameValue = Md.Get(false, @"Generic\NameValue");
        static IVertex GenericName = Md.Get(false, @"Generic\NameValue\Name");
        static IVertex GenericValue = Md.Get(false, @"Generic\NameValue\Value");
        static IVertex BlockquoteStart = Md.Get(false, "BlockquoteStart");
        static IVertex BlockquoteEnd = Md.Get(false, "BlockquoteEnd");
        static IVertex ListItemsStart = Md.Get(false, "ListItemsStart");
        static IVertex ListItemStart = Md.Get(false, "ListItemStart");
        static IVertex ListItemsEnd = Md.Get(false, "ListItemsEnd");
        static IVertex OrderedListItemsStart = Md.Get(false, "OrderedListItemsStart");
        static IVertex OrderedListItemStart = Md.Get(false, "OrderedListItemStart");
        static IVertex OrderedListItemsEnd = Md.Get(false, "OrderedListItemsEnd");
        static IVertex TableStart = Md.Get(false, "TableStart");
        static IVertex HeaderBegin = Md.Get(false, "HeaderBegin");
        static IVertex HeaderEnd = Md.Get(false, "HeaderEnd");
        static IVertex TableEnd = Md.Get(false, "TableEnd");
        static IVertex RowBegin = Md.Get(false, "RowBegin");
        static IVertex RowEnd = Md.Get(false, "RowEnd");
        static IVertex HeaderColumnBegin = Md.Get(false, "HeaderColumnBegin");
        static IVertex HeaderColumnEnd = Md.Get(false, "HeaderColumnEnd");
        static IVertex CellBegin = Md.Get(false, "CellBegin");
        static IVertex CellEnd = Md.Get(false, "CellEnd");
        static IVertex AlignLeft = Md.Get(false, "AlignLeft");
        static IVertex AlignCenter = Md.Get(false, "AlignCenter");
        static IVertex AlignRight = Md.Get(false, "AlignRight");
        static IVertex AlignJustifty = Md.Get(false, "AlignJustify");

        // State tracking for nested elements
        private static bool isInsideBold = false;
        private static bool isInsideItalic = false;
        private static bool isInsideCodeBlock = false;
        private static bool isInsideInlineCode = false;
        private static bool isInsideLink = false;
        private static bool isInsideImage = false;
        private static bool isInsideTable = false;
        private static int blockquoteLevel = 0;
        private static int currentHeaderLevel = 0;
        private static StringBuilder codeBlockContent = null;
        private static string codeBlockLanguageName = null;

        private enum ListType
        {
            None,
            Unordered,
            Ordered
        }

        private struct ListContext
        {
            public ListType Type;
            public int Indent;
        }

        private static readonly Stack<ListContext> listContextStack = new Stack<ListContext>();
        private const int TabWidth = 4;

        private struct LineAnalysis
        {
            public int Indent;
            public int ContentPosition;
            public bool HasContent;
            public bool IsBlankLine;
        }

        public static INoInEdgeInOutVertexVertex MdStringToMdTokenVertexes_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (from == null || to == null)
                return exe.Stack;

            MdStringToTokenVertexes_Process(GraphUtil.GetStringValue(from), to);

            return exe.Stack;
        }

        public static void MdStringToTokenVertexes_Process(string md, IVertex to)
        {
            if (string.IsNullOrEmpty(md))
                return;

            // Reset state variables
            isInsideBold = false;
            isInsideItalic = false;
            isInsideCodeBlock = false;
            isInsideInlineCode = false;
            isInsideLink = false;
            isInsideImage = false;
            isInsideTable = false;
            blockquoteLevel = 0;
            currentHeaderLevel = 0;
            codeBlockContent = null;
            codeBlockLanguageName = null;
            listContextStack.Clear();

            int position = 0;

            while (position < md.Length)
            {
                char currentChar = md[position];
                
                // If inside code block, only process closing ``` or extract text
                if (isInsideCodeBlock)
                {
                    // Check for code block end (```)
                    if (IsCodeBlockEnd(md, position))
                    {
                        position += 3; // Skip closing ```
                        isInsideCodeBlock = false;
                        
                        // Add CodeBlock token with collected content
                        string content = codeBlockContent != null ? codeBlockContent.ToString() : "";
                        
                        // Always use FormalTextLanguageName (empty string if not specified)
                        // This ensures HTML generator creates <pre><code> wrapper
                        string languageName = codeBlockLanguageName ?? "";
                        AddCodeBlockToTarget_withFormalTextLanguageName(to, content, languageName);
                        
                        codeBlockContent = null;
                        codeBlockLanguageName = null;
                        continue;
                    }
                    // Otherwise, collect text until we find closing ```
                    if (codeBlockContent != null)
                    {
                        codeBlockContent.Append(md[position]);
                    }
                    position++;
                    continue;
                }
                
                // If inside inline code, only process closing ` or extract text
                if (isInsideInlineCode)
                {
                    // Check for inline code end (`)
                    if (IsInlineCodeEnd(md, position))
                    {
                        position++; // Skip closing `
                        isInsideInlineCode = false;
                        AddTokenToTarget(to, InlineCodeEnd);
                        continue;
                    }
                    // Otherwise, extract text until we find closing `
                    string inlineCodeText = ExtractInlineCodeText(md, ref position);
                    if (!string.IsNullOrEmpty(inlineCodeText))
                    {
                        AddTextTokenToTarget(to, inlineCodeText);
                    }
                    continue;
                }
                
                // Handle hard breaks (two or more spaces at end of line)
                if (IsHardBreak(md, position))
                {
                    CloseHeaderIfOpen(to);
                    ExtractHardBreak(md, ref position);
                    AddTokenToTarget(to, HardBreak);
                    continue;
                }
                
                // Handle paragraph breaks (double newline = empty line)
                if (IsParagraphBreak(md, position))
                {
                    CloseHeaderIfOpen(to);
                    ExtractParagraphBreak(md, ref position);

                    LineAnalysis paragraphAnalysis = AnalyzeLine(md, position);
                    bool listContinues = false;
                    bool shouldContinueListItem = false;

                    if (listContextStack.Count > 0)
                    {
                        if (paragraphAnalysis.HasContent)
                        {
                            // Check if this is a new list item
                            bool isNewListItem = TryGetListItem(md, paragraphAnalysis.ContentPosition, out _, out _, out _);
                            
                            if (isNewListItem)
                            {
                                // It's a new list item, close lists to its indent level
                                CloseListsToIndent(to, paragraphAnalysis.Indent);
                                listContinues = true;
                            }
                            else
                            {
                                // Not a new list item - check if it should continue current list
                                ListContext current = listContextStack.Peek();
                                
                                if (paragraphAnalysis.Indent > current.Indent)
                                {
                                    // Text with indent greater than list indent - continuation of list item
                                    listContinues = true;
                                    shouldContinueListItem = true;
                                }
                                else if (ShouldStayInCurrentList(paragraphAnalysis.Indent))
                                {
                                    // Text with same or compatible indent - list continues
                                    listContinues = true;
                                }
                                else
                                {
                                    // Text with smaller indent - close lists
                                    // But don't close if we're continuing a list item (shouldn't happen here, but be safe)
                                    if (!shouldContinueListItem)
                                    {
                                        CloseListsToIndent(to, paragraphAnalysis.Indent);
                                    }
                                }
                            }
                        }
                        else
                        {
                            listContinues = true;
                        }
                    }

                    if (paragraphAnalysis.HasContent)
                    {
                        position = paragraphAnalysis.ContentPosition;
                    }

                    // Only close lists if we're not continuing a list item
                    // (shouldContinueListItem means we want to keep the list open)
                    if (listContextStack.Count > 0 && !listContinues && !shouldContinueListItem)
                    {
                        CloseAllLists(to);
                    }

                    // Close blockquote if next line doesn't start with >
                    if (blockquoteLevel > 0 && paragraphAnalysis.HasContent)
                    {
                        bool nextLineIsBlockquote = paragraphAnalysis.ContentPosition < md.Length && 
                                                    md[paragraphAnalysis.ContentPosition] == '>';
                        
                        if (!nextLineIsBlockquote)
                        {
                            // Close all blockquote levels before the ParagraphBreak
                            while (blockquoteLevel > 0)
                            {
                                blockquoteLevel--;
                                AddTokenToTarget(to, BlockquoteEnd);
                            }
                        }
                    }

                    // Generate ParagraphBreak for empty line (unless we're continuing a list item)
                    if (!shouldContinueListItem)
                    {
                        AddTokenToTarget(to, ParagraphBreak);
                    }
                    
                    // If this is indented text continuing a list item, add HardBreak and process the text
                    // directly as part of the list item (don't return to main loop yet)
                    if (shouldContinueListItem && paragraphAnalysis.HasContent)
                    {
                        // Add two HardBreaks to match paragraph break effect (empty line = two breaks)
                        AddTokenToTarget(to, HardBreak);
                        AddTokenToTarget(to, HardBreak);
                        // Position is already set to paragraphAnalysis.ContentPosition above (after tab)
                        // Process the text line by line until we hit a newline or end of string
                        // This ensures the text is processed within the list item context
                        int lineEnd = position;
                        while (lineEnd < md.Length && md[lineEnd] != '\n' && md[lineEnd] != '\r')
                        {
                            lineEnd++;
                        }
                        
                        // Process the text from current position to line end
                        while (position < lineEnd)
                        {
                            // Process inline markdown formatting (bold, italic, links, etc.)
                            char contChar = md[position];
                            
                            // Handle bold text (**text** or __text__)
                            if (IsBoldStart(md, position))
                            {
                                position += 2;
                                isInsideBold = true;
                                AddTokenToTarget(to, BoldStart);
                                continue;
                            }
                            
                            if (IsBoldEnd(md, position))
                            {
                                position += 2;
                                isInsideBold = false;
                                AddTokenToTarget(to, BoldEnd);
                                continue;
                            }
                            
                            // Handle italic text (*text* or _text_)
                            if (IsItalicStart(md, position))
                            {
                                position++;
                                isInsideItalic = true;
                                AddTokenToTarget(to, ItalicStart);
                                continue;
                            }
                            
                            if (IsItalicEnd(md, position))
                            {
                                position++;
                                isInsideItalic = false;
                                AddTokenToTarget(to, ItalicEnd);
                                continue;
                            }
                            
                            // Handle inline code (`code`)
                            if (IsInlineCodeStart(md, position))
                            {
                                position++;
                                isInsideInlineCode = true;
                                AddTokenToTarget(to, InlineCodeStart);
                                continue;
                            }
                            
                            if (isInsideInlineCode && IsInlineCodeEnd(md, position))
                            {
                                position++;
                                isInsideInlineCode = false;
                                AddTokenToTarget(to, InlineCodeEnd);
                                continue;
                            }
                            
                            // Handle links [text](url)
                            if (IsLinkStart(md, position))
                            {
                                ExtractLink(md, ref position, to);
                                continue;
                            }
                            
                            // Handle regular text
                            string continuationText = ExtractRegularText(md, ref position);
                            if (!string.IsNullOrEmpty(continuationText))
                            {
                                AddTextTokenToTarget(to, continuationText);
                            }
                            else
                            {
                                if (position < lineEnd)
                                {
                                    position++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        
                        // Don't skip the newline - let the main loop handle it
                        // This ensures empty lines after continuation text are properly detected
                        // as ParagraphBreak (for proper <BR><BR> before next list item or continuation)
                        continue;
                    }
                    
                    continue;
                }
                
                // Handle single newlines (generate HardBreak if next line has content)
                if (currentChar == '\n')
                {
                    CloseHeaderIfOpen(to);
                    int lookaheadPosition = position + 1;
                    LineAnalysis lineAnalysis = AnalyzeLine(md, lookaheadPosition);

                    position++;

                    // Check if we need to close blockquote (next line doesn't start with >)
                    if (blockquoteLevel > 0 && lineAnalysis.HasContent)
                    {
                        // Check if the content position starts with > (blockquote continues)
                        bool nextLineIsBlockquote = lineAnalysis.ContentPosition < md.Length && 
                                                    md[lineAnalysis.ContentPosition] == '>';
                        
                        if (!nextLineIsBlockquote)
                        {
                            // Close all blockquote levels before the HardBreak for the next line
                            while (blockquoteLevel > 0)
                            {
                                blockquoteLevel--;
                                AddTokenToTarget(to, BlockquoteEnd);
                            }
                            
                            // Generate HardBreak for single newline when next line has content (after BlockquoteEnd)
                            AddTokenToTarget(to, HardBreak);
                            position = lineAnalysis.ContentPosition;
                            continue;
                        }
                    }

                    if (listContextStack.Count > 0 && lineAnalysis.HasContent)
                    {
                        // Check if this is a new list item first
                        if (TryGetListItem(md, lineAnalysis.ContentPosition, out _, out _, out _))
                        {
                            CloseListsToIndent(to, lineAnalysis.Indent);
                            position = lineAnalysis.ContentPosition;
                            continue;
                        }

                        // Check if this is continuation of current list item (indent greater than list indent)
                        ListContext current = listContextStack.Peek();
                        if (lineAnalysis.Indent > current.Indent)
                        {
                            // This is continuation of list item - don't close lists, just add HardBreak
                            AddTokenToTarget(to, HardBreak);
                            position = lineAnalysis.ContentPosition;
                            continue;
                        }

                        // Check if list should stay open
                        if (ShouldStayInCurrentList(lineAnalysis.Indent))
                        {
                            AddTokenToTarget(to, HardBreak);
                            position = lineAnalysis.ContentPosition;
                            continue;
                        }

                        // Otherwise, close lists as needed
                        if (lineAnalysis.Indent == 0)
                        {
                            CloseAllLists(to);
                        }
                        else
                        {
                            CloseListsToIndent(to, lineAnalysis.Indent);
                            if (!ShouldStayInCurrentList(lineAnalysis.Indent))
                            {
                                CloseAllLists(to);
                            }
                        }

                        // Generate HardBreak for single newline when next line has content
                        AddTokenToTarget(to, HardBreak);
                        position = lineAnalysis.ContentPosition;
                        continue;
                    }

                    if (lineAnalysis.HasContent)
                    {
                        // Generate HardBreak for single newline when next line has content
                        AddTokenToTarget(to, HardBreak);
                        position = lineAnalysis.ContentPosition;
                        continue;
                    }

                    continue;
                }
                
                // Handle carriage returns (just advance position)
                if (currentChar == '\r')
                {
                    CloseHeaderIfOpen(to);
                    position++;
                    if (position < md.Length && md[position] == '\n')
                    {
                        continue;
                    }
                    continue;
                }

                // Handle horizontal rules (--- or ***)
                if (IsHorizontalRule(md, position))
                {
                    ExtractHorizontalRule(md, ref position);
                    AddTokenToTarget(to, HorizontalRule);
                    continue;
                }
                
                // Handle headers (# ## ### etc.) - only at the start of a line
                if (currentChar == '#' && IsAtLineStartOrIndented(md, position))
                {
                    int headerLevel = CountConsecutiveChars(md, position, '#');
                    if (headerLevel > 0 && headerLevel <= 6)
                    {
                        position += headerLevel;

                        switch (headerLevel)
                        {
                            case 1:
                                AddTokenToTarget(to, Header1Start);
                                break;
                            case 2:
                                AddTokenToTarget(to, Header2Start);
                                break;
                            case 3:
                                AddTokenToTarget(to, Header3Start);
                                break;
                            case 4:
                                AddTokenToTarget(to, Header4Start);
                                break;
                            case 5:
                                AddTokenToTarget(to, Header5Start);
                                break;
                            case 6:
                                AddTokenToTarget(to, Header6Start);
                                break;
                        }
                        
                        currentHeaderLevel = headerLevel;
                        
                        SkipWhitespace(md, ref position);
                        continue;
                    }
                }
                
                // Handle bold text (**text** or __text__)
                if (IsBoldStart(md, position))
                {
                    position += 2; // Skip opening markers
                    isInsideBold = true;
                    AddTokenToTarget(to, BoldStart);
                    continue;
                }
                
                // Handle bold end (** or __)
                if (IsBoldEnd(md, position))
                {
                    position += 2; // Skip closing markers
                    isInsideBold = false;
                    AddTokenToTarget(to, BoldEnd);
                    continue;
                }
                
                // Handle italic text (*text* or _text_)
                if (IsItalicStart(md, position))
                {
                    position++; // Skip opening marker
                    isInsideItalic = true;
                    AddTokenToTarget(to, ItalicStart);
                    continue;
                }
                
                // Handle italic end (* or _)
                if (IsItalicEnd(md, position))
                {
                    position++; // Skip closing marker
                    isInsideItalic = false;
                    AddTokenToTarget(to, ItalicEnd);
                    continue;
                }
                
                // Handle code blocks (```code```)
                if (IsCodeBlockStart(md, position))
                {
                    position += 3; // Skip opening ```
                    isInsideCodeBlock = true;
                    codeBlockContent = new StringBuilder();
                    codeBlockLanguageName = null;
                    
                    // Check if there's text immediately after ``` (before newline)
                    int tempPos = position;
                    StringBuilder languageNameBuilder = new StringBuilder();
                    
                    while (tempPos < md.Length && md[tempPos] != '\n' && md[tempPos] != '\r')
                    {
                        languageNameBuilder.Append(md[tempPos]);
                        tempPos++;
                    }
                    
                    string potentialLanguageName = languageNameBuilder.ToString().Trim();
                    if (!string.IsNullOrEmpty(potentialLanguageName))
                    {
                        // There's language name, use it and start content from next line
                        codeBlockLanguageName = potentialLanguageName;
                        position = tempPos;
                    }
                    else
                    {
                        // No language name, but still skip to next line to avoid empty line
                        position = tempPos;
                    }
                    
                    // Skip newline to start content from next line (don't start from empty line)
                    if (position < md.Length && md[position] == '\r')
                    {
                        position++;
                    }
                    if (position < md.Length && md[position] == '\n')
                    {
                        position++;
                    }
                    continue;
                }
                
                // Handle inline code (`code`)
                if (IsInlineCodeStart(md, position))
                {
                    position++; // Skip opening `
                    isInsideInlineCode = true;
                    AddTokenToTarget(to, InlineCodeStart);
                    continue;
                }
                
                // Note: Code block end and inline code end are handled at the beginning
                // of the loop when inside code blocks to prevent markdown processing
                
                // Handle links [text](url)
                if (IsLinkStart(md, position))
                {
                    ExtractLink(md, ref position, to);
                    continue;
                }
                
                // Handle generic elements !ELEMENT_NAME[param1](value1)[param2](value2)...
                if (IsGenericElementStart(md, position))
                {
                    ExtractGenericElement(md, ref position, to);
                    continue;
                }

                // Handle images ![alt](url) or !(url)
                if (IsImageStart(md, position))
                {
                    ExtractImage(md, ref position, to);
                    continue;
                }
                
                // Handle list items (-, +, *, or ordered lists)
                if (TryGetListItem(md, position, out int markerPosition, out int listIndent, out ListType listType))
                {
                    position = markerPosition;
                    ExtractListItem(md, ref position, to, listIndent, listType);
                    continue;
                }
                
                // Handle blockquotes (> text)
                if (currentChar == '>' && IsBlockquoteStart(md, position))
                {
                    // Count > symbols (with optional spaces between them) to determine level
                    int newLevel = 0;
                    int i = position;
                    while (i < md.Length && md[i] == '>')
                    {
                        newLevel++;
                        i++;
                        // Skip optional space after >
                        if (i < md.Length && md[i] == ' ')
                        {
                            i++;
                        }
                    }
                    
                    // If we're going to a higher level, add BlockquoteStart tokens
                    while (blockquoteLevel < newLevel)
                    {
                        blockquoteLevel++;
                        AddTokenToTarget(to, BlockquoteStart);
                    }
                    
                    // If we're going to a lower level, add BlockquoteEnd tokens
                    while (blockquoteLevel > newLevel)
                    {
                        blockquoteLevel--;
                        AddTokenToTarget(to, BlockquoteEnd);
                    }
                    
                    position = i; // Skip all > symbols and spaces
                    
                    // After processing blockquote, check if there's a list item on the same line
                    if (TryGetListItem(md, position, out int blockquoteListMarkerPosition, out int blockquoteListIndent, out ListType blockquoteListType))
                    {
                        position = blockquoteListMarkerPosition;
                        ExtractListItem(md, ref position, to, blockquoteListIndent, blockquoteListType);
                        continue;
                    }
                    
                    continue;
                }
                
                
                // Handle tables (| col1 | col2 |)
                if (currentChar == '|' && !isInsideTable)
                {
                    isInsideTable = true;
                    ExtractTable(md, ref position, to);
                    isInsideTable = false;
                    continue;
                }
                
                // Check if we're in a list context and text starts with indent greater than list indent
                // This handles continuation of list items after empty lines (e.g., tab-indented text)
                // Only check at the start of a line (after newline or at beginning of string)
                if (listContextStack.Count > 0 && IsAtLineStart(md, position))
                {
                    LineAnalysis textAnalysis = AnalyzeLine(md, position);
                    if (textAnalysis.HasContent)
                    {
                        // Check if this is not a new list item
                        if (!TryGetListItem(md, textAnalysis.ContentPosition, out _, out _, out _))
                        {
                            ListContext current = listContextStack.Peek();
                            // If text has indent greater than list indent, it's continuation of list item
                            if (textAnalysis.Indent > current.Indent)
                            {
                                // Not a new list item, so treat as continuation
                                AddTokenToTarget(to, HardBreak);
                                position = textAnalysis.ContentPosition;
                                // Continue processing - text will be handled as regular text within list item
                                continue;
                            }
                        }
                    }
                }
                
                // Handle regular text
                string regularText = ExtractRegularText(md, ref position);
                if (!string.IsNullOrEmpty(regularText))
                {
                    AddTextTokenToTarget(to, regularText);
                }
                else
                {
                    // If no text extracted, advance position to avoid infinite loop
                    if (position < md.Length)
                    {
                        position++;
                    }
                    else
                    {
                        break; // End of string
                    }
                }
            }
            
            CloseAllLists(to);
            CloseHeaderIfOpen(to);
            
            // Close all remaining blockquote levels at the end
            while (blockquoteLevel > 0)
            {
                blockquoteLevel--;
                AddTokenToTarget(to, BlockquoteEnd);
            }
        }

        private static void AddTokenToTarget(IVertex target, IVertex tokenType)
        {        
            target.AddVertex(tokenType, "");            
        }

        private static void AddCodeBlockToTarget(IVertex target, string text)
        {
            target.AddVertex(CodeBlock, HtmlUtil.QuoteString(text));
        }

        private static void AddCodeBlockToTarget_withFormalTextLanguageName(IVertex target, string text, string FormalTextLanguageName_value)
        {
            IVertex v = target.AddVertex(CodeBlock, HtmlUtil.QuoteString(text));
            v.AddVertex(FormalTextLanguageName, FormalTextLanguageName_value);
        }        

        private static void AddTextTokenToTarget(IVertex target, string textValue)
        {            
            target.AddVertex(Text, HtmlUtil.QuoteString(textValue));
        }

        private static void AddLinkTokenToTarget(IVertex target, string textValue, string urlValue)
        {
            IVertex v = target.AddVertex(Link, null);
            v.AddVertex(LinkText, textValue);
            v.AddVertex(LinkUrl, urlValue);
        }

        private static void AddImageTokenToTarget(IVertex target, string textValue, string urlValue)
        {
            IVertex v = target.AddVertex(Image, null);
            v.AddVertex(ImageText, textValue);
            v.AddVertex(ImageUrl, urlValue);
        }

        private static void AddImageTokenToTarget(IVertex target, string urlValue)
        {
            IVertex v = target.AddVertex(Image, null);
            v.AddVertex(ImageUrl, urlValue);
        }

        private static void AddGenericTokenToTarget(IVertex target, string genericElementName, IList<StringKeyValue> parameters)
        {
            IVertex v = target.AddVertex(Generic, genericElementName);
            foreach (var param in parameters)
            {
                IVertex nv = v.AddVertex(GenericNameValue, null);
                nv.AddVertex(GenericName, param.Key);
                nv.AddVertex(GenericValue, param.Value);
            }
        }



        private static void CloseHeaderIfOpen(IVertex target)
        {
            if (currentHeaderLevel == 0)
            {
                return;
            }

            switch (currentHeaderLevel)
            {
                case 1:
                    AddTokenToTarget(target, Header1End);
                    break;
                case 2:
                    AddTokenToTarget(target, Header2End);
                    break;
                case 3:
                    AddTokenToTarget(target, Header3End);
                    break;
                case 4:
                    AddTokenToTarget(target, Header4End);
                    break;
                case 5:
                    AddTokenToTarget(target, Header5End);
                    break;
                case 6:
                    AddTokenToTarget(target, Header6End);
                    break;
            }

            currentHeaderLevel = 0;
        }

        private static void CloseCurrentList(IVertex target)
        {
            if (listContextStack.Count == 0)
            {
                return;
            }

            ListContext context = listContextStack.Pop();

            switch (context.Type)
            {
                case ListType.Unordered:
                    AddTokenToTarget(target, ListItemsEnd);
                    break;
                case ListType.Ordered:
                    AddTokenToTarget(target, OrderedListItemsEnd);
                    break;
            }
        }

        private static void CloseAllLists(IVertex target)
        {
            while (listContextStack.Count > 0)
            {
                CloseCurrentList(target);
            }
        }

        private static void OpenList(IVertex target, ListType type, int indent)
        {
            switch (type)
            {
                case ListType.Unordered:
                    AddTokenToTarget(target, ListItemsStart);
                    break;
                case ListType.Ordered:
                    AddTokenToTarget(target, OrderedListItemsStart);
                    break;
                default:
                    return;
            }

            listContextStack.Push(new ListContext { Type = type, Indent = indent });
        }

        private static void EnsureListContext(IVertex target, ListType desired, int indent)
        {
            if (desired == ListType.None)
            {
                return;
            }

            while (listContextStack.Count > 0 && indent < listContextStack.Peek().Indent)
            {
                CloseCurrentList(target);
            }

            if (listContextStack.Count == 0)
            {
                OpenList(target, desired, indent);
                return;
            }

            if (indent > listContextStack.Peek().Indent)
            {
                OpenList(target, desired, indent);
                return;
            }

            if (listContextStack.Peek().Type != desired)
            {
                CloseCurrentList(target);
                OpenList(target, desired, indent);
            }
        }

        private static bool TryGetListItem(string md, int position, out int markerPosition, out int indent, out ListType type)
        {
            markerPosition = position;
            indent = 0;
            type = ListType.None;

            if (position >= md.Length)
            {
                return false;
            }

            int lineStart = position;
            while (lineStart > 0 && md[lineStart - 1] != '\n' && md[lineStart - 1] != '\r')
            {
                lineStart--;
            }

            int temp = lineStart;
            int localIndent = 0;
            while (temp < md.Length && (md[temp] == ' ' || md[temp] == '\t'))
            {
                localIndent += md[temp] == '\t' ? TabWidth : 1;
                temp++;
            }

            // Skip blockquote markers (> and optional space after)
            while (temp < md.Length && md[temp] == '>')
            {
                temp++;
                // Skip optional space after >
                if (temp < md.Length && md[temp] == ' ')
                {
                    temp++;
                }
            }

            if (temp >= md.Length)
            {
                return false;
            }

            if (position > temp)
            {
                return false;
            }

            if (!IsListItem(md, temp))
            {
                return false;
            }

            markerPosition = temp;
            indent = localIndent;
            type = char.IsDigit(md[temp]) ? ListType.Ordered : ListType.Unordered;
            return true;
        }

        private static LineAnalysis AnalyzeLine(string md, int position)
        {
            LineAnalysis analysis = new LineAnalysis
            {
                Indent = 0,
                ContentPosition = position,
                HasContent = false,
                IsBlankLine = false
            };

            int idx = position;
            while (idx < md.Length)
            {
                char c = md[idx];
                if (c == ' ')
                {
                    analysis.Indent++;
                    idx++;
                    continue;
                }
                if (c == '\t')
                {
                    analysis.Indent += TabWidth;
                    idx++;
                    continue;
                }
                if (c == '\r')
                {
                    idx++;
                    continue;
                }
                if (c == '\n')
                {
                    analysis.IsBlankLine = true;
                    analysis.ContentPosition = idx;
                    return analysis;
                }

                analysis.HasContent = true;
                analysis.ContentPosition = idx;
                return analysis;
            }

            analysis.IsBlankLine = !analysis.HasContent;
            analysis.ContentPosition = idx;
            return analysis;
        }

        private static bool ShouldStayInCurrentList(int indent)
        {
            if (listContextStack.Count == 0)
            {
                return false;
            }

            ListContext current = listContextStack.Peek();

            if (indent > current.Indent)
            {
                return true;
            }

            if (indent == current.Indent && current.Indent > 0)
            {
                return true;
            }

            return false;
        }

        private static void CloseListsToIndent(IVertex target, int indent)
        {
            while (listContextStack.Count > 0 && listContextStack.Peek().Indent > indent)
            {
                CloseCurrentList(target);
            }
        }

        private static bool IsHorizontalRule(string md, int position)
        {
            if (position + 2 >= md.Length) return false;
            
            char first = md[position];
            if (first != '-' && first != '*' && first != '_') return false;
            
            int count = CountConsecutiveChars(md, position, first);
            return count >= 3;
        }

        private static string ExtractHorizontalRule(string md, ref int position)
        {
            char ruleChar = md[position];
            int count = CountConsecutiveChars(md, position, ruleChar);
            position += count;
            return new string(ruleChar, count);
        }

        private static int CountConsecutiveChars(string md, int position, char charToCount)
        {
            int count = 0;
            while (position + count < md.Length && md[position + count] == charToCount)
            {
                count++;
            }
            return count;
        }

        private static void SkipWhitespace(string md, ref int position)
        {
            while (position < md.Length && char.IsWhiteSpace(md[position]) && md[position] != '\n' && md[position] != '\r')
            {
                position++;
            }
        }

        private static string ExtractUntilNewline(string md, ref int position)
        {
            StringBuilder result = new StringBuilder();
            while (position < md.Length && md[position] != '\n' && md[position] != '\r')
            {
                result.Append(md[position]);
                position++;
            }
            return result.ToString().Trim();
        }

        private static bool IsBoldStart(string md, int position)
        {
            if (position + 1 >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false; // Don't process inside code blocks
            if (isInsideBold) return false; // Already inside bold
            return (md[position] == '*' && md[position + 1] == '*') ||
                   (md[position] == '_' && md[position + 1] == '_');
        }

        private static bool IsBoldEnd(string md, int position)
        {
            if (position + 1 >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false; // Don't process inside code blocks
            if (!isInsideBold) return false; // Not inside bold
            return (md[position] == '*' && md[position + 1] == '*') ||
                   (md[position] == '_' && md[position + 1] == '_');
        }

        private static bool IsItalicStart(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false; // Don't process inside code blocks
            if (isInsideItalic) return false; // Already inside italic
            char current = md[position];
            if (current == '*' && IsListItem(md, position)) return false;
            return (current == '*' || current == '_') && 
                   (position + 1 >= md.Length || md[position + 1] != current);
        }

        private static bool IsItalicEnd(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false; // Don't process inside code blocks
            if (!isInsideItalic) return false; // Not inside italic
            char current = md[position];
            if (current == '*' && IsListItem(md, position)) return false;
            return (current == '*' || current == '_') && 
                   (position + 1 >= md.Length || md[position + 1] != current);
        }   

        private static bool IsCodeBlockStart(string md, int position)
        {
            if (position + 2 >= md.Length) return false;
            if (isInsideCodeBlock) return false; // Already inside code block
            // Check for ``` - must have all three backticks
            if (md[position] != '`' || md[position + 1] != '`' || md[position + 2] != '`') return false;
            // After ```, next char should be newline or letter (language name) or end of string
            int afterBackticks = position + 3;
            if (afterBackticks >= md.Length) return true; // ``` at end of string
            char nextChar = md[afterBackticks];
            // Valid code block: ``` followed by newline, letter (language name), or space
            return nextChar == '\n' || nextChar == '\r' || char.IsLetter(nextChar) || nextChar == ' ';
        }

        private static bool IsCodeBlockEnd(string md, int position)
        {
            if (position + 2 >= md.Length) return false;
            if (!isInsideCodeBlock) return false; // Not inside code block
            return md[position] == '`' && md[position + 1] == '`' && md[position + 2] == '`';
        }

        private static bool IsInlineCodeStart(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideInlineCode) return false; // Already inside inline code
            return md[position] == '`';
        }

        private static bool IsInlineCodeEnd(string md, int position)
        {
            if (position >= md.Length) return false;
            if (!isInsideInlineCode) return false; // Not inside inline code
            return md[position] == '`';
        }
                
        private static string ExtractLink(string md, ref int position)
        {
            position++; // Skip opening [
            
            StringBuilder result = new StringBuilder();
            result.Append('[');
            
            while (position < md.Length)
            {
                if (md[position] == ']' && position + 1 < md.Length && md[position + 1] == '(')
                {
                    result.Append(']');
                    position++;
                    
                    // Extract URL part
                    position++; // Skip opening (
                    result.Append('(');
                    
                    while (position < md.Length && md[position] != ')')
                    {
                        result.Append(md[position]);
                        position++;
                    }
                    
                    if (position < md.Length)
                    {
                        result.Append(')');
                        position++;
                    }
                    break;
                }
                result.Append(md[position]);
                position++;
            }
            
            return result.ToString();
        }

        private static bool IsLinkStart(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false; // Don't process inside code blocks
            if (isInsideLink) return false; // Already inside link
            return md[position] == '[';
        }

        private static void ExtractLink(string md, ref int position, IVertex target)
        {
            position++; // Skip opening [
            
            // Extract link text
            StringBuilder textBuilder = new StringBuilder();
            while (position < md.Length && md[position] != ']')
            {
                textBuilder.Append(md[position]);
                position++;
            }
            
            if (position >= md.Length)
            {
                return; // Malformed link
            }
            
            position++; // Skip ]
            
            if (position >= md.Length || md[position] != '(')
            {
                return; // Malformed link
            }
            
            position++; // Skip (
            
            // Extract URL
            StringBuilder urlBuilder = new StringBuilder();
            while (position < md.Length && md[position] != ')')
            {
                urlBuilder.Append(md[position]);
                position++;
            }
            
            if (position < md.Length)
            {
                position++; // Skip )
            }
            
            AddLinkTokenToTarget(target, textBuilder.ToString(), urlBuilder.ToString());
        }

        private static bool IsImageStart(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false; // Don't process inside code blocks
            if (md[position] != '!') return false;
            
            // Check for ![text](url) format
            if (position + 1 < md.Length && md[position + 1] == '[')
                return true;
            
            // Check for !(url) format
            if (position + 1 < md.Length && md[position + 1] == '(')
                return true;
            
            return false;
        }

        private static void ExtractImage(string md, ref int position, IVertex target)
        {
            position++; // Skip !
            
            // Check if it's !(url) format (no alt text)
            if (md[position] == '(')
            {
                position++; // Skip (
                
                // Extract URL
                StringBuilder simpleUrlBuilder = new StringBuilder();
                while (position < md.Length && md[position] != ')')
                {
                    simpleUrlBuilder.Append(md[position]);
                    position++;
                }
                
                if (position < md.Length)
                {
                    position++; // Skip )
                }
                
                AddImageTokenToTarget(target, simpleUrlBuilder.ToString());
                return;
            }
            
            // It's ![text](url) format
            position++; // Skip [
            
            // Extract alt text
            StringBuilder textBuilder = new StringBuilder();
            while (position < md.Length && md[position] != ']')
            {
                textBuilder.Append(md[position]);
                position++;
            }
            
            if (position >= md.Length)
            {
                return; // Malformed image
            }
            
            position++; // Skip ]
            
            if (position >= md.Length || md[position] != '(')
            {
                return; // Malformed image
            }
            
            position++; // Skip (
            
            // Extract URL
            StringBuilder urlBuilder = new StringBuilder();
            while (position < md.Length && md[position] != ')')
            {
                urlBuilder.Append(md[position]);
                position++;
            }
            
            if (position < md.Length)
            {
                position++; // Skip )
            }
            
            AddImageTokenToTarget(target, textBuilder.ToString(), urlBuilder.ToString());
        }

        private static bool IsGenericElementStart(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false;
            if (md[position] != '!') return false;
            
            // Check if next char is uppercase letter (element name start)
            if (position + 1 >= md.Length) return false;
            char nextChar = md[position + 1];
            return char.IsUpper(nextChar);
        }

        private static bool IsBangTokenStart(string md, int position)
        {
            return IsGenericElementStart(md, position) || IsImageStart(md, position);
        }

        private static void ExtractGenericElement(string md, ref int position, IVertex target)
        {
            position++; // Skip !
            
            // Extract element name (until [ or end)
            StringBuilder nameBuilder = new StringBuilder();
            while (position < md.Length && md[position] != '[' && !char.IsWhiteSpace(md[position]))
            {
                nameBuilder.Append(md[position]);
                position++;
            }
            
            string elementName = nameBuilder.ToString();
            List<StringKeyValue> parameters = new List<StringKeyValue>();
            
            // Extract parameters [name](value) pairs
            while (position < md.Length && md[position] == '[')
            {
                position++; // Skip [
                
                // Extract parameter name
                StringBuilder paramNameBuilder = new StringBuilder();
                while (position < md.Length && md[position] != ']')
                {
                    paramNameBuilder.Append(md[position]);
                    position++;
                }
                
                if (position >= md.Length)
                {
                    break; // Malformed
                }
                
                position++; // Skip ]
                
                if (position >= md.Length || md[position] != '(')
                {
                    break; // Malformed
                }
                
                position++; // Skip (
                
                // Extract parameter value
                StringBuilder paramValueBuilder = new StringBuilder();
                while (position < md.Length && md[position] != ')')
                {
                    paramValueBuilder.Append(md[position]);
                    position++;
                }
                
                if (position < md.Length)
                {
                    position++; // Skip )
                }
                
                parameters.Add(new StringKeyValue(paramNameBuilder.ToString(), paramValueBuilder.ToString()));
            }
            
            AddGenericTokenToTarget(target, elementName, parameters);
        }

        private static bool IsImageEnd(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideCodeBlock || isInsideInlineCode) return false; // Don't process inside code blocks
            if (!isInsideImage) return false; // Not inside image
            return md[position] == ']' && position + 1 < md.Length && md[position + 1] == '(';
        }

        private static void ExtractImageEnd(string md, ref int position)
        {
            position++; // Skip ]
            position++; // Skip (
            
            // Skip URL until )
            while (position < md.Length && md[position] != ')')
            {
                position++;
            }
            
            if (position < md.Length)
            {
                position++; // Skip )
            }
        }

        private static bool IsAtLineStartOrIndented(string md, int position)
        {
            int idx = position - 1;

            while (idx >= 0 && (md[idx] == ' ' || md[idx] == '\t'))
            {
                idx--;
            }

            // Skip blockquote markers (> and optional space after) going backwards
            while (idx >= 0 && md[idx] == '>')
            {
                idx--;
                // Skip optional space before >
                if (idx >= 0 && md[idx] == ' ')
                {
                    idx--;
                }
            }

            return idx < 0 || md[idx] == '\n' || md[idx] == '\r';
        }
        
        private static bool IsAtLineStart(string md, int position)
        {
            if (position == 0)
                return true;
            
            int idx = position - 1;
            
            // Skip carriage return if present
            if (idx >= 0 && md[idx] == '\r')
            {
                idx--;
            }
            
            // Check if previous character is newline
            return idx >= 0 && md[idx] == '\n';
        }

        private static bool IsListItem(string md, int position)
        {
            if (position >= md.Length) return false;

            // Check for unordered list (- or * or +)
            if (md[position] == '-' || md[position] == '*' || md[position] == '+')
            {
                if (!IsAtLineStartOrIndented(md, position))
                {
                    return false;
                }

                return position + 1 >= md.Length || char.IsWhiteSpace(md[position + 1]);
            }

            // Check for ordered list (1. 2. etc.)
            if (char.IsDigit(md[position]))
            {
                if (!IsAtLineStartOrIndented(md, position))
                {
                    return false;
                }

                int i = position;
                while (i < md.Length && char.IsDigit(md[i]))
                {
                    i++;
                }
                return i < md.Length && md[i] == '.' && (i + 1 >= md.Length || char.IsWhiteSpace(md[i + 1]));
            }

            return false;
        }

        private static void ExtractListItem(string md, ref int position, IVertex to, int indent, ListType listType)
        {
            EnsureListContext(to, listType, indent);

            if (listType == ListType.Unordered)
            {
                AddTokenToTarget(to, ListItemStart);
                position++; // Skip marker (-, *, +)
            }
            else if (listType == ListType.Ordered)
            {
                AddTokenToTarget(to, OrderedListItemStart);
                while (position < md.Length && char.IsDigit(md[position]))
                {
                    position++;
                }
                if (position < md.Length && md[position] == '.')
                {
                    position++;
                }
            }

            // Skip whitespace after marker
            SkipWhitespace(md, ref position);
        }

        private static bool IsBlockquoteStart(string md, int position)
        {
            // First check: Don't treat > as blockquote if we're inside a code block or inline code (using state flags)
            if (isInsideCodeBlock || isInsideInlineCode)
            {
                return false;
            }
            
            // Second check: Check if we're inside inline code by scanning backwards for unclosed backticks
            // This handles cases like "The `(?<ANY>)` meta edge" where > is inside inline code
            // This is a backup check in case the state flag wasn't set yet
            if (IsInsideInlineCodeByScanning(md, position))
            {
                return false;
            }
            
            // Third check: Check if > is at the start of a line (after newline or at start of string, with optional spaces/tabs before)
            int idx = position - 1;
            
            // Skip whitespace (spaces and tabs) before the >
            while (idx >= 0 && (md[idx] == ' ' || md[idx] == '\t'))
            {
                idx--;
            }
            
            // Check if we're at the start of a line (after newline or at start of string)
            if (idx >= 0 && md[idx] != '\n' && md[idx] != '\r')
            {
                return false; // Not at start of line
            }
            
            return true;
        }
        
        private static bool IsInsideInlineCodeByScanning(string md, int position)
        {
            // Scan backwards from position to find if we're inside inline code
            // This is a backup check in case the state flag wasn't set yet
            int idx = position - 1;
            bool insideInlineCode = false;
            
            while (idx >= 0)
            {
                char c = md[idx];
                
                // If we hit a newline, stop scanning (inline code can't span lines)
                if (c == '\n' || c == '\r')
                {
                    break;
                }
                
                // Check for backtick
                if (c == '`')
                {
                    // Check if it's part of a code block (triple backtick) - skip it
                    // We check if previous two characters are also backticks
                    if (idx >= 2 && md[idx - 1] == '`' && md[idx - 2] == '`')
                    {
                        // Found triple backtick - skip all three
                        idx -= 3;
                        if (idx < 0) break;
                        continue;
                    }
                    
                    // It's a single backtick - toggle inline code state
                    insideInlineCode = !insideInlineCode;
                }
                
                idx--;
            }
            
            return insideInlineCode;
        }

        
        private static string ExtractCodeBlockText(string md, ref int position)
        {
            StringBuilder result = new StringBuilder();
            
            while (position < md.Length)
            {
                // Check if we found the closing ```
                if (position + 2 < md.Length && 
                    md[position] == '`' && 
                    md[position + 1] == '`' && 
                    md[position + 2] == '`')
                {
                    break; // Stop before the closing ```
                }
                
                result.Append(md[position]);
                position++;
            }
            
            return result.ToString();
        }
        
        private static string ExtractInlineCodeText(string md, ref int position)
        {
            StringBuilder result = new StringBuilder();
            
            while (position < md.Length)
            {
                // Check if we found the closing `
                if (md[position] == '`')
                {
                    break; // Stop before the closing `
                }
                
                result.Append(md[position]);
                position++;
            }
            
            return result.ToString();
        }
        
        private static string ExtractRegularText(string md, ref int position)
        {
            StringBuilder result = new StringBuilder();
            
            while (position < md.Length)
            {
                char current = md[position];
                
                // Stop at special characters that start other tokens
                if (current == '\n' || current == '\r' || 
                    current == '_' || current == '`' ||
                    current == '[' || current == '>' || current == '|' ||
                    (current == '#' && IsAtLineStartOrIndented(md, position)) ||
                    (current == '-' && IsListItem(md, position)) ||
                    (current == '+' && IsListItem(md, position)) ||
                    (char.IsDigit(current) && IsListItem(md, position)))
                {
                    break;
                }

                if (current == '!' && IsBangTokenStart(md, position))
                {
                    break;
                }

                if (current == '*')
                {
                    if (IsListItem(md, position) ||
                        IsBoldStart(md, position) ||
                        IsBoldEnd(md, position) ||
                        IsItalicStart(md, position) ||
                        IsItalicEnd(md, position))
                    {
                        break;
                    }
                }
                
                result.Append(current);
                position++;
            }
            
            return result.ToString();
        }

        private static void ExtractTable(string md, ref int position, IVertex to)
        {
            AddTokenToTarget(to, TableStart);
            
            List<string> alignments = new List<string>();
            int originalPosition = position;
            
            // First pass: find separator and process it
            int currentPos = position;
            
            while (currentPos < md.Length)
            {
                SkipWhitespace(md, ref currentPos);
                
                if (currentPos >= md.Length || md[currentPos] != '|')
                {
                    break;
                }
                
                if (IsTableSeparatorLine(md, currentPos))
                {
                    // Found separator, process it
                    position = currentPos;
                    alignments = ProcessTableSeparator(md, ref position, to);
                    break;
                }
                
                // Skip to next line
                while (currentPos < md.Length && md[currentPos] != '\n' && md[currentPos] != '\r')
                {
                    currentPos++;
                }
                
                // Skip the newline character(s)
                if (currentPos < md.Length && md[currentPos] == '\r')
                {
                    currentPos++;
                }
                if (currentPos < md.Length && md[currentPos] == '\n')
                {
                    currentPos++;
                }
                
                // Skip whitespace at beginning of next line
                while (currentPos < md.Length && char.IsWhiteSpace(md[currentPos]) && md[currentPos] != '\n' && md[currentPos] != '\r')
                {
                    currentPos++;
                }
            }
            
            // Reset position to start of table
            position = originalPosition;
            
            // Second pass: process all lines with alignments
            bool isFirstRow = true;
            while (position < md.Length)
            {
                // Skip leading whitespace
                SkipWhitespace(md, ref position);
                
                // Check if we're at a table row (starts with |)
                if (position >= md.Length || md[position] != '|')
                {
                    // Check if next line starts with | (continuation of table)
                    if (position < md.Length)
                    {
                        int nextLineStart = position;
                        // Skip to next line
                        while (nextLineStart < md.Length && md[nextLineStart] != '\n' && md[nextLineStart] != '\r')
                        {
                            nextLineStart++;
                        }
                        if (nextLineStart < md.Length && md[nextLineStart] == '\n')
                        {
                            nextLineStart++;
                        }
                        if (nextLineStart < md.Length && md[nextLineStart] == '\r')
                        {
                            nextLineStart++;
                        }
                        // Skip whitespace on next line
                        while (nextLineStart < md.Length && char.IsWhiteSpace(md[nextLineStart]) && md[nextLineStart] != '\n' && md[nextLineStart] != '\r')
                        {
                            nextLineStart++;
                        }
                        // Check if next line starts with |
                        if (nextLineStart < md.Length && md[nextLineStart] == '|')
                        {
                            position = nextLineStart;
                            continue;
                        }
                    }
                    break; // End of table
                }
                
                // Check if this is a separator line
                bool isSeparatorLine = IsTableSeparatorLine(md, position);
                
                if (isSeparatorLine)
                {
                    // Skip separator line (already processed)
                    SkipToNextLine(md, ref position);
                    continue;
                }
                else
                {
                    // Process regular table row
                    if (isFirstRow)
                    {
                        // Add HeaderBegin for the first row (header)
                        AddTokenToTarget(to, HeaderBegin);
                    }
                    
                    ProcessTableRow(md, ref position, to, isFirstRow, alignments);
                    
                    if (isFirstRow)
                    {
                        // Add HeaderEnd after processing the first row (header)
                        AddTokenToTarget(to, HeaderEnd);
                    }
                    
                    isFirstRow = false;
                    // Skip to next line after processing row
                    SkipToNextLine(md, ref position);
                }
            }
            
            AddTokenToTarget(to, TableEnd);
        }
        
        private static void SkipToNextLine(string md, ref int position)
        {
            // Skip to next line
            while (position < md.Length && md[position] != '\n' && md[position] != '\r')
            {
                position++;
            }
            if (position < md.Length && md[position] == '\n')
            {
                position++;
            }
            if (position < md.Length && md[position] == '\r')
            {
                position++;
            }
        }
        
        private static bool IsTableSeparatorLine(string md, int position)
        {
            int startPos = position;
            bool hasDash = false;
            StringBuilder debugLine = new StringBuilder();
            
            while (position < md.Length && md[position] != '\n' && md[position] != '\r')
            {
                char c = md[position];
                debugLine.Append(c);
                if (c == '-')
                {
                    hasDash = true;
                }
                if (c != '|' && c != '-' && c != ':' && c != ' ')
                {
                    position = startPos; // Reset position
                    return false;
                }
                position++;
            }
            position = startPos; // Reset position
            
            // Debug output - we can't call AddTokenToTarget here without a target
            // Let's just return the result for now
            
            return hasDash; // Must have at least one dash to be a separator
        }
        
        private static List<string> ProcessTableSeparator(string md, ref int position, IVertex to)
        {
            List<string> alignments = new List<string>();
            int tempPosition = position;
            
            // Skip opening |
            tempPosition++;
            
            while (tempPosition < md.Length && md[tempPosition] != '\n' && md[tempPosition] != '\r')
            {
                // Skip whitespace
                while (tempPosition < md.Length && char.IsWhiteSpace(md[tempPosition]) && md[tempPosition] != '\n' && md[tempPosition] != '\r')
                {
                    tempPosition++;
                }
                
                if (tempPosition >= md.Length || md[tempPosition] == '\n' || md[tempPosition] == '\r')
                    break;
                    
                // Extract alignment for this column
                string alignment = ExtractColumnAlignment(md, ref tempPosition);
                alignments.Add(alignment);
                
                // Skip to next |
                while (tempPosition < md.Length && md[tempPosition] != '|' && md[tempPosition] != '\n' && md[tempPosition] != '\r')
                {
                    tempPosition++;
                }
                
                if (tempPosition < md.Length && md[tempPosition] == '|')
                {
                    tempPosition++; // Skip |
                }
            }
            
            // Update position to end of separator line
            position = tempPosition;
            
            return alignments;
        }
        
        private static string ExtractColumnAlignment(string md, ref int position)
        {
            int startPos = position;
            bool hasLeftColon = false;
            bool hasRightColon = false;
            int dashCount = 0;
            
            // First, skip any leading spaces
            while (position < md.Length && md[position] == ' ')
            {
                position++;
            }
            
            // Check if first non-space character is a colon (left alignment)
            if (position < md.Length && md[position] == ':')
            {
                hasLeftColon = true;
                position++;
            }
            
            // Count dashes
            while (position < md.Length && md[position] == '-')
            {
                dashCount++;
                position++;
            }
            
            // Check if there's a colon after dashes (right alignment)
            if (position < md.Length && md[position] == ':')
            {
                hasRightColon = true;
                position++;
            }
            
            // Skip any trailing spaces
            while (position < md.Length && md[position] == ' ')
            {
                position++;
            }
            
            // Check if we have at least 3 dashes
            if (dashCount < 3)
                return "justify"; // default if not enough dashes
            
            if (hasLeftColon && hasRightColon)
                return "center";
            else if (hasLeftColon)
                return "left";
            else if (hasRightColon)
                return "right";
            else
                return "justify"; // default
        }
        
        private static void ProcessTableRow(string md, ref int position, IVertex to, bool isFirstRow, List<string> alignments)
        {
            if (!isFirstRow)
            {
                AddTokenToTarget(to, RowBegin);
            }
            
            // Skip opening |
            position++;
            
            int columnIndex = 0;
            while (position < md.Length && md[position] != '\n' && md[position] != '\r')
            {
                SkipWhitespace(md, ref position);
                
                if (position >= md.Length || md[position] == '\n' || md[position] == '\r')
                    break;
                
                // Process cell content
                ProcessTableCell(md, ref position, to, isFirstRow, alignments, columnIndex);
                columnIndex++;
                
                // Skip to next |
                while (position < md.Length && md[position] != '|' && md[position] != '\n' && md[position] != '\r')
                {
                    position++;
                }
                
                if (position < md.Length && md[position] == '|')
                {
                    position++; // Skip |
                }
            }
            
            if (!isFirstRow)
            {
                AddTokenToTarget(to, RowEnd);
            }
        }
        
        private static void ProcessTableCell(string md, ref int position, IVertex to, bool isFirstRow, List<string> alignments, int columnIndex)
        {
            if (isFirstRow)
            {
                AddTokenToTarget(to, HeaderColumnBegin);
            }
            else
            {
                AddTokenToTarget(to, CellBegin);
            }
            
            // Add alignment token for both header and regular cells
            if (columnIndex < alignments.Count)
            {
                string alignment = alignments[columnIndex];
                if (alignment == "left")
                {
                    AddTokenToTarget(to, AlignLeft);
                }
                else if (alignment == "center")
                {
                    AddTokenToTarget(to, AlignCenter);
                }
                else if (alignment == "right")
                {
                    AddTokenToTarget(to, AlignRight);
                }
                else if (alignment == "justify")
                {
                    AddTokenToTarget(to, AlignJustifty);
                }
            }
            
            // Extract cell content boundaries
            int cellStart = position;
            int cellEnd = position;
            while (cellEnd < md.Length && md[cellEnd] != '|' && md[cellEnd] != '\n' && md[cellEnd] != '\r')
            {
                cellEnd++;
            }
            
            // Process cell content for inline markdown formatting
            ProcessInlineMarkdown(md, cellStart, cellEnd, to);
            
            // Update position to end of cell
            position = cellEnd;
            
            if (isFirstRow)
            {
                AddTokenToTarget(to, HeaderColumnEnd);
            }
            else
            {
                AddTokenToTarget(to, CellEnd);
            }
        }
        
        private static void ProcessInlineMarkdown(string md, int startPos, int endPos, IVertex to)
        {
            // Save current state
            bool savedIsInsideBold = isInsideBold;
            bool savedIsInsideItalic = isInsideItalic;
            bool savedIsInsideInlineCode = isInsideInlineCode;
            bool savedIsInsideLink = isInsideLink;
            
            // Reset state for cell processing
            isInsideBold = false;
            isInsideItalic = false;
            isInsideInlineCode = false;
            isInsideLink = false;
            
            int pos = startPos;
            
            // Skip leading whitespace
            while (pos < endPos && char.IsWhiteSpace(md[pos]))
            {
                pos++;
            }
            
            // Skip trailing whitespace
            int contentEnd = endPos;
            while (contentEnd > pos && char.IsWhiteSpace(md[contentEnd - 1]))
            {
                contentEnd--;
            }
            
            while (pos < contentEnd)
            {
                char currentChar = md[pos];
                
                // Handle inline code
                if (isInsideInlineCode)
                {
                    if (IsInlineCodeEnd(md, pos))
                    {
                        pos++;
                        isInsideInlineCode = false;
                        AddTokenToTarget(to, InlineCodeEnd);
                        continue;
                    }
                    string inlineCodeText = ExtractInlineCodeText(md, ref pos);
                    if (!string.IsNullOrEmpty(inlineCodeText))
                    {
                        AddTextTokenToTarget(to, inlineCodeText);
                    }
                    continue;
                }
                
                // Handle bold text (**text** or __text__)
                if (IsBoldStart(md, pos))
                {
                    pos += 2;
                    isInsideBold = true;
                    AddTokenToTarget(to, BoldStart);
                    continue;
                }
                
                if (IsBoldEnd(md, pos))
                {
                    pos += 2;
                    isInsideBold = false;
                    AddTokenToTarget(to, BoldEnd);
                    continue;
                }
                
                // Handle italic text (*text* or _text_)
                if (IsItalicStart(md, pos))
                {
                    pos++;
                    isInsideItalic = true;
                    AddTokenToTarget(to, ItalicStart);
                    continue;
                }
                
                if (IsItalicEnd(md, pos))
                {
                    pos++;
                    isInsideItalic = false;
                    AddTokenToTarget(to, ItalicEnd);
                    continue;
                }
                
                // Handle inline code (`code`)
                if (IsInlineCodeStart(md, pos))
                {
                    pos++;
                    isInsideInlineCode = true;
                    AddTokenToTarget(to, InlineCodeStart);
                    continue;
                }
                
                // Handle links [text](url)
                if (IsLinkStart(md, pos))
                {
                    int posBeforeLink = pos;
                    ExtractLink(md, ref pos, to);
                    // Ensure we don't go beyond cell boundary
                    if (pos > contentEnd)
                    {
                        pos = contentEnd;
                    }
                    continue;
                }
                
                // Handle regular text
                string regularText = ExtractRegularTextInCell(md, ref pos, contentEnd);
                if (!string.IsNullOrEmpty(regularText))
                {
                    AddTextTokenToTarget(to, regularText);
                }
                else
                {
                    if (pos < contentEnd)
                    {
                        pos++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            // Restore state
            isInsideBold = savedIsInsideBold;
            isInsideItalic = savedIsInsideItalic;
            isInsideInlineCode = savedIsInsideInlineCode;
            isInsideLink = savedIsInsideLink;
        }
        
        private static string ExtractRegularTextInCell(string md, ref int position, int endPos)
        {
            StringBuilder result = new StringBuilder();
            
            while (position < endPos)
            {
                char current = md[position];
                
                // Stop at special characters that start other tokens
                if (current == '*' || current == '_' || current == '`' || current == '[')
                {
                    // Check if it's actually a markdown token
                    if (IsBoldStart(md, position) || IsBoldEnd(md, position) ||
                        IsItalicStart(md, position) || IsItalicEnd(md, position) ||
                        IsInlineCodeStart(md, position) || IsLinkStart(md, position))
                    {
                        break;
                    }
                }
                
                result.Append(current);
                position++;
            }
            
            return result.ToString();
        }

        private static bool IsHardBreak(string md, int position)
        {
            // Look for two or more spaces at the end of a line
            if (position < md.Length && (md[position] == '\n' || md[position] == '\r'))
            {
                int spaces = 0;
                int pos = position - 1;
                
                // Count spaces backwards from before the newline
                while (pos >= 0 && md[pos] == ' ')
                {
                    spaces++;
                    pos--;
                }
                
                // Check if we have 2+ spaces
                return spaces >= 2;
            }
            
            return false;
        }

        private static void ExtractHardBreak(string md, ref int position)
        {
            // Skip the newline
            if (position < md.Length && md[position] == '\n')
            {
                position++;
            }
            else if (position < md.Length && md[position] == '\r')
            {
                position++;
                if (position < md.Length && md[position] == '\n')
                {
                    position++;
                }
            }
        }

        private static bool IsParagraphBreak(string md, int position)
        {
            // Look for double newline (empty line)
            if (position < md.Length && md[position] == '\n')
            {
                int nextPos = position + 1;
                
                // Skip any carriage return
                if (nextPos < md.Length && md[nextPos] == '\r')
                {
                    nextPos++;
                }
                
                // Skip any whitespace on the next line
                while (nextPos < md.Length && char.IsWhiteSpace(md[nextPos]) && md[nextPos] != '\n' && md[nextPos] != '\r')
                {
                    nextPos++;
                }
                
                // Check if next line is also a newline (empty line)
                if (nextPos < md.Length && (md[nextPos] == '\n' || md[nextPos] == '\r'))
                {
                    return true;
                }
            }
            
            return false;
        }

        private static void ExtractParagraphBreak(string md, ref int position)
        {
            // Skip all consecutive empty lines (2 or more newlines)
            // Skip first newline
            if (position < md.Length && md[position] == '\n')
            {
                position++;
            }
            else if (position < md.Length && md[position] == '\r')
            {
                position++;
                if (position < md.Length && md[position] == '\n')
                {
                    position++;
                }
            }
            
            // Skip all subsequent empty lines (but preserve whitespace on content lines for indent detection)
            while (position < md.Length)
            {
                // Skip any carriage return
                if (position < md.Length && md[position] == '\r')
                {
                    position++;
                }
                
                // Remember position before skipping whitespace
                int posBeforeWhitespace = position;
                
                // Skip any whitespace on the empty line
                while (position < md.Length && char.IsWhiteSpace(md[position]) && md[position] != '\n' && md[position] != '\r')
                {
                    position++;
                }
                
                // Check if next character is a newline (empty line continues)
                if (position < md.Length && (md[position] == '\n' || md[position] == '\r'))
                {
                    // Skip the newline
                    if (md[position] == '\n')
                    {
                        position++;
                    }
                    else if (md[position] == '\r')
                    {
                        position++;
                        if (position < md.Length && md[position] == '\n')
                        {
                            position++;
                        }
                    }
                    // Continue loop to check for more empty lines
                }
                else
                {
                    // Next line has content, restore position to preserve indentation
                    position = posBeforeWhitespace;
                    break;
                }
            }
        }

    }
}
