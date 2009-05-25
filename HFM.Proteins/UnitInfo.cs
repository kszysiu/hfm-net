/*
 * HFM.NET - Unit Info Class
 * Copyright (C) 2006 David Rawling
 * Copyright (C) 2009 Ryan Harlamert (harlam357)
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
using Debug=HFM.Instrumentation.Debug;

namespace HFM.Proteins
{
   #region Enum
   public enum ClientType
   {
      Unknown,
      Standard,
      SMP,
      GPU
   } 
   #endregion

   /// <summary>
   /// Contains the state of a protein in progress
   /// </summary>
   [Serializable]
   public class UnitInfo
   {
      #region CTOR
      /// <summary>
      /// Primary Constructor
      /// </summary>
      public UnitInfo(string ownerName, string ownerPath)
      {
         _OwningInstanceName = ownerName;
         _OwningInstancePath = ownerPath;
         
         Clear();
      } 
      #endregion

      #region Owner Data Properties
      /// <summary>
      /// Name of the Client Instance that owns this UnitInfo
      /// </summary>
      private string _OwningInstanceName;
      /// <summary>
      /// Name of the Client Instance that owns this UnitInfo
      /// </summary>
      public string OwningInstanceName
      {
         get { return _OwningInstanceName; }
         set { _OwningInstanceName = value; }
      }

      /// <summary>
      /// Path of the Client Instance that owns this UnitInfo
      /// </summary>
      private string _OwningInstancePath;
      /// <summary>
      /// Path of the Client Instance that owns this UnitInfo
      /// </summary>
      public string OwningInstancePath
      {
         get { return _OwningInstancePath; }
         set { _OwningInstancePath = value; }
      }
      #endregion

      #region ReadOnly Properties
      /// <summary>
      /// Formatted Project (Run, Clone, Gen) Information
      /// </summary>
      public string ProjectRunCloneGen
      {
         get
         {
            return String.Format("P{0} (R{1}, C{2}, G{3})", ProjectID,
                                                            ProjectRun,
                                                            ProjectClone,
                                                            ProjectGen);
         }
      }

      /// <summary>
      /// Time per section based on current PPD calculation setting (readonly)
      /// </summary>
      public Int32 RawTimePerSection
      {
         get
         {
            switch (Preferences.PreferenceSet.Instance.PpdCalculation)
            {
               case Preferences.ePpdCalculation.LastFrame:
                  return _RawTimePerLastSection;
               case Preferences.ePpdCalculation.LastThreeFrames:
                  return _RawTimePerThreeSections;
               case Preferences.ePpdCalculation.AllFrames:
                  return _RawTimePerAllSections;
               case Preferences.ePpdCalculation.EffectiveRate:
                  return _RawTimePerUnitDownload;
            }

            return 0;
         }
      }

      #region Values Based on CurrentFrame
      /// <summary>
      /// Timestamp from the last completed frame
      /// </summary>
      public TimeSpan TimeOfLastFrame
      {
         get
         {
            if (_CurrentFrame != null)
            {
               return _CurrentFrame.TimeOfFrame;
            }

            return TimeSpan.Zero;
         }
      }

      /// <summary>
      /// Frame time for the last completed frame
      /// </summary>
      public TimeSpan CurrentFrameDuration
      {
         get
         {
            if (_CurrentFrame != null)
            {
               return _CurrentFrame.FrameDuration;
            }

            return TimeSpan.Zero;
         }
      }

      /// <summary>
      /// Percentage from the last completed frame
      /// </summary>
      public Int32 CurrentFramePercent
      {
         get
         {
            if (_CurrentFrame != null)
            {
               return _CurrentFrame.FramePercent;
            }

            return 0;
         }
      }
      #endregion
      
      //public bool HasFrameData
      //{
      //   get { return _FramesObserved > 0; }
      //}
      #endregion
      
      #region Public Properties and Related Private Members
      /// <summary>
      /// The Folding ID (Username) attached to this work unit
      /// </summary>
      private string _FoldingID;
      /// <summary>
      /// The Folding ID (Username) attached to this work unit
      /// </summary>
      public string FoldingID
      {
         get { return _FoldingID; }
         set { _FoldingID = value; }
      }

      /// <summary>
      /// The Team number attached to this work unit
      /// </summary>
      private Int32 _Team;
      /// <summary>
      /// The Team number attached to this work unit
      /// </summary>
      public Int32 Team
      {
         get { return _Team; }
         set { _Team = value; }
      }
      
      /// <summary>
      /// Client Type for this work unit
      /// </summary>
      private ClientType _TypeOfClient;
      /// <summary>
      /// Client Type for this work unit
      /// </summary>
      public ClientType TypeOfClient
      {
         get { return _TypeOfClient; }
         set { _TypeOfClient = value; }
      }

      /// <summary>
      /// Core Version Number
      /// </summary>
      private string _CoreVersion = String.Empty;
      /// <summary>
      /// Core Version Number
      /// </summary>
      public string CoreVersion
      {
         get { return _CoreVersion; }
         set { _CoreVersion = value; }
      }

      /// <summary>
      /// Date/time the unit was downloaded
      /// </summary>
      private DateTime _DownloadTime;
      /// <summary>
      /// Date/time the unit was downloaded
      /// </summary>
      public DateTime DownloadTime
      {
         get { return _DownloadTime; }
         set { _DownloadTime = value; }
      }

      /// <summary>
      /// Date/time the unit is due (preferred deadline)
      /// </summary>
      private DateTime _DueTime;
      /// <summary>
      /// Date/time the unit is due (preferred deadline)
      /// </summary>
      public DateTime DueTime
      {
         get { return _DueTime; }
         set { _DueTime = value; }
      }

      /// <summary>
      /// Frame progress of the unit
      /// </summary>
      private Int32 _FramesComplete;
      /// <summary>
      /// Frame progress of the unit
      /// </summary>
      public Int32 FramesComplete
      {
         get { return _FramesComplete; }
         set { _FramesComplete = value; }
      }

      /// <summary>
      /// Current progress (percentage) of the unit
      /// </summary>
      private Int32 _PercentComplete;
      /// <summary>
      /// Current progress (percentage) of the unit
      /// </summary>
      public Int32 PercentComplete
      {
         get { return _PercentComplete; }
         set
         {
            // Add check for valid values instead of just accepting whatever is given - Issue 2
            if (value < 0 || value > 100)
            {
               _PercentComplete = 0;
            }
            else
            {
               _PercentComplete = value;
            }
         }
      }

      /// <summary>
      /// Project ID Number
      /// </summary>
      private Int32 _ProjectID;
      /// <summary>
      /// Project ID Number
      /// </summary>
      public Int32 ProjectID
      {
         get { return _ProjectID; }
         set { _ProjectID = value; }
      }

      /// <summary>
      /// Project ID (Run)
      /// </summary>
      private Int32 _ProjectRun;
      /// <summary>
      /// Project ID (Run)
      /// </summary>
      public Int32 ProjectRun
      {
         get { return _ProjectRun; }
         set { _ProjectRun = value; }
      }

      /// <summary>
      /// Project ID (Clone)
      /// </summary>
      private Int32 _ProjectClone;
      /// <summary>
      /// Project ID (Clone)
      /// </summary>
      public Int32 ProjectClone
      {
         get { return _ProjectClone; }
         set { _ProjectClone = value; }
      }

      /// <summary>
      /// Project ID (Gen)
      /// </summary>
      private Int32 _ProjectGen;
      /// <summary>
      /// Project ID (Gen)
      /// </summary>
      public Int32 ProjectGen
      {
         get { return _ProjectGen; }
         set { _ProjectGen = value; }
      }
      
      /// <summary>
      /// Name of the unit
      /// </summary>
      private String _ProteinName = String.Empty;
      /// <summary>
      /// Name of the unit
      /// </summary>
      public String ProteinName
      {
         get { return _ProteinName; }
         set { _ProteinName = value; }
      }

      /// <summary>
      /// Tag string as read from the UnitInfo.txt file
      /// </summary>
      private string _ProteinTag = String.Empty;
      /// <summary>
      /// Tag string as read from the UnitInfo.txt file
      /// </summary>
      public string ProteinTag
      {
         get { return _ProteinTag; }
         set { _ProteinTag = value; }
      }

      /// <summary>
      /// Raw number of frames complete (this is not always a percent value)
      /// </summary>
      private Int32 _RawComplete;
      /// <summary>
      /// Raw number of frames complete (this is not always a percent value)
      /// </summary>
      public Int32 RawFramesComplete
      {
         get { return _RawComplete; }
         set
         {
            _RawComplete = value;
         }
      }

      /// <summary>
      /// Raw total number of frames (this is not always 100)
      /// </summary>
      private Int32 _RawTotal;
      /// <summary>
      /// Raw total number of frames (this is not always 100)
      /// </summary>
      public Int32 RawFramesTotal
      {
         get { return _RawTotal; }
         set
         {
            _RawTotal = value;
         }
      }

      /// <summary>
      /// List of current log file text lines
      /// </summary>
      private readonly List<string> _CurrentLogText = new List<string>();
      /// <summary>
      /// List of current log file text lines
      /// </summary>
      public List<string> CurrentLogText
      {
         get { return _CurrentLogText; }
      }

      /// <summary>
      /// Class member containing info on the currently running protein
      /// </summary>
      private Protein _CurrentProtein;
      /// <summary>
      /// Class member containing info on the currently running protein
      /// </summary>
      public Protein CurrentProtein
      {
         get { return _CurrentProtein; }
         set 
         {
            _CurrentProtein = value; 
         }
      }

      #region Time Based Values
      /// <summary>
      /// Time per frame (TPF) of the unit
      /// </summary>
      private TimeSpan _TimePerFrame;
      /// <summary>
      /// Time per frame (TPF) of the unit
      /// </summary>
      public TimeSpan TimePerFrame
      {
         get { return _TimePerFrame; }
         set { _TimePerFrame = value; }
      }

      /// <summary>
      /// Units per day (UPD) rating for this instance
      /// </summary>
      private Double _UPD;
      /// <summary>
      /// Units per day (UPD) rating for this instance
      /// </summary>
      public Double UPD
      {
         get
         {
            return _UPD;
         }
         set { _UPD = value; }
      }

      /// <summary>
      /// Points per day (PPD) rating for this instance
      /// </summary>
      private Double _PPD;
      /// <summary>
      /// Points per day (PPD) rating for this instance
      /// </summary>
      public Double PPD
      {
         get
         {
            return _PPD;
         }
         set { _PPD = value; }
      }

      /// <summary>
      /// Esimated time of arrival (ETA) for this protein
      /// </summary>
      private TimeSpan _ETA;
      /// <summary>
      /// Esimated time of arrival (ETA) for this protein
      /// </summary>
      public TimeSpan ETA
      {
         get { return _ETA; }
         set { _ETA = value; }
      }

      /// <summary>
      /// Average frame time since unit download
      /// </summary>
      private Int32 _RawTimePerUnitDownload;
      /// <summary>
      /// Average frame time since unit download
      /// </summary>
      public Int32 RawTimePerUnitDownload
      {
         get { return _RawTimePerUnitDownload; }
         set
         {
            _RawTimePerUnitDownload = value;
         }
      }
      
      /// <summary>
      /// Average frame time over all sections
      /// </summary>
      private Int32 _RawTimePerAllSections = 0;
      /// <summary>
      /// Average frame time over all sections
      /// </summary>
      public Int32 RawTimePerAllSections
      {
         get { return _RawTimePerAllSections; }
         set
         {
            _RawTimePerAllSections = value;
         }
      }

      /// <summary>
      /// Average frame time over the last three sections
      /// </summary>
      private Int32 _RawTimePerThreeSections = 0;
      /// <summary>
      /// Average frame time over the last three sections
      /// </summary>
      public Int32 RawTimePerThreeSections
      {
         get { return _RawTimePerThreeSections; }
         set
         {
            _RawTimePerThreeSections = value;
         }
      }

      /// <summary>
      /// Frame time of the last section
      /// </summary>
      private Int32 _RawTimePerLastSection = 0;
      /// <summary>
      /// Frame time of the last section
      /// </summary>
      public Int32 RawTimePerLastSection
      {
         get { return _RawTimePerLastSection; }
         set
         {
            _RawTimePerLastSection = value;
         }
      }
      #endregion

      #region Frame (UnitFrame) Data Variables

      /// <summary>
      /// Number of Frames Observed on this Unit
      /// </summary>
      private Int32 _FramesObserved = 0;
      /// <summary>
      /// Number of Frames Observed on this Unit
      /// </summary>
      public Int32 FramesObserved
      {
         get { return _FramesObserved; }
         set { _FramesObserved = value; }
      }
      
      /// <summary>
      /// Last Observed Frame on this Unit
      /// </summary>
      private UnitFrame _CurrentFrame = null;
      public UnitFrame CurrentFrame
      {
         get { return _CurrentFrame; }
         set { _CurrentFrame = value; }
      }
      
      /// <summary>
      /// Frame Data for this Unit
      /// </summary>
      private UnitFrame[] _UnitFrames = new UnitFrame[101];
      /// <summary>
      /// Frame Data for this Unit
      /// </summary>
      public UnitFrame[] UnitFrames
      {
         get { return _UnitFrames; }
         set { _UnitFrames = value; }
      }
      
      #endregion
      
      #endregion

      #region Clear UnitInfo and Clear Time Based Values
      private void Clear()
      {
         FoldingID = "Unknown";
         Team = 0;
         TypeOfClient = ClientType.Unknown;
         CoreVersion = String.Empty;
         DownloadTime = DateTime.MinValue;
         DueTime = DateTime.MinValue;
         FramesComplete = 0;
         PercentComplete = 0;
         ProjectID = 0;
         ProjectRun = 0;
         ProjectClone = 0;
         ProjectGen = 0;
         ProteinName = String.Empty;
         ProteinTag = String.Empty;
         RawFramesComplete = 0;
         RawFramesTotal = 0;
         _CurrentLogText.Clear();
         CurrentProtein = new Protein();

         ClearTimeBasedValues();
      }
      
      /// <summary>
      /// Clear only the time based values for this instance
      /// </summary>
      public void ClearTimeBasedValues()
      {
         // Set in SetTimeBasedValues()
         PercentComplete = 0;
         TimePerFrame = TimeSpan.Zero;
         UPD = 0.0;
         PPD = 0.0;
         ETA = TimeSpan.Zero;

         // Set in SetFrameTimes()
         RawTimePerUnitDownload = 0;
         RawTimePerAllSections = 0;
         RawTimePerThreeSections = 0;
         RawTimePerLastSection = 0;
      }
      #endregion

      #region Set Frame and Clear Frame Data
      /// <summary>
      /// Set the Current Work Unit Frame
      /// </summary>
      /// <param name="frame">Current Work Unit Frame</param>
      public void SetCurrentFrame(UnitFrame frame)
      {
         if (_UnitFrames[frame.FramePercent] == null)
         {
            // increment observed count
            _FramesObserved++;
         
            CurrentFrame = frame;
            UnitFrames[CurrentFrame.FramePercent] = CurrentFrame;
            
            CurrentFrame.FrameDuration = TimeSpan.Zero;
            if (CurrentFramePercent > 0 && UnitFrames[CurrentFramePercent - 1] != null)
            {
               CurrentFrame.FrameDuration = GetDelta(CurrentFrame.TimeOfFrame, UnitFrames[CurrentFramePercent - 1].TimeOfFrame);
            }
         }
      }

      /// <summary>
      /// Clear the Observed Count, Current Frame Pointer, and the UnitFrames Array
      /// </summary>
      public void ClearFrameData()
      {
         _FramesObserved = 0;
         _CurrentFrame = null;
         _UnitFrames = new UnitFrame[101];
      }
      #endregion

      #region Calculate Frame Time Variations
      /// <summary>
      /// Sets Frame Time Variations based on the observed frames
      /// </summary>
      public void SetFrameTimes()
      {
         RawTimePerLastSection = 0;
         RawTimePerThreeSections = 0;
         RawTimePerAllSections = 0;
         RawTimePerUnitDownload = 0;
         
         if (FramesObserved > 0)
         {
            // time is valid for 1 "set" ago
            if (_FramesObserved > 1)
            {
               RawTimePerLastSection = Convert.ToInt32(_CurrentFrame.FrameDuration.TotalSeconds);
            }
            
            // time is valid for 3 "sets" ago
            if (_FramesObserved > 3)
            {
               RawTimePerThreeSections = (GetDuration(3) / 3);
            }

            RawTimePerAllSections = (GetDuration(FramesObserved) / FramesObserved);
            
            if (DownloadTime.Equals(DateTime.MinValue) == false)
            {
               TimeSpan timeSinceUnitDownload = DateTime.Now.Subtract(DownloadTime);
               RawTimePerUnitDownload = (Convert.ToInt32(timeSinceUnitDownload.TotalSeconds) / CurrentFramePercent);
            }
         }
      }
      
      /// <summary>
      /// Get the total duration over the specified number of most recent frames
      /// </summary>
      /// <param name="numberOfFrames">Number of most recent frames</param>
      public int GetDuration(int numberOfFrames)
      {
         int TotalSeconds = 0;
         int frameNumber = CurrentFrame.FramePercent;
         
         try
         {
            for (int i = 0; i < numberOfFrames; i++)
            {
               TotalSeconds += Convert.ToInt32(UnitFrames[frameNumber].FrameDuration.TotalSeconds);
               frameNumber--;
            }
         }
         catch (NullReferenceException ex)
         {
            TotalSeconds = 0;
            Debug.WriteToHfmConsole(TraceLevel.Warning,
                                    String.Format("{0} threw exception {1}.", Debug.FunctionName, ex.Message));
         }
         
         return TotalSeconds;
      }

      /// <summary>
      /// Get Time Delta between given frames
      /// </summary>
      /// <param name="timeLastFrame">Time of last frame</param>
      /// <param name="timeCompareFrame">Time of a previous frame to compare</param>
      private static TimeSpan GetDelta(TimeSpan timeLastFrame, TimeSpan timeCompareFrame)
      {
         TimeSpan tDelta;

         // check for rollover back to 00:00:00 timeLastFrame will be less than previous timeCompareFrame reading
         if (timeLastFrame < timeCompareFrame)
         {
            // get time before rollover
            tDelta = TimeSpan.FromDays(1).Subtract(timeCompareFrame);
            // add time from latest reading
            tDelta = tDelta.Add(timeLastFrame);
         }
         else
         {
            tDelta = timeLastFrame.Subtract(timeCompareFrame);
         }

         return tDelta;
      }
      #endregion
   }
}