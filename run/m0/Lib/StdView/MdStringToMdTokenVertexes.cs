using m0.Foundation;
using m0.Graph;
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
        static IVertex BoldStart = Md.Get(false, "BoldStart");
        static IVertex BoldEnd = Md.Get(false, "BoldEnd");
        static IVertex ItalicStart = Md.Get(false, "ItalicStart");
        static IVertex ItalicEnd = Md.Get(false, "ItalicEnd");
        static IVertex CodeBlockStart = Md.Get(false, "CodeBlockStart");
        static IVertex CodeBlockEnd = Md.Get(false, "CodeBlockEnd");
        static IVertex InlineCodeStart = Md.Get(false, "InlineCodeStart");
        static IVertex InlineCodeEnd = Md.Get(false, "InlineCodeEnd");
        static IVertex LinkStart = Md.Get(false, "LinkStart");
        static IVertex LinkText = Md.Get(false, "LinkText");
        static IVertex LinkUrl = Md.Get(false, "LinkUrl");
        static IVertex LinkEnd = Md.Get(false, "LinkEnd");
        static IVertex ImageStart = Md.Get(false, "ImageStart");
        static IVertex ImageUrl = Md.Get(false, "ImageUrl");
        static IVertex ImageEnd = Md.Get(false, "ImageEnd");
        static IVertex BlockquoteStart = Md.Get(false, "BlockquoteStart");
        static IVertex BlockquoteEnd = Md.Get(false, "BlockquoteEnd");
        static IVertex ListItemStart = Md.Get(false, "ListItemStart");
        static IVertex OrderedListItemStart = Md.Get(false, "OrderedListItemStart");
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

        // State tracking for nested elements
        private static bool isInsideBold = false;
        private static bool isInsideItalic = false;
        private static bool isInsideCodeBlock = false;
        private static bool isInsideInlineCode = false;
        private static bool isInsideLink = false;
        private static bool isInsideImage = false;
        private static bool isInsideTable = false;
        private static int blockquoteLevel = 0;

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

            int position = 0;

            while (position < md.Length)
            {
                char currentChar = md[position];
                
                // Handle hard breaks (two or more spaces at end of line)
                if (IsHardBreak(md, position))
                {
                    ExtractHardBreak(md, ref position);
                    AddTokenToTarget(to, HardBreak);
                    continue;
                }
                
                // Handle paragraph breaks (double newline = empty line)
                if (IsParagraphBreak(md, position))
                {
                    ExtractParagraphBreak(md, ref position);
                    AddTokenToTarget(to, ParagraphBreak);
                    continue;
                }
                
                // Handle single newlines (just advance position)
                if (currentChar == '\n')
                {
                    position++;
                    continue;
                }
                
                // Handle carriage returns (just advance position)
                if (currentChar == '\r')
                {
                    position++;
                    continue;
                }

                // Handle horizontal rules (--- or ***)
                if (IsHorizontalRule(md, position))
                {
                    ExtractHorizontalRule(md, ref position);
                    AddTokenToTarget(to, HorizontalRule);
                    continue;
                }
                
                // Handle headers (# ## ### etc.)
                if (currentChar == '#')
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
                    AddTokenToTarget(to, CodeBlockStart);
                    continue;
                }
                
                // Handle code block end (```)
                if (IsCodeBlockEnd(md, position))
                {
                    position += 3; // Skip closing ```
                    isInsideCodeBlock = false;
                    AddTokenToTarget(to, CodeBlockEnd);
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
                
                // Handle inline code end (`)
                if (IsInlineCodeEnd(md, position))
                {
                    position++; // Skip closing `
                    isInsideInlineCode = false;
                    AddTokenToTarget(to, InlineCodeEnd);
                    continue;
                }
                
                // Handle links [text](url)
                if (IsLinkStart(md, position))
                {
                    position++; // Skip opening [
                    isInsideLink = true;
                    AddTokenToTarget(to, LinkStart);
                    continue;
                }
                
                // Handle link end ](url)
                if (IsLinkEnd(md, position))
                {
                    ExtractLinkEnd(md, ref position);
                    isInsideLink = false;
                    AddTokenToTarget(to, LinkEnd);
                    continue;
                }
                
                // Handle images ![alt](url)
                if (IsImageStart(md, position))
                {
                    position += 2; // Skip opening ![
                    isInsideImage = true;
                    AddTokenToTarget(to, ImageStart);
                    continue;
                }
                
                // Handle image end ](url)
                if (IsImageEnd(md, position))
                {
                    ExtractImageEnd(md, ref position);
                    isInsideImage = false;
                    AddTokenToTarget(to, ImageEnd);
                    continue;
                }
                
                // Handle list items (- item or * item or 1. item)
                if (IsListItem(md, position))
                {
                    ExtractListItem(md, ref position, to);
                    continue;
                }
                
                // Handle blockquotes (> text)
                if (currentChar == '>')
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
                    continue;
                }
                
                // Handle blockquote end (newline not followed by >)
                if (IsBlockquoteEnd(md, position))
                {
                    // Close all remaining blockquote levels
                    while (blockquoteLevel > 0)
                    {
                        blockquoteLevel--;
                        AddTokenToTarget(to, BlockquoteEnd);
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

        private static void AddTokenWithTextToTarget(IVertex target, IVertex tokenType, string text)
        {            
            target.AddVertex(tokenType, text);
        }

        private static void AddTextTokenToTarget(IVertex target, string textValue)
        {         
            target.AddVertex(Text, textValue);        
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
            if (isInsideBold) return false; // Already inside bold
            return (md[position] == '*' && md[position + 1] == '*') ||
                   (md[position] == '_' && md[position + 1] == '_');
        }

        private static bool IsBoldEnd(string md, int position)
        {
            if (position + 1 >= md.Length) return false;
            if (!isInsideBold) return false; // Not inside bold
            return (md[position] == '*' && md[position + 1] == '*') ||
                   (md[position] == '_' && md[position + 1] == '_');
        }

        private static bool IsItalicStart(string md, int position)
        {
            if (position >= md.Length) return false;
            if (isInsideItalic) return false; // Already inside italic
            char current = md[position];
            return (current == '*' || current == '_') && 
                   (position + 1 >= md.Length || md[position + 1] != current);
        }

        private static bool IsItalicEnd(string md, int position)
        {
            if (position >= md.Length) return false;
            if (!isInsideItalic) return false; // Not inside italic
            char current = md[position];
            return (current == '*' || current == '_') && 
                   (position + 1 >= md.Length || md[position + 1] != current);
        }   

        private static bool IsCodeBlockStart(string md, int position)
        {
            if (position + 2 >= md.Length) return false;
            if (isInsideCodeBlock) return false; // Already inside code block
            return md[position] == '`' && md[position + 1] == '`' && md[position + 2] == '`';
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
            if (isInsideLink) return false; // Already inside link
            return md[position] == '[';
        }

        private static bool IsLinkEnd(string md, int position)
        {
            if (position >= md.Length) return false;
            if (!isInsideLink) return false; // Not inside link
            return md[position] == ']' && position + 1 < md.Length && md[position + 1] == '(';
        }

        private static void ExtractLinkEnd(string md, ref int position)
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

        private static bool IsImageStart(string md, int position)
        {
            if (position + 1 >= md.Length) return false;
            if (isInsideImage) return false; // Already inside image
            return md[position] == '!' && md[position + 1] == '[';
        }

        private static string ExtractImage(string md, ref int position)
        {
            position += 2; // Skip opening ![
            
            StringBuilder result = new StringBuilder();
            result.Append("![");
            
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

        private static bool IsImageEnd(string md, int position)
        {
            if (position >= md.Length) return false;
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

        private static bool IsListItem(string md, int position)
        {
            if (position >= md.Length) return false;
            
            // Check for unordered list (- or *)
            if (md[position] == '-' || md[position] == '*')
            {
                return position + 1 >= md.Length || char.IsWhiteSpace(md[position + 1]);
            }
            
            // Check for ordered list (1. 2. etc.)
            if (char.IsDigit(md[position]))
            {
                int i = position;
                while (i < md.Length && char.IsDigit(md[i]))
                {
                    i++;
                }
                return i < md.Length && md[i] == '.' && (i + 1 >= md.Length || char.IsWhiteSpace(md[i + 1]));
            }
            
            return false;
        }

        private static void ExtractListItem(string md, ref int position, IVertex to)
        {
            // Extract list marker
            if (md[position] == '-' || md[position] == '*')
            {
                AddTokenToTarget(to, ListItemStart);
                position++;
            }
            else if (char.IsDigit(md[position]))
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

        private static bool IsBlockquoteEnd(string md, int position)
        {
            // Check if we're at a newline and next line doesn't start with >
            if (position < md.Length && md[position] == '\n')
            {
                int nextLineStart = position + 1;
                // Skip any carriage return
                if (nextLineStart < md.Length && md[nextLineStart] == '\r')
                {
                    nextLineStart++;
                }
                // Skip whitespace (but not newlines)
                while (nextLineStart < md.Length && char.IsWhiteSpace(md[nextLineStart]) && md[nextLineStart] != '\n' && md[nextLineStart] != '\r')
                {
                    nextLineStart++;
                }
                // Check if next line doesn't start with > (end of blockquote)
                return nextLineStart >= md.Length || md[nextLineStart] != '>';
            }
            return false;
        }
        
        private static string ExtractRegularText(string md, ref int position)
        {
            StringBuilder result = new StringBuilder();
            
            while (position < md.Length)
            {
                char current = md[position];
                
                // Stop at special characters that start other tokens
                if (current == '\n' || current == '\r' || 
                    current == '#' || current == '*' || current == '_' || 
                    current == '`' || current == '[' || current == '!' ||
                    current == '-' || current == '>' || current == '|' ||
                    (char.IsDigit(current) && IsListItem(md, position)))
                {
                    break;
                }
                
                result.Append(current);
                position++;
            }
            
            return result.ToString();
        }

        private static void ExtractTable(string md, ref int position, IVertex to)
        {
            AddTokenToTarget(to, "TableStart");
            
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
                return "left"; // default if not enough dashes
            
            if (hasLeftColon && hasRightColon)
                return "center";
            else if (hasLeftColon)
                return "left";
            else if (hasRightColon)
                return "right";
            else
                return "left"; // default
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
                    AddTokenToTarget(to, AlignCenter);
                }
            }
            
            // Extract cell content (everything until | or end of line)
            StringBuilder cellContent = new StringBuilder();
            while (position < md.Length && md[position] != '|' && md[position] != '\n' && md[position] != '\r')
            {
                cellContent.Append(md[position]);
                position++;
            }
            
            string content = cellContent.ToString().Trim();
            if (!string.IsNullOrEmpty(content))
            {
                AddTextTokenToTarget(to, content);
            }
            
            if (isFirstRow)
            {
                AddTokenToTarget(to, HeaderColumnEnd);
            }
            else
            {
                AddTokenToTarget(to, CellEnd);
            }
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
            
            // Skip any carriage return
            if (position < md.Length && md[position] == '\r')
            {
                position++;
            }
            
            // Skip any whitespace on the empty line
            while (position < md.Length && char.IsWhiteSpace(md[position]) && md[position] != '\n' && md[position] != '\r')
            {
                position++;
            }
            
            // Skip the second newline
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

    }
}
