/*
 * HFM.NET - Client Instance Class
 * Copyright (C) 2006 David Rawling
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
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

using HFM.Framework;
using HFM.Helpers;
using HFM.Instrumentation;

namespace HFM.Instances
{
   public class ClientInstance : IClientInstance
   {
      #region Constants
      // Default ID Constants
      public const string DefaultUserID = "";
      public const int DefaultMachineID = 0;

      /// <summary>
      /// UnitInfo Log File Maximum Download Size.
      /// </summary>
      private const int UnitInfoMax = 1048576; // 1 Megabyte
      #endregion

      /// <summary>
      /// PreferenceSet Interface
      /// </summary>
      private readonly IPreferenceSet _prefs;
      
      /// <summary>
      /// Protein Collection Interface
      /// </summary>
      private readonly IProteinCollection _proteinCollection;
      
      /// <summary>
      /// Protein Collection Interface
      /// </summary>
      private readonly IProteinBenchmarkContainer _benchmarkContainer;
      
      private readonly IDataAggregator _dataAggregator;
      /// <summary>
      /// Data Aggregator Interface
      /// </summary>
      [CLSCompliant(false)]
      public IDataAggregator DataAggregator
      {
         get { return _dataAggregator; }
      }

      private readonly ClientInstanceSettings _settings;
      /// <summary>
      /// Client Instance Settings
      /// </summary>
      public IClientInstanceSettings Settings
      {
         get { return _settings; }
      }

      /// <summary>
      /// Client Instance Settings
      /// </summary>
      public ClientInstanceSettings SettingsConcrete
      {
         get { return _settings; }
      }
      
      #region Constructor
      /// <summary>
      /// Primary Constructor
      /// </summary>
      public ClientInstance(IPreferenceSet prefs, IProteinCollection proteinCollection, IProteinBenchmarkContainer benchmarkContainer)
         : this(prefs, proteinCollection, benchmarkContainer, null)
      {
         
      }
      
      /// <summary>
      /// Primary Constructor
      /// </summary>
      public ClientInstance(IPreferenceSet prefs, IProteinCollection proteinCollection, IProteinBenchmarkContainer benchmarkContainer, ClientInstanceSettings instanceSettings)
      {
         _prefs = prefs;
         _proteinCollection = proteinCollection;
         _benchmarkContainer = benchmarkContainer;
         _dataAggregator = InstanceProvider.GetInstance<IDataAggregator>();
         // Init User Specified Client Level Members
         if (instanceSettings == null)
         {
            _settings = new ClientInstanceSettings(InstanceType.PathInstance);
         }
         else
         {
            _settings = instanceSettings;
         }
         
         // Init Client Level Members
         Init();
         // Create a fresh UnitInfo
         _currentUnitInfo = new UnitInfoLogic(_prefs, _proteinCollection, _benchmarkContainer, new UnitInfo(), this);
      }
      #endregion

      #region Client Level Members
      private ClientStatus _status;
      /// <summary>
      /// Status of this client
      /// </summary>
      public ClientStatus Status
      {
         get { return _status; }
         set
         {
            if (_status != value)
            {
               _status = value;
               //OnStatusChanged(EventArgs.Empty);
            }
         }
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
      /// Client Version
      /// </summary>
      public string ClientVersion { get; set; }

      /// <summary>
      /// Client Startup Arguments
      /// </summary>
      public string Arguments { get; set; }

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
      /// User ID associated with this client
      /// </summary>
      public string UserId { get; set; }
      
      /// <summary>
      /// User ID is a Duplicate of another Client's User ID
      /// </summary>
      public bool UserIdIsDuplicate { get; set; }

      /// <summary>
      /// Project (R/C/G) is a Duplicate of another Client's Project (R/C/G)
      /// </summary>
      public bool ProjectIsDuplicate { get; set; }

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
      public string FoldingID { get; set; }

      /// <summary>
      /// The Team number attached to this client
      /// </summary>
      public int Team { get; set; }

      /// <summary>
      /// Combined Folding ID and Team String
      /// </summary>
      public string FoldingIDAndTeam
      {
         get { return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", FoldingID, Team); }
      }

      /// <summary>
      /// Number of completed units since the last client start
      /// </summary>
      public int TotalRunCompletedUnits { get; set; }

      /// <summary>
      /// Number of failed units since the last client start
      /// </summary>
      public int TotalRunFailedUnits { get; set; }

      /// <summary>
      /// Total Units Completed for lifetime of the client (read from log file)
      /// </summary>
      public int TotalClientCompletedUnits { get; set; }

      private UnitInfoLogic _currentUnitInfo;
      /// <summary>
      /// Class member containing info specific to the current work unit
      /// </summary>
      public UnitInfoLogic CurrentUnitInfoConcrete
      {
         get { return _currentUnitInfo; }
         protected set
         {
            UpdateTimeOfLastProgress(value);
            _currentUnitInfo = value;
         }
      }
      
      /// <summary>
      /// Class member containing info specific to the current work unit
      /// </summary>
      public IUnitInfoLogic CurrentUnitInfo
      {
         get { return _currentUnitInfo; }
      }

      /// <summary>
      /// Init Client Level Members
      /// </summary>
      private void Init()
      {
         Arguments = String.Empty;
         UserId = DefaultUserID;
         MachineId = DefaultMachineID;
         FoldingID = Constants.FoldingIDDefault;
         Team = Constants.TeamDefault;
         TotalRunCompletedUnits = 0;
         TotalRunFailedUnits = 0;
         TotalClientCompletedUnits = 0;
      }

      /// <summary>
      /// Return LogLine List for Specified Queue Index
      /// </summary>
      /// <param name="queueIndex">Index in Queue</param>
      /// <exception cref="ArgumentOutOfRangeException">If queueIndex is outside the bounds of the Log Lines Array</exception>
      public IList<ILogLine> GetLogLinesForQueueIndex(int queueIndex)
      {
         // Check the UnitLogLines array against the requested Queue Index - Issue 171
         if (queueIndex < 0 || queueIndex > _dataAggregator.UnitLogLines.Length - 1)
         {
            throw new ArgumentOutOfRangeException("QueueIndex", String.Format(CultureInfo.CurrentCulture, 
               "Index is out of range.  Requested Index: {0}.  Array Length: {1}", queueIndex, _dataAggregator.UnitLogLines.Length));
         }

         if (_dataAggregator.UnitLogLines != null && 
             _dataAggregator.UnitLogLines[queueIndex] != null)
         {
            return _dataAggregator.UnitLogLines[queueIndex];
         }

         return null;
      }
      #endregion

      #region User Specified Client Level Members
      /// <summary>
      /// Client host type (Path, FTP, or HTTP)
      /// </summary>
      public InstanceType InstanceHostType
      {
         get { return Settings.InstanceHostType; }
         set { Settings.InstanceHostType = value; }
      }
      
      /// <summary>
      /// The name assigned to this client instance
      /// </summary>
      public string InstanceName
      {
         get { return Settings.InstanceName; }
         set { Settings.InstanceName = value; }
      }

      #region Cached Log File Name Properties
      /// <summary>
      /// Cached FAHlog Filename for this instance
      /// </summary>
      public string CachedFAHLogName
      {
         get { return String.Format("{0}-{1}", InstanceName, Constants.LocalFAHLog); }
      }

      /// <summary>
      /// Cached UnitInfo Filename for this instance
      /// </summary>
      public string CachedUnitInfoName
      {
         get { return String.Format("{0}-{1}", InstanceName, Constants.LocalUnitInfo); }
      }

      /// <summary>
      /// Cached Queue Filename for this instance
      /// </summary>
      public string CachedQueueName
      {
         get { return String.Format("{0}-{1}", InstanceName, Constants.LocalQueue); }
      }
      #endregion

      /// <summary>
      /// The number of processor megahertz for this client instance
      /// </summary>
      public int ClientProcessorMegahertz
      {
         get { return Settings.ClientProcessorMegahertz; }
         set { Settings.ClientProcessorMegahertz = value; }
      }

      /// <summary>
      /// Remote client log file name
      /// </summary>
      public string RemoteFAHLogFilename
      {
         get { return Settings.RemoteFAHLogFilename; }
         set { Settings.RemoteFAHLogFilename = value; }
      }

      /// <summary>
      /// Remote client unit info log file name
      /// </summary>
      public string RemoteUnitInfoFilename
      {
         get { return Settings.RemoteUnitInfoFilename; }
         set { Settings.RemoteUnitInfoFilename = value; }
      }

      /// <summary>
      /// Remote client queue.dat file name
      /// </summary>
      public string RemoteQueueFilename
      {
         get { return Settings.RemoteQueueFilename; }
         set { Settings.RemoteQueueFilename = value; }
      }

      /// <summary>
      /// Location of log files for this instance
      /// </summary>
      public string Path
      {
         get { return Settings.Path; }
         set { Settings.Path = value; }
      }

      /// <summary>
      /// FTP Server name or IP Address
      /// </summary>
      public string Server
      {
         get { return Settings.Server; }
         set { Settings.Server = value; }
      }

      /// <summary>
      /// Username on remote server
      /// </summary>
      public string Username
      {
         get { return Settings.Username; }
         set { Settings.Username = value; }
      }

      /// <summary>
      /// Password on remote server
      /// </summary>
      public string Password
      {
         get { return Settings.Password; }
         set { Settings.Password = value; }
      }

      /// <summary>
      /// Specifies the FTP Communication Mode for this client
      /// </summary>
      public FtpType FtpMode
      {
         get { return Settings.FtpMode; }
         set { Settings.FtpMode = value; }
      }

      /// <summary>
      /// Specifies that this client is on a VM that reports local time as UTC
      /// </summary>
      public bool ClientIsOnVirtualMachine
      {
         get { return Settings.ClientIsOnVirtualMachine; }
         set { Settings.ClientIsOnVirtualMachine = value; }
      }

      /// <summary>
      /// Specifies the number of minutes (+/-) this client's clock differentiates
      /// </summary>
      public int ClientTimeOffset
      {
         get { return Settings.ClientTimeOffset; }
         set { Settings.ClientTimeOffset = value; }
      }
      #endregion

      #region Unit Progress Client Level Members
      private DateTime _timeOfLastUnitStart = DateTime.MinValue;
      /// <summary>
      /// Local Time when this Client last detected Frame Progress
      /// </summary>
      internal DateTime TimeOfLastUnitStart
      {
         get { return _timeOfLastUnitStart; }
         set { _timeOfLastUnitStart = value; }
      }

      private DateTime _timeOfLastFrameProgress = DateTime.MinValue;
      /// <summary>
      /// Local Time when this Client last detected Frame Progress
      /// </summary>
      internal DateTime TimeOfLastFrameProgress
      {
         get { return _timeOfLastFrameProgress; }
         set { _timeOfLastFrameProgress = value; }
      } 
      #endregion

      #region CurrentUnitInfo Pass-Through Properties
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
      /// Time per frame (TPF) of the unit
      /// </summary>
      public TimeSpan TimePerFrame
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

      /// <summary>
      /// Points per day (PPD) rating for this instance
      /// </summary>
      public double PPD
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.PPD;
            }

            return 0;
         }
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

      ///// <summary>
      ///// Esimated Finishing Time for this unit
      ///// </summary>
      //public TimeSpan EFT
      //{
      //   get
      //   {
      //      if (ProductionValuesOk)
      //      {
      //         return CurrentUnitInfo.EFT;
      //      }

      //      return TimeSpan.Zero;
      //   }
      //}

      public double Credit
      {
         get
         {
            // Issue 125
            if (ProductionValuesOk && _prefs.GetPreference<bool>(Preference.CalculateBonus))
            {
               return CurrentUnitInfo.GetBonusCredit();
            }

            return CurrentUnitInfo.Credit;
         }
      }
      #endregion

      #region Retrieval Properties
      private bool _HandleStatusOnRetrieve = true;
      /// <summary>
      /// Local flag set when retrieval acts on the status returned from parse
      /// </summary>
      public bool HandleStatusOnRetrieve
      {
         get { return _HandleStatusOnRetrieve; }
         set { _HandleStatusOnRetrieve = value; }
      }

      private volatile bool _retrievalInProgress;
      /// <summary>
      /// Local flag set when log retrieval is in progress
      /// </summary>
      public bool RetrievalInProgress
      {
         get { return _retrievalInProgress; }
         protected set 
         { 
            _retrievalInProgress = value;
         }
      }

      private DateTime _lastRetrievalTime = DateTime.MinValue;
      /// <summary>
      /// When the log files were last successfully retrieved
      /// </summary>
      public DateTime LastRetrievalTime
      {
         get { return _lastRetrievalTime; }
         protected set
         {
            _lastRetrievalTime = value;
         }
      }
      #endregion

      #region Retrieval Methods
      /// <summary>
      /// Retrieve Instance Log Files based on Instance Type
      /// </summary>
      public void Retrieve()
      {
         // Don't allow this to fire more than once at a time
         if (RetrievalInProgress) return;

         try
         {
            RetrievalInProgress = true;

            switch (InstanceHostType)
            {
               case InstanceType.PathInstance:
                  RetrievePathInstance();
                  break;
               case InstanceType.HttpInstance:
                  RetrieveHTTPInstance();
                  break;
               case InstanceType.FtpInstance:
                  RetrieveFTPInstance();
                  break;
               default:
                  throw new NotImplementedException(String.Format(CultureInfo.CurrentCulture,
                     "Instance Type '{0}' is not implemented", InstanceHostType));
            }

            // Re-Init Client Level Members Before Processing
            Init();
            // Process the retrieved logs
            ClientStatus returnedStatus = ProcessExisting();

            /*** Setting this flag false aids in Unit Test since the results of *
             *   determining status are relative to the current time of day. ***/
            if (HandleStatusOnRetrieve)
            {
               // Handle the status retured from the log parse
               HandleReturnedStatus(returnedStatus);
            }
            else
            {
               Status = returnedStatus;
            }
         }
         catch (Exception ex)
         {
            Status = ClientStatus.Offline;
            HfmTrace.WriteToHfmConsole(Settings.InstanceName, ex);
         }
         finally
         {
            RetrievalInProgress = false;
         }

         HfmTrace.WriteToHfmConsole(TraceLevel.Info, String.Format("{0} ({1}) Client Status: {2}", HfmTrace.FunctionName, InstanceName, Status));
      }

      /// <summary>
      /// Retrieve the log and unit info files from the configured Local path
      /// </summary>
      private void RetrievePathInstance()
      {
         DateTime start = HfmTrace.ExecStart;

         try
         {
            FileInfo fiLog = new FileInfo(System.IO.Path.Combine(Path, RemoteFAHLogFilename));
            string FAHLog_txt = System.IO.Path.Combine(_prefs.CacheDirectory, CachedFAHLogName);
            FileInfo fiCachedLog = new FileInfo(FAHLog_txt);

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "FAHlog copy (start)");
            
            if (fiLog.Exists)
            {
               if (fiCachedLog.Exists == false || fiLog.Length != fiCachedLog.Length)
               {
                  fiLog.CopyTo(FAHLog_txt, true);
                  HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "FAHlog copy (success)");
               }
               else
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "FAHlog copy (file has not changed)");
               }
            }
            else
            {
               throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, 
                  "The path {0} is inaccessible.", fiLog.FullName));
            }

            // Retrieve unitinfo.txt (or equivalent)
            FileInfo fiUI = new FileInfo(System.IO.Path.Combine(Path, RemoteUnitInfoFilename));
            string UnitInfo_txt = System.IO.Path.Combine(_prefs.CacheDirectory, CachedUnitInfoName);

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "unitinfo copy (start)");
            
            if (fiUI.Exists)
            {
               // If file size is too large, do not copy it and delete the current cached copy - Issue 2
               if (fiUI.Length < UnitInfoMax)
               {
                  fiUI.CopyTo(UnitInfo_txt, true);
                  HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "unitinfo copy (success)");
               }
               else
               {
                  if (File.Exists(UnitInfo_txt))
                  {
                     File.Delete(UnitInfo_txt);
                  }
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                     "unitinfo copy (file is too big: {0} bytes).", fiUI.Length));
               }
            }
            /*** Remove Requirement for UnitInfo to be Present ***/
            else
            {
               if (File.Exists(UnitInfo_txt))
               {
                  File.Delete(UnitInfo_txt);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "The path {0} is inaccessible.", fiUI.FullName));
            }

            // Retrieve queue.dat (or equivalent)
            FileInfo fiQueue = new FileInfo(System.IO.Path.Combine(Path, RemoteQueueFilename));
            string Queue_dat = System.IO.Path.Combine(_prefs.CacheDirectory, CachedQueueName);

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "queue copy (start)");
            
            if (fiQueue.Exists)
            {
               fiQueue.CopyTo(Queue_dat, true);
               HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "queue copy (success)");
            }
            /*** Remove Requirement for Queue to be Present ***/
            else
            {
               if (File.Exists(Queue_dat))
               {
                  File.Delete(Queue_dat);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "The path {0} is inaccessible.", fiQueue.FullName));
            }

            LastRetrievalTime = DateTime.Now;
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, InstanceName, start);
         }
      }

      /// <summary>
      /// Retrieve the log and unit info files from the configured HTTP location
      /// </summary>
      private void RetrieveHTTPInstance()
      {
         DateTime start = HfmTrace.ExecStart;

         NetworkOps net = new NetworkOps();

         try
         {
            string HttpPath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Path, "/", RemoteFAHLogFilename);
            string LocalFile = System.IO.Path.Combine(_prefs.CacheDirectory, CachedFAHLogName);
            net.HttpDownloadHelper(HttpPath, LocalFile, Username, Password);

            try
            {
               HttpPath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Path, "/", RemoteUnitInfoFilename);
               LocalFile = System.IO.Path.Combine(_prefs.CacheDirectory, CachedUnitInfoName);

               long length = net.GetHttpDownloadLength(HttpPath, Username, Password);
               if (length < UnitInfoMax)
               {
                  net.HttpDownloadHelper(HttpPath, LocalFile, Username, Password);
               }
               else
               {
                  if (File.Exists(LocalFile))
                  {
                     File.Delete(LocalFile);
                  }

                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                     "unitinfo download (file is too big: {0} bytes).", length));
               }
            }
            /*** Remove Requirement for UnitInfo to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFile))
               {
                  File.Delete(LocalFile);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "unitinfo download failed: {0}", ex.Message));
            }

            try
            {
               HttpPath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Path, "/", RemoteQueueFilename);
               LocalFile = System.IO.Path.Combine(_prefs.CacheDirectory, CachedQueueName);
               net.HttpDownloadHelper(HttpPath, LocalFile, Username, Password);
            }
            /*** Remove Requirement for Queue to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFile))
               {
                  File.Delete(LocalFile);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                  "queue download failed: {0}", ex.Message));
            }

            LastRetrievalTime = DateTime.Now;
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, InstanceName, start);
         }
      }

      /// <summary>
      /// Retrieve the log and unit info files from the configured FTP location
      /// </summary>
      private void RetrieveFTPInstance()
      {
         DateTime start = HfmTrace.ExecStart;

         NetworkOps net = new NetworkOps();

         try
         {
            string LocalFilePath = System.IO.Path.Combine(_prefs.CacheDirectory, CachedFAHLogName);
            net.FtpDownloadHelper(Server, Path, RemoteFAHLogFilename, LocalFilePath, Username, Password, FtpMode);

            try
            {
               LocalFilePath = System.IO.Path.Combine(_prefs.CacheDirectory, CachedUnitInfoName);

               long length = net.GetFtpDownloadLength(Server, Path, RemoteUnitInfoFilename, Username, Password, FtpMode);
               if (length < UnitInfoMax)
               {
                  net.FtpDownloadHelper(Server, Path, RemoteUnitInfoFilename, LocalFilePath, Username, Password, FtpMode);
               }
               else
               {
                  if (File.Exists(LocalFilePath))
                  {
                     File.Delete(LocalFilePath);
                  }

                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                     "unitinfo download (file is too big: {0} bytes).", length));
               }
            }
            /*** Remove Requirement for UnitInfo to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFilePath))
               {
                  File.Delete(LocalFilePath);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "unitinfo download failed: {0}.", ex.Message));
            }

            try
            {
               LocalFilePath = System.IO.Path.Combine(_prefs.CacheDirectory, CachedQueueName);
               net.FtpDownloadHelper(Server, Path, RemoteQueueFilename, LocalFilePath, Username, Password, FtpMode);
            }
            /*** Remove Requirement for Queue to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFilePath))
               {
                  File.Delete(LocalFilePath);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                  "queue download failed: {0}", ex.Message));
            }

            LastRetrievalTime = DateTime.Now;
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, InstanceName, start);
         }
      }
      #endregion

      #region Queue and Log Processing Functions
      /// <summary>
      /// Process the cached log files that exist on this machine
      /// </summary>
      public ClientStatus ProcessExisting()
      {
         // Exec Start
         DateTime start = HfmTrace.ExecStart;

         #region Setup UnitInfo Aggregator
         _dataAggregator.InstanceName = InstanceName;
         _dataAggregator.QueueFilePath = System.IO.Path.Combine(_prefs.CacheDirectory, CachedQueueName);
         _dataAggregator.FahLogFilePath = System.IO.Path.Combine(_prefs.CacheDirectory, CachedFAHLogName);
         _dataAggregator.UnitInfoLogFilePath = System.IO.Path.Combine(_prefs.CacheDirectory, CachedUnitInfoName); 
         #endregion
         
         #region Run the Aggregator and Set ClientInstance Level Results
         IList<IUnitInfo> units = _dataAggregator.AggregateData();
         // Issue 126 - Use the Folding ID, Team, User ID, and Machine ID from the FAHlog data.
         // Use the Current Queue Entry as a backup data source.
         PopulateRunLevelData(_dataAggregator.CurrentClientRun);
         if (_dataAggregator.Queue != null)
         {
            PopulateRunLevelData(_dataAggregator.Queue.CurrentQueueEntry);
         }
         #endregion
         
         UnitInfoLogic[] parsedUnits = new UnitInfoLogic[units.Count];
         for (int i = 0; i < units.Count; i++)
         {
            if (units[i] != null)
            {
               parsedUnits[i] = new UnitInfoLogic(_prefs, _proteinCollection, _benchmarkContainer, units[i], this);
            }
         }

         // *** THIS HAS TO BE DONE BEFORE UPDATING THE CurrentUnitInfo ***
         // Update Benchmarks from parsedUnits array 
         UpdateBenchmarkData(parsedUnits, _dataAggregator.CurrentUnitIndex);

         // Update the CurrentUnitInfo if we have a Status
         ClientStatus currentWorkUnitStatus = _dataAggregator.CurrentWorkUnitStatus;
         if (currentWorkUnitStatus.Equals(ClientStatus.Unknown) == false)
         {
            CurrentUnitInfoConcrete = parsedUnits[_dataAggregator.CurrentUnitIndex];
         }
         
         CurrentUnitInfoConcrete.ShowPPDTrace();
         HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, start);

         // Return the Status
         return currentWorkUnitStatus;
      }

      private void PopulateRunLevelData(IClientRun run)
      {
         ClientVersion = run.ClientVersion;
         Arguments = run.Arguments;
      
         FoldingID = run.FoldingID;
         Team = run.Team;
         
         UserId = run.UserID;
         MachineId = run.MachineID;

         TotalRunCompletedUnits = run.NumberOfCompletedUnits;
         TotalRunFailedUnits = run.NumberOfFailedUnits;
         TotalClientCompletedUnits = run.NumberOfTotalUnitsCompleted;
      }

      private void PopulateRunLevelData(IQueueEntry queueEntry)
      {
         if (FoldingID == Constants.FoldingIDDefault)
         {
            FoldingID = queueEntry.FoldingID;
         }
         if (Team == Constants.TeamDefault)
         {
            Team = (int)queueEntry.TeamNumber;
         }
         if (UserId == DefaultUserID)
         {
            UserId = queueEntry.UserID;
         }
         if (MachineId == DefaultMachineID)
         {
            MachineId = (int)queueEntry.MachineID;
         }
      }

      /// <summary>
      /// Update Project Benchmarks
      /// </summary>
      /// <param name="parsedUnits">Parsed UnitInfo Array</param>
      /// <param name="benchmarkUpdateIndex">Index of Current UnitInfo</param>
      private void UpdateBenchmarkData(UnitInfoLogic[] parsedUnits, int benchmarkUpdateIndex)
      {
         bool FoundCurrent = false;
         bool ProcessUpdates = false;
         int index = benchmarkUpdateIndex;
         
         #region Set index for the oldest unit in the array
         if (index == parsedUnits.Length - 1)
         {
            index = 0;
         }
         else
         {
            index++;
         }
         #endregion

         while (index != -1)
         {
            // If Current has not been found, check the benchmarkUpdateIndex
            // or try to match the Current Project and Raw Download Time
            if (ProcessUpdates == false && (index == benchmarkUpdateIndex || IsUnitInfoCurrentUnitInfo(parsedUnits[index])))
            {
               FoundCurrent = true;
               ProcessUpdates = true;
            }

            if (ProcessUpdates)
            {
               int previousFrameID = 0;
               if (FoundCurrent)
               {
                  // current frame has already been recorded, increment to the next frame
                  previousFrameID = CurrentUnitInfo.LastUnitFrameID + 1;
                  FoundCurrent = false;
               }

               // Even though the CurrentUnitInfo has been found in the parsed UnitInfoLogic array doesn't
               // mean that all entries in the array will be present.  See TestFiles\SMP_12\FAHlog.txt.
               if (parsedUnits[index] != null)
               {
                  // Update benchmarks
                  _benchmarkContainer.UpdateBenchmarkData(parsedUnits[index], previousFrameID, parsedUnits[index].LastUnitFrameID);

                  // Write Completed Unit Info only for units that are NOT current (i.e. have moved into history)
                  // For some WUs (typically bigadv) all frames could be complete but the FinishedTime read from
                  // the queue.dat is not yet populated.  To write this units production using an accurate bonus
                  // multiplier that FinishedTime needs to be populated.
                  if (index != benchmarkUpdateIndex)
                  {
                     // Make sure all Frames have been completed (not necessarily observed, but completed)
                     if (parsedUnits[index].AllFramesAreCompleted)
                     {
                        UnitInfoContainer.WriteCompletedUnitInfo(parsedUnits[index]);
                     }
                  }
               }
            }

            #region Increment to the next unit or set terminal value
            if (index == benchmarkUpdateIndex)
            {
               index = -1;
            }
            else if (index == parsedUnits.Length - 1)
            {
               index = 0;
            }
            else
            {
               index++;
            }
            #endregion
         }
      }

      /// <summary>
      /// Update Time of Last Frame Progress based on Current and Parsed UnitInfo
      /// </summary>
      private void UpdateTimeOfLastProgress(IUnitInfoLogic parsedUnitInfo)
      {
         // Matches the Current Project and Raw Download Time
         if (IsUnitInfoCurrentUnitInfo(parsedUnitInfo))
         {
            // If the Unit Start Time Stamp is no longer the same as the CurrentUnitInfo
            if (parsedUnitInfo.UnitStartTimeStamp.Equals(TimeSpan.MinValue) == false &&
                CurrentUnitInfo.UnitStartTimeStamp.Equals(TimeSpan.MinValue) == false &&
                parsedUnitInfo.UnitStartTimeStamp.Equals(CurrentUnitInfo.UnitStartTimeStamp) == false)
            {
               TimeOfLastUnitStart = DateTime.Now;
            }
         
            // If the Last Unit Frame ID is greater than the CurrentUnitInfo Last Unit Frame ID
            if (parsedUnitInfo.LastUnitFrameID > CurrentUnitInfo.LastUnitFrameID)
            {
               // Update the Time Of Last Frame Progress
               TimeOfLastFrameProgress = DateTime.Now;
            }
         }
         else // Different UnitInfo - Update the Time Of Last 
              // Unit Start and Clear Frame Progress Value
         {
            TimeOfLastUnitStart = DateTime.Now;
            TimeOfLastFrameProgress = DateTime.MinValue;
         }
      }

      /// <summary>
      /// Does the given UnitInfo.ProjectRunCloneGen match the CurrentUnitInfo.ProjectRunCloneGen?
      /// </summary>
      private bool IsUnitInfoCurrentUnitInfo(IUnitInfoLogic parsedUnitInfo)
      {
         Debug.Assert(CurrentUnitInfo != null);
      
         // if the parsed Project is known
         if (parsedUnitInfo != null && parsedUnitInfo.ProjectIsUnknown == false)
         {
            // Matches the Current Project and Raw Download Time
            // DownloadTime check should be made on the Raw DownloadTime
            // value from the internal UnitInfoData data source object
            if (ProjectsMatch(parsedUnitInfo, CurrentUnitInfo) &&
                parsedUnitInfo.UnitInfoData.DownloadTime.Equals(CurrentUnitInfo.UnitInfoData.DownloadTime))
            {
               return true;
            }
         }

         return false;
      }

      private static bool ProjectsMatch(IProjectInfo project1, IProjectInfo project2)
      {
         if (project1 == null || project2 == null) return false;

         return (project1.ProjectID == project2.ProjectID &&
                 project1.ProjectRun == project2.ProjectRun &&
                 project1.ProjectClone == project2.ProjectClone &&
                 project1.ProjectGen == project2.ProjectGen);
      }
      #endregion

      #region Status Handling and Determination
      
      /// <summary>
      /// Handles the Client Status Returned by Log Parsing and then determines what values to feed the DetermineStatus routine.
      /// </summary>
      /// <param name="returnedStatus">Client Status</param>
      private void HandleReturnedStatus(ClientStatus returnedStatus)
      {
         StatusData statusData = new StatusData();
         statusData.InstanceName = InstanceName;
         statusData.TypeOfClient = CurrentUnitInfo.TypeOfClient;
         statusData.LastRetrievalTime = LastRetrievalTime;
         statusData.IgnoreUtcOffset = ClientIsOnVirtualMachine;
         statusData.UtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
         statusData.ClientTimeOffset = ClientTimeOffset;
         statusData.TimeOfLastUnitStart = TimeOfLastUnitStart;
         statusData.TimeOfLastFrameProgress = TimeOfLastFrameProgress;
         statusData.CurrentStatus = Status;
         statusData.ReturnedStatus = returnedStatus;
         statusData.FrameTime = CurrentUnitInfo.RawTimePerSection;
         statusData.AverageFrameTime = InstanceProvider.GetInstance<IProteinBenchmarkContainer>().GetBenchmarkAverageFrameTime(CurrentUnitInfo);
         statusData.TimeOfLastFrame = CurrentUnitInfo.TimeOfLastFrame;
         statusData.UnitStartTimeStamp = CurrentUnitInfo.UnitStartTimeStamp;
         statusData.AllowRunningAsync = _prefs.GetPreference<bool>(Preference.AllowRunningAsync);
      
         Status = HandleReturnedStatus(statusData, _prefs);
      }

      /// <summary>
      /// Handles the Client Status Returned by Log Parsing and then determines what values to feed the DetermineStatus routine.
      /// </summary>
      /// <param name="statusData">Client Status Data</param>
      /// <param name="Prefs">PreferenceSet Interface</param>
      public static ClientStatus HandleReturnedStatus(StatusData statusData, IPreferenceSet Prefs)
      {
         // If the returned status is EuePause and current status is not
         if (statusData.ReturnedStatus.Equals(ClientStatus.EuePause) && statusData.CurrentStatus.Equals(ClientStatus.EuePause) == false)
         {
            if (Prefs.GetPreference<bool>(Preference.EmailReportingEnabled) && 
                Prefs.GetPreference<bool>(Preference.ReportEuePause))
            {
               SendEuePauseEmail(statusData.InstanceName, Prefs);
            }
         }

         switch (statusData.ReturnedStatus)
         {
            case ClientStatus.Running:      // at this point, we should not see Running Status
            case ClientStatus.RunningAsync: // at this point, we should not see RunningAsync Status
            case ClientStatus.RunningNoFrameTimes:
               break;
            case ClientStatus.Unknown:
               HfmTrace.WriteToHfmConsole(TraceLevel.Error,
                                          String.Format("Unable to Determine Status for Client '{0}'", statusData.InstanceName), true);
               // Update Client Status - don't call Determine Status
               return statusData.ReturnedStatus;
            case ClientStatus.Offline:
            case ClientStatus.Stopped:
            case ClientStatus.EuePause:
            case ClientStatus.Hung:
            case ClientStatus.Paused:
            case ClientStatus.SendingWorkPacket:
            case ClientStatus.GettingWorkPacket:
               // Update Client Status - don't call Determine Status
               return statusData.ReturnedStatus;
         }

         // if we have a frame time, use it
         if (statusData.FrameTime > 0)
         {
            ClientStatus Status = DetermineStatus(statusData);
            if (Status.Equals(ClientStatus.Hung) && statusData.AllowRunningAsync) // Issue 124
            {
               return DetermineAsyncStatus(statusData);
            }
            
            return Status;
         }

         // no frame time based on the current PPD calculation selection ('LastFrame', 'LastThreeFrames', etc)
         // this section attempts to give DetermineStats values to detect Hung clients before they have a valid
         // frame time - Issue 10
         else
         {
            // if we have no time stamp
            if (statusData.TimeOfLastFrame == TimeSpan.Zero)
            {
               // use the unit start time
               statusData.TimeOfLastFrame = statusData.UnitStartTimeStamp;
            }

            statusData.FrameTime = GetBaseFrameTime(statusData.AverageFrameTime, statusData.TypeOfClient);
            if (DetermineStatus(statusData).Equals(ClientStatus.Hung))
            {
               // Issue 124
               if (statusData.AllowRunningAsync)
               {
                  if (DetermineAsyncStatus(statusData).Equals(ClientStatus.Hung))
                  {
                     return ClientStatus.Hung;
                  }
                  else
                  {
                     return statusData.ReturnedStatus;
                  }
               }
               
               return ClientStatus.Hung;
            }
            else
            {
               return statusData.ReturnedStatus;
            }
         }
      }
      
      private static int GetBaseFrameTime(TimeSpan averageFrameTime, ClientType TypeOfClient)
      {
         // no frame time based on the current PPD calculation selection ('LastFrame', 'LastThreeFrames', etc)
         // this section attempts to give DetermineStats values to detect Hung clients before they have a valid
         // frame time - Issue 10

         // get the average frame time for this client and project id
         if (averageFrameTime > TimeSpan.Zero)
         {
            return Convert.ToInt32(averageFrameTime.TotalSeconds);
         }

         // no benchmarked average frame time, use some arbitrary (and large) values for the frame time
         // we want to give the client plenty of time to show progress but don't want it to sit idle for days
         else
         {
            // CPU: use 1 hour (3600 seconds) as a base frame time
            int BaseFrameTime = 3600;
            if (TypeOfClient.Equals(ClientType.GPU))
            {
               // GPU: use 10 minutes (600 seconds) as a base frame time
               BaseFrameTime = 600;
            }

            return BaseFrameTime;
         }
      }

      /// <summary>
      /// Send EuePause Status Email
      /// </summary>
      private static void SendEuePauseEmail(string instanceName, IPreferenceSet prefs)
      {
         string messageBody = String.Format("HFM.NET detected that Client '{0}' has entered a 24 hour EUE Pause state.", instanceName);
         try
         {
            NetworkOps.SendEmail(prefs.GetPreference<bool>(Preference.EmailReportingServerSecure), 
                                 prefs.GetPreference<string>(Preference.EmailReportingFromAddress), 
                                 prefs.GetPreference<string>(Preference.EmailReportingToAddress),
                                 "HFM.NET - Client EUE Pause Error", messageBody, 
                                 prefs.GetPreference<string>(Preference.EmailReportingServerAddress),
                                 prefs.GetPreference<int>(Preference.EmailReportingServerPort),
                                 prefs.GetPreference<string>(Preference.EmailReportingServerUsername), 
                                 prefs.GetPreference<string>(Preference.EmailReportingServerPassword));
         }
         catch (Exception ex)
         {
            HfmTrace.WriteToHfmConsole(ex);
         }
      }

      /// <summary>
      /// Determine Client Status
      /// </summary>
      /// <param name="statusData">Client Status Data</param>
      private static ClientStatus DetermineStatus(StatusData statusData)
      {
         #region Get Terminal Time
         // Terminal Time - defined as last retrieval time minus twice (7 times for GPU) the current Raw Time per Section.
         // if a new frame has not completed in twice the amount of time it should take to complete we should deem this client Hung.
         DateTime terminalDateTime;

         if (statusData.TypeOfClient.Equals(ClientType.GPU))
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 7));
         }
         else
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 2));
         }
         #endregion

         #region Get Last Retrieval Time Date
         DateTime currentFrameDateTime;

         if (statusData.IgnoreUtcOffset)
         {
            // get only the date from the last retrieval time (in universal), we'll add the current time below
            currentFrameDateTime = new DateTime(statusData.LastRetrievalTime.Date.Ticks, DateTimeKind.Utc);
         }
         else
         {
            // get only the date from the last retrieval time, we'll add the current time below
            currentFrameDateTime = statusData.LastRetrievalTime.Date;
         }
         #endregion

         #region Apply Frame Time Offset and Set Current Frame Time Date
         TimeSpan offset = TimeSpan.FromMinutes(statusData.ClientTimeOffset);
         TimeSpan adjustedFrameTime = statusData.TimeOfLastFrame;
         if (statusData.IgnoreUtcOffset == false)
         {
            adjustedFrameTime = adjustedFrameTime.Add(statusData.UtcOffset);
         }
         adjustedFrameTime = adjustedFrameTime.Subtract(offset);

         // client time has already rolled over to the next day. the offset correction has 
         // caused the adjusted frame time span to be negetive.  take the that negetive span
         // and add it to a full 24 hours to correct.
         if (adjustedFrameTime < TimeSpan.Zero)
         {
            adjustedFrameTime = TimeSpan.FromDays(1).Add(adjustedFrameTime);
         }

         // the offset correction has caused the frame time span to be greater than 24 hours.
         // subtract the extra day from the adjusted frame time span.
         else if (adjustedFrameTime > TimeSpan.FromDays(1))
         {
            adjustedFrameTime = adjustedFrameTime.Subtract(TimeSpan.FromDays(1));
         }

         // add adjusted Time of Last Frame (TimeSpan) to the DateTime with the correct date
         currentFrameDateTime = currentFrameDateTime.Add(adjustedFrameTime);
         #endregion

         #region Check For Frame from Prior Day (Midnight Rollover on Local Machine)
         bool priorDayAdjust = false;

         // if the current (and adjusted) frame time hours is greater than the last retrieval time hours, 
         // and the time difference is greater than an hour, then frame is from the day prior.
         // this should only happen after midnight time on the machine running HFM when the monitored client has 
         // not completed a frame since the local machine time rolled over to the next day, otherwise the time
         // stamps between HFM and the client are too far off, a positive offset should be set to correct.
         if (currentFrameDateTime.TimeOfDay.Hours > statusData.LastRetrievalTime.TimeOfDay.Hours &&
             currentFrameDateTime.TimeOfDay.Subtract(statusData.LastRetrievalTime.TimeOfDay).Hours > 0)
         {
            priorDayAdjust = true;

            // subtract 1 day from today's date
            currentFrameDateTime = currentFrameDateTime.Subtract(TimeSpan.FromDays(1));
         }
         #endregion

         #region Write Verbose Trace
         if (TraceLevelSwitch.Switch.TraceVerbose)
         {
            List<string> messages = new List<string>(10);

            messages.Add(String.Format("{0} ({1})", HfmTrace.FunctionName, statusData.InstanceName));
            messages.Add(String.Format(" - Retrieval Time (Date) ------- : {0}", statusData.LastRetrievalTime));
            messages.Add(String.Format(" - Time Of Last Frame (TimeSpan) : {0}", statusData.TimeOfLastFrame));
            messages.Add(String.Format(" - Offset (Minutes) ------------ : {0}", statusData.ClientTimeOffset));
            messages.Add(String.Format(" - Time Of Last Frame (Adjusted) : {0}", adjustedFrameTime));
            messages.Add(String.Format(" - Prior Day Adjustment -------- : {0}", priorDayAdjust));
            messages.Add(String.Format(" - Time Of Last Frame (Date) --- : {0}", currentFrameDateTime));
            messages.Add(String.Format(" - Terminal Time (Date) -------- : {0}", terminalDateTime));

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, messages);
         }
         #endregion

         if (currentFrameDateTime > terminalDateTime)
         {
            return ClientStatus.Running;
         }
         else // current frame is less than terminal time
         {
            return ClientStatus.Hung;
         }
      }

      /// <summary>
      /// Determine Client Status
      /// </summary>
      /// <param name="statusData">Client Status Data</param>
      private static ClientStatus DetermineAsyncStatus(StatusData statusData)
      {
         #region Get Terminal Time
         // Terminal Time - defined as last retrieval time minus twice (7 times for GPU) the current Raw Time per Section.
         // if a new frame has not completed in twice the amount of time it should take to complete we should deem this client Hung.
         DateTime terminalDateTime;

         if (statusData.TypeOfClient.Equals(ClientType.GPU))
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 7));
         }
         else
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 2));
         }
         #endregion

         #region Determine Unit Progress Value to Use
         Debug.Assert(statusData.TimeOfLastUnitStart.Equals(DateTime.MinValue) == false);

         DateTime LastProgress = statusData.TimeOfLastUnitStart;
         if (statusData.TimeOfLastFrameProgress > statusData.TimeOfLastUnitStart)
         {
            LastProgress = statusData.TimeOfLastFrameProgress;
         } 
         #endregion
         
         #region Write Verbose Trace
         if (TraceLevelSwitch.Switch.TraceVerbose)
         {
            List<string> messages = new List<string>(4);

            messages.Add(String.Format("{0} ({1})", HfmTrace.FunctionName, statusData.InstanceName));
            messages.Add(String.Format(" - Retrieval Time (Date) ------- : {0}", statusData.LastRetrievalTime));
            messages.Add(String.Format(" - Time Of Last Unit Start ----- : {0}", statusData.TimeOfLastUnitStart));
            messages.Add(String.Format(" - Time Of Last Frame Progress - : {0}", statusData.TimeOfLastFrameProgress));
            messages.Add(String.Format(" - Terminal Time (Date) -------- : {0}", terminalDateTime));

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, messages);
         }
         #endregion

         if (LastProgress > terminalDateTime)
         {
            return ClientStatus.RunningAsync;
         }
         else // time of last progress is less than terminal time
         {
            return ClientStatus.Hung;
         }
      }
      
      #endregion

      #region Other Helper Functions
      /// <summary>
      /// Restore the given UnitInfo into this Client Instance
      /// </summary>
      /// <param name="unitInfo">UnitInfo Object to Restore</param>
      public void RestoreUnitInfo(IUnitInfo unitInfo)
      {
         CurrentUnitInfoConcrete = new UnitInfoLogic(_prefs, _proteinCollection, _benchmarkContainer, unitInfo, this);
      }
      
      public bool IsUsernameOk()
      {
         // if these are the default assigned values, don't check otherwise and just return true
         if (FoldingID == Constants.FoldingIDDefault && Team == Constants.TeamDefault)
         {
            return true;
         }

         if ((FoldingID != _prefs.GetPreference<string>(Preference.StanfordId) || 
                   Team != _prefs.GetPreference<int>(Preference.TeamId)) &&
             (Status.Equals(ClientStatus.Unknown) == false && Status.Equals(ClientStatus.Offline) == false))
         {
            return false;
         }

         return true;
      }
      
      public bool Owns(IOwnedByClientInstance value)
      {
         if (value.OwningInstanceName.Equals(InstanceName) &&
             StringOps.PathsEqual(value.OwningInstancePath, Path))
         {
            return true;
         }
         
         return false;
      }
      #endregion
   }
}
