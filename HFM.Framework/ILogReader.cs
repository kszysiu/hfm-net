/*
 * HFM.NET - Log Reader Interface
 * Copyright (C) 2009-2010 Ryan Harlamert (harlam357)
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
using System.Collections.Generic;

namespace HFM.Framework
{
   public interface ILogReader
   {
      /// <summary>
      /// Returns the last client run data.
      /// </summary>
      IClientRun LastClientRun { get; }

      /// <summary>
      /// Returns log text of the previous work unit.
      /// </summary>
      IList<ILogLine> PreviousWorkUnitLogLines { get; }

      /// <summary>
      /// Returns log text of the current work unit.
      /// </summary>
      IList<ILogLine> CurrentWorkUnitLogLines { get; }

      /// <summary>
      /// Get a list of Log Lines that correspond to the given Queue Index.
      /// </summary>
      /// <param name="QueueIndex">The Queue Index (0-9)</param>
      IList<ILogLine> GetLogLinesFromQueueIndex(int QueueIndex);

      /// <summary>
      /// Find the WorkUnitProject Line in the given collection and return it's value.
      /// </summary>
      /// <param name="logLines">Log Lines to search</param>
      string GetProjectFromLogLines(ICollection<ILogLine> logLines);

      /// <summary>
      /// Get the ClientStatus based on the given collection of Log Lines.
      /// </summary>
      /// <param name="logLines">Log Lines to search</param>
      ClientStatus GetStatusFromLogLines(ICollection<ILogLine> logLines);

      /// <summary>
      /// Scan the FAHLog text lines to determine work unit boundries.
      /// </summary>
      /// <param name="InstanceName">Client Instance Name that owns the log file we're parsing.</param>
      /// <param name="LogFilePath">Path to the log file.</param>
      /// <exception cref="ArgumentException">Throws if LogFileName is Null or Empty.</exception>
      void ScanFAHLog(string InstanceName, string LogFilePath);
   }
}