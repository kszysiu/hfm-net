﻿/*
 * HFM.NET - Data Aggregator Class
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
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

using HFM.Log;
using HFM.Queue;
using HFM.Framework;
using HFM.Framework.DataTypes;

namespace HFM.DataAggregator
{
   public class DataAggregator : IDataAggregator
   {
      private LogReader _logReader;
      private LogInterpreter _logInterpreter;

      /// <summary>
      /// Instance Name
      /// </summary>
      public string InstanceName { get; set; }

      /// <summary>
      /// queue.dat File Path
      /// </summary>
      public string QueueFilePath { get; set; }

      /// <summary>
      /// FAHlog.txt File Path
      /// </summary>
      public string FahLogFilePath { get; set; }

      /// <summary>
      /// unitinfo.txt File Path
      /// </summary>
      public string UnitInfoLogFilePath { get; set; }

      private ClientQueue _clientQueue;
      /// <summary>
      /// Client Queue
      /// </summary>
      public ClientQueue Queue
      {
         get
         {
            return _clientQueue;
         }
      }

      /// <summary>
      /// Current Index in List of returned UnitInfo and UnitLogLines
      /// </summary>
      public int CurrentUnitIndex
      {
         get
         {
            if (_clientQueue != null)
            {
               return _clientQueue.CurrentIndex;
            }

            // default Unit Index if only parsing logs
            return 1;
         }
      }

      private ClientRun _currentClientRun;
      /// <summary>
      /// Client Run Data for the Current Run
      /// </summary>
      public ClientRun CurrentClientRun
      {
         get { return _currentClientRun; }
      }

      private FahLogUnitData _currentFahLogUnitData;
      /// <summary>
      /// Current Work Unit Status based on LogReader CurrentWorkUnitLogLines
      /// </summary>
      public ClientStatus CurrentWorkUnitStatus
      {
         get { return _currentFahLogUnitData.Status; }
      }

      /// <summary>
      /// Current Log Lines based on UnitLogLines Array and CurrentUnitIndex
      /// </summary>
      public IList<LogLine> CurrentLogLines
      {
         get
         {
            return _unitLogLines == null ? new List<LogLine>() : _unitLogLines[CurrentUnitIndex];
         }
      }

      private IList<LogLine>[] _unitLogLines;
      /// <summary>
      /// Array of LogLine Lists
      /// </summary>
      [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
      public IList<LogLine>[] UnitLogLines
      {
         get { return _unitLogLines; }
      }

      #region Aggregation Logic
      
      /// <summary>
      /// Aggregate Data and return UnitInfo List
      /// </summary>
      public IList<IUnitInfo> AggregateData()
      {
         _logReader = new LogReader(Default.DateTimeStyle);
         var logLines = _logReader.GetLogLines(FahLogFilePath);
         var clientRuns = _logReader.GetClientRuns(logLines);

         _logInterpreter = new LogInterpreter(logLines, clientRuns);
         _currentClientRun = _logInterpreter.LastClientRun;
         
         // report errors that came back from log parsing
         foreach (var s in _logInterpreter.LogLineParsingErrors)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, s);
         }

         IList<IUnitInfo> parsedUnits;
         // Decision Time: If Queue Read fails parse from logs only
         QueueData qData = ReadQueueFile();
         if (qData != null)
         {
            parsedUnits = GenerateUnitInfoDataFromQueue(qData);
            _clientQueue = BuildClientQueue(qData);
         }
         else
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, 
               "Queue unavailable or failed read.  Parsing logs without queue.");

            parsedUnits = GenerateUnitInfoDataFromLogs();
            _clientQueue = null;
         }
         
         _logReader = null;
         _logInterpreter = null;

         return parsedUnits;
      }

      private static ClientQueue BuildClientQueue(QueueData q)
      {
         var cq = new ClientQueue();
         cq.CurrentIndex = (int)q.CurrentIndex;
         cq.PerformanceFraction = q.PerformanceFraction;
         cq.PerformanceFractionUnitWeight = (int)q.PerformanceFractionUnitWeight;
         cq.DownloadRateAverage = q.DownloadRateAverage;
         cq.DownloadRateUnitWeight = (int)q.DownloadRateUnitWeight;
         cq.UploadRateAverage = q.UploadRateAverage;
         cq.UploadRateUnitWeight = (int)q.UploadRateUnitWeight;
         
         for (int i = 0; i < 10; i++)
         {
            PopulateClientQueueEntry(cq.GetQueueEntry(i), q.GetQueueEntry((uint)i));
         }

         return cq;
      }
      
      private static void PopulateClientQueueEntry(ClientQueueEntry cqe, QueueEntry qe)
      {
         cqe.EntryStatus = (QueueEntryStatus)qe.EntryStatus;
         cqe.SpeedFactor = qe.SpeedFactor;
         cqe.UseCores = (int)qe.UseCores;
         cqe.BeginTimeUtc = qe.BeginTimeUtc;
         cqe.BeginTimeLocal = qe.BeginTimeLocal;
         cqe.EndTimeUtc = qe.EndTimeUtc;
         cqe.EndTimeLocal = qe.EndTimeLocal;
         cqe.ProjectID = qe.ProjectID;
         cqe.ProjectRun = qe.ProjectRun;
         cqe.ProjectClone = qe.ProjectClone;
         cqe.ProjectGen = qe.ProjectGen;
         cqe.MachineID = (int)qe.MachineID;
         cqe.ServerIP = qe.ServerIP;
         cqe.UserID = qe.UserID;
         cqe.Benchmark = (int)qe.Benchmark;
         cqe.CpuString = qe.CpuString;
         cqe.OsString = qe.OsString;
         cqe.NumberOfSmpCores = (int)qe.NumberOfSmpCores;
         cqe.MegaFlops = qe.MegaFlops;
         cqe.Memory = (int)qe.Memory;
         cqe.GpuMemory = (int)qe.GpuMemory;
      }

      /// <summary>
      /// Read the queue.dat file
      /// </summary>
      private QueueData ReadQueueFile()
      {
         // Make sure the queue file exists first.  Would like to avoid the exception overhead.
         if (File.Exists(QueueFilePath))
         {
            // queue.dat is not required to get a reading 
            // if something goes wrong just catch and log
            try
            {
               return QueueReader.ReadQueue(QueueFilePath);
            }
            catch (Exception ex)
            {
               HfmTrace.WriteToHfmConsole(InstanceName, ex);
            }
         }

         return null;
      }

      private IUnitInfo[] GenerateUnitInfoDataFromLogs()
      {
         var parsedUnits = new IUnitInfo[2];
         _unitLogLines = new IList<LogLine>[2];

         if (_logInterpreter.PreviousWorkUnitLogLines != null)
         {
            _unitLogLines[0] = _logInterpreter.PreviousWorkUnitLogLines;
            parsedUnits[0] = BuildUnitInfo(null, _logReader.GetFahLogDataFromLogLines(_logInterpreter.PreviousWorkUnitLogLines), null);
         }

         bool matchOverride = false;
         _unitLogLines[1] = _logInterpreter.CurrentWorkUnitLogLines;
         if (_unitLogLines[1] == null)
         {
            matchOverride = true;
            _unitLogLines[1] = _logInterpreter.CurrentClientRunLogLines;
         }
         
         _currentFahLogUnitData = _logReader.GetFahLogDataFromLogLines(_unitLogLines[1]);
         parsedUnits[1] = BuildUnitInfo(null, _currentFahLogUnitData, GetUnitInfoLogData(), matchOverride);

         return parsedUnits;
      }

      private IUnitInfo[] GenerateUnitInfoDataFromQueue(QueueData q)
      {
         var parsedUnits = new IUnitInfo[10];
         _unitLogLines = new IList<LogLine>[10];

         for (int queueIndex = 0; queueIndex < parsedUnits.Length; queueIndex++)
         {
            // Get the Log Lines for this queue position from the reader
            _unitLogLines[queueIndex] = _logInterpreter.GetLogLinesFromQueueIndex(queueIndex);
            // Get the FAH Log Data from the Log Lines
            FahLogUnitData fahLogUnitData = _logReader.GetFahLogDataFromLogLines(_unitLogLines[queueIndex]);
            UnitInfoLogData unitInfoLogData = null;
            // On the Current Queue Index
            if (queueIndex == q.CurrentIndex)
            {
               // Get the UnitInfo Log Data
               unitInfoLogData = GetUnitInfoLogData();
               _currentFahLogUnitData = fahLogUnitData;
            }

            parsedUnits[queueIndex] = BuildUnitInfo(q.GetQueueEntry((uint)queueIndex), fahLogUnitData, unitInfoLogData);
            if (parsedUnits[queueIndex] == null)
            {
               if (queueIndex == q.CurrentIndex)
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                     "Could not verify log section for current queue entry ({0}). Trying to parse with most recent log section.", queueIndex));

                  _unitLogLines[queueIndex] = _logInterpreter.CurrentWorkUnitLogLines;
                  // If got no Work Unit Log Lines based on Current Work Unit Log Lines
                  // then take the entire Current Client Run Log Lines - likely the run
                  // was short and never contained any Work Unit Data.
                  if (_unitLogLines[queueIndex] == null)
                  {
                     _unitLogLines[queueIndex] = _logInterpreter.CurrentClientRunLogLines;
                  }
                  fahLogUnitData = _logReader.GetFahLogDataFromLogLines(_unitLogLines[queueIndex]);
                  _currentFahLogUnitData = fahLogUnitData;

                  if (_currentFahLogUnitData.Status.Equals(ClientStatus.GettingWorkPacket))
                  {
                     // Use either the current Work Unit log lines or current Client Run log lines
                     // as decided upon above... don't clear it here and show the user nothing - 10/9/10
                     //_unitLogLines[queueIndex] = null;
                     fahLogUnitData = new FahLogUnitData();
                     unitInfoLogData = new UnitInfoLogData();
                  }
                  parsedUnits[queueIndex] = BuildUnitInfo(q.GetQueueEntry((uint)queueIndex), fahLogUnitData, unitInfoLogData, true);
               }
               else
               {
                  // Just skip this unit and continue
                  HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, String.Format(CultureInfo.CurrentCulture,
                     "Could not find or verify log section for queue entry {0} (this is not a problem).", queueIndex));
               }
            }
         }

         return parsedUnits;
      }
      
      private UnitInfoLogData GetUnitInfoLogData()
      {
         try
         {
            return _logReader.GetUnitInfoLogData(UnitInfoLogFilePath);
         }
         catch (Exception ex)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, ex);
            return null;
         }
      }
      
      private IUnitInfo BuildUnitInfo(QueueEntry queueEntry, FahLogUnitData fahLogUnitData, UnitInfoLogData unitInfoLogData)
      {
         return BuildUnitInfo(queueEntry, fahLogUnitData, unitInfoLogData, false);
      }

      private IUnitInfo BuildUnitInfo(QueueEntry queueEntry, FahLogUnitData fahLogUnitData, UnitInfoLogData unitInfoLogData, bool matchOverride)
      {
         var unit = new UnitInfo();
         unit.UnitStartTimeStamp = fahLogUnitData.UnitStartTimeStamp;
         unit.FramesObserved = fahLogUnitData.FramesObserved;
         unit.CoreVersion = fahLogUnitData.CoreVersion;
         unit.UnitResult = fahLogUnitData.UnitResult;

         if (queueEntry != null)
         {
            PopulateUnitInfoFromQueueEntry(queueEntry, unit);
            SearchFahLogUnitDataProjects(unit, fahLogUnitData);
            PopulateUnitInfoFromLogs(CurrentClientRun, fahLogUnitData, unitInfoLogData, unit);

            if (ProjectsMatch(unit, fahLogUnitData) ||
                ProjectsMatch(unit, unitInfoLogData) ||
                matchOverride)
            {
               // continue parsing the frame data
               ParseFrameData(fahLogUnitData.FrameDataList, unit);
            }
            else
            {
               return null;
            }
         }
         else
         {
            PopulateUnitInfoFromLogs(CurrentClientRun, fahLogUnitData, unitInfoLogData, unit);
            ParseFrameData(fahLogUnitData.FrameDataList, unit);
         }

         return unit;
      }

      private static void SearchFahLogUnitDataProjects(IUnitInfo unit, FahLogUnitData fahLogUnitData)
      {
         Debug.Assert(unit != null);
         for (int i = 0; i < fahLogUnitData.ProjectInfoList.Count; i++)
         {
            if (ProjectsMatch(unit, fahLogUnitData.ProjectInfoList[i]))
            {
               fahLogUnitData.ProjectInfoIndex = i;
            }
         }
      }

      private static bool ProjectsMatch(IUnitInfo unit, IProjectInfo projectInfo)
      {
         Debug.Assert(unit != null);
         if (unit.ProjectIsUnknown || projectInfo == null) return false;
      
         return (unit.ProjectID == projectInfo.ProjectID &&
                 unit.ProjectRun == projectInfo.ProjectRun &&
                 unit.ProjectClone == projectInfo.ProjectClone &&
                 unit.ProjectGen == projectInfo.ProjectGen);
      }

      #region Unit Population Methods
      private static void PopulateUnitInfoFromQueueEntry(QueueEntry entry, UnitInfo unit)
      {
         // convert to enum
         var queueEntryStatus = (QueueEntryStatus)entry.EntryStatus;
      
         if ((queueEntryStatus.Equals(QueueEntryStatus.Unknown) ||
              queueEntryStatus.Equals(QueueEntryStatus.Empty) ||
              queueEntryStatus.Equals(QueueEntryStatus.Garbage) ||
              queueEntryStatus.Equals(QueueEntryStatus.Abandonded)) == false)
         {
            /* Tag (Could be read here or through the unitinfo.txt file) */
            unit.ProteinTag = entry.WorkUnitTag;

            /* DownloadTime (Could be read here or through the unitinfo.txt file) */
            unit.DownloadTime = entry.BeginTimeUtc;

            /* DueTime (Could be read here or through the unitinfo.txt file) */
            unit.DueTime = entry.DueTimeUtc;

            /* FinishedTime */
            if (queueEntryStatus.Equals(QueueEntryStatus.Finished) ||
                queueEntryStatus.Equals(QueueEntryStatus.ReadyForUpload))
            {
               unit.FinishedTime = entry.EndTimeUtc;
            }

            /* Project (R/C/G) */
            unit.ProjectID = entry.ProjectID;
            unit.ProjectRun = entry.ProjectRun;
            unit.ProjectClone = entry.ProjectClone;
            unit.ProjectGen = entry.ProjectGen;

            /* FoldingID and Team from Queue Entry */
            unit.FoldingID = entry.FoldingID;
            unit.Team = (int) entry.TeamNumber;
            
            /* Core ID */
            unit.CoreID = entry.CoreNumber.ToUpperInvariant();
         }
      }

      private static void PopulateUnitInfoFromLogs(ClientRun currentClientRun, FahLogUnitData fahLogUnitData, 
                                                   UnitInfoLogData unitInfoLogData, UnitInfo unit)
      {
         Debug.Assert(currentClientRun != null);
         Debug.Assert(fahLogUnitData != null);
         Debug.Assert(unit != null);

         /* Project (R/C/G) (Could have already been read through Queue) */
         if (unit.ProjectIsUnknown)
         {
            unit.ProjectID = fahLogUnitData.ProjectID;
            unit.ProjectRun = fahLogUnitData.ProjectRun;
            unit.ProjectClone = fahLogUnitData.ProjectClone;
            unit.ProjectGen = fahLogUnitData.ProjectGen;
         }

         if (unitInfoLogData != null)
         {
            unit.ProteinName = unitInfoLogData.ProteinName;

            /* Tag (Could have already been read through Queue) */
            if (unit.ProteinTag.Length == 0)
            {
               unit.ProteinTag = unitInfoLogData.ProteinTag;
            }

            /* DownloadTime (Could have already been read through Queue) */
            if (unit.DownloadTimeUnknown)
            {
               unit.DownloadTime = unitInfoLogData.DownloadTime;
            }

            /* DueTime (Could have already been read through Queue) */
            if (unit.DueTimeUnknown)
            {
               unit.DueTime = unitInfoLogData.DueTime;
            }

            /* FinishedTime (Not available in unitinfo log) */

            /* Project (R/C/G) (Could have already been read through Queue) */
            if (unit.ProjectIsUnknown)
            {
               unit.ProjectID = unitInfoLogData.ProjectID;
               unit.ProjectRun = unitInfoLogData.ProjectRun;
               unit.ProjectClone = unitInfoLogData.ProjectClone;
               unit.ProjectGen = unitInfoLogData.ProjectGen;
            }
         }

         /* FoldingID and Team from Last Client Run (Could have already been read through Queue) */
         if (unit.FoldingID.Equals(Default.FoldingIDDefault))
         {
            unit.FoldingID = currentClientRun.FoldingID;
         }
         if (unit.Team == Default.TeamDefault)
         {
            unit.Team = currentClientRun.Team;
         }
      }

      private static void ParseFrameData(IEnumerable<LogLine> frameData, UnitInfo unit)
      {
         foreach (var logLine in frameData)
         {
            // Check for FrameData
            var frame = logLine.LineData as UnitFrame;
            if (frame == null)
            {
               // If not found, clear the LineType and get out
               logLine.LineType = LogLineType.Unknown;
               continue;
            }
            
            unit.SetCurrentFrame(frame);
         }
      }
      #endregion
      
      #endregion
   }
}
