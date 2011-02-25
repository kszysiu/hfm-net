/*
 * HFM.NET - Platform Operations Class
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

using HFM.Framework.DataTypes;

namespace HFM.Framework
{
   public static class PlatformOps
   {
      public static bool IsRunningOnMono()
      {
         return Type.GetType("Mono.Runtime") != null;
      }

      /// <summary>
      /// Major.Minor.Build
      /// </summary>
      public static string ApplicationVersion
      {
         get { return CreateVersionString("{0}.{1}.{2}"); }
      }

      /// <summary>
      /// Major.Minor.Build
      /// </summary>
      public static string ApplicationNameAndVersion
      {
         get { return String.Concat("HFM.NET v", CreateVersionString("{0}.{1}.{2}")); }
      }

      /// <summary>
      /// Major.Minor.Build.Revision
      /// </summary>
      public static string ApplicationNameAndVersionWithRevision
      {
         get { return String.Concat("HFM.NET v", CreateVersionString("{0}.{1}.{2}.{3}")); }
      }

      /// <summary>
      /// Major.Minor.Build.Revision
      /// </summary>
      public static string ApplicationVersionWithRevision
      {
         get { return CreateVersionString("{0}.{1}.{2}.{3}"); }
      }

      /// <summary>
      /// Formatted vMajor.Minor.Build.Revision
      /// </summary>
      public static string ShortFormattedApplicationVersionWithRevision
      {
         get { return CreateVersionString("v{0}.{1}.{2}.{3}"); }
      }

      /// <summary>
      /// Formatted Version Major.Minor.Build - Revision
      /// </summary>
      public static string LongFormattedApplicationVersionWithRevision
      {
         get { return CreateVersionString("Version {0}.{1}.{2} - Revision {3}"); }
      }
      
      public static long VersionNumber
      {
         get
         {
            // Example: 0.3.1.50 == 30010045 / 1.3.4.75 == 1030040075
            FileVersionInfo fileVersionInfo =
               FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            return GetVersionLongFromArray(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart,
                                           fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart);
         }
      }
      
      /// <summary>
      /// Parse version number from 'x.x.x.x' formatted string.
      /// </summary>
      /// <exception cref="ArgumentNullException">Throws when argument is null.</exception>
      /// <exception cref="FormatException">Throws when given version cannot be parsed.</exception>
      public static long ParseVersion(string version)
      {
         if (version == null) throw new ArgumentNullException("version");

         var versionNumbers = GetVersionNumbers(version);
         return GetVersionLongFromArray(versionNumbers);
      }

      private static int[] GetVersionNumbers(string version)
      {
         Debug.Assert(version != null);

         var regex = new Regex("^(?<Major>(\\d+))\\.(?<Minor>(\\d+))\\.(?<Build>(\\d+))\\.(?<Revision>(\\d+))$", RegexOptions.ExplicitCapture);
         var match = regex.Match(version);
         if (match.Success)
         {
            var versionNumbers = new int[4];
            versionNumbers[0] = Int32.Parse(match.Result("${Major}"), CultureInfo.InvariantCulture);
            versionNumbers[1] = Int32.Parse(match.Result("${Minor}"), CultureInfo.InvariantCulture);
            versionNumbers[2] = Int32.Parse(match.Result("${Build}"), CultureInfo.InvariantCulture);
            versionNumbers[3] = Int32.Parse(match.Result("${Revision}"), CultureInfo.InvariantCulture);
            return versionNumbers;
         }

         throw new FormatException(String.Format(CultureInfo.CurrentCulture, 
            "Given version '{0}' is not in the correct format.", version));
      }
      
      private static long GetVersionLongFromArray(params int[] versionNumbers)
      {
         return (versionNumbers[0] * 1000000000) + (versionNumbers[1] * 10000000) +
                (versionNumbers[2] * 10000) + versionNumbers[3];
      }

      private static string CreateVersionString(string format)
      {
         Debug.Assert(format != null);

         FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
         return String.Format(CultureInfo.InvariantCulture, format, fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart,
                                                                    fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart);
      }

      public static string AssemblyGuid
      {
         get
         {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
            if (attributes.Length == 0)
            {
               return String.Empty;
            }
            return ((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;
         }
      }

      public static string AssemblyTitle
      {
         get
         {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
               AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
               if (String.IsNullOrEmpty(titleAttribute.Title) == false)
               {
                  return titleAttribute.Title;
               }
            }
            return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
         }
      }

      #region Status Color Helper Functions
      /// <summary>
      /// Gets Status Color Pen Object
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Color (Pen)</returns>
      public static Pen GetStatusPen(ClientStatus status)
      {
         return new Pen(GetStatusColor(status));
      }

      /// <summary>
      /// Gets Status Color Brush Object
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Color (Brush)</returns>
      public static SolidBrush GetStatusBrush(ClientStatus status)
      {
         return new SolidBrush(GetStatusColor(status));
      }

      /// <summary>
      /// Gets Status Html Color String
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Html Color (String)</returns>
      public static string GetStatusHtmlColor(ClientStatus status)
      {
         return ColorTranslator.ToHtml(GetStatusColor(status));
      }

      /// <summary>
      /// Gets Status Html Font Color String
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Html Font Color (String)</returns>
      public static string GetStatusHtmlFontColor(ClientStatus status)
      {
         switch (status)
         {
            case ClientStatus.Running:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.RunningAsync:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.RunningNoFrameTimes:
               return ColorTranslator.ToHtml(Color.Black);
            case ClientStatus.Stopped:
            case ClientStatus.EuePause:
            case ClientStatus.Hung:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.Paused:
               return ColorTranslator.ToHtml(Color.Black);
            case ClientStatus.SendingWorkPacket:
            case ClientStatus.GettingWorkPacket:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.Offline:
               return ColorTranslator.ToHtml(Color.Black);
            default:
               return ColorTranslator.ToHtml(Color.Black);
         }
      }

      /// <summary>
      /// Gets Status Color Object
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Color (Color)</returns>
      public static Color GetStatusColor(ClientStatus status)
      {
         switch (status)
         {
            case ClientStatus.Running:
               return Color.Green; // Issue 45
            case ClientStatus.RunningAsync:
               return Color.Blue;
            case ClientStatus.RunningNoFrameTimes:
               return Color.Yellow;
            case ClientStatus.Stopped:
            case ClientStatus.EuePause:
            case ClientStatus.Hung:
               return Color.DarkRed;
            case ClientStatus.Paused:
               return Color.Orange;
            case ClientStatus.SendingWorkPacket:
            case ClientStatus.GettingWorkPacket:
               return Color.Purple;
            case ClientStatus.Offline:
               return Color.Gray;
            default:
               return Color.Gray;
         }
      }
      #endregion

      /// <summary>
      /// Does the currentUnitInfo match the parsedUnitInfo?
      /// </summary>
      public static bool IsUnitInfoCurrentUnitInfo(IUnitInfoLogic currentUnitInfo, IUnitInfoLogic parsedUnitInfo)
      {
         // if the Projects are known
         if (currentUnitInfo != null && currentUnitInfo.UnitInfoData.ProjectIsUnknown() == false &&
             parsedUnitInfo != null && parsedUnitInfo.UnitInfoData.ProjectIsUnknown() == false)
         {
            // Matches the Current Project and Raw Download Time.
            // Download Time check should be made on the DownloadTime
            // property value available in the UnitInfoData property.
            if (ProjectsMatch(currentUnitInfo.UnitInfoData, parsedUnitInfo.UnitInfoData) &&
                currentUnitInfo.UnitInfoData.DownloadTime.Equals(parsedUnitInfo.UnitInfoData.DownloadTime))
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

      public static string[] GetQueryFieldColumnNames()
      {
         // Indexes Must Match QueryFieldName enum defined in Enumerations.cs
         var list = new List<string>();
         list.Add("ProjectID");
         list.Add("Run");
         list.Add("Clone");
         list.Add("Gen");
         list.Add("Instance Name");
         list.Add("Instance Path");
         list.Add("Username");
         list.Add("Team");
         list.Add("Core Version");
         list.Add("Frames Completed");
         list.Add("Frame Time");
         list.Add("Unit Result");
         list.Add("Download Date (UTC)");
         list.Add("Completion Date (UTC)");
         list.Add("Work Unit Name");
         list.Add("KFactor");
         list.Add("Core Name");
         list.Add("Total Frames");
         list.Add("Atoms");
         list.Add("Client Type");
         list.Add("PPD");
         list.Add("Credit");

         return list.ToArray();
      }
   }
}