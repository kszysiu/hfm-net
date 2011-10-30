﻿/*
 * HFM.NET - Framework.DataTypes Enumerations
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

namespace HFM.Core.DataTypes
{
   /// <summary>
   /// Client Types
   /// </summary>
   public enum ClientType
   {
      Version7,
      Legacy,
      External //?
   }

   /// <summary>
   /// Legacy Client Sub Types
   /// </summary>
   public enum LegacyClientSubType
   {
      None,
      Path,
      Ftp,
      Http
   }

   /// <summary>
   /// Folding Slot Type
   /// </summary>
   public enum SlotType
   {
      Unknown, // for Legacy Clients
      Uniprocessor,
      SMP,
      GPU
   }

   /// <summary>
   /// Ftp Transfer Types
   /// </summary>
   public enum FtpType
   {
      Passive,
      Active
   }

   /// <summary>
   /// Log Line Types
   /// </summary>
   public enum LogLineType
   {
      Error = -2,
      Unknown = -1,
      LogOpen = 0,
      LogHeader,
      ClientVersion,
      ClientSendWorkToServer,
      ClientAutosendStart,
      ClientAutosendComplete,
      ClientSendStart,
      ClientSendConnectFailed,
      ClientSendFailed,
      ClientSendComplete,
      ClientArguments,
      ClientUserNameTeam,
      ClientRequestingUserID,
      ClientReceivedUserID,
      ClientUserID,
      ClientMachineID,
      ClientAttemptGetWorkPacket,
      ClientIndicateMemory,
      ClientDetectCpu,
      WorkUnitProcessing,
      WorkUnitCoreDownload,
      WorkUnitIndex,
      WorkUnitQueueIndex,
      WorkUnitWorking,
      WorkUnitStart,
      WorkUnitCoreVersion,
      WorkUnitRunning,
      WorkUnitProject,
      WorkUnitFrame,
      WorkUnitPaused,
      WorkUnitPausedForBattery,
      WorkUnitResumeFromBattery,
      WorkUnitShuttingDownCore,
      WorkUnitCoreShutdown,
      WorkUnitCoreReturn,           // v7 specific
      WorkUnitCleaningUp,           // v7 specific
      ClientNumberOfUnitsCompleted,
      ClientCoreCommunicationsError,
      ClientCoreCommunicationsErrorShutdown,
      ClientEuePauseState,
      ClientShutdown
   }

   /// <summary>
   /// Work Unit Result Types
   /// </summary>
   public enum WorkUnitResult
   {
      Unknown,
      FinishedUnit,
      EarlyUnitEnd,
      UnstableMachine,
      Interrupted,
      BadWorkUnit,
      CoreOutdated,
      ClientCoreError
   }

   /// <summary>
   /// Client Status Types
   /// </summary>
   public enum ClientStatus
   {
      Unknown,
      Offline,
      Stopped,
      EuePause,
      Hung,
      Paused,
      SendingWorkPacket,
      GettingWorkPacket,
      RunningNoFrameTimes,
      RunningAsync,
      Running
   }

   /// <summary>
   /// Queue Entry Status Types
   /// </summary>
   public enum QueueEntryStatus
   {
      Unknown,
      Empty,
      Deleted,
      Finished,
      Garbage,
      FoldingNow,
      Queued,
      ReadyForUpload,
      Abandonded,
      FetchingFromServer
   }

   /// <summary>
   /// Work Unit History Production View Type
   /// </summary>
   public enum HistoryProductionView
   {
      BonusDownloadTime,
      BonusFrameTime,
      Standard
   }

   /// <summary>
   /// Work Unit History Query Field Names
   /// </summary>
   public enum QueryFieldName
   {
      ID = -1,
      ProjectID = 0,
      ProjectRun,
      ProjectClone,
      ProjectGen,
      InstanceName,
      InstancePath,
      Username,
      Team,
      CoreVersion,
      FramesCompleted,
      FrameTime,
      Result,
      DownloadDateTime,
      CompletionDateTime,
      WorkUnitName,
      KFactor,
      Core,
      Frames,
      Atoms,
      ClientType,
      PPD,
      Credit
   }

   /// <summary>
   /// Work Unit History Query Field Types
   /// </summary>
   public enum QueryFieldType
   {
      Equal,
      GreaterThan,
      GreaterThanOrEqual,
      LessThan,
      LessThanOrEqual,
   }

   /// <summary>
   /// Work Unit Log Filter Types
   /// </summary>
   public enum LogFilterType
   {
      IndexOnly,
      IndexAndNonIndexed
   }
}