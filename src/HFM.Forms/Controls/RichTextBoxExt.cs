/*
 * HFM.NET - RichTextBox Wrapper Class
 * Copyright (C) 2009-2012 Ryan Harlamert (harlam357)
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; version 2
 * of the License. See the included file GPLv2.TXT.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using HFM.Core;
using HFM.Core.DataTypes;

namespace HFM.Forms.Controls
{
   public interface ILogFileViewer
   {
      bool Visible { get; set; }
   
      string LogOwnedByInstanceName { get; }
      
      void SetLogLines(IEnumerable<LogLine> lines, string logOwnedByInstance, bool highlightLines);

      void HighlightLines(bool value);

      string[] Lines { get; }
      
      void SetNoLogLines();
      
      void ScrollToBottom();
   }

   [CoverageExclude]
   public partial class RichTextBoxExt : RichTextBox, ILogFileViewer
   {
      private IList<LogLine> _logLines;
      
      private string _logOwnedByInstanceName = String.Empty;
      
      public string LogOwnedByInstanceName
      {
         get { return _logOwnedByInstanceName; }
      }
   
      public RichTextBoxExt()
      {
         InitializeComponent();
      }

      public void SetLogLines(IEnumerable<LogLine> lines, string logOwnedByInstance, bool highlightLines)
      {
         if (InvokeRequired)
         {
            Invoke(new Action<IEnumerable<LogLine>, string, bool>(SetLogLines), lines, logOwnedByInstance, highlightLines);
            return;
         }

         _logOwnedByInstanceName = logOwnedByInstance;

#if !LOGTOOL
         // limit the maximum number of log lines
         int lineOffset = lines.Count() - Constants.MaxDisplayableLogLines;
         if (lineOffset > 0)
         {
            lines = lines.Where((x, i) => i > lineOffset);
         }
#endif

         _logLines = lines.ToList();
         HighlightLines(highlightLines);
      }

      public void HighlightLines(bool value)
      {
         if (value)
         {
            Rtf = BuildRtfString();
         }
         else
         {
            Rtf = null;
            Lines = (from LogLine line in _logLines select line.LineRaw.Replace("\r", String.Empty)).ToArray();
         }
      }

      private string BuildRtfString()
      {
         // cf1 - Dark Green
         // cf2 - Dark Red
         // cf3 - Dark Orange
         // cf4 - Blue
         // cf5 - Slate Gray

         var sb = new StringBuilder();
         sb.Append(@"{\rtf1\ansi\deff0{\colortbl;\red0\green150\blue0;\red139\green0\blue0;\red255\green140\blue0;\red0\green0\blue255;\red120\green120\blue120;}");
         foreach(var line in _logLines)
         {
            sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}{1}\line", GetLineColor(line), line);
         }
         return sb.ToString();
      }

      private static string GetLineColor(LogLine line)
      {
         switch (line.LineType)
         {
            case LogLineType.WorkUnitFrame:
               return @"\cf1 ";
            case LogLineType.ClientShutdown:
            case LogLineType.ClientCoreCommunicationsError:
            case LogLineType.ClientCoreCommunicationsErrorShutdown:
            case LogLineType.ClientEuePauseState:
            case LogLineType.WorkUnitCoreShutdown:
            case LogLineType.WorkUnitCoreReturn:
               return @"\cf2 ";
            case LogLineType.Error:
               return @"\cf3 ";
            case LogLineType.Unknown:
               return @"\cf5 ";
            default:
               return @"\cf4 ";
         }
      }
      
      public void SetNoLogLines()
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(SetNoLogLines));
            return;
         }

         _logLines = null;

         Rtf = Core.Application.IsRunningOnMono ? String.Empty : null;
         Text = "No Log Available";
      }

      #region Native Scroll Messages (don't call under Mono)

      public void ScrollToBottom()
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(ScrollToBottom));
            return;
         }

         SelectionStart = TextLength;

         if (Core.Application.IsRunningOnMono)
         {
            ScrollToCaret();
         }
         else
         {
            NativeMethods.SendMessage(Handle, NativeMethods.WM_VSCROLL, new IntPtr(NativeMethods.SB_BOTTOM), new IntPtr(0));
         }
      }

      public void ScrollToTop()
      {
         if (Core.Application.IsRunningOnMono)
         {
            throw new NotImplementedException("This function is not implemented when running under the Mono Runtime.");
         }

         NativeMethods.SendMessage(Handle, NativeMethods.WM_VSCROLL, new IntPtr(NativeMethods.SB_TOP), new IntPtr(0));
      }

      public void ScrollLineDown()
      {
         if (Core.Application.IsRunningOnMono)
         {
            throw new NotImplementedException("This function is not implemented when running under the Mono Runtime.");
         }

         NativeMethods.SendMessage(Handle, NativeMethods.WM_VSCROLL, new IntPtr(NativeMethods.SB_LINEDOWN), new IntPtr(0));
      }

      public void ScrollLineUp()
      {
         if (Core.Application.IsRunningOnMono)
         {
            throw new NotImplementedException("This function is not implemented when running under the Mono Runtime.");
         }

         NativeMethods.SendMessage(Handle, NativeMethods.WM_VSCROLL, new IntPtr(NativeMethods.SB_LINEUP), new IntPtr(0));
      }

      public void ScrollToLine(int lineNumber)
      {
         if (Core.Application.IsRunningOnMono)
         {
            throw new NotImplementedException("This function is not implemented when running under the Mono Runtime.");
         }

         NativeMethods.SendMessage(Handle, NativeMethods.EM_LINESCROLL, new IntPtr(0), new IntPtr(lineNumber));
      }
      
      #endregion
   }
}
