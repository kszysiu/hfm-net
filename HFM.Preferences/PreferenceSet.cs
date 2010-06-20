/*
 * HFM.NET - User Preferences Class
 * Copyright (C) 2006-2007 David Rawling
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
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using harlam357.Security;
using harlam357.Security.Encryption;

using HFM.Framework;
using HFM.Preferences.Properties;
using HFM.Instrumentation;

namespace HFM.Preferences
{
   public sealed class PreferenceSet : IPreferenceSet
   {
      #region Members
      private readonly Data _iv = new Data("3k1vKL=Cz6!wZS`I");
      private readonly Data _symmetricKey = new Data("%`Bb9ega;$.GUDaf");

      private readonly Dictionary<Preference, IMetadata> _Preferences = new Dictionary<Preference, IMetadata>();
      #endregion

      #region Properties
      public string ApplicationPath
      {
         get
         {
            return Path.GetDirectoryName(Application.ExecutablePath);
         }
      }

      /// <summary>
      /// Log File Cache Directory
      /// </summary>
      public string CacheDirectory
      {
         get { return Path.Combine(GetPreference<string>(Preference.ApplicationDataFolderPath), 
                                   GetPreference<string>(Preference.CacheFolder)); }
      }
      
      /// <summary>
      /// Url to EOC User Xml File
      /// </summary>
      public string EocUserXml
      {
         get 
         { 
            return String.Concat(Constants.EOCUserXmlUrl, GetPreference<int>(Preference.EocUserId));
         }
      }

      /// <summary>
      /// Url to EOC User Page
      /// </summary>
      public Uri EocUserUrl
      {
         get
         {
            return new Uri(String.Concat(Constants.EOCUserBaseUrl, GetPreference<int>(Preference.EocUserId)));
         }
      }

      /// <summary>
      /// Url to EOC Team Page
      /// </summary>
      public Uri EocTeamUrl
      {
         get
         {
            return new Uri(String.Concat(Constants.EOCTeamBaseUrl, GetPreference<int>(Preference.TeamId)));
         }
      }

      /// <summary>
      /// Url to Stanford User Page
      /// </summary>
      public Uri StanfordUserUrl
      {
         get
         {
            return new Uri(String.Concat(Constants.StanfordBaseUrl, GetPreference<string>(Preference.StanfordId)));
         }
      }

      #endregion

      #region Implementation
      public bool Initialize()
      {
         SetupDictionary();

         try
         {
            // Issue 176
            Load();
         }
         catch (Exception ex)
         {
            string filename = HandleConfigurationErrorsException(ex);
            if (String.IsNullOrEmpty(filename)) throw;
            
            File.Delete(filename);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HFM.NET has detected that your user settings file has become");
            sb.AppendLine("corrupted.  This may be due to a crash or improper exiting of");
            sb.AppendLine("the program.  HFM.NET must reset your user settings in order");
            sb.AppendLine("to continue.  The program will now exit.  Please restart.");
            MessageBox.Show(sb.ToString(), PlatformOps.ApplicationNameAndVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            
            return false;
         }

         return true;
      }
      
      private static string HandleConfigurationErrorsException(Exception ex)
      {
         ConfigurationErrorsException configurationErrorsException = ex as ConfigurationErrorsException;
         if (configurationErrorsException == null) return null;

         string filename = configurationErrorsException.Filename;
         if (String.IsNullOrEmpty(filename))
         {
            filename = HandleConfigurationErrorsException(ex.InnerException);
         }

         return filename;
      }
      
      /// <summary>
      /// Get a Preference of Type T
      /// </summary>
      /// <typeparam name="T">Preference Data Type</typeparam>
      /// <param name="key">Preference Key</param>
      public T GetPreference<T>(Preference key)
      {
         if (_Preferences[key].DataType == typeof(T))
         {
            return (T) _Preferences[key].Data;
         }
         
         throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, 
            "Preference '{0}' of Type '{1}' does not exist.", key, typeof(T)));
      }

      /// <summary>
      /// Set a Preference of Type T
      /// </summary>
      /// <typeparam name="T">Preference Data Type</typeparam>
      /// <param name="key">Preference Key</param>
      /// <param name="value">Preference Value</param>
      public void SetPreference<T>(Preference key, T value)
      {
         if (_Preferences[key].DataType == typeof(T))
         {
            ((Metadata<T>)_Preferences[key]).Data = value;
            return;
         }
         if (_Preferences[key].DataType == typeof(int))
         {
            string stringValue = value as string;
            // Issue 189 - Use Default Value if String is Null or Empty
            ((Metadata<int>) _Preferences[key]).Data = String.IsNullOrEmpty(stringValue) ? default(int) : Int32.Parse(stringValue);
            return;
         }

         throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
            "Preference '{0}' of Type '{1}' does not exist.", key, typeof(T)));
      }

      private void SetupDictionary()
      {
         DateTime start = HfmTrace.ExecStart;
      
         _Preferences.Add(Preference.FormLocation, new Metadata<Point>());
         _Preferences.Add(Preference.FormSize, new Metadata<Size>());
         _Preferences.Add(Preference.FormColumns, new Metadata<StringCollection>());
         _Preferences.Add(Preference.FormSortColumn, new Metadata<string>());
         _Preferences.Add(Preference.FormSortOrder, new Metadata<SortOrder>());
         _Preferences.Add(Preference.FormSplitLocation, new Metadata<int>());
         _Preferences.Add(Preference.FormLogWindowHeight, new Metadata<int>());
         _Preferences.Add(Preference.FormLogVisible, new Metadata<bool>());
         _Preferences.Add(Preference.QueueViewerVisible, new Metadata<bool>());
         _Preferences.Add(Preference.TimeStyle, new Metadata<TimeStyleType>());
         _Preferences.Add(Preference.CompletedCountDisplay, new Metadata<CompletedCountDisplayType>());
         _Preferences.Add(Preference.FormShowStyle, new Metadata<FormShowStyleType>());

         _Preferences.Add(Preference.BenchmarksFormLocation, new Metadata<Point>());
         _Preferences.Add(Preference.BenchmarksFormSize, new Metadata<Size>());
         _Preferences.Add(Preference.GraphColors, new Metadata<List<Color>>());

         _Preferences.Add(Preference.MessagesFormLocation, new Metadata<Point>());
         _Preferences.Add(Preference.MessagesFormSize, new Metadata<Size>());

         _Preferences.Add(Preference.SyncOnLoad, new Metadata<bool>());
         _Preferences.Add(Preference.SyncOnSchedule, new Metadata<bool>());
         _Preferences.Add(Preference.SyncTimeMinutes, new Metadata<int>());
         _Preferences.Add(Preference.DuplicateUserIdCheck, new Metadata<bool>());
         _Preferences.Add(Preference.DuplicateProjectCheck, new Metadata<bool>());
         _Preferences.Add(Preference.AllowRunningAsync, new Metadata<bool>());
         _Preferences.Add(Preference.ShowUserStats, new Metadata<bool>());

         _Preferences.Add(Preference.GenerateWeb, new Metadata<bool>());
         _Preferences.Add(Preference.GenerateInterval, new Metadata<int>());
         _Preferences.Add(Preference.WebGenAfterRefresh, new Metadata<bool>());
         _Preferences.Add(Preference.WebRoot, new Metadata<string>());
         _Preferences.Add(Preference.WebGenCopyFAHlog, new Metadata<bool>());
         _Preferences.Add(Preference.WebGenFtpMode, new Metadata<FtpType>());
         _Preferences.Add(Preference.WebGenCopyHtml, new Metadata<bool>());
         _Preferences.Add(Preference.WebGenCopyXml, new Metadata<bool>());
         _Preferences.Add(Preference.WebGenLimitLogSize, new Metadata<bool>());
         _Preferences.Add(Preference.WebGenLimitLogSizeLength, new Metadata<int>());
         _Preferences.Add(Preference.CssFile, new Metadata<string>());
         _Preferences.Add(Preference.WebOverview, new Metadata<string>());
         _Preferences.Add(Preference.WebMobileOverview, new Metadata<string>());
         _Preferences.Add(Preference.WebSummary, new Metadata<string>());
         _Preferences.Add(Preference.WebMobileSummary, new Metadata<string>());
         _Preferences.Add(Preference.WebInstance, new Metadata<string>());

         _Preferences.Add(Preference.RunMinimized, new Metadata<bool>());
         _Preferences.Add(Preference.StartupCheckForUpdate, new Metadata<bool>());
         _Preferences.Add(Preference.UseDefaultConfigFile, new Metadata<bool>());
         _Preferences.Add(Preference.DefaultConfigFile, new Metadata<string>());

         _Preferences.Add(Preference.OfflineLast, new Metadata<bool>());
         _Preferences.Add(Preference.ColorLogFile, new Metadata<bool>());
         _Preferences.Add(Preference.AutoSaveConfig, new Metadata<bool>());
         _Preferences.Add(Preference.MaintainSelectedClient, new Metadata<bool>());
         _Preferences.Add(Preference.PpdCalculation, new Metadata<PpdCalculationType>());
         _Preferences.Add(Preference.DecimalPlaces, new Metadata<int>());
         _Preferences.Add(Preference.CalculateBonus, new Metadata<bool>());
         _Preferences.Add(Preference.LogFileViewer, new Metadata<string>());
         _Preferences.Add(Preference.FileExplorer, new Metadata<string>());
         _Preferences.Add(Preference.MessageLevel, new Metadata<int>());

         _Preferences.Add(Preference.EmailReportingEnabled, new Metadata<bool>());
         _Preferences.Add(Preference.EmailReportingServerSecure, new Metadata<bool>());
         _Preferences.Add(Preference.EmailReportingToAddress, new Metadata<string>());
         _Preferences.Add(Preference.EmailReportingFromAddress, new Metadata<string>());
         _Preferences.Add(Preference.EmailReportingServerAddress, new Metadata<string>());
         _Preferences.Add(Preference.EmailReportingServerPort, new Metadata<int>());
         _Preferences.Add(Preference.EmailReportingServerUsername, new Metadata<string>());
         _Preferences.Add(Preference.EmailReportingServerPassword, new Metadata<string>());
         _Preferences.Add(Preference.ReportEuePause, new Metadata<bool>());

         _Preferences.Add(Preference.EocUserId, new Metadata<int>());
         _Preferences.Add(Preference.StanfordId, new Metadata<string>());
         _Preferences.Add(Preference.TeamId, new Metadata<int>());
         _Preferences.Add(Preference.ProjectDownloadUrl, new Metadata<string>());
         _Preferences.Add(Preference.UseProxy, new Metadata<bool>());
         _Preferences.Add(Preference.ProxyServer, new Metadata<string>());
         _Preferences.Add(Preference.ProxyPort, new Metadata<int>());
         _Preferences.Add(Preference.UseProxyAuth, new Metadata<bool>());
         _Preferences.Add(Preference.ProxyUser, new Metadata<string>());
         _Preferences.Add(Preference.ProxyPass, new Metadata<string>());

         _Preferences.Add(Preference.CacheFolder, new Metadata<string>());
         _Preferences.Add(Preference.ApplicationDataFolderPath, new Metadata<string>());

         Debug.WriteLine(String.Format("{0} Execution Time: {1}", HfmTrace.FunctionName, HfmTrace.GetExecTime(start)));
      }

      /// <summary>
      /// Load the Preferences Set
      /// </summary>
      public void Load()
      {
         DateTime start = HfmTrace.ExecStart;
         Symmetric symmetricProvider = new Symmetric(Symmetric.Provider.Rijndael, false);

         UpgradeUserSettings();

         Point location = new Point();
         Size size = new Size();
         StringCollection columns = null;
         GetFormStateValues(ref location, ref size, ref columns);
         SetPreference(Preference.FormLocation, location);
         SetPreference(Preference.FormSize, size);
         SetPreference(Preference.FormColumns, columns);
         SetPreference(Preference.FormSortColumn, Settings.Default.FormSortColumn);
         SetPreference(Preference.FormSortOrder, GetFormSortOrder());
         SetPreference(Preference.FormSplitLocation, Settings.Default.FormSplitLocation);
         SetPreference(Preference.FormLogWindowHeight, Settings.Default.FormLogWindowHeight);
         SetPreference(Preference.FormLogVisible, Settings.Default.FormLogVisible);
         SetPreference(Preference.QueueViewerVisible, Settings.Default.QueueViewerVisible);
         SetPreference(Preference.TimeStyle, GetTimeStyle());
         SetPreference(Preference.CompletedCountDisplay, GetCompletedCountDisplay());
         SetPreference(Preference.FormShowStyle, GetFormShowStyle());

         location = new Point();
         size = new Size();
         GetBenchmarksFormStateValues(ref location, ref size);
         SetPreference(Preference.BenchmarksFormLocation, location);
         SetPreference(Preference.BenchmarksFormSize, size);
         SetPreference(Preference.GraphColors, GetGraphColorsList());

         location = new Point();
         size = new Size();
         GetMessagesFormStateValues(ref location, ref size);
         SetPreference(Preference.MessagesFormLocation, location);
         SetPreference(Preference.MessagesFormSize, size);

         SetPreference(Preference.SyncOnLoad, Settings.Default.SyncOnLoad);
         SetPreference(Preference.SyncOnSchedule, Settings.Default.SyncOnSchedule);
         SetPreference(Preference.SyncTimeMinutes, GetValidNumeric(Settings.Default.SyncTimeMinutes, Constants.MinutesDefault));
         SetPreference(Preference.DuplicateUserIdCheck, Settings.Default.DuplicateUserIDCheck);
         SetPreference(Preference.DuplicateProjectCheck, Settings.Default.DuplicateProjectCheck);
         SetPreference(Preference.AllowRunningAsync, Settings.Default.AllowRunningAsync);
         SetPreference(Preference.ShowUserStats, Settings.Default.ShowUserStats);
        
         SetPreference(Preference.GenerateWeb, Settings.Default.GenerateWeb);
         SetPreference(Preference.GenerateInterval, GetValidNumeric(Settings.Default.GenerateInterval, Constants.MinutesDefault));
         SetPreference(Preference.WebGenAfterRefresh, Settings.Default.WebGenAfterRefresh);
         SetPreference(Preference.WebRoot, DecryptWebRoot(Settings.Default.WebRoot, symmetricProvider, _iv, _symmetricKey));
         SetPreference(Preference.WebGenCopyFAHlog, Settings.Default.WebGenCopyFAHlog);
         SetPreference(Preference.WebGenFtpMode, GetFtpType());
         SetPreference(Preference.WebGenCopyHtml, Settings.Default.WebGenCopyHtml);
         SetPreference(Preference.WebGenCopyXml, Settings.Default.WebGenCopyXml);
         SetPreference(Preference.WebGenLimitLogSize, Settings.Default.WebGenLimitLogSize);
         SetPreference(Preference.WebGenLimitLogSizeLength, Settings.Default.WebGenLimitLogSizeLength);
         SetPreference(Preference.CssFile, Settings.Default.CSSFile);
         SetPreference(Preference.WebOverview, Settings.Default.WebOverview);
         SetPreference(Preference.WebMobileOverview, Settings.Default.WebMobileOverview);
         SetPreference(Preference.WebSummary, Settings.Default.WebSummary);
         SetPreference(Preference.WebMobileSummary, Settings.Default.WebMobileSummary);
         SetPreference(Preference.WebInstance, Settings.Default.WebInstance);

         SetPreference(Preference.RunMinimized, Settings.Default.RunMinimized);
         SetPreference(Preference.StartupCheckForUpdate, Settings.Default.StartupCheckForUpdate);
         SetPreference(Preference.UseDefaultConfigFile, Settings.Default.UseDefaultConfigFile);
         SetPreference(Preference.DefaultConfigFile, Settings.Default.DefaultConfigFile);

         SetPreference(Preference.OfflineLast, Settings.Default.OfflineLast);
         SetPreference(Preference.ColorLogFile, Settings.Default.ColorLogFile);
         SetPreference(Preference.AutoSaveConfig, Settings.Default.AutoSaveConfig);
         SetPreference(Preference.MaintainSelectedClient, Settings.Default.MaintainSelectedClient);
         SetPreference(Preference.PpdCalculation, GetPpdCalculation());
         SetPreference(Preference.DecimalPlaces, Settings.Default.DecimalPlaces);
         SetPreference(Preference.CalculateBonus, Settings.Default.CalculateBonus);
         SetPreference(Preference.LogFileViewer, Settings.Default.LogFileViewer);
         SetPreference(Preference.FileExplorer, Settings.Default.FileExplorer);
         SetPreference(Preference.MessageLevel, Settings.Default.MessageLevel);

         SetPreference(Preference.EmailReportingEnabled, Settings.Default.EmailReportingEnabled);
         SetPreference(Preference.EmailReportingServerSecure, Settings.Default.EmailReportingServerSecure);
         SetPreference(Preference.EmailReportingToAddress, Settings.Default.EmailReportingToAddress);
         SetPreference(Preference.EmailReportingFromAddress, Settings.Default.EmailReportingFromAddress);
         SetPreference(Preference.EmailReportingServerAddress, Settings.Default.EmailReportingServerAddress);
         SetPreference(Preference.EmailReportingServerPort, Settings.Default.EmailReportingServerPort);
         SetPreference(Preference.EmailReportingServerUsername, Settings.Default.EmailReportingServerUsername);
         SetPreference(Preference.EmailReportingServerPassword, DecryptEmailReportingServerPassword(Settings.Default.EmailReportingServerPassword, symmetricProvider, _iv, _symmetricKey));
         SetPreference(Preference.ReportEuePause, Settings.Default.ReportEuePause);
         
         SetPreference(Preference.EocUserId, Settings.Default.EOCUserID);
         SetPreference(Preference.StanfordId, Settings.Default.StanfordID);
         SetPreference(Preference.TeamId, Settings.Default.TeamID);
         SetPreference(Preference.ProjectDownloadUrl, Settings.Default.ProjectDownloadUrl);
         SetPreference(Preference.UseProxy, Settings.Default.UseProxy);
         SetPreference(Preference.ProxyServer, Settings.Default.ProxyServer);
         SetPreference(Preference.ProxyPort, Settings.Default.ProxyPort);
         SetPreference(Preference.UseProxyAuth, Settings.Default.UseProxyAuth);
         SetPreference(Preference.ProxyUser, Settings.Default.ProxyUser);
         SetPreference(Preference.ProxyPass, DecryptProxyPass(Settings.Default.ProxyPass, symmetricProvider, _iv, _symmetricKey));

         SetPreference(Preference.CacheFolder, Settings.Default.CacheFolder);
         SetPreference(Preference.ApplicationDataFolderPath, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.ExeName));

         Debug.WriteLine(String.Format("{0} Execution Time: {1}", HfmTrace.FunctionName, HfmTrace.GetExecTime(start)));
      }

      #region Load Support Methods
      private static void UpgradeUserSettings()
      {
         string appVersionString = PlatformOps.ApplicationVersionWithRevision;

         if (Settings.Default.ApplicationVersion != appVersionString)
         {
            Settings.Default.Upgrade();
            Settings.Default.ApplicationVersion = appVersionString;
            Settings.Default.Save();
         }
      }

      private static void GetFormStateValues(ref Point location, ref Size size, ref StringCollection columns)
      {
         try
         {
            location = Settings.Default.FormLocation;
            size = Settings.Default.FormSize;
            columns = Settings.Default.FormColumns;
         }
         catch (NullReferenceException)
         { }
      }

      private static SortOrder GetFormSortOrder()
      {
         SortOrder order = SortOrder.None;
         try
         {
            order = Settings.Default.FormSortOrder;
         }
         catch (NullReferenceException)
         { }

         return order;
      }

      private static TimeStyleType GetTimeStyle()
      {
         switch (Settings.Default.TimeStyle)
         {
            case "Standard":
               return TimeStyleType.Standard;
            case "Formatted":
               return TimeStyleType.Formatted;
            default:
               return TimeStyleType.Standard;
         }
      }

      private static CompletedCountDisplayType GetCompletedCountDisplay()
      {
         switch (Settings.Default.CompletedCountDisplay)
         {
            case "ClientTotal":
               return CompletedCountDisplayType.ClientTotal;
            case "ClientRunTotal":
               return CompletedCountDisplayType.ClientRunTotal;
            default:
               return CompletedCountDisplayType.ClientRunTotal;
         }
      }

      private static FormShowStyleType GetFormShowStyle()
      {
         switch (Settings.Default.FormShowStyle)
         {
            case "SystemTray":
               return FormShowStyleType.SystemTray;
            case "TaskBar":
               return FormShowStyleType.TaskBar;
            case "Both":
               return FormShowStyleType.Both;
            default:
               return FormShowStyleType.SystemTray;
         }
      }

      private static void GetBenchmarksFormStateValues(ref Point location, ref Size size)
      {
         try
         {
            location = Settings.Default.BenchmarksFormLocation;
            size = Settings.Default.BenchmarksFormSize;
         }
         catch (NullReferenceException)
         { }
      }

      private static List<Color> GetGraphColorsList()
      {
         List<Color> graphColors = new List<Color>();
         foreach (string color in Settings.Default.GraphColors)
         {
            Color realColor = Color.FromName(color);
            if (realColor.IsEmpty == false)
            {
               graphColors.Add(realColor);
            }
         }

         return graphColors;
      }

      private static void GetMessagesFormStateValues(ref Point location, ref Size size)
      {
         try
         {
            location = Settings.Default.MessagesFormLocation;
            size = Settings.Default.MessagesFormSize;
         }
         catch (NullReferenceException)
         { }
      }

      private static string DecryptWebRoot(string encrypted, Symmetric symmetricProvider, Data iv, Data symmetricKey)
      {
         string webRoot = String.Empty;
         if (Settings.Default.WebRoot.Length > 0)
         {
            try
            {
               symmetricProvider.IntializationVector = iv;
               webRoot = symmetricProvider.Decrypt(new Data(Utils.FromBase64(encrypted)), symmetricKey).ToString();
            }
            catch (FormatException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Web Generation Root Folder is not Base64 encoded... loading clear value.", true);
               webRoot = Settings.Default.WebRoot;
            }
            catch (CryptographicException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Cannot decrypt Web Generation Root Folder... loading clear value.", true);
               webRoot = Settings.Default.WebRoot;
            }
         }

         return webRoot;
      }
      
      private static FtpType GetFtpType()
      {
         switch (Settings.Default.WebGenFtpMode)
         {
            case "Passive":
               return FtpType.Passive;
            case "Active":
               return FtpType.Active;
            default:
               return FtpType.Passive;
         }
      }

      private static PpdCalculationType GetPpdCalculation()
      {
         switch (Settings.Default.PpdCalculation)
         {
            case "LastFrame":
               return PpdCalculationType.LastFrame;
            case "LastThreeFrames":
               return PpdCalculationType.LastThreeFrames;
            case "AllFrames":
               return PpdCalculationType.AllFrames;
            case "EffectiveRate":
               return PpdCalculationType.EffectiveRate;
            default:
               return PpdCalculationType.LastThreeFrames;
         }
      }

      private static string DecryptEmailReportingServerPassword(string encrypted, Symmetric symmetricProvider, Data iv, Data symmetricKey)
      {
         string emailReportingServerPassword = String.Empty;
         if (Settings.Default.EmailReportingServerPassword.Length > 0)
         {
            try
            {
               symmetricProvider.IntializationVector = iv;
               emailReportingServerPassword =
                  symmetricProvider.Decrypt(new Data(Utils.FromBase64(encrypted)), symmetricKey).ToString();
            }
            catch (FormatException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Stmp Server Password is not Base64 encoded... loading clear value.", true);
               emailReportingServerPassword = Settings.Default.EmailReportingServerPassword;
            }
            catch (CryptographicException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Cannot decrypt Stmp Server Password... loading clear value.", true);
               emailReportingServerPassword = Settings.Default.EmailReportingServerPassword;
            }
         }
         
         return emailReportingServerPassword;
      }

      private static string DecryptProxyPass(string encrypted, Symmetric symmetricProvider, Data iv, Data symmetricKey)
      {
         string proxyPass = String.Empty;
         if (Settings.Default.ProxyPass.Length > 0)
         {
            try
            {
               symmetricProvider.IntializationVector = iv;
               proxyPass = symmetricProvider.Decrypt(new Data(Utils.FromBase64(encrypted)), symmetricKey).ToString();
            }
            catch (FormatException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Proxy Password is not Base64 encoded... loading clear value.", true);
               proxyPass = Settings.Default.ProxyPass;
            }
            catch (CryptographicException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Cannot decrypt Proxy Password... loading clear value.", true);
               proxyPass = Settings.Default.ProxyPass;
            }
         }

         return proxyPass;
      }
      #endregion

      /// <summary>
      /// Revert to the previously saved settings
      /// </summary>
      public void Discard()
      {
         Load();
      }

      /// <summary>
      /// Save the Preferences Set
      /// </summary>
      public void Save()
      {
         DateTime start = HfmTrace.ExecStart;

         Symmetric symmetricProvider = new Symmetric(Symmetric.Provider.Rijndael, false);

         bool raiseFormShowStyleChanged = false;
         bool raiseTimerSettingsChanged = false;
         bool raiseShowUserStatsChanged = false;
         bool raiseOfflineLastChanged = false;
         bool raiseColorLogFileChanged = false;
         bool raisePpdCalculationChanged = false;
         bool raiseDecimalPlacesChanged = false;
         bool raiseCalculateBonusChanged = false;
         bool raiseMessageLevelChanged = false;
         
         try
         {
            Settings.Default.FormLocation = GetPreference<Point>(Preference.FormLocation);
            Settings.Default.FormSize = GetPreference<Size>(Preference.FormSize);
            Settings.Default.FormColumns = GetPreference<StringCollection>(Preference.FormColumns);
            Settings.Default.FormSortColumn = GetPreference<string>(Preference.FormSortColumn);
            Settings.Default.FormSortOrder = GetPreference<SortOrder>(Preference.FormSortOrder);
            Settings.Default.FormSplitLocation = GetPreference<int>(Preference.FormSplitLocation);
            Settings.Default.FormLogWindowHeight = GetPreference<int>(Preference.FormLogWindowHeight);
            Settings.Default.FormLogVisible = GetPreference<bool>(Preference.FormLogVisible);
            Settings.Default.QueueViewerVisible = GetPreference<bool>(Preference.QueueViewerVisible);
            Settings.Default.TimeStyle = GetPreference<TimeStyleType>(Preference.TimeStyle).ToString();
            Settings.Default.CompletedCountDisplay = GetPreference<CompletedCountDisplayType>(Preference.CompletedCountDisplay).ToString();
            if (Settings.Default.FormShowStyle != GetPreference<FormShowStyleType>(Preference.FormShowStyle).ToString())
            {
               raiseFormShowStyleChanged = true;
            }
            Settings.Default.FormShowStyle = GetPreference<FormShowStyleType>(Preference.FormShowStyle).ToString();

            Settings.Default.BenchmarksFormLocation = GetPreference<Point>(Preference.BenchmarksFormLocation);
            Settings.Default.BenchmarksFormSize = GetPreference<Size>(Preference.BenchmarksFormSize);
            Settings.Default.GraphColors = GetGraphColorsStringCollection(GetPreference<List<Color>>(Preference.GraphColors));

            Settings.Default.MessagesFormLocation = GetPreference<Point>(Preference.MessagesFormLocation);
            Settings.Default.MessagesFormSize = GetPreference<Size>(Preference.MessagesFormSize);

            Settings.Default.SyncOnLoad = GetPreference<bool>(Preference.SyncOnLoad);
            if (Settings.Default.SyncOnSchedule != GetPreference<bool>(Preference.SyncOnSchedule) ||
                Settings.Default.SyncTimeMinutes != GetPreference<int>(Preference.SyncTimeMinutes).ToString())
            {
               raiseTimerSettingsChanged = true;
            }
            Settings.Default.SyncOnSchedule = GetPreference<bool>(Preference.SyncOnSchedule);
            Settings.Default.SyncTimeMinutes = GetPreference<int>(Preference.SyncTimeMinutes).ToString();
            Settings.Default.DuplicateUserIDCheck = GetPreference<bool>(Preference.DuplicateUserIdCheck);
            Settings.Default.DuplicateProjectCheck = GetPreference<bool>(Preference.DuplicateProjectCheck);
            Settings.Default.AllowRunningAsync = GetPreference<bool>(Preference.AllowRunningAsync);
            if (Settings.Default.ShowUserStats != GetPreference<bool>(Preference.ShowUserStats))
            {
               raiseShowUserStatsChanged = true;
            }
            Settings.Default.ShowUserStats = GetPreference<bool>(Preference.ShowUserStats);

            if (Settings.Default.GenerateWeb != GetPreference<bool>(Preference.GenerateWeb) ||
                Settings.Default.GenerateInterval != GetPreference<int>(Preference.GenerateInterval).ToString() ||
                Settings.Default.WebGenAfterRefresh != GetPreference<bool>(Preference.WebGenAfterRefresh))
            {
               raiseTimerSettingsChanged = true;
            }
            Settings.Default.GenerateWeb = GetPreference<bool>(Preference.GenerateWeb);
            Settings.Default.GenerateInterval = GetPreference<int>(Preference.GenerateInterval).ToString();
            Settings.Default.WebGenAfterRefresh = GetPreference<bool>(Preference.WebGenAfterRefresh);
            Settings.Default.WebRoot = EncryptWebRoot(GetPreference<string>(Preference.WebRoot), symmetricProvider, _iv, _symmetricKey);
            Settings.Default.WebGenCopyFAHlog = GetPreference<bool>(Preference.WebGenCopyFAHlog);
            Settings.Default.WebGenFtpMode = GetPreference<FtpType>(Preference.WebGenFtpMode).ToString();
            Settings.Default.WebGenCopyHtml = GetPreference<bool>(Preference.WebGenCopyHtml);
            Settings.Default.WebGenCopyXml = GetPreference<bool>(Preference.WebGenCopyXml);
            Settings.Default.WebGenLimitLogSize = GetPreference<bool>(Preference.WebGenLimitLogSize);
            Settings.Default.WebGenLimitLogSizeLength = GetPreference<int>(Preference.WebGenLimitLogSizeLength);
            Settings.Default.CSSFile = GetPreference<string>(Preference.CssFile);
            Settings.Default.WebOverview = GetPreference<string>(Preference.WebOverview);
            Settings.Default.WebMobileOverview = GetPreference<string>(Preference.WebMobileOverview);
            Settings.Default.WebSummary = GetPreference<string>(Preference.WebSummary);
            Settings.Default.WebMobileSummary = GetPreference<string>(Preference.WebMobileSummary);
            Settings.Default.WebInstance = GetPreference<string>(Preference.WebInstance);

            Settings.Default.RunMinimized = GetPreference<bool>(Preference.RunMinimized);
            Settings.Default.StartupCheckForUpdate = GetPreference<bool>(Preference.StartupCheckForUpdate);
            Settings.Default.UseDefaultConfigFile = GetPreference<bool>(Preference.UseDefaultConfigFile);
            Settings.Default.DefaultConfigFile = GetPreference<string>(Preference.DefaultConfigFile);
            // if config file name is nothing, automatically set default config to false
            if (Settings.Default.DefaultConfigFile.Length == 0)
            {
               SetPreference(Preference.UseDefaultConfigFile, false);
               Settings.Default.UseDefaultConfigFile = false;
            }

            if (Settings.Default.OfflineLast != GetPreference<bool>(Preference.OfflineLast))
            {
               raiseOfflineLastChanged = true;
            }
            Settings.Default.OfflineLast = GetPreference<bool>(Preference.OfflineLast);
            if (Settings.Default.ColorLogFile != GetPreference<bool>(Preference.ColorLogFile))
            {
               raiseColorLogFileChanged = true;
            }
            Settings.Default.ColorLogFile = GetPreference<bool>(Preference.ColorLogFile);
            Settings.Default.AutoSaveConfig = GetPreference<bool>(Preference.AutoSaveConfig);
            Settings.Default.MaintainSelectedClient = GetPreference<bool>(Preference.MaintainSelectedClient);
            if (Settings.Default.PpdCalculation != GetPreference<PpdCalculationType>(Preference.PpdCalculation).ToString())
            {
               raisePpdCalculationChanged = true;
            }
            Settings.Default.PpdCalculation = GetPreference<PpdCalculationType>(Preference.PpdCalculation).ToString();
            if (Settings.Default.DecimalPlaces != GetPreference<int>(Preference.DecimalPlaces))
            {
               raiseDecimalPlacesChanged = true;
            }
            Settings.Default.DecimalPlaces = GetPreference<int>(Preference.DecimalPlaces);
            if (Settings.Default.CalculateBonus != GetPreference<bool>(Preference.CalculateBonus))
            {
               raiseCalculateBonusChanged = true;
            }
            Settings.Default.CalculateBonus = GetPreference<bool>(Preference.CalculateBonus);
            Settings.Default.LogFileViewer = GetPreference<string>(Preference.LogFileViewer);
            Settings.Default.FileExplorer = GetPreference<string>(Preference.FileExplorer);
            if (Settings.Default.MessageLevel != GetPreference<int>(Preference.MessageLevel))
            {
               raiseMessageLevelChanged = true;
            }
            Settings.Default.MessageLevel = GetPreference<int>(Preference.MessageLevel);

            Settings.Default.EmailReportingEnabled = GetPreference<bool>(Preference.EmailReportingEnabled);
            Settings.Default.EmailReportingServerSecure = GetPreference<bool>(Preference.EmailReportingServerSecure);
            Settings.Default.EmailReportingToAddress = GetPreference<string>(Preference.EmailReportingToAddress);
            Settings.Default.EmailReportingFromAddress = GetPreference<string>(Preference.EmailReportingFromAddress);
            Settings.Default.EmailReportingServerAddress = GetPreference<string>(Preference.EmailReportingServerAddress);
            Settings.Default.EmailReportingServerPort = GetPreference<int>(Preference.EmailReportingServerPort);
            Settings.Default.EmailReportingServerUsername = GetPreference<string>(Preference.EmailReportingServerUsername);
            Settings.Default.EmailReportingServerPassword = EncryptEmailReportingServerPassword(GetPreference<string>(Preference.EmailReportingServerPassword), symmetricProvider, _iv, _symmetricKey);
            Settings.Default.ReportEuePause = GetPreference<bool>(Preference.ReportEuePause);

            Settings.Default.EOCUserID = GetPreference<int>(Preference.EocUserId);
            Settings.Default.StanfordID = GetPreference<string>(Preference.StanfordId);
            Settings.Default.TeamID = GetPreference<int>(Preference.TeamId);
            Settings.Default.ProjectDownloadUrl = GetPreference<string>(Preference.ProjectDownloadUrl);
            Settings.Default.UseProxy = GetPreference<bool>(Preference.UseProxy);
            Settings.Default.ProxyServer = GetPreference<string>(Preference.ProxyServer);
            Settings.Default.ProxyPort = GetPreference<int>(Preference.ProxyPort);
            Settings.Default.UseProxyAuth = GetPreference<bool>(Preference.UseProxyAuth);
            Settings.Default.ProxyUser = GetPreference<string>(Preference.ProxyUser);
            Settings.Default.ProxyPass = EncryptProxyPass(GetPreference<string>(Preference.ProxyPass), symmetricProvider, _iv, _symmetricKey);
            
            if (raiseFormShowStyleChanged) OnFormShowStyleSettingsChanged(EventArgs.Empty);
            if (raiseTimerSettingsChanged) OnTimerSettingsChanged(EventArgs.Empty);
            if (raiseShowUserStatsChanged) OnShowUserStatsChanged(EventArgs.Empty);
            if (raiseOfflineLastChanged) OnOfflineLastChanged(EventArgs.Empty);
            if (raiseColorLogFileChanged) OnColorLogFileChanged(EventArgs.Empty);
            if (raisePpdCalculationChanged) OnPpdCalculationChanged(EventArgs.Empty);
            if (raiseDecimalPlacesChanged) OnDecimalPlacesChanged(EventArgs.Empty);
            if (raiseCalculateBonusChanged) OnCalculateBonusChanged(EventArgs.Empty);
            if (raiseMessageLevelChanged) OnMessageLevelChanged(EventArgs.Empty);

            Settings.Default.Save();
         }
         catch (Exception ex)
         {
            HfmTrace.WriteToHfmConsole(ex);
         }

         Debug.WriteLine(String.Format("{0} Execution Time: {1}", HfmTrace.FunctionName, HfmTrace.GetExecTime(start)));
      }

      #region Save Support Methods
      private static StringCollection GetGraphColorsStringCollection(IEnumerable<Color> collection)
      {
         StringCollection col = new StringCollection();
         foreach (Color color in collection)
         {
            col.Add(color.Name);
         }
         return col;
      }

      private static string EncryptWebRoot(string clear, Symmetric symmetricProvider, Data iv, Data symmetricKey)
      {
         string webRoot = String.Empty;
         if (clear.Length > 0)
         {
            try
            {
               symmetricProvider.IntializationVector = iv;
               webRoot = symmetricProvider.Encrypt(new Data(clear), symmetricKey).ToBase64();
            }
            catch (CryptographicException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Failed to encrypt Web Generation Root Folder... saving clear value.");
               webRoot = clear;
            }
         }
         
         return webRoot;
      }

      private static string EncryptEmailReportingServerPassword(string clear, Symmetric symmetricProvider, Data iv, Data symmetricKey)
      {
         string emailReportingServerPassword = String.Empty;
         if (clear.Length > 0)
         {
            try
            {
               symmetricProvider.IntializationVector = iv;
               emailReportingServerPassword = symmetricProvider.Encrypt(new Data(clear), symmetricKey).ToBase64();
            }
            catch (CryptographicException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Failed to encrypt Smtp Server Password... saving clear value.");
               emailReportingServerPassword = clear;
            }
         }
         
         return emailReportingServerPassword;
      }

      private static string EncryptProxyPass(string clear, Symmetric symmetricProvider, Data iv, Data symmetricKey)
      {
         string proxyPass = String.Empty;
         if (clear.Length > 0)
         {
            try
            {
               symmetricProvider.IntializationVector = iv;
               proxyPass = symmetricProvider.Encrypt(new Data(clear), symmetricKey).ToBase64();
            }
            catch (CryptographicException)
            {
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, "Failed to encrypt Proxy Password... saving clear value.");
               proxyPass = clear;
            }
         }

         return proxyPass;
      }
      #endregion
      
      #endregion

      #region Event Wrappers
      /// <summary>
      /// Form Show Style Settings Changed
      /// </summary>
      public event EventHandler FormShowStyleSettingsChanged;
      private void OnFormShowStyleSettingsChanged(EventArgs e)
      {
         if (FormShowStyleSettingsChanged != null)
         {
            FormShowStyleSettingsChanged(this, e);
         }
      }
      
      /// <summary>
      /// Background Timer (Refresh or Web) Settings Changed
      /// </summary>
      public event EventHandler TimerSettingsChanged;
      private void OnTimerSettingsChanged(EventArgs e)
      {
         if (TimerSettingsChanged != null)
         {
            TimerSettingsChanged(this, e);
         }
      }

      /// <summary>
      /// Offline Last Setting Changed
      /// </summary>
      public event EventHandler OfflineLastChanged;
      private void OnOfflineLastChanged(EventArgs e)
      {
         if (OfflineLastChanged != null)
         {
            OfflineLastChanged(this, e);
         }
      }

      /// <summary>
      /// PPD Calculation Type Changed
      /// </summary>
      public event EventHandler PpdCalculationChanged;
      private void OnPpdCalculationChanged(EventArgs e)
      {
         if (PpdCalculationChanged != null)
         {
            PpdCalculationChanged(this, e);
         }
      }

      /// <summary>
      /// Debug Message Level Changed
      /// </summary>
      public event EventHandler MessageLevelChanged;
      private void OnMessageLevelChanged(EventArgs e)
      {
         if (MessageLevelChanged != null)
         {
            MessageLevelChanged(this, e);
         }
      }

      /// <summary>
      /// PPD Decimal Places Setting Changed
      /// </summary>
      public event EventHandler DecimalPlacesChanged;
      private void OnDecimalPlacesChanged(EventArgs e)
      {
         if (DecimalPlacesChanged != null)
         {
            DecimalPlacesChanged(this, e);
         }
      }

      /// <summary>
      /// Show User Statistics Setting Changed
      /// </summary>
      public event EventHandler ShowUserStatsChanged;
      private void OnShowUserStatsChanged(EventArgs e)
      {
         if (ShowUserStatsChanged != null)
         {
            ShowUserStatsChanged(this, e);
         }
      }

      /// <summary>
      /// Color Log File Setting Changed
      /// </summary>
      public event EventHandler ColorLogFileChanged;
      private void OnColorLogFileChanged(EventArgs e)
      {
         if (ColorLogFileChanged != null)
         {
            ColorLogFileChanged(this, e);
         }
      } 

      /// <summary>
      /// Calculate Bonus Credit and PPD Setting Changed
      /// </summary>
      public event EventHandler CalculateBonusChanged;
      private void OnCalculateBonusChanged(EventArgs e)
      {
         if (CalculateBonusChanged != null)
         {
            CalculateBonusChanged(this, e);
         }
      } 
      #endregion

      #region Preference Validation
      private static int GetValidNumeric(string input, int defaultValue)
      {
         int output;
         if (Int32.TryParse(input, out output) == false)
         {
            output = defaultValue;
         }
         else if (StringOps.ValidateMinutes(output) == false)
         {
            output = defaultValue;
         }
         
         return output;
      }
      
      //public static bool ValidateDecimalPlaces(int Places)
      //{
      //   if ((Places > MaxDecimalPlaces) || (Places < MinDecimalPlaces))
      //   {
      //      return false;
      //   }

      //   return true;
      //}
      #endregion
      
      #region Preference Formatting
      /// <summary>
      /// PPD String Formatter
      /// </summary>
      public string PpdFormatString
      {
         get
         {
            int decimalPlaces = GetPreference<int>(Preference.DecimalPlaces);

            StringBuilder sbldr = new StringBuilder("###,###,##0");
            if (decimalPlaces > 0)
            {
               sbldr.Append(".");
               for (int i = 0; i < decimalPlaces; i++)
               {
                  sbldr.Append("0");
               }
            }

            return sbldr.ToString();
         }
      }
      #endregion
   }
}
