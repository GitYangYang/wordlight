﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using EnvDTE;
using Microsoft.VisualStudio.TextManager.Interop;

namespace WordLight
{
    public static class IVsTextLinesExtensions
    {
        public static TextSpan CreateSpanForAllLines(this IVsTextBuffer buffer)
        {
            TextSpan span = new TextSpan();
            buffer.GetLastLineIndex(out span.iEndLine, out span.iEndIndex);
            return span;
        }

        public static EditPoint CreateEditPoint(this IVsTextLines buffer, int line, int column)
        {
            object tempPointer;
            buffer.CreateEditPoint(line, column, out tempPointer);
            return tempPointer as EditPoint;
        }

        public static int GetPositionOfLineIndex(this IVsTextLines buffer, int iLine, int iIndex)
        {
            int piPosition;
            buffer.GetPositionOfLineIndex(iLine, iIndex, out piPosition);
            return piPosition;
        }

        public static string GetText(this IVsTextLines buffer)
        {
            var entireSpan = buffer.CreateSpanForAllLines();
            
            string text;
            buffer.GetLineText(entireSpan.iStartLine, entireSpan.iStartIndex, entireSpan.iEndLine, entireSpan.iEndIndex, out text);

            return text;
        }

		public static string GetSubstring(this IVsTextLines buffer, int start, int end)
		{
			string text = buffer.GetText();
			if (!string.IsNullOrEmpty(text))
			{
				return text.Substring(start, end - start);
			}			
			return string.Empty;
		}
    }
}
