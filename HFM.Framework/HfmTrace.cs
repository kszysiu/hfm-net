/*
 * HFM.NET - HfmTrace (Instrumentation) Class
 * Copyright (C) 2006-2007 David Rawling
 * Copyright (C) 2009-2011 Ryan Harlamert (harlam357)
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
using System.Collections;
using System.Diagnostics;

namespace HFM.Framework
{
   //TODO: Need a way to notify the main UI when 
   //      Error or Warning exceptions are logged.
   
   //TODO: Would like to show full function names
   //      only in the HFM.log file and not in 
   //      the Messages Window.

   public class HfmTrace
   {
      #region Instrumentation
      /// <summary>
      /// The function name of the caller function (1 stack level up)
      /// </summary>
      /// <returns>Class.Function name</returns>
      public static String FunctionName
      {
         get
         {
            StackFrame sf = new StackFrame(1, true);

            return String.Format("{0}.{1}", sf.GetMethod().DeclaringType, sf.GetMethod().Name);
         }
      }

      /// <summary>
      /// The function name of the parent function (2 stack levels up)
      /// </summary>
      /// <returns>Class.Function name</returns>
      public static String ParentFunctionName
      {
         get
         {
            StackFrame sf = new StackFrame(2, true);

            return String.Format("{0}.{1}", sf.GetMethod().DeclaringType, sf.GetMethod().Name);
         }
      }

      /// <summary>
      /// The function name of the grandparent function (3 stack levels up)
      /// </summary>
      /// <returns>Class.Function name</returns>
      public static String GParentFunctionName
      {
         get
         {
            StackFrame sf = new StackFrame(3, true);

            return String.Format("{0}.{1}", sf.GetMethod().DeclaringType, sf.GetMethod().Name);
         }
      }

      /// <summary>
      /// The filename in which the function can be found
      /// </summary>
      /// <returns></returns>
      public static String FileName
      {
         get
         {
            StackFrame sf = new StackFrame(1, true);

            return System.IO.Path.GetFileName(sf.GetFileName());
         }
      }

      /// <summary>
      /// Returns the execution time of the function
      /// </summary>
      /// <param name="Start">The start time as previously returned by ExecStart</param>
      /// <returns>String formatted as "#,##0 ms"</returns>
      public static String GetExecTime(DateTime Start)
      {
         TimeSpan t = DateTime.Now.Subtract(Start);

         return String.Format("{0:#,##0} ms", t.TotalMilliseconds);
      }

      /// <summary>
      /// Simple wrapper around DateTime to return current time (usable for Start Time of a function or operation)
      /// </summary>
      public static DateTime ExecStart
      {
         get
         {
            return DateTime.Now;
         }
      } 
      #endregion

      #region HFM Console Write Support
      private static readonly object LockTraceWrite = typeof(Trace);

      #region Static
      public static void WriteToHfmConsole(string message)
      {
         Instance.DoWrite(TraceLevel.Off, message, false);
      }

      public static void WriteToHfmConsole(Exception ex)
      {
         TraceLevel level = TraceLevel.Error;
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, ex);
         }
      }

      public static void WriteToHfmConsole(string subMessage, Exception ex)
      {
         TraceLevel level = TraceLevel.Error;
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, subMessage, ex);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, IEnumerable messages)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, messages);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, string message)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, message, false);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, string message, bool showFunctionName)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, message, showFunctionName);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, Exception ex)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, ex);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, DateTime Start)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, Start);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, string subMessage, string message)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, subMessage, message);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, string subMessage, Exception ex)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, subMessage, ex);
         }
      }

      public static void WriteToHfmConsole(TraceLevel level, string subMessage, DateTime Start)
      {
         if (level.CompareTo(TraceLevelSwitch.Switch.Level) < 1)
         {
            Instance.DoWrite(level, subMessage, Start);
         }
      }

      private static string FormatTraceString(TraceLevel level, object message)
      {
         string messageIdentifier = String.Empty;

         switch (level)
         {
            case TraceLevel.Off:
               messageIdentifier = " ";
               break;
            case TraceLevel.Error:
               messageIdentifier = "X";
               break;
            case TraceLevel.Warning:
               messageIdentifier = "!";
               break;
            case TraceLevel.Info:
               messageIdentifier = "-";
               break;
            case TraceLevel.Verbose:
               messageIdentifier = "+";
               break;
         }

         DateTime dateTime = DateTime.Now;
         return String.Format("[{0}-{1}] {2} {3}", dateTime.ToShortDateString(), dateTime.ToLongTimeString(), messageIdentifier, message);
      } 
      #endregion

      #region Private Instance
      private void DoWrite(TraceLevel level, IEnumerable messages)
      {
         lock (LockTraceWrite)
         {
            foreach (string message in messages)
            {
               string traceString = FormatTraceString(level, message);
               Trace.WriteLine(traceString);
               OnTextMessage(new TextMessageEventArgs(traceString));
            }
         }
      }

      private void DoWrite(TraceLevel level, string message, bool showFunctionName)
      {
         lock (LockTraceWrite)
         {
            string traceString;
            if (showFunctionName)
            {
               traceString = FormatTraceString(level, String.Format("{0} {1}", GParentFunctionName, message));
            }
            else
            {
               traceString = FormatTraceString(level, message);
            }
            Trace.WriteLine(traceString);
            OnTextMessage(new TextMessageEventArgs(traceString));
         }
      }

      private void DoWrite(TraceLevel level, Exception ex)
      {
         lock (LockTraceWrite)
         {
            string traceString = FormatTraceString(level, String.Format("{0} Threw Exception: {1}", GParentFunctionName, ex));
            Trace.WriteLine(traceString);

            string consoleString = FormatTraceString(level, String.Format("{0} Threw Exception: {1}", GParentFunctionName, ex.Message));
            OnTextMessage(new TextMessageEventArgs(consoleString));
         }
      }

      private void DoWrite(TraceLevel level, DateTime Start)
      {
         lock (LockTraceWrite)
         {
            string traceString = FormatTraceString(level, String.Format("{0} Execution Time: {1}", GParentFunctionName, GetExecTime(Start)));
            Trace.WriteLine(traceString);
            OnTextMessage(new TextMessageEventArgs(traceString));
         }
      }

      private void DoWrite(TraceLevel level, string subMessage, string message)
      {
         lock (LockTraceWrite)
         {
            string traceString = FormatTraceString(level, String.Format("{0} ({1}) {2}", GParentFunctionName, subMessage, message));
            Trace.WriteLine(traceString);
            OnTextMessage(new TextMessageEventArgs(traceString));
         }
      }

      private void DoWrite(TraceLevel level, string subMessage, Exception ex)
      {
         lock (LockTraceWrite)
         {
            string traceString = FormatTraceString(level, String.Format("{0} ({1}) Threw Exception: {2}", GParentFunctionName, subMessage, ex));
            Trace.WriteLine(traceString);

            string consoleString = FormatTraceString(level, String.Format("{0} ({1}) Threw Exception: {2}", GParentFunctionName, subMessage, ex.Message));
            OnTextMessage(new TextMessageEventArgs(consoleString));
         }
      }

      private void DoWrite(TraceLevel level, string subMessage, DateTime Start)
      {
         lock (LockTraceWrite)
         {
            string traceString = FormatTraceString(level, String.Format("{0} ({1}) Execution Time: {2}", GParentFunctionName, subMessage, GetExecTime(Start)));
            Trace.WriteLine(traceString);
            OnTextMessage(new TextMessageEventArgs(traceString));
         }
      } 
      #endregion
      
      #endregion

      #region Text Message Event
      public event EventHandler<TextMessageEventArgs> TextMessage;

      private void OnTextMessage(TextMessageEventArgs e)
      {
         if (TextMessage != null)
         {
            TextMessage(this, e);
         }
      } 
      #endregion

      #region Singleton Support
      private static HfmTrace _Instance;
      private static readonly object classLock = typeof(HfmTrace);

      public static HfmTrace Instance
      {
         get
         {
            lock (classLock)
            {
               if (_Instance == null)
               {
                  _Instance = new HfmTrace();
               }
            }
            return _Instance;
         }
      }
      #endregion
   }

   public class TextMessageEventArgs : EventArgs
   {
      private readonly string _message;
      public string Message
      {
         get { return _message; }
      }

      public TextMessageEventArgs(string message)
      {
         _message = message;
      }
   }

   public static class TraceLevelSwitch
   {
      private static TraceSwitch _Instance;
      private static readonly Object classLock = typeof(TraceSwitch);

      public static TraceSwitch Instance
      {
         get
         {
            lock (classLock)
            {
               if (_Instance == null)
               {
                  _Instance = new TraceSwitch("TraceLevelSwitch", "TraceLevelSwitch");
               }
            }
            return _Instance;
         }
      }

      public static TraceSwitch Switch
      {
         get { return Instance; }
      }
   }
}
