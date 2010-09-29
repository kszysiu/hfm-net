/*
 * HFM.NET - Display Instance Class
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;

using ProtoBuf;

using HFM.Framework;

namespace HFM.Instances
{
   [ProtoContract]
   public class DisplayInstance : IDisplayInstance
   {
      public const int NumberOfDisplayFields = 19;

      /// <summary>
      /// Name (key) of ExternalInstance that owns this DisplayInstance (otherwise null)
      /// </summary>
      public string ExternalInstanceName { get; set; }
      
      public IPreferenceSet Prefs { get; set; }
      public IProteinCollection ProteinCollection { get; set; }
      public IProteinBenchmarkContainer BenchmarkContainer { get; set; }
      
      public IUnitInfoLogic CurrentUnitInfo { get; set; }
      
      private UnitInfo _unitInfo;
      [ProtoMember(1)]
      public UnitInfo UnitInfo
      {
         get
         {
            if (CurrentUnitInfo != null)
            {
               return (UnitInfo)CurrentUnitInfo.UnitInfoData;
            }
            return null;
         }
         set { _unitInfo = value; }
      }
      
      public IClientInstanceSettings Settings
      {
         get { return SettingsConcrete; }
      }
      [ProtoMember(2)]
      public ClientInstanceSettings SettingsConcrete { get; set; }
      
      public void BuildUnitInfoLogic()
      {
         if (_unitInfo == null) throw new InvalidOperationException();
      
         IProtein protein = ProteinCollection.GetProtein(_unitInfo.ProjectID, false);
         CurrentUnitInfo = new UnitInfoLogic(Prefs, protein, BenchmarkContainer, _unitInfo, Settings, this);
         _unitInfo = null;
      }

      /// <summary>
      /// Client Startup Arguments
      /// </summary>
      [ProtoMember(3)]
      public string Arguments { get; set; }

      /// <summary>
      /// User ID associated with this client
      /// </summary>
      [ProtoMember(4)]
      public string UserId { get; set; }

      /// <summary>
      /// True if User ID is Unknown
      /// </summary>
      public bool UserIdUnknown
      {
         get { return UserId.Length == 0; }
      }

      /// <summary>
      /// Machine ID associated with this client
      /// </summary>
      [ProtoMember(5)]
      public int MachineId { get; set; }

      /// <summary>
      /// Combined User ID and Machine ID String
      /// </summary>
      public string UserAndMachineId
      {
         get { return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", UserId, MachineId); }
      }

      /// <summary>
      /// The Folding ID (Username) attached to this client
      /// </summary>
      public string FoldingID
      {
         get { return CurrentUnitInfo.UnitInfoData.FoldingID; }
      }

      /// <summary>
      /// The Team number attached to this client
      /// </summary>
      public int Team
      {
         get { return CurrentUnitInfo.UnitInfoData.Team; }
      }

      /// <summary>
      /// Combined Folding ID and Team String
      /// </summary>
      public string FoldingIDAndTeam
      {
         get { return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", FoldingID, Team); }
      }

      #region IDisplayInstance Members

      #region Grid Properties

      /// <summary>
      /// Status of this client
      /// </summary>
      [ProtoMember(6)]
      public ClientStatus Status { get; set; }

      public float Progress
      {
         get { return ((float)PercentComplete) / 100; }
      }

      public string Name
      {
         get { return Settings.InstanceName; }
      }

      public string ClientType
      {
         get
         {
            if (Prefs.GetPreference<bool>(Preference.ShowVersions) && String.IsNullOrEmpty(ClientVersion) == false)
            {
               return String.Format(CultureInfo.CurrentCulture, "{0} ({1})", TypeOfClient, ClientVersion);
            }
            return TypeOfClient.ToString();
         }
      }
      
      /// <summary>
      /// Time per frame (TPF) of the unit
      /// </summary>
      public TimeSpan TPF
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.TimePerFrame;
            }

            return TimeSpan.Zero;
         }
      }
      
      /// <summary>
      /// Points per day (PPD) rating for this instance
      /// </summary>
      public double PPD
      {
         get
         {
            if (ProductionValuesOk)
            {
               return Math.Round(CurrentUnitInfo.PPD, Prefs.GetPreference<int>(Preference.DecimalPlaces));
            }

            return 0;
         }
      }
      
      /// <summary>
      /// Units per day (UPD) rating for this instance
      /// </summary>
      public double UPD
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.UPD;
            }

            return 0;
         }
      }

      public int MHz
      {
         get { return Settings.ClientProcessorMegahertz; }
      }

      public double PPD_MHz
      {
         get { return Math.Round(PPD / MHz, 3); }
      }

      /// <summary>
      /// Esimated time of arrival (ETA) for this protein
      /// </summary>
      public TimeSpan ETA
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.ETA;
            }

            return TimeSpan.Zero;
         }
      }
      
      public string Core
      {
         get
         {
            if (Prefs.GetPreference<bool>(Preference.ShowVersions) && String.IsNullOrEmpty(CoreVersion) == false)
            {
               return String.Format(CultureInfo.CurrentCulture, "{0} ({1})", CurrentUnitInfo.CurrentProtein.Core, CoreVersion);
            }
            return CurrentUnitInfo.CurrentProtein.Core;
         }
      }

      public string CoreID
      {
         get { return CurrentUnitInfo.UnitInfoData.CoreID; }
      }

      public string ProjectRunCloneGen
      {
         get { return CurrentUnitInfo.ProjectRunCloneGen; }
      }

      public double Credit
      {
         get
         {
            // Issue 125
            if (ProductionValuesOk && Prefs.GetPreference<bool>(Preference.CalculateBonus))
            {
               return GetBonusCredit();
            }

            return CurrentUnitInfo.CurrentProtein.Credit;
         }
      }

      /// <summary>
      /// Get the Credit of the Unit (including bonus)
      /// </summary>
      [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
      private double GetBonusCredit()
      {
         // Issue 183
         if (Status.Equals(ClientStatus.RunningAsync) ||
             Status.Equals(ClientStatus.RunningNoFrameTimes))
         {
            return CurrentUnitInfo.CurrentProtein.GetBonusCredit(CurrentUnitInfo.EftByFrameTime);
         }

         return CurrentUnitInfo.CurrentProtein.GetBonusCredit(CurrentUnitInfo.EftByDownloadTime);
      }
      
      public int Complete
      {
         get
         {
            if (Prefs.GetPreference<CompletedCountDisplayType>(Preference.CompletedCountDisplay).Equals(CompletedCountDisplayType.ClientTotal))
            {
               return TotalClientCompletedUnits;
            }
            return TotalRunCompletedUnits;
         }
      }

      /// <summary>
      /// Number of failed units since the last client start
      /// </summary>
      [ProtoMember(7)]
      public int TotalRunFailedUnits { get; set; }

      public string Username
      {
         get { return FoldingIDAndTeam; }
      }

      public DateTime DownloadTime
      {
         get { return CurrentUnitInfo.DownloadTime; }
      }

      public DateTime PreferredDeadline
      {
         get { return CurrentUnitInfo.PreferredDeadline; }
      }

      #endregion

      #region Complex Interfaces

      /// <summary>
      /// Class member containing info on the currently running protein
      /// </summary>
      public IProtein CurrentProtein
      {
         get { return CurrentUnitInfo.CurrentProtein; }
      }

      /// <summary>
      /// Current Log Lines based on UnitLogLines Array and CurrentUnitIndex
      /// </summary>
      [ProtoMember(8)]
      public IList<LogLine> CurrentLogLines { get; set; }

      /// <summary>
      /// Return LogLine List for Specified Queue Index
      /// </summary>
      /// <param name="queueIndex">Index in Queue</param>
      /// <exception cref="ArgumentOutOfRangeException">If queueIndex is outside the bounds of the Log Lines Array</exception>
      public IList<LogLine> GetLogLinesForQueueIndex(int queueIndex)
      {
         if (UnitLogLines == null) return null;

         // Check the UnitLogLines array against the requested Queue Index - Issue 171
         if (queueIndex < 0 || queueIndex > UnitLogLines.Length - 1)
         {
            throw new ArgumentOutOfRangeException("QueueIndex", String.Format(CultureInfo.CurrentCulture,
               "Index is out of range.  Requested Index: {0}.  Array Length: {1}", queueIndex, UnitLogLines.Length));
         }

         if (UnitLogLines[queueIndex] != null)
         {
            return UnitLogLines[queueIndex];
         }

         return null;
      }

      public IList<LogLine>[] UnitLogLines { get; set; }

      /// <summary>
      /// Queue Base Interface
      /// </summary>
      [CLSCompliant(false)]
      public IQueueBase Queue { get; set; }
      
      #endregion

      private DateTime _lastRetrievalTime = DateTime.MinValue;
      /// <summary>
      /// When the log files were last successfully retrieved
      /// </summary>
      [ProtoMember(9)]
      public DateTime LastRetrievalTime
      {
         get { return _lastRetrievalTime; }
         set { _lastRetrievalTime = value; }
      }

      /// <summary>
      /// Flag denoting if Progress, Production, and Time based values are OK to Display
      /// </summary>
      public bool ProductionValuesOk
      {
         get
         {
            if (Status.Equals(ClientStatus.Running) ||
                Status.Equals(ClientStatus.RunningAsync) ||
                Status.Equals(ClientStatus.RunningNoFrameTimes))
            {
               return true;
            }

            return false;
         }
      }

      /// <summary>
      /// Client host type (Path, FTP, or HTTP)
      /// </summary>
      public InstanceType InstanceHostType
      {
         get { return Settings.InstanceHostType; }
      }

      /// <summary>
      /// Location of log files for this instance
      /// </summary>
      public string Path
      {
         get { return Settings.Path; }
      }

      /// <summary>
      /// Client Path and Arguments (If Arguments Exist)
      /// </summary>
      public string ClientPathAndArguments
      {
         get
         {
            if (Arguments.Length == 0)
            {
               return Path;
            }

            return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", Path, Arguments);
         }
      }

      /// <summary>
      /// Specifies that this client is on a VM that reports local time as UTC
      /// </summary>
      public bool ClientIsOnVirtualMachine
      {
         get { return Settings.ClientIsOnVirtualMachine; }
      }

      /// <summary>
      /// Frame progress of the unit
      /// </summary>
      public int FramesComplete
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.FramesComplete;
            }

            return 0;
         }
      }
      
      /// <summary>
      /// Current progress (percentage) of the unit
      /// </summary>
      public int PercentComplete
      {
         get
         {
            if (ProductionValuesOk ||
                Status.Equals(ClientStatus.Paused))
            {
               return CurrentUnitInfo.PercentComplete;
            }

            return 0;
         }
      }

      /// <summary>
      /// Client Type for this work unit
      /// </summary>
      public ClientType TypeOfClient
      {
         get { return CurrentUnitInfo.UnitInfoData.TypeOfClient; }
      }

      /// <summary>
      /// Client Version
      /// </summary>
      [ProtoMember(10)]
      public string ClientVersion { get; set; }

      public string CoreName
      {
         get { return CurrentUnitInfo.CurrentProtein.Core; }
      }

      public string CoreVersion
      {
         get { return CurrentUnitInfo.UnitInfoData.CoreVersion; }
      }

      /// <summary>
      /// Project ID Number
      /// </summary>
      public int ProjectID
      {
         get { return CurrentUnitInfo.UnitInfoData.ProjectID; }
      }

      /// <summary>
      /// User ID is a Duplicate of another Client's User ID
      /// </summary>
      public bool UserIdIsDuplicate { get; set; }

      /// <summary>
      /// Project (R/C/G) is a Duplicate of another Client's Project (R/C/G)
      /// </summary>
      public bool ProjectIsDuplicate { get; set; }

      public bool UsernameOk
      {
         get
         {
            // if these are the default assigned values, don't check otherwise and just return true
            if (FoldingID == Constants.FoldingIDDefault && Team == Constants.TeamDefault)
            {
               return true;
            }

            if ((FoldingID != Prefs.GetPreference<string>(Preference.StanfordId) ||
                      Team != Prefs.GetPreference<int>(Preference.TeamId)) &&
                (Status.Equals(ClientStatus.Unknown) == false && Status.Equals(ClientStatus.Offline) == false))
            {
               return false;
            }

            return true;
         }
      }

      /// <summary>
      /// Number of completed units since the last client start
      /// </summary>
      [ProtoMember(11)]
      public int TotalRunCompletedUnits { get; set; }

      /// <summary>
      /// Total Units Completed for lifetime of the client (read from log file)
      /// </summary>
      [ProtoMember(12)]
      public int TotalClientCompletedUnits { get; set; }

      /// <summary>
      /// Cached FAHlog Filename for this instance
      /// </summary>
      public string CachedFahLogName
      {
         get { return Settings.CachedFahLogName; }
      }

      /// <summary>
      /// Esimated time of arrival (ETA) for this protein
      /// </summary>
      public DateTime EtaDate
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.EtaDate;
            }

            return DateTime.MinValue;
         }
      }

      #endregion

      public bool Owns(IOwnedByClientInstance value)
      {
         // External Instances don't own anything local
         if (ExternalInstanceName == null &&
             value.OwningInstanceName.Equals(Settings.InstanceName) &&
             StringOps.PathsEqual(value.OwningInstancePath, Settings.Path))
         {
            return true;
         }

         return false;
      }

      [CoverageExclude]
      public static void SetupDataGridViewColumns(DataGridView dataGridView1)
      {
         // ReSharper disable PossibleNullReferenceException
         dataGridView1.Columns.Add("Status", "Status");
         dataGridView1.Columns["Status"].DataPropertyName = "Status";
         dataGridView1.Columns.Add("Progress", "Progress");
         dataGridView1.Columns["Progress"].DataPropertyName = "Progress";
         var progressStyle = new DataGridViewCellStyle { Format = "00%" };
         dataGridView1.Columns["Progress"].DefaultCellStyle = progressStyle;
         dataGridView1.Columns.Add("Name", "Name");
         dataGridView1.Columns["Name"].DataPropertyName = "Name";
         dataGridView1.Columns.Add("ClientType", "Client Type");
         dataGridView1.Columns["ClientType"].DataPropertyName = "ClientType";
         dataGridView1.Columns.Add("TPF", "TPF");
         dataGridView1.Columns["TPF"].DataPropertyName = "TPF";
         dataGridView1.Columns.Add("PPD", "PPD");
         dataGridView1.Columns["PPD"].DataPropertyName = "PPD";
         dataGridView1.Columns.Add("MHz", "MHz");
         dataGridView1.Columns["MHz"].DataPropertyName = "MHz";
         dataGridView1.Columns.Add("PPD_MHz", "PPD/MHz");
         dataGridView1.Columns["PPD_MHz"].DataPropertyName = "PPD_MHz";
         dataGridView1.Columns.Add("ETA", "ETA");
         dataGridView1.Columns["ETA"].DataPropertyName = "ETA";
         dataGridView1.Columns.Add("Core", "Core");
         dataGridView1.Columns["Core"].DataPropertyName = "Core";
         dataGridView1.Columns.Add("CoreID", "Core ID");
         dataGridView1.Columns["CoreID"].DataPropertyName = "CoreID";
         dataGridView1.Columns.Add("ProjectRunCloneGen", "Project (Run, Clone, Gen)");
         dataGridView1.Columns["ProjectRunCloneGen"].DataPropertyName = "ProjectRunCloneGen";
         dataGridView1.Columns.Add("Credit", "Credit");
         dataGridView1.Columns["Credit"].DataPropertyName = "Credit";
         dataGridView1.Columns.Add("Complete", "Complete");
         dataGridView1.Columns["Complete"].DataPropertyName = "Complete";
         dataGridView1.Columns.Add("Failed", "Failed");
         dataGridView1.Columns["Failed"].DataPropertyName = "TotalRunFailedUnits";
         dataGridView1.Columns.Add("Username", "User Name");
         dataGridView1.Columns["Username"].DataPropertyName = "Username";
         dataGridView1.Columns.Add("DownloadTime", "Download Time");
         dataGridView1.Columns["DownloadTime"].DataPropertyName = "DownloadTime";
         dataGridView1.Columns.Add("Deadline", "Deadline");
         dataGridView1.Columns["Deadline"].DataPropertyName = "PreferredDeadline";
         dataGridView1.Columns.Add("Dummy", String.Empty);
         //dataGridView1.Columns["Dummy"].DataPropertyName = "Dummy";
         // ReSharper restore PossibleNullReferenceException
      }
   }
}
