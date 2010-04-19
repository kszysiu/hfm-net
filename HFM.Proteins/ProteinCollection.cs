/*
 * HFM.NET - Protein Collection Class
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
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using HFM.Framework;
using HFM.Instrumentation;

namespace HFM.Proteins
{
   /// <summary>
   /// Protein Collection
   /// </summary>
   public sealed class ProteinCollection : SortedDictionary<Int32, IProtein>, IProteinCollection
   {
      #region Members
      private readonly string _projectInfoLocation;
      /// <summary>
      /// ProjectInfo.tab File Location
      /// </summary>
      public string ProjectInfoLocation
      {
         get { return _projectInfoLocation; }
      }
      
      private readonly IProjectSummaryDownloader _downloader;
      /// <summary>
      /// Project Summary Downloader Interface
      /// </summary>
      public IProjectSummaryDownloader Downloader
      {
         get { return _downloader; }
      }
      
      /// <summary>
      /// Preferences Interface
      /// </summary>
      private readonly IPreferenceSet _prefs;

      /// <summary>
      /// List of Projects that were not found after a download attempt.
      /// </summary>
      /// <remarks>See GetProtein() and BeginDownloadFromStanford().</remarks>
      private readonly Dictionary<Int32, DateTime> _projectsNotFound = new Dictionary<Int32, DateTime>(); 
      #endregion
      
      public ProteinCollection(IProjectSummaryDownloader downloader, IPreferenceSet prefs)
      {
         _prefs = prefs;
         _projectInfoLocation = Path.Combine(_prefs.GetPreference<string>(Preference.ApplicationDataFolderPath), Constants.ProjectInfoFileName);
         
         _downloader = downloader;
         _downloader.Dictionary = this;
         _downloader.ProjectInfoLocation = ProjectInfoLocation;
      }

      #region Methods

      /// <summary>
      /// Execute Primary Collection Read Sequence
      /// </summary>
      public void Read()
      {
         if (ReadFromTabDelimitedFile() == false)
         {
            _downloader.DownloadFromStanford();
         }
      }

      /// <summary>
      /// Read the Protein Collection from Tab Delimited File
      /// </summary>
      public bool ReadFromTabDelimitedFile()
      {
         return ReadFromTabDelimitedFile(_projectInfoLocation);
      }

      /// <summary>
      /// Read the Protein Collection from Tab Delimited File
      /// </summary>
      /// <param name="projectInfoFilePath">Path to File</param>
      public bool ReadFromTabDelimitedFile(string projectInfoFilePath)
      {
         DateTime start = HfmTrace.ExecStart;
         try
         {
            // If this is set to true, then it signals that the Project load
            // was successful but all expected data was not present.  We now
            // need to Download the psummary and update currently running
            // Projects with current data. Old Projects not found on the 
            // psummary will be left in the collection for historical purposes
            // and the missing values defaulted.
            bool backwardCompatibleDownload = false;

            String[] csvData = File.ReadAllLines(projectInfoFilePath);
            foreach (String sLine in csvData)
            {
               try
               {
                  // Parse the current line from the CSV file
                  Protein p = new Protein();
                  String[] lineData = sLine.Split(new[] {'\t'}, StringSplitOptions.None);
                  p.ProjectNumber = Int32.Parse(lineData[0], CultureInfo.InvariantCulture);
                  p.ServerIP = lineData[1].Trim();
                  p.WorkUnitName = lineData[2].Trim();
                  p.NumAtoms = Int32.Parse(lineData[3], CultureInfo.InvariantCulture);
                  p.PreferredDays = Double.Parse(lineData[4], CultureInfo.InvariantCulture);
                  p.MaxDays = Double.Parse(lineData[5], CultureInfo.InvariantCulture);
                  p.Credit = Double.Parse(lineData[6], CultureInfo.InvariantCulture);
                  p.Frames = Int32.Parse(lineData[7], CultureInfo.InvariantCulture);
                  p.Core = lineData[8];
                  p.Description = lineData[9];
                  p.Contact = lineData[10];

                  // Backward Compatibility with v0.4.0 or prior
                  // Newer versions download and parse the KFactor
                  if (lineData.Length > 11)
                  {
                     p.KFactor = Double.Parse(lineData[11], CultureInfo.InvariantCulture);
                  }
                  else
                  {
                     backwardCompatibleDownload = true;
                     p.KFactor = 0;
                  }

                  if (ContainsKey(p.ProjectNumber))
                  {
                     this[p.ProjectNumber] = p;
                  }
                  else
                  {
                     Add(p.ProjectNumber, p);
                  }
               }
               catch (Exception ex)
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, ex);
               }
            }

            if (backwardCompatibleDownload)
            {
               // Signal the caller to Download and update from the psummary
               return false;
            }
         }
         catch (Exception ex)
         {
            HfmTrace.WriteToHfmConsole(ex);
            return false;
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, start);
         }

         return true;
      }

      /// <summary>
      /// Clear the Projects not found cache
      /// </summary>
      public void ClearProjectsNotFoundCache()
      {
         // See GetProtein() implementation
         _projectsNotFound.Clear();
      }

      private delegate void DownloadFromStanfordDelegate();

      /// <summary>
      /// Download project information from Stanford University (THREAD SAFE)
      /// </summary>
      public IAsyncResult BeginDownloadFromStanford()
      {
         return new DownloadFromStanfordDelegate(_downloader.DownloadFromStanford).BeginInvoke(null, null);
      }
      
      /// <summary>
      /// Get Protein from Collection (should be called from worker thread)
      /// </summary>
      /// <param name="projectId">Project ID</param>
      public IProtein GetProtein(int projectId)
      {
         // If Project Requested is 0, just return a "blank" Protein.
         if (projectId == 0) return new Protein();
      
         // If Project is Found, return it
         if (ContainsKey(projectId)) return this[projectId];
         
         HfmTrace.WriteToHfmConsole(TraceLevel.Warning,
                                    String.Format("Project ID '{0}' not found in Protein Collection.", projectId), true);
         
         // If Project has already been looked for previously
         if (_projectsNotFound.ContainsKey(projectId))
         {
            // If it has been less than one day since this Project triggered an
            // automatic download attempt, just return a "blank" Protein.
            if (DateTime.Now.Subtract(_projectsNotFound[projectId]).TotalDays < 1)
            {
               return new Protein();
            }
         }
         
         // Execute a Download
         _downloader.DownloadFromStanford();

         // If the Project is now Found
         if (ContainsKey(projectId))
         {
            // Remove it from the Not Found List and return it
            _projectsNotFound.Remove(projectId);
            return this[projectId];
         }
         
         // If already on the Not Found List
         if (_projectsNotFound.ContainsKey(projectId))
         {
            // Update the Last Download Attempt Date
            _projectsNotFound[projectId] = DateTime.Now;
         }
         else // Add it
         {
            _projectsNotFound.Add(projectId, DateTime.Now);
         }
            
         HfmTrace.WriteToHfmConsole(TraceLevel.Error,
                                    String.Format("Project ID '{0}' not found on Stanford Web Project Summary.", projectId), true);

         // Return a "blank" Protein
         return new Protein();
      }
      
      /// <summary>
      /// Get a New Protein from the Collection
      /// </summary>
      public IProtein GetNewProtein()
      {
         return new Protein();
      }
      
      #endregion
   }
}
